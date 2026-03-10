using System;
using DG.Tweening;
using UnityEngine;

namespace Scripts
{
    public class Block : MonoBehaviour
    {
        [SerializeField] private bool _destroyOnReturn = false;
        [SerializeField] private Vector2Int _boardPos = new Vector2Int(-1, -1);
        [SerializeField] private Direction _direction = Direction.Right;
        [SerializeField] private SpriteRenderer _arrowRenderer;
        [SerializeField] private Sprite _arrowSprite;
        [SerializeField, Min(0.01f)] private float _slideDuration = 0.2f;
        [SerializeField, Min(0.1f)] private float _maxSlideDuration = 1.4f;
        [SerializeField] private Ease _slideEase = Ease.InCubic;

        [SerializeField, Min(0.01f)] private float _blockedShakeDuration = 0.16f;
        [SerializeField, Min(0.001f)] private float _blockedShakeStrength = 0.08f;
        [SerializeField, Min(1)] private int _blockedShakeVibrato = 18;
        [SerializeField, Min(0.01f)] private float _blockedFlashDuration = 0.08f;
        [SerializeField] private Color _blockedFlashColor = new Color(1f, 0.25f, 0.25f, 1f);

        public Vector2Int BoardPos => _boardPos;
        public Direction Direction => _direction;

        private int _colorId;
        public int ColorID => _colorId;

        private SpriteRenderer _spriteRenderer;
        private Collider2D _collider2D;
        private Camera _mainCamera;
        private Board _board;
        private Tween _slideTween;
        private Sequence _blockedFeedbackTween;
        private bool _isMoving;
        private Color _baseBlockColor = Color.white;
        private Color _baseArrowColor = Color.white;

        public bool IsMoving => _isMoving;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _collider2D = GetComponent<Collider2D>();
            _mainCamera = Camera.main;
            CacheBaseColors();
            RefreshArrowVisual();
        }
        private void Update()
        {
            if (!isActiveAndEnabled || _isMoving)
            {
                return;
            }

            if (!Input.GetMouseButtonDown(0))
            {
                return;
            }

            if (UIHelper.Instance != null && UIHelper.Instance.IsPointerOverUI())
            {
                return;
            }

            if (_board == null || _collider2D == null)
            {
                return;
            }

            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
                if (_mainCamera == null)
                {
                    return;
                }
            }

            Vector3 mouseWorld = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 point = new Vector2(mouseWorld.x, mouseWorld.y);
            if (_collider2D.OverlapPoint(point))
            {
                _board.TrySlideBlock(this);
            }
        }
        private void OnDisable()
        {
            _slideTween?.Kill();
            _blockedFeedbackTween?.Kill();
            _isMoving = false;
            _slideTween = null;
            _blockedFeedbackTween = null;
            ResetVisualState();
        }

        public void Init(Sprite sprite, int colorId)
        {
            _spriteRenderer.sprite = sprite;
            _colorId = colorId;
            CacheBaseColors();
            ResetVisualState();
            RefreshArrowVisual();
        }

        public void SetDirection(Direction dir)
        {
            _direction = dir;
            RefreshArrowVisual();
        }

        public void SetCellVisualSize(float targetCellSize)
        {
            Vector2 spriteSize = _spriteRenderer.sprite.bounds.size;

            float safeSize = targetCellSize;
            float scaleX = safeSize / spriteSize.x;
            float scaleY = safeSize / spriteSize.y; 
            float newScale = Mathf.Min(scaleX, scaleY);
            transform.localScale = Vector3.one * newScale;
        }

        private void RefreshArrowVisual()
        {
            float zAngle = DirectionToZ(_direction);
            _arrowRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, zAngle);
        }

        private static float DirectionToZ(Direction dir)
        {
            switch (dir)
            {
                case Direction.Right:
                    return 0f;
                case Direction.Up:
                    return 90f;
                case Direction.Left:
                    return 180f;
                case Direction.Down:
                    return -90f;
                default:
                    return 0f;
            }
        }

        public bool PlaySlideTo(Vector3 worldTarget)
        {
            return PlaySlideTo(worldTarget, null);
        }

        public bool PlaySlideTo(Vector3 worldTarget, Action onComplete)
        {
            if (_isMoving)
            {
                return false;
            }

            _slideTween?.Kill();
            _isMoving = true;

            float distance = Vector3.Distance(transform.position, worldTarget);
            float duration = _slideDuration * Mathf.Sqrt(Mathf.Max(1f, distance));
            duration = Mathf.Clamp(duration, _slideDuration, _maxSlideDuration);

            _slideTween = transform.DOMove(worldTarget, duration)
                .SetEase(_slideEase)
                .OnComplete(() =>
                {
                    _isMoving = false;
                    _slideTween = null;
                    onComplete?.Invoke();
                })
                .OnKill(() =>
                {
                    _isMoving = false;
                    _slideTween = null;
                });

            return true;
        }

        public void PlayBlockedFeedback()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            _blockedFeedbackTween?.Kill();
            ResetVisualState();

            _blockedFeedbackTween = DOTween.Sequence();
            _blockedFeedbackTween.Append(transform.DOShakePosition(
                    _blockedShakeDuration,
                    _blockedShakeStrength,
                    _blockedShakeVibrato,
                    90f,
                    false,
                    true))
                .Join(_spriteRenderer.DOColor(_blockedFlashColor, _blockedFlashDuration))
                .Append(_spriteRenderer.DOColor(_baseBlockColor, _blockedFlashDuration));

            if (_arrowRenderer != null)
            {
                _blockedFeedbackTween.Join(_arrowRenderer.DOColor(_blockedFlashColor, _blockedFlashDuration));
                _blockedFeedbackTween.Append(_arrowRenderer.DOColor(_baseArrowColor, _blockedFlashDuration));
            }

            _blockedFeedbackTween.OnComplete(() =>
            {
                _blockedFeedbackTween = null;
                ResetVisualState();
            });

            _blockedFeedbackTween.OnKill(() =>
            {
                _blockedFeedbackTween = null;
                ResetVisualState();
            });
        }

        public void SetBoardPos(Vector2Int pos)
        {
            _boardPos = pos;
        }

        public void ReturnToPool()
        {
            _slideTween?.Kill();
            _blockedFeedbackTween?.Kill();
            _boardPos = new Vector2Int(-1, -1);
            ResetVisualState();
            if (_destroyOnReturn)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        public void SetBoard(Board board)
        {
            _board = board;
        }

        private void CacheBaseColors()
        {
            _baseBlockColor = _spriteRenderer.color;
            _baseArrowColor = _arrowRenderer.color;
        }

        private void ResetVisualState()
        {
            _spriteRenderer.color = _baseBlockColor;
            _arrowRenderer.color = _baseArrowColor;
        }
    }
}