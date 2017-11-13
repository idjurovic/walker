using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewNarration : MonoBehaviour {

    public bool playStory;  //should we play the narration?
    private int counter;    //how many points of interest has the player interacted with?
    private int randomLine;
    private int lineSelectIndex;
    public int totalPoints;  //how many points of interest are there?
    private bool gameOver;
    public GameObject finalEventObj;    //reference to the final event object, so we can get it to play particles when game is over
    //private bool[] neverEntered = new bool[2];  //have you entered this tree before m8
    public bool neverEntered;
    AudioSource audioSrc;
    //private List<AudioClip> lineSelect = new List<AudioClip>(); //randomly select a story line to play
    private List<int> playedLines = new List<int>();    //lines that have already played

    [Header("Story Lines")]  //add a header to this section
    [TextArea(2, 10)]  //display the list of strings that follows as text areas, with a min or 2 lines & a max of 10 lines for the text
    public List<string> captions;  //Lists for closed captioning

    public List<AudioClip> lines = new List<AudioClip>();

    // Use this for initialization
    void Start() {
        playStory = false;
        counter = 0;
        totalPoints = 6;
        gameOver = false;
        neverEntered = true;
        audioSrc = gameObject.GetComponent<AudioSource>();

        //for (int i = 0; i < lines.Length; i++) {
        //    lineSelect.Add(lines[i]);
        //}
    }

    // Update is called once per frame
    void Update() {

    }

    private void OnTriggerEnter(Collider other) {
        if (other.tag == "Player") {
            //Debug.Log("Detected player entry on " + gameObject.name);

            ParticleSystem parti = gameObject.GetComponentInChildren<ParticleSystem>();
            if (neverEntered) {
                //Debug.Log("entered");
                neverEntered = false;
                counter++;
                playStory = true;   //SOUND

                if (parti.isPlaying) {
                    parti.Stop();
                    //Debug.Log("parti OVER");
                }

                if (playStory == true) {
                    lineSelectIndex = Random.Range(0, lines.Count); //get a random line from the list
                    Debug.Log(lineSelectIndex);
                    //randomLine = lineSelect[lineSelectIndex];

                    lines.Remove(lines[lineSelectIndex]);
                    audioSrc.PlayOneShot(lines[lineSelectIndex]);
                    //lineSelect.Remove(randomLine);

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
