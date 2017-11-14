using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Audio : MonoBehaviour {

	public List<AudioClip> lines = new List<AudioClip>();
	private List<int> lineSelect = new List<int>();
	public bool playStory;
	AudioSource audioSrc;
	public bool gameOver;
	private int eventCount;

	// Use this for initialization
	void Start () {
		playStory = false;
		eventCount = 0;
		gameOver = false;
		audioSrc = GetComponent<AudioSource> ();
		for (int i = 0; i < lines.Count; i++) {
			lineSelect.Add(i);
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (eventCount > 4) {
			gameOver = true;
		}
	}

	private void OnTriggerEnter(Collider other) {
		if (other.tag == "event") {
			//NewNarration narrationScript = other.GetComponent<NewNarration>();
			bool justEntered = other.gameObject.GetComponent<NewNarration> ().neverEntered;
			if (justEntered == true) {
				//Debug.Log ("we haven't entered it so now play the sound");
				eventCount++;
				Debug.Log (eventCount);
				other.gameObject.GetComponent<NewNarration> ().neverEntered = false;
				other.gameObject.GetComponentInChildren<ParticleSystem> ().Stop ();
				//do the random line select here
				//remove the audio parts from the newnarration script
				playStory = true;
				if (playStory == true) {
					if (lineSelect.Count > 0) {
						int lineSelectIndex = Random.Range (0, lineSelect.Count);
						Debug.Log ("line to play: " + lineSelectIndex);
						int randomLine = lineSelect [lineSelectIndex];
						audioSrc.PlayOneShot (lines [randomLine]);

						lineSelect.RemoveAt (lineSelectIndex);
						//                        lineSelect.Remove(lineSelectIndex);

						playStory = false;
					}
				}
			}
		}
	}
}
