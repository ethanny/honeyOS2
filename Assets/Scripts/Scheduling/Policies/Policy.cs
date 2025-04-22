using System;
using System.Collections;
using System.Collections.Generic;

public abstract class Policy
{
    public abstract Process GetRunningProcess(List<Process> processList);
}
