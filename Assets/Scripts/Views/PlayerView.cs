using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerView : EntityView
{
    public PlayerView(Player player, Animator anim) : base(player,anim)
    {
        player.OnEvade += Evade;
        player.OnRangeAttack += LongRangeCombat;
        _anim.GetBehaviour<AttackBehaviour>().owner = player;
        _anim.GetBehaviour<EvadeBehaviour>().owner = player;
    }

    public void UseFirstCombo()
    {
        _anim.SetTrigger("FirstCombo");
    }

    public void Evade()
    {
        _anim.SetTrigger("evade");
    }

    public void LongRangeCombat()
    {
        _anim.SetTrigger("LongRangeCombat");
    }
}
