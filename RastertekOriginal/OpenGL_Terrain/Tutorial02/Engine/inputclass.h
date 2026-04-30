////////////////////////////////////////////////////////////////////////////////
// Filename: inputclass.h
////////////////////////////////////////////////////////////////////////////////
#ifndef _INPUTCLASS_H_
#define _INPUTCLASS_H_


/////////////
// DEFINES //
/////////////
const int KEY_ESCAPE = 0;
const int KEY_LEFT = 1;
const int KEY_RIGHT = 2;
const int KEY_UP = 3;
const int KEY_DOWN = 4;
const int KEY_A = 5;
const int KEY_Z = 6;
const int KEY_PGUP = 7;
const int KEY_PGDN = 8;


////////////////////////////////////////////////////////////////////////////////
// Class name: InputClass
////////////////////////////////////////////////////////////////////////////////
class InputClass
{
public:
    InputClass();
    InputClass(const InputClass&);
    ~InputClass();

    void Initialize();

    void KeyDown(int);
    void KeyUp(int);

    bool IsEscapePressed();
    bool IsLeftPressed();
    bool IsRightPressed();
    bool IsUpPressed();
    bool IsDownPressed();
    bool IsAPressed();
    bool IsZPressed();
    bool IsPgUpPressed();
    bool IsPgDownPressed();

    void ProcessMouse(int, int);
    void GetMouseLocation(int&, int&);

    void MouseDown();
    void MouseUp();

    bool IsMousePressed();

private:
    bool m_keyboardState[256];
    int m_mouseX, m_mouseY;
    bool m_mousePressed;
};

#endif
