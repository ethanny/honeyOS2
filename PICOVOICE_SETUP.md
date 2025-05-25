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
  - Expressions: "open sweet", "launch sweet", "start sweet"
- **Intent**: `openSugar`
  - Expressions: "open sugar", "launch sugar", "start sugar"
- **Intent**: `openCake`
  - Expressions: "open cake", "launch cake", "start cake"
- **Intent**: `closeApp`
  - Expressions: "close application", "close app", "exit app"
- **Intent**: `closeAll`
  - Expressions: "close all", "close all apps", "exit all"
- **Intent**: `minimizeApp`
  - Expressions: "minimize application", "minimize app", "minimize"

### File Operations
- **Intent**: `saveFile`
  - Expressions: "save file", "save document", "save"
- **Intent**: `saveAsFile`
  - Expressions: "save as file", "save as", "save file as"
- **Intent**: `openFile`
  - Expressions: "open file", "open document", "load file"
- **Intent**: `newFile`
  - Expressions: "new file", "create file", "new document"

### Text Editing
- **Intent**: `undo`
  - Expressions: "undo", "undo changes"
- **Intent**: `redo`
  - Expressions: "redo", "redo changes"
- **Intent**: `copy`
  - Expressions: "copy", "copy text"
- **Intent**: `cut`
  - Expressions: "cut", "cut text"
- **Intent**: `paste`
  - Expressions: "paste", "paste text"

### Help Navigation
- **Intent**: `goBack`
  - Expressions: "go back", "back", "return"
- **Intent**: `openSystemBasics`
  - Expressions: "open system basics", "system basics"
- **Intent**: `openAppGuide`
  - Expressions: "open application guide", "app guide", "application guide"
- **Intent**: `openAboutUs`
  - Expressions: "open about us", "about us"

### Scheduler Operations
- **Intent**: `startCake`
  - Expressions: "start cake", "begin cake"
- **Intent**: `backToPolicySelection`
  - Expressions: "back to policy selection", "policy selection"
- **Intent**: `chooseFCFS`
  - Expressions: "choose first come first serve", "select fcfs", "fcfs"
- **Intent**: `choosePriority`
  - Expressions: "choose priority", "select priority", "priority"
- **Intent**: `chooseRoundRobin`
  - Expressions: "choose round robin", "select round robin", "round robin"
- **Intent**: `chooseSJF`
  - Expressions: "choose shortest job first", "select sjf", "sjf"

### Simulation Control
- **Intent**: `playSimulation`
  - Expressions: "play simulation", "start simulation", "run simulation"
- **Intent**: `pauseSimulation`
  - Expressions: "pause simulation", "pause"
- **Intent**: `stopSimulation`
  - Expressions: "stop simulation", "end simulation"
- **Intent**: `addProcess`
  - Expressions: "add process", "create process", "new process"
- **Intent**: `nextStep`
  - Expressions: "next", "next step", "continue"

6. Train the context (this may take a few minutes)
7. Download the generated `.rhn` file
8. Place the `.rhn` file in your Unity project's `Assets/StreamingAssets/` folder
9. Rename it to `honeyos_context.rhn`

## Step 4: Configure Unity Project
1. Open your Unity project
2. Find the `MicrophoneDemo` script in the scene
3. In the Inspector, set the following:
   - **Access Key**: Paste your Picovoice access key
   - **Context Path**: `honeyos_context.rhn`

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