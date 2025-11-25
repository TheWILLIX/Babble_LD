using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using ClemCAddons;

public class Elevator : MonoBehaviour
{
    [SerializeField] private float _floorHeight;
    [SerializeField] private int _minFloor = -1;
    [SerializeField] private int _maxFloor = 1;
    private Vector3 _basePosition;
    private int _current;
    private List<int> _targets = new List<int>();

    void Start()
    {
        _basePosition = transform.position;
    }

    public void Move(bool up)
    {
        if (_targets.Contains(_current + (up ? 1 : -1)))
            return;
        _targets.Add(_current + (up ? 1 : -1));
        Sort();
    }

    public void Move(int target)
    {
        if (_targets.Contains(target))
            return;
        _targets.Add(target);
        Sort();
    }

    private void Sort()
    {
        _targets.RemoveAll(t => !t.IsBetween(_minFloor, _maxFloor));
        _targets.OrderBy(t => (t - _current).Abs());
        var mainTarget = _targets[0];
        var possibilities = _targets.Where(t => Mathf.Sign(t - _current) == Mathf.Sign(mainTarget - _current));
        var rest = _targets.Where(t => Mathf.Sign(t - _current) != Mathf.Sign(mainTarget - _current));
        _targets = possibilities.Concat(rest).ToList();
    }

    void Update()
    {
        if(_targets.Count > 0)
        {
            var targetPos = _basePosition.y + _targets[0] * _floorHeight;
            if (transform.position.y == targetPos)
            {
                _current = _targets[0];
                _targets.RemoveAt(0);
                return;
            }
            var dist = targetPos - transform.position.y;
            transform.position = transform.position + Vector3.up * (dist.Sign() * Time.deltaTime).Clamp(dist.Min(-dist), dist.Max(-dist)); 
        }
    }
}
