using System.Collections.Generic;
using UnityEngine;

// Define the DesktopManager class to control opening and closing of apps
// Define the DesktopManager class to control opening and closing of apps
public class DesktopManager : MonoBehaviour
{
    public List<App> apps; // List to store references to app scripts

    public App currentAppInstance; // Reference to the currently opened app instance

    public static DesktopManager Instance { get; private set; }

    private void Start()
    {
        Instance = this;
        // // Close all apps when DesktopManager starts
        // CloseAllApps();
    }

    public void CloseAllApps()
    {
        // Iterate through all app prefabs and close them
        foreach (App appPrefab in apps)
        {
            appPrefab.Close();
            
        }
    }

    public void OpenApp(int index)
    {
        // Ensure index is within range
        if (index >= 0 && index < apps.Count)
        {
            App appScript = apps[index];

            if (appScript == currentAppInstance)
            {
                // Do nothing if the app is already open
                return;
            }

            if (currentAppInstance != null)
            {   
                // Minimize currently opened app that is not the opened one
                MinCurrentApp();
            }

            currentAppInstance = appScript;
            currentAppInstance.Open();
        }
        
        else
        {
            Debug.LogError("Index out of range.");
        }
    }

    public void CloseCurrentApp()
    {
        if (currentAppInstance != null)
        {
            currentAppInstance.Close();
            currentAppInstance = null;
        }
    }

    public void MinCurrentApp()
    {
        if (currentAppInstance != null)
        {
            currentAppInstance.Minimize();
            currentAppInstance = null;
        }
    }

    public App CurrentAppInstance {
        get => currentAppInstance;
        set => currentAppInstance = value;
    }
}
