using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Potion : PickUps
{
    public override void PickedUp(Player player)
    {
        player.potions++;
        Destroy(gameObject);
    }
}
