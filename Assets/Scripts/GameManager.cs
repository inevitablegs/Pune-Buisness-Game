using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public List<PlayerMovement> players = new List<PlayerMovement>();
    public Dice dice;

    private int currentPlayerIndex = 0;
    private bool isRolling = false;

    void Awake()
    {
        Instance = this;
    }

    public void RegisterPlayer(PlayerMovement player)
    {
        players.Add(player);

        // Start game only when all players are registered (optional)
        if (players.Count >= 2) // adjust for your min players
        {
            SetCurrentPlayerTurn(true);
        }
    }

    public void OnDiceRolled(int steps)
    {
        isRolling = true;
        SetCurrentPlayerTurn(false);

        if (players[currentPlayerIndex] != null)
        {
            players[currentPlayerIndex].MoveSteps(steps, OnPlayerMoveComplete);
        }
        else
        {
            Debug.LogError("❌ current player is null!");
        }
    }

    private void OnPlayerMoveComplete()
    {
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
        SetCurrentPlayerTurn(true);
        isRolling = false;
    }

    void SetCurrentPlayerTurn(bool active)
    {
        if (dice != null)
        {
            dice.SetDiceActive(active);
        }
    }
}
