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
        [SerializeField] private Ease _slideEase = Ease.OutQuad;

        public Vector2Int BoardPos => _boardPos;
        public Direction Direction => _direction;
        private int _colorId;
        public int ColorID => _colorId;
        private SpriteRenderer _spriteRenderer;
        private Board _board;
        private Tween _slideTween;
        private bool _isMoving;

        public bool IsMoving => _isMoving;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();

            if (_arrowRenderer != null && _arrowSprite != null && _arrowRenderer.sprite == null)
            {
                _arrowRenderer.sprite = _arrowSprite;
            }

            RefreshArrowVisual();
        }

        public void Init(Sprite sprite, int colorId)
        {
            _spriteRenderer.sprite = sprite;
            _colorId = colorId;
            RefreshArrowVisual();
        }

        public void SetDirection(Direction dir)
        {
            _direction = dir;
            RefreshArrowVisual();
        }

        private void RefreshArrowVisual()
        {
            if (_arrowRenderer == null)
            {
                return;
            }

            if (_arrowRenderer.sprite == null && _arrowSprite != null)
            {
                _arrowRenderer.sprite = _arrowSprite;
            }

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

            _slideTween = transform
                .DOMove(worldTarget, _slideDuration)
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

        public void SetBoardPos(Vector2Int pos)
        {
            _boardPos = pos;
        }

        public void ReturnToPool()
        {
            _slideTween?.Kill();
            _boardPos = new Vector2Int(-1, -1);
            if (_destroyOnReturn)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        public void SetBoardManager(Board board)
        {
            _board = board;
        }

        private void OnDisable()
        {
            _slideTween?.Kill();
            _isMoving = false;
            _slideTween = null;
        }

        private void OnMouseDown()
        {
            if (!isActiveAndEnabled || _isMoving)
            {
                return;
            }

            if (_board == null)
            {
                _board = FindFirstObjectByType<Board>();
            }

            if (_board != null)
            {
                _board.TrySlideBlock(this);
            }
        }

        private static Vector2Int DirectionToStep(Direction direction)
        {
            switch (direction)
            {
                case Direction.Right:
                    return Vector2Int.right;
                case Direction.Left:
                    return Vector2Int.left;
                case Direction.Up:
                    return Vector2Int.up;
                case Direction.Down:
                    return Vector2Int.down;
                default:
                    return Vector2Int.zero;
            }
        }
    }
}