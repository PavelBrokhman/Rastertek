client: applicationclass.o inputclass.o main.o openglclass.o systemclass.o
	g++ -o client applicationclass.o inputclass.o main.o openglclass.o systemclass.o -l GL -l X11

applicationclass.o: applicationclass.cpp
	g++ -c applicationclass.cpp

inputclass.o: inputclass.cpp
	g++ -c inputclass.cpp

main.o: main.cpp
	g++ -c main.cpp

openglclass.o: openglclass.cpp
	g++ -c openglclass.cpp

systemclass.o: systemclass.cpp
	g++ -c systemclass.cpp
