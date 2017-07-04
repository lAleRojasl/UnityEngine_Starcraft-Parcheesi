using System.Collections.Generic;
using UnityEngine;


namespace Assets
{
    class HomeGame : MonoBehaviour
    {
        /* Network Server
         * Datos de InitMovement (Dados 1,2,3 y lista de posibles movimientos por dado)
         * Datos de RespondMovement (Cual movimiento escogio el jugador)
         * Datos de ResolveMovement (Enviar a jugadores datos necesarios para animar movimientos)
         * Tablero
         * Jugadores
         * Turno
         * Comenzo nuevo turno
         * Turnos extra
         * Posible token muerto (id de jugador y id de token a eliminar)
         * Lista de fichas en base
         * Lista de fichas en juego
         * Clase Dado
         * Dado seleccionado
         * Cuales dados ya uso en el turno
         * Salida
         * Tiempo límite
         */
        private Server netServer;
        private InitMovement initMov;
        private RespondMovement respondMov;
        private ResolveMovement resolveMov;
        private HomeBoard board;
        private List<HomePlayer> players;
        private int actualTurn = -1;
        private bool newTurn = true;
        private HomePlayer playerTurn;
        private int extraTurns = 0;
        private KeyValuePair<int, int> deadToken = new KeyValuePair<int, int>(-1, -1);
        private List<int> tokensInBase = new List<int>();
        private List<int> tokensInGame = new List<int>();
        private SixDice dice;
        private int selectedDice;
        private bool dice1Used = false;
        private bool dice2Used = false;
        private bool dice3Used = false;
        private float targetTime = 60.0f;
        
        #region SINGLETON
        /* Definimos los constructores privados */
        static HomeGame() { }
        private HomeGame() {  }
        /* Creamos una instancia privada statica */
        private static HomeGame instance = null;
        /* Y un metodo publico para poder crearlo si esta vacio o devolver el existente */
        public static HomeGame Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new HomeGame();
                }
                return instance;
            }
        }
        #endregion

        public void Update()
        {
            targetTime -= Time.deltaTime;

            if (targetTime <= 0.0f)
            {
               // Debug.Log("TIEMPO FUERA!. Terminó su turno.");
               // newTurn = true;
               // netServer.ExpectedAnimations += netServer.PlayerList.Count;
               // netServer.Broadcast("S_TIMESUP|");
            }

        }

        public void Start()
        {
            if (instance == null)
            {
                //Referencia al servidor para mandar mensajes.
                netServer = FindObjectOfType<Server>();
                //Datos logicos
                CreateBoard();
                dice = new SixDice();
                initMov = new InitMovement();
                respondMov = new RespondMovement();
                resolveMov = new ResolveMovement();
                players = new List<HomePlayer>();
                /* Creamos los jugadores logicos */
                for (int i = 0; i < netServer.PlayerList.Count; i++)
                {
                    CreatePlayer(netServer.PlayerList[i].clientName);
                }
                /* Le decimos al tablero quien es el jugador actual, para saber dentro de HomeBoard cual estructura usar */
                board.SetTurn(actualTurn);
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void CreateBoard()
        {
            board = new HomeBoard();
            board.Inicialize();
            Debug.Log("Estructuras del tablero creadas correctamente!");
        }
        
        /* El servidor crea un nuevo jugador en logica */
        public void CreatePlayer(string playerName)
        {
            players.Add(new HomePlayer(playerName, 0));
        }

        #region Protocolo Mensaje 1: InitMovement (Serializar y enviar movimientos posibles por dado)
        public void SetInitMovement()
        {
            if (newTurn)
            {
                SetNewTurn();
                newTurn = false;
            }

            initMov.Dice1Interactable = false;
            initMov.Dice2Interactable = false;
            initMov.Dice3Interactable = false;
            initMov.ForcedMove = false;
            for (int i = 0; i < 4; i++)
            {
                initMov.Dice1Tokens[i] = false;
                initMov.Dice2Tokens[i] = false;
                initMov.DicesTokens[i] = false;
            }

            int Mov3 = initMov.Dice1 + initMov.Dice2;

            /* Tokens en Base y en Juego */
            tokensInBase = playerTurn.TokensInBase();
            tokensInGame = playerTurn.TokensInGame();

            bool canTakeOut = false;
            bool canMove = false;
            /* Revisamos si le salio un 5 y si no ha usado el dado
             * Si se cumple lo anterior, revisamos si puede sacar fichas.. */
            if ((initMov.Dice1 == 5 && !dice1Used) ||
               (initMov.Dice2 == 5 && !dice2Used) ||
               (Mov3 == 5 && !dice3Used))
            {
                canTakeOut = CanTakeOutTokens();
                if (canTakeOut) initMov.ForcedMove = true;
                //Debug.Log("CHECKING CANTAKEOUT : " + canTakeOut);
            }

            if (!canTakeOut)
            {

                /*Si no puede sacar tokens entonces mandamos movimientos normales*/
                if (!dice1Used)
                    canMove = CanMoveTokens(initMov.Dice1, 1);
                if (!dice2Used)
                    canMove = CanMoveTokens(initMov.Dice2, 2);
                if (!dice3Used)
                    canMove = CanMoveTokens(Mov3, 3);

            }
            /*Si tiene algun movimiento mandamos notificacion a los clientes */
            if (canMove || canTakeOut)
            {
                /* Serilizamos y enviamos los datos */
                string s = initMov.SerializeObject();
                netServer.Broadcast("S_MOV|"+actualTurn+"|"+ s);
            }
            /* Si no tiene absolutamente ninguna ficha para mover, no se que pasa segun las reglas, 
               Pero por ahora pasa al turno siguiente... */
            else
            {
                newTurn = true;
                /* Tenemos que esperar hasta que todos los jugadores terminen de animar */
                netServer.ExpectedAnimations += netServer.PlayerList.Count;
                /* Enviamos a los jugadores notificacion de que el jugador actual no puede mover ninguna ficha con los dados que saco */
                netServer.Broadcast("S_NOMOV|" + actualTurn + "|" + initMov.Dice1+ "|"+ initMov.Dice2+"\n");
            }
        }

        private bool CanTakeOutTokens()
        {
            /* Revisamos si tiene fichas para sacar */
            if (tokensInBase.Count > 0)
            {
                /* Revisamos si puede sacar ficha */
                if (board.CheckExitBlockade(actualTurn))
                {
                    /* Revisamos cual dado es el 5 para setear los datos correctamente */
                    if (initMov.Dice1 == 5 && !dice1Used)
                    {
                        SetTakeOutTokens(initMov.Dice1Tokens);
                        initMov.Dice1Interactable = true;
                        return true;
                    }
                    if (initMov.Dice2 == 5 && !dice2Used)
                    {
                        SetTakeOutTokens(initMov.Dice2Tokens);
                        initMov.Dice2Interactable = true;
                        return true;
                    }
                    if ((initMov.Dice1 + initMov.Dice2) == 5 && !dice3Used)
                    {
                        SetTakeOutTokens(initMov.DicesTokens);
                        initMov.Dice3Interactable = true;
                        return true;
                    }
                }
                return false;
            }
            return false;
        }

        public void SetTakeOutTokens(List<bool> tokenOptions)
        {
            foreach(int i in tokensInBase)
            {
                tokenOptions[i-1] = true;
            }
        }

        private bool CanMoveTokens(int DiceValue, int DiceNumber)
        {
            bool canMoveTempResult = false;
            bool canMoveFinalResult = false;
            /* Revisamos los tokens en juego y cuales puede mover con ese dado */
            for (int i = 0; i < tokensInGame.Count; i++)
            {
                int tokID = tokensInGame[i];
                HomeToken HT = playerTurn.GetHomeToken(tokID);
                int tokPos = HT.GetPosition();
                canMoveTempResult = board.CheckMoveBlockade(tokPos, DiceValue, actualTurn);
                if (canMoveTempResult && DiceNumber == 1) { initMov.Dice1Tokens[tokID - 1] = true; initMov.Dice1Interactable = true; canMoveFinalResult = true; }
                else if (canMoveTempResult && DiceNumber == 2) { initMov.Dice2Tokens[tokID - 1] = true; initMov.Dice2Interactable = true; canMoveFinalResult = true; }
                else if (canMoveTempResult && DiceNumber == 3) { initMov.DicesTokens[tokID - 1] = true; initMov.Dice3Interactable = true; canMoveFinalResult = true; }
            }
            return canMoveFinalResult;
        }
        #endregion

        #region Protocolo Mensaje 2: RespondMovement (Deserializar movimiento seleccionado por el jugador)
        public void SetRespondMovement(string data)
        {
            respondMov = respondMov.DeserializeObject(data);

            /* Revisamos y guardamos cual fue el dado que escogio */
            if (respondMov.Dice == 1) { selectedDice = initMov.Dice1; dice1Used = true; dice3Used = true; }
            if (respondMov.Dice == 2) { selectedDice = initMov.Dice2; dice2Used = true; dice3Used = true; }
            if (respondMov.Dice == 3) { selectedDice = initMov.Dice1 + initMov.Dice2; dice1Used = true; dice2Used = true; dice3Used = true;  }

            /* Resolvemos el movimiento solicitado por el jugador */
            SetResolveMovement();
            
            /* Si ya uso todos los dados, pasa el turno al jugador siguiente */
            if (dice1Used && dice2Used && dice3Used)
            {
                newTurn = true;
            }
        }
        #endregion

        #region Protocolo Mensaje 3: ResolveMovement (Serializar y enviar datos para que los clientes hagan las animaciones requeridas)

        /* Cuando el jugador selecciona un movimiento, debemos revisar si el movimiento mata una ficha enemiga */
        /* Una vez verificado eso, hacemos un Broadcast a todas los clientes para que hagan las animaciones */
        public void SetResolveMovement()
        {
            /* Realizamos el movimiento logico del token en el tablero */
            Debug.Log("Moviendo token #" + respondMov.TokenID + " del jugador " + (actualTurn + 1) + ", " + selectedDice + " posiciones.");

            /* Seteamos el jugador en juego, el TokenID que quiere mover, y la cantidad de espacios en Resolve */
            resolveMov.PlayerID = actualTurn;
            resolveMov.TokenID = respondMov.TokenID;
            resolveMov.TokenInFinalList = false;
            resolveMov.TokenInHome = false;
            resolveMov.BlockPlayerID = -1;
            resolveMov.BlockTokenID = -1;
            resolveMov.WinnerID = -1;

            /* Movimiento logico del token */
            MoveLogicToken();

            /* Solo revisamos reglas de matar si NO esta en la recta final y si el token no ha llegado al HOME */
            if (!resolveMov.TokenInHome)
            {
                if (!playerTurn.GetHomeToken(respondMov.TokenID).InFinalList())
                {
                    /* Posicion del token luego del movimiento logico */
                    int newTokPos = playerTurn.GetHomeToken(respondMov.TokenID).GetPosition();

                    /* Si al mover hay una ficha enemiga la eliminamos */
                    Debug.Log("Revisando si mata ficha en pos: " + newTokPos + " turno:" + actualTurn);
                    bool enemyCheck = board.CheckEnemyToken(newTokPos, actualTurn, resolveMov);

                    if (enemyCheck)
                    {
                        /* Hay enemigo en la casilla, seteamos deadToken con el token a morir y lo matamos*/
                        deadToken = board.GetDeadToken(newTokPos, actualTurn);
                        /* Seteamos la informacion del token muerto en ResolveMovement */
                        resolveMov.EatenPlayerID = deadToken.Key;
                        resolveMov.EatenTokenID = deadToken.Value;
                        EatToken();
                    }
                    else
                    {
                        resolveMov.EatenPlayerID = -1;
                        resolveMov.EatenTokenID = -1;
                    }
                }
            }
            
            /* Serilizamos y enviamos los datos a todos los jugadores */
            string s = resolveMov.SerializeObject();
            /* Tenemos que esperar hasta que todos los jugadores terminen de animar */
            netServer.ExpectedAnimations += netServer.PlayerList.Count;
            netServer.Broadcast("S_RSLV|" + s);
        }

        /* Funcion auxiliar de moverFicha para hacer movimiento logico y verificar si la ficha llego al Home */
        public void MoveLogicToken()
        {
            HomeToken homeToken = playerTurn.GetHomeToken(respondMov.TokenID);
            int currentPos = homeToken.GetPosition();

            /* Primero "quitamos" la ficha del jugador de la posicion anterior*/
            /* Si currentPos es diferente de -1 ya esta en el tablero */
            if (currentPos != -1)
            {
                board.RemoveToken(actualTurn, currentPos, respondMov.TokenID);
                /* Posicion antes de hacer el movimiento */
                resolveMov.WaypointID = currentPos + 1;
                /* Sumamos la posicion anterior mas el # de movimientos */
                currentPos = currentPos + selectedDice;
                resolveMov.MoveCount = selectedDice;
                /* Seteamos la nueva posicion del token tanto en la logica como en ResolveMovement */
                homeToken.SetPosition(currentPos);
            } 
            /*Si es -1 esta saliendo de la base la nueva posicion es la 0 */
            else {
                currentPos = 0;
                resolveMov.MoveCount = 1;
                playerTurn.PutInGame(respondMov.TokenID);
                resolveMov.WaypointID = currentPos;
            }
            
            resolveMov.NewPosition = currentPos;
            
            /* Verificamos si ya llego al HOME */
            int tokenPos = homeToken.GetPosition();
            if (tokenPos >= 71)
            {
                Debug.Log("El token #" + respondMov.TokenID + " del jugador " + playerTurn.GetName() + " llego a HOME!");
                playerTurn.tokensHome += 1;
                /* Como ya llego al HOME se puede quitar del juego */
                playerTurn.RemoveToken(respondMov.TokenID);
                /* Seteamos que el token llego al Home para hacer las animaciones */
                resolveMov.TokenInHome = true;

                /* Si ese era el ultimo token que faltaba por llegar al Home, el jugador gana*/
                if (playerTurn.HasWon())
                {
                    /* Seteamos el ResolveMovement que el jugador ha ganado */
                    resolveMov.WinnerID = actualTurn;
                    Debug.Log("El jugador " + playerTurn.GetName() + " llevo todas sus fichas al HOME y ha GANADO!");
                }
            }
            /* Si no: */
            else
            {
                if (tokenPos >= 64)
                {
                    Debug.Log("La ficha esta en la recta final!");
                    /* Si entra a la lista final se setea el token como InFinalList*/
                    if (!homeToken.InFinalList())
                    {
                        homeToken.SetInFinalList(true);
                    }
                    /* Adicionalmente seteamos en resolveMovement 
                       Esto es para que los UIClient puedan encontrar correctamente el Waypoint destino */
                    resolveMov.TokenInFinalList = true;
                }
                /* Le decimos a la casilla que tiene una nueva ficha de este jugador */
                board.AddToken(actualTurn, currentPos, respondMov.TokenID);
            }
        }

        /* Hay que matar este token y mandarlo al respawn */
        private void EatToken()
        {
            int jugID = deadToken.Key;
            int tokID = deadToken.Value;
            HomePlayer HP = players[jugID];
            int pos = HP.GetHomeToken(tokID).GetPosition();
            HomeToken HT = HP.GetHomeToken(tokID);
            HT.SetInGame(false);
            HT.SetPosition(-1);
            board.RemoveToken(jugID, pos, tokID);
            Debug.Log("Token #" + tokID + " del jugador " + (jugID + 1) + " eliminado!");
        }

        #endregion

        public void SetNewTurn()
        {
            actualTurn += 1;

            /* Se pasó, el turno vuelve al primer jugador */
            if (actualTurn == players.Count)
            {
                actualTurn = 0;
            }

            /* Notificamos a los clientes que hay un nuevo jugador en turno */
            //netServer.Broadcast("S_NEWTURN|" + actualTurn);

            respondMov = new RespondMovement();
            resolveMov = new ResolveMovement();
            
            initMov.Dice1 = dice.Roll();
            initMov.Dice2 = dice.Roll();
            dice1Used = false;
            dice2Used = false;
            dice3Used = false;

            /* Turno de siguiente jugador */
            /* En red aqui habría que reemplazar que haga un broadcast de quien es el nuevo turno */
            playerTurn = players[actualTurn];
            /* Le decimos al tablero quien es el jugador actual, para saber dentro de HomeBoard cual estructura usar */
            board.SetTurn(actualTurn);
            /* El timer vuelve a 1 minuto */
            targetTime = 60.0f;

            playerTurn = players[actualTurn];
            //playersAnimating = true;
        }

    }
}
