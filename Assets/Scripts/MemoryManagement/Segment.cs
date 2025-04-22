using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Segment 
{
    public int baseAdr { get; private set; }
    public int endAdr  { get; private set; }
    public Process process  { get; private set; }
    public GameObject segmentObjRef { get; private set; }

    public Segment(int baseAdr, int limit, Process process) {
        this.baseAdr = baseAdr;
        this.endAdr = baseAdr + limit;
        this.process = process;
    }

    public void SetObjectRef(GameObject objReference)
    {
        this.segmentObjRef = objReference;
    }

}