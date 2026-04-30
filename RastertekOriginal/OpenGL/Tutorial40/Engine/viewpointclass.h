////////////////////////////////////////////////////////////////////////////////
// Filename: viewpointclass.h
////////////////////////////////////////////////////////////////////////////////
#ifndef _VIEWPOINTCLASS_H_
#define _VIEWPOINTCLASS_H_


//////////////
// INCLUDES //
//////////////
#include <math.h>


////////////////////////////////////////////////////////////////////////////////
// Class name: ViewPointClass
////////////////////////////////////////////////////////////////////////////////
class ViewPointClass
{
private:
    struct VectorType
    {
        float x, y, z;
    };

public:
    ViewPointClass();
    ViewPointClass(const ViewPointClass&);
    ~ViewPointClass();

    void SetPosition(float, float, float);
    void SetLookAt(float, float, float);
    void SetProjectionParameters(float, float, float, float);

    void GenerateViewMatrix();
    void GenerateProjectionMatrix();

    void GetViewMatrix(float*);
    void GetProjectionMatrix(float*);

private:
    void BuildMatrixLookAtLH(VectorType, VectorType, VectorType);
    void BuildMatrixPerspectiveFovLH(float, float, float, float);

private:
    VectorType m_position, m_lookAt;
    float m_fieldOfView, m_aspectRatio, m_nearPlane, m_farPlane;
    float m_viewMatrix[16], m_projectionMatrix[16];
};

#endif
