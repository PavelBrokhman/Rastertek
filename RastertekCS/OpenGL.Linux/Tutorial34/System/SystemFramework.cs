using RastertekCS.OpenGL.Tutorial34.Graphics; using RastertekCS.OpenGL.Tutorial34.Inputs; using Silk.NET.Input; using Silk.NET.Maths; using Silk.NET.Windowing;
namespace RastertekCS.OpenGL.Tutorial34.System;
public class SystemFramework
{
    private IWindow m_w; private IInputContext m_ic; private GL4 m_gl; private Input m_in; private GraphicsFramework m_gfx; private bool m_done, m_init;
    public bool Initialize() { int sw = 0, sh = 0; m_gl = new GL4(); if (!InitWin(ref sw, ref sh)) return false; if (!m_gl.Initialize(m_w, sw, sh, SystemConfiguration.ScreenDepth, SystemConfiguration.ScreenNear, SystemConfiguration.VerticalSyncEnabled)) return false; m_in = new Input(); m_in.Initialize(); m_gfx = new GraphicsFramework(); if (!m_gfx.Initialize(m_gl, sw, sh)) return false; m_init = true; return true; }
    public void Shutdown() { m_gfx?.Shutdown(); m_gfx = null; m_gl?.Shutdown(); m_gl = null; m_ic?.Dispose(); m_ic = null; m_w?.Dispose(); m_w = null; }
    public void Run() { m_done = false; m_w.Run(); }
    bool InitWin(ref int sw, ref int sh) { sw = 800; sh = 600; var o = WindowOptions.Default; o.Title = "Tutorial 34"; o.Size = new Vector2D<int>(sw, sh); o.WindowBorder = WindowBorder.Fixed; o.VSync = SystemConfiguration.VerticalSyncEnabled; o.API = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.ForwardCompatible, new APIVersion(4, 0)); m_w = Window.Create(o); m_w.Load += () => { m_ic = m_w.CreateInput(); foreach (var kb in m_ic.Keyboards) { kb.KeyDown += (_, key, _) => m_in?.KeyDown(key); kb.KeyUp += (_, key, _) => m_in?.KeyUp(key); } }; m_w.Render += _ => { if (!m_init) return; if (m_done || m_in.IsKeyDown(Key.Escape)) { m_done = true; m_w.Close(); return; } if (!m_gfx.Frame(m_in)) { m_done = true; m_w.Close(); } }; m_w.Closing += () => { m_done = true; m_gfx?.Shutdown(); m_gfx = null; m_gl?.Shutdown(); m_gl = null; }; m_w.Initialize(); sw = m_w.Size.X; sh = m_w.Size.Y; return true; }
}
