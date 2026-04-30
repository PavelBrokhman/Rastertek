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

	void SetDirection(float, float, float);
	void SetDiffuseColor(float, float, float, float);
	void SetDiffuseIntensity(float);
	void SetPosition(float, float, float);
    void SetLookAt(float, float, float);

	void GetDirection(float*);
	void GetDiffuseColor(float*);
    void GetDiffuseIntensity(float&);

    void GenerateViewMatrix();
    void GetViewMatrix(float*);

    void GenerateOrthoMatrix(float, float, float);
    void GetOrthoMatrix(float*);

private:
    void BuildMatrixLookAtLH(VectorType, VectorType, VectorType);

private:
    float m_direction[3];
    float m_diffuseColor[4];
    float m_diffuseIntensity;
    float m_position[3];
    float m_lookAt[3];
    float m_viewMatrix[16], m_orthoMatrix[16];
};

#endif
