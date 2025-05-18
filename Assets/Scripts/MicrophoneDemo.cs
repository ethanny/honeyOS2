using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using Whisper.Utils;
using Button = UnityEngine.UI.Button;
using Toggle = UnityEngine.UI.Toggle;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace Whisper.Samples
{
    /// <summary>
    /// Record audio clip from microphone and make a transcription.
    /// </summary>
    public class MicrophoneDemo : MonoBehaviour
    {
        public WhisperManager whisper;
        public MicrophoneRecord microphoneRecord;
        public bool streamSegments = true;
        private string _buffer;
        public bool isButtonUsed = false;
        // public bool printLanguage = true;

        // Add Apps
        [Header("Apps")]
        public DesktopManager desktopManager;
        public TextEditorController textEditor;
        public PageController help;
        public ProcessManager processManager;
        public PageController2 scheduler;
        public AlgorithmManager algorithmManager;

        [Header("UI")]
        public Button button;
        // public Text buttonText;
        public TextMeshProUGUI outputText;
        public float outputTextDuration = 2.0f;

        // public Text timeText;
        // public Dropdown languageDropdown;
        // public Toggle translateToggle;
        // public Toggle vadToggle;
        // public ScrollRect scroll;

        [Header("Dictionary")]
        private Dictionary<string, (Action function, string message)> commandDictionary;

        [Header("Voice Activity Detection (VAD)")]
        public Image vadIndicatorImage;
        public Color defaultIndicatorColor;
        public Color voiceDetectedColor;
        public Color voiceUndetectedColor;

        [Header("SFX")]
        public AudioSource audioSource; // Reference to the AudioSource component 
        public AudioClip startRecordingSound; // Assign this in the Unity editor
        public AudioClip stopRecordingSound; // Assign this in the Unity editor
        public AudioClip whatCanIDoSound;

        [Header("SpaceBarHandler")]
        public float longLongPressDuration = 0.2f; // Adjust this duration in seconds as needed
        private float pressStartTime;
        private bool isLongLongPressTriggered = false;

        private void Awake()
        {
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            whisper.OnNewSegment += OnNewSegment;
            // whisper.OnProgress += OnProgressHandler;

            microphoneRecord.OnRecordStop += OnRecordStop;

            button.onClick.AddListener(OnButtonPressed);

            InitCommandDictionary();
            // languageDropdown.value = languageDropdown.options
            //     .FindIndex(op => op.text == whisper.language);
            // languageDropdown.onValueChanged.AddListener(OnLanguageChanged);

            // translateToggle.isOn = whisper.translateToEnglish;
            // translateToggle.onValueChanged.AddListener(OnTranslateChanged);

            // vadToggle.isOn = microphoneRecord.vadStop;
            // vadToggle.onValueChanged.AddListener(OnVadChanged);
        }

        // Function to initialize the command dictionary
        private void InitCommandDictionary()
        {
            commandDictionary = new Dictionary<string, (Action function, string message)>
            {
                { "open sweet", ( () => desktopManager.OpenApp(0), "Opening sweet") },
                // { "open file manager", ( () => desktopManager.OpenApp(1), "Opening File Manager") },
                { "open sugar", ( () => desktopManager.OpenApp(2), "Opening Sugar") },
                { "close application", ( () => desktopManager.CloseCurrentApp(), "Closing app") },
                { "close all", ( () => desktopManager.CloseAllApps(), "Closing all app") },
                { "minimize application", ( () => desktopManager.MinCurrentApp() ,"Minimizing app") },
                { "save file", ( () => textEditor.Save(), "Saving File") },
                { "save as file", ( () => textEditor.SaveAs(), "Saving File") },
                { "open file", ( () => textEditor.OpenFile(), "Opening File") },
                { "new file", ( () => textEditor.NewFile(), "Creating New File") },
                { "undo", ( () => textEditor.Undo(), "Undo text changes") },
                { "redo", ( () => textEditor.Redo(), "Redo text changes") },
                { "copy", ( () => textEditor.Copy(), "Copying selected text") },
                { "cut", ( () => textEditor.Cut(), "Cutting selected text") },
                { "paste", ( () => textEditor.Paste(), "Pasting text from clipboard") },
                { "go back", ( () => help.OpenHome(), "Going back to Home Page") },
                { "open system basics", ( () => help.OpenSystemBasics(), "Opening System Basics") },
                { "open application guide", ( () => help.OpenAppGuide(), "Opening Application Guide") },
                { "open about us", ( () => help.OpenAboutUs(), "Opening About Us") },
                // Add more commands and their corresponding functions and messages as needed
                { "open cake", ( () => desktopManager.OpenApp(1), "Opening Cake") },
                { "start cake", ( () => scheduler.OpenSelectionPage(), "Opening Policy selector") },
                { "back to policy selection", ( () => {scheduler.OpenSelectionPage(); processManager.Stop();}, "Opening Policy selector") },
                { "choose first come first serve", ( () => {scheduler.OpenSimPage(); processManager.SetSchedulingPolicy("FCFS");}, "FCFS Simulator") },
                { "choose priority", ( () => {scheduler.OpenSimPage(); processManager.SetSchedulingPolicy("Prio");}, "Priority Simulator") },
                { "choose round robin", ( () => {scheduler.OpenSimPage(); processManager.SetSchedulingPolicy("RR");}, "Round Robin Simulator") },
                { "choose shortest job first", ( () => {scheduler.OpenSimPage(); processManager.SetSchedulingPolicy("SJF");}, "Shortest Job First Simulator") },
                { "play simulation", ( () => processManager.Play(), "Starting simulation") },
                
                // Page Replacement Algorithm commands
                { "start page replacement", ( () => scheduler.OpenSelectionPage(), "Opening Algorithm selector") },
                { "back to algorithm selection", ( () => {scheduler.OpenSelectionPage();}, "Opening Algorithm selector") },
                { "choose fifo", ( () => {scheduler.OpenSimPage(); algorithmManager.SetAlgorithm("FIFO");}, "FIFO Algorithm") },
                { "choose lru", ( () => {scheduler.OpenSimPage(); algorithmManager.SetAlgorithm("LRU");}, "LRU Algorithm") },
                { "choose optimal", ( () => {scheduler.OpenSimPage(); algorithmManager.SetAlgorithm("OPT");}, "Optimal Algorithm") },
                { "choose clock", ( () => {scheduler.OpenSimPage(); algorithmManager.SetAlgorithm("CLOCK");}, "Clock Algorithm") },
                
                { "pause", ( () => processManager.Pause(), "Pausing simulation") },
                { "add process", ( () => processManager.AddProcess(false), "Adding Process") },
                { "stop simulation", ( () => processManager.Stop(), "Stoping simulation") },
                { "next", ( () => processManager.Next(), "Next") },
            };
        }

        private void Update()
        {

            // Call the separate function to handle space bar input events
            HandleSpaceBarInput();

            Color color;

            if (microphoneRecord.IsRecording)
                color = microphoneRecord.IsVoiceDetected ? voiceDetectedColor : voiceUndetectedColor;

            else
                color = defaultIndicatorColor;

            vadIndicatorImage.color = color;

        }

        private void OnVadChanged(bool vadStop)
        {
            microphoneRecord.vadStop = vadStop;
        }

        private void OnButtonPressed()
        {

            if (!microphoneRecord.IsRecording)
            {
                // Play the start recording sound effect
                audioSource.clip = startRecordingSound;
                audioSource.Play();

                // audioSource.clip = whatCanIDoSound;
                // audioSource.Play();

                microphoneRecord.StartRecord();
                // buttonText.text = "Stop";
                UnityEngine.Debug.Log("Recording...");

                string message = "Honey, what can I do for you?";
                DisplayOutputText(message);
            }

            else
            {
                // Play the start recording sound effect
                audioSource.clip = stopRecordingSound;
                audioSource.Play();

                microphoneRecord.StopRecord();
                // buttonText.text = "Record";
                UnityEngine.Debug.Log("Stop recording.");

                // UnityEngine.Debug.Log(isButtonUsed);
                if (isButtonUsed)
                    isButtonUsed = false;

            }

        }

        private async void OnRecordStop(AudioChunk recordedAudio)
        {
            // buttonText.text = "Record";
            UnityEngine.Debug.Log("Stop recording.");
            _buffer = "";

            var sw = new Stopwatch();
            sw.Start();

            var res = await whisper.GetTextAsync(recordedAudio.Data, recordedAudio.Frequency, recordedAudio.Channels);
            if (res == null || !outputText)
                return;

            var time = sw.ElapsedMilliseconds;
            var rate = recordedAudio.Length / (time * 0.001f);
            // timeText.text = $"Time: {time} ms\nRate: {rate:F1}x";

            var text = res.Result;

            // if (printLanguage)
            //     text += $"\n\nLanguage: {res.Language}";

            RunCommand(text);
            // UiUtils.ScrollDown(scroll);
        }

        // private void OnLanguageChanged(int ind)
        // {
        //     var opt = languageDropdown.options[ind];
        //     whisper.language = opt.text;
        // }

        // private void OnTranslateChanged(bool translate)
        // {
        //     whisper.translateToEnglish = translate;
        // }

        // private void OnProgressHandler(int progress)
        // {
        //     if (!timeText)
        //         return;
        //     timeText.text = $"Progress: {progress}%";
        // }

        private void OnNewSegment(WhisperSegment segment)
        {
            if (!streamSegments || !outputText)
                return;

            _buffer += segment.Text;
            // outputText.text = _buffer + "...";
            // UiUtils.ScrollDown(scroll);
        }

        private void DisplayOutputText(string message)
        {
            StartCoroutine(HandleDisplayOutputText(message));
        }

        // Define a coroutine to display output text for a specified duration
        private IEnumerator HandleDisplayOutputText(string message)
        {
            // Display the message
            outputText.text = message;

            // Wait for the specified duration
            yield return new WaitForSeconds(outputTextDuration);

            // Clear the output text
            outputText.text = "";
        }

        private void RunCommand(string command)
        {
            // Convert the command text to lower case for easier matching
            string normalizedCommand = command.Trim().ToLower();

            // Iterate over the dictionary to find a command that is contained in the input text
            foreach (var entry in commandDictionary)
            {
                // Check if a command contains a specific string rather than an exact match
                if (normalizedCommand.Contains(entry.Key))
                {
                    // Execute the corresponding function and display the output message
                    entry.Value.function();
                    DisplayOutputText(entry.Value.message); // Display message for 2 seconds
                    return; // Exit the loop after finding a match
                }
            }

            // Handle unknown commands (e.g., log a message or ignore)
            DisplayOutputText("Sorry, bitch.");
            UnityEngine.Debug.Log($"Unknown command: {command}");
        }

        // Handle space bar input events
        void HandleSpaceBarInput()
        {
            if (!isButtonUsed)
            {
                // UnityEngine.Debug.Log("Hey" + isButtonUsed);

                // Check if the space bar is pressed down
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    pressStartTime = Time.time; // Record the start time of the press
                    isLongLongPressTriggered = false; // Reset the long long press trigger flag
                }

                // Check if the space bar is being held down
                if (Input.GetKey(KeyCode.Space))
                {
                    // Calculate the duration the space bar has been held down
                    float holdDuration = Time.time - pressStartTime;

                    // If the hold duration exceeds longLongPressDuration and the function has not been triggered yet
                    if (!isLongLongPressTriggered && holdDuration >= longLongPressDuration)
                    {
                        // Execute the function during long long press
                        OnButtonPressed();
                        // Set the flag to true to prevent repeated execution while the key is still held down
                        isLongLongPressTriggered = true;
                    }
                }

                // Check if the space bar is released
                if (Input.GetKeyUp(KeyCode.Space))
                {
                    if (isLongLongPressTriggered)
                        // Execute the function when the space bar is released
                        OnButtonPressed();
                }
            }

        }

        public void SetIsButtonUsed(bool boolean)
        {
            isButtonUsed = boolean;
        }
    }
}