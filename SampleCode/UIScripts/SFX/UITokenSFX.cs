using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITokenSFX : MonoBehaviour {
    /* Clase del cliente */
    private Client c;

    /* Archivos de efectos de sonido */

    /* Ataque */
    public AudioClip attack00;
    public AudioClip attack01;
    public AudioClip attack02;
    /* Muerte */
    public AudioClip dth00;
    public AudioClip dth01;
    /* Seleccion */
    public AudioClip wht00;
    public AudioClip wht01;
    public AudioClip wht02;
    public AudioClip wht03;
    public AudioClip wht04;

    /* CN = Clip Number */
    /* Numero del ultimo sonido de seleccion reproducido */
    private int selectCN = -1;
    /* Numero del ultimo sonido de movimiento reproducido */
    private int movCN = -1;
    /* Numero del ultimo sonido de movimiento (con ataque) reproducido */
    private int attackCN = -1;
    /* Numero del ultimo sonido de 'burla' (taunt) reproducido */
    private int tauntCN = -1;

    /* GameObject del Portrait */
    public GameObject portrait;

    void Awake()
    {
        /* Referencia a clase Client */
        c = FindObjectOfType<Client>();

        /* Combinacion seleccionada por usuario ACTUAL, 
           es decir, si yo soy el jugador 2, voy a buscar la combinacion que YO (jug2) seleccione */
        /* Esto es para cargar los archivos de sonido correspondientes a la raza */
        string mySelectedTokens = c.PlayerList[c.myPlayerNumber].selectedTokens;
        string selectedRace = mySelectedTokens.Substring(1, 1);
        attack00 = Resources.Load<AudioClip>("GameAudio/Voices/" + selectedRace + "/attack00");
        attack01 = Resources.Load<AudioClip>("GameAudio/Voices/" + selectedRace + "/attack01");
        attack02 = Resources.Load<AudioClip>("GameAudio/Voices/" + selectedRace + "/attack02");
        dth00 = Resources.Load<AudioClip>("GameAudio/Voices/" + selectedRace + "/dth00");
        dth01 = Resources.Load<AudioClip>("GameAudio/Voices/" + selectedRace + "/dth01");
        wht00 = Resources.Load<AudioClip>("GameAudio/Voices/" + selectedRace + "/wht00");
        wht01 = Resources.Load<AudioClip>("GameAudio/Voices/" + selectedRace + "/wht01");
        wht02 = Resources.Load<AudioClip>("GameAudio/Voices/" + selectedRace + "/wht02");
        wht03 = Resources.Load<AudioClip>("GameAudio/Voices/" + selectedRace + "/wht03");
        wht04 = Resources.Load<AudioClip>("GameAudio/Voices/" + selectedRace + "/wht04");
    }
    
    public void tokenSelected(bool sameToken)
    {
        AudioSource audio = GetComponents<AudioSource>()[0];
        if (!audio.isPlaying)
        {
            /*Generamos un numero aleatorio para reproducir ese SFX*/
            int RN = Random.Range(0, 5);
            /*Esto solo asegura que no se repita el mismo 2 veces seguidas*/
            if (RN == selectCN) selectCN = (RN + 1) % 4;
            else selectCN = RN;
            /* Asignamos el clip */
            if (selectCN == 0) audio.clip = wht00;
            if (selectCN == 1) audio.clip = wht01;
            if (selectCN == 2) audio.clip = wht02;
            if (selectCN == 3) audio.clip = wht03;
            if (selectCN == 4) audio.clip = wht04;

            /* Animamos el portrait */
            portrait.GetComponent<Animator>().SetTrigger("Talk");

            /*Reproducimos el sonido*/
            audio.Play();
        }
    }

    public void tokenMoving(int playerNumber, bool animatePortrait)
    {
        AudioSource audio = GetComponents<AudioSource>()[0];
        /*Generamos un numero aleatorio para reproducir ese SFX*/
        int RN = Random.Range(0, 5);
        /*Esto solo asegura que no se repita el mismo 2 veces seguidas*/
        if (RN == movCN) movCN = (RN + 1) % 4;
        else movCN = RN;

        string mySelectedTokens = c.PlayerList[playerNumber].selectedTokens;
        string selectedRace = mySelectedTokens.Substring(1, 1);
        audio.clip = Resources.Load<AudioClip>("GameAudio/Voices/" + selectedRace + "/yes0"+ movCN);
        /*Reproducimos el sonido*/
        audio.Play();
        /* Animamos el portrait si el que esta hablando es nuestro token */
        if(animatePortrait)
           portrait.GetComponent<Animator>().SetTrigger("Talk");
    }

    public void tokenDead(int deadPlayer, int attackingPlayer)
    {
        /* Sonido de muerte del token a morir */
        AudioSource audio = GetComponents<AudioSource>()[0];
        string SelectedTokens1 = c.PlayerList[deadPlayer].selectedTokens;
        string selectedRace1 = SelectedTokens1.Substring(1, 1);
        audio.clip = Resources.Load<AudioClip>("GameAudio/Voices/" + selectedRace1 + "/dth00");
        /* Sonido de disparo/ataque del token atacante */
        AudioSource audio2 = GetComponents<AudioSource>()[2];
        string selectedTokens2 = c.PlayerList[attackingPlayer].selectedTokens;
        string selectedRace2 = selectedTokens2.Substring(1, 1);
        audio2.clip = Resources.Load<AudioClip>("GameAudio/Voices/" + selectedRace2 + "/shoot00");

        audio.Play();
        audio2.Play();
    }

    public void playerTaunt(int attackingPlayer)
    {
        AudioSource audio = GetComponents<AudioSource>()[2];
        /*Generamos un numero aleatorio para reproducir ese SFX*/
        int RN = Random.Range(0, 5);
        /*Esto solo asegura que no se repita el mismo 2 veces seguidas*/
        if (RN == tauntCN) tauntCN = (RN + 1) % 4;
        else tauntCN = RN;

        string SelectedTokens = c.PlayerList[attackingPlayer].selectedTokens;
        string selectedRace = SelectedTokens.Substring(1, 1);
        audio.clip = Resources.Load<AudioClip>("GameAudio/Voices/" + selectedRace + "/kill0" + tauntCN);
        /*Reproducimos el sonido*/
        audio.Play();
        
        GameObject.Find("portrait").GetComponent<Animator>().runtimeAnimatorController =
            (RuntimeAnimatorController)Resources.Load("HUD/Portraits/" + selectedRace + "/portCont_" + selectedRace);

        portrait.GetComponent<Animator>().SetTrigger("Talk");
    }

    public void resetPortrait()
    {
        string mySelectedTokens = c.PlayerList[c.myPlayerNumber].selectedTokens;
        string selectedRace = mySelectedTokens.Substring(1, 1);
        GameObject.Find("portrait").GetComponent<Animator>().runtimeAnimatorController =
            (RuntimeAnimatorController)Resources.Load("HUD/Portraits/" + selectedRace + "/portCont_" + selectedRace);
    }

    public void tokenAttack(int playerNumber, bool animatePortrait)
    {
        AudioSource audio = GetComponents<AudioSource>()[0];
        /*Generamos un numero aleatorio para reproducir ese SFX*/
        int RN = Random.Range(0, 4);
        /*Esto solo asegura que no se repita el mismo 2 veces seguidas*/
        if (RN == attackCN) attackCN = (RN + 1) % 3;
        else attackCN = RN;

        string mySelectedTokens = c.PlayerList[playerNumber].selectedTokens;
        string selectedRace = mySelectedTokens.Substring(1, 1);
        audio.clip = Resources.Load<AudioClip>("GameAudio/Voices/" + selectedRace + "/attack0" + attackCN);
        /*Reproducimos el sonido*/
        audio.Play();
        /* Animamos el portrait si el que esta hablando es nuestro token */
        if (animatePortrait)
            portrait.GetComponent<Animator>().SetTrigger("Talk");

    }

}
