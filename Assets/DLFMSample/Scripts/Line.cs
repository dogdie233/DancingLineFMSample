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
            [Serializable]
            public class DieEvent : UnityEvent<DeathCause> { }
            public UnityEvent onTurn;
            public DieEvent onDie;
            public UnityEvent OnExitGround;
            public UnityEvent OnEnterGround;
        }

        public GameObject bodyObject;
        private new Rigidbody rigidbody;
        public float speed;
        public Vector3 nextWay;
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
                GameController.instance.bgButton.onClick.AddListener(() => { Turn(false); });
            else
                GameController.instance.bgButton.onClick.RemoveListener(() => { Turn(false); });
        }

        void Awake()
		{
            rigidbody = GetComponent<Rigidbody>();
            GameController.events.onStart.AddListener(() => { if (_controlled) moving = true;});
        }

		void Start()
        {
            UpdateTurnListener(_controlled);
            bodiesParent = new GameObject("Bodies").transform;
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

        private void CreateBody()
		{
            if (body != null && (transform.localScale.z / 2) < Vector3.Distance(lastTurnPosition, transform.position))
			{
                body.localScale += Vector3.forward * transform.localScale.z / 2;
                body.Translate(Vector3.forward * transform.localScale.z / 4, Space.Self);
            }
            body = Instantiate(bodyObject, transform.position, transform.rotation, bodiesParent).transform;
            lastTurnPosition = transform.position;
        }

        /// <summary>
        /// 使线转弯
        /// </summary>
        /// <param name="focus">强制转弯(即无视controlled, IsGrounded, State)</param>
        public void Turn(bool focus)
		{
            if ((_controlled && IsGrounded && GameController.State == GameState.Playing) || focus)
            {
                (transform.localEulerAngles, nextWay) = (nextWay, transform.localEulerAngles);
                CreateBody();
            }
		}

        /// <summary>
        /// 给爷死
        /// </summary>
        /// <param name="deathCause">怎么死的</param>
        public void Die(DeathCause deathCause)
		{
            switch (deathCause)
			{
                case DeathCause.Obstacle:
                    moving = false;
                    rigidbody.isKinematic = true;
                    break;
			}
            events.onDie.Invoke(deathCause);
		}

        void OnCollisionEnter(Collision collision)
		{
            touchings.Add(collision.gameObject);
            if (collision.gameObject.CompareTag("Obstacle"))
                Die(DeathCause.Obstacle);
		}

        private void OnCollisionExit(Collision collision) { touchings.Remove(collision.gameObject); }
    }
}
