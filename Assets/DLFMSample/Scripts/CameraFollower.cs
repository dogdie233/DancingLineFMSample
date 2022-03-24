using System;
using UnityEngine;
using UnityEngine.Serialization;
using DG.Tweening;
using Event;
using System.Collections.Generic;
using DG.Tweening.Plugins.Options;
using DG.Tweening.Core;

namespace Level
{
    [Serializable]
    public struct CameraFollowerRespawnAttribute
    {
        public bool enabled;
        public float smoothTime;
        public Vector2 rotation;
        public Quaternion cameraRotation;
        public float distance;
        public Vector3 pivot;

        public CameraFollowerRespawnAttribute(bool enabled, float smoothTime, Vector2 rotation, Quaternion cameraRotation, float distance, Vector3 pivot)
        {
            this.enabled = enabled;
            this.smoothTime = smoothTime;
            this.rotation = rotation;
            this.cameraRotation = cameraRotation;
            this.distance = distance;
            this.pivot = pivot;
        }

        public override string ToString() => $"CameraFollowerRespawnAttribute(enabled:{enabled}, smoothTime:{smoothTime}, rotation:{rotation}, cameraRotation:{cameraRotation}, distance:{distance}, pivot:{pivot})";
    }

    [RequireComponent(typeof(Camera))]
    public class CameraFollower : MonoBehaviour
    {
        public Line line;
        public Transform arm;
        public float smoothTime;
        [FormerlySerializedAs("initPivotOffset")] [HideInInspector] public Vector3 pivotOffset;  // 焦点偏移
        private Vector3 currentVelocity = Vector3.zero;
        private Vector3 followPoint;
        private TweenerCore<float, float, FloatOptions> rotatorX;
        private TweenerCore<float, float, FloatOptions> rotatorY;
        private TweenerCore<float, float, FloatOptions> distanceChanger;
        private TweenerCore<Vector3, Vector3, VectorOptions> pivotChanger;
        private CameraFollowerRespawnAttribute startAttribute;
        private Dictionary<ICheckpoint, CameraFollowerRespawnAttribute> respawnAttributes = new Dictionary<ICheckpoint, CameraFollowerRespawnAttribute>();
        public Vector3 FollowPoint
        {
            get => followPoint;
            set => FollowPoint = value;
        }

        private void Start()
        {
            startAttribute = new CameraFollowerRespawnAttribute(true, smoothTime, new Vector2(arm.localEulerAngles.x, arm.localEulerAngles.y), transform.localRotation, transform.localPosition.z, pivotOffset);
            followPoint = line.transform.position;
            arm.transform.position = followPoint + pivotOffset;
            GameController.Instance.OnStateChange.AddListener(e =>
            {
                if (e.canceled) { return; }
                switch (e.newState)
                {
                    case GameState.WaitingRespawn:
                    case GameState.GameOver:
                        PauseAll();
                        break;
                    case GameState.SelectingSkins:
                        PauseAll();
                        smoothTime = startAttribute.smoothTime;
                        arm.localRotation = Quaternion.Euler(startAttribute.rotation.x, startAttribute.rotation.y, 0f);
                        transform.localPosition = new Vector3(0f, 0f, startAttribute.distance);
                        followPoint = line.StartAttributes.position;
                        pivotOffset = startAttribute.pivot;
                        arm.transform.position = followPoint + pivotOffset;
                        transform.localRotation = startAttribute.cameraRotation;
                        currentVelocity = Vector3.zero;
                        break;
                }
            }, Priority.Monitor);
            GameController.Instance.OnRespawn.AddListener(e =>
            {
                if (e.canceled) { return; }
                if (respawnAttributes.TryGetValue(e.checkpoint, out var attribute))
                arm.localEulerAngles = attribute.rotation;
                transform.localPosition = Vector3.forward * attribute.distance;
                enabled = attribute.enabled;
                pivotOffset = attribute.pivot;
                followPoint = line.transform.position;
                arm.transform.position = followPoint + pivotOffset;
                transform.localRotation = attribute.cameraRotation;
                currentVelocity = Vector3.zero;
            }, Priority.Monitor);
            GameController.Instance.OnCheckpointReach.AddListener(e =>
            {
                var attribute = new CameraFollowerRespawnAttribute(enabled, smoothTime,
                    new Vector2(rotatorX == null ? arm.localEulerAngles.x : rotatorX.endValue, rotatorY == null ? arm.localEulerAngles.y : rotatorY.endValue),
                    transform.localRotation, distanceChanger == null ? transform.localPosition.z : distanceChanger.endValue, pivotChanger == null ? pivotOffset : pivotChanger.endValue);
                respawnAttributes[e.checkpoint] = attribute;
                Debug.Log("Save checkpoint data " + attribute);
            }, Priority.Monitor);
        }

        private void Update()
        {
            if (GameController.Instance.State == GameState.Playing)
			{
                followPoint = Vector3.SmoothDamp(followPoint, line.transform.position, ref currentVelocity, smoothTime);
                arm.transform.position = followPoint + pivotOffset;
            }
        }

        public void PauseAll()
		{
            rotatorX?.Pause();
            rotatorY?.Pause();
            distanceChanger?.Pause();
            pivotChanger?.Pause();
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
