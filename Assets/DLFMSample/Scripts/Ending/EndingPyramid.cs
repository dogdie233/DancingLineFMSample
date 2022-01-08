using DG.Tweening;
using Event;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Level.Ending
{
	public class EndingPyramid : MonoBehaviour
	{
		[SerializeField] private Transform left = null;
		[SerializeField] private Transform right = null;
		[SerializeField] private float time = 1f;
		[SerializeField] private float scale = 1f;
		[SerializeField] private AnimationCurve curve = new AnimationCurve(new Keyframe[] { new Keyframe(0.0f, 0.0f, 1.0f, 1.0f), new Keyframe(1.0f, 1.0f, 1.0f, 1.0f) });
		private Tweener leftTweener;
		private Tweener rightTweener;

		private void OnEnable()
		{
			leftTweener = left.DOLocalMoveX(-scale, time).SetEase(curve).Pause();
			rightTweener = right.DOLocalMoveX(scale, time).SetEase(curve).Pause();
			GameController.Instance.OnStateChange.AddListener(OnStateChange, Priority.Monitor);
		}

		private void OnDisable()
		{
			leftTweener.Kill();
			leftTweener = null;
			rightTweener.Kill();
			rightTweener = null;
			GameController.Instance.OnStateChange.RemoveListener(OnStateChange, Priority.Monitor);
		}

		private void OnStateChange(StateChangeEventArgs args)
		{
			switch (args.newState)
			{
				case GameState.WaitingRespawn:
					leftTweener?.Pause();
					rightTweener?.Pause();
					break;
				case GameState.WaitingContinue:
					leftTweener.ForceInit();
					rightTweener?.ForceInit();
					left.position = Vector3.zero;
					right.position = Vector3.zero;
					break;
			}
		}

		public void OnOpenTriggerEnter()
		{
			// 开门动画
			leftTweener?.Play();
			rightTweener?.Play();
			Debug.Log(GameController.lines[0].name);
			foreach (Line line in GameController.lines)
			{
				line.Controllable = false;
			}
		}

		public void OnFinishTriggerEnter()
		{
			GameController.Instance.GameOver(false);
		}
	}
}
