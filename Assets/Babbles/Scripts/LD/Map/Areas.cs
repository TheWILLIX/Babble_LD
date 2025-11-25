using UnityEngine;
using ClemCAddons;
using ClemCAddons.Utilities;
using System;
using System.Linq;
using System.Collections.Generic;
using ClemCAddons.Player;

public class Areas : MonoBehaviour
{
    [Header("Area")]
    [SerializeField] private AreaTypes _areaType;
    [SerializeField] private float _increase = 1f;
    [Header("Delay")]
    [SerializeField] private DelayType _delayType;
    [SerializeField] private float delay = 10;
    [SerializeField, DrawIf("_delayType", DelayType.RandomInRange,ComparisonType.Equals)] private float maxDelay = 15;
    [SerializeField] private string[] _exceptions;
    [Header("Pathfinding")]
    [SerializeField] private int _pollingRateMs = 150;
    [SerializeField] private float _minDistance = 0.5f;
    private Rigidbody _validator;


    private static Transform _playerRef;
    private static List<Vector3> _path = new List<Vector3>();
    private static int _count = 0;
    private static int _interdiction;
    private static bool _enteredFull;
    private int currentDelay;
    private bool isIn = false;
    private static Areas _latest;

    public static bool EnteredFull { get => _enteredFull; }
    public static Transform PlayerRef { get => _playerRef; set => _playerRef = value; }
    public static List<Vector3> Path { get => _path; set => _path = value; }
    public bool IsIn { get => isIn; }
    public static Areas Latest { get => _latest;  }
    public AreaTypes AreaType { get => _areaType; }

    public enum AreaTypes
    {
        Dark,
        Pollution
    }
    
    public enum DelayType
    {
        Fixed,
        RandomInRange
    }

    public static void Interdict(bool value)
    {
        if (value)
            _interdiction++;
        else
            _interdiction--;
    }

    void Start()
    {
        if (_delayType == DelayType.Fixed)
            currentDelay = (delay * 1000).Round();
        else
            currentDelay = (RandomC.RandomFloat(delay, maxDelay) * 1000).Round();
        _validator = Seum.Instance.Validator;
    }

    void Update()
    {
        if (isIn)
        {
            _latest = this;
            if (ClemCAddons.Utilities.Timer.MinimumDelay("AreaNavigation".GetHashCode(), _pollingRateMs, false))
            {
                bool t = true;
                var playerPos = PreCalculatePoint(_playerRef.position);
                foreach (var pos in _path)
                {
                    if (playerPos.Distance(pos) < _minDistance)
                    {
                        t = false;
                    }
                }
                if (t)
                {
                    AddPoint(playerPos);
                }
            }
            for(int i = 0; i < _path.Count - 1; i++)
            {
                Debug.DrawLine(_path[i], _path[i + 1], Color.red);
            }
            int count = 0;
            foreach (string exception in _exceptions)
            {
                var r = FindObjectsOfType<ExceptionItem>().ToList().Find(t => t.Type.ToLower() == exception.ToLower());
                if (r != null)
                    count++;
            }
            if (count > 0 || _interdiction > 0)
                return;
            switch (_areaType)
            {
                case AreaTypes.Dark:
                    if (ClemCAddons.Utilities.Timer.MinimumDelay("DARK".GetHashCode(), currentDelay, false))
                    {
                        Seum.Instance.ChangeSeum(_increase);
                        if (_delayType == DelayType.RandomInRange)
                            currentDelay = (RandomC.RandomFloat(delay, maxDelay) * 1000).Round();
                    }
                    break;
                case AreaTypes.Pollution:
                    if (ClemCAddons.Utilities.Timer.MinimumDelay("DARK".GetHashCode(), currentDelay, false))
                    {
                        Seum.Instance.ChangeSeum(_increase);
                        if(_delayType == DelayType.RandomInRange)
                            currentDelay = (RandomC.RandomFloat(delay, maxDelay) * 1000).Round();
                    }
                    break;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isIn = true; 

            _playerRef = other.transform;

            switch (_areaType)
            {
                case AreaTypes.Dark:
                    AudioManager.Start2DSound("S_EntreePollution");
                    break;
                case AreaTypes.Pollution:
                    AudioManager.Start2DSound("S_EntreeZoneSombre");
                    break;
            }

            if (_count == 0)
            {
                BabblesVibration.CustomVibration(1f, 0.025f); //Vibrations

                _path.Clear();
                int inside = GetComponent<Collider>().bounds.Contains(_playerRef.position) ? 1 : -1;
                var closest = GetComponent<Collider>().ClosestPointOnBounds(_playerRef.position);
                var toAdd = _playerRef.position.Direction(closest).SetY(0) * 3 * inside;
                if (toAdd.Equals(Vector3.zero))
                {
                    toAdd = -_playerRef.GetComponent<Rigidbody>().velocity.SetY(0);
                }
                _validator.position = PositionOnGround(_playerRef.position) + Vector3.zero.SetY(0.1f);
                if(_validator.SweepTest(toAdd.normalized, out var hit, toAdd.magnitude, QueryTriggerInteraction.Ignore))
                {
                    toAdd = toAdd.normalized * (hit.distance * 0.9f);
                }
                _path.Add(PositionOnGround(closest + toAdd));
                _path.Add(_playerRef.position);
                _enteredFull = Seum.Instance.GetSeum() == Seum.Instance.SeumMax;
            }
            _count++;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isIn = false;
            _count--;
            if(_count == 0)
            {
                _playerRef.GetComponent<CharacterMovement>().SetIgnoreInput(false);
                _playerRef = null;
            }
        }
    }

    public void SimulateExit()
    {
        if (_playerRef == null || !isIn)
            return;
        isIn = false;
        _count--;
        if (_count == 0)
        {
            _playerRef.GetComponent<CharacterMovement>().SetIgnoreInput(false);
            _playerRef = null;
        }
    }

    private Vector3 PreCalculatePoint(Vector3 point)
    {
        point = PositionOnGround(point);
        bool cleft = CheckDirection(point, _path[_path.Count - 1].Direction(point).Left(), 0.3f);
        bool cright = CheckDirection(point, _path[_path.Count - 1].Direction(point).Right(), 0.3f);
        if (cleft && cright)
        {
            return point;
        }
        else if (cleft)
        {
            return point + _path[_path.Count - 1].Direction(point).Left() * 0.3f;
        }
        else if (cright)
        {
            return point + _path[_path.Count - 1].Direction(point).Right() * 0.3f;
        }
        else
        {
            return point;
        }
    }

    private void AddPoint(Vector3 point)
    {
        _path.Add(point);
    }
    private Vector3 PositionOnGround(Vector3 point)
    {
        _validator.position = point;
        if (_validator.SweepTest(Vector3.down, out var hit, 5, QueryTriggerInteraction.Ignore))
        {
            point = point + Vector3.down * hit.distance;
        }
        return point;
    }
    private bool CheckDirection(Vector3 origin, Vector3 direction, float distance)
    {
        _validator.position = origin;
        return !_validator.SweepTest(direction, out _, distance, QueryTriggerInteraction.Ignore);
    }
}
