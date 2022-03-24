using Event;
using Level.Skins;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Level
{
    [Serializable]
    public class LineRespawnAttributes
    {
        public Line line;
        public Vector3 position;
        public Vector3 way;
        public Vector3 nextWay;
        public bool controllable;

        public LineRespawnAttributes(Line line, Vector3 position, Vector3 way, Vector3 nextWay, bool controllable)
        {
            this.line = line;
            this.position = position;
            this.way = way;
            this.nextWay = nextWay;
            this.controllable = controllable;
        }

        public LineRespawnAttributes() { }
    }

    public class Line : MonoBehaviour
    {
        public class EventsClass
        {
            public EventPipeline<LineTurnEventArgs> OnTurn { get; } = new EventPipeline<LineTurnEventArgs>();
            public EventPipeline<LineDieEventArgs> OnDie { get; } = new EventPipeline<LineDieEventArgs>();
            public EventPipeline<DiamondPickEventArgs> OnDiamondPicked { get; } = new EventPipeline<DiamondPickEventArgs>();
            public EventPipeline<CrownPickEventArgs> OnCrownPicked { get; } = new EventPipeline<CrownPickEventArgs>();
            public EventPipeline<SkinChangeEventArgs> OnSkinChange { get; } = new EventPipeline<SkinChangeEventArgs>();
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
                    RespawnByAttribute(startAttributes);
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
            GameController.Instance.lines.Add(this);
            startAttributes = new LineRespawnAttributes(this, transform.position, transform.localEulerAngles, nextWay, controllable);
        }
        private void Move()
        {
            Vector3 targetPos = previousTurnPosition + transform.rotation * Vector3.forward * speed * (Time.time - previousTurnTime);
            transform.position = new Vector3(targetPos.x, transform.position.y, targetPos.z);
        }

        public void GoOffset(Vector3 offset)
        {
            if (IsGrounded) { Skin.StartFly(); }
            transform.position += offset;
            previousTurnPosition += offset;
        }

        private void Update()
        {
            if (Moving)
			{
                Move();
                if (auto && autoIndex < turnTime.Count && turnTime[autoIndex] < BGMController.Time)
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
            if ((IsGrounded && GameController.Instance.State == GameState.Playing && controllable && !died) || focus)
            {
                Events.OnTurn.Invoke(new LineTurnEventArgs(this, transform.localEulerAngles, NextWay, focus), args =>
                {
                    if (!args.canceled)
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
            }
        }

        public void RespawnByAttribute(LineRespawnAttributes attributes)
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
            GameController.Instance.OnLineDie.Invoke(new LineDieEventArgs(this, cause), args =>
			{
                Events.OnDie.Invoke(args, args2 =>
                {
                    if (!args2.canceled)
                    {
                        died = true;
                        switch (args2.cause)
                        {
                            case DeathCause.Obstacle:
                                Moving = false;
                                break;
                            case DeathCause.Air:
                                break;
                            case DeathCause.Water:
                                break;
                        }
                        Skin.Die(args2.cause);
                        if (overWhenDie) { GameController.Instance.GameOver(true); }
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
                GameController.Instance.OnSkinChange.Invoke(new SkinChangeEventArgs(this, Skin.GetType(), newSkinType), args =>
                {
                    Events.OnSkinChange.Invoke(args, args2 =>
                    {
                        canceled = args2.canceled;
                        newSkinType = args2.newSkin;
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
