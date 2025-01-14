using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class TimelineEndHandler : MonoBehaviour
{
    public PlayableDirector playableDirector;

    void Start()
    {
        if (playableDirector != null)
        {
            playableDirector.stopped += OnTimelineStopped;
        }
    }

    void OnTimelineStopped(PlayableDirector director)
    {
        // Eğer doğru Timeline bittiyse:
        if (director == playableDirector)
        {
            SceneManager.LoadScene("MainMenu"); // Ana menü sahnenizin adı
        }
    }

    void OnDestroy()
    {
        if (playableDirector != null)
        {
            playableDirector.stopped -= OnTimelineStopped;
        }
    }
}