using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallBump : MonoBehaviour {

	public AudioClip bumpSound;
	AudioSource src;
	private bool gameEnd;
	public GameObject player;

	// Use this for initialization
	void Start () {
		src = GetComponent<AudioSource> ();
		gameEnd = false;
	}
	
	// Update is called once per frame
	void Update () {
		gameEnd = player.GetComponent<Audio> ().gameOver;
		if (gameEnd == true) {
			this.gameObject.SetActive (false);
		}
	}

	private void OnTriggerEnter (Collider other) {
		Debug.Log ("bump");
		src.PlayOneShot(bumpSound);
	}
}
