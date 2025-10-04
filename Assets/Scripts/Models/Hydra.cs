using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hydra : Boss
{

    private BossView _bossView;

    public BoneWhirlwind boneWhirlwindPrefab;
    public SphereCollider spawnArea;

    public override event Action<float> OnSpeedChange;
    public override event Action<int> OnAttack;
    public override event Action Evade = () => { };
    public override event Action OnSpecialAttack = () => { };

    void Awake()
    {
        _life = _maxLife = 100;
        _playerMinDistance = 20;
        _distanceToAttack = 10;
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

    public void SpawnBoneWhirlwind()
    {
        int amount = UnityEngine.Random.Range(12, 18);
        var posY = transform.position.y;
        for (int i = 0; i < amount; i++)
        {
            var flame = GameObject.Instantiate(boneWhirlwindPrefab);

            var randomX = UnityEngine.Random.Range(spawnArea.transform.position.x - spawnArea.radius, spawnArea.transform.position.x + spawnArea.radius);
            var randomZ = UnityEngine.Random.Range(spawnArea.transform.position.z - spawnArea.radius, spawnArea.transform.position.z + spawnArea.radius);
            flame.transform.position = new Vector3(randomX, posY, randomZ);
        }
    }
}
