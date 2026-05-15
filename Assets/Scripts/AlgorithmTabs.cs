using UnityEngine;
using UnityEngine.UI;

public class AlgorithmTabs : MonoBehaviour
{
    private enum Tab
    {
        Home,
        Reels,
        Profile
    }

    [Header("Tab Buttons")]
    [SerializeField] private Button homeButton;
    [SerializeField] private Button reelsButton;
    [SerializeField] private Button profileButton;

    [Header("Tab Panels")]
    [SerializeField] private GameObject homePanel;
    [SerializeField] private GameObject reelsPanel;
    [SerializeField] private GameObject profilePanel;

    [Header("Default")]
    [SerializeField] private bool startOnReels = true;

    private Image rootBackgroundImage;

    private void Awake()
    {
        rootBackgroundImage = GetComponent<Image>();
        ResolveReferences();
        DisablePanelBackgroundRaycasts();
        BindButtons();
    }

    private void OnEnable()
    {
        ShowTab(startOnReels ? Tab.Reels : Tab.Home);
    }

    private void ResolveReferences()
    {
        if (homeButton == null)
        {
            homeButton = FindButton("HOME");
        }

        if (reelsButton == null)
        {
            reelsButton = FindButton("Reels");
        }

        if (profileButton == null)
        {
            profileButton = FindButton("Profile");
        }

        if (reelsPanel == null)
        {
            Transform found = transform.Find("AlgorithmPostArea");
            if (found != null)
            {
                reelsPanel = found.gameObject;
            }
        }
    }

    private Button FindButton(string childName)
    {
        Transform child = transform.Find(childName);
        return child != null ? child.GetComponent<Button>() : null;
    }

    private void BindButtons()
    {
        if (homeButton != null)
        {
            homeButton.onClick.RemoveAllListeners();
            homeButton.onClick.AddListener(() => ShowTab(Tab.Home));
        }

        if (reelsButton != null)
        {
            reelsButton.onClick.RemoveAllListeners();
            reelsButton.onClick.AddListener(() => ShowTab(Tab.Reels));
        }

        if (profileButton != null)
        {
            profileButton.onClick.RemoveAllListeners();
            profileButton.onClick.AddListener(() => ShowTab(Tab.Profile));
        }
    }

    private void ShowTab(Tab tab)
    {
        if (rootBackgroundImage != null)
        {
            rootBackgroundImage.enabled = homePanel == null && tab == Tab.Home;
            rootBackgroundImage.raycastTarget = false;
        }

        SetActive(homePanel, tab == Tab.Home);
        SetActive(reelsPanel, tab == Tab.Reels);
        SetActive(profilePanel, tab == Tab.Profile);
    }

    private void DisablePanelBackgroundRaycasts()
    {
        DisableBackgroundRaycast(homePanel);
        DisableBackgroundRaycast(reelsPanel);
        DisableBackgroundRaycast(profilePanel);
    }

    private void DisableBackgroundRaycast(GameObject panel)
    {
        if (panel == null)
        {
            return;
        }

        Image panelImage = panel.GetComponent<Image>();
        if (panelImage != null)
        {
            panelImage.raycastTarget = false;
        }
    }

    private void SetActive(GameObject target, bool isActive)
    {
        if (target != null && target.activeSelf != isActive)
        {
            target.SetActive(isActive);
        }
    }
}
