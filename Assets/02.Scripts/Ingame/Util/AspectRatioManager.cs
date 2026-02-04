using UnityEngine;

namespace Ingame
{
    public class AspectRatioManager : MonoBehaviour
    {
        private const float CHECK_INTERVAL = 0.25f;

        [Header("Target Aspect Ratio")]
        [SerializeField] private float _targetWidth = 9f;
        [SerializeField] private float _targetHeight = 16f;

        [Header("Pillarbox Settings")]
        [SerializeField] private Color _pillarboxColor = Color.black;

        private Camera _mainCamera;
        private Camera _pillarboxCamera;
        private float _lastAspect;
        private float _checkTimer;

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
            UpdateAspectRatio(GetCurrentAspect());
        }

        private void Update()
        {
            _checkTimer += Time.deltaTime;
            if (_checkTimer < CHECK_INTERVAL) return;
            _checkTimer = 0f;

            float currentAspect = GetCurrentAspect();
            if (!Mathf.Approximately(currentAspect, _lastAspect))
            {
                UpdateAspectRatio(currentAspect);
            }
        }

        private float GetCurrentAspect()
        {
            return (float)Screen.width / Screen.height;
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

        private void UpdateAspectRatio(float currentAspect)
        {
            _lastAspect = currentAspect;

            if (currentAspect > TargetAspect)
            {
                ApplyPillarbox(currentAspect);
            }
            else
            {
                ResetToFullScreen();
            }
        }

        private void ApplyPillarbox(float currentAspect)
        {
            float viewportWidth = TargetAspect / currentAspect;
            float viewportX = (1f - viewportWidth) / 2f;

            _mainCamera.rect = new Rect(viewportX, 0f, viewportWidth, 1f);
        }

        private void ResetToFullScreen()
        {
            _mainCamera.rect = new Rect(0f, 0f, 1f, 1f);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_targetWidth <= 0) _targetWidth = 9f;
            if (_targetHeight <= 0) _targetHeight = 16f;
        }
#endif
    }
}
