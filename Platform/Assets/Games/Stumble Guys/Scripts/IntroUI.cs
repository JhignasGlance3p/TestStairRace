using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace nostra.origami.stumble
{
    public class IntroUI : MonoBehaviour
    {
        public GameObject missionUI;
        public GameObject missionPos;
        Vector3 dir;
        public float speed = 3f;

        void Update()
        {
            dir = missionPos.transform.position - missionUI.transform.position;
            missionUI.transform.position += dir * speed * Time.deltaTime;
        }
    }
}