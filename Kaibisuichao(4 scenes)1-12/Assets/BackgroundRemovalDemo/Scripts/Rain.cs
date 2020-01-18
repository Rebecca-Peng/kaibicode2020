using UnityEngine;
using System.Collections.Generic;
using Random = System.Random;

public class Rain : MonoBehaviour {
    public Transform[] eggPrefab = new Transform[9];

    private float nextEggTime = 0.0f;
    [Range(0.01f, 1f)]
    public float spawnRate = 0.01f;
    [Range(0.1f,4.0f)]
    public float width = 3.0f;
    
    public Vector2 sizeRange;
    float sizescale = 1.0f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M)) {
            sizeRange.y += 0.05f;
        }
        else if (Input.GetKeyDown(KeyCode.N)) { 
            sizeRange.y -= 0.05f;
        }
        else if (Input.GetKeyDown(KeyCode.X)) {
            sizeRange.x += 0.05f;
        }
        else if (Input.GetKeyDown(KeyCode.Z)) {
            sizeRange.x -= 0.05f;
        }
        else if (Input.GetKeyDown(KeyCode.K))
        {
            spawnRate += 0.01f;
        }
        else if (Input.GetKeyDown(KeyCode.J))
        {
            spawnRate -= 0.01f;
        }
        if (nextEggTime < Time.time)
        {
            genNewClone();
            nextEggTime = Time.time + spawnRate;
            spawnRate = Mathf.Clamp(spawnRate, 0.01f, 1f);
        }
    }

    void genNewClone() {
        sizescale = UnityEngine.Random.Range(sizeRange.x,sizeRange.y);
        float addXPos = UnityEngine.Random.Range(-1*width, width);


        Vector3 spawnPos = new Vector3(addXPos, UnityEngine.Random.Range(4.0f,5.0f), 0.0f);
        Transform eggTransform = Instantiate(eggPrefab[UnityEngine.Random.Range(0,eggPrefab.Length-1)], spawnPos, Quaternion.identity) as Transform;
        eggTransform.localScale = eggTransform.localScale * sizescale;
        eggTransform.parent = transform;
    }
}
