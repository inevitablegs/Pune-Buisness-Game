using UnityEngine;
using Photon.Pun;
using System.Collections.Generic;

public class PlayerSpawner : MonoBehaviour
{
    public Transform[] spawnPoints;

    void Start()
    {
        if (!PhotonNetwork.InRoom)
        {
            Debug.LogWarning("⚠ Not in room — cannot spawn player!");
            return;
        }

        int index = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        Transform spawn = spawnPoints[index % spawnPoints.Length];

        GameObject playerObj = PhotonNetwork.Instantiate("Player", spawn.position, Quaternion.identity);
        Debug.Log("✅ Spawned player at: " + spawn.position);

        // Use TilePathManager
        List<Transform> path = TilePathManager.Instance.GetPath();
        playerObj.GetComponent<PlayerMovement>().SetTilePath(path);
    }
}
