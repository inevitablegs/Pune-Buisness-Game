using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PhotonView))]
public class PlayerSpawner : MonoBehaviourPunCallbacks
{
    public Transform[] spawnPoints;
    public GameObject playerPrefab;
    public int maxPlayers = 6;
    
    private bool hasSpawned = false;
    private static Dictionary<int, int> playerSpawnIndices = new Dictionary<int, int>();
    private static bool indicesInitialized = false;

    void Awake()
    {
        // Ensure PhotonView exists
        if (GetComponent<PhotonView>() == null)
        {
            var pv = gameObject.AddComponent<PhotonView>();
            pv.ObservedComponents = new List<Component> { this };
            Debug.Log("Added missing PhotonView component");
        }
    }

    void Start()
    {
        Debug.Log("PlayerSpawner initialized");
        
        if (!indicesInitialized && PhotonNetwork.IsMasterClient)
        {
            InitializeSpawnIndices();
        }
        
        if (PhotonNetwork.InRoom && !hasSpawned)
        {
            StartCoroutine(AttemptSpawnPlayer());
        }
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"Player {PhotonNetwork.LocalPlayer.ActorNumber} joined room");
        
        if (!indicesInitialized && PhotonNetwork.IsMasterClient)
        {
            InitializeSpawnIndices();
        }
        
        if (!hasSpawned)
        {
            StartCoroutine(AttemptSpawnPlayer());
        }
    }

    void InitializeSpawnIndices()
    {
        playerSpawnIndices.Clear();
        int index = 1;
        foreach (var player in PhotonNetwork.CurrentRoom.Players.Values)
        {
            playerSpawnIndices[player.ActorNumber] = index++;
        }
        indicesInitialized = true;
        photonView.RPC("SyncSpawnIndices", RpcTarget.OthersBuffered, playerSpawnIndices);
    }

    [PunRPC]
    void SyncSpawnIndices(Dictionary<int, int> indices)
    {
        playerSpawnIndices = indices;
        indicesInitialized = true;
        Debug.Log("Received spawn indices from master");
    }

    IEnumerator AttemptSpawnPlayer()
    {
        if (hasSpawned) yield break;

        // Wait for indices to be ready
        float timeout = Time.time + 5f;
        while (!indicesInitialized && Time.time < timeout)
        {
            yield return null;
        }

        if (!indicesInitialized)
        {
            Debug.LogError("Spawn indices not initialized!");
            yield break;
        }

        // Get spawn index
        int spawnIndex;
        if (!playerSpawnIndices.TryGetValue(PhotonNetwork.LocalPlayer.ActorNumber, out spawnIndex))
        {
            Debug.LogError("No spawn index assigned for player!");
            yield break;
        }

        // Wait for scene to fully load
        while (!SceneManager.GetActiveScene().isLoaded)
        {
            yield return null;
        }

        SpawnPlayer(spawnIndex);
    }

    void SpawnPlayer(int spawnIndex)
    {
        if (hasSpawned) return;

        int spawnPointIndex = (spawnIndex - 1) % spawnPoints.Length;
        spawnPointIndex = Mathf.Clamp(spawnPointIndex, 0, spawnPoints.Length - 1);

        if (spawnPoints[spawnPointIndex] == null)
        {
            Debug.LogError($"Spawn point {spawnPointIndex} is null!");
            return;
        }

        Debug.Log($"Spawning player {PhotonNetwork.LocalPlayer.ActorNumber} at index {spawnPointIndex}");

        GameObject playerObj = PhotonNetwork.Instantiate(
            playerPrefab.name,
            spawnPoints[spawnPointIndex].position,
            spawnPoints[spawnPointIndex].rotation,
            0
        );

        if (playerObj == null)
        {
            Debug.LogError("Instantiation failed!");
            return;
        }

        // Initialize player
        PlayerMovement movement = playerObj.GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.photonView.RPC("InitializePlayer", RpcTarget.AllBuffered, 
                spawnIndex,
                spawnPoints[spawnPointIndex].position);
        }

        hasSpawned = true;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"Player {newPlayer.ActorNumber} entered room");
        
        if (PhotonNetwork.IsMasterClient)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount > maxPlayers)
            {
                PhotonNetwork.CloseConnection(newPlayer);
            }
            else
            {
                // Assign new spawn index
                int newIndex = playerSpawnIndices.Count + 1;
                playerSpawnIndices[newPlayer.ActorNumber] = newIndex;
                photonView.RPC("AddSpawnIndex", RpcTarget.AllBuffered, newPlayer.ActorNumber, newIndex);
            }
        }
    }

    [PunRPC]
    void AddSpawnIndex(int actorNumber, int spawnIndex)
    {
        playerSpawnIndices[actorNumber] = spawnIndex;
        Debug.Log($"Added spawn index {spawnIndex} for player {actorNumber}");
    }
}