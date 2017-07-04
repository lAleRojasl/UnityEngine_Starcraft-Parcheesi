using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Assets
{
    [Serializable()]
    public class RespondMovement
    {
        private int dice;
        private int tokenID;

        public int Dice
        {
            get
            {
                return dice;
            }

            set
            {
                dice = value;
            }
        }

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

        public RespondMovement DeserializeObject(string str)
        {
            BinaryFormatter bf = new BinaryFormatter();
            byte[] b = Convert.FromBase64String(str);
            MemoryStream ms = new MemoryStream(b);

            try
            {
                return (RespondMovement)bf.Deserialize(ms);
            }
            finally
            {
                ms.Close();
            }
        }
    }
}
