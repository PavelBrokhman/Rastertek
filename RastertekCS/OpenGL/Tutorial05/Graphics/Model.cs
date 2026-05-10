using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial05.Graphics;

public class Model
{
    private struct VertexType
    {
        public float x,
            y,
            z;
        public float tu,
            tv;
    }

    private uint _vertexArrayId;
    private uint _vertexBufferId;
    private uint _indexBufferId;
    private int _vertexCount;
    private int _indexCount;

    private Texture _texture;

    public bool Initialize(GL4 OpenGL, string textureFilename, bool wrap)
    {
        // Initialize the vertex and index buffer that hold the geometry for the triangle.
        if (!InitializeBuffers(OpenGL))
            return false;

        // Load the texture for this model.
        if (!LoadTexture(OpenGL, textureFilename, wrap))
            return false;

        return true;
    }

    public void Shutdown(GL4 OpenGL)
    {
        // Release the texture used for this model.
        ReleaseTexture(OpenGL);

        // Release the vertex and index buffers.
        ShutdownBuffers(OpenGL);
    }

    public void Render(GL4 OpenGL)
    {
        // Put the vertex and index buffers on the graphics pipeline to prepare them for drawing.
        RenderBuffers(OpenGL);
    }

    public void SetTexture(GL4 OpenGL, uint textureUnit)
    {
        // Set the texture for the model.
        _texture.SetTexture(OpenGL, textureUnit);
    }

    private unsafe bool InitializeBuffers(GL4 OpenGL)
    {
        var gl = OpenGL.Driver;

        // Set the number of vertices in the vertex array.
        _vertexCount = 3;

        // Set the number of indices in the index array.
        _indexCount = 3;

        // Create the vertex array.
        var vertices = new VertexType[_vertexCount];

        // Create the index array.
        var indices = new uint[_indexCount];

        // Load the vertex array with data.

        // Bottom left.
        vertices[0].x = -1.0f; vertices[0].y = -1.0f; vertices[0].z =  0.0f;
        vertices[0].tu =  0.0f; vertices[0].tv =  0.0f;

        // Top middle.
        vertices[1].x =  0.0f; vertices[1].y =  1.0f; vertices[1].z =  0.0f;
        vertices[1].tu =  0.5f; vertices[1].tv =  1.0f;

        // Bottom right.
        vertices[2].x =  1.0f; vertices[2].y = -1.0f; vertices[2].z =  0.0f;
        vertices[2].tu =  1.0f; vertices[2].tv =  0.0f;

        // Load the index array with data.
        indices[0] = 0;
        indices[1] = 1;
        indices[2] = 2;

        // Allocate an OpenGL vertex array object.
        _vertexArrayId = gl.GenVertexArray();

        // Bind the vertex array object to store all the buffers and vertex attributes we create here.
        gl.BindVertexArray(_vertexArrayId);

        // Generate an ID for the vertex buffer.
        _vertexBufferId = gl.GenBuffer();

        // Bind the vertex buffer and load the vertex (position and color) data into the vertex buffer.
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vertexBufferId);
        fixed (VertexType* p = vertices)
        {
            gl.BufferData(
                BufferTargetARB.ArrayBuffer,
                (nuint)(_vertexCount * sizeof(VertexType)),
                p,
                BufferUsageARB.StaticDraw
            );
        }

        // Enable the two vertex array attributes.
        gl.EnableVertexAttribArray(0); // Vertex position.
        gl.EnableVertexAttribArray(1); // Texture coordinates.

        // Specify the location and format of the position portion of the vertex buffer.
        gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, (uint)sizeof(VertexType), (void*)0);

        // Specify the location and format of the texture coordinates portion of the vertex buffer.
        gl.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, (uint)sizeof(VertexType), (void*)(3 * sizeof(float)));

        // Generate an ID for the index buffer.
        _indexBufferId = gl.GenBuffer();

        // Bind the index buffer and load the index data into it.
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _indexBufferId);
        fixed (uint* p = indices)
        {
            gl.BufferData(
                BufferTargetARB.ElementArrayBuffer,
                (nuint)(_indexCount * sizeof(uint)),
                p,
                BufferUsageARB.StaticDraw
            );
        }

        return true;
    }

    private void ShutdownBuffers(GL4 OpenGL)
    {
        var gl = OpenGL.Driver;

        // Disable the two vertex array attributes.
        gl.DisableVertexAttribArray(0);
        gl.DisableVertexAttribArray(1);

        // Release the vertex buffer.
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        gl.DeleteBuffer(_vertexBufferId);

        // Release the index buffer.
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
        gl.DeleteBuffer(_indexBufferId);

        // Release the vertex array object.
        gl.BindVertexArray(0);
        gl.DeleteVertexArray(_vertexArrayId);
    }

    private unsafe void RenderBuffers(GL4 OpenGL)
    {
        var gl = OpenGL.Driver;

        // Bind the vertex array object that stored all the information about the vertex and index buffers.
        gl.BindVertexArray(_vertexArrayId);

        // Render the vertex buffer using the index buffer.
        gl.DrawElements(PrimitiveType.Triangles, (uint)_indexCount, DrawElementsType.UnsignedInt, (void*)0);
    }

    private bool LoadTexture(GL4 OpenGL, string textureFilename, bool wrap)
    {
        // Create and initialize the texture object.
        _texture = new Texture();
        return _texture.Initialize(OpenGL, textureFilename, wrap);
    }

    private void ReleaseTexture(GL4 OpenGL)
    {
        if (_texture != null)
        {
            _texture.Shutdown(OpenGL);
            _texture = null;
        }
    }
}
