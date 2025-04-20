using System;
using System.Collections.Generic;
using System.Linq;

namespace HathatulServer
{
    internal class GameBoard
    {// this class is in charge of managing the entire board


        /// <summary>
        /// this property 'stackOfCards' contains all of the cards.  contains 52 cards
        /// </summary>
        public LinkedList<Card> stackOfCards;

        /// <summary>
        /// this property 'positionForStackOfCards' contains the index for the stackOfCards
        /// </summary>
        public int positionForStackOfCards = 0;

        // default constructor. creates the stackOfCards property and calls the function 'ShuffleStackOfCards' which shuffles the cards.
        public GameBoard()
        {
            stackOfCards = new LinkedList<Card>();
            ShuffleStackOfCards();
        }

        /// <summary>
        /// this function recieves a list and randoms it. basically shuffles the cards
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public void ShuffleLinkedList<T>(LinkedList<T> list)
        {
            Random rng = new Random();
            int n = list.Count;
            LinkedListNode<T> current = list.First;

            for (int i = 0; i < n; i++)
            {
                int r = i + rng.Next(n - i);
                LinkedListNode<T> nodeAtIndex = GetNodeAtIndex(list, r);

                // Swap values
                T temp = current.Value;
                current.Value = nodeAtIndex.Value;
                nodeAtIndex.Value = temp;

                current = current.Next;
            }
        }

        /// <summary>
        /// The function helps the 'ShuffleLinkedList' function. it returns the Node in the certain index
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public LinkedListNode<T> GetNodeAtIndex<T>(LinkedList<T> list, int index)
        {
            LinkedListNode<T> current = list.First;
            for (int i = 0; i < index; i++)
            {
                current = current.Next;
            }
            return current;
        }

        /// <summary>
        /// This function puts all the cards in a list called "stackOfCards" and shuffles the list.
        /// </summary>
        public void ShuffleStackOfCards()
        {
            // Assuming Card class has been properly defined with appropriate constructors

            for (int i = 0; i < 4; i++)
            {
                stackOfCards.AddLast(new Card("card-0",0));
            }
            for (int i = 0; i < 4; i++)
            {
                stackOfCards.AddLast(new Card("card-1",1));
            }
            for (int i = 0; i < 4; i++)
            {
                stackOfCards.AddLast(new Card("card-2",2));
            }
            for (int i = 0; i < 4; i++)
            {
                stackOfCards.AddLast(new Card("card-3",3));
            }
            for (int i = 0; i < 4; i++)
            {
                stackOfCards.AddLast(new Card("card-4",4));
            }
            for (int i = 0; i < 4; i++)
            {
                stackOfCards.AddLast(new Card("card-5",5));
            }
            for (int i = 0; i < 4; i++)
            {
                stackOfCards.AddLast(new Card("card-6",6));
            }
            for (int i = 0; i < 4; i++)
            {
                stackOfCards.AddLast(new Card("card-7",7));
            }
            for (int i = 0; i < 4; i++)
            {
                stackOfCards.AddLast(new Card("card-8",8));
            }
            for (int i = 0; i < 9; i++)
            {
                stackOfCards.AddLast(new Card("card-9",9));
            }
            for (int i = 0; i < 3; i++)
            {
                stackOfCards.AddLast(new Card("swap",10));
            }
            for (int i = 0; i < 3; i++)
            {
                stackOfCards.AddLast(new Card("drawTwo",10));
            }
            for (int i = 0; i < 3; i++)
            {
                stackOfCards.AddLast(new Card("peek",10));
            }
            ShuffleLinkedList(stackOfCards);
        }

        /// <summary>
        /// This function receives the list of the player and returns the card which we were asked to send from the players cards
        /// </summary>
        /// <param name="s"></param>
        /// <param name="whichCard"></param>
        /// <returns></returns>
        public Card ShowRequestedCard(LinkedList<Card> s,string whichCard)
        {
            if (whichCard == "Left")
            {
                return s.First();
            }
            if (whichCard == "MiddleLeft")
            {
                return s.ElementAt(1);
            }
            if(whichCard == "MiddleRight")
            {
                return s.ElementAt(2);
            }
            if (whichCard == "Right")
            {
                return s.Last();
            }
            return null;
        }

        /// <summary>
        /// This function gives you the card from the stack and moves forward the position
        /// </summary>
        /// <returns></returns> 
        public Card DrawCardFromStack()
        {
            int position = positionForStackOfCards;
            positionForStackOfCards++;
            if(position == 54)
            {
                return null;
            }
            return stackOfCards.ElementAt(position);
        }

        /// <summary>
        /// This function deals the card to the player and returns the list
        /// </summary>
        /// <returns></returns>
        public LinkedList<Card> DealCardsPlayer()
        {
            LinkedList<Card> list = new LinkedList<Card>();
            for (int i = 0; i < 4; i++)
            {
                list.AddLast(stackOfCards.ElementAt(positionForStackOfCards));
                positionForStackOfCards++;
            }
            return list;
        }
        /// <summary>
        /// this function recieves a list of cards, which card location should the function change, and the value of the card the function change's with.
        /// and it changes it in the list to whichever card the requester wanted.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="whichCard"></param>
        /// <param name="theCard"></param>
        /// <returns></returns>
        public string SwitchCardWithAnotherOne(LinkedList<Card> s, string whichCard, Card theCard)
        {
            string answer = null;

            switch (whichCard)
            {
                case "Left":
                    if (s.Count > 0)
                    {
                        answer = s.First.Value.ToString();
                        s.RemoveFirst();
                        s.AddFirst(theCard);
                    }
                    break;
                case "MiddleLeft":
                    if (s.Count >= 2)
                    {
                        var node1 = s.First.Next;
                        answer = node1.Value.ToString();
                        s.Remove(node1);
                        s.AddBefore(s.Last, theCard);
                    }
                    break;
                case "MiddleRight":
                    if (s.Count >= 3)
                    {
                        var node1 = s.Last.Previous;
                        answer = node1.Value.ToString();
                        s.Remove(node1);
                        s.AddBefore(s.Last, theCard);
                    }
                    break;
                case "Right":
                    if (s.Count > 0)
                    {
                        answer = s.Last.Value.ToString();
                        s.RemoveLast();
                        s.AddLast(theCard);
                    }
                    break;
            }

            return answer;
        }

        /// <summary>
        /// this function gives you 2 cards from stack for special card draw two. it returns two values.
        /// </summary>
        /// <returns></returns>
        public (string,string) SpecialCardDrawTwo()
        {
            if (positionForStackOfCards == 54)
            {
                return ("endGame", "endGame");
            }
            int position = positionForStackOfCards;
            if (positionForStackOfCards == 53)
            {
                positionForStackOfCards++;
                return (stackOfCards.ElementAt(position).ToString(),"null");
            }
            positionForStackOfCards = positionForStackOfCards + 2;
            return (stackOfCards.ElementAt(position).ToString(), stackOfCards.ElementAt(position + 1).ToString());
        }

        
    }
}