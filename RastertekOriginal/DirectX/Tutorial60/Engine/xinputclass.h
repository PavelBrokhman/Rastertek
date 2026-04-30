////////////////////////////////////////////////////////////////////////////////
// Filename: xinputclass.h
////////////////////////////////////////////////////////////////////////////////
#ifndef _XINPUTCLASS_H_
#define _XINPUTCLASS_H_


/////////////
// LINKING //
/////////////
#pragma comment(lib, "Xinput.lib")


//////////////
// INCLUDES //
//////////////
#include <windows.h>
#include <xinput.h>
#include <math.h>


////////////////////////////////////////////////////////////////////////////////
// Class name: XInputClass
////////////////////////////////////////////////////////////////////////////////
class XInputClass
{
public:
	XInputClass();
	XInputClass(const XInputClass&);
	~XInputClass();

	bool Initialize();
	void Shutdown();
	void Frame();

	bool IsControllerActive(int);

	bool IsButtonADown(int);
	bool IsButtonBDown(int);

	float GetLeftTrigger(int);
	float GetRightTrigger(int);

	void GetLeftThumbStickLocation(int, int&, int&);

private:
	bool IsLeftThumbStickInDeadZone(int);

private:
	XINPUT_STATE m_controllerState[4];
	bool m_controllerActive[4];
};

#endif