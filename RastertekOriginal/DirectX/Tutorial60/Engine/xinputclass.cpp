////////////////////////////////////////////////////////////////////////////////
// Filename: xinputclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "xinputclass.h"


XInputClass::XInputClass()
{
}


XInputClass::XInputClass(const XInputClass& other)
{
}


XInputClass::~XInputClass()
{
}


bool XInputClass::Initialize()
{
    int i;


    // Initialize the state of the four controllers to be off.
    for(i=0; i<4; i++)
    {
        m_controllerActive[i] = false;
    }

	return true;
}


void XInputClass::Shutdown()
{

	return;
}


void XInputClass::Frame()
{
	unsigned long i, result;


    // Loop through all four possible XInput devices connected via USB.
    for(i=0; i<4; i++)
    {
        // Zero out the state structure prior to retrieving the new state for it.
        ZeroMemory(&m_controllerState[i], sizeof(XINPUT_STATE));

        // Get the state of the controller.
        result = XInputGetState(i, &m_controllerState[i]);

        // Store whether the controller is currently connected or not.
        if(result == ERROR_SUCCESS)
        {
            m_controllerActive[i] = true;
        }
        else
        {
            m_controllerActive[i] = false;
        }
    }

	return;
}


bool XInputClass::IsControllerActive(int index)
{
    // Boundary check.
    if((index < 0) || (index > 3))
    {
        return false;
    }

    return m_controllerActive[index];
}


bool XInputClass::IsButtonADown(int index)
{
    if(m_controllerState[index].Gamepad.wButtons & XINPUT_GAMEPAD_A)
	{
		return true;
	}

	return false;
}


bool XInputClass::IsButtonBDown(int index)
{
    if(m_controllerState[index].Gamepad.wButtons & XINPUT_GAMEPAD_B)
	{
		return true;
	}

	return false;
}


float XInputClass::GetLeftTrigger(int index)
{
    unsigned char triggerValue;
    float finalValue;


    // Get the amount the left trigger is pressed.
    triggerValue = m_controllerState[index].Gamepad.bLeftTrigger;
 
    // If it is really light then return zero to avoid being oversensitive.
    if(triggerValue < XINPUT_GAMEPAD_TRIGGER_THRESHOLD)
    {
        return 0.0f;
    }

    // Otherwise convert from 0-255 unsigned char to 0.0-1.0 float range.
    finalValue = (float)triggerValue / 255.0f;

    return finalValue;
}


float XInputClass::GetRightTrigger(int index)
{
    unsigned char triggerValue;
    float finalValue;


    // Get the amount the left trigger is pressed.
    triggerValue = m_controllerState[index].Gamepad.bRightTrigger;
 
    // If it is really light then return zero to avoid being oversensitive.
    if(triggerValue < XINPUT_GAMEPAD_TRIGGER_THRESHOLD)
    {
        return 0.0f;
    }

    // Otherwise convert from 0-255 unsigned char to 0.0-1.0 float range.
    finalValue = (float)triggerValue / 255.0f;

    return finalValue;
}


void XInputClass::GetLeftThumbStickLocation(int index, int& thumbLeftX, int& thumbLeftY)
{
    // Get the current state of the left thumb stick.
    thumbLeftX = (int)m_controllerState[index].Gamepad.sThumbLX;
    thumbLeftY = (int)m_controllerState[index].Gamepad.sThumbLY;

    // Check for dead zone, if so return zero to reduce the noise.
    if(IsLeftThumbStickInDeadZone(index) == true)
    {
        thumbLeftX = 0;
        thumbLeftY = 0;
    }

    return;
}


bool XInputClass::IsLeftThumbStickInDeadZone(int index)
{
    int thumbLeftX, thumbLeftY, magnitude;


    // Get the current state of the left thumb stick.
    thumbLeftX = (int)m_controllerState[index].Gamepad.sThumbLX;
    thumbLeftY = (int)m_controllerState[index].Gamepad.sThumbLY;

    // Determine how far the controller is pushed.
    magnitude = (int)sqrt((thumbLeftX * thumbLeftX) + (thumbLeftY * thumbLeftY));

    // Check if the controller is inside a circular dead zone.
    if(magnitude < XINPUT_GAMEPAD_LEFT_THUMB_DEADZONE)
    {
        return true;
    }

    return false;
}