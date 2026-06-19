using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shows a persistent back button after low mental state starts.
/// Put the button under Canvas, not inside a screen panel, if it should stay visible across screens.
/// </summary>
public class LowMentalBackButton : MonoBehaviour
{
    [SerializeField] private Button backButton;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private bool hideUntilLowMental = true;

    private void Awake()
    {
        if (backButton == null)
        {
            backButton = GetComponent<Button>();
        }

        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        BindButton();
        SubscribeMentalEvents();

        RefreshVisibility();
    }

    private void OnEnable()
    {
        BindButton();
        SubscribeMentalEvents();

        RefreshVisibility();
    }

    private void OnDisable()
    {
        UnsubscribeMentalEvents();
    }

    private void Update()
    {
        RefreshVisibility();
    }

    private void RefreshVisibility()
    {
        if (ShouldShowButton())
        {
            ShowButton();
        }
        else
        {
            HideButton();
        }
    }

    private void BindButton()
    {
        if (backButton == null)
        {
            return;
        }

        backButton.onClick.RemoveListener(GoBackOneScreen);
        backButton.onClick.RemoveAllListeners();
        backButton.onClick.AddListener(GoBackOneScreen);
    }

    private void SubscribeMentalEvents()
    {
        if (MentalManager.Instance != null)
        {
            MentalManager.Instance.OnLowMentalState -= RefreshVisibility;
            MentalManager.Instance.OnLowMentalState += RefreshVisibility;
        }
    }

    private void UnsubscribeMentalEvents()
    {
        if (MentalManager.Instance != null)
        {
            MentalManager.Instance.OnLowMentalState -= RefreshVisibility;
        }
    }

    private void ShowButton()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }

    private bool ShouldShowButton()
    {
        if (MentalManager.Instance == null || !MentalManager.Instance.IsLowMentalState)
        {
            return !hideUntilLowMental;
        }

        return GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.AppClick;
    }

    private void HideButton()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }

    private void GoBackOneScreen()
    {
        ShowButton();

        if (GameManager.Instance == null)
        {
            return;
        }

        switch (GameManager.Instance.CurrentState)
        {
            case GameManager.GameState.Algorithm:
                GameManager.Instance.ChangeState(GameManager.GameState.Login);
                break;

            case GameManager.GameState.Login:
                GameManager.Instance.ChangeState(GameManager.GameState.AppClick);
                HideButton();
                break;

            case GameManager.GameState.AppClick:
                HideButton();
                break;

            default:
                GameManager.Instance.ChangeState(GameManager.GameState.AppClick);
                HideButton();
                break;
        }
    }
}
