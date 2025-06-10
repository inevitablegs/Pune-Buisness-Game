using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public List<PlayerMovement> players;
    public Dice dice;

    private int currentPlayerIndex = 0;
    private bool isRolling = false;

    void Start()
    {
        SetCurrentPlayerTurn(true);
    }

    public void OnDiceRolled(int steps)
    {
        isRolling = true;
        SetCurrentPlayerTurn(false);
        players[currentPlayerIndex].MoveSteps(steps, OnPlayerMoveComplete);
    }

    private void OnPlayerMoveComplete()
    {
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
        SetCurrentPlayerTurn(true);
        isRolling = false;
    }

    void SetCurrentPlayerTurn(bool active)
    {
        dice.SetDiceActive(active); // Enable or disable dice based on turn
    }
}
