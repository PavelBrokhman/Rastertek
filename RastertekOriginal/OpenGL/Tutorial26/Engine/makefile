client: applicationclass.o cameraclass.o inputclass.o main.o modelclass.o openglclass.o systemclass.o textureclass.o fogshaderclass.o
	g++ -o client applicationclass.o cameraclass.o inputclass.o main.o modelclass.o openglclass.o systemclass.o textureclass.o fogshaderclass.cpp -l GL -l X11

applicationclass.o: applicationclass.cpp
	g++ -c applicationclass.cpp

cameraclass.o: cameraclass.cpp
	g++ -c cameraclass.cpp

fogshaderclass.o: fogshaderclass.cpp
	g++ -c fogshaderclass.cpp

inputclass.o: inputclass.cpp
	g++ -c inputclass.cpp

main.o: main.cpp
	g++ -c main.cpp

modelclass.o: modelclass.cpp
	g++ -c modelclass.cpp

openglclass.o: openglclass.cpp
	g++ -c openglclass.cpp

systemclass.o: systemclass.cpp
	g++ -c systemclass.cpp

textureclass.o: textureclass.cpp
	g++ -c textureclass.cpp
