using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Global mental gauge manager.
/// Other scripts can call MentalManager.Instance.DecreaseMental(amount).
/// </summary>
public class MentalManager : MonoBehaviour
{
    public static MentalManager Instance { get; private set; }

    public event Action<int> OnMentalChanged;
    public event Action OnLowMentalState;

    [Header("Mental Settings")]
    [SerializeField] private int maxMental = 100;
    [SerializeField] private int currentMental = 100;
    [SerializeField] private int lowMentalThreshold = 50;

    [Header("UI")]
    [SerializeField] private Text mentalText;
    [SerializeField] private TMP_Text mentalTMPText;
    [SerializeField] private Slider mentalGauge;

    [Header("Time Drain")]
    [SerializeField] private bool decreaseDuringMiniGameScenes = true;
    [SerializeField] private int mentalDecreasePerMinute = 1;
    [SerializeField] private string miniGameSceneNamePrefix = "MiniGame";

    private bool isLowMentalState;
    private float minuteTimer;

    public int CurrentMental => currentMental;
    public int MaxMental => maxMental;
    public bool IsLowMentalState => isLowMentalState;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        currentMental = Mathf.Clamp(currentMental, 0, maxMental);
        isLowMentalState = currentMental <= lowMentalThreshold;
        SceneManager.sceneLoaded += OnSceneLoaded;
        UpdateMentalUI();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void Update()
    {
        // Time drain: mini-game scenes cost mental every minute.
        if (!decreaseDuringMiniGameScenes || !IsMiniGameScene())
        {
            minuteTimer = 0f;
            return;
        }

        minuteTimer += Time.deltaTime;
        while (minuteTimer >= 60f)
        {
            minuteTimer -= 60f;
            DecreaseMental(mentalDecreasePerMinute);
        }
    }

    public void DecreaseMental(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        SetMental(currentMental - amount);
    }

    public void ResetMental()
    {
        SetMental(maxMental);
    }

    public void SetMental(int value)
    {
        int previous = currentMental;
        currentMental = Mathf.Clamp(value, 0, maxMental);

        if (previous != currentMental)
        {
            UpdateMentalUI();
            OnMentalChanged?.Invoke(currentMental);
        }

        if (currentMental > lowMentalThreshold)
        {
            isLowMentalState = false;
        }

        if (!isLowMentalState && currentMental <= lowMentalThreshold)
        {
            isLowMentalState = true;
            OnLowMentalState?.Invoke();
        }
    }

    public void BindUI(Text text, Slider gauge = null)
    {
        mentalText = text;
        mentalGauge = gauge;
        UpdateMentalUI();
    }

    public void BindUI(TMP_Text text, Slider gauge = null)
    {
        mentalTMPText = text;
        mentalGauge = gauge;
        UpdateMentalUI();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        minuteTimer = 0f;
    }

    private bool IsMiniGameScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        return !string.IsNullOrEmpty(sceneName) && sceneName.StartsWith(miniGameSceneNamePrefix);
    }

    private void UpdateMentalUI()
    {
        if (mentalText != null)
        {
            mentalText.text = $"Mental {currentMental}/{maxMental}";
        }

        if (mentalTMPText != null)
        {
            mentalTMPText.text = $"Mental {currentMental}/{maxMental}";
        }

        if (mentalGauge != null)
        {
            mentalGauge.maxValue = maxMental;
            mentalGauge.value = currentMental;
        }
    }
}
