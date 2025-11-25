using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;
using System.Linq;
using UnityEngine.Serialization;

public class CollapsibleWall : MonoBehaviour
{
    [SerializeField] private Transform _groundMarker;
    private List<GameObject> _everyRock = new List<GameObject>();
    [SerializeField] private bool _check = true;
    [SerializeField] private int _timeBetweenBlocks = 100;
    [SerializeField] private float _fallDurationMultiplier = 0.5f;
    [SerializeField, Range(1,100)] private float a = 5; // must be > 1


    public void Collapse(Vector3 position)
    {
        for (int i = 0; i < transform.childCount; i++)
            _everyRock.Add(transform.GetChild(i).gameObject);
        _everyRock.Remove(_groundMarker.gameObject);

        var length = _everyRock.Count;
        for (int i = 0; i < length; i++)
        {
            _everyRock.Sort(new RandomComparer());
            var rock = _everyRock.GetClosest(position);
            _everyRock.Remove(rock);
            var rockPosition = rock.transform.position;
            _ = ClemCAddons.Utilities.GameTools.DelayedCall(_timeBetweenBlocks * i,
                () => {
                    ClemCAddons.Utilities.Lerper.ConstantLerp
                    (0, 1, (rock.transform.position.y - _groundMarker.position.y).Abs() * _fallDurationMultiplier,
                        (float x) => {
                            SetRockProgression(rock,
                                rockPosition,
                                rockPosition.SetY(0)
                                    + Vector3.zero.SetY(_groundMarker.position.y - rock.GetComponentInChildren<Renderer>().bounds.extents.y),
                                x);
                        },
                        () => {
                            DestroyRock(rock);
                        }
                    );
                }
            );
        }
    }

    private void DestroyRock(GameObject rock)
    {
        if(Application.isPlaying)
            Destroy(rock);
    }

    private void SetRockProgression(GameObject rock, Vector3 origin, Vector3 target, float x)
    {
        var ax = Mathf.Pow(a, x);
        var r = (ax - 1) / (a - 1);
        // someone just happened to have searched the exact same thing than me before https://math.stackexchange.com/a/384615
        if(Application.isPlaying)
            rock.transform.position = Vector3.Lerp(origin, target, r);
    }
}

public class CollapsibleComparer : IComparer<GameObject>
{
    public int Compare(GameObject x, GameObject y)
    {
        return x.transform.position.y.CompareTo(y.transform.position.y);
    }
}
public class RandomComparer : IComparer<GameObject>
{
    public int Compare(GameObject x, GameObject y)
    {
        return Random.Range(-1,1);
    }
}