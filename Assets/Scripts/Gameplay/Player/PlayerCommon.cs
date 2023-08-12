using UnityEngine;

public class PlayerCommon : MonoBehaviour
{
    public static Vector2 charSize = -Vector2.one;

    public enum CharactersIndex
    {
        Char1 = 1,
        Char2 = 2,
        Char3 = 3,
        Char4 = 4,
        Char5 = 5
    }

    public CharactersIndex charIndex;//l'index du personnage de jeu (char1; char2 ect)
    public PlayerIndex playerIndex;//l'index pour les input
    [ColorUsage(true, true)] public Color color;
    public uint id;
    public GameObject prefabs;

    private void Start()
    {
        if(charSize.x < 0f)
        {
            charSize = GetComponent<BoxCollider2D>().size;
        }
#if UNITY_EDITOR
        else if(charSize.SqrDistance(GetComponent<BoxCollider2D>().size) < 1e-5f)
        {
            Debug.LogWarning("Different char hitbox size found!");
            charSize = GetComponent<BoxCollider2D>().size;
        }
#endif
    }
}
