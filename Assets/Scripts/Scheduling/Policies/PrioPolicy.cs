// MIKO TRISTAN ABADIA

using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;

public class PrioPolicy : Policy
{
    public override Process GetRunningProcess(List<Process> processList)
    {
        if (processList.Count == 0)
            return null; // No process to run
        
        // Initialize the first ready process as the prioritized job
        Process prioritizedJob = null;
        
        foreach (Process process in processList)
        {   

            // Check if the process is ready and has shorter burst time than current prioritized.
            if ((process.status == Status.Running || process.status == Status.Ready) && 
                (prioritizedJob == null || process.priority > prioritizedJob.priority))
            {
                prioritizedJob = process;
            }

            // Age processes in ready queue with interval 1
            if (process.priority < 100) process.priority++;

            /*
                Logic already handles FCFS if there are more than one of the same prioritized burst time.
                For instance, if current most prioritized has 5 priority and the next process has also 5 priority,
                the most prioritized job will not be changed since 5 !< 5.
                Since the list is ordered from the arrival of process, it makes sense that 
                for every same prioritized time, the current (i.e. the one who first came) is the one executed.
            */
        }
        
        return prioritizedJob;
    }
}