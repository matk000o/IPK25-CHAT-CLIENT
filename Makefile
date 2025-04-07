APP_NAME = ipk25-chat
SRC_DIR = ./Client/
BIN = bin
OBJ = obj

.PHONY: clean build 

all: build

build: 
	dotnet publish ./Client -c Release /p:DebugType=None -o .
