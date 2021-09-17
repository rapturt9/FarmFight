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

    public static Dictionary<Hex, GameTile> BoardFiller(int n)
    {
        Dictionary<Hex, GameTile> final = new Dictionary<Hex, GameTile>();

        for (int i = -n; i <= n; i++)
        {
            for (int ii = -n; ii <= n; ii++)
            {
                if (distance(Hex.zero, new Hex(i, ii)) <= n)
                    final[new Hex(i, ii)] = GameTile.empty;
            }
        }

        return final;
    }
}
