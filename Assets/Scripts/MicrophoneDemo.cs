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
        public string accessKey = "YOUR_ACCESS_KEY_HERE"; // Replace with your Picovoice access key
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

        [Header("UI")]
        public Button button;
        public TextMeshProUGUI outputText;
        public float outputTextDuration = 2.0f;

        [Header("Intent Actions")]
        private Dictionary<string, (Action function, string message)> intentActionDictionary;

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

        private void Awake()
        {
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            // Get or add microphone manager
            microphoneManager = GetComponent<PicovoiceMicrophoneManager>();
            if (microphoneManager == null)
            {
                microphoneManager = gameObject.AddComponent<PicovoiceMicrophoneManager>();
            }

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
                
                // Build the full path to the context file
                string fullContextPath;
                
                #if UNITY_EDITOR
                    fullContextPath = System.IO.Path.Combine(Application.streamingAssetsPath, contextPath);
                #elif UNITY_ANDROID
                    fullContextPath = contextPath; // On Android, use relative path
                #else
                    fullContextPath = System.IO.Path.Combine(Application.streamingAssetsPath, contextPath);
                #endif
                
                UnityEngine.Debug.Log($"Initializing Rhino - Context path: {fullContextPath}");
                UnityEngine.Debug.Log($"Access key length: {accessKey.Length}");
                
                if (accessKey == "YOUR_ACCESS_KEY_HERE")
                {
                    UnityEngine.Debug.LogError("Access key not set! Please set your Picovoice access key.");
                    DisplayOutputText("Access key not configured");
                    return;
                }
                
                // Verify context file exists
                if (!System.IO.File.Exists(fullContextPath))
                {
                    UnityEngine.Debug.LogError($"Context file not found at: {fullContextPath}");
                    DisplayOutputText("Context file not found");
                    return;
                }
                
                rhinoManager = RhinoManager.Create(
                    accessKey,
                    fullContextPath,
                    OnInferenceResult);
                
                UnityEngine.Debug.Log("Rhino Speech-to-Intent initialized successfully");
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to initialize Rhino: {ex.Message}");
                UnityEngine.Debug.LogError($"Stack trace: {ex.StackTrace}");
                DisplayOutputText("Voice recognition initialization failed");
            }
        }

        private void InitIntentActionDictionary()
        {
            intentActionDictionary = new Dictionary<string, (Action function, string message)>
            {
                // App Management - using original command phrases as fallback
                { "openSweet", ( () => desktopManager.OpenApp(0), "Opening Sweet") },
                { "open sweet", ( () => desktopManager.OpenApp(0), "Opening Sweet") },
                { "openSugar", ( () => desktopManager.OpenApp(2), "Opening Sugar") },
                { "open sugar", ( () => desktopManager.OpenApp(2), "Opening Sugar") },
                { "openCake", ( () => desktopManager.OpenApp(1), "Opening Cake") },
                { "open cake", ( () => desktopManager.OpenApp(1), "Opening Cake") },
                { "closeApp", ( () => desktopManager.CloseCurrentApp(), "Closing app") },
                { "close application", ( () => desktopManager.CloseCurrentApp(), "Closing app") },
                { "closeAll", ( () => desktopManager.CloseAllApps(), "Closing all apps") },
                { "close all", ( () => desktopManager.CloseAllApps(), "Closing all apps") },
                { "minimizeApp", ( () => desktopManager.MinCurrentApp(), "Minimizing app") },
                { "minimize application", ( () => desktopManager.MinCurrentApp(), "Minimizing app") },
                
                // File Operations
                { "saveFile", ( () => textEditor.Save(), "Saving File") },
                { "save file", ( () => textEditor.Save(), "Saving File") },
                { "saveAsFile", ( () => textEditor.SaveAs(), "Saving File As") },
                { "save as file", ( () => textEditor.SaveAs(), "Saving File As") },
                { "openFile", ( () => textEditor.OpenFile(), "Opening File") },
                { "open file", ( () => textEditor.OpenFile(), "Opening File") },
                { "newFile", ( () => textEditor.NewFile(), "Creating New File") },
                { "new file", ( () => textEditor.NewFile(), "Creating New File") },
                
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
            };
        }

        private void Update()
        {
            HandleSpaceBarInput();
            UpdateVADIndicator();
        }

        private void UpdateVADIndicator()
        {
            Color color;

            if (isListening)
            {
                // Use microphone manager's voice detection if available
                if (microphoneManager != null && microphoneManager.IsRecording)
                    color = microphoneManager.IsVoiceDetected ? voiceDetectedColor : voiceUndetectedColor;
                else
                    color = voiceDetectedColor;
            }
            else
                color = defaultIndicatorColor;

            vadIndicatorImage.color = color;
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

                audioSource.clip = startRecordingSound;
                audioSource.Play();

                UnityEngine.Debug.Log("Starting Rhino processing...");
                rhinoManager.Process();
                isListening = true;
                
                UnityEngine.Debug.Log("Listening for voice commands...");
                string message = "Honey, what can I do for you?";
                DisplayOutputText(message);
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to start listening: {ex.Message}");
                
                // Try to reinitialize Rhino if there's an error
                InitializeRhino();
                
                DisplayOutputText("Failed to start voice recognition");
            }
        }

        private void StopListening()
        {
            try
            {
                audioSource.clip = stopRecordingSound;
                audioSource.Play();

                isListening = false;

                if (isButtonUsed)
                    isButtonUsed = false;
                    
                // Don't reset Rhino when manually stopped - it should be reusable
                // ResetRhinoForNextCommand();
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to stop listening: {ex.Message}");
            }
        }

        private void OnInferenceResult(Inference inference)
        {
            isListening = false;
            commandCount++;
            
            UnityEngine.Debug.Log($"Command #{commandCount} - Intent understood: {inference.IsUnderstood}");
            
            if (inference.IsUnderstood)
            {
                string intent = inference.Intent;
                Dictionary<string, string> slots = inference.Slots;
                
                UnityEngine.Debug.Log($"Executing intent: {intent}");
                ExecuteIntent(intent, slots);
            }
            else
            {
                UnityEngine.Debug.Log("Intent not understood");
                DisplayOutputText("Sorry, I didn't understand that command.");
            }
            
            // Reset Rhino for next command with minimal delay to allow animations to start
            StartCoroutine(DelayedRhinoReset());
        }

        private void ExecuteIntent(string intent, Dictionary<string, string> slots)
        {
            // First try exact match
            if (intentActionDictionary.ContainsKey(intent))
            {
                var action = intentActionDictionary[intent];
                action.function();
                DisplayOutputText(action.message);
                return;
            }
            
            // If no exact match, try to find a partial match or similar intent
            foreach (var kvp in intentActionDictionary)
            {
                // Try case-insensitive match
                if (string.Equals(kvp.Key, intent, StringComparison.OrdinalIgnoreCase))
                {
                    kvp.Value.function();
                    DisplayOutputText(kvp.Value.message);
                    return;
                }
                
                // Try partial match (intent contains key or key contains intent)
                if (intent.ToLower().Contains(kvp.Key.ToLower()) || kvp.Key.ToLower().Contains(intent.ToLower()))
                {
                    kvp.Value.function();
                    DisplayOutputText(kvp.Value.message);
                    return;
                }
            }
            
            // If still no match, log for debugging
            UnityEngine.Debug.LogWarning($"No match found for intent: '{intent}'");
            DisplayOutputText("Command not recognized.");
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

        private void OnDestroy()
        {
            if (rhinoManager != null)
            {
                rhinoManager.Delete();
            }
        }
    }
}