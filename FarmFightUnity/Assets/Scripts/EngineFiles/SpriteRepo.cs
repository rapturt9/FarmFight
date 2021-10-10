using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class SpriteRepo : MonoBehaviour
{
    // Start is called before the first frame update

    public static SpriteRepo Sprites;
    public Repository central;

    private Dictionary<string, GameObject> SpriteObjects;

    public GameObject this[string name, Hex hex]
    {
        get
        {
            GameObject temp = Instantiate(SpriteObjects[name]);
            temp.transform.parent = gameObject.transform;
            if (temp.TryGetComponent(typeof(Soldier), out Component component))
            {
                ((Soldier)component).owner = central.localPlayerId;
            }
            temp.SetActive(true);
            temp.GetComponent<NetworkObject>().Spawn();

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
