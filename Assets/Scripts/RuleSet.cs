using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is a template class for a ruleset. All functions must be defined in a ruleset in order to be used in this program.
/// </summary>
public class RuleSet : MonoBehaviour
{
    /// <summary>
    /// Reference to volume object
    /// </summary>
    public Volume volume;

    public RuleSet(Volume volume)
    {
        this.volume = volume;
    }

     public virtual Volume.CellActionID CellRule(int neighbors, byte neighborCount, bool isCellOccupied)
    {
        return Volume.CellActionID.Idle; 
    }
}