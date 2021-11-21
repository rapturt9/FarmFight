using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileInfoArrangement : MonoBehaviour
{
    public Transform top, bottom;

    public GameObject panel;

    public Vector3[] positions;

    public panelmover[] panels;

    

    Coroutine valuechecker;

    

    // Start is called before the first frame update
    void Start()
    {

        
        positions = new Vector3[6];
        var topPos = top.GetComponent<RectTransform>().anchoredPosition;
        var botPos = bottom.GetComponent<RectTransform>().anchoredPosition;

        for (int i = 0; i < 6; i++)
        {
            
            positions[i] = new Vector3(topPos.x,  topPos.y - (topPos.y - botPos.y) * i / 5, 0);

            panels[i].GetComponent<RectTransform>().anchoredPosition = positions[i];
        }

        tileValues = new Dictionary<int, (float, float, float)>();

        valuechecker = StartCoroutine(GetTileInfo());

        
    }

    // Update is called once per frame
    void Update()
    {

        if (TileManager.TM.validHexes == null)
            return;

        else if (TileManager.TM.isValidHex(Repository.Central.selectedHex))
        {
            TileTemp selected
                = Repository.Central.cropHandler
                [Repository.Central.selectedHex];


            if (selected.tileOwner != -1)
                sortedSoldiers = selected.SortedSoldiers;

            if (tileValues.Count > 5)
            {
                sendInfo();
            }
            
        }
                
    }

    Dictionary<int, List<Soldier>> sortedSoldiers;

    void sendInfo()
    {

        (int, float)[] sorted = sort();

        for (int i = 0; i<6; i++)
        {
            panels[sorted[i].Item1].goalposition = positions[i];
            panels[sorted[i].Item1].values = tileValues[sorted[i].Item1];
            panels[sorted[i].Item1].Init(sorted[i].Item1);
        }


    }

    (int,float)[] sort()
    {
        
        List<(int, float)> sorting = new List<(int, float)>();

        
        sorting.Add((0,
            tileValues[0].Item2));

        foreach(var player in tileValues)
        {
            for(int i = 0; i < sorting.Count; i++)
            {
                if(player.Value.Item2 > sorting[i].Item2)
                {
                    sorting.Insert(i, (player.Key, player.Value.Item2));
                    break;
                }
            }


            if(!sorting.Contains((player.Key, player.Value.Item2)))
            {
                sorting.Add((player.Key, player.Value.Item2));
            }

        }


        return sorting.ToArray();
        
        
    }




    Dictionary<int, (float, float, float)> tileValues;

    

    private float totalHealth { get
        {
            float a = 0;
            foreach(var health in tileValues)
            {
                a += health.Value.Item2;
            }
            return a;
        } }

    private IEnumerator GetTileInfo()
    {
        
        int player = 0;

       
        while(true)
        {


            if (sortedSoldiers == null)
                yield return null;
            else
            {
               
                float Health = 0;

                var hold = new List<Soldier>(sortedSoldiers[player]);

              

                foreach (var soldier in hold)
                {

                    Health += soldier.Health.Value;

                }
                

                if(totalHealth != 0)

                    tileValues[player] = (
                         sortedSoldiers[player].Count,
                         Health,
                         Health / totalHealth
                         );
                else
                {
                    tileValues[player] = (
                         sortedSoldiers[player].Count,
                         Health,
                         0
                         );
                }
                

                player++;
                player %= 6;

                yield return null;
            }
            

            
        }

        
    }

}
