using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using ClemCAddons;
using System.Linq;
using UnityEngine.SceneManagement;
using ClemCAddons.Player;
using ClemCAddons.CameraAndNodes;

public class UberStation : MonoBehaviour
{
    [SerializeField] public int _stationID = -1;
    [SerializeField] private int _targetStation = -1;
    public Vector3 ExitPoint = Vector3.positiveInfinity;
    public Vector3 CamPosition = Vector3.positiveInfinity;
    [SerializeField]
    public int SelectedScene = -1;
    [SerializeField]
    public string TargetMap = "";

    [SerializeField] private bool _marineCurrentMode = false;
    [SerializeField] private int _direction = 1;

    public void OnTriggerEnter(Collider other) // temporary activation
    {
        Debug.Log("going for tp");
        var cam = FindObjectOfType<TPSCameraWithNodeSupport>();
        cam.TransiMode = true;
        ClemCAddons.Utilities.Lerper.ConstantLerp(cam.transform.position, transform.position + CamPosition, 3,
        (v) =>
        {
            cam.transform.position = v;
        }, () =>
        {
            _ = ClemCAddons.Utilities.GameTools.DelayedCall(100, () =>
            {
                var scene = gameObject.scene.name;
                FindObjectOfType<SceneLoader>().UberTravelPreLoad(scene, TargetMap, () =>
                {
                    _ = ClemCAddons.Utilities.GameTools.DelayedCall(3000, () => { UberTravel(scene); });
                });
            });
            
        });
    }

    public void UberTravel(string fromLevel) // can be used in final version, requires the target scene to be preloaded (SceneLoader.UberTravelPreLoad)
    {
        StartCoroutine(UberTravelInternal(fromLevel));
    }

    IEnumerator UberTravelInternal(string fromLevel)
    {
        Debug.Log("Waiting for scene to load loading");
        UberStation station = null;
        while (station == null)
        {
            yield return null;
            station = FindObjectsOfType<UberStation>().Where(t => t._stationID == _targetStation).FirstOrDefault();
        }
        Debug.Log("Scene loaded");
        Vector3 position = transform.position; // just in case somehow the pc lags enough to fail the debug, it's still not teleporting to 0,0
        position = station.transform.position + station.ExitPoint;
        if (_marineCurrentMode)
            BeamPush.ForcedDirection = _direction;
        var player = FindObjectOfType<CharacterMovement>();
        FindObjectOfType<SceneLoader>().ExecuteUberTravel(player.transform, position, fromLevel, TargetMap);
        var cam = FindObjectOfType<TPSCameraWithNodeSupport>();
        cam.TransiMode = false;
    }

    public void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, transform.position + ExitPoint);

        Gizmos.DrawLine(transform.position, transform.position + CamPosition);
    }
    public void OnDrawGizmosSelected()
    {
        Gizmos.DrawSphere(transform.position + ExitPoint, 0.2f);

        Gizmos.DrawSphere(transform.position + CamPosition, 0.2f);
    }
}
#if(UNITY_EDITOR)
[CustomEditor(typeof(UberStation))] 
public class UberStationEditor : Editor
{
    public void OnEnable()
    {
        if ((target as UberStation)._stationID == -1)
        {
            var rand = new System.Random();
            (target as UberStation)._stationID = rand.Next(100000);
        }
        if ((target as UberStation).ExitPoint.Equals(Vector3.positiveInfinity))
        {
            (target as UberStation).ExitPoint = Vector3.up + Vector3.left;
        }
        if ((target as UberStation).CamPosition.Equals(Vector3.positiveInfinity))
        {
            (target as UberStation).CamPosition = Vector3.up + Vector3.left;
        }
    }

    public override void OnInspectorGUI()
    {
        var station = target as UberStation;
        serializedObject.Update();
        if(GUILayout.Button("Generate Station ID"))
        {
            var rand = new System.Random();
            station._stationID = rand.Next(100000);
            EditorUtility.SetDirty(station);
        }
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_stationID"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("_targetStation"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("ExitPoint"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("CamPosition"));
        GUILayout.Label("Target Map:");
		var scenes = EditorBuildSettings.scenes;
		var scenesNames = new string[] { };
		GUIContent[] content = new GUIContent[] { };
		foreach (var scene in scenes)
		{
			content = content.Add(new GUIContent(scene.path.Split('/').Last().Replace(".unity", "").Replace("_", " ")));
			scenesNames = scenesNames.Add(scene.path.Split('/').Last().Replace(".unity", ""));
		}
		var r = GUILayout.SelectionGrid(station.SelectedScene, content, 2);
		if (r != station.SelectedScene)
		{
			station.SelectedScene = r;
            station.TargetMap = scenesNames[r];
            EditorUtility.SetDirty(station);
        }

        EditorGUILayout.PropertyField(serializedObject.FindProperty("_marineCurrentMode"));
        if(serializedObject.FindProperty("_marineCurrentMode").boolValue)
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_direction"));
        serializedObject.ApplyModifiedProperties();
    }
}
#endif