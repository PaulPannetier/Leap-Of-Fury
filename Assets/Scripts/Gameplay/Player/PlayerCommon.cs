using UnityEngine;

public class PlayerCommon : MonoBehaviour
{
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
}
