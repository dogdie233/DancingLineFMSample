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
        public static List<ICollection> collections = new List<ICollection>();
        public static List<Crown> crowns;
        
        public static GameState State
		{
			get { return _state; }
            set
            {
                if (_state != value)
                {
                    EventManager.onStateChange.Invoke(new StateChangeEventArgs(_state, value), (StateChangeEventArgs e) => {
                        if (!e.canceled) { _state = e.newState; }
                    });
                }
            }
        }

		private void Update()
		{
            Debug.Log(_state);
		}

		public static bool IsStarted
		{
            get { return !(_state == GameState.SelectingSkins || _state == GameState.WaitingStart); }
        }

		private void Awake()
        {
            bgButton.onClick.AddListener(() => {
                switch (_state)
				{
                    case GameState.SelectingSkins:
                        State = GameState.WaitingStart;
                        break;
                    case GameState.WaitingStart:
                    case GameState.WaitingContinue:
                        State = GameState.Playing;
                        break;
				}
            });
            if (instance == null && instance != this)
                instance = this;
            else
                Debug.LogError("[Error] There is more than one Game Controller");
		}

        public static void Respawn(Crown crown)
		{
            if (EventManager.onRespawn.Invoke(new RespawnEventArgs(crown)).canceled) { return; }
            EventManager.onStateChange.Invoke(new StateChangeEventArgs(_state, GameState.WaitingContinue), (StateChangeEventArgs e) => {
                if (!e.canceled)
				{
                    _state = GameState.WaitingContinue;
                    foreach (LineRespawnAttributes attribute in crown.lineRespawnAttributes)
                    {
                        attribute.line.Respawn(attribute);
                    }
                    for (int i = collections.Count - 1; i >= 0f; i--)
                    {
                        if (collections[i] is Crown && (Crown)collections[i] == crown) { break; }
                        collections[i].Recover();
                        collections.RemoveAt(i);
                    }
                    crown.Respawn();
                }
            });
        }

        public static void GameOver()
		{
            foreach (ICollection collection in collections)
            {
                if (collection is Crown)
				{
                    State = GameState.WaitingRespawn;
                    return;
				}
			}
            State = GameState.GameOver;
		}
	}
}