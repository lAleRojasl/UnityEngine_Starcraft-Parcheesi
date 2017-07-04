using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHomeToken : MonoBehaviour
{
    public static List<Transform> tokens;
    public Transform Map;
    private Client c;

    void Awake()
    {
        /* Cantidad de waypoints en el tablero */
        tokens = new List<Transform>(transform.childCount);
        /* Referencia a clase Client */
        c = FindObjectOfType<Client>();
        /* Tokens en juego */
        Transform tok;

        /* Combinacion seleccionada por usuario ACTUAL, 
           es decir, si yo soy el jugador 2, voy a buscar la combinacion que YO (jug2) seleccione */
        /* Esto es para cargar el color de HUD especifico */
        string mySelectedTokens = c.PlayerList[c.myPlayerNumber].selectedTokens;
        string selectedColor = mySelectedTokens.Substring(2, 1);
        Image HUD = GameObject.Find("HUD").GetComponent<Image>();
        HUD.sprite = Resources.Load("HUD/HUD_" + selectedColor, typeof(Sprite)) as Sprite;

        string selectedRace = mySelectedTokens.Substring(1, 1);
        GameObject.Find("portrait").GetComponent<Animator>().runtimeAnimatorController =
            (RuntimeAnimatorController)Resources.Load("HUD/Portraits/"+selectedRace+"/portCont_" + selectedRace);

        /* Ahora toca cargar los sprites para TODOS los jugadores */
        /* Solo cargamos 4 tokens por cada jugador (si solo hay 2 jugadores no cargamos los tokens 8-16)*/
        for (int i = 0; i < (c.PlayerList.Count * 4); i++)
        {
            tok = transform.GetChild(i);
            /* Cargar controlador de sprites 0,1,2,3 (seleccion de jugador 1)*/
            if (i <= 3)
            {
                setPlayerTokens(0, tok, "WalkDL");
            }
            /* Cargar controlador de sprites 4,5,6,7 (seleccion de jugador 2)*/
            if (i > 3 && i <= 7)
            {
                setPlayerTokens(1, tok, "WalkDR");
            }
            /* Cargar controlador de sprites 8,9,10,11 (seleccion de jugador 3)*/
            if (i > 7 && i <= 11)
            {
               setPlayerTokens(2,  tok, "WalkUR");
            }
            /* Cargar controlador de sprites 12,13,14,15 (seleccion de jugador 4)*/
            if (i > 11 && i <= 15)
            {
                setPlayerTokens(3, tok, "WalkUL");
            }
            tokens.Add(tok);
        }

        /* Tambien necesitamos cargar los colores del overlay del mapa :) */
        string LineDirection = "";
        for(int i = 0; i < c.PlayerList.Count; i++)
        {
            mySelectedTokens = c.PlayerList[i].selectedTokens;
            selectedColor = mySelectedTokens.Substring(2, 1);

            if (i == 0 || i == 2)
                LineDirection = "DRUL";
            if (i == 1 || i == 3)
                LineDirection = "DLUR";
            /* Final Lists */
            Map.GetChild(i).GetComponent<SpriteRenderer>().sprite =
                Resources.Load("HUD/MapOverlay/Line" + LineDirection + "_" + selectedColor, typeof(Sprite)) as Sprite; ;
            /* Exits */
            Map.GetChild(i+4).GetComponent<SpriteRenderer>().sprite =
                Resources.Load("HUD/MapOverlay/Exit_"+selectedColor, typeof(Sprite)) as Sprite;

            Map.GetChild(i + 8).GetComponent<SpriteRenderer>().sprite =
                Resources.Load("HUD/Homes/Home" + mySelectedTokens, typeof(Sprite)) as Sprite;
        }

    }

    public void setPlayerTokens(int playerNum, Transform tok, string direction)
    {
        string mySelectedTokens = c.PlayerList[playerNum].selectedTokens;
        string selectedColor = mySelectedTokens.Substring(2, 1);

        Animator anim = tok.GetComponent<Animator>();
        if (mySelectedTokens != "_MB")
        {
            anim.runtimeAnimatorController =
                (AnimatorOverrideController)Resources.Load("StateMachine/Controllers/AC" + mySelectedTokens);
        }
        else
        {
            anim.runtimeAnimatorController =
                (RuntimeAnimatorController)Resources.Load("StateMachine/Controllers/AC" + mySelectedTokens);
        }

        anim.SetTrigger(direction);
        
        anim.speed = 0;
        tok.GetChild(0).GetComponent<SpriteRenderer>().sprite =
            Resources.Load("Tokens/tokenSelect_" + selectedColor, typeof(Sprite)) as Sprite;

        /*Esto es para asegurarse que mis tokens esten "por encima" (un valor Z mas bajo) que las otras fichas, 
         * para no bloquear el box collider */
        if(c.myPlayerNumber == playerNum)
        {
            tok.GetComponent<BoxCollider>().enabled = true;
        }
        
    }
}