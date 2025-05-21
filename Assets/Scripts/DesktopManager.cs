using System.Collections.Generic;
using UnityEngine;

public class DesktopManager : MonoBehaviour
{
    public List<App> apps; // List of app scripts (Sweet, Cake, Sugar)
    public static DesktopManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void OpenApp(int index)
    {
        if (index >= 0 && index < apps.Count)
        {
            apps[index].Open(); // ✅ Just open the app — no hiding others
        }
        else
        {
            Debug.LogError("Index out of range.");
        }
    }

    public void CloseApp(int index)
    {
        if (index >= 0 && index < apps.Count)
        {
            apps[index].Close();
        }
    }

    public void MinimizeApp(int index)
    {
        if (index >= 0 && index < apps.Count)
        {
            apps[index].Minimize();
        }
    }

    public void CloseAllApps()
    {
        foreach (App app in apps)
        {
            app.Close();
        }
    }
}
