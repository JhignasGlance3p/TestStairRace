using nostra.origami.common;
using nostra.origami.stumble;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

namespace nostra.origami.stumble
{
    public class LHS_MainPlayer : MonoBehaviour, ICharacter
    {
        [SerializeField] Joystick joystick;
        [SerializeField] Button jumpBt;
        [SerializeField] float speed = 10;
        [SerializeField] float rotateSpeed = 5;
        [SerializeField] float jumpPower = 5;
        [SerializeField] bool UseCameraRotation = true;
        [SerializeField] Billboard billboard;
        [SerializeField] ParticleSystem dust;
        [SerializeField] ParticleSystem bounce;
        [SerializeField] AudioSource mysfx;
        [SerializeField] AudioClip jumpfx;
        [SerializeField] AudioClip bouncefx;
        [SerializeField] UIManager UI;
        [SerializeField] LHS_CountdownController Controller;
        [SerializeField] Rigidbody m_rigidbody;
        [SerializeField] NavMeshAgent m_agent;
        [SerializeField] Camera currentCamera;
        [SerializeField] LHS_Camera LHS_Camera;
        [SerializeField] float forwardPower = 5f;
        
        Vector3 startPos;
        bool isJump;
        bool isDie;
        float hAxis;
        float vAxis;
        Vector3 moveVec;
        bool jPress;
        GameObject destPos;
        bool cannotControl;
        Vector3 joystickInput;
        Touch joystickTouch, panningTouch;
        bool slideOnJump = false;

        public bool canMove = false;
        public string playerTag;
        public float bounceForce;
        public bool cannotMove;

        public Animator anim;
        public Rigidbody Rigid => m_rigidbody;

        public void OnLoaded()
        {
            jumpBt.onClick.AddListener(() => 
            { 
                if (!jPress) 
                {
                    jPress = true; 
                }
            });
            startPos = transform.position;
        }
        public void OnFocus(GameObject _character)
        {
            _character.transform.SetParent(transform);
            _character.transform.localPosition = new Vector3(0, 0, 0);
            _character.transform.localRotation = Quaternion.identity;
            _character.transform.localScale = Vector3.one;
            _character.SetActive(true);

            this.transform.position = startPos;
        }
        public void OnAutoPlay()
        {
            canMove = true;
            int randomPoint = UnityEngine.Random.Range(1, 7);
            destPos = UI.AIDistGameObjects[randomPoint];
            isAgentStopped(true);

            Rigid.isKinematic = false;
            if(anim != null) anim.SetBool("isMove", true);
            isAgentStopped(false);
            cannotControl = false;
        }

        void isAgentStopped(bool status)
        {
            if (m_agent.isOnNavMesh)
            {
                m_agent.isStopped = status;
            }
            if (status)
            {
                Deactive();
            }
        }

        private void FixedUpdate()
        {
            if(!canMove)
            {
                return;
            }
            if (cannotControl || UI.RoundEnded)
            {
                transform.LookAt(startPos);
                Rigid.isKinematic = true;
                if(anim != null) anim.SetBool("isMove", false);
                this.enabled = false;
                return;
            }

            FreezeRotation();
            if (Controller.PlayerDemoRun)
            {
                if (destPos != null && m_agent != null)
                {
                    Rigid.isKinematic = true;
                    if(anim != null) anim.SetBool("isMove", true);
                    m_agent.destination = destPos.transform.position;
                }
            }
            else
            {
                GetInput();
                if (!cannotMove)
                {
                    Move();
                    Turn();
                    Jump();
                }
            }
            
            Die();
        }
        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag == "Floor")
            {
                if(anim != null) anim.SetBool("isJump", false);
                if(anim != null) anim.SetBool("isFalling", false);

                isJump = false;
                jPress = false;

                slideOnJump = false;
            }
            else if (collision.gameObject.tag == "Platform")
            {
                if(anim != null) anim.SetBool("isJump", false);
                if(anim != null) anim.SetBool("isFalling", false);

                isJump = false;
                jPress = false;

                slideOnJump = false;
            }
            else if (collision.collider.tag == "Wall" && !cannotMove)
            {
                if(anim != null) anim.SetTrigger("doDie");
                isDie = false;
                cannotMove = true;
                Rigid.linearVelocity = new Vector3(0, 0, 0);
                Rigid.AddForce(Vector3.back * bounceForce, ForceMode.Impulse);

                mysfx.PlayOneShot(bouncefx);
                bounce.Play();

                bounce.transform.position = transform.position;
                MyUtils.Execute(0.5f, () =>
                {
                    cannotMove = false;
                });
            }
        }

        public void resetMoveVec()
        {
            moveVec = Vector3.zero;
            //agent = null;
        }
        public void Deactive()
        {
            Rigid.linearVelocity = Vector3.zero;
            Rigid.isKinematic = true;
            if(anim != null) anim.SetBool("isMove", false);
            cannotControl = true;
        }
        public void Activate()
        {
            //Rigid.velocity = Vector3.zero;
            //Rigid.isKinematic = false;
           // if(anim != null) anim.SetBool("isMove", false);
            //cannotControl = false;
        }

        void FreezeRotation()
        {
            Rigid.angularVelocity = Vector3.zero;
        }
        void GetInput()
        {
            // Vector3 keyboardInput = new(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

            // if (InputManager.touchControls)
            // {
            //     if (Input.touchCount > 0)
            //     {
            //         foreach (Touch touch in Input.touches)
            //         {
            //             if (touch.position.y > Screen.height * 0.4f)
            //             {
            //                 panningTouch = touch;
            //             }
            //             else
            //             {
            //                 joystickTouch = touch;
            //             }
            //         }
            //     }
            // }

            // if (joystickTouch.position.y > Screen.height * 0.4f)
            // {
            //     joystickTouch.phase = TouchPhase.Ended;
            // }
            // if (joystickTouch.phase == TouchPhase.Ended)
            // {
            //     joystickInput = Vector3.zero;
            // }
            // else if(joystickTouch.phase == TouchPhase.Moved)
            // {
            //     joystickInput = new(joystick.Horizontal, 0, joystick.Vertical);
            // }
            // if (panningTouch.phase == TouchPhase.Moved)
            // {
            //     LHS_Camera.X += panningTouch.deltaPosition.x * 0.2f;
            //     LHS_Camera.Y -= panningTouch.deltaPosition.y * 0.2f;
            // }
            // Vector3 activeInput = joystickInput.magnitude >= keyboardInput.magnitude ? joystickInput : keyboardInput;
            // hAxis = activeInput.x;
            // vAxis = activeInput.z;
        }
        void Move()
        {
            moveVec = new Vector3(hAxis, 0, vAxis).normalized;
            if (UseCameraRotation)
            {
                Quaternion v3Rotation = Quaternion.Euler(0f, currentCamera.transform.eulerAngles.y, 0f);
                moveVec = v3Rotation * moveVec;
            }
            Vector3 velocity = moveVec * speed * Time.deltaTime;
            velocity.y = Rigid.linearVelocity.y;

            if (moveVec.magnitude > 0f)
                Rigid.linearVelocity = velocity;
            if(anim != null) anim.SetBool("isMove", moveVec != Vector3.zero);
        }
        void Turn()
        {
            if (hAxis == 0 && vAxis == 0)
                return;
            Quaternion newRotation = Quaternion.LookRotation(moveVec);
            Rigid.rotation = Quaternion.Slerp(Rigid.rotation, newRotation, rotateSpeed * Time.deltaTime);
        }
        void Jump()
        {
            if ((jPress))
            {
                if (!isJump)
                {
                    Rigid.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
                    isJump = true;
                    slideOnJump = true;

                    //if(anim != null) anim.SetBool("isJump", true);
                    if(anim != null) anim.SetTrigger("doJump");
                    mysfx.PlayOneShot(jumpfx);
                    dust.Play();

                    jPress = false;
                }
                else if (slideOnJump)
                {
                    Rigid.AddForce(transform.forward * forwardPower, ForceMode.Impulse);

                    if(anim != null) anim.SetBool("isFalling", true);
                    slideOnJump = false;
                }
            }
        }
        void Die()
        {
            if (isDie)
            {
                if(anim != null) anim.SetTrigger("doDie");
                isDie = true;
            }
        }
    }
}