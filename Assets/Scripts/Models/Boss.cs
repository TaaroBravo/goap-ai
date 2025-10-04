using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public abstract class Boss : Enemy
{

    protected bool _playerFound;
    protected bool _usingSpecialAttack;

    protected bool _specialAttackCooldown;
    protected float _specialAttackCooldownTimer;

    public event Action OnPlayerFound = () => { };
    public abstract event Action OnSpecialAttack;

    protected IEnumerator EnemyFound()
    {
        yield return new WaitUntil(() => _player);
        OnPlayerFound();
    }

    protected IEnumerator SpecialCooldown(float x)
    {
        yield return new WaitUntil(() => _specialAttackCooldown);
        yield return new WaitForSeconds(x);
        _specialAttackCooldown = false;
    }

    public void FoundPlayerFinished()
    {
        _specialAttackCooldown = true;
        _playerFound = true;
    }

    public void DisableSpecialAttack()
    {
        _usingSpecialAttack = false;
    }

}
