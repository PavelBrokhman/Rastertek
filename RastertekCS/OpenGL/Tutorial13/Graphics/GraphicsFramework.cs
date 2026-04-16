namespace RastertekCS.OpenGL.Tutorial13.Graphics;

public class GraphicsFramework
{
    private const uint TEXTURE_UNIT = 0;

    private GL4 _openGL;
    private Camera _camera;
    private TextureShader _textureShader;
    private Sprite _sprite;

    public bool Initialize(GL4 OpenGL, int screenWidth, int screenHeight)
    {
        _openGL = OpenGL;
        _camera = new Camera();
        _camera.SetPosition(0.0f, 0.0f, -1.0f);

        _textureShader = new TextureShader();
        if (!_textureShader.Initialize(OpenGL))
            return false;

        _sprite = new Sprite();
        if (!_sprite.Initialize(OpenGL, screenWidth, screenHeight, "Data/Sprite.txt", TEXTURE_UNIT))
            return false;
        _sprite.SetRenderLocation(100, 100);
        return true;
    }

    public void Shutdown()
    {
        _sprite?.Shutdown(_openGL);
        _textureShader?.Shutdown(_openGL);
        _sprite = null;
        _textureShader = null;
        _camera = null;
        _openGL = null;
    }

    public bool Frame(float frameTimeMs)
    {
        _sprite.Update(frameTimeMs);
        return Render();
    }

    private bool Render()
    {
        _openGL.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);
        _camera.Render();
        var world = _openGL.GetWorldMatrix();
        var view = _camera.GetViewMatrix();
        var ortho = _openGL.GetOrthoMatrix();

        _openGL.TurnZBufferOff();

        _textureShader.SetShader(_openGL);
        if (!_textureShader.SetShaderParameters(_openGL, world, view, ortho, (int)TEXTURE_UNIT))
            return false;
        _sprite.SetTexture(_openGL, TEXTURE_UNIT);
        _sprite.Render(_openGL);

        _openGL.TurnZBufferOn();
        _openGL.EndScene();
        return true;
    }
}
