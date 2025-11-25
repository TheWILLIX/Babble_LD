using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : Singleton<AudioManager>
{

    #region Fields
    [Header("Sound Datas")]
    [Tooltip("Place Every Sound Data you want to use in here ! (Watch out to not let a slot empty)")]
    [SerializeField] private SoundData[] _soundDatas = null; //THE SOUND DATA TABLE, 
    [SerializeField] private SoundData[] _musicDatas = null;

    [SerializeField] private SoundData[] _soundDatasDecors = null;
    [SerializeField] private SoundData[] _soundDatasExploration= null;
    [SerializeField] private SoundData[] _soundDatasInteractionPNJ = null;
    [SerializeField] private SoundData[] _soundDatasObjetsSpeciaux = null;
    [SerializeField] private SoundData[] _soundDatasMenus = null;




    [Header("Sound & Music Sources")]
    [Tooltip("2D Sound Source is the prefab model used for all 2D non-looped 'Sound Datas' (Find it in the 'Prefabs/Audio' folder)")]
    [SerializeField] private AudioSource _2DSoundSource = null;
    [Tooltip("3D Sound Source is the prefab model used for all 3D non-looped 'Sound Datas' (Find it in the 'Prefabs/Audio' folder)")]
    [SerializeField] private AudioSource _3DSoundSource = null;

    [Tooltip("2D Repetitive Sound Source is the prefab model used for all 2D looped 'Sound Datas' (Find it in the 'Prefabs/Audio' folder)")]
    [SerializeField] private AudioSource _2DRepetitiveSoundSource = null;
    [Tooltip("3D Repetitive Sound Source is the prefab model used for all 3D looped 'Sound Datas' (Find it in the 'Prefabs/Audio' folder)")]
    [SerializeField] private AudioSource _3DRepetitiveSoundSource = null;
    [Space]

    [Tooltip("Music Source is a unique 2D AudioSource, it's children of the AudioManager")]
    [SerializeField] private AudioSource _ambiantSource = null;
    [Tooltip("Transition Source is a 2D Audiosource used to do fades and transition. It's also children of the AudioManager")]
    [SerializeField] private AudioSource _transitionSource = null;

    [SerializeField] private AudioSource _NPCThemeSource = null;
    private bool _npcThemePlaying = false;
    [SerializeField] private AudioSource _NPCNearbySource = null;



    [Header("Volumes")]
    [Range(0.0f, 1.0f)]
    [SerializeField] private float _soundsVolume = 1f;
    [Range(0.0f, 1.0f)]
    [SerializeField] private float _musicsVolume = 1f;
    [Range(0.0f, 1.0f)]
    [SerializeField] private float _npcVolume = 1f;
    [Space]

    private Dictionary<string, SoundData> _soundData = null;

    private Dictionary<string, AudioSource> _2DSources = null;
    private Dictionary<string, AudioSource> _2DRepetitiveSources = null;

    private Dictionary<string, AudioSource> _3DSources = null;
    private Dictionary<string, AudioSource> _3DRepetitiveSources = null;

    private float _npcNearbyThemeTime = 0f;
    private bool _npcNearbyActive = false;


    //Timer Réference/Attribut
    [Header("Default Timer Value")]
    [Tooltip("Default value and suggested value is 0.1f")]
    [SerializeField] private float _fadeTick = 0.1f;

    //--- FADE IN
    private float _timeToFadeIn = 3f;

    private Timer _timerFadeInTick = null;
    private float _volumeFadeInTarget = 1f;
    private float _fadeInTickVolumeValue = 0;


    //--- FADE OUT
    private float _timeToFadeOut = 3f;

    private Timer _timerFadeOutTick = null;
    private float _volumeFadeOutTarget = 1f;
    private float _fadeOutTickVolumeValue = 0;

    //OTHERS
    private AudioSource _audioSourceToFadeOut = null;
    private bool _pauseAtFadeOutEnding = false;



    #region Resuming & Pausing Ambiant Timer
    //RESUMINGAMBIANT FADEIN
    private float _resAmbiantTimeToFadeIn = 3f;

    private Timer _resAmbiantTimerFadeInTick = null;
    private float _resAmbiantVolumeFadeInTarget = 1f;
    private float _resAmbiantFadeInTickVolumeValue = 0;
    private bool _resumingAmbiant = false;


    //PAUSINGAMBIANT FADEOUT
    private float _pausAmbiantTimeToFadeOut = 3f;

    private Timer _pausAmbiantTimerFadeOutTick = null;
    private float _pausAmbiantVolumeFadeOutTarget = 1f;
    private float _pausAmbiantFadeOutTickVolumeValue = 0;
    private bool _pausingAmbiant = false;

    #endregion Resuming & Pausing Ambiant Timer


    [Header("Optional")]
    [Tooltip("Experimental Method that destroy the sound after it played. Set to true if you want it to be active")]
    [SerializeField] private bool _activateSoundDestroyer = true;

    [Space]
    [Tooltip("True = the 'Music Data On Start' will start playing when the game launch")]
    [SerializeField] private bool _playMusicOnStart = false;

    [Tooltip("Place the SoundData of the music you want to play on start (If 'Play Music On Start' is set to false nothing will happen)")]
    [SerializeField] private SoundData _musicDataOnStart = null;


    [SerializeField] private bool _noMusic = false;
    //DIRTY
    private bool _isPlayingChallengeMusic;
    #endregion Fields

    #region Property
    public bool NPCThemePlaying => _npcThemePlaying;
    public float SoundsVolume
    {
        get
        {
            return _soundsVolume;
        }
        set
        {
            _soundsVolume = Mathf.Clamp(value, 0, 1);
            MainSoundVolumeUpdate();
        }
    }

    public float MusicsVolume
    {
        get
        {
            return _musicsVolume;
        }
        set
        {
            _musicsVolume = Mathf.Clamp(value, 0, 1);
            MusicVolumeUpdate();
        }
    }

    public float NPCNearbyVolume
    {
        get
        {
            return _npcVolume;
        }
        set
        {
            _npcVolume = Mathf.Clamp(value, 0, 1);
            _NPCNearbySource.volume = _npcVolume;
        }
    }

    public float NPCThemeVolume
    {
        get
        {
            return _npcVolume;
        }
        set
        {
            _npcVolume = Mathf.Clamp(value, 0, 1);
            _NPCThemeSource.volume = _npcVolume;
        }
    }

    public AudioSource AmbiantSource { get => _ambiantSource;  }
    public bool IsPlayingChallengeMusic { get => _isPlayingChallengeMusic; set => _isPlayingChallengeMusic = value; }


    #endregion Property

    #region Methods



    #region Start
    protected override void Start()
    {
         
        _soundData = new Dictionary<string, SoundData>();

        if (_soundDatas.Length >= 1 || _musicDatas.Length >= 1)    //CHECK IF ANY SOUND DATAS EXIST
        {
            for (int i = 0; i < _soundDatas.Length; i++)    //SETUP THE DICTIONARY CORRECTLY
            {
                _soundData.Add(_soundDatas[i].Key, _soundDatas[i]);
            }

            _2DSources = new Dictionary<string, AudioSource>();
            _2DRepetitiveSources = new Dictionary<string, AudioSource>();

            _3DSources = new Dictionary<string, AudioSource>();
            _3DRepetitiveSources = new Dictionary<string, AudioSource>();


            _timerFadeInTick = new Timer(); //SET UP THE TIMER CORRECTLY (USED FOR MUSIC TRANSITIONS)
            _timerFadeInTick.OnTick += FadeInTick;

            _timerFadeOutTick = new Timer();
            _timerFadeOutTick.OnTick += FadeOutTick;

            //RESUMING AND PAUSING AMBIANT TIMERS
            _resAmbiantTimerFadeInTick = new Timer();
            _resAmbiantTimerFadeInTick.OnTick += ResumeAmbiantFadeInTick;

            _pausAmbiantTimerFadeOutTick = new Timer();
            _pausAmbiantTimerFadeOutTick.OnTick += PauseAmbiantFadeOutTick;
        }

        #region Sound Datas Category Configs
        if (_musicDatas.Length >=1)
        {
            for( int i = 0; i < _musicDatas.Length; i++)
            {
                _soundData.Add(_musicDatas[i].Key, _musicDatas[i]);
            }
        }

        if (_soundDatasDecors.Length >= 1)
        {
            for (int i = 0; i < _soundDatasDecors.Length; i++)
            {
                _soundData.Add(_soundDatasDecors[i].Key, _soundDatasDecors[i]);
            }
        }

        if (_soundDatasExploration.Length >= 1)
        {
            for (int i = 0; i < _soundDatasExploration.Length; i++)
            {
                _soundData.Add(_soundDatasExploration[i].Key, _soundDatasExploration[i]);
            }
        }

        if (_soundDatasInteractionPNJ.Length >= 1)
        {
            for (int i = 0; i < _soundDatasInteractionPNJ.Length; i++)
            {
                _soundData.Add(_soundDatasInteractionPNJ[i].Key, _soundDatasInteractionPNJ[i]);
            }
        }

        if (_soundDatasObjetsSpeciaux.Length >= 1)
        {
            for (int i = 0; i < _soundDatasObjetsSpeciaux.Length; i++)
            {
                _soundData.Add(_soundDatasObjetsSpeciaux[i].Key, _soundDatasObjetsSpeciaux[i]);
            }
        }

        if(_soundDatasMenus.Length >= 1 )
        {
            for (int i = 0; i < _soundDatasMenus.Length; i++)
            {
                _soundData.Add(_soundDatasMenus[i].Key, _soundDatasMenus[i]);
            }
        }


        #endregion Sound Datas Category Configs

        #region Play Music On Start
        if (_playMusicOnStart == true)
        {
            if (_musicDataOnStart == null)
            {
                Debug.LogWarning("You have to setup the 'Music Data On Start' with the corresponding SoundData of the Music");
            }
            else
            {
                AudioSource source = AmbiantSource;

                AudioClip clipToPlay = _musicDataOnStart.Clip;

                source.volume = (_musicDataOnStart.Volume * _soundsVolume);

                source.pitch = _musicDataOnStart.Pitch;

                source.loop = _musicDataOnStart.Loop;

                source.clip = clipToPlay;

                source.Play();
            }
        }
        #endregion Play Music On Start

        if (_noMusic == true) {
            _musicsVolume = 0;
        }
    }


    #endregion Start

    #region Update
    private void Update()
    {
        if(_npcNearbyActive == true)
        {
            _npcNearbyThemeTime += Time.deltaTime;
        }

      
    }

    #endregion Update



    #region Volume Manager
    private void MainSoundVolumeUpdate()
    {
        _2DSoundSource.volume = _soundsVolume;
        _3DSoundSource.volume = _soundsVolume;
        _2DRepetitiveSoundSource.volume = _soundsVolume;
        _3DRepetitiveSoundSource.volume = _soundsVolume;
    }

    private void MusicVolumeUpdate()
    {
        AmbiantSource.volume = _musicsVolume;
        _transitionSource.volume = _musicsVolume;
        _NPCThemeSource.volume = _musicsVolume;
        _NPCNearbySource.volume = _musicsVolume;
    }
    #endregion Volume Manager


    #region Music Ambiant
    public void PlayAmbiant(string key)
    {
        if (_soundData.ContainsKey(key) == false)
        {
            Debug.LogError("Fnct Play Music : Specified key not found for the audio file");
        }
        else
        {

            if (_soundData[key].Loop == false)   //Verify if the Soundata is set to loop, if it's not the case use PlayAudioOneShot instead
            {
                Debug.LogWarning("Fnct PlayMusic : The Soundata is not set as loop");
            }
            PlayAudio(AmbiantSource, key);
        }
    }

    public void StopAmbiant()
    {
        AmbiantSource.Stop();
    }

    public void PlayAmbiantWithFadeIn(string key, float speed)
    {

        if (_soundData.ContainsKey(key) == false)
        {
            Debug.LogError("Fnct Play Music with Fade In : Specified key not found for the audio file");
        }
        else
        {

            if (_soundData[key].Loop == false)   //Verify if the Soundata is set to loop, if it's not the case use PlayAudioOneShot instead
            {
                Debug.LogWarning("Fnct PlayMusicWithFadeIn : The Soundata is not set as loop");
            }

            _timeToFadeIn = speed;

            float numberofTick = _timeToFadeIn / _fadeTick;  //Calcul du nombre de tick en fonction de speed et la valeur fadeTick

            _fadeInTickVolumeValue = (_soundData[key].Volume * _musicsVolume) / numberofTick;  //Calcul du volume à incrémenter à chaque tick
            _volumeFadeInTarget = _soundData[key].Volume * _musicsVolume; //Calcul du volume que la source va avoir (Valeur Max)

            PlayAudio(AmbiantSource, key);

            AmbiantSource.volume = 0; //A faire plus propre (comment?)

            _timerFadeInTick.StartTimer(_fadeTick);
        }


    }

    public void PauseAmbiantWithFadeOut(float speed)
    {
        if (!AmbiantSource.isPlaying)
        {
            Debug.LogWarning("Fnct Stop Music with Fade Out : There is no Audio playing to fade Out");
            return;
        }
        else
        {

        }
    }

    public void StopAmbiantWithFadeOut(float speed, bool pauseInstead = false)
    {

        if (!AmbiantSource.isPlaying)
        {
            Debug.LogWarning("Fnct Stop Music with Fade Out : There is no Audio playing to fade Out");
            return;
        }
        else
        {
            // SwitchAudioSource(_ambiantSource, _transitionSource); //Switch du clip vers l'audio source transition

            // _ambiantSource.Stop();


            _timeToFadeOut = speed;

            float numberofTick = _timeToFadeOut / _fadeTick;  //Calcul du nombre de tick en fonction de speed et la valeur fadeTick

            _fadeOutTickVolumeValue = AmbiantSource.volume / numberofTick;  //Calcul du volume à incrémenter à chaque tick
            _volumeFadeOutTarget = 0; //Calcul du volume que la source va avoir (Valeur Max)

            // _transitionSource.Play();

            //Configuration of the fade Out Tick Event
            _audioSourceToFadeOut = AmbiantSource;
            _pauseAtFadeOutEnding = pauseInstead;
            _timerFadeOutTick.StartTimer(_fadeTick);

            Debug.Log("StopAmbiantWithFadeOut  " + _audioSourceToFadeOut);


        }
    }
    #endregion Music Ambiant


    #region Global FadeIn & FadeOut
    private void FadeInTick()
    {
        if (AmbiantSource.volume >= _volumeFadeInTarget)
        {
            _timerFadeInTick.StopTimer();
        }
        else
        {
            AmbiantSource.volume += _fadeInTickVolumeValue;
        }
    }
    private void FadeOutTick()
    {
        if (_audioSourceToFadeOut.volume <= _volumeFadeOutTarget)
        {
            _timerFadeOutTick.StopTimer();

            if (_pauseAtFadeOutEnding == true)
            {
                _audioSourceToFadeOut.Pause();
            }
            else
            {
                _audioSourceToFadeOut.Stop();
            }
        }
        else
        {
            _audioSourceToFadeOut.volume -= _fadeOutTickVolumeValue;
        }
    }
    #endregion Global FadeIn & FadeOut


    #region Switch Source & Transition
    private void SwitchAudioSource(AudioSource audioToChange, AudioSource audioTarget)
    {
        AudioClip clipToSwitch = audioToChange.clip;

        audioTarget.volume = audioToChange.volume;

        audioTarget.pitch = audioToChange.pitch;

        audioTarget.clip = clipToSwitch;

        audioTarget.time = audioToChange.time; //Démare la musique sur une nouvelle source âu même moment T que sur l'audio à changer
    }

   
    public void SwitchAmbiantTransition(string key, float fadeInSpeed, float fadeOutSpeed)
    {


        if (_soundData.ContainsKey(key) == false)
        {
            Debug.LogError("Fnct Switch Music : Specified key not found for the audio file");
            return;
        }
        else
        {

            if (!AmbiantSource.isPlaying)
            {
                Debug.LogWarning("Fnct Switch Music : There is no Audio already playing to fade Out");
            }


            #region Audio Source Switch + Fade Out

            SwitchAudioSource(AmbiantSource, _transitionSource); //Switch du clip vers l'audio source transition
            _audioSourceToFadeOut = _transitionSource;

            _transitionSource.Play();

            #endregion Audio Source Switch



            #region Fade Out

            _timeToFadeOut = fadeOutSpeed;

            float numberofTickFadeOut = _timeToFadeOut / _fadeTick;  //Calcul du nombre de tick en fonction de speed et la valeur fadeTick

            _fadeOutTickVolumeValue = _transitionSource.volume / numberofTickFadeOut;  //Calcul du volume à incrémenter à chaque tick
            _volumeFadeOutTarget = 0; //Calcul du volume que la source va avoir (Valeur Max)

            _transitionSource.Play();

            _timerFadeOutTick.StartTimer(_fadeTick);

            #endregion Fade Out



            #region New Music Play + Fade In



            _timeToFadeIn = fadeInSpeed;

            float numberofTickFadeIn = _timeToFadeIn / _fadeTick;  //Calcul du nombre de tick en fonction de speed et la valeur fadeTick

            _fadeInTickVolumeValue = (_soundData[key].Volume * _musicsVolume) / numberofTickFadeIn;  //Calcul du volume à incrémenter à chaque tick
            _volumeFadeInTarget = _soundData[key].Volume * _musicsVolume; //Calcul du volume que la source va avoir (Valeur Max)

            PlayAudio(AmbiantSource, key);

            AmbiantSource.volume = 0; //A faire plus propre (comment?)

            _timerFadeInTick.StartTimer(_fadeTick);

            #endregion New Music Play + Fade In

        }


    }
    #endregion Switch Source & Transition


    #region NPC Musics

    public void StartNearbyNPCMusic(string key)
    {

            PlayAudio(_NPCNearbySource, key);

    }

    public void ResumeAmbiantMusicWithFadeIn(string key, float speed)
    {
        _timerFadeOutTick.StopTimer();

        _npcThemePlaying = false;

        if (_soundData.ContainsKey(key) == false)
        {
            Debug.LogError("Fnct Play Music with Fade In : Specified key not found for the audio file");
        }
        else
        {
            _timeToFadeIn = speed;

            float numberofTick = _timeToFadeIn / _fadeTick;  //Calcul du nombre de tick en fonction de speed et la valeur fadeTick

            _fadeInTickVolumeValue = (_soundData[key].Volume * _musicsVolume) / numberofTick;  //Calcul du volume à incrémenter à chaque tick
            _volumeFadeInTarget = _soundData[key].Volume * _musicsVolume; //Calcul du volume que la source va avoir (Valeur Max)

            //PlayAudio(AmbiantSource, key);
            //We Un pause instead of playing the game from scratch
            AmbiantSource.UnPause();

            AmbiantSource.volume = 0; //A faire plus propre (comment?)

            _timerFadeInTick.StartTimer(_fadeTick);

            _NPCNearbySource.volume = 0;
            _NPCThemeSource.volume = 0;
        }
    }

    public void StartNPCThemeMusic(string key)
    {
        _npcThemePlaying = true;


        float playTime = _NPCNearbySource.time;


        PlayAudio(_NPCThemeSource, key); //We play the Ambiant Sound


        _NPCThemeSource.time = playTime;

        _NPCNearbySource.Stop();

    }



    #region Ambiant Transition When In NPC Music Range
    public void TryResumingAmbiant(ELevelType level, float fadeInSpeed)
    {
        if(_pausingAmbiant == true)
        {
            //Cancel the Pausing Event
            _pausAmbiantTimerFadeOutTick.StopTimer();
            _pausingAmbiant = false;
            Debug.Log("Canceling The Pausing Ambiant");
        }

        _resumingAmbiant = true; //We let the system know we are resuming ambiant sound

        //Setup the Fade In Ambiant Event
        _resAmbiantTimeToFadeIn = fadeInSpeed;
        float numberofTick = _resAmbiantTimeToFadeIn / _fadeTick;  //Calcul du nombre de tick en fonction de speed et la valeur fadeTick
        
        
        switch(level) //Depending on the level the ambiant sound to resume is different
        {
            case ELevelType.EPAVE: //If level = Epave then the music name to play is "M_Epave"
                _resAmbiantFadeInTickVolumeValue = (_soundData["M_Epave"].Volume * _musicsVolume) / numberofTick;  //Calcul du volume à incrémenter à chaque tick
                _resAmbiantVolumeFadeInTarget = _soundData["M_Epave"].Volume * _musicsVolume; //Calcul du volume que la source va avoir (Valeur Max)
                break;

            case ELevelType.CIMETIERE://If level = Epave then the music name to play is "M_CimetiereMusic"
                _resAmbiantFadeInTickVolumeValue = (_soundData["M_CimetiereMusic"].Volume * _musicsVolume) / numberofTick;  //Calcul du volume à incrémenter à chaque tick
                _resAmbiantVolumeFadeInTarget = _soundData["M_CimetiereMusic"].Volume * _musicsVolume; //Calcul du volume que la source va avoir (Valeur Max)
                break;
        }

        _ambiantSource.UnPause();
        _resAmbiantTimerFadeInTick.StartTimer(_fadeTick);

        //NPC Music Config

        _npcThemePlaying = false;
    }

    public void TryPausingAmbiant(float fadeOutSpeed)
    {
        if (_resumingAmbiant == true)
        {
            //Cancel the Resuming Event
            _resAmbiantTimerFadeInTick.StopTimer();
            _resumingAmbiant = false;
            Debug.Log("Canceling The Resuming Ambiant");
        }

        _pausingAmbiant = true;

        //Setup the Fade Out Ambiant Event
        _pausAmbiantTimeToFadeOut = fadeOutSpeed;
        float numberofTick = _pausAmbiantTimeToFadeOut / _fadeTick;  //Calcul du nombre de tick en fonction de speed et la valeur fadeTick
        
        _pausAmbiantFadeOutTickVolumeValue = _ambiantSource.volume / numberofTick;  //Calcul du volume à incrémenter à chaque tick
        _pausAmbiantVolumeFadeOutTarget = 0; //Calcul du volume que la source va avoir (Valeur Max)

        _pausAmbiantTimerFadeOutTick.StartTimer(_fadeTick);

    }


    private void ResumeAmbiantFadeInTick()
    {
        //Debug.Log("ResumeFadeInTick");

        if (_ambiantSource.volume >= _resAmbiantVolumeFadeInTarget)
        {
            _resAmbiantTimerFadeInTick.StopTimer();
            _resumingAmbiant = false;
        }
        else
        {
            _ambiantSource.volume += _resAmbiantFadeInTickVolumeValue;
        }
    }
   
    private void PauseAmbiantFadeOutTick()
    {
        //Debug.Log("PausingFadeOutTick");

        if (_ambiantSource.volume <= _pausAmbiantVolumeFadeOutTarget)
        {
            _pausAmbiantTimerFadeOutTick.StopTimer();
            _ambiantSource.Pause();
            _pausingAmbiant = true;
        }
        else
        {
            _ambiantSource.volume -= _pausAmbiantFadeOutTickVolumeValue;
        }
    }


    #endregion Ambiant Transition When In NPC Music Range

    #endregion NPC Musics


    #region PNJVoiceSound
    public void StartPNJSound(SoundData soundData)
    {

        AudioSource oneShotSource2D = Instantiate(_2DSoundSource, transform);

        PlayPNJAudio(oneShotSource2D, soundData);
       // StartCoroutine(SoundDestroyer(soundData.Clip.length, oneShotSource2D));

        /*if (soundData.PitchVariation == true)
        {
            float rand = Random.Range(soundData.PitchMinimum, soundData.PitchMaximum);
            oneShotSource2D.pitch = rand;
            PlayPNJAudio(oneShotSource2D, soundData, rand);
            StartCoroutine(SoundDestroyer(soundData.Clip.length, oneShotSource2D));
        }
        else
        {
           
        }*/

       


        /*        AudioSource oneShotSource3D = Instantiate(_3DSoundSource, position);


            if (_soundData[key].PitchVariation == true)
            {
                float rand = Random.Range(_soundData[key].PitchMinimum, _soundData[key].PitchMaximum);
                oneShotSource3D.pitch = rand;
                PlayAudioWithPitchVariation(oneShotSource3D, key, rand);
                StartCoroutine(SoundDestroyer(_soundData[key].Clip.length, oneShotSource3D));

            }
            else
            {
                PlayAudio(oneShotSource3D, key);
                StartCoroutine(SoundDestroyer(_soundData[key].Clip.length, oneShotSource3D));
            }*/

    }

    private void PlayPNJAudio(AudioSource source, SoundData soundData, float pitch = 0)
    {
        AudioClip clipToPlay = soundData.Clip;

        source.volume = (soundData.Volume * _soundsVolume);

        if(pitch != 0)
        {
            source.pitch = pitch;
        }
        else
        {
            source.pitch = soundData.Pitch;
        }

        source.loop = soundData.Loop;

        source.clip = clipToPlay;

        source.Play();
    }
    #endregion PNJVoiceSound


    #region 2DSound

    /// <summary>
    /// Start a 2D Sound
    /// </summary>
    /// <param name="key">Parameter value to pass.</param>

    public static void Start2DSound(string key, int tracker = 0)
    {
        GetInstanceNoNotify()?.Start2DSoundInternal(key, tracker);
    }

    private static List<string> single = new List<string>();
    public static void Start2DSoundSingleCall(string key, int maxDelay, int tracker = 0)
    {
        if (single.FindIndex(t => t == key) != -1)
            return;
        GetInstanceNoNotify()?.Start2DSoundInternal(key, tracker);
        single.Add(key);
        _ = ClemCAddons.Utilities.GameTools.DelayedCall(maxDelay, () =>
        {
            single.Remove(key);
        });
    }

    private void Start2DSoundInternal(string key, int tracker = 0)
    {

        try
        {
            if (_soundData.ContainsKey(key) == false)
            {
                Debug.LogError("Fnct StartSound2D : Specified key : " + key + "  not found for the audio file");
                return;
            }
            else if (_soundData[key].Loop == true) //Verify if the Soundata is set to loop, if it's not the case use PlayAudioOneShot instead
            {
                AudioSource repSource2D = Instantiate(_2DRepetitiveSoundSource, transform);

                TrackingSetup(key, repSource2D, tracker);

                PlayAudio(repSource2D, key);
            }
            else
            {
                AudioSource oneShotSource2D = Instantiate(_2DSoundSource, transform);

                TrackingSetup(key, oneShotSource2D, tracker);

           
                PlayAudio(oneShotSource2D, key);
                StartCoroutine(SoundDestroyer(_soundData[key].Clip.length, oneShotSource2D));
                

            }
        }
        catch(System.Exception e)
        {
            Debug.LogError("Error In Start2DFunction at"+e.StackTrace+"\n"+e.Message);
            return;
        }
    
    }


    #endregion 2DSound



    #region 3DSound

    public static void Start3DSound(string key, Transform position, bool removeFromParents = false, int tracker = 0)
    {
        GetInstanceNoNotify()?.Start3DSoundInternal(key, position, removeFromParents, tracker);
    }

    private void Start3DSoundInternal(string key, Transform position, bool removeFromParents = false, int tracker = 0)
    {
        try
        {
            if (_soundData.ContainsKey(key) == false)   //Verify if the Soundata is set to loop, if it's not the case use PlayAudioOneShot instead
            {
                Debug.LogError("Fnct Start3DSound : Specified key not found for the audio file");
                return;
            }
            else if (_soundData[key].Loop == true)
            {
                AudioSource repSource3D = Instantiate(_3DRepetitiveSoundSource, position);

                TrackingSetup(key, repSource3D, tracker);

                PlayAudio(repSource3D, key);
            }
            else
            {
                AudioSource oneShotSource3D = Instantiate(_3DSoundSource, position);

                TrackingSetup(key, oneShotSource3D, tracker);

                if (removeFromParents == true)
                {
                    oneShotSource3D.transform.parent = null;
                }

                //TrackingSetup(key, repSource3D, tracker);

                PlayAudio(oneShotSource3D, key);
                StartCoroutine(SoundDestroyer(_soundData[key].Clip.length, oneShotSource3D));
                
            }
        }
        catch
        {
            Debug.LogError("Error In Start3DFunction");
            return;
        }
     
    }

    public static void Start3DSound(string key, AudioSource audioSource, Transform targetTransform = null, bool doNotDestroy = false)
    {
        GetInstanceNoNotify()?.Start3DSoundInternal(key, audioSource, doNotDestroy, targetTransform);
    }

    private void Start3DSoundInternal(string key, AudioSource audioSource, bool doNotDestroy, Transform targetTransform = null)
    {
        try
        { 
            if (_soundData.ContainsKey(key) == false)   //Verify if the Soundata is set to loop, if it's not the case use PlayAudioOneShot instead
            {
                Debug.LogError("Fnct Start3DSound : Specified key not found for the audio file");
                return;
            }
            else if (_soundData[key].Loop == true)
            {
                PlayAudio(audioSource, key);
            }
            else
            {

                if (targetTransform != null)
                {
                    audioSource.transform.parent = null;
                    audioSource.transform.position = targetTransform.position;             }

                //TrackingSetup(key, repSource3D, tracker);

               
                PlayAudio(audioSource, key);
                if(!doNotDestroy)
                    StartCoroutine(SoundDestroyer(_soundData[key].Clip.length, audioSource));
                

            }
        }
        catch
        {
            Debug.LogError("Error In Start3DFunction");
            return;
        }
     
    }

    
        #endregion 3DSound



    #region Common Sounds

    private void PlayAudio(AudioSource source, string key) //USED BY OTHER METHOD IN THE CLASS
    {
        SoundData soundData = _soundData[key];

        AudioClip clipToPlay = soundData.Clip;

        source.volume = (soundData.Volume * _soundsVolume);

        if (soundData.PitchVariation == false) //Depending on the Pitch Variation
        {
            source.pitch = soundData.Pitch;
        }
        else
        {
            float rand = Random.Range(soundData.PitchMinimum, soundData.PitchMaximum);
            source.pitch = rand;
        }


        if (soundData.MultipleSound == false) //Depending on the Multiple Sound Random
        {
            source.clip = clipToPlay;
        }
        else
        {
            clipToPlay = SetupRandomClip(soundData);
            source.clip = clipToPlay;
        }

        source.loop = soundData.Loop;


        source.Play();
    }


    private AudioClip SetupRandomClip(SoundData soundData)
    {

        int random = Random.Range(0 , soundData.AdditionalClips.Length + 1);
        AudioClip audioClipToPlay = null;
        if(random == 0)
        {
            audioClipToPlay = soundData.Clip;
        }
        else
        {
            audioClipToPlay = soundData.AdditionalClips[random - 1];
        }

        return audioClipToPlay;

    }

    #region Stop Sound
    private void TrackingSetup(string key, AudioSource source, int tracker = 0) //Function to setup the track and the AudioSource List later usefull if we want to stop those sounds
    {
        try
        {
            if (tracker == 0 && _soundData[key].UniqueSound == true)
            {
                try
                {
                    //If the sound is Unique there shouldn't be two of them playing at the same time
                    _2DRepetitiveSources.Add(key, source); //If the sound is suposed to be unique we add it to a List (It will be usefull to stop the sound later on), to stop the sound use StopUniqueSound
                }
                catch
                {
                    Debug.LogError("The sound is set to unique although, a second one is trying to be played");
                    return;
                }
            }
            else if (tracker != 0)
            {
                try
                {
                    _2DRepetitiveSources.Add(key + tracker, source); //We setup a Tracker
                }
                catch
                {
                    Debug.LogError("This track number for this sound is already used, use a different one");
                    return;
                }
            }
            else
            {
                if (tracker != 0 && _soundData[key].UniqueSound == true)
                {
                    Debug.LogError("The sound is set to unique but you are using a tracker");
                }

                return;
            }
        }
        catch
        {
            Debug.LogError("Error In TrackingSetup");
            return;
        }
       
    }


    public void StopUniqueSound(ESoundType soundType, string audioSourceName)
    {
        try
        {
            GameObject audioSourceToDestoy;

            if (_soundData.ContainsKey(audioSourceName) == false)
            {
                Debug.LogError("Fnct StopSound : Specified key not found for the audio file");
                return;
            }

            switch (soundType)
            {
                case ESoundType.ONESHOT2D:

                    if (_2DSources.Count == 0)
                    {
                        Debug.LogWarning("Fnct StopSound : There is no audio source currently playing, the type of sound you are trying to stop is probably incorrect");
                        return;
                    }
                    else
                    {
                        audioSourceToDestoy = _2DSources[audioSourceName].gameObject;

                        _2DSources.Remove(audioSourceName);
                        Destroy(audioSourceToDestoy);
                    }
                    break;


                case ESoundType.REPETITIVE2D:

                    if (_2DRepetitiveSources.Count == 0)
                    {
                        Debug.LogWarning("Fnct StopSound : There is no audio source currently playing, the type of sound you are trying to stop is probably incorrect");
                        return;
                    }
                    else
                    {
                        audioSourceToDestoy = _2DRepetitiveSources[audioSourceName].gameObject;

                        _2DRepetitiveSources.Remove(audioSourceName);
                        Destroy(audioSourceToDestoy);
                    }
                    break;


                case ESoundType.ONESHOT3D:

                    if (_3DSources.Count == 0)
                    {
                        Debug.LogWarning("Fnct StopSound : There is no audio source currently playing, the type of sound you are trying to stop is probably incorrect");
                        return;
                    }
                    else
                    {
                        audioSourceToDestoy = _3DSources[audioSourceName].gameObject;

                        _3DSources.Remove(audioSourceName);
                        Destroy(audioSourceToDestoy);
                    }
                    break;


                case ESoundType.REPETITIVE3D:

                    if (_3DRepetitiveSources.Count == 0)
                    {
                        Debug.LogWarning("Fnct StopSound : There is no audio source currently playing, the type of sound you are trying to stop is probably incorrect");
                        return;
                    }
                    else
                    {
                        audioSourceToDestoy = _3DRepetitiveSources[audioSourceName].gameObject;

                        _3DRepetitiveSources.Remove(audioSourceName);
                        Destroy(audioSourceToDestoy);
                    }
                    break;

                default:
                    Debug.LogWarning("Fnct StopSound : soundType is not correct");
                    break;
            }
        }
        catch
        {
            Debug.LogError("Error In StopUniqueSound");
            return;
        }


    }

    public void StopSoundWithTracker(ESoundType soundType, string audioSourceName, int trackKey = 0, bool stopAllSound = false)
    {
        try
        {
            GameObject audioSourceToDestoy;

            if (_soundData.ContainsKey(audioSourceName) == false)
            {
                Debug.LogError("Fnct StopSound : Specified key not found for the audio file");
                return;
            }

            switch (soundType)
            {
                case ESoundType.ONESHOT2D:

                    if (_2DSources.Count == 0)
                    {
                        Debug.LogWarning("Fnct StopSound : There is no audio source currently playing, the type of sound you are trying to stop is probably incorrect");
                        return;
                    }
                    else
                    {
                        if (stopAllSound == true)
                        {
                            foreach (KeyValuePair<string, AudioSource> source in _2DSources)
                            {
                                string name = source.Key.Substring(1, source.Key.Length);
                                Debug.Log(name);

                                if (name == audioSourceName)
                                {
                                    audioSourceToDestoy = _2DSources[source.Key].gameObject;
                                    _2DSources.Remove(source.Key);
                                    Destroy(audioSourceToDestoy);

                                }
                            }
                        }
                        else
                        {
                            if (trackKey != 0)
                            {
                                audioSourceToDestoy = _2DSources[trackKey + audioSourceName].gameObject;
                            }
                            else
                            {
                                audioSourceToDestoy = _2DSources[audioSourceName].gameObject;
                            }

                            _2DSources.Remove(audioSourceName);
                            Destroy(audioSourceToDestoy);
                        }


                    }
                    break;


                case ESoundType.REPETITIVE2D:

                    if (_2DRepetitiveSources.Count == 0)
                    {
                        Debug.LogWarning("Fnct StopSound : There is no audio source currently playing, the type of sound you are trying to stop is probably incorrect");
                        return;
                    }
                    else
                    {
                        if (stopAllSound == true)
                        {
                            foreach (KeyValuePair<string, AudioSource> source in _2DRepetitiveSources)
                            {
                                string name = source.Key.Substring(1, source.Key.Length);
                                Debug.Log(name);

                                if (name == audioSourceName)
                                {
                                    audioSourceToDestoy = _2DRepetitiveSources[source.Key].gameObject;
                                    _2DRepetitiveSources.Remove(source.Key);
                                    Destroy(audioSourceToDestoy);

                                }
                            }
                        }
                        else
                        {
                            if (trackKey != 0)
                            {
                                audioSourceToDestoy = _2DRepetitiveSources[trackKey + audioSourceName].gameObject;
                            }
                            else
                            {
                                audioSourceToDestoy = _2DRepetitiveSources[audioSourceName].gameObject;
                            }

                            _2DRepetitiveSources.Remove(audioSourceName);
                            Destroy(audioSourceToDestoy);
                        }
                    }
                    break;


                case ESoundType.ONESHOT3D:

                    if (_3DSources.Count == 0)
                    {
                        Debug.LogWarning("Fnct StopSound : There is no audio source currently playing, the type of sound you are trying to stop is probably incorrect");
                        return;
                    }
                    else
                    {
                        if (stopAllSound == true)
                        {
                            foreach (KeyValuePair<string, AudioSource> source in _3DSources)
                            {
                                string name = source.Key.Substring(1, source.Key.Length);
                                Debug.Log(name);

                                if (name == audioSourceName)
                                {
                                    audioSourceToDestoy = _3DSources[source.Key].gameObject;
                                    _3DSources.Remove(source.Key);
                                    Destroy(audioSourceToDestoy);

                                }
                            }
                        }
                        else
                        {
                            if (trackKey != 0)
                            {
                                audioSourceToDestoy = _3DSources[trackKey + audioSourceName].gameObject;
                            }
                            else
                            {
                                audioSourceToDestoy = _3DSources[audioSourceName].gameObject;
                            }

                            _3DSources.Remove(audioSourceName);
                            Destroy(audioSourceToDestoy);
                        }
                    }
                    break;


                case ESoundType.REPETITIVE3D:

                    if (_3DRepetitiveSources.Count == 0)
                    {
                        Debug.LogWarning("Fnct StopSound : There is no audio source currently playing, the type of sound you are trying to stop is probably incorrect");
                        return;
                    }
                    else
                    {
                        if (stopAllSound == true)
                        {
                            foreach (KeyValuePair<string, AudioSource> source in _3DRepetitiveSources)
                            {
                                string name = source.Key.Substring(1, source.Key.Length);
                                Debug.Log(name);

                                if (name == audioSourceName)
                                {
                                    audioSourceToDestoy = _3DRepetitiveSources[source.Key].gameObject;
                                    _3DRepetitiveSources.Remove(source.Key);
                                    Destroy(audioSourceToDestoy);

                                }
                            }
                        }
                        else
                        {
                            if (trackKey != 0)
                            {
                                audioSourceToDestoy = _3DRepetitiveSources[trackKey + audioSourceName].gameObject;
                            }
                            else
                            {
                                audioSourceToDestoy = _3DRepetitiveSources[audioSourceName].gameObject;
                            }

                            _3DRepetitiveSources.Remove(audioSourceName);
                            Destroy(audioSourceToDestoy);
                        }
                    }
                    break;

                default:
                    Debug.LogWarning("Fnct StopSound : soundType is not correct");
                    break;
            }
        }
        catch
        {
            Debug.LogError("Error In StopSoundWithTracker");
            return;
        }
    }

 

    #endregion Stop Sound


    IEnumerator SoundDestroyer(float soundLength, AudioSource sourceToDestroy) //SOUND DESTROYER, WAIT FOR THE LENGTH OF THE SOUND AND WHEN THE SOUND IS DONE PLAYING IT WILL DESTROY IT
    {

        yield return new WaitForSeconds(soundLength);
        Destroy(sourceToDestroy.gameObject);
       
        
    }
    #endregion Common Sounds



    #region Repetitive Sounds

    public void ChangeRepetitiveSoundRate(string audioSourceName, float newRate)
    {

    }

    #endregion Repetitive Sounds



    #endregion Methods
}