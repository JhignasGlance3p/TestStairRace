using UnityEngine;
using DG.Tweening;

namespace com.nampstudios.bumper.Player
{
    public class Pusher : MonoBehaviour
    {
        public GameObject Hitter;
        public GameObject Spring;
        public float HittingSpeed;
        public GameObject Left_Jnt;
        public GameObject Right_Jnt;
        public float LeftLength;
        public float RightLength;
        public BoxCollider staticWeaponCollider;
        public BoxCollider dynamicWeaponCollider;

        private bool weaponMoving;
        private Vector3 originalColliderSize;
        private Vector3 originalColliderCenter;

        private void Start()
        {
            Hitter.SetActive(false);
            dynamicWeaponCollider.enabled = false;
            originalColliderSize = staticWeaponCollider.size;
            originalColliderCenter = staticWeaponCollider.center;
        }

        public void ThrowWeapon(float hittingDistance)
        {
            if (Hitter == null || weaponMoving) return;
            Hitter.SetActive(true);
            dynamicWeaponCollider.enabled = true;

            weaponMoving = true;

            Sequence weaponSequence = DOTween.Sequence();
            weaponSequence.Append(Hitter.transform.DOLocalMoveZ(hittingDistance, HittingSpeed).SetEase(Ease.InSine, 3))
                .OnUpdate(() =>
                {
                    float currentDistance = Hitter.transform.localPosition.z;
                    staticWeaponCollider.size = new Vector3(originalColliderSize.x, originalColliderSize.y, originalColliderSize.z + currentDistance);
                    staticWeaponCollider.center = new Vector3(originalColliderCenter.x, originalColliderCenter.y, originalColliderCenter.z + currentDistance / 2f);
                })
                .OnComplete(() =>
                {
                    dynamicWeaponCollider.enabled = false;
                    Hitter.transform.DOLocalMoveZ(0f, HittingSpeed).SetEase(Ease.OutSine, 3)
                        .OnUpdate(() =>
                        {
                            float currentDistance = Hitter.transform.localPosition.z;
                            staticWeaponCollider.size = new Vector3(originalColliderSize.x, originalColliderSize.y, originalColliderSize.z + currentDistance);
                            staticWeaponCollider.center = new Vector3(originalColliderCenter.x, originalColliderCenter.y, originalColliderCenter.z + currentDistance / 2f);
                        })
                        .OnComplete(() =>
                        {
                            staticWeaponCollider.size = originalColliderSize;
                            staticWeaponCollider.center = originalColliderCenter;
                            Hitter.SetActive(false);
                            weaponMoving = false;
                        });
                });
            Spring.transform.DOScaleY(hittingDistance, HittingSpeed).SetEase(Ease.InSine, 3.5f).OnComplete(() =>
                Spring.transform.DOScaleY(0f, HittingSpeed).SetEase(Ease.OutSine, 3.5f));
        }
    }
}
