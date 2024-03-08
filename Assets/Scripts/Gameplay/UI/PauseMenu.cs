using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private Button resumeButton;
    [SerializeField] private TMP_Text resumeText;
    [SerializeField] private Button mapSelectionButton;
    [SerializeField] private TMP_Text mapSelectionText;
    [SerializeField] private Button mainTitleButton;
    [SerializeField] private TMP_Text mainTitleText;

    private void OnResumeButtonDown()
    {

    }

    private void OnMapSelectionButtonDown()
    {

    }

    private void OnMainTitleButtonDown()
    {

    }

    private void OnEnable()
    {
        resumeButton.onClick.RemoveAllListeners();
        mapSelectionButton.onClick.RemoveAllListeners();
        mainTitleButton.onClick.RemoveAllListeners();

        resumeButton.onClick.AddListener(OnResumeButtonDown);
        mapSelectionButton.onClick.AddListener(OnMapSelectionButtonDown);
        mainTitleButton.onClick.AddListener(OnMainTitleButtonDown);



    }

}
