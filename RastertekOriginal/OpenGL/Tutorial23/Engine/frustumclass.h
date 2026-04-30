////////////////////////////////////////////////////////////////////////////////
// Filename: frustumclass.h
////////////////////////////////////////////////////////////////////////////////
#ifndef _FRUSTUMCLASS_H_
#define _FRUSTUMCLASS_H_


///////////////////////
// MY CLASS INCLUDES //
///////////////////////
#include "openglclass.h"


////////////////////////////////////////////////////////////////////////////////
// Class name: FrustumClass
////////////////////////////////////////////////////////////////////////////////
class FrustumClass
{
public:
    FrustumClass();
    FrustumClass(const FrustumClass&);
    ~FrustumClass();

    void ConstructFrustum(OpenGLClass*, float, float*, float*);

    bool CheckPoint(float, float, float);
    bool CheckCube(float, float, float, float);
    bool CheckSphere(float, float, float, float);
    bool CheckRectangle(float, float, float, float, float, float);

private:
    float m_planes[6][4];
};

#endif
