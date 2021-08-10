using Event;
using Level.Skins;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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
            public EventBase<SkinChangeEventArgs> onSkinChange = new EventBase<SkinChangeEventArgs>();
            public UnityEvent onExitGround;
            public UnityEvent onEnterGround;
        }

        public GameObject bodyObject;
        private new Rigidbody rigidbody;
        public Vector3 nextWay;
        public float speed = 10f;
        [Range(0f, 1f)]
        public float slopeRange = 0.1f;
        private bool moving = false;
        public bool controllable = true;
        public bool overWhenDie = true;
        public EventsClass events;
        [HideInInspector] public bool died = false;
        private List<GameObject> touchings = new List<GameObject>();
        private bool previousFrameIsGrounded;
        [HideInInspector] public LineRespawnAttributes startAttributes;
        public SkinBase skin;
        private SkinBase[] skins;

        public bool IsGrounded
		{
            get => (touchings.Count != 0 || (rigidbody.velocity.y >= -0.1f && rigidbody.velocity.y <= 0.1f && Physics.gravity.y != 0f)) ? true : false;
        }

        public bool Moving
		{
            get => moving;
			set
			{
                if (moving == value) { return; }
                moving = value;
			}
		}

        private void Awake()
		{
            rigidbody = GetComponent<Rigidbody>();
            EventManager.onStateChange.AddListener((StateChangeEventArgs e) =>
            {
                if (e.canceled) { return e; }
				switch (e.newState)
				{
                    case GameState.Playing:
                        Moving = true;
                        break;
                    case GameState.WaitingRespawn:
                    case GameState.GameOver:
                        Moving = false;
                        break;
                    case GameState.SelectingSkins:
                        Moving = false;
                        Respawn(startAttributes);
                        break;
                }
                return e;
            }, Priority.Monitor);
        }

		private void Start()
        {
            skins = SkinManager.InstantiateSkins(this);  // 实例化皮肤
            ChangeSkin(SkinManager.defaultSkin);
            previousFrameIsGrounded = IsGrounded;
            GameController.lines.Add(this);
            startAttributes = new LineRespawnAttributes(this, transform.position, transform.localEulerAngles, nextWay, controllable);
        }

		private void Update()
        {
            if (Moving)
			{
                gameObject.transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);
            }
            if (previousFrameIsGrounded != IsGrounded)
			{
                previousFrameIsGrounded = IsGrounded;
                if (IsGrounded)
				{
                    skin.EndFly();
                    events.onEnterGround.Invoke();
				}
				else
				{
                    skin.StartFly();
                    events.onExitGround.Invoke();
				}
			}
            skin.Update();
        }

        /// <summary>
        /// 使线转弯
        /// </summary>
        /// <param name="focus">是否为强制转弯(即无视controlled, IsGrounded, State)</param>
        public void Turn(bool focus)
		{
            if ((IsGrounded && GameController.State == GameState.Playing && controllable && !died) || focus)
            {
                EventManager.onLineTurn.Invoke(new LineTurnEventArgs(this, transform.localEulerAngles, nextWay, focus), e1 =>
                {
                    events.onTurn.Invoke(e1, e2 =>
                    {
                        if (!e2.canceled)
                        {
                            if (IsGrounded || focus)
                            {
                                (transform.localEulerAngles, nextWay) = (nextWay, transform.localEulerAngles);
                                skin.Turn(focus);
                            }
                        }
                    });
                });
            }
		}

        public void Respawn(LineRespawnAttributes attributes)
		{
            transform.position = attributes.position;
            transform.localEulerAngles = attributes.way;
            nextWay = attributes.nextWay;
            controllable = attributes.controllable;
            died = false;
            skin.Respawn();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Obstacle"))
            {
                EventManager.onLineDie.Invoke(new LineDieEventArgs(this, DeathCause.Obstacle), e1 =>
                {
                    events.onDie.Invoke(e1, (LineDieEventArgs e2) =>
                    {
                        if (!e2.canceled)
                        {
                            died = true;
                            switch (e2.cause)
                            {
                                case DeathCause.Obstacle:
                                    Moving = false;
                                    break;
                                case DeathCause.Air:
                                    break;
                                case DeathCause.Water:
                                    break;
                            }
                            skin.Die(e2.cause);
                            if (overWhenDie) { GameController.GameOver(true); }
                        }
                    });
                });
            }
            if (collision.contacts[0].normal.y < 1f - slopeRange || collision.contacts[0].normal.y > 1f + slopeRange) { return; }  // 最大允许控制坡度
            touchings.Add(collision.gameObject);
        }

        private void OnCollisionExit(Collision collision) => touchings.Remove(collision.gameObject);

        public void ChangeSkin(Type newSkinType)
		{
            if (skin != null && skin.GetType() == newSkinType) { return; }
            bool canceled = false;
            if (skin != null)  // 初始化的时候skin是null
			{
                EventManager.onSkinChange.Invoke(new SkinChangeEventArgs(this, skin.GetType(), newSkinType), e1 =>
                {
                    events.onSkinChange.Invoke(e1, e2 =>
                    {
                        canceled = e2.canceled;
                        newSkinType = e2.newSkin;
                    });
                });
            }
            if (!canceled)
			{
                foreach (SkinBase skinBase in skins)
                {
                    if (skinBase.GetType() == newSkinType)
                    {
                        skinBase.Enable();
                        skin = skinBase;
                    }
                    else { skin.Disable(); }
                }
            }
		}
    }
}
