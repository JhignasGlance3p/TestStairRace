using UnityEngine;

namespace nostra.origami.crowdcity
{
    public class MinimapIcon : MonoBehaviour
    {
        public enum ClampType { Circle, Square };

        [SerializeField] Transform MinimapCam;
        [SerializeField] float MinimapSize;
        [SerializeField] ClampType clampType;

        Vector3 TempV3;

        void Update()
        {
            TempV3 = transform.parent.transform.position;
            TempV3.y = transform.position.y;
            transform.position = TempV3;
        }

        void LateUpdate()
        {
            if (MinimapCam != null)
            {
                if (clampType == ClampType.Circle)
                {
                    // Center of Minimap
                    Vector3 centerPosition = MinimapCam.transform.localPosition;
                    // Just to keep a distance between Minimap camera and this Object (So that camera don't clip it out)
                    centerPosition.y -= 0.5f;
                    // Distance from the gameObject to Minimap
                    float Distance = Vector3.Distance(transform.position, centerPosition);
                    // If the Distance is less than MinimapSize, it is within the Minimap view and we don't need to do anything
                    // But if the Distance is greater than the MinimapSize, then do this
                    if (Distance > MinimapSize)
                    {
                        // Gameobject - Minimap
                        Vector3 fromOriginToObject = transform.position - centerPosition;
                        // Multiply by MinimapSize and Divide by Distance
                        fromOriginToObject *= MinimapSize / Distance;
                        // Minimap + above calculation
                        transform.position = centerPosition + fromOriginToObject;
                    }
                }
                else if (clampType == ClampType.Square)
                {
                    transform.position = new Vector3(
                        Mathf.Clamp(transform.position.x, MinimapCam.position.x - MinimapSize, MinimapSize + MinimapCam.position.x),
                        transform.position.y,
                        Mathf.Clamp(transform.position.z, MinimapCam.position.z - MinimapSize, MinimapSize + MinimapCam.position.z)
                    );
                }
            }
        }
    }
}