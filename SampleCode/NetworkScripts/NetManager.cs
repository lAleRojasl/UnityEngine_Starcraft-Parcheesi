using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Assets;

public class NetManager : MonoBehaviour {

    public static NetManager Instance { set; get; }

    public GameObject ServerLogic;
    public GameObject serverPrefab;
    public GameObject clientPrefab;

    public InstantGuiInputText serverName;
    public InstantGuiInputText hostName;
    public InstantGuiInputText playerName;
    public InstantGuiInputText hostIP;

    void Start()
    {
        if (!Instance)
        {
            Instance = this;
            DontDestroyOnLoad(Instance);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void HostButton()
    {
        try
        {
            /*Instanciamos el server, este jugador va a funcionar de Host*/
            Server s = Instantiate(serverPrefab).GetComponent<Server>();
            s.serverName = serverName.text;
            s.Init();
                        
            /*De igual manera se realiza una conexion para que el host pueda jugar*/
            Client c = Instantiate(clientPrefab).GetComponent<Client>();
            c.isHost = true;
            c.myName = hostName.text;
            if (c.myName == "")
            {
                System.Random r = new System.Random();
                c.myName = "Host" + r.Next(1, 100);
            }
            c.ConnectToServer("127.0.0.1", 6321);

            LobbyManager LM = FindObjectOfType<LobbyManager>();
            LM.c = c;
            LM.clientLoad(true);
        }
        catch(Exception e)
        {
            Debug.Log(e.Message);
        }
}

    public void ConnectToServerButton()
    {
        string hostAddress = hostIP.text;
        if (hostAddress == "")
            hostAddress = "127.0.0.1";
        try
        {
            Client c = Instantiate(clientPrefab).GetComponent<Client>();

            c.myName = playerName.text;
            if (c.myName == "")
            {
                System.Random r = new System.Random();
                c.myName = "Player" + r.Next(1, 100);
            }

            c.ConnectToServer(hostAddress, 6321);

            LobbyManager LM = FindObjectOfType<LobbyManager>();
            LM.c = c;
            LM.clientLoad(true);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }

    }

    public void BackButton()
    {
        LobbyManager LM = FindObjectOfType<LobbyManager>();
        LM.clientLoad(false);
        CloseNetwork();
    }

    public void CloseNetwork()
    {
        UIClient UIC = FindObjectOfType<UIClient>();
        if (UIC != null)
        {
            Destroy(UIC.gameObject);
        }
        Server s = FindObjectOfType<Server>();
        if (s != null)
        {
            s.StopListening();
            Destroy(s.gameObject);
        }
        HomeGame hg = FindObjectOfType<HomeGame>();
        if (hg != null)
            Destroy(hg.gameObject);
        Client c = FindObjectOfType<Client>();
        if (c != null)
        {
            Destroy(c.gameObject);
        }


    }
}
