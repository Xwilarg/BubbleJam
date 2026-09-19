using BubbleJam.Bullet;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BubbleJam.Player
{
    public class PlayerController : MonoBehaviour
    {
        public bool DidStartMoving { private set; get; }

        private Rigidbody2D _rb;

        private Vector2 _mov;

        private const float Speed = 20f;
        private const float BulletSpeed = 8f;
        private const float BulletLifespan = 10f;

        private bool _isAttacking;
        private float _reloadTimer;

        private Camera _cam;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _cam = Camera.main;
        }

        private void Update()
        {
            if (!DidStartMoving && _mov.magnitude > 0f)
            {
                DidStartMoving = true;
            }
            _rb.linearVelocity = _mov * Speed;

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

        public void OnMovement(InputAction.CallbackContext value)
        {
            _mov = value.ReadValue<Vector2>();
        }

        public void OnAttack(InputAction.CallbackContext value)
        {
            if (value.phase == InputActionPhase.Started) _isAttacking = true;
            else if (value.phase == InputActionPhase.Canceled) _isAttacking = false;
        }
    }
}
