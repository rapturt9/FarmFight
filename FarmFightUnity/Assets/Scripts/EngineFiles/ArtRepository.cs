using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ArtRepository : MonoBehaviour
{


    public static ArtRepository Art;

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

    public void OnEnable()
    {

        if (Art == null)
        {
            Art = this;
        }
        else if (Art != this)
        {
            Destroy(gameObject);
        }


        artCollectionInternal = new Dictionary<string, TileArt>();

        foreach(var art in tileArtCollection)
        {
            artCollectionInternal[art.artName] = art;
        }

        
        
    }
}
