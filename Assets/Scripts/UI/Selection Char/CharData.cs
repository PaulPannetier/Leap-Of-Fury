using UnityEngine;

public struct CharData : ICloneable<CharData>
{
    public PlayerIndex playerIndex;
    public ControllerType controllerType;
    public GameObject charPrefabs;

    public CharData(PlayerIndex playerIndex, ControllerType controllerType, GameObject charPrefabs)
    {
        this.playerIndex = playerIndex;
        this.controllerType = controllerType;
        this.charPrefabs = charPrefabs;
    }

    public CharData Clone() => new CharData(playerIndex, controllerType, charPrefabs);
}