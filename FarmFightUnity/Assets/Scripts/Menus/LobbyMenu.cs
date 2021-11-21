using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LobbyMenu : MonoBehaviour
{
    public void PlayGame(bool isHosting)
    {
        SceneManager.LoadScene("MainScene");
        SceneVariables.isHosting = isHosting;
        SceneVariables.cameThroughMenu = true;
    }
}
