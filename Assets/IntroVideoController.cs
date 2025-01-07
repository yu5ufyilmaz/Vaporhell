using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class IntroVideoController : MonoBehaviour
{
    [Header("Video Settings")]
    public VideoPlayer videoPlayer; // VideoPlayer komponentini atayın
    public string mainGameSceneName = "mete"; // Ana oyun sahnesinin ismi

    [Header("UI Settings")]
    public GameObject skipUI; // "Space ile Atlama" UI'sını atayın

    [Header("Input Settings")]
    public InputActionReference skipVideoAction; // SkipVideo action referansı

    private bool skipRequested = false; // Oyuncunun videoyu atlayıp atlamadığını takip eder

    void OnEnable()
    {
        // SkipVideo Action'ını etkinleştir ve olay dinleyicisi ekle
        if (skipVideoAction != null)
        {
            skipVideoAction.action.Enable();
            skipVideoAction.action.performed += OnSkipVideo;
        }

        // VideoPlayer'ın bitiş olayını dinle
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoFinished;
        }
    }

    void OnDisable()
    {
        // SkipVideo Action'ından olay dinleyicisini kaldır ve devre dışı bırak
        if (skipVideoAction != null)
        {
            skipVideoAction.action.performed -= OnSkipVideo;
            skipVideoAction.action.Disable();
        }

        // VideoPlayer'ın bitiş olayını kaldır
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }

    void Start()
    {
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }

        // "Space ile Atlama" UI'sını etkinleştir
        if (skipUI != null)
        {
            skipUI.SetActive(true);
        }

        // Videoyu oynat
        videoPlayer.Play();
    }

    void Update()
    {
        // Video oynatılırken herhangi bir işlem yapılmaz
    }

    void OnSkipVideo(InputAction.CallbackContext context)
    {
        // Oyuncu videoyu atlamak isterse
        SkipVideo();
    }

    void SkipVideo()
    {
        if (!skipRequested && videoPlayer.isPlaying)
        {
            skipRequested = true; // Atlandığını işaretle
            videoPlayer.Stop(); // Videoyu durdur
            LoadMainGame(); // Ana sahneye geçiş yap
        }
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        // Video tamamen bittiyse oyuna geç
        if (!skipRequested)
        {
            LoadMainGame();
        }
    }

    void LoadMainGame()
    {
        // Ana oyun sahnesini yükle
        SceneManager.LoadScene(mainGameSceneName);
    }
}
