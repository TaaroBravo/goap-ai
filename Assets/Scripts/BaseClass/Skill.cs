using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill : MonoBehaviour {

    public int dmg;    

    private void OnTriggerEnter(Collider other)
    {
        var temp = other.GetComponent<Entity>();
        if (temp)
            temp.TakeDamage(dmg);
    }
}
