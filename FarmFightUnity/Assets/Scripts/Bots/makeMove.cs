using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class makeMove : MonoBehaviour
{
    public bool gameIsRunning = true;
    public GameState gameData;

    // Start is called before the first frame update
    /*void Start()
    {
        StartCoroutine(startGame());
    }

    IEnumerator startGame ()
    {
        while (gameIsRunning){
            pickMove();
            yield return new WaitForSeconds(1.0F);
        }

    }

    void pickMove () {
        gameData.updateGameState();

    }*/
}
