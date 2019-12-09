using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class CellularAutomata : MonoBehaviour
{
    // Start is called before the first frame update
    public Volume volume;
    public delegate void Task();
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            volume.DoStep();
        }
    }
}

