using Event;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Level.Tools
{
    [RequireComponent(typeof(Line))]
    public class BuildRoad : MonoBehaviour
    {
        [Serializable]
        public class VariableBpm
		{
            public float time;
            public float bpm;
            public int timevalue;
		}

        [SerializeField] private GameObject roadPrefab = null;
        [SerializeField] private float width = 1f;
        [SerializeField] private VariableBpm[] bpms = new VariableBpm[1];
        private float bpm = -1f;
        private int timevalue = -1;
        [SerializeField] private new AudioSource audio = null;
        [SerializeField] private Transform roadParent;
        private Line line;
        private float turnFuture = -1f;
        private Vector3? lastTurnPosition = null;

        private float Interval => 60f / bpm / timevalue;

        private void Start()
        {
            roadParent = roadParent ?? new GameObject("Roads").transform;
            line = GetComponent<Line>();
            // 完全接管线的转弯函数
            line.events.onTurn.AddListener(e =>
            {
                e.canceled = true;
                if (turnFuture != -1f)
				{
                    if (audio.time >= turnFuture)
					{
                        CreateRoad();
                        lastTurnPosition = line.transform.position;
                        (line.transform.localEulerAngles, line.nextWay) = (line.nextWay, line.transform.localEulerAngles);  // 转弯
                        line.skin.Turn(true);  // 皮肤转弯
                        turnFuture = -1f;
                    }
					else { return e; }
                }
				else
				{
                    float interval = audio.time % Interval;
                    if (interval <= Interval / 2)  // 点慢了
					{
                        Debug.Log($"点慢了, 往回走 {(line.speed * interval)}");
                        line.transform.Translate(Vector3.back * (line.speed * interval), Space.Self);  // 往回走
                        line.skin.Update();  // 更新线身长度
                        CreateRoad();
                        lastTurnPosition = line.transform.position;
                        (line.transform.localEulerAngles, line.nextWay) = (line.nextWay, line.transform.localEulerAngles);  // 转弯
                        line.skin.Turn(true);  // 皮肤转弯
                        line.transform.Translate(Vector3.forward * (line.speed * interval), Space.Self);  // 往前走
                    }
                    else  // 点快了
                    {
                        turnFuture = ((int)(audio.time / Interval) + 1) * Interval;
                        Debug.Log($"点快了, turnFuture: {turnFuture}");
                    }
				}

                return e;
            }, Priority.Low);

            /*for (int i = 0; i < 1000; i++)
			{
                float time = Interval * i;
                Debug.DrawLine(new Vector3(time * line.speed, 0f, 1000f), new Vector3(time * line.speed, 0f, -1000f), Color.red, 10000f);
                Debug.DrawLine(new Vector3(1000f, 0f, time * line.speed), new Vector3(-1000f, 0f, time * line.speed), Color.red, 10000f);
            }*/
        }

        private void CreateRoad()
		{
            if (lastTurnPosition == null) { return; }
            Vector3 roadPosition = new Vector3((line.transform.position.x + lastTurnPosition.Value.x) / 2, line.transform.position.y - (roadParent.localScale.y / 2 + line.transform.localScale.y / 2), (line.transform.position.z + lastTurnPosition.Value.z) / 2);
            Transform road = Instantiate(roadPrefab, roadPosition, line.transform.rotation, roadParent).transform;
            road.localScale = new Vector3(width, roadPrefab.transform.localScale.y, Vector2.Distance(new Vector2(line.transform.position.x, line.transform.position.z), new Vector2(lastTurnPosition.Value.x, lastTurnPosition.Value.z)) + width);
		}

        private void Update()
        {
            if (turnFuture != -1f && audio.time >= turnFuture)
			{
                line.Turn(true);
			}
            // 反正不会太多，就遍历着呗
            bpm = bpms[bpms.Length - 1].bpm;
            timevalue = bpms[bpms.Length - 1].timevalue;
            for (int i = 0; i < bpms.Length; i++)
			{
                if (audio.time < bpms[i].time)
				{
                    bpm = bpms[i - 1].bpm;
                    timevalue = bpms[i - 1].timevalue;
                    break;
				}
			}
        }
    }
}
