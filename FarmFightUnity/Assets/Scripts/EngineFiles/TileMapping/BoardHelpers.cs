using System.Collections.Generic;

using UnityEngine;

/// <summary>
/// Here to keep the Board code nice and readable
/// </summary>
public static class BoardHelperFns
{



    public static int distance(Hex pt1, Hex pt2)
    {
        if ((pt1.x - pt2.x) * (pt1.y - pt2.y) >= 0)
        {
            int a = Mathf.Abs(pt1.x - pt2.x);
            int b = Mathf.Abs(pt1.y - pt2.y);
            return a + b;
        }

        else
        {
            int a = Mathf.Abs(pt1.x + pt2.x);
            int b = Mathf.Abs(pt1.y - pt2.y);
            return a < b ? a : b;
        }
    }

    public static Dictionary<Hex, TileInterFace> BoardFiller(int n)
    {
        Dictionary<Hex, TileInterFace> final = new Dictionary<Hex, TileInterFace>();

        for (int i = -n; i <= n; i++)
        {
            for (int ii = -n; ii <= n; ii++)
            {
                if (distance(Hex.zero, new Hex(i, ii)) <= n)
                    final[new Hex(i, ii)] = new TileInterFace(new Hex(i, ii), new BlankTile());
            }
        }

        return final;
    }

    public static List<Hex> HexList(int n)
    {
        List<Hex> final = new List<Hex>();

        for (int i = -n; i <= n; i++)
        {
            for (int ii = -n; ii <= n; ii++)
            {
                if (distance(Hex.zero, new Hex(i, ii)) <= n)
                    final.Add( new Hex(i, ii));
            }
        }

        return final;
    }

    // Used for MLAPI syncing, since it doesn't like Hexes
    public static int[] HexToArray(Hex coord)
    {
        return new int[2] { coord.x, coord.y };
    }

    public static Hex ArrayToHex(int[] coord)
    {
        return new Hex(coord[0], coord[1]);
    }
}
