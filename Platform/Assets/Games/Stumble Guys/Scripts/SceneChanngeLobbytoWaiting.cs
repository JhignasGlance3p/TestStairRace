using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace nostra.origami.stumble
{
    public class SceneChanngeLobbytoWaiting : MonoBehaviour
    {
        public void SceneChange()
        {
            SceneManager.LoadScene("WaitingUser");
        }
    }
}