using Assets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

public class Server : MonoBehaviour {

    public string serverName;
    public int port = 6321;

    private List<ServerClient> playerList;
    private List<ServerClient> disconnectList;
    
    private int expectedAnimations = 0;
    private TcpListener server;
    private bool serverStarted;

    public void Init()
    {
        DontDestroyOnLoad(gameObject);
        playerList = new List<ServerClient>(4);
        disconnectList = new List<ServerClient>();

        try
        {
            server = new TcpListener(IPAddress.Any, port);
            server.Start();
            serverStarted = true;
            StartListening();
        }
        catch (Exception e)
        {
            Debug.Log("Error de Socket:" + e.Message);
        }
    }

    private void Update()
    {
        if (!serverStarted)
            return;

        foreach(ServerClient c in playerList)
        {
            /* Si el cliente se desconectó */
            if(!IsConnected(c.tcp))
            {
                Debug.Log("PLAYER" + c.clientName + " DISCONNECTED!!");
                c.tcp.Close();
                disconnectList.Add(c);
                continue;
            }
            else
            {
                NetworkStream s = c.tcp.GetStream();
                if(s.DataAvailable)
                {
                    StreamReader reader = new StreamReader(s, true);
                    string data = reader.ReadLine();
                    if (data != null)
                        OnIncomingData(c, data);
                }
            }
        }

        for(int i = 0; i < disconnectList.Count -1; i++)
        {
            /* Decirle a los playerList alguien se ha desconectado*/ 

            playerList.Remove(disconnectList[i]);
            disconnectList.RemoveAt(i);
        }
    }

    private void StartListening()
    {
        server.BeginAcceptTcpClient(AcceptTcpClient, server);
    }

    public void StopListening()
    {
        server.Stop();
    }

    private void AcceptTcpClient(IAsyncResult ar)
    {
        /* Rechazar cliente si ya hay 4 jugadores conectados */
        if (playerList.Count == 4) return;

        TcpListener listener = (TcpListener)ar.AsyncState;

        string allUsers = "";
        foreach (ServerClient i in playerList)
        {
            allUsers += i.clientName + "|";
        }

        ServerClient sc = new ServerClient(listener.EndAcceptTcpClient(ar));
        playerList.Add(sc);

        StartListening();
        int playerNumber = playerList.Count - 1;
        Unicast("S_QUIEN|"+ playerNumber +"|"+serverName+"|"+allUsers, playerNumber);
    }

    private bool IsConnected(TcpClient c)
    {
        try
        {
            if (c != null && c.Client != null && c.Client.Connected)
            {
                if (c.Client.Poll(0, SelectMode.SelectRead))
                    return !(c.Client.Receive(new byte[1], SocketFlags.Peek) == 0);

                return true;
            }
            else 
                return false;
        }
        catch
        {
            return false;
        }
    }

    /*Enviar del servidor a todos los playerList*/
    public void Broadcast(string data)
    {
        //Debug.Log("BROADCASTING: "+data);
        foreach(ServerClient sc in playerList)
        {
            try
            {
                StreamWriter writer = new StreamWriter(sc.tcp.GetStream());
                writer.WriteLine(data);
                writer.Flush();
            }
            catch(Exception e)
            {
                Debug.Log("Error de escritura: "+ e.Message);
            }
        }
    }

    /*Enviar datos a 1 solo jugador*/
    public void Unicast(string data, int playerID)
    {
        //Debug.Log("PLAYER ID" + playerID);
        ServerClient client = playerList[playerID];
        try
        {
            StreamWriter writer = new StreamWriter(client.tcp.GetStream());
            writer.WriteLine(data);
            writer.Flush();
        }
        catch (Exception e)
        {
            Debug.Log("Error de escritura: " + e.Message);
        }
    }

    /*Funcion de lectura del servidor*/
    private void OnIncomingData(ServerClient c, string data)
    {
        //Debug.Log("Cliente Dice:" + data);
        string[] aData = data.Split('|');

        switch (aData[0])
        {
            /* El servidor espera hasta que los clientes terminen las animaciones y actualizaciones */
            case "C_DONE":
                expectedAnimations -= 1;
                if (expectedAnimations == 0)
                {
                    HomeGame.Instance.SetInitMovement();
                }
                break;
            case "C_RESP":
                HomeGame.Instance.SetRespondMovement(aData[1]);
                break;
            case "C_QUIEN":
                c.clientName = aData[1];
                //HomeGame.Instance.CreatePlayer(c.clientName);
                Broadcast("S_CNN|" + c.clientName);
                break;
            case "C_READY":
                int playerNum = int.Parse(aData[1]);
                /* Combinacion seleccionada por el usuario */
                string selectedRace = aData[2]; /*Ej: M (Marine)*/
                string selectedColor = aData[3]; /*Ej: R (Red)*/
                /* Setear variables de raza y color en la informacion del cliente Ej: _MR */
                playerList[playerNum].selectedTokens = "_"+selectedRace + selectedColor;
                /* Revisar si el jugador esta listo*/
                if (!playerList[playerNum].isReady)
                {
                    /* Hacemos Broadcast de que el jugador esta listo.
                     * Enviamos el playerNum, las fichas seleccionadas, y
                       tambien mandamos si con ese jugador ya estan todos listos*/
                    playerList[playerNum].isReady = true;
                    Broadcast("S_PREADY|"+playerNum+"|"+selectedRace+"|"+selectedColor+"|"+ checkAllReady());
                }                
                break;

            /* El host presiono el boton START en el lobby y va a iniciar la partida */
            case "C_HOSTSTART":
                /* Instanciamos la logica del server */
                GameObject ServerLogic = Instantiate(Resources.Load("Prefabs/LogicPrefabs/ServerLogic", typeof(GameObject))) as GameObject;
                Instantiate(ServerLogic).GetComponent<HomeGame>();

                /* Esperamos respuesta (C_DONE) de playerList.Count jugadores */
                ExpectedAnimations = playerList.Count;
                /* Notificamos a todos los clientes de que carguen sus UI */
                Broadcast("S_STARTGAME");
                break;

        }
    }

    public bool checkAllReady()
    {
        foreach (ServerClient sc in playerList)
        {
            if (!sc.isReady) return false;
        }
        return true;
    }

    public int ExpectedAnimations
    {
        get
        {
            return expectedAnimations;
        }

        set
        {
            expectedAnimations = value;
        }
    }

    public List<ServerClient> PlayerList
    {
        get
        {
            return playerList;
        }

        set
        {
            playerList = value;
        }
    }
}

public class ServerClient
{
    /* Nombre del jugador */
    public string clientName;
    /* Conexion TCP */
    public TcpClient tcp;
    /* booleano que indica si esta listo (selecciono ficha/color) */
    public bool isReady = false;

    /* Combinacion de ficha/color escogida por el jugador */
    /* Tiene el formato _(raza)(color) . Ej: _ZG es el Zealot Verde */
    /* Esto sirve para poder carga los respectivos controllers para cada jugador 
     * Ej: AC_ZG es el controlador del State Macine de los Zealots Verdes
       Este controlador se duplica 4 veces (1 por ficha), de manera que cada ficha
       tiene movimientos(states) independientes. */
    public string selectedTokens = "";

    public ServerClient(TcpClient tcp)
    {
        this.tcp = tcp;
    }
}