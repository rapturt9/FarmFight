using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TileInformationPanel : MonoBehaviour
{
    public int index;
    public int SoldierCount;
    public float health;

    public TextMeshPro healthText, countText;

    public float movementSpeed;

    public Vector2 GoalPosition;

    

    private void Start()
    {
        StartCoroutine(Mover());
    }

    void FixedUpdate()
    {
        healthText.text = health.ToString();
        countText.text = SoldierCount.ToString();
    }

    private IEnumerator Mover()
    {
        while (true)
        {
            transform.position = Vector3.MoveTowards(transform.position,
                                                        GoalPosition,
                                                        movementSpeed);
        }
    }

}
