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
        public class EventsClass
        {
            public EventBase<LineTurnEventArgs> OnTurn { get; } = new EventBase<LineTurnEventArgs>();
            public EventBase<LineDieEventArgs> OnDie { get; } = new EventBase<LineDieEventArgs>();
            public EventBase<DiamondPickedEventArgs> OnDiamondPicked { get; } = new EventBase<DiamondPickedEventArgs>();
            public EventBase<CrownPickedEventArgs> OnCrownPicked { get; } = new EventBase<CrownPickedEventArgs>();
            public EventBase<SkinChangeEventArgs> OnSkinChange { get; } = new EventBase<SkinChangeEventArgs>();
            public UnityEvent OnExitGround { get; } = new UnityEvent();
            public UnityEvent OnEnterGround { get; } = new UnityEvent();
        }

        private Rigidbody _rigidbody;
        [SerializeField] private Vector3 nextWay;
        [SerializeField] private float speed = 10f;
        [SerializeField][Range(0f, 1f)] private float slopeRange = 0.1f;
        [SerializeField] private bool moving = false;
        [SerializeField] private bool controllable = true;
        [SerializeField] private bool overWhenDie = true;
        private bool died = false;
        private List<GameObject> touchings = new List<GameObject>();
        private bool previousFrameIsGrounded;
        [SerializeField] private LineRespawnAttributes startAttributes;
        private SkinBase[] Skins;
        public List<float> turnTime = new List<float>();
        [SerializeField] private bool auto = false;
        private int autoIndex = 0;
        private Vector3 previousTurnPosition;
        private float previousTurnTime;

        public bool IsGrounded
        {
            get => (touchings.Count != 0 || (_rigidbody.velocity.y >= -0.1f && _rigidbody.velocity.y <= 0.1f && Physics.gravity.y != 0f)) ? true : false;
        }
        public bool Moving
        {
            get => moving;
            set
			{
                moving = value;
				if (value)
				{
                    previousTurnPosition = transform.position;
                    previousTurnTime = Time.time;
                    moving = value;
                }
			}
        }
        public float Speed
		{
            get => speed;
            set
			{
                if (moving)
                {
                    previousTurnPosition = transform.position;
                    previousTurnTime = Time.time;
                    speed = value;
                }
			}
		}
        public bool Auto
		{
            get => auto;
            set
			{
                if (value)
                {
                    autoIndex = 0;
                    for (int i = 0; i < turnTime.Count; i++)
					{
                        if (BGMController.Time > turnTime[i]) { autoIndex = i; }
					}
				}
                auto = value;
			}
		}
        public bool Died { get => died; }
        public EventsClass Events { get; } = new EventsClass();
        public SkinBase Skin { get; private set; }
        public LineRespawnAttributes StartAttributes { get => startAttributes; private set => StartAttributes = value; }
        public Vector3 NextWay { get => nextWay; set => nextWay = value; }
        public bool Controllable { get => controllable; set => controllable = value; }

        public void OnStateChange(GameState newState)
		{
            switch (newState)
            {
                case GameState.Playing:
                    previousTurnPosition = transform.position;
                    previousTurnTime = Time.time;
                    Moving = true;
                    break;
                case GameState.WaitingRespawn:
                case GameState.GameOver:
                    Moving = false;
                    break;
                case GameState.SelectingSkins:
                    Moving = false;
                    Respawn(startAttributes);
                    autoIndex = 0;
                    break;
            }
        }

        private void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
            Skins = SkinManager.InstantiateSkins(this);  // 实例化皮肤
            ChangeSkin(SkinManager.defaultSkin);
            previousFrameIsGrounded = IsGrounded;
            GameController.lines.Add(this);
            startAttributes = new LineRespawnAttributes(this, transform.position, transform.localEulerAngles, nextWay, controllable);
        }
        private void Move()
        {
            Vector3 targetPos = previousTurnPosition + transform.rotation * Vector3.forward * speed * (Time.time - previousTurnTime);
            transform.position = new Vector3(targetPos.x, transform.position.y, targetPos.z);
        }

        public void GoOffset(Vector3 offset)
        {
            transform.position += offset;
            previousTurnPosition += offset;
        }

        private void Update()
        {
            if (Moving)
			{
                Move();
                if (auto && autoIndex < turnTime.Count && turnTime[autoIndex] < Time.time - GameController.StartTime)
				{
                    Turn(true);
                    autoIndex++;
                }
			}
            if (previousFrameIsGrounded != IsGrounded)
            {
                previousFrameIsGrounded = IsGrounded;
                if (IsGrounded)
                {
                    Skin.EndFly();
                    Events.OnEnterGround.Invoke();
                }
                else
                {
                    Skin.StartFly();
                    Events.OnExitGround.Invoke();
                }
            }
            Skin.Update();
        }

        /// <summary>
        /// 使线转弯
        /// </summary>
        /// <param name="focus">是否为强制转弯(即无视controlled, IsGrounded, State)</param>
        public void Turn(bool focus)
        {
            if ((IsGrounded && GameController.State == GameState.Playing && controllable && !died) || focus)
            {
                EventManager.OnLineTurn.Invoke(new LineTurnEventArgs(this, transform.localEulerAngles, nextWay, focus), e1 =>
                {
                    Events.OnTurn.Invoke(e1, e2 =>
                    {
                        if (!e2.canceled)
                        {
                            if (IsGrounded || focus)
                            {
                                Move();
                                (transform.localEulerAngles, nextWay) = (nextWay, transform.localEulerAngles);
                                Skin.Turn(focus);
                                previousTurnPosition = transform.position;
                                previousTurnTime = Time.time;
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
            Skin.Respawn();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag("Obstacle"))
            {
                Die(DeathCause.Obstacle);
            }
            Vector3 contact0Normal = collision.GetContact(0).normal;
            if (contact0Normal.y < 1f - slopeRange || contact0Normal.y > 1f + slopeRange) { return; }  // 最大允许控制坡度
            touchings.Add(collision.gameObject);
        }

        public void Die(DeathCause cause)
		{
            EventManager.OnLineDie.Invoke(new LineDieEventArgs(this, cause), e1 =>
            {
                Events.OnDie.Invoke(e1, (LineDieEventArgs e2) =>
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
                        Skin.Die(e2.cause);
                        if (overWhenDie) { GameController.GameOver(true); }
                    }
                });
            });
        }

        private void OnCollisionExit(Collision collision) => touchings.Remove(collision.gameObject);

        public void ChangeSkin(Type newSkinType)
		{
            if (Skin != null && Skin.GetType() == newSkinType) { return; }
            bool canceled = false;
            if (Skin != null)  // 初始化的时候Skin是null
			{
                EventManager.OnSkinChange.Invoke(new SkinChangeEventArgs(this, Skin.GetType(), newSkinType), e1 =>
                {
                    Events.OnSkinChange.Invoke(e1, e2 =>
                    {
                        canceled = e2.canceled;
                        newSkinType = e2.newSkin;
                    });
                });
            }
            if (!canceled)
			{
                foreach (SkinBase SkinBase in Skins)
                {
                    if (SkinBase.GetType() == newSkinType)
                    {
                        SkinBase.Enable();
                        Skin = SkinBase;
                    }
                    else { Skin.Disable(); }
                }
            }
		}
    }
}
