using UnityEngine;
using Photon.Pun;

public class DebugCommands : MonoBehaviour
{
    private PhotonView photonView;

    [PunRPC]
    void LogNetworkStatus()
    {
        Debug.Log($"Network Status:");
        Debug.Log($"- Connected: {PhotonNetwork.IsConnected}");
        Debug.Log($"- In Room: {PhotonNetwork.InRoom}");
        Debug.Log($"- Players: {PhotonNetwork.CurrentRoom?.PlayerCount}");
        Debug.Log($"- Is Master: {PhotonNetwork.IsMasterClient}");
    }
    void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            photonView.RPC("LogNetworkStatus", RpcTarget.All);
        }
    }
} 
  