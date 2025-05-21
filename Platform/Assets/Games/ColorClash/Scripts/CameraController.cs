using UnityEngine;

namespace nostra.sarvottam.colorclash
{
    public enum CameraMode
    {
        Default,
        ThirdPerson,
        ThirdPersonFront,
        FollowPlayerTopDown
    }
    public class CameraController : MonoBehaviour
    {
        Transform player;

        [Header("Default Config")]
        [SerializeField] float defaultOrthographicSize;

        [Header("Follow Cam Config")]
        [SerializeField] float followOrthographicSize;
        [SerializeField] Vector2 followCamOffset;

        [Header("TP Cam Config")]
        [SerializeField] Vector3 tpOffset;

        Camera _cam;
        Vector3 _defPos;
        Transform _defParent;
        Quaternion _defRot;
        CameraMode _mode;

        public void Start()
        {
            _cam = GetComponent<Camera>();
            SetDefaultPos(transform.position);
            _defParent = transform.parent;
            _defRot = transform.rotation;
            _mode = CameraMode.Default;
        }

        public void SetPlayerTransform(Transform player)
        {
            this.player = player;
            Debug.Log($"Player transform set to {player.name}");
        }

        public void SetDefaultPos(Vector3 pos)
        {
            _defPos = pos;
        }

        public void SetCameraMode(CameraMode mode)
        {
            _mode = mode;
        }

        void Update()
        {
            switch(_mode)
            {
                case CameraMode.Default:
                    _cam.orthographic = true;
                    _cam.orthographicSize = defaultOrthographicSize;
                    transform.SetParent(_defParent);
                    _cam.transform.position = _defPos;
                    transform.rotation = _defRot;
                    break;
                case CameraMode.FollowPlayerTopDown:
                    _cam.orthographic = true;
                    _cam.orthographicSize = followOrthographicSize;
                    transform.SetParent(player);
                    transform.localPosition = Vector3.zero; 
                    transform.rotation = _defRot;
                    transform.position = new Vector3(transform.position.x, _defPos.y, transform.position.z);
                    transform.position += new Vector3(followCamOffset.x, 0, followCamOffset.y);
                    break;
                case CameraMode.ThirdPerson:
                    _cam.orthographic = false;
                    transform.SetParent(player);
                    transform.localPosition = tpOffset;
                    transform.LookAt(player);
                    break;
                case CameraMode.ThirdPersonFront:
                    _cam.orthographic = false;
                    transform.SetParent(player);
                    transform.localPosition = new Vector3(tpOffset.x,tpOffset.y,-tpOffset.z);
                    transform.LookAt(player);
                    break;

            }
        }
    }
}
