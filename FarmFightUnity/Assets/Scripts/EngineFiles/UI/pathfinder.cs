using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// finds a path for the soldier to travel
/// </summary>
public class PathFinder
{
    public List<Hex> Path;

    private HashSet<Hex> searched;

    private Hex start, end;

    private int owner;

    private SoldierTrip trip;



    public PathFinder(Hex start, Hex end, int owner)
    {
        this.start = start;
        this.end = end;
        this.owner = owner;
        searched = new HashSet<Hex>() { start };

        Path = PathBuilder();

        //if (path != null)
            //path.Reverse();
    }


    private List<Hex> PathBuilder()
    {
        
        if (ForwardTrace())
        {
            //Debug.Log("Forward Finished");

            searched.Add(end);
            

            var RawPath = BackwardTrace();
            
            return ReverseAndTrim(RawPath);
        }
        else
        {
            Path = null;
            return null;
        }



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


    // helpers for forward stepping

    /// <summary>
    /// searches for valid neighbors to the last step and adds them to searched and the stepList
    /// </summary>

    private bool NextStep(List<Hex[]> steps)
    {
        // base case for recursive function
        if (steps.Count == 0)
            return false;

        List<Hex> nextStep = getNextStep(steps[steps.Count - 1]);


        // if there is no way forward discard the current step and try again
        if (nextStep.Count == 0)
        {

            return NextStep(steps.GetRange(0, steps.Count - 1));
        }

        // add the found values to the searched set
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

        foreach (var hex in step)
        {
            nextProspects.UnionWith(ValidNeighbors(hex));
        }


        if (nextProspects.Count == 0)
            return new List<Hex>();


        List<Hex> closestHexes = new List<Hex>();

        int closestDist = int.MaxValue;


        foreach (var hex in nextProspects)
        {
            int dist = distance(end, hex);
            /// if its better than our best, get rid of the current and record it
            if (dist < closestDist)
            {
                closestDist = dist;
                closestHexes.Clear();
                closestHexes.Add(hex);
            }
            // if it is equal to our best, add it to the list
            else if (dist == closestDist)
            {
                closestHexes.Add(hex);
            }

            /// if it is worse, we dont care about it

        }


        return closestHexes;

    }


    private List<Hex> ValidNeighbors(Hex hex)
    {
        List<Hex> temp = new List<Hex>(TileManager.TM.getValidNeighbors(hex));

        int i = 0;

        while (i < temp.Count)
        {



            int tileOwner = TileManager.TM["Crops", temp[i]].tileOwner;


            

            if (temp[i] == end &&
                TileManager.TM["Crops", hex].tileOwner != tileOwner)
            {

               return new List<Hex>() { end };
            }


            else if (searched.Contains(temp[i]) ||
                    (tileOwner != -1 &&
                     tileOwner != owner)
                    )
            {
                temp.RemoveAt(i);
            }


            else
                i++;


        }

        return temp;
    }







    /// <summary>
    /// step backward to get the fastest single path 
    /// </summary>
    
    private List<Hex> BackwardTrace()
    {
        List<Hex> path = new List<Hex>() { end };
        return pathTrace(path);
    }


    /// backstep helper functions

    /// finds all neighbors in the searched set
    private List<Hex> getSearchedNeighbors(Hex end)
    {
        List<Hex> neighbors = new List<Hex>(TileManager.TM.getNeighbors(end));

        List<Hex> temp = new List<Hex>();

        foreach (var hex in neighbors)
        {
            if (hex == end)
                return new List<Hex>() { end };

            if (searched.Contains(hex))
                temp.Add(hex);

        }

        return temp;
    }

    /// <summary>
	/// FIND THE CLOSEST Hex to another hex
	/// </summary>
	/// <param name="goal"></param>
	/// <param name="options"></param>
	/// <param name="path"></param>
	/// <returns></returns>
    public Hex getClosest(Hex goal, List<Hex> options, List<Hex> path)
    {
        Hex? closest = null;

        int closestDist = int.MaxValue;

        
        Debug.Assert(options.Count > 0 , "What The Fuck How?");
        

        foreach (var hex in options)
        {
            if (path.Contains(hex) && closest != null)
            {
                continue;
            }
                

            int dist = distance(hex, goal);

            if (dist == 0)
            {
                return hex;
            }


            else if (dist < closestDist)
            {

                closest = hex;
                closestDist = dist;
            }

            /// 
            /// select straightest path
            
            else if(dist == closestDist  )
            {
                if(path.Count > 1)
                {

                    Hex current = path[path.Count - 1];
                    Hex before = path[path.Count - 2];

                    Hex close = closest ?? before;

                    closest = StraightOMeter(before-current, hex-current)
                        > StraightOMeter(before-current, close-current) ?
                        hex : close;

                     
                }
                else //if (Random.Range(0, 2) == 1) // Sorry Courtney, I disabled this for now since it was causing my client and server to not move together
                {
                    
                        closest = hex;
                }
                
            }
            
        }
        if (closest == null)
            Debug.LogError("somehow nothing is less than maxint");

        return closest ?? goal;
    }


    /// <summary>
    /// Recursive function to step through searched back to beginning
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private List<Hex> pathTrace(List<Hex> path)
    {
        // the end of the path
        Hex pathEnd = path[path.Count - 1];

        // too long
        if (path.Count >= 30)
        {
            Debug.LogError("Path too long");

        }

        /// start has been found
        if (path.Contains(start))
        {
            return path;
        }

        // the neighbors that have been searched
        List<Hex> searchedNeighbors = getSearchedNeighbors(pathEnd);



        // if there is no way forward
        if (searchedNeighbors.Count == 0)
        {
            //Debug.LogError($"Trace Bottleneck. length = {path.Count}");

            /// we know this path Back doesnt work so remove it from the search
            searched.Remove(pathEnd);
            return pathTrace(path.GetRange(0, path.Count - 1));
        }



        /// the next section is about finding the closest neighbor

        Hex closest = getClosest(start, searchedNeighbors, path);

        searched.Remove(closest);
        path.Add(closest);

        return pathTrace(new List<Hex>(path));

    }


    public int StraightOMeter(Hex from, Hex to)
    {

        if (from == -to)
        {
            return 3;
        }

        else
        {
            return distance(from, to);
        }

    }


    private List<Hex> ReverseAndTrim(List <Hex> rawPath)
    {

        
        Debug.Assert(rawPath[rawPath.Count - 1] == start, "start not at end");
        Debug.Assert(rawPath[0] == end, "end not at start");

        //get the end of the Rawpath, which is the start of the path
        List<Hex> temp = new List<Hex>() { rawPath[rawPath.Count - 1] };

        
        while (temp[temp.Count - 1] != end)
        {
            
            foreach (var hex in rawPath)
            {
                

                
                if (isNeighbor(temp[temp.Count - 1], hex))
                   
                {
                    
                        temp.Add(hex);
                        break;
                    
                    
                }

                
            }
        }

        return temp;
        


    }

    private bool isNeighbor(Hex to, Hex from)
    {
        return
            to - from == new Hex(0, 1) ||
            to - from == new Hex(1, 0) ||
            to - from == new Hex(1, -1) ||
            to - from == new Hex(0, -1) ||
            to - from == new Hex(-1, 0) ||
            to - from == new Hex(-1, 1);

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


    

   

}
