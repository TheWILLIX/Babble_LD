using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


//Fermé pour dégat des eaux !!!
public class VoiceSFXDialogue : MonoBehaviour
{

    #region Fields

    [SerializeField] private bool _desactivateSound = true;

    [Header("Dialogue Info (Obligatoire)")]
    [SerializeField] private TMP_Text _dialogueText = null;
    [SerializeField] private DialogueManager _dialogueManager = null;
    private SpeakerData _currentSpeaker = null;

    [Header("AudioConfig")]
    [SerializeField] private AudioSource _audioSource = null;
    [SerializeField] private AudioClip[] _lettersAudioSounds = null;




    [Header("Audio Tweaking")]
    private float _speedOfSpeaker = 0f;
    private float _minimumPitchOfSpeaker = 1f;
    private float _maximumPitchOfSpeaker = 1f;



    private int _keyOfPreviousSound = 0 ;


    private float _timeStamp = 0f;
    private float _soundLenght = 0f;

    private bool _playingDialogue;


    //Improvement = Make the Silence smaller and more random
    //Make the audio clip look a bit more similar, add effect to all of them or make the sound closer to each other (exemple : Mainly "N" and "Na")
    //Le silence entre les sons doit être beaucoup plus random
    //Enlever la possibilité d'avoir deux silence a la suite   
    //Enlever la possibilité d'avoir deux fois le même son dans la phrase

    #endregion Fields

    #region Properties



    #endregion Properties



    #region Methods
    void Start()
    {
      //  _audioSource.Play();
      //  _startSoundSpeed = _soundSpeedTest;
    }

    void Update() 
    {
        if(_desactivateSound == false)
        {

            if(_playingDialogue == true)
            {

                if(_timeStamp >= _soundLenght - _speedOfSpeaker)
                {
                    _timeStamp = 0f;

                    int keyOfNewSound = 0;

                    keyOfNewSound = Random.Range(0, _lettersAudioSounds.Length);

                    while (keyOfNewSound == _keyOfPreviousSound) //Make sort that it never play a sound twice in a row
                    {
                        keyOfNewSound = Random.Range(0, _lettersAudioSounds.Length);
                    }

                    _keyOfPreviousSound = keyOfNewSound;


                    AudioClip newSoundClip = _lettersAudioSounds[keyOfNewSound];
                    _soundLenght = newSoundClip.length;
                    _audioSource.clip = newSoundClip;
                    _audioSource.pitch = Random.Range(_minimumPitchOfSpeaker, _maximumPitchOfSpeaker);
                    _audioSource.Play();
                    
                }
                else
                {
                    _timeStamp += Time.deltaTime;
                }

            }
        }
        
    }

    

   

    public void StartLettersSFX(SpeakerData speaker) //This is called in the UpdateSpeaker Function (has to be setted up after the speaker)
    {

        int numberOfCharacter = _dialogueText.text.Length;
        _speedOfSpeaker = speaker.SpeedOfLettersSound;
        _minimumPitchOfSpeaker = speaker.MinimumPitchOfLettersSound;
        _maximumPitchOfSpeaker = speaker.MaximumPitchOfLettersSound;

        _playingDialogue = true;
    }

    public void StopLettersSFX()
    {
        _playingDialogue = false;
    }
    #endregion Methods

}
