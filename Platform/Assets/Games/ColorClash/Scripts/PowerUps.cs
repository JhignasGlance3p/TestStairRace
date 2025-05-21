using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace nostra.sarvottam.colorclash
{
    public class PowerUps : MonoBehaviour
    {
        [SerializeField] private PowerUpsType type;
        [SerializeField] private GameObject m_particleEffect;
        [SerializeField] GameManager m_gameManager;
        [SerializeField] private GameObject m_powerupGO;
        [SerializeField] private CapsuleCollider m_collider;

        public PlayerMoment collectedpayer;
        public int id;

        private BoxCollider captureSaroundingarea;
        private bool collected;

        public void OnLoaded()
        {
            captureSaroundingarea = GetComponent<BoxCollider>();
        }

        public void Blast()
        {
            if (collected)
            {
                return;
            }

            collected = true;
            
            // var powerUpData = new Dictionary<string, object>
            // {
            //     {"powerUpType", type.ToString()},
            //     {"collectorId", collectedpayer.id.ToString()}
            // };
            // m_gameManager.WriteAction("PowerUpActivate", transform.position, collectedpayer.id, powerUpData);

            GetComponent<AudioSource>().volume = 1f;
            GetComponent<AudioSource>().Play();
            StartCoroutine(Destroying());
            if (type == PowerUpsType.Speedboost)
            {
                // m_gameManager.WriteAction("SpeedBoost", collectedpayer.transform.position, 
                //     collectedpayer.id, 
                //     new Dictionary<string, object> { {"effect", "speedBoost"} });
                collectedpayer?.StartCoroutine("speedIncresed");
            }
            else if (type == PowerUpsType.Freezetime)
            {
                foreach (var item in m_gameManager.allPlayers)
                {
                    if (item != collectedpayer)
                    {
                        // m_gameManager.WriteAction("Freeze", item.transform.position, 
                        //     item.id, 
                        //     new Dictionary<string, object> { {"effect", "freeze"} });
                        item.StartCoroutine("speedDecrease");
                    }
                }
            }
            if (captureSaroundingarea)
            {
                captureSaroundingarea.enabled = true;
            }
        }
        IEnumerator Destroying()
        {
            m_powerupGO.SetActive(false);
            m_collider.enabled = false;
            m_particleEffect.SetActive(true);

            yield return new WaitForSeconds(1.5f);

            m_particleEffect.SetActive(false);
            this.gameObject.SetActive(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent<Tiles>(out Tiles tiles))
            {
                if (tiles.Obstacle)
                    return;

                if (type == PowerUpsType.Shield)
                {
                    if (tiles.Owner == collectedpayer)
                    {
                        // m_gameManager.WriteAction("ShieldActivate", tiles.transform.position, 
                        //     collectedpayer.id, 
                        //     new Dictionary<string, object> { 
                        //         {"tileId", tiles.id.ToString()}
                        //     });
                        tiles.OnShieldActive();
                    }
                }
                else if (type == PowerUpsType.Colorbomb)
                {
                    // var bombData = new Dictionary<string, object> {
                    //     {"tileId", tiles.id.ToString()},
                    //     {"isDarkTile", tiles.darktile.ToString()}
                    // };

                    if (tiles.Owner)
                    {
                        if (tiles.Owner != collectedpayer)
                        {
                            if (tiles.ShieldOn)
                                return;

                            // m_gameManager.WriteAction("ColorBombCapture", tiles.transform.position, 
                            //     collectedpayer.id, bombData);
                            tiles.Owner.score--;
                            collectedpayer.score++;
                            tiles.Owner = collectedpayer;
                            if (tiles.darktile)
                            {
                                tiles.TileMaterial.material.color = collectedpayer.darkColor;
                            }
                            else
                            {
                                tiles.TileMaterial.material.color = collectedpayer.LightColor;
                            }
                            if (tiles.BlastParticle != null) tiles.BlastParticle.SetActive(false);
                            if (tiles.BlastParticle != null) tiles.BlastParticle.SetActive(true);
                        }
                    }
                    else
                    {
                        // m_gameManager.WriteAction("ColorBombCaptureEmpty", tiles.transform.position, 
                        //     collectedpayer.id, bombData);
                        tiles.Owner = collectedpayer;
                        collectedpayer.score++;
                        if (tiles.darktile)
                        {
                            tiles.TileMaterial.material.color = collectedpayer.darkColor;
                        }
                        else
                        {
                            tiles.TileMaterial.material.color = collectedpayer.LightColor;
                        }
                    }
                }
            }
        }
    }
    public enum PowerUpsType
    {
        Speedboost,
        Colorbomb,
        Shield,
        Freezetime,
    }
}
