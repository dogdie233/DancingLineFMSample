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
		public static float Time
		{
			get => Instance.source.time;
			set => Instance.source.time = value;
		}

		public static int TimeSample
		{
			get => Instance.source.timeSamples;
			set => Instance.source.timeSamples = value;
		}
		public static bool IsPlaying
		{
			get => Instance.source.isPlaying;
			set
			{
				if (value) { Instance.source.Play(); }
				else { Instance.source.Pause(); }
			}
		}

		public static AudioClip Clip
		{
			get => Instance.source.clip;
			set => Instance.source.clip = value;
		}

		private void Awake()
		{
			if (instance == null) { instance = this; }
			else
			{
				enabled = false;
				return;
			}
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
			if (fadeTweener == null) { fadeTweener = source.DOFade(0f, fadeTime).OnComplete(() => { source.Pause(); }).SetAutoKill(false).Play(); }
			else { fadeTweener.Restart(); }
		}

		public void StopImmediately()
		{
			/*if (fadeTweener != null) { fadeTweener.Complete(); }
			else { source.Pause(); }*/
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
		}
		public void Play(float time)
		{
			Play();
			Time = time;
		}
	}
}
