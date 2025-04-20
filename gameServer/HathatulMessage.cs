using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gameServer
{
    public class HathatulMessage
    {// this class contains the message that was transfered from the communication protocol

        /// <summary>
        /// property 'command' means what the request is.
        /// </summary>
        public string command = "";
        /// <summary>
        /// this property 'username' contains the username of the requester.
        /// </summary>
        public string username = "";
        /// <summary>
        /// this property 'arguments' contains the arguments of what the server should recieve to fulfill the request
        /// </summary>
        public string arguments = "";

        /// <summary>
        /// default constructor
        /// </summary>
        public HathatulMessage()
        {

        }
    }
}
