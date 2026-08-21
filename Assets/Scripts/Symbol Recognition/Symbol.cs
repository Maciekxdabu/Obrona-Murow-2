using UnityEngine;

namespace SymbolRecognition
{
    [CreateAssetMenu(fileName = "Spell", menuName = "Scriptable Objects/Spell Symbol", order = 1)]
    public class Symbol : ScriptableObject
    {
        public string symbolName;
        public SymbolLine[] lines;
    }

    [System.Serializable]
    public struct SymbolLine
    {
        public Direction direction;
        public float length;
    }

    public enum Direction
    {
        UL = 0,
        LEFT = 1,
        DL = 2,
        DOWN = 3,
        DR = 4,
        RIGHT = 5,
        UR = 6,
        UP = 7
    }
}