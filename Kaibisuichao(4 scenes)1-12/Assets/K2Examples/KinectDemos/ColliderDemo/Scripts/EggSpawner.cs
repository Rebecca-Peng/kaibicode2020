using UnityEngine;
//using System;
//using System.Collections;
using System.Collections.Generic;
using Random = System.Random;

public class EggSpawner : MonoBehaviour 
{
	[Tooltip("Index of the player, tracked by this component. 0 means the 1st player, 1 - the 2nd one, 2 - the 3rd one, etc.")]
	public int playerIndex = 0;

    [Tooltip("Prefab (model and components) used to instantiate eggs in the scene.")]
    //public Transform[] eggPrefab = new Transform[9];
    public Transform eggPrefab;

    private float nextEggTime = 0.0f;
    [Range(0.3f,30f)]
    public float spawnRate = 1.0f;
 	
	void Update () 
	{
        if (nextEggTime < Time.time)
        {
            SpawnEgg();
            nextEggTime = Time.time + spawnRate;

            spawnRate = Mathf.Clamp(spawnRate, 0.01f, 10f);
        }
	}

    void SpawnEgg()
    {
		KinectManager manager = KinectManager.Instance;
        int playerCount = manager.GetUsersCount();
        float addXPos = UnityEngine.Random.Range(-2.0f, 2.0f);

        if (playerCount == 0)
        {
            playerIndex = 0;
        }
        else {
            playerIndex = playerCount - 1;
        }
        Debug.Log("PlayerIndex: " + playerCount);
        Debug.Log("Boolean: " + manager.IsUserDetected(playerIndex));

        if (eggPrefab && manager && manager.IsInitialized() && manager.IsUserDetected(playerIndex))
        {
            List<long> userId = new List<long>();
            List<Vector3> posUser = new List<Vector3>();
            List<Vector3> spawnPos = new List<Vector3>();
            List<Transform> eggTransform = new List<Transform>();

            //Debug.Log("detected!");

            for (int i = 0; i < playerCount; i++)
            {
                userId.Add(manager.GetUserIdByIndex(i));
                posUser.Add(manager.GetUserPosition(i));
                spawnPos.Add(new Vector3(addXPos, 5f, posUser[i].z - 0.1f));
                //Debug.Log("Z: " + posUser[i].z);
                eggTransform.Add(Instantiate(eggPrefab, spawnPos[i], Quaternion.identity) as Transform);
                eggTransform[i].parent = transform;
                //Debug.Log(i + ": " + spawnPos[i]);
            }
        }
        else
        {
            Vector3 spawnPos = new Vector3(addXPos,5f,0);
            Transform eggTransform = Instantiate(eggPrefab, spawnPos, Quaternion.identity) as Transform;
            eggTransform.parent = transform;
    }
}

}
