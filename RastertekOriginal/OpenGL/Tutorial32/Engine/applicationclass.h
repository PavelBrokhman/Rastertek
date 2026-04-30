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


///////////////////////
// MY CLASS INCLUDES //
///////////////////////
#include "inputclass.h"
#include "openglclass.h"
#include "modelclass.h"
#include "cameraclass.h"
#include "textureshaderclass.h"
#include "rendertextureclass.h"
#include "glassshaderclass.h"


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
    bool RenderSceneToTexture(float);
    bool Render(float);

private:
    OpenGLClass* m_OpenGL;
    CameraClass* m_Camera;
    ModelClass* m_Model;
    ModelClass* m_WindowModel;
    RenderTextureClass* m_RenderTexture;
    TextureShaderClass* m_TextureShader;
    GlassShaderClass* m_GlassShader;
};

#endif

