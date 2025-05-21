using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace nostra.origami.stumble
{
    public class WaitingCharacterMove : MonoBehaviour
    {
        public GameObject destPos;
        float speed = 1.8f;
        Vector3 dir;

        void Update()
        {
            dir = destPos.transform.position - transform.position;
            transform.position += dir * speed * Time.deltaTime;
        }
    }
}