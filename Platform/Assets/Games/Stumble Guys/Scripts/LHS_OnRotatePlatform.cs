using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LHS_OnRotatePlatform : MonoBehaviour
{
    private Transform mainRoot;
    private string name;

    private void Start()
    {
        name = this.name;
        if(name == "Player")
        {
            mainRoot = this.transform;
        }
        else { mainRoot = this.transform.parent; }
        //mainRoot = transform.root;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
        {
            this.transform.parent = collision.transform;
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Platform"))
            this.transform.parent = mainRoot;
    }
}
