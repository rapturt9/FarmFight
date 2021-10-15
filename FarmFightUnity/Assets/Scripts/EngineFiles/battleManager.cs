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
    static int[] DamageCalculate(Dictionary<int, List<Soldier>> SortedSoldiers, int owner)
    {

        int[] col = new int[6];
        int total = 0;

        foreach(var player in SortedSoldiers)
        {
            col[player.Key] = player.Value.Count;
            total += player.Value.Count;
        }


        return new int[]
        {
            (total - 1)/ col[0],
            (total - 1)/ col[1],
            (total - 1)/ col[2],
            (total - 1)/ col[3],
            (total - 1)/ col[4],
            (total - 1)/ col[5]
        };
        

    }

    

    static void DamageDealing(Dictionary<int, List<Soldier>> SortedSoldiers, int[] damages)
    {
        foreach(var player in SortedSoldiers)
        {
            foreach(var soldier in player.Value)
            {
                soldier.Health.Value -= damages[player.Key];
            }
        }
    }



    static void KillSoldiers(List<Soldier> soldiers)
    {

        
        int i = 0;
        while (i < soldiers.Count)
        {
            Soldier soldier = soldiers[0];

            if (soldiers[i].Health.Value <= 0)
            {

                soldiers.RemoveAt(0);
                soldier.Kill();
            }
            else
            {
                i++;
            }
        }
        
        
    }



    public static void BattleFunction(Dictionary<int, List<Soldier>> SortedSoldiers, List<Soldier> soldiers, int owner)
    {
        int[] damages = DamageCalculate(SortedSoldiers, owner);

        DamageDealing(SortedSoldiers, damages);

        KillSoldiers(soldiers);
    }
}