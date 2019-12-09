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
    
    public override Volume.CellActionID CellRule(int neighbors, byte neighborCount, bool isCellOccupied)
    {
        if (isCellOccupied)
        {
            if (neighborCount > 7)
            {
                return Volume.CellActionID.Destroy;
            }
            else if (neighborCount >= 5)
            {
                return Volume.CellActionID.Idle;
            }
            else
            {
                return Volume.CellActionID.Destroy;
            }
        }
        else
        {
            if (neighborCount == 6)
            {
                return Volume.CellActionID.Create;
            }
            else
            {
                return Volume.CellActionID.IgnorePos;
            }
        }
    }
}
