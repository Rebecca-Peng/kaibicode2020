using UnityEngine;
using System.Collections;

public class EggMover : MonoBehaviour 
{
    void Awake()
    {
        //GetComponent<Rigidbody>().AddForce(new Vector3(0, -10.0f, 0), ForceMode.Force);
    }

	void Update () 
	{
        if (transform.position.y < -2)
        {
            Destroy(gameObject);
        }
	}
}
