using RastertekCS.OpenGL.Tutorial23.Inputs;
using Silk.NET.Input;
using Silk.NET.Maths;

namespace RastertekCS.OpenGL.Tutorial23.Graphics;

public class GraphicsFramework
{
    private const uint TEXTURE_UNIT = 0;

    private GL4 m_OpenGL;
    private Input m_Input;
    private Camera m_Camera;
    private Model m_Model;
    private Light m_Light;
    private LightShader m_LightShader;
    private FontShader m_FontShader;
    private Font m_Font;
    private Text m_RenderCountString;
    private Position m_Position;
    private ModelList m_ModelList;
    private Frustum m_Frustum;
    private Matrix4X4<float> m_baseViewMatrix;
    private int m_screenWidth, m_screenHeight;

    public bool Initialize(GL4 OpenGL, Input input, int screenWidth, int screenHeight)
    {
        m_OpenGL = OpenGL;
        m_Input = input;
        m_screenWidth = screenWidth;
        m_screenHeight = screenHeight;

        // Create and initialize the camera.
        m_Camera = new Camera();
        m_Camera.SetPosition(0.0f, 0.0f, -10.0f);
        m_Camera.Render();
        m_baseViewMatrix = m_Camera.GetViewMatrix();

        // Create and initialize the model.
        m_Model = new Model();
        if (!m_Model.Initialize(OpenGL, "Data/sphere.txt", "Data/stone01.tga", TEXTURE_UNIT)) return false;

        // Create and initialize the light.
        m_Light = new Light();
        m_Light.SetDiffuseColor(1.0f, 1.0f, 1.0f, 1.0f);
        m_Light.SetDirection(0.0f, 0.0f, 1.0f);

        // Create and initialize the light shader.
        m_LightShader = new LightShader();
        if (!m_LightShader.Initialize(OpenGL)) return false;

        // Create and initialize the font shader.
        m_FontShader = new FontShader();
        if (!m_FontShader.Initialize(OpenGL)) return false;

        // Create and initialize the font.
        m_Font = new Font();
        if (!m_Font.Initialize(OpenGL, "Data/font01.txt", "Data/font01.tga", TEXTURE_UNIT)) return false;

        // Create and initialize the render count text.
        m_RenderCountString = new Text();
        if (!m_RenderCountString.Initialize(OpenGL, m_Font, "Render Count: 0",
            10, 10, 1.0f, 1.0f, 1.0f, screenWidth, screenHeight, 32)) return false;

        // Create the position object.
        m_Position = new Position();

        // Create and initialize the model list.
        m_ModelList = new ModelList();
        m_ModelList.Initialize(25);

        // Create the frustum object.
        m_Frustum = new Frustum();

        return true;
    }

    public void Shutdown()
    {
        m_Frustum = null;
        m_ModelList?.Shutdown(); m_ModelList = null;
        m_Position = null;
        m_RenderCountString?.Shutdown(m_OpenGL); m_RenderCountString = null;
        m_Font?.Shutdown(m_OpenGL); m_Font = null;
        m_FontShader?.Shutdown(m_OpenGL); m_FontShader = null;
        m_LightShader?.Shutdown(m_OpenGL); m_LightShader = null;
        m_Light = null;
        m_Model?.Shutdown(m_OpenGL); m_Model = null;
        m_Camera = null;
        m_Input = null;
        m_OpenGL = null;
    }

    public bool Frame(RastertekCS.OpenGL.Tutorial23.System.Timer timer)
    {
        // Set the frame time for position calculations.
        m_Position.SetFrameTime(timer.GetTime());

        // Check if left or right arrow keys are pressed.
        bool leftKey = m_Input.IsKeyDown(Key.Left);
        m_Position.TurnLeft(leftKey);

        bool rightKey = m_Input.IsKeyDown(Key.Right);
        m_Position.TurnRight(rightKey);

        // Get the current rotation and set the camera.
        float rotationY = m_Position.GetRotation();
        m_Camera.SetRotation(0.0f, rotationY, 0.0f);
        m_Camera.Render();

        return Render();
    }

    private bool Render()
    {
        m_OpenGL.BeginScene(0.0f, 0.0f, 0.0f, 1.0f);

        var world = m_OpenGL.GetWorldMatrix();
        var view = m_Camera.GetViewMatrix();
        var projection = m_OpenGL.GetProjectionMatrix();
        var ortho = m_OpenGL.GetOrthoMatrix();

        // Construct the frustum.
        m_Frustum.ConstructFrustum(view, projection);

        // Get light properties.
        var lightDirection = m_Light.GetDirection();
        var diffuseLightColor = m_Light.GetDiffuseColor();

        // Get the number of models.
        int modelCount = m_ModelList.GetModelCount();
        int renderCount = 0;

        // Go through all models and render only those in the frustum.
        for (int i = 0; i < modelCount; i++)
        {
            m_ModelList.GetData(i, out float posX, out float posY, out float posZ);

            // Check if the sphere (radius 1.0) is in the frustum.
            if (m_Frustum.CheckSphere(posX, posY, posZ, 1.0f))
            {
                // Create translation matrix for this model position.
                var worldTranslated = Matrix4X4.CreateTranslation(posX, posY, posZ);

                // Set the light shader parameters.
                if (!m_LightShader.SetShaderParameters(m_OpenGL, worldTranslated, view, projection,
                    (int)TEXTURE_UNIT, lightDirection, diffuseLightColor))
                    return false;

                // Render the sphere.
                m_Model.SetTexture(m_OpenGL, TEXTURE_UNIT);
                m_Model.Render(m_OpenGL);

                renderCount++;
            }
        }

        // Disable Z buffer and enable alpha blending for 2D rendering.
        m_OpenGL.TurnZBufferOff();
        m_OpenGL.EnableAlphaBlending();

        // Disable face culling for text rendering.
        m_OpenGL.Gl.Disable(Silk.NET.OpenGL.EnableCap.CullFace);

        // Update the render count text.
        UpdateRenderCountString(renderCount);

        // Use identity as view matrix for 2D text rendering.
        var identityView = Matrix4X4<float>.Identity;

        // Set font shader parameters and render text.
        m_FontShader.SetShader(m_OpenGL);
        m_Font.SetTexture(m_OpenGL, TEXTURE_UNIT);

        if (!m_FontShader.SetShaderParameters(m_OpenGL, world, identityView, ortho,
            (int)TEXTURE_UNIT, m_RenderCountString.GetPixelColor()))
            return false;

        m_RenderCountString.Render(m_OpenGL);

        // Re-enable face culling.
        m_OpenGL.Gl.Enable(Silk.NET.OpenGL.EnableCap.CullFace);

        // Re-enable Z buffer and disable alpha blending.
        m_OpenGL.TurnZBufferOn();
        m_OpenGL.DisableAlphaBlending();

        m_OpenGL.EndScene();
        return true;
    }

    private void UpdateRenderCountString(int renderCount)
    {
        string text = $"Render Count: {renderCount}";
        m_RenderCountString.UpdateText(m_OpenGL, m_Font, text,
            10, 10, 1.0f, 1.0f, 1.0f, m_screenWidth, m_screenHeight);
    }
}
