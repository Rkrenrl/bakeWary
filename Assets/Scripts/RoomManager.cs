using UnityEngine;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager instance;

    public GameObject player;

    [Space]
    public Transform[] spawnPoints;

    [Space]
    public GameObject roomCam;

    [Space]
    public GameObject connectingUI;

    public string nickname;

    [HideInInspector]
    public int kills = 0;
    [HideInInspector]
    public int deaths = 0;
    [HideInInspector]
    public int upgradingLevel = 1;

    // 리스폰과 무관하게 유지되어야 하는 값이라 여기로 이동
    [HideInInspector]
    public float upgradingXP = 0;
    [HideInInspector]
    public float upgradeRequireXP = 100;

    public string roomNameToJoin = "test";

    void Awake()
    {
        instance = this;
    }

    public void JoinRoomButtonPressed()
    {
        PhotonNetwork.JoinOrCreateRoom(roomNameToJoin, null, null);
        nickname = FirebaseManager.instance.username;
        connectingUI.SetActive(true);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log("We're connected and in a room now");
        roomCam.SetActive(false); 
        SpawnPlayer();
    }

    public void SpawnPlayer()
    {
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        
        GameObject _player = PhotonNetwork.Instantiate(player.name, spawnPoint.position, Quaternion.identity);
        _player.GetComponent<PlayerSetup>().IsLocalPlayer();
        _player.GetComponent<Health>().isLocalPlayer = true;

        _player.GetComponent<PhotonView>().RPC("SetNickname", RpcTarget.AllBuffered, nickname);
        PhotonNetwork.LocalPlayer.NickName = nickname;
    }

    // XP 증가 + 레벨업 체크를 한 곳에서 처리 (다음 Update를 기다릴 필요 없이 즉시 반영)
    public void AddUpgradingXP(float _amount)
    {
        upgradingXP += _amount;

        while (upgradingXP >= upgradeRequireXP)
        {
            upgradingXP -= upgradeRequireXP;
            upgradingLevel++;
            upgradeRequireXP *= 1.5f;
            SetHashes();
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
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }
        catch { }
    }
}