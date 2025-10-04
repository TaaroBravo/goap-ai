using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossView : EntityView
{

    public BossView(Boss boss, Animator anim) : base(boss, anim)
    {
        boss.OnCooldown += UseCooldown;
        boss.OnPlayerFound += FoundEnemy;
        boss.OnSpecialAttack += UseSpecialAttack;
        boss.OnPlayerFound += FoundEnemy;
        boss.OnCooldownMove += UseWalkCombatCooldown;
        _anim.GetBehaviour<AttackBehaviour>().owner = boss;
    }

    public void FoundEnemy()
    {
        _anim.SetTrigger("PlayerFound");
    }

    public void UseSpecialAttack()
    {
        _anim.SetTrigger("SpecialAttack");
    }
    public void UseWalkCombatCooldown(float idx)
    {
        
    }

    public void UseCooldown(bool state)
    {
        _anim.SetBool("cooldown", state);
    }
}
