using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun; // 👈 Needed for Photon

public class PlayerMovement : MonoBehaviourPun
{
    public float moveSpeed = 5f;
    private int currentTileIndex = 0;
    private bool isMoving = false;

    public List<Transform> tilePath = new List<Transform>();

    // 👇 Called by PlayerSpawner
    public void SetTilePath(List<Transform> path)
    {
        tilePath = path;
    }

    public void MoveSteps(int steps, System.Action onComplete)
    {
        StartCoroutine(MoveAlongPath(steps, onComplete));
    }

    IEnumerator MoveAlongPath(int steps, System.Action onComplete)
    {
        isMoving = true;

        while (steps > 0)
        {
            currentTileIndex = (currentTileIndex + 1) % tilePath.Count;
            Transform targetTile = tilePath[currentTileIndex];

            while (Vector3.Distance(transform.position, targetTile.position) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetTile.position, moveSpeed * Time.deltaTime);
                yield return null;
            }

            transform.position = targetTile.position;
            yield return new WaitForSeconds(0.2f);
            steps--;
        }

        isMoving = false;
        onComplete?.Invoke(); // Notify GameManager
    }

    void Start()
    {
        // Only the local player registers with GameManager
        if (photonView.IsMine && GameManager.Instance != null)
        {
            GameManager.Instance.RegisterPlayer(this);
        }
    }
}
