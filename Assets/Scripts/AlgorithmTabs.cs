using UnityEngine;
using UnityEngine.UI;

public class AlgorithmTabs : MonoBehaviour
{
    [System.Serializable]
    private class TabBackground
    {
        public Image image;
        public Sprite normalSprite;
        public Sprite lowMentalSprite;

        public void CacheNormalSprite()
        {
            if (image != null && normalSprite == null)
            {
                normalSprite = image.sprite;
            }
        }

        public void Apply(bool isLowMental)
        {
            if (image == null)
            {
                return;
            }

            Sprite targetSprite = isLowMental && lowMentalSprite != null ? lowMentalSprite : normalSprite;
            if (targetSprite != null)
            {
                image.sprite = targetSprite;
                image.color = Color.white;
            }
        }
    }

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

    [Header("Low Mental Backgrounds")]
    [SerializeField] private TabBackground homeBackground = new TabBackground();
    [SerializeField] private TabBackground reelsBackground = new TabBackground();
    [SerializeField] private TabBackground profileBackground = new TabBackground();

    private Image rootBackgroundImage;
    private bool isLowMentalState;

    private void Awake()
    {
        rootBackgroundImage = GetComponent<Image>();
        ResolveReferences();
        CacheNormalBackgrounds();
        DisablePanelBackgroundRaycasts();
        BindButtons();
        KeepTabButtonsOnTop();
    }

    private void OnEnable()
    {
        SubscribeMentalEvents();
        RefreshLowMentalState();
        ShowTab(startOnReels ? Tab.Reels : Tab.Home);
        KeepTabButtonsOnTop();
    }

    private void OnDisable()
    {
        UnsubscribeMentalEvents();
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

        AutoBindBackground(ref homeBackground, homePanel);
        AutoBindBackground(ref reelsBackground, reelsPanel);
        AutoBindBackground(ref profileBackground, profilePanel);
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
        RefreshLowMentalBackgrounds();

        if (rootBackgroundImage != null)
        {
            rootBackgroundImage.enabled = homePanel == null && tab == Tab.Home;
            rootBackgroundImage.raycastTarget = false;
        }

        SetActive(homePanel, tab == Tab.Home);
        SetActive(reelsPanel, tab == Tab.Reels);
        SetActive(profilePanel, tab == Tab.Profile);
        KeepTabButtonsOnTop();
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

    private void AutoBindBackground(ref TabBackground background, GameObject panel)
    {
        if (background == null)
        {
            background = new TabBackground();
        }

        if (background.image != null || panel == null)
        {
            return;
        }

        background.image = panel.GetComponent<Image>();
    }

    private void CacheNormalBackgrounds()
    {
        homeBackground?.CacheNormalSprite();
        reelsBackground?.CacheNormalSprite();
        profileBackground?.CacheNormalSprite();
    }

    private void SubscribeMentalEvents()
    {
        if (MentalManager.Instance == null)
        {
            return;
        }

        MentalManager.Instance.OnLowMentalState -= OnLowMentalState;
        MentalManager.Instance.OnLowMentalState += OnLowMentalState;
        MentalManager.Instance.OnMentalChanged -= OnMentalChanged;
        MentalManager.Instance.OnMentalChanged += OnMentalChanged;
    }

    private void UnsubscribeMentalEvents()
    {
        if (MentalManager.Instance != null)
        {
            MentalManager.Instance.OnLowMentalState -= OnLowMentalState;
            MentalManager.Instance.OnMentalChanged -= OnMentalChanged;
        }
    }

    private void RefreshLowMentalState()
    {
        isLowMentalState = MentalManager.Instance != null && MentalManager.Instance.IsLowMentalState;
        RefreshLowMentalBackgrounds();
    }

    private void OnLowMentalState()
    {
        isLowMentalState = true;
        RefreshLowMentalBackgrounds();
    }

    private void OnMentalChanged(int currentMental)
    {
        RefreshLowMentalState();
    }

    private void RefreshLowMentalBackgrounds()
    {
        homeBackground?.Apply(isLowMentalState);
        reelsBackground?.Apply(isLowMentalState);
        profileBackground?.Apply(isLowMentalState);
    }

    private void KeepTabButtonsOnTop()
    {
        KeepButtonVisible(homeButton);
        KeepButtonVisible(reelsButton);
        KeepButtonVisible(profileButton);
    }

    private void KeepButtonVisible(Button button)
    {
        if (button == null)
        {
            return;
        }

        button.gameObject.SetActive(true);
        button.transform.SetAsLastSibling();

        Graphic graphic = button.targetGraphic;
        if (graphic != null)
        {
            graphic.raycastTarget = true;
        }

        Image iconImage = graphic as Image;
        if (iconImage == null)
        {
            iconImage = button.GetComponent<Image>();
        }

        if (iconImage == null)
        {
            return;
        }

        iconImage.sprite = null;
        iconImage.color = new Color(1f, 1f, 1f, 0f);
        iconImage.preserveAspect = true;
        iconImage.enabled = true;
    }
}
