using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vegetable : MonoBehaviour
{
    public Sprite[] vegietableSprites;

    public GameObject basket;

    private GameObject SpriteFolder;

    public float InvSpeed;

    private float value;


    public float expScaling, SpeedScaleFactor;

    Vector3 startPos;
    Vector3 endPos { get { return basket.transform.position; } }
    
    public void init(Hex start, CropType crop, double cropValue)
    {

        

        transform.parent = SpriteRepo.Sprites.transform;

        startPos = TileManager.TM.HexToWorld(start);
        value = (float) cropValue;

        InvSpeed -= SpeedScaleFactor*Mathf.Exp(-(startPos-endPos).magnitude/expScaling);
        

        if (crop == CropType.potato)
            GetComponent<SpriteRenderer>().sprite = vegietableSprites[0];

        else if(crop == CropType.carrot)
            GetComponent<SpriteRenderer>().sprite = vegietableSprites[1];

        else if (crop == CropType.rice)
            GetComponent<SpriteRenderer>().sprite = vegietableSprites[2];

        else if (crop == CropType.eggplant)
            GetComponent<SpriteRenderer>().sprite = vegietableSprites[3];


        StartCoroutine("Mover");

    }


    private IEnumerator Mover()
    {
        Vector3 direction = (endPos - startPos);

        Vector3 OffsetDirection = Vector2.Perpendicular(direction).normalized;



        if(OffsetDirection.y < 0)
        {
            OffsetDirection = - OffsetDirection;
        }


        float distance = direction.magnitude;
        direction.Normalize();

        float offsetDist = (distance / 2) - (distance/2) * Random.Range(-.25f, .25f);

        float along = 0;

        while(along <= 1)
        {
            transform.position = startPos + (direction * distance * along) +
                                   (offset(along,offsetDist) * OffsetDirection);



            along += 1/InvSpeed;

            yield return new WaitForFixedUpdate();

        }

        Repository.Central.money += value;
        Destroy(gameObject);
    }

    private float offset(float along, float max)
    {
        return max* (2 * along * (-along + 1));
    }
}
