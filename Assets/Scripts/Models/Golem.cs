using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Golem : Boss
{

    private BossView _bossView;

    public Rock rockPrefab;
    public SphereCollider spawnArea;

    public override event Action<float> OnSpeedChange;
    public override event Action<int> OnAttack;
    public override event Action Evade = () => { };
    public override event Action OnSpecialAttack = () => { };

    void Awake()
    {
        _life = _maxLife = 100;
        _playerMinDistance = 20;
        _distanceToAttack = 5;
        _cooldownTimer = 3f;
        _specialAttackCooldownTimer = 15f;
        _bossView = new BossView(this, GetComponent<Animator>());
        attackCollider = GetComponentInChildren<Skill>().GetComponent<Collider>();
        EndAttack();
        StartCoroutine(WaitToMove(0f));
        StartCoroutine(EnemyFound());
    }

    void Update()
    {
        FindPlayer();

        if (!_playerFound || _usingSpecialAttack)
            return;

        MoveTo(_player.transform.position);

        if (!_specialAttackCooldown)
        {
            _usingSpecialAttack = true;
            _specialAttackCooldown = true;
            OnSpecialAttack();
            StartCoroutine(SpecialCooldown(_specialAttackCooldownTimer));
        }
        else if (_isMoving && !PlayerCloseToAttack())
            OnSpeedChange(0.5f);
        else if (PlayerCloseToAttack())
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

    public void SpawnRocks()
    {
        int amount = UnityEngine.Random.Range(3, 6);
        var radius = spawnArea.radius;

        for (int i = 0; i < amount; i++)
        {
            var rock = GameObject.Instantiate(rockPrefab);

            float angle = i * Mathf.PI / 8;
            Vector3 newPos = new Vector3(transform.position.x - Mathf.Cos(angle) * radius, transform.position.y - rock.GetComponent<Collider>().bounds.extents.y, transform.position.z - Mathf.Sin(angle) * radius);

            rock.transform.position = newPos;
        }
    }
}
