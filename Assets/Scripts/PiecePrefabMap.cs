using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct PiecePrefabEntry
{
    public Piece.PieceType key;
    public GameObject prefab;
}


[CreateAssetMenu(fileName = "PiecePrefabMap", menuName = "Scriptable Objects/PiecePrefabMap")]
public class PiecePrefabMap : ScriptableObject
{
    public List<PiecePrefabEntry> entries = new List<PiecePrefabEntry>();
    public Dictionary<Piece.PieceType, GameObject> ToDictionary()
    {
        var d = new Dictionary<Piece.PieceType, GameObject>();
        foreach (var e in entries) if (e.prefab != null && !d.ContainsKey(e.key)) d[e.key] = e.prefab;
        return d;
    }
}
