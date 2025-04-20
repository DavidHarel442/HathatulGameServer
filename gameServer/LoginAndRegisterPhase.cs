using HathatulServer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace HathatulServer
{
    internal class LoginAndRegisterPhase
    {// this class is incharge of the login/register phase
        /// <summary>
        /// this property 'Alphabet' contains the letters of the alphabet. camel case and upper case
        /// </summary>
        private const string Alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        /// <summary>
        /// this property is a static property which is a random used to create the token
        /// </summary>
        private static readonly Random Random = new Random();
        /// <summary>
        /// this property 'firstLoggedIn' is a static property and if someone logged in first then the property turns false and the player is the player who starts first
        /// </summary>
        public static bool firstLoggedIn = true;
        /// <summary>
        /// this property 'forgotPasswordToken' is a token generated with 6 digits.
        /// and the user recieves the token to his gmail and he has to put it in and then after verification he can change his password
        /// </summary>
        public string forgotPasswordToken ;
        /// <summary>
        /// this property 'C' is an object which with it you communicate with the Sql Database
        /// </summary>
        public SqlConnection C = new SqlConnection();
        /// <summary>
        /// this property allows the server to communicate with its clients.
        /// </summary>
        public CommunicationProtocol communicationProtocol = null;

        /// <summary>
        /// constructor. receives a communication protocol object and equals it to the property that is null
        /// </summary>
        /// <param name="communicationProtocol"></param>
        public LoginAndRegisterPhase(CommunicationProtocol communicationProtocol)
        {
            this.communicationProtocol = communicationProtocol;
            this.forgotPasswordToken = GenerateToken(6);
        }
        /// <summary>
        /// this function recieves all the information about a client and registering it in the database
        /// </summary>
        /// <param name="allTheInfo"></param>
        /// <param name="Manager"></param>
        public void Register(string allTheInfo, GameManager Manager)
        {
            string[] info = allTheInfo.Split(',');
            if (C.IsExist(info[0]))
            {
                foreach (DictionaryEntry c in GameServer.Sessions)
                {
                    ClientSession client = ((ClientSession)(c.Value));
                    if (info[0] == client._ClientNick)
                    {
                        string message = communicationProtocol.TransferToProtocol("the username already exists", "");
                        ((ClientSession)(c.Value)).SendMessage(message);
                        GameServer.Sessions.Remove(c.Key);
                    }
                }
            }
            else
            {
                string EncryptedPassword = EncryptPassword(info[1]);
                C.InsertNewUser(info[0], EncryptedPassword, info[2], info[3], info[4], info[5], info[6]);
                foreach (DictionaryEntry c in GameServer.Sessions)
                {
                    ClientSession client = ((ClientSession)(c.Value));
                    if (info[0] == client._ClientNick)
                    {
                        if (Manager.gameStarted)
                        {
                            string message = communicationProtocol.TransferToProtocol("registedButGameStarted", "");
                            ((ClientSession)(c.Value)).SendMessage(message);
                            GameServer.Sessions.Remove(c.Key);
                        }
                        else if (Manager.PlayerList.Count == 4)
                        {
                            communicationProtocol = new CommunicationProtocol();
                            string message = communicationProtocol.TransferToProtocol("MaxPlayerReached ", "");
                            ((ClientSession)(c.Value)).SendMessage(message);
                            GameServer.Sessions.Remove(c.Key);
                        }
                        else
                        {
                            string token = GenerateToken(6);
                            C.InsertToken(info[0], token);
                            client.ClientToken = token;
                            Manager.PlayerList.AddLast(new Player(info[0], new LinkedList<Card>(), false));
                            Player.sitForAll++;
                            if (firstLoggedIn)
                            {
                                Manager.PlayerList.ElementAt(Manager.PlayerList.Count - 1).GetIsMyturn = true;
                                firstLoggedIn = false;
                            }
                            string data = info[2] + "|" + token + "|" + info[0] + "|" + GameServer.Manager.PlayerList.Count.ToString();
                            string message = communicationProtocol.TransferToProtocol("you have registered successfully", data);
                            ((ClientSession)(c.Value)).SendMessage(message);
                            foreach (DictionaryEntry s in GameServer.Sessions)
                            {
                                ClientSession client1 = ((ClientSession)(s.Value));
                                if (client1._ClientNick != info[0])
                                {
                                    message = communicationProtocol.TransferToProtocol("updateconnected", GameServer.Manager.PlayerList.Count.ToString());
                                    client1.SendMessage(message);
                                }
                            }

                        }

                    }
                }
            }

        }

        /// <summary>
        /// this function checks if the recieved name and password exist in the database. and if it does it sends an approval and the first name of the user
        /// </summary>
        /// <param name="name"></param>
        /// <param name="password"></param>
        /// <param name="Manager"></param>
        public void CheckLogin(string name, string password, GameManager Manager)
        {
            string EncryptedPassword = EncryptPassword(password);
            bool LoginCorrect = C.LoginCheck(name, EncryptedPassword);
            string FirstName = C.GetFirstName(name);

            if (!LoginCorrect)
            {
                foreach (DictionaryEntry c in GameServer.Sessions)
                {
                    ClientSession client = ((ClientSession)(c.Value));
                    if (name == client._ClientNick)
                    {
                        string message = communicationProtocol.TransferToProtocol("NotLogged ", "");
                        ((ClientSession)(c.Value)).SendMessage(message);
                        GameServer.Sessions.Remove(c.Key);
                    }

                }

            }
            else
            {
                foreach (DictionaryEntry c in GameServer.Sessions)
                {
                    ClientSession client = ((ClientSession)(c.Value));
                    if (name == client._ClientNick)
                    {
                        if (Manager.gameStarted)
                        {
                            string message = communicationProtocol.TransferToProtocol("loggedButGameStarted ", "");
                            ((ClientSession)(c.Value)).SendMessage(message);
                            GameServer.Sessions.Remove(c.Key);
                        }
                        else if (Manager.PlayerList.Count == 4)
                        {
                            string message = communicationProtocol.TransferToProtocol("MaxPlayerReached ", "");
                            ((ClientSession)(c.Value)).SendMessage(message);
                            GameServer.Sessions.Remove(c.Key);
                        }
                        else if (Manager.PlayerList.Count != 4)
                        {

                            string token = GenerateToken(6);
                            client.ClientToken = token;
                            C.InsertToken(name, token);
                            if (firstLoggedIn)
                            {

                                Manager.PlayerFirst(name);
                                firstLoggedIn = false;
                            }
                            else
                            {

                                Manager.PlayerNotFirst(name);
                            }
                            string data = FirstName + "|" + token + "|" + name + "|" + GameServer.Manager.PlayerList.Count.ToString();
                            string message = communicationProtocol.TransferToProtocol("LoggedIn ", data);
                            ((ClientSession)(c.Value)).SendMessage(message);
                            foreach (DictionaryEntry s in GameServer.Sessions)
                            {
                                ClientSession client1 = ((ClientSession)(s.Value));
                                if (client1._ClientNick != name)
                                {
                                    message = communicationProtocol.TransferToProtocol("updateconnected", GameServer.Manager.PlayerList.Count.ToString());
                                    client1.SendMessage(message);
                                }
                            }
                        }
                    }
                }
                


            }

        }




        /// <summary>
        /// sends the mail to the user who asked to change the password. it sends him code which he needs to put to validate that it is in fact him
        /// </summary>
        /// <param name="mailTo"></param>
        public void SendForgotPassword(string mailTo)
        {
            ForgotPassword obj = new ForgotPassword();
            obj.SendForgotPasswordMail(mailTo, forgotPasswordToken);
        }

        /// <summary>
        /// this function generates a unique token that the client and server will communicate with
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public string GenerateToken(int length)
        {
            return GenerateToken(Alphabet, length);
        }

        /// <summary>
        /// this function is called for in the function 'GenerateToken'. it generate the Unique token. 6 digits and contains all the letters in the Alphabet.
        /// </summary>
        /// <param name="characters"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public string GenerateToken(string characters, int length)
        {
            return new string(Enumerable.Range(0, length).Select(num => characters[Random.Next() % characters.Length]).ToArray());
        }

        /// <summary>
        /// this function recieves a password and encrypts it using the 'SHA1' encryption
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public string EncryptPassword(string password)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(password);
            byte[] inArray = HashAlgorithm.Create("SHA1").ComputeHash(bytes);
            return Convert.ToBase64String(inArray);
        }

    }
}
