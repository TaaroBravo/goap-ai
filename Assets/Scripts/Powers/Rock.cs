using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rock : MonoBehaviour {

    Vector3 pointA;
    Vector3 pointB;
    float duration;

	void Start ()
    {
        pointA = transform.position;
        pointB = new Vector3(transform.position.x, transform.position.y + GetComponent<Collider>().bounds.extents.y * 2, transform.position.z);
        duration = 0.3f;
        StartCoroutine(Movement());
	}

    IEnumerator Movement()
    {
        yield return MoveToPoint(pointA, pointB, duration);
        yield return new WaitForSeconds(3f);
        yield return MoveToPoint(pointB, pointA, duration);
        Destroy(gameObject);
    }

    IEnumerator MoveToPoint(Vector3 a, Vector3 b, float time)
    {
        float i = 0f;
        float rate = (1f / time);
        while (i < 1)
        {
            i += Time.deltaTime * rate;
            transform.position = Vector3.Lerp(a, b, i);
            yield return null;
        }
    }

    void DestroyGameObject()
    {
        Destroy(gameObject);
    }
}
