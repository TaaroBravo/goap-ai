using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneWhirlwind : MonoBehaviour
{

    private void Start()
    {
        StartCoroutine(StopFlames());
    }

    IEnumerator StopFlames()
    {
        while(true)
        {
            yield return new WaitForSeconds(5f);
            GetComponent<Collider>().enabled = false;
            GetComponent<Animator>().SetTrigger("EndFlames");
            break;
        }
    }

    public void DestroyGameObject()
    {
        Destroy(gameObject);
    }
}
