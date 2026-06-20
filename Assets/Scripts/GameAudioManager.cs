using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameAudioManager : MonoBehaviour
{
    public static GameAudioManager Instance { get; private set; }

    private AudioSource musicSource;
    private AudioSource sfxSource;
    private AudioClip defaultBgm;
    private AudioClip mental75Bgm;
    private AudioClip lowMentalBgm;
    private AudioClip miniGameBgm;
    private AudioClip clickSfx;
    private AudioClip scoreUpSfx;
    private AudioClip noteHitSfx;
    private AudioClip healthDownSfx;
    private AudioClip collisionSfx;
    private AudioClip gunshotSfx;
    private MentalManager subscribedMentalManager;
    private int previousAvoidLives = -1;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateAutomatically()
    {
        if (Instance == null)
        {
            new GameObject("GameAudioManager").AddComponent<GameAudioManager>();
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = 0.35f;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.volume = 0.8f;

        LoadClips();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        RefreshAudioState();
        StartCoroutine(BindButtonsAfterSceneLoad());
    }

    private void Update()
    {
        WatchMiniGame6Lives();
    }

    private void OnDestroy()
    {
        if (Instance != this)
        {
            return;
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;
        UnsubscribeMentalManager();
        Instance = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        previousAvoidLives = -1;
        RefreshAudioState();
        StartCoroutine(BindButtonsAfterSceneLoad());
    }

    private IEnumerator BindButtonsAfterSceneLoad()
    {
        yield return null;
        BindClickSoundsToButtons();
        SubscribeMentalManager();
        RefreshAudioState();
    }

    private void LoadClips()
    {
        defaultBgm = Resources.Load<AudioClip>("Audio/default_bgm");
        mental75Bgm = Resources.Load<AudioClip>("Audio/mental_75_bgm");
        lowMentalBgm = Resources.Load<AudioClip>("Audio/low_mental_bgm");
        miniGameBgm = Resources.Load<AudioClip>("Audio/minigame_bgm");
        clickSfx = Resources.Load<AudioClip>("Audio/click");
        scoreUpSfx = Resources.Load<AudioClip>("Audio/score_up");
        noteHitSfx = Resources.Load<AudioClip>("Audio/note_hit");
        healthDownSfx = Resources.Load<AudioClip>("Audio/health_down");
        collisionSfx = Resources.Load<AudioClip>("Audio/collision");
        gunshotSfx = Resources.Load<AudioClip>("Audio/gunshot");
    }

    private void SubscribeMentalManager()
    {
        if (subscribedMentalManager == MentalManager.Instance)
        {
            return;
        }

        UnsubscribeMentalManager();
        subscribedMentalManager = MentalManager.Instance;
        if (subscribedMentalManager != null)
        {
            subscribedMentalManager.OnMentalChanged += OnMentalChanged;
        }
    }

    private void UnsubscribeMentalManager()
    {
        if (subscribedMentalManager != null)
        {
            subscribedMentalManager.OnMentalChanged -= OnMentalChanged;
            subscribedMentalManager = null;
        }
    }

    private void OnMentalChanged(int currentMental)
    {
        RefreshAudioState();
    }

    private void RefreshAudioState()
    {
        AudioClip desiredClip = GetDesiredMusic();
        if (desiredClip == null || musicSource.clip == desiredClip)
        {
            return;
        }

        musicSource.Stop();
        musicSource.clip = desiredClip;
        musicSource.Play();
    }

    private AudioClip GetDesiredMusic()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (!string.IsNullOrEmpty(sceneName) && sceneName.StartsWith("MiniGame"))
        {
            return miniGameBgm;
        }

        int mental = MentalManager.Instance != null ? MentalManager.Instance.CurrentMental : 100;
        if (mental <= 50)
        {
            return lowMentalBgm;
        }

        return mental <= 75 ? mental75Bgm : defaultBgm;
    }

    private void BindClickSoundsToButtons()
    {
        Button[] buttons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Button button in buttons)
        {
            if (button != null && button.GetComponent<UIButtonClickSound>() == null)
            {
                button.gameObject.AddComponent<UIButtonClickSound>();
            }
        }
    }

    private void WatchMiniGame6Lives()
    {
        if (SceneManager.GetActiveScene().name != "MiniGame6" || AvoidGameManager.Instance == null)
        {
            previousAvoidLives = -1;
            return;
        }

        int activeLives = 0;
        Image[] hearts = AvoidGameManager.Instance.heartImages;
        if (hearts != null)
        {
            foreach (Image heart in hearts)
            {
                if (heart != null && heart.gameObject.activeSelf)
                {
                    activeLives++;
                }
            }
        }

        if (previousAvoidLives >= 0 && activeLives < previousAvoidLives)
        {
            PlayCollision();
            PlayHealthDown();
        }

        previousAvoidLives = activeLives;
    }

    private void PlaySfx(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public static void PlayClick() => Instance?.PlaySfx(Instance.clickSfx);
    public static void PlayScoreUp() => Instance?.PlaySfx(Instance.scoreUpSfx);
    public static void PlayNoteHit() => Instance?.PlaySfx(Instance.noteHitSfx);
    public static void PlayHealthDown() => Instance?.PlaySfx(Instance.healthDownSfx);
    public static void PlayCollision() => Instance?.PlaySfx(Instance.collisionSfx);
    public static void PlayGunshot() => Instance?.PlaySfx(Instance.gunshotSfx);
}

public class UIButtonClickSound : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        if (GetComponent<ShooterItem>() == null)
        {
            GameAudioManager.PlayClick();
        }
    }
}
