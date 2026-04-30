////////////////////////////////////////////////////////////////////////////////
// Filename: applicationclass.h
////////////////////////////////////////////////////////////////////////////////
#ifndef _APPLICATIONCLASS_H_
#define _APPLICATIONCLASS_H_


/////////////
// GLOBALS //
/////////////
const bool FULL_SCREEN = false;
const bool VSYNC_ENABLED = true;
const float SCREEN_NEAR = 1.0f;
const float SCREEN_DEPTH = 100.0f;
const int SHADOWMAP_WIDTH = 1024;
const int SHADOWMAP_HEIGHT = 1024;


///////////////////////
// MY CLASS INCLUDES //
///////////////////////
#include "inputclass.h"
#include "openglclass.h"
#include "cameraclass.h"
#include "modelclass.h"
#include "lightclass.h"
#include "rendertextureclass.h"
#include "depthshaderclass.h"
#include "shadowshaderclass.h"
#include "softshadowshaderclass.h"
#include "blurclass.h"


////////////////////////////////////////////////////////////////////////////////
// Class Name: ApplicationClass
////////////////////////////////////////////////////////////////////////////////
class ApplicationClass
{
public:
    ApplicationClass();
    ApplicationClass(const ApplicationClass&);
    ~ApplicationClass();

    bool Initialize(Display*, Window, int, int);
    void Shutdown();
    bool Frame(InputClass*);

private:
    bool RenderDepthToTexture();
    bool RenderBlackAndWhiteShadows();
    bool Render();

private:
    OpenGLClass* m_OpenGL;
    CameraClass* m_Camera;
    ModelClass *m_CubeModel, *m_SphereModel, *m_GroundModel;
    LightClass* m_Light;
    RenderTextureClass *m_RenderTexture, *m_BlackWhiteRenderTexture;
    DepthShaderClass* m_DepthShader;
    ShadowShaderClass* m_ShadowShader;
    SoftShadowShaderClass* m_SoftShadowShader;
    BlurClass* m_Blur;
    TextureShaderClass* m_TextureShader;
    BlurShaderClass* m_BlurShader;
    float m_shadowMapBias;
};

#endif

