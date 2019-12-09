/*
 * TODO Create
 */


using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UIElements;

public class Volume : MonoBehaviour
{
    public enum CellAction {Destroy, Create, Idle, IgnorePos}
    
    public float stepTime = 2.0f;
    public int radius = 10;
    public float xUnit, yUnit, zUnit = 1.0f;
    public RuleSet ruleSet;
    
    public delegate void CellRuleSimple(byte neighborCount);
    public bool simpleCellRule = false;
    
    protected internal ConcurrentDictionary<Vector3, Cell> _cells = new ConcurrentDictionary<Vector3, Cell>();
    protected internal HashSet<Vector3> _interestingCells;
    
    // Start is called before the first frame update
    void Start()
    {
        AddCell(new Vector3(0,0,0));
    }

    public void AddCell(Vector3 position)
    {
        var cellObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _cells.TryAdd(position,cellObj.AddComponent<Cell>());
        
        for (var x = position.x - xUnit; x <= position.x + 1; x += xUnit)
        {
            for (var y = position.y - yUnit; y <= position.y + 1; y += yUnit)
            {
                for (var z = position.z - zUnit; z <= position.z + 1; z += zUnit)
                {
                    _interestingCells.Add(new Vector3(x,y,z));
                }
            }
        }
    }

    public void RemoveCell(Vector3 position)
    {
        Cell value;
        _cells.TryRemove(position, out value);
    }
    
    public CellAction EvaluatePoint(Vector3 position)
    {
        byte neighborCount = 0;
        int  neighbors = 0; /*A cell has 26 possible neighbors; All we care about is whether or not an adjacent cell is occupied.
                                    So, we are going to store that in an int and use only 26 of the bits, where each adjacent cell is represented by a bit,
                                    with 0 meaning that the cell is empty, and 1 meaning that it is occupied.
                                    
                                    A cell's neighbors are organized into three layers:
                                                                     TOP3      MID3      BTM3
                                                    (1,1,1)------>  X X X    X X X    X X X  
                                                                    X X X    X O X    X X X
                                                                    X X X    X X X    X X X (-1,-1,-1) <-------
                                            ... where X's are adjacent to the cell of interest O                                    
                                    We store that as this:
                                            TOP3:          MID3:           BTM3: 
                                               top mid btm    top ... ...   ... ... ...
                                               XXX XXX XXX    XXX XOX XXX   XXX XXX XXX
                                               [0, 1, 2, ...               ..., 24, 25]*/
        bool isCellOccupied = _cells.ContainsKey(position);
        for (var x = position.x - xUnit; x <= position.x + xUnit; x++)
        {
            for (var y = position.x - yUnit; y <= position.y + yUnit; y++)
            {
                for (var z = position.z - zUnit; z <= position.z + zUnit; z++)
                {
                    var vec = new Vector3(x, y, z);
                    if(_cells.ContainsKey(vec))
                    {
                        if (position != vec)
                        {
                            neighbors++; //set this cell's bit to 1;
                            neighborCount++;
                        }
                            
                    }
                    neighbors <<= 1; //shift bit to the left;
                }
            }
        }

        if (neighborCount == 0 && isCellOccupied == false) return CellAction.IgnorePos;
        return ruleSet.CellRule(neighbors, neighborCount, isCellOccupied);
    }

    public void DoStep()
    {
        List<Action> actionQueue = new List<Action>();
        foreach (var pos in _interestingCells)
        {
            var response = EvaluatePoint(pos);
            if (response == CellAction.Create)
            {
                actionQueue.Add(() => { AddCell(pos); });
            }
            else if (response == CellAction.Destroy)
            {
                actionQueue.Add(() => { RemoveCell(pos); });
            }
            else if(response == CellAction.IgnorePos)
            {
                actionQueue.Add(() => { _interestingCells.Remove(pos);});
            }
        }
        
        foreach(var action in actionQueue)
        {
            action();
        }
    }
}