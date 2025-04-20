APP_NAME = ipk25-chat
SRC_DIR = ./Client/
BIN = bin
OBJ = obj

.PHONY: clean build 

all: build

build: 
	dotnet publish ./Client -c Release /p:DebugType=None -o .

tcp: build
	./$(APP_NAME) -s 127.0.0.1 -t tcp
udp: build
	./$(APP_NAME) -s 127.0.0.1 -t udp
tcpD: build
	./$(APP_NAME) -s anton5.fit.vutbr.cz -t tcp --discord
udpD: build
	./$(APP_NAME) -s anton5.fit.vutbr.cz -t udp --discord
clear:
	dotnet nuget locals all --clear
clean:
	rm -rf $(APP_NAME)
	rm -rf $(SRC_DIR)/$(BIN)
	rm -rf $(SRC_DIR)/$(OBJ)
