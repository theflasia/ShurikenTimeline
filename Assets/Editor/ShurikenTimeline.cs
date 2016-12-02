using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using System.Collections.Generic;

public class ShurikenTimeline : EditorWindow
{

#if UNITY_EDITOR

	[MenuItem("Window/ShurikenTimeline")]
	static void ShowWindow()
	{

		EditorWindow.GetWindow<ShurikenTimeline>("ShurikenTimeline");
		EditorWindow.GetWindow<ShurikenTimeline>("ShurikenTimeline").minSize = new Vector2(480, 240);

	}

	ParticleSystem[] particleList;
	List<bool> particleBoolList = new List<bool>();

	float maxDuration = 1.0f;

	bool isPlaying = false;

	bool isFirst = true;

	bool isLoop = true;

	float currentTime = 0.0f;
	float prevTime = 0.0f;

	static string sSceneName = null;

	void OnGUI()
	{

		if(isFirst)
		{
			particleList = FindObjectsOfType(typeof(ParticleSystem)) as ParticleSystem[];
			int len = particleList.Length;
			for(int i = 0; i < len; i++)
			{
				ParticleSystem.MainModule main = particleList[i].main;
				if(maxDuration < main.duration)
				{
					maxDuration = main.duration;
				}
				particleList[i].Stop(true);
				particleBoolList.Add(true);
			}

			isFirst = false;
		}

		EditorGUILayout.BeginVertical();

		GUILayout.Space(5);

		EditorGUILayout.BeginHorizontal();

		GUILayout.FlexibleSpace();
		if(GUILayout.Button("RELOAD", EditorStyles.miniButton))
		{
			ResetWindow();
		}

		EditorGUILayout.EndHorizontal();

		GUILayout.Space(5);

		EditorGUILayout.BeginHorizontal();

		if(!isPlaying)
		{
			if(GUILayout.Button("PLAY", GUILayout.Width(100f), GUILayout.Height(40f)))
			{
				isPlaying = true;
			}
		}
		else
		{
			if(GUILayout.Button("STOP", GUILayout.Width(100f), GUILayout.Height(40f)))
			{
				isPlaying = false;
				StopParticle();
			}
		}

		currentTime = GUILayout.HorizontalSlider(currentTime, 0, maxDuration, "box", "box", GUILayout.Height(40), GUILayout.ExpandWidth(true));

		isLoop = GUI.Toggle(new Rect(5, 70, 100, 20), isLoop, "Looping");

		var centerStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
		centerStyle.alignment = TextAnchor.UpperCenter;

		var rightStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
		rightStyle.alignment = TextAnchor.UpperRight;

		GUI.Label(new Rect(104, 70, 40, 20), "0s");
		GUI.Label(new Rect(position.width - (position.width * 0.5f), 70, 100, 20), (Mathf.Floor(currentTime * 100) / 100).ToString() + "s", centerStyle);
		GUI.Label(new Rect(position.width - 40, 70, 40, 20), maxDuration.ToString() + "s", rightStyle);

		EditorGUILayout.EndHorizontal();

		GUILayout.Space(40);

		// for new line
		int btnNum = 0;
		bool isBeginHorizontal = false;
		bool isEndHorizontal = false;
		if(particleList.Length > 0 && particleBoolList.Count > 0)
		{
			for(int i = 0; i < particleBoolList.Count; i++)
			{
				if(!isBeginHorizontal && btnNum % 3 == 0)
				{
					EditorGUILayout.BeginHorizontal();
					isBeginHorizontal = true;
					isEndHorizontal = false;
				}
				if(particleList[i] != null && IsRoot(particleList[i]))
				{
					particleBoolList[i] = GUILayout.Toggle(particleBoolList[i], particleList[i].name, EditorStyles.miniButton, GUILayout.Width(position.width / 3 - 6f), GUILayout.Height(30));
					if(!isEndHorizontal && btnNum % 3 == 2)
					{
						EditorGUILayout.EndHorizontal();
						isBeginHorizontal = false;
						isEndHorizontal = true;
					}
					btnNum += 1;
				}
				if(!isEndHorizontal && i == particleBoolList.Count - 1)
				{
					EditorGUILayout.EndHorizontal();
					isBeginHorizontal = false;
					isEndHorizontal = true;
				}
			}
		}
		else
		{
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("No Shuriken Object in Scene", GUILayout.Height(40), GUILayout.ExpandWidth(true));
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();
		}

		EditorGUILayout.EndVertical();


		if(Event.current.type == EventType.KeyDown)
		{
			if(Event.current.keyCode == KeyCode.RightArrow)
			{
				currentTime += 0.01f;
				if(currentTime > maxDuration)
				{
					currentTime = 0;
				}
				isPlaying = false;
				StopParticle();
			}

			if(Event.current.keyCode == KeyCode.LeftArrow)
			{
				currentTime -= 0.01f;
				if(currentTime < 0)
				{
					currentTime = maxDuration;
				}
				isPlaying = false;
				StopParticle();
			}
		}

		GUI.Label(new Rect(position.width - 184, position.height - 20, 184, 20), "Shuriken Timeline (version : 0.0.2)", EditorStyles.miniLabel);

	}

	void Update()
	{

		if(sSceneName != SceneManager.GetActiveScene().name)
		{
			sSceneName = SceneManager.GetActiveScene().name;
			ResetWindow();
		}

		if(particleList == null || FindObjectsOfType(typeof(ParticleSystem)) == null)
			return;

		if(particleList.Length != FindObjectsOfType(typeof(ParticleSystem)).Length)
		{
			ResetWindow();
		}

		if(particleList.Length == 0)
		{
			particleBoolList.Clear();
			return;
		}

		if(isPlaying)
		{
			currentTime += 0.01f;
		}

		if(currentTime > maxDuration)
		{
			currentTime = 0;
			if(!isLoop)
			{
				isPlaying = false;
				StopParticle();
				Repaint();
			}
		}
		else if(currentTime != prevTime)
		{
			SimulateParticle();
			prevTime = currentTime;
		}

	}

	void OnHierarchyChange()
	{

		ResetWindow();

	}

	void SimulateParticle()
	{

		GameObject go = Selection.activeObject as GameObject;
		if((!go || !go.GetComponent<ParticleSystem>()) && particleList.Length > 0)
		{
			Selection.activeObject = particleList[particleList.Length - 1];
		}

		if(particleList.Length != particleBoolList.Count)
			return;

		for(int i = 0; i < particleList.Length; i++)
		{
			if(particleList[i] != null && IsRoot(particleList[i]) && particleBoolList[i])
			{
				particleList[i].Simulate(Mathf.Floor(currentTime * 100) * 0.01f, true, true);
			}
		}

		Repaint();

	}

	void StopParticle()
	{

		for(int i = 0; i < particleList.Length; i++)
		{
			if(IsRoot(particleList[i]))
			{
				particleList[i].Simulate(0, true, false);
			}
		}

	}

	bool IsRoot(ParticleSystem ps)
	{

		if(ps == null)
		{
			//ResetWindow();
			return false;
		}

		var parent = ps.transform.parent;

		if(parent == null)
			return true;

		return parent.GetComponent<ParticleSystem>() == false;

	}

	void ResetWindow()
	{

		isFirst = true;
		maxDuration = 0;
		particleBoolList.Clear();
		Repaint();

	}

#endif

}
