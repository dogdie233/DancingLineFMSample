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
        [HideInInspector] public static GameController instance = null;
        public Button bgButton;
        private static GameState _state = GameState.SelectingSkins;
        public static int diamondNumber = 0;
        
        public static GameState State
		{
			get { return _state; }
			set { if (_state != value) { EventManager.onStateChange.Invoke(_state, value); } }
		}

		public static bool IsStarted
		{
            get { return !(_state == GameState.SelectingSkins || _state == GameState.WaitingStart); }
        }

		private static StateChangeEventArgs OnStateChange(StateChangeEventArgs arg)
		{
            if (!arg.canceled)
                _state = arg.newState;
            return arg;
		}

		private void Awake()
        {
            bgButton.onClick.AddListener(() => {
                if (_state != GameState.Playing && _state != GameState.WaitingRespawn && (int)_state + 1 < Enum.GetNames(typeof(GameState)).Length)
                    State = (GameState)((int)_state + 1);
            });
            EventManager.onStateChange.AddListener(OnStateChange, Priority.Lowest);
            if (instance == null && instance != this)
                instance = this;
            else
                Debug.LogError("[Error] There is more than one Game Controller");
		}
	}
}