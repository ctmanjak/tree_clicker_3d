using UnityEngine;

/// <summary>
/// PC/웹에서 세로형 게임 비율을 유지하고 좌우에 Pillarbox를 표시합니다.
/// 모바일에서는 Portrait 고정으로 동작합니다.
/// </summary>
public class AspectRatioManager : MonoBehaviour
{
    [Header("Target Aspect Ratio")]
    [SerializeField] private float _targetWidth = 9f;
    [SerializeField] private float _targetHeight = 16f;

    [Header("Pillarbox Settings")]
    [SerializeField] private Color _pillarboxColor = Color.black;

    private Camera _mainCamera;
    private Camera _pillarboxCamera;
    private float _lastAspect;

    private float TargetAspect => _targetWidth / _targetHeight;

    private void Awake()
    {
        _mainCamera = Camera.main;

        if (_mainCamera == null)
        {
            Debug.LogError("Main Camera not found!");
            enabled = false;
            return;
        }

        SetupPlatformSettings();
        CreatePillarboxCamera();
    }

    private void Start()
    {
        UpdateAspectRatio();
    }

    private void Update()
    {
        float currentAspect = (float)Screen.width / Screen.height;

        if (!Mathf.Approximately(currentAspect, _lastAspect))
        {
            UpdateAspectRatio();
        }
    }

    private void SetupPlatformSettings()
    {
#if UNITY_IOS || UNITY_ANDROID
        if (!Application.isEditor)
        {
            Screen.orientation = ScreenOrientation.Portrait;
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
        }
#endif
    }

    private void CreatePillarboxCamera()
    {
        GameObject pillarboxObject = new GameObject("PillarboxCamera");
        pillarboxObject.transform.SetParent(transform);

        _pillarboxCamera = pillarboxObject.AddComponent<Camera>();
        _pillarboxCamera.depth = _mainCamera.depth - 1;
        _pillarboxCamera.clearFlags = CameraClearFlags.SolidColor;
        _pillarboxCamera.backgroundColor = _pillarboxColor;
        _pillarboxCamera.cullingMask = 0;
    }

    private void UpdateAspectRatio()
    {
        float currentAspect = (float)Screen.width / Screen.height;
        _lastAspect = currentAspect;

        // 현재 화면이 타겟보다 가로로 긴 경우 (PC/웹 전체화면)
        if (currentAspect > TargetAspect)
        {
            ApplyPillarbox(currentAspect);
        }
        else
        {
            // 세로 비율이거나 비슷한 경우 전체 화면 사용
            ResetToFullScreen();
        }
    }

    private void ApplyPillarbox(float currentAspect)
    {
        float viewportWidth = TargetAspect / currentAspect;
        float viewportX = (1f - viewportWidth) / 2f;

        _mainCamera.rect = new Rect(viewportX, 0f, viewportWidth, 1f);

        // UI Canvas들도 같은 영역에 맞추기 위해 이벤트 발생
        UpdateCanvasScalers(viewportX, viewportWidth);
    }

    private void ResetToFullScreen()
    {
        _mainCamera.rect = new Rect(0f, 0f, 1f, 1f);
    }

    private void UpdateCanvasScalers(float viewportX, float viewportWidth)
    {
        // Screen Space - Overlay Canvas들은 자동으로 뷰포트를 따라가지 않으므로
        // 필요시 Canvas들의 RectTransform을 조정하거나
        // Screen Space - Camera로 변경해야 합니다.

        // 현재는 메인 카메라 뷰포트만 조정하고,
        // UI는 Screen Space - Camera 모드 사용을 권장합니다.
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_targetWidth <= 0) _targetWidth = 9f;
        if (_targetHeight <= 0) _targetHeight = 16f;
    }
#endif
}
