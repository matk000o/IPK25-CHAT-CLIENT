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
   4.1 [TCP](#41-transmission-control-protocol-(tcp))\
   4.2 [UDP](#42-user-datagram-protocol-(udp))\
   4.3 [IPK25](#43-ipk25-chat)\
   4.4 [Socket](#44-sockets)
5. [Project Implementation](#5-project-implementation)
6. [Testing](#6-testing)
   6.1 [Unit](#61-unit-testing)
   6.2 [Manual](#62-manual-testing)
   6.3 [Automated](#63-automated-tests)
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

### 4.1 Transmission Control Protocol (TCP) [RFC9293]
TCP provides a reliable, in‑order stream of bytes from one application to another. 
It breaks your data into segments, wraps each in an IP packet, and ensures every byte arrives
exactly once and in the right order. Because of these guarantees, TCP adds extra overhead and
can be slower or larger in total data sent. It’s the protocol used for file transfers, email,
and secure shells (SSH).

### 4.2 User Datagram Protocol (UDP) [RFC768]
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
implemented in its own class, but I still think that the project needs a lot of refactoring.

The basic structure of the project consists of the following class groups:
- **ChatClient** (UDP/TCP)
  - this class handles user input, message receiving from the server, message sending to the server and client state management
- **Command**
  - uses command factory to create the correct command class based on the user input caught in the ChatClient Class
  - implements command execution, some send messages to server, some only change/print local state/output
- **Message**
  - data structure class
  - each message type implementation holds data needed to construct/prase the message that was caught or that is meant to be sent
- **Other Helper Classes**
  - ClArgumentsParser
  - ExitHandler
  - MessageBuilder
  - MessageParser
  - CommandFactory
  - Enums

You can see basic project structure in the Class diagram below 

![Chat App Class Diagram](/Docs/ClientClassDiagram.png)


## 6. Testing

### 6.1 Unit testing

I have created simple unit tests to check the correct functionality of functions used to implement this project.
Classes that are checked by the unit test are: ``ClArgumentParser, MessageBuilder, MessageParser, CommandFactory``\
The tests can be run using ``make test`` command in the root directory

example output of running tests in CL:

```shell
dotnet test
Restore complete (1.1s)
  Client succeeded (2.8s) → Client/bin/Debug/linux-x64/ipk25-chat.dll
  ClientTest succeeded (0.7s) → ClientTest/bin/Debug/net9.0/ClientTest.dll
[xUnit.net 00:00:00.00] xUnit.net VSTest Adapter v2.8.2+699d445a1a (64-bit .NET 9.0.3)
[xUnit.net 00:00:00.07]   Discovering: ClientTest
[xUnit.net 00:00:00.12]   Discovered:  ClientTest
[xUnit.net 00:00:00.13]   Starting:    ClientTest
[xUnit.net 00:00:00.25]   Finished:    ClientTest
  ClientTest test succeeded (1.0s)

Test summary: total: 33, failed: 0, succeeded: 33, skipped: 0, duration: 1.0s
Build succeeded in 5.9s
martin@KnorPc:~/school_LX/IPK/IPK25-CHAT-CLIENT$ 
```



### 6.2 manual testing

I have tested the functionality of the application manually against the reference server hosted at `anton5.fit.vutbr.cz` provided by
the project assigners. I've caught the client-server communication in wireshark with the ipk25-chat plugin

#### tcp
- command line input:\
![tcpInput](/Docs/tcpInputCopy.png)

- wireshark:
![tcpTestCase](/Docs/tcpTestCaseCopy.png)

#### udp

- command line input:\
![udpInput](/Docs/udpInputcopy.png)

- wireshark:\
sadly for the UDP test case I wasn't able to install the ipk25-chat plugin for wireshark, so it's hard the content of the messages.
![udpTestCase](/Docs/udpTestCase.png)

### 6.3 automated tests
- to test the correct formating of my programs output I've used automated student tests **created by Tomáš HOBZA and Vladyslav MALASHCHUK** [1]

## 7. Bibliography

[RFC768] Postel, J. User Datagram Protocol [online]. March 1997. [cited 2025-04-21]. DOI: 10.17487/RFC0768. Available at:\
https://datatracker.ietf.org/doc/html/rfc768 \
[RFC9293] Eddy, W. Transmission Control Protocol (TCP) [online]. August 2022. [cited 2025-04-21]. DOI: 10.17487/RFC9293. Available at:\
https://datatracker.ietf.org/doc/html/rfc9293#name-key-tcp-concept \
[IBM] IBM. How Sockets Work [online]. [used 2025-04-21]. Available at:\
https://www.ibm.com/docs/en/i/7.3?topic=programming-how-sockets-work \
[Microsoft] Microsoft. System.Timers.Timer Class [online]. [used 2025-04-21]. Available at:\
https://learn.microsoft.com/en-us/dotnet/api/system.timers.timer?view=net-8.0 \
[Microsoft] Microsoft. Asynchronous Programming in C# [online]. [used 2025-04-21]. Available at:\
https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/ \
[1] Vladyslav MALASHCHUK, Tomáš HOBZA, et al. VUT_IPK_CLIENT_TESTS [online]. GitHub, 2025 [cit. 2025-04-21]. Available at:\
https://github.com/Vlad6422/VUT_IPK_CLIENT_TESTS





