using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Assets
{
    [Serializable()]
    public class ResolveMovement
    {
        private int tokenID;
        private int waypointID;
        private int playerID;
        private int moveCount;
        private bool tokenInFinalList = false;
        private bool tokenInHome = false;
        private int newPosition;
        private int eatenTokenID;
        private int blockPlayerID;
        private int blockTokenID;
        private int eatenPlayerID = -1;
        private int winnerID = -1;

        public int TokenID
        {
            get
            {
                return tokenID;
            }

            set
            {
                tokenID = value;
            }
        }

        public int EatenTokenID
        {
            get
            {
                return eatenTokenID;
            }

            set
            {
                eatenTokenID = value;
            }
        }

        public int NewPosition
        {
            get
            {
                return newPosition;
            }

            set
            {
                newPosition = value;
            }
        }

        public int WinnerID
        {
            get
            {
                return winnerID;
            }

            set
            {
                winnerID = value;
            }
        }

        public int PlayerID
        {
            get
            {
                return playerID;
            }

            set
            {
                playerID = value;
            }
        }

        public int EatenPlayerID
        {
            get
            {
                return eatenPlayerID;
            }

            set
            {
                eatenPlayerID = value;
            }
        }

        public bool TokenInFinalList
        {
            get
            {
                return tokenInFinalList;
            }

            set
            {
                tokenInFinalList = value;
            }
        }

        public int WaypointID
        {
            get
            {
                return waypointID;
            }

            set
            {
                waypointID = value;
            }
        }

        public int MoveCount
        {
            get
            {
                return moveCount;
            }

            set
            {
                moveCount = value;
            }
        }

        public bool TokenInHome
        {
            get
            {
                return tokenInHome;
            }

            set
            {
                tokenInHome = value;
            }
        }

        public int BlockPlayerID
        {
            get
            {
                return blockPlayerID;
            }

            set
            {
                blockPlayerID = value;
            }
        }

        public int BlockTokenID
        {
            get
            {
                return blockTokenID;
            }

            set
            {
                blockTokenID = value;
            }
        }

        public string SerializeObject()
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream memStr = new MemoryStream();

            try
            {
                bf.Serialize(memStr, this);
                memStr.Position = 0;

                return Convert.ToBase64String(memStr.ToArray());
            }
            finally
            {
                memStr.Close();
            }
        }

        public ResolveMovement DeserializeObject(string str)
        {
            BinaryFormatter bf = new BinaryFormatter();
            byte[] b = Convert.FromBase64String(str);
            MemoryStream ms = new MemoryStream(b);

            try
            {
                return (ResolveMovement)bf.Deserialize(ms);
            }
            finally
            {
                ms.Close();
            }
        }
    }
}
