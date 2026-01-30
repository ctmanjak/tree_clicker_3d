using UnityEngine;

public enum FloatingTextStyle
{
    Normal,
    Critical,
    Bonus
}

[CreateAssetMenu(fileName = "FloatingTextStyle", menuName = "Game/FloatingTextStyleConfig")]
public class FloatingTextStyleConfig : ScriptableObject
{
    [Header("Normal Style")]
    public float NormalFontSize = 24f;
    public Color NormalColor = Color.white;
    public float NormalFloatDistance = 50f;
    public float NormalDuration = 0.8f;

    [Header("Critical Style")]
    public float CriticalFontSize = 36f;
    public Color CriticalColor = new Color(1f, 0.9f, 0f);
    public Color CriticalOutlineColor = new Color(1f, 0.5f, 0f);
    public float CriticalFloatDistance = 80f;
    public float CriticalDuration = 1f;
    public float CriticalScalePunch = 0.3f;

    [Header("Bonus Style")]
    public float BonusFontSize = 28f;
    public Color BonusColor = new Color(0.3f, 1f, 0.3f);
    public float BonusFloatDistance = 60f;
    public float BonusDuration = 0.9f;
}

public class FloatingTextAnimator : MonoBehaviour
{
    [Header("Animation Curves")]
    [SerializeField] private AnimationCurve _moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve _scaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 1);
    [SerializeField] private AnimationCurve _alphaCurve = AnimationCurve.Linear(0, 1, 1, 0);

    [Header("Style Settings")]
    [SerializeField] private float _normalFontSize = 24f;
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private float _normalFloatDistance = 1f;
    [SerializeField] private float _normalDuration = 0.8f;

    [SerializeField] private float _criticalFontSize = 36f;
    [SerializeField] private Color _criticalColor = new Color(1f, 0.9f, 0f);
    [SerializeField] private float _criticalFloatDistance = 1.5f;
    [SerializeField] private float _criticalDuration = 1f;
    [SerializeField] private float _criticalScalePunch = 1.3f;

    [SerializeField] private float _bonusFontSize = 28f;
    [SerializeField] private Color _bonusColor = new Color(0.3f, 1f, 0.3f);
    [SerializeField] private float _bonusFloatDistance = 1.2f;
    [SerializeField] private float _bonusDuration = 0.9f;

    private FloatingTextStyle _currentStyle = FloatingTextStyle.Normal;

    public AnimationCurve MoveCurve => _moveCurve;
    public AnimationCurve ScaleCurve => _scaleCurve;
    public AnimationCurve AlphaCurve => _alphaCurve;

    public float FontSize => _currentStyle switch
    {
        FloatingTextStyle.Critical => _criticalFontSize,
        FloatingTextStyle.Bonus => _bonusFontSize,
        _ => _normalFontSize
    };

    public Color TextColor => _currentStyle switch
    {
        FloatingTextStyle.Critical => _criticalColor,
        FloatingTextStyle.Bonus => _bonusColor,
        _ => _normalColor
    };

    public float FloatDistance => _currentStyle switch
    {
        FloatingTextStyle.Critical => _criticalFloatDistance,
        FloatingTextStyle.Bonus => _bonusFloatDistance,
        _ => _normalFloatDistance
    };

    public float Duration => _currentStyle switch
    {
        FloatingTextStyle.Critical => _criticalDuration,
        FloatingTextStyle.Bonus => _bonusDuration,
        _ => _normalDuration
    };

    public float InitialScale => _currentStyle switch
    {
        FloatingTextStyle.Critical => _criticalScalePunch,
        _ => 1f
    };

    public void SetNormalStyle()
    {
        _currentStyle = FloatingTextStyle.Normal;
    }

    public void SetCriticalStyle()
    {
        _currentStyle = FloatingTextStyle.Critical;
    }

    public void SetBonusStyle()
    {
        _currentStyle = FloatingTextStyle.Bonus;
    }

    public void SetStyle(FloatingTextStyle style)
    {
        _currentStyle = style;
    }

    public FloatingTextStyle CurrentStyle => _currentStyle;
}
