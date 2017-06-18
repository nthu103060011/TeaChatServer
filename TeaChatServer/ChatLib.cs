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
        public static readonly int PACKET_MAX_SIZE = 2048;

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
            byte[] msg = new byte[PACKET_MAX_SIZE];
            int bytesRead = socket.Receive(msg);
            Console.WriteLine("receive " + bytesRead);
            if (bytesRead != PACKET_MAX_SIZE)
                Console.WriteLine("first " + bytesRead);
            while (bytesRead < PACKET_MAX_SIZE)
            {
                byte[] tmp = new byte[PACKET_MAX_SIZE];
                int bytesReadtmp = socket.Receive(tmp);
                Console.WriteLine(bytesReadtmp);
                if (bytesRead + bytesReadtmp > PACKET_MAX_SIZE)
                    return tmp;
                else
                    Array.Copy(tmp, 0, msg, bytesRead, bytesReadtmp);
                bytesRead += bytesReadtmp;
            }
            return msg;
        }

        public void close()
        {
            socket.Close();
        }

        public ChatSocket send(byte[] line)
        {
            Console.WriteLine("send "+line);
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
                    if (line != null)
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
