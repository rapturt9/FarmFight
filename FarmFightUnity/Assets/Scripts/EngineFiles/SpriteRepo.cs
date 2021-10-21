using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;
using MLAPI.Messaging;

public class SpriteRepo : NetworkBehaviour
{
    // Start is called before the first frame update

    public static SpriteRepo Sprites;

    public List<GameObject> SpriteList;

    private Dictionary<string, GameObject> SpriteObjects;

    public GameObject this[string name]
    {
        get
        {
            GameObject temp = Instantiate(SpriteObjects[name]);
            temp.transform.parent = gameObject.transform;
            //temp.GetComponent<NetworkObject>().Spawn(); // Do this yourself

            return temp;
        }
    }


    public void Awake()
    {
        if (Sprites != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Sprites = this;
        }

        SpriteObjects = new Dictionary<string, GameObject>();

        foreach (GameObject obj in SpriteList)
        {
            SpriteObjects.Add(obj.name, obj);
        }
    }
}
