using nostra.quickplay.core.Recorder;
using System;
using UnityEngine;

namespace nostra.pkpl.stairrace
{
    public class Stair : MonoBehaviour, ITrackable, IReconstructable
    {
        private StairRaceManager m_gameManager;
        private int stairIndex;
        private string replayPrefix;
        private Action<IPlayer, int> onPlayerEnterCB;
        private MeshRenderer mesh;
        private ColorName currentColor = ColorName.None;
        private SRStairProgress stairProgress;
        bool isWatching = false;

        public ColorName Color => currentColor;
        public int StairIndex => stairIndex;
        public string ReplayPrefix => replayPrefix;

        SRStairData m_stairData;

        public IGameObjectState CaptureState ()
        {
            isWatching = false;
            if(m_stairData == null)
            {
                m_stairData = new SRStairData();
                m_stairData.Id = (replayPrefix + "_" + stairIndex);
                m_stairData.color = currentColor;
                m_stairData.canCapture = true;
            }
            else
            {
                if(m_stairData.color != currentColor)
                {
                    m_stairData.color = currentColor;
                    m_stairData.canCapture = true;
                }
                else
                {
                    m_stairData.canCapture = false;
                }
                m_stairData.color = currentColor;
            }
            return m_stairData;
        }
        public void ApplyState(IGameObjectState _state)
        {
            isWatching = true;
            if (_state is SRStairData stairData)
            {
                currentColor = stairData.color;
                mesh.material = m_gameManager.GetMaterial(currentColor);
            }
        }

        void OnTriggerEnter(Collider _collision)
        {
            if(isWatching)
            {
                return;
            }
            if (_collision.gameObject.TryGetComponent<IPlayer>(out IPlayer controller))
            {
                if (currentColor == controller.Color)
                {
                    return;
                }

                if (controller.CollectedStairCount > 0)
                {
                    SetStair(controller);
                }
            }
        }

        public void OnLoaded(StairRaceManager _gameManager, int _index, string _replayPrefix, Action<IPlayer, int> _callback)
        {
            m_gameManager = _gameManager;
            stairIndex = _index;
            replayPrefix = _replayPrefix;
            onPlayerEnterCB = _callback;
            mesh = GetComponent<MeshRenderer>();
            m_gameManager.RegisterStair(this);
            isWatching = false;
        }
        public void Reset()
        {
            stairProgress = null;
            currentColor = ColorName.None;
            mesh.material = m_gameManager.GetMaterial(currentColor);
            isWatching = false;
            m_stairData = null;
        }
        public void SetProgress(SRStairProgress _stairProgress)
        {
            stairProgress = _stairProgress;
            currentColor = _stairProgress.color;
            mesh.material = m_gameManager.GetMaterial(currentColor);
        }

        private void SetStair(IPlayer _controller)
        {
            if (currentColor != _controller.Color)
            {
                currentColor = _controller.Color;
                mesh.material = m_gameManager.GetMaterial(currentColor);
                onPlayerEnterCB?.Invoke(_controller, stairIndex);
                if (stairProgress != null)
                {
                    stairProgress.color = currentColor;
                }
            }
        }
    }
}