using Assets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Client : MonoBehaviour
{
    public string myName;
    public int myPlayerNumber;
    private bool socketReady;
    private TcpClient socket;
    private NetworkStream stream;
    private StreamReader reader;
    private StreamWriter writer;
    public string serverName;

    private List<GameClient> playerList = new List<GameClient>();

    public bool isHost = false;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public bool ConnectToServer(string host, int port)
    {
        if (socketReady)
            return false;

        try
        {
            socket = new TcpClient(host, port);
            stream = socket.GetStream();
            writer = new StreamWriter(stream);
            reader = new StreamReader(stream);

            socketReady = true;
        }
        catch (Exception e)
        {
            Debug.Log("Error del socket: "+ e.Message);
        }

        return socketReady;
    }

    public void Update()
    {
        if (socketReady)
        {
            if (stream.DataAvailable)
            {
                string data = reader.ReadLine();
                if (data != null)
                {
                    OnIncomingData(data);
                }
            }
        }
    }

    /*Enviar mensaje al server*/
    public void Send(string data)
    {
        if (!socketReady)
            return;

        writer.WriteLine(data);
        writer.Flush();
    }

    /* Cada vez que el servidor manda un nuevo mensaje, esta funcion se encarga de parsearlo
     * El formato es (encabezado)|(datos)|(otros datos...)
     * Donde (encabezado) determina que tipo de mensaje es:
     * -- S_QUIEN: El servidor pregunta mi nombre. Por lo que luego uso Send() para enviarle la info.
     * -- S_CNN: Notificacion del servidor de que se acaba de conectar otro jugador. Ej: S_CNN|Alejandro
     * -- S_INIT: El server notifica de quien es el siguiente turno. Ej: S_INIT|0 es el turno del jug 1 
     * -- S_MAX: Indica la cantidad de jugadores que se deben conectar antes de comenzar la partida (ya no se usa)
     * -- S_TURN: Indica el nombre del jugador que esta en turno
     * -- S_MOV: El servidor envia la clase serializada InitMovement que contiene los dados y movimientos posibles 
     * -- S_RSVL 
     * -- S_TIMESUP 
     * -- S_PREADY 
     * -- S_STARTGAME */

    private void OnIncomingData(string data)
    {
        //Debug.Log("Server dice: " + data);
        string[] aData = data.Split('|');
        int playerTurn = -1;
        switch (aData[0])
        {
            case "S_RSLV":
                UIClient.Instance.SetResolveMovement(aData[1]);
                UIClient.Instance.ResetTimer();
                break;
            /*Informacion de dados y fichas que puede mover. Los datos vienen serializados */ 
            case "S_MOV":
                playerTurn = int.Parse(aData[1]);
                UIClient.Instance.setCurrentPlayerName(playerList[playerTurn].name);
                /* Mostramos las opciones solo al jugador en turno, si no, se ignoran */
                if(playerTurn == myPlayerNumber)
                    UIClient.Instance.SetInitMovement(aData[2]);
                break;
            case "S_NOMOV":
                playerTurn = int.Parse(aData[1]);
                UIClient.Instance.SetNoMoveNotification(playerList[playerTurn].name, int.Parse(aData[2]), int.Parse(aData[3]));
                UIClient.Instance.ResetTimer();
                break;
            case "S_NEWTURN":
                string playerName = playerList[int.Parse(aData[1])].name;
                UIClient.Instance.setCurrentPlayerName(playerName);
                break;
            case "S_TIMESUP":
                UIClient.Instance.SetUIMessage(1);
                break;
            case "S_CNN":
                UserConnected(aData[1], false);

                /* Si todos estaban ready, pero se conecta alguien mas, deshabilita el boton de START del Host*/
                if (isHost)
                {
                    InstantGuiButton startButton2 = GameObject.Find("StartButton").GetComponent<InstantGuiButton>();
                    startButton2.disabled = true;
                }
                break;
            case "S_QUIEN":
                myPlayerNumber = int.Parse(aData[1]);
                serverName = aData[2];
                
                for (int i = 3; i < aData.Length - 1; i++)
                {
                    UserConnected(aData[i], false);
                }
                /*Le responde al server quien soy y si este cliente es el Host.*/
                Send("C_QUIEN|" + myName + "|" + ((isHost) ? 1 : 0).ToString());
                break;
            /* Un jugador esta listo*/
            case "S_PREADY":
                int playerNum = int.Parse(aData[1]);
                InstantGuiTextArea playerDisplay = GameObject.Find("PlayerDisplay"+(playerNum+1)).GetComponent<InstantGuiTextArea>();
                playerDisplay.pressed = true;

                /* Guardamos la combinacion de fichas que ese jugador selecciono con formato _(raza)(color) EJ: "ZR" Zealot Red 
                   Esto lo necesitamos para que en cada UI se carguen correctamente las fichas seleccionadas por cada jugador */
                playerList[playerNum].selectedTokens = "_"+aData[2] + aData[3];
                
                /* Si soy Host, reviso si todos los jugadores estan listos*/
                if (isHost)
                {
                    bool allReady = bool.Parse(aData[4]);
                    /* Si lo estan, habilitamos el boton de START! */
                    if (allReady)
                    {
                        InstantGuiButton startButton = GameObject.Find("StartButton").GetComponent<InstantGuiButton>();
                        startButton.disabled = false;
                    }
                }
                break;
            case "S_STARTGAME":
                /* Cargamos la escena del juego */
                SceneManager.LoadScene("Game");
                /* Enviamos al server confirmacion. 
                 * Una vez que todos los UIs esten DONE, el server envia el primer movimiento */
                Send("C_DONE");
                break;
        }
    }

    /*Actualizar la lista de playerList
     Una vez que esten al menos 2 conectados se puede iniciar el juego.*/

    private void UserConnected(string name, bool host)
    {
        GameClient c = new GameClient();
        c.name = name;

        playerList.Add(c);
    }
    

    /*get/set de la lista de playerList*/
    public List<GameClient> PlayerList
    {
        get
        {  return playerList;  }

        set
        {  playerList = value;  }
    }

    /*Cerrar sockets en caso de que cierren el juego.*/
    private void OnApplicationQuit()
    {
        CloseSocket();
    }

    private void OnDisable()
    {
        CloseSocket();
    }

    private void CloseSocket()
    {
        if (!socketReady)
            return;
        playerList.Clear();
        writer.Close();
        reader.Close();
        socket.Close();
        socketReady = false;
    }
}

public class GameClient
{
    public string name;
    public bool isHost;

    /* Combinacion de ficha/color escogida por el jugador */
    /* Tiene el formato _(raza)(color) . Ej: _ZG es el Zealot Verde */
    /* Esto sirve para poder carga los respectivos controllers para cada jugador 
     * Ej: AC_ZG es el controlador del State Macine de los Zealots Verdes
       Este controlador se duplica 4 veces (1 por ficha), de manera que cada ficha
       tiene movimientos(states) independientes. */
    public string selectedTokens = "";
}
