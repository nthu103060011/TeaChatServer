using System;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TeaChatServer
{
    public class ChatServer
    {
        List<ChatSocket> clientList = new List<ChatSocket>();
        List<string> userList = new List<string>(); 
        List<ChatroomInfo> chatroomList = new List<ChatroomInfo>(); //server chatroom
        List<string> accountList = new List<string>(); //register list
        List<string> passwordList = new List<string>();
        //ChatroomInfo info;
        
        public static void Main(String[] args)
        {
            ChatServer chatServer = new ChatServer();
            chatServer.run();
        }

        public void run()
        {
            if (File.Exists("account.json"))
                accountList = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText("account.json"));
            if (File.Exists("password.json"))
                passwordList = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText("password.json"));
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, ChatSetting.port);

            Socket newsock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            newsock.Bind(ipep);
            newsock.Listen(1000);

            while (true)
            {
                Socket socket = newsock.Accept();
                Console.WriteLine("接受一個新連線!");
                ChatSocket client = new ChatSocket(socket);
                try
                {
                    //clientList.Add(client);
                    client.newListener(processMsgComeIn);
                }
                catch
                {
                }
//                clientList.Remove(client);
            }
            //	  newsock.Close();
        }

        public void processMsgComeIn(byte[] msg, ChatSocket socket)
        {
            Packet packet = new Packet(msg);
            switch (packet.getCommand())
            {
                case Packet.Commands.RequestUserRegister:
                    string[]  accountName= packet.GetUserRegisterData();
                    bool sure_reg = false; 
                    foreach (string reg in accountList)
                    {
                        if (reg.Equals(accountName[0]))
                        {
                            sure_reg = true;
                            break;
                        }
                    }
                    if (sure_reg == false)
                    {
                        Console.WriteLine(accountName + " 註冊成功");
                        accountList.Add(accountName[0]);
                        passwordList.Add(accountName[1]);
                        File.WriteAllText("account.json", JsonConvert.SerializeObject(accountList));
                        File.WriteAllText("password.json", JsonConvert.SerializeObject(passwordList));
                        packet.MakePacketUserRegisterAccept();
                        socket.send(packet.getPacket());
                    }
                    else
                    {
                        Console.WriteLine(accountName + " 註冊失敗");
                        packet.MakePacketUserRegisterDeny();
                        socket.send(packet.getPacket());
                    }
                   
                    break;

                case Packet.Commands.ReportName:
                    string[] name = packet.getReportNameData();
                    bool sure = false;
                    Console.WriteLine("收到使用者：" + name);
                    Console.WriteLine(socket.socket.RemoteEndPoint.ToString());
                    foreach (string account in accountList)
                    {                              
                        if (name[0].Equals(account) )
                        {
                            int pwd_index = accountList.IndexOf(account);
                            //Console.WriteLine("index = " + pwd_index);
                            if (name[1].Equals(passwordList[pwd_index]))
                            {
                                sure = true;
                                Console.WriteLine(name[0] + " 已登入");
                                packet.MakePakcetAccountAuthorized();
                                socket.send(packet.getPacket());
                                clientList.Add(socket);
                                userList.Insert(clientList.IndexOf(socket), name[0]);
                                sendUserList();
                                break;
                            }
                            
                        }
                        
                    }
                    if (sure == false)
                    {
                        packet.MakePaketAccountInvalid();
                        socket.send(packet.getPacket());
                        //socket.close();
                    }
                   
                    break;

                case Packet.Commands.ChatRequest:
                    //Console.WriteLine("enter chatrequest");
                    ChatroomInfo info = new ChatroomInfo();
                    info.memberList.Add(new ChatroomInfo.Member(socket, packet.getChatroomIndex()));
                    foreach (string user in packet.getChatRequestData())
                    {                      
                        //Console.WriteLine(user);
                        info.memberList.Add(new ChatroomInfo.Member(getSocketByName(user), -1));                     
                    }
                    chatroomList.Add(info);
                    int serverindex = chatroomList.IndexOf(info);

                    foreach (string user in packet.getChatRequestData())
                    {
                        List<string> sendto = new List<string>();
                        for (int i = 0; i < info.memberList.Count; i++)
                        {
                            
                            if (user != getNameBySocket(info.memberList[i].socket))
                            {
                                //Console.WriteLine("sendto" + user + "List" + getNameBySocket(info.memberList[i].socket));
                                sendto.Add(getNameBySocket(info.memberList[i].socket));
                            }
                                
                        }
                        Packet packet1 = new Packet();
                   
                        packet1.makePacketChatRequest(sendto);
                        packet1.changeChatroomIndex(serverindex);
                        byte[] byte1 = packet1.getPacket();
                        getSocketByName(user).send(byte1);
                    }

                    break;

                case Packet.Commands.RegisterChatroom:
                    //Console.WriteLine("enter RegisterChatroom");
                    int socketindex = packet.getChatroomIndex();
                    int chatroomindex = packet.getRegisterChatroomData();
                    chatroomList[chatroomindex].setChatroomIndex(socket, socketindex);
                    break;

                case Packet.Commands.LeaveChatroom:                
                case Packet.Commands.TextMessage:
                case Packet.Commands.AddStroke:
                case Packet.Commands.EraseAll:
                case Packet.Commands.AddTextBox:
                case Packet.Commands.BackgroundImage:
                case Packet.Commands.File:
                case Packet.Commands.AudioData:
                    int chatroomIndex = packet.getChatroomIndex();
                    

                    int findIndex = findChatroomIndex(socket, chatroomIndex);
                    //Console.WriteLine("findIndex " + findIndex);
                   
                    for (int i =0; i< chatroomList[findIndex].memberList.Count; i++)
                    {
                        ChatSocket sock = chatroomList[findIndex].memberList[i].socket;
                        int index = chatroomList[findIndex].memberList[i].chatroomIndex;
                        
                        if (sock != socket)
                        {
                            //Console.WriteLine("change index " + index);
                            packet.changeChatroomIndex(index);
                            sock.send(packet.getPacket());
                        }
                    }
                    //paket.changeChatroomIndex(2);
                    break;
                case Packet.Commands.OpenConferenceCall:
                    //send to host
                    int clientIndex = packet.getChatroomIndex();
                    Packet packet2 = new Packet();
                    packet2.MakeConfCallOnPacket(clientIndex);
                    packet2.changeChatroomIndex(clientIndex);
                    socket.send(packet2.getPacket());
                    //send to peer
                    Packet packet3 = new Packet();
                    int serverIndex = findChatroomIndex(socket, clientIndex);
                    //Console.WriteLine("serverIndex " + serverIndex);
                    for (int i = 0; i < chatroomList[serverIndex].memberList.Count; i++)
                    {
                        ChatSocket sock = chatroomList[serverIndex].memberList[i].socket;
                        int index = chatroomList[serverIndex].memberList[i].chatroomIndex;

                        if (sock != socket)
                        {
                            packet3.MakeOpenConfCallPakcet(index);
                            packet3.changeChatroomIndex(index);
                            sock.send(packet3.getPacket());
                        }
                    }
                    break;
                case Packet.Commands.ParticipateConferenceCall:
                    Packet packet4 = new Packet();
                    packet4.MakeConfCallOnPacket(packet.getChatroomIndex());
                    socket.send(packet4.getPacket());
                    break; 

                //case Packet.Commands.
                
                case Packet.Commands.LogOut:
                    Console.WriteLine(userList[clientList.IndexOf(socket)] + "登出");
                    userList.Remove(getNameBySocket(socket));
                    clientList.Remove(socket);
                    sendUserList();
                    socket.close();
                    break;
            }
        }
        public int findChatroomIndex(ChatSocket socket , int index)
        {
            int findIndex = 0;
            for (int i = 0; i < chatroomList.Count; i++)
            {
                for (int j = 0; j < chatroomList[i].memberList.Count; j++)
                {
                    if (socket == chatroomList[i].memberList[j].socket && index == chatroomList[i].memberList[j].chatroomIndex)
                    {
                        findIndex = i;
                    }
                }
            }
            return findIndex;
        }
        public ChatSocket getSocketByName(string name)
        {
            for (int i=0; i<userList.Count;i++)
            {
                if (userList[i] == name)
                    return clientList[i];
            }
            return null;
        }
        public string getNameBySocket(ChatSocket sock)
        {
            for (int i = 0; i < clientList.Count; i++)
            {
                if (clientList[i] == sock)
                    return userList[i];
            }
            return null;
        }
        public void sendUserList()
        {
            for (int i = 0; i < clientList.Count; i++)
            {
                List<string> exceptList = new List<string>();
                for (int j = 0; j < userList.Count; j++)
                {
                    if (i != j)
                        exceptList.Add(userList[j]);

                }
                Packet packet = new Packet();
                packet.makePacketUpdateUserList(exceptList);
               
                byte[] exceptListByte = packet.getPacket();
                clientList[i].send(exceptListByte);
            }
        }

    }
}
