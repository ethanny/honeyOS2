// AGNES NAPOLES

using System;
using System.Collections;
using System.Collections.Generic;

public class FCFSPolicy : Policy
{
    public override Process GetRunningProcess(List<Process> processList)
    {
        // No process to run
        if (processList.Count == 0)
            return null; 

        // Initialize the first ready process as the first job
        Process firstJob = null;
        
        foreach (Process process in processList)
        {   

            if ((process.status == Status.Running || process.status == Status.Ready) && 
                (firstJob == null || process.id < firstJob.id))
            {
                firstJob = process;
            }
        }
        
        return firstJob;
    }
}