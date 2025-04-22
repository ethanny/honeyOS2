// DAVE NOCEDA

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class RRPolicy : Policy
{
    private List<Process> processQueue = new List<Process>();
    //  private HashSet<Process> queuedProcesses = new HashSet<Process>();
    public override Process GetRunningProcess(List<Process> processList)
    {
        if (processList.Count == 0)
            return null; // No process to run
        
        foreach (Process process in processList)
        {
            if (!processQueue.Contains(process)) 
            // && !queuedProcesses.Contains(process))
            {
                // Debug.Log("Processes Added: ");
                // Debug.Log("Process ID: " + process.id);
                processQueue.Add(process);
                // queuedProcesses.Add(process);
            }
        }

        // foreach (Process proc in processQueue){
        //     Debug.Log("process: " + proc);
        // }

        if (processQueue.Count == 0)
            return null;
        
        //Gets the next process
        Process currentProcess = processQueue.First();
        // Debug.Log("Running: " + currentProcess.id);
        currentProcess.DecQuantumTime();
        // Debug.Log("quantumTime: " + currentProcess.quantumTime);

        //Check if the quantum time is 0
        if (currentProcess.quantumTime <= 0 || currentProcess.burstTime <= 0){
            processQueue.RemoveAt(0);
            if(currentProcess.burstTime > 1)
                processQueue.Add(currentProcess);
        }
        
        return currentProcess;
    }
}