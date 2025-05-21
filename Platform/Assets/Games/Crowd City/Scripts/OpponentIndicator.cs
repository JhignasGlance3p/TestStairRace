using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace nostra.origami.crowdcity
{
    public class MultipleOpponentsIndicator : MonoBehaviour
    {
        public GameObject indicatorPrefab; // Reference to the indicator prefab
        public RectTransform indicatorsParent; // Reference to the parent object for indicators
        public List<Transform> opponents; // List of opponent Transforms
        public Camera mainCamera; // Reference to the main camera
        public float edgeBuffer = 10f; // Buffer from the screen edge

        private List<RectTransform> indicators;

        private void Start()
        {
            // Instantiate indicators for each opponent
            indicators = new List<RectTransform>();
            foreach (Transform opponent in opponents)
            {
                GameObject indicatorObject = Instantiate(indicatorPrefab, indicatorsParent);
                RectTransform indicatorRectTransform = indicatorObject.GetComponent<RectTransform>();
                indicators.Add(indicatorRectTransform);
                indicatorObject.SetActive(false);

                Color color = opponent.transform.GetChild(0).GetChild(1).GetComponent<Image>().color;

                indicatorRectTransform.transform.GetComponent<Image>().color = color;
                indicatorRectTransform.transform.GetChild(0).GetComponent<Image>().color = color;
            }
        }

        private void Update()
        {
            for (int i = 0; i < opponents.Count; i++)
            {
                Transform opponent = opponents[i];
                RectTransform indicatorUI = indicators[i];

                Vector3 screenPoint = mainCamera.WorldToScreenPoint(opponent.position);

                // Check if the opponent is out of the screen view
                if (screenPoint.z > 0 && (screenPoint.x < 0 || screenPoint.x > Screen.width || screenPoint.y < 0 || screenPoint.y > Screen.height))
                {
                    indicatorUI.gameObject.SetActive(true);

                    // Clamp the indicator position to the screen edges with buffer
                    screenPoint.x = Mathf.Clamp(screenPoint.x, edgeBuffer, Screen.width - edgeBuffer);
                    screenPoint.y = Mathf.Clamp(screenPoint.y, edgeBuffer, Screen.height - edgeBuffer);

                    // Set the indicator position and rotation
                    indicatorUI.position = screenPoint;

                    // Calculate the direction vector from the player to the opponent
                    Vector3 direction = opponent.position - mainCamera.transform.position;
                    direction.z = 0; // Ignore Z axis for 2D rotation
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + mainCamera.transform.eulerAngles.y;
                    indicatorUI.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

                    CrowdHandler opponentCrowd = opponent.GetComponentInParent<CrowdHandler>(true);
                    if (opponentCrowd != null)
                    {
                        indicatorUI.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = opponentCrowd.CrowdCount.ToString();

                        if (opponentCrowd.CrowdCount == 0)
                        {
                            opponents.RemoveAt(i);
                            indicators.RemoveAt(i);
                            indicatorUI.gameObject.SetActive(false);
                        }
                    }
                }
                else
                {
                    indicatorUI.gameObject.SetActive(false);
                }
            }
        }
    }
}