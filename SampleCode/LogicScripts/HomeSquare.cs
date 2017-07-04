using System;
using System.Collections.Generic;
using UnityEngine;


namespace Assets
{
    public class HomeSquare
    {
        /* Número real de casilla [1-64].
         * Número de tokens que hay actualmente en la casilla.
         * Orden de llegada.
         * Tipo (0 = común, 1 = segura, 2 = salida de base de jugador, 3 = recta final hacia HOME).
         */

        private int number;
        private List<int> playerTokens = new List<int>();
        private List<KeyValuePair<int, int>> arrivalOrder = new List<KeyValuePair<int, int>>();
        private int type;

        #region INIT
        public HomeSquare(int number, int type)
        {
            Number = number;
            Type = type;
            /* Cantidad de fichas del jugador 1 en esta casilla */
            playerTokens.Add(0);
            /* cantidad de fichas del jugador 2 en esta casilla */
            playerTokens.Add(0);
            /* Cantidad de fichas del jugador 3 en esta casilla */
            playerTokens.Add(0);
            /* Cantidad de fichas del jugador 4 en esta casilla */
            playerTokens.Add(0);
        }
        #endregion

        /* Recibe id de jugador y id de token
         * Añade token de jugador a la casilla
         */
        public void AddToken(int playerID, int tokenID)
        {
            Debug.Log("Nuevo token #"+ tokenID +" en casilla: " + (number+5));
            playerTokens[playerID] += 1;
            if(type != 3) 
                arrivalOrder.Add(new KeyValuePair<int, int>(playerID,tokenID));
        }

        /* Recibe id de jugador y id de token
         * Remueve token de jugador a la casilla
         */
        public void RemoveToken(int playerID, int tokenID)
        {
            playerTokens[playerID] -= 1;
            if(type != 3)
                arrivalOrder.Remove(new KeyValuePair<int,int>(playerID,tokenID));
        }

        public int TokensInSquare()
        {
            int res = 0;
            foreach(int tk in playerTokens)
            {
                res += tk;
            }
            return res;
        }
            
        /* Recibe id de jugador
         * Devuelve cual fue la ultima ficha enemiga del jugador que llego a esa casilla
         */
        public KeyValuePair<int,int> LastEnemyToken(int playerID)
        {
            KeyValuePair<int, int> deadToken = new KeyValuePair<int, int>(-1,-1);
            /* Revisamos la lista de adelante para atras porque el ultimo en entrar es el ultimo de la lista */
            for (int i = arrivalOrder.Count - 1; i >= 0; i--)
            {
                /* Esta ficha es la ultima enemiga en entrar */
                if(arrivalOrder[i].Key != playerID)
                {
                    /* Devolvemos cual ficha es y de quien */
                    deadToken = new KeyValuePair<int, int>(arrivalOrder[i].Key, arrivalOrder[i].Value);
                    return deadToken;
                }
            }
            return deadToken;
        }

        /* Recibe id de jugador
         * Devuelve la ficha aliada de un jugador en esta casilla */
        public KeyValuePair<int, int> AlliedToken(int playerID)
        {
            KeyValuePair<int, int> ally = new KeyValuePair<int, int>(-1, -1);
            /* Revisamos la lista desde el principio para buscar el primer aliado que encontremos (menos el mismo)*/
            for (int i = 0; i < arrivalOrder.Count-1; i++)
            {
                /* Esta ficha es la ficha aliada en esta casilla */
                if (arrivalOrder[i].Key == playerID)
                {
                    /* Devolvemos cual ficha es y de quien */
                    ally = new KeyValuePair<int, int>(arrivalOrder[i].Key, arrivalOrder[i].Value);
                    return ally;
                }
            }
            return ally;
        }


        /* Recibe id de jugador
         * Revisa simplemente si hay alguna ficha enemiga de un jugador en esta casilla 
         */
        public bool HasEnemy(int playerID)
        {
            for(int jgID = 0; jgID < 4; jgID++)
            {
                /* Solo me interesan las fichas enemigas */
                if (jgID != playerID)
                {
                    if(playerTokens[jgID] > 0)
                    {
                        /* Si hay al menos una ficha enemiga */
                        return true;
                    }
                }
            }
            return false;
        }

        /* devuelve la informacion del token que llego primero, segundo, etc; a esa casilla s*/
        public KeyValuePair<int, int> getTokenByArrival(int arrivalPos)
        {
            return arrivalOrder[arrivalPos];
        }

        #region GETTERS SETTERS
        public int Type
        {
            get
            {
                return type;
            }

            set
            {
                type = value;
            }
        }

        public int Number
        {
            get
            {
                return number;
            }

            set
            {
                number = value;
            }
        }
        #endregion
    }
}
