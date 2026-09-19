using BubbleJam.Bullet;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BubbleJam.Player
{
    public class PlayerController : MonoBehaviour
    {
        private Rigidbody2D _rb;

        private Vector2 _mov;

        private const float Speed = 20f;

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
            _rb.linearVelocity = _mov * Speed;

            if (_reloadTimer > 0f)
            {
                _reloadTimer -= Time.deltaTime;
            }

            if (_reloadTimer <= 0f && _isAttacking)
            {
                var mouse = (Vector2)(_cam.ScreenToWorldPoint(Mouse.current.position.ReadValue()) - transform.position);

                var angle = Mathf.Atan2(mouse.y, mouse.x);
                BulletManager.Instance.Spawn(transform.position, angle, 20f, 10f, AttackShape.Straight);
                //BulletManager.Instance.Spawn(transform.position, angle, 20f, 10f, AttackShape.Cos);

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
