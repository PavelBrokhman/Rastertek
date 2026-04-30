////////////////////////////////////////////////////////////////////////////////
// Filename: inputclass.h
////////////////////////////////////////////////////////////////////////////////
#ifndef _INPUTCLASS_H_
#define _INPUTCLASS_H_


/////////////
// DEFINES //
/////////////
const int KEY_ESCAPE = 0;


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
