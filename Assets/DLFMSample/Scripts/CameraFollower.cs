using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

namespace Level
{
    public class CameraFollower : MonoBehaviour
    {
        public class Task
		{
            public Vector3 startRotation;
            public Vector3 targetRotation;
            public float startDistance;
            public float targetDistance;
            public AnimationCurve curve;
            public float startTime = 0f;

            public Task(Vector3 arg1, Vector3 arg2, float arg3, float arg4, AnimationCurve arg5)
			{
                this.startRotation = arg1;
                this.targetRotation = arg2;
                this.startDistance = arg3;
                this.targetDistance = arg4;
                this.curve = arg5;
			}
		}

        public Transform line;
        public float distance;
        private Vector3 currentVelocity = Vector3.zero;
        public float smoothTime;
        private Vector3 followPoint;
        private Vector3 vector;
        private Task runningTask;
        private Tweener rotaterX;
        private Tweener rotaterY;
        private Tweener rotaterZ;
        private Tweener distanceChanger;

        void Start()
        {
            followPoint = line.position;
            Rotate(transform.eulerAngles, distance, 0.00001f, new AnimationCurve(new Keyframe[] { new Keyframe(0f, 0f, 0f, 0f) }));
            vector = (transform.position - followPoint) / Vector3.Distance(transform.position, followPoint) * distance;
        }

		void Update()
        {
            followPoint = Vector3.SmoothDamp(followPoint, line.position, ref currentVelocity, smoothTime);
            transform.LookAt(followPoint);
            transform.position = followPoint + vector;
            /*
            if (runningTask != null)
			{
                if (runningTask.startTime == 0f)
                    runningTask.startTime = Time.time;
                Rotate(Vector3.Lerp(runningTask.startRotation, runningTask.targetRotation, runningTask.curve.Evaluate(Time.time - runningTask.startTime)), Mathf.Lerp(runningTask.startDistance, runningTask.targetDistance, runningTask.curve.Evaluate(Time.time - runningTask.startTime)));
                distance = Vector3.Distance(followPoint, transform.position);
                if (Time.time >= runningTask.startTime + runningTask.curve.length)
                    runningTask = null;
            }
            */
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
            if (rotaterX != null)
			{
                rotaterX.Kill(false);
                rotaterX = null;
			}
            if (rotaterY != null)
            {
                rotaterY.Kill(false);
                rotaterY = null;
            }
            if (rotaterZ != null)
            {
                rotaterZ.Kill(false);
                rotaterZ = null;
            }
            if (distanceChanger != null)
			{
                distanceChanger.Kill(false);
                distanceChanger = null;
			}
            if (target.x != transform.eulerAngles.x)
                rotaterX = DOTween.To(() => transform.eulerAngles.x, x => { transform.RotateAround(followPoint, Vector3.right, x - transform.eulerAngles.x); }, target.x, duration).SetEase(curve);
            if (target.y != transform.eulerAngles.y)
                rotaterY = DOTween.To(() => transform.eulerAngles.y, y => { transform.RotateAround(followPoint, Vector3.up, y - transform.eulerAngles.y); }, target.y, duration).SetEase(curve);
            if (Vector3.Distance(transform.position, followPoint) != distance)
                distanceChanger = DOTween.To(() => Vector3.Distance(transform.position, followPoint), d => vector = (transform.position - followPoint) / Vector3.Distance(transform.position, followPoint) * d, distance, duration).SetEase(curve);
        }
    }
}
