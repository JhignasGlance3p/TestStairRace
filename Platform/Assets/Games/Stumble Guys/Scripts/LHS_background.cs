using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace nostra.origami.stumble
{
    public class LHS_background : MonoBehaviour
    {
        public float delta = 0.02f;
        public float speed = 20f;
        Vector3 pos;

        void Start()
        {
            pos = transform.position;
        }
        void Update()
        {
            Vector3 v = pos;
            v.x += delta * Mathf.Sin(Time.time * speed);
            transform.position = v;
        }
    }
}