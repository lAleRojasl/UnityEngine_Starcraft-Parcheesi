using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets
{
    class UIClient : MonoBehaviour
    {
        /* 
         * Datos de InitMovement (Dados 1,2,3 y lista de posibles movimientos por dado)
         * Datos de RespondMovement (Cual movimiento escogio el jugador)
         * Datos de ResolveMovement (Enviar a jugadores datos necesarios para animar movimientos)
         * Clase encargada de reproducir voces de los personajes (tokens)
         * Clase encargada de modificar el State de los personajes (tokens)
         * Numero de turno que se le asigno al jugador
         * Token muerto (id de jugador y id de token a eliminar)
         * Tiempo límite
         * c es la referencia a la clase Cliente que usa el juego para mandar mensajes al server 
         */
        // private int actualTurn = 0;
        private InitMovement initMov;
        private RespondMovement respondMov;
        private ResolveMovement resolveMov;
        private UITokenSFX tokSFX;
        private StateManager stateMng;
        private int myTurnNumber = -1;
        private KeyValuePair<int, int> deadToken = new KeyValuePair<int, int>(-1, -1);
        private float targetTime = 65.0f;
        private float deathTime = 0.60f;
        private float messageTime = 0.0f;
        private float enemyPortraitTime = 2f;
        private int lastSelectedButton = -1;
        private Client c;

        /* UI variables */
        public float speed = 90f;
        public GameObject DiceOne;
        public GameObject DiceTwo;
        public GameObject DiceThree;
        private int musicPlaying = -1;
        private GameObject countdownLabel;

        /* Casilla destino */
        private Transform Target; 
        private Transform actualToken;
        private Transform blockTokTransform;
        private Transform deadTokTransform;
        /* 0:UL - 1:UR - 2:DR - 3:DL*/
        private int movDirection;
        private bool animatingMove = false;
        private bool animatingBlock = false;
        private bool showingEnemyPortrait = false;
        private bool canSelectToken = false;
        private bool animatingDead = false;
        private List<int> availableTokens;
        private int waypointPos = -1;
        private int tempMoves = 0;


        #region SINGLETON

        /* Definimos los constructores privados */
        static UIClient() { }
        private UIClient() {  }
        /* Creamos una instancia privada statica */
        private static UIClient instance = null;
        /* Y un metodo publico para poder crearlo si esta vacio o devolver el existente */
        public static UIClient Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = GameObject.Find("UIClient").GetComponent<UIClient>();
                }
                return instance;
            }
        }
        #endregion

        public void Update()
        {
            if (animatingDead)
            {
                #region Animated Dead
                int deadTokNum = (deadToken.Key * 4) + (deadToken.Value - 1);
                deadTokTransform = UIHomeToken.tokens[deadTokNum];
                /* Mostramos animacion de disparar y morir (solo una vez) */
                if (!stateMng.getCurrentState(deadTokTransform, "Die"))
                {
                    stateMng.setShootingState(actualToken);
                    stateMng.setDeadState(deadTokTransform);
                    tokSFX.tokenDead(deadToken.Key, resolveMov.PlayerID);
                }

                /* Ya termino la animacion de muerte y tenemos que devolver la ficha */
                if (deathTime < 0)
                {
                    /* Token Respawn */
                    stateMng.tokenRespawn(deadTokTransform);
                    Transform Graveyard = UIWaypoint.graveyard[deadToken.Key];
                    deadTokTransform.position = Graveyard.position;
                    deadTokTransform = null;
                    tokSFX.playerTaunt(resolveMov.PlayerID);
                    showingEnemyPortrait = true;
                    /* Limpiamos la tupla */
                    deadToken = new KeyValuePair<int, int>(-1, -1);
                    animatingDead = false;
                    deathTime = 0.65f;
                } else { deathTime -= Time.deltaTime;  }
                #endregion
            }

            #region Animated Movement
            if (animatingMove && !animatingDead)
            {
                if (tempMoves > 0)
                {
                    Vector3 dir = Target.position - actualToken.position;

                    /* Revisamos si hay que matar un token cuando estamos cerca de el */
                    if (tempMoves == 1 && deadToken.Key != -1)
                    {
                        string mySelectedTokens = c.PlayerList[resolveMov.PlayerID].selectedTokens;
                        string selectedRace = mySelectedTokens.Substring(1, 1);
                        float distance = Vector3.Distance(actualToken.position, Target.position);
                        if ((distance <= 40f && selectedRace == "M") || distance <= 25f)
                            animatingDead = true;
                    }
                    /* Seteamos la animacion (state) de la ficha basado en la direccion */
                    stateMng.setMovingState(dir, actualToken);

                    /* Deltatime es para mantener una velocidad constante dependiendo del framerate */
                    actualToken.Translate(dir.normalized * speed * Time.deltaTime);

                    if (Vector3.Distance(actualToken.position, Target.position) <= 2f)
                    {
                        waypointPos += 1;
                        if(waypointPos <= 70) checkNextWaypoint();
                        tempMoves -= 1;
                        /* Le damos un pequeño movimiento hacia el siguiente waypoint para que quede viendo en la direccion correcta */
                        if (tempMoves == 0)
                        {
                            Vector3 dirF = Target.position - actualToken.position;
                            /* Deltatime es para mantener una velocidad constante dependiendo del framerate */
                            stateMng.setMovingState(dirF, actualToken);
                            /* En caso de una barrera ocupamos saber en que direccion iba al ficha para moverlas acordemente */
                            /* Con X negativo y Y positivo, nos estamos moviendo UpLeft = 0*/
                            if (dirF.x < 0 && dirF.y > 0) movDirection = 0;
                            /* Con X positivo y Y positivo, nos estamos moviendo UpRight = 1*/
                            if (dirF.x > 0 && dirF.y > 0) movDirection = 1;
                            /* Con X positivo y Y negativo, nos estamos moviendo DownRight = 2*/
                            if (dirF.x > 0 && dirF.y < 0) movDirection = 2;
                            /* Con X negativo y Y negativo, nos estamos moviendo DownLeft = 3*/
                            if (dirF.x < 0 && dirF.y < 0) movDirection = 3;
                        }
                    }
                }
                else
                {
                    /* Termino el movimiento pero se formo una barrera, debemos separar los tokens para que se puedan distinguir */
                    if (animatingBlock)
                    {
                        /* Direccion de movimiento basado en direccion actual */
                        Vector3 dir1 = blockSplitDirection(1, actualToken.position);
                        Vector3 dir2 = blockSplitDirection(2, blockTokTransform.position);

                        /* Seteamos la animacion (state) de la ficha 1 basado en la direccion */
                        stateMng.setMovingState(dir1, actualToken);
                        /* Seteamos la animacion (state) de la ficha 2 basado en la direccion */
                        stateMng.setMovingState(dir2, blockTokTransform);

                        /* Deltatime es para mantener una velocidad constante dependiendo del framerate */
                        actualToken.Translate(dir1 * speed * Time.deltaTime);
                        /* Deltatime es para mantener una velocidad constante dependiendo del framerate */
                        blockTokTransform.Translate(dir2.normalized * speed * Time.deltaTime);

                        if (Vector3.Distance(actualToken.position, blockTokTransform.position) >= 15f)
                        {
                            Vector3 dir3 = blockSplitDirection(3, dir1);
                            stateMng.setMovingState(dir3, actualToken);
                            stateMng.setMovingState(dir3, blockTokTransform);

                            stateMng.setIdleState(blockTokTransform);
                            stateMng.setIdleState(actualToken);
                            animatingBlock = false;
                        }
                    }
                    else
                    {
                        animatingMove = false;
                        stateMng.setIdleState(actualToken);
                        DeselectToken();
                        actualToken = null;
                        blockTokTransform = null;
                        /* Un jugador llevo todas las fichas al HOME y Gano!*/
                        if (resolveMov.WinnerID != -1)
                        {
                            Debug.Log("JUGADOR " + resolveMov.WinnerID + " GANO LA PARTIDA!");
                            /* Hacer aqui las animaciones o lo que sea del gane ...*/
                            /* Por ahora simplemente el juego para y no se envian mas jugadas, 
                             * porque los UI nunca "terminan de animar".. */

                        }
                        else c.Send("C_DONE|");
                    }
                }
            }
            #endregion
            
            if (Input.GetMouseButtonDown(0) && !animatingDead && !animatingMove && canSelectToken)
            {
                RaycastHit hitInfo = new RaycastHit();
                bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);
                if (hit)
                {
                    Transform t = hitInfo.transform;

                    if (t.gameObject.tag == ("Ficha" + (myTurnNumber + 1).ToString()))
                    {
                        int transformPos = UIHomeToken.tokens.IndexOf(t);
                        if (availableTokens.Contains(transformPos))
                        {
                            respondMov.TokenID = availableTokens.IndexOf(transformPos)+1;
                            InteractMovButton(true);
                            ShiftSprite(transformPos);
                        }
                    }
                }

            }

            if (showingEnemyPortrait)
            {
                if (enemyPortraitTime < 0)
                {
                    enemyPortraitTime = 2f;
                    showingEnemyPortrait = false;
                    tokSFX.resetPortrait();
                }
                else { enemyPortraitTime -= Time.deltaTime; }
            }

            if (targetTime > 0)
            {
                targetTime -= Time.deltaTime;
                countdownLabel.GetComponent<Text>().text = targetTime.ToString("N0");
                if (messageTime > 0)
                {
                    messageTime -= Time.deltaTime;
                    if (messageTime < 1) { SetUIMessage(-1); }
                }
            }

            /* Si se acabo la musica reproducir la siguiente */
            if (!GetComponents<AudioSource>()[1].isPlaying) playMusic();

        }

        public void Start()
        {
            if (instance == null)
            {
                instance = GameObject.Find("UIClient").GetComponent<UIClient>();
                c = FindObjectOfType<Client>();
                myTurnNumber = c.myPlayerNumber;
                for (int i = 0; i < c.PlayerList.Count; i++)
                {
                    //Mostramos en pantalla los nombre de los jugadores
                    GameObject.Find("pName" + (i + 1)).GetComponent<Text>().text = c.PlayerList[i].name;
                }
                initMov = new InitMovement();
                resolveMov = new ResolveMovement();
                respondMov = new RespondMovement();
                tokSFX = GetComponent<UITokenSFX>();
                stateMng = GetComponent<StateManager>();
                countdownLabel = GameObject.Find("Countdown");
                setDiceButtonSprite(-1, 3);
                playMusic();
                availableTokens = new List<int>() {-1,-1,-1,-1};
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
            
        }
        
        /* Public UI Methods */
        public Vector3 blockSplitDirection(int tokNum, Vector3 currentPos)
        {
            Vector3 dir = new Vector3(0,0,0);

            /* tokNum 3 es para devolverlos a la direccion original */
            if(tokNum == 3)
            {
                if (movDirection == 0) dir = new Vector3(-0.1f, 0.1f, 0f); /*UpLeft*/
                if (movDirection == 1) dir = new Vector3(0.1f, 0.1f, 0f); /*UpRight*/
                if (movDirection == 2) dir = new Vector3(0.1f, -0.1f, 0f); /*DownRight*/
                if (movDirection == 3) dir = new Vector3(-0.1f, -0.1f, 0f); /*DownLeft*/
                return dir;
            }

            /* La direccion de movimiento es UL (UpLeft) o DR (DownRight).*/
            if (movDirection == 0 || movDirection == 2)
            {
                if (tokNum == 1) dir = new Vector3(0.2f, 0.1f, 0);
                if (tokNum == 2) dir = new Vector3(-0.2f, -0.1f, 0);
            }
            /* La direccion de movimiento es UR (UpRight) o DL (DownLeft).*/
            if (movDirection == 1 || movDirection == 3)
            {
                if (tokNum == 1) dir = new Vector3(-0.2f, 0.1f, 0);
                if (tokNum == 2) dir = new Vector3(0.2f, -0.1f, 0);
            }
            return dir;
        }

        /* Cuando el jugador selecciona uno de los dados, le devolvemos cuales fichas puede mover con esa opcion */
        public void DiceButtonClicked(int mov)
        {
            /* Se deshabilita el boton mientras se esta animando un movimiento */
            if (animatingMove) return;

            respondMov.Dice = mov;

            /* Cambiamos el sprite del dado seleccionado */
            setDiceButtonSprite(mov, 2);
            
            string dice = "";
            if (mov == 1) dice = initMov.Dice1.ToString(); 
            if (mov == 2) dice = initMov.Dice2.ToString();
            if (mov == 3) dice = (initMov.Dice1+initMov.Dice2).ToString();

            GameObject.Find("movSelectLbl").GetComponent<Text>().text = dice;

            List<bool> optionsBool = initMov.Dice1Tokens;
            if (respondMov.Dice == 2)
                optionsBool = initMov.Dice2Tokens;
            if (respondMov.Dice == 3)
                optionsBool = initMov.DicesTokens;
            
            for(int i = 0; i < 4; i++)
            {
                if (optionsBool[i])
                {
                    availableTokens[i] = (myTurnNumber * 4) + i;
                    canSelectToken = true;
                }
            }
        }

        public void setDiceButtonSprite(int mov, int option)
        {
            string mySelectedTokens = c.PlayerList[myTurnNumber].selectedTokens;
            string selectedColor = mySelectedTokens.Substring(2, 1);
            /* Deseleccionar un boton*/
            if (option == 1)
            {
                GameObject.Find("op" + lastSelectedButton + "MovButton").GetComponent<Image>().sprite =
                    Resources.Load<Sprite>("HUD/DiceButtons/diceDefault_" + selectedColor);
            }
            /* Intercambiar botones */
            else if(option == 2)
            {
                if (mov != lastSelectedButton)
                {
                    GameObject.Find("op" + mov + "MovButton").GetComponent<Image>().sprite =
                        Resources.Load<Sprite>("HUD/DiceButtons/diceSelected_" + selectedColor);

                    if (lastSelectedButton != -1)
                    {
                        GameObject.Find("op" + lastSelectedButton + "MovButton").GetComponent<Image>().sprite =
                            Resources.Load<Sprite>("HUD/DiceButtons/diceDefault_" + selectedColor);
                    }
                    lastSelectedButton = mov;
                }
            }
            /* Set Default inicial */
            else if(option == 3)
            {
                DiceOne.GetComponent<Image>().sprite =
                    Resources.Load<Sprite>("HUD/DiceButtons/diceDefault_" + selectedColor);
                DiceTwo.GetComponent<Image>().sprite =
                    Resources.Load<Sprite>("HUD/DiceButtons/diceDefault_" + selectedColor);
               DiceThree.GetComponent<Image>().sprite =
                    Resources.Load<Sprite>("HUD/DiceButtons/diceDefault_" + selectedColor);
            }
        }
        
        public void SetNoMoveNotification(string playerName, int dice1, int dice2)
        {
            /* Escribir en mensaje que "yo" no puedo mover ninguna ficha con esos dados*/
            if (playerName == c.myName) SetUIMessage(1, "", dice1, dice2);
            /* Escribir en mensaje que el jugador "playerName" no pudo mover ficha */
            else SetUIMessage(2, playerName, dice1, dice2);
        }

        public void setCurrentPlayerName(string playerName)
        {
            GameObject.Find("TurnoLbl").GetComponent<Text>().text = playerName;
        }

        #region Protocolo Mensaje 1: InitMovement (Deserializar mensaje del server con movimientos posibles por dado)
        public void SetInitMovement(string data)
        {
            /* Deserializamos los datos */
            initMov = initMov.DeserializeObject(data);
            SetDiceOptions();
            if (initMov.ForcedMove) SetUIMessage(0);
        }
        #endregion

        #region Protocolo Mensaje 2: RespondMovement (Serializar y enviar el movimiento seleccionado)
        public void SetRespondMovement()
        {
            ClearDiceOptions();
            InteractMovButton(false);
            canSelectToken = false;
            availableTokens = new List<int>() { -1, -1, -1, -1 };
            setDiceButtonSprite(lastSelectedButton, 1);
            lastSelectedButton = -1;
            /* Serilizamos y enviamos los datos */
            string message = respondMov.SerializeObject();
            c.Send("C_RESP|" + message);
        }
        #endregion

        #region Protocolo Mensaje 3: ResolveMovement (Deserializar  y enviar el movimiento seleccionado)
        public void SetResolveMovement(string data)
        {
            /* Deserializamos los datos */
            resolveMov = resolveMov.DeserializeObject(data);

            int transformToken = (resolveMov.PlayerID * 4) + (resolveMov.TokenID - 1);
            actualToken = UIHomeToken.tokens[transformToken];

            /* Primera posicion del waypoint al que hay que mover el token */
            waypointPos = resolveMov.WaypointID;
            /* Cantidad de espacios a animar */
            tempMoves = resolveMov.MoveCount;

            /* Se creo una barrera, por lo que debemos separar las fichas un poco */
            if (resolveMov.BlockPlayerID != -1)
            {
                int blockToken = (resolveMov.BlockPlayerID * 4) + (resolveMov.BlockTokenID - 1);
                blockTokTransform = UIHomeToken.tokens[blockToken];
                animatingBlock = true;
            }

            /*Seteamos el deadtoken si es que hay uno */
            deadToken = new KeyValuePair<int, int>(resolveMov.EatenPlayerID, resolveMov.EatenTokenID);

            /* Revisamos si el Token ya llego al HOME en cuyo caso mostramos un mensaje 
             * y quitamos (por el momento) la ficha del juego.. (idealmente aqui pasa algo cool con mas sentido) */
            if (resolveMov.TokenInHome)
            {
                SetUIMessage(4, c.PlayerList[resolveMov.PlayerID].name, -1, -1);
                actualToken.gameObject.GetComponent<BoxCollider>().enabled = false;
                waypointPos = 71;
                tempMoves = 1;
            }
            /* Tambien revisamos si entro a la recta final, igual mostramos un mensaje */
            if (resolveMov.TokenInFinalList)
            {
                SetUIMessage(3, c.PlayerList[resolveMov.PlayerID].name, -1, -1);
            }
            checkNextWaypoint();

            /* Empezamos animacion de movimiento */
            animatingMove = true;
            bool animPortrait = (resolveMov.PlayerID == myTurnNumber);
            if (deadToken.Key == -1 && !resolveMov.TokenInHome)
            {
                tokSFX.tokenMoving(resolveMov.PlayerID, animPortrait);
            }
            else
            {
                tokSFX.tokenAttack(resolveMov.PlayerID, animPortrait);
            }
        }
        #endregion

        private void checkNextWaypoint()
        {
            switch (resolveMov.PlayerID)
            {
                case 0:
                    Target = UIWaypoint.player1Waypoints[waypointPos];
                    break;
                case 1:
                    Target = UIWaypoint.player2Waypoints[waypointPos];
                    break;
                case 2:
                    Target = UIWaypoint.player3Waypoints[waypointPos];
                    break;
                case 3:
                    Target = UIWaypoint.player4Waypoints[waypointPos];
                    break;
            }
        }
        
        
        public void ResetTimer()
        {
            targetTime = 60f;
        }

        /* Mensajes a mostrar en UI por distintas razones 
         * -1 > Limpiar campo 
         * 0 -> Yo no puede hacer ningun movimiento
         * 1 -> Se acabo el tiempo 
        */

        public void SetUIMessage(int msgNum)
        {
            if (msgNum == -1)
            {
                GameObject.Find("UIMessage").GetComponent<Text>().text = "";
            }
            if (msgNum == 0)
            {
                GameObject.Find("UIMessage").GetComponent<Text>().text = "We got a 5! Send a soldier into the battlefield, now!";
                messageTime = 5.0f;
            }
            if (msgNum == 1)
            {
                GameObject.Find("UIMessage").GetComponent<Text>().text = "Se acabo el tiempo!";
                messageTime = 5f;
                /* Listo */
                c.Send("C_DONE|");
            }
        }
        /* Overload con parametros extra */
        /* Mensajes a mostrar en UI por distintas razones 
         * 1 -> Otro jugador no puede hacer ningun movimiento
         * 2 -> Yo puedo sacar ficha 
         * 3 -> La ficha de un jugador entro en la recta final
         * 4 -> Un jugador llevo una ficha al HOME
        */
        public void SetUIMessage(int msgNum, string playerName, int dice1, int dice2)
        {
            if (msgNum == 1)
            {
                GameObject.Find("UIMessage").GetComponent<Text>().text = "We have no possible moves with this dices: " + dice1 + " - " + dice2;
                /* Listo */
                c.Send("C_DONE|");
            }
            if (msgNum == 2)
            {
                GameObject.Find("UIMessage").GetComponent<Text>().text = "Player "+ playerName + " can't do anything with those dices: " + dice1 + " - " + dice2;
                /* Listo */
                c.Send("C_DONE|");
            }
            if (msgNum == 3)
            {
                GameObject.Find("UIMessage").GetComponent<Text>().text = "One of " + playerName + "'s units it's getting closer to it's destination!";
            }
            if (msgNum == 4)
            {
                GameObject.Find("UIMessage").GetComponent<Text>().text = "One of " + playerName + " units has reached it's destination! One step closer to victory!";
            }
            if (msgNum == 5)
            {
                GameObject.Find("UIMessage").GetComponent<Text>().text = playerName;
            }
            messageTime = 5.0f;
        }


        /* Private UI STUFF */
        /* Agrandar sprite de token seleccionado para ver cual vamos a mover */

        private void ShiftSprite(int transformNum)
        {
            if (actualToken == null)
            {
                actualToken = UIHomeToken.tokens[transformNum];
            }
            else
            {
                actualToken.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
                actualToken = UIHomeToken.tokens[transformNum];
            }
            actualToken.GetChild(0).GetComponent<SpriteRenderer>().enabled = true;
            tokSFX.tokenSelected(true);
        }

        private void InteractMovButton(bool bl)
        {
            GameObject.Find("moverFichaButton").GetComponent<Button>().interactable = bl;
        }

        private void InteractDice(int DiceNum, bool bl)
        {
            switch (DiceNum)
            {
                case 1:
                    DiceOne.GetComponent<Button>().interactable = bl;
                    break;
                case 2:
                    DiceTwo.GetComponent<Button>().interactable = bl;
                    break;
                case 3:
                    DiceThree.GetComponent<Button>().interactable = bl;
                    break;
            }
        }

        private void ClearDiceOptions()
        {
            InteractDice(1, false);
            InteractDice(2, false);
            InteractDice(3, false);
            GameObject.Find("movSelectLbl").GetComponent<Text>().text = "-";
        }

        private void SetDiceOptions()
        {
            InteractDice(1, initMov.Dice1Interactable);
            InteractDice(2, initMov.Dice2Interactable);
            InteractDice(3, initMov.Dice3Interactable);
            int d1 = initMov.Dice1;
            int d2 = initMov.Dice2;
            DiceOne.GetComponentInChildren<Text>().text = d1.ToString();
            DiceTwo.GetComponentInChildren<Text>().text = d2.ToString();
            DiceThree.GetComponentInChildren<Text>().text = (d1 + d2).ToString();
        }

        private void playMusic()
        {
            AudioSource audio = GetComponents<AudioSource>()[1];
            musicPlaying += 1;
            if (musicPlaying == 4) musicPlaying = 1;
            string mySelectedTokens = c.PlayerList[myTurnNumber].selectedTokens;
            string selectedRace = mySelectedTokens.Substring(1, 1);
            audio.clip = Resources.Load<AudioClip>("GameAudio/Music/" + selectedRace + "/"+ musicPlaying);
            audio.Play();
        }

        private void UIShutdown()
        {
            InteractMovButton(false);
            ClearDiceOptions();
            
            GameObject.Find("movSelectLbl").GetComponent<Text>().text = "-";
            

        }

        private void DeselectToken()
        {
            actualToken.GetChild(0).GetComponent<SpriteRenderer>().enabled = false;
        }
    }
}
