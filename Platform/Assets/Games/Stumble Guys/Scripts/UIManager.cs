using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace nostra.origami.stumble
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] RoundSystem roundSystem;
        [SerializeField] LHS_Particle completionParticle;
        
        int min;
        float sec;
        public GameObject roundOver;
        public GameObject success;
        public GameObject failure;
        public GameObject player;
        public GameObject destPos;
        public GameObject boxTriggerPoint;
        public GameObject Joystick, JumpBtn;
        int curRank = 0;
        float waitTime = 2f;
        float curretTime = 0;
        public TextMeshProUGUI curRankUI;
        public GameObject qualificationPanel;
        public List<string> names;
        public Animator camAnim;
        public GameObject[] AIDistGameObjects;
        public LHS_CountdownController gameControllerScript;
        
        public int CurRank
        {
            get
            {
                return curRank;
            }
            set
            {
                curRank = value;
                curRankUI.text = curRank + " / " + Qualitfication;
            }
        }

        public int Qualitfication { get; set; }
        public int SpwanCount { get; set; }
        public bool RoundEnded { get; set; }
        public bool CheckResult { get; set; }
        public int Round { get; set; }
        public int PlayerRank { get; set; }

        public void OnLoaded()
        {
            roundOver.SetActive(false);
            Joystick.gameObject.SetActive(false);
            JumpBtn.gameObject.SetActive(false);
            curRankUI.text = curRank + " / " + Qualitfication;
            Round = roundSystem.matchRound;
        }
        void Update()
        {
            if (CheckResult)
            {
                RoundOver();
            }
        }

        public void ShowQualificationList()
        {
            if (Round < 3)
            {
                for (int i = 0; i < (Qualitfication * 2); i++)
                {
                    string name = names[i];
                    if (i < Qualitfication)
                    {
                        qualificationPanel.transform.GetChild(0).GetChild(i).gameObject.SetActive(true);
                        qualificationPanel.transform.GetChild(0).GetChild(i).GetChild(1).GetComponent<TextMeshProUGUI>().text = name;
                        qualificationPanel.transform.GetChild(0).GetChild(i).GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = (i + 1).ToString();
                    }
                    else
                    {
                        qualificationPanel.transform.GetChild(1).GetChild(i - Qualitfication).gameObject.SetActive(true);
                        qualificationPanel.transform.GetChild(1).GetChild(i - Qualitfication).GetChild(1).GetComponent<TextMeshProUGUI>().text = name;
                        qualificationPanel.transform.GetChild(1).GetChild(i - Qualitfication).GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = (i + 1).ToString();
                    }
                }
            }
            else if (Round == 3)
            {
                for (int i = 0; i < 8; i++)
                {
                    string name = names[i];
                    if (i < Qualitfication)
                    {
                        qualificationPanel.transform.GetChild(0).GetChild(i).gameObject.SetActive(true);
                        qualificationPanel.transform.GetChild(0).GetChild(i).GetChild(1).GetComponent<TextMeshProUGUI>().text = name;
                        qualificationPanel.transform.GetChild(0).GetChild(i).GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = (i + 1).ToString();
                    }
                    else
                    {
                        qualificationPanel.transform.GetChild(1).GetChild(i - Qualitfication).gameObject.SetActive(true);
                        qualificationPanel.transform.GetChild(1).GetChild(i - Qualitfication).GetChild(1).GetComponent<TextMeshProUGUI>().text = name;
                        qualificationPanel.transform.GetChild(1).GetChild(i - Qualitfication).GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = (i + 1).ToString();
                    }
                }
            }
        }

        public void RoundOver()
        {
            curretTime += Time.deltaTime;
            if (!RoundEnded)
            {
                roundOver.SetActive(true);
                Joystick.gameObject.SetActive(false);
                JumpBtn.gameObject.SetActive(false);
            }
            RoundEnded = true;

            if (roundOver.activeSelf == true)
            {
                if (curretTime > waitTime)
                {
                    if (player.transform.position.z > 560)
                    {
                        if (curretTime > 3f)
                        {
                            /*if (success.activeSelf)
                                return;*/
                            roundOver.SetActive(false);

                            completionParticle.SuccessParticle();
                            completionParticle.transform.position = player.transform.position + new Vector3(0, 4f, 0);
                            ShowQualificationList();
                            roundSystem.NextRound();
                        }
                    }
                    else
                    {
                        if (curretTime > 3f)
                        {
                            if (failure.activeSelf)
                                return;
                            roundOver.SetActive(false);
                            failure.SetActive(true);
                            if (Round == 1)
                            {
                                PlayerRank = 31;
                            }
                            else if (Round == 2)
                            {
                                PlayerRank = 15;
                            }
                            else if (Round == 3)
                            {
                                PlayerRank = 7;
                            }
                            names.Insert(PlayerRank, "YOU");
                            ShowQualificationList();
                            roundSystem.RoundFailed();

                            //mysfx.PlayOneShot(losefx);
                        }
                    }
                }
            }
        }
    }
}