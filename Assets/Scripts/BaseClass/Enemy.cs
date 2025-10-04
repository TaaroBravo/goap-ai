using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public abstract class Enemy : Entity
{
    protected Player _player;
    protected EnemyView _view;

    public float rotSpeed;

    protected bool _playerInArea;
    protected float _playerMinDistance;
    protected float _distanceToAttack;

    protected bool _cooldown;
    protected float _cooldownTimer;

    public LayerMask _playerLayer;
    public bool isBoss;
    public float distanceToAttack;

    public event Action<bool> OnCooldown;
    public event Action<float> OnCooldownMove;
    public event Action OnEnemyDown = ()=> { };

    protected IEnumerator WaitToMove(float x)
    {
        while (true)
        {
            if (_playerInArea)
            {
                yield return new WaitForSeconds(x);
                _isMoving = true;
                break;
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    protected override void MoveTo(Vector3 pos)
    {
        var dir = (_player.transform.position - transform.position).normalized;
        GetObstacle();
        if (_closerOb != null)
            dir += ((transform.position - _closerOb.position) * avoidWeight);
        dir.y = 0;
        transform.forward = Vector3.Slerp(transform.forward, dir, rotSpeed * Time.deltaTime);
    }

    protected bool PlayerCloseToAttack()
    {
        if ((_player.transform.position - transform.position).magnitude < _distanceToAttack)
            return true;
        return false;
    }

    protected void FindPlayer()
    {
        var targets = Physics.OverlapSphere(transform.position, _playerMinDistance, _playerLayer)
                      .Select(x =>
                      {
                          var temp = x.GetComponent<Player>();
                          if (!temp)
                              temp = x.GetComponentInParent<Player>();
                          return temp;
                      }).Where(x => x != null).Where(x =>
                      {
                          var dir = x.transform.position - transform.position;
                          dir.y = 0;
                          return Vector3.Angle(dir, transform.forward) <= 45f;
                      });
        if (targets.Any())
        {
            if (_player != null)
                targets = targets.Where(x => x != _player);
            if (targets.Any())
                _player = targets.OrderBy(x => Vector3.Distance(transform.position, x.transform.position)).First();
            _playerInArea = true;
            return;
        }
        _playerInArea = false;
    }


    protected IEnumerator AttackCoolDown(float x)
    {
        yield return new WaitUntil(() => _cooldown);
        OnCooldown(true);
        OnCooldownMove(UnityEngine.Random.Range(0, 2) == 0 ? 0 : 1);
        yield return new WaitForSeconds(x);
        OnCooldownMove(0.5f);
        OnCooldown(false);
        _cooldown = false;
    }

    protected IEnumerator RandomPatrol()
    {
        while (true)
        {
            yield return new WaitUntil(() => _player);
            bool random = UnityEngine.Random.value > 0.5f;
            if (random)
                _cooldown = true;
            break;
        }
    }
}
