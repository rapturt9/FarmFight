using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;

public class SoldierTrip: NetworkBehaviour
{
    

    Soldier soldier { get { return GetComponent<Soldier>(); } }

    List<Vector3> Path;

    Hex start, end;

    public void init( Hex start, Hex end)
    {
        this.start = start;
        this.end = end;

        Path = PathCreator(start, end);


        StartCoroutine("Mover");
    }



    


    static List<Vector3> PathCreator(Hex start, Hex end)
    {
        List<Vector3> temp = new List<Vector3>();

        

        //if(TileManager.TM["Crops"][end].soldierCount != 0)
            return new List<Vector3>() { start.world(), end.world() };

        //else
    }

    private IEnumerator Mover()
    {

        soldier.FadeIn();

        //soldier.GetComponent<SpriteRenderer>().color = Color.white;

        soldier.GetComponent<SpriteRenderer>().enabled = true;

        foreach (var wayPoint in Path)
        {
            while(transform.position != wayPoint)
            {
                transform.position = Vector3.MoveTowards(transform.position, wayPoint, soldier.travelSpeed);
                yield return new WaitForFixedUpdate();
            }
        }
        
        TileManager.TM["Crops"][end].addSoldier(soldier);

        //Repository.Central.cropHandler.SyncTile(end);
    }
}
