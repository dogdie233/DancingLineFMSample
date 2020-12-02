using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

namespace Level
{
    public class CameraFollower : MonoBehaviour
    {
        public Transform line;
        public float distance;
        private Vector3 currentVelocity = Vector3.zero;
        public float smoothTime;
        public bool enable;
        private Vector3 followPoint;
        private Vector3 vector;
        private Tweener rotaterX;
        private Tweener rotaterY;
        private Tweener distanceChanger;

        void Start()
        {
            followPoint = line.position;
            Rotate(transform.eulerAngles, distance, 0.00001f, new AnimationCurve(new Keyframe[] { new Keyframe(0f, 0f, 0f, 0f) }));
            vector = (transform.position - followPoint) / Vector3.Distance(transform.position, followPoint) * distance;
        }

        void Update()
        {
            if (!enable) return;
            followPoint = Vector3.SmoothDamp(followPoint, line.position, ref currentVelocity, smoothTime);
            transform.LookAt(followPoint);
            transform.position = followPoint + vector;
        }

        /// <summary>
        /// 转弯啊啊啊啊啊啊
        /// </summary>
        /// <param name="target">目标角度</param>
        /// <param name="distance">距离</param>
        /// <param name="duration">消耗时间</param>
        /// <param name="curve">时间曲线</param>
        public void Rotate(Vector2 target, float distance, float duration, AnimationCurve curve)
        {
            if (!enable) return;
            rotaterX?.Kill(false);
            rotaterX = null;
            rotaterY?.Kill(false);
            rotaterY = null;
            distanceChanger?.Kill(false);
            distanceChanger = null;
            if (target.x != transform.eulerAngles.x)
				rotaterX = DOTween.To(() => transform.eulerAngles.x, x => { transform.RotateAround(followPoint, Vector3.right, x - transform.eulerAngles.x); }, target.x, duration).SetEase(curve);
			if (target.y != transform.eulerAngles.y)
				rotaterY = DOTween.To(() => transform.eulerAngles.y, y => { transform.RotateAround(followPoint, Vector3.up, y - transform.eulerAngles.y); }, target.y, duration).SetEase(curve);
			if (Vector3.Distance(transform.position, followPoint) != distance)
				distanceChanger = DOTween.To(() => Vector3.Distance(transform.position, followPoint), d => vector = (transform.position - followPoint) / Vector3.Distance(transform.position, followPoint) * d, distance, duration).SetEase(curve);
		}
	}
}
