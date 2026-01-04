using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class IntroManager : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    private string videoPath = "Video/Intro.mp4";

    void Start()
    {
        string fullPath = System.IO.Path.Combine(Application.streamingAssetsPath, videoPath);
        videoPlayer.url = fullPath;
        videoPlayer.loopPointReached += OnVideoEnd;
        videoPlayer.Play();
    }
    void OnVideoEnd(VideoPlayer vp)
    {
        SceneManager.LoadScene("GameScene");
    }
}
