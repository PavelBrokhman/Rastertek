////////////////////////////////////////////////////////////////////////////////
// Filename: applicationclass.h
////////////////////////////////////////////////////////////////////////////////
#ifndef _APPLICATIONCLASS_H_
#define _APPLICATIONCLASS_H_


/////////////
// GLOBALS //
/////////////
const bool FULL_SCREEN = true;
const bool VSYNC_ENABLED = true;
const float SCREEN_NEAR = 0.3f;
const float SCREEN_DEPTH = 1000.0f;


///////////////////////
// MY CLASS INCLUDES //
///////////////////////
#include "d3dclass.h"
#include "inputclass.h"
#include "timerclass.h"
#include "cameraclass.h"
#include "orthowindowclass.h"
#include "parallaxscrollclass.h"
#include "scrollshaderclass.h"


////////////////////////////////////////////////////////////////////////////////
// Class name: ApplicationClass
////////////////////////////////////////////////////////////////////////////////
class ApplicationClass
{
public:
	ApplicationClass();
	ApplicationClass(const ApplicationClass&);
	~ApplicationClass();

	bool Initialize(int, int, HWND);
	void Shutdown();
	bool Frame(InputClass*);

private:
    bool Render();

private:
	D3DClass* m_Direct3D;
	TimerClass* m_Timer;
	CameraClass* m_Camera;
	OrthoWindowClass* m_FullScreenWindow;
	ScrollShaderClass* m_ScrollShader;
	ParallaxScrollClass* m_ParallaxForest;
};

#endif