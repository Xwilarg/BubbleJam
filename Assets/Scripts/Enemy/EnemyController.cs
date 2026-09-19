using BubbleJam;
using BubbleJam.Player;
using System;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField]
    private PlayerController _player;

    [SerializeField]
    private float Speed = 15f;
    [SerializeField]
    private float MaxDist = 10f;
    [SerializeField]
    private float Size = 1f;
    [SerializeField]
    private float MoveDirtyDuration = .1f;
    [SerializeField]
    private float EmergencySize = 1f;

    [SerializeField]
    private GameObject _slashPrefab;

    private Vector2 _dir;

    private Rigidbody2D _rb;

    private int _bulletMask;

    private Skill _slashSkill;
    private Skill _moveSkill;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _bulletMask = LayerMask.GetMask("Bullet");

        _moveSkill = new(MoveDirtyDuration, this);
        _slashSkill = new(.5f, this);
    }

    private void Update()
    {
        if (_moveSkill .CanUse && _player.DidStartMoving)
        {
            var a = GetBestAngle();
            _dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));

            if (_slashSkill.CanUse)
            {
                var atckPos = (Vector2)transform.position + _dir;
                if (Physics2D.OverlapCircle(atckPos, 1f, _bulletMask))
                {
                    Destroy(Instantiate(_slashPrefab, atckPos, Quaternion.Euler(0f, 0f, a * Mathf.Rad2Deg)), .5f);
                    _slashSkill.Use();
                }
            }

            _moveSkill.Use();
        }
    }

    private void FixedUpdate()
    {
        _rb.linearVelocity = _dir * Speed;
    }

    private float GetBestAngle()
    {
        float bestScore = 0f;
        float bestAngle = float.MinValue;

        var dir = (Vector2)(_player.transform.position - transform.position);
        var optimal = Mathf.Atan2(dir.y, dir.x);
        var optimalDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        var up = Mathf.Atan2(transform.right.y, transform.right.x);

        for (var angle = 0f; angle < 2f * Mathf.PI; angle += MathF.PI / 20f)
        {
            var finalAngle = angle + up;
            var d = new Vector2(Mathf.Cos(finalAngle), Mathf.Sin(finalAngle));
            var hit = Physics2D.CircleCast(transform.position, Size, d, MaxDist, _bulletMask);
            var emergency = Physics2D.OverlapCircle(transform.position + (Vector3)d * EmergencySize, EmergencySize, _bulletMask);

            var scoreFree = hit.collider == null ? 1f : (hit.distance / MaxDist);
            var scoreSafeFree = emergency == null ? 1f : 0f;
            var scorePlayer = 1f - Mathf.Abs(Mathf.DeltaAngle(finalAngle * Mathf.Rad2Deg, optimalDeg) / 180f);

            var score = (scoreFree * 5f) + (scoreSafeFree * 5f) + (scorePlayer * ((_slashSkill?.CanUse ?? false) ? 20f : 1f));
            if (score > bestScore)
            {
                bestAngle = finalAngle;
                bestScore = score;
            }
        }

        return bestAngle;
    }

    private void OnDrawGizmos()
    {
        _bulletMask = LayerMask.GetMask("Bullet");

        var dir = (Vector2)(_player.transform.position - transform.position);
        var optimal = Mathf.Atan2(dir.y, dir.x);
        var up = Mathf.Atan2(transform.right.y, transform.right.x);

        for (var angle = 0f; angle < 2f * Mathf.PI; angle += MathF.PI / 20f)
        {
            var finalAngle = angle + up;
            var hit = Physics2D.CircleCast(transform.position, Size, new Vector2(Mathf.Cos(finalAngle), Mathf.Sin(finalAngle)), MaxDist, _bulletMask);
            if (hit.collider != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, hit.point);
            }
            else
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, transform.position + new Vector3(Mathf.Cos(finalAngle), Mathf.Sin(finalAngle)) * MaxDist);
            }

            var ep = transform.position + (Vector3)new Vector2(Mathf.Cos(finalAngle), Mathf.Sin(finalAngle)) * EmergencySize;
            var emergency = Physics2D.OverlapCircle(ep, EmergencySize, _bulletMask);
            if (emergency != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(ep, EmergencySize);
            }
        }

        var aF = GetBestAngle();
        Gizmos.color = Color.green;
        var a = new Vector2(Mathf.Cos(aF), Mathf.Sin(aF));
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)a.normalized * MaxDist);
    }
}
