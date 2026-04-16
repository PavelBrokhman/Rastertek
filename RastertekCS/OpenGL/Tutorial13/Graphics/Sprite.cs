using System.Globalization;
using Silk.NET.OpenGL;

namespace RastertekCS.OpenGL.Tutorial13.Graphics;

public class Sprite
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
    private Texture[] _textures;
    private int _textureCount;
    private int _currentTexture;
    private float _cycleTime;
    private float _frameTime;

    private int _screenWidth,
        _screenHeight;
    private int _bitmapWidth,
        _bitmapHeight;
    private int _renderX,
        _renderY;

    public unsafe bool Initialize(
        GL4 OpenGL,
        int screenWidth,
        int screenHeight,
        string spriteFilename,
        uint textureUnit
    )
    {
        var gl = OpenGL.Driver;
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;
        _currentTexture = 0;
        _frameTime = 0;

        if (!LoadSpriteFile(spriteFilename))
            return false;

        _textures = new Texture[_textureCount];
        var lines = File.ReadAllLines(spriteFilename);
        int startLine = 3;
        for (int i = 0; i < _textureCount; i++)
        {
            var texFile = lines[startLine + i].Trim();
            _textures[i] = new Texture();
            if (!_textures[i].Initialize(OpenGL, texFile, textureUnit, false))
                return false;
        }

        _vertexCount = 6;
        _indexCount = 6;
        var vertices = new VertexType[_vertexCount];
        var indices = new uint[] { 0, 1, 2, 3, 4, 5 };

        _vertexArrayId = gl.GenVertexArray();
        gl.BindVertexArray(_vertexArrayId);

        _vertexBufferId = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vertexBufferId);
        fixed (VertexType* p = vertices)
            gl.BufferData(
                BufferTargetARB.ArrayBuffer,
                (nuint)(sizeof(VertexType) * vertices.Length),
                p,
                BufferUsageARB.DynamicDraw
            );

        gl.EnableVertexAttribArray(0);
        gl.EnableVertexAttribArray(1);
        gl.VertexAttribPointer(
            0,
            3,
            VertexAttribPointerType.Float,
            false,
            (uint)sizeof(VertexType),
            (void*)0
        );
        gl.VertexAttribPointer(
            1,
            2,
            VertexAttribPointerType.Float,
            false,
            (uint)sizeof(VertexType),
            (void*)(3 * sizeof(float))
        );

        _indexBufferId = gl.GenBuffer();
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _indexBufferId);
        fixed (uint* p = indices)
            gl.BufferData(
                BufferTargetARB.ElementArrayBuffer,
                (nuint)(sizeof(uint) * indices.Length),
                p,
                BufferUsageARB.StaticDraw
            );

        return true;
    }

    public void Shutdown(GL4 OpenGL)
    {
        if (_textures != null)
        {
            foreach (var t in _textures)
                t?.Shutdown(OpenGL);
            _textures = null;
        }
        var gl = OpenGL.Driver;
        gl.DisableVertexAttribArray(0);
        gl.DisableVertexAttribArray(1);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        gl.DeleteBuffer(_vertexBufferId);
        gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, 0);
        gl.DeleteBuffer(_indexBufferId);
        gl.BindVertexArray(0);
        gl.DeleteVertexArray(_vertexArrayId);
    }

    public void Update(float frameTimeMs)
    {
        _frameTime += frameTimeMs;
        if (_frameTime >= _cycleTime)
        {
            _frameTime -= _cycleTime;
            _currentTexture++;
            if (_currentTexture >= _textureCount)
                _currentTexture = 0;
        }
    }

    public void SetRenderLocation(int x, int y)
    {
        _renderX = x;
        _renderY = y;
    }

    public void SetTexture(GL4 OpenGL, uint textureUnit)
    {
        _textures[_currentTexture].SetTexture(OpenGL, textureUnit);
    }

    public unsafe void Render(GL4 OpenGL)
    {
        var gl = OpenGL.Driver;

        float left = -((float)_screenWidth / 2.0f) + _renderX;
        float right = left + _bitmapWidth;
        float top = ((float)_screenHeight / 2.0f) - _renderY;
        float bottom = top - _bitmapHeight;

        var v = new VertexType[6]
        {
            new()
            {
                x = left,
                y = top,
                z = 0,
                tu = 0,
                tv = 0,
            },
            new()
            {
                x = right,
                y = bottom,
                z = 0,
                tu = 1,
                tv = 1,
            },
            new()
            {
                x = left,
                y = bottom,
                z = 0,
                tu = 0,
                tv = 1,
            },
            new()
            {
                x = left,
                y = top,
                z = 0,
                tu = 0,
                tv = 0,
            },
            new()
            {
                x = right,
                y = top,
                z = 0,
                tu = 1,
                tv = 0,
            },
            new()
            {
                x = right,
                y = bottom,
                z = 0,
                tu = 1,
                tv = 1,
            },
        };

        gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vertexBufferId);
        fixed (VertexType* p = v)
            gl.BufferSubData(
                BufferTargetARB.ArrayBuffer,
                0,
                (nuint)(sizeof(VertexType) * v.Length),
                p
            );

        gl.BindVertexArray(_vertexArrayId);
        gl.DrawElements(
            PrimitiveType.Triangles,
            (uint)_indexCount,
            DrawElementsType.UnsignedInt,
            (void*)0
        );
    }

    private bool LoadSpriteFile(string filename)
    {
        if (!File.Exists(filename))
            return false;
        var lines = File.ReadAllLines(filename);
        // Format:
        //   TextureCount: N
        //   CycleTime: Ms
        //   BitmapSize: W H
        //   file1.tga
        //   file2.tga
        //   ...
        _textureCount = int.Parse(lines[0].Split(':')[1].Trim(), CultureInfo.InvariantCulture);
        _cycleTime = float.Parse(lines[1].Split(':')[1].Trim(), CultureInfo.InvariantCulture);
        var size = lines[2].Split(':')[1].Trim().Split(' ');
        _bitmapWidth = int.Parse(size[0], CultureInfo.InvariantCulture);
        _bitmapHeight = int.Parse(size[1], CultureInfo.InvariantCulture);
        return true;
    }
}
