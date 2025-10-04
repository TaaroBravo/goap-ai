using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityView
{
    protected Animator _anim;

    public EntityView(Entity ent, Animator animator)
    {
        _anim = animator;
        
        ent.OnSpeedChange += ChangeSpeed;
        ent.OnAttack += Attack;
        ent.OnTakeDamage += GetHit;
        ent.OnDying += Death;
    }

    public void ChangeSpeed(float speed)
    {
        _anim.SetFloat("speed", speed);
    }

    public void Attack(int index)
    {
        _anim.SetTrigger("FirstCombo");
    }

    public void GetHit()
    {

    }

    public void Death()
    {
        _anim.SetTrigger("Death");
    }
}
