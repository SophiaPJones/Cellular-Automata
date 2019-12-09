/*
 * TODO Create
 */


using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading;
using Aura2API;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class Volume : MonoBehaviour
{   
    
    
    public enum CellActionID {Destroy, Create, Idle, IgnorePos} //We are going to define a set of actions that a cell can experience
    
    public struct CellAction 
    {
        public Vector3 position;
        public CellActionID action;
        public CellAction(Vector3 position, CellActionID action)
        {
            this.position = position;
            this.action = action;
        }
    }
    
    public float stepTime = 2.0f;
    public int radius = 10;
    public float xUnit = 1.0f;
    public float yUnit = 1.0f;
    public float zUnit = 1.0f;
    public RuleSet ruleSet;
    private GameObject cellsWithin;
    private CellularAutomata cellularAutomata;
    public delegate void CellRuleSimple(byte neighborCount);
    public bool simpleCellRule = false;

    public GameObject cellPrefab;
    
    private ConcurrentDictionary<Vector3, bool> _interestingCells = new ConcurrentDictionary<Vector3, bool>();
    private ConcurrentDictionary<Vector3, GameObject> _cells = new ConcurrentDictionary<Vector3, GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        cellsWithin = GameObject.Find("Space");
        cellularAutomata = cellsWithin.GetComponent<CellularAutomata>();
        List<int> randomx = new List<int>();
        List<int> randomy = new List<int>();
        List<int> randomz = new List<int>();

        for (int i = 0; i < 150; i++)
        {
            var randX = Random.RandomRange(0, 23);
            var randY = Random.RandomRange(0, 23);
            var randZ = Random.RandomRange(0, 23);
            AddCell(new Vector3(randX,randY,randZ));
        }
        return;
    }

    public void AddCell(Vector3 position)
    {
        var cellObj = GameObject.Instantiate(cellPrefab,
            position,
            Quaternion.identity,
            GameObject.FindGameObjectWithTag("Space").transform);
        
        cellObj.SetActive(true);
        
        _interestingCells.AddOrUpdate(position, 
                            true, 
                    (oldKey, oldValue) => { return oldValue;});
        _cells.AddOrUpdate(position, cellObj, (oldKey, oldVal) => { return oldVal;});
        
        for (var x = position.x - xUnit; x <= position.x + 1; x += xUnit)
        {
            for (var y = position.y - yUnit; y <= position.y + 1; y += yUnit)
            {
                for (var z = position.z - zUnit; z <= position.z + 1; z += zUnit)
                {
                    if (x != position.x && y != position.y && z != position.z)
                    {
                        _interestingCells.AddOrUpdate(new Vector3(x,y,z),
                            false,
                            (oldKey, oldValue) => { return oldValue;});
                    }
                }
            }
        }

        return;
    }

    public void RemoveCell(Vector3 position)
    {
        GameObject gameobj;
        _interestingCells.AddOrUpdate(position, false, (oldkey, oldval) => { return oldval;});
        _cells.TryRemove(position, out gameobj);
        gameobj.Destroy();
        return;
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
                    if(_interestingCells.ContainsKey(vec))
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

        if (neighborCount == 0 && isCellOccupied == false) return new CellAction(position, CellActionID.IgnorePos);
        return new CellAction(position, ruleSet.CellRule(neighbors, neighborCount, isCellOccupied));
    }

    public void DoStep()
    {
        List<Action> actionList = new List<Action>();
        foreach (var cell in _interestingCells)
        {
            var pos = cell.Key;
            CellAction response = EvaluatePoint(pos);
            switch (response.action)
            {
                case CellActionID.Create:
                    actionList.Add(() => { AddCell(pos);});
                    Debug.Log(String.Format("Cell CREATED at {0}.", pos));
                    break;
                case CellActionID.Destroy:
                    actionList.Add(() => { RemoveCell(pos);});
                    Debug.Log(String.Format("Cell REMOVED at {0}.", pos));
                    break;
                case CellActionID.IgnorePos:
                    bool dump;
                    actionList.Add(() =>
                    {
                        _interestingCells.TryRemove(pos,out dump);
                        GameObject gameObj;
                        _cells.TryRemove(pos, out gameObj);
                        gameObj.Destroy();
                    });
                    Debug.Log(String.Format("Cell IGNORED at {0}.", pos));
                    break;
            }
        }

        foreach (var action in actionList)
        {
            action();
        }
        return;
    }
}