using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SocialPlatforms;
using Unity.VisualScripting;

public enum Team
{
    Brown,
    White,
    None
}

public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager instance;

    public GameObject player;

    [Space]
    public Transform[] spawnPoints;

    [Space]
    public GameObject roomCam;
    public GameObject UIcam;

    [Space]
    public GameObject connectingUI;

    public string nickname;

    [HideInInspector]
    public int kills = 0;
    [HideInInspector]
    public int deaths = 0;
    [Space]
    public int upgradingLevel = 1;

    // 리스폰과 무관하게 유지되어야 하는 값이라 여기로 이동
    [HideInInspector]
    public float upgradingXP = 0;
    [HideInInspector]
    public float upgradeRequireXP = 100;

    public string roomNameToJoin = "Unnamed";

    [Space]
    public GameObject localPlayerInstance;

    [Header("Team")]
    public Team myTeam;

    public TextMeshProUGUI brownTeamCount;
    public TextMeshProUGUI whiteTeamCount;
    public Button brownTeamButton;
    public Button whiteTeamButton;
    public Image teamLocked;

    public void SetTeam(Team team)
    {
        myTeam = team;
        SetHashes(); // 기존 SetHashes()가 CustomProperties를 갱신하므로, 거기에 팀 정보도 같이 실어 보냄
    }

    // public void TeamSet_None()
    // {
    //     SetTeam(Team.None);
    // }

    public void TeamSet_Brown()
    {
        SetTeam(Team.Brown);
        UIcam.SetActive(false);
        SpawnPlayer();
    }

    public void TeamSet_White()
    {
        SetTeam(Team.White);
        UIcam.SetActive(false);
        SpawnPlayer();
    }

    Team GetTeam(Photon.Realtime.Player player)
    {
        if (player.CustomProperties.TryGetValue("team", out object value))
        {
            return (Team)(int)value;
        }
        return Team.White; // 아직 선택 안 했을 때의 기본값
    }

    int CountTeam(Team team)
    {
        int count = 0;
        foreach (var p in PhotonNetwork.PlayerList)
        {
            if (GetTeam(p) == team) count++;
        }
        return count;
    }

    void Awake()
    {
        instance = this;
    }

    public void JoinRoomButtonPressed()
    {
        PhotonNetwork.JoinOrCreateRoom(roomNameToJoin, null, null);
        nickname = FirebaseManager.instance.username;
        SetTeam(Team.None);
        connectingUI.SetActive(true);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log("We're connected and in a room now");
        connectingUI.SetActive(false);
    }

    public void SpawnPlayer()
    {
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        
        GameObject _player = PhotonNetwork.Instantiate(player.name, spawnPoint.position, Quaternion.identity);
        _player.GetComponent<PlayerSetup>().IsLocalPlayer();
        _player.GetComponent<Health>().isLocalPlayer = true;

        _player.GetComponent<PhotonView>().RPC("SetNickname", RpcTarget.AllBuffered, nickname);
        PhotonNetwork.LocalPlayer.NickName = nickname;

        localPlayerInstance = _player; // 추가
    }

    public void AddUpgradingXP(float _amount)
    {
        upgradingXP += _amount;

        while (upgradingXP >= upgradeRequireXP)
        {
            upgradingXP -= upgradeRequireXP;
            upgradingLevel++;
            upgradeRequireXP *= 1.5f;
            SetHashes(); // 이것만 있으면 됨. 모든 클라이언트에 자동으로 전파됨
        }
    }

    public void SetHashes()
    {
        try
        {
            Hashtable hash = PhotonNetwork.LocalPlayer.CustomProperties;
            hash["kills"] = kills;
            hash["deaths"] = deaths;
            hash["upgradingLevel"] = upgradingLevel;
            hash["team"] = (int)myTeam; // 추가

            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }
        catch { }
    }

    void Update()
    {
        brownTeamCount.text = "Players:"+CountTeam(Team.Brown).ToString();
        whiteTeamCount.text = "Players:"+CountTeam(Team.White).ToString();
        teamLocked.gameObject.SetActive(false);

        if(CountTeam(Team.Brown) > CountTeam(Team.White))
        {
            teamLocked.gameObject.SetActive(true);
            teamLocked.transform.position = brownTeamButton.transform.position;
            brownTeamButton.gameObject.SetActive(false);
        }

        if(CountTeam(Team.Brown) < CountTeam(Team.White))
        {
            teamLocked.gameObject.SetActive(true);
            teamLocked.transform.position = whiteTeamButton.transform.position;
            whiteTeamButton.gameObject.SetActive(false);
        }
    }
}