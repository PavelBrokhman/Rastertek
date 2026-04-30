////////////////////////////////////////////////////////////////////////////////
// Filename: openalclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "openalclass.h"


OpenALClass::OpenALClass()
{
}


OpenALClass::OpenALClass(const OpenALClass& other)
{
}


OpenALClass::~OpenALClass()
{
}


bool OpenALClass::Initialize()
{
    ALCdevice* device;
    ALCcontext* context;
    float position[3];
    bool result;
    
    
    // Select the default audio device on the system.
    device = alcOpenDevice(NULL);
    if(!device)
    {
        return false;
    }

    // Create a context on the device.
    context = alcCreateContext(device, NULL);

    // Open the context by setting it as the current context on the device.
    alcMakeContextCurrent(context);

    // Set the initial position of the listener.
    position[0] = 0.0f;
    position[1] = 0.0f;
    position[2] = 0.0f;

    // Clear any previous unaddressed error codes.
    alGetError();
    
    // Set the listener position.
    alListenerfv(AL_POSITION, position);
    if(alGetError() != AL_NO_ERROR)
    {
	return false;
    }

    return true;
}


void OpenALClass::Shutdown()
{
    ALCdevice* device;
    ALCcontext* context;

    
    // Retrieve the current context and device.
    context = alcGetCurrentContext();
    device = alcGetContextsDevice(context);
 
    // Unset our audio context as the current one.
    alcMakeContextCurrent(NULL);

    // Destroy our audio context.
    alcDestroyContext(context);

    // Close our audio device.
    alcCloseDevice(device);
    
    return;
}
