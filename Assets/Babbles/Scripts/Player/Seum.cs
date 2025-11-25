using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;
using Yarn.Unity;
using System.Linq;
using ClemCAddons.Player;

public class Seum : Singleton<Seum>
{
    #region Attributes
    [SerializeField,LabelOverride("Max Seum")] private float _seumMax = 100;
    [SerializeField] private InMemoryVariableStorage _variableStorage = null;
    [SerializeField] private string _seumYarnVariableName = string.Empty;


    //[SerializeField] private string _startScene = "ClemCaTestScene";
    [SerializeField] private float _defaultSeum = 80f;

    [SerializeField] private float _changeUpdateSpeed = 10f;
    [Header("Pathfinding")]
    [SerializeField] private float _teleportDistance = 20f;
    [SerializeField] private float _goBackDelay = 3f;
    [SerializeField] private float _goingBackSpeed = 10f;

    [SerializeField] private Rigidbody _validator;

    private bool _goingBack = false;

    private float _goBackTimer = 0;

    private float _seum = 0;
    private bool _preloaded = false;
    private List<float> _queue = new List<float>();
    private bool _queueReserved;
    private float _seumSave;

    private bool _firstTimeBeingTpOut;

    public float SeumMax { get => _seumMax;}
    public Rigidbody Validator { get => _validator; }
    #endregion Attributes

    #region Methods
    protected new void Start()
    {
        _seum = _defaultSeum;
        _seumSave = _seum;
        if(_seumYarnVariableName != "")
            _variableStorage.SetValue(_seumYarnVariableName, _seum);
    }

    protected override void Update()
    {
        RunVFX();
        if (_seum >= _seumMax && Areas.PlayerRef != null)
        {
            if (!_goingBack)
            {
                if (!Areas.EnteredFull)
                {
                     switch(Areas.Latest.AreaType)
                     {
                        case Areas.AreaTypes.Pollution:
                            UIManager.Instance.UIController.StartDialogue("ZonePolluée_DemiTour", false);
                            break;
                        case Areas.AreaTypes.Dark:
                            UIManager.Instance.UIController.StartDialogue("ZoneSombre_DemiTour", false);

                            break;
                     }
                    _goingBack = true;
                }
                else if (_goBackTimer > _goBackDelay)
                {
                    switch (Areas.Latest.AreaType)
                    {
                        case Areas.AreaTypes.Pollution:
                            UIManager.Instance.UIController.StartDialogue("ZonePolluée_Peur", false);
                            break;
                        case Areas.AreaTypes.Dark:
                            UIManager.Instance.UIController.StartDialogue("ZoneSombre_Peur", false);
                            break;
                    }
                    _goingBack = true;
                }
            }
            if(!Areas.EnteredFull || _goBackTimer > _goBackDelay)
                Areas.PlayerRef.GetComponent<CharacterMovement>().SetIgnoreInput(true);
            _goBackTimer += Time.deltaTime;
            if(_goBackTimer > _goBackDelay)
            {
                if (Areas.PlayerRef.Distance(Areas.Path[0]) >= _teleportDistance)
                {
                    _goBackTimer = 0;
                    Areas.PlayerRef.position = Areas.Path[0];
                    var r = FindObjectsOfType<Areas>();
                    foreach(var area in r)
                    {
                        area.SimulateExit();

                        if(_firstTimeBeingTpOut == false)
                        {

                        }

                        UIManager.Instance.UIController.StartDialogue("ZonePolluée_Peur", false);
                    }
                }
                else
                    GoBack();
            }
        }
        else
        {
            _goingBack = false;
            _goBackTimer = 0;
        }
    }

    private GameObject _currentVFX;
    private int state = 1;
    private void RunVFX()
    {
        int newstate;
        if (_seum > 2 / 3f * _seumMax)
            newstate = 2;
        else if (_seum < 1 / 3f * _seumMax)
            newstate = 0;
        else
            newstate = 1;

        if(newstate != state)
        {
            if (_currentVFX != null)
                Destroy(_currentVFX);
            state = newstate;
            var player = FindObjectOfType<CharacterMovement>();
            switch (newstate)
            {
                case 0:
                    AudioManager.Start2DSound("S_BulleGigaFrais");
                    _currentVFX = VFXSpawner.Spawn("UltraFrais", player.transform, player.transform.position + Vector3.up);
                    break;
                case 1:
                    AudioManager.Start2DSound("S_BulleOK");
                    break;
                case 2:
                    AudioManager.Start2DSound("S_BulleSeum");
                    _currentVFX = VFXSpawner.Spawn("Seum", player.transform, player.transform.position + Vector3.up);
                    break;
                default:
                    break;
            }
        }
    }


    private int _currentDirection;
    private void GoBack()
    {
        var target = Areas.Path[0];
        for (int i = 0; i < Areas.Path.Count; i++)
        {
            if (!Areas.PlayerRef.GetComponent<Rigidbody>().SweepTest(Areas.PlayerRef.position.Direction(Areas.Path[i] + Vector3.up * 0.5f), out _, Areas.PlayerRef.Distance(Areas.Path[i] + Vector3.up * 0.5f), QueryTriggerInteraction.Ignore))
            {
                target = Areas.Path[i];
                Debug.DrawLine(Areas.PlayerRef.position, Areas.Path[i], Color.magenta);
                break;
            }
        }
        _currentDirection = ScoreAllPaths(Areas.PlayerRef.position, target, 5, _currentDirection);
        if(_currentDirection != 0)
        {
            Vector3 direction;
            if (_currentDirection == 1)
                direction = Areas.PlayerRef.position.Direction(target).normalized.Right();
            else
                direction = Areas.PlayerRef.position.Direction(target).normalized.Left();
            var origin = Vector3.Lerp(Areas.PlayerRef.position, target, 0.5f) + direction;
            var scoredTarget = Vector3.Slerp(Areas.PlayerRef.position - origin, target - origin, 1 / 5f);
            scoredTarget += origin;
            Areas.PlayerRef.GetComponent<CharacterMovement>().AddImpulse(Areas.PlayerRef.position.Direction(scoredTarget) * _goingBackSpeed);
        }
        else
        {
            Areas.PlayerRef.GetComponent<CharacterMovement>().AddImpulse(Areas.PlayerRef.position.Direction(target) * _goingBackSpeed);
        }
    }

    private int ScoreAllPaths(Vector3 origin, Vector3 destination, int pollingRate, int previousDirection)
    {
        bool cleft = CheckDirection(origin, origin.Direction(destination).Left(), 1);
        bool cright = CheckDirection(origin, origin.Direction(destination).Right(), 1);
        if(cright && cleft)
        {
            //if we are not constrained by walls, we can go straight
            return 0;
        }
        bool left = ScorePath(origin, destination, pollingRate, false);
        bool right = ScorePath(origin, destination, pollingRate, true);
        if(right && left)
        {
            // we are contrained by walls, but we can turn in either direction
            // following the previous direction has the best consistency, we might still be turning
            return previousDirection;
        }
        if (right && cleft)
        {
            // we can't go left but can go right, it's most likely a turn right, we want to take the right path
            return 1;
        }
        if (left && cright)
        {
            // we can't go right but can go left, it's most likely a turn left, we want to take the left path
            return -1;
        }
        return 0;
    }
    
    private bool CheckDirection(Vector3 origin, Vector3 direction, float distance)
    {
        _validator.position = origin;
        return !_validator.SweepTest(direction, out _, distance, QueryTriggerInteraction.Ignore);
    }

    private bool ScorePath(Vector3 origin, Vector3 destination, int pollingRate, bool direction)
    {
        var offset = direction ? origin.Direction(destination).Right() : origin.Direction(destination).Left();
        Vector3 center = Vector3.Lerp(origin, destination, 0.5f) + offset;
        bool valid = true;
        float delta = 1f / pollingRate;
        Vector3[] points = new Vector3[pollingRate];
        points[0] = origin;
        _validator.position = origin;
        for (int i = 1; i < pollingRate; i++)
        {
            points[i] = Vector3.Slerp(origin - center, destination - center, i * delta) + center;
            Debug.DrawLine(_validator.position, points[i], Color.blue);
            if (_validator.SweepTest(_validator.position.Direction(points[i]), out _, _validator.position.Distance(points[i]), QueryTriggerInteraction.Ignore))
                return false;
            _validator.position = points[i];
        }
        return valid;
    }

    public float GetSeum()
    {
        return _seum;
    }

    public void ChangeSeum(float change)
    {
        _queue.Add(change); // add change value to queue
        EvaluateQueue();
    }

    private void EvaluateQueue()
    {
        if (_queueReserved)
            return;
        _queueReserved = true;
        StartCoroutine(SeumUpdate());
    }

    IEnumerator SeumUpdate()
    {
        while (_queue.Count > 0)
        {
            var queueV = (_seumSave + _queue[0]).Clamp(0, _seumMax);
            _seum = (_seum + (queueV - _seum).Sign() * Time.smoothDeltaTime * _changeUpdateSpeed).Clamp(_seum.Min(queueV),_seum.Max(queueV));
            if (_seum == queueV)
            {
                _queue.RemoveAt(0);
                if(_seumSave >= _seum)
                {
                    //Thermomètre going down so we should be happy
                } else {
                    BabblesVibration.CustomVibration(1.2f, 0.2f); //Vibration 
                    //Not Happy
                }

                _seumSave = _seum;
                if (_seumYarnVariableName != "")
                    _variableStorage.SetValue(_seumYarnVariableName, _seumSave);
                Debug.Log("Moved seum to " + _seumSave);
            }
            yield return new WaitForEndOfFrame();
        }
        _queueReserved = false;
    }

    // IMPORTANT:
    // il va falloir à la place vérifier si le joueur est dans une zone stressante, si c'est le cas il faut le téléporter en dehors (Definir 'en dehors').
    #endregion Methods
}