////////////////////////////////////////////////////////////////////////////////
// Filename: lightclass.h
////////////////////////////////////////////////////////////////////////////////
#ifndef _LIGHTCLASS_H_
#define _LIGHTCLASS_H_


////////////////////////////////////////////////////////////////////////////////
// Class name: LightClass
////////////////////////////////////////////////////////////////////////////////
class LightClass
{
public:
	LightClass();
	LightClass(const LightClass&);
	~LightClass();

	void SetDiffuseColor(float, float, float, float);
	void SetDirection(float, float, float);
    void SetAmbientLight(float, float, float, float);
    void SetSpecularColor(float, float, float, float);
	void SetSpecularPower(float);

	void GetDiffuseColor(float*);
	void GetDirection(float*);
    void GetAmbientLight(float*);
    void GetSpecularColor(float*);
	void GetSpecularPower(float&);

private:
	float m_diffuseColor[4];
	float m_direction[3];
	float m_ambientLight[4];
	float m_specularColor[4];
	float m_specularPower;
};

#endif
