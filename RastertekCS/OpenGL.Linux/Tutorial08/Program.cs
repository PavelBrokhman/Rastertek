// OBJ -> Rastertek .txt model converter.
//
// Читает Wavefront .obj и пишет упрощённый текстовый формат:
//   Vertex Count: N
//
//   Data:
//
//   x y z  tu tv  nx ny nz   (по одной строке на вершину, треугольники подряд)
//
// В отличие от оригинального Rastertek (Maya RH -> DX LH), здесь
// используется OpenGL (RH), поэтому Z-координаты и winding НЕ инвертируются.
// tv инвертируется (1 - tv), чтобы соответствовать ожидаемому формату
// Model.cs из Tutorial 7 (который снова делает 1 - tv при загрузке).

using System.Globalization;

namespace RastertekCS.OpenGL.Tutorial08;

internal static class Program
{
    private static int Main(string[] args)
    {
        string objPath;
        if (args.Length > 0)
        {
            objPath = args[0];
        }
        else
        {
            Console.Write("Путь к .obj файлу: ");
            objPath = Console.ReadLine()?.Trim() ?? "";
        }

        if (string.IsNullOrEmpty(objPath) || !File.Exists(objPath))
        {
            Console.WriteLine($"Файл не найден: {objPath}");
            return 1;
        }

        string outPath = Path.ChangeExtension(objPath, ".txt");

        if (!Convert(objPath, outPath))
        {
            Console.WriteLine("Конвертация не удалась.");
            return 1;
        }

        Console.WriteLine($"Успешно: {outPath}");
        return 0;
    }

    private static bool Convert(string objPath, string outPath)
    {
        var positions = new List<(float x, float y, float z)>();
        var texcoords = new List<(float u, float v)>();
        var normals = new List<(float x, float y, float z)>();
        var faces = new List<(int vi, int ti, int ni)[]>();

        foreach (var rawLine in File.ReadLines(objPath))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line[0] == '#') continue;
            var t = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (t.Length == 0) continue;

            switch (t[0])
            {
                case "v":
                    positions.Add((
                        float.Parse(t[1], CultureInfo.InvariantCulture),
                        float.Parse(t[2], CultureInfo.InvariantCulture),
                        float.Parse(t[3], CultureInfo.InvariantCulture)));
                    break;
                case "vt":
                    texcoords.Add((
                        float.Parse(t[1], CultureInfo.InvariantCulture),
                        float.Parse(t[2], CultureInfo.InvariantCulture)));
                    break;
                case "vn":
                    normals.Add((
                        float.Parse(t[1], CultureInfo.InvariantCulture),
                        float.Parse(t[2], CultureInfo.InvariantCulture),
                        float.Parse(t[3], CultureInfo.InvariantCulture)));
                    break;
                case "f":
                {
                    // Поддерживаем формат v/t/n (треугольники и n-угольники).
                    var verts = new List<(int vi, int ti, int ni)>();
                    for (int k = 1; k < t.Length; k++)
                    {
                        var parts = t[k].Split('/');
                        int vi = int.Parse(parts[0], CultureInfo.InvariantCulture);
                        int ti = parts.Length > 1 && parts[1].Length > 0 ? int.Parse(parts[1], CultureInfo.InvariantCulture) : 0;
                        int ni = parts.Length > 2 && parts[2].Length > 0 ? int.Parse(parts[2], CultureInfo.InvariantCulture) : 0;
                        verts.Add((vi, ti, ni));
                    }
                    // Триангулируем fan-ом если больше 3 вершин.
                    for (int k = 1; k < verts.Count - 1; k++)
                    {
                        faces.Add(new[] { verts[0], verts[k], verts[k + 1] });
                    }
                    break;
                }
            }
        }

        int vertexCount = faces.Count * 3;
        if (vertexCount == 0)
        {
            Console.WriteLine("В файле не найдено треугольников.");
            return false;
        }

        using var w = new StreamWriter(outPath);
        w.WriteLine($"Vertex Count: {vertexCount}");
        w.WriteLine();
        w.WriteLine("Data:");
        w.WriteLine();

        var inv = CultureInfo.InvariantCulture;
        foreach (var face in faces)
        {
            foreach (var (vi, ti, ni) in face)
            {
                var p = positions[vi - 1];
                (float u, float v) tc = ti > 0 ? texcoords[ti - 1] : (0f, 0f);
                (float x, float y, float z) n = ni > 0 ? normals[ni - 1] : (0f, 0f, 1f);

                // Инвертируем tv под формат Rastertek .txt (согласуется с Model.cs).
                float tv = 1.0f - tc.v;

                w.WriteLine(
                    $"{p.x.ToString("0.0######", inv)} {p.y.ToString("0.0######", inv)} {p.z.ToString("0.0######", inv)}  " +
                    $"{tc.u.ToString("0.0######", inv)} {tv.ToString("0.0######", inv)}  " +
                    $"{n.x.ToString("0.0######", inv)} {n.y.ToString("0.0######", inv)} {n.z.ToString("0.0######", inv)}");
            }
        }

        return true;
    }
}
