using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace HathatulServer
{
    internal class GameServer
    {//the class that runs when you start the run. By pressing 'Start'. this class is incharge of the GameServer. it basically managers the whole game


        // Store list of all clients connecting to the server
        // the list is static so all memebers of the chat will be able to obtain list
        // of current connected client
        public static Hashtable Sessions = new Hashtable();
        /// <summary>
        /// this property allows the server to communicate with the client
        /// </summary>
        public static CommunicationProtocol communicationProtocol = new CommunicationProtocol();

        /// <summary>
        /// two static properties. manages the game with 'Manager'. and protects againts a dos attack with 'IPDosManager'
        /// </summary>
        public static GameManager Manager = new GameManager(communicationProtocol);
        public static DosProtection IPDosManager = new DosProtection();
        /// <summary>
        /// this property 'portNo' contains the value of the port the server sits on
        /// </summary>
        const int portNo = 5000;
        /// <summary>
        /// this property 'ipAddress' is the IPAddress the server sits on. local host
        /// </summary>
        private const string ipAddress = "127.0.0.1";
        /// <summary>
        /// the main. function that runs when the server runs, By pressing on "Start" or F5
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            System.Net.IPAddress localAdd = System.Net.IPAddress.Parse(ipAddress);

            TcpListener listener = new TcpListener(localAdd, portNo);

            Console.WriteLine("Simple TCP Server");
            Console.WriteLine("Listening to ip {0} port: {1}", ipAddress, portNo);
            Console.WriteLine("Server is ready.");

            // Start listen to incoming connection requests
            listener.Start();

            // infinit loop.
            while (true)
            {
                // AcceptTcpClient - Blocking call
                // Execute will not continue until a connection is established

                // create an instance of ClientSession so the server will be able to 
                // serve multiple client at the same time.
                //AcceptTcpClient open new socket for the new client
                TcpClient tcpClient = listener.AcceptTcpClient();
                Console.WriteLine("new socket: " + tcpClient.Client.RemoteEndPoint.ToString());

                IPAddress ip = IPAddress.Parse(((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString());
                if (IPDosManager.ShouldAllowToContinueSession(ip))
                {
                    ClientSession userSession = new ClientSession(tcpClient,communicationProtocol);
                    GameServer.Sessions.Add(userSession.GetClientIP, userSession);
                }
               
            }
        }


        /// <summary>
        /// this function removes a ClientSession from the hashtable
        /// </summary>
        /// <param name="clientIP"></param>
        public static void RemoveClientSession(string clientIP)
        {
            GameServer.Sessions.Remove(clientIP);
        }

        /// <summary>
        /// this function checks if the token is correct and exists in the database and if so it will return the name of the username with the same token
        /// </summary>
        /// <param name="token"></param>
        /// <param name="certain"></param>
        /// <returns></returns>
        public static string VerifyTokenAndReturnUsername(string token, LoginAndRegisterPhase certain)
        {
            ClientSession client = GetClientWithToken(token);
            if (certain.C.DoesTokenExist(token))
            {
                return certain.C.GetUsernameFromToken(token);
            }
            return "DoesNotExist";
        }

        /// <summary>
        /// this fucntion return the client with the same token.
        /// </summary>
        /// <param name="Token"></param>
        /// <returns></returns>
        public static ClientSession GetClientWithToken(string Token)
        {
            foreach (DictionaryEntry c in GameServer.Sessions)
            {
                ClientSession client = ((ClientSession)(c.Value));
                if (client.ClientToken == Token)
                {
                    return client;
                }
            }
            return null;
        }

        /// <summary>
        /// this function checks if someone is already logged in from that user
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public static bool SomeoneAlreadyConnected(string username)
        {
            foreach (DictionaryEntry c in GameServer.Sessions)
            {
                ClientSession client = (ClientSession)(c.Value);
                if (client._ClientNick == username)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
