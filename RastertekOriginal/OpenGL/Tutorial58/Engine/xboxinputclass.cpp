////////////////////////////////////////////////////////////////////////////////
// Filename: xboxinputclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "xboxinputclass.h"


XboxInputClass::XboxInputClass()
{
}


XboxInputClass::XboxInputClass(const XboxInputClass& other)
{
}


XboxInputClass::~XboxInputClass()
{
}


bool XboxInputClass::Initialize()
{
    char filename[256], deviceName[256], tempString[16];
    int i, j, id;


    // Initialize all of the controllers.
    for(i=0; i<4; i++)
    {
        m_controllerActive[i] = false;
	m_buttonADown[i] = false;
	m_buttonBDown[i] = false;
	m_leftTrigger[i] = 0;
	m_rightTrigger[i] = 0;
	m_thumbLeftX[i] = 0;
	m_thumbLeftY[i] = 0;
    }

    // Loop through the first 50 event devices to find up to four xbox controllers.
    j=0;
    for(i=0; i<50; i++)
    {
        strcpy(filename, "/dev/input/event");
	sprintf(tempString, "%d", i);
	strcat(filename, tempString);

	// See if you can open the event device.
	id = open(filename, O_RDONLY | O_NONBLOCK);
	if(id != -1)
	{
	    // If it can be opened then get the device name.
	    ioctl(id, EVIOCGNAME(sizeof(deviceName)), deviceName);

	    // Check if it has a name similar to Microsoft Xbox Series S|X Controller
	    if((deviceName[0] == 'M') && (deviceName[10] == 'X'))
	    {
	        // Make sure we haven't gone over four controllers.
	        if(j != 4)
		{
		    // Store the id to the controller.
		    m_controller[j] = id;
		    m_controllerActive[j] = true;
		    j++;
		}
	    }
	}
    }

    // Exit out if no controllers were found.
    if(j == 0)
    {
        return false;
    }
	
    return true;
}


void XboxInputClass::Shutdown()
{

    return;
}


bool XboxInputClass::Frame()
{
    int i;
    bool result;
    
    
    // Loop through all four possible XboxInput devices connected via USB.
    for(i=0; i<4; i++)
    {
        result = CheckControllerStatus(i);
	if(!result)
	{
	    return false;
	}
    }
  
    return true;
}


bool XboxInputClass::CheckControllerStatus(int index)
{
    input_event event;
    int result;


    if(m_controllerActive[index] == true)
    {
        // Zero out the state structure prior to retrieving the new state for it.
        memset(&event, 0, sizeof(event));

	// Read events from the controller.
	result = read(m_controller[index], &event, sizeof(event));

	// If there is an event then process it.
	if(result == -1)
	{
	    return true;
	}

	if(event.type == EV_KEY)
	{
	    // A button.
	    if(event.code == BTN_SOUTH)
	    {
	        if((int)event.value == 0)
		{
		    m_buttonADown[index] = false;
		}
		if((int)event.value == 1)
		{
		    m_buttonADown[index] = true;
		}
	    }

	    // B button.
	    if(event.code == BTN_EAST)
	    {
	        if((int)event.value == 0)
		{
		    m_buttonBDown[index] = false;
		}
		if((int)event.value == 1)
		{
		    m_buttonBDown[index] = true;
		}
	    }
	}

	if(event.type == EV_ABS)
	{
	    // Left trigger.
	    if(event.code == ABS_Z)
	    {
	        m_leftTrigger[index] = (int)event.value;
	    }

	    // Right trigger.
	    if(event.code == ABS_RZ)
	    {
	        m_rightTrigger[index] = (int)event.value;
	    }

	    // Thumb left stick X.
	    if(event.code == ABS_X)
	    {
	        m_thumbLeftX[index] = (int)event.value;
	    }

	    // Thumb left stick Y.
	    if(event.code == ABS_Y)
	    {
	        m_thumbLeftY[index] = (int)event.value;
	    }
	}
    }

    return true;
}


bool XboxInputClass::IsControllerActive(int index)
{
    // Boundary check.
    if((index < 0) || (index > 3))
    {
        return false;
    }

    return m_controllerActive[index];
}


bool XboxInputClass::IsButtonADown(int index)
{
    // Boundary check.
    if((index < 0) || (index > 3))
    {
        return false;
    }
    
    return m_buttonADown[index];
}


bool XboxInputClass::IsButtonBDown(int index)
{
    // Boundary check.
    if((index < 0) || (index > 3))
    {
        return false;
    }
    
    return m_buttonBDown[index];
}


float XboxInputClass::GetLeftTrigger(int index)
{
    int triggerValue;
    float finalValue;


    // Boundary check.
    if((index < 0) || (index > 3))
    {
        return 0.0f;
    }

    // Get the amount the left trigger is pressed.
    triggerValue = m_leftTrigger[index];
 
    // If it is really light then return zero to avoid being oversensitive.
    if((triggerValue / 4) < XINPUT_GAMEPAD_TRIGGER_THRESHOLD)
    {
        return 0.0f;
    }

    // Otherwise convert from 0-1023 int to 0.0-1.0 float range.
    finalValue = (float)triggerValue / 1023.0f;

    return finalValue;
}


float XboxInputClass::GetRightTrigger(int index)
{
    int triggerValue;
    float finalValue;


    // Boundary check.
    if((index < 0) || (index > 3))
    {
        return 0.0f;
    }

    // Get the amount the right trigger is pressed.
    triggerValue = m_rightTrigger[index];
 
    // If it is really light then return zero to avoid being oversensitive.
    if((triggerValue / 4) < XINPUT_GAMEPAD_TRIGGER_THRESHOLD)
    {
        return 0.0f;
    }

    // Otherwise convert from 0-1023 int to 0.0-1.0 float range.
    finalValue = (float)triggerValue / 1023.0f;

    return finalValue;
}


void XboxInputClass::GetLeftThumbStickLocation(int index, int& thumbLeftX, int& thumbLeftY)
{
    // Boundary check.
    if((index < 0) || (index > 3))
    {
        thumbLeftX = 0;
        thumbLeftY = 0;
	return;
    }
    
    // Get the current state of the left thumb stick.
    thumbLeftX = m_thumbLeftX[index];
    thumbLeftY = m_thumbLeftY[index];

    // Check for dead zone, if so return zero to reduce the noise.
    if(IsLeftThumbStickInDeadZone(index) == true)
    {
        thumbLeftX = 0;
        thumbLeftY = 0;
    }

    return;
}


bool XboxInputClass::IsLeftThumbStickInDeadZone(int index)
{
    int thumbLeftX, thumbLeftY, magnitude;


    // Get the current state of the left thumb stick.
    thumbLeftX = m_thumbLeftX[index];
    thumbLeftY = m_thumbLeftY[index];

    // Determine how far the controller is pushed.
    magnitude = (int)sqrt((thumbLeftX * thumbLeftX) + (thumbLeftY * thumbLeftY));

    // Check if the controller is inside a circular dead zone.
    if(magnitude < XINPUT_GAMEPAD_LEFT_THUMB_DEADZONE)
    {
        return true;
    }

    return false;
}
