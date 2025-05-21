using UnityEngine;

namespace nostra.origami.crowdcity
{
    public class Character : MonoBehaviour
    {
        private CrowdHandler crowdHandler;

        public void SetCharacter(CrowdHandler _crowdHandler)
        {
            crowdHandler = _crowdHandler;
        }
        private void OnTriggerEnter(Collider col)
        {
            if (col.CompareTag("Character") && transform.parent != col.transform.parent && crowdHandler != null)
            {
                if (col.GetComponent<NeutralCharacter>() != null)
                {
                    return;
                }
                CrowdHandler triggeredCrowd = col.gameObject.GetComponentInParent<CrowdHandler>(); ;
                if (triggeredCrowd != null && crowdHandler.CrowdCount >= triggeredCrowd.CrowdCount)
                {
                    triggeredCrowd.RemoveCrowd(col.gameObject, killer: crowdHandler.crowdGangName);
                    crowdHandler.UpdateCrowd(1);
                }
                else if (crowdHandler != null && triggeredCrowd != null)
                {
                    crowdHandler.RemoveCrowd(gameObject, killer: triggeredCrowd.crowdGangName);
                    triggeredCrowd.UpdateCrowd(1);
                }
            }
        }
    }
}