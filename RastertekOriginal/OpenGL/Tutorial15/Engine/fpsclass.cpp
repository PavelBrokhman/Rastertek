///////////////////////////////////////////////////////////////////////////////
// Filename: fpsclass.cpp
///////////////////////////////////////////////////////////////////////////////
#include "fpsclass.h"


FpsClass::FpsClass()
{
}


FpsClass::FpsClass(const FpsClass& other)
{
}


FpsClass::~FpsClass()
{
}


void FpsClass::Initialize()
{
    struct timeval time;


    // Initialize the fps counters.
    m_fps = 0;
    m_count = 0;

    // Get the start time.
    gettimeofday(&time, 0);

    // Store the time.
    m_startSeconds = time.tv_sec;
    m_startMilliseconds = time.tv_usec / 1000;

    return;
}


void FpsClass::Frame()
{
    struct timeval time;
    long seconds, microseconds, secondsDelta, milliseconds, currentMs;


    // Increment the frame count.
    m_count++;

    // Get the current time.
    gettimeofday(&time, 0);
    seconds = time.tv_sec;
    microseconds = time.tv_usec;

    // Calculate the current milliseconds that have passed since the previous frame count increment.
    milliseconds = microseconds / 1000;
    secondsDelta = seconds - m_startSeconds;
    currentMs = (secondsDelta * 1000) + milliseconds;
    currentMs = currentMs - m_startMilliseconds;

    // If it has been one second (1000 ms) then store the frames per second count.
    if(currentMs >= 1000)
    {
        m_fps = m_count;
	m_count = 0;

	// Reset the start timer.
	gettimeofday(&time, 0);
        m_startSeconds = time.tv_sec;
        m_startMilliseconds = time.tv_usec / 1000;
    }

    return;
}


int FpsClass::GetFps()
{
    return m_fps;
}
