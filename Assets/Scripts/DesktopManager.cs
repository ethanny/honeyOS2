using System.Collections.Generic;
using UnityEngine;

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
            App appToOpen = apps[index];

            // If clicking the icon of the currently open app
            if (appToOpen == currentAppInstance)
            {
                // Minimize it
                MinCurrentApp();
                return;
            }

            // If the app is already minimized (has an indicator but not current)
            if (appToOpen.appIcon.IsMinimized())
            {
                // If there's another app open, minimize it first
                if (currentAppInstance != null && currentAppInstance != appToOpen)
                {
                    MinCurrentApp();
                }

                // Restore the clicked app
                currentAppInstance = appToOpen;
                currentAppInstance.Open();
                return;
            }

            // Opening a new app
            if (currentAppInstance != null)
            {   
                // Minimize currently opened app that is not the opened one
                MinCurrentApp();
            }

            currentAppInstance = appToOpen;
            currentAppInstance.Open();
        }
        else
        {
            Debug.LogError($"Index {index} out of range. Available apps: {apps.Count}");
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
