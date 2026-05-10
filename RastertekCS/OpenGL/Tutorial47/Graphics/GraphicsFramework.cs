using RastertekCS.OpenGL.Tutorial47.Inputs;
using Silk.NET.Maths;

namespace RastertekCS.OpenGL.Tutorial47.Graphics;

public class GraphicsFramework
{
    private GL4 _driver;
    private Camera _camera;
    private Model _model;
    private Light _light;
    private LightShader _lightShader;
    private FontShader _fontShader;
    private Font _font;
    private Text _textString;
    private Bitmap _mouseBitmap;
    private TextureShader _textureShader;
    private int _screenWidth, _screenHeight;

    public bool Initialize(GL4 OpenGL, int screenWidth, int screenHeight)
    {
        _driver = OpenGL;
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;

        _camera = new Camera();
        _camera.SetPosition(0.0f, 0.0f, -10.0f);
        _camera.Render();
        _camera.RenderBaseViewMatrix();

        _model = new Model();
        if (!_model.Initialize(OpenGL, "Models/sphere.txt", "Data/blue.tga")) return false;

        _light = new Light();
        _light.SetDirection(0.0f, 0.0f, 1.0f);
        _light.SetDiffuseColor(1.0f, 1.0f, 1.0f, 1.0f);

        _lightShader = new LightShader();
        if (!_lightShader.Initialize(OpenGL)) return false;

        _fontShader = new FontShader();
        if (!_fontShader.Initialize(OpenGL)) return false;

        _font = new Font();
        if (!_font.Initialize(OpenGL, 0)) return false;

        _textString = new Text();
        if (!_textString.Initialize(OpenGL, screenWidth, screenHeight, 32, _font, "Intersection: No", 10, 10, 0.0f, 1.0f, 0.0f)) return false;

        _mouseBitmap = new Bitmap();
        if (!_mouseBitmap.Initialize(OpenGL, screenWidth, screenHeight, "Data/mouse.tga", 50, 50)) return false;

        _textureShader = new TextureShader();
        if (!_textureShader.Initialize(OpenGL)) return false;

        return true;
    }

    public void Shutdown()
    {
        _textureShader?.Shutdown(_driver); _textureShader = null;
        _mouseBitmap?.Shutdown(_driver); _mouseBitmap = null;
        _textString?.Shutdown(_driver); _textString = null;
        _font?.Shutdown(_driver); _font = null;
        _fontShader?.Shutdown(_driver); _fontShader = null;
        _lightShader?.Shutdown(_driver); _lightShader = null;
        _light = null;
        _model?.Shutdown(_driver); _model = null;
        _camera = null;
        _driver = null;
    }

    public bool Frame(Input input)
    {
        input.GetMouseLocation(out int mouseX, out int mouseY);
        _mouseBitmap.SetRenderLocation(mouseX, mouseY);

        bool intersect = TestIntersection(mouseX, mouseY);
        string text = intersect ? "Intersection: Yes" : "Intersection: No";
        if (!_textString.UpdateText(_driver, _font, text, 10, 10, 0.0f, 1.0f, 0.0f)) return false;

        return Render();
    }

    private bool Render()
    {
        _driver.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);
        var worldMatrix = _driver.GetWorldMatrix();
        var viewMatrix = _camera.GetViewMatrix();
        var projectionMatrix = _driver.GetProjectionMatrix();
        var baseViewMatrix = _camera.GetBaseViewMatrix();
        var orthoMatrix = _driver.GetOrthoMatrix();

        var translateMatrix = Matrix4X4.CreateTranslation<float>(-5.0f, 1.0f, 5.0f);

        if (!_lightShader.SetShaderParameters(_driver, translateMatrix, viewMatrix, projectionMatrix, 0,
                _light.GetDirection(), _light.GetDiffuseColor())) return false;
        _model.SetTexture(_driver, 0);
        _model.Render(_driver);

        _driver.TurnZBufferOff();
        _driver.EnableAlphaBlending();

        if (!_fontShader.SetShaderParameters(_driver, worldMatrix, baseViewMatrix, orthoMatrix, 0, _textString.GetPixelColor())) return false;
        _font.SetTexture(_driver, 0);
        _textString.Render(_driver);

        if (!_textureShader.SetShaderParameters(_driver, worldMatrix, baseViewMatrix, orthoMatrix, 0)) return false;
        _mouseBitmap.SetTexture(_driver, 0);
        if (!_mouseBitmap.Render(_driver)) return false;

        _driver.TurnZBufferOn();
        _driver.DisableAlphaBlending();

        _driver.EndScene();
        return true;
    }

    private bool TestIntersection(int mouseX, int mouseY)
    {
        float pointX = (2.0f * mouseX / _screenWidth) - 1.0f;
        float pointY = -((2.0f * mouseY / _screenHeight) - 1.0f);

        var projectionMatrix = _driver.GetProjectionMatrix();
        pointX /= projectionMatrix.M11;
        pointY /= projectionMatrix.M22;

        var viewMatrix = _camera.GetViewMatrix();
        if (!Matrix4X4.Invert(viewMatrix, out var iView)) return false;

        var direction = new Vector3D<float>(
            pointX * iView.M11 + pointY * iView.M21 + iView.M31,
            pointX * iView.M12 + pointY * iView.M22 + iView.M32,
            pointX * iView.M13 + pointY * iView.M23 + iView.M33);

        var origin = _camera.GetPosition();

        var worldMatrix = Matrix4X4.CreateTranslation<float>(-5.0f, 1.0f, 5.0f);
        if (!Matrix4X4.Invert(worldMatrix, out var iWorld)) return false;

        var rayOrigin = Vector3D.Transform(origin, iWorld);
        var rayDirection = Vector3D.Normalize(TransformNormal(direction, iWorld));

        return RaySphereIntersect(rayOrigin, rayDirection, 1.0f);
    }

    private static Vector3D<float> TransformNormal(Vector3D<float> n, Matrix4X4<float> m) =>
        new(n.X * m.M11 + n.Y * m.M21 + n.Z * m.M31,
            n.X * m.M12 + n.Y * m.M22 + n.Z * m.M32,
            n.X * m.M13 + n.Y * m.M23 + n.Z * m.M33);

    private static bool RaySphereIntersect(Vector3D<float> rayOrigin, Vector3D<float> rayDirection, float radius)
    {
        float a = rayDirection.X * rayDirection.X + rayDirection.Y * rayDirection.Y + rayDirection.Z * rayDirection.Z;
        float b = (rayDirection.X * rayOrigin.X + rayDirection.Y * rayOrigin.Y + rayDirection.Z * rayOrigin.Z) * 2.0f;
        float c = rayOrigin.X * rayOrigin.X + rayOrigin.Y * rayOrigin.Y + rayOrigin.Z * rayOrigin.Z - radius * radius;
        float discriminant = b * b - 4 * a * c;
        return discriminant >= 0.0f;
    }
}
