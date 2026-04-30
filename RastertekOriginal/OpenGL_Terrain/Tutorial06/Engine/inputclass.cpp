////////////////////////////////////////////////////////////////////////////////
// Filename: inputclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "inputclass.h"


InputClass::InputClass()
{
}


InputClass::InputClass(const InputClass& other)
{
}


InputClass::~InputClass()
{
}


void InputClass::Initialize()
{
    int i;


    // Initialize the keyboard state.
    for(i=0; i<256; i++)
    {
        m_keyboardState[i] = false;
    }

    // Initialize the mouse state.
    m_mouseX = 0;
    m_mouseY = 0;
    m_mousePressed = false;

    return;
}


void InputClass::KeyDown(int keySymbol)
{
    switch(keySymbol)
    {
        case 65307:
        {
            m_keyboardState[KEY_ESCAPE] = true;
            break;
        }
        case 65361:
        {
            m_keyboardState[KEY_LEFT] = true;
            break;
        }
        case 65363:
        {
            m_keyboardState[KEY_RIGHT] = true;
            break;
        }
        case 65362:
        {
            m_keyboardState[KEY_UP] = true;
            break;
        }
        case 65364:
        {
            m_keyboardState[KEY_DOWN] = true;
            break;
        }
        case 97:
        {
            m_keyboardState[KEY_A] = true;
            break;
        }
        case 122:
        {
            m_keyboardState[KEY_Z] = true;
            break;
        }
        case 65365:
        {
            m_keyboardState[KEY_PGUP] = true;
            break;
        }
        case 65366:
        {
            m_keyboardState[KEY_PGDN] = true;
            break;
        }
        default:
        {
            break;
        }
    }

    return;
}


void InputClass::KeyUp(int keySymbol)
{
    switch(keySymbol)
    {
        case 65307:
        {
            m_keyboardState[KEY_ESCAPE] = false;
            break;
        }
        case 65361:
        {
            m_keyboardState[KEY_LEFT] = false;
            break;
        }
        case 65363:
        {
            m_keyboardState[KEY_RIGHT] = false;
            break;
        }
        case 65362:
        {
            m_keyboardState[KEY_UP] = false;
            break;
        }
        case 65364:
        {
            m_keyboardState[KEY_DOWN] = false;
            break;
        }
        case 97:
        {
            m_keyboardState[KEY_A] = false;
            break;
    	}
        case 122:
        {
            m_keyboardState[KEY_Z] = false;
            break;
        }
        case 65365:
        {
            m_keyboardState[KEY_PGUP] = false;
            break;
        }
        case 65366:
        {
            m_keyboardState[KEY_PGDN] = false;
            break;
        }
        default:
        {
            break;
        }
    }

    return;
}


bool InputClass::IsEscapePressed()
{
    return m_keyboardState[KEY_ESCAPE];
}


bool InputClass::IsLeftPressed()
{
    return m_keyboardState[KEY_LEFT];
}


bool InputClass::IsRightPressed()
{
    return m_keyboardState[KEY_RIGHT];
}


bool InputClass::IsUpPressed()
{
    return m_keyboardState[KEY_UP];
}


bool InputClass::IsDownPressed()
{
    return m_keyboardState[KEY_DOWN];
}


bool InputClass::IsAPressed()
{
    return m_keyboardState[KEY_A];
}


bool InputClass::IsZPressed()
{
    return m_keyboardState[KEY_Z];
}


bool InputClass::IsPgUpPressed()
{
    return m_keyboardState[KEY_PGUP];
}


bool InputClass::IsPgDownPressed()
{
    return m_keyboardState[KEY_PGDN];
}


void InputClass::ProcessMouse(int mouseMotionX, int mouseMotionY)
{
    m_mouseX = mouseMotionX;
    m_mouseY = mouseMotionY;
    return;
}


void InputClass::GetMouseLocation(int& mouseX, int& mouseY)
{
    mouseX = m_mouseX;
    mouseY = m_mouseY;
    return;
}


void InputClass::MouseDown()
{
    m_mousePressed = true;
    return;
}


void InputClass::MouseUp()
{
    m_mousePressed = false;
    return;
}


bool InputClass::IsMousePressed()
{
    return m_mousePressed;
}
