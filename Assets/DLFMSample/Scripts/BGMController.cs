using Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;
using System;

namespace Level
{
    public class BGMController : MonoBehaviour
    {
		[SerializeField] private AudioSource source;
		private Tweener tweener;

		public float Time
		{
			get => source.time;
			set => source.time = value;
		}

		public bool IsPlaying
		{
			get => source.isPlaying;
			set
			{
				if (value) { source.Play(); }
				else { source.Pause(); }
			}
		}

		public AudioClip Clip
		{
			get => source.clip;
			set => source.clip = value;
		}

		private void Awake()
		{
			if (source == null) { source = GetComponent<AudioSource>(); }
			EventManager.onStateChange.AddListener(e =>
			{
				if (source == null)
				{
					Debug.LogError("[BGM Controller] AudioSource Not Found!!!");
					e.canceled = true;
				}
				return e;
			}, Priority.Low);  // source为空，阻止启动游戏
			EventManager.onStateChange.AddListener(e =>
			{
				if (!e.canceled)
				{
					switch (e.newState)
					{
						case GameState.Playing:
							source.Play();
							break;
						case GameState.WaitingRespawn:
						case GameState.GameOver:
							tweener?.Kill();
							tweener = source.DOFade(0f, 3f);
							break;
						case GameState.SelectingSkins:
							tweener?.Kill();
							source.Stop();
							source.volume = 1f;
							break;
					}
				}
				return e;
			}, Priority.Monitor);
			EventManager.onRespawn.AddListener(e =>
			{
				if (e.canceled) { return e; }
				tweener?.Kill();
				source.Play();
				source.volume = 1f;
				source.time = e.crown.time;
				source.Pause();
				return e;
			}, Priority.Monitor);
		}

		public void Stop() => source.Stop();
/*
		private void Update()
		{
			if (GameController.State == GameState.Playing)
			{
				if (!source.isPlaying && source.time == 0f)
				{
					GameController.State = GameState.GameOver;
					ToDogdie.Utils.AnimationClipMaker.Save("Assets/az/qwq.anim");
				}
			}
		}*/
	}
}
