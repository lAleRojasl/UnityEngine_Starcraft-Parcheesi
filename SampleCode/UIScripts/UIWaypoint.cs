using System.Collections.Generic;
using UnityEngine;

public class UIWaypoint : MonoBehaviour {

    public static List<Transform> whiteSquares;
    public static List<Transform> player1Waypoints;
    public static List<Transform> player2Waypoints;
    public static List<Transform> player3Waypoints;
    public static List<Transform> player4Waypoints;
    public static List<Transform> graveyard;

    void Awake()
    {
        whiteSquares = new List<Transform>();
        player1Waypoints = new List<Transform>();
        player2Waypoints = new List<Transform>();
        player3Waypoints = new List<Transform>();
        player4Waypoints = new List<Transform>();
        graveyard = new List<Transform>();

        #region INIT WHITE SQUARES
        for (int i = 0; i < 68; i++)
        {
            whiteSquares.Add(transform.GetChild(i));
        }
        #endregion

        #region INIT PLAYER 1 WAYPOINTS
        /* Hacemos referencia a la lista de campos comunes */
        for (int i = 4; i < 68; i++)
            player1Waypoints.Add(whiteSquares[i]);

        /* Recta Final Jugador 1*/
        for (int i = 68; i < 76; i++)
        {
            player1Waypoints.Add(transform.GetChild(i));
        }
        #endregion

        #region INIT PLAYER 2 WAYPOINTS
        /* Hacemos referencia a la lista de campos comunes
         * En este caso el jugador 2 tiene como inicio la posicion 17 
         */
        for (int i = 21; i < 68; i++)
            player2Waypoints.Add(whiteSquares[i]);

        for (int i = 0; i < 17; i++)
            player2Waypoints.Add(whiteSquares[i]);

        for (int i = 76; i < 84; i++)
        {
            player2Waypoints.Add(transform.GetChild(i));
        }
        #endregion

        #region INIT PLAYER 3 WAYPOINTS
        /* Hacemos referencia a la lista de campos comunes
         * En este caso el jugador 3 tiene como inicio la posicion 34 
         */
        for (int i = 38; i < 68; i++)
            player3Waypoints.Add(whiteSquares[i]);

        for (int i = 0; i < 34; i++)
            player3Waypoints.Add(whiteSquares[i]);

        for (int i = 84; i < 92; i++)
        {
            player3Waypoints.Add(transform.GetChild(i));
        }
        #endregion

        #region INIT PLAYER 4 WAYPOINTS
        /* Hacemos referencia a la lista de campos comunes
         * En este caso el jugador 4 tiene como inicio la posicion 51 
         */
        for (int i = 55; i < 68; i++)
            player4Waypoints.Add(whiteSquares[i]);

        for (int i = 0; i < 51; i++)
            player4Waypoints.Add(whiteSquares[i]);

        for (int i = 92; i < 100; i++)
        {
            player4Waypoints.Add(transform.GetChild(i));
        }
        #endregion

        graveyard.Add(transform.GetChild(100));
        graveyard.Add(transform.GetChild(101));
        graveyard.Add(transform.GetChild(102));
        graveyard.Add(transform.GetChild(103));
    }
}
