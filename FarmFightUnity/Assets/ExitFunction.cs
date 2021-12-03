using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitFunction : MonoBehaviour
{
    public void exit()
    {
        SceneManager.LoadScene("MainMenu");

        foreach(var obj in objToDestroy)
        {
            Destroy(obj);
        }
    }

    public GameObject[] objToDestroy;
}
