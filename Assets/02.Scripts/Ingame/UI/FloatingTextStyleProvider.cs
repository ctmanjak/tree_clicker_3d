using UnityEngine;

namespace Ingame
{
    public class FloatingTextStyleProvider : MonoBehaviour
    {
        [Header("Animation Curves")]
        [SerializeField] private AnimationCurve _moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private AnimationCurve _scaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 1);
        [SerializeField] private AnimationCurve _alphaCurve = AnimationCurve.Linear(0, 1, 1, 0);

        [Header("Style Config")]
        [SerializeField] private FloatingTextStyleConfig _styleConfig;

        private FloatingTextStyle _currentStyle = FloatingTextStyle.Normal;

        public AnimationCurve MoveCurve => _moveCurve;
        public AnimationCurve ScaleCurve => _scaleCurve;
        public AnimationCurve AlphaCurve => _alphaCurve;

        public float FontSize => _currentStyle switch
        {
            FloatingTextStyle.Critical => _styleConfig.CriticalFontSize,
            FloatingTextStyle.Bonus => _styleConfig.BonusFontSize,
            _ => _styleConfig.NormalFontSize
        };

        public Color TextColor => _currentStyle switch
        {
            FloatingTextStyle.Critical => _styleConfig.CriticalColor,
            FloatingTextStyle.Bonus => _styleConfig.BonusColor,
            _ => _styleConfig.NormalColor
        };

        public float FloatDistance => _currentStyle switch
        {
            FloatingTextStyle.Critical => _styleConfig.CriticalFloatDistance,
            FloatingTextStyle.Bonus => _styleConfig.BonusFloatDistance,
            _ => _styleConfig.NormalFloatDistance
        };

        public float Duration => _currentStyle switch
        {
            FloatingTextStyle.Critical => _styleConfig.CriticalDuration,
            FloatingTextStyle.Bonus => _styleConfig.BonusDuration,
            _ => _styleConfig.NormalDuration
        };

        public float InitialScale => _currentStyle switch
        {
            FloatingTextStyle.Critical => _styleConfig.CriticalScalePunch,
            _ => 1f
        };

        public Color CriticalOutlineColor => _styleConfig.CriticalOutlineColor;

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
}
