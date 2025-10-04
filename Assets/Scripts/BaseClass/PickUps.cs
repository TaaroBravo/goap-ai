using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PickUps : MonoBehaviour {

    public string pickUpName;

    public abstract void PickedUp(Player player);
}
