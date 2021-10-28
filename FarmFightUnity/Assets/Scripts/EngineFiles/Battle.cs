using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// here to keep croptiles semiClear
/// so a battle based functions are put here
/// </summary>
public static class Battle
{

    /// <summary>
    /// Rough approximation of 
    /// </summary>
    /// <param name="SortedSoldiers"></param>
    /// <param name="owner"></param>
    /// <returns></returns>
    static float[] DamageCalculate(Dictionary<int, List<Soldier>> SortedSoldiers, int soldierCount, int owner)
    {
        //int[] col = new int[7];
        //int total = 0;

        //foreach(var player in SortedSoldiers)
        //{
        //    col[player.Key] = player.Value.Count;
        //    total += player.Value.Count;
        //}


        //return new int[]
        //{
        //    (total - 1)/ col[0],
        //    (total - 1)/ col[1],
        //    (total - 1)/ col[2],
        //    (total - 1)/ col[3],
        //    (total - 1)/ col[4],
        //    (total - 1)/ col[5]
        //};

        float[] damages = new float[Repository.maxPlayers];
        for (int playerId = 0; playerId<Repository.maxPlayers; playerId++)
        {
            damages[playerId] = (soldierCount - SortedSoldiers[playerId].Count) / (float)SortedSoldiers[playerId].Count;
        }
        return damages;

    }



    static void DamageDealing(Dictionary<int, List<Soldier>> SortedSoldiers, float[] damages)
    {
        foreach(var player in SortedSoldiers)
        {
            foreach(var soldier in player.Value)
            {
                float mult = 1;
                if(player.Key == Repository.Central.tileinfo.homePlayer){
                    mult = .8f;
                }
                soldier.Health.Value -= mult * damages[player.Key];
            }
        }
    }



    static bool KillSoldiers(Dictionary<int, List<Soldier>> SortedSoldiers)
    {
        bool killed = false;
        // Get all dead soldiers
        for (int playerId = 0; playerId < Repository.maxPlayers; playerId++)
        {
            foreach (var soldier in SortedSoldiers[playerId])
            {
                if (soldier.Health.Value <= 0)
                {
                    soldier.Kill();
                    Debug.Log("killed soldier owned by " + soldier.owner.Value.ToString());
                    killed = true;
                }
            }
        }
        return killed;
    }


    // Returns true if we've killed a soldier
    public static bool BattleFunction(Dictionary<int, List<Soldier>> SortedSoldiers, int soldierCount, int owner)
    {
        float[] damages = DamageCalculate(SortedSoldiers, soldierCount, owner);

        DamageDealing(SortedSoldiers, damages);

        return KillSoldiers(SortedSoldiers);
    }
}
