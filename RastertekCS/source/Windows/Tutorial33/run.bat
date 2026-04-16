@echo off
cd /d "%~dp0"
rem Build and run DX11 Tutorial33 C++ reference source
rem Requires MSVC (cl.exe) in PATH - run from Developer Command Prompt
cl /EHsc /W3 /O2 /Fe:tutorial33.exe ^
    main.cpp applicationclass.cpp cameraclass.cpp d3dclass.cpp ^
    fireshaderclass.cpp inputclass.cpp modelclass.cpp ^
    systemclass.cpp textureclass.cpp ^
    /link d3d11.lib d3dcompiler.lib dxgi.lib
if %ERRORLEVEL% == 0 tutorial33.exe
