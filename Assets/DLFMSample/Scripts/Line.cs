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
            public UnityEvent OnExitGround;
            public UnityEvent OnEnterGround;
        }

        public GameObject bodyObject;
        private new Rigidbody rigidbody;
        public float speed;
        public Vector3 nextWay;
        public bool overWhenDie = true;
        public ParticlesGroup[] particles;
        [HideInInspector] public bool moving = false;
        [HideInInspector] public bool _controlled = true;
        public EventsClass events;
        private List<GameObject> touchings = new List<GameObject>();
        private Transform bodiesParent;
        private Transform body;
        private Vector3 lastTurnPosition;
        private bool previousFrameIsGrounded;

        public bool IsControlled
		{
			get
			{
                return _controlled;
			}
			set
            {
                if (value != _controlled)
				{
                    UpdateTurnListener(value);
                    _controlled = value;
                }
            }
		}

        public bool IsGrounded
		{
			get
			{
                if (touchings.Count == 0)
                    return false;
                return true;
			}
		}

        private void UpdateTurnListener(bool value)
		{
            if (value)
                if (GameController.IsStarted) { GameController.instance.bgButton.onClick.AddListener(() => { Turn(false); }); }
			else
                if (GameController.IsStarted) { GameController.instance.bgButton.onClick.RemoveListener(() => { Turn(false); }); }
        }

        void Awake()
		{
            rigidbody = GetComponent<Rigidbody>();
            EventManager.onStateChange.AddListener((StateChangeEventArgs e) => {
                if (e.newState == GameState.Playing && !e.canceled)
				{
                    moving = true;
                    if (_controlled) { GameController.instance.bgButton.onClick.AddListener(() => { Turn(false); }); }
                }
                return e;
            }, Priority.Lowest);
            events.onDie.AddListener(OnDie, Priority.Lowest);
            events.onTurn.AddListener(OnTurn, Priority.Lowest);
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
            events.onTurn.Invoke(new LineTurnEventArgs(this, transform.localEulerAngles, nextWay, focus));
		}

        public LineDieEventArgs OnDie(LineDieEventArgs e)
		{
            if (!e.canceled)
			{
                switch (e.cause)
                {
                    case DeathCause.Obstacle:
                        moving = false;
                        rigidbody.isKinematic = true;
                        break;
                }
                if (overWhenDie) { GameController.State = GameState.WaitingRespawn; }
            }
            return e;
		}

        public LineTurnEventArgs OnTurn(LineTurnEventArgs e)
		{
            if (!e.canceled)
			{
                if ((IsGrounded && GameController.State == GameState.Playing) || e.foucs)
                {
                    (transform.localEulerAngles, nextWay) = (nextWay, transform.localEulerAngles);
                    CreateBody();
                }
            }
            return e;
		}

        void OnCollisionEnter(Collision collision)
		{
            touchings.Add(collision.gameObject);
            if (collision.gameObject.CompareTag("Obstacle")) { events.onDie.Invoke(new LineDieEventArgs(this, DeathCause.Obstacle)); }
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
