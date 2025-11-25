using ClemCAddons.Player;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons.Utilities;
using System.Linq;
namespace ClemCAddons
{
    namespace NPCMovement
    {
        [ExecuteInEditMode]
        public class SeekerMedusa : MonoBehaviour
        {
            [Header("Jumps")]
            [SerializeField] private float _jumpsDistance = 0.5f;
            [SerializeField] private float _jumpVariation = 0.1f;
            [SerializeField] private float _precision = 0.1f;
            [Header("Speed")]
            [SerializeField] private float _speed = 1;
            [SerializeField] private float _factorLowerSpeedCurve = 2;
            [Header("VFX")]
            [SerializeField] private string _VFX;
            [Header("Collisions")]
            [SerializeField] private float _targetTolerance = 2f;
            [SerializeField] private int _maxPathLength = 20;
            [SerializeField] private int _iterationsPerPoint = 10;

            [Header("Resource")]
            [SerializeField] private Item _itemType;

            [Header("Animation")]
            [SerializeField] private Animator _medusaAnimator = null;

            [Header("Debug")]
            [SerializeField] private Transform _target;
            [SerializeField] private bool _activate;

            private int _step = -1;

            private Vector3[] _path;

            private CharacterMovement _player;

            private CharacterMovement Player { get {
                    if(_player == null)
                        FindObjectOfType<CharacterMovement>();
                    return _player; } }
           
            private float _timing = 0;

            private Rigidbody rigidbody;

            private Vector3 _currentGoal;

            void Awake()
            {
                _step = -1;
            }

            void Start()
            {
                if (!Application.isPlaying)
                    return;
                _player = FindObjectOfType<CharacterMovement>();
                _currentGoal = transform.position;
                rigidbody = GetComponent<Rigidbody>();
                var potentialTargets = FindResources();
                foreach(var potentialTarget in potentialTargets)
                {
                    var res = IteratePath(transform.position, potentialTarget.position);
                    if (res != null)
                    {
                        res.Add(potentialTarget.position);
                        _path = res.ToArray();
                        _step = 0;
                        NewPath();
                        return;
                    }
                }
                GetComponentInChildren<PickupEffect>()?.Pickup(() => { Destroy(transform.parent.gameObject); });
                InventoryManager.Instance.AddItem(_itemType);
                var notificationData = new Notification.NotificationData()
                {
                    Title = _itemType.CleanName,
                    Subtitle = "$Resource.Subtitle",
                    Description = "$Resource.Poulpe.Description",
                    KeyIcon = UIManager.Instance.UIController.HUDBank.InventoryInteractionKey,
                    ElementIcon = _itemType.Sprite,
                    Background = UIManager.Instance.UIController.HUDBank.BackgroundNotification,
                    ActionUponOpening = () => { UIManager.Instance.UIController.OpenInventory(true, false, _itemType.Name); }
                };
                Notification.TriggerNotification(notificationData, 0);
            }

            void Update()
            {
                if (Player == null)
                    return;

                if (_activate == true)
                {
                    _activate = false;
                    IteratePath(transform.position, _target.position);
                }

                if (_step != -1 && Application.isPlaying)
                    Roam();
            }

            private Transform[] FindResources()
            {
                var resourcePickUps = FindObjectsOfType<ResourcePickUp>()
                    .Where(t => t.isActiveAndEnabled == true)
                    .Select(t => t.transform);
                resourcePickUps.OrderBy(t => t.position.Distance(transform.position));
                return resourcePickUps.ToArray();
            }

            private void MoveTo(Vector3 newPosition)
            {
                if(GetComponent<Rigidbody>().SweepTest(transform.position.Direction(newPosition), out var hit, transform.position.Distance(newPosition),QueryTriggerInteraction.Ignore))
                {
                    transform.position = hit.point;
                    _currentGoal = transform.position + hit.normal * (_jumpsDistance + (Random.Range(-1f, 1) * _jumpVariation));
                }
                else
                {
                   
                    transform.position = newPosition;
                }
            }


            private void Roam()
            {
                MoveTo(Vector3.Lerp(transform.position, _currentGoal, Time.deltaTime * _speed + (transform.Distance(_currentGoal) * 0.01f * _factorLowerSpeedCurve)));
                transform.rotation = Quaternion.Lerp(transform.rotation, (transform.position - _currentGoal).normalized.ToQuaternion(Quaternion.identity), Time.deltaTime * 5);
                if (transform.Distance(_currentGoal) < _precision)
                {
                    NewPath();
                }
                if(transform.Distance(_path[_step]) < _precision)
                {
                    _step++;
                    if (_step == _path.Length)
                    {
                        _step = -1;
                        GetComponentInChildren<PickupEffect>()?.Pickup(() => { Destroy(transform.parent.gameObject); });
                    }
                }
            }

            private void NewPath()
            {
                Vector3 dir = transform.position.Direction(_path[_step]);
                _currentGoal = transform.position + (dir * (_jumpsDistance + (Random.Range(-1f, 1) * _jumpVariation))) + (dir.Right() * (Random.Range(-1f, 1) * _jumpVariation));
                if (_VFX != "")
                    VFXSpawner.Spawn(_VFX, transform.position, Quaternion.FromToRotation(transform.position, _currentGoal));
                _medusaAnimator.SetTrigger("DeplacementOn");
            }

            private List<Vector3> IteratePath(Vector3 source, Vector3 target)
            {
                target += Vector3.up;
                List<Vector3> points = new List<Vector3>() { source };
                for (int t = 0; t < _maxPathLength; t++)
                {
                    points.Add(Vector3.Lerp(points[t], target, 0.5f));
                    if (IteratePoints(ref points, target, t + 1))
                        return points;
                }
                return null;
            }

            private bool IteratePoints(ref List<Vector3> points, Vector3 target, int point)
            {
                var po = points;
                for (int t = 0; t < _iterationsPerPoint; t++)
                {
                    po[point] = Vector3.Lerp(po[point - 1], target, 0.5f) + po[point - 1].Direction(target).Right() * t;
                    if (CheckPath(po, target))
                    {
                        points = po;
                        return true;
                    }
                    po[point] = Vector3.Lerp(po[point - 1], target, 0.5f) + po[point - 1].Direction(target).Left() * t;
                    if (CheckPath(po, target))
                    {
                        points = po;
                        return true;
                    }
                }
                return false;
            }

            private bool CheckPath(List<Vector3> points, Vector3 target)
            {
                for(int i = 0; i < points.Count; i++)
                {
                    if (i == points.Count - 1)
                    {
                        if (CheckForTarget(points[i], target, true))
                            return false;
                    }
                    else
                    {
                        if (CheckForTarget(points[i], points[i+1], false))
                            return false;
                    }
                }
                return true;
            }

            private bool CheckForTarget(Vector3 source, Vector3 target, bool withTolerance)
            {
                var posSave = rigidbody.position;
                rigidbody.position = source;
                bool result = rigidbody.SweepTest(source.Direction(target), out var hit, source.Distance(target), QueryTriggerInteraction.Ignore);
                rigidbody.position = posSave;

                if (hit.point.Distance(target) < _targetTolerance && withTolerance)
                    result = false;

                if (!result)
                    Debug.DrawLine(source, target, Color.green, 10);
                else
                    Debug.DrawLine(source, hit.point, Color.red, 10);

                return result;
            }
        }
    }
}

