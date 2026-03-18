
namespace Piece
{
    public enum PieceType
    {
        D,
        B,
        I,
        L,
        InvertL,
        T, Plus
    }

    public static class PieceTypes
    {
        public static PieceType GetRandomPieceType()
        {
            var values = System.Enum.GetValues(typeof(PieceType));
            return (PieceType)values.GetValue(UnityEngine.Random.Range(0, values.Length));
        }
    }
}
