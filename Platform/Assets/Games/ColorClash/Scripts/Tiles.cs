using nostra.quickplay.core.Recorder;
using System.Collections;
using UnityEngine;

namespace nostra.sarvottam.colorclash
{
    public class Tiles : MonoBehaviour, ITrackable, IReconstructable
    {
        [SerializeField] bool isObstacle;
        [SerializeField] bool isDarkTile;
        [SerializeField] MeshRenderer tileRenderer;
        [SerializeField] GameObject shieldParticle;
        [SerializeField] GameObject blastParticle;

        public bool Obstacle => isObstacle;
        public MeshRenderer TileMaterial => tileRenderer;
        public GameObject BlastParticle => blastParticle;
        public bool darktile => isDarkTile;
        public PlayerMoment Owner;

        public bool ShieldOn { private set; get; }
        public AudioSource Audio_source { private set; get; }

        Coroutine ShieldCoroutine;

        public int id;
        TilesState state;

        public IGameObjectState CaptureState ()
        {
            if(state == null)
            {
                state = new TilesState();
                state.id = id;
                state.color = ColorUtility.ToHtmlStringRGBA(TileMaterial.material.color);
                state.canCapture = true;
            }
            else
            {
                if(state.color == ColorUtility.ToHtmlStringRGBA(TileMaterial.material.color))
                {
                    state.canCapture = false;
                }
                else
                {
                    state.color = ColorUtility.ToHtmlStringRGBA(TileMaterial.material.color);
                    state.canCapture = true;
                }
            }
            return state;
        }
        public void ApplyState(IGameObjectState _state)
        {
            TilesState state = _state as TilesState;
            Color color;
            if (ColorUtility.TryParseHtmlString("#" + state.color, out color))
            {
                TileMaterial.material.color = color;
            }
            else
            {
                TileMaterial.material.color = new Color(0.8f, 0.8f, 0.8f, 0.8f);
            }
        }

        public void OnLoaded()
        {
            Audio_source = GetComponent<AudioSource>();
            transform.Rotate(-90, 0, 0);
        }
        public void OnReset()
        {
            ShieldOn = false;
            Owner = null;
            if (shieldParticle != null) shieldParticle.gameObject.SetActive(false);
            if (BlastParticle != null) BlastParticle.gameObject.SetActive(false);
            if (ShieldCoroutine != null)
            {
                StopCoroutine(ShieldCoroutine);
                ShieldCoroutine = null;
            }
            if(TileMaterial != null)
            {
                if(isDarkTile)
                {
                    TileMaterial.material.color = new Color(0.6f, 0.6f, 0.6f, 1);
                }
                else
                {
                    TileMaterial.material.color = new Color(0.8f, 0.8f, 0.8f, 0.8f);
                }
            }
        }
        public void SetTileColor(Color _color)
        {
            if (TileMaterial != null)
            {
                TileMaterial.material.color = _color;
            }
        }
        public void OnShieldActive()
        {
            ShieldCoroutine = StartCoroutine(ShieldAcivated());
        }

        IEnumerator ShieldAcivated()
        {
            ShieldOn = true;
            if(shieldParticle != null) shieldParticle.gameObject.SetActive(true);

            yield return new WaitForSeconds(8);

            ShieldOn = false;
            if(shieldParticle != null) shieldParticle.gameObject.SetActive(false);
        }
    }
    [System.Serializable]
    public class TilesState : IGameObjectState
    {
        public int id;
        public string color = "0xffffff";
        public bool canCapture;

        string IGameObjectState.Id => id.ToString();
        bool IGameObjectState.canCapture => canCapture;
    }
}