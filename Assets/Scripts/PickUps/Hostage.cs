using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hostage : PickUps
{
    public override void PickedUp(Player player)
    {
        WorldInfo.Instance.hostageSaved++;
        Destroy(gameObject);
    }
}
