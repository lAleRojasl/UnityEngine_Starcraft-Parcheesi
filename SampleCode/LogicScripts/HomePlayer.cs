using System;
using System.Collections.Generic;
using UnityEngine;


namespace Assets
{
    public class HomePlayer
    {
        /* Fichas del jugador en la meta (HOME).
         * Nombre del jugador.
         * Color del jugador (0 = naranja, 1 = azul, 2 = rojo, 3 = verde).
         * Lista de fichas del jugador.
         */

        public int tokensHome = 0;
        private string name = "";
        private int color = -1;
        private List<HomeToken> tokens = new List<HomeToken>();

        #region INIT
        public HomePlayer(string name, int color)
        {
            this.name = name;
            this.color = color;

            /* Las cuatro fichas del jugador */
            HomeToken Token1, Token2, Token3, Token4;
            Token1 = new HomeToken();
            Token1.SetId(1);
            Token2 = new HomeToken();
            Token2.SetId(2);
            Token3 = new HomeToken();
            Token3.SetId(3);
            Token4 = new HomeToken();
            Token4.SetId(4);
            tokens.Add(Token1);
            tokens.Add(Token2);
            tokens.Add(Token3);
            tokens.Add(Token4);
            
        }
        #endregion
        #region GETTERS
        public string GetName()
        {
            return name;
        }

        public int GetColor()
        {
            return color;
        }
        #endregion

        /* Recibe id de token
         * Coloca token en juego
         */
        public void PutInGame(int tokenID)
        {
            foreach (HomeToken token in tokens)
            {
                if (token.GetId() == tokenID)
                {
                    token.SetInGame(true);
                    token.SetPosition(0);
                }
            }
        }

        /* Recibe id de token
         * Devuelve token
         */
        public HomeToken GetHomeToken(int tokenID)
        {
            foreach(HomeToken token in tokens)
            {
                if (token.GetId() == tokenID)
                    return token;
            }
            return null;
        }

        /* Recibe id de token
         * Remueve token
         */
        public void RemoveToken(int tokenID)
        {
            foreach (HomeToken token in tokens)
            {
                if (token.GetId() == tokenID)
                {
                    tokens.Remove(token);
                    return;
                }
            }
        }

        /* Verifica si el jugador todavia tiene fichas en la base (fuera del tablero) */
        public List<int> TokensInBase()
        {
            List<int> tokenList = new List<int>();
            for (int i = 0; i < tokens.Count; i++)
            {
                if (!tokens[i].IsInGame())
                {
                    tokenList.Add(tokens[i].GetId());
                }
            }
            return tokenList;
        }

        /* Devuelve la lista de fichas que tiene en juego */
        public List<int> TokensInGame()
        {
            List<int> tokenList = new List<int>();
            for(int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i].IsInGame())
                {
                    tokenList.Add(tokens[i].GetId());
                }
            }
            return tokenList;
        }

        /* Verifica si hay tokens en la base */
        public bool TokenInBase()
        {
            foreach(HomeToken token in tokens)
            {
                if (!token.IsInGame())
                    return true;
            }
            return false;
        }

        /* Verifica si el jugador ha ganado */
        public Boolean HasWon()
        {
            if (tokensHome == 4)
            {
                Debug.Log(name + " ha ganado!");
                return true;
            }
            else
                return false;
        }

    }
}
