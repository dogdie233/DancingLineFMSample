using Level;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class Aff2RoadWindow : EditorWindow
{
    private GameObject roadPrefab = null;
    private TextAsset affTextAsset = null;
    private Line line = null;
    private float width = 1f;
    private bool track1;
    private bool track2;
    private bool track3;
    private bool track4;
    private Transform roadParent = null;
    private int lastTurnTime = 0;
    private Vector3 lastTurnPosition = Vector3.zero;
    private Vector3 nowAngle = Vector3.zero;

    [MenuItem("DLFMSample/Chart2Road/Aff")]//在unity菜单Window下有MyWindow选项
    private static void Init()
    {
        Aff2RoadWindow window = GetWindow<Aff2RoadWindow>(false, "Aff转路线", true);//创建窗口
        window.Show();//展示
    }

    private void OnGUI()
    {
        roadPrefab = EditorGUILayout.ObjectField("路的预制件", roadPrefab, typeof(GameObject), false) as GameObject;
        affTextAsset = EditorGUILayout.ObjectField("Arc谱面文件", affTextAsset, typeof(TextAsset), false) as TextAsset;
        line = EditorGUILayout.ObjectField("线", line, typeof(Line), true) as Line;
        width = EditorGUILayout.FloatField("路宽", width);
        track1 = EditorGUILayout.Toggle("轨道1", track1);
        track2 = EditorGUILayout.Toggle("轨道2", track2);
        track3 = EditorGUILayout.Toggle("轨道3", track3);
        track4 = EditorGUILayout.Toggle("轨道4", track4);
        if (GUILayout.Button("好"))
		{
            if (roadPrefab != null && affTextAsset != null && line != null)
            {
                roadParent = new GameObject("Roads").transform;
                nowAngle = line.transform.localEulerAngles;
                lastTurnPosition = line.transform.position - Vector3.up * (roadPrefab.transform.localScale.y / 2 + line.transform.localScale.y / 2);
                lastTurnTime = 0;
                string[] lines = affTextAsset.text.Split('\n');
                bool readingHead = true;
                foreach (string line in lines)
				{
                    if (readingHead == true)
					{
                        if (line == "-")
						{
                            readingHead = false;
                            Debug.Log($"ReadHeadFinish, audioOffset: {-lastTurnTime}");
                            continue;
						}
                        string[] kvp = line.Split(':');
                        if (kvp[0] == "audioOffset") { lastTurnTime = -int.Parse(kvp[1]); }
					}
					else
					{
                        Match match = Regex.Match(line, "^\\((\\d+),(\\d+)\\);$");
                        if (match.Success &&
                            ((int.Parse(match.Groups[2].Value) == 1 && track1) ||
                            (int.Parse(match.Groups[2].Value) == 2 && track2) ||
                            (int.Parse(match.Groups[2].Value) == 3 && track3) ||
                            (int.Parse(match.Groups[2].Value) == 4 && track4))) { Turn(int.Parse(match.Groups[1].Value)); }
                    }
				}
            }
		}
    }

    private void Turn(int time)
	{
        if (lastTurnPosition == null) { return; }
        if (time <= lastTurnTime) { return; }
        float roadLength = line.speed * ((time - lastTurnTime) / 1000) + width;
        Vector3 endPosition = lastTurnPosition + (Quaternion.Euler(nowAngle) * Vector3.forward) * roadLength;
        Transform road = Instantiate(roadPrefab, (lastTurnPosition + endPosition) / 2f, Quaternion.Euler(nowAngle), roadParent).transform;
        road.localScale = new Vector3(width, roadPrefab.transform.localScale.y, roadLength);
        lastTurnTime = time;
        lastTurnPosition = endPosition;
        nowAngle = nowAngle == line.transform.localEulerAngles ? line.nextWay : line.transform.localEulerAngles;
    }
}
