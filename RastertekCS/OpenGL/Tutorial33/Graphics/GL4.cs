using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace RastertekCS.OpenGL.Tutorial33.Graphics;

public class GL4
{
    private GL _gl;
    private Matrix4X4<float> _worldMatrix,
        _projectionMatrix;
    public GL Gl => _gl;

    public bool Initialize(IWindow w, int sw, int sh, float sd, float sn, bool vs)
    {
        _gl = GL.GetApi(w);
        _gl.ClearDepth(1f);
        _gl.Enable(EnableCap.DepthTest);
        _gl.FrontFace(FrontFaceDirection.CW);
        _gl.Enable(EnableCap.CullFace);
        _gl.CullFace(TriangleFace.Back);
        _gl.Viewport(0, 0, (uint)sw, (uint)sh);
        _worldMatrix = Matrix4X4<float>.Identity;
        _projectionMatrix = PerspectiveFovLH(MathF.PI / 4f, (float)sw / sh, sn, sd);
        return true;
    }

    public void Shutdown()
    {
        _gl?.Dispose();
        _gl = null;
    }

    public void BeginScene(float r, float g, float b, float a)
    {
        _gl.ClearColor(r, g, b, a);
        _gl.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));
    }

    public void EndScene() { }

    public Matrix4X4<float> GetWorldMatrix() => _worldMatrix;

    public Matrix4X4<float> GetProjectionMatrix() => _projectionMatrix;

    public void EnableAlphaBlending()
    {
        _gl.Enable(EnableCap.Blend);
        _gl.BlendFuncSeparate(
            BlendingFactor.SrcAlpha,
            BlendingFactor.OneMinusSrcAlpha,
            BlendingFactor.One,
            BlendingFactor.Zero
        );
    }

    public void DisableAlphaBlending()
    {
        _gl.Disable(EnableCap.Blend);
    }

    private static Matrix4X4<float> PerspectiveFovLH(
        float fov,
        float aspect,
        float nearZ,
        float farZ
    )
    {
        float h = 1.0f / MathF.Tan(fov * 0.5f);
        float w = h / aspect;
        float range = farZ / (farZ - nearZ);
        return new Matrix4X4<float>(
            w,
            0,
            0,
            0,
            0,
            h,
            0,
            0,
            0,
            0,
            range,
            1,
            0,
            0,
            -range * nearZ,
            0
        );
    }
}
