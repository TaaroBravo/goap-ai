using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Dragonide : Enemy
{

    public override event Action<float> OnSpeedChange;
    public override event Action<int> OnAttack;
    public override event Action Evade = () => { };

    void Awake()
    {
        _life = _maxLife = 30;
        _playerMinDistance = 10;
        _distanceToAttack = 2;
        _cooldownTimer = 3f;
        _view = new EnemyView(this, GetComponent<Animator>());
        attackCollider = GetComponentInChildren<Skill>().GetComponent<Collider>();
        EndAttack();
        StartCoroutine(WaitToMove(UnityEngine.Random.Range(0, 2)));
        distanceToAttack = 1.5f;
    }

    void Update()
    {
        FindPlayer();
        if (!_player || Dead)
            return;

        MoveTo(_player.transform.position);

        if (_isMoving && !PlayerCloseToAttack())
            OnSpeedChange(1f);

        if (PlayerCloseToAttack())
        {
            OnSpeedChange(0f);
            if (!_cooldown)
            {
                _attacking = true;
                OnAttack(0);
                _cooldown = true;
                StartCoroutine(AttackCoolDown(_cooldownTimer));
            }
        }
    }
}
