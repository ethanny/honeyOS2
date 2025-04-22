// using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ProcessManager : MonoBehaviour
{
    private int processCount;
    private List<Process> processes;
    private List<Process> readyQueue;
    private List<Process> jobQueue;
    private Process runningProcess;
    private Process prevRunningProcess;
    private bool isPlaying;
    // private Stack stack;
    private Policy schedulingPolicy;
    private int time;

    // private Memory memory;
    public Memory memory { get; private set; }

    public GameObject readyContainer;
    public GameObject jobContainer;
    public GameObject processPrefab;

    public TMP_Text schedPolicyText;
    public TMP_Text timeText;
  
    public 
    void Awake() {
        processCount = 0;
        processes = new List<Process>();
        readyQueue = new List<Process>();
        jobQueue = new List<Process>();
        isPlaying = false;
        time = 0;
        memory = Memory.Instance;
        prevRunningProcess = null;
        timeText.text = time.ToString();
        // SetSchedulingPolicy(SchedPolicy.FCFS);
        

    }

    // public enum SchedPolicy { FCFS, SJF, Prio, RR }
    
    public void SetSchedulingPolicy(string policy)
    {
        switch (policy)
        {
            case "FCFS":
                schedulingPolicy = new FCFSPolicy();
                schedPolicyText.text = "First Come, First Serve";
                break;
            case "SJF":
                schedulingPolicy = new SJFPolicy();
                schedPolicyText.text = "Shortest Job First";
                break;
            case "Prio":
                schedulingPolicy = new PrioPolicy();
                schedPolicyText.text = "Priority";
                break;
            case "RR":
                schedulingPolicy = new RRPolicy();
                schedPolicyText.text = "Round Robin";
                break;
            default:
                // Handle unknown scheduling policies
                break;
        }
    }

	
    public void Play() 
    {
        isPlaying = true;
        StartCoroutine(CountDown());
    }

    private IEnumerator CountDown()
    {
        while(isPlaying)
        {
            yield return new WaitForSeconds(1f);
            Next();
        }
    }
        
    public void Next()
    {
        // // Get running process from chosen scheduling policy
        // runningProcess = schedulingPolicy.GetRunningProcess(processes);

        // if (runningProcess != null)
        // {

        //     if (runningProcess != prevRunningProcess) 
        //     {   
        //         if (prevRunningProcess != null)
        //             prevRunningProcess.SetStatus(Status.Ready);
                
        //         runningProcess.SetStatus(Status.Running);
        //         prevRunningProcess = runningProcess;
        //     }            
            
        //     ++processCount;
        //     ++time;
            
        //     runningProcess.DecBurstTime();
            
        //     foreach (Process process in processes) 
        //     {
        //         process.DecWaitTime();  // Decrement wait time for all processes
        //         process.UpdateStatus(); // Update status of all process
        //         process.UpdateAttributes();
                
        //         if (process.status == Status.Terminated)
        //             processes.Remove(process);
                
        //     }
            
        // }

        ++time;

        UpdateProcesses();

        // Decide whether to add new process 
        if (UnityEngine.Random.Range(1, 5) == 1) 
        {   
            --time; // To avoid double time increment
            AddProcess(true);
             
        }
        
    }
    // OPTIONAL
    // public void Previous()
    // {
    //     --processCount;
    //     --time;
        
    //     runningProcess.IncBurstTime();
        
    //     // Decrement wait time for all processes
    //     foreach (Process process in processes) 
    //     {
    //         process.IncWaitTime();
    //     }
    // }

    public void UpdateProcesses()
    {   
        // Update timeText text
        timeText.text = time.ToString();

        // Get running process from chosen scheduling policy
        runningProcess = schedulingPolicy.GetRunningProcess(readyQueue);

        if (runningProcess != null)
        {

            if (runningProcess != prevRunningProcess) 
            {   
                if (prevRunningProcess != null && prevRunningProcess.status != Status.Terminated) // Second part might not be necessary if delete lahos ang terminated
                    prevRunningProcess.SetStatus(Status.Ready);
                
                runningProcess.SetStatus(Status.Running);
                Debug.Log("nirunning");
                prevRunningProcess = runningProcess;
            }            
            
            runningProcess.DecBurstTime();
              
        }

        Process processToDestroy = null;

        foreach (Process process in processes) 
        {
            process.DecWaitTime();  // Decrement wait time for all processes
            process.UpdateStatus(); // Update status of all process
            process.UpdateAttributes();
            
            // Create new reference to terminated process since cant be done while iterating
            // Works since there is only one or no processes that will terminated at a time

            if (process.status == Status.Ready) {
                
                if (!readyQueue.Contains(process))
                {   
                    readyQueue.Add(process);

                    if (jobQueue.Contains(process))
                    {
                        jobQueue.Remove(process);
                        
                    }

                    // Move to ready table
                    process.prefab.transform.SetParent(readyContainer.transform, false);

                    // memory.HasMemory(process);
                    memory.AllocateMemory(process);
                }

                

            }

            else if (process.status == Status.Waiting) {
                
                if (!jobQueue.Contains(process))
                {
                    jobQueue.Add(process);
                    // Move to ready table
                    process.prefab.transform.SetParent(jobContainer.transform, false);
                }


            }

            else if (process.status == Status.Terminated) {
                processToDestroy = process;
            }    

        }

        // Remove terminated process if there is one
        if (processToDestroy != null)
        {
            memory.DeallocateMemory(processToDestroy);
            processes.Remove(processToDestroy);
            readyQueue.Remove(processToDestroy);
            Destroy(processToDestroy.objReference);
        }
            

    }
        
    public void Pause()
    {
        isPlaying = false;
    }
        
    public void Stop()
    {
        List<Process> processesToRemove = new List<Process>(processes); // Create a copy of the list

        foreach (Process process in processesToRemove)
        {
            processes.Remove(process); // Remove the process from the original list
            Destroy(process.objReference); // Destroy the associated GameObject
        }

        processes.Clear();
        processCount = 0;
        time = 0;
        timeText.text = time.ToString();

        memory.Reset();

        isPlaying = false;
    }
        
    public void AddProcess(bool isFromNext)
    {
        // Instantiate a new process with processCount as its ID and a default time
        // Process newProcess = new Process(processCount, defaultProcessTime);
        
        // Add the new process to the processes list
        // , processesContainer.transform)
        GameObject newProcess = Object.Instantiate(processPrefab);
        
        // Sets "processHolder" as the new parent of the process GameObject.
        // newProcess.transform.SetParent(processesContainer.transform); // Set position relative to parent
        // process.transform.SetParent(processHolder.transform, false); // Set position in global orientation

        Process process = newProcess.GetComponent<Process>();
        process.prefab = newProcess;
        processes.Add(process);
        process.InitAttributes(newProcess, ++processCount, ++time);
        process.UpdateAttributes();

        if (!isFromNext)
            UpdateProcesses();
    
    }
    
    // private void Randomize(int min, int max) {
    //     return Randomizer.i.Randomize(min, max + 1); // Include min and max
    // }
    
    // private Inc
}