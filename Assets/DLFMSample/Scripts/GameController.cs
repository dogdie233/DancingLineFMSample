using Event;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.Playables;
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
        public static List<Line> lines = new List<Line>();
        public AudioMixerGroup soundMixerGroup;
        public BGMController bgmController;
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
                    EventManager.onStateChange.Invoke(new StateChangeEventArgs(_state, value), (StateChangeEventArgs e) =>
                    {
                        if (!e.canceled)
                        {
                            _state = e.newState;
                            if (e.newState == GameState.SelectingSkins)  // 清空Collections
                            {
                                for (int i = collections.Count - 1; i >= 0f; i--)
                                {
                                    collections[i].Recover();
                                }
                                collections.Clear();
                            }
                        }
                    });
                }
            }
        }

		public static bool IsStarted
		{
            get => !(_state == GameState.SelectingSkins || _state == GameState.WaitingStart);
        }

		private void Awake()
        {
            if (instance == null && instance != this) { instance = this; }
			else
			{
                Debug.LogError("[Error] There is more than one Game Controller");
                this.enabled = false;
                return;
            }
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
		}

		public static void Respawn(Crown crown)
		{
            EventManager.onRespawn.Invoke(new RespawnEventArgs(crown), e1 =>
            {
                if (e1.canceled) { return; }
                EventManager.onStateChange.Invoke(new StateChangeEventArgs(_state, GameState.WaitingContinue), (StateChangeEventArgs e2) =>
                {
                    if (!e2.canceled)
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
            });
        }

        public static void GameOver(bool fail)
		{
            if (fail)
			{
                foreach (ICollection collection in collections)
                {
                    if (collection is Crown)
                    {
                        State = GameState.WaitingRespawn;
                        return;
                    }
                }
            }
            State = GameState.GameOver;
		}

        public static void PlaySound(AudioClip clip)
		{
            GameObject dieSoundObj = new GameObject("SoundPlayer");
            dieSoundObj.transform.position = Camera.main.transform.position;
            AudioSource audioSource = dieSoundObj.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.clip = clip;
            audioSource.outputAudioMixerGroup = instance.soundMixerGroup;
            audioSource.Play();
            Destroy(dieSoundObj, clip.length);
        }
	}
}