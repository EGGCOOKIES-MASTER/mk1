using UnityEngine;
using UnityEngine.UI;

/// ============================================================
/// 【 AlgorithmScreen - 테마 선택 화면 】
/// 
/// 역할:
/// - 4가지 테마 중 하나를 선택하는 화면
/// - 테마 선택 후 게임 본편(릴스)으로 진입
/// - 배터리를 100%로 초기화
/// 
/// 특징:
/// - 4개의 테마 버튼 (일상, 게임, 엔터테인먼트, 교육)
/// - 어느 테마를 선택해도 게임 로직은 같음
/// - 나중에 테마별 콘텐츠 추가 가능 (확장성)
/// 
/// 흐름:
/// [로그인 완료]
///   ↓
/// AlgorithmScreen 표시 (이 화면)
///   ↓
/// 사용자가 테마 선택
///   ↓
/// OnThemeButtonClicked() 호출
///   ↓
/// GameManager.OnThemeSelected()
///   ↓
/// ReelsScreen 표시 (게임 본편)
/// ============================================================

public class AlgorithmScreen : MonoBehaviour
{
    /// ===== 【 UI 요소 참조 】 =====
    /// themeButtons: 4개의 테마 버튼 배열
    /// - [0] = 📱 일상
    /// - [1] = 🎮 게임
    /// - [2] = 🎬 엔터테인먼트
    /// - [3] = 📚 교육
    [SerializeField] private Button[] themeButtons;      // 테마 버튼들 (배열 4개)
    [SerializeField] private Text titleText;             // 화면 제목

    // ===== [TEMP] 하단 3버튼 미리보기 이미지 전환 =====
    [SerializeField] private Image previewImage;
    [SerializeField] private Button previewButton;
    [SerializeField] private Button bottomLeftButton;
    [SerializeField] private Button bottomCenterButton;
    [SerializeField] private Button bottomRightButton;
    [SerializeField] private Sprite leftPreviewSprite;    // KakaoTalk_20260426_221316735_04
    [SerializeField] private Sprite centerPreviewSprite;  // 비우면 초기 previewImage 사용
    [SerializeField] private Sprite rightPreviewSprite;   // KakaoTalk_20260426_221316735_01
    [SerializeField] private GameObject reelsEnterButtonObject; // 중앙 릴스 진입 버튼(없으면 themeButtons[0] 사용)
    [SerializeField] private GameObject algorithmPostArea;
    [SerializeField] private GameObject refreshButtonObject;

    private Sprite cachedCenterPreviewSprite;
    private bool isReelsTabActive;

    /// ===== 【 테마 이름 정의 】 =====
    /// themeNames: 각 버튼에 표시될 테마 이름
    /// themeButtons와 순서가 일치해야 함
    private string[] themeNames = { "📱 일상", "🎮 게임", "🎬 엔터테인먼트", "📚 교육" };

    /// ===== 【 선택된 테마 저장 】 =====
    /// selectedTheme: 사용자가 선택한 테마의 인덱스
    /// -1 = 아직 선택하지 않음
    /// 0~3 = 선택된 테마 인덱스
    private int selectedTheme = -1;

    // ============================================================
    /// 【 Initialize() - 화면 초기화 】
    /// 
    /// 호출 시점: UIManager.ShowScreen(GameState.Algorithm)에서 호출
    /// 
    /// 역할:
    /// 1. 화면 제목 설정
    /// 2. 4개의 테마 버튼 초기화
    /// 3. 각 버튼에 클릭 이벤트 등록
    /// 4. 선택된 테마를 -1로 리셋
    /// ============================================================
    public void Initialize()
    {
        Debug.Log("🎨 알고리즘 화면 (테마 선택) 초기화됨");

        ResolveTabObjects();

        // [TEMP] 하단 3버튼 이미지 전환 리스너 설정
        SetupBottomPreviewButtons();
        SetupPreviewButton();

        // 초기 진입 시에는 릴스 탭 상태(중앙)를 기본으로 적용
        ShowReelsTab();

        /// 화면 제목 설정
        if (titleText != null)
        {
            titleText.text = "어떤 테마로 시작할까요?";
        }

        /// 테마 버튼 초기화
        if (themeButtons != null && themeButtons.Length > 0)
        {
            for (int i = 0; i < themeButtons.Length; i++)
            {
                /// 클로저 문제 해결을 위해 로컬 변수에 저장
                /// → foreach에서는 i가 변하므로, 명시적으로 복사
                int themeIndex = i;
                
                if (themeButtons[i] != null)
                {
                    /// 버튼 텍스트 설정
                    /// GetComponentInChildren<Text>()로 버튼 내 Text 찾기
                    Text buttonText = themeButtons[i].GetComponentInChildren<Text>();
                    if (buttonText != null && i < themeNames.Length)
                    {
                        buttonText.text = themeNames[i];  // 테마 이름 표시
                    }

                    /// 기존 리스너 제거 (중복 등록 방지)
                    themeButtons[i].onClick.RemoveAllListeners();
                    
                    /// 클릭 이벤트 등록
                    /// → 이 버튼을 클릭하면 OnThemeButtonClicked(themeIndex) 호출
                    themeButtons[i].onClick.AddListener(() => OnThemeButtonClicked(themeIndex));
                }
            }
        }
    }

    private void SetupBottomPreviewButtons()
    {
        ResolveBottomButtons();

        if (previewImage != null)
        {
            cachedCenterPreviewSprite = centerPreviewSprite != null ? centerPreviewSprite : previewImage.sprite;
        }

        if (bottomLeftButton != null)
        {
            bottomLeftButton.onClick.RemoveAllListeners();
            bottomLeftButton.onClick.AddListener(OnBottomLeftButtonClicked);
        }

        if (bottomCenterButton != null)
        {
            bottomCenterButton.onClick.RemoveAllListeners();
            bottomCenterButton.onClick.AddListener(OnBottomCenterButtonClicked);
        }

        if (bottomRightButton != null)
        {
            bottomRightButton.onClick.RemoveAllListeners();
            bottomRightButton.onClick.AddListener(OnBottomRightButtonClicked);
        }
    }

    private void ResolveBottomButtons()
    {
        if (bottomLeftButton == null)
        {
            bottomLeftButton = FindChildButton("HOME");
        }

        if (bottomCenterButton == null)
        {
            bottomCenterButton = FindChildButton("Reels");
        }

        if (bottomRightButton == null)
        {
            bottomRightButton = FindChildButton("Profile");
        }
    }

    private Button FindChildButton(string childName)
    {
        Transform child = transform.Find(childName);
        if (child == null)
        {
            return null;
        }

        return child.GetComponent<Button>();
    }

    private void SetupPreviewButton()
    {
        Button targetButton = previewButton;

        if (targetButton == null && previewImage != null)
        {
            targetButton = previewImage.GetComponent<Button>();
        }

        if (targetButton != null)
        {
            targetButton.onClick.RemoveAllListeners();
            targetButton.onClick.AddListener(OnPreviewImageClicked);
            previewButton = targetButton;
        }
    }

    public void OnPreviewImageClicked()
    {
        Debug.Log("🖼️ 알고리즘 화면 이미지 클릭 - 미니게임으로 이동");
        GameManager.Instance.OnMiniGameStart();
    }

    public void OnBottomLeftButtonClicked()
    {
        SetPreviewVisible(true);
        ApplyPreviewSprite(leftPreviewSprite, "LEFT");
        HideReelsTab();
    }

    public void OnBottomCenterButtonClicked()
    {
        SetPreviewVisible(false);
        ShowReelsTab();
    }

    public void OnBottomRightButtonClicked()
    {
        SetPreviewVisible(true);
        ApplyPreviewSprite(rightPreviewSprite, "RIGHT");
        HideReelsTab();
    }

    private void ResolveTabObjects()
    {
        if (previewImage == null)
        {
            Transform foundPreviewImage = transform.Find("Image");
            if (foundPreviewImage != null)
            {
                previewImage = foundPreviewImage.GetComponent<Image>();
            }
        }

        if (algorithmPostArea == null)
        {
            Transform foundPostArea = transform.Find("AlgorithmPostArea");
            if (foundPostArea != null)
            {
                algorithmPostArea = foundPostArea.gameObject;
            }
        }

        if (refreshButtonObject == null)
        {
            Transform foundRefreshButton = transform.Find("RefreshButton");
            if (foundRefreshButton != null)
            {
                refreshButtonObject = foundRefreshButton.gameObject;
            }
        }
    }

    private void ShowReelsTab()
    {
        SetPreviewVisible(false);
        SetReelsTabVisible(true);
    }

    private void HideReelsTab()
    {
        SetReelsTabVisible(false);
    }

    private void SetReelsTabVisible(bool isVisible)
    {
        isReelsTabActive = isVisible;

        if (algorithmPostArea != null)
        {
            algorithmPostArea.SetActive(isVisible);
        }

        if (refreshButtonObject != null)
        {
            refreshButtonObject.SetActive(isVisible);
        }

        SetReelsEnterButtonVisible(isVisible);
    }

    private void LateUpdate()
    {
        if (!isReelsTabActive)
        {
            return;
        }

        SetPreviewVisible(false);
    }

    private void SetPreviewVisible(bool isVisible)
    {
        ResolvePreviewImage();

        if (previewImage != null)
        {
            previewImage.enabled = isVisible;
            previewImage.raycastTarget = isVisible;
            previewImage.gameObject.SetActive(isVisible);
        }
    }

    private void ResolvePreviewImage()
    {
        if (previewImage != null)
        {
            return;
        }

        if (previewImage == null)
        {
            Transform foundPreviewImage = transform.Find("Image");
            if (foundPreviewImage != null)
            {
                previewImage = foundPreviewImage.GetComponent<Image>();
            }
        }
    }

    private void SetReelsEnterButtonVisible(bool isVisible)
    {
        GameObject targetObject = reelsEnterButtonObject;

        // Inspector 연결을 안 했을 때 기존 구조(themeButtons[0])로 자동 폴백
        if (targetObject == null && themeButtons != null && themeButtons.Length > 0 && themeButtons[0] != null)
        {
            targetObject = themeButtons[0].gameObject;
        }

        if (targetObject != null)
        {
            targetObject.SetActive(isVisible);
        }
    }

    private void ApplyPreviewSprite(Sprite targetSprite, string source)
    {
        if (previewImage == null)
        {
            return;
        }

        if (targetSprite == null)
        {
            return;
        }

        previewImage.sprite = targetSprite;
        // 레이아웃이 깨지지 않도록 크기 재설정은 하지 않고 스프라이트만 교체
        Debug.Log($"🖼️ {source} 버튼 클릭 - 미리보기 이미지 변경 완료");
    }

    // ============================================================
    /// 【 OnThemeButtonClicked() - 테마 버튼 클릭 이벤트 】
    /// 
    /// 호출 시점: 4개의 테마 버튼 중 하나를 클릭했을 때
    /// 
    /// 매개변수:
    /// - themeIndex: 선택한 테마의 인덱스 (0~3)
    /// 
    /// 역할:
    /// 1. 선택된 테마 저장
    /// 2. 클릭 로그 출력
    /// 3. GameManager.OnThemeSelected() 호출
    /// 4. Algorithm → Reels 상태 전환
    /// 5. 게임 본편(릴스) 화면 표시
    /// ============================================================
    private void OnThemeButtonClicked(int themeIndex)
    {
        selectedTheme = themeIndex;
        
        /// 선택된 테마 정보 로그
        string themeName = (themeIndex < themeNames.Length) ? themeNames[themeIndex] : "unknown";
        Debug.Log($"📌 테마 선택: {themeName}");
        
        /// GameManager에 테마 선택 완료 알림
        /// → 배터리 100% 초기화
        /// → 상태가 Algorithm → Reels로 변경
        /// → 화면이 AlgorithmScreen → ReelsScreen으로 전환
        GameManager.Instance.OnThemeSelected();
    }

    // ============================================================
    /// 【 GetSelectedTheme() - 선택된 테마 반환 (선택사항) 】
    /// 
    /// 역할:
    /// - 현재 선택된 테마의 인덱스 반환
    /// - 나중에 테마별 콘텐츠 표시 시 사용 가능
    /// 
    /// 용도:
    /// - ReelsScreen에서 테마에 맞는 게시물 표시
    /// - 게임 통계에서 어떤 테마로 플레이했는지 기록
    /// ============================================================
    public int GetSelectedTheme()
    {
        return selectedTheme;
    }
}
