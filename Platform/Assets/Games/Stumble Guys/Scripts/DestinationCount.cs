using UnityEngine;
using nostra.origami.common;

namespace nostra.origami.stumble
{
    public class DestinationCount : MonoBehaviour
    {
        [SerializeField] private UIManager uIManager;

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"[DestinationCount] OnTriggerEnter: {other.gameObject.name}, {this.gameObject.name}");
            if (other.CompareTag("Player") && uIManager.CurRank < uIManager.Qualitfication)
            {
                if (uIManager.gameControllerScript.PlayerDemoRun)
                {
                    if (other.GetComponent<LHS_MainPlayer>() != null)
                    {
                        uIManager.gameControllerScript._DestinationLoop();
                    }
                }
                else
                {
                    uIManager.CurRank++;
                    if (uIManager.CurRank == uIManager.Qualitfication)
                    {
                        uIManager.CheckResult = true;
                    }
                    if (other.GetComponent<LHS_MainPlayer>() != null)
                    {
                        uIManager.success.SetActive(true);

                        uIManager.PlayerRank = uIManager.CurRank - 1;
                        uIManager.names.Insert(uIManager.PlayerRank, "YOU");
                        AINavMesh[] ais = FindObjectsOfType<AINavMesh>();
                        foreach (AINavMesh ai in ais)
                        {
                            ai.agent.speed = 16f;
                        }

                        MyUtils.Execute(4f, () =>
                        {
                            uIManager.success.SetActive(false);
                            if (uIManager.CurRank != uIManager.Qualitfication)
                            {
                                uIManager.camAnim.GetComponent<LHS_Camera>().enabled = false;
                                uIManager.camAnim.SetBool("EndCamPan", true);
                            }

                            /*MyUtils.Execute(10f, () =>
                            {
                                uIManager.camAnim.SetBool("EndCamPan", false);
                                uIManager.camAnim.GetComponent<LHS_Camera>().enabled = true;
                            });*/
                            /*MyUtils.Execute(1f, () =>
                            {

                            });*/
                        });
                    }
                    /*ICharacter character = other.GetComponent<ICharacter>();
                    character.Deactive();*/
                }
            }
        }
    }
}