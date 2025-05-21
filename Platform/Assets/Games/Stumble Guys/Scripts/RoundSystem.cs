using nostra.origami.common;
using nostra.origami.stumble;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace nostra.origami.stumble
{
    public class RoundSystem : MonoBehaviour
    {
        [SerializeField] private UIManager UI;
        [SerializeField] private Transform Opponents;
        [SerializeField] private GameObject winAnimateObj;
        [SerializeField] private GameObject[] objsToDisable;

        AINavMesh[] opponentAINavMesh;

        public int matchRound { get; private set; }

        public void OnLoaded()
        {
            matchRound = 1;
            Reset();
        }
        void Reset()
        {
            matchRound = 1;
            switch (matchRound)
            {
                case 1:
                    UI.SpwanCount = 32;
                    UI.Qualitfication = 16;
                    break;
                case 2:
                    UI.SpwanCount = 16;
                    UI.Qualitfication = 8;
                    break;
                case 3:
                    UI.SpwanCount = 8;
                    UI.Qualitfication = 1;
                    break;
                default:
                    break;
            }
            UI.curRankUI.text = "0 / " + UI.Qualitfication;
            opponentAINavMesh = new AINavMesh[UI.SpwanCount - 1];
            for (int i = 0; i < UI.SpwanCount - 1; i++)
            {
                Opponents.GetChild(i).gameObject.SetActive(true);
                opponentAINavMesh[i] = Opponents.GetChild(i).gameObject.GetComponent<AINavMesh>();
            }
        }
        public void NextRound()
        {
            StopCharacter();
            if (matchRound == 3)
            {
                matchRound = 1;
                foreach (GameObject ga in objsToDisable)
                {
                    ga.SetActive(false);
                }
                winAnimateObj.SetActive(true);
            }
            else
            {
                matchRound++;
                UI.camAnim.SetBool("EndCamPan", false);
                UI.camAnim.GetComponent<LHS_Camera>().enabled = true;
                MyUtils.Execute(2f, () =>
                {
                    UI.qualificationPanel.SetActive(true);
                });
            }
        }
        public void RoundFailed()
        {
            StopCharacter();
            matchRound = 1;
            MyUtils.Execute(2f, () =>
            {
                UI.failure.SetActive(false);
                UI.qualificationPanel.SetActive(true);
            });
        }
        public void StopCharacter()
        {
            for (int i = 0; i < UI.SpwanCount - 1; i++)
            {
                opponentAINavMesh[i].Deactive();
            }
            UI.player.GetComponent<LHS_MainPlayer>().Deactive();
        }
    }
}