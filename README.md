# IPK25‑CHAT Client

## 1. Introduction
This README is a documentation for client application implementing the IPK25 chat protocol. Developed for the
second project in the IPK course (2025) at VUT (Brno University of Technology) by student Martin Knor \[xknorm01].


## 2. Table of content
1. [Introduction](#1-Introduction)
2. [Table of content](#2-table-of-content)
3. [Project set up](#3-project-set-up)\
   3.1[Program Execution](#31-program-execution)
4. [Basic Theory](#4-basic-theory)\
   4.1 [TCP](#41-tcp-transmission-control-protocol)\
   4.2 [UDP](#42-udp-user-datagram-protocol)\
   4.3 [Socket](#43-socket)
5. [Project Implementation](#5-project-implementation)
6. [Testing](#6-testing)
7. [Bibliografy](#7-bibliografy)


## 3. Project set up
Project can be built from source files by using `make` command in the root directory. 
The resulting executable is located in root directory as well.

## 3.1 Program Execution
The CLI program supports command line arguments as they are specified in the assignment plus one extra flag

| Argument | Default Value | Possible values        | Meaning or expected program behaviour
|----------|---------------|------------------------| -------------------------------------
| `-t`     | User provided | `tcp` or `udp`         | Transport protocol used for connection
| `-s`     | User provided | IP address or hostname | Server IP or hostname
| `-p`     | `4567`        | `uint16`               | Server port
| `-d`     | `250`         | `uint16`               | UDP confirmation timeout (in milliseconds)
| `-r`     | `3`           | `uint8`                | Maximum number of UDP retransmissions
| `-h`     |               |                        | Prints program help output and exits
|`discord` | false         | boolean                | Flag - adds "discord." prefix to /join

## 4. Basic Theory

### 4.1 Transmission Control Protocol (TCP)
TCP provides a reliable, in‑order stream of bytes from one application to another. 
It breaks your data into segments, wraps each in an IP packet, and ensures every byte arrives
exactly once and in the right order. Because of these guarantees, TCP adds extra overhead and
can be slower or larger in total data sent. It’s the protocol used for file transfers, email,
and secure shells (SSH).

### 4.2 User Datagram Protocol (UDP)
UDP is a very simple protocol for sending discrete messages (“datagrams”) between applications.
Unlike TCP, it does not guarantee delivery, ordering, or protection against duplicates.
It’s used when low latency matters more than perfect reliability—for example, in video calls or live streaming.

### 4.3 IPK25-CHAT
The IPK25-CHAT application protocol that the client is running has 2 different implementations based on the transport
layer protocol it is using. 
- TCP variant is entirely textual defined by ABNF grammar from the assignment. This is possible thanks to the reliability guarantees of the TCP protocol 
- UDP variant introduces a header consisting of 1 byte for message type and 2 bytes for message id and
every IPK25-chat protocol message body is defined differently message to message.\
The header for the UDP variant is needed to implement the connection reliability, that the UDP is missing, on the application level

### 4.4 Sockets
A socket is the programming interface used by clients and servers to talk to each other over the network.
On the server side, you create a socket, bind it to an address (IP + port), and then listen for incoming
client connections. On the client side, you open a socket and connect it to the server’s address. Once connected,
the two can exchange data: the server processes each request and sends back a response over the same socket.

## 5. Project Implementation
I've decided to implement the project in the *C#* using *.NET 9+* framework. I've made this decision because i like the 
high level nature of the language and also because of the excellent documentation of the base SDK.
I've tried my best to approach the project in OOP way, dividing the project into sub-problems where each problem is 
implemented in its own class, but I still think that the project could a lot of refactoring.

The basic structure of the project consists of the following class groups:
- **ChatClient** (UDP/TCP)
  - this class handles user input, message receiving from the server, message sending to the server and client state management
- **Command**
  - uses command factory to create the correct command class based on the user input caught in the ChatClient Class
  - implements command execution, some send messages to server, some only change/print local state/output
- **Message**
  - data structure class
  - each message type implementation holds data needed to construct/prase the message that was caught or that is meant to be sent
- **Other Helper Classes/Enums**
  - ClArgumentsParser
  - ExitHandler
  - Enums
  - Message builder/parser

You can see basic project structure in the Class diagram below 

![Chat App Class Diagram](/Docs/ClientClassDiagram.png)


## 6. Testing
I have tested the application only manually against the reference server hosted at `anton5.fit.vutbr.cz` provided by
the project assigners and by watching the client-server communication in wireshark with the ipk25-chat plugin

### tcp
- command line input:

![tcpInput](/Docs/tcpInput.png)

- wireshark:
![tcpTestCase](/Docs/tcpTestCase.png)

### udp

- honestly i don't have time to finish this section

## 7. Bibliography

https://www.ibm.com/docs/en/i/7.3.0?topic=programming-how-sockets-work

https://www.c-sharpcorner.com/article/socket-programming-in-C-Sharp/

https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/sockets/socket-services#create-a-socket-client

https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.networkstream.-ctor?view=net-9.0#system-net-sockets-networkstream-ctor(system-net-sockets-socket)

https://www.rfc-editor.org/rfc/rfc5234

https://www.cs.uaf.edu/courses/cs441/notes/protocols/index.html





