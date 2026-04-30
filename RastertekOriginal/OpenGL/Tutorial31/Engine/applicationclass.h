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
#include "cameraclass.h"
#include "modelclass.h"
#include "lightclass.h"
#include "rendertextureclass.h"
#include "lightshaderclass.h"
#include "refractionshaderclass.h"
#include "watershaderclass.h"


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
    bool RenderRefractionToTexture();
    bool RenderReflectionToTexture();
    bool Render();

private:
    OpenGLClass* m_OpenGL;
    CameraClass* m_Camera;
    ModelClass *m_GroundModel, *m_WallModel, *m_BathModel, *m_WaterModel;
    LightClass* m_Light;
    RenderTextureClass *m_RefractionTexture, *m_ReflectionTexture;
    LightShaderClass* m_LightShader;
    RefractionShaderClass* m_RefractionShader;
    WaterShaderClass* m_WaterShader;
    float m_waterHeight, m_waterTranslation;
};

#endif

