using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using MLAPI;

public class GameEndDisplay : MonoBehaviour
{
    public GameObject tintPanel;
    public GameObject winlose;
    public GameObject playAgainButton;
    public GameObject winloseText;
    public GameObject hostPlea;
    public Sprite[] endingCardTypes;

    string[] endingMessages = new string[] { "You Won!", "You Lost" };

    const float maxTint = 0.6f;
    const float maxScale = 0.5f;
    const float startScale = 0.01f;
    const float fadesSpeed = 0.6f;
    const float scaleSpeed = 1f;

    bool gameEnded = false;
    public bool hostCanContinue = false;

    public void EndingDisplay(bool won)
    {
        if (gameEnded)
        {
            return;
        }
        gameEnded = true;

        // If we're hosting, make it so you can't quit
        // Listen server means host quitting makes everyone gets kicked out
        if (NetworkManager.Singleton.IsHost && !won)
        {
            winlose.GetComponent<SpriteRenderer>().sprite = endingCardTypes[1];
            playAgainButton.SetActive(false);
            hostPlea.SetActive(true);
        }
        else
        {
            winlose.GetComponent<SpriteRenderer>().sprite = endingCardTypes[0];
        }

        // Setting the win/lose text
        winloseText.GetComponent<TextMeshProUGUI>().text = won ? endingMessages[0] : endingMessages[1];

        // Making visible
        winlose.transform.localScale = new Vector3(startScale, startScale, startScale);
        tintPanel.SetActive(true);
        winlose.SetActive(true);

        StartCoroutine("StartEndCoroutine");
    }

    IEnumerator StartEndCoroutine()
    {
        do
        {
            // Tinting
            if (tintPanel.GetComponent<SpriteRenderer>().color.a < maxTint)
            {
                tintPanel.GetComponent<SpriteRenderer>().color += (Color.black * fadesSpeed) * Time.deltaTime;
            }
            else
            {
                tintPanel.GetComponent<SpriteRenderer>().color = new Color(0, 0, 0, maxTint);
            }

            // Scaling
            if (winlose.transform.localScale.x < maxScale)
            {
                float scaleChange = scaleSpeed * Time.deltaTime;
                winlose.transform.localScale += new Vector3(1, 1, 1) * scaleChange;
            }
            else
            {
                winlose.transform.localScale = Vector3.one * maxScale;
            }

            yield return new WaitForEndOfFrame();
        }
        while (tintPanel.GetComponent<SpriteRenderer>().color.a < maxTint || winlose.transform.localScale.x <= maxScale);
    }

    public void EnableHostPlayAgain()
    {
        winlose.GetComponent<SpriteRenderer>().sprite = endingCardTypes[0];
        hostPlea.SetActive(false);
        playAgainButton.SetActive(true);
    }
}
