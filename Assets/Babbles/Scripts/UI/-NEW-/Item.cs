using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[CreateAssetMenu(fileName = "Item", menuName = "Database/Engine/Item")]

public class Item : ScriptableObject
{
    #region Fields

    [SerializeField] private string _name = string.Empty;

    [SerializeField] private string _cleanName = string.Empty;


    [SerializeField] private int _defaultQuantity = 1;

    [SerializeField] private bool _isRessource = false;

    [Tooltip("!!! Do not set this to true unless it's a ressource !!!")]
    [SerializeField] private bool _isDiscoveredByDefault = false;

    [SerializeField] private bool _useMultipleSelection = false;
    [SerializeField] private bool _craftMultipleSelection = false;



    [Header("Remember To Fill This !")]
    [SerializeField] private Sprite _sprite = null;
    [SerializeField] private Sprite _spriteQuantity0 = null;
    [SerializeField] private Sprite _spriteQuantity1 = null;
    [SerializeField] private Sprite _spriteQuantity2 = null;
    [SerializeField] private Sprite _spriteQuantity3 = null;

    [Header("Item Usage")]
    [Tooltip("The Type of function that will be started when item is used")]
    [SerializeField] private EItemUsageType _usageType = EItemUsageType.NONE;

    [Tooltip("Depending on the usageType some require a prefab to instantiate etc... Put what is needed here")]
    [SerializeField] private GameObject _linkedObject = null;

    [Tooltip("Depending on the usageType some require a value like how much time the effect of the item will last... Put what is needed here")]
    [SerializeField] private float _linkedValue = 5f;

    [Header("Item Usage")]
    [SerializeField] private string _audio = null;



    #endregion Fields

    #region Properties
    public string Name => _name;
    public string CleanName => _cleanName;

    public int DefaultQuantity
    {
        get { return _defaultQuantity; }
        set { _defaultQuantity = value; }
    }
        

    public bool IsRessource => _isRessource;
    public bool IsDiscoveredByDefault => _isDiscoveredByDefault;


    public Sprite SpriteQuantity0 => _spriteQuantity0;
    public Sprite Sprite => _sprite;
    public Sprite SpriteQuantity1 => _spriteQuantity1;
    public Sprite SpriteQuantity2 => _spriteQuantity2;
    public Sprite SpriteQuantity3 => _spriteQuantity3;

    public EItemUsageType UsageType => _usageType;
    public GameObject LinkedObject => _linkedObject;
    public float LinkedValue => _linkedValue;

    public bool UseMultipleSelection { get => _useMultipleSelection; set => _useMultipleSelection = value; }
    public bool CraftMultipleSelection { get => _craftMultipleSelection; set => _craftMultipleSelection = value; }

    public string Audio => _audio;


    #endregion Properties


}
