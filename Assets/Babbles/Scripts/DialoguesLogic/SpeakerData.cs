using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Database/Dialog/Speaker")]
public class SpeakerData : ScriptableObject
{
    #region Fields
    [Header("Main")]
    [SerializeField] private string _textColorID = "#FF5733";
    [SerializeField] private string _characterName = "Bulle";

    [Tooltip("Mettre le cadre unique pour les PNJ++")]
    [SerializeField] private Sprite _faceSprite = null; //Normalement on va pas faire ça donc inutile mais je garde au cas ou pour l'instant


    [Header("PNJ Type")]
    [SerializeField] private bool _isPNJClassic = true;
    [SerializeField] private bool _seumHasInfluence = true;


    [Header("Only For PNJ++")]
    [SerializeField] private int _stadePNJ = 0; //For the Dialogue Story
    [SerializeField] private int _relationPointsPNJ = 1; //For the Dialogue Story
    [SerializeField] private int _seuilToIncreaseEachStade = 10;

    [Tooltip("Only for PNJ++")]
    [SerializeField] private int _seuilSeumExplo = 12;

    [Header("Like")]
    [SerializeField] private Item[] _itemLike = null;

    [Header("Dislike")]
    [SerializeField] private Item[] _itemDislike = null;


    [Header("Favorite")]
    [Tooltip("!! Attention l'item receiveUnique doit correspondre avec le nom de la node (Le premier Item doit correspondre au premier nom de node et vice versa) !!")]
    [SerializeField] private Item[] _receiveUniqueItem = null;

//    [Tooltip("!! Attention l'item receiveUnique doit correspondre avec le nom de la node (Le premier Item doit correspondre au premier nom de node et vice versa) !!")]
//    [SerializeField] private string[] _receiveUniqueNodeName = null;

    [Header("Audio")]
    [SerializeField] private SoundData[] _voiceSound = null;

    [Tooltip("-1 =  No Modification")]
    [SerializeField] private float _speedOfLettersSound = 0f;
    [SerializeField] private float _minimumPitchOfLettersSound = 1f;
    [SerializeField] private float _maximumPitchOfLettersSound = 1f;




    #endregion Fields


    #region Properties
    public string TextColorID => _textColorID;
    public string CharacterName => _characterName;
    public bool IsPNJClassic => _isPNJClassic;
    public bool SeumHasInfluence => _seumHasInfluence;


    public int StadePNJ
    {
        get
        {
           return _stadePNJ;
        }
        set
        {
            _stadePNJ = value;
        }
    }
    public int RelationPointsPNJ
    {
        get
        {
           return _relationPointsPNJ;
        }
        set
        {
            _relationPointsPNJ = value;
        }
    }
    public int SeuilToIncreaseEachStade => _seuilToIncreaseEachStade;

    public int SeuilSeumExplo => _seuilSeumExplo;

    public Item[] ItemLike => _itemLike;
    public Item[] ItemDislike => _itemDislike;

    public Item[] ReceiveUniqueItem => _receiveUniqueItem;
//    public string[] ReceiveUniqueNodeName => _receiveUniqueNodeName;

    public SoundData[] VoiceSound => _voiceSound;

    public Sprite FaceSprite => _faceSprite;

    public float SpeedOfLettersSound { get => _speedOfLettersSound; }
    public float MinimumPitchOfLettersSound { get => _minimumPitchOfLettersSound; }
    public float MaximumPitchOfLettersSound { get => _maximumPitchOfLettersSound; }


    #endregion Properties

}
