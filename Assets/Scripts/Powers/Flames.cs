using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flames : MonoBehaviour {

    float speed = 5;
	
	void Update ()
    {
        transform.position += transform.forward * 5 * Time.deltaTime;
	}

    public void SetDir(Vector3 dir)
    {
        transform.forward = dir;
        Destroy(gameObject, 5f);
    }
}
