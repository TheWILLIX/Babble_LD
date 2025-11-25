using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luminosity.IO;
using ClemCAddons.Player;
using ClemCAddons.Utilities;
using System.Linq;
using UnityEngine.Serialization;
using System;

namespace ClemCAddons
{
    namespace CameraAndNodes
    {
        public class TPSCameraWithNodeSupport : MonoBehaviour
        {
            [Header("Inputs")]
            [SerializeField] private string _horizontalMovement = "LookHorizontal";
            [SerializeField] private string _verticalMovement = "LookVertical";
            [SerializeField] private int _inputSensitivity = 200;
            [Header("Settings")]
            [SerializeField] private bool _scrollToZoom = false;
            [SerializeField] private float _scrollSensitivity = 1;
            [SerializeField] private float _distance = 2;
            [SerializeField] private float _heightOffset = 2;
            [SerializeField] private float _linearSmoothing = 0.2f;
            [SerializeField] private float _nodePercSmoothing = 0.2f;
            [SerializeField] private float _nodeFixedTolerance = 0.5f;
            [SerializeField] private float _playerMovementSpring = 0.02f;
            [SerializeField] private bool _customPlayerClass = false;
            [SerializeField, DrawIf("_customPlayerClass", true, ComparisonType.Equals)] private string _playerClassName;
            [SerializeField] private bool _useDifferentTransform = false;
            [SerializeField, DrawIf("_useDifferentTransform", true, ComparisonType.Equals)] private Transform _differentTransform;
            [SerializeField] private bool _inverseCamButton = false;
            [SerializeField, DrawIf("_inverseCamButton", true, ComparisonType.Equals)] private string _inverseCamButtonName = "InverseCam";
            [SerializeField] private bool _blockCamButton = false;
            [SerializeField, DrawIf("_blockCamButton", true, ComparisonType.Equals)] private string _blockCamButtonName = "BlockCam";
            [Header("Camera boom")]
            [SerializeField] private string _cameraHitTag = "Hittable";
            [SerializeField] private float _cameraBoomSmoothing = 0.05f;
            [SerializeField] private float _obstacleMinimumDistance = 0.1f;
            [Header("Angle Limiting")]
            [SerializeField] private float _topBottomLimit = 0.1f;
            [Header("Delay")]
            [SerializeField] private int _delayFrames = 0;
            [SerializeField] private DelayMode _delayMode = DelayMode.y;

            private dynamic _player = null;
            public float _defaultDistance;
            private Vector3 _position = Vector3.forward;
            private Vector2 _currentOffset = Vector2.zero;
            private float _currentCheatOffset = 0f;
            private BoxCollider _boxCollider;
            private NodeHelpSettings _settings;
            private Vector3 _offSetTurned = Vector3.forward;
            private Vector3 _previousChange = Vector3.zero;
            private Vector3 _previousPosition = Vector3.zero;
            private Vector3 _cheatOffsetTurned = Vector3.forward;
            private float _zoom = 0;
            private bool _camInversed;
            private bool _frozen;
            private bool _transiMode;

            public event EventHandler BreakSimulatedInput;


            private List<KeyValuePair<Vector3, float>> _delaySavedFrames = new List<KeyValuePair<Vector3, float>>();
           
            private enum DelayMode
            {
                y,
                xz,
                xyz
            }

            private Vector2 _simulatedInput;

            private bool _blocked;
            private bool isPlayerCharacterMovement
            {
                get
                {
                    return _player.GetType() == typeof(CharacterMovement);
                }
            }

            private Transform playerTransform
            {
                get
                {
                    return _useDifferentTransform ? _differentTransform : (Transform)_player.transform;
                }
            }

            public BoxCollider BoxCollider
            {
                get
                {
                    try
                    {
                        _boxCollider = FindObjectOfType<NodeHelperSettings>().GetComponentInChildren<BoxCollider>();
                    }
                    catch
                    {
                        Debug.LogWarning("Si une node rectangulaire est utilisée, le prefab NodeHelperSettings doit être dans la scène");
                    }
                    return _boxCollider;
                }
                set
                {
                    _boxCollider = value;
                }
            }

            private Vector3 playerPosition
            {
                get
                {
                    if (_delayFrames == 0 || _delaySavedFrames.Count == 0)
                        return playerTransform.position;
                    else
                    {
                        var saveFrame = _delaySavedFrames[0].Key;
                        var timeDiff = Time.timeSinceLevelLoad - _delaySavedFrames[0].Value;
                        var saveDiff = playerTransform.position - saveFrame;
                        if(timeDiff != 0 && saveDiff != Vector3.zero)
                            saveFrame += saveDiff / timeDiff * Time.deltaTime;
                        switch (_delayMode)
                        {
                            case DelayMode.y:
                                return playerTransform.position.SetY(saveFrame.y);
                            case DelayMode.xz:
                                return saveFrame.SetY(playerTransform.position.y);
                            case DelayMode.xyz:
                                return saveFrame;
                            default:
                                return saveFrame;
                        }
                    }
                }
            }

            public bool BlockCam { get => _blocked; set => _blocked = value; }
            public float Distance { get => _distance; set => _distance = value; }
            public Vector2 SimulatedInput { get => _simulatedInput; set => _simulatedInput = value; }
            public Vector3 Position { get => _position; set => _position = value; }
            public int Delay { get => _delayFrames; set => _delayFrames = value; }
            public bool CamInversed { get => _camInversed; set => _camInversed = value; }
            public bool TransiMode { get => _transiMode; set => _transiMode = value; }

            void Start()
            {
                _defaultDistance = _distance;
                if (_customPlayerClass)
                    _player = FindObjectOfType(Type.GetType(_playerClassName));
                else
                    _player = FindObjectOfType<CharacterMovement>();
                var r = FindObjectOfType<NodeHelperSettings>();
                _settings = r != null ? r.Settings : new NodeHelpSettings(false, false);
            }

            void Update()
            {
                try
                {
                    if (_blockCamButton && InputManager.GetButton(_blockCamButtonName))
                    {
                        _blocked = !_blocked;
                    }
                }
                catch
                {
                    Debug.LogWarning("Inputs are not updated, missing blocking cam debug button");
                }
                if (_frozen)
                {
                    transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(transform.position.Direction(playerPosition)), Time.deltaTime);
                    return;
                }

                ManageDelay();

                CamTriangulation();
                GetInputs();
                var change = (_position * (_distance + _zoom + Camera.allCameras[0].nearClipPlane / 2)) + Vector3.up * _heightOffset + _offSetTurned + _cheatOffsetTurned;
                if (_linearSmoothing != 0)
                { // lerp around the player, but moves with the player as point of reference
                    var position = Vector3.Lerp(_previousPosition, playerPosition, Time.smoothDeltaTime / _playerMovementSpring);
                    var objective = position + change;
                    Vector3 pos;
                    if (playerPosition.CastToLineOnly(objective, (LayerMask)(isPlayerCharacterMovement ? _player.CollisionLayer : 1 << LayerMask.NameToLayer("Default")), _cameraHitTag, out RaycastHit hit))
                    {
                        pos = position + ((objective - position).normalized * (hit.distance - Camera.allCameras[0].nearClipPlane / 2).Max(Camera.allCameras[0].nearClipPlane / 2));
                    }
                    else
                    {
                        pos = objective;
                    }
                    if (!_transiMode)
                    {
                        transform.position = Vector3.Lerp(transform.position, pos, Time.deltaTime / _cameraBoomSmoothing);
                        transform.rotation = Quaternion.LookRotation(Vector3.Lerp(transform.forward, -_position.normalized, Time.deltaTime / _cameraBoomSmoothing));
                    }
                    else
                        transform.LookAt(playerTransform.position + _offSetTurned + Vector3.up * _heightOffset);
                    _previousChange = change;
                    _previousPosition = position;
                }
                else
                {
                    var objective = playerPosition + change;
                    if (playerPosition.CastToLineOnly(objective, (LayerMask)(isPlayerCharacterMovement ? _player.CollisionLayer : 1 << LayerMask.NameToLayer("Default")), _cameraHitTag, out RaycastHit hit))
                    {
                        if (!_transiMode)
                            transform.position = hit.point - ((objective - playerPosition).normalized * Camera.allCameras[0].nearClipPlane / 2);
                    }
                    else
                    {
                        if (!_transiMode)
                            transform.position = objective;
                    }
                }
            }

            public void FreezePosition()
            {
                _frozen = true;
            }

            public void Unfreeze()
            {
                _frozen = false;
            }

            public void LookAtTransform(int duration, int transiDuration, Transform targetTransform, Action callback)
            {
                var tempTransform = new GameObject().transform;

                tempTransform.position = playerTransform.position;

                var currentDiff = _useDifferentTransform;
                var currentTran = _differentTransform;
                var currentDir = _position;
                _useDifferentTransform = true;
                _differentTransform = tempTransform;
                _transiMode = true;


                Lerper.ConstantLerp(tempTransform.position, targetTransform.position, transiDuration / 1000f,
                    (v) =>
                    {
                        tempTransform.position = v;
                    },
                    () =>
                    {
                        _ = GameTools.DelayedCall(duration, () =>
                        {
                            Lastlerp();
                        });
                    });

                void Lastlerp()
                {
                    var targetPos = currentDiff ? currentTran : ((Component)_player).transform;
                    currentDir = _position = _position.normalized * currentDir.magnitude;
                    Lerper.ConstantLerp(tempTransform.position, targetPos.position, transiDuration / 1000f,
                        (v) =>
                        {
                            tempTransform.position = v;
                        },
                        () =>
                        {
                            _useDifferentTransform = currentDiff;
                            _differentTransform = currentTran;
                            _position = currentDir;
                            _transiMode = false;
                            Destroy(tempTransform.gameObject);
                            callback.Invoke();
                        });
                }
            }

            private void ManageDelay()
            {
                if (_delayFrames < 0)
                    _delayFrames = 0;
                if(_delayFrames == 0)
                {
                    _delaySavedFrames.Clear();
                    return;
                }
                _delaySavedFrames.Add(new KeyValuePair<Vector3, float>(playerTransform.position,Time.timeSinceLevelLoad));
                while (_delaySavedFrames.Count > _delayFrames)
                    _delaySavedFrames.RemoveAt(0);
            }

            private void GetInputs()
            {
                if (_scrollToZoom && !_blocked && InputManager.mouseScrollDelta.y != 0)
                    _zoom = (_zoom - InputManager.mouseScrollDelta.y * _scrollSensitivity * 10 * Time.smoothDeltaTime).Clamp(-_distance + 0.1f, _distance * 2);

                float x = _blocked ? 0 : InputManager.GetAxis(_horizontalMovement);
                float y = _blocked ? 0 : InputManager.GetAxis(_verticalMovement) * (_camInversed ? 1 : -1);

                if(!_blocked && x == 0 && y == 0)
                {
                    x = _simulatedInput.x;
                    y = _simulatedInput.y;
                }
                else if (_simulatedInput != Vector2.zero)
                {
                    EventHandler handler = BreakSimulatedInput;
                    handler?.Invoke(this, new EventArgs());
                }

                if (y != 0)
                {
                    var t = Vector3.Slerp(_position, _position.magnitude * Vector3.up * Mathf.Sign(y), y.Abs() * 0.1f * _inputSensitivity * Time.deltaTime);
                    if (t.Distance(Vector3.up * Math.Sign(y)) > _topBottomLimit)
                        _position = t;
                }
                if (x != 0)
                    _position = _position.Rotate(Vector3.up, x * _inputSensitivity * Time.smoothDeltaTime * 5);

                var posNorm = _position.normalized;
                if (_currentOffset == Vector2.zero)
                    _offSetTurned = Vector2.zero;
                else
                    _offSetTurned = posNorm.Left() * _currentOffset.x + posNorm.Up() * _currentOffset.y;
                _cheatOffsetTurned = Vector3.up * _currentCheatOffset;

                if (_inverseCamButton && InputManager.GetButtonDown(_inverseCamButtonName))
                    _camInversed = !_camInversed;
            }

            private void CamTriangulation()
            {
                TPSNode[] nodes = (TPSNode[])FindObjectsOfType(typeof(TPSNode));
                if (nodes.Length < 1)
                {
                    var res = new TPSNodeContent
                    {
                        offset = Vector2.zero,
                        fakeMiddle = 0f,
                        distance = _defaultDistance
                    };
                    MoveToNode(res);
                    return;
                }
                Vector3[] positions = new Vector3[nodes.Length];
                for (int i = 0; i < positions.Length; i++)
                {
                    positions[i] = nodes[i].transform.position;
                }
                var temp = WeedOutBadNodes(nodes, positions, playerPosition);
                TPSNode[] GoodNodes = temp.Key;
                positions = temp.Value;
                if (GoodNodes.Length < 1) // no node in range
                {
                    var res = new TPSNodeContent
                    {
                        offset = Vector2.zero,
                        fakeMiddle = 0f,
                        distance = _defaultDistance
                    };
                    MoveToNode(res);
                    return;
                }
                else if (GoodNodes.Length < 2) // create a fake node at the edge of the range to use for smoothing out of the node's area of effect when only a single node is effective
                {
                    TPSNodeContent res = EdgeOut(GoodNodes[0], positions[0]);
                    MoveToNode(res);
                    return;
                }
                var r = (NodesDistances(positions, playerPosition));
                r = NormalizeToMax(r);
                r = GetSumTo1(r);
                TPSNodeContent[] val = new TPSNodeContent[GoodNodes.Length];
                for (int i = 0; i < GoodNodes.Length; i++)
                {
                    val[i] = EdgeOut(GoodNodes[i], positions[i]);
                }
                var result = AverageNodes(val, r, playerPosition, positions);
                MoveToNode(result);
            }

            private void MoveToNode(TPSNodeContent node)
            {
                _currentOffset = node.offset;
                _currentCheatOffset = node.fakeMiddle;
                _distance = node.distance;
            }

            private TPSNodeContent AverageNodes(TPSNodeContent[] TPSNodes, float[] distancesTo1, Vector3 playerPos, Vector3[] TPSNodesPosition)
            {
                TPSNodeContent result = new TPSNodeContent();
                result.Default();
                Vector2[] allTPSNodesOffset = new Vector2[TPSNodes.Length];
                float[] allTPSNodesCheatOffset = new float[TPSNodes.Length];
                for (int i = 0; i < TPSNodes.Length; i++) // calculate all positions
                {
                    allTPSNodesOffset[i] = TPSNodes[i].offset;
                    allTPSNodesCheatOffset[i] = TPSNodes[i].fakeMiddle;
                }
                var total = distancesTo1[0];       // had a weird bug I couldn't wrap my head around with basic multiplication of weight & addition that inversed
                var currentOffset = allTPSNodesOffset[0];   // the weights I couldn't wrap my head around, so I decided to do the same than the quaternions in the end
                var currentCheatOffset = allTPSNodesCheatOffset[0];
                for (int i = 0; i < allTPSNodesOffset.Length - 1; i++)
                {
                    var weight = CalculateLerpWeight(total, distancesTo1[i + 1]);
                    total += weight;
                    currentOffset = currentOffset == allTPSNodesOffset[i + 1] ? allTPSNodesOffset[i + 1] : Vector2.Lerp(currentOffset, allTPSNodesOffset[i + 1], weight);
                }
                total = distancesTo1[0];
                for (int i = 0; i < allTPSNodesCheatOffset.Length - 1; i++)
                {
                    var weight = CalculateLerpWeight(total, distancesTo1[i + 1]);
                    total += weight;
                    currentCheatOffset = currentCheatOffset == allTPSNodesCheatOffset[i + 1] ? allTPSNodesCheatOffset[i + 1] : Mathf.Lerp(currentCheatOffset, allTPSNodesCheatOffset[i + 1], weight);
                }
                result.offset = currentOffset;
                result.fakeMiddle = _currentCheatOffset;
                return result;
            }

            private float CalculateLerpWeight(float a, float b)
            {
                var result = Mathf.Min(a, b) / Mathf.Max(a, b) / 2;
                result = a == Mathf.Min(a, b) ? result : 1 - result;
                return result;
            }

            private KeyValuePair<TPSNode[], Vector3[]> WeedOutBadNodes(TPSNode[] nodes, Vector3[] positions, Vector3 playerpos)
            {
                TPSNode[] result = nodes;
                float[] distances = new float[nodes.Length];
                for (int i = nodes.Length - 1; i >= 0; i--)
                {
                    if (!nodes[i].enabled)
                    {
                        distances = distances.RemoveAt(i);
                        positions = positions.RemoveAt(i);
                        result = result.RemoveAt(i);
                    }
                }
                for (int i = result.Length - 1; i >= 0; i--) // weed out by range
                {
                    distances[i] = Vector3.Distance(positions[i], playerpos);
                    if (((distances[i] >= result[i].Content.range) && !(result[i].IsRectangular)) || (result[i].IsRectangular && !GameTools.IsInRectangle(result[i], playerpos)))
                    {
#if (UNITY_EDITOR)
                        result[i].Visible = false;
#endif
                        distances = distances.RemoveAt(i);
                        positions = positions.RemoveAt(i);
                        result = result.RemoveAt(i);
                    }
                }
                List<int> toDelete = new List<int> { };
                for (int i = result.Length - 1; i >= 0; i--)
                {
                    if (toDelete.IndexOf(i) != -1)
                    {
                        result = result.RemoveAt(i);
                        positions = positions.RemoveAt(i);
                    }
                }
                return new KeyValuePair<TPSNode[], Vector3[]>(result, positions);
            }

            private TPSNodeContent EdgeOut(TPSNode node, Vector3 pos)
            {
                TPSNodeContent result = Extensions.Copy(node.Content);
                float distPerc;
                if (!node.IsRectangular)
                {
                    if (node.Content.range < 0)
                    {
                        Debug.LogWarning("Le range ne doit pas �tre < 0 sur une node circulaire");
                    }
                    if (node.UseSafeZone)
                    {
                        if (Vector3.Distance(playerPosition, pos) <= node.SafeZoneSize)
                        {
                            distPerc = 1;
                        }
                        else
                        {
                            distPerc = (1 / (node.SafeZoneSize / node.Content.range)) - (Vector3.Distance(playerPosition, pos) / node.Content.range / (node.SafeZoneSize / node.Content.range));
                            distPerc = Mathf.Clamp01(distPerc * (1f + _nodePercSmoothing) - _nodePercSmoothing);
                            if (node.SafeZoneSize < 0)
                            {
                                Debug.LogError("Si elle est activ�e, la safe-zone ne doit pas �tre < 0");
                            }
                        }
                    }
                    else
                    {
                        distPerc = 1 - (Vector3.Distance(playerPosition, pos) / (node.Content.range));
                        distPerc = Mathf.Clamp01(distPerc * (1f + _nodePercSmoothing) - _nodePercSmoothing);
                    }
                }
                else
                {
                    var t1 = pos;
                    var t2 = pos;
                    if (GameTools.IsInRectangle(t2, node.SafeDimensions, playerPosition))
                    {
                        distPerc = 1;
                    }
                    else
                    {
                        var pos1 = GameTools.ClosestOnCube(playerPosition, t1, node.Dimensions, BoxCollider, !Application.isPlaying && _settings.ShowRectangularDebug);
                        var pos2 = GameTools.ClosestOnCube(playerPosition, t2, node.SafeDimensions, BoxCollider, Application.isPlaying && _settings.ShowRectangularDebug);
                        if (!Application.isPlaying)
                        {
                            if (_settings.ShowRectangularDebug)
                            {
                                EditorTools.DrawLineInEditor(pos1, playerPosition, Color.red);
                                EditorTools.DrawLineInEditor(pos2, playerPosition, Color.red);
                            }
                        }
                        var dist1 = Vector3.Distance(pos1, playerPosition);
                        var dist2 = Vector3.Distance(pos2, playerPosition);
                        distPerc = dist1 / (dist2 + dist1);
                    }
                }
                result.offset = node.Content.offset;
                result.fakeMiddle = node.Content.fakeMiddle;
                var res = new TPSNodeContent
                {
                    offset = Vector2.Lerp(_currentOffset, result.offset, distPerc),
                    fakeMiddle = Mathf.Lerp(_currentCheatOffset, result.fakeMiddle, distPerc),
                    distance = Mathf.Lerp(_defaultDistance, result.distance, distPerc)
                };
                return res;
            }

            private float[] GetSumTo1(float[] normalizedDistances)
            {
                float[] result = new float[normalizedDistances.Length];
                var r = normalizedDistances.Sum();
                for (int i = 0; i < result.Length; i++)
                {
                    result[i] = normalizedDistances[i] / r;
                }
                return result;
            }
            private float[] NormalizeToMax(float[] distances)
            {
                float[] result = new float[distances.Length];
                var r = distances.Max();
                for (int i = 0; i < distances.Length; i++)
                {
                    result[i] = distances[i] / r;
                }
                return result;
            }
            private float[] NodesDistances(Vector3[] nodes, Vector3 playerPos)
            {
                float[] result = new float[nodes.Length];
                for (int i = 0; i < nodes.Length; i++)
                {
                    result[i] = Vector3.Distance(nodes[i], playerPos);
                }
                return result;
            }
        }
    }
}