using UnityEngine;

public class MayaTempleManager : LevelManager
{
    protected override void StartLevel()
    {
        base.StartLevel();
        InitMayasTemple();
    }

    protected override void RestartLevel()
    {
        base.RestartLevel();
        InitMayasTemple();
    }

    private void InitMayasTemple()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Char");
        foreach (GameObject player in players)
        {
            FightController fc = player.GetComponent<FightController>();
            fc.enableAttackStrong = fc.enableAttackWeak = false;
        }

        Totem.ResetAllTotems();
    }
}
