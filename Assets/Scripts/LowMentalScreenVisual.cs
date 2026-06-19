using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Applies a low-mental visual mood to any UI screen.
/// Attach this to AppClickScreen, LoginScreen, or other screen root objects.
/// </summary>
public class LowMentalScreenVisual : MonoBehaviour
{
    [Header("Background")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Sprite normalBackground;
    [SerializeField] private Sprite lowMentalBackground;

    [Header("Tint")]
    [SerializeField] private bool autoCollectChildGraphics = true;
    [SerializeField] private Graphic[] tintTargets;
    [SerializeField] private Color lowMentalColor = new Color(0.35f, 0.18f, 0.23f, 1f);
    [Range(0f, 1f)]
    [SerializeField] private float tintBlendAmount = 0.45f;

    [Header("Objects")]
    [SerializeField] private GameObject[] showOnLowMental;
    [SerializeField] private GameObject[] hideOnLowMental;

    private readonly List<Graphic> collectedGraphics = new List<Graphic>();
    private readonly List<Color> originalGraphicColors = new List<Color>();
    private bool isApplied;

    private void Awake()
    {
        CacheGraphics();
    }

    private void OnEnable()
    {
        SubscribeMentalEvents();

        if (MentalManager.Instance != null && MentalManager.Instance.IsLowMentalState)
        {
            ApplyLowMentalState();
        }
    }

    private void OnDisable()
    {
        UnsubscribeMentalEvents();
    }

    private void CacheGraphics()
    {
        collectedGraphics.Clear();
        originalGraphicColors.Clear();

        if (autoCollectChildGraphics)
        {
            Graphic[] children = GetComponentsInChildren<Graphic>(true);
            for (int i = 0; i < children.Length; i++)
            {
                AddGraphic(children[i]);
            }
        }

        if (tintTargets != null)
        {
            for (int i = 0; i < tintTargets.Length; i++)
            {
                AddGraphic(tintTargets[i]);
            }
        }
    }

    private void AddGraphic(Graphic graphic)
    {
        if (graphic == null || collectedGraphics.Contains(graphic))
        {
            return;
        }

        collectedGraphics.Add(graphic);
        originalGraphicColors.Add(graphic.color);
    }

    private void SubscribeMentalEvents()
    {
        if (MentalManager.Instance != null)
        {
            MentalManager.Instance.OnLowMentalState -= ApplyLowMentalState;
            MentalManager.Instance.OnLowMentalState += ApplyLowMentalState;
        }
    }

    private void UnsubscribeMentalEvents()
    {
        if (MentalManager.Instance != null)
        {
            MentalManager.Instance.OnLowMentalState -= ApplyLowMentalState;
        }
    }

    public void ApplyLowMentalState()
    {
        if (isApplied)
        {
            return;
        }

        isApplied = true;

        if (backgroundImage != null && lowMentalBackground != null)
        {
            if (normalBackground == null)
            {
                normalBackground = backgroundImage.sprite;
            }

            backgroundImage.sprite = lowMentalBackground;
        }

        for (int i = 0; i < collectedGraphics.Count; i++)
        {
            if (collectedGraphics[i] == null)
            {
                continue;
            }

            Color originalColor = originalGraphicColors[i];
            Color lowColor = Color.Lerp(originalColor, lowMentalColor, tintBlendAmount);
            lowColor.a = originalColor.a;
            collectedGraphics[i].color = lowColor;
        }

        SetObjectsActive(showOnLowMental, true);
        SetObjectsActive(hideOnLowMental, false);
    }

    private void SetObjectsActive(GameObject[] targets, bool isActive)
    {
        if (targets == null)
        {
            return;
        }

        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i] != null)
            {
                targets[i].SetActive(isActive);
            }
        }
    }
}
