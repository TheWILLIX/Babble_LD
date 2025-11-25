using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EpaveCustomBehaviour : Singleton<EpaveCustomBehaviour>
{
    #region Fields




    [Header("Hotu Challenge")]
    private bool _isInChallenge = false;
    [SerializeField] private Transform[] _hotuChallengeStartingPoint = null;
    [SerializeField] private Transform _inFrontOfHotuPos = null;
    [SerializeField] private GameObject[] _hotuChallengeMeduseGroup = null;
    [SerializeField] private int[] _hotuMedusePerChallenge = null;

    [SerializeField] private Collider _hotuCollider = null;

    [SerializeField] private float[] _challengeTimers = null; //45f / 40 /75f


    private int _numberOfMeduseActivated = 0;
    private int _numberOfMeduseNeeded = 10;

    private int _challengeNumber = 1;


    [Header("Timers")]
    private bool _timerActivated = false;
    private float _timeStamp = 0f;

    private bool _showMeduseNumber = false;
    private float _timeStampMeduseShow = 0f;
    [SerializeField] private float _timeBeforeHideMeduseNumber = 1.8f;

    #endregion Fields

    #region Properties

    public int NumberOfMeduseActivated { get => _numberOfMeduseActivated;  }
    public bool IsInChallenge { get => _isInChallenge; set => _isInChallenge = value; }

    #endregion Properties

    #region Methods
    protected override void Update()
    {
        if(_timerActivated == true)
        {
            if(_timeStamp >= _challengeTimers[_challengeNumber-1])
            {
                FailHotuChallenge();
                _timerActivated = false;
            }
            else
            {
                _timeStamp += Time.deltaTime;

                UIManager.Instance.UIController.TimerTextHotu.text = ( _challengeTimers[_challengeNumber -1] - _timeStamp).ToString().Substring(0, 4);
            }
        }

        if(_showMeduseNumber == true)
        {
            if(_timeStampMeduseShow >= _timeBeforeHideMeduseNumber)
            {
                UIManager.Instance.UIController.NumberOfMeduseHotu.gameObject.SetActive(false); //Desactivation Meduse Number
                _timeStampMeduseShow = 0;
                _showMeduseNumber = false;
            }
            else
            {
                _timeStampMeduseShow += Time.deltaTime;
            }
        }

    }



    #region Hotu Challenge
    public void StartHotuChallenge(int difficulty)
    {
        AudioManager.Instance.IsPlayingChallengeMusic = true;
        _isInChallenge = true;
        int groupNumber = difficulty - 1;

        _hotuChallengeMeduseGroup[groupNumber].SetActive(true); //We Show the Correct Meduse Of the challenge


        AudioManager.Instance.SwitchAmbiantTransition("M_HotuChallenge", 1f, 0.75f); //We Start the Music (Will set the time later one)

        _numberOfMeduseActivated = 0; //We reset the number if Meduse that the player has currently jumped on
        _numberOfMeduseNeeded = _hotuMedusePerChallenge[groupNumber]; //We set the number of Meduse Needed to Finish the challenge


        switch (groupNumber)
        {
            case 0:
                AudioManager.Instance.AmbiantSource.time = 0f;

                _challengeNumber = 1;


                break;

            case 1:
                AudioManager.Instance.AmbiantSource.time = 55f;

                _challengeNumber = 2;



                break;

            case 2:
                AudioManager.Instance.AmbiantSource.time = 231f;

                _challengeNumber = 3;


                break;
        }


        StartCoroutine(HotuTeleportStartDelay());
    }

    public void SucessHotuChallenge()
    {
        _timeStamp = 0f;

        AudioManager.Start2DSound("S_FinishLine");

        Debug.Log("HOTU CHALLENGE SUCESS");
        //Fade Out
        //Teleport
        //Hotu qui Parle
        switch (_challengeNumber)
        {
            case 1:
                UIManager.Instance.UIController.DialogueManager.VariableStorage.SetValue("$hotuQueteSucced1", true);
                break;

            case 2:
                UIManager.Instance.UIController.DialogueManager.VariableStorage.SetValue("$hotuQueteSucced2", true);
                break;

            case 3:
                UIManager.Instance.UIController.DialogueManager.VariableStorage.SetValue("$hotuQueteSucced3", true);
                break;

        }


         _hotuChallengeMeduseGroup[_challengeNumber-1].SetActive(false); //We Show the Correct Meduse Of the challenge


        UIManager.Instance.UIController.TimerTextHotu.gameObject.SetActive(false); //Desactivation Timer

        AudioManager.Start2DSound("S_WhistleEnd");

        StartCoroutine(HotuSucessDelay());

        _timerActivated = false;
        _isInChallenge = false;
    }

    public void FailHotuChallenge()
    {
        _timeStamp = 0f;

        Debug.Log("HOTU CHALLENGE FAIL");
        //Fade Out
        //Teleport
        //Hotu qui Parle


        // _hotuChallengeMeduseGroup[groupNumber].SetActive(false); //We Show the Correct Meduse Of the challenge

        foreach(MeduseHotuChallenge meduse in _hotuChallengeMeduseGroup[_challengeNumber-1].GetComponentsInChildren<MeduseHotuChallenge>())
        {
            meduse.HasBeenActivated = false;
            meduse.SetActivatedMaterial(false);
        }

        _hotuChallengeMeduseGroup[_challengeNumber - 1].SetActive(false); //We Show the Correct Meduse Of the challenge

        UIManager.Instance.UIController.TimerTextHotu.gameObject.SetActive(false);

        AudioManager.Start2DSound("S_FinishLine");
        AudioManager.Start2DSound("S_WhistleEnd");

        _timerActivated = false;
        _isInChallenge = false;

        StartCoroutine(HotuFailDelay());

    }






    IEnumerator HotuTeleportStartDelay()
    {

        UIManager.Instance.UIController.Character.SetCanMove(false);
        // Fade Out
        yield return new WaitForSeconds(0.5f);

        UIManager.Instance.UIController.Character.Teleport(_hotuChallengeStartingPoint[_challengeNumber -1].position); //We Set the player to the correct challenge start Position
        UIManager.Instance.UIController.Character.SetCanMove(false);
      //  UIManager.Instance.UIController.Fade.FadeIn();
        yield return new WaitForSeconds(0.5f);


        UIManager.Instance.UIController.Character.TpsCamera.LookAtTransform(3000, 1000, _hotuChallengeMeduseGroup[_challengeNumber -1 ].transform.GetChild(1),
            () =>
            {
                StartCoroutine(StartCountdown());
            });

    }

    IEnumerator StartCountdown()
    {
        Debug.Log("already finished");

        UIManager.Instance.UIController.CountDownHotuChallenge.SetTrigger("CountdownOn"); //Start the Countdown

        //Audio in sync with the animation
        yield return new WaitForSeconds(0.05f);
        AudioManager.Start2DSound("S_Tuu1");
        yield return new WaitForSeconds(1f);
        AudioManager.Start2DSound("S_Tuu2");
        yield return new WaitForSeconds(1f);
        AudioManager.Start2DSound("S_Tuu3");
        yield return new WaitForSeconds(1f);
        AudioManager.Start2DSound("S_TuuGo");


        //At the end of the Countdown unlock the player
        UIManager.Instance.UIController.Character.SetCanMove(true);


        UIManager.Instance.UIController.TimerTextHotu.gameObject.SetActive(true); //Real stuff begin now !
        _timerActivated = true;
    }


    IEnumerator HotuFailDelay()
    {
        UIManager.Instance.UIController.Character.SetCanMove(false);

        yield return new WaitForSeconds(1f);

        UIManager.Instance.UIController.Character.Teleport(_inFrontOfHotuPos.position);

        if (_hotuCollider != null)
        {
            var t = GameObject.Find("PlacingUI").GetComponent<PlaceAboveTarget>();
            t.Target = _hotuCollider;
        }

        yield return new WaitForSeconds(0.5f);

        UIManager.Instance.UIController.StartDialogue("Hotu_Quete_Fail", true); //"FailChallenge_" + _challengeNumber

       // Hotu_Quete_Good_1
    }

    IEnumerator HotuSucessDelay()
    {
        UIManager.Instance.UIController.Character.SetCanMove(false);

        yield return new WaitForSeconds(1f);

        UIManager.Instance.UIController.Character.Teleport(_inFrontOfHotuPos.position);

        if (_hotuCollider != null)
        {
            var t = GameObject.Find("PlacingUI").GetComponent<PlaceAboveTarget>();
            t.Target = _hotuCollider;
        }

        yield return new WaitForSeconds(0.5f);

        UIManager.Instance.UIController.StartDialogue("Hotu_Quete_Good" + _challengeNumber, true); //"FailChallenge_" + _challengeNumber
    }

    public void JumpOnMeduse()
    {
        _numberOfMeduseActivated++;

        UIManager.Instance.UIController.NumberOfMeduseHotu.gameObject.SetActive(true); //Activation Meduse Number
        _showMeduseNumber = true;
        _timeStampMeduseShow = 0;

        if (NumberOfMeduseActivated >= _numberOfMeduseNeeded)
        {
            UIManager.Instance.UIController.NumberOfMeduseHotu.text = NumberOfMeduseActivated + " / " + _numberOfMeduseNeeded;
            SucessHotuChallenge();
        }
        else
        {

            UIManager.Instance.UIController.NumberOfMeduseHotu.text = NumberOfMeduseActivated + " / " + _numberOfMeduseNeeded;
        }


        //Feedbacks
        AudioManager.Start2DSound("S_Saut2");
        AudioManager.Start2DSound("S_MeduseRapide");

    }
    #endregion Hotu Challenge





  
    #endregion Methods

}
