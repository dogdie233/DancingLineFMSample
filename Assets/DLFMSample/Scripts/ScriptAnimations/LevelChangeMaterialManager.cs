using Event;
using System;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;

namespace Level.Animations
{
	public class LevelChangeMaterialManager : MonoBehaviour
	{
		public static LevelChangeMaterialManager instance;
		private Dictionary<Material, TweenerCore<Color, Color, ColorOptions>> tweeners = new Dictionary<Material, TweenerCore<Color, Color, ColorOptions>>();
		private Dictionary<Crown, List<KeyValuePair<Material, Color>>> colorRespawnRecords = new Dictionary<Crown, List<KeyValuePair<Material, Color>>>();
		private List<KeyValuePair<Material, Color>> colorRestartRecords = new List<KeyValuePair<Material, Color>>();
		private Crown latestCrown;

		public static LevelChangeMaterialManager Instance => instance;

		private void Awake()
		{
			if (instance != null)
			{
				Destroy(this);
				return;
			}
			instance = this;
			GameController.Instance.OnStateChange.AddListener(OnStateChange, Priority.Monitor);
			GameController.Instance.OnCrownPick.AddListener(OnCrownPick, Priority.Monitor);
			GameController.Instance.OnRespawn.AddListener(OnRespawn, Priority.Monitor);
		}

		private void OnDestroy()
		{
			foreach (KeyValuePair<Material, TweenerCore<Color, Color, ColorOptions>> kvp in tweeners)
			{
				kvp.Value.Kill();
			}
			foreach (KeyValuePair<Material, Color> attribute in colorRestartRecords)
			{
				attribute.Key.color = attribute.Value;
			}
			instance = null;
			GameController.Instance.OnStateChange.RemoveListener(OnStateChange, Priority.Monitor);
			GameController.Instance.OnCrownPick.RemoveListener(OnCrownPick, Priority.Monitor);
			GameController.Instance.OnRespawn.RemoveListener(OnRespawn, Priority.Monitor);
		}

		private void OnStateChange(StateChangeEventArgs e)
		{
			if (e.canceled) { return; }
			switch (e.newState)
			{
				case GameState.WaitingRespawn:
					foreach (KeyValuePair<Material, TweenerCore<Color, Color, ColorOptions>> kvp in tweeners)
					{
						kvp.Value.Pause();
					}
					break;
				case GameState.SelectingSkins:
					latestCrown = null;
					colorRespawnRecords.Clear();
					foreach (KeyValuePair<Material, TweenerCore<Color, Color, ColorOptions>> kvp in tweeners)
					{
						kvp.Value.Pause();
					}
					foreach (KeyValuePair<Material, Color> attribute in colorRestartRecords)
					{
						attribute.Key.color = attribute.Value;
					}
					break;
			}
		}

		// 记录颜色
		private void OnCrownPick(CrownPickEventArgs e)
		{
			if (e.canceled) { return; }
			latestCrown = e.crown;
			colorRespawnRecords.Add(e.crown, new List<KeyValuePair<Material, Color>>());
			foreach (KeyValuePair<Material, TweenerCore<Color, Color, ColorOptions>> kvp in tweeners)
			{
				if (kvp.Value.IsPlaying())
				{
					colorRespawnRecords[e.crown].Add(new KeyValuePair<Material, Color>(kvp.Key, kvp.Value.endValue));
				}
			}
		}

		// 恢复颜色
		private void OnRespawn(RespawnEventArgs e)
		{
			if (e.canceled) { return; }
			List<KeyValuePair<Material, Color>> attributes = colorRespawnRecords[e.crown];
			foreach (KeyValuePair<Material, Color> attribute in attributes)
			{
				attribute.Key.color = attribute.Value;
			}
		}

		public void ChangeMaterialColor(Material material, Color targetColor, float duration, AnimationCurve curve = null)
		{
			if (tweeners.TryGetValue(material, out var tweener))
			{
				if (tweener.IsPlaying())
				{
					tweener.Pause();
				}
				tweener.startValue = material.color;
				tweener.ChangeEndValue(targetColor, duration, false);
				if (curve != null) { tweener.SetEase(curve); }
				else { tweener.SetEase(Ease.Linear); }
				tweener.Restart();
				tweener.Play();
			}
			else
			{
				tweener = material.DOColor(targetColor, duration).SetAutoKill(false);
				if (curve != null) { tweener.SetEase(curve); }
				tweeners.Add(material, tweener);
			}
			if (latestCrown != null)
			{
				foreach (KeyValuePair<Material, Color> kvp in colorRespawnRecords[latestCrown])
				{
					if (kvp.Key == material) { return; }
				}
				colorRespawnRecords[latestCrown].Add(new KeyValuePair<Material, Color>(material, targetColor));
			}
			else
			{
				foreach (KeyValuePair<Material, Color> kvp in colorRestartRecords)
				{
					if (kvp.Key == material) { return; }
				}
				colorRestartRecords.Add(new KeyValuePair<Material, Color>(material, material.color));
			}
		}
	}
}
