using UnityEngine;
using DG.Tweening;
using DG.Tweening.Plugins.Options;
using DG.Tweening.Core;

namespace Level
{
    public class BGMController : MonoBehaviour
    {
		private static BGMController instance;
		[SerializeField] private AudioSource source;
		[SerializeField] private float fadeTime = 3f;
		private TweenerCore<float, float, FloatOptions> fadeTweener;

		public static BGMController Instance => instance;

		/// <summary>
		/// 如果需要更精确的时间，请使用 GameController.AudioTime
		/// </summary>
		public float Time
		{
			get => source.time;
			set
            {
				source.time = value;
				LastPlayTime = (float)AudioSettings.dspTime - source.time;
			}
		}

		public int TimeSample
		{
			get => source.timeSamples;
			set => source.timeSamples = value;
		}

		public bool IsPlaying
		{
			get => source.isPlaying;
			set
			{
				if (value) { Play(); }
				else { StopImmediately(); }
			}
		}

		public AudioClip Clip
		{
			get => source.clip;
			set => source.clip = value;
		}

		public float LastPlayTime { get; private set; }

		private void Awake()
		{
			if (instance != null && instance != this)
            {
				Debug.LogWarning($"[BGM Controller] There is another instance({instance.gameObject.name}) in this scene, destroy this.");
				Destroy(this);
				return;
            }
			instance = this;
			source = source ?? GetComponent<AudioSource>();
			if (source == null)
			{
				Debug.LogError("[BGM Controller] AudioSource Not Found!!!");
			}
		}

		private void OnDestroy()
		{
			if (instance == this) { instance = null; }
		}

		public void FadeOut()
		{
			if (fadeTweener == null) { fadeTweener = source.DOFade(0f, fadeTime).OnComplete(() => { source.Pause(); }).SetAutoKill(false); }
			else { fadeTweener.Restart(); }
		}

		public void StopImmediately()
		{
			fadeTweener?.Complete();
			source.volume = 1f;
			source.Pause();
		}

		public void Pause() => source.Pause();
		public void UnPause() => source.UnPause();

		public void Play()
		{
			StopImmediately();
			source.Play();
			source.volume = 1f;
			LastPlayTime = (float)AudioSettings.dspTime - source.time;
		}
		public void Play(float time)
		{
			Play();
			Time = time;
		}
	}
}
