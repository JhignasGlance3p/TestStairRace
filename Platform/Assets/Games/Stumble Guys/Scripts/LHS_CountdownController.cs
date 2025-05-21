using nostra.origami.common;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using System.Threading.Tasks;

namespace nostra.origami.stumble
{
    public class LHS_CountdownController : MonoBehaviour
    {
        private const int COUNT_DOWN_TIME = 4;

        [SerializeField] StumbleGuyController stumbleGuyController;
        [SerializeField] ObjectPooler pooler;
        [SerializeField] LHS_MainPlayer m_player;
        [SerializeField] RoundSystem roundSystem;
        [SerializeField] UIManager UI;
        [SerializeField] GameObject Opponents;
        [SerializeField] GameObject Num_A;
        [SerializeField] GameObject Num_B;
        [SerializeField] GameObject Num_C;
        [SerializeField] GameObject Num_GO;
        [SerializeField] GameObject anim;

        int countdownTime = 4;
        AINavMesh[] AIPlayers;
        public Animator camAnim;

        Animator animator;

        [SerializeField]  AudioSource mysfx;
        [SerializeField]  AudioClip startsfx;
        [SerializeField]  AudioClip gosfx;

        [SerializeField] private bool directStart;
        public Transform tutorialPanel;
        [SerializeField] private Animator PlayerAnimator;
        [SerializeField] private LHS_MainPlayer PlayerScript;
        [SerializeField] private GameObject[] AIPrefabGO;
        [SerializeField] private Camera mainCamera;

        GameObject playerGO;
        Vector3 PlayerInitPosition;
        public bool PlayerDemoRun = false;
        bool canResume = true;
        int animatorIndex = 0;

        public void OnLoaded()
        {
            pooler.OnLoaded();
            animator = anim.GetComponent<Animator>();
            PlayerInitPosition = m_player.transform.position;
            m_player.OnLoaded();
            AIPlayers = new AINavMesh[Opponents.transform.childCount];
            for (int i = 0; i < Opponents.transform.childCount; i++)
            {
                AIPlayers[i] = Opponents.transform.GetChild(i).gameObject.GetComponent<AINavMesh>();
                AIPlayers[i].OnLoaded();
            }
            Num_A.SetActive(false);
            Num_B.SetActive(false);
            Num_C.SetActive(false);
            Num_GO.SetActive(false);
            roundSystem.OnLoaded();
            UI.OnLoaded();
        }
        public void OnFocussed()
        {
            m_player.OnFocus(stumbleGuyController.GetCharacter(0).gameObject);
            int i = 1;
            foreach(AINavMesh ai in AIPlayers)
            {
                ai.OnFocus(stumbleGuyController.GetCharacter(i).gameObject);
                i++;
                ai.OnAutoPlay();
            }
            m_player.OnAutoPlay();
            PlayerDemoRun = true;
        }

        void reset()
        {
            countdownTime = 4;
            mainCamera.gameObject.transform.position = new Vector3(0, 5.5f, 20);
            mainCamera.gameObject.transform.eulerAngles = Vector3.zero;
            Num_A.SetActive(false);
            Num_B.SetActive(false);
            Num_C.SetActive(false);
            Num_GO.SetActive(false);
            Num_A.transform.localScale = new Vector3(0.45f, 0.45f, 0.45f);
            Num_B.transform.localScale = new Vector3(0.45f, 0.45f, 0.45f);
            Num_C.transform.localScale = new Vector3(0.45f, 0.45f, 0.45f);
            Num_GO.transform.localScale = new Vector3(0.45f, 0.45f, 0.45f);
            animator.SetBool("Num3", false);
            animator.Play("Numc");
            PlayerScript.canMove = false;
            m_player.gameObject.SetActive(false);
            m_player.transform.parent = null;
            m_player.transform.parent = (Opponents.transform.parent).transform;
            m_player.gameObject.SetActive(true);
            m_player.transform.position = PlayerInitPosition;
            m_player.transform.eulerAngles = Vector3.zero;

            playerGO.transform.parent = m_player.transform;
            playerGO.transform.localPosition = Vector3.zero;

            for (int i = 0; i < AIPlayers.Length; i++)
            {
                AIPlayers[i].gameObject.SetActive(false);
            }
            for (int i = 0; i < AIPlayers.Length; i++)
            {
                AIPlayers[i].transform.parent = null;
                AIPlayers[i].transform.position = Vector3.zero;
                AIPlayers[i].transform.parent = Opponents.transform;
                AIPlayers[i].gameObject.SetActive(true);
                AIPlayers[i].isAgentStopped(true);
                AIPlayers[i].transform.localEulerAngles = Vector3.zero;
            }
            StartCoroutine(CountdownToStart());
        }
        void onAwake()
        {
            animator = anim.GetComponent<Animator>();
            //StartCoroutine ( CountdownToStart ( ) );
            camAnim.GetComponent<LHS_Camera>().enabled = true;
            Num_A.SetActive(false);
            Num_B.SetActive(false);
            Num_C.SetActive(false);
            Num_GO.SetActive(false);

            //playerGO.transform.GetChild ( 0 ).gameObject.transform.localScale = Vector3.one * 100;
            if (PlayerScript.anim == null)
            {
                Animator RC = playerGO.transform.gameObject.GetComponent<Animator>();
                //RC.runtimeAnimatorController = PlayerAnimator as RuntimeAnimatorController;
                PlayerScript.anim = RC;
            }
        }
        IEnumerator CountdownToStart()
        {
            stopAiAgent();
            while (countdownTime > 0)
            {
                ChangeImage();
                yield return new WaitForSecondsRealtime(1f);
                countdownTime--;
            }
            //Num_GO.SetActive(false);
            // Time.timeScale = 1;
            yield return new WaitForSecondsRealtime(1f);
            InitOpponent();
        }

        void ChangeImage()
        {
            int i = countdownTime;
            if (i == 4)
            {
                Num_C.SetActive(true);
                animator.SetBool("Num3", true);
                mysfx.PlayOneShot(startsfx);
            }

            if (i == 3)
            {
                //Num_C.SetActive(false);
                Num_B.SetActive(true);
                //animator.SetBool("Num3", true);
                mysfx.PlayOneShot(startsfx);
            }

            if (i == 2)
            {
                //Num_B.SetActive(false);
                Num_A.SetActive(true);
                //animator.SetBool("Num3", true);
                mysfx.PlayOneShot(startsfx);
            }

            if (i == 1)
            {
                //Num_A.SetActive(false);
                Num_GO.SetActive(true);
                //animator.SetBool("Num3", true);
                mysfx.PlayOneShot(gosfx);
            }
        }

        void InitOpponent()
        {
            //playerMesh = playerRandomMesh[0];
            if (PlayerScript.anim != null)
            {
                PlayerScript.canMove = true;
            }
            /*if (PlayerDemoRun)
            {
                for (int i = 0; i < AIPlayers.Length; i++)
                {
                    AIPlayers[i].gameObject.SetActive(true);
                    AIPlayers[i].isAgentStopped(false);
                }
            }*/
        }
        void stopAiAgent()
        {
            /*for ( int i = 0; i < Opponents.transform.childCount; i++ )
            {
                if ( Opponents.transform.GetChild ( i ).gameObject.activeSelf )
                {
                    Opponents.transform.GetChild ( i ).gameObject.GetComponent<AINavMesh> ( ).isAgentStopped ( true );
                }
            }*/
            for (int i = 0; i < AIPlayers.Length; i++)
            {
                AIPlayers[i].gameObject.SetActive(false);
            }
            for (int i = 0; i < AIPlayers.Length; i++)
            {
                AIPlayers[i].gameObject.SetActive(true);
                AIPlayers[i].isAgentStopped(true);
            }
        }
        public void StartGame()
        {
            PlayerDemoRun = false;
            if (canResume)
            {
                PlayerScript.canMove = false;
                PlayerScript.Rigid.isKinematic = false;
                PlayerScript.resetMoveVec();
                m_player.GetComponent<NavMeshAgent>().enabled = false;
                if (PlayerScript.anim != null)
                {
                    PlayerScript.anim.Play("Idle");
                    PlayerScript.anim.SetBool("isMove", false);
                }

                onAwake();
                reset();
                canResume = false;
            }
            else
            {
                camAnim.GetComponent<LHS_Camera>().enabled = true;
                if (PlayerScript.anim != null)
                {
                    PlayerScript.canMove = true;
                }
            }
            /*StartCoroutine(CountdownToStart());
            RoundSystemScript.Reset();
            InitOpponent();*/
        }
        public void _DestinationLoop()
        {
            /*PlayerScript.canMove = false;
            PlayerScript.Rigid.isKinematic = false;
            PlayerScript.resetMoveVec();

            //m_player.gameObject.GetComponent<NavMeshAgent>().enabled = false;

            if (PlayerScript.anim != null)
            {
                PlayerScript.anim.Play("Idle");
                PlayerScript.anim.SetBool("isMove", false);
            }
            onAwake();*/
            m_player.transform.position = PlayerInitPosition;
            m_player.transform.eulerAngles = Vector3.zero;
            reset();
        }
        public void ToggleAI(bool isInFocus)
        {
            if (isInFocus)
            {
                camAnim.GetComponent<LHS_Camera>().enabled = false;
                PlayerDemoRun = true;
                //playerGO.transform.GetChild ( 0 ).gameObject.transform.localScale = Vector3.one * 100;
                // for (int i = 0; i < AIPlayers.Length; i++)
                // {
                //     AIPlayers[i].onStart();
                // }

                if (PlayerScript.anim == null)
                {
                    Animator RC = playerGO.transform.gameObject.GetComponent<Animator>();
                    //RC.runtimeAnimatorController = PlayerAnimator as RuntimeAnimatorController;
                    PlayerScript.anim = RC;
                }
                PlayerScript.canMove = true;
                //PlayerScript.agent.isStopped = false;
                InitOpponent();
            }
            else
            {
                PlayerDemoRun = false;
                stopAiAgent();
            }
        }
    }
}