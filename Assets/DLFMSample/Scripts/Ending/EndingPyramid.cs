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

		private void Start()
		{
			EventManager.onStateChange.AddListener(args =>
			{
				switch (args.newState)
				{
					case GameState.WaitingRespawn:
						leftTweener?.Kill();
						leftTweener = null;
						rightTweener?.Kill();
						rightTweener = null;
						break;
					case GameState.WaitingContinue:
						left.position = Vector3.zero;
						right.position = Vector3.zero;
						break;
				}
				return args;
			}, Priority.Monitor);
		}

		public void OnOpenTriggerEnter()
		{
			// 开门动画
			leftTweener = left.DOLocalMoveX(-scale, time).SetEase(curve);
			rightTweener = right.DOLocalMoveX(scale, time).SetEase(curve);
			Debug.Log(GameController.lines[0].name);
			foreach (Line line in GameController.lines)
			{
				line.controllable = false;
			}
		}

		public void OnFinishTriggerEnter()
		{
			GameController.GameOver(false);
		}
	}
}
