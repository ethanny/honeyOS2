# HoneyOS Picovoice Migration Guide

## Overview
This guide will help you migrate your HoneyOS Unity application from Whisper speech-to-text to Picovoice Rhino speech-to-intent recognition.

## Why Rhino Speech-to-Intent?
- **Better for voice commands**: Rhino is specifically designed for voice command recognition
- **Higher accuracy**: Direct intent recognition vs speech-to-text + parsing
- **Lower latency**: No intermediate text processing step
- **On-device processing**: Privacy-focused, no cloud dependency
- **Structured output**: Returns intents and slots directly

## Prerequisites
1. Unity 2021.3 or later
2. Picovoice account (free tier available)
3. Windows, macOS, or Linux development environment

## Step 1: Get Picovoice Account and Access Key
1. Go to [Picovoice Console](https://console.picovoice.ai/)
2. Sign up for a free account
3. Copy your Access Key from the dashboard
4. Keep this key secure - you'll need it in your Unity project

## Step 2: Download Picovoice Unity SDK
1. Go to [Picovoice Unity SDK releases](https://github.com/Picovoice/rhino/releases)
2. Download the latest `pv-unity-rhino-*.unitypackage`
3. Import the package into your Unity project:
   - Assets → Import Package → Custom Package
   - Select the downloaded .unitypackage file
   - Import all assets

## Step 3: Create Custom Rhino Context
1. Go to [Picovoice Console](https://console.picovoice.ai/)
2. Navigate to "Rhino Speech-to-Intent"
3. Click "Create Context"
4. Name your context "HoneyOS"
5. Add the following intents and expressions:

### App Management
- **Intent**: `openSweet`
  - Expressions: "open sweet", "start sweet", "honey start sweet", "honey open sweet"
- **Intent**: `openSugar`
  - Expressions: "open sugar", "start sugar", "honey start sugar", "honey open sugar"
- **Intent**: `openCake`
  - Expressions: "launch cake", "open cake", "start cake", "honey start cake", "honey launch cake", "honey open cake"
- **Intent**: `openBlueberry`
  - Expressions: "honey start blueberry", "open blueberry", "honey open blueberry"
- **Intent**: `closeApp`
  - Expressions: "exit app", "close app", "close application"
- **Intent**: `closeAll`
  - Expressions: "close applications", "close apps", "close all"
- **Intent**: `minimizeApp`
  - Expressions: "minimize", "minimize application", "minimize app"
- **Intent**: `maximizeScreen`
  - Expressions: "maximize window size"
- **Intent**: `changeSize`
  - Expressions: "reset window size", "change window size"

### File Operations
- **Intent**: `saveFile`
  - Expressions: "save", "save document", "save file"
- **Intent**: `saveAsFile`
  - Expressions: "save file as", "save as", "save as file"
- **Intent**: `openFile`
  - Expressions: "load file", "open document", "open file"
- **Intent**: `newFile`
  - Expressions: "create file", "new document", "new file"

### Text Editing
- **Intent**: `undo`
  - Expressions: "undo", "undo changes"
- **Intent**: `redo`
  - Expressions: "redo changes", "redo"
- **Intent**: `copy`
  - Expressions: "copy text", "copy"
- **Intent**: `cut`
  - Expressions: "cut text", "cut"
- **Intent**: `paste`
  - Expressions: "paste text", "paste"

### Help Navigation
- **Intent**: `goBack`
  - Expressions: "back", "return", "go back"
- **Intent**: `openSystemBasics`
  - Expressions: "system basics", "open system basics"
- **Intent**: `openAppGuide`
  - Expressions: "open application guide", "application guide", "app guide", "open app guide"
- **Intent**: `openAboutUs`
  - Expressions: "open about us", "honey open about us"

### Scheduler Operations (Cake)
- **Intent**: `startCake`
  - Expressions: "begin cake", "play cake"
- **Intent**: `backToPolicySelection`
  - Expressions: "policy selection", "back to policy selection"
- **Intent**: `chooseFCFS`
  - Expressions: "f c f s", "select f c f s", "choose first come first serve"
- **Intent**: `choosePriority`
  - Expressions: "priority", "choose priority", "select priority"
- **Intent**: `chooseRoundRobin`
  - Expressions: "round robin", "choose round robin", "select round robin"
- **Intent**: `chooseSJF`
  - Expressions: "s j f", "choose s j f", "choose shortest job first"
- **Intent**: `homeScheduler`
  - Expressions: "go back to cake ville", "home"

### Page Replacement Operations (Blueberry)
- **Intent**: `startBlueberry`
  - Expressions: "begin blueberry", "start blueberry"
- **Intent**: `chooseFIFO`
  - Expressions: "select FIFO", "select first in first out", "choose first in first out", "choose FIFO"
- **Intent**: `chooseLRU`
  - Expressions: "select l r u", "select least recently used", "choose least recently used", "choose l r u"
- **Intent**: `chooseMRU`
  - Expressions: "select most recently used", "choose most recently used", "select M R U", "choose M R U"
- **Intent**: `chooseLFU`
  - Expressions: "select least frequently used", "choose least frequently used", "select L F U", "choose L F U"
- **Intent**: `chooseOPR`
  - Expressions: "choose o p r", "select o p r", "select optimal page replacement", "choose optimal page replacement"
- **Intent**: `backToAlgorithmSelection`
  - Expressions: "back to algorithm selection"
- **Intent**: `homeBlueberry`
  - Expressions: "home to blueberry ville", "back to blueberry ville"

### Frame Management
- **Intent**: `setFrameCount1`
  - Expressions: "set frame count to one"
- **Intent**: `setFrameCount2`
  - Expressions: "set frame count to two"
- **Intent**: `setFrameCount3`
  - Expressions: "set frame count to three"
- **Intent**: `setFrameCount4`
  - Expressions: "set frame count to four"
- **Intent**: `setFrameCount5`
  - Expressions: "set frame count to five"
- **Intent**: `setFrameCount6`
  - Expressions: "set frame count to six"
- **Intent**: `setFrameCount7`
  - Expressions: "set frame count to seven"
- **Intent**: `increaseFrames`
  - Expressions: "increase frames"
- **Intent**: `decreaseFrames`
  - Expressions: "decrease frames"

### Simulation Control
- **Intent**: `playSimulation`
  - Expressions: "play", "run simulation", "start simulation", "play simulation"
- **Intent**: `pauseSimulation`
  - Expressions: "pause", "pause simulation"
- **Intent**: `stopSimulation`
  - Expressions: "stop", "end simulation", "stop simulation"
- **Intent**: `addProcess`
  - Expressions: "new process", "create process", "add process"
- **Intent**: `nextStep`
  - Expressions: "continue", "next step", "next"

### Algorithm Control
- **Intent**: `simulateAlgorithm`
  - Expressions: "simulate algorithm"
- **Intent**: `pauseAlgorithm`
  - Expressions: "pause algorithm", "pause page replacement"
- **Intent**: `resumeAlgorithm`
  - Expressions: "resume algorithm", "resume page replacement"
- **Intent**: `resetAlgorithm`
  - Expressions: "stop algorithm", "clear page replacement", "reset page replacement"

6. Train the context (this may take a few minutes)
7. Download the generated `.rhn` file
8. Place the `.rhn` file in the appropriate platform folder under `Assets/StreamingAssets/contexts/` and rename it:
   - **Windows**: Place in `Assets/StreamingAssets/contexts/windows/` and rename to `honeyos_context_windows.rhn`
   - **Mac**: Place in `Assets/StreamingAssets/contexts/mac/` and rename to `honeyos_context_mac.rhn`
   - **Linux**: Place in `Assets/StreamingAssets/contexts/linux/` and rename to `honeyos_context_linux.rhn`
   - **Android**: Place in `Assets/StreamingAssets/contexts/android/` and rename to `honeyos_context_android.rhn`
   - **iOS**: Place in `Assets/StreamingAssets/contexts/ios/` and rename to `honeyos_context_ios.rhn`

## Step 4: Configure Unity Project
1. Open your Unity project
2. Find the `MicrophoneDemo` script in the scene
3. In the Inspector, set the following:
   - **Access Key**: Paste your Picovoice access key
   - **Context Path**: Use the appropriate platform-specific path:
     - **Windows**: `honeyos_context_windows.rhn`
     - **Mac**: `honeyos_context_mac.rhn`
     - **Linux**: `honeyos_context_linux.rhn`
     - **Android**: `honeyos_context_android.rhn`
     - **iOS**: `honeyos_context_ios.rhn`

## Step 5: Remove Whisper Dependencies
1. Remove or disable the old Whisper-related scripts and assets
2. Remove Whisper package references from your project
3. Clean up any unused Whisper assets

## Step 6: Test the Integration
1. Build and run your Unity project
2. Press the voice command button or hold spacebar
3. Try saying commands like:
   - "Open sweet"
   - "Save file"
   - "Start simulation"
   - "Go back"

## Troubleshooting

### Common Issues:
1. **"Access Key not valid"**: Double-check your access key from Picovoice Console
2. **"Context file not found"**: Ensure the `.rhn` file is in `Assets/StreamingAssets/`
3. **Commands not recognized**: Verify your context includes the exact intent names used in the code
4. **Microphone permissions**: Ensure your app has microphone permissions

### Debug Tips:
- Check Unity Console for Rhino initialization messages
- Enable verbose logging in Rhino settings
- Test with simple commands first
- Verify microphone is working with other applications

## Performance Considerations
- Rhino processes audio on-device, so no internet connection required
- Lower latency than cloud-based solutions
- Minimal CPU usage compared to general speech-to-text
- Memory usage depends on context complexity

## Extending the System
To add new voice commands:
1. Add new intents to your Picovoice Console context
2. Retrain and download the updated `.rhn` file
3. Add corresponding entries to `intentActionDictionary` in `MicrophoneDemo.cs`
4. Implement the action functions

## Support
- [Picovoice Documentation](https://picovoice.ai/docs/)
- [Picovoice Unity SDK GitHub](https://github.com/Picovoice/rhino/tree/master/binding/unity)
- [Picovoice Community Forum](https://github.com/Picovoice/rhino/discussions)

## License Notes
- Picovoice offers a free tier with usage limits
- Check [Picovoice pricing](https://picovoice.ai/pricing/) for commercial usage
- Unity SDK will be deprecated after December 15, 2025 - plan accordingly 