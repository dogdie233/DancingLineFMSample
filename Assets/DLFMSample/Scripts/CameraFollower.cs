using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;
using Event;

namespace Level
{
    [RequireComponent(typeof(Camera))]
    public class CameraFollower : MonoBehaviour
    {
        public Line line;
        public Transform arm;
        public float smoothTime;
        public Vector3 initPivotOffset;
        [HideInInspector] public Vector3 pivotOffset;  // 焦点偏移
        private Vector3 currentVelocity = Vector3.zero;
        [HideInInspector] public Vector3 followPoint;
        private Tweener rotatorX;
        private Tweener rotatorY;
        private Tweener distanceChanger;
        private Tweener pivotChanger;
        private float startSmoothTime;
        private Quaternion startArmRotation;
        private float startDistance;

        void Start()
        {
            startSmoothTime = smoothTime;
            startArmRotation = arm.localRotation;
            startDistance = transform.localPosition.z;
            followPoint = line.transform.position;
            pivotOffset = initPivotOffset;
            arm.transform.position = followPoint + pivotOffset;
            EventManager.onStateChange.AddListener(e =>
            {
                if (e.canceled) { return e; }
                switch (e.newState)
                {
                    case GameState.WaitingRespawn:
                    case GameState.GameOver:
                        KillTweener();
                        break;
                    case GameState.SelectingSkins:
                        KillTweener();
                        smoothTime = startSmoothTime;
                        arm.localRotation = startArmRotation;
                        transform.localPosition = new Vector3(0f, 0f, startDistance);
                        followPoint = line.startAttributes.position;
                        pivotOffset = initPivotOffset;
                        arm.transform.position = followPoint + pivotOffset;
                        currentVelocity = Vector3.zero;
                        break;
                }
                return e;
            }, Priority.Monitor);
            EventManager.onRespawn.AddListener(e =>
            {
                foreach (CameraFollowerRespawnAttributes attributes in e.crown.cameraFollowerRespawnAttributes)
                {
                    if (attributes.follower == this)
                    {
                        arm.localEulerAngles = attributes.rotation;
                        transform.localPosition = Vector3.back * attributes.distance;
                        enabled = attributes.enabled;
                        pivotOffset = attributes.pivot;
                        followPoint = line.transform.position;
                        arm.transform.position = followPoint + pivotOffset;
                        currentVelocity = Vector3.zero;
                        break;
                    }
                }
                return e;
            }, Priority.Monitor);
        }

        void Update()
        {
            if (GameController.State == GameState.Playing)
			{
                followPoint = Vector3.SmoothDamp(followPoint, line.transform.position, ref currentVelocity, smoothTime);
                arm.transform.position = followPoint + pivotOffset;
            }
        }

        private void KillTweener()
		{
            rotatorX?.Kill(false);
            rotatorX = null;
            rotatorY?.Kill(false);
            rotatorY = null;
            distanceChanger?.Kill(false);
            distanceChanger = null;
            pivotChanger?.Kill(false);
            pivotChanger = null;
        }

        /// <summary>
        /// 旋转啊啊啊啊啊啊
        /// </summary>
        /// <param name="target">目标角度</param>
        /// <param name="distance">距离</param>
        /// <param name="duration">消耗时间</param>
        /// <param name="pivotOffset">焦点偏移</param>
        /// <param name="smoothTime">平滑时间</param>
        /// <param name="curve">时间曲线</param>
        public void Rotate(Vector2 target, float distance, float duration, Vector3 pivotOffset, float smoothTime, AnimationCurve curve)
        {
            KillTweener();
            this.smoothTime = smoothTime;
            if (target.x != arm.localEulerAngles.x)
			{
                rotatorX = DOTween.To(() => arm.localEulerAngles.x, x =>
                {
                    arm.localEulerAngles = new Vector3(x, arm.localEulerAngles.y, arm.localEulerAngles.z);
                }, target.x, duration).SetEase(curve);
            }
            if (target.y != arm.localEulerAngles.y)
			{
                rotatorY = DOTween.To(() => arm.localEulerAngles.y, y =>
                {
                    arm.localEulerAngles = new Vector3(arm.localEulerAngles.x, y, arm.localEulerAngles.z);
                }, target.y, duration).SetEase(curve);
            }
            distance = -1f * Mathf.Abs(distance);
            if (Mathf.Abs(transform.localPosition.z) != distance)
			{
                Vector3 startPivotOffset = this.pivotOffset;
                distanceChanger = DOTween.To(() => transform.localPosition.z, d =>
                {
                    transform.Translate(0f, 0f, d - transform.localPosition.z, Space.Self);
                }, distance, duration).SetEase(curve);
            }
            if (pivotOffset != this.pivotOffset)
			{
                pivotChanger = DOTween.To(() => this.pivotOffset, v => this.pivotOffset = v, pivotOffset, duration).SetEase(curve);
            }
        }
    }
}
