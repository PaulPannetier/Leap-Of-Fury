using System;
using UnityEngine;

public class EndMenuPlayerDisplayer : MonoBehaviour
{
    private GameObject[] skulls;

    [SerializeField] private GameObject skullPrefabs;
    [SerializeField] private Transform skullsLine;
    [SerializeField] private Transform charImage;

    public void Display(DisplaySettings displaySettings)
    {
        skulls = new GameObject[displaySettings.nbKillToWin];
        for (int i = 0; i < displaySettings.nbKillToWin; i++)
        {
            skulls[i] = Instantiate(skullPrefabs, skullsLine);
        }

        Instantiate(displaySettings.playerImage, charImage);

        for (int i = 0;i < displaySettings.currentKills; i++)
        {
            Animator animator = skulls[i].GetComponent<Animator>();
            animator.SetTrigger("Activate");
        }

        Invoke(nameof(EndDisplay), displaySettings.duration);
    }

    private void EndDisplay()
    {
        Destroy(gameObject);

        LevelManager.instance.OnEndDisplayEndMenu();
    }

    public struct DisplaySettings
    {
        public int nbKillToWin;
        public int currentKills;
        public GameObject playerImage;
        public float duration;

        public DisplaySettings(int nbKillToWin, int currentKills, GameObject playerImage, float duration)
        {
            this.nbKillToWin = nbKillToWin;
            this.currentKills = currentKills;
            this.playerImage = playerImage;
            this.duration = duration;
        }
    }
}
