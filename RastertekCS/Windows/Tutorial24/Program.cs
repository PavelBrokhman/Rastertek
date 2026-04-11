using System.Globalization;

namespace RastertekCS.Windows.Tutorial24;

// Wavefront .obj → Rastertek .txt converter (Maya right-handed → DX11 left-handed).
// Mirrors Rastertek DX11 tut24 (https://www.rastertek.com/dx11win10tut24.html):
//   - Inverts Z on vertex positions and normals
//   - Inverts V on texture coordinates
//   - Reverses face winding order
internal static class Program
{
    private struct Vec3 { public float X, Y, Z; }

    private struct Face
    {
        public int V1, V2, V3;
        public int T1, T2, T3;
        public int N1, N2, N3;
    }

    private static int Main(string[] args)
    {
        string baseDir = AppContext.BaseDirectory;
        string filename;
        if (args.Length > 0)
        {
            filename = args[0];
        }
        else
        {
            // Default to the bundled Maya cube so the converter just works.
            filename = Path.Combine("ExternalModels", "cube_maya.obj");
            Console.WriteLine($"No filename argument; defaulting to {filename}");
        }

        if (!Path.IsPathRooted(filename))
        {
            // Resolve relative to where the dll lives, not the current working dir.
            filename = Path.Combine(baseDir, filename);
        }

        if (!File.Exists(filename))
        {
            Console.WriteLine($"File {filename} could not be opened.");
            return -1;
        }

        var vertices = new List<Vec3>();
        var texcoords = new List<Vec3>();
        var normals = new List<Vec3>();
        var faces = new List<Face>();

        var inv = CultureInfo.InvariantCulture;
        foreach (var raw in File.ReadLines(filename))
        {
            var line = raw.Trim();
            if (line.Length == 0 || line[0] == '#') continue;
            var t = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (t.Length == 0) continue;

            switch (t[0])
            {
                case "v":
                    vertices.Add(new Vec3
                    {
                        X = float.Parse(t[1], inv),
                        Y = float.Parse(t[2], inv),
                        Z = -float.Parse(t[3], inv) // invert Z
                    });
                    break;

                case "vt":
                    texcoords.Add(new Vec3
                    {
                        X = float.Parse(t[1], inv),
                        Y = 1.0f - float.Parse(t[2], inv) // invert V
                    });
                    break;

                case "vn":
                    normals.Add(new Vec3
                    {
                        X = float.Parse(t[1], inv),
                        Y = float.Parse(t[2], inv),
                        Z = -float.Parse(t[3], inv) // invert Z
                    });
                    break;

                case "f":
                {
                    if (t.Length < 4) break;
                    var (v1, vt1, vn1) = ParseFaceVertex(t[1]);
                    var (v2, vt2, vn2) = ParseFaceVertex(t[2]);
                    var (v3, vt3, vn3) = ParseFaceVertex(t[3]);
                    // Reverse winding (3,2,1) for left-hand system.
                    faces.Add(new Face
                    {
                        V1 = v3, V2 = v2, V3 = v1,
                        T1 = vt3, T2 = vt2, T3 = vt1,
                        N1 = vn3, N2 = vn2, N3 = vn1
                    });
                    break;
                }
            }
        }

        if (faces.Count == 0)
        {
            Console.WriteLine("No faces found in input.");
            return -1;
        }

        Console.WriteLine();
        Console.WriteLine($"Vertices: {vertices.Count}");
        Console.WriteLine($"UVs:      {texcoords.Count}");
        Console.WriteLine($"Normals:  {normals.Count}");
        Console.WriteLine($"Faces:    {faces.Count}");

        string outputPath = Path.Combine(baseDir, "model.txt");
        using var fout = new StreamWriter(outputPath);
        fout.WriteLine($"Vertex Count: {faces.Count * 3}");
        fout.WriteLine();
        fout.WriteLine("Data:");
        fout.WriteLine();

        foreach (var f in faces)
        {
            WriteVertex(fout, vertices, texcoords, normals, f.V1, f.T1, f.N1);
            WriteVertex(fout, vertices, texcoords, normals, f.V2, f.T2, f.N2);
            WriteVertex(fout, vertices, texcoords, normals, f.V3, f.T3, f.N3);
        }

        Console.WriteLine();
        Console.WriteLine($"File has been converted: {outputPath}");
        return 0;
    }

    private static (int v, int vt, int vn) ParseFaceVertex(string token)
    {
        var parts = token.Split('/');
        int v = int.Parse(parts[0], CultureInfo.InvariantCulture);
        int vt = parts.Length > 1 && parts[1].Length > 0 ? int.Parse(parts[1], CultureInfo.InvariantCulture) : 0;
        int vn = parts.Length > 2 && parts[2].Length > 0 ? int.Parse(parts[2], CultureInfo.InvariantCulture) : 0;
        return (v, vt, vn);
    }

    private static void WriteVertex(StreamWriter w, List<Vec3> v, List<Vec3> t, List<Vec3> n,
        int vi, int ti, int ni)
    {
        var inv = CultureInfo.InvariantCulture;
        var p = v[vi - 1];
        var tc = ti > 0 ? t[ti - 1] : new Vec3 { X = 0, Y = 0 };
        var nm = ni > 0 ? n[ni - 1] : new Vec3 { X = 0, Y = 0, Z = 1 };
        w.WriteLine(
            $"{p.X.ToString("0.0######", inv)} {p.Y.ToString("0.0######", inv)} {p.Z.ToString("0.0######", inv)} " +
            $"{tc.X.ToString("0.0######", inv)} {tc.Y.ToString("0.0######", inv)} " +
            $"{nm.X.ToString("0.0######", inv)} {nm.Y.ToString("0.0######", inv)} {nm.Z.ToString("0.0######", inv)}");
    }
}
