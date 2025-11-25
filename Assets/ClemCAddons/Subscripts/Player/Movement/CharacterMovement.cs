using UnityEngine;
using Luminosity.IO;
using ClemCAddons.Utilities;
using System.Diagnostics;
using ClemCAddons.CameraAndNodes;
using Debug = UnityEngine.Debug;
using UnityEngine.Serialization;

namespace ClemCAddons
{
    namespace Player
    {
        public class CharacterMovement : MonoBehaviour
        {

            #region Fields
            [Header("Update")]
            [SerializeField] private UpdateMode _updateMode;
            [SerializeField] private UpdateMode _impulseMode;

            [Header("Collision")]
            [Tooltip("The layer(s) to be considered when looking for ground")]
            [SerializeField] private LayerMask _collisionLayer = 8;

            [Header("Camera")]
            [Tooltip("Is using the TPS camera script, or the other node-based camera scirpt?")]
            [SerializeField] private bool _isUsingTPS = true;

            [Header("Gliding")]
            [Tooltip("What gravity is the player under when gliding?")]
            [SerializeField] private float _glidingGravity = 0.5f;
            [Tooltip("What are the limits to the player's fall speed?")]
            [SerializeField] private float _maxGlidingFallSpeed = 0.5f;
            [Tooltip("If over max fall speed, how fast does the player get to its max speed")]
            [SerializeField, FormerlySerializedAs("_glidingDeceleration")] private float _glidingYDeceleration = 2;
            [Tooltip("The speed the player rotates at while gliding")]
            [SerializeField] [LabelOverride("Gliding Rotation Speed")] private float _airControl = 2f;
            [Tooltip("The max speed the player can attain while gliding")]
            [SerializeField] [LabelOverride("Air Control Max Speed")] private float _airMaxMagnitude = 2f;
            [Tooltip("The acceleration of the player while gliding")]
            [SerializeField] [LabelOverride("Air Control Acceleration")] private float _airMagnitudeAcceleration = 0.3f;
            [Tooltip("The deceleration of the player while gliding")]
            [SerializeField] [LabelOverride("Air Control Deceleration")] private float _airMagnitudeDeceleration = 0.3f;


            [Header("Walking")]
            [Tooltip("The max speed the player can walk at")]
            [SerializeField] private float _maxSpeed = 10;

            [Tooltip("By how much should the player turn to match the ground's shape?")]
            [SerializeField] private float _maxGroundAdaptation = 10;
            [Tooltip("How fast does the player do this?")]
            [SerializeField] private float _groundAdaptationSpeed = 10;

            [Header("Jumping & Gravity")]
            [Tooltip("How fast and high is the player's jump?")]
            [SerializeField] private float _jumpStrength = 5;
            [SerializeField] private JumpInputs _jumpInputMode = JumpInputs.Both;

            [Tooltip("How fast is the player allowed to fall?")]
            [SerializeField] private float _maxFallSpeed = 10;

            [Tooltip("Any higher fall height doesn't gives a bonus to a bounce")]
            [SerializeField] private float _maxBounceHeight = 10;
            [Tooltip("The height-to-bounce ratio, affects how much height boosts your bounces")]
            [SerializeField] private float _bounceFactor = 0.1f;
            [Tooltip("Multiplied to the player inputs, affects the feeling of the player's walking, serving as both acceleration and deceleration")]
            [SerializeField] private float _inputMultiplier = 10f;

            [Header("Jump Buffering")]
            [Tooltip("How long after the player left the ground can the character still jump?")]
            [SerializeField, LabelOverride("Coyote time (s)")] private float _postJumpBuffering = 0.2f;
            [Tooltip("How long before the player reaches the ground can the character prepare to jump?")]
            [SerializeField, LabelOverride("Jump Buffering (s)")] private float _preJumpBuffering = 0.2f;

            [Header("Reactive Bouncing")]
            [Tooltip("How high should the player need to fall from to bounce on any surface")]
            [SerializeField] private float _fallBounceThreshold = 4;
            [Tooltip("When falling, won't be able to bounce more than that")]
            [SerializeField] private int _fallMaxBounce = 10;
            [Tooltip("How does fall height translate into bouncing")]
            [SerializeField] private float _fallBounceRatio = 0.5f;
            [Tooltip("How close to vertical does a surface need to be to be considered vertical?")]
            [SerializeField] private float _wallBouncePrecision = 0.1f;
            [Tooltip("How straight must the player hit the wall? (0 to 1)")]
            [SerializeField] private float _wallBounceThreshold = 0.7f;
            [Tooltip("How strongly should the player bounce?")]
            [SerializeField] private float _bounceStrength = 2;
            [Tooltip("Should an object bounce on a GameObject containing those components, it will be ignored")]
            [SerializeField] private UnityEngine.Object[] _bounceScriptExceptions;
            [Tooltip("Defines how long the movements are disabled after bouncing straight on a wall (in ms)")]
            [SerializeField] private int _wallDisableDuration = 500;
            [Tooltip("Defines how long the movements are disabled after bouncing along a wall (in ms)")]
            [SerializeField] private int _softWallDisableDuration = 250;

            private Vector3 _impulse = new Vector3();
            private Vector3 _immediateImpulse = new Vector3(); // clear after applied
            private float _permanentImpulse; // does not clear
            private Vector3 _direction = new Vector3();
            private Vector3 _localVelocity;
            private float groundDistance;
            private float _fallingHeight = 0f;
            private bool _doNotMove = false;
            private bool _ignoreInputs = false;
            private bool _untieRigidbody = false;
            private bool _wallSlide = false;

            private Collider _collider;
            private Rigidbody _rigidbody;
            private bool _firstTimeGround;
            private bool _isOnGround;
            private bool _isFalling = true;
            private bool _isGliding = false;
            private NodeBasedCamera _camera;
            private TPSCameraWithNodeSupport _tpsCamera;
            private float _jumpDelay = 0;
            private float _postJumpDelay = 0;
            private Vector3 _lastUnityPosition = new Vector3(); // bug in rigidbody causes velocity to not reset on strong slopes
            private bool _preJump;

            private Vector3 _previousDirection; // For use with gliding calculations
            private Vector3 _savedDirection;    // ^
            private float _magnitudeVelocity;   // ^

            private Vector3 _movementDirection;

            private ELevelType _currentLevelType = ELevelType.NONE;
            private bool _disableAll = false;

            private int _bounceCount = 0;

            private bool _jumping;

            #endregion Fields


            #region Properties
            private float DeltaTime
            {
                get
                {
                    switch (_updateMode)
                    {
                        case UpdateMode.LateUpdate:
                            return Time.deltaTime;
                        case UpdateMode.Update:
                            return Time.deltaTime;
                        case UpdateMode.FixedUpdate:
                            return Time.fixedDeltaTime;
                        default:
                            return Time.deltaTime;
                    }
                }
            }
            private float SmoothDeltaTime
            {
                get
                {
                    switch (_updateMode)
                    {
                        case UpdateMode.LateUpdate:
                            return Time.smoothDeltaTime;
                        case UpdateMode.Update:
                            return Time.smoothDeltaTime;
                        case UpdateMode.FixedUpdate:
                            return Time.fixedDeltaTime;
                        default:
                            return Time.smoothDeltaTime;
                    }
                }
            }

            public LayerMask CollisionLayer { get => _collisionLayer; set => _collisionLayer = value; }
            public Rigidbody Rigidbody
            {
                get
                {
                    if (_rigidbody == null)
                    {
                        _rigidbody = _rigidbody = GetComponent<Rigidbody>();
                    }
                    return _rigidbody;
                }
                set
                {
                    _rigidbody = value;
                }
            }
            public bool IsOnGround { get => _isOnGround; set => _isOnGround = value; }
            public bool IsFalling { get => _isFalling; set => _isFalling = value; }
            public bool IsGliding { get => _isGliding; set => _isGliding = value; }

            public float GroundDistance { get => groundDistance; set => groundDistance = value; }

            public bool IsWalking
            {
                get
                {
                    return (_direction.x.Abs() > 0 || _direction.z.Abs() > 0) && IsOnGround && !_doNotMove;
                }
            }

            public bool IsMoving
            {
                get
                {
                    return (_direction.x.Abs() > 0 || _direction.z.Abs() > 0) && !_doNotMove;
                }
            }

            public ELevelType CurrentLevelType
            {
                get
                {
                    return _currentLevelType;
                }
                set
                {
                    _currentLevelType = value;
                }
            }

            public bool Jumping { get => _jumping; set => _jumping = value; }
            public JumpInputs JumpInputMode { get => _jumpInputMode; set => _jumpInputMode = value; }
            public TPSCameraWithNodeSupport TpsCamera { get => _tpsCamera;  }
            #endregion Properties

            public enum JumpInputs
            {
                Both,
                Jump,
                AltJump
            }

            public enum UpdateMode
            {
                LateUpdate,
                Update,
                FixedUpdate
            }

            void Start()
            {
                _collider = GetComponent<Collider>();
                _rigidbody = GetComponent<Rigidbody>();
                if (_isUsingTPS)
                    _tpsCamera = FindObjectOfType<TPSCameraWithNodeSupport>();
                else
                    _camera = FindObjectOfType<NodeBasedCamera>();

            }

            void Update()
            {
                if (_updateMode == UpdateMode.Update)
                    LocalUpdate();
                if (_updateMode == UpdateMode.Update)
                    ApplyImpulse();
            }

            void LateUpdate()
            {
                if (_updateMode == UpdateMode.LateUpdate)
                    LocalUpdate();
                if (_updateMode == UpdateMode.LateUpdate)
                    ApplyImpulse();
            }

            void FixedUpdate()
            {
                if (_updateMode == UpdateMode.FixedUpdate)
                    LocalUpdate();
                if(_updateMode == UpdateMode.FixedUpdate)
                    ApplyImpulse();
            }

            void LocalUpdate()
            {
                if (_disableAll)
                    return;
                _jumping = false;
                UpdateInputs();
                UpdateMovementDirection();

                _localVelocity = new Vector3();

                // early assigning
                _isFalling = _rigidbody.velocity.y < 0 && (transform.position.y - _lastUnityPosition.y).Abs() > 0.01f;
                _lastUnityPosition = transform.position;
                // bug in rigidbody causes velocity to not reset on strong slopes       ^
                _fallingHeight = !_isFalling ? 0 : _fallingHeight;
                
                // Special conditions that cana affect local velocity
                GroundAndJump();
                Gliding();


                // late assigning
                _previousDirection = _direction;
                _jumpDelay = Mathf.Max(0, _jumpDelay - SmoothDeltaTime);

                // apply direction inputs to local velocity
                _localVelocity.x = _direction.x * _inputMultiplier;
                _localVelocity.z = _direction.z * _inputMultiplier;
                _localVelocity = _localVelocity.ClampXZTotal(_inputMultiplier); // ClampXZ total turns a square field of possible {X,Z} points into a circular one

                // camera direction
                Vector3 _aim = (_isUsingTPS ? _tpsCamera.transform : _camera.transform).forward;
                _aim.y = 0;
                if (_aim == new Vector3())
                {
                    _aim = transform.forward; // defaults to current player forward
                }

                // conditions that might ignore local velocity
                _localVelocity = _doNotMove ? new Vector3() : _localVelocity;
                _immediateImpulse = _doNotMove ? new Vector3() : _immediateImpulse;

                // if doesn't move, freeze position
                if ((_localVelocity + _immediateImpulse) == Vector3.zero && !_untieRigidbody && !_wallSlide)
                {
                    _rigidbody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
                }
                else
                {
                    _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
                }

                // rotate local velocity to match target direction
                Vector3 velocityFinal = _localVelocity.Remap(_aim);
                // our local velocity y is relative, not absolute
                velocityFinal.y += _rigidbody.velocity.y;
                
                // limit to max speed, in order:
                // within bounds
                // total within bounds
                // y to max fallspeed
                velocityFinal = velocityFinal.ClampXZKeepRatio(-_maxSpeed, _maxSpeed);
                velocityFinal = velocityFinal.ClampXZTotal(_maxSpeed);
                velocityFinal = velocityFinal.ClampY(-_maxFallSpeed, _maxFallSpeed);

                // if rigidbody is untied or currently wall-sliding
                if (!_untieRigidbody && !_wallSlide)
                {
                    if (_permanentImpulse == 0)
                    {
                        _rigidbody.velocity = (_immediateImpulse + velocityFinal);
                    }
                    else
                    {
                        var r = (_immediateImpulse + velocityFinal);
                        _rigidbody.velocity = r.SetY(Mathf.Lerp(r.y, _permanentImpulse, DeltaTime * 4));
                    }
                }

                // wall-slide logic, to make sliding down faster
                if(_wallSlide)
                {
                    if (_rigidbody.velocity.y > 2)
                        _rigidbody.velocity -= new Vector3(0, _rigidbody.velocity.y * 10f) * DeltaTime;
                    // RELATIVE FORCE - Velocity = constant speed without overwriting velocity
                    // so it doesn't fuck with other scripts using the rigidbody
                }

                _immediateImpulse = new Vector3();
            }

            public void SetWallSlide(bool wallSlide)
            {
                _wallSlide = wallSlide;
            }

            public void SetCanMove(bool canMove)
            {
                _doNotMove = !canMove;
            }
            public void SetIgnoreInput(bool ignoreInput)
            {
                _ignoreInputs = ignoreInput;
            }
            public void SetUntieRigidbody(bool untieRigidbody)
            {
                _untieRigidbody = untieRigidbody;
            }

            public void Teleport(Vector3 position)
            {
                transform.position = position;
            }

            public void Teleport(Vector3 position, Vector3 direction)
            {
                transform.position = position;
                _tpsCamera.Position = direction;
            }

            public void Teleport(Vector3 position, Quaternion direction)
            {
                transform.position = position;
                _tpsCamera.Position = direction * Vector3.forward; // might want to use up axis instead, not sure, didn't check
            }

            private void Gliding()
            {
                _isGliding = !_isOnGround && _direction.y > 0;
                if (!_isOnGround && _isFalling && _direction.y > 0)
                {
                    bool firstTime = _rigidbody.useGravity;
                    _rigidbody.useGravity = false;
                    if (Mathf.Abs(_rigidbody.velocity.y) > _maxGlidingFallSpeed)
                    {
                        _localVelocity.y += (_glidingYDeceleration - _glidingGravity) * DeltaTime;
                    }
                    else
                    {
                        _impulse.y -= _glidingGravity * DeltaTime;
                    }


                    _fallingHeight = 0;
                    _direction.y = 0; // consume direction.y, everything after that can be considered plane math, as Y is negligible
                    _previousDirection.y = 0;
                    bool headingTo0 = _magnitudeVelocity > _direction.magnitude && _magnitudeVelocity > 0;
                    if (_magnitudeVelocity != 0 || _direction.magnitude != 0) // air control => velocity
                    {
                        if (firstTime)
                            _magnitudeVelocity = (_direction.magnitude * _airMaxMagnitude).Min(_airMaxMagnitude); // directly set to target
                        else
                            _magnitudeVelocity =
                               ((_magnitudeVelocity +
                                   ((_direction.magnitude * _airMaxMagnitude).Min(_airMaxMagnitude) - _magnitudeVelocity).Sign()
                                   // scale speed in direction, serves as both scaling and max
                                   * DeltaTime
                                   * ((!headingTo0).ToInt() * _airMagnitudeAcceleration + headingTo0.ToInt() * _airMagnitudeDeceleration)
                               // one of either acceleration or deceleration is multiplied by 1, the other 0.
                               // It is the equivalent of a condition, without the additionnal overhead
                               ));
                        if (!headingTo0)
                            _magnitudeVelocity = _magnitudeVelocity.Min(_airMaxMagnitude);
                        else
                            _magnitudeVelocity = _magnitudeVelocity.Max(0);
                        // originally put a clamp to 0 in direction, but doesn't have any use as this value is never supposed to be negative
                    }

                    if (Vector3.Dot(_direction.normalized, _previousDirection.normalized) < 0.99)
                    // for we need that to change the rotation only, we don't care about magnitude
                    // small changes would result in an unstable slerp, as it would constantly overshoot the target
                    {
                        _direction = Vector3.Slerp(_previousDirection, _direction, DeltaTime * _airControl);
                        // air control => turning 
                    }
                    // Makes sure the direction magnitude matches the velocity
                    if (_direction.sqrMagnitude != 0)
                    {
                        _savedDirection = _direction.normalized;
                        _direction = _direction.NormalizeTo(_magnitudeVelocity);
                    }
                    else
                    {
                        // potential troulesome area around 0
                        _direction = _savedDirection.NormalizeTo(_magnitudeVelocity);
                    }
                }
                else
                {
                    _savedDirection = _direction.SetY(0);
                    _magnitudeVelocity = 0;
                    if (_ignoreInputs == false)
                        _rigidbody.useGravity = true;
                    if (!_isOnGround && _isFalling)
                    {
                        _fallingHeight -= _rigidbody.velocity.y * SmoothDeltaTime; // y velocity is negative as _isfalling and !_isOnGround
                    }
                }
            }

            private void GroundAndJump()
            {
                var slope = GameTools.FindSlope(transform.position, _movementDirection * _collider.bounds.extents.x, _collider.bounds.extents.y, 1, _collisionLayer);
                slope = (slope.Abs() <= _collider.bounds.extents.y).ToInt() * slope; // slope isn't considered if it's steeper than the player's half height,
                                                                                     // such as a wall
                groundDistance = GameTools.FindGround(transform.position, _collider.bounds.extents.y, 10, _collisionLayer, out RaycastHit hit);
                groundDistance = (groundDistance - slope.Abs() - 0.05f).Max(0); // removes slope
                _firstTimeGround = !_isOnGround && groundDistance == 0; // if is on ground is false, has a chance to be true if it becomes true.

                // ability to jump after leaving the ground for a time
                if (groundDistance == 0 && _isOnGround && _postJumpDelay <= 0)
                {
                    _postJumpDelay = _postJumpBuffering;
                }
                _isOnGround = groundDistance == 0 || _postJumpDelay > 0;
                if (groundDistance > 0 && _postJumpDelay > 0)
                {
                    _postJumpDelay -= DeltaTime;
                }

                // ability to give the jump input sligthly before reaching the ground
                if (_firstTimeGround && _preJump)
                {
                    _preJump = false;
                    if (_jumpDelay <= 0 && _doNotMove == false)
                    {
                        _jumping = true;
                        AudioManager.Start2DSoundSingleCall("S_Saut", 100);

                        _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z); // set rigidbody velocity y to 0
                        _impulse += new Vector3(0, _jumpStrength);
                        _jumpDelay += 0.1f;
                    }
                }
                if (!_isOnGround && _direction.y > 0 && groundDistance <= _rigidbody.velocity.y * -1 * _preJumpBuffering && _isFalling)
                {
                    _preJump = true;
                }

                // jump && ground adaptation, if there is some
                if (_isOnGround)
                {
                    _wallSlide = false;

                    // ground adaptation
                    if(_maxGroundAdaptation != 0)
                    {
                        Vector3 r = hit.normal.ToQuaternion(transform.rotation).eulerAngles.SetY(0);
                        if (r.x.MinusAngle(0, true).Abs() > _maxGroundAdaptation)
                        {
                            r.x = transform.Find("Base").eulerAngles.x;
                        }
                        if (r.z.MinusAngle(0, true).Abs() > _maxGroundAdaptation)
                        {
                            r.z = transform.Find("Base").eulerAngles.z;
                        }
                        transform.Find("Base").rotation = Quaternion.Lerp(transform.Find("Base").rotation, r.ToQuaternion(), SmoothDeltaTime * _groundAdaptationSpeed);
                    }

                    // jump
                    if (_direction.y > 0 && _jumpDelay <= 0 && _doNotMove == false)
                    {
                        _jumping = true;
                        AudioManager.Start2DSoundSingleCall("S_Saut", 100);

                        _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z); // set rigidbody velocity y to 0
                        _impulse += new Vector3(0, _jumpStrength);
                        _jumpDelay += 0.1f;
                    }
                }
            }

            //private bool _tempStoreOfValue = false;
            //private Stopwatch stopwatch = new Stopwatch();
            private void UpdateInputs()
            {
                if (!_ignoreInputs)
                {
                    _direction.x = InputManager.GetAxis("Horizontal");
                    _direction.z = InputManager.GetAxis("Vertical");
                    switch (_jumpInputMode)
                    {
                        case JumpInputs.Both:
                            _direction.y = (InputManager.GetButton("JumpAlt") || InputManager.GetButton("Jump")).ToInt();
                            break;
                        case JumpInputs.AltJump:
                            _direction.y = InputManager.GetButton("JumpAlt").ToInt();
                            break;
                        case JumpInputs.Jump:
                            _direction.y = InputManager.GetButton("Jump").ToInt();
                            break;
                    }
                }
                else
                {
                    _direction = Vector3.zero;
                }
                // ###### A UTILISER SI IL Y A BESOIN DE MESURER la dur�e d'arr�t complet ######
                //if(Mathf.Abs(_direction.x) == 1)
                //{
                //    _tempStoreOfValue = true;
                //} else
                //{
                //    if (_tempStoreOfValue)
                //    {
                //        if (!stopwatch.IsRunning)
                //        {
                //            stopwatch.Start();
                //        }
                //        if(_rigidbody.velocity.magnitude == 0)
                //        {
                //            stopwatch.Stop();
                //            UnityEngine.Debug.Log(stopwatch.ElapsedMilliseconds);
                //            UnityEngine.Debug.Log(stopwatch.ElapsedTicks);
                //            _tempStoreOfValue = false;
                //            stopwatch.Reset();
                //        }
                //    }
                //}
            }

            void OnCollisionEnter(Collision collision)
            {
                _disableAll = false;
                if (_preJump)
                    return;
                foreach(Component component in collision.gameObject.GetComponentsInChildren<Component>())
                {
                    foreach (Object exception in _bounceScriptExceptions)
                    {
                        if (component.name == exception.name)
                        {
                            return;
                        }
                    }
                }
                if (_fallingHeight > _fallBounceThreshold)
                {
                    //Vibration Forces
                    float vibrationForce = Mathf.InverseLerp(_fallBounceThreshold, 20, _fallingHeight); //The Highest the fall is the more vibration there will be 

                    vibrationForce = vibrationForce / 1.5f;
                    float vibrationTimer = 0.5f * vibrationForce;

                    BabblesVibration.CustomVibration(vibrationTimer, vibrationForce); //Applying the Vibration

                    AudioManager.Start2DSound("S_Aterissage");


                    _immediateImpulse += collision.GetContact(0).normal * (_bounceStrength * (_fallingHeight - _fallBounceThreshold) * _fallBounceRatio).Min(_fallMaxBounce);
                    _fallingHeight = 0;
                }
                else if (collision.GetContact(0).normal.y.Abs() < _wallBouncePrecision && !_isOnGround)
                {
                    if (collision.collider.TryGetComponent<NoAcceleration>(out _))
                        return;
                    if(Vector3.Dot(collision.relativeVelocity.SetY(0).normalized, collision.GetContact(0).normal) > _wallBounceThreshold)
                    {
                        _bounceCount++;

                        _rigidbody.velocity += collision.GetContact(0).normal * _bounceStrength;
                        _fallingHeight = 0;
                        _disableAll = true;
                        _ = GameTools.DelayedCall(_wallDisableDuration, EnableAll);
                        var r = GetComponentInChildren<FollowVelocity>();
                        if (r != null)
                        {
                            r.enabled = false;
                        }
                        if (_bounceCount == 4 && !UIManager.Instance.UIController.DialogueManager.IsInDialog)
                        {
                            UIManager.Instance.UIController.StartDialogue("Wall_Touched", false);
                        }
                    }
                    else
                    {
                        _bounceCount = 0;
                        var target1 = (-collision.relativeVelocity.SetY(0).normalized).Reflect(collision.GetContact(0).normal);
                        var target2 = (-collision.relativeVelocity.SetY(0).normalized).Reflect(collision.GetContact(0).normal).Reflect(collision.GetContact(0).normal);
                        var targetBlend = Vector3.Lerp(target1, target2, 0.25f);
                        Debug.DrawLine(_rigidbody.position + collision.relativeVelocity.SetY(0).normalized, _rigidbody.position, Color.red, 5);
                        Debug.DrawLine(_rigidbody.position, _rigidbody.position + targetBlend, Color.green, 5);
                        _rigidbody.velocity += targetBlend  * collision.relativeVelocity.magnitude;
                        _fallingHeight = 0;
                        _disableAll = true;
                        _ = GameTools.DelayedCall(_softWallDisableDuration, EnableAll);
                    }
                }
                else
                {
                    _bounceCount = 0;
                }
            }

            public void EnableAll()
            {
                _disableAll = false;
                var r = GetComponentInChildren<FollowVelocity>();
                if (r != null)
                {
                    r.enabled = true;
                }
            }

            public void DisableAll()
            {
                _disableAll = true;
            }

            private void UpdateMovementDirection() // to follow velocity
            {
                if (_rigidbody.velocity.normalized != Vector3.zero && _rigidbody.velocity.normalized.GetMaxXZ(true) > 0.1f)
                {
                    _movementDirection = _rigidbody.velocity.normalized;
                }
            }

            private void ApplyImpulse()
            {
                _rigidbody.AddForce(_impulse, ForceMode.Impulse);
                _impulse = new Vector3();
            }

            public void Bounce(float value, bool autoHeight = false)
            {
                if (autoHeight)
                {
                    _impulse.y += Mathf.Min(_maxBounceHeight, value + _fallingHeight * _bounceFactor);
                    _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z);
                }
                else
                {
                    _impulse.y += Mathf.Min(_maxBounceHeight, value);
                    _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z);
                }
            }

            public void Bounce(float value, float height)
            {
                _impulse.y += Mathf.Min(_maxBounceHeight, value + height * _bounceFactor);
                _rigidbody.velocity = new Vector3(_rigidbody.velocity.x, 0, _rigidbody.velocity.z);
            }


            public void AddImpulse(Vector3 value)
            {
                _immediateImpulse += value;
            }

            public void Push(float value)
            {
                _permanentImpulse = value;
            }
        }
    }
}