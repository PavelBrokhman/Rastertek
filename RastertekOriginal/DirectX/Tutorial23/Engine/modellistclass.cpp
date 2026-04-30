////////////////////////////////////////////////////////////////////////////////
// Filename: modellistclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "modellistclass.h"


ModelListClass::ModelListClass()
{
	m_ModelInfoList = 0;
}


ModelListClass::ModelListClass(const ModelListClass& other)
{
}


ModelListClass::~ModelListClass()
{
}


void ModelListClass::Initialize(int numModels)
{
    int i;

	// Store the number of models.
    m_modelCount = numModels;

    // Create a list array of the model information.
    m_ModelInfoList = new ModelInfoType[m_modelCount];

	 // Seed the random generator with the current time.
    srand((unsigned int)time(NULL));

    // Go through all the models and randomly generate the position.
    for(i=0; i<m_modelCount; i++)
    {
        // Generate a random position in front of the viewer for the mode.
        m_ModelInfoList[i].positionX = (((float)rand() - (float)rand()) / RAND_MAX) * 10.0f;
        m_ModelInfoList[i].positionY = (((float)rand() - (float)rand()) / RAND_MAX) * 10.0f;
        m_ModelInfoList[i].positionZ = ((((float)rand() - (float)rand()) / RAND_MAX) * 10.0f) + 5.0f;
    }

    return;
}


void ModelListClass::Shutdown()
{
    // Release the model information list.
    if(m_ModelInfoList)
    {
        delete [] m_ModelInfoList;
        m_ModelInfoList = 0;
    }

    return;
}


int ModelListClass::GetModelCount()
{
    return m_modelCount;
}


void ModelListClass::GetData(int index, float& positionX, float& positionY, float& positionZ)
{
    positionX = m_ModelInfoList[index].positionX;
    positionY = m_ModelInfoList[index].positionY;
    positionZ = m_ModelInfoList[index].positionZ;
    return;
}