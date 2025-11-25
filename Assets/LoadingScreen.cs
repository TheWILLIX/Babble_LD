using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;
using UnityEngine.Video;
using TMPro;
using System;

public class LoadingScreen : MonoBehaviour
{
    [SerializeField] private float _delayAfterLoading = 5;
    private bool _visible;
    [TextArea]
    [SerializeField] private string _hintTag = "LoadingTip";
    [SerializeField] private int _hintCount = 9;
    [SerializeField] private TMP_Text _textTMP = null;
    [SerializeField] private bool _debugNoTimeScale;
    private float _delay;
    private GameObject[] _loadingPlatforms = new GameObject[0];
    private static LoadingScreen _instance;
    private Action _actionFinished;
    private bool _isNewLoadingScreen = false;

    void Start()
    {
        _instance = this;
        _delay = 4;
        Hide(null);
    }

    void Update()
    {
        if(_visible)
        {
            _delay += Time.unscaledDeltaTime;

            if (_isNewLoadingScreen == false)
            {
                _isNewLoadingScreen = true;
                int rand = UnityEngine.Random.Range(0, _hintCount);
                _textTMP.text = "$"+_hintTag+rand;
            }
        }
            
   

        if (!_visible && _delay != 0)
        {
            if(_delay >= _delayAfterLoading * 0.75f)
                Time.timeScale = _debugNoTimeScale ? 0 : 1;
            if (_delay >= _delayAfterLoading)
            {
                Time.timeScale = _debugNoTimeScale ? 0 : 1;
                _delay = 0;
                _instance.transform.GetChild(0).gameObject.SetActive(false);
                _instance.transform.FindDeep("Video").GetComponent<VideoPlayer>().Stop();
                _isNewLoadingScreen = false;

                foreach (GameObject go in _loadingPlatforms)
                    go.SetActive(false);
                if(_actionFinished != null)
                    _actionFinished.Invoke();
                return;
            }
            _delay += Time.unscaledDeltaTime;
        }
    }

    public static void Show()
    {
        _instance._visible = true;
        _instance.transform.GetChild(0).gameObject.SetActive(true);
        var player = _instance.transform.FindDeep("Video").GetComponent<VideoPlayer>();
        player.time = 0;
        player.Play();
        
    }

    public static void Hide(Action finished, GameObject[] loadingPlatforms = null)
    {
        if (_instance._delay == 0)
            _instance._delay += Time.unscaledDeltaTime;
        if(loadingPlatforms == null)
            loadingPlatforms = new GameObject[0];
        _instance._loadingPlatforms = loadingPlatforms;
        _instance._visible = false;
        _instance._actionFinished = finished;
    }
}
