APP_NAME = ipk25-chat
SRC_DIR = ./Client/
BIN = bin
OBJ = obj

.PHONY: clean build 

all: build

build: 
	dotnet publish ./Client -c Release /p:DebugType=None -o .

tcp: build
	./ipk25-chat -s anton5.fit.vutbr.cz -t tcp --discord
udp: build
	./ipk25-chat -s anton5.fit.vutbr.cz -t udp --discord
restore:
	dotnet nuget locals all --clear
	dotnet restore --verbosity diagnostic

clear:
	dotnet nuget locals all --clear

clean:
	rm -rf $(APP_NAME)
	rm -rf $(SRC_DIR)/$(BIN)
	rm -rf $(SRC_DIR)/$(OBJ)
#	rm -rf $(TEST_DIR)/$(BIN)
#	rm -rf $(TEST_DIR)/$(OBJ)
