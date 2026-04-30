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
const float SCREEN_NEAR = 0.3f;
const float SCREEN_DEPTH = 1000.0f;

const int SHADOWMAP_WIDTH = 1024;
const int SHADOWMAP_HEIGHT = 1024;
const float SHADOWMAP_DEPTH = 50.0f;
const float SHADOWMAP_NEAR = 1.0f;


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
#include "transparentdepthshaderclass.h"
#include "shadowshaderclass.h"


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
    bool RenderSceneToTexture();
    bool Render();

private:
    OpenGLClass* m_OpenGL;
    CameraClass* m_Camera;
    ModelClass* m_GroundModel;
    ModelClass *m_TreeTrunkModel, *m_TreeLeafModel;
    LightClass* m_Light;
    RenderTextureClass* m_RenderTexture;
    DepthShaderClass* m_DepthShader;
    TransparentDepthShaderClass* m_TransparentDepthShader;
    ShadowShaderClass* m_ShadowShader;
    float m_shadowMapBias;
};

#endif

