using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace HathatulServer
{
    internal class GameManager
    {// this classes goal is to manage the game. and all of the players.

        /// <summary>
        /// this property 'playerList' contains a list of all the players connected to the game
        /// </summary>
        LinkedList<Player> playerList;
        /// <summary>
        /// this property 'board' contains an object which linkes the GameManager to the board object. 
        /// </summary>
        GameBoard board;
        /// <summary>
        /// this property 'gameStarted' is a static property.
        /// after the game started the boolean is turned into true and if someone loges in it doesnt let him join the game
        /// </summary>
        public bool gameStarted = false;
        /// <summary>
        /// this property 'communicatin' is a property which helps you transfer to the communication protocol
        /// </summary>
        public CommunicationProtocol communicationProtocol = null;
        /// <summary>
        /// this property is incharge of calling when the game will end
        /// </summary>
        public bool lastTurn = false;
        /// <summary>
        /// default constructor. creates the list of players and the board.
        /// </summary>
        public GameManager(CommunicationProtocol communicationProtocol)
        {
            this.communicationProtocol = communicationProtocol;
            board = new GameBoard();
            PlayerList = new LinkedList<Player>();
        }
        /// <summary>
        /// getters and setters of the linkedList of the players.
        /// </summary>
        internal LinkedList<Player> PlayerList { get => playerList; set => playerList = value; }

        /// <summary>
        /// get the object of the certain player. by its name
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public Player GetPlayerByName(string username)
        {
            foreach (Player p in this.playerList)
            {
                if (p.GetName == username)
                {
                    return p;
                }
            }
            return null; // Return null if player with given username is not found
        }

        /// <summary>
        /// gets the object of the client by its name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ClientSession GetClientByName(string name)
        {
            foreach (DictionaryEntry c in GameServer.Sessions)
            {
                ClientSession client = ((ClientSession)(c.Value));
                if (client._ClientNick == name)
                {
                    return client;
                }
            }
            return null;
        }

        /// <summary>
        /// this function starts the game. it sends each player the names of all players and where they should be sitted on his board.
        /// </summary>
        public void StartGame()
        {
            gameStarted = true;
            string playersNameAndItsSit = "";
            for (int i = 0; i < playerList.Count; i++)
            {
                playersNameAndItsSit = playersNameAndItsSit + playerList.ElementAt(i).GetSit + "|" + playerList.ElementAt(i).GetName + "|";
            }
            foreach (DictionaryEntry c in GameServer.Sessions)
            {
                ClientSession client = ((ClientSession)(c.Value));
                SqlConnection s = new SqlConnection();
                Player p = GetPlayerByName(client._ClientNick);
                string firstName = s.GetFirstName(client._ClientNick);
                string data = firstName + "|" + p.GetSit + "|" + playerList.Count;
                string message = communicationProtocol.TransferToProtocol("startTheGame", data);// send a message for all players to start the game
                client.SendMessage(message);
                
            }
            foreach (DictionaryEntry c in GameServer.Sessions)
            {
                ClientSession client = ((ClientSession)(c.Value));
                string message = communicationProtocol.TransferToProtocol("playerAndItsSit", playersNameAndItsSit);// send a message to all players of the sit and name of all players
                client.SendMessage(message);
            }
        }

        /// <summary>
        /// this function gives four cards to the player who asked for them
        /// </summary>
        /// <param name="username"></param>
        public void DealCards(string username)
        {
            ClientSession client = GetClientByName(username);
            GetPlayerByName(client._ClientNick).GetCards = board.DealCardsPlayer();
        }


        /// <summary>
        /// this function returns the Left/Right/MiddleLeft/MiddleRight card of the player. depends who asked for it
        /// </summary>
        /// <param name="name"></param>
        /// <param name="whichCard"></param>
        public void ShowSpecifiedCard(string name, string whichCard)
        {
            ClientSession client = GetClientByName(name);
            if (whichCard.Equals("Left"))
            {
                string message = communicationProtocol.TransferToProtocol("yourLeftCardIs", board.ShowRequestedCard(GetPlayerByName(client._ClientNick).GetCards, "Left").ToString());
                client.SendMessage(message);//sends a message to the client to show the left card
            }
            if (whichCard.Equals("Right"))
            {
                string message = communicationProtocol.TransferToProtocol("yourRightCardIs", board.ShowRequestedCard(GetPlayerByName(client._ClientNick).GetCards, "Right").ToString());
                client.SendMessage(message);//sends a message to the client to show the right card

            }
            if (whichCard.Equals("MiddleLeft"))
            {
                string message = communicationProtocol.TransferToProtocol("yourMiddleLeftCardIs", board.ShowRequestedCard(GetPlayerByName(client._ClientNick).GetCards, "MiddleLeft").ToString());
                client.SendMessage(message);//sends a message to the client to show the middleLeft card

            }
            if (whichCard.Equals("MiddleRight"))
            {
                string message = communicationProtocol.TransferToProtocol("yourMiddleRightCardIs", board.ShowRequestedCard(GetPlayerByName(client._ClientNick).GetCards, "Right").ToString());
                client.SendMessage(message);//sends a message to the client to show the middleRight card

            }
        }


        /// <summary>
        /// this function checks if it is your turn
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsItYourTurn(string name)
        {
            ClientSession client = GetClientByName(name);
            if (GetPlayerByName(client._ClientNick).GetIsMyturn)
            {
                return true;
            }
            else if (client._ClientNick == name)
            {
                string message = communicationProtocol.TransferToProtocol("ITSNOTYOURTURN", "");
                client.SendMessage(message);//send a message to the client to indicate that it is not his turn
            }
            return false;
        }

        /// <summary>
        /// this function sends a card from the stackOfCards to the one who asked it
        /// </summary>
        /// <param name="name"></param>
        /// <param name="sendTheMessage"></param>
        /// <returns></returns>
        public string DrawCardFromStack(string name,bool sendTheMessage)
        {
            ClientSession client = GetClientByName(name);
            Card c = board.DrawCardFromStack();
            if(c == null)
            {
                return "endGame";
            }
            if (sendTheMessage)
            {
                string message = communicationProtocol.TransferToProtocol("cardFromStackIs", c.GetName);
                client.SendMessage(message);// sends a message to the client with the card from the stack of cards
            }
            return c.GetName;
        }

        /// <summary>
        /// this function moves the turn to the next player. managed by its sit. and it send the player which is his turn a notification.
        /// and all the other player, that its his turn
        /// </summary>
        /// <param name="name"></param>
        public void MoveTheTurn(string name)
        {

            string arguments;
            string message;
            LinkedListNode<Player> currentNode = playerList.Find(playerList.FirstOrDefault(p => p.GetName == name));
            if (currentNode == null)
                return; // Player not found, consider handling this case

            currentNode.Value.GetIsMyturn = false;
            
            var currentPlayerClient = GetClientByName(currentNode.Value.GetName);
            if (this.lastTurn)
            {
                currentNode.Value.lastTurn = true;
                currentPlayerClient.SendMessage("LastTurn!!");

            }
            // Find the next player
            LinkedListNode<Player> nextNode = currentNode.Next ?? playerList.First; 
            
            // Start the next player's turn
            nextNode.Value.GetIsMyturn = true;
            if (nextNode.Value.lastTurn)
            {
                arguments = GameServer.Manager.WhoIsTheWinner() + "|";
                message = communicationProtocol.TransferToProtocol("GameEnded", arguments);
                ClientSession.Broadcast(message);// sends a message saying the gameEnded with the winner of the game
                arguments = GameServer.Manager.ReturnAllOfCards();
                string SecondMessage = communicationProtocol.TransferToProtocol("AllOfTheCards", arguments);
                ClientSession.Broadcast(SecondMessage);// sends a message to all clients with all of the cards
            }
            var nextPlayerClient = GetClientByName(nextNode.Value.GetName);
            arguments = " |" + nextPlayerClient._ClientNick + "|  |";
            
            message = communicationProtocol.TransferToProtocol("yourTurn", nextPlayerClient._ClientNick);
            nextPlayerClient.SendMessage(message);// send a message to the client which is his turn to tell him that it is his turn
            string message1 = communicationProtocol.TransferToProtocol("hisTurn", arguments);
            foreach (DictionaryEntry c in GameServer.Sessions)
            {
                ClientSession client = ((ClientSession)(c.Value));
                if (client._ClientNick != nextPlayerClient._ClientNick)
                {
                    client.SendMessage(message1);// send a message to everyone to tell them who has the turn now 
                }
            }
            

        }

        /// <summary>
        /// this function switchs the player card with the card he wanted where he wanted it
        /// </summary>
        /// <param name="cardName"></param>
        /// <param name="whichCard"></param>
        /// <param name="name"></param>
        public void ReplaceTheCardOfThePLayerCard(Card cardName, string whichCard, string name)
        {
            string arguments = board.SwitchCardWithAnotherOne(GetPlayerByName(name).GetCards, whichCard, cardName) + "|"; // the call to the function 'SwitchCardWithAnotherOne' switches a cards of a certain player with another cards
            string message = communicationProtocol.TransferToProtocol("TheCardInTheTrashCardIS",arguments);
            ClientSession.Broadcast(message);// sends a message to everyone of what card is in the trash
 
        }

       
        /// <summary>
        /// this function manages the situation if the player logged/register in first. then he starts first
        /// </summary>
        /// <param name="name"></param>
        public void PlayerFirst(string name)
        {
            this.PlayerList.AddLast(new Player(name, new LinkedList<Card>(), true));// creates a player and gives the first player to login/register his turn
        }

        /// <summary>
        /// this function manages the situation if the player didnt log in/ register first. 
        /// </summary>
        /// <param name="name"></param>
        public void PlayerNotFirst(string name)
        {
            this.PlayerList.AddLast(new Player(name, new LinkedList<Card>(), false));// creates a player which is not first
        }


        /// <summary>
        /// this function manages the situation of the special card 'Draw Two'. handle special case for drawTwo
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string SpecialCardDrawTwo(string name)
        {
            string message = "";
            ClientSession client = GetClientByName(name);
            client.SpecialSituationDrawTwo = true;
            if (name == client._ClientNick)
            {
                message = "";
                var storeTwoValues = board.SpecialCardDrawTwo();// returns two cards.
                string data = storeTwoValues.Item1 + "|" + storeTwoValues.Item2;
                if (storeTwoValues.Item2.Contains("null"))
                {
                    client.SpecialSituationDrawTwo = false;
                }
                if (storeTwoValues.Item1 == "endGame")// if there are no more cards in the stack
                {
                    return "endGame";
                }
                else
                {
                    message = communicationProtocol.TransferToProtocol("specialCardDrawTwo", data);
                    client.SendMessage(message);//send the 2 cards from stack to the certain player
                }
            }
            
            message = communicationProtocol.TransferToProtocol("ShowBothDrawnCards", name);
            ClientSession.Broadcast(message);// show all players that the certain player 2 cards drawn
            return "allGood";
        }

        /// <summary>
        /// this function manages the situation created by the specialCard swap.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="whichCardPosition"></param>
        /// <param name="otherPlayersName"></param>
        /// <param name="WhichCardOfHis"></param>
        public void SpecialCardSwap(string player, string whichCardPosition, string otherPlayersName, string WhichCardOfHis)
        {
            ClientSession client = GetClientByName(player);
            ClientSession Otherclient = GetClientByName(otherPlayersName);
            int locationForMyCard = -1;
            int locationForOtherCard = -1;
            Player reciever = GetPlayerByName(otherPlayersName);
            Player asker = GetPlayerByName(player);
            if (whichCardPosition == "Left")
            {
                locationForMyCard = 0;
            }
            if (whichCardPosition == "MiddleLeft")
            {
                locationForMyCard = 1;
            }
            if (whichCardPosition == "MiddleRight")
            {
                locationForMyCard = 2;
            }
            if (whichCardPosition == "Right")
            {
                locationForMyCard = 3;
            }
            if (WhichCardOfHis == "Left")
            {
                locationForOtherCard = 0;
            }
            if (WhichCardOfHis == "MiddleLeft")
            {
                locationForOtherCard = 1;
            }
            if (WhichCardOfHis == "MiddleRight")
            {
                locationForOtherCard = 2;
            }
            if (WhichCardOfHis == "Right")
            {
                locationForOtherCard = 3;
            }
            Card ezer = reciever.GetCards.ElementAt(locationForOtherCard);
            board.SwitchCardWithAnotherOne(reciever.GetCards, WhichCardOfHis, asker.GetCards.ElementAt(locationForMyCard));// switches card of a certain player with another
            board.SwitchCardWithAnotherOne(asker.GetCards, whichCardPosition, ezer);// switches card of a certain player with another
            string messageClient1 = communicationProtocol.TransferToProtocol("switching the cards have been a success", "");
            client.SendMessage(messageClient1);// sends the switcher a message indicationg switching was a success
            string commandLine = "your card has been switched with player: " + player + " his " + whichCardPosition + " card with your " + WhichCardOfHis + " Card ";
            string messageClient2 = communicationProtocol.TransferToProtocol(commandLine,"");
            Otherclient.SendMessage(messageClient2);// sends a message  to the person getting swapped a message indicating his card was swapped
        }

        /// <summary>
        /// this function checks who the winner (has the smallest sum of cards) is and returns it in a form of string
        /// </summary>
        public string WhoIsTheWinner()
        {
           
            for (int i = 0;i<playerList.Count;i++)
            {
                int count = 0;
                int card1 = playerList.ElementAt(i).GetCards.ElementAt(0).GetlValue;
                int card2 = playerList.ElementAt(i).GetCards.ElementAt(1).GetlValue;
                int card3 = playerList.ElementAt(i).GetCards.ElementAt(2).GetlValue;
                int card4 = playerList.ElementAt(i).GetCards.ElementAt(3).GetlValue;
                count = card1 + card2 + card3 + card4;
                playerList.ElementAt(i).GetCountForWinner = count;
            }

            
            int min = 50;
            for (int i = 0; i < playerList.Count; i++)
            {
                if (min > playerList.ElementAt(i).GetCountForWinner)
                {
                    min = playerList.ElementAt(i).GetCountForWinner;
                }
            }
            if (playerList.Count == 2)
            {
                if (playerList.ElementAt(0).GetCountForWinner == playerList.ElementAt(1).GetCountForWinner && playerList.ElementAt(0).GetCountForWinner == min)
                {
                    return ("ItsATie");
                }
            }
            if (playerList.Count == 3)
            {
                if (playerList.ElementAt(0).GetCountForWinner == playerList.ElementAt(1).GetCountForWinner && playerList.ElementAt(2).GetCountForWinner != playerList.ElementAt(0).GetCountForWinner && playerList.ElementAt(0).GetCountForWinner == min)
                {
                    return (playerList.ElementAt(0).GetName + "," + playerList.ElementAt(1).GetName);
                }
                else if (playerList.ElementAt(0).GetCountForWinner == playerList.ElementAt(2).GetCountForWinner && playerList.ElementAt(0).GetCountForWinner != playerList.ElementAt(1).GetCountForWinner && playerList.ElementAt(0).GetCountForWinner == min)
                {
                    return (playerList.ElementAt(0).GetName + "," + playerList.ElementAt(2).GetName);
                }
                else if (playerList.ElementAt(1).GetCountForWinner == playerList.ElementAt(2).GetCountForWinner && playerList.ElementAt(0).GetCountForWinner != playerList.ElementAt(2).GetCountForWinner && playerList.ElementAt(1).GetCountForWinner == min)
                {
                    return (playerList.ElementAt(1).GetName + "," + playerList.ElementAt(2).GetName);
                }
                else if ((playerList.ElementAt(1).GetCountForWinner == playerList.ElementAt(2).GetCountForWinner && playerList.ElementAt(0).GetCountForWinner == playerList.ElementAt(2).GetCountForWinner) && playerList.ElementAt(1).GetCountForWinner == min)
                {
                    return "ItsATie";
                }
            }
            if (playerList.Count == 4)
            {
                if (playerList.ElementAt(0).GetCountForWinner == playerList.ElementAt(1).GetCountForWinner && playerList.ElementAt(2).GetCountForWinner != playerList.ElementAt(0).GetCountForWinner && playerList.ElementAt(3).GetCountForWinner != playerList.ElementAt(0).GetCountForWinner && playerList.ElementAt(0).GetCountForWinner == min)
                {
                    return (playerList.ElementAt(0).GetName + "," + playerList.ElementAt(1).GetName);
                }
                else if (playerList.ElementAt(0).GetCountForWinner == playerList.ElementAt(2).GetCountForWinner && playerList.ElementAt(0).GetCountForWinner != playerList.ElementAt(1).GetCountForWinner && playerList.ElementAt(3).GetCountForWinner != playerList.ElementAt(0).GetCountForWinner && playerList.ElementAt(0).GetCountForWinner == min)
                {
                    return (playerList.ElementAt(0).GetName + "," + playerList.ElementAt(2).GetName);
                }
                else if (playerList.ElementAt(0).GetCountForWinner == playerList.ElementAt(3).GetCountForWinner && playerList.ElementAt(0).GetCountForWinner != playerList.ElementAt(2).GetCountForWinner && playerList.ElementAt(1).GetCountForWinner != playerList.ElementAt(0).GetCountForWinner && playerList.ElementAt(0).GetCountForWinner == min)
                {
                    return (playerList.ElementAt(0).GetName + "," + playerList.ElementAt(3).GetName);
                }
                else if (playerList.ElementAt(1).GetCountForWinner == playerList.ElementAt(2).GetCountForWinner && playerList.ElementAt(0).GetCountForWinner != playerList.ElementAt(2).GetCountForWinner && playerList.ElementAt(0).GetCountForWinner != playerList.ElementAt(3).GetCountForWinner && playerList.ElementAt(1).GetCountForWinner == min)
                {
                    return (playerList.ElementAt(1).GetName + "," + playerList.ElementAt(2).GetName);
                }
                else if ((playerList.ElementAt(1).GetCountForWinner == playerList.ElementAt(3).GetCountForWinner && playerList.ElementAt(0).GetCountForWinner != playerList.ElementAt(1).GetCountForWinner && playerList.ElementAt(2).GetCountForWinner != playerList.ElementAt(1).GetCountForWinner) && playerList.ElementAt(1).GetCountForWinner == min)
                {
                    return (playerList.ElementAt(1).GetName + "," + playerList.ElementAt(3).GetName);
                }
                else if ((playerList.ElementAt(2).GetCountForWinner == playerList.ElementAt(3).GetCountForWinner && playerList.ElementAt(0).GetCountForWinner != playerList.ElementAt(2).GetCountForWinner && playerList.ElementAt(2).GetCountForWinner != playerList.ElementAt(1).GetCountForWinner) && playerList.ElementAt(2).GetCountForWinner == min)
                {
                    return (playerList.ElementAt(2).GetName + "," + playerList.ElementAt(3).GetName);
                }
                else if ((playerList.ElementAt(0).GetCountForWinner == playerList.ElementAt(1).GetCountForWinner && playerList.ElementAt(0).GetCountForWinner == playerList.ElementAt(2).GetCountForWinner && playerList.ElementAt(0).GetCountForWinner == playerList.ElementAt(3).GetCountForWinner) && playerList.ElementAt(0).GetCountForWinner == min)
                {
                    return ("ItsATie");
                }
            }
            for (int i = 0; i < playerList.Count; i++)
            {
                if (min == playerList.ElementAt(i).GetCountForWinner)
                {
                    return playerList.ElementAt(i).GetName;
                }
            }
            return "Impossible";
        }

        /// <summary>
        /// this function returns as a string. all of the cards of all of the players.
        /// </summary>
        /// <returns></returns>
        public string ReturnAllOfCards()
        {
            string sit1 = playerList.ElementAt(0).GetCards.ElementAt(0).GetName + "|" + playerList.ElementAt(0).GetCards.ElementAt(1).GetName + "|" + playerList.ElementAt(0).GetCards.ElementAt(2).GetName + "|" + playerList.ElementAt(0).GetCards.ElementAt(3).GetName + "|" + ",";
            string sit2 = playerList.ElementAt(1).GetCards.ElementAt(0).GetName + "|" + playerList.ElementAt(1).GetCards.ElementAt(1).GetName + "|" + playerList.ElementAt(1).GetCards.ElementAt(2).GetName + "|" + playerList.ElementAt(1).GetCards.ElementAt(3).GetName + "|" + ",";
            if (playerList.Count==3)
            {
                string sit3 = playerList.ElementAt(2).GetCards.ElementAt(0).GetName + "|" + playerList.ElementAt(2).GetCards.ElementAt(1).GetName + "|" + playerList.ElementAt(2).GetCards.ElementAt(2).GetName + "|" + playerList.ElementAt(2).GetCards.ElementAt(3).GetName + "|,";
                return (sit1 + sit2 + sit3);
            }
            if(playerList.Count==4)
            {
                string sit3 = playerList.ElementAt(2).GetCards.ElementAt(0).GetName + "|" + playerList.ElementAt(2).GetCards.ElementAt(1).GetName + "|" + playerList.ElementAt(2).GetCards.ElementAt(2).GetName + "|" + playerList.ElementAt(2).GetCards.ElementAt(3).GetName + "|,";
                string sit4 = playerList.ElementAt(3).GetCards.ElementAt(0).GetName + "|" + playerList.ElementAt(3).GetCards.ElementAt(1).GetName + "|" + playerList.ElementAt(3).GetCards.ElementAt(2).GetName + "|" + playerList.ElementAt(3).GetCards.ElementAt(3).GetName + "|";
                return (sit1 + sit2 + sit3 + sit4);
            }
            return (sit1 + sit2);


        }
    }
}
