using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public List<Transform> tilePath; // Assign all 40 tile transforms in order
    public float moveSpeed = 5f;

    private int currentTileIndex = 0;
    private bool isMoving = false;

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
    onComplete?.Invoke(); // Notify game manager
}
}
