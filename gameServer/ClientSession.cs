using gameServer;
using HathatulServer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HathatulServer
{
    internal class ClientSession
    {// this class is in charge of managing each client's session


        // this class contains details and info about each client
        /// <summary>
        /// this property '_client' is an object which represents the client tcp
        /// </summary>
        public TcpClient _client;
        /// <summary>
        /// this property 'SpecialSituationDrawTwo' contains a value which represents if its in the special situation where the drawn card is draw2 for the client in question.
        /// </summary>
        public bool SpecialSituationDrawTwo = false;
        /// <summary>
        /// this property 'clientIP' contains the ipAddress of the client
        /// </summary>
        private string clientIP;
        /// <summary>
        /// this property '_ClientNick' contains the name of the client
        /// </summary>
        public string _ClientNick;
        /// <summary>
        /// this property 'ClientToken' contains the unique token of the client
        /// </summary>
        public string ClientToken;
        // used for sending and reciving data
        public byte[] data;
        // the nickname being sent
        public bool ReceiveNick = true;
        /// <summary>
        /// this property allows the server to communicate with the client
        /// </summary>
        public CommunicationProtocol communicationProtocol = null;
        /// <summary>
        /// this property 'LoginAndRegister' is incharge of managing the said ClientSession object with its Login/Register phase.
        /// </summary>
        public LoginAndRegisterPhase loginAndRegister = null;
        /// <summary>
        /// getters and setters for the clientIP
        /// </summary>
        public string GetClientIP { get => clientIP; set => clientIP = value; }


        /// <summary>
        /// When the client gets connected to the server the server will create an instance of the ClientSession and pass the TcpClient
        /// </summary>
        /// <param name="client"></param>
        public ClientSession(TcpClient client,CommunicationProtocol communicationProtocol)
        {
            _client = client;
            this.communicationProtocol = communicationProtocol;
            loginAndRegister = new LoginAndRegisterPhase(communicationProtocol);
            // get the ip address of the client to register him with our client list
            GetClientIP = client.Client.RemoteEndPoint.ToString();
            // Add the new client to our clients collection

            // Read data from the client async
            data = new byte[_client.ReceiveBufferSize];

            // BeginRead will begin async read from the NetworkStream
            // This allows the server to remain responsive and continue accepting new connections from other clients
            // When reading complete control will be transfered to the ReviveMessage() function.
            _client.GetStream().BeginRead(data,
                                          0,
                                          System.Convert.ToInt32(_client.ReceiveBufferSize),
                                          ReceiveMessage,
                                          null);
        }


        /// <summary>
        /// allow the server to send message to the client.
        /// </summary>
        /// <param name="message"></param>
        public void SendMessage(string message)
        {
            try
            {
                System.Net.Sockets.NetworkStream ns;

                // use lock to present multiple threads from using the networkstream object
                // this is likely to occur when the server is connected to multiple clients all of 
                // them trying to access to the networkstram at the same time.
                lock (_client.GetStream())
                {
                    ns = _client.GetStream();
                }

                // Send data to the client
                byte[] bytesToSend = System.Text.Encoding.ASCII.GetBytes(message);
                ns.Write(bytesToSend, 0, bytesToSend.Length);
                ns.Flush();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// reciev and handel incomming streem 
        /// Asynchrom
        /// </summary>
        /// <param name="ar">IAsyncResult Interface</param>
        public void ReceiveMessage(IAsyncResult ar)
        {
            IPAddress ip = IPAddress.Parse(((IPEndPoint)_client.Client.RemoteEndPoint).Address.ToString());
            if (!GameServer.IPDosManager.ShouldAllowToContinueSession(ip))
            {
                ClientSession client = GameServer.IPDosManager.GetClientByIP(clientIP);
                client.SendMessage("Blocked" + ",");
                GameServer.RemoveClientSession(client.clientIP);
            }
            else
            {
                int bytesRead;
                try
                {
                    lock (_client.GetStream())
                    {
                        // call EndRead to handle the end of an async read.
                        bytesRead = _client.GetStream().EndRead(ar);
                    }
                    // if bytesread<1 -> the client disconnected
                    if (bytesRead < 1)
                    {
                        // remove the client from out list of clients
                        GameServer.RemoveClientSession(GetClientIP);
                        return;
                    }
                    else // client still connected
                    {
                        string messageReceived = System.Text.Encoding.ASCII.GetString(data, 0, bytesRead);
                        // if the client is sending its nickname
                        if (ReceiveNick)
                        {


                            if (GameServer.SomeoneAlreadyConnected(messageReceived))
                            {
                                _ClientNick = "ClientToRemove1223452008";
                                ClientSession client = GameServer.Manager.GetClientByName("ClientToRemove1223452008");
                                client.SendMessage("SomeoneAlreadyConnected");
                                GameServer.Sessions.Remove(client.GetClientIP);
                            }
                            else
                            {
                                _ClientNick = messageReceived;
                                ReceiveNick = false;
                            }
                        }
                        else
                        {
                            HathatulMessage messageRecievedProt = communicationProtocol.TransferFromProtocol(messageReceived);
                            if (messageRecievedProt.command.Contains("register:"))
                            {
                                string[] s = messageRecievedProt.arguments.Split('|');
                                string info = s[0] + "," + s[1] + "," + s[2] + "," + s[3] + "," + s[4] + "," + s[5] + "," + s[6];
                                loginAndRegister.Register(info, GameServer.Manager);//inserts all the information of the client into the Sql dataBase as a result of registering
                            }
                            else if (messageRecievedProt.command.Contains("checkLogin"))
                            {
                                string[] s = messageRecievedProt.arguments.Split('|');
                                ClientSession client = GameServer.Manager.GetClientByName(s[0]);
                                string username = s[0];
                                string password = s[1];
                                loginAndRegister.CheckLogin(username, password, GameServer.Manager);//checks if the username and password are correct


                            }
                            else if (messageRecievedProt.command.Contains("SendForgotPassword"))
                            {
                                ClientSession client = GameServer.Manager.GetClientByName(messageRecievedProt.arguments);
                                if (client.loginAndRegister.C.IsExist(messageRecievedProt.arguments))
                                {
                                    string email = client.loginAndRegister.C.GetEmailFromUsername(messageRecievedProt.arguments);
                                    client.loginAndRegister.SendForgotPassword(email);//sends a mail to the client who asked for it.(forgot password)
                                }
                                else
                                {
                                    string message = communicationProtocol.TransferToProtocol("UsernameDoesntExist", "");
                                    client.SendMessage(message);// sends a feedback saying his username doesnt exist
                                }
                            }
                            else if (messageRecievedProt.command.Contains("ValidateCode"))
                            {
                                string[] info = messageRecievedProt.arguments.Split('|');
                                ClientSession client = GameServer.Manager.GetClientByName(info[0]);
                                if (client.loginAndRegister.forgotPasswordToken == info[1])
                                {
                                    string message = communicationProtocol.TransferToProtocol("CodeValidated", "");
                                    client.SendMessage(message);//sends a feedback that his verification code is correct
                                }
                                else
                                {
                                    string message = communicationProtocol.TransferToProtocol("WrongCode", "");
                                    client.SendMessage(message);//sends a feedback that his verification code was wrong
                                }
                            }
                            else if (messageRecievedProt.command.Contains("ChangePassword"))
                            {
                                string[] info = messageRecievedProt.arguments.Split('|');
                                ClientSession client = GameServer.Manager.GetClientByName(info[0]);
                                string password = loginAndRegister.EncryptPassword(info[1]);
                                client.loginAndRegister.C.InsertNewPassword(info[0], password);
                                string message = communicationProtocol.TransferToProtocol("PasswordChanged", "");
                                client.SendMessage(message);//sends a feedback that his password was changed
                                GameServer.RemoveClientSession(client.GetClientIP);
                            }
                            else if (messageRecievedProt.username == "DoesNotExist")
                            {
                                SendMessage("Imposter");// if the username is not correct it sends a message to the certain user to disconnect.
                            }
                            else if (messageRecievedProt.command.Contains("startYourEngines"))
                            {
                                GameServer.Manager.StartGame();//starts the game

                            }
                            else if (messageRecievedProt.command.Contains("TurnToFalseSecondCard"))
                            {
                                string arguements = messageRecievedProt.username + "|";
                                string message = communicationProtocol.TransferToProtocol("SecondCardToFalse", arguements);
                                Broadcast(message);//after special drawTwo card if someone threw their second card/used it you send to all clients a message to turn visible to false. (meaning he already used the card)
                            }
                            else if (messageRecievedProt.command.Contains("TurnToFalseFirstCard"))
                            {
                                string arguements = messageRecievedProt.username + "|";
                                string message = communicationProtocol.TransferToProtocol("FirstCardToFalse", arguements);
                                Broadcast(message);//after special drawTwo card if someone threw their first card/used it you send to all clients a message to turn visible to false. (meaning he already used the card)
                            }
                            else if (messageRecievedProt.command.Contains("throwCardToTrash"))
                            {
                                string[] s = messageRecievedProt.arguments.Split('|');
                                if (s[2].Length > 1)
                                {
                                    GameServer.Manager.MoveTheTurn(messageRecievedProt.username);//moves the turn
                                }
                                ClientSession client = GameServer.Manager.GetClientByName(messageRecievedProt.username);
                                if (!client.SpecialSituationDrawTwo)
                                {
                                    string data = s[0] + "|" + s[1] + "|" + messageRecievedProt.username;
                                    string message = communicationProtocol.TransferToProtocol("TheCardInTheTrashCardIS", data);
                                    Broadcast(message);//sends to all clients which card is in the trash
                                }
                                else
                                {
                                    string data = s[0] + "|" + s[1];
                                    string message = communicationProtocol.TransferToProtocol("TheCardInTheTrashCardIS", data);
                                    Broadcast(message);//sends to all clients which card is in the trash
                                }
                            }
                            else if (messageRecievedProt.command.Contains("DealMeCards"))
                            {
                                GameServer.Manager.DealCards(messageRecievedProt.username);//deals the card to the player
                            }
                            else if (messageRecievedProt.command.Contains("showMeLeftCard"))
                            {
                                if (messageRecievedProt.command.Contains("showMeLeftCardFirstTime"))
                                {
                                    GameServer.Manager.ShowSpecifiedCard(messageRecievedProt.username, "Left");// sendw the left card to the player
                                }
                                else
                                {
                                    GameServer.Manager.ShowSpecifiedCard(messageRecievedProt.username, "Left");// sends the left card to the player
                                }
                            }
                            else if (messageRecievedProt.command.Contains("showMeRightCard"))
                            {
                                if (messageRecievedProt.command.Contains("showMeRightCardFirstTime"))
                                {
                                    GameServer.Manager.ShowSpecifiedCard(messageRecievedProt.username, "Right");//sends the right card to the player
                                }
                                else
                                {
                                    GameServer.Manager.ShowSpecifiedCard(messageRecievedProt.username, "Right");//sends the right card to the player
                                }
                            }
                            else if (messageRecievedProt.command.Contains("drawCardFromStack"))
                            {
                                if (GameServer.Manager.IsItYourTurn(messageRecievedProt.username))
                                {
                                    string s = GameServer.Manager.DrawCardFromStack(messageRecievedProt.username,true);//draws a card from the stackofcards
                                    if (s == "endGame")
                                    {
                                        string arguments = GameServer.Manager.WhoIsTheWinner() + "|";
                                        string message = communicationProtocol.TransferToProtocol("GameEndedCardsEnded", arguments);
                                        Broadcast(message);//sends a message to all clients indicating the game ended because the stackofcards is empty. and it shows theh winner
                                        string SecondMessage = communicationProtocol.TransferToProtocol("AllOfTheCards", GameServer.Manager.ReturnAllOfCards());
                                        Broadcast(SecondMessage);// sends all of the cards to all of the players
                                    }
                                    else
                                    {
                                        ClientSession client = GameServer.Manager.GetClientByName(messageRecievedProt.username);//gets the object of the client from the username
                                        if (!client.SpecialSituationDrawTwo)
                                        {
                                            string message = communicationProtocol.TransferToProtocol("showDrawnCard", messageRecievedProt.username);
                                            Broadcast(message);//shows the back of the drawncard of a player to all the players
                                        }
                                    }
                                }

                            }
                            else if (messageRecievedProt.command.Contains("switchLeft"))
                            {
                                string[] s = messageRecievedProt.arguments.Split('|');
                                if (s[2].Length>1)
                                {
                                    GameServer.Manager.MoveTheTurn(messageRecievedProt.username);//moves the turn
                                }
                                ClientSession client = GameServer.Manager.GetClientByName(messageRecievedProt.username);
                                if (!client.SpecialSituationDrawTwo)
                                {
                                    string message = communicationProtocol.TransferToProtocol("turnDrawnCardToFalse", messageRecievedProt.username);
                                    Broadcast(message + "|");//sending a message to all the clients to turn false the drawn card someone took from stackOfCards
                                }
                                GameServer.Manager.ReplaceTheCardOfThePLayerCard(new Card(s[0], Card.ReturnCardValue(s[0])), "Left", messageRecievedProt.username);//replaces a card of the player with another depending on what the client requested

                            }
                            else if (messageRecievedProt.command.Contains("switchMiddleLeft"))
                            {
                                string[] s = messageRecievedProt.arguments.Split('|');
                                if (s[2].Length > 1)
                                {
                                    GameServer.Manager.MoveTheTurn(messageRecievedProt.username);//moves the turn
                                }
                                GameServer.Manager.ReplaceTheCardOfThePLayerCard(new Card(s[0], Card.ReturnCardValue(s[0])), "MiddleLeft", messageRecievedProt.username);//replaces a card of the player with another depending on what the client requested
                                ClientSession client = GameServer.Manager.GetClientByName(messageRecievedProt.username);
                                if (!client.SpecialSituationDrawTwo)
                                {
                                    string message = communicationProtocol.TransferToProtocol("turnDrawnCardToFalse", messageRecievedProt.username);
                                    Broadcast(message + "|");//sending a message to all the clients to turn false the drawn card someone took from stackOfCards
                                }
                                
                            }
                            else if (messageRecievedProt.command.Contains("switchMiddleRight"))
                            {
                                string[] s = messageRecievedProt.arguments.Split('|');
                                if (s[2].Length > 1)
                                {
                                    GameServer.Manager.MoveTheTurn(messageRecievedProt.username);//moves the turn
                                }
                                GameServer.Manager.ReplaceTheCardOfThePLayerCard(new Card(s[0], Card.ReturnCardValue(s[0])), "MiddleRight", messageRecievedProt.username);//replaces a card of the player with another depending on what the client requested
                                ClientSession client = GameServer.Manager.GetClientByName(messageRecievedProt.username);
                                if (!client.SpecialSituationDrawTwo)
                                {
                                    string message = communicationProtocol.TransferToProtocol("turnDrawnCardToFalse", messageRecievedProt.username);
                                    Broadcast(message + "|");//sending a message to all the clients to turn false the drawn card someone took from stackOfCards
                                }
                            }
                            else if (messageRecievedProt.command.Contains("switchRight"))
                            {
                                string[] s = messageRecievedProt.arguments.Split('|');
                                if (s[2].Length > 1)
                                {
                                    GameServer.Manager.MoveTheTurn(messageRecievedProt.username);//moves the turn
                                }
                                GameServer.Manager.ReplaceTheCardOfThePLayerCard(new Card(s[0], Card.ReturnCardValue(s[0])), "Right", messageRecievedProt.username);//replaces a card of the player with another depending on what the client requested
                                ClientSession client = GameServer.Manager.GetClientByName(messageRecievedProt.username);
                                if (!client.SpecialSituationDrawTwo)
                                {
                                    string message = communicationProtocol.TransferToProtocol("turnDrawnCardToFalse", messageRecievedProt.username);
                                    Broadcast(message + "|");//sending a message to all the clients to turn false the drawn card someone took from stackOfCards
                                }
                            }
                            else if (messageRecievedProt.command.Contains("SwitchFromTrashWithMyLeft"))
                            {
                                string[] s = messageRecievedProt.arguments.Split('|');
                                if (GameServer.Manager.IsItYourTurn(messageRecievedProt.username))//checks if it is the players turn
                                {
                                    GameServer.Manager.ReplaceTheCardOfThePLayerCard((new Card(s[0], Card.ReturnCardValue(s[0]))), "Left", messageRecievedProt.username);//switch the card from the TrashCards with one of a players card depending on who asked for it
                                                                                                                                                                   //and which card he asked for
                                }
                            }
                            else if (messageRecievedProt.command.Contains("SwitchFromTrashWithMyMiddleLeft"))
                            {
                               string[] s = messageRecievedProt.arguments.Split('|');
                                if (GameServer.Manager.IsItYourTurn(messageRecievedProt.username))//checks if it is the players turn
                                {   

                                    GameServer.Manager.ReplaceTheCardOfThePLayerCard((new Card(s[0], Card.ReturnCardValue(s[0]))), "MiddleLeft", messageRecievedProt.username);//switch the card from the TrashCards with one of a players card depending on who asked for it
                                                                                                                                                                         //and which card he asked for
                                }
                            }
                            else if (messageRecievedProt.command.Contains("SwitchFromTrashWithMyMiddleRight"))
                            {
                                string[] s = messageRecievedProt.arguments.Split('|');
                                if (GameServer.Manager.IsItYourTurn(messageRecievedProt.username))//checks if it is the players turn
                                {
                                    GameServer.Manager.ReplaceTheCardOfThePLayerCard((new Card(s[0], Card.ReturnCardValue(s[0]))), "MiddleRight", messageRecievedProt.username);//switch the card from the TrashCards with one of a players card depending on who asked for it
                                                                                                                                                                          //and which card he asked for

                                }
                            }
                            else if (messageRecievedProt.command.Contains("SwitchFromTrashWithMyRight"))
                            {
                                string[] s = messageRecievedProt.arguments.Split('|');

                                if (GameServer.Manager.IsItYourTurn(messageRecievedProt.username))//checks if it is the players turn
                                {
                                    GameServer.Manager.ReplaceTheCardOfThePLayerCard((new Card(s[0], Card.ReturnCardValue(s[0]))), "Right", messageRecievedProt.username);//switch the card from the TrashCards with one of a players card depending on who asked for it
                                                                                                                                                                    //and which card he asked for

                                }
                            }
                            else if (messageRecievedProt.command.Contains("showMeMiddleLeftCard"))
                            {
                                GameServer.Manager.ShowSpecifiedCard(messageRecievedProt.username, "MiddleLeft");//send the MiddleLeft card to the player who asked for it
                            }
                            else if (messageRecievedProt.command.Contains("showMeMiddleRightCard"))
                            {
                                GameServer.Manager.ShowSpecifiedCard(messageRecievedProt.username, "MiddleRight");//send the MiddleRight card to the player who asked for it
                            }
                            else if (messageRecievedProt.command.Contains("specialCardDrawTwo"))
                            {
                                string s = GameServer.Manager.SpecialCardDrawTwo(messageRecievedProt.username);//draws two cards from stack because of the special card 'DrawTwo'
                                if (s == "endGame")
                                {
                                    string arguments = GameServer.Manager.WhoIsTheWinner() + "|";
                                    string message = communicationProtocol.TransferToProtocol("GameEndedCardsEnded", arguments);
                                    Broadcast(message);// send a message to all clients that the game ended because the stackOfCards has no more cards left. also sends the winner
                                    string SecondMessage = communicationProtocol.TransferToProtocol("AllOfTheCards", GameServer.Manager.ReturnAllOfCards());
                                    Broadcast(SecondMessage);// sends a message to all clients containing the cards of all players
                                }
                            }
                            else if (messageRecievedProt.command.Contains("SwitchCard"))
                            {
                                string[] s = messageRecievedProt.arguments.Split('|');
                                GameServer.Manager.SpecialCardSwap(messageRecievedProt.username, s[0], s[1], s[2]);// handling special situation of the 'Swap' special card
                            }
                            else if (messageRecievedProt.command.Contains("Hathatul"))
                            {
                                GameServer.Manager.lastTurn = true;
                                string message = communicationProtocol.TransferToProtocol("lastTurn", messageRecievedProt.username);
                                Broadcast(message);//sends a message to everyone that it is their last turn and the game is about to end
                            }
                            else if (messageRecievedProt.command.Contains("turnTheturnToAnotherPlayer"))
                            {
                                ClientSession client = GameServer.Manager.GetClientByName(messageRecievedProt.username);
                                if (client.SpecialSituationDrawTwo)
                                {
                                    client.SpecialSituationDrawTwo = false;
                                }
                                Thread thread = new Thread(() => { GameServer.Manager.MoveTheTurn(messageRecievedProt.username); });// moves the turn
                                thread.Start();
                            }
                            else if (messageRecievedProt.command.Contains("AFK"))
                            {
                                string drawnCard = GameServer.Manager.DrawCardFromStack(messageRecievedProt.username,false);
                                string message = communicationProtocol.TransferToProtocol("ThrowToTrash", drawnCard);
                                Broadcast(message);//if someone was afk and didnt play his turn for 30 sec
                                GameServer.Manager.MoveTheTurn(messageRecievedProt.username);// moves the turn
                                    
                                
                            }

                        }
                    }
                    lock (_client.GetStream())
                    {
                        // continue reading form the client
                        _client.GetStream().BeginRead(data, 0, System.Convert.ToInt32(_client.ReceiveBufferSize), ReceiveMessage, null);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    GameServer.Sessions.Remove(GetClientIP);

                }
            }

        }
    
        /// <summary>
        /// send message to all the clients that are stored in the allclients hashtable
        /// </summary>
        /// <param name="message"></param>
        public static void Broadcast(string message)
        {
            foreach (DictionaryEntry c in GameServer.Sessions)
            {

                ((ClientSession)(c.Value)).SendMessage(message);

            }

        }
        
        
    }
}

