using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class SceneSwitch : MonoBehaviour
{
    // Reference to the VideoPlayer component
    public VideoPlayer videoPlayer;

    // Name or index of the scene you want to load after the video finishes
    public string nextSceneName;

    void Start()
    {
        // Make sure the video player is not null
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }

        // Attach the event handler to the video player
        videoPlayer.loopPointReached += OnVideoFinished;
    }

    // Event handler called when the video finishes playing
    void OnVideoFinished(VideoPlayer vp)
    {
        // Load the next scene
        SceneManager.LoadScene(nextSceneName);
    }

    // Clean up the event listener when the script is disabled or destroyed
    private void OnDisable()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }
}
