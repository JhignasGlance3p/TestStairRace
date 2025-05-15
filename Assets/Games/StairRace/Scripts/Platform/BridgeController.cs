using UnityEngine;

namespace nostra.pkpl.stairrace
{
    public class BridgeController : MonoBehaviour
    {
        [SerializeField] GameObject blocker;
        [SerializeField] Transform stairParent;

        StairRaceManager m_gameManager;
        int m_bridgeIndex;
        int m_platformIndex;
        Stair[] stairs;
        Transform playerTransform;
        int currentBlockedIndex;
        SRBridgeProgress bridgeProgress;

        public Stair[] StairsOnBridge => stairs;
        public bool IsBridgeOccupied { get; private set; }
        public int BridgeIndex => m_bridgeIndex;

        public void OnLoaded(StairRaceManager _gameManager, int _bridgeIndex, int _platformIndex)
        {
            m_gameManager = _gameManager;
            m_bridgeIndex = _bridgeIndex;
            m_platformIndex = _platformIndex;
            stairs = stairParent.GetComponentsInChildren<Stair>();
            ConfigureStair();
        }
        public void Reset()
        {
            bridgeProgress = null;
            IsBridgeOccupied = false;
            currentBlockedIndex = 0;
            blocker.SetActive(true);
            blocker.transform.position = stairs[currentBlockedIndex].transform.position;
            for (int i = 0; i < stairs.Length; i++)
            {
                stairs[i].Reset();
            }
        }
        public void SetProgress(SRBridgeProgress _bridgeProgress)
        {
            bridgeProgress = _bridgeProgress;
            foreach (SRStairProgress stairProgress in _bridgeProgress.stairProgress)
            {
                if (stairProgress.stairIndex < stairs.Length)
                    stairs[stairProgress.stairIndex].SetProgress(stairProgress);
            }
            IsBridgeOccupied = _bridgeProgress.isOccupied;
            currentBlockedIndex = bridgeProgress.currentBlockedIndex;
            if (currentBlockedIndex + 1 < stairs.Length)
            {
                blocker.transform.position = stairs[currentBlockedIndex + 1].transform.position;
            }
        }
        public void ClaimBridge(ColorName color)
        {
            IsBridgeOccupied = true;
            if (bridgeProgress != null)
            {
                bridgeProgress.isOccupied = IsBridgeOccupied;
            }
        }
        private void ConfigureStair()
        {
            IsBridgeOccupied = false;
            currentBlockedIndex = 0;
            blocker.transform.position = stairs[currentBlockedIndex].transform.position;
            for (int i = 0; i < stairs.Length; i++)
            {
                stairs[i].OnLoaded(m_gameManager, i, (m_platformIndex + "_" + m_bridgeIndex), PlayerEnteredStair);
            }
        }
        private void PlayerEnteredStair(IPlayer _player, int _stairIndex)
        {
            if(_player.CollectedStairCount > 0)
            {
                _player.PlacedBrickOnStair(this, m_bridgeIndex, _stairIndex, (_stairIndex == 0), (_stairIndex >= stairs.Length - 1));
                if (_player.IsPlayer)
                {
                    if (_stairIndex >= stairs.Length - 1)
                    {
                        blocker.SetActive(false);
                        return;
                    }
                    var currentBlocked = _stairIndex;
                    for (int i = currentBlocked; i < stairs.Length; i++)
                    {
                        if (stairs[i].Color != _player.Color)
                        {
                            currentBlocked = i;
                            blocker.SetActive(true);
                            break;
                        }

                        if (i == stairs.Length - 1)
                        {
                            currentBlocked = stairs.Length - 1;
                            blocker.SetActive(false);
                        }
                    }

                    currentBlockedIndex = currentBlocked;
                    blocker.transform.position = _player.transform.position.z > stairs[currentBlockedIndex].transform.position.z-0.1f ?
                        _player.transform.position+ new Vector3(0f,0f,0.1f) : stairs[currentBlockedIndex].transform.position;
                }
            }
        }
    }
}