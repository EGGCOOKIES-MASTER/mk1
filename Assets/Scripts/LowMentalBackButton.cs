using System.Collections;
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
    [SerializeField] private float navigationCooldown = 0.5f;

    private bool isNavigating;

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
        if (isNavigating || GameManager.Instance == null)
        {
            return;
        }

        isNavigating = true;
        backButton.interactable = false;

        UIManager uiManager = FindMainSceneUIManager();
        GameManager.GameState targetState = GameManager.Instance.CurrentState == GameManager.GameState.Login
            ? GameManager.GameState.AppClick
            : GameManager.GameState.Login;

        if (targetState == GameManager.GameState.AppClick)
        {
            HideButton();
        }

        ApplyTargetState(uiManager, targetState);
        StartCoroutine(CompleteNavigation(uiManager, targetState));
    }

    private UIManager FindMainSceneUIManager()
    {
        UIManager[] managers = FindObjectsByType<UIManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < managers.Length; i++)
        {
            UIManager manager = managers[i];
            if (manager != null && manager.gameObject.scene.name == "MainScene")
            {
                return manager;
            }
        }

        return null;
    }

    private void ApplyTargetState(UIManager uiManager, GameManager.GameState targetState)
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        if (uiManager != null)
        {
            GameManager.Instance.SetStateWithoutUI(targetState);
            if (targetState == GameManager.GameState.Login)
            {
                uiManager.ShowLoginFromBackButton();
            }
            else
            {
                uiManager.ShowScreen(targetState);
            }

            transform.SetAsLastSibling();
            return;
        }

        GameManager.Instance.SetStateWithoutUI(targetState);
        UIManager.ShowScreenDirectly(targetState);
        transform.SetAsLastSibling();
    }

    private IEnumerator CompleteNavigation(UIManager uiManager, GameManager.GameState targetState)
    {
        yield return new WaitForEndOfFrame();

        bool wrongGameState = GameManager.Instance != null && GameManager.Instance.CurrentState != targetState;
        bool wrongVisibleState = !UIManager.IsScreenVisibleDirectly(targetState);
        if (wrongGameState || wrongVisibleState)
        {
            ApplyTargetState(uiManager, targetState);
        }

        yield return new WaitForSecondsRealtime(navigationCooldown);
        isNavigating = false;

        if (backButton != null)
        {
            backButton.interactable = true;
        }

        RefreshVisibility();
    }
}
