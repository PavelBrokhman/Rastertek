////////////////////////////////////////////////////////////////////////////////
// Filename: userinterfaceclass.h
////////////////////////////////////////////////////////////////////////////////
#ifndef _USERINTERFACECLASS_H_
#define _USERINTERFACECLASS_H_


///////////////////////
// MY CLASS INCLUDES //
///////////////////////
#include "textclass.h"
#include "shadermanagerclass.h"


////////////////////////////////////////////////////////////////////////////////
// Class name: UserInterfaceClass
////////////////////////////////////////////////////////////////////////////////
class UserInterfaceClass
{
public:
    UserInterfaceClass();
    UserInterfaceClass(const UserInterfaceClass&);
    ~UserInterfaceClass();

    bool Initialize(OpenGLClass*, FontClass*, int, int);
    void Shutdown();
    bool Frame(FontClass*, int);
    bool Render(OpenGLClass*, ShaderManagerClass*, FontClass*, float*, float*, float*);

    bool UpdatePositonStrings(FontClass*, float, float, float, float, float, float);

private:
    bool UpdateFpsString(FontClass*, int);

private:
    TextClass* m_VideoCardString;
    TextClass* m_FpsString;
    TextClass* m_PositionStrings;
    int m_previousFps;
    int m_previousPosition[6];
};

#endif
