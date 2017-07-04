using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets
{
    class HomeBoard
    {
        /* Casillas comunes (68 en total) 
         * Casillas de rectas finales
         * Casilla de turno
         */
        private List<HomeSquare> whiteSquares; 
        private List<HomeSquare> player1Squares;
        private List<HomeSquare> player2Squares;
        private List<HomeSquare> player3Squares;
        private List<HomeSquare> player4Squares;
        private List<HomeSquare> turnSquare;

        public void Inicialize()
        {
            #region INIT WHITE SQUARES
            whiteSquares = new List<HomeSquare>();
            List<int> seguros = new List<int>(new int[] { 7, 12, 24, 29, 41, 46, 58, 63 });
            List<int> salidas = new List<int>(new int[] { 0, 17, 34, 51 });
            for (int i = 0; i < 68; i++)
            {
                /* Marcamos estos campos como tipo seguro (tipo 1) */
                if (seguros.Contains(i))
                {
                    whiteSquares.Add(new HomeSquare(i, 1));
                }
                /* Marcamos estos campos como salidas (tipo 2) */
                else if (salidas.Contains(i))
                {
                    whiteSquares.Add(new HomeSquare(i, 2));
                }
                else
                    whiteSquares.Add(new HomeSquare(i, 0));
            }
            #endregion

            #region INIT ORANGE SQUARES
            player1Squares = new List<HomeSquare>();
            /* Hacemos referencia a la lista de campos comunes */
            for (int i = 0; i < 64; i++)
                player1Squares.Add(whiteSquares[i]);

            /* El resto son de la recta final, propios del jugador */
            for (int i = 64; i < 71; i++)
                player1Squares.Add(new HomeSquare(i, 3));
            #endregion

            #region INIT BLUE SQUARES
            player2Squares = new List<HomeSquare>();
            /* Hacemos referencia a la lista de campos comunes
             * En este caso el jugador 2 tiene como inicio la posicion 17 
             */
            for (int i = 17; i < 68; i++)
                player2Squares.Add(whiteSquares[i]);

            for (int i = 0; i < 13; i++)
                player2Squares.Add(whiteSquares[i]);

            /* El resto son de la recta final, propios del jugador */
            for (int i = 64; i < 71; i++)
                player2Squares.Add(new HomeSquare(i, 3));
            #endregion

            #region INIT RED SQUARES
            player3Squares = new List<HomeSquare>();
            /* Hacemos referencia a la lista de campos comunes
             * En este caso el jugador 3 tiene como inicio la posicion 34 
             */
            for (int i = 34; i < 68; i++)
                player3Squares.Add(whiteSquares[i]);

            for (int i = 0; i < 29; i++)
                player3Squares.Add(whiteSquares[i]);

            /* El resto son de la recta final, propios del jugador */
            for (int i = 64; i < 71; i++)
                player3Squares.Add(new HomeSquare(i, 3));
            #endregion

            #region INIT GREEN SQUARES
            player4Squares = new List<HomeSquare>();
            /* Hacemos referencia a la lista de campos comunes
             * En este caso el jugador 4 tiene como inicio la posicion 51 
             */
            for (int i = 51; i < 68; i++)
                player4Squares.Add(whiteSquares[i]);

            for (int i = 0; i < 46; i++)
                player4Squares.Add(whiteSquares[i]);

            /* El resto son de la recta final, propios del jugador */
            for (int i = 64; i < 71; i++)
                player4Squares.Add(new HomeSquare(i, 3));
            #endregion

        }

        public void SetTurn(int playerTurn)
        {
            switch (playerTurn)
            {
                case 0:
                    turnSquare = player1Squares;
                    break;

                case 1:
                    turnSquare = player2Squares;
                    break;

                case 2:
                    turnSquare = player3Squares;
                    break;

                case 3:
                    turnSquare = player4Squares;
                    break;
            }
        }

        /* Recibe posición destino del token, y turno
         * Vemos si existe un token enemigo en la casilla (true)
         * O si hay que matar al token (false)
         */
        public bool CheckEnemyToken(int tokenPosition, int playerTurn, ResolveMovement resolv)
        {
            bool checkRes = false;

            /* Casilla destino, no importa que inicie null, siempre se le va a asignar un valor abajo */
            HomeSquare HS = turnSquare[tokenPosition];

            /* En caso de barrera ocupamos saber con cual ficha se esta creando la barrera */
            KeyValuePair<int, int> blockToken = new KeyValuePair<int, int>(-1,-1);

            /* Si la ficha que movio esta sola en la casilla no hay problema */
            if (HS.TokensInSquare() == 1)
            {
                Debug.Log("Campo destino libre.");
                return checkRes;
            }
            /* La otra opcion es que ya habia 1 ficha y ahora hay 2 fichas en la casilla, y debemos revisar si es enemiga */
            else
            {
                /* Hay que ver si esa ficha es del jugador o si es enemiga */
                if (HS.HasEnemy(playerTurn))
                {
                    blockToken = HS.getTokenByArrival(0);
                    resolv.BlockPlayerID = blockToken.Key;
                    resolv.BlockTokenID = blockToken.Value;
                    /* Es enemiga pero hay que verificar que no este en un campo seguro (Type == 1 o 2).
                     * Ya que dos enemigas pueden estar juntas en estos campos
                     */
                    if (HS.Type == 1)
                    {
                        Debug.Log("Es enemiga, pero esta en zona segura. Crean una barrera!.");
                        return checkRes;
                    }
                    if(HS.Type == 2)
                    {
                        /* Existe un ultimo caso particular cuando hay un bloqueo en la salida, y el jugador puede sacar ficha
                           en este caso, es decir, ahora hay 3 fichas en la casilla, pero una es enemiga, en este caso se saca la ficha
                           y muere la enemiga */
                        if (HS.TokensInSquare() == 3)
                        {
                            Debug.Log("Ya hay 2 fichas en la salida, pero una es enemiga. Puede matarla! Crean una barrera.");
                            blockToken = HS.AlliedToken(playerTurn);
                            /* Si no hay aliado entonces es que las dos son enemigas, por lo que la barrera sera con la primera que llego */
                            if(blockToken.Key == -1)
                            {
                                blockToken = HS.getTokenByArrival(0);
                            }
                            resolv.BlockPlayerID = blockToken.Key;
                            resolv.BlockTokenID = blockToken.Value;
                            checkRes = true;
                        }
                        return checkRes;
                    }
                    else
                    {
                        checkRes = true;
                        return checkRes;
                    }
                }
                Debug.Log("Ya habia un token en la casilla, pero es aliado. Crean una barrera!");
                blockToken = HS.getTokenByArrival(0);
                resolv.BlockPlayerID = blockToken.Key;
                resolv.BlockTokenID = blockToken.Value;
                return checkRes;
            }
        }

        /* Buscamos los datos del token enemigo que debe morir */
        public KeyValuePair<int, int> GetDeadToken(int tokenPosition, int playerTurn)
        {
            KeyValuePair<int, int> deadToken = new KeyValuePair<int, int>(-1, -1);
            HomeSquare HS = turnSquare[tokenPosition];
            deadToken = HS.LastEnemyToken(playerTurn);
            Debug.Log("Moriría la ficha #" + deadToken.Value + " de el jugador " + (deadToken.Key + 1));
            return deadToken;
        }
        
        /* Recibe posición de token, siguiente movimiento, si es jugada de salida y el turno
         * Revisa que no hayan bloqueos de camino si es un movimiento normal, o en la salida si es al sacar una ficha
         */
        public bool CheckMoveBlockade(int tokenPosition, int nextMove, int playerTurn)
        {
            for (int i = tokenPosition + 1; i <= nextMove + tokenPosition; i++)
            {
                /* Revisamos cuantas fichas hay en cada casilla */
                //int cantFichas = 0;
                if (i <= 70)
                {
                    int cantFichas = turnSquare[i].TokensInSquare();

                    /* Si ya hay dos tokens en alguna de las casillas entre la posiciona actual y la destino no se mueve mover del todo*/
                    if (cantFichas == 2)
                    {
                        return false;
                    }
                }
            }
            /* En ninguna de las casillas hasta el destino hay bloqueo */
            return true;
        }

        public bool CheckExitBlockade(int playerTurn)
        {
            /* Revisamos cuantas fichas hay en la casilla de salida (0) */
            int cantFichas = turnSquare[0].TokensInSquare();

            /* Si ya hay dos tokens en la casilla destino pero al menos 1 es enemiga, si me puedo mover */
            if (cantFichas == 2)
            {
                if (turnSquare[0].HasEnemy(playerTurn))
                    return true;
                else return false;
            }
            
            return true;
        }

        /* Recibe id de jugador, id de token y posición
         * Remueve un token
         */
        public void RemoveToken(int playerID, int pos, int tokenID)
        {
            switch (playerID)
            {
                case 0:
                    player1Squares[pos].RemoveToken(0, tokenID);
                    break;
                case 1:
                    player2Squares[pos].RemoveToken(1, tokenID);
                    break;
                case 2:
                    player3Squares[pos].RemoveToken(2, tokenID);
                    break;
                case 3:
                    player4Squares[pos].RemoveToken(3, tokenID);
                    break;
            }

        }
        
        /* Recibe id de jugador, id de token y posición
         * Agrega un token a una casilla 
         */
        public void AddToken(int playerID, int pos, int tokenID)
        {
            switch (playerID)
            {
                case 0:
                    player1Squares[pos].AddToken(0, tokenID);
                    break;
                case 1:
                    player2Squares[pos].AddToken(1, tokenID);
                    break;
                case 2:
                    player3Squares[pos].AddToken(2, tokenID);
                    break;
                case 3:
                    player4Squares[pos].AddToken(3, tokenID);
                    break;
            }

        }

        /* Recibe turno y posición de ficha
         * Devuelve posicion real de la casilla en la que esta el token de un jugador
         */
        public int RealPosition(int playerTurn, int tokenPosition)
        {
            switch (playerTurn)
            {
                case 0:
                    return player1Squares[tokenPosition].Number;
                case 1:
                    return player2Squares[tokenPosition].Number;
                case 2:
                    return player3Squares[tokenPosition].Number;
                case 3:
                    return player4Squares[tokenPosition].Number;
                default:
                    return -1;
            }
        }
        
    }
}
