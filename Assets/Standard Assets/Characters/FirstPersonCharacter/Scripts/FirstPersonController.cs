using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility;
using Random = UnityEngine.Random;
using System.Collections.Generic;

namespace UnityStandardAssets.Characters.FirstPerson
{
    [RequireComponent(typeof (CharacterController))]
    [RequireComponent(typeof (AudioSource))]
    public class FirstPersonController : MonoBehaviour
    {
        [SerializeField] private bool m_IsWalking;
        [SerializeField] private float m_WalkSpeed;
        [SerializeField] private float m_RunSpeed;
        [SerializeField] [Range(0f, 1f)] private float m_RunstepLenghten;
        [SerializeField] private float m_JumpSpeed;
        [SerializeField] private float m_StickToGroundForce;
        [SerializeField] private float m_GravityMultiplier;
        [SerializeField] private MouseLook m_MouseLook;
        [SerializeField] private bool m_UseFovKick;
        [SerializeField] private FOVKick m_FovKick = new FOVKick();
        [SerializeField] private bool m_UseHeadBob;
        [SerializeField] private CurveControlledBob m_HeadBob = new CurveControlledBob();
        [SerializeField] private LerpControlledBob m_JumpBob = new LerpControlledBob();
        [SerializeField] private float m_StepInterval;
        [SerializeField] private AudioClip[] m_FootstepSounds;    // an array of footstep sounds that will be randomly selected from.
        [SerializeField] private AudioClip m_JumpSound;           // the sound played when character leaves the ground.
        [SerializeField] private AudioClip m_LandSound;           // the sound played when character touches back on ground.

        private Camera m_Camera;
        private bool m_Jump;
        private float m_YRotation;
        private Vector2 m_Input;
        private Vector3 m_MoveDir = Vector3.zero;
        private CharacterController m_CharacterController;
        private CollisionFlags m_CollisionFlags;
        private bool m_PreviouslyGrounded;
        private Vector3 m_OriginalCameraPosition;
        private float m_StepCycle;
        private float m_NextStep;
        private bool m_Jumping;
        private AudioSource m_AudioSource;

        ////NEW CODE
        public List<AudioClip> lines = new List<AudioClip>();
        private List<int> lineSelect = new List<int>();
        public bool playStory;
        //private int randomLine;
        //public bool playStory;  //should we play the narration?
        //private int counter;    //how many points of interest has the player interacted with?
        //public int totalPoints;  //how many points of interest are there?
        //private bool gameOver;
        //public GameObject finalEventObj;    //reference to the final event object, so we can get it to play particles when game is over
        //private bool[] neverEntered = new bool[2];  //have you entered this tree before m8

        //[Header("Story Lines")]  //add a header to this section
        //[TextArea(2, 10)]  //display the list of strings that follows as text areas, with a min or 2 lines & a max of 10 lines for the text
        //public List<string> captions;  //Lists for closed captioning

        //public AudioClip[] lines;

        // Use this for initialization
        private void Start()
        {
            m_CharacterController = GetComponent<CharacterController>();
            m_Camera = Camera.main;
            m_OriginalCameraPosition = m_Camera.transform.localPosition;
            m_FovKick.Setup(m_Camera);
            m_HeadBob.Setup(m_Camera, m_StepInterval);
            m_StepCycle = 0f;
            m_NextStep = m_StepCycle/2f;
            m_Jumping = false;
            m_AudioSource = GetComponent<AudioSource>();
			m_MouseLook.Init(transform , m_Camera.transform);

            ////NEW CODE
            //playStory = false;
            //counter = 0;
            //totalPoints = 2;
            //gameOver = false;
            //for (int i = 0; i < totalPoints; i++) {
            //    neverEntered[i] = true;
            //}
            playStory = false;
            for (int i = 0; i < lines.Count; i++) {
                lineSelect.Add(i);
            }
        }


        // Update is called once per frame
        private void Update()
        {
            RotateView();
            // the jump state needs to read here to make sure it is not missed
            if (!m_Jump)
            {
                m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
            }

            if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
            {
                StartCoroutine(m_JumpBob.DoBobCycle());
                PlayLandingSound();
                m_MoveDir.y = 0f;
                m_Jumping = false;
            }
            if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
            {
                m_MoveDir.y = 0f;
            }

            m_PreviouslyGrounded = m_CharacterController.isGrounded;
        }


        private void PlayLandingSound()
        {
            m_AudioSource.clip = m_LandSound;
            m_AudioSource.Play();
            m_NextStep = m_StepCycle + .5f;
        }


        private void FixedUpdate()
        {
            float speed;
            GetInput(out speed);
            // always move along the camera forward as it is the direction that it being aimed at
            Vector3 desiredMove = transform.forward*m_Input.y + transform.right*m_Input.x;

            // get a normal for the surface that is being touched to move along it
            RaycastHit hitInfo;
            Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
                               m_CharacterController.height/2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

            m_MoveDir.x = desiredMove.x*speed;
            m_MoveDir.z = desiredMove.z*speed;


            if (m_CharacterController.isGrounded)
            {
                m_MoveDir.y = -m_StickToGroundForce;

                if (m_Jump)
                {
                    m_MoveDir.y = m_JumpSpeed;
                    PlayJumpSound();
                    m_Jump = false;
                    m_Jumping = true;
                }
            }
            else
            {
                m_MoveDir += Physics.gravity*m_GravityMultiplier*Time.fixedDeltaTime;
            }
            m_CollisionFlags = m_CharacterController.Move(m_MoveDir*Time.fixedDeltaTime);

            ProgressStepCycle(speed);
            UpdateCameraPosition(speed);

            m_MouseLook.UpdateCursorLock();
        }


        private void PlayJumpSound()
        {
            m_AudioSource.clip = m_JumpSound;
            m_AudioSource.Play();
        }


        private void ProgressStepCycle(float speed)
        {
            if (m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0))
            {
                m_StepCycle += (m_CharacterController.velocity.magnitude + (speed*(m_IsWalking ? 1f : m_RunstepLenghten)))*
                             Time.fixedDeltaTime;
            }

            if (!(m_StepCycle > m_NextStep))
            {
                return;
            }

            m_NextStep = m_StepCycle + m_StepInterval;

            PlayFootStepAudio();
        }


        private void PlayFootStepAudio()
        {
            if (!m_CharacterController.isGrounded)
            {
                return;
            }
            // pick & play a random footstep sound from the array,
            // excluding sound at index 0
            int n = Random.Range(1, m_FootstepSounds.Length);
            m_AudioSource.clip = m_FootstepSounds[n];
            m_AudioSource.PlayOneShot(m_AudioSource.clip);
            // move picked sound to index 0 so it's not picked next time
            m_FootstepSounds[n] = m_FootstepSounds[0];
            m_FootstepSounds[0] = m_AudioSource.clip;
        }


        private void UpdateCameraPosition(float speed)
        {
            Vector3 newCameraPosition;
            if (!m_UseHeadBob)
            {
                return;
            }
            if (m_CharacterController.velocity.magnitude > 0 && m_CharacterController.isGrounded)
            {
                m_Camera.transform.localPosition =
                    m_HeadBob.DoHeadBob(m_CharacterController.velocity.magnitude +
                                      (speed*(m_IsWalking ? 1f : m_RunstepLenghten)));
                newCameraPosition = m_Camera.transform.localPosition;
                newCameraPosition.y = m_Camera.transform.localPosition.y - m_JumpBob.Offset();
            }
            else
            {
                newCameraPosition = m_Camera.transform.localPosition;
                newCameraPosition.y = m_OriginalCameraPosition.y - m_JumpBob.Offset();
            }
            m_Camera.transform.localPosition = newCameraPosition;
        }


        private void GetInput(out float speed)
        {
            // Read input
            float horizontal = CrossPlatformInputManager.GetAxis("Horizontal");
            float vertical = CrossPlatformInputManager.GetAxis("Vertical");

            bool waswalking = m_IsWalking;

#if !MOBILE_INPUT
            // On standalone builds, walk/run speed is modified by a key press.
            // keep track of whether or not the character is walking or running
            m_IsWalking = !Input.GetKey(KeyCode.LeftShift);
#endif
            // set the desired speed to be walking or running
            speed = m_IsWalking ? m_WalkSpeed : m_RunSpeed;
            m_Input = new Vector2(horizontal, vertical);

            // normalize input if it exceeds 1 in combined length:
            if (m_Input.sqrMagnitude > 1)
            {
                m_Input.Normalize();
            }

            // handle speed change to give an fov kick
            // only if the player is going to a run, is running and the fovkick is to be used
            if (m_IsWalking != waswalking && m_UseFovKick && m_CharacterController.velocity.sqrMagnitude > 0)
            {
                StopAllCoroutines();
                StartCoroutine(!m_IsWalking ? m_FovKick.FOVKickUp() : m_FovKick.FOVKickDown());
            }
        }


        private void RotateView()
        {
            m_MouseLook.LookRotation (transform, m_Camera.transform);
        }


        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Rigidbody body = hit.collider.attachedRigidbody;
            //dont move the rigidbody if the character is on top of it
            if (m_CollisionFlags == CollisionFlags.Below)
            {
                return;
            }

            if (body == null || body.isKinematic)
            {
                return;
            }
            body.AddForceAtPosition(m_CharacterController.velocity*0.1f, hit.point, ForceMode.Impulse);
        }

        //NEW CODE

        //private void OnTriggerEnter(Collider other) {
        //    if (other.tag == "event") {
        //        NewNarration narrationScript = other.GetComponent<NewNarration>();
        //        bool justEntered = other.GetComponent<NewNarration>().neverEntered;
        //        if (justEntered == true) {
        //            //do the random line select here
        //            //remove the audio parts from the newnarration script
        //            //playStory here?
        //            int randomLine = Random.Range(0, lineSelect.Count);
        //            Debug.Log(randomLine);
        //            lineSelect.RemoveAt(randomLine);
        //            m_AudioSource.PlayOneShot(lines[randomLine]);
        //        }
        //    }
        //}

        private void OnTriggerEnter(Collider other) {
            if (other.tag == "event") {
                //NewNarration narrationScript = other.GetComponent<NewNarration>();
                bool justEntered = other.GetComponent<NewNarration>().neverEntered;
                if (justEntered == false) {
                    //do the random line select here
                    //remove the audio parts from the newnarration script
                    playStory = true;
                    if (playStory == true) {
                        int lineSelectIndex = Random.Range(0, lineSelect.Count);
                        Debug.Log(lineSelectIndex);
                        int randomLine = lineSelect[lineSelectIndex];
                        m_AudioSource.PlayOneShot(lines[randomLine]);

                        lineSelect.RemoveAt(lineSelectIndex);
                        lineSelect.Remove(lineSelectIndex);

                        playStory = false;
                    }
                }
            }
            //    if (other.tag == "event") {
            //        ParticleSystem parti = other.GetComponentInChildren<ParticleSystem>();
            //        if (neverEntered[counter] == true) {
            //            Debug.Log("entered");
            //            neverEntered[counter] = false;
            //            counter++;
            //            Debug.Log(neverEntered[counter]);
            //            playStory = true;   //SOUND

            //            if (parti.isPlaying) {
            //                parti.Stop();
            //                Debug.Log("parti OVER");
            //            }

            //            if (playStory == true) {
            //                int randomLine = Random.Range(0, lines.Length);
            //                //Debug.Log(randomLine);
            //                m_AudioSource.PlayOneShot(lines[randomLine]);
            //                //Debug.Log("sound would be playing rn i promise fam");
            //                playStory = false;
            //            }
            //        }

            //        if (counter == totalPoints) {
            //            //put an IEnumerator here to count down to game over?
            //            Debug.Log("the game is over fam sry u had to ifnd out this way");
            //            gameOver = true;
            //            ParticleSystem lastParti = finalEventObj.GetComponentInChildren<ParticleSystem>();
            //            if (lastParti.isStopped) {
            //                lastParti.Play();
            //            }
            //        }
            //    }
            //    if (other.tag == "finalEvent") {
            //        if (gameOver == true) {
            //            //do a fancy game over thing here I guess
            //            Debug.Log("you did it!");
            //            //run the main scene again to replay
            //        }
            //    }
        }
    }
}
