using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace TeaChatServer
{
    public class ChatSetting
    {
        public static String serverIp = "127.0.0.1";
        public static int port = 9877;
    }

    public delegate void StrHandler(byte[] str, ChatSocket socket);

    public class ChatSocket
    {
        public Socket socket;
        public NetworkStream stream;
        public StreamReader reader;
        public StreamWriter writer;
        public StrHandler inHandler;
        public EndPoint remoteEndPoint;
        public bool isDead = false;

        public ChatSocket(Socket s)
        {
            socket = s;
            stream = new NetworkStream(s);
            reader = new StreamReader(stream);
            writer = new StreamWriter(stream);
            remoteEndPoint = socket.RemoteEndPoint;
        }

        public byte[] receive()
        {
            byte[] msg = new byte[8192];
            socket.Receive(msg);
            return msg;
        }

        public void close()
        {
            socket.Close();
        }

        public ChatSocket send(byte[] line)
        {
            socket.Send(line);
            return this;
        }

        public static ChatSocket connect(String ip)
        {
            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(ip), ChatSetting.port);

            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.Connect(ipep);
            }
            catch
            {
                return null;
            }
            return new ChatSocket(socket);
        }

        public Thread newListener(StrHandler pHandler)
        {
            inHandler = pHandler;

            Thread listenThread = new Thread(new ThreadStart(listen));
            listenThread.Start();
            return listenThread;
        }

        public void listen()
        {
            try
            {
                while (true)
                {
                    byte[] line = receive();
                    inHandler(line, this);
                }
            }
            catch (Exception ex)
            {
                isDead = true;
                Console.WriteLine(ex.Message);
            }
        }
    }
}
