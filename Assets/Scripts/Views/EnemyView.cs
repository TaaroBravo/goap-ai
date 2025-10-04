using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyView : EntityView {

    public EnemyView(Enemy enemy, Animator anim) : base(enemy, anim)
    {
        enemy.OnCooldown += UseCooldown;
        enemy.OnCooldownMove += UseWalkCombatCooldown;
        _anim.GetBehaviour<AttackBehaviour>().owner = enemy;
    }

    public void UseFirstAttack()
    {
        _anim.SetTrigger("FirstCombo");
    }

    public void UseCooldown(bool state)
    {
        _anim.SetBool("cooldown", state);
    }

    public void UseWalkCombatCooldown(float idx)
    {
        _anim.SetFloat("sideSpeed", idx);
    }
}
