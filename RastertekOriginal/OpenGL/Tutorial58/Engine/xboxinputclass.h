////////////////////////////////////////////////////////////////////////////////
// Filename: xboxinputclass.h
////////////////////////////////////////////////////////////////////////////////
#ifndef _XBOXINPUTCLASS_H_
#define _XBOXINPUTCLASS_H_


//////////////
// INCLUDES //
//////////////
#include <linux/input.h>
#include <string.h>
#include <fcntl.h>
#include <unistd.h>
#include <math.h>
#include <stdio.h>


/////////////
// DEFINES //
/////////////
#define XINPUT_GAMEPAD_TRIGGER_THRESHOLD 30
#define XINPUT_GAMEPAD_LEFT_THUMB_DEADZONE 7849


////////////////////////////////////////////////////////////////////////////////
// Class name: XboxInputClass
////////////////////////////////////////////////////////////////////////////////
class XboxInputClass
{
public:
    XboxInputClass();
    XboxInputClass(const XboxInputClass&);
    ~XboxInputClass();

    bool Initialize();
    void Shutdown();
    bool Frame();

    bool IsControllerActive(int);

    bool IsButtonADown(int);
    bool IsButtonBDown(int);

    float GetLeftTrigger(int);
    float GetRightTrigger(int);

    void GetLeftThumbStickLocation(int, int&, int&);

private:
    bool CheckControllerStatus(int);
    bool IsLeftThumbStickInDeadZone(int);
    
private:
    int m_controller[4];
    bool m_controllerActive[4];
    bool m_buttonADown[4];
    bool m_buttonBDown[4];
    int m_leftTrigger[4];
    int m_rightTrigger[4];
    int m_thumbLeftX[4];
    int m_thumbLeftY[4];
};

#endif
