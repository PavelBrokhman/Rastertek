///////////////////////////////////////////////////////////////////////////////
// Filename: directsoundclass.h
///////////////////////////////////////////////////////////////////////////////
#ifndef _DIRECTSOUNDCLASS_H_
#define _DIRECTSOUNDCLASS_H_


/////////////
// LINKING //
/////////////
#pragma comment(lib, "dsound.lib")
#pragma comment(lib, "dxguid.lib")
#pragma comment(lib, "winmm.lib")


//////////////
// INCLUDES //
//////////////
#include <windows.h>
#include <mmsystem.h>
#include <dsound.h>
#include <stdio.h>


///////////////////////////////////////////////////////////////////////////////
// Class name: DirectSoundClass
///////////////////////////////////////////////////////////////////////////////
class DirectSoundClass
{
public:
	DirectSoundClass();
	DirectSoundClass(const DirectSoundClass&);
	~DirectSoundClass();

	bool Initialize(HWND);
	void Shutdown();

	IDirectSound8* GetDirectSound();

private:
	IDirectSound8* m_DirectSound;
	IDirectSoundBuffer* m_primaryBuffer;
	IDirectSound3DListener8* m_listener;
};

#endif