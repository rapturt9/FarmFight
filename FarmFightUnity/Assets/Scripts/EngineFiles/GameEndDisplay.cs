using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEndDisplay : MonoBehaviour
{
    public GameObject tintPanel;
    public List<GameObject> endingMessages;

    float maxTint = 0.6f;
    float maxScale = 0.8f;
    float startScale = 0.01f;
    float fadesSpeed = 0.6f;
    float scaleSpeed = 1f;

    public void EndDisplay(bool won)
    {
        StartCoroutine("StartEndCoroutine", won);
    }

    IEnumerator StartEndCoroutine(bool won)
    {
        GameObject endingMessage = won ? endingMessages[0] : endingMessages[1];
        endingMessage.transform.localScale = new Vector3(startScale, startScale, startScale);
        tintPanel.SetActive(true);
        endingMessage.SetActive(true);

        do
        {
            if (tintPanel.GetComponent<SpriteRenderer>().color.a < maxTint)
                tintPanel.GetComponent<SpriteRenderer>().color += (Color.black * fadesSpeed) * Time.deltaTime;
            if (endingMessage.transform.localScale.magnitude < Mathf.Sqrt(3*maxScale*maxScale))
            {
                float scaleChange = scaleSpeed * Time.deltaTime;
                endingMessage.transform.localScale += new Vector3(1, 1, 1) * scaleChange;
            }

            yield return new WaitForEndOfFrame();
        }
        while (tintPanel.GetComponent<SpriteRenderer>().color.a < maxTint || endingMessage.transform.localScale != new Vector3(1, 1, 1));
    }
}
