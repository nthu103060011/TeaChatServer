using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeaChatServer
{
    public class ChatroomInfo
    {
        public class Member
        {
            public ChatSocket socket;
            public int chatroomIndex;

            public Member(ChatSocket socket, int chatroomIndex)
            {
                this.socket = socket;
                this.chatroomIndex = chatroomIndex;
            }
        }

        public List<Member> memberList = new List<Member>();

        public void setChatroomIndex(ChatSocket socket, int index)
        {
            foreach (Member m in memberList)
            {
                if (m.socket.Equals(socket))
                {
                    m.chatroomIndex = index;
                    break;
                }
            }
        }

    }
}
