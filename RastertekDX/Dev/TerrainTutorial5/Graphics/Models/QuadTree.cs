using System;
using System.Linq;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;
using GraphicsClass = Engine.Graphics.Graphics;
using InputClass = Engine.Inputs.Input;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using System.Runtime.InteropServices;
using Engine.SystemData;
using System.IO;
using System.Collections.Generic;
using Engine.Graphics.Shaders;
using Engine.Graphics.Data;

namespace Engine.Graphics.Models
{
	public class QuadTree : ICloneable
	{
		#region Structures
		public struct Node
		{
			public float PositionX, PositionZ, Width;
			public int TriangleCount;
			public Buffer VertexBuffer, IndexBuffer;
			public Node?[] Nodes;
		}
		#endregion

		#region Variables / Properties
		private int TriangleCount { get; set; }
		public int DrawCount { get; private set; }

		private List<HeightMapTerrainShader.Vertex> VertexList { get; set; }
		private Node? ParentNode { get; set; }

		public readonly int MaxTriangles = 10000;
		#endregion

		#region Constructors
		#endregion

		#region Methods
		public bool Initialize(Device device, HeightMapTerrain terrain)
		{
			// Get the number of the vertices on the terrain vertex array.
			var vertexCount = terrain.VertexCount;

			// Store the total triangle count for the vertex list.
			TriangleCount = vertexCount / 3;

			VertexList = new List<HeightMapTerrainShader.Vertex>(vertexCount);

			// Copy the terrain vertices into vertex list.
			terrain.CopyVertexArray(VertexList);

			// Calculate the center x,z and the width of the mesh.
			float centerX, centerZ, width;
			CalculateMeshDimensions(vertexCount, out centerX, out centerZ, out width);

			// Create the parent node for the quad tree
			// Recursively build the quad tree based on the vertex list data and mesh dimensions.
			ParentNode = CreateTreeNode(device, centerX, centerZ, width);

			// Release the vertex list since the quad tree now has the vertices in each node.
			VertexList.Clear();

			return true;
		}

		public void Shutdown()
		{
			ReleaseNode(ParentNode);
		}

		public void Render(DeviceContext deviceContext, Frustum frustum, HeightMapTerrainShader shader)
		{
			// Reset the number of the triangles that are drawn for this frame.
			DrawCount = 0;

			// Render each node that is visible at the parent node and moving down the tree.
			RenderNode(deviceContext, ParentNode, frustum, shader);
		}

		private void CalculateMeshDimensions(int vertexCount, out float centerX, out float centerZ, out float meshWidth)
		{
			// Initialize the center position of the mesh to zero.
			centerX = 0;
			centerZ = 0;

			// Sum all the vertices in the mesh.
			for (var i = 0; i < vertexCount; i++)
			{
				centerX += VertexList[i].position.X;
				centerZ += VertexList[i].position.Z;
			}

			// And then divide it by the number of the vertices to find the mid-point of the mesh.
			centerX /= vertexCount;
			centerZ /= vertexCount;

			// Initialize the maximum and minimum suze of the mesh.
			var maxWidth = 0.0f;
			var maxDepth = 0.0f;

			var minWidth = Math.Abs(VertexList[0].position.X - centerX);
			var minDepth = Math.Abs(VertexList[0].position.Z - centerZ);

			// Go through all the vertices and find the maximum and minimum width and depth of the mesh.
			for (int i = 0; i < vertexCount; i++)
			{
				var width = Math.Abs(VertexList[i].position.X - centerX);
				var depth = Math.Abs(VertexList[i].position.Z - centerZ);

				if (width > maxWidth) maxWidth = width;
				if (depth > maxDepth) maxDepth = depth;
				if (width < minWidth) minWidth = width;
				if (depth < minDepth) minDepth = depth;
			}

			// Find the absolute maximum value between the min and max depth and width.
			var maxX = Math.Max(Math.Abs(minWidth), Math.Abs(maxWidth));
			var maxZ = Math.Max(Math.Abs(minDepth), Math.Abs(maxDepth));

			// Calculate the maximum diameter of the mesh.
			meshWidth = Math.Max(maxX, maxZ) * 2;
		}

		private Node? CreateTreeNode(Device device, float positionX, float positionZ, float width)
		{
			var node = new Node();

			// Store the node position and size.
			node.PositionX = positionX;
			node.PositionZ = positionZ;
			node.Width = width;

			// Initialize the triangle count to zero for the node.
			node.TriangleCount = 0;

			// Initialize the vertex and index buffers
			node.VertexBuffer = null;
			node.IndexBuffer = null;

			// Initialize the nodes
			node.Nodes = new Node?[4];

			// Count the number of the triangle that are inside this node.
			var numTriangles = CountTriangles(positionX, positionZ, width);

			// Case 1: If there are no triangles in this node then return as it is empty and requires no processing.
			if (numTriangles == 0)
				return null;

			// Case 2: If there are too many triangles in this node then split it into four equal sized smaller tree nodes.
			if (numTriangles > MaxTriangles)
			{
				for (int i = 0; i < 4; i++)
				{
					// Calculate the position offsets for the new child node.
					var offsetX = (((i % 2) < 1) ? -1 : 1) * (width / 4);
					var offsetZ = (((i % 4) < 2) ? -1 : 1) * (width / 4);

					// See if there are triangles in the new node.
					var count = CountTriangles((positionX + offsetX), (positionZ + offsetZ), width / 2);
					if (count > 0)
						// If there are triangles inside where this node would be then create the child node.
						// Extend the tree starting from this child node now.
						node.Nodes[i] = CreateTreeNode(device, (positionX + offsetX), (positionZ + offsetZ), width / 2);
				}
			}

			// Case 3: If this node is not empty and the triangle count for it is less than the max then
			// this node is at the bottom of the tree so create the list of triangles to store in it.
			node.TriangleCount = numTriangles;

			// Calculate the number of vertices
			var vertexCount = numTriangles * 3;

			// Create the vertex array
			var vertices = new HeightMapTerrainShader.Vertex[vertexCount];

			// Create the index array
			var indices = new int[vertexCount];

			// Initialize the index for this new vertex and index array.
			var index = 0;

			// Go through all the triangles in the vertex list.
			for (int i = 0; i < TriangleCount; i++)
			{
				// If the triangle is inside this node then add it to the vertex array.
				var result = IsTriangleContained(i, positionX, positionZ, width);
				if (result)
				{
					// Calculate the index into the terrain vertex list
					var vertexIndex = i * 3;

					// Get the three vertices of this triangle from the vertex list
					vertices[index] = VertexList[vertexIndex];
					indices[index] = index++;

					vertexIndex++;
					vertices[index] = VertexList[vertexIndex];
					indices[index] = index++;

					vertexIndex++;
					vertices[index] = VertexList[vertexIndex];
					indices[index] = index++;
				}
			}

			// Create the vertex buffer.
			node.VertexBuffer = Buffer.Create(device, BindFlags.VertexBuffer, vertices);

			// Create the index buffer.
			node.IndexBuffer = Buffer.Create(device, BindFlags.IndexBuffer, indices);

			return node;
		}

		private int CountTriangles(float positionX, float positionZ, float width)
		{
			// Initialize the count to 0
			var count = 0;

			// Go through all the triangles in the entire mesh and check which ones should be inside this node.
			for (int i = 0; i < TriangleCount; i++)
			{
				// If the triangle is inside the node then increment the count by one
				if(IsTriangleContained(i, positionX, positionZ, width))
					count++;
			}

			return count;
		}

		private bool IsTriangleContained(int index, float positionX, float positionZ, float width)
		{
			// Calculate the radius of this node
			var radius = width / 2;

			// Get the index to the vertex list
			var vertexIndex = index * 3;

			// Get the three vertices of this triangle from the vertex list
			var x1 = VertexList[vertexIndex].position.X;
			var z1 = VertexList[vertexIndex].position.Z;
			vertexIndex++;

			var x2 = VertexList[vertexIndex].position.X;
			var z2 = VertexList[vertexIndex].position.Z;
			vertexIndex++;

			var x3 = VertexList[vertexIndex].position.X;
			var z3 = VertexList[vertexIndex].position.Z;

			// Check to see if the minimum of the x coordinate of the triangle is inside the node
			var minimumX = Math.Min(x1, Math.Min(x2, x3));
			if (minimumX > (positionX + radius))
				return false;

			// Check to see if the maximum of the x coordinate of the triangle is inside the node
			var maximumX = Math.Max(x1, Math.Max(x2, x3));
			if (maximumX < (positionX - radius))
				return false;

			// Check to see if the minimum of the z coordinate of the triangle is inside the node
			var minimumZ = Math.Min(z1, Math.Min(z2, z3));
			if (minimumZ > (positionZ + radius))
				return false;

			// Check to see if the maximum of the x coordinate of the triangle is inside the node
			var maximumZ = Math.Max(z1, Math.Max(z2, z3));
			if (maximumZ < (positionZ - radius))
				return false;

			return true;
		}

		private void ReleaseNode(Node? node)
		{
			if (node == null)
				return;

			var theNode = node.Value;

			// Recursively go down the tree and release the bottom nodes first
			for (int i = 0; i < 4; i++)
			{
				if (theNode.Nodes[i] != null)
					ReleaseNode(theNode.Nodes[i]);
			}

			// Release the vertex buffer for this node
			if (theNode.VertexBuffer != null)
			{
				theNode.VertexBuffer.Dispose();
				theNode.VertexBuffer = null;
			}

			// Release the index buffer for this node
			if (theNode.IndexBuffer != null)
			{
				theNode.IndexBuffer.Dispose();
				theNode.IndexBuffer = null;
			}
		}

		private void RenderNode(DeviceContext deviceContext, Node? node, Frustum frustum, HeightMapTerrainShader shader)
		{
			if (node == null)
				return;

			var theNode = node.Value;

			// Check to see if the node can be viewed, height doesn't matter in a quad tree.
			// If it can't be seen then none of its children can either, so don't continue down the tree, this is where the speed is gained. 
			if (!frustum.CheckCube(theNode.PositionX, 0, theNode.PositionZ, theNode.Width / 2))
				return;

			// If it can be seen then check all four child nodes to if they can also be seen
			var count = 0;
			for (int i = 0; i < 4; i++)
			{
				if (theNode.Nodes[i] != null)
				{
					count++;
					RenderNode(deviceContext, theNode.Nodes[i], frustum, shader);
				}
			}

			// If there were any children nodes then there is no need to continue as parent nodes won't contain any triangles to render.
			if (count != 0)
				return;

			// Otherwise if this node can be seen and has triangles in it then render these triangles.

			// Set the vertex buffer to active in the input assembler so it can be rendered.
			deviceContext.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(theNode.VertexBuffer, Utilities.SizeOf<HeightMapTerrainShader.Vertex>(), 0));
			// Set the index buffer to active in the input assembler so it can be rendered.
			deviceContext.InputAssembler.SetIndexBuffer(theNode.IndexBuffer, Format.R32_UInt, 0);
			// Set the type of the primitive that should be rendered from this vertex buffer, in this case triangles.
			deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

			// Determine the number of the indices in this node.
			var indexCount = theNode.TriangleCount * 3;

			// Call the terrain shader to render the polygons in this node.
			shader.RenderShader(deviceContext, indexCount);

			// Increase the count of the number of polygons that have been rendered during this frame.
			DrawCount += theNode.TriangleCount;
		}
		#endregion

		#region Override Methods
		public object Clone()
		{
			return MemberwiseClone();
		}
		#endregion
	}
}