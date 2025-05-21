using nostra.character;
using nostra.quickplay.core.Recorder;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace nostra.sarvottam.colorclash
{
    public class PlayerMoment : MonoBehaviour, ITrackable, IReconstructable
    {
        [SerializeField] GameObject SpeedpowerPartical;
        [SerializeField] GameObject debuffpowerPartical;
        [SerializeField] Transform m_playerHolder;
        [SerializeField] Billboard m_billboard;

        public bool Ai;
        public int id;
        [SerializeField] private float speed;
        [SerializeField] private float rotationSpeed = 720.0f; // Rotation speed
        [SerializeField] private float thinkingTime;
        [SerializeField] private float speedpowerupTime = 8f;

        private GameManager m_gameManager;
        private CharacterController characterController;
        private Animator anime;
        public Color LightColor, darkColor;
        public string playercolor;
        public int score;
        Vector3 movement;
        bool powerupsGetted;
        bool alreadyThinked;
        Coroutine AIThinking;
        float initialSpeed;
        private NostraCharacter m_character;
        bool isWatching = false;

        float m_animSpeed = 0f;

        public IGameObjectState CaptureState ()
        {
            isWatching = false;
            return new ColorClashPlayerState
            {
                Id = id.ToString(),
                Position = new SerializableVector3(transform.position),
                Rotation = new SerializableVector3(transform.rotation.eulerAngles),
                score = score,
                animSpeed = m_animSpeed,
                isAI = Ai
            };
        }
        public void ApplyState(IGameObjectState _state)
        {
            isWatching = true;
            if (_state is ColorClashPlayerState state)
            {
                transform.position = state.Position.ToVector3();
                transform.rotation = Quaternion.Euler(state.Rotation.ToVector3());
                score = state.score;
                m_animSpeed = state.animSpeed;
                m_gameManager.PlayCharacterAnimation(m_character, "walkBlend", m_animSpeed);
                m_gameManager.ScoreUpdate();
            }
        }
        public void OnLoaded(GameManager _gameManager)
        {
            m_gameManager = _gameManager;
            initialSpeed = speed;
            anime = GetComponent<Animator>();
            characterController = GetComponent<CharacterController>();
            if (m_billboard != null) m_billboard.SetCamera(m_gameManager.GameCamera);
        }
        public void OnFocussed()
        {
            isWatching = false;
            if (Ai)
            {
                if (AIThinking == null)
                {
                    AIThinking = StartCoroutine(AiThinkging());
                }
            }
        }
        public void OnReset()
        {
            isWatching = false;
            if (AIThinking != null)
            {
                StopCoroutine(AIThinking);
                AIThinking = null;
            }
            score = 0;
            speed = initialSpeed;
            movement = Vector3.zero;
            alreadyThinked = false;
            powerupsGetted = false;
            debuffpowerPartical.SetActive(false);
            SpeedpowerPartical.SetActive(false);
            if (m_character != null)
            {
                m_gameManager.PlayCharacterAnimation(m_character, "walkBlend", 0f);
            }
        }
        public void OnStart()
        {
            OnFocussed();
        }
        public void OnWatch()
        {
            isWatching = true;
            if (AIThinking != null)
            {
                StopCoroutine(AIThinking);
                AIThinking = null;
            }
            if (m_character != null)
            {
                m_gameManager.PlayCharacterAnimation(m_character, "walkBlend", 0.1f);
            }
        }
        public void SetCharacter(NostraCharacter _character)
        {
            m_character = _character;
            m_character.gameObject.SetActive(true);
            m_character.transform.SetParent(m_playerHolder);
            m_character.transform.localPosition = Vector3.zero;
            m_character.transform.localRotation = Quaternion.identity;
            m_character.transform.localScale = Vector3.one * 4.4f;
            if (m_character != null)
            {
                m_gameManager.PlayCharacterAnimation(m_character, "Playing", true);
                m_gameManager.PlayCharacterAnimation(m_character, "walkBlend", 0f);
            }
        }
        private void Update()
        {
            if(isWatching)
            {
                return;
            }
            if (!Ai)
            {
                movement = m_gameManager.GetJoystickDirection();
            }
            if (movement.magnitude > 0.1f)
            {
                Vector3 previousPos = transform.position;
                movement = movement.normalized;
                characterController.Move(movement * speed * Time.deltaTime);
                Quaternion toRotation = Quaternion.LookRotation(movement, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);

                if (previousPos != transform.position)
                {
                    if (m_character != null)
                    {
                        m_gameManager.PlayCharacterAnimation(m_character, "walkBlend", 0.1f);
                        m_animSpeed = 0.1f;
                    }
                }
                else
                {
                    if (m_character != null)
                    {
                        m_gameManager.PlayCharacterAnimation(m_character, "walkBlend", 0f);
                        m_animSpeed = 0f;
                    }
                }
            }
            else
            {
                if (m_character != null)
                {
                    m_gameManager.PlayCharacterAnimation(m_character, "walkBlend", 0f);
                    m_animSpeed = 0f;
                }
            }
            transform.position = new Vector3(transform.position.x, .6f, transform.position.z);
            // transform.position = new Vector3(transform.position.x, Mathf.Clamp(transform.position.y, 0, .6f), transform.position.z);
        }
        IEnumerator AiThinkging()
        {
            while (true)
            {
                yield return new WaitForSeconds(UnityEngine.Random.Range(thinkingTime, thinkingTime + 1f));
                movement = new Vector3(UnityEngine.Random.Range(-1f, 1f), 0, UnityEngine.Random.Range(-1f, 1f));
            }
        }
        public IEnumerator speedIncresed()
        {
            if(isWatching)
            {
                yield return null;
            }
            if (!powerupsGetted)
            {
                SpeedpowerPartical.SetActive(true);
                powerupsGetted = true;
                float tempspeed = speed;
                speed *= 1.5f;
                yield return new WaitForSeconds(speedpowerupTime);
                SpeedpowerPartical.SetActive(false);
                speed = tempspeed;
                powerupsGetted = false;
            }
            else
            {
                yield return new WaitForSeconds(.1f);
            }
        }
        public IEnumerator speedDecrease()
        {
            if(isWatching)
            {
                yield return null;
            }
            if (!powerupsGetted)
            {
                debuffpowerPartical.SetActive(true);
                powerupsGetted = true;
                float tempspeed = speed;
                speed /= 1.5f;
                yield return new WaitForSeconds(speedpowerupTime);
                speed = tempspeed;
                powerupsGetted = false;
                debuffpowerPartical.SetActive(false);
            }
            else
            {
                yield return new WaitForSeconds(.1f);
            }
        }
        IEnumerator thinkDelay()
        {
            if(isWatching)
            {
                yield return null;
            }
            yield return new WaitForSeconds(1);
            alreadyThinked = false;
        }
        private void OnControllerColliderHit(ControllerColliderHit collision)
        {
        }
        private void OnTriggerEnter(Collider collision)
        {
            if(isWatching || m_gameManager.GameOver == true)
            {
                return;
            }
            if (collision.gameObject.GetComponent<PlayerMoment>())
            {
                if (alreadyThinked && AIThinking != null)
                {
                    alreadyThinked = true;
                    StartCoroutine(thinkDelay());
                    StopCoroutine(AIThinking);
                    AIThinking = StartCoroutine(AiThinkging());
                }
                return;
            }

            if (collision.transform.TryGetComponent<PowerUps>(out PowerUps power))
            {
                power.collectedpayer = this;
                power.Blast();
            }

            if (collision.transform.TryGetComponent<Tiles>(out Tiles tile))
            {
                if (tile.Obstacle)
                {
                    if (alreadyThinked && AIThinking != null)
                    {
                        alreadyThinked = true;
                        StartCoroutine(thinkDelay());
                        StopCoroutine(AIThinking);
                        AIThinking = StartCoroutine(AiThinkging());
                    }
                    return;
                }

                if (tile.Owner)
                {
                    if (tile.Owner != this)
                    {
                        if (tile.ShieldOn)
                            return;

                        tile.Owner.score--;
                        tile.Owner = this;
                        if (tile.darktile)
                        {
                            tile.TileMaterial.material.color = darkColor;
                        }
                        else
                        {
                            tile.TileMaterial.material.color = LightColor;
                        }
                        score++;
                        if (!Ai && tile.Audio_source != null)
                        {
                            tile.Audio_source.volume = 1f;
                            tile.Audio_source.Play();
                        }
                    }
                }
                else
                {
                    tile.Owner = this;
                    if (tile.darktile)
                    {
                        tile.TileMaterial.material.color = darkColor;
                    }
                    else
                    {
                        tile.TileMaterial.material.color = LightColor;
                    }
                    score++;
                    if (!Ai && tile.Audio_source != null)
                    {
                        tile.Audio_source.volume = 1f;
                        tile.Audio_source.Play();
                    }
                }
            }
            m_gameManager.ScoreUpdate();
        }
    }

    [Serializable]
    public class ColorClashPlayerState : IGameObjectState
    {
        public string Id;
        public SerializableVector3 Position;
        public SerializableVector3 Rotation;
        public int score;
        public float animSpeed;
        public bool isAI;

        string IGameObjectState.Id => Id;
        bool IGameObjectState.canCapture => true;
    }
}
