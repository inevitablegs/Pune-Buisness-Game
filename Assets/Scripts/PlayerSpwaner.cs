using UnityEngine;
using Photon.Pun;

public class PlayerSpawner : MonoBehaviour
{
    public GameObject playerPrefab;
    public Transform[] spawnPoints;

    void Start()
    {
        int index = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        Transform spawn = spawnPoints[index % spawnPoints.Length];
        PhotonNetwork.Instantiate(playerPrefab.name, spawn.position, Quaternion.identity);
    }
}
