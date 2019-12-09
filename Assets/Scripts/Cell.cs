using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : Volume
{
    private byte neighborCount;
    private Volume parentVolume;
    private GameObject cellObject;

    public Cell()
    {
        
    }

    ~Cell()
    {
        GameObject.Destroy(cellObject);
    }

}
