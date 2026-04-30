///////////////////////////////////////////////////////////////////////////////
// Filename: zoneclass.h
///////////////////////////////////////////////////////////////////////////////
#ifndef _ZONECLASS_H_
#define _ZONECLASS_H_


///////////////////////
// MY CLASS INCLUDES //
///////////////////////
#include "openglclass.h"
#include "inputclass.h"
#include "userinterfaceclass.h"
#include "cameraclass.h"
#include "positionclass.h"
#include "terrainclass.h"


////////////////////////////////////////////////////////////////////////////////
// Class name: ZoneClass
////////////////////////////////////////////////////////////////////////////////
class ZoneClass
{
public:
    ZoneClass();
    ZoneClass(const ZoneClass&);
    ~ZoneClass();

    bool Initialize(OpenGLClass*);
    void Shutdown(OpenGLClass*);
    bool Frame(OpenGLClass*, ShaderManagerClass*, FontClass*, UserInterfaceClass*, InputClass*, float);

private:
    bool Render(OpenGLClass*, ShaderManagerClass*, FontClass*, UserInterfaceClass*);
    void HandleMovementInput(InputClass*, float);

private:
    CameraClass* m_Camera;
    PositionClass* m_Position;
    TerrainClass* m_Terrain;
    LightClass* m_Light;
};

#endif
