// using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Process : MonoBehaviour
{   
    public GameObject prefab { get; set; }
    public TMP_Text idText;
    public TMP_Text priorityText;
    public TMP_Text arrivalTimeText;
    public TMP_Text burstTimeText;
    public TMP_Text memorySizeText;
    public TMP_Text statusText;

    private Memory memory;

    public int id { get; private set; }
    public int priority { get; set; }
    private int arrivalTime;
    public int burstTime { get; private set; }
    public int memorySize { get; private set; }
    public Status status { get; private set; }
    public int waitTime {get; private set; }
    public int quantumTime {get; private set;}
    public GameObject objReference { get; private set; }


    // Init attributes
    public void InitAttributes(GameObject objReference, int id, int arrivalTime)
    {   
        this.objReference = objReference;
        this.id = id;
        this.arrivalTime = arrivalTime;
        priority = Random.Range(1,101); // Generate a number from 1 to 100
        burstTime = Random.Range(1,11);
        memorySize = Random.Range(64,129);
        // SetStatus(Status.New);
        // waitTime = Random.Range(0,10);
        waitTime = 0;
        quantumTime = 4;
    }

    void Awake()
    {
        memory = Memory.Instance;
        // memory
    }
  
    public void UpdateAttributes() 
    {
        idText.text = id.ToString();
        priorityText.text = priority.ToString();
        arrivalTimeText.text = arrivalTime.ToString();
        burstTimeText.text = burstTime.ToString();
        memorySizeText.text = memorySize.ToString();
        
        if (IsStatus(Status.Waiting))
        {   
            if (waitTime > 0)
                statusText.text = status.ToString() + "(" + waitTime + ")";
            else
                statusText.text = "Waiting for memory";
        }
            
        else
            statusText.text = status.ToString();

    }

    public void UpdateStatus()
    {   
        if (status != Status.Terminated)
        {   
            // Set status to ready if waitTime == 0 and there is enough memory (one-time on first setting status to ready)
            // !IsStatus(Status.Ready) ensures that we do not repeat checking if there is enough memory and set status again
            // Switching from running to ready is handled in ProcessManager.UpdateProcessses()
            if (waitTime == 0 && !IsStatus(Status.Ready) && !IsStatus(Status.Running) && memory.HasMemory(this)) 
                SetStatus(Status.Ready);
            
            // If waiting or there is not enough memory
            // !IsStatus(Status.Ready) ensures that we do not go back to waiting once a process is ready
            else if (waitTime >= 0 && !IsStatus(Status.Ready) && !IsStatus(Status.Running)) 
                SetStatus(Status.Waiting);
           
            else if (burstTime == 0) 
                SetStatus(Status.Terminated); 
        }

        // Change to Status.Running in ProcessManager
    }

    // public Status GetStatus()
    // {
    //     return status;
    // }

    public void SetStatus(Status newStatus) { status = newStatus; }

    private bool IsStatus(Status toCheckStatus) { return status == toCheckStatus; }

    public void DecBurstTime()	{ if (burstTime != 0) burstTime--; }
    public void IncBurstTime()	{ burstTime++; }
    public void DecWaitTime() 	{ if (waitTime != 0) waitTime--; }
    public void IncWaitTime()   { waitTime++; }
    public void DecQuantumTime() {
        if (quantumTime == 0 || burstTime == 0){
            quantumTime = 4;
        }
        quantumTime--;
    }
    public void IncQuantumTime() {quantumTime++;}

    // private int Randomize(int min, int max) 
    // {
    //     // Implementation of your Randomize method
    //     return new Random().Next(min, max);
    // }

}

public enum Status { New, Ready, Waiting, Running, Terminated}