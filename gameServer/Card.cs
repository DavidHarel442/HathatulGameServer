using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HathatulServer
{
    internal class Card
    {// this class contains the Cards for the game. it contains two params. one is the name and the other is the Value

        /// <summary>
        /// this property 'name' contains the name of the card
        /// </summary>
        public string name;
        /// <summary>
        /// this property 'value' contains the actual value of the cards, for instance name : card-9 has the value of 9
        /// </summary>
        public int value;

        /// <summary>
        /// a constructor which recieves the name and value and puts it as the properties of the object
        /// </summary>
        /// <param name="name"></param>
        /// <param name="Value"></param>
        public Card(string name, int Value)
        {
            this.GetName = name;
            this.GetlValue = Value;
            
        }

        /// <summary>
        /// getters and setters for 'name' and 'value'
        /// </summary>
        public string GetName { get => name; set => this.name = value; }
        public int GetlValue { get => value; set => this.value = value; }


        /// <summary>
        /// a ToString function. to return the name
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.name;
        }

        /// <summary>
        /// this function recieves a cards name and returns its value
        /// </summary>
        /// <param name="cardName"></param>
        /// <returns></returns>
        public static int ReturnCardValue(string cardName)
        {
            if (cardName.Contains("0"))
            {
                return 0;
            }
            else if(cardName.Contains("1"))
            {
                return 1;
            }
            else if(cardName.Contains("2"))
            {
                return 2;
            }
            else if( cardName.Contains("3"))
            {
                return 3;
            }
            else if (cardName.Contains("4"))
            {
                return 4;
            }
            else if (cardName.Contains("5"))
            {
                return 5;
            }
            else if (cardName.Contains("6"))
            {
                return 6;
            }
            else if (cardName.Contains("7"))
            {
                return 7;
            }
            else if (cardName.Contains("8"))
            {
                return 8;
            }
            else if (cardName.Contains("9"))
            {
                return 9;
            }
            else if (cardName.Contains("swap"))
            {
                return 10;
            }
            else if (cardName.Contains("draw"))
            {
                return 10;
            }
            else if (cardName.Contains("peek"))
            {
                return 10;
            }
            return -1;
        }
    }
}
