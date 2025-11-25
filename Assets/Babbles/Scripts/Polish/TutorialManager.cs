using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class TutorialManager : Singleton<TutorialManager>
{
    #region Fields

    [Header("Tutorial Main")]

    [SerializeField] private bool _tutorialDone = false;

    [Header("Tutorial Skip")]
    [SerializeField] private Transform _tutorialSkipPosition = null;
    [SerializeField] private Transform _missedCourantMarinTPPosition = null;



    [Header("Video Tuto")]
    [SerializeField] private VideoClip[] _videoClips = null;
    #endregion Fields



    #region Properties
    public bool TutorialDone { get => _tutorialDone; set => _tutorialDone = value; }
    #endregion Properties


    #region Methods
    public void SkipTuto()
    {
        UIManager.Instance.UIController.Character.Teleport(_tutorialSkipPosition.position);
        UIManager.Instance.UIController.StartDialogue("Tuto_Skip", true);
        _tutorialDone = true;
    }

    public void MissedTutoCourantMarinTeleport()
    {
        UIManager.Instance.UIController.Character.Teleport(_missedCourantMarinTPPosition.position);
    }

    public void LoadTutoVideo(EVideoTutoType videoTutoType)
    {
        

          
        switch(videoTutoType)
        {
            case EVideoTutoType.FLEURINVENTORY:
                UIManager.Instance.UIController.TopRightVideoPlayer.gameObject.SetActive(true);
                UIManager.Instance.UIController.TopRightVideoPlayer.clip = _videoClips[0];
                break;
            case EVideoTutoType.RESSOURCEINSHAKER:
                UIManager.Instance.UIController.BottomLeftVideoPlayer.gameObject.SetActive(true);
                UIManager.Instance.UIController.BottomLeftVideoPlayer.clip = _videoClips[1];
                break;
            case EVideoTutoType.USEPOULPECOUSSIN:
                UIManager.Instance.UIController.TopRightVideoPlayer.gameObject.SetActive(true);
                UIManager.Instance.UIController.TopRightVideoPlayer.clip = _videoClips[2];
                break;
        }

        
    }

    public void HideTutoVideo()
    {
        UIManager.Instance.UIController.BottomLeftVideoPlayer.gameObject.SetActive(false);
        UIManager.Instance.UIController.TopRightVideoPlayer.gameObject.SetActive(false);
    }

    public enum EVideoTutoType
    {
        FLEURINVENTORY,
        FLEURUSE,
        RESSOURCEINSHAKER,
        SHAKE,
        USEPOULPECOUSSIN,
    }
    #endregion Methods
}
