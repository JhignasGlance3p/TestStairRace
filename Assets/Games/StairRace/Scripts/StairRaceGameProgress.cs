using nostra.quickplay.core.Recorder;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace nostra.pkpl.stairrace
{
    public class StairRaceGameProgress
    {
        public int currentFrame;
        public BaseProgress gameProgress;
    }
    [Serializable]
    public class BaseProgress { }
    [Serializable]
    public class SRFirstFrame : BaseProgress
    {
        public List<SRPlatformData> platformData;
    }
    [Serializable]
    public class SRPlatformData
    {
        public int[] randomColors;
        public List<SRPowerupData> powerups;
    }
    [Serializable]
    public class SRPowerupData
    {
        public int operation;
        public int randomValue;
    }
    [Serializable]
    public class SRUpdateProgress : BaseProgress
    {
        public bool isGameOver = false;
        public List<SRPlatformProgress> platformProgress = new();
        public List<SRPlayerProgress> playerProgress = new();
    }
    [Serializable]
    public class SRPlatformProgress
    {
        public int platformIndex;
        public List<SRBridgeProgress> bridgeProgress = new();
    }
    [Serializable]
    public class SRBridgeProgress
    {
        public int bridgeIndex;
        public bool isOccupied;
        public int currentBlockedIndex;
        public List<SRStairProgress> stairProgress = new();
    }
    [Serializable]
    public class SRStairProgress
    {
        public int stairIndex;
        public ColorName color;
    }
    [Serializable]
    public class SRPlayerProgress
    {
        public ColorName color;
        public int blockInHand;
        public int currentPlatformIndex;
        public int currentBridgeIndex = -1;
        public int currentStairIndex = -1;
        public int playerIndex;
        public SerializableVector3 currentPosition;
        public SerializableVector3 curentRotation;
        public State currentState;
        public List<SRBlockProgress> collectedBlocks = new();
    }
    [Serializable]
    public class SRBlockProgress
    {
        public int platformIndex;
        public int blockIndex;
        public bool isPowerupBlock;
    }
}