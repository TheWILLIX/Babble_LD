using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;

public class TickTockMinigame : MonoBehaviour
{
    [SerializeField] private Transform tock;
    [SerializeField] private Transform target;
    [SerializeField] private Transform[] smallTargets;
    [SerializeField] private Transform tick;
    [SerializeField] private float tickSpeed = 2f;
    [SerializeField, Range(0f, 1f)] private float range = 0.38f;
    [SerializeField, Range(0f, 1f)] private float smallRange = 0.1f;
    public float _current;
    private bool _direction;
    private bool _valid;

    private List<float[]> _bounds = new List<float[]>();

    public bool Valid { get => _valid; set => _valid = value; }

    public void Enable()
    {
        gameObject.SetActive(true);
        _bounds.Clear();
        var rand = new System.Random();
        var count = rand.Next(1, smallTargets.Length + 1);
        if (count == 1)
        {
            var randomPoint = (float)rand.NextDouble();
            _bounds.Add(new float[] { randomPoint, (randomPoint + range).Min(1) });
            if (randomPoint + range > 1)
                _bounds.Add(new float[] { 0, (1 - randomPoint - range).Abs() });
            _direction = Random.Range(0, 2) == 0;
            target.rotation = Quaternion.identity * Quaternion.Euler(new Vector3(0, 0, 360 * (randomPoint + range / 2 - 0.5f)));
            target.gameObject.SetActive(true);
            foreach (var smalltarget in smallTargets)
            {
                smalltarget.gameObject.SetActive(false);
            }
        }
        else
        {
            float areaSize = 1f / count;
            for(int i = 0; i < count; i++)
            {
                var currentArea = areaSize * i;
                var randomPoint = (float)rand.NextDouble();
                randomPoint = (areaSize - smallRange) * randomPoint;
                randomPoint += currentArea;
                _bounds.Add(new float[] { randomPoint, (randomPoint + smallRange).Min(1) });
                if (randomPoint + smallRange > 1)
                    _bounds.Add(new float[] { 0, (1 - randomPoint - smallRange).Abs() });
                smallTargets[i].rotation = Quaternion.identity * Quaternion.Euler(new Vector3(0, 0, 360 * (randomPoint + smallRange / 2 - 0.5f)));
            }
            target.gameObject.SetActive(false);
            for (int i = 0; i < smallTargets.Length; i++)
            {
                var smalltarget = smallTargets[i];
                smalltarget.gameObject.SetActive(i < count);
            }
        }
        
    }

    public void Disable()
    {
        _current = 0;
        _valid = false;
        gameObject.SetActive(false);
    }
    void Update()
    {
        if (_direction)
        {
            _current += Time.deltaTime * tickSpeed;
            if (_current >= 1)
                _current -= 1;
        }
        else
        {
            _current -= Time.deltaTime * tickSpeed;
            if (_current < 0)
                _current += 1;
        }
        var val = false;
        foreach (var bound in _bounds)
        {
            if (_current.IsBetween(bound[0], bound[1]))
                val = true;
        }
        _valid = val;
        tick.rotation = Quaternion.identity * Quaternion.Euler(new Vector3(0,0, 360 * (_current - 0.5f)));
    }
}
