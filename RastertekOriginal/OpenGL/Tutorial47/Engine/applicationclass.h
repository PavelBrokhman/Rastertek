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
#include "lightshaderclass.h"
#include "fontshaderclass.h"
#include "fontclass.h"
#include "textclass.h"
#include "bitmapclass.h"
#include "textureshaderclass.h"


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
    bool Render();

    bool TestIntersection(int, int);
    bool RaySphereIntersect(float[3], float[3], float);

    void TransformCoord(float[3], float[16]);
    void TransformNormal(float[3], float[16]);
    void Vec3Normalize(float[3]);
  
private:
    OpenGLClass* m_OpenGL;
    CameraClass* m_Camera;
    ModelClass* m_Model;
    LightClass* m_Light;
    LightShaderClass* m_LightShader;
    FontShaderClass* m_FontShader;
    FontClass* m_Font;
    TextClass* m_TextString;
    BitmapClass* m_MouseBitmap;
    TextureShaderClass* m_TextureShader;
    int m_screenWidth, m_screenHeight;
};

#endif

