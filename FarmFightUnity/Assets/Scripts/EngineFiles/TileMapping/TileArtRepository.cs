using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TileArtRepository : MonoBehaviour
{


    public static TileArtRepository Art;

    [SerializeField]
    private List<TileArt> tileArtCollection;

    private Dictionary<string, TileArt> artCollectionInternal;

    public TileArt this[string name]
    {
        get
        {
            return artCollectionInternal[name];
        }
    }

    public void Init()
    {

        


        artCollectionInternal = new Dictionary<string, TileArt>();

        foreach(var art in tileArtCollection)
        {
            artCollectionInternal[art.artName] = art;
        }

        
        
    }

    public void Awake()
    {
        if (Art == null)
        {
            Art = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
