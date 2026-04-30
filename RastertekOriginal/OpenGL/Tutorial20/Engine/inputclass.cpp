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
    if(keySymbol == 65307) { m_keyboardState[KEY_ESCAPE] = true; }

    return;
}


void InputClass::KeyUp(int keySymbol)
{
    if(keySymbol == 65307) { m_keyboardState[KEY_ESCAPE] = false; }

    return;
}


bool InputClass::IsEscapePressed()
{
    return m_keyboardState[KEY_ESCAPE];
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
