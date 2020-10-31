using Event;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum DeathCause
{
    Obstacle,
    Water,
    Air,
    Custom
}

namespace Level
{
    public enum GameState
	{
        SelectingSkins,  // 选择皮肤阶段
        WaitingStart,  // 等待开始阶段
        Playing,  // 正在游戏阶段
        WaitingRespawn,  // 确认复活阶段
        WaitingContinue,  // 复活后等待开始
        GameOver,  // 游戏结束(拒绝复活)
	}

    public class GameController : MonoBehaviour
    {
        [Serializable]
        public class EventsClass
        {
            public delegate void StateChange(ref StateChangeEvent arg);
            public UnityEvent onStart = new UnityEvent();
            public StateChange OnStateChange;
        }

        [HideInInspector] public static GameController instance = null;
        public Button bgButton;
        public static EventsClass events = new EventsClass();
        private static GameState _state = GameState.SelectingSkins;
        
        public static GameState State
		{
			get { return _state; }
			set { RunStateChangeEvent(value); }
		}

		public static bool IsStarted
		{
            get { return !(_state == GameState.SelectingSkins || _state == GameState.WaitingStart); }
        }

        private static void RunStateChangeEvent()
		{
            RunStateChangeEvent((GameState)((int)_state + 1));
		}

        private static void RunStateChangeEvent(GameState newState)
		{
            StateChangeEvent arg = new StateChangeEvent(_state, newState);
            try { events.OnStateChange.BeginInvoke(ref arg, ar => { events.OnStateChange.EndInvoke(ref arg, ar); StateChangeEventCompleted(arg); }, null); }
            catch (NullReferenceException) { StateChangeEventCompleted(arg); }
        }

		private static void StateChangeEventCompleted(StateChangeEvent arg)
		{
            Debug.Log("test: " + arg.test);
            if (arg.canceled)
                return;
            _state = arg.newState;
            switch (_state)
			{
                case GameState.Playing:
                    events.onStart.Invoke();
                    instance.bgButton.onClick.RemoveListener(RunStateChangeEvent);
                    break;
			}
		}

		private void Awake()
		{
            bgButton.onClick.AddListener(RunStateChangeEvent);
            if (instance == null && instance != this)
                instance = this;
            else
                Debug.LogError("[Error] There is more than one Game Controller");
		}
	}
}