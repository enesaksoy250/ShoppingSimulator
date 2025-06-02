using UnityEngine;
using UnityEngine.UI;

public class SetUsernameButton : MonoBehaviour
{
    Button button;

    private int completedProcess = 0;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void Start()
    {
        button.onClick.AddListener(SetNickname);
    }

    private void SetNickname()
    {

        string nickname = UIRepository.Instance.NicknameIF.text;

        if(string.IsNullOrEmpty(nickname) )
        {
            PanelManager.instance.LoadPanel("ErrorPanel");
            return;
        }

        PanelManager.instance.LoadPanel("WaitingPanel");

        DatabaseManager.Instance.UpdateFirebaseInfo("nickname", nickname, () => { SetNicknameFinished(); });
        DatabaseManager.Instance.UpdateLeaderboardName(nickname, () => { SetNicknameFinished(); });
    
    }

    private void SetNicknameFinished()
    {

        completedProcess++;

        if(completedProcess == 2)
        {
            PlayerPrefs.SetInt("SetNickname", 1);
            PanelManager.instance.ClosePanel("WaitingPanel");
            PanelManager.instance.ClosePanel("LeaderboardInfoPanel");
            LeaderboardButton.Instance.GetBestPlayers();
        }
  
    }

}
