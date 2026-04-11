using System.Globalization;

namespace RastertekCS.Tools.ObjToModelLH;

/// <summary>
/// OBJ to Rastertek model format converter.
/// Reads a Wavefront .obj file and outputs a model.txt in the engine's format.
/// Converts from Blender's coordinate system to left-hand system.
/// </summary>
internal static class Program
{
    struct Vector3 { public float X, Y, Z; }
    struct Face { public int V1, V2, V3, T1, T2, T3, N1, N2, N3; }

    static int Main()
    {
        Console.Write("Enter model filename: ");
        string filename = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(filename) || !File.Exists(filename))
        {
            Console.WriteLine($"File {filename} could not be opened.");
            return -1;
        }

        var vertices = new List<Vector3>();
        var texcoords = new List<Vector3>();
        var normals = new List<Vector3>();
        var faces = new List<Face>();

        foreach (var line in File.ReadLines(filename))
        {
            if (line.StartsWith("v "))
            {
                var p = line.Substring(2).Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                float x = float.Parse(p[0], CultureInfo.InvariantCulture);
                float y = float.Parse(p[1], CultureInfo.InvariantCulture);
                float z = float.Parse(p[2], CultureInfo.InvariantCulture);
                // Convert Blender to LH: swap X and Z, then negate both
                vertices.Add(new Vector3 { X = -z, Y = y, Z = -x });
            }
            else if (line.StartsWith("vt "))
            {
                var p = line.Substring(3).Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                float u = float.Parse(p[0], CultureInfo.InvariantCulture);
                float v = float.Parse(p[1], CultureInfo.InvariantCulture);
                // Invert V for LH
                texcoords.Add(new Vector3 { X = u, Y = 1.0f - v, Z = 0 });
            }
            else if (line.StartsWith("vn "))
            {
                var p = line.Substring(3).Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                float x = float.Parse(p[0], CultureInfo.InvariantCulture);
                float y = float.Parse(p[1], CultureInfo.InvariantCulture);
                float z = float.Parse(p[2], CultureInfo.InvariantCulture);
                // Convert Blender to LH: swap X and Z, then negate both
                normals.Add(new Vector3 { X = -z, Y = y, Z = -x });
            }
            else if (line.StartsWith("f "))
            {
                var p = line.Substring(2).Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (p.Length >= 3)
                {
                    // Read in reverse order for LH winding
                    var f3 = ParseFaceVertex(p[0]);
                    var f2 = ParseFaceVertex(p[1]);
                    var f1 = ParseFaceVertex(p[2]);
                    faces.Add(new Face
                    {
                        V1 = f1.v, V2 = f2.v, V3 = f3.v,
                        T1 = f1.t, T2 = f2.t, T3 = f3.t,
                        N1 = f1.n, N2 = f2.n, N3 = f3.n
                    });
                }
            }
        }

        Console.WriteLine();
        Console.WriteLine($"Vertices: {vertices.Count}");
        Console.WriteLine($"UVs:      {texcoords.Count}");
        Console.WriteLine($"Normals:  {normals.Count}");
        Console.WriteLine($"Faces:    {faces.Count}");

        using (var w = new StreamWriter("model.txt"))
        {
            w.WriteLine($"Vertex Count: {faces.Count * 3}");
            w.WriteLine();
            w.WriteLine("Data:");
            w.WriteLine();

            var inv = CultureInfo.InvariantCulture;
            foreach (var f in faces)
            {
                WriteFaceVertex(w, vertices[f.V1 - 1], texcoords[f.T1 - 1], normals[f.N1 - 1], inv);
                WriteFaceVertex(w, vertices[f.V2 - 1], texcoords[f.T2 - 1], normals[f.N2 - 1], inv);
                WriteFaceVertex(w, vertices[f.V3 - 1], texcoords[f.T3 - 1], normals[f.N3 - 1], inv);
            }
        }

        Console.WriteLine("\nFile has been converted.");
        return 0;
    }

    static (int v, int t, int n) ParseFaceVertex(string s)
    {
        var parts = s.Split('/');
        int v = int.Parse(parts[0]);
        int t = parts.Length > 1 && parts[1].Length > 0 ? int.Parse(parts[1]) : 0;
        int n = parts.Length > 2 ? int.Parse(parts[2]) : 0;
        return (v, t, n);
    }

    static void WriteFaceVertex(StreamWriter w, Vector3 v, Vector3 t, Vector3 n, CultureInfo inv)
    {
        w.WriteLine($"{v.X.ToString(inv)} {v.Y.ToString(inv)} {v.Z.ToString(inv)} {t.X.ToString(inv)} {t.Y.ToString(inv)} {n.X.ToString(inv)} {n.Y.ToString(inv)} {n.Z.ToString(inv)}");
    }
}
