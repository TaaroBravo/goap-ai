using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyDoor : PickUps
{
    public string doorToOpen;

    public override void PickedUp(Player player)
    {
        WorldInfo.Instance.doorsAndKeys[doorToOpen] = true;
        Destroy(gameObject);
    }
}
