using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FiveSevenSixSix : RuleSet
{
    public FiveSevenSixSix(Volume volume) : base(volume)
    {
        this.volume = volume;
    }
    
    public override Volume.CellAction CellRule(int neighbors, byte neighborCount, bool isCellOccupied)
    {
        if (isCellOccupied)
        {
            if (neighborCount < 5)
            {
                return Volume.CellAction.Destroy;
            }
            else if (neighborCount >= 7)
            {
                return Volume.CellAction.Destroy;
            }
            else
            {
                return Volume.CellAction.Idle;
            }
        }
        else
        {
            if (neighborCount == 6)
            {
                return Volume.CellAction.Create;
            }
            else
            {
                return Volume.CellAction.Idle;
            }
        }
    }
}
