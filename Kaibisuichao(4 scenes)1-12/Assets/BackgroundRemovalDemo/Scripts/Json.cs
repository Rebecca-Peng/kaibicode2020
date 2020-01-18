using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Json : MonoBehaviour {

    // Use this for initialization
    public GameObject Rain;
    float spawnRate;
    Vector2 range;
    SaveInputData last = new SaveInputData();
    SaveInputData current = new SaveInputData();
    string jsonpath;

	void Start () {
        jsonpath = Application.dataPath + "/range.json";
        Debug.Log(Application.dataPath);
        if (File.Exists(jsonpath)) {
            string contents = File.ReadAllText(jsonpath);
            last = JsonUtility.FromJson<SaveInputData>(contents);
            spawnRate = last.spawnRate;
            range = last.range;
            Rain.GetComponent<Rain>().sizeRange = range;
            Rain.GetComponent<Rain>().spawnRate = spawnRate;
        }
	}
	
	// Update is called once per frame
	void Update () {
//        if (Input.GetKeyDown(KeyCode.S)) {
//            Debug.Log("saved");
//            current.range = Rain.transform.GetComponent<Rain>().sizeRange;
//            current.spawnRate = Rain.transform.GetComponent<Rain>().spawnRate;
//            string json = JsonUtility.ToJson(current,true);
//            System.IO.File.WriteAllText(jsonpath,json);
//            Debug.Log("saved" + "current range: " + current.range);
//        }
        if (Input.GetKeyDown(KeyCode.Escape)) {
            //Debug.Log("saved");
            current.range = Rain.GetComponent<Rain>().sizeRange;
            current.spawnRate = Rain.GetComponent<Rain>().spawnRate;
            string json = JsonUtility.ToJson(current,true);
            System.IO.File.WriteAllText(jsonpath,json);
            Debug.Log("saved");
        }
	}
}
