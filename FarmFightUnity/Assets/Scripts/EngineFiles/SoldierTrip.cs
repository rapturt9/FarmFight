using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;


public class SoldierTrip : NetworkBehaviour
{


    Soldier soldier { get { return GetComponent<Soldier>(); } }


    public List<Hex> Path;

    Hex start, end;

    public bool init(Hex start, Hex end)
    {
        this.start = start;
        this.end = end;

        Path = PathCreator(start, end);


        if (Path == null)
            return false;

        StartCoroutine("Mover");

        return true;
    }


    private IEnumerator Mover()
    {

        soldier.FadeIn();



        soldier.GetComponent<SpriteRenderer>().enabled = true;

        foreach (var wayPoint in finder.optimalPath)
        {
            Vector3 pos = TileManager.TM.HexToWorld(wayPoint);
            while (transform.position != pos)
            {
                transform.position = Vector3.MoveTowards(transform.position, pos, soldier.travelSpeed);
                yield return new WaitForFixedUpdate();
            }
        }

        if (TileManager.TM["Crops"][end].soldierCount != 0)
            soldier.FadeOut();

        soldier.AddToTile(end);


        //TileManager.TM["Crops"][end].addSoldier(soldier);


    }

    public List<Hex> searched;

    public PathFinder finder;

    List<Hex> PathCreator(Hex start, Hex end)
    {
        List<Hex> temp;


        finder = new PathFinder(start, end, soldier.owner.Value, out temp, this);

        return temp;


    }


}

