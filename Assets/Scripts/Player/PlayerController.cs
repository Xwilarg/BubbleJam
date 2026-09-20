using BubbleJam.Bullet;
using Sketch.VN;
using System.Collections;
using TMPro;
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
        private Image _healthBar;

        [SerializeField]
        private EnemyController _enemy;

        [SerializeField]
        private TMP_Text _skillDebug;

        private int _health = 3;

        public bool DidStartMoving { set; get; }

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

        private bool _isUsingSkill3;

        private Skill _skill1, _skill2, _skill3;

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

            _skill1 = new(10f, this);
            _skill2 = new(10f, this);
            _skill3 = new(15f, this);
        }

        private IEnumerator DashCoroutine()
        {
            yield return new WaitForSeconds(.5f);
            _isDashing = false;
        }

        private void Update()
        {
            _skillDebug.text = $"Skill 1: {(_skill1.CanUse ? "Ready" : "Reloading...")}\nSkill 2: {(_skill2.CanUse ? "Ready" : "Reloading...")}\nSkill 3: {(_skill3.CanUse ? "Ready" : "Reloading...")}";

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
                
                if (_isUsingSkill3)
                {
                    BulletManager.Instance.Spawn(transform.position, angle, BulletSpeed, BulletLifespan, AttackShape.Sin);
                    BulletManager.Instance.Spawn(transform.position, angle, BulletSpeed, BulletLifespan, AttackShape.Cos);
                }
                else
                {
                    BulletManager.Instance.Spawn(transform.position, angle, BulletSpeed, BulletLifespan, AttackShape.Straight);
                }

                _reloadTimer = .1f;
            }
        }

        public void Throw(Vector2 dir)
        {
            _isBeingThrown = true;
            _thrownDir = dir;
            TakeDamage();
            StartCoroutine(ThrowCoroutine());
        }

        private IEnumerator ThrowCoroutine()
        {
            yield return new WaitForSeconds(.75f);
            _isBeingThrown = false;
        }

        private IEnumerator PlaySkillAllDirections()
        {
            var wait = new WaitForSeconds(.1f);
            for (float angle = 0f; angle < Mathf.PI * 2f; angle += Mathf.PI / 10f)
            {
                BulletManager.Instance.Spawn(transform.position, angle, 10f, 10f, AttackShape.Straight);
                BulletManager.Instance.Spawn(transform.position, angle + Mathf.PI, 10f, 10f, AttackShape.Straight);
                BulletManager.Instance.Spawn(transform.position, angle - Mathf.PI / 2f, 10f, 10f, AttackShape.Straight);
                BulletManager.Instance.Spawn(transform.position, angle + Mathf.PI / 2f, 10f, 10f, AttackShape.Straight);

                yield return wait;
            }
        }

        private IEnumerator PlaySkillBlast()
        {
            var wait = new WaitForSeconds(.25f);
            for (float i = 0f; i < 10; i++)
            {
                var dir = _enemy.transform.position - transform.position;
                var angle = Mathf.Atan2(dir.y, dir.x);
                BulletManager.Instance.Spawn(transform.position, angle, 10f, 10f, AttackShape.Straight);
                BulletManager.Instance.Spawn(transform.position, angle - Mathf.PI / 4f, 10f, 10f, AttackShape.Straight);
                BulletManager.Instance.Spawn(transform.position, angle + Mathf.PI / 4f, 10f, 10f, AttackShape.Straight);
                BulletManager.Instance.Spawn(transform.position, angle - Mathf.PI / 2f, 10f, 10f, AttackShape.Straight);
                BulletManager.Instance.Spawn(transform.position, angle + Mathf.PI / 2f, 10f, 10f, AttackShape.Straight);

                yield return wait;
            }
        }

        private IEnumerator PlaySkill3Wave()
        {
            _isUsingSkill3 = true;
            yield return new WaitForSeconds(3f);
            _isUsingSkill3 = false;
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

                DidStartMoving = true;
            }
        }

        public void Skill1(InputAction.CallbackContext value)
        {
            if (!VNManager.Instance.IsStoryOngoing && value.phase == InputActionPhase.Started && _skill1.CanUse)
            {
                StartCoroutine(PlaySkillAllDirections());

                _skill1.Use();
                DidStartMoving = true;
            }
        }

        public void Skill2(InputAction.CallbackContext value)
        {
            if (!VNManager.Instance.IsStoryOngoing && value.phase == InputActionPhase.Started && _skill2.CanUse)
            {
                StartCoroutine(PlaySkillBlast());

                _skill2.Use();
                DidStartMoving = true;
            }
        }

        public void Skill3(InputAction.CallbackContext value)
        {
            if (!VNManager.Instance.IsStoryOngoing && value.phase == InputActionPhase.Started && _skill3.CanUse)
            {
                StartCoroutine(PlaySkill3Wave());

                _skill3.Use();
                DidStartMoving = true;
            }
        }
    }
}
