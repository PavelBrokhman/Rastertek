////////////////////////////////////////////////////////////////////////////////
// Filename: lightclass.h
////////////////////////////////////////////////////////////////////////////////
#ifndef _LIGHTCLASS_H_
#define _LIGHTCLASS_H_


//////////////
// INCLUDES //
//////////////
#include <math.h>


////////////////////////////////////////////////////////////////////////////////
// Class name: LightClass
////////////////////////////////////////////////////////////////////////////////
class LightClass
{
private:
    struct VectorType
    {
        float x, y, z;
    };
  
public:
    LightClass();
    LightClass(const LightClass&);
    ~LightClass();

    void SetDiffuseColor(float, float, float, float);
    void SetDirection(float, float, float);
    void SetAmbientLight(float, float, float, float);
    void SetSpecularColor(float, float, float, float);
    void SetSpecularPower(float);
    void SetPosition(float, float, float);
    void SetLookAt(float, float, float);
  
    void GetDiffuseColor(float*);
    void GetDirection(float*);
    void GetAmbientLight(float*);
    void GetSpecularColor(float*);
    void GetSpecularPower(float&);
    void GetPosition(float*);

    void GenerateViewMatrix();
    void GenerateProjectionMatrix(float, float);
    void GenerateOrthoMatrix(float, float, float);
  
    void GetViewMatrix(float*);
    void GetProjectionMatrix(float*);
    void GetOrthoMatrix(float*);
  
private:
    void BuildMatrixLookAtLH(VectorType, VectorType, VectorType);
    void BuildMatrixPerspectiveFovLH(float, float, float, float);
  
private:
    float m_diffuseColor[4];
    float m_direction[3];
    float m_ambientLight[4];
    float m_specularColor[4];
    float m_specularPower;
    float m_position[3];
    float m_lookAt[3];
    float m_viewMatrix[16], m_projectionMatrix[16], m_orthoMatrix[16];
};

#endif
