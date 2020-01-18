using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpireMe : MonoBehaviour {

	public GameObject canvasObject;

	void Awake () {
		canvasObject.SetActive (false);
	}

	public void QuitApplication () {
		Debug.Log ("quitting");
		Application.Quit ();
	}

	public void ShowExpireMessage () {
		Debug.Log ("expired!");
		canvasObject.SetActive (true);

		Invoke ("QuitApplication", 10);
	}
}