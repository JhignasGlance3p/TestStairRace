using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
#if JAILBREAK_FUSION
using Fusion;
#endif

namespace nostra.PKPL.JailBreakGame
{
    public class PlayerDoor :
#if JAILBREAK_FUSION
    NetworkBehaviour
#else
    MonoBehaviour
#endif
    {
        public GameManager gameManager;
        public int keysRequired = 2;
        private int currentKeys = 0;
        private bool isOpen = false;

        public List<Transform> doors;
        public List<Vector3> doorInitialScales;
        public List<MeshRenderer> keyRenderers;
        public List<Color> keyInitialColors;

        public void OnStart()
        {
            foreach (var door in doors)
            {
                doorInitialScales.Add(door.localScale);
            }
            for (int i = 0; i < keyRenderers.Count; i++)
            {
                keyInitialColors.Add(keyRenderers[i].material.GetColor("_Color"));
            }
        }

        public void IncreaseKeyCount()
        {
            if (gameManager.isOfflineMode)
            {
                IncreaseKeyCountLogic();
            }
#if JAILBREAK_FUSION
            else if (GameManager.Instance.networkRunner.IsSharedModeMasterClient)
            {
                RPC_IncreaseKeyCount();
            }
#endif
        }

        private void IncreaseKeyCountLogic()
        {
            currentKeys++;
            if (currentKeys >= keysRequired)
            {
                OpenDoor();
            }

            //set renderer emission to on
            keyRenderers[currentKeys - 1].material.SetInt("_Emission", 1);
            keyRenderers[currentKeys - 1].material.DOColor(Color.green, "_Color", 0.15f);
        }

#if JAILBREAK_FUSION
        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
#endif
        public void RPC_IncreaseKeyCount()
        {
            IncreaseKeyCountLogic();
        }
        private void OpenDoor()
        {
            isOpen = true;
            // Disable the door or play an open animation
            foreach (var door in doors)
            {
                door.DOScaleX(0, 0.25f);
            }
        }

        public bool IsOpen()
        {
            return isOpen;
        }

        public void ResetDoor()
        {
            isOpen = false;
            currentKeys = 0;
            for (int i = 0; i < doors.Count; i++)
            {
                doors[i].localScale = doorInitialScales[i];
            }
            for (int i = 0; i < keyRenderers.Count; i++)
            {
                keyRenderers[i].material.SetInt("_Emission", 0);
                keyRenderers[i].material.DOColor(keyInitialColors[i], "_Color", 0.15f);
            }
        }
    }
}
