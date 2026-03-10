using UnityEngine;

namespace UI
{
    public class TrailMatchSpriteColor : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private TrailRenderer _trailRenderer;

        [Header("Gradient")]
        [SerializeField, Range(0f, 1f)] private float headAlpha = 1f;
        [SerializeField, Range(0f, 1f)] private float tailAlpha = 0f;

        [Header("Size")]
        [SerializeField, Min(0.01f)] private float widthMultiplier = 2f;
        [SerializeField, Min(0.01f)] private float trailTime = 0.28f;

        private Color _lastSpriteColor = Color.clear;
        private bool _applied;

        private void Awake()
        {
            TryAutoBind();
            ApplyTrailStyle(true);
        }

        private void OnEnable()
        {
            ApplyTrailStyle(true);
        }

        private void LateUpdate()
        {
            if (_spriteRenderer == null || _trailRenderer == null)
            {
                return;
            }

            if (!_applied || _spriteRenderer.color != _lastSpriteColor)
            {
                ApplyTrailStyle(false);
            }
        }

        private void OnValidate()
        {
            TryAutoBind();
            ApplyTrailStyle(false);
        }

        private void TryAutoBind()
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }

            if (_trailRenderer == null)
            {
                _trailRenderer = GetComponent<TrailRenderer>();
            }
        }

        private void ApplyTrailStyle(bool clearTrail)
        {
            if (_spriteRenderer == null || _trailRenderer == null)
            {
                return;
            }

            Color baseColor = _spriteRenderer.color;
            Color headColor = new Color(baseColor.r, baseColor.g, baseColor.b, headAlpha);
            Color tailColor = new Color(baseColor.r, baseColor.g, baseColor.b, tailAlpha);

            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(headColor, 0f),
                    new GradientColorKey(tailColor, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(headAlpha, 0f),
                    new GradientAlphaKey(tailAlpha, 1f)
                });

            _trailRenderer.colorGradient = gradient;
            _trailRenderer.widthMultiplier = widthMultiplier;
            _trailRenderer.time = trailTime;

            if (clearTrail)
            {
                _trailRenderer.Clear();
            }

            _lastSpriteColor = baseColor;
            _applied = true;
        }
    }
}