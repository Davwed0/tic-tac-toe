using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;

public class GameOverState : GameState
{
    private PlayerColor winner;
    private List<Vector2Int> winningPositions;

    public GameOverState(GameStateMachine stateMachine, PlayerColor winner, List<Vector2Int> winningPositions) : base(stateMachine)
    {
        this.winner = winner;
        this.winningPositions = winningPositions;
    }

    public override void Enter()
    {
        Debug.Log($"Game Over! Player {winner} has won!");
        AudioSource audioSource = Camera.main.GetComponent<AudioSource>();
        if (audioSource != null)
        {
            AudioClip moveSound = Resources.Load<AudioClip>("Audio/game-end");
            if (moveSound != null)
                audioSource.PlayOneShot(moveSound);
        }

        GameManager.Instance.board.scores[(int)winner].IncrementScore();
        GameManager.Instance.board.HighlightWinningPositions(winningPositions);

        // Start a new game after short delay
        // In multiplayer, only master client controls game reset
        if (!PhotonNetwork.IsConnected || PhotonNetwork.IsMasterClient)
        {
            GameManager.Instance.StartCoroutine(GameManager.Instance.StartNewGame(2.0f));
        }
    }
}
