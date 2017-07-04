using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateManager : MonoBehaviour {

    private List<string> oldStates;
    private List<string> newStates;

    void Start()
    {
        oldStates = new List<string>(16);
        newStates = new List<string>(16);
        for(int i = 0; i <= 3; i++)
        {
            oldStates.Add("");
            newStates.Add("WalkDL");
        }
        for (int i = 4; i <= 7; i++)
        {
            oldStates.Add("");
            newStates.Add("WalkDR");
        }
        for (int i = 8; i <= 11; i++)
        {
            oldStates.Add("");
            newStates.Add("WalkUR");
        }
        for (int i = 12; i <= 15; i++)
        {
            oldStates.Add("");
            newStates.Add("WalkUL");
        }
    }

    public void setMovingState(Vector3 dir, Transform token)
    {
        float x = dir.x;
        float y = dir.y;
        int tokenIndex = UIHomeToken.tokens.IndexOf(token);

        /* Primero guardamos el state actual para poder desactivarlo */
        oldStates[tokenIndex] = newStates[tokenIndex];

        /*Si X es Negativo y Y Positivo --> Direccion: Arriba Izquierda (UL)*/
        if (x < 0 && y > 0) newStates[tokenIndex] = "WalkUL";
        
        /*Si X es Negativo y Y Negativo --> Direccion: Abajo  Izquierda (DL)*/
        if (x < 0 && y < 0) newStates[tokenIndex] = "WalkDL";

        /*Si X es Positivo y Y Positivo --> Direccion: Arriba Derecha   (UR)*/
        if (x > 0 && y > 0) newStates[tokenIndex] = "WalkUR";

        /*Si X es Positivo y Y Negativo --> Direccion: Abajo  Derecha   (DR)*/
        if (x > 0 && y < 0) newStates[tokenIndex] = "WalkDR";

        /* Buscamos el manager del token y le cambiamos el state */
        /* Pero solo si es un estado nuevo, si no es que se esta moviendo en la misma direccion 
           y no hace falta cambiarlo */
        Animator anim = token.GetComponent<Animator>();
        anim.speed = 1;
        string oldS = oldStates[tokenIndex];
        string newS = newStates[tokenIndex];
        anim.SetTrigger(newS);
        if (oldS != "" && oldS != newS)
        {
            anim.ResetTrigger(oldS);
        }
    }
    
    public void setIdleState(Transform token)
    {
        int tokenIndex = UIHomeToken.tokens.IndexOf(token);
        /* Primero guardamos el state actual para poder desactivarlo */
        oldStates[tokenIndex] = newStates[tokenIndex];
        token.GetComponent<Animator>().speed = 0;

    }
    
    public void setDeadState(Transform token)
    {
        int tokenIndex = UIHomeToken.tokens.IndexOf(token);
        Animator anim = token.GetComponent<Animator>();
        oldStates[tokenIndex] = newStates[tokenIndex];
        newStates[tokenIndex] = "Die";
        anim.SetTrigger("Die");
        anim.speed = 1;
    }

    public void setShootingState(Transform token)
    {
        int tokenIndex = UIHomeToken.tokens.IndexOf(token);
        Animator anim = token.GetComponent<Animator>();
        oldStates[tokenIndex] = newStates[tokenIndex];
        newStates[tokenIndex] = "Shoot";
        anim.SetTrigger("Shoot");
    }
    
    public bool getCurrentState(Transform token, string state)
    {
        int tokenIndex = UIHomeToken.tokens.IndexOf(token);
        return newStates[tokenIndex] == state;
    }

    public void tokenRespawn(Transform token)
    {
        int tokenIndex = UIHomeToken.tokens.IndexOf(token);
        Animator anim = token.GetComponent<Animator>();
        oldStates[tokenIndex] = newStates[tokenIndex];
        if (tokenIndex <= 3)
            newStates[tokenIndex] = "WalkDL";

        if (tokenIndex > 3 && tokenIndex <= 7)
            newStates[tokenIndex] = "WalkDR";

        if (tokenIndex > 7 && tokenIndex <= 11)
            newStates[tokenIndex] = "WalkUR";

        if (tokenIndex > 11 && tokenIndex <= 15)
            newStates[tokenIndex] = "WalkUL";

        anim.SetTrigger(newStates[tokenIndex]);
        anim.speed = 0;
    }

}
