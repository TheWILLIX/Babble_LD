using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using ClemCAddons;

public class VFXSpawner : MonoBehaviour
{
    [SerializeField] private VFXData[] _data;

    private static VFXSpawner _instance;

    public static VFXSpawner Instance {
        get
        {
            if (_instance != null)
                return _instance;
            _instance = FindObjectOfType<VFXSpawner>();
            if (_instance != null)
                return _instance;
            var r = Resources.Load<GameObject>("VFXSpawner");
            _instance = Instantiate(r).GetComponent<VFXSpawner>();
            return _instance;
        }
        set => _instance = value; }

    void Start()
    {
        _instance = this;
    }

    private bool IsValidSpawn(Vector3 position)
    {
        var point = Camera.allCameras[0].WorldToViewportPoint(position); // 00 to 11
        return point.z >= 0 && point.ToVector2().IsBetween(new Vector2(-0.2f, -0.2f), new Vector2(1.2f, 1.2f));
    }

    [Serializable]
    public class VFXData
    {
        public string Name;
        public GameObject Prefab;
    }

    private void SpawnVFXInternal(string name, Vector3 position, Quaternion rotation)
    {
        if (!IsValidSpawn(position))
            return;
        var r = _data.First(t => t.Name == name);
        Instantiate(r.Prefab, position, rotation, transform);
    }

    private GameObject SpawnVFXInternal(string name, Vector3 position, Quaternion rotation, Transform transform)
    {
        var r = _data.First(t => t.Name == name);
        return Instantiate(r.Prefab, position, rotation, transform);
    }

    public static void Spawn(string name, Vector3 position, Quaternion rotation = default)
    {
        Instance.SpawnVFXInternal(name, position, rotation);
    }

    public static GameObject Spawn(string name, Transform parent, Vector3 position, Quaternion rotation = default)
    {
        return Instance.SpawnVFXInternal(name, position, rotation, parent);
    }
}
