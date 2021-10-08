using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteRepo : MonoBehaviour
{
    // Start is called before the first frame update

    public static SpriteRepo Sprites;

    private Dictionary<string, GameObject> SpriteObjects;

    public GameObject this[string name, Hex hex]
    {
        get
        {
            GameObject temp = Instantiate(SpriteObjects[name]);
            temp.transform.parent = gameObject.transform;
            temp.SetActive(true);

            return temp;
        }
    }


    public void Awake()
    {

        if(Sprites != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Sprites = this;
        }

        SpriteObjects = new Dictionary<string, GameObject>();


        List<GameObject> childrenTrans = new List<GameObject>();
        foreach(Transform child in transform)
        {
            SpriteObjects.Add(child.gameObject.name, child.gameObject);
        }
    }



}
