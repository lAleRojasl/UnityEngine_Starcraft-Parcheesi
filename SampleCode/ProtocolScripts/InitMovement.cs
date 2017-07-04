using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable()]
public class InitMovement
{
    private int dice1;
    private int dice2;
    private bool forcedMove = false;
    private bool dice1Interactable = false;
    private bool dice2Interactable = false;
    private bool dice3Interactable = false;
    private List<bool> dice1Tokens = new List<bool>() { false, false, false, false };
    private List<bool> dice2Tokens = new List<bool>() { false, false, false, false };
    private List<bool> dicesTokens = new List<bool>() { false, false, false, false };

    public int Dice1
    {
        get
        {
            return dice1;
        }

        set
        {
            dice1 = value;
        }
    }

    public int Dice2
    {
        get
        {
            return dice2;
        }

        set
        {
            dice2 = value;
        }
    }

    public List<bool> Dice1Tokens
    {
        get
        {
            return dice1Tokens;
        }

        set
        {
            dice1Tokens = value;
        }
    }

    public List<bool> Dice2Tokens
    {
        get
        {
            return dice2Tokens;
        }

        set
        {
            dice2Tokens = value;
        }
    }

    public List<bool> DicesTokens
    {
        get
        {
            return dicesTokens;
        }

        set
        {
            dicesTokens = value;
        }
    }

    public bool Dice1Interactable
    {
        get
        {
            return dice1Interactable;
        }

        set
        {
            dice1Interactable = value;
        }
    }

    public bool Dice2Interactable
    {
        get
        {
            return dice2Interactable;
        }

        set
        {
            dice2Interactable = value;
        }
    }

    public bool Dice3Interactable
    {
        get
        {
            return dice3Interactable;
        }

        set
        {
            dice3Interactable = value;
        }
    }

    public bool ForcedMove
    {
        get
        {
            return forcedMove;
        }

        set
        {
            forcedMove = value;
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

    public InitMovement DeserializeObject(string str)
    {
        BinaryFormatter bf = new BinaryFormatter();
        byte[] b = Convert.FromBase64String(str);
        MemoryStream ms = new MemoryStream(b);

        try
        {
            return (InitMovement)bf.Deserialize(ms);
        }
        finally
        {
            ms.Close();
        }
    }
}
