using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI;


public class SoldierTrip: NetworkBehaviour
{
    

    Soldier soldier { get { return GetComponent<Soldier>(); } }

    [SerializeField]
    List<Hex> Path;

    Hex start, end;

    public bool init( Hex start, Hex end)
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

        foreach (var wayPoint in Path)
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


List<Hex> PathCreator(Hex start, Hex end)
    {
        List<Hex> temp;


        PathFinder find = new PathFinder(start, end, soldier.owner.Value, out temp);

        return temp;


    }

   
}

/// <summary>
/// finds a path for the soldier to travel
/// </summary>
public class PathFinder
{

    private HashSet<Hex> searched;

    private Hex start, end;

    private int owner;

    public PathFinder(Hex start, Hex end, int owner, out List<Hex> path)
    {
        this.start = start;
        this.end = end;
        this.owner = owner;
        searched = new HashSet<Hex>() { start};

        
        
        path = PathBuilder();
        if(path != null)
            path.Reverse();
    }


    private List<Hex> PathBuilder()
    {
        if (ForwardTrace())
        {
            //Debug.Log(searched.Count);
            return BackwardTrace();
        }
        else
        {
            //Debug.Log(searched.Count);
            return null;
        }

        return null;
        
    }

    /// <summary>
    /// approximate a path forward through searched tiles
    /// returns false if there is no path forward.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    private bool ForwardTrace()
    {
        List<Hex[]> path = new List<Hex[]>();
        path.Add(new Hex[] { start });

        
        return NextStep(path);

    }


    

    /// <summary>
    /// searches for valid neighbors to the last step and adds them to searched and the stepList
    /// </summary>
  
    private bool NextStep(List<Hex[]> steps)
    {
        // base case for recursive function
        if (steps.Count == 0)
            return false;

        List<Hex> nextStep = getNextStep(steps[steps.Count-1]);

        //Debug.Log($"nextStep {nextStep[0]}");


        // add the found values to the searched set
        

        // if there is no way forward discard the current step and try again
        if (nextStep.Count == 0)
        {
            
            return NextStep(steps.GetRange(0, steps.Count - 1));
        }

        searched.UnionWith(nextStep);

        // if the next step has the end, we are finished
        if (nextStep.Contains(end))
            return true;

        // if there are next steps to be had, add them to the steps and keep searching
        
        steps.Add(nextStep.ToArray());

        return NextStep(steps);
        

    }


    /// <summary>
    /// return the closest valid values from the current step
    /// </summary>
    /// <param name="step"></param>
    /// <returns></returns>
    private List<Hex> getNextStep(Hex[] step)
    {
        HashSet<Hex> nextProspects = new HashSet<Hex>();

        foreach(var hex in step)
        {
            nextProspects.UnionWith(ValidNeighbors(hex));
        }
        Debug.Log($"temp init {nextProspects.Count}");

        if (nextProspects.Count == 0)
            return new List<Hex>();


        List<Hex> closestHexes = new List<Hex>();

        int closestDist = int.MaxValue;


        foreach(var hex in nextProspects)
        {
            int dist = distance(end, hex);
            /// if its better than our best, get rid of the current and record it
            if ( dist < closestDist)
            {
                closestDist = dist;
                closestHexes.Clear();
                closestHexes.Add(hex);
            }
            // if it is equal to our best, add it to the list
            else if( dist == closestDist)
            {
                closestHexes.Add(hex);
            }

            /// if it is worse, we dont care about it

        }


        return closestHexes;
            
    }

    /// <summary>
    /// step backward to get the fastest single path 
    /// </summary>
    /// <param name="forwardPath"></param>
    /// <returns></returns>
    private List<Hex> BackwardTrace()
    {
        List<Hex> path = new List<Hex>() { end };
        return pathTrace(path);
    }

    
    private List<Hex> getSearchedNeighbors(Hex end)
    {
        List<Hex> neighbors = new List<Hex>(TileManager.TM.getValidNeighbors(end));
        List<Hex> temp = new List<Hex>();
        foreach(var hex in neighbors)
        {
            if (searched.Contains(hex))
                temp.Add(hex);
        }

        return temp;
    }

    private List<Hex> pathTrace(List<Hex> path)
    {
        Hex pathEnd = path[path.Count - 1];

        if (path.Contains(start))
        {
            return path;
        }

        List<Hex> searchedNeighbors = getSearchedNeighbors(pathEnd);
        

        if(searchedNeighbors.Count == 0)
        {
            Debug.LogError("TraceBottleneck");
            return pathTrace(path.GetRange(0, path.Count - 1));
        }
        

        Hex closest = new Hex();
        int closestDist = int.MaxValue;

        foreach (var hex in searchedNeighbors )
        {
            if (path.Contains(hex))
                continue;

            int dist = distance(hex, start);

            if (dist == 0)
            {
                closest = hex;

                path.Add(closest);

                return path;
            }

            else if (dist < closestDist)
            {
                
                closest = hex;
                closestDist = dist;
            }
            else if (dist == closestDist)
            {
                if(Random.Range(0, 2) == 1)
                    closest = hex;
            }

        }

        path.Add(closest);

        return pathTrace(path);

    }


    /// <summary>
    /// distance from to
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    private int distance(Hex from, Hex to)
    {
        return BoardHelperFns.distance(from, to);
    }



    private List<Hex> ValidNeighbors(Hex hex)
    {
        List<Hex> temp = new List<Hex>( TileManager.TM.getValidNeighbors(hex));

        int i = 0;

        while(i < temp.Count )
        {
            int tileowner = TileManager.TM["Crops", temp[i]].tileOwner;

            if(false && temp[i] == start)
            {
                return new List<Hex>() { start };
            }

            if (searched.Contains(temp[i]) ||
                    (tileowner != -1 &&
                     tileowner != owner)
                    )
            {
                temp.RemoveAt(i);
            }


            else
                i++;


        }

        return temp;
    }
}
