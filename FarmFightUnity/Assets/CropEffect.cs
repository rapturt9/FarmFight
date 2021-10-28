using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CropEffect : MonoBehaviour
{
    // Start is called before the first frame update
    public Sprite[] effectSprites;
    private GameObject SpriteFolder;

    Vector3 startPos;
    public void init(Hex start, string type)
    {
        
        transform.parent = SpriteRepo.Sprites.transform;

        startPos = TileManager.TM.HexToWorld(start);

        transform.position = startPos;

        if (type == "sparkle")
            GetComponent<SpriteRenderer>().sprite = effectSprites[0];

        else if(type=="rot")
            GetComponent<SpriteRenderer>().sprite = effectSprites[1];

        //StartCoroutine("Mover");

    }

}
