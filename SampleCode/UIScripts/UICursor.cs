using UnityEngine;

public class UICursor : MonoBehaviour
{
    public Texture2D tokenCursor;
    public Texture2D defaultCursor;

    void OnMouseEnter()
    {
        Cursor.SetCursor(tokenCursor, Vector2.zero, CursorMode.Auto);
        if (gameObject.tag.Contains("Ficha"))
        {
            Cursor.SetCursor(tokenCursor, Vector2.zero, CursorMode.Auto);
        }
    }

    void OnMouseExit()
    {
        Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
    }
}