using UnityEngine;
using TMPro;

namespace com.nampstudios.bumper
{
    public class FPSDisplay : MonoBehaviour
    {
        public TextMeshProUGUI fpsText;
        private int frameCount = 0;
        private float deltaTime = 0.0f;
        private float fps = 0.0f;
        private float updateInterval = 1.0f;
        private float highestFPS = 0.0f;
        private float lowestFPS = float.MaxValue;

        private void Start()
        {
            Invoke(nameof(SetLowest), 10f);
        }
        void Update()
        {
            frameCount++;
            deltaTime += Time.unscaledDeltaTime;

            if (deltaTime > updateInterval)
            {
                fps = frameCount / deltaTime;

                if (fps > highestFPS)
                {
                    highestFPS = fps;
                }

                if (fps < lowestFPS)
                {
                    lowestFPS = fps;
                }

                fpsText.text = string.Format("FPS: {0:0.} H: {1:0.}  L: {2:0.} ", fps, highestFPS, lowestFPS);

                frameCount = 0;
                deltaTime -= updateInterval;
            }
        }

        private void SetLowest()
        {
            lowestFPS = float.MaxValue;
        }
    }
}
