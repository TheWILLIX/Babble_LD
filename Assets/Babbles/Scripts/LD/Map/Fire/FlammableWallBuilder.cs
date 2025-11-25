using System.Linq;
using UnityEditor;
using UnityEngine;
using ClemCAddons;
using System.Text;
using UnityEngine.Events;
using System;

public class FlammableWallBuilder : MonoBehaviour
{
	public int Health = 100;
	public int FireLevel = 0;
	public int Resistance = 50;
	public int MinimumContaminationFireLevel = 50;
	public GameObject VFX;
	public WallEvent Ablaze;
	public WallEvent Burned;

	[Serializable]
	public class WallEvent : UnityEvent { };

	void Awake()
    {
		FireLevel = 0;
    }
#if UNITY_EDITOR
	public void Add(Vector3 direction)
    {
		var r = FindObjectsOfType<FlammableWallBuilder>();
		var idealPos = transform.position + transform.rotation * (direction.Multiply(transform.lossyScale));
		foreach (FlammableWallBuilder res in r)
        {
			if((res.transform.position - idealPos).magnitude < 0.1f)
            {
				Selection.objects = Selection.objects.Add(res.gameObject);
				return;
            }
        }
		var t = Instantiate(gameObject, idealPos, transform.rotation, transform.parent);
		Selection.objects = Selection.objects.Add(t);
		Undo.RegisterCreatedObjectUndo(t, "Cloned " + name);
	}
#endif

	public void SetAblaze()
    {
		Resistance = 0;
		FireLevel = FireLevel == 0 ? 1 : FireLevel;
		Ablaze.Invoke();
		Instantiate(VFX, transform.position, Quaternion.identity, transform);
	}

	void Update()
    {
		if (FireLevel > 0)
		{
			if (ClemCAddons.Utilities.Timer.MinimumDelay(this.GetHashCode(), 100, true))
			{
				var r = FindObjectsOfType<FlammableWallBuilder>();
				var distance = transform.lossyScale.magnitude+0.1f;
				foreach (FlammableWallBuilder res in r)
				{
					if (FireLevel >= MinimumContaminationFireLevel && res.transform.position.Distance(transform.position) < distance)
					{
						if (res.Resistance > 0)
							res.Resistance -= Mathf.FloorToInt(Mathf.Max(FireLevel * 0.1f, 1) * Time.deltaTime * 100);
					}
				}
				Health -= FireLevel;
				FireLevel += Mathf.FloorToInt(Mathf.Max(FireLevel * 0.1f, 1) * Time.deltaTime * 100);
				if (FireLevel > 100)
					FireLevel = 100;
				if (Health < 0)
                {
					enabled = false;
					for(int i = 0; i < transform.childCount; i++)
                    {
						transform.GetChild(i).gameObject.SetActive(false);
                    }
					Burned.Invoke();
					_ = ClemCAddons.Utilities.GameTools.DelayedCall(1000, () => {
						Destroy(gameObject);
					});
				}
			}
		}
		else if (Resistance <= 0)
		{
			FireLevel = 1;
			Resistance = 0;
			if (VFX != null)
				Instantiate(VFX, transform.position, Quaternion.identity, transform);
		}
	}

#if UNITY_EDITOR
	public void Select(Vector3 direction)
	{
		var r = FindObjectsOfType<FlammableWallBuilder>();
		var idealPos = transform.position + transform.rotation * (direction.Multiply(transform.lossyScale));
		foreach (FlammableWallBuilder res in r)
		{
			if ((res.transform.position - idealPos).magnitude < 0.1f)
			{
				Selection.objects = Selection.objects.Add(res.gameObject);
				return;
			}
		}
	}
	public void SelectAll()
	{
		Select(Vector3.left);
		Select(Vector3.right);
		Select(Vector3.up);
		Select(Vector3.down);
	}
	public void Unselect()
	{
		Selection.objects = Selection.objects.RemoveAll(gameObject);
	}
}

[CustomEditor(typeof(FlammableWallBuilder))]
[CanEditMultipleObjects]
public class FlammableWallBuilderInspector : Editor
{
	protected virtual void OnSceneGUI()
    {
		if(Event.current.type == EventType.Repaint)
		{
			Transform transform = ((FlammableWallBuilder)target).transform;
			Handles.color = Color.red;
			Handles.ArrowHandleCap(
				0,
				transform.position,
				transform.rotation * Quaternion.LookRotation(Vector3.forward),
				1,
				EventType.Repaint
			);
		}
    }
	public override void OnInspectorGUI()
	{
		var bl = new SerializedObject(targets);

		// ADD / NAVIGATION
		EditorGUI.BeginChangeCheck();
		if (GUILayout.Button("\u25B2"))
		{
			var t = targets;
			foreach (FlammableWallBuilder builder in t)
			{
				builder.Unselect();
			}
			foreach (FlammableWallBuilder builder in t)
			{
				builder.Add(Vector3.up);
				EditorUtility.SetDirty(builder);
			}
		}
		GUILayout.BeginHorizontal();
		EditorGUI.BeginChangeCheck();
		if (GUILayout.Button("\u25C0", new GUILayoutOption[] { GUILayout.Width(100) }))
		{
			var t = targets;
			foreach (FlammableWallBuilder builder in t)
			{
				builder.Unselect();
			}
			foreach (FlammableWallBuilder builder in t)
			{
				builder.Add(Vector3.right);
				EditorUtility.SetDirty(builder);
			}
		}
		GUILayout.FlexibleSpace();
		GUILayout.Label("Add / Navigate", new GUIStyle(GUI.skin.label) { fontSize = 15 });
		GUILayout.FlexibleSpace();
		EditorGUI.BeginChangeCheck();
		if (GUILayout.Button("\u25B6", new GUILayoutOption[] { GUILayout.Width(100) }))
		{
			var t = targets;
			foreach (FlammableWallBuilder builder in t)
			{
				builder.Unselect();
			}
			foreach (FlammableWallBuilder builder in t)
			{
				builder.Add(Vector3.left);
				EditorUtility.SetDirty(builder);
			}
		}
		GUILayout.EndHorizontal();
		EditorGUI.BeginChangeCheck();
		if (GUILayout.Button("\u25BC"))
		{
			var t = targets;
			foreach (FlammableWallBuilder builder in t)
			{
				builder.Unselect();
			}
			foreach (FlammableWallBuilder builder in t)
			{
				builder.Add(Vector3.down);
				EditorUtility.SetDirty(builder);
			}
		}
		// SPACING

		GUILayout.Space(10);

		// EXTEND SELECTION

		EditorGUI.BeginChangeCheck();
		if (GUILayout.Button("\u2191\u2191"))
		{
			var t = targets;
			foreach (FlammableWallBuilder builder in t)
			{
				builder.Select(Vector3.up);
				EditorUtility.SetDirty(builder);
			}
		}
		GUILayout.BeginHorizontal();
		EditorGUI.BeginChangeCheck();
		if (GUILayout.Button("\u2190\u2190", new GUILayoutOption[] { GUILayout.Width(100) }))
		{
			var t = targets;
			foreach (FlammableWallBuilder builder in t)
			{
				builder.Select(Vector3.right);
				EditorUtility.SetDirty(builder);
			}
		}
		GUILayout.FlexibleSpace();
		GUILayout.Label("Extend Selection", new GUIStyle(GUI.skin.label) { fontSize = 15 });
		GUILayout.FlexibleSpace();
		EditorGUI.BeginChangeCheck();
		if (GUILayout.Button("\u2192\u2192", new GUILayoutOption[] { GUILayout.Width(100) }))
		{
			var t = targets;
			foreach (FlammableWallBuilder builder in t)
			{
				builder.Select(Vector3.left);
				EditorUtility.SetDirty(builder);
			}
		}
		GUILayout.EndHorizontal();
		EditorGUI.BeginChangeCheck();
		if (GUILayout.Button("\u2193\u2193"))
		{
			var t = targets;
			foreach (FlammableWallBuilder builder in t)
			{
				builder.Select(Vector3.down);
				EditorUtility.SetDirty(builder);
			}
		}

		GUILayout.Space(10);

		EditorGUI.BeginChangeCheck();
		if (GUILayout.Button("Unselect", new GUIStyle(GUI.skin.button) { fontSize = 15 }))
		{
			var t = targets;
			foreach (FlammableWallBuilder builder in t)
			{
				builder.Unselect();
			}
		}

		GUILayout.Space(10);
		bool destroyed = false;
		EditorGUI.BeginChangeCheck();
		if (GUILayout.Button("Remove", new GUIStyle(GUI.skin.button) { fontSize = 15 }))
		{
			var t = targets;
			foreach (FlammableWallBuilder builder in t)
			{
				builder.SelectAll();
				builder.Unselect();
				DestroyImmediate(builder.gameObject);
				destroyed = true;
			}
		}
		GUILayout.Space(10);



		// SETTINGS
		GUILayout.BeginHorizontal();
		GUILayout.Label("Health");
		EditorGUILayout.PropertyField(bl.FindProperty("Health"),new GUIContent(""));
		GUILayout.Label("Resistance");
		EditorGUILayout.PropertyField(bl.FindProperty("Resistance"), new GUIContent(""));
		GUILayout.EndHorizontal();
		EditorGUILayout.IntSlider(bl.FindProperty("MinimumContaminationFireLevel"), 0, 100, new GUIContent("Minimum Fire Spread"));
		GUI.enabled = false;
		EditorGUILayout.PropertyField(bl.FindProperty("FireLevel"), new GUIContent("Fire Level"));
		GUI.enabled = true;
		EditorGUILayout.PropertyField(bl.FindProperty("VFX"), new GUIContent("VFX"));
		
		GUILayout.Space(10);

		EditorGUI.BeginChangeCheck();
		if (GUILayout.Button("Set Ablaze", new GUIStyle(GUI.skin.button) { fixedHeight = 30, fontSize = 15, fontStyle = FontStyle.Bold}))
		{
			foreach (FlammableWallBuilder builder in targets)
			{
				builder.SetAblaze();
				EditorUtility.SetDirty(builder);
			}
		}

		GUILayout.Space(10);
		EditorGUILayout.PropertyField(bl.FindProperty("Ablaze"));
		EditorGUILayout.PropertyField(bl.FindProperty("Burned"));
		if (!destroyed)
			bl.ApplyModifiedProperties();

	}
#endif

}
