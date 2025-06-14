using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public InputField roomInput;
    public int maxPlayers = 6;

    public void CreateRoom()
    {
        string roomName = roomInput.text;
        if (!string.IsNullOrEmpty(roomName))
        {
            RoomOptions options = new RoomOptions();
            options.MaxPlayers = (byte)maxPlayers;
            PhotonNetwork.CreateRoom(roomName, options);
        }
    }

    public void JoinRoom()
    {
        string roomName = roomInput.text;
        if (!string.IsNullOrEmpty(roomName))
        {
            PhotonNetwork.JoinRoom(roomName);
        }
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room: " + PhotonNetwork.CurrentRoom.Name);
        PhotonNetwork.LoadLevel("GameScene"); // Replace with your game scene
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("Join failed: " + message);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("Room creation failed: " + message);
    }
}
