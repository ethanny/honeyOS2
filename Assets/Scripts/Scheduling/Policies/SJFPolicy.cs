// LIAM IÑAKI GILLAMAC

using System;
using System.Collections;
using System.Collections.Generic;

public class SJFPolicy : Policy
{
    public override Process GetRunningProcess(List<Process> processList)
    {   
        // No process to run
        if (processList.Count == 0)
            return null; 

        // Initialize the first ready process as the shortest job
        Process shortestJob = null;
        
        foreach (Process process in processList)
        {   

            // Check if the process is ready and has shorter burst time than current shortest.
            if ((process.status == Status.Running || process.status == Status.Ready) && 
                (shortestJob == null || process.burstTime < shortestJob.burstTime))
            {
                shortestJob = process;
            }

            /*
                Logic already handles FCFS if there are more than one of the same shortest burst time.
                For instance, if current shortest has 5 burst time and the next process has also 5 burst time,
                the shortest job will not be changed since 5 !< 5.
                Since the list is ordered from the arrival of process, it makes sense that 
                for every same shortest time, the current (i.e. the one who first came) is the one executed.
            */
        }
        
        return shortestJob;
    }
}