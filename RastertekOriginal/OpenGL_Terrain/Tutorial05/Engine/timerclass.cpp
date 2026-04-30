///////////////////////////////////////////////////////////////////////////////
// Filename: timerclass.cpp
///////////////////////////////////////////////////////////////////////////////
#include "timerclass.h"


TimerClass::TimerClass()
{
}


TimerClass::TimerClass(const TimerClass& other)
{
}


TimerClass::~TimerClass()
{
}


void TimerClass::Initialize()
{
    struct timeval time;


    // Get the start time.
    gettimeofday(&time, 0);

    // Store the start time.
    m_startSeconds = time.tv_sec;
    m_startMilliseconds = time.tv_usec / 1000;

    return;
}


void TimerClass::Frame()
{
    struct timeval time;
    long seconds, microseconds, secondsDelta, milliseconds, currentMs;


    // Get the current time.
    gettimeofday(&time, 0);
    seconds = time.tv_sec;
    microseconds = time.tv_usec;

    // Calculate the current milliseconds that have passed since the previous frame.
    milliseconds = microseconds / 1000;
    secondsDelta = seconds - m_startSeconds;
    currentMs = (secondsDelta * 1000) + milliseconds;
    m_frameTime = (int)(currentMs - m_startMilliseconds);

    // Restart the timer.
    gettimeofday(&time, 0);
    m_startSeconds = time.tv_sec;
    m_startMilliseconds = time.tv_usec / 1000;

    return;
}


float TimerClass::GetTime()
{
    // Modify to match windows.
    return (float)(m_frameTime * 0.001f);
}


int TimerClass::GetTimeUnmodified()
{
    return m_frameTime;
}
