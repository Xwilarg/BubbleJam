using BubbleJam;
using BubbleJam.Audio;
using BubbleJam.Game;
using BubbleJam.Player;
using Sketch.VN;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour
{
    [SerializeField]
    private PlayerController _player;

    [SerializeField]
    private Image _ultimateProgression;

    [SerializeField]
    private TMP_Text _ultimateText;

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

    [SerializeField]
    private Animator _ultimateAnim;

    private float _currSpeed = 15f;

    private Vector2 _dir;

    private Rigidbody2D _rb;

    private int _bulletMask;

    private Skill _slashSkill;
    private Skill _moveSkill;

    private float _ultimate;

    private Vector2 _thrownDir;
    private float _throwTimer;

    private bool _didGameEnd;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _bulletMask = LayerMask.GetMask("Bullet");

        _moveSkill = new(MoveDirtyDuration, this);
        _slashSkill = new(.5f, this);

        UpdateUltimateUI();
    }

    private IEnumerator PlayEndingCoroutine()
    {
        yield return new WaitForSeconds(3.5f);
        GameManager.Instance.PlayGoodEnding();
    }

    private void Update()
    {
        if (_didGameEnd) return;

        if (_player.DidStartMoving && !VNManager.Instance.IsStoryOngoing)
        {
            _ultimate = Mathf.Clamp(_ultimate + Time.deltaTime / 50f, 0f, 1f);
            if (_ultimate == 1f)
            {
                _didGameEnd = true;
                _ultimateAnim.SetTrigger("Ultimate");
                StartCoroutine(PlayEndingCoroutine());
            }
            UpdateUltimateUI();

            _currSpeed = Mathf.Clamp(_currSpeed + Time.deltaTime / 2f, _currSpeed, Speed);

            if (_throwTimer > 0f) _throwTimer -= Time.deltaTime;

            if (_moveSkill.CanUse)
            {
                var a = GetBestAngle();

                if (_slashSkill.CanUse)
                {
                    var atckPos = (Vector2)transform.position + _dir;
                    if (Physics2D.OverlapCircle(atckPos, 1f, _bulletMask))
                    {
                        ShowAttack(atckPos, a * Mathf.Rad2Deg - 90f);
                        _slashSkill.Use();
                    }
                }

                _dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));

                _moveSkill.Use();
            }
        }
    }

    private void FixedUpdate()
    {
        if (VNManager.Instance.IsStoryOngoing || !_player.DidStartMoving || _didGameEnd)
        {
            _rb.linearVelocity = Vector2.zero;
        }
        else if (_throwTimer > 0f)
        {
            _rb.linearVelocity = _thrownDir * _currSpeed * 2f;
        }
        else
        {
            _rb.linearVelocity = _dir * _currSpeed;
        }
    }

    public void HitBullet(Vector3 bulletPos)
    {
        _throwTimer = .1f;
        _thrownDir = Vector2.Perpendicular((transform.position - bulletPos).normalized);
    }

    private void UpdateUltimateUI()
    {
        var ultimateInt = Mathf.FloorToInt(_ultimate * 100f);

        _ultimateText.text = $"{ultimateInt}%";
        _ultimateProgression.fillAmount = _ultimate;
    }

    private void ShowAttack(Vector2 pos, float angle)
    {
        Destroy(Instantiate(_slashPrefab, pos, Quaternion.Euler(0f, 0f, angle)), .5f);
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !_didGameEnd)
        {
            var dir = (collision.transform.position - transform.position).normalized;
            collision.GetComponent<PlayerController>().Throw(dir);
            ShowAttack(collision.transform.position, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f);
        }
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
