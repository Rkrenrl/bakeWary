using TMPro;
using UnityEngine;

public class RoomScore : MonoBehaviour
{
    public TextMeshProUGUI brownScoreText;
    public TextMeshProUGUI whiteScoreText;

    void Update()
    {
        brownScoreText.text = RoomManager.instance.GetTeamScore(Team.Brown).ToString();
        whiteScoreText.text = RoomManager.instance.GetTeamScore(Team.White).ToString();
    }
}
