using ClemCAddons;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class OneSidedCollision : MonoBehaviour
{
	public Vector3 direction = Vector3.forward;

	private bool Lock = false;

    void OnCollisionEnter(Collision collision)
    {
		Debug.DrawRay(GetComponent<Collider>().bounds.center, direction, Color.green, 10);
		var collisionDirection = collision.GetContact(0).normal;
		Debug.DrawRay(GetComponent<Collider>().bounds.center, -collisionDirection, Color.red, 10);
		Debug.DrawRay(GetComponent<Collider>().bounds.center, collisionDirection, Color.red, 10);
		if (Vector3.Dot(collisionDirection, direction) > 0)
        {
			if (collision.rigidbody != null)
			{
				GetComponent<Collider>().isTrigger = true;
				collision.rigidbody.velocity = collision.relativeVelocity;
			}
        }
    }
    void OnTriggerExit(Collider other)
    {
		GetComponent<Collider>().isTrigger = false;
	}
}

# if UNITY_EDITOR
[CustomEditor(typeof(OneSidedCollision))]
public class OneSidedCollisionEditor: Editor
{
	private OneSidedCollision Target;

	private bool selected;

    public void OnSceneGUI()
    {
		Target = target as OneSidedCollision;
		ShowPoint();
	}
	public override void OnInspectorGUI()
    {
		EditorGUILayout.PropertyField(serializedObject.FindProperty("direction"));
	}

	private void ShowPoint()
	{
		if (Target.direction == Vector3.zero)
			Target.direction = Vector3.forward;
		var rotation = Tools.pivotRotation == PivotRotation.Local ?
			   Target.transform.rotation : Quaternion.identity;
		Vector3 point = Target.transform.position + Target.direction;
		float size = HandleUtility.GetHandleSize(point);
		Handles.color = Color.white;
		if (Handles.Button(point, rotation, size * 0.1f, size * 0.1f, Handles.DotHandleCap))
		{
			selected = !selected;
			Repaint();
		}
		if (selected)
		{
			EditorGUI.BeginChangeCheck();
			var rot = Handles.DoRotationHandle(Quaternion.identity, Target.transform.position);
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(Target, "Move Target Point");
				EditorUtility.SetDirty(Target);
				Target.direction = rot * Target.direction;
			}
			EditorGUI.BeginChangeCheck();
			var p = Handles.DoPositionHandle(point, Quaternion.identity);
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(Target, "Move Target Point");
				EditorUtility.SetDirty(Target);
				Target.direction = (p - Target.transform.position).normalized;
			}
		}
		Handles.DrawLine(point, Target.transform.position);
	}
}
#endif
