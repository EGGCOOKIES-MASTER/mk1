using UnityEngine;
using UnityEngine.Video;

public class ReelsController : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public GameObject reelsUI; // 영상 보여줄 UI

    public void PlayReels()
    {
        reelsUI.SetActive(true);
        videoPlayer.Play();
    }
}