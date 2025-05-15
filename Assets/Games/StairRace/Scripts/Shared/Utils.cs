using UnityEngine;

namespace nostra.pkpl.stairrace
{
    public enum ColorName
    {
        Red = 0,
        Blue = 1,
        Yellow = 2,
        Violet = 3,
        Grey = 4,
        None = 5
    }
    public enum WriteGameData
    {
        None,
        ZoneInfo,
        GameProgress,
    }
    public enum State
    {
        Idle = 0,
        Collecting = 1,
        Building = 2,
        MovingToNextPlatform = 3,
        MoveToFinalEntrance = 4
    }
    [System.Serializable]
    public class StairBlock
    {
        public CollectibleBlock Prefab;
        public ColorName Color;
        public Material Material;
    }
    [System.Serializable]
    public class StairBlockSpawnArea
    {
        public Transform Center;
        public Vector2 Size;
    }
}