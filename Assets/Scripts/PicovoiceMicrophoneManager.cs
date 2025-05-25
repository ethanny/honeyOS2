using UnityEngine;
using System.Collections;

namespace HoneyOS.VoiceControl
{
    /// <summary>
    /// Simple microphone manager for Picovoice integration
    /// Handles basic microphone functionality that was previously in MicrophoneRecord
    /// </summary>
    public class PicovoiceMicrophoneManager : MonoBehaviour
    {
        [Header("Microphone Settings")]
        public bool autoStartMicrophone = true;
        public int sampleRate = 16000;
        public int maxRecordingLength = 10; // seconds
        
        [Header("Voice Activity Detection")]
        public bool enableVAD = true;
        public float vadThreshold = 0.01f;
        public float vadSilenceDuration = 1.0f;
        
        private AudioSource audioSource;
        private string microphoneDevice;
        private bool isRecording = false;
        private bool isVoiceDetected = false;
        private float lastVoiceTime;
        private float[] audioBuffer;
        
        public bool IsRecording => isRecording;
        public bool IsVoiceDetected => isVoiceDetected;
        
        // Events
        public System.Action OnRecordingStarted;
        public System.Action OnRecordingStopped;
        public System.Action<bool> OnVoiceActivityChanged;
        
        private void Start()
        {
            InitializeMicrophone();
        }
        
        private void InitializeMicrophone()
        {
            // Get the default microphone device
            if (Microphone.devices.Length > 0)
            {
                microphoneDevice = Microphone.devices[0];
                UnityEngine.Debug.Log($"Using microphone: {microphoneDevice}");
            }
            else
            {
                UnityEngine.Debug.LogError("No microphone devices found!");
                return;
            }
            
            // Setup audio source
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            audioSource.loop = true;
            audioSource.mute = true; // We don't want to hear the microphone input
            
            if (autoStartMicrophone)
            {
                StartMicrophoneCapture();
            }
        }
        
        public void StartMicrophoneCapture()
        {
            if (string.IsNullOrEmpty(microphoneDevice))
            {
                UnityEngine.Debug.LogError("No microphone device available!");
                return;
            }
            
            try
            {
                audioSource.clip = Microphone.Start(microphoneDevice, true, maxRecordingLength, sampleRate);
                
                // Wait for microphone to start
                while (!(Microphone.GetPosition(microphoneDevice) > 0)) { }
                
                audioSource.Play();
                isRecording = true;
                
                OnRecordingStarted?.Invoke();
                UnityEngine.Debug.Log("Microphone capture started");
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to start microphone: {ex.Message}");
            }
        }
        
        public void StopMicrophoneCapture()
        {
            if (isRecording)
            {
                Microphone.End(microphoneDevice);
                audioSource.Stop();
                isRecording = false;
                
                OnRecordingStopped?.Invoke();
                UnityEngine.Debug.Log("Microphone capture stopped");
            }
        }
        
        private void Update()
        {
            if (isRecording && enableVAD)
            {
                UpdateVoiceActivityDetection();
            }
        }
        
        private void UpdateVoiceActivityDetection()
        {
            if (audioSource.clip == null) return;
            
            // Get current audio data
            int sampleWindow = 1024;
            audioBuffer = new float[sampleWindow];
            
            int micPosition = Microphone.GetPosition(microphoneDevice);
            if (micPosition < sampleWindow) return;
            
            audioSource.clip.GetData(audioBuffer, micPosition - sampleWindow);
            
            // Calculate RMS (Root Mean Square) for volume detection
            float rms = 0f;
            for (int i = 0; i < audioBuffer.Length; i++)
            {
                rms += audioBuffer[i] * audioBuffer[i];
            }
            rms = Mathf.Sqrt(rms / audioBuffer.Length);
            
            // Check if voice is detected
            bool voiceDetected = rms > vadThreshold;
            
            if (voiceDetected)
            {
                lastVoiceTime = Time.time;
            }
            
            // Update voice detection status
            bool previousVoiceDetected = isVoiceDetected;
            isVoiceDetected = voiceDetected || (Time.time - lastVoiceTime < vadSilenceDuration);
            
            // Trigger event if status changed
            if (previousVoiceDetected != isVoiceDetected)
            {
                OnVoiceActivityChanged?.Invoke(isVoiceDetected);
            }
        }
        
        public float GetCurrentVolume()
        {
            if (!isRecording || audioSource.clip == null) return 0f;
            
            int sampleWindow = 128;
            float[] samples = new float[sampleWindow];
            
            int micPosition = Microphone.GetPosition(microphoneDevice);
            if (micPosition < sampleWindow) return 0f;
            
            audioSource.clip.GetData(samples, micPosition - sampleWindow);
            
            float sum = 0f;
            for (int i = 0; i < samples.Length; i++)
            {
                sum += Mathf.Abs(samples[i]);
            }
            
            return sum / samples.Length;
        }
        
        private void OnDestroy()
        {
            StopMicrophoneCapture();
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                StopMicrophoneCapture();
            }
            else if (autoStartMicrophone)
            {
                StartMicrophoneCapture();
            }
        }
    }
} 