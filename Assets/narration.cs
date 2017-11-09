using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class narration : MonoBehaviour {

    public bool playStory;  //should we play the narration?
    private int counter;    //how many points of interest has the player interacted with?
    public int totalPoints;  //how many points of interest are there?
    private bool gameOver;
    public GameObject finalEventObj;    //reference to the final event object, so we can get it to play particles when game is over
    //private bool[] neverEntered = new bool[2];  //have you entered this tree before m8
    private bool neverEntered;
    AudioSource audioSrc;

    [Header("Story Lines")]  //add a header to this section
    [TextArea(2, 10)]  //display the list of strings that follows as text areas, with a min or 2 lines & a max of 10 lines for the text
    public List<string> captions;  //Lists for closed captioning

    public AudioClip[] lines;

    // Use this for initialization
    void Start () {
        playStory = false;
        counter = 0;
        totalPoints = 2;
        gameOver = false;
        neverEntered = true;
        audioSrc = gameObject.GetComponent<AudioSource>();
        //for (int i = 0; i < totalPoints; i++) {
        //    neverEntered[i] = true;
        //}
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter(Collider other) {
        if (other.tag == "Player") {
            Debug.Log("Detected player entry on " + gameObject.name);

            ParticleSystem parti = other.GetComponentInChildren<ParticleSystem>();
            if (neverEntered) {
                Debug.Log("entered");
                neverEntered = false;
                counter++;
                Debug.Log(neverEntered);
                playStory = true;   //SOUND

                if (parti.isPlaying) {
                    parti.Stop();
                    Debug.Log("parti OVER");
                }

                if (playStory == true) {
                    int randomLine = Random.Range(0, lines.Length);
                    //Debug.Log(randomLine);
                    audioSrc.PlayOneShot(lines[randomLine]);
                    //Debug.Log("sound would be playing rn i promise fam");
                    playStory = false;
                }
            }

            if (counter == totalPoints) {
                //put an IEnumerator here to count down to game over?
                Debug.Log("the game is over fam sry u had to ifnd out this way");
                gameOver = true;
                ParticleSystem lastParti = finalEventObj.GetComponentInChildren<ParticleSystem>();
                if (lastParti.isStopped) {
                    lastParti.Play();
                }
            }
        }
        if (other.tag == "finalEvent") {
            if (gameOver == true) {
                //do a fancy game over thing here I guess
                Debug.Log("you did it!");
                //run the main scene again to replay
            }
        }
    }
}
