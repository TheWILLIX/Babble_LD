using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cinema : MonoBehaviour
{
    [SerializeField] private Transform _movieScreens;
    [SerializeField] private AudioSources[] _movieAudio;

    private int currentlyPlaying = -1;
    private float volumeSave;

    [Serializable]
    public class AudioSources
    {
        public int Movie;
        public AudioSource audioSource;
    }

    void Start()
    {
        volumeSave = AudioManager.Instance.MusicsVolume;
    }

    public void Play(int movie)
    {
        if (currentlyPlaying == movie)
        {
            for (int i = 0; i < _movieScreens.childCount; i++)
            {
                var child = _movieScreens.GetChild(i);
                if (child.gameObject.activeSelf && child.TryGetComponent<Animator>(out var animator))
                    animator.SetTrigger("Reset");
                _movieScreens.GetChild(i).gameObject.SetActive(i == 0);
            }
            for(int i = 0; i < _movieAudio.Length; i++)
            {
                _movieAudio[i].audioSource.Stop();
            }
            currentlyPlaying = -1;
            AudioManager.Instance.MusicsVolume = volumeSave;
            return;
        }
        else
        {
            AudioManager.Instance.MusicsVolume = 0;
            currentlyPlaying = movie;
            for (int i = 0; i < _movieScreens.childCount; i++)
            {
                if (i == currentlyPlaying && _movieScreens.GetChild(i).TryGetComponent<Animator>(out var animatoreset))
                    animatoreset.SetTrigger("Reset");
                _movieScreens.GetChild(i).gameObject.SetActive(i == movie + 1);
                if (i == movie + 1 && _movieScreens.GetChild(i).TryGetComponent<Animator>(out var animator))
                    animator.SetTrigger("Play");
            }
            for (int i = 0; i < _movieAudio.Length; i++)
            {
                if (_movieAudio[i].Movie == movie)
                    _movieAudio[i].audioSource.Play();
                else
                    _movieAudio[i].audioSource.Stop();
            }
        }
    }
}
