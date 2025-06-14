using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    public List<PlayerMovement> players = new List<PlayerMovement>();
    public Dice dice;
    
    private int currentPlayerIndex = 0;
    private bool isRolling = false;
    private bool gameStarted = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RegisterPlayer(PlayerMovement player)
    {
        if (!players.Contains(player))
        {
            players.Add(player);
            Debug.Log($"Registered player {players.Count} - {player.photonView.Owner.NickName}");

            // Only start game if we're the master client and have enough players
            if (PhotonNetwork.IsMasterClient && players.Count >= 2 && !gameStarted)
            {
                gameStarted = true;
                photonView.RPC("StartGame", RpcTarget.AllBuffered);
            }
        }
    }

    [PunRPC]
    void StartGame()
    {
        Debug.Log($"Game started with {players.Count} players");
        SetCurrentPlayerTurn(true);
    }

    public void OnDiceRolled(int steps)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (players.Count == 0) return;
        
        isRolling = true;
        SetCurrentPlayerTurn(false);

        // Safely handle currentPlayerIndex
        currentPlayerIndex = Mathf.Clamp(currentPlayerIndex, 0, players.Count - 1);
        
        if (currentPlayerIndex >= 0 && currentPlayerIndex < players.Count && 
            players[currentPlayerIndex] != null)
        {
            players[currentPlayerIndex].MoveSteps(steps, OnPlayerMoveComplete);
        }
        else
        {
            Debug.LogError("Invalid player index or null player!");
            isRolling = false;
            SetCurrentPlayerTurn(true);
        }
    }

    public void OnPlayerMoveComplete()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
        SetCurrentPlayerTurn(true);
        isRolling = false;
    }

    void SetCurrentPlayerTurn(bool active)
    {
        if (dice != null)
        {
            dice.SetDiceActive(active);
            
            // Only enable dice for the current player
            if (active && players.Count > 0 && currentPlayerIndex < players.Count)
            {
                PlayerMovement currentPlayer = players[currentPlayerIndex];
                if (currentPlayer != null && currentPlayer.photonView.IsMine)
                {
                    PhotonView dicePhotonView = dice.GetComponent<PhotonView>();
                    if (dicePhotonView != null)
                    {
                        dicePhotonView.RPC("EnableDice", RpcTarget.AllBuffered, active);
                    }
                    else
                    {
                        Debug.LogError("Dice does not have a PhotonView component!");
                    }
                }
            }
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // Clean up disconnected players
        players.RemoveAll(player => player == null || player.photonView.Owner == otherPlayer);
        
        if (PhotonNetwork.IsMasterClient)
        {
            // Reset turn if current player left
            if (players.Count > 0)
            {
                currentPlayerIndex %= players.Count;
                SetCurrentPlayerTurn(true);
            }
        }
    }
}