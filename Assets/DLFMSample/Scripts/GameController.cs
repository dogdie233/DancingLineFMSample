using Event;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

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
        private static GameController instance = null;
        public readonly List<Line> lines = new List<Line>();
        private GameState _state = GameState.SelectingSkins;
        private float startTime;

        public static GameController Instance => instance;
        public float StartTime => startTime;
        // 未来这里还有延迟设置
        public float LevelTime => BGMController.Instance.IsPlaying ? (float)AudioSettings.dspTime - BGMController.Instance.LastPlayTime : BGMController.Instance.Time;
        public float AudioTime => BGMController.Instance.IsPlaying ? (float)AudioSettings.dspTime - BGMController.Instance.LastPlayTime : BGMController.Instance.Time;

        #region Events
        public EventPipeline<StateChangeEventArgs> OnStateChange { get; private set; }
        public EventPipeline<RespawnEventArgs> OnRespawn { get; private set; }
        public EventPipeline<DiamondPickEventArgs> OnDiamondPick { get; private set; }
        public EventPipeline<CrownPickEventArgs> OnCheckpointReach { get; private set; }
        public EventPipeline<LineDieEventArgs> OnLineDie { get; private set; }
        public EventPipeline<SkinChangeEventArgs> OnSkinChange { get; private set; }
        #endregion

        public GameState State
		{
			get { return _state; }
            internal set
            {
                if (_state != value)
                {
                    OnStateChange.Invoke(new StateChangeEventArgs(_state, value), (StateChangeEventArgs e) =>
                    {
                        if (e.canceled) { return; }
                        _state = e.newState;
                        OnStateChanged(e.lastState);
                    });
                }
            }
        }

		public bool IsStarted
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
            OnStateChange = new EventPipeline<StateChangeEventArgs>();
            OnRespawn = new EventPipeline<RespawnEventArgs>();
            OnDiamondPick = new EventPipeline<DiamondPickEventArgs>();
            OnCheckpointReach = new EventPipeline<CrownPickEventArgs>();
            OnLineDie = new EventPipeline<LineDieEventArgs>();
            OnSkinChange = new EventPipeline<SkinChangeEventArgs>();
        }

        private void OnBackgroundButtonClick()
		{
            if (State == GameState.Playing)
			{
                foreach (Line line in lines) { line.Turn(false); }
            }
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
        }

		private void Update()
		{
			if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)) { OnBackgroundButtonClick(); }
            if (State == GameState.WaitingRespawn && Input.GetKeyDown(KeyCode.P)) { RespawnFromLatestAvailableCheckpoint(); }
		}

        public void RespawnFromCheckpoint(ICheckpoint checkpoint)
		{
            OnRespawn.Invoke(new RespawnEventArgs(checkpoint), e1 =>
            {
                if (e1.canceled) { return; }
                State = GameState.WaitingContinue;
                if (State != GameState.WaitingContinue) { return; }  // 被取消
                checkpoint.Respawn();
                var collections = CollectionController.Instance.RemoveAfterByTime(checkpoint.Time);
                foreach (var collection in collections)
                {
                    collection.Key.Recover();
                }
                BGMController.Instance.Time = checkpoint.Time;
                BGMController.Instance.StopImmediately();
            });
        }

        private void OnStateChanged(GameState lastState)
        {
            switch (State)
            {
                case GameState.Playing:
                    if (lastState == GameState.WaitingStart) { startTime = (float)AudioSettings.dspTime; }
                    BGMController.Instance.Play();
                    break;
                case GameState.SelectingSkins:  // 重开
                    var collections = CollectionController.Instance.RemoveAfter(-1);
                    Debug.Log($"Recover {collections.Count} collection{(collections.Count > 1 ? "s" : "")}");
                    foreach (var collection in collections)
                    {
                        collection.Key.Recover();
                    }
                    BGMController.Instance.Time = 0f;
                    BGMController.Instance.StopImmediately();
                    break;
                case GameState.WaitingRespawn:
                case GameState.GameOver:
                    BGMController.Instance.FadeOut();
                    break;
            }
            foreach (Line line in lines)
            {
                line.OnStateChange(State);
            }
        }

		public void RespawnFromLatestAvailableCheckpoint()
		{
            var collections = CollectionController.Instance.Collections;
            for (int i = collections.Count - 1; i >= 0; i--)
			{
                var checkpoint = collections[i].Key as ICheckpoint;
                if (checkpoint != null)
				{
                    RespawnFromCheckpoint(checkpoint);
                    break;
				}
			}
        }

        public void GameOver(bool fail)
		{
            if (fail)
			{
                var collections = CollectionController.Instance.Collections;
                foreach (KeyValuePair<ICollection, float> collection in collections)
				{
                    if (collection.Key as ICheckpoint != null)
					{
                        State = GameState.WaitingRespawn;
                        return;
					}
				}
            }
            State = GameState.GameOver;
		}

        public void PlaySound(AudioClip clip) => AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
    }
}