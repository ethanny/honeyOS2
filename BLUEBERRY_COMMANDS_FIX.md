# Blueberry Commands Fix Guide

## The Problem
Your Blueberry (page replacement) voice commands are not working even though voice initialization is successful.

## Quick Diagnostic Steps

### Step 1: Run the Diagnostic Tool
1. In Unity, select the GameObject with the MicrophoneDemo script
2. Right-click on the script in the Inspector
3. Choose **"Diagnose Blueberry Commands"** from the context menu
4. Check the Console for detailed results

### Step 2: Check Component References
The most common issue is missing component references in the Inspector:

1. Select the GameObject with MicrophoneDemo script
2. In the Inspector, look for these fields:
   - **Page Replacement Nav** (should reference PageController3)
   - **Algorithm Manager** (should reference AlgorithmManager)

3. If either field shows "None (PageController3)" or "None (AlgorithmManager)":
   - Drag the appropriate GameObject from the scene hierarchy
   - Or click the circle icon and select the component

## Available Blueberry Voice Commands

### Navigation Commands:
- **"home blueberry"** - Go back to Blueberry Ville
- **"start blueberry"** - Open algorithm selector

### Algorithm Selection:
- **"choose FIFO"** or **"choose fifo"** - Select FIFO algorithm
- **"choose LRU"** or **"choose lru"** - Select LRU algorithm  
- **"choose OPR"** or **"choose opr"** - Select Optimal algorithm
- **"choose MRU"** or **"choose mru"** - Select MRU algorithm
- **"choose LFU"** or **"choose lfu"** - Select LFU algorithm
- **"first in first out"** - Select FIFO algorithm
- **"least recently used"** - Select LRU algorithm
- **"optimal page replacement"** - Select OPR algorithm

### Algorithm Control:
- **"simulate algorithm"** - Start the simulation
- **"run algorithm"** - Start the simulation
- **"pause algorithm"** - Pause the simulation
- **"resume algorithm"** - Resume the simulation
- **"reset algorithm"** - Stop and reset the simulation

### Frame Count Control:
- **"increase frames"** - Add one frame
- **"decrease frames"** - Remove one frame
- **"set frame count 1"** through **"set frame count 7"** - Set specific frame count

## Common Issues and Solutions

### Issue 1: "Component references missing"
**Solution:** Assign the missing components in the Inspector:
- Find PageController3 component in your scene and assign it to "Page Replacement Nav"
- Find AlgorithmManager component in your scene and assign it to "Algorithm Manager"

### Issue 2: "Commands not recognized"
**Solution:** Try these exact phrases:
- Say "start blueberry" first to open the algorithm selector
- Then say "choose FIFO" (or other algorithm names)
- Finally say "simulate algorithm" to run it

### Issue 3: "Voice recognition works but commands do nothing"
**Solution:** 
1. Make sure you're in the correct app/scene context
2. Check that the Blueberry app is open and active
3. Verify component references are properly assigned

### Issue 4: "Some commands work, others don't"
**Solution:**
1. Check the Console for error messages when commands fail
2. Make sure all UI elements (buttons, sliders) are properly assigned in AlgorithmManager
3. Verify that the reference string input field has valid data

## Testing Your Fix

### Manual Test:
1. Open the Blueberry app
2. Say "start blueberry" - should open algorithm selector
3. Say "choose FIFO" - should navigate to simulation page and select FIFO
4. Enter a reference string (e.g., "1 2 3 4 1 2 5")
5. Say "simulate algorithm" - should start the simulation

### Voice Command Flow:
```
"start blueberry" → "choose FIFO" → "simulate algorithm"
```

## Advanced Troubleshooting

If commands still don't work after checking components:

1. **Check Scene Setup:**
   - Ensure PageController3 has proper GameObject references
   - Verify AlgorithmManager has all UI elements assigned

2. **Check Console for Errors:**
   - Look for null reference exceptions
   - Check for missing UI component errors

3. **Test Components Manually:**
   - Try clicking buttons in the UI to ensure they work
   - Verify that manual navigation works before testing voice commands

## Need More Help?

Run the "Diagnose Blueberry Commands" tool - it will tell you exactly what's wrong and provide specific solutions for your setup! 