using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Photon;

public class OnStart : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private GameObject characterPrefab;
    
    // Start is called before the first frame update
    void Start()
    {      
        PhotonNetwork.ConnectUsingSettings();
    }
    
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinOrCreateRoom("AnyRoomName", new RoomOptions { IsOpen = true, IsVisible = true, EmptyRoomTtl = 30000, BroadcastPropsChangeToAll = true }, TypedLobby.Default);
    }
    
    public override void OnJoinedRoom()
    {
        PhotonNetwork.Instantiate("Test", new Vector3(0, 0, 0), Quaternion.identity, 0,  new object[] { "AnyData" });
        
        base.OnJoinedRoom();
    }     
}