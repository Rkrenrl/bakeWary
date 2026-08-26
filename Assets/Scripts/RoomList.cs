using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

public class RoomList : MonoBehaviourPunCallbacks
{
    public static RoomList instance;

    public GameObject roomManagerGameobject;
    public RoomManager roomManager;
    public GameObject LobbyCamera;

    [Header("UI")]
    public Transform roomListParent;
    public GameObject roomListItemPrefab;

    List<RoomInfo> cachedRoomList = new List<RoomInfo>();

    public void ChangeRoomToCreateName(string _roomName)
    {
        roomManager.roomNameToJoin = _roomName;
    }

    void Awake()
    {
        instance = this;
    }

    IEnumerator Start()
    {
        if(PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
            PhotonNetwork.Disconnect();
        }

        yield return new WaitUntil(() => !PhotonNetwork.IsConnected);

        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();

        PhotonNetwork.JoinLobby();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);

        foreach (var room in roomList)
        {
            int existingIndex = cachedRoomList.FindIndex(r => r.Name == room.Name);

            if (room.RemovedFromList)
            {
                // 방이 사라짐 (닫히거나 꽉 참) → 목록에 있으면 제거
                if (existingIndex != -1)
                    cachedRoomList.RemoveAt(existingIndex);
            }
            else
            {
                if (existingIndex != -1)
                    cachedRoomList[existingIndex] = room; // 기존 방 정보 갱신
                else
                    cachedRoomList.Add(room); // 새로 생긴 방 → 추가! (이 부분이 원래 빠져있었음)
            }
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        foreach(Transform roomItem in roomListParent)
        {
            Destroy(roomItem.gameObject);
        }

        foreach(var room in cachedRoomList)
        {
            GameObject roomItem = Instantiate(roomListItemPrefab, roomListParent);

            roomItem.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = room.Name;
            roomItem.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = room.PlayerCount + "/10";
        
            roomItem.GetComponent<RoomItemButton>().RoomName = room.Name;
        }
    }

    public void JoinRoomByName(string _name)
    {
        roomManager.roomNameToJoin = _name;
        roomManagerGameobject.SetActive(true);
        LobbyCamera.SetActive(false);
        roomManager.JoinRoomButtonPressed();
    }
}