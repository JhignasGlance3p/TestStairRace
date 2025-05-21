using System.Collections;
using UnityEngine;
using nostra.origami.common;
using nostra.origami.stumble;

namespace nostra.origami.stumble
{
    public class BounceWall : MonoBehaviour
    {
        [SerializeField] string playerTag;
        [SerializeField] float bounceForce;
        [SerializeField] LHS_CountdownController gameControllerScript;

        private void OnCollisionEnter(Collision collision)
        {
            Debug.Log($"[BounceWall] OnCollisionEnter: {collision.gameObject.name}, {this.gameObject.name}");
            if (collision.transform.tag == playerTag)
            {
                if (collision.gameObject.GetComponent<LHS_MainPlayer>() != null)
                {
                    LHS_MainPlayer MainPlayer = collision.gameObject.GetComponent<LHS_MainPlayer>();
                    if (!MainPlayer.cannotMove)
                    {
                        MainPlayer.anim.SetBool("fallBack", true);
                        MainPlayer.cannotMove = true;
                        CollisionSound();
                        Rigidbody otherRB = collision.rigidbody;
                        otherRB.linearVelocity = new Vector3(0, 0, 0);
                        otherRB.AddForce(Vector3.back * (bounceForce + 5f), ForceMode.Impulse);
                        MyUtils.Execute(3f, () =>
                        {
                            MainPlayer.anim.SetBool("isMove", false);
                            MainPlayer.anim.SetBool("fallBack", false);
                            MainPlayer.cannotMove = false;
                        });
                    }
                }
                else if (collision.gameObject.GetComponent<AINavMesh>() != null)
                {
                    AINavMesh AIPlayer = collision.gameObject.GetComponent<AINavMesh>();
                    if (!AIPlayer.agent.isStopped)
                    {
                        AIPlayer.animator.SetBool("fallBack", true);
                        AIPlayer.agent.isStopped = true;
                        CollisionSound();
                        Rigidbody otherRB = collision.rigidbody;
                        AIPlayer.rigid.linearVelocity = Vector3.zero;
                        if (!AIPlayer.transform.parent.name.Contains("Platform Circle Rotated"))
                        {
                            AIPlayer.rigid.AddForce(Vector3.back * bounceForce, ForceMode.Impulse);
                        }
                        MyUtils.Execute(0.5f, () =>
                        {
                            if (!AIPlayer.transform.parent.name.Contains("Platform Circle Rotated"))
                            {
                                AIPlayer.rigid.isKinematic = true;
                            }
                            MyUtils.Execute(3f, () =>
                            {
                                AIPlayer.animator.SetBool("fallBack", false);
                                MyUtils.Execute(0.5f, () =>
                                {
                                    if (gameControllerScript != null && gameControllerScript.PlayerDemoRun)
                                    {
                                        AIPlayer.agent.isStopped = false;
                                    }
                                    AIPlayer.rigid.isKinematic = false;
                                });
                            });
                        });
                    }
                }
            }
        }
        void CollisionSound()
        {
            AudioSource colsound = GetComponent<AudioSource>();
            if (colsound != null)
            {
                colsound.Play();
            }
        }
    }
}