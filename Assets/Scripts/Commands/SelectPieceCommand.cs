public class SelectPieceCommand : ICommand
{
    private ChessPiece piece;
    private ChessPiece previousSelection;

    public SelectPieceCommand(ChessPiece piece)
    {
        this.piece = piece;
    }

    public void Execute()
    {
        previousSelection = GameManager.Instance.selectedPiece;
        GameManager.Instance.selectedPiece = piece;
    }

    public void Undo()
    {
        GameManager.Instance.selectedPiece = previousSelection;
    }
}
