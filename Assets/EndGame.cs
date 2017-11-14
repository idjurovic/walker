using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndGame : MonoBehaviour {

	public GameObject levelImage;
	public float gameStartDelay = 5f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnTriggerEnter (Collider other) {
		if (other.tag == "player" || other.tag == "MainCamera") {
			//SceneManager.LoadScene ("main");
			InitGame();
		}
	}

	void InitGame() {
		Debug.Log("doing setup");
		//levelImage = GameObject.Find("LevelImage");     //finds curtain in inspector

		levelImage.SetActive(true);
		levelImage.GetComponent<CanvasRenderer>().SetAlpha(0f);
		levelImage.GetComponent<Image>().CrossFadeAlpha(1f, 3f, false);
		Invoke("HideLevelImage", gameStartDelay);
	}

	private void HideLevelImage() {
		levelImage.SetActive(false);
		Debug.Log("now running HideLevelImage()");
		//levelImage.GetComponent<CanvasRenderer>().SetAlpha(0f);
		SceneManager.LoadScene ("main");
	}
}
