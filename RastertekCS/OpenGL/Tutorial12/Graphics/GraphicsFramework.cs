namespace RastertekCS.OpenGL.Tutorial12.Graphics;

public class GraphicsFramework
{
    private const uint TEXTURE_UNIT = 0;
    private const int BITMAP_SIZE = 256;

    private GL4 _openGL;
    private Camera _camera;
    private TextureShader _textureShader;
    private Bitmap _bitmap;
    private int _screenWidth,
        _screenHeight;

    public bool Initialize(GL4 OpenGL, int screenWidth, int screenHeight)
    {
        _openGL = OpenGL;
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;

        _camera = new Camera();
        _camera.SetPosition(0.0f, 0.0f, -1.0f);

        _textureShader = new TextureShader();
        if (!_textureShader.Initialize(OpenGL))
            return false;

        _bitmap = new Bitmap();
        if (
            !_bitmap.Initialize(
                OpenGL,
                screenWidth,
                screenHeight,
                "Data/Stone01.tga",
                TEXTURE_UNIT,
                BITMAP_SIZE,
                BITMAP_SIZE
            )
        )
            return false;
        _bitmap.SetRenderLocation(100, 100);

        return true;
    }

    public void Shutdown()
    {
        _bitmap?.Shutdown(_openGL);
        _textureShader?.Shutdown(_openGL);
        _bitmap = null;
        _textureShader = null;
        _camera = null;
        _openGL = null;
    }

    public bool Frame() => Render();

    private bool Render()
    {
        _openGL.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

        _camera.Render();
        var world = _openGL.GetWorldMatrix();
        var view = _camera.GetViewMatrix();
        var ortho = _openGL.GetOrthoMatrix();

        // Отключаем z-buffer для 2D рендеринга.
        _openGL.TurnZBufferOff();

        _textureShader.SetShader(_openGL);
        if (!_textureShader.SetShaderParameters(_openGL, world, view, ortho, (int)TEXTURE_UNIT))
            return false;
        _bitmap.Render(_openGL);

        _openGL.TurnZBufferOn();

        _openGL.EndScene();
        return true;
    }
}
