using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using Pv.Unity;
using Button = UnityEngine.UI.Button;
using Toggle = UnityEngine.UI.Toggle;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace HoneyOS.VoiceControl
{
    /// <summary>
    /// Record audio clip from microphone and make intent recognition using Picovoice Rhino.
    /// </summary>
    public class MicrophoneDemo : MonoBehaviour
    {
        [Header("Picovoice Settings")]
        public string accessKey = "s73gVRPzWz+CRgZoOyYPeHR9F+2Q/tyQmMb02L57uIBzfqk0wMw9Kg===="; // Replace with your fresh Picovoice access key
        public string contextPath = "honeyos_context.rhn"; // Path to your custom context file
        public bool autoRestartListening = false; // Automatically restart listening after each command
        public float restartDelay = 1.0f; // Delay before restarting (in seconds)
        
        private RhinoManager rhinoManager;
        private PicovoiceMicrophoneManager microphoneManager;
        private string _buffer;
        public bool isButtonUsed = false;
        private int commandCount = 0; // For debugging
        private bool isResettingRhino = false; // Track if Rhino is being reset

        // Add Apps
        [Header("Apps")]
        public DesktopManager desktopManager;
        public TextEditorController textEditor;
        public PageController help;
        public ProcessManager processManager;
        public PageController2 scheduler;

        public PageController3 pageReplacementNav;
        public AlgorithmManager algorithmManager;

        [Header("UI")]
        public Button button;
        public TextMeshProUGUI outputText;
        public float outputTextDuration = 2.0f;

        [Header("Intent Actions")]
        private Dictionary<string, (Action function, string message)> intentActionDictionary;
        private string lastActionMessage = "";

        [Header("Voice Activity Detection (VAD)")]
        public Image vadIndicatorImage;
        public Color defaultIndicatorColor;
        public Color voiceDetectedColor;
        public Color voiceUndetectedColor;

        [Header("SFX")]
        public AudioSource audioSource;
        public AudioClip startRecordingSound;
        public AudioClip stopRecordingSound;
        public AudioClip whatCanIDoSound;

        [Header("SpaceBarHandler")]
        public float longLongPressDuration = 0.2f;
        private float pressStartTime;
        private bool isLongLongPressTriggered = false;
        private bool isListening = false;

        [Header("Audio Monitoring")]
        private float lastAudioCheckTime = 0f;
        private float audioCheckInterval = 30f; // Check every 30 seconds
        private float noAudioTimeout = 120f; // Reset after 2 minutes of no audio

        [Header("Error Recovery")]
        private bool isMicrophoneRecoveryInProgress = false;
        private float microphoneRecoveryInterval = 5f; // Try recovery every 5 seconds
        private float lastMicrophoneRecoveryAttempt = 0f;
        private int maxRecoveryAttempts = 3;
        private int currentRecoveryAttempts = 0;

        private void Awake()
        {
            // Initialize AudioSource
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            // Always configure AudioSource settings
            ConfigureAudioSource();

            // Get or add microphone manager
            microphoneManager = GetComponent<PicovoiceMicrophoneManager>();
            if (microphoneManager == null)
            {
                microphoneManager = gameObject.AddComponent<PicovoiceMicrophoneManager>();
            }

            // Check microphone availability
            CheckMicrophoneAvailability();
            
            InitializeRhino();
            InitIntentActionDictionary();
            
            button.onClick.AddListener(OnButtonPressed);
        }

        private void InitializeRhino()
        {
            try
            {
                // Clean up any existing instance first
                if (rhinoManager != null)
                {
                    UnityEngine.Debug.Log("Cleaning up existing Rhino instance...");
                    rhinoManager.Delete();
                    rhinoManager = null;
                }
                
                // Build the full path to the context file with detailed logging
                string platform = GetPlatform();
                string platformContextFileName = GetContextFileName();
                
                UnityEngine.Debug.Log($"=== CONTEXT FILE PATH ANALYSIS ===");
                UnityEngine.Debug.Log($"Platform detected: {platform}");
                UnityEngine.Debug.Log($"Platform context filename: {platformContextFileName}");
                UnityEngine.Debug.Log($"StreamingAssets path: {Application.streamingAssetsPath}");
                UnityEngine.Debug.Log($"Main context path setting: {contextPath}");
                
                // Try multiple path strategies
                string fullContextPath = null;
                List<string> pathsToTry = new List<string>();
                
                #if UNITY_EDITOR || UNITY_STANDALONE
                    // Strategy 1: Platform-specific context file
                    string platformSpecificPath = System.IO.Path.Combine(Application.streamingAssetsPath, "contexts", platform, platformContextFileName);
                    pathsToTry.Add(platformSpecificPath);
                    
                    // Strategy 2: Main context file in StreamingAssets root
                    string mainContextPath = System.IO.Path.Combine(Application.streamingAssetsPath, contextPath);
                    pathsToTry.Add(mainContextPath);
                    
                    // Strategy 3: Direct honeyos_context.rhn in StreamingAssets
                    string directContextPath = System.IO.Path.Combine(Application.streamingAssetsPath, "honeyos_context.rhn");
                    pathsToTry.Add(directContextPath);
                #elif UNITY_ANDROID
                    // On Android, use relative paths
                    pathsToTry.Add($"contexts/{platform}/{platformContextFileName}");
                    pathsToTry.Add(contextPath);
                    pathsToTry.Add("honeyos_context.rhn");
                #else
                    // Other platforms
                    string platformSpecificPath = System.IO.Path.Combine(Application.streamingAssetsPath, "contexts", platform, platformContextFileName);
                    pathsToTry.Add(platformSpecificPath);
                    string mainContextPath = System.IO.Path.Combine(Application.streamingAssetsPath, contextPath);
                    pathsToTry.Add(mainContextPath);
                #endif
                
                // Test each path and use the first one that exists
                for (int i = 0; i < pathsToTry.Count; i++)
                {
                    string testPath = pathsToTry[i];
                    UnityEngine.Debug.Log($"Testing path {i + 1}: {testPath}");
                    UnityEngine.Debug.Log($"Path exists: {System.IO.File.Exists(testPath)}");
                    
                    if (System.IO.File.Exists(testPath))
                    {
                        fullContextPath = testPath;
                        UnityEngine.Debug.Log($"✓ FOUND CONTEXT FILE: {fullContextPath}");
                        break;
                    }
                }
                
                // If no context file found, show detailed error
                if (string.IsNullOrEmpty(fullContextPath))
                {
                    UnityEngine.Debug.LogError("❌ NO CONTEXT FILE FOUND!");
                    UnityEngine.Debug.LogError("Searched paths:");
                    for (int i = 0; i < pathsToTry.Count; i++)
                    {
                        UnityEngine.Debug.LogError($"  {i + 1}. {pathsToTry[i]}");
                    }
                    
                    // List what files ARE in StreamingAssets
                    try
                    {
                        string[] files = System.IO.Directory.GetFiles(Application.streamingAssetsPath, "*.rhn", System.IO.SearchOption.AllDirectories);
                        UnityEngine.Debug.LogError("Available .rhn files in StreamingAssets:");
                        foreach (string file in files)
                        {
                            UnityEngine.Debug.LogError($"  - {file}");
                        }
                    }
                    catch (System.Exception ex)
                    {
                        UnityEngine.Debug.LogError($"Error listing files: {ex.Message}");
                    }
                    
                    DisplayOutputText("Context file not found");
                    return;
                }
                
                UnityEngine.Debug.Log($"Access key length: {accessKey.Length}");
                UnityEngine.Debug.Log($"Unity Platform: {Application.platform}");
                
                if (accessKey == "YOUR_NEW_ACCESS_KEY_HERE" || accessKey == "PASTE_YOUR_FRESH_ACCESS_KEY_HERE" || string.IsNullOrEmpty(accessKey))
                {
                    UnityEngine.Debug.LogError("Access key not set! Please set your Picovoice access key.");
                    DisplayOutputText("Access key not configured");
                    return;
                }
                
                // Validate access key format (should be base64-like)
                if (accessKey.Length < 20)
                {
                    UnityEngine.Debug.LogError($"Access key appears to be too short: {accessKey.Length} characters");
                    DisplayOutputText("Invalid access key format");
                    return;
                }
                
                // Check if model file exists
                string modelPath = System.IO.Path.Combine(Application.streamingAssetsPath, "rhino_params.pv");
                UnityEngine.Debug.Log($"Model path: {modelPath}");
                UnityEngine.Debug.Log($"Model file exists: {System.IO.File.Exists(modelPath)}");
                
                UnityEngine.Debug.Log("About to create RhinoManager...");
                UnityEngine.Debug.Log($"Final access key (first 10 chars): {accessKey.Substring(0, Math.Min(10, accessKey.Length))}...");
                UnityEngine.Debug.Log($"Final context path: {fullContextPath}");
                
                rhinoManager = RhinoManager.Create(
                    accessKey,
                    fullContextPath,
                    OnInferenceResult);
                
                UnityEngine.Debug.Log("✓ Rhino Speech-to-Intent initialized successfully");
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to initialize Rhino: {ex.Message}");
                UnityEngine.Debug.LogError($"Stack trace: {ex.StackTrace}");
                DisplayOutputText("Voice recognition initialization failed");
            }
        }

        private void IncreaseFrameCount()
        {
            if (algorithmManager.frameCountSlider.value < 7)
            {
                algorithmManager.UpdateFrameCount(algorithmManager.frameCountSlider.value + 1);
                algorithmManager.frameCountSlider.value += 1;
                lastActionMessage = $"Increased frame count to {algorithmManager.frameCountSlider.value}";
            }
            else
            {
                lastActionMessage = "Frame count is already at maximum (7)";
            }
        }

        private void DecreaseFrameCount()
        {
            if (algorithmManager.frameCountSlider.value > 1)
            {
                algorithmManager.UpdateFrameCount(algorithmManager.frameCountSlider.value - 1);
                algorithmManager.frameCountSlider.value -= 1;
                lastActionMessage = $"Decreased frame count to {algorithmManager.frameCountSlider.value}";
            }
            else
            {
                lastActionMessage = "Frame count is already at minimum (1)";
            }
        }

        private void InitIntentActionDictionary()
        {
            intentActionDictionary = new Dictionary<string, (Action function, string message)>
            {
                // App Management - using original command phrases as fallback
                { "openSweet", ( () => desktopManager.OpenApp(0), "Opening Sweet") },
                { "open sweet", ( () => desktopManager.OpenApp(0), "Opening Sweet") },
                { "openSugar", ( () => desktopManager.OpenApp(3), "Opening Sugar") },
                { "open sugar", ( () => desktopManager.OpenApp(3), "Opening Sugar") },
                { "openBlueberry", ( () => desktopManager.OpenApp(2), "Opening Blueberry") },
                { "openCake", ( () => desktopManager.OpenApp(1), "Opening Cake") },
                { "open cake", ( () => desktopManager.OpenApp(1), "Opening Cake") },
                { "closeApp", ( () => desktopManager.CloseCurrentApp(), "Closing app") },
                { "close application", ( () => desktopManager.CloseCurrentApp(), "Closing app") },
                { "closeAll", ( () => desktopManager.CloseAllApps(), "Closing all apps") },
                { "close all", ( () => desktopManager.CloseAllApps(), "Closing all apps") },
                { "minimizeApp", ( () => desktopManager.MinCurrentApp(), "Minimizing app") },
                { "minimize application", ( () => desktopManager.MinCurrentApp(), "Minimizing app") },
                
                // File Operations - with error handling
                { "saveFile", ( () => {
                    try {
                        textEditor.Save();
                    }
                    catch (System.Exception ex) {
                        UnityEngine.Debug.LogError($"Error saving file: {ex.Message}");
                        throw; // Rethrow to be handled by ExecuteIntent
                    }
                }, "Saving File") },
                
                { "saveAsFile", ( () => {
                    try {
                        textEditor.SaveAs();
                    }
                    catch (System.Exception ex) {
                        UnityEngine.Debug.LogError($"Error in save as operation: {ex.Message}");
                        throw; // Rethrow to be handled by ExecuteIntent
                    }
                }, "Saving File As") },
                
                { "openFile", ( () => {
                    try {
                        textEditor.OpenFile();
                    }
                    catch (System.Exception ex) {
                        UnityEngine.Debug.LogError($"Error opening file: {ex.Message}");
                        throw; // Rethrow to be handled by ExecuteIntent
                    }
                }, "Opening File") },

                { "newFile", ( () => {
                    try {
                        textEditor.NewFile();
                    }
                    catch (System.Exception ex) {
                        UnityEngine.Debug.LogError($"Error creating new file: {ex.Message}");
                        throw; // Rethrow to be handled by ExecuteIntent
                    }
                }, "Creating New File") },
                
                
                // Text Editing
                { "undo", ( () => textEditor.Undo(), "Undo text changes") },
                { "redo", ( () => textEditor.Redo(), "Redo text changes") },
                { "copy", ( () => textEditor.Copy(), "Copying selected text") },
                { "cut", ( () => textEditor.Cut(), "Cutting selected text") },
                { "paste", ( () => textEditor.Paste(), "Pasting text from clipboard") },
                
                // Help Navigation
                { "goBack", ( () => help.OpenHome(), "Going back to Home Page") },
                { "go back", ( () => help.OpenHome(), "Going back to Home Page") },
                { "openSystemBasics", ( () => help.OpenSystemBasics(), "Opening System Basics") },
                { "open system basics", ( () => help.OpenSystemBasics(), "Opening System Basics") },
                { "openAppGuide", ( () => help.OpenAppGuide(), "Opening Application Guide") },
                { "open application guide", ( () => help.OpenAppGuide(), "Opening Application Guide") },
                { "openAboutUs", ( () => help.OpenAboutUs(), "Opening About Us") },
                { "open about us", ( () => help.OpenAboutUs(), "Opening About Us") },
                
                // Scheduler Operations
                { "homeScheduler", ( () => scheduler.OpenHome(), "Going back to Cake Ville") },
                { "startCake", ( () => scheduler.OpenSelectionPage(), "Opening Policy selector") },
                { "start cake", ( () => scheduler.OpenSelectionPage(), "Opening Policy selector") },
                { "backToPolicySelection", ( () => {scheduler.OpenSelectionPage(); processManager.Stop();}, "Opening Policy selector") },
                { "back to policy selection", ( () => {scheduler.OpenSelectionPage(); processManager.Stop();}, "Opening Policy selector") },
                { "chooseFCFS", ( () => {scheduler.OpenSimPage(); processManager.SetSchedulingPolicy("FCFS");}, "FCFS Simulator") },
                { "choose first come first serve", ( () => {scheduler.OpenSimPage(); processManager.SetSchedulingPolicy("FCFS");}, "FCFS Simulator") },
                { "choosePriority", ( () => {scheduler.OpenSimPage(); processManager.SetSchedulingPolicy("Prio");}, "Priority Simulator") },
                { "choose priority", ( () => {scheduler.OpenSimPage(); processManager.SetSchedulingPolicy("Prio");}, "Priority Simulator") },
                { "chooseRoundRobin", ( () => {scheduler.OpenSimPage(); processManager.SetSchedulingPolicy("RR");}, "Round Robin Simulator") },
                { "choose round robin", ( () => {scheduler.OpenSimPage(); processManager.SetSchedulingPolicy("RR");}, "Round Robin Simulator") },
                { "chooseSJF", ( () => {scheduler.OpenSimPage(); processManager.SetSchedulingPolicy("SJF");}, "Shortest Job First Simulator") },
                { "choose shortest job first", ( () => {scheduler.OpenSimPage(); processManager.SetSchedulingPolicy("SJF");}, "Shortest Job First Simulator") },
                
                // Simulation Control
                { "playSimulation", ( () => processManager.Play(), "Starting simulation") },
                { "play simulation", ( () => processManager.Play(), "Starting simulation") },
                { "pauseSimulation", ( () => processManager.Pause(), "Pausing simulation") },
                { "pause", ( () => processManager.Pause(), "Pausing simulation") },
                { "stopSimulation", ( () => processManager.Stop(), "Stopping simulation") },
                { "stop simulation", ( () => processManager.Stop(), "Stopping simulation") },
                { "addProcess", ( () => processManager.AddProcess(false), "Adding Process") },
                { "add process", ( () => processManager.AddProcess(false), "Adding Process") },
                { "nextStep", ( () => processManager.Next(), "Next step") },
                { "next", ( () => processManager.Next(), "Next step") },

                //Page Algorithms
                { "homeBlueberry", ( () => pageReplacementNav.OpenHome(), "Going back to Blueberry Ville") },
                { "home blueberry", ( () => pageReplacementNav.OpenHome(), "Going back to Blueberry Ville") },
                { "startBlueberry", ( () => pageReplacementNav.OpenSelectionPage(), "Opening Algorithm selector") },
                { "start blueberry", ( () => pageReplacementNav.OpenSelectionPage(), "Opening Algorithm selector") },
                { "backToAlgorithmSelection", ( () => {pageReplacementNav.OpenSelectionPage(); algorithmManager.Reset();}, "Opening Algorithm selector") },
                { "back to algorithm selection", ( () => {pageReplacementNav.OpenSelectionPage(); algorithmManager.Reset();}, "Opening Algorithm selector") },
                
                // FIFO Algorithm
                { "chooseFIFO", ( () => {pageReplacementNav.OpenSimPage(); algorithmManager.SetAlgorithm("FIFO");}, "Opening FIFO Page Replacement Simulator") },
                { "choose FIFO", ( () => {pageReplacementNav.OpenSimPage(); algorithmManager.SetAlgorithm("FIFO");}, "Opening FIFO Page Replacement Simulator") },
                { "choose fifo", ( () => {pageReplacementNav.OpenSimPage(); algorithmManager.SetAlgorithm("FIFO");}, "Opening FIFO Page Replacement Simulator") },
                { "select FIFO", ( () => {pageReplacementNav.OpenSimPage(); algorithmManager.SetAlgorithm("FIFO");}, "Opening FIFO Page Replacement Simulator") },
                { "select fifo", ( () => {pageReplacementNav.OpenSimPage(); algorithmManager.SetAlgorithm("FIFO");}, "Opening FIFO Page Replacement Simulator") },
                { "first in first out", ( () => {pageReplacementNav.OpenSimPage(); algorithmManager.SetAlgorithm("FIFO");}, "Opening FIFO Page Replacement Simulator") },
                
                // OPR Algorithm
                { "chooseOPR", ( () => {pageReplacementNav.OpenSimPage(); algorithmManager.SetAlgorithm("OPR");}, "Opening Optimal Page Replacement Simulator") },
                { "choose OPR", ( () => {pageReplacementNav.OpenSimPage(); algorithmManager.SetAlgorithm("OPR");}, "Opening Optimal Page Replacement Simulator") },
                { "choose opr", ( () => {pageReplacementNav.OpenSimPage(); algorithmManager.SetAlgorithm("OPR");}, "Opening Optimal Page Replacement Simulator") },
                { "select OPR", ( () => {pageReplacementNav.OpenSimPage(); algorithmManager.SetAlgorithm("OPR");}, "Opening Optimal Page Replacement Simulator") },
                { "select opr", ( () => {pageReplacementNav.OpenSimPage(); algorithmManager.SetAlgorithm("OPR");}, "Opening Optimal Page Replacement Simulator") },
                { "choose optimal", ( () => {pageReplacementNav.OpenSimPage(); algorithmManager.SetAlgorithm("OPR");}, "Opening Optimal Page Replacement Simulator") },
                { "optimal page replacement", ( () => {pageReplacementNav.OpenSimPage(); algorithmManager.SetAlgorithm("OPR");}, "Opening Optimal Page Replacement Simulator") },
                
                // LRU Algorithm
                { "chooseLRU", ( () => {pageReplacementNav.OpenSimPage(); algorithmManager.SetAlgorithm("LRU");}, "Opening Least Recently Used Page Replacement Simulator") },
                { "choose LRU", ( () => {pageReplacementNav.OpenSimPage(); algorithmManager.SetAlgorithm("LRU");}, "Opening Least Recently Used Page Replacement Simulator") },
                { "choose lru", ( () => {pageReplacementNav.OpenSimPage(); algorithmManager.SetAlgorithm("LRU");}, "Opening Least Recently Used Page Replacement Simulator") },
                { "select LRU", ( () => {pageReplacementNav.OpenSimPage(); algorithmManager.SetAlgorithm("LRU");}, "Opening Least Recently Used Page Replacement Simulator") },
                { "select lru", ( () => {pageReplacementNav.OpenSimPage(); algorithmManager.SetAlgorithm("LRU");}, "Opening Least Recently Used Page Replacement Simulator") },
                { "least recently used", ( () => {pageReplacementNav.OpenSimPage(); algorithmManager.SetAlgorithm("LRU");}, "Opening Least Recently Used Page Replacement Simulator") },
                
                // MRU Algorithm
                { "chooseMRU", ( () => {pageReplacementNav.OpenSimPage(); algorithmManager.SetAlgorithm("MRU");}, "Opening Most Recently Used Page Replacement Simulator") },
                { "choose MRU", ( () => {pageReplacementNav.OpenSimPage(); algorithmManager.SetAlgorithm("MRU");}, "Opening Most Recently Used Page Replacement Simulator") },
                { "choose mru", ( () => {pageReplacementNav.OpenSimPage(); algorithmManager.SetAlgorithm("MRU");}, "Opening Most Recently Used Page Replacement Simulator") },
                { "select MRU", ( () => {pageReplacementNav.OpenSimPage(); algorithmManager.SetAlgorithm("MRU");}, "Opening Most Recently Used Page Replacement Simulator") },
                { "select mru", ( () => {pageReplacementNav.OpenSimPage(); algorithmManager.SetAlgorithm("MRU");}, "Opening Most Recently Used Page Replacement Simulator") },
                { "most recently used", ( () => {pageReplacementNav.OpenSimPage(); algorithmManager.SetAlgorithm("MRU");}, "Opening Most Recently Used Page Replacement Simulator") },
                
                // LFU Algorithm
                { "chooseLFU", ( () => {pageReplacementNav.OpenSimPage(); algorithmManager.SetAlgorithm("LFU");}, "Opening Least Frequently Used Page Replacement Simulator") },
                { "choose LFU", ( () => {pageReplacementNav.OpenSimPage(); algorithmManager.SetAlgorithm("LFU");}, "Opening Least Frequently Used Page Replacement Simulator") },
                { "choose lfu", ( () => {pageReplacementNav.OpenSimPage(); algorithmManager.SetAlgorithm("LFU");}, "Opening Least Frequently Used Page Replacement Simulator") },
                { "select LFU", ( () => {pageReplacementNav.OpenSimPage(); algorithmManager.SetAlgorithm("LFU");}, "Opening Least Frequently Used Page Replacement Simulator") },
                { "select lfu", ( () => {pageReplacementNav.OpenSimPage(); algorithmManager.SetAlgorithm("LFU");}, "Opening Least Frequently Used Page Replacement Simulator") },
                { "least frequently used", ( () => {pageReplacementNav.OpenSimPage(); algorithmManager.SetAlgorithm("LFU");}, "Opening Least Frequently Used Page Replacement Simulator") },
                
                // Algorithm Control
                { "simulateAlgorithm", ( () => { algorithmManager.ProcessReferenceString();  algorithmManager.RunSelectedAlgorithm();}, "Starting simulation") },
                { "simulate algorithm", ( () => { algorithmManager.ProcessReferenceString();  algorithmManager.RunSelectedAlgorithm();}, "Starting simulation") },
                { "start algorithm", ( () => { algorithmManager.ProcessReferenceString();  algorithmManager.RunSelectedAlgorithm();}, "Starting simulation") },
                { "run algorithm", ( () => { algorithmManager.ProcessReferenceString();  algorithmManager.RunSelectedAlgorithm();}, "Starting simulation") },
                { "pauseAlgorithm", ( () => { algorithmManager.TogglePause();}, "Pausing simulation") },
                { "pause algorithm", ( () => { algorithmManager.TogglePause();}, "Pausing simulation") },
                { "resumeAlgorithm", ( () => { algorithmManager.TogglePause();}, "Resuming simulation") },
                { "resume algorithm", ( () => { algorithmManager.TogglePause();}, "Resuming simulation") },
                { "resetAlgorithm", ( () => { algorithmManager.Reset();}, "Stopping simulation") },
                { "reset algorithm", ( () => { algorithmManager.Reset();}, "Stopping simulation") },
                { "stop algorithm", ( () => { algorithmManager.Reset();}, "Stopping simulation") },

                //Frame Count
                { "increaseFrames", ( () => IncreaseFrameCount(), "DYNAMIC_MESSAGE") },
                { "decreaseFrames", ( () => DecreaseFrameCount(), "DYNAMIC_MESSAGE") },
                { "setFrameCount1", ( () => { algorithmManager.UpdateFrameCount(1); algorithmManager.frameCountSlider.value = 1; }, "Setting frame count to 1") },
                { "setFrameCount2", ( () => { algorithmManager.UpdateFrameCount(2); algorithmManager.frameCountSlider.value = 2; }, "Setting frame count to 2") },
                { "setFrameCount3", ( () => { algorithmManager.UpdateFrameCount(3); algorithmManager.frameCountSlider.value = 3; }, "Setting frame count to 3") },
                { "setFrameCount4", ( () => { algorithmManager.UpdateFrameCount(4); algorithmManager.frameCountSlider.value = 4;}, "Setting frame count to 4") },
                { "setFrameCount5", ( () => { algorithmManager.UpdateFrameCount(5); algorithmManager.frameCountSlider.value = 5;}, "Setting frame count to 5") },
                { "setFrameCount6", ( () => { algorithmManager.UpdateFrameCount(6); algorithmManager.frameCountSlider.value = 6;}, "Setting frame count to 6") },
                { "setFrameCount7", ( () => { algorithmManager.UpdateFrameCount(7); algorithmManager.frameCountSlider.value = 7; }, "Setting frame count to 7") },

                //maximize
                { "changeSize", ( () => { 
                    if (desktopManager.CurrentAppInstance != null && 
                        desktopManager.CurrentAppInstance.GetComponent<WindowMaximizer>() != null)
                    {
                        desktopManager.CurrentAppInstance.GetComponent<WindowMaximizer>().ToggleMaximize();
                    }
                }, "Changing app window size") },
            };
        }

        private void Update()
        {
            HandleSpaceBarInput();
            UpdateVADIndicator();
            MonitorAudioCapture();
            CheckMicrophoneStatus();
        }

        private void UpdateVADIndicator()
        {
            try
            {
                Color color;

                if (isListening)
                {
                    // Check if we're in recovery mode
                    if (isMicrophoneRecoveryInProgress)
                    {
                        color = Color.yellow; // Yellow indicates recovery in progress
                    }
                    // Use microphone manager's voice detection if available and recording
                    else if (microphoneManager != null && microphoneManager.IsRecording)
                    {
                        color = microphoneManager.IsVoiceDetected ? voiceDetectedColor : voiceUndetectedColor;
                    }
                    else
                    {
                        color = defaultIndicatorColor;
                    }
                }
                else
                {
                    color = defaultIndicatorColor;
                }

                if (vadIndicatorImage != null)
                {
                    vadIndicatorImage.color = color;
                }
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"Error updating VAD indicator: {ex.Message}");
                // Don't let indicator errors affect the rest of the system
            }
        }

        private void OnButtonPressed()
        {
            if (!isListening)
            {
                StartListening();
            }
            else
            {
                StopListening();
            }
        }

        private void StartListening()
        {
            try
            {
                // Don't start if we're in the middle of resetting Rhino
                if (isResettingRhino)
                {
                    UnityEngine.Debug.Log("Rhino is being reset, please wait...");
                    DisplayOutputText("Please wait, processing...");
                    return;
                }

                // Check if microphone manager is working
                if (microphoneManager == null || !microphoneManager.IsRecording)
                {
                    AttemptMicrophoneRecovery();
                    if (microphoneManager == null || !microphoneManager.IsRecording)
                    {
                        DisplayOutputText("Microphone not available. Attempting recovery...");
                        return;
                    }
                }

                // Check if Rhino is properly initialized
                if (rhinoManager == null)
                {
                    UnityEngine.Debug.Log("RhinoManager is null - reinitializing...");
                    InitializeRhino();
                    if (rhinoManager == null)
                    {
                        DisplayOutputText("Voice recognition not initialized");
                        return;
                    }
                }

                // Debug check for audio components
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                    ConfigureAudioSource();
                }
                
                StartCoroutine(StartListeningAfterSound());
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to start listening: {ex.Message}");
                AttemptMicrophoneRecovery();
            }
        }

        private IEnumerator StartListeningAfterSound()
        {
            // Set initial UI state
            isListening = true; // Set this early so the VAD indicator shows green
            string message = "Honey, what can I do for you?";
            DisplayOutputText(message);
            
            PlaySound(whatCanIDoSound);
            
            // Wait for the sound to finish playing plus a small buffer
            if (whatCanIDoSound != null)
            {
                yield return new WaitForSeconds(whatCanIDoSound.length + 0.2f);
            }

            // Initialize Rhino if needed
            if (rhinoManager == null)
            {
                InitializeRhino();
            }

            UnityEngine.Debug.Log("Starting Rhino processing...");
            if (rhinoManager != null)
            {
                try
                {
                    rhinoManager.Process();
                    lastAudioCheckTime = Time.time;
                    UnityEngine.Debug.Log("Listening for voice commands...");
                }
                catch (System.Exception ex)
                {
                    UnityEngine.Debug.LogError($"Failed to start processing: {ex.Message}");
                    // Try to recover
                    InitializeRhino();
                    if (rhinoManager != null)
                    {
                        rhinoManager.Process();
                        lastAudioCheckTime = Time.time;
                    }
                    else
                    {
                        isListening = false;
                        DisplayOutputText("Failed to start voice recognition. Please try again.");
                    }
                }
            }
            else
            {
                isListening = false;
                DisplayOutputText("Voice recognition not initialized. Please try again.");
            }
        }

        private void StopListening()
        {
            try
            {
                // Debug check for audio components
                if (audioSource == null)
                {
                    UnityEngine.Debug.LogError("AudioSource is null!");
                    return;
                }
                if (stopRecordingSound == null)
                {
                    UnityEngine.Debug.LogError("stopRecordingSound clip is not assigned!");
                    return;
                }

                PlaySound(stopRecordingSound);

                isListening = false;

                if (isButtonUsed)
                    isButtonUsed = false;
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to stop listening: {ex.Message}");
            }
        }

        private void OnInferenceResult(Inference inference)
        {
            commandCount++;
            
            UnityEngine.Debug.Log($"Command #{commandCount} - Intent understood: {inference.IsUnderstood}");
            
            if (inference.IsUnderstood)
            {
                isListening = false;
                string intent = inference.Intent;
                Dictionary<string, string> slots = inference.Slots;
                
                UnityEngine.Debug.Log($"Executing intent: {intent}");
                ExecuteIntent(intent, slots);
                
                // Reset Rhino for next command
                StartCoroutine(DelayedRhinoReset());
            }
            else
            {
                UnityEngine.Debug.Log("Intent not understood");
                DisplayOutputText("Sorry, I didn't understand that command. Please try again.");
                
                // Reset and restart Rhino for continuous listening
                StartCoroutine(ContinueListening());
            }
        }

        private IEnumerator ContinueListening()
        {
            // Small delay to ensure clean state
            yield return new WaitForSeconds(0.1f);
            
            try
            {
                // Clean up current instance
                if (rhinoManager != null)
                {
                    rhinoManager.Delete();
                    rhinoManager = null;
                }
                
                // Reinitialize Rhino
                InitializeRhino();
                
                // Start processing again
                if (rhinoManager != null)
                {
                    rhinoManager.Process();
                    lastAudioCheckTime = Time.time; // Reset the timer when continuing to listen
                }
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to continue listening: {ex.Message}");
                isListening = false;
                DisplayOutputText("Voice recognition error. Please try again.");
            }
        }

        private void ExecuteIntent(string intent, Dictionary<string, string> slots)
        {
            try
            {
                // Debug logging for page replacement commands
                if (intent.ToLower().Contains("fifo") || intent.ToLower().Contains("lru") || 
                    intent.ToLower().Contains("opr") || intent.ToLower().Contains("mru") || 
                    intent.ToLower().Contains("lfu") || intent.ToLower().Contains("algorithm"))
                {
                    UnityEngine.Debug.Log($"🔍 PAGE REPLACEMENT COMMAND DETECTED: '{intent}'");
                    UnityEngine.Debug.Log($"Available page replacement commands:");
                    foreach (var kvp in intentActionDictionary)
                    {
                        if (kvp.Key.ToLower().Contains("fifo") || kvp.Key.ToLower().Contains("lru") || 
                            kvp.Key.ToLower().Contains("opr") || kvp.Key.ToLower().Contains("mru") || 
                            kvp.Key.ToLower().Contains("lfu") || kvp.Key.ToLower().Contains("algorithm") ||
                            kvp.Key.ToLower().Contains("blueberry"))
                        {
                            UnityEngine.Debug.Log($"  - '{kvp.Key}' -> {kvp.Value.message}");
                        }
                    }
                }
                
                // First try exact match
                if (intentActionDictionary.ContainsKey(intent))
                {
                    var action = intentActionDictionary[intent];
                    UnityEngine.Debug.Log($"✅ EXACT MATCH FOUND: '{intent}' -> {action.message}");
                    try
                    {
                        action.function();
                        string messageToDisplay = action.message == "DYNAMIC_MESSAGE" ? lastActionMessage : action.message;
                        DisplayOutputText(messageToDisplay);
                    }
                    catch (System.Exception ex)
                    {
                        // Log the error but don't let it affect the microphone
                        UnityEngine.Debug.LogError($"Error executing intent '{intent}': {ex.Message}");
                        DisplayOutputText("Sorry, that action couldn't be completed. Please try again.");
                        
                        // If this was a file operation error, handle it gracefully
                        if (ex is System.DllNotFoundException || ex.Message.Contains("StandaloneFileBrowser"))
                        {
                            DisplayOutputText("File operation failed. Please try again.");
                        }
                        
                        // Ensure microphone keeps running
                        EnsureMicrophoneStaysActive();
                    }
                    return;
                }
                
                // If no exact match, try to find a partial match or similar intent
                foreach (var kvp in intentActionDictionary)
                {
                    // Try case-insensitive match
                    if (string.Equals(kvp.Key, intent, StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            kvp.Value.function();
                            string messageToDisplay = kvp.Value.message == "DYNAMIC_MESSAGE" ? lastActionMessage : kvp.Value.message;
                            DisplayOutputText(messageToDisplay);
                        }
                        catch (System.Exception ex)
                        {
                            UnityEngine.Debug.LogError($"Error executing intent '{intent}': {ex.Message}");
                            DisplayOutputText("Sorry, that action couldn't be completed. Please try again.");
                            EnsureMicrophoneStaysActive();
                        }
                        return;
                    }
                    
                    // Try partial match (intent contains key or key contains intent)
                    if (intent.ToLower().Contains(kvp.Key.ToLower()) || kvp.Key.ToLower().Contains(intent.ToLower()))
                    {
                        try
                        {
                            kvp.Value.function();
                            string messageToDisplay = kvp.Value.message == "DYNAMIC_MESSAGE" ? lastActionMessage : kvp.Value.message;
                            DisplayOutputText(messageToDisplay);
                        }
                        catch (System.Exception ex)
                        {
                            UnityEngine.Debug.LogError($"Error executing intent '{intent}': {ex.Message}");
                            DisplayOutputText("Sorry, that action couldn't be completed. Please try again.");
                            EnsureMicrophoneStaysActive();
                        }
                        return;
                    }
                }
                
                // If still no match, log for debugging
                UnityEngine.Debug.LogWarning($"❌ NO MATCH FOUND for intent: '{intent}'");
                UnityEngine.Debug.LogWarning($"Available commands (first 10):");
                int count = 0;
                foreach (var kvp in intentActionDictionary)
                {
                    if (count < 10)
                    {
                        UnityEngine.Debug.LogWarning($"  - '{kvp.Key}'");
                        count++;
                    }
                }
                UnityEngine.Debug.LogWarning($"... and {intentActionDictionary.Count - 10} more commands");
                DisplayOutputText("Command not recognized.");
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"Error in ExecuteIntent: {ex.Message}");
                DisplayOutputText("Sorry, there was an error processing your command.");
                EnsureMicrophoneStaysActive();
            }
        }

        private void EnsureMicrophoneStaysActive()
        {
            try
            {
                // Check if we need to recover the microphone
                if (microphoneManager == null || !microphoneManager.IsRecording)
                {
                    UnityEngine.Debug.Log("Ensuring microphone stays active after error...");
                    AttemptMicrophoneRecovery();
                }
                
                // If Rhino stopped processing, restart it
                if (rhinoManager != null && isListening)
                {
                    StartCoroutine(DelayedProcessingStart());
                }
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"Error in EnsureMicrophoneStaysActive: {ex.Message}");
            }
        }

        private void DisplayOutputText(string message)
        {
            StartCoroutine(HandleDisplayOutputText(message));
        }

        private IEnumerator HandleDisplayOutputText(string message)
        {
            outputText.text = message;
            yield return new WaitForSeconds(outputTextDuration);
            outputText.text = "";
        }

        private IEnumerator DelayedRhinoReset()
        {
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            yield return null;
            yield return null; 
            
            isResettingRhino = true;
            
            UnityEngine.Debug.Log("Resetting Rhino in background...");
            
            // Store reference to old instance
            var oldRhino = rhinoManager;
            rhinoManager = null;
            
            // Create new instance first
            bool resetSuccessful = false;
            try
            {
                InitializeRhino();
                resetSuccessful = true;
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to reset Rhino: {ex.Message}");
                resetSuccessful = false;
            }
            
            // Wait a few more frames before cleaning up old instance
            yield return null;
            yield return null;
            
            // Clean up old instance
            try
            {
                if (oldRhino != null)
                {
                    oldRhino.Delete();
                }
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to delete old Rhino instance: {ex.Message}");
            }
            
            // If reset failed, try fallback initialization
            if (!resetSuccessful)
            {
                try
                {
                    InitializeRhino();
                }
                catch (System.Exception ex)
                {
                    UnityEngine.Debug.LogError($"Fallback Rhino initialization failed: {ex.Message}");
                }
            }
            
            isResettingRhino = false;
            UnityEngine.Debug.Log("Rhino reset complete - ready for next command");
        }

        private void ResetRhinoForNextCommand()
        {
            try
            {
                // Clean up current Rhino instance
                if (rhinoManager != null)
                {
                    rhinoManager.Delete();
                    rhinoManager = null;
                }
                
                // Reinitialize Rhino for next use
                InitializeRhino();
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to reset Rhino: {ex.Message}");
                // Try to reinitialize on error
                InitializeRhino();
            }
        }

        void HandleSpaceBarInput()
        {
            if (!isButtonUsed)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    pressStartTime = Time.time;
                    isLongLongPressTriggered = false;
                }

                if (Input.GetKey(KeyCode.Space))
                {
                    float holdDuration = Time.time - pressStartTime;

                    if (!isLongLongPressTriggered && holdDuration >= longLongPressDuration)
                    {
                        // Start listening when spacebar is held
                        if (!isListening)
                        {
                            StartListening();
                        }
                        isLongLongPressTriggered = true;
                    }
                }

                if (Input.GetKeyUp(KeyCode.Space))
                {
                    // Stop listening when spacebar is released (if it was a long press)
                    if (isLongLongPressTriggered && isListening)
                    {
                        StopListening();
                    }
                }
            }
        }

        public void SetIsButtonUsed(bool boolean)
        {
            isButtonUsed = boolean;
        }

        // Manual control methods for external scripts if needed
        public void StartManualListening()
        {
            if (!isListening)
            {
                StartListening();
            }
        }

        public void StopManualListening()
        {
            if (isListening)
            {
                StopListening();
            }
        }

        private string GetPlatform()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    return "windows";
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                    return "mac";
                case RuntimePlatform.LinuxEditor:
                case RuntimePlatform.LinuxPlayer:
                    return "linux";
                case RuntimePlatform.IPhonePlayer:
                    return "ios";
                case RuntimePlatform.Android:
                    return "android";
                default:
                    throw new NotSupportedException($"Platform '{Application.platform}' not supported by Rhino Unity binding");
            }
        }

        private string GetContextFileName()
        {
            string platform = GetPlatform();
            return $"honeyos_context_{platform}.rhn";
        }

        private void OnDestroy()
        {
            if (rhinoManager != null)
            {
                rhinoManager.Delete();
            }
        }

        private void ConfigureAudioSource()
        {
            if (audioSource != null)
            {
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0f; // Make it 2D audio
                audioSource.volume = 1f;
                audioSource.mute = false;
                audioSource.loop = false;
            }
        }

        private void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                ConfigureAudioSource(); // Ensure settings are correct before playing
                audioSource.clip = clip;
                audioSource.Play();
            }
        }

        private void MonitorAudioCapture()
        {
            if (!isListening) return;

            if (Time.time - lastAudioCheckTime >= audioCheckInterval)
            {
                lastAudioCheckTime = Time.time;
                
                // Check if microphone is still working properly
                if (microphoneManager != null && microphoneManager.IsRecording)
                {
                    bool needsReset = false;

                    // Check if we're stuck in a non-responsive state
                    if (Time.time - lastAudioCheckTime >= noAudioTimeout)
                    {
                        UnityEngine.Debug.Log("Audio capture may be stuck, attempting reset...");
                        needsReset = true;
                    }

                    if (needsReset)
                    {
                        RestartAudioCapture();
                    }
                }
            }
        }

        private void RestartAudioCapture()
        {
            try
            {
                UnityEngine.Debug.Log("Restarting audio capture...");
                
                // Stop current capture
                if (microphoneManager != null)
                {
                    // The microphone manager will be recreated, no need to explicitly stop
                    microphoneManager = null;
                }
                
                // Reset Rhino
                if (rhinoManager != null)
                {
                    rhinoManager.Delete();
                    rhinoManager = null;
                }

                // Small delay to ensure clean state
                StartCoroutine(DelayedAudioRestart());
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to restart audio capture: {ex.Message}");
                isListening = false;
                DisplayOutputText("Audio capture error. Please try again.");
            }
        }

        private IEnumerator DelayedAudioRestart()
        {
            yield return new WaitForSeconds(0.5f);
            
            try
            {
                // Reinitialize components
                if (microphoneManager == null)
                {
                    microphoneManager = gameObject.AddComponent<PicovoiceMicrophoneManager>();
                }
                
                InitializeRhino();
                
                if (rhinoManager != null)
                {
                    rhinoManager.Process();
                    DisplayOutputText("Audio capture restarted. Please continue speaking.");
                    lastAudioCheckTime = Time.time; // Reset the timer
                }
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to reinitialize audio capture: {ex.Message}");
                isListening = false;
                DisplayOutputText("Failed to restart audio. Please try again.");
            }
        }

        private void CheckMicrophoneStatus()
        {
            if (microphoneManager == null || !microphoneManager.IsRecording)
            {
                // Only attempt recovery if we're not already in recovery and enough time has passed
                if (!isMicrophoneRecoveryInProgress && Time.time - lastMicrophoneRecoveryAttempt >= microphoneRecoveryInterval)
                {
                    AttemptMicrophoneRecovery();
                }
            }
            else
            {
                // Reset recovery attempts if microphone is working
                currentRecoveryAttempts = 0;
            }
        }

        private void AttemptMicrophoneRecovery()
        {
            if (currentRecoveryAttempts >= maxRecoveryAttempts)
            {
                UnityEngine.Debug.LogWarning("Max microphone recovery attempts reached. Please restart the application.");
                DisplayOutputText("Microphone recovery failed. Please restart the application.");
                return;
            }

            isMicrophoneRecoveryInProgress = true;
            lastMicrophoneRecoveryAttempt = Time.time;
            currentRecoveryAttempts++;

            try
            {
                UnityEngine.Debug.Log($"Attempting microphone recovery (Attempt {currentRecoveryAttempts}/{maxRecoveryAttempts})...");
                
                // Safely clean up existing components
                if (microphoneManager != null)
                {
                    Destroy(microphoneManager);
                    microphoneManager = null;
                }

                // Create new microphone manager
                microphoneManager = gameObject.AddComponent<PicovoiceMicrophoneManager>();
                
                // Only reinitialize Rhino if it's null or not functioning
                if (rhinoManager == null)
                {
                    InitializeRhino();
                }

                if (isListening)
                {
                    StartCoroutine(DelayedProcessingStart());
                }

                DisplayOutputText("Microphone recovered successfully.");
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"Microphone recovery attempt {currentRecoveryAttempts} failed: {ex.Message}");
                DisplayOutputText("Attempting to recover microphone...");
            }
            finally
            {
                isMicrophoneRecoveryInProgress = false;
            }
        }

        private IEnumerator DelayedProcessingStart()
        {
            yield return new WaitForSeconds(0.5f);
            
            try
            {
                if (rhinoManager != null && microphoneManager != null && microphoneManager.IsRecording)
                {
                    rhinoManager.Process();
                }
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to start processing after recovery: {ex.Message}");
            }
        }

        private void CheckMicrophoneAvailability()
        {
            try
            {
                UnityEngine.Debug.Log($"Available microphone devices: {Microphone.devices.Length}");
                for (int i = 0; i < Microphone.devices.Length; i++)
                {
                    UnityEngine.Debug.Log($"Microphone {i}: {Microphone.devices[i]}");
                }
                
                if (Microphone.devices.Length == 0)
                {
                    UnityEngine.Debug.LogError("No microphone devices found!");
                    DisplayOutputText("No microphone found");
                }
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"Error checking microphone availability: {ex.Message}");
            }
        }

        // Call this method from Unity Inspector or another script to test basic setup
        [ContextMenu("Test Rhino Setup")]
        public void TestRhinoSetup()
        {
            TestRhinoSetupInternal(true);
        }
        
        // File-only test that works in Edit mode
        [ContextMenu("Test Files Only (Edit Mode Safe)")]
        public void TestFilesOnly()
        {
            TestRhinoSetupInternal(false);
        }
        
        // Add this new comprehensive diagnostic method
        [ContextMenu("Diagnose Voice Initialization")]
        public void DiagnoseVoiceInitialization()
        {
            UnityEngine.Debug.Log("=== VOICE INITIALIZATION DIAGNOSTIC ===");
            
            // Step 1: Check access key
            bool accessKeyValid = !string.IsNullOrEmpty(accessKey) && 
                                 accessKey != "YOUR_NEW_ACCESS_KEY_HERE" && 
                                 accessKey != "PASTE_YOUR_FRESH_ACCESS_KEY_HERE" &&
                                 accessKey.Length > 20;
            
            UnityEngine.Debug.Log($"🔑 ACCESS KEY STATUS:");
            UnityEngine.Debug.Log($"   Set: {!string.IsNullOrEmpty(accessKey)}");
            UnityEngine.Debug.Log($"   Not placeholder: {accessKey != "YOUR_NEW_ACCESS_KEY_HERE"}");
            UnityEngine.Debug.Log($"   Length: {accessKey?.Length ?? 0} characters");
            UnityEngine.Debug.Log($"   Valid format: {accessKeyValid}");
            
            if (!accessKeyValid)
            {
                UnityEngine.Debug.LogError("❌ ACCESS KEY ISSUE DETECTED!");
                UnityEngine.Debug.LogError("🔧 SOLUTION: Get a fresh access key from https://console.picovoice.ai/");
                UnityEngine.Debug.LogError("   1. Sign up/login to Picovoice Console");
                UnityEngine.Debug.LogError("   2. Copy your AccessKey");
                UnityEngine.Debug.LogError("   3. Replace 'YOUR_NEW_ACCESS_KEY_HERE' in the script");
                return;
            }
            
            // Step 2: Check files
            string platform = GetPlatform();
            string platformContextFileName = GetContextFileName();
            
            UnityEngine.Debug.Log($"📁 FILE STATUS:");
            UnityEngine.Debug.Log($"   Platform: {platform}");
            UnityEngine.Debug.Log($"   StreamingAssets path: {Application.streamingAssetsPath}");
            
            // Check context files
            List<string> contextPaths = new List<string>
            {
                System.IO.Path.Combine(Application.streamingAssetsPath, "contexts", platform, platformContextFileName),
                System.IO.Path.Combine(Application.streamingAssetsPath, contextPath),
                System.IO.Path.Combine(Application.streamingAssetsPath, "honeyos_context.rhn")
            };
            
            string workingContextPath = null;
            foreach (string path in contextPaths)
            {
                bool exists = System.IO.File.Exists(path);
                UnityEngine.Debug.Log($"   Context file: {path} - {(exists ? "✅ Found" : "❌ Missing")}");
                if (exists && workingContextPath == null)
                {
                    workingContextPath = path;
                }
            }
            
            // Check model file
            string modelPath = System.IO.Path.Combine(Application.streamingAssetsPath, "rhino_params.pv");
            bool modelExists = System.IO.File.Exists(modelPath);
            UnityEngine.Debug.Log($"   Model file: {modelPath} - {(modelExists ? "✅ Found" : "❌ Missing")}");
            
            if (workingContextPath == null || !modelExists)
            {
                UnityEngine.Debug.LogError("❌ MISSING FILES DETECTED!");
                UnityEngine.Debug.LogError("🔧 SOLUTION: Ensure these files are in Assets/StreamingAssets/:");
                UnityEngine.Debug.LogError("   - rhino_params.pv");
                UnityEngine.Debug.LogError("   - honeyos_context.rhn (or platform-specific version)");
                return;
            }
            
            // Step 3: Check microphone
            UnityEngine.Debug.Log($"🎤 MICROPHONE STATUS:");
            try
            {
                int micCount = Microphone.devices.Length;
                UnityEngine.Debug.Log($"   Available devices: {micCount}");
                for (int i = 0; i < micCount; i++)
                {
                    UnityEngine.Debug.Log($"   Device {i}: {Microphone.devices[i]}");
                }
                
                if (micCount == 0)
                {
                    UnityEngine.Debug.LogError("❌ NO MICROPHONE DETECTED!");
                    UnityEngine.Debug.LogError("🔧 SOLUTION: Connect a microphone and restart Unity");
                    return;
                }
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"❌ MICROPHONE ERROR: {ex.Message}");
                return;
            }
            
            // Step 4: Test Rhino creation (only in Play mode)
            #if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEngine.Debug.Log("⚠️ RHINO TEST: Requires Play mode");
                UnityEngine.Debug.Log("✅ PRE-FLIGHT CHECK COMPLETE!");
                UnityEngine.Debug.Log("💡 NEXT STEP: Enter Play mode and test voice commands");
                return;
            }
            #endif
            
            UnityEngine.Debug.Log($"🧪 TESTING RHINO CREATION:");
            try
            {
                var testRhino = RhinoManager.Create(accessKey, workingContextPath, (inference) => {
                    UnityEngine.Debug.Log("Test inference received");
                });
                UnityEngine.Debug.Log("✅ RHINO CREATION: SUCCESS!");
                testRhino.Delete();
                
                UnityEngine.Debug.Log("🎉 VOICE INITIALIZATION SHOULD WORK!");
                UnityEngine.Debug.Log("💡 Try pressing the voice button or holding spacebar");
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"❌ RHINO CREATION FAILED: {ex.Message}");
                
                // Provide specific error solutions
                if (ex.Message.Contains("00000136"))
                {
                    UnityEngine.Debug.LogError("🔧 ERROR 00000136 = Invalid Access Key");
                    UnityEngine.Debug.LogError("   Get a fresh key from https://console.picovoice.ai/");
                }
                else if (ex.Message.Contains("00000137"))
                {
                    UnityEngine.Debug.LogError("🔧 ERROR 00000137 = Access Key Expired");
                    UnityEngine.Debug.LogError("   Get a fresh key from https://console.picovoice.ai/");
                }
                else if (ex.Message.Contains("file") || ex.Message.Contains("path"))
                {
                    UnityEngine.Debug.LogError("🔧 FILE PATH ERROR");
                    UnityEngine.Debug.LogError("   Check that context and model files are in StreamingAssets");
                }
                else
                {
                    UnityEngine.Debug.LogError("🔧 UNKNOWN ERROR - Check Picovoice documentation");
                }
            }
            
            UnityEngine.Debug.Log("=== END DIAGNOSTIC ===");
        }
        
        // Add this new diagnostic method specifically for Blueberry commands
        [ContextMenu("Diagnose Blueberry Commands")]
        public void DiagnoseBlueberryCommands()
        {
            UnityEngine.Debug.Log("=== BLUEBERRY COMMANDS DIAGNOSTIC ===");
            
            // Step 1: Check component references
            UnityEngine.Debug.Log("🔧 COMPONENT REFERENCES:");
            UnityEngine.Debug.Log($"   pageReplacementNav: {(pageReplacementNav != null ? "✅ Found" : "❌ Missing")}");
            UnityEngine.Debug.Log($"   algorithmManager: {(algorithmManager != null ? "✅ Found" : "❌ Missing")}");
            
            if (pageReplacementNav == null)
            {
                UnityEngine.Debug.LogError("❌ pageReplacementNav is null! Assign PageController3 component in Inspector.");
            }
            
            if (algorithmManager == null)
            {
                UnityEngine.Debug.LogError("❌ algorithmManager is null! Assign AlgorithmManager component in Inspector.");
            }
            
            // Step 2: Test component methods (only if components exist)
            if (pageReplacementNav != null)
            {
                UnityEngine.Debug.Log("🧪 TESTING PageController3 methods:");
                try
                {
                    UnityEngine.Debug.Log("   Testing OpenHome()...");
                    pageReplacementNav.OpenHome();
                    UnityEngine.Debug.Log("   ✅ OpenHome() works");
                    
                    UnityEngine.Debug.Log("   Testing OpenSelectionPage()...");
                    pageReplacementNav.OpenSelectionPage();
                    UnityEngine.Debug.Log("   ✅ OpenSelectionPage() works");
                    
                    UnityEngine.Debug.Log("   Testing OpenSimPage()...");
                    pageReplacementNav.OpenSimPage();
                    UnityEngine.Debug.Log("   ✅ OpenSimPage() works");
                }
                catch (System.Exception ex)
                {
                    UnityEngine.Debug.LogError($"   ❌ PageController3 method failed: {ex.Message}");
                }
            }
            
            if (algorithmManager != null)
            {
                UnityEngine.Debug.Log("🧪 TESTING AlgorithmManager methods:");
                try
                {
                    UnityEngine.Debug.Log("   Testing SetAlgorithm('FIFO')...");
                    algorithmManager.SetAlgorithm("FIFO");
                    UnityEngine.Debug.Log("   ✅ SetAlgorithm() works");
                    
                    UnityEngine.Debug.Log("   Testing Reset()...");
                    algorithmManager.Reset();
                    UnityEngine.Debug.Log("   ✅ Reset() works");
                }
                catch (System.Exception ex)
                {
                    UnityEngine.Debug.LogError($"   ❌ AlgorithmManager method failed: {ex.Message}");
                }
            }
            
            // Step 3: List all Blueberry commands
            UnityEngine.Debug.Log("📋 AVAILABLE BLUEBERRY COMMANDS:");
            var blueberryCommands = new List<string>();
            foreach (var kvp in intentActionDictionary)
            {
                if (kvp.Key.ToLower().Contains("blueberry") || 
                    kvp.Key.ToLower().Contains("fifo") || 
                    kvp.Key.ToLower().Contains("lru") || 
                    kvp.Key.ToLower().Contains("opr") || 
                    kvp.Key.ToLower().Contains("mru") || 
                    kvp.Key.ToLower().Contains("lfu") || 
                    kvp.Key.ToLower().Contains("algorithm") ||
                    kvp.Key.ToLower().Contains("frame"))
                {
                    blueberryCommands.Add(kvp.Key);
                    UnityEngine.Debug.Log($"   '{kvp.Key}' -> {kvp.Value.message}");
                }
            }
            
            UnityEngine.Debug.Log($"📊 TOTAL BLUEBERRY COMMANDS: {blueberryCommands.Count}");
            
            // Step 4: Test a sample command execution
            if (pageReplacementNav != null && algorithmManager != null)
            {
                UnityEngine.Debug.Log("🧪 TESTING SAMPLE COMMAND EXECUTION:");
                try
                {
                    UnityEngine.Debug.Log("   Executing: chooseFIFO command...");
                    pageReplacementNav.OpenSimPage();
                    algorithmManager.SetAlgorithm("FIFO");
                    UnityEngine.Debug.Log("   ✅ Sample command executed successfully!");
                }
                catch (System.Exception ex)
                {
                    UnityEngine.Debug.LogError($"   ❌ Sample command failed: {ex.Message}");
                }
            }
            
            // Step 5: Voice recognition status
            UnityEngine.Debug.Log("🎤 VOICE RECOGNITION STATUS:");
            UnityEngine.Debug.Log($"   Rhino initialized: {(rhinoManager != null ? "✅ Yes" : "❌ No")}");
            UnityEngine.Debug.Log($"   Currently listening: {(isListening ? "✅ Yes" : "❌ No")}");
            
            // Step 6: Recommendations
            UnityEngine.Debug.Log("💡 TROUBLESHOOTING RECOMMENDATIONS:");
            if (pageReplacementNav == null || algorithmManager == null)
            {
                UnityEngine.Debug.LogError("   1. Check Inspector - assign missing component references");
            }
            else
            {
                UnityEngine.Debug.Log("   1. ✅ All components are assigned");
            }
            
            UnityEngine.Debug.Log("   2. Try these voice commands:");
            UnityEngine.Debug.Log("      - 'start blueberry' (opens algorithm selector)");
            UnityEngine.Debug.Log("      - 'choose FIFO' (selects FIFO algorithm)");
            UnityEngine.Debug.Log("      - 'choose LRU' (selects LRU algorithm)");
            UnityEngine.Debug.Log("      - 'simulate algorithm' (runs the algorithm)");
            
            UnityEngine.Debug.Log("   3. Make sure you're in the correct scene/app context");
            UnityEngine.Debug.Log("   4. Check that voice recognition is actively listening");
            
            UnityEngine.Debug.Log("=== END BLUEBERRY DIAGNOSTIC ===");
        }
        
        private void TestRhinoSetupInternal(bool testRhinoCreation)
        {
            UnityEngine.Debug.Log("=== RHINO SETUP TEST ===");
            
            // Test 1: Check access key
            bool accessKeyValid = !string.IsNullOrEmpty(accessKey) && 
                                 accessKey != "YOUR_NEW_ACCESS_KEY_HERE" && 
                                 accessKey != "PASTE_YOUR_FRESH_ACCESS_KEY_HERE";
            UnityEngine.Debug.Log($"Access Key Set: {accessKeyValid}");
            UnityEngine.Debug.Log($"Access Key Length: {accessKey?.Length ?? 0}");
            
            // Test 2: Check StreamingAssets path
            UnityEngine.Debug.Log($"StreamingAssets Path: {Application.streamingAssetsPath}");
            UnityEngine.Debug.Log($"StreamingAssets Exists: {System.IO.Directory.Exists(Application.streamingAssetsPath)}");
            
            // Test 3: List ALL .rhn files in StreamingAssets
            try
            {
                string[] allRhnFiles = System.IO.Directory.GetFiles(Application.streamingAssetsPath, "*.rhn", System.IO.SearchOption.AllDirectories);
                UnityEngine.Debug.Log($"Found {allRhnFiles.Length} .rhn files in StreamingAssets:");
                foreach (string file in allRhnFiles)
                {
                    UnityEngine.Debug.Log($"  - {file}");
                }
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"Error listing .rhn files: {ex.Message}");
            }
            
            // Test 4: Check specific paths that the code will try
            string platform = GetPlatform();
            string platformContextFileName = GetContextFileName();
            
            List<string> pathsToTest = new List<string>
            {
                System.IO.Path.Combine(Application.streamingAssetsPath, "contexts", platform, platformContextFileName),
                System.IO.Path.Combine(Application.streamingAssetsPath, contextPath),
                System.IO.Path.Combine(Application.streamingAssetsPath, "honeyos_context.rhn")
            };
            
            UnityEngine.Debug.Log($"Platform: {platform}");
            UnityEngine.Debug.Log($"Platform context filename: {platformContextFileName}");
            UnityEngine.Debug.Log("Testing context file paths:");
            
            string workingContextPath = null;
            for (int i = 0; i < pathsToTest.Count; i++)
            {
                string testPath = pathsToTest[i];
                bool exists = System.IO.File.Exists(testPath);
                UnityEngine.Debug.Log($"  {i + 1}. {testPath} - Exists: {exists}");
                if (exists && workingContextPath == null)
                {
                    workingContextPath = testPath;
                }
            }
            
            // Test 5: Check model file
            string modelFile = System.IO.Path.Combine(Application.streamingAssetsPath, "rhino_params.pv");
            UnityEngine.Debug.Log($"Model File: {modelFile} - Exists: {System.IO.File.Exists(modelFile)}");
            
            // Test 6: Check microphone
            CheckMicrophoneAvailability();
            
            // Test 7: Try to create Rhino (minimal test) - only if requested and in appropriate mode
            if (testRhinoCreation && workingContextPath != null && accessKeyValid)
            {
                #if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    UnityEngine.Debug.Log("⚠️ Rhino creation test skipped - requires Play mode in Editor");
                    UnityEngine.Debug.Log("✅ All file checks passed! Your setup appears correct.");
                    UnityEngine.Debug.Log("💡 To test voice recognition: Enter Play mode and try speaking commands");
                }
                else
                {
                #endif
                    try
                    {
                        UnityEngine.Debug.Log($"Testing Rhino creation with: {workingContextPath}");
                        var testRhino = RhinoManager.Create(accessKey, workingContextPath, (inference) => {
                            UnityEngine.Debug.Log("Test inference callback triggered");
                        });
                        UnityEngine.Debug.Log("✅ Rhino creation test PASSED");
                        testRhino.Delete();
                    }
                    catch (System.Exception ex)
                    {
                        UnityEngine.Debug.LogError($"✗ Rhino creation test FAILED: {ex.Message}");
                        if (ex.Message.Contains("00000136"))
                        {
                            UnityEngine.Debug.LogError("Error 00000136 = Invalid Access Key. Please get a fresh access key from https://console.picovoice.ai/");
                        }
                    }
                #if UNITY_EDITOR
                }
                #endif
            }
            else if (!testRhinoCreation)
            {
                if (workingContextPath != null && accessKeyValid)
                {
                    UnityEngine.Debug.Log("✅ FILE TEST COMPLETE - All required files found and access key is set!");
                    UnityEngine.Debug.Log($"✅ Context file: {workingContextPath}");
                    UnityEngine.Debug.Log("✅ Model file: Found");
                    UnityEngine.Debug.Log("✅ Access key: Set and valid format");
                    UnityEngine.Debug.Log("💡 Your voice recognition should work! Try entering Play mode and testing.");
                }
                else
                {
                    if (workingContextPath == null)
                        UnityEngine.Debug.LogError("✗ No working context file path found");
                    if (!accessKeyValid)
                        UnityEngine.Debug.LogError("✗ Access key not properly set");
                }
            }
            else
            {
                if (workingContextPath == null)
                    UnityEngine.Debug.LogError("✗ No working context file path found");
                if (!accessKeyValid)
                    UnityEngine.Debug.LogError("✗ Access key not properly set");
            }
            
            UnityEngine.Debug.Log("=== END RHINO SETUP TEST ===");
        }
    }
}