using TMPro;
using UnityEngine;

namespace nostra.origami.crowdcity
{
    public class Timer : MonoBehaviour
    {
        public float duration = 300f; // Duration of the timer in seconds (e.g., 5 minutes)
        public float timeRemaining;
        private TextMeshProUGUI timerText; // Reference to a UI Text component to display the timer

        private bool halfTimeWarningGiven = false;
        private bool fiveSecondsBeforeHalfTimeLogged = false;
        private int previousSecondsBeforeHalfTime = -1;

        void Start ()
        {
            timerText = GetComponent<TextMeshProUGUI> ( );
            timeRemaining = duration;
        }

        void LateUpdate ()
        {
            if ( timeRemaining > 0 )
            {
                timeRemaining -= Time.deltaTime;
                UpdateTimerText ( timeRemaining );

                float halfTimeMark = duration / 2;

                // Check for the 5 seconds before halftime warning
                if ( !fiveSecondsBeforeHalfTimeLogged && timeRemaining <= halfTimeMark + 5f && timeRemaining > halfTimeMark )
                {
                    int secondsBeforeHalfTime = Mathf.CeilToInt ( timeRemaining - halfTimeMark );

                    if ( secondsBeforeHalfTime != previousSecondsBeforeHalfTime )
                    {
                        Debug.Log ( secondsBeforeHalfTime + " seconds remaining until halftime." );
                        previousSecondsBeforeHalfTime = secondsBeforeHalfTime;
                    }

                    if ( secondsBeforeHalfTime <= 1 )
                    {
                        fiveSecondsBeforeHalfTimeLogged = true;
                    }
                }

                // Check for the halftime warning
                if ( !halfTimeWarningGiven && timeRemaining <= halfTimeMark )
                {
                    Debug.Log ( "Half of the time has passed." );
                    halfTimeWarningGiven = true;
                }
            } else
            {
                // Timer has finished
                timeRemaining = 0;
                UpdateTimerText ( timeRemaining );
                // You can add additional actions here, like triggering an event
            }
        }

        void UpdateTimerText ( float timeToDisplay )
        {
            // Format the time into minutes and seconds
            int minutes = Mathf.FloorToInt ( timeToDisplay / 60 );
            int seconds = Mathf.FloorToInt ( timeToDisplay % 60 );

            // Update the timer text
            timerText.text = string.Format ( "{0:00}:{1:00}", minutes, seconds );
        }
    }
}