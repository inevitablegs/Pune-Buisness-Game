using UnityEngine;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;

public class PlayerMovement : MonoBehaviourPun, IPunObservable
{
    public float moveSpeed = 5f;
    private int currentTileIndex = 0;
    private bool isMoving = false;
    public List<Transform> tilePath = new List<Transform>();
    private Vector3 networkPosition;


    [Header("Spawning")]
    public int spawnIndex = -1;
    public Vector3 spawnPosition;

    [PunRPC]
    public void InitializePlayer(int assignedSpawnIndex, Vector3 spawnPos)
    {
        spawnIndex = assignedSpawnIndex;
        spawnPosition = spawnPos;
        
        Debug.Log($"Player {photonView.Owner.ActorNumber} initialized at spawn {spawnIndex}");

        if (photonView.IsMine)
        {
            gameObject.name = $"Player_{PhotonNetwork.LocalPlayer.ActorNumber}";
            transform.position = spawnPosition;
            
            if (TilePathManager.Instance != null)
            {
                tilePath = TilePathManager.Instance.GetPath();
                Debug.Log($"Path contains {tilePath.Count} points");
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.RegisterPlayer(this);
            }
        }
    }

    void Start()
    {
        if (spawnIndex >= 0)
        {
            transform.position = spawnPosition;
            Debug.Log($"Player spawned at position: {spawnPosition}");
        }
        if (photonView.IsMine)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RegisterPlayer(this);
            }
        }
    }

    

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(currentTileIndex);
            stream.SendNext(isMoving);
            stream.SendNext(transform.position);
        }
        else
        {
            // Network player, receive data
            currentTileIndex = (int)stream.ReceiveNext();
            isMoving = (bool)stream.ReceiveNext();
            networkPosition = (Vector3)stream.ReceiveNext();
            
            // Smooth movement for remote players
            if (!photonView.IsMine)
            {
                transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * 10f);
            }
        }
    }

    public void SetTilePath(List<Transform> path)
    {
        tilePath = path;
        photonView.RPC("SyncPathCount", RpcTarget.Others, path.Count);
    }

    [PunRPC]
    private void SyncPathCount(int count)
    {
        // Just synchronize the count, actual path should be same on all clients
    }

    public void MoveSteps(int steps, System.Action onComplete)
    {
        if (!photonView.IsMine) return;
        
        photonView.RPC("MoveStepsRPC", RpcTarget.AllBuffered, steps);
    }

    [PunRPC]
    public void MoveStepsRPC(int steps)
    {
        StartCoroutine(MoveAlongPath(steps, () => {
            if (photonView.IsMine && GameManager.Instance != null)
            {
                GameManager.Instance.OnPlayerMoveComplete();
            }
        }));
    }

    IEnumerator MoveAlongPath(int steps, System.Action onComplete)
    {
        isMoving = true;

        while (steps > 0 && tilePath.Count > 0)
        {
            currentTileIndex = (currentTileIndex + 1) % tilePath.Count;
            Transform targetTile = tilePath[currentTileIndex];

            while (Vector3.Distance(transform.position, targetTile.position) > 0.01f)
            {
                if (photonView.IsMine)
                {
                    transform.position = Vector3.MoveTowards(
                        transform.position, 
                        targetTile.position, 
                        moveSpeed * Time.deltaTime
                    );
                }
                yield return null;
            }

            transform.position = targetTile.position;
            yield return new WaitForSeconds(0.2f);
            steps--;
        }

        isMoving = false;
        onComplete?.Invoke();
    }
}