using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using MLAPI;

public class ExitFunction : MonoBehaviour
{
    public void exit()
    {
        SceneManager.LoadScene("MainMenu");

        //foreach(var obj in objToDestroy)
        //{
        //    Destroy(obj);
        //}


        // This stops the network
        NetworkManager.Singleton.Shutdown();
    }

    public GameObject[] objToDestroy;
}
