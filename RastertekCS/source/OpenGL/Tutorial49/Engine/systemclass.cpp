////////////////////////////////////////////////////////////////////////////////
// Filename: systemclass.cpp
////////////////////////////////////////////////////////////////////////////////
#include "systemclass.h"


SystemClass::SystemClass()
{
    m_Input = 0;
    m_Application = 0;
}


SystemClass::SystemClass(const SystemClass& other)
{
}


SystemClass::~SystemClass()
{
}


bool SystemClass::Initialize()
{
    int screenWidth, screenHeight;
    bool result;


    // Create and initialize the input object.
    m_Input = new InputClass;

    m_Input->Initialize();

    // Initialize the screen size.
    screenWidth = 0;
    screenHeight = 0;

    // Initialize the X11 window.
    result = InitializeWindow(screenWidth, screenHeight);
    if(!result)
    {
        return false;
    }

    // Create and initialize the application object.
    m_Application = new ApplicationClass;

    result = m_Application->Initialize(m_videoDisplay, m_hwnd, screenWidth, screenHeight);
    if(!result)
    {
        return false;
    }

    return true;
}


void SystemClass::Shutdown()
{
    // Release the application object.
    if(m_Application)
    {
        m_Application->Shutdown();
        delete m_Application;
        m_Application = 0;
    }

    // Release the X11 window.
    ShutdownWindow();

    // Release the input object.
    if(m_Input)
    {
        delete m_Input;
        m_Input = 0;
    }

    return;
}


void SystemClass::Frame()
{
    bool done, result;


    // Loop until the application is finished running.
    done = false;
    while(!done)
    {
        // Read the X11 input.
        ReadInput();

        // Do the frame processing for the application object.
        result = m_Application->Frame(m_Input);
        if(!result)
        {
            done = true;
        }
    }

    return;
}


bool SystemClass::InitializeWindow(int& screenWidth, int& screenHeight)
{
    Window rootWindow;
    XVisualInfo* visualInfo;
    GLint attributeList[15];
    Colormap colorMap;
    XSetWindowAttributes setWindowAttributes;
    Screen* defaultScreen;
    bool result;
    int majorVersion;
    Atom wmState, fullScreenState, motifHints;
    XEvent fullScreenEvent;
    long motifHintList[5];
    int status, posX, posY, defaultScreenWidth, defaultScreenHeight;
    

    // Open a connection to the X server on the local computer.
    m_videoDisplay = XOpenDisplay(NULL);
    if(m_videoDisplay == NULL)
    {
        return false;
    }

    // Get a handle to the root window.
    rootWindow = DefaultRootWindow(m_videoDisplay);

    // Setup the OpenGL attributes for the window.
    attributeList[0]  = GLX_RGBA;  // Support for 24 bit color and an alpha channel.
    attributeList[1]  = GLX_DEPTH_SIZE;  // Support for 24 bit depth buffer.
    attributeList[2]  = 24;
    attributeList[3]  = GLX_STENCIL_SIZE;  // Support for 8 bit stencil buffer.
    attributeList[4]  = 8;
    attributeList[5]  = GLX_DOUBLEBUFFER;  // Support for double buffering.
    attributeList[6]  = GLX_RED_SIZE;  // 8 bits for each channel.
    attributeList[7]  = 8;
    attributeList[8]  = GLX_GREEN_SIZE;
    attributeList[9]  = 8;
    attributeList[10] = GLX_BLUE_SIZE;
    attributeList[11] = 8;
    attributeList[12] = GLX_ALPHA_SIZE;
    attributeList[13] = 8;
    attributeList[14] = None;  // Null terminate the attribute list.

    // Query for a visual format that fits the attributes we want.
    visualInfo = glXChooseVisual(m_videoDisplay, 0, attributeList);
    if(visualInfo == NULL)
    {
        return false;
    }

    // Create a color map for the window for the specified visual type.
    colorMap = XCreateColormap(m_videoDisplay, rootWindow, visualInfo->visual, AllocNone);

    // Fill out the structure for setting the window attributes.
    setWindowAttributes.colormap = colorMap;
    setWindowAttributes.event_mask = KeyPressMask | KeyReleaseMask | ButtonPressMask | ButtonReleaseMask;

    // Get the size of the default screen.
    if(FULL_SCREEN)
    {
        defaultScreen = XDefaultScreenOfDisplay(m_videoDisplay);
        screenWidth = XWidthOfScreen(defaultScreen);
        screenHeight = XHeightOfScreen(defaultScreen);
    }
    else
    {
        screenWidth = 1024;
        screenHeight = 768;
    }

    // Create the window with the desired settings.
    m_hwnd = XCreateWindow(m_videoDisplay, rootWindow, 0, 0, screenWidth, screenHeight, 0, visualInfo->depth,
			   InputOutput, visualInfo->visual, CWColormap | CWEventMask, &setWindowAttributes);

    // Map the newly created regular window to the video display.
    XMapWindow(m_videoDisplay, m_hwnd);
    
    // Set the name of the window.
    XStoreName(m_videoDisplay, m_hwnd, "Engine");

    // If full screen mode then we send the full screen event and remove the border decoration.
    if(FULL_SCREEN)
    {
        // Setup the full screen states. 
        wmState = XInternAtom(m_videoDisplay, "_NET_WM_STATE", False);
        fullScreenState = XInternAtom(m_videoDisplay, "_NET_WM_STATE_FULLSCREEN", False);

	// Setup the X11 event message that we need to send to make the screen go full screen mode.
        memset(&fullScreenEvent, 0, sizeof(fullScreenEvent));

        fullScreenEvent.type = ClientMessage;
        fullScreenEvent.xclient.window = m_hwnd;
        fullScreenEvent.xclient.message_type = wmState;
        fullScreenEvent.xclient.format = 32;
        fullScreenEvent.xclient.data.l[0] = 1;
        fullScreenEvent.xclient.data.l[1] = fullScreenState;
        fullScreenEvent.xclient.data.l[2] = 0;
	fullScreenEvent.xclient.data.l[3] = 0;
        fullScreenEvent.xclient.data.l[4] = 0;
	
	// Send the full screen event message to the X11 server.
        status = XSendEvent(m_videoDisplay, DefaultRootWindow(m_videoDisplay), False, SubstructureRedirectMask | SubstructureNotifyMask, &fullScreenEvent);
	if(status != 1)
        {
            return false;
        }
 
        // Setup the motif hints to remove the border in full screen mode.
        motifHints = XInternAtom(m_videoDisplay, "_MOTIF_WM_HINTS", False);

        motifHintList[0] = 2;  // Remove border.
	motifHintList[1] = 0;
        motifHintList[2] = 0;
	motifHintList[3] = 0;
	motifHintList[4] = 0;

	// Change the window property and remove the border.
	status = XChangeProperty(m_videoDisplay, m_hwnd, motifHints, motifHints, 32, PropModeReplace, (unsigned char*)&motifHintList, 5);
	if(status != 1)
	{
	    return false;
	}
	
        // Flush the display to apply all the full screen settings.
        status = XFlush(m_videoDisplay);
	if(status != 1)
	{
	    return false;
	}

	// Now sleep for one second for the flush to occur before making a gl context, or if too early the screen gets squished in full screen sometimes.
        sleep(1);
    }
    
    // Create an OpenGL rendering context.
    m_renderingContext = glXCreateContext(m_videoDisplay, visualInfo, NULL, GL_TRUE);
    if(m_renderingContext == NULL)
    {
        return false;
    }

    // Attach the OpenGL rendering context to the newly created window.
    result = glXMakeCurrent(m_videoDisplay, m_hwnd, m_renderingContext);
    if(!result)
    {
        return false;
    }

    // Check that OpenGL 4.0 is supported at a minimum.
    glGetIntegerv(GL_MAJOR_VERSION, &majorVersion);
    if(majorVersion < 4)
    {
        return false;
    }

    // Confirm that we have a direct rendering context.
    result = glXIsDirect(m_videoDisplay, m_renderingContext);
    if(!result)
    {
        return false;
    }

    // If windowed then move the window to the middle of the screen.
    if(!FULL_SCREEN)
    {
        defaultScreen = XDefaultScreenOfDisplay(m_videoDisplay);
        defaultScreenWidth = XWidthOfScreen(defaultScreen);
        defaultScreenHeight = XHeightOfScreen(defaultScreen);

        posX = (defaultScreenWidth - screenWidth) / 2;
        posY = (defaultScreenHeight - screenHeight) / 2;

        status = XMoveWindow(m_videoDisplay, m_hwnd, posX, posY);
        if(status != 1)
        {
            return false;
        }
    }
    
    return true;
}


void SystemClass::ShutdownWindow()
{
    // Shutdown and close the window.
    glXMakeCurrent(m_videoDisplay, None, NULL);
    glXDestroyContext(m_videoDisplay, m_renderingContext);
    XUnmapWindow(m_videoDisplay, m_hwnd);
    XDestroyWindow(m_videoDisplay, m_hwnd);
    XCloseDisplay(m_videoDisplay);

    return;
}


void SystemClass::ReadInput()
{
    XEvent event;
    long eventMask;
    bool foundEvent;
    char keyBuffer[32];
    KeySym keySymbol;


    // Set the event mask to the types of events we are interested in.
    eventMask = KeyPressMask | KeyReleaseMask;

    // Perform the frame processing for the application.
    foundEvent = XCheckWindowEvent(m_videoDisplay, m_hwnd, eventMask, &event);
    if(foundEvent)
    {
        // Key press.
        if(event.type == KeyPress)
        {
            XLookupString(&event.xkey, keyBuffer, sizeof(keyBuffer), &keySymbol, NULL);
            m_Input->KeyDown((int)keySymbol);
        }

        // Key release.
        if(event.type == KeyRelease)
        {
            XLookupString(&event.xkey, keyBuffer, sizeof(keyBuffer), &keySymbol, NULL);
            m_Input->KeyUp((int)keySymbol);
        }
    }

    return;
}
