using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;
using UnityEngine.SceneManagement;
using System;
using System.Linq;

public class SceneLoader : MonoBehaviour
{
    [Tooltip("Will only be enabled when at least one scene is loaded")]
    [SerializeField] private GameObject[] _loadingPlatforms;
    [SerializeField] private Transform _positionTarget;
    [SerializeField] private Scenes _scenesAreas;
    [SerializeField] private bool _debug;
    [SerializeField, DrawIf("_debug",true,ComparisonType.Equals)] private ELevelType _debugValue;
    [SerializeField] private VisualEffectPlayer _vfxPlayer;
    private string[] _lockedScenes = new string[] { };
    private static string _currentScene;
    private static ELevelType _currentSceneElevelType;
    private static bool DebugB;
    private static ELevelType DebugV;
    private static int _loadedScenes = 1;
    private static List<AsyncOperation> _loadingScenes = new List<AsyncOperation>();

    private AsyncOperation _currentLoading;

    public Scenes ScenesAreas { get => _scenesAreas; set => _scenesAreas = value; }
    public static string CurrentScene { get => _currentScene; }
    public static ELevelType CurrentSceneElevelType { get { return DebugB ? DebugV : _currentSceneElevelType; } }

    public Transform PositionTarget { get => _positionTarget; set => _positionTarget = value; }

    [Serializable]
    public class Scenes
    {
        public string[] Names;
        public Transform[] Areas;
        public ELevelType[] LevelTypes;
        public Scenes(string[] scenes, Transform[] areas, ELevelType[] levelType)
        {
            Names = scenes;
            Areas = areas;
            LevelTypes = levelType;
        }
    }

    void Start()
    {
        DebugB = _debug;
        DebugV = _debugValue;
    }

    void Update()
    {
        if (_debug || Time.timeScale == 0)
            return;
        if (ClemCAddons.Utilities.Timer.MinimumDelay(777, 100))
        {
            for(int i = _loadingScenes.Count - 1; i >= 0; i--)
            {
                var operation = _loadingScenes[i];
                if (operation.isDone)
                {
                    _loadedScenes++;
                    _loadingScenes.RemoveAt(i);
                }
            }
            if(SceneManager.sceneCount > 1)
            {
                if (SceneManager.GetSceneAt(0) == gameObject.scene)
                    _currentScene = SceneManager.GetSceneAt(1).name;
                else
                    _currentScene = SceneManager.GetSceneAt(0).name;
            }
            try
            {
                if (_currentScene != null)
                    _currentSceneElevelType = _scenesAreas.LevelTypes[_scenesAreas.Names.FindIndex(_currentScene)];
            }
            catch(Exception ex)
            {
                Debug.LogError("(Scene Loader) Scene "+_currentScene+" not in build settings!!!!\n"+ex);
            }
            for (int i = 0; i < _scenesAreas.Names.Length; i++)
            {
                if (_scenesAreas.Areas[i].GetComponent<ChunkEntrance>().IsInside)
                {
                    bool valid = true;
                    for (int t = 0; t < SceneManager.sceneCount; t++)
                    {
                        if (SceneManager.GetSceneAt(t).name == _scenesAreas.Names[i])
                            valid = false;
                    }
                    if (valid && !_lockedScenes.Contains(_scenesAreas.Names[i]))
                    {
                        LoadScene(_scenesAreas.Names[i]);
                    }
                }
            }
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                if (SceneManager.GetSceneAt(i) == gameObject.scene)
                    continue;
                var r = _scenesAreas.Names.Select((t, index) => new { T = t, Index = index }).Where(o => o.T == SceneManager.GetSceneAt(i).name).Select(o => o.Index);
                if (r.Count() > 0)
                {
                    bool v = true;
                    foreach(var t in r)
                    {
                        if (_scenesAreas.Areas[t].GetComponent<ChunkEntrance>().IsInside)
                        {
                            v = false;
                        }
                    }
                    if (v && !_lockedScenes.Contains(SceneManager.GetSceneAt(i).name))
                    {
                        UnloadScene(SceneManager.GetSceneAt(i).name);
                        return;
                    }
                }
                else
                {
                    Debug.LogWarning("There is a loaded scene outside the system");
                }
            }
        }
        if ((SceneManager.sceneCount <= 1).OnceIfTrueGate("essentials".GetHashCode()))
        {
            foreach (GameObject go in _loadingPlatforms)
                go.SetActive(true);
        }
        if((_loadedScenes > 1).OnceIfTrueGate("essentials too".GetHashCode()))
        {
            LoadingScreen.Hide(null, _loadingPlatforms);
        }
    }


    private void LoadScene(string name)
    {
        bool inbuild = false;
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            var path = SceneUtility.GetScenePathByBuildIndex(i);
            var paths = path.Split('/');
            if(paths[paths.Count()-1] == name+".unity")
                inbuild = true;
        }
        if (!inbuild)
            return;
        if(SceneManager.sceneCount <= 1)
        {
            LoadingScreen.Show();
            Application.backgroundLoadingPriority = ThreadPriority.BelowNormal;
        }
        else
            Application.backgroundLoadingPriority = ThreadPriority.Low;
        Debug.Log("Loading " + name);
        _lockedScenes.Add(name);
        _ = ClemCAddons.Utilities.GameTools.DelayedCall(100, () =>
        {
            var operation = SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);
            if (operation != null)
            {
                _loadingScenes.Add(operation);
            }
            operation.completed += (AsyncOperation) => { _lockedScenes.RemoveAll(name); }; // when it's done, remove from locked scenes
        });
    }

    private void UnloadScene(string name)
    {
        Application.backgroundLoadingPriority = ThreadPriority.Low;
        Debug.Log("Unloading " + name);
        SceneManager.UnloadSceneAsync(name);
        _loadedScenes--;
    }

    public void UberTravelPreLoad(string fromLvl, string toLvl, Action callback)
    {
        Debug.Log("Initializing Uber from " + fromLvl + " to " + toLvl);
        _currentScene = fromLvl;
        _currentSceneElevelType = _scenesAreas.LevelTypes[_scenesAreas.Names.FindIndex(fromLvl)];
        _lockedScenes = _lockedScenes.Add(toLvl).Add(fromLvl);
        bool valid = true;
        for (int t = 0; t < SceneManager.sceneCountInBuildSettings; t++)
        {
            if (SceneManager.GetSceneByBuildIndex(t).name == toLvl)
                valid = false;
        }
        if (valid)
        {
            _vfxPlayer.StartTransi(() =>
            {
                Time.timeScale = 0;
                LoadingScreen.Show();
                _ = ClemCAddons.Utilities.GameTools.DelayedCall(100, () =>
                {
                    Application.backgroundLoadingPriority = ThreadPriority.High;
                    _currentLoading = SceneManager.LoadSceneAsync(toLvl, LoadSceneMode.Additive);
                    _vfxPlayer.Active = false;
                    callback.Invoke();
                });
            });
        }
    }

    public void ExecuteUberTravel(Transform player, Vector3 targetPosition, string fromLevel, string newLevel)
    {
        StartCoroutine(UberLoading(player, targetPosition, fromLevel, newLevel));
    }

    IEnumerator UberLoading(Transform player, Vector3 targetPosition, string fromLevel, string newLevel)
    {
        Debug.Log("Waiting for uber to be ready");
        while (_currentLoading != null && !_currentLoading.isDone)
            yield return new WaitForSecondsRealtime(0.1f);
        Debug.Log("Uber traveling from " + fromLevel + " to " + newLevel);
        player.GetComponent<Rigidbody>().velocity = Vector3.zero;
        player.GetComponent<Rigidbody>().position = targetPosition;
        _vfxPlayer.Active = true;
        _currentScene = newLevel;
        _currentSceneElevelType = _scenesAreas.LevelTypes[_scenesAreas.Names.FindIndex(newLevel)];
        _lockedScenes = _lockedScenes.RemoveAll(fromLevel);
        LoadingScreen.Hide(
            () =>
            {
                _vfxPlayer.StartReverseTransi(() => {
                    _vfxPlayer.Active = false;
                    _ = ClemCAddons.Utilities.GameTools.DelayedCall(100, () =>
                    {
                        _lockedScenes = _lockedScenes.RemoveAll(newLevel);
                        BeamPush.ForcedDirection = 0;
                    });
                });
            }
            );
    }
}
