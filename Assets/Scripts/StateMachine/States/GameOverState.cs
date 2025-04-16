using UnityEngine;
using System.Collections.Generic;

public class GameOverState : GameState
{
    private PlayerColor winner;
    private List<Vector2Int> winningPositions;
    private bool stateTransitionPending = false;

    public GameOverState(GameStateMachine stateMachine, PlayerColor winner, List<Vector2Int> winningPositions) : base(stateMachine)
    {
        this.winner = winner;
        this.winningPositions = winningPositions;
    }

    public override void Enter()
    {
        Debug.Log($"Game Over! Player {winner} has won!");

        // Play victory sound
        PlayEndGameSound();

        // Update score for the winner
        GameManager.Instance.board.scores[(int)winner].IncrementScore();

        // Highlight the winning positions on the board
        GameManager.Instance.board.HighlightWinningPositions(winningPositions);

        // Start a new game after a delay
        stateTransitionPending = true;
        GameManager.Instance.StartCoroutine(DelayedStateTransition());
    }

    private System.Collections.IEnumerator DelayedStateTransition()
    {
        // Allow enough time for players to see the win
        yield return new WaitForSeconds(0.2f);
        if (stateTransitionPending)
        {
            stateTransitionPending = false;
            stateMachine.ChangeState(new NewGameState(stateMachine));
        }
    }

    private void PlayEndGameSound()
    {
        AudioSource audioSource = Camera.main.GetComponent<AudioSource>();
        if (audioSource != null)
        {
            AudioClip moveSound = Resources.Load<AudioClip>("Audio/game-end");
            if (moveSound != null)
                audioSource.PlayOneShot(moveSound);
        }
    }

    public override void Exit()
    {
        stateTransitionPending = false;
    }
}
