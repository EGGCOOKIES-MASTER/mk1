using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    // UI Panels
    public GameObject homeUI;
    public GameObject snsUI;
    public GameObject miniGameUI;
    public GameObject endingUI;

    // Battery System
    public Text batteryText;
    public float batteryLevel = 100f;
    public float batteryDrainRate = 1f; // per second

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        ShowHomeUI();
        UpdateBatteryUI();
    }

    private void Update()
    {
        if (snsUI.activeSelf)
        {
            DrainBattery();
        }
    }

    public void ShowHomeUI()
    {
        homeUI.SetActive(true);
        snsUI.SetActive(false);
        miniGameUI.SetActive(false);
        endingUI.SetActive(false);
    }

    public void ShowSNSUI()
    {
        homeUI.SetActive(false);
        snsUI.SetActive(true);
        miniGameUI.SetActive(false);
        endingUI.SetActive(false);
    }

    public void ShowMiniGameUI()
    {
        homeUI.SetActive(false);
        snsUI.SetActive(false);
        miniGameUI.SetActive(true);
        endingUI.SetActive(false);
    }

    public void ShowEndingUI()
    {
        homeUI.SetActive(false);
        snsUI.SetActive(false);
        miniGameUI.SetActive(false);
        endingUI.SetActive(true);
    }

    private void DrainBattery()
    {
        batteryLevel -= batteryDrainRate * Time.deltaTime;
        if (batteryLevel <= 0)
        {
            batteryLevel = 0;
            ShowEndingUI();
        }
        UpdateBatteryUI();
    }

    private void UpdateBatteryUI()
    {
        if (batteryText != null)
        {
            batteryText.text = Mathf.CeilToInt(batteryLevel) + "%";
        }
    }

    public void StartGame()
    {
        ShowSNSUI();
    }

    public void ReturnToSNS()
    {
        ShowSNSUI();
    }
}
