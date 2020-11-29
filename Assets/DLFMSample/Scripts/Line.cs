using Event;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Level
{
    public class Line : MonoBehaviour
    {
        [Serializable]
        public class EventsClass
        {
            public EventBase<LineTurnEventArgs> onTurn = new EventBase<LineTurnEventArgs>();
            public EventBase<LineDieEventArgs> onDie = new EventBase<LineDieEventArgs>();
            public EventBase<DiamondPickedEventArgs> onDiamondPicked = new EventBase<DiamondPickedEventArgs>();
            public EventBase<CrownPickedEventArgs> onCrownPicked = new EventBase<CrownPickedEventArgs>();
            public UnityEvent OnExitGround;
            public UnityEvent OnEnterGround;
        }

        public GameObject bodyObject;
        private new Rigidbody rigidbody;
        public float speed = 10f;
        public Vector3 nextWay;
        public bool overWhenDie = true;
        [Range(0f, 1f)]
        public float slopeRange = 0.1f;
        public ParticlesGroup[] particles;
        [HideInInspector] public bool moving = false;
        public bool controllable = true;
        public EventsClass events;
        [HideInInspector] public bool died = false;
        private List<GameObject> touchings = new List<GameObject>();
        private Transform bodiesParent;
        private Transform body;
        private Vector3 lastTurnPosition;
        private bool previousFrameIsGrounded;

        public bool IsGrounded
		{
			get
			{
                if (touchings.Count == 0)
                    return false;
                return true;
			}
		}

        void Awake()
		{
            rigidbody = GetComponent<Rigidbody>();
            EventManager.onStateChange.AddListener((StateChangeEventArgs e) => {
                if (e.canceled) { return e; }
				switch (e.newState)
				{
                    case GameState.Playing:
                        moving = true;
                        break;
                    case GameState.WaitingRespawn:
                        moving = false;
                        break;
                }
                return e;
            }, Priority.Lowest);
        }

		void Start()
        {
            previousFrameIsGrounded = IsGrounded;
        }

		void Update()
        {
            if (moving)
            {
                transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);
                if (body != null)
                {
                    body.position = Vector3.Lerp(lastTurnPosition, transform.position, 0.5f);
                    body.localScale = new Vector3(body.localScale.x, body.localScale.y, Vector3.Distance(lastTurnPosition, transform.position));
                }
            }
            if (previousFrameIsGrounded != IsGrounded)
			{
                previousFrameIsGrounded = IsGrounded;
                if (IsGrounded)
				{
                    CreateBody();
                    events.OnEnterGround.Invoke();
				}
				else
				{
                    if (body != null)  //下落线身与地板对齐
                    {
                        body.localScale -= Vector3.forward * GetComponent<BoxCollider>().size.z;
                        body.Translate(Vector3.back * GetComponent<BoxCollider>().size.z / 2, Space.Self);
                        body = null;
                    }
                    events.OnExitGround.Invoke();
				}
			}
        }

        /// <summary>
        /// 使线转弯
        /// </summary>
        /// <param name="focus">强制转弯(即无视controlled, IsGrounded, State)</param>
        public void Turn(bool focus)
		{
            if ((IsGrounded && GameController.State == GameState.Playing && controllable && !died) || focus)
            {
                events.onTurn.Invoke(new LineTurnEventArgs(this, transform.localEulerAngles, nextWay, focus), (LineTurnEventArgs e) => {
                    if (!e.canceled)
                    {
                        (transform.localEulerAngles, nextWay) = (nextWay, transform.localEulerAngles);
                        CreateBody();
                    }
                });
            }
		}

        public void Respawn(LineRespawnAttributes attributes)
		{
            transform.position = attributes.positon;
            transform.localEulerAngles = attributes.way;
            nextWay = attributes.nextWay;
            rigidbody.isKinematic = false;
            died = false;
            Destroy(bodiesParent.gameObject);
        }

        void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Obstacle"))
            {
                events.onDie.Invoke(new LineDieEventArgs(this, DeathCause.Obstacle), (LineDieEventArgs e) => {
                    if (!e.canceled)
                    {
                        switch (e.cause)
                        {
                            case DeathCause.Obstacle:
                                moving = false;
                                rigidbody.isKinematic = true;
                                break;
                        }
                        died = true;
                        if (overWhenDie) { GameController.GameOver(); }
                    }
                });
            }
            if (collision.contacts[0].normal.y < 1f - slopeRange || collision.contacts[0].normal.y > 1f + slopeRange) { return; }
            touchings.Add(collision.gameObject);
        }

        private void OnCollisionExit(Collision collision) { touchings.Remove(collision.gameObject); }

        private void CreateBody()
        {
            if (bodiesParent == null) { bodiesParent = new GameObject("Bodies").transform; }
            if (body != null && (transform.localScale.z / 2) < Vector3.Distance(lastTurnPosition, transform.position))
            {
                body.localScale += Vector3.forward * transform.localScale.z / 2;
                body.Translate(Vector3.forward * transform.localScale.z / 4, Space.Self);
            }
            body = Instantiate(bodyObject, transform.position, transform.rotation, bodiesParent).transform;
            lastTurnPosition = transform.position;
        }
    }
}
