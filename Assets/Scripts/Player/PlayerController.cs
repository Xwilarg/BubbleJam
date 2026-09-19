using BubbleJam.Bullet;
using Sketch.VN;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace BubbleJam.Player
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField]
        private CinemachineCamera _camera;

        [SerializeField]
        private Transform _playerCenterTracking;

        [SerializeField]
        private GameObject _gameUI;

        [SerializeField]
        private Image _healthBar;

        private int _health = 3;

        private bool _didStartMoving;
        public bool DidStartMoving
        {
            set
            {
                if (!_didStartMoving && value)
                {
                    _gameUI.SetActive(true);
                }
                _didStartMoving = value;
            }
            get => _didStartMoving;
        }

        private Rigidbody2D _rb;

        private Vector2 _mov;
        private Vector2 _lastDir = Vector2.up;
        private Vector2 _dashDir;
        private Vector2 _thrownDir;

        private const float Speed = 20f;
        private const float BulletSpeed = 8f;
        private const float BulletLifespan = 10f;

        private float _camTimer;
        private bool _isCamCentered;

        private bool _isAttacking;
        private float _reloadTimer;

        private Camera _cam;

        private Skill _dashSkill;
        private bool _isDashing;

        private bool _isBeingThrown;
        private float _thrownTimer;

        public void TakeDamage()
        {
            _health--;

            _healthBar.transform.localScale = new(_health / 3f, 1f, 1f);
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _cam = Camera.main;

            _dashSkill = new(2f, this);

            _gameUI.SetActive(false);
        }

        private IEnumerator DashCoroutine()
        {
            yield return new WaitForSeconds(.5f);
            _isDashing = false;
        }

        private void Update()
        {
            if (VNManager.Instance.IsStoryOngoing) return;

            if (!DidStartMoving && _mov.magnitude > 0f)
            {
                DidStartMoving = true;
            }

            if (_isBeingThrown)
            {
                _rb.linearVelocity = _thrownDir * Speed * 3f;
            }
            else if (_isDashing)
            {
                _rb.linearVelocity = _dashDir * Speed * 2f;
            }
            else
            {
                _rb.linearVelocity = _mov * Speed;
            }

            if (!_isCamCentered)
            {
                _camTimer += Time.deltaTime;

                if (_camTimer >= 1f)
                {
                    _camera.Follow = transform;
                }
                else
                {
                    _playerCenterTracking.position = Vector2.Lerp(Vector2.zero, transform.position, _camTimer);
                }
            }

            if (_reloadTimer > 0f)
            {
                _reloadTimer -= Time.deltaTime;
            }

            if (_reloadTimer <= 0f && _isAttacking)
            {
                DidStartMoving = true;

                var mouse = (Vector2)(_cam.ScreenToWorldPoint(Mouse.current.position.ReadValue()) - transform.position);

                var angle = Mathf.Atan2(mouse.y, mouse.x);
                BulletManager.Instance.Spawn(transform.position, angle, BulletSpeed, BulletLifespan, AttackShape.Straight);
                BulletManager.Instance.Spawn(transform.position, angle, BulletSpeed / 2f, BulletLifespan, AttackShape.Sin);

                _reloadTimer = .1f;
            }
        }

        public void Throw(Vector2 dir)
        {
            _isBeingThrown = true;
            _thrownDir = dir;
            StartCoroutine(ThrowCoroutine());
        }

        private IEnumerator ThrowCoroutine()
        {
            yield return new WaitForSeconds(.75f);
            _isBeingThrown = false;
        }

        public void OnMovement(InputAction.CallbackContext value)
        {
            _mov = value.ReadValue<Vector2>();

            if (_mov.magnitude > 0f)
            {
                _lastDir = _mov;
            }
        }

        public void OnAttack(InputAction.CallbackContext value)
        {
            if (VNManager.Instance.IsStoryOngoing)
            {
                if (value.phase == InputActionPhase.Started) VNManager.Instance.DisplayNextDialogue();
                return;
            }

            if (value.phase == InputActionPhase.Started) _isAttacking = true;
            else if (value.phase == InputActionPhase.Canceled) _isAttacking = false;
        }

        public void OnDash(InputAction.CallbackContext value)
        {
            if (!VNManager.Instance.IsStoryOngoing && _dashSkill.CanUse && value.phase == InputActionPhase.Started)
            {
                _dashSkill.Use();
                _isDashing = true;
                _dashDir = _lastDir;
                StartCoroutine(DashCoroutine());
            }
        }
    }
}
