using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Energy : PickUps
{
    public override void PickedUp(Player player)
    {
        player.RechargeEnergy();
        Destroy(gameObject);
    }
}
