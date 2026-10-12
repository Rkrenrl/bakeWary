using UnityEngine;
using System.Linq;
using Photon.Pun;
using TMPro;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using UnityEngine.UI;

public class Leaderboard : MonoBehaviour
{
    public GameObject playersHolder;

    [Header("Options")]
    public float refreshRate = 1f;

    [Header("UI")]
    public GameObject[] slots;
    [Space]
    public TextMeshProUGUI[] KDTexts;
    public TextMeshProUGUI[] nameTexts;

    void Start()
    {
        InvokeRepeating(nameof(Refresh), 1f, refreshRate);
    }

    public void Refresh()
    {
        foreach(var slot in slots)
        {
            slot.SetActive(false);
        }

        var sortedPlayerList = 
            (from player in PhotonNetwork.PlayerList orderby player.GetScore() descending select player).ToList();

        int i = 0;
        foreach(var player in sortedPlayerList)
        {
            slots[i].SetActive(true);

            if(player.NickName == "")
                player.NickName = "unnamed";

            nameTexts[i].text = player.NickName;
            KDTexts[i].text = player.GetScore().ToString();

            if(player.CustomProperties["kills"] != null)
            {
                KDTexts[i].text = player.CustomProperties["kills"] + "/" + player.CustomProperties["deaths"];

            }
            else
            {
                KDTexts[i].text = "0/0";
            }

            slots[i].GetComponent<Image>().color = 
                RoomManager.instance.GetTeam(player) == Team.Brown ? 
                new Color(195, 137, 78) : new Color(255,255,255);
            

            i++;
        }
    }

    void Update()
    {
        playersHolder.SetActive(Input.GetKey(KeyCode.Tab));
    }
}
