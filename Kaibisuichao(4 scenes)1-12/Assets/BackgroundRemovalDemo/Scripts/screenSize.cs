using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class screenSize : MonoBehaviour {
    public int width = 704;
    public int height = 528;
    bool isfullscreen;

	// Use this for initialization
	void Start () {
        isfullscreen = true;
        Screen.SetResolution(width, height, isfullscreen);
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKey(KeyCode.Escape)) {
            Application.Quit();

        }
        if (Input.GetKey(KeyCode.K)) {
            isfullscreen = false;
            Screen.SetResolution(width, height, isfullscreen);
        }
       
    }

}
