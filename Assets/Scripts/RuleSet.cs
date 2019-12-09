using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuleSet : MonoBehaviour
{
    /*This class is a template class for a ruleset. All functions must be defined in a ruleset in order to be used in this program.*/
    public Volume volume;

    public RuleSet(Volume volume)
    {
        this.volume = volume;
    }

     public virtual Volume.CellAction CellRule(int neighbors, byte neighborCount, bool isCellOccupied)
    {
        return Volume.CellAction.Idle; 
    }



}
