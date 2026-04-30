client: applicationclass.o inputclass.o main.o openalclass.o openglclass.o sound3dclass.o soundclass.o systemclass.o
	g++ -o client applicationclass.o inputclass.o main.o openalclass.o openglclass.o sound3dclass.o soundclass.o systemclass.o -l GL -l X11 -l openal

applicationclass.o: applicationclass.cpp
	g++ -c applicationclass.cpp

inputclass.o: inputclass.cpp
	g++ -c inputclass.cpp

main.o: main.cpp
	g++ -c main.cpp

openalclass.o: openalclass.cpp
	g++ -c openalclass.cpp

openglclass.o: openglclass.cpp
	g++ -c openglclass.cpp

sound3dclass.o: sound3dclass.cpp
	g++ -c sound3dclass.cpp

soundclass.o: soundclass.cpp
	g++ -c soundclass.cpp

systemclass.o: systemclass.cpp
	g++ -c systemclass.cpp
