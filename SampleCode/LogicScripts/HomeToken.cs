
namespace Assets
{
    public class HomeToken : Token
    {
        /* ID de la ficha.
         * BOOL ficha en juego.
         * Posición de la ficha (-1 = en la base 72 = en HOME)   
         */

        private int id;
        private bool inGame = false;
        private int position = -1;
        private bool inFinalList = false;

        #region SETTERS & GETTERS
        public void SetId(int id)
        {      
            this.id = id;  
        }

        public int GetId()
        {
            return id;
        }

        public void SetPosition(int position)
        {
            this.position = position;
        }

        public int GetPosition()
        {
            return position;
        }

        public void SetInFinalList(bool inFinalList)
        {
            this.inFinalList = inFinalList;
        }

        public bool InFinalList()
        {
            return inFinalList;
        }

        public void SetInGame(bool inGame)
        {
            this.inGame = inGame;
        }

        public bool IsInGame()
        {
            return inGame;
        }
        #endregion

    }
}
