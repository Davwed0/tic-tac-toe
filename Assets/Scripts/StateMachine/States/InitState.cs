using UnityEngine;

public class InitState : GameState
{
    public InitState(GameStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        Debug.Log("Entering Init State");
        GameManager.Instance.board.InitializeBoard();
        GameManager.Instance.board.InitializePieces();
        GameManager.Instance.board.InitializeSlots();
        GameManager.Instance.board.InitializeScores();

        GameManager.Instance.board.GenerateNewPieces();

        AudioSource audioSource = Camera.main.GetComponent<AudioSource>();
        if (audioSource != null)
        {
            AudioClip moveSound = Resources.Load<AudioClip>("Audio/game-start");
            if (moveSound != null)
                audioSource.PlayOneShot(moveSound);
        }

        // Transition to PlayerTurnState after initialization
        stateMachine.ChangeState(new PlayerTurnState(stateMachine, PlayerColor.WHITE));
    }
}
