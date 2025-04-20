using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HathatulServer
{
    internal class Player
    {// this class is incharge of creating properties for each player
        /// <summary>
        /// this property 'name' contains the name of the player
        /// </summary>
        private string name;
        /// <summary>
        /// this property 'sitForAll' is incharge of moving the sit for each player that connects
        /// </summary>
        public static int sitForAll = 1;
        /// <summary>
        /// this property 'sit; is incharge of indexing each players sit
        /// </summary>
        private int sit;
        /// <summary>
        /// this property 'cards' contains a list of all the cards for a player
        /// </summary>
        private LinkedList<Card> cards;
        /// <summary>
        /// this property 'isMyTurn' contains a value. if its the turn of the player
        /// </summary>
        private bool isMyTurn = false;
        /// <summary>
        /// this property 'countForWinner' counts the values of each card for each player. each player has 4 cards.
        /// and for each player has the property 'countForWinner' which counts the values of all the cards.
        /// </summary>
        private int countForWinner;
        /// <summary>
        /// this property is incharge of each players last turn
        /// </summary>
        public bool lastTurn = false;
        /// <summary>
        /// Getters and Setters to the properties: 'IsMyTurn', 'cards', 'name','sit','countForWinner'
        /// </summary>
        public bool GetIsMyturn { get => isMyTurn; set => isMyTurn = value; }
        internal LinkedList<Card> GetCards { get => cards; set => cards = value; }
        public string GetName { get => name; set => name = value; }
        public int GetSit { get => sit; set => sit = value; }
        public int GetCountForWinner { get => countForWinner; set => countForWinner = value; }


        
        /// <summary>
        /// thie is a constructor of the object player. it creates a new player with its properties.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="cards"></param>
        /// <param name="isMyturn"></param>
        public Player(string name, LinkedList<Card> cards, bool isMyturn)
        {
            this.name = name;
            this.sit = sitForAll;
            sitForAll++;
            this.GetCards = cards;
            GetIsMyturn = isMyturn;
        }
       
      


    }
}
