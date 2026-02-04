using UnityEngine;

namespace Ingame
{
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
}
