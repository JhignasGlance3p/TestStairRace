using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

namespace nostra.PKPL.JailBreakGame
{
    public class PopAnimator : MonoBehaviour
    {
        [SerializeField] GameManager gameManager;
        public List<Transform> popObjects;
        List<Vector3> initialScales = new List<Vector3>();

        public float initialDelay = 0;
        [SerializeField] TMP_Text _PlayerID;
        private void Awake()
        {
            foreach (var item in popObjects)
            {
                initialScales.Add(item.localScale);
            }
        }

        private void OnEnable()
        {
            StartCoroutine(PopCoroutine());
        }

        IEnumerator PopCoroutine()
        {
            _PlayerID.text = $"{gameManager.winnerID}";
            foreach (var item in popObjects)
            {
                item.localScale = Vector3.zero;
            }

            yield return new WaitForSeconds(initialDelay);

            for (int i = 0; i < popObjects.Count; i++)
            {
                if (popObjects[i].gameObject.activeSelf)
                {
                    yield return new WaitForSeconds(.15f);
                }
                popObjects[i].DOScale(initialScales[i], .25f).SetEase(Ease.OutSine);
            }
            yield return new WaitForSeconds(.25f);

        }

        public void Close()
        {
            if (gameObject.activeSelf)
            {
                StartCoroutine(CloseCoroutine());
            }
        }

        public IEnumerator CloseCoroutine()
        {
            foreach (var item in popObjects)
            {
                item.DOScale(0, .25f).SetEase(Ease.InSine);
                yield return new WaitForSeconds(.25f);
            }
            gameObject.SetActive(false);
        }
    }
}
