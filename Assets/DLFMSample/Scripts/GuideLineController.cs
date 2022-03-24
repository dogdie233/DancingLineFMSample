using Event;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Rendering;

namespace Level
{
	[RequireComponent(typeof(Line), typeof(BoxCollider))]
	public class GuideLineController : MonoBehaviour
	{
		[Serializable]
		public struct GuideLineAttribute
		{
			public float time;
			public Matrix4x4 matrix;

			public GuideLineAttribute(float time, Matrix4x4 matrix)
			{
				this.time = time;
				this.matrix = matrix;
			}

			public override int GetHashCode()
			{
				int hash = 17;
				hash = hash * 31 + time.GetHashCode();
				hash = hash * 31 + matrix.GetHashCode();
				return hash;
			}

			public override string ToString() => $"GuideLineAttribute(time:{time}, matrix:{matrix})";
		}
		[Serializable]
		public struct GuideLineKeyFrame
		{
			public float time;
			public float speed;
			public Vector3 start;
			public Vector3 end;

			public GuideLineKeyFrame(float time, float speed, Vector3 start, Vector3 end)
			{
				this.time = time;
				this.speed = speed;
				this.start = start;
				this.end = end;
			}

			public override int GetHashCode()
			{
				int hash = 17;
				hash = hash * 31 + time.GetHashCode();
				hash = hash * 31 + speed.GetHashCode();
				hash = hash * 31 + start.GetHashCode();
				hash = hash * 31 + end.GetHashCode();
				return hash;
			}
		}

		public Line line;
		public new BoxCollider collider;
		public Mesh quadMesh;
		public Material lineMat;
		public Vector3 rotationOffset = new Vector3(90f, 0f, 0f);
		public float lineWidth = 0.1f;
		public float frameWidth = 1f;
		public float longLineLength = 0.8f;
		public float shortLineLength = 0.2f;
		public float noneLineLength = 0.1f;
		public float disappearEarlyTime = 0.1f;
		public Camera[] cameras;
		public GuideLineKeyFrame[] keyframes;
		private GuideLineAttribute[] lines;
		private MaterialPropertyBlock materialProperty;
		private int showIndex = 0;
#if UNITY_EDITOR
		public CommandBuffer buffer;
		public bool editing;
		private List<GuideLineKeyFrame> keyframesInEditing = new List<GuideLineKeyFrame>();
		private GuideLineKeyFrame editingKeyframe;
		private float previousFrameSpeed;
#else
		private CommandBuffer buffer;
#endif

		public void Awake()
		{
			line = line ?? GetComponent<Line>();
			collider = collider ?? GetComponent<BoxCollider>();
			MakeGuideLine();
			materialProperty = new MaterialPropertyBlock();
			buffer = new CommandBuffer();
			buffer.name = "GuideLine";
		}

		private void Start()
		{
			cameras = (cameras != null && cameras.Length > 0) ? cameras : Camera.allCameras;
#if UNITY_EDITOR
			if (editing && Application.isPlaying)
			{
				previousFrameSpeed = line.Speed;
				line.Events.OnTurn.AddListener(OnLineTurnEditor, Priority.Monitor);
				line.Events.OnEnterGround.AddListener(() =>
				{
					if (GameController.Instance.State == GameState.Playing)
					{
						editingKeyframe = new GuideLineKeyFrame(Time.time - GameController.Instance.StartTime, line.Speed, line.transform.position, Vector3.zero);
					}
				});
				line.Events.OnExitGround.AddListener(() =>
				{
					if (GameController.Instance.State == GameState.Playing)
					{
						editingKeyframe.end = line.transform.position - Quaternion.Euler(line.transform.localEulerAngles) * Vector3.forward * collider.size.z * line.transform.localScale.z;
						keyframesInEditing.Add(editingKeyframe);
					}
				});
			}
#endif
		}

		private void OnEnable()
		{
			GameController.Instance.OnRespawn.AddListener(OnRespawn, Priority.Monitor);
			GameController.Instance.OnStateChange.AddListener(OnStateChange, Priority.Monitor);
			foreach (Camera camera in cameras)
			{
				camera.AddCommandBuffer(CameraEvent.AfterForwardOpaque, buffer);
			}
			ReDraw();
		}

		private void OnDisable()
		{
			GameController.Instance.OnRespawn.RemoveListener(OnRespawn, Priority.Monitor);
			GameController.Instance.OnStateChange.RemoveListener(OnStateChange, Priority.Monitor);
			foreach (Camera camera in cameras)
			{
				camera.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, buffer);
			}
		}

		private void OnStateChange(StateChangeEventArgs e)
		{
			if (e.canceled) { return; }
			if (e.newState == GameState.SelectingSkins) { showIndex = 0; }
#if UNITY_EDITOR
			if (editing)
			{
				if (e.newState == GameState.Playing)
				{
					previousFrameSpeed = line.Speed;
					editingKeyframe = new GuideLineKeyFrame(0f, line.Speed, line.transform.position, Vector3.zero);
				}
				if (e.newState == GameState.GameOver || e.newState == GameState.WaitingRespawn)
				{
					editingKeyframe.end = line.transform.position;
					keyframesInEditing.Add(editingKeyframe);
					keyframes = keyframesInEditing.Where(f => Vector3.Distance(f.start, f.end) > frameWidth).ToArray();
					UnityEngine.Debug.Log("Guideline edit finish");
					EditorUtility.SetDirty(this);
				}
			}
#endif
		}
#if UNITY_EDITOR
		private void OnLineTurnEditor(LineTurnEventArgs e)
		{
			if (e.canceled) { return; }
			editingKeyframe.end = line.transform.position - Quaternion.Euler(line.NextWay) * Vector3.forward * frameWidth / 2f;
			keyframesInEditing.Add(editingKeyframe);
			editingKeyframe = new GuideLineKeyFrame((Time.time - GameController.Instance.StartTime) + (frameWidth / 2f) / line.Speed, line.Speed, line.transform.position + Quaternion.Euler(line.transform.localEulerAngles) * Vector3.forward * frameWidth / 2f, Vector3.zero);
		}
#endif

		private void OnRespawn(RespawnEventArgs e)
		{
			if (e.canceled) { return; }
			showIndex = 0;
			while (e.checkpoint.Time + disappearEarlyTime >= lines[showIndex].time) { showIndex++; }
		}

		private void MakeGuideLine()
		{
			Stopwatch sw = new Stopwatch();
			sw.Start();
			List<GuideLineAttribute> attributes = new List<GuideLineAttribute>();
			for (int i = 0; i < keyframes.Length; i++)
			{
				float remainDistance = Vector3.Distance(keyframes[i].start, keyframes[i].end);
				Vector3 previousPosition = keyframes[i].start;
				Vector3 nowWayVector = (keyframes[i].end - keyframes[i].start).normalized;
				bool isLong = false;
				bool isNone = false;
				//UnityEngine.Debug.DrawLine(keyframes[i].start, keyframes[i].end, Color.red, 10f);
				while (remainDistance > 0f)
				{
					float length = isNone ? noneLineLength : (isLong ? longLineLength : shortLineLength);
					float realLength = Mathf.Min(length, remainDistance);
					Vector3 endPosition = previousPosition + nowWayVector * realLength;
					remainDistance -= length;
					if (!isNone)
					{
						Matrix4x4 matrix = Matrix4x4.TRS((previousPosition + endPosition) / 2f - Vector3.up * (line.transform.localScale.y / 2f - 0.1f), Quaternion.Euler(Quaternion.FromToRotation(Vector3.forward, nowWayVector).eulerAngles + rotationOffset), new Vector3(lineWidth, realLength, 1f));
						attributes.Add(new GuideLineAttribute(keyframes[i].time + Vector3.Distance(endPosition, keyframes[i].start) / keyframes[i].speed, matrix));
					}
					previousPosition = endPosition;
					isLong = isNone ? (isLong ? false : true) : isLong;
					isNone = !isNone;
				}
			}
			lines = attributes.ToArray();
			sw.Stop();
			UnityEngine.Debug.Log($"{keyframes.Length} keyframes and {lines.Length} lines, taking {sw.ElapsedMilliseconds}ms");
		}

		private void Update()
		{
#if UNITY_EDITOR
			if (Application.isPlaying)
			{
				if (previousFrameSpeed != line.Speed && line.IsGrounded)
				{
					editingKeyframe.end = line.transform.position;
					keyframesInEditing.Add(editingKeyframe);
					editingKeyframe = new GuideLineKeyFrame(Time.time - GameController.Instance.StartTime, line.Speed, line.transform.position, Vector3.zero);
					previousFrameSpeed = line.Speed;
				}
				if (lines.Length > 0 && showIndex < lines.Length - 1)
				{
					while (GameController.Instance.LevelTime + disappearEarlyTime >= lines[showIndex].time)
					{
						showIndex++;
						ReDraw();
					}
				}
			}
#else
			if (lines.Length > 0 && showIndex < lines.Length - 1)
				{
					while (BGMController.Time + disappearEarlyTime >= lines[showIndex].time)
					{
						showIndex++;
						ReDraw();
					}
				}
#endif
		}

		private void ReDraw()
		{
			// 分割数组
			List<Matrix4x4[]> splitLines = new List<Matrix4x4[]>();
			for (int i = 0; i * 1023 < lines.Length; i++)
			{
				splitLines.Add(lines.Skip(showIndex + i * 1023).Take(1023).Select(a => a.matrix).ToArray());
			}

			buffer.Clear();
			foreach (Matrix4x4[] line in splitLines)
			{
				buffer.DrawMeshInstanced(quadMesh, 0, lineMat, 0, line, line.Length, materialProperty);
			}
		}
	}
}
