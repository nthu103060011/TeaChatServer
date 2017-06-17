using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TeaChat.Uitlity;

namespace TeaChatServer
{
    public class Packet
    {
        byte[] packet;

        public static readonly int PACKET_MAX_SIZE = 8192;
        public static readonly int PACKET_HEADER_SIZE = 6;
        public static readonly int PACKET_MAX_BODY_SIZE = PACKET_MAX_SIZE - PACKET_HEADER_SIZE;

        public enum Commands
        {
            ReportName,     // string username
            UpdateUserList, // List<string> onlineUsers
            ChatRequest,    // List<string> chatFriends
            RegisterChatroom,   // int chatroomIndex, int chatroomIndexOnServer
            LeaveChatroom,  // int chatroomIndex, string leavingFriend
            LogOut,

            AddStroke,      // int chatroomIndex, string drawingAttributesText, string stylusPointsText
            EraseAll,       // int chatroomIndex
            AddTextBox,     // int chatroomIndex, string text, string X, string Y
            TextMessage,    // int chatroomIndex, string fromWho, string text
            BackgroundImage,// int chatroomIndex, string filename, byte[] data
            File,           // int chatroomIndex, string filename, byte[] data
            OpenConferenceCall, // client-to-server first then server-to-client
            ParticipateConferenceCall, // client-to-server
            ConferenceCallOn, // server-to-client
            AudioData,      // char room number, data
        }

        public Packet()
        {
            packet = new byte[PACKET_MAX_SIZE];
        }

        public Packet(byte[] packet)
        {
            this.packet = new byte[PACKET_MAX_SIZE];
            this.SetPacket(packet);
        }

        public byte[] getPacket()
        {
            return packet;
        }

        public Commands getCommand()
        {
            return (Commands)packet[0];
        }

        public int getChatroomIndex()
        {
            return packet[1];
        }

        public void changeChatroomIndex(int chatroomIndex)
        {
            packet[1] = (byte)chatroomIndex;
        }

        public void SetPacket(byte[] buff)
        {
            ArrayUtility.CopyByteArray(this.packet, 0, buff, 0, buff.Length);
        }

        public int getDataSize()
        {
            return BitConverter.ToInt32(packet, 2);
        }

        public int GetPacketBody(byte[] buff)
        {
            return ArrayUtility.CopyByteArray(buff, 0, this.packet, 2, this.getDataSize() + PACKET_HEADER_SIZE);
        }

        #region getPacketData
        public string getReportNameData()
        {
            int dataSize = getDataSize();
            byte[] data = new byte[dataSize];
            Array.Copy(packet, 6, data, 0, dataSize);
            return Encoding.UTF8.GetString(data);
        }

        public List<string> getUpdateUserListData()
        {
            int dataSize = getDataSize();
            byte[] data = new byte[dataSize];
            Array.Copy(packet, 6, data, 0, dataSize);
            string json = Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject<List<string>>(json);
        }

        public List<string> getChatRequestData()
        {
            int dataSize = getDataSize();
            byte[] data = new byte[dataSize];
            Array.Copy(packet, 6, data, 0, dataSize);
            string json = Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject<List<string>>(json);
        }

        public int getRegisterChatroomData()
        {
            return packet[2];
        }

        public string getLeavingFriendData()
        {
            int dataSize = getDataSize();
            byte[] data = new byte[dataSize];
            Array.Copy(packet, 6, data, 0, dataSize);
            return Encoding.UTF8.GetString(data);
        }

        public string[] getAddStrokeData()
        {
            int dataSize = getDataSize();
            byte[] data = new byte[dataSize];
            Array.Copy(packet, 6, data, 0, dataSize);
            string json = Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject<string[]>(json);
        }

        public string[] getAddTextBoxData()
        {
            int dataSize = getDataSize();
            byte[] data = new byte[dataSize];
            Array.Copy(packet, 6, data, 0, dataSize);
            string json = Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject<string[]>(json);
        }

        public string[] getTextMessageData()
        {
            int dataSize = getDataSize();
            byte[] data = new byte[dataSize];
            Array.Copy(packet, 6, data, 0, dataSize);
            string json = Encoding.UTF8.GetString(data);
            return JsonConvert.DeserializeObject<string[]>(json);
        }

        public string getFilename()
        {
            byte[] filenameByte = new byte[64];
            Array.Copy(packet, 6, filenameByte, 0, 64);
            string filename = Encoding.UTF8.GetString(filenameByte);
            return filename.TrimEnd((char)0);
        }

        public byte[] getFileData()
        {
            int dataSize = Math.Min(8118, getDataSize());
            byte[] data = new byte[dataSize];
            Array.Copy(packet, 74, data, 0, dataSize);
            return data;
        }

        public int getFileSerialNumber()
        {
            return BitConverter.ToInt32(packet, 70);
        }
        #endregion

        #region makePacket
        public void makePacketReportName(string username)
        {
            packet.Initialize();
            packet[0] = (byte)Commands.ReportName;
            packet[1] = byte.MaxValue;
            byte[] data = Encoding.UTF8.GetBytes(username);
            byte[] dataSize = BitConverter.GetBytes(data.Length);
            Array.Copy(dataSize, 0, packet, 2, 4);
            Array.Copy(data, 0, packet, 6, data.Length);
        }

        public void makePacketUpdateUserList(List<string> onlineUsers)
        {
            packet.Initialize();
            packet[0] = (byte)Commands.UpdateUserList;
            packet[1] = byte.MaxValue;
            string json = JsonConvert.SerializeObject(onlineUsers);
            byte[] data = Encoding.UTF8.GetBytes(json);
            byte[] dataSize = BitConverter.GetBytes(data.Length);
            Array.Copy(dataSize, 0, packet, 2, 4);
            Array.Copy(data, 0, packet, 6, data.Length);
        }

        public void makePacketChatRequest(List<string> chatFriends)
        {
            packet.Initialize();
            packet[0] = (byte)Commands.ChatRequest;
            packet[1] = byte.MaxValue;
            string json = JsonConvert.SerializeObject(chatFriends);
            byte[] data = Encoding.UTF8.GetBytes(json);
            byte[] dataSize = BitConverter.GetBytes(data.Length);
            Array.Copy(dataSize, 0, packet, 2, 4);
            Array.Copy(data, 0, packet, 6, data.Length);
        }

        public void makePacketRegisterChatroom(int chatroomIndex, int chatroomIndexOnServer)
        {
            packet.Initialize();
            packet[0] = (byte)Commands.RegisterChatroom;
            packet[1] = (byte)chatroomIndex;
            packet[2] = (byte)chatroomIndexOnServer;
        }

        public void makePacketLeaveChatroom(int chatroomIndex, string myName)
        {
            packet.Initialize();
            packet[0] = (byte)Commands.LeaveChatroom;
            packet[1] = (byte)chatroomIndex;
            byte[] data = Encoding.UTF8.GetBytes(myName);
            byte[] dataSize = BitConverter.GetBytes(data.Length);
            Array.Copy(dataSize, 0, packet, 2, 4);
            Array.Copy(data, 0, packet, 6, data.Length);
        }

        public void makePacketLogOut()
        {
            packet.Initialize();
            packet[0] = (byte)Commands.LogOut;
            packet[1] = byte.MaxValue;
            byte[] dataSize = BitConverter.GetBytes(0);
            Array.Copy(dataSize, 0, packet, 2, 4);
        }

        public void makePacketAddStroke(int chatroomIndex, string drawingAttributesText, string stylusPointsText)
        {
            packet.Initialize();
            packet[0] = (byte)Commands.AddStroke;
            packet[1] = (byte)chatroomIndex;
            string[] stringArray = new string[2] { drawingAttributesText, stylusPointsText };
            string json = JsonConvert.SerializeObject(stringArray);
            byte[] data = Encoding.UTF8.GetBytes(json);
            byte[] dataSize = BitConverter.GetBytes(data.Length);
            Array.Copy(dataSize, 0, packet, 2, 4);
            Array.Copy(data, 0, packet, 6, Math.Min(8186, data.Length));
        }

        public void makePacketEraseAll(int chatroomIndex)
        {
            packet.Initialize();
            packet[0] = (byte)Commands.EraseAll;
            packet[1] = (byte)chatroomIndex;
            byte[] dataSize = BitConverter.GetBytes(0);
            Array.Copy(dataSize, 0, packet, 2, 4);
        }

        public void makePacketAddTextBox(int chatroomIndex, string text, string X, string Y)
        {
            packet.Initialize();
            packet[0] = (byte)Commands.AddTextBox;
            packet[1] = (byte)chatroomIndex;
            string[] stringArray = new string[3] { text, X, Y };
            string json = JsonConvert.SerializeObject(stringArray);
            byte[] data = Encoding.UTF8.GetBytes(json);
            byte[] dataSize = BitConverter.GetBytes(data.Length);
            Array.Copy(dataSize, 0, packet, 2, 4);
            Array.Copy(data, 0, packet, 6, data.Length);
        }

        public void makePacketTextMessage(int chatroomIndex, string fromWho, string text)
        {
            packet.Initialize();
            packet[0] = (byte)Commands.TextMessage;
            packet[1] = (byte)chatroomIndex;
            string[] stringArray = new string[2] { fromWho, text };
            string json = JsonConvert.SerializeObject(stringArray);
            byte[] data = Encoding.UTF8.GetBytes(json);
            byte[] dataSize = BitConverter.GetBytes(data.Length);
            Array.Copy(dataSize, 0, packet, 2, 4);
            Array.Copy(data, 0, packet, 6, data.Length);
        }

        public void makePacketBackgroundImage(int chatroomIndex, string filename, int serialNumber, byte[] data, int dataSize)
        {
            packet.Initialize();
            packet[0] = (byte)Commands.BackgroundImage;
            packet[1] = (byte)chatroomIndex;
            Array.Copy(BitConverter.GetBytes(dataSize), 0, packet, 2, 4);
            byte[] filenameByte = Encoding.UTF8.GetBytes(filename);
            Array.Copy(filenameByte, 0, packet, 6, filenameByte.Length);
            Array.Copy(BitConverter.GetBytes(serialNumber), 0, packet, 70, 4);
            Array.Copy(data, 0, packet, 74, Math.Min(8118, dataSize));
        }

        public void makePacketFile(int chatroomIndex, string filename, int serialNumber, byte[] data, int dataSize)
        {
            packet.Initialize();
            packet[0] = (byte)Commands.File;
            packet[1] = (byte)chatroomIndex;
            Array.Copy(BitConverter.GetBytes(dataSize), 0, packet, 2, 4);
            byte[] filenameByte = Encoding.UTF8.GetBytes(filename);
            Array.Copy(filenameByte, 0, packet, 6, filenameByte.Length);
            Array.Copy(BitConverter.GetBytes(serialNumber), 0, packet, 70, 4);
            Array.Copy(data, 0, packet, 74, Math.Min(8118, dataSize));
        }

        #region conferecen call handshake packet creation
        /// <summary>
        /// Create packet that contains command of opening conference call.
        /// No extra body data is in need.
        /// This packet is created by client host or server.
        /// </summary>
        /// <param name="chat_room_index">index of chat room</param>
        public void MakeOpenConfCallPakcet(int chat_room_index)
        {
            ArrayUtility.ZeroByteArray(this.packet);
            this.packet[0] = (byte)Commands.OpenConferenceCall;
            this.packet[1] = (byte)chat_room_index;
        }

        /// <summary>
        /// Create packet that contains command of participating conference call.
        /// No extra body data is in need.
        /// This packet is only created and sent by client.
        /// </summary>
        /// <param name="chat_room_index">index of chat room</param>
        public void MakePartConfCallPacket(int chat_room_index)
        {
            ArrayUtility.ZeroByteArray(this.packet);
            this.packet[0] = (byte)Commands.ParticipateConferenceCall;
            this.packet[1] = (byte)chat_room_index;
        }

        /// <summary>
        /// Create packet that contains command of conference call on.
        /// No extra body data is in need.
        /// This packet is only created and sent by server.
        /// </summary>
        /// <param name="chat_room_index">index of chat room</param>
        public void MakeConfCallOnPacket(int chat_room_index)
        {
            ArrayUtility.ZeroByteArray(this.packet);
            this.packet[0] = (byte)Commands.ConferenceCallOn;
            this.packet[1] = (byte)chat_room_index;
        }
        #endregion

        #region audio packet creation
        /// <summary>
        /// Create packet that contain raw audio data into buffer.
        /// </summary>
        /// <param name="buff">buffer to load the packet</param>
        /// <param name="chat_room_index">index of chat room</param>
        /// <param name="data">audio data</param>
        /// <param name="data_size">size of audio data</param>
        /// <returns>size of packet</returns>
        public static int CreateAudioPacket(byte[] buff, int chat_room_index, byte[] data, int data_size)
        {
            if (buff == null || data == null) throw new ArgumentNullException();
            if (buff.Length < data_size + PACKET_HEADER_SIZE) return -1;

            int packet_size = 0;

            ArrayUtility.ZeroByteArray(buff);

            buff[0] = (byte)Commands.AudioData;
            buff[1] = (byte)chat_room_index;
            packet_size += 2;

            byte[] data_size_num_to_bytes = BitConverter.GetBytes(data_size);
            packet_size += ArrayUtility.CopyByteArray(
                buff, packet_size,
                data_size_num_to_bytes, 0, data_size_num_to_bytes.Length
                );


            packet_size += ArrayUtility.CopyByteArray(buff, packet_size, data, 0, data_size);

            return packet_size;
        }
        #endregion
        #endregion

    }
}
