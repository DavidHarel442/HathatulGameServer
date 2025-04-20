using gameServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HathatulServer
{
    public class CommunicationProtocol
    {// this class manages the communication protocol. the structure is first a message of what to do. second is the unique token. and third is the data for each request

       
       
        /// <summary>
        /// default constructor
        /// </summary>
        public CommunicationProtocol()
        {

        }
        /// <summary>
        /// this function recieves the command and all the other data needed for each reply and transfer it to the protocol
        /// </summary>
        /// <param name="command"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public string TransferToProtocol(string command,string info)
        {
            string answer = command + "\n" + info;
            return answer;
        }
        /// <summary>
        /// this function takes the received message that the client sent and transfers it from the protocol. to the properties 'command', 'username' and arguments
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public HathatulMessage TransferFromProtocol(string message)
        {
            HathatulMessage messageReceived = new HathatulMessage();
            string[] MessageSplited = message.Split('\n');
            messageReceived.command = MessageSplited[0];
            if (MessageSplited[1] != "")
            {
                ClientSession client = GameServer.GetClientWithToken(MessageSplited[1]);
                messageReceived.username = GameServer.VerifyTokenAndReturnUsername(MessageSplited[1], client.loginAndRegister);
            }
            else
            {
                messageReceived.username = MessageSplited[1];
            }


            if (MessageSplited.Length > 2)
            {
                messageReceived.arguments = MessageSplited[2];
            }
            return messageReceived;

        }


    }
}
