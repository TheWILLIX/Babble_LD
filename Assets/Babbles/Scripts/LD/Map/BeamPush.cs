using ClemCAddons.Player;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;
using ClemCAddons.Utilities;
using System.Linq;
using Luminosity.IO;
using UnityEditor;
using UnityEngine.Serialization;

[ExecuteInEditMode]
public class BeamPush : MonoBehaviour
{
    // static self reserved
    public static BeamPush CurrentReservation = null;
    public static Collider Target = null;

    // static boost reserved
    public static float BoostAmount = 0;

    // parameters, need to be accessible from editor constructor class
    public string _upDownAxis = "Vertical";
    public string _leftRightAxis = "Horizontal";
    public string _jumpButton = "Jump";
    public float _inputImpact = 0.3f;
    public float _driftSpeed = 1f;
    public float _driftToCenterSpeed = 1f;
    public float _exitStrength = 10f;
    public float _forwardForce = 1f;
    public float _centeringForce = 1f;
    public float _acceleration = 1f;
    public float _driftReactivity = 1f;
    public Transform _debugObject;
    public bool _showLines;
    public float _debugDistance = 100;
    public int _doubleKeyPressDelayInMilliseconds = 100;
    public float _exitSpeed = 10f;
    public float _centeringResistanceCoefficient = 0.001f;
    public float _camAdditionalDistance = 2;

    public bool _preventDirectionChange = false;

    [Tooltip("The object to search for"), FormerlySerializedAs("_reverseDirectionComponent")]
    public string _reverseDirectionException;

    // internal values
    private Vector2 _inputs = new Vector2();
    private Vector2 _driftCoordinates = new Vector2();
    private Vector2 _limits;

    private bool _officiallyIn;

    // static exit internal values
    private static float _xv;
    private static float _yv;
    private static bool _jv;
    private static Vector2 _rawInputs;
    private static bool _jumpInput;
    private static float _XleaveDirection;
    private static float _YleaveDirection;
    private static bool _leavePressed { get { return _XleaveDirection != 0 || _YleaveDirection != 0; } }
    private static int _direction;
    private static float defaultDistance = 0;

    private static CharacterMovement _targetSave;

    private static float _safeReverseLock = 0;
    private static BeamPush _safeReverseInstance;

    public static int ForcedDirection = 0;


    public void SetProperties(string _upDownAxis, string _leftRightAxis, float _inputImpact, float _driftSpeed,
        float _driftToCenterSpeed, float _exitStrength, float _forwardForce, float _centeringForce,
        float _acceleration, Transform _debugObject, bool _showLines, float _debugDistance,
        int _doubleKeyPressDelayInMilliseconds, float _springiness, string _jumpButton, float _exitSpeed,
        float _centeringStiffnessCoefficient, string _reverseDirectionObject, float _camAdditionalDistance, bool _preventDirectionChange)
    {
        this._upDownAxis = _upDownAxis;
        this._leftRightAxis = _leftRightAxis;
        this._inputImpact = _inputImpact;
        this._driftSpeed = _driftSpeed;
        this._driftToCenterSpeed = _driftToCenterSpeed;
        this._exitStrength = _exitStrength;
        this._forwardForce = _forwardForce;
        this._centeringForce = _centeringForce;
        this._acceleration = _acceleration;
        this._debugObject = _debugObject;
        this._showLines = _showLines;
        this._debugDistance = _debugDistance;
        this._driftReactivity = _springiness;
        this._doubleKeyPressDelayInMilliseconds = _doubleKeyPressDelayInMilliseconds;
        this._jumpButton = _jumpButton;
        this._exitSpeed = _exitSpeed;
        this._centeringResistanceCoefficient = _centeringStiffnessCoefficient;
        this._reverseDirectionException = _reverseDirectionObject;
        this._camAdditionalDistance = _camAdditionalDistance;
        this._preventDirectionChange = _preventDirectionChange;
    }

    void OnDrawGizmos()
    {
        if (_showLines)
            EditorTools.DrawLineInEditor(transform.position, transform.position + transform.forward * 5, Color.red);
    }

    void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player"))
        {

            _officiallyIn = true;

            BabblesVibration.CustomVibration(0.1f, 0.1f); //Vibration


            var firstEstimation = transform.GetLocalPosition(GetComponent<Collider>().ClosestPoint(other.transform.position)).ToVector2();
            var playerHalfSize = other.GetComponent<CapsuleCollider>().bounds.extents;
            var halfSize = transform.localScale.ToVector2() / 2;
            _limits = (halfSize - transform.GetLocal(playerHalfSize).ToVector2()).Abs();
            var outside = firstEstimation - _limits;
            if (outside.x > 0 || outside.y > 0)
            {
                var howMuch = firstEstimation / outside;
                if (howMuch.x > howMuch.y)
                {
                    _driftCoordinates = firstEstimation * _limits.x / firstEstimation.x;
                }
                else
                {
                    _driftCoordinates = firstEstimation * _limits.y / firstEstimation.y;
                }
            }
            else
            {
                _driftCoordinates = firstEstimation;
            }
            // if it is: adjust the estimation to fit in that
            // (the biggest difference in % is the reference reduction, the other is reduced in function to keep the direction)

            if (CurrentReservation == null && _safeReverseLock <= 0)
            {
             //   AudioManager.Start2DSound("S_CourantMarin");

                var r = FindObjectsOfType<ExceptionItem>().ToList().Find(t => t.Type == _reverseDirectionException);
                if(r != null)
                {
                    var dir = other.GetComponentInChildren<FollowVelocity>().transform.forward;
                    var t = Vector3.Dot(dir, transform.forward) > 0;
                    _direction = t ? 1 : -1;
                }
                else
                {
                    _direction = 1;
                }
                if(ForcedDirection != 0)
                {
                    _direction = ForcedDirection;
                }
                OverrideController(other);
                _XleaveDirection = 0;
                _YleaveDirection = 0;
                if (Camera.allCameras[0].TryGetComponent<ClemCAddons.CameraAndNodes.TPSCameraWithNodeSupport>(out var cam))
                {
                    if(defaultDistance == 0)
                        defaultDistance = cam._defaultDistance;
                    Lerper.ConstantLerp(ref cam._defaultDistance, defaultDistance + _camAdditionalDistance, 0.5f);
                }
            }

            CurrentReservation = this;
            Target = other;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {



            // BabblesVibration.CustomVibration(0.1f, 0.3f); //Vibration

            _officiallyIn = false;
            if (CurrentReservation == this && (_safeReverseLock <= 0 || _leavePressed))
            {
               // AudioManager.Instance?.StopUniqueSound(ESoundType.REPETITIVE2D, "S_CourantMarin");
                //AudioStart
                Vector3 inertia = Vector3.zero;
                if(!_leavePressed)
                {
                    inertia = other.GetComponent<Rigidbody>().velocity;
                }
                UnOverrideController(other, inertia);
                AudioManager.Start2DSoundSingleCall("S_SortieCourantMarin", 500);
                _XleaveDirection = _YleaveDirection = 0;
                CurrentReservation = null;

                if (Camera.allCameras[0].TryGetComponent<ClemCAddons.CameraAndNodes.TPSCameraWithNodeSupport>(out var cam))
                {
                    Lerper.ConstantLerp(ref cam._defaultDistance, defaultDistance, 0.5f);
                }
            }
        }
    }

    void LateUpdate()
    {
        if (CurrentReservation == this && Application.isPlaying)
        {
            MoveForwardInCoordinates(Target, transform.GetWorldPosition(_driftCoordinates));
            CalculateDrift(Target);
            if (_debugObject != null)
                _debugObject.position = transform.GetWorldPosition(_driftCoordinates) + transform.forward * _debugDistance;
        }
    }

    private void OverrideController(Collider target)
    {
        _targetSave = null;
        target.GetComponent<Rigidbody>().useGravity = false;
        if (target.TryGetComponent<CharacterMovement>(out var t))
        {
            t.SetUntieRigidbody(true);
            t.SetIgnoreInput(true);
        }
        target.GetComponent<Rigidbody>().velocity = Vector3.zero;
    }

    private void UnOverrideController(Collider target, Vector3 inertia)
    {
        var rigidbody = target.GetComponent<Rigidbody>();
        rigidbody.useGravity = true;
        rigidbody.velocity = transform.GetWorld(new Vector2(_XleaveDirection, _YleaveDirection)).normalized * _exitSpeed + inertia;
        if (target.TryGetComponent<CharacterMovement>(out var t))
        {
            _targetSave = t;
            _ = GameTools.DelayedCall(100, UnblockInputs);
        }
    }

    public void UnblockInputs()
    {
        if (_targetSave != null && _targetSave.TryGetComponent<CharacterMovement>(out var t))
        {
            t.SetUntieRigidbody(false);
            t.SetIgnoreInput(false);
        }
    }

    private void MoveForwardInCoordinates(Collider target, Vector3 targetPosition)
    {
        Vector3 playerPositionFlat = transform.GetWorldPosition(transform.GetLocalPosition(target.transform.position).SetZ(0));

        Vector2 playerPosition2D = transform.GetLocalPosition(target.transform.position).SetZ(0);

        var centerObjective = (playerPositionFlat.Direction(targetPosition) * _centeringForce * (playerPositionFlat.Distance(targetPosition) / 10 * _driftReactivity))
            / _acceleration;

        var distanceCoeff = (-playerPositionFlat.Distance(targetPosition)) + _limits.magnitude * 2;
        distanceCoeff = distanceCoeff.Max(0);

        centerObjective = centerObjective -
            (centerObjective * distanceCoeff * _centeringResistanceCoefficient).ClampAround0(centerObjective.Abs());

        var forwardObjective = transform.forward * BoostAmount.IfZero(1) * _forwardForce * _direction;

        var vel = target.GetComponent<Rigidbody>().velocity;
        vel = vel + (vel.Direction(forwardObjective) * _acceleration + centerObjective * 0.05f) * Time.deltaTime;


        if (playerPosition2D.CircularClamp(_limits * 0.9f) != playerPosition2D && !_leavePressed)
        {
            target.GetComponent<Rigidbody>().velocity = Vector3.Lerp(vel, transform.position - playerPositionFlat + forwardObjective, Time.deltaTime * 5);

        }
        else if (_leavePressed)
        {
            target.GetComponent<Rigidbody>().velocity = Vector3.Lerp(target.GetComponent<Rigidbody>().velocity, transform.position.Direction(targetPosition) * _exitStrength, Time.deltaTime * 5);
        }
        else
        {
            target.GetComponent<Rigidbody>().velocity = vel;
        }

        //minus drag (square of velocity * drag coefficient, at most velocity itself
    }

    private void CalculateDrift(Collider target)
    {
        TakeInputs();
        if (_leavePressed)
            _driftCoordinates = new Vector2(_XleaveDirection, _YleaveDirection);
        else if (!_driftCoordinates.ApproximatelyEqual(Vector2.zero, 0.01f) || _inputs.sqrMagnitude > 0)
        {
            var t = (_driftCoordinates + _inputs * _driftSpeed * Time.deltaTime) - _driftCoordinates;
            var r = (_driftCoordinates + _driftCoordinates.Direction(Vector2.zero) * _driftToCenterSpeed * Time.deltaTime).ClampTo0InDirection(_driftCoordinates.Direction(Vector2.zero)) - _driftCoordinates;
            var drift = t + r;
            drift = (drift * 1.2f - drift).ClampTo0InDirection(drift.normalized);
            _driftCoordinates = Vector3.Lerp(_driftCoordinates, _driftCoordinates + drift, Time.deltaTime * 10).CircularClamp(_limits * 0.75f);
        }
    }

    private void TakeInputs()
    {
        //_inputs.x = InputManager.GetAxis(_leftRightAxis);
        //_inputs.y = InputManager.GetAxis(_upDownAxis);

        //_rawInputs = new Vector2(InputManager.GetAxisRaw(_leftRightAxis), InputManager.GetAxisRaw(_upDownAxis));


        _jumpInput = InputManager.GetButtonDown(_jumpButton);

        if(_safeReverseLock > 0)
        {
            _safeReverseLock -= Time.deltaTime;
            if(_safeReverseLock <= 0 && (CurrentReservation == _safeReverseInstance || !GetComponent<Collider>().bounds.Intersects(Target.bounds)))
            {
                OnTriggerExit(Target);
            }
        }
        Vector3 direction = new Vector2(InputManager.GetAxis(_leftRightAxis), InputManager.GetAxis(_upDownAxis));
        if (direction != Vector3.zero && !_preventDirectionChange)
        {
            var r = FindObjectsOfType<ExceptionItem>().ToList().Find(t => t.Type == _reverseDirectionException);
            if (r != null && _officiallyIn)
            {
                direction = direction.Remap(Camera.allCameras[0].transform.forward);
                var dot = Vector3.Dot(direction, transform.forward);
                int pastDir = _direction;
                if (dot < -0.2f)
                    _direction = -1;
                if (dot > 0.2f)
                    _direction = 1;
                if (pastDir != _direction)
                {
                    _safeReverseLock = 2;
                    _safeReverseInstance = this;
                }
            }
        }

        if (_preventDirectionChange) // no forced exit
            return;

        if (_jumpInput)
        {
            if (_rawInputs.x != 0 || _rawInputs.y != 0)
            {
                _XleaveDirection = _rawInputs.x * _limits.x * 4;
                _YleaveDirection = _rawInputs.y * _limits.y * 4;
            }
            else
            {
                _YleaveDirection = _limits.y * 4;
            }

            AudioManager.Start2DSound("S_SortieCourantMarin");
            BabblesVibration.CustomVibration(0.25f, 0.4f);
        }

        //    if ((_rawInputs.x != 0).OnceIfTrueGate("EntryKeyX".GetHashCode())) // only if true for the first time, so needs to release input
        //    {
        //        if (_rawInputs.x.IfNZero(1, true) != _xv) // step 1: opposite direction or 0, can happen at any stage (default, step 1 reset, after step2)
        //        {
        //            _xv = _rawInputs.x.IfNZero(1, true);
        //            ClemCAddons.Utilities.Timer.StartTimer("ResetKeyX".GetHashCode(), _doubleKeyPressDelayInMilliseconds, ResetX, false);
        //        }
        //        else // step 2: same direction, can only happen after step 1
        //        {
        //            _XleaveDirection = _xv.IfNZero(_limits.x * 4, true);
        //            _xv = 0;
        //            ClemCAddons.Utilities.Timer.EndTimer("ResetKeyX".GetHashCode());
        //            _safeReverseLock = 0;
        //        }
        //    }

        //    if ((_rawInputs.y != 0).OnceIfTrueGate("EntryKeyY".GetHashCode())) // only if true for the first time, so needs to release input
        //    {
        //        if (_rawInputs.y.IfNZero(1, true) != _yv) // step 1: opposite direction or 0, can happen at any stage (default, step 1 reset, after step2)
        //        {
        //            _yv = _rawInputs.y.IfNZero(1, true);
        //            ClemCAddons.Utilities.Timer.StartTimer("ResetKeyY".GetHashCode(), _doubleKeyPressDelayInMilliseconds, ResetY, false);
        //        }
        //        else // step 2: same direction, can only happen after step 1
        //        {

        //            _YleaveDirection = _yv.IfNZero(_limits.y * 4, true);
        //            _yv = 0;
        //            ClemCAddons.Utilities.Timer.EndTimer("ResetKeyY".GetHashCode());
        //            _safeReverseLock = 0;
        //        }
        //    }

        //    if (_jumpInput.OnceIfTrueGate("EntryKeyJ".GetHashCode())) // only if true for the first time, so needs to release input
        //    {
        //        if (!_jv) // step 1: opposite direction or 0, can happen at any stage (default, step 1 reset, after step2)
        //        {
        //            _jv = true;
        //            ClemCAddons.Utilities.Timer.StartTimer("ResetKeyJ".GetHashCode(), _doubleKeyPressDelayInMilliseconds, ResetJ, false);
        //        }
        //        else // step 2: same direction, can only happen after step 1
        //        {

        //            _jv = false;
        //            if (_rawInputs.x != 0 || _rawInputs.y != 0)
        //            {
        //                _XleaveDirection = _rawInputs.x * _limits.x * 4;
        //                _YleaveDirection = _rawInputs.y * _limits.y * 4;
        //            }
        //            else
        //            {
        //                _YleaveDirection = _limits.y * 4;
        //            }
        //            ClemCAddons.Utilities.Timer.EndTimer("ResetKeyJ".GetHashCode());
        //            _safeReverseLock = 0;
        //        }
        //    }
    }
    private void ResetX()
    {
        _xv = 0;
    }
    private void ResetY()
    {
        _yv = 0;
    }
    private void ResetJ()
    {
        _jv = false;
    }
}

#if(UNITY_EDITOR)
[CustomEditor(typeof(BeamPush))]
public class BeamPushInspector : Editor
{
    public override void OnInspectorGUI()
    {
        var sd = new SerializedObject(target as BeamPush);
        var beampush = target as BeamPush;
        GUILayout.Label("Inputs", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(sd.FindProperty("_upDownAxis"));
        EditorGUILayout.PropertyField(sd.FindProperty("_leftRightAxis"));
        EditorGUILayout.PropertyField(sd.FindProperty("_jumpButton"));
        EditorGUILayout.PropertyField(sd.FindProperty("_inputImpact"));
        EditorGUILayout.PropertyField(sd.FindProperty("_doubleKeyPressDelayInMilliseconds"));
        EditorGUI.indentLevel--;
        GUILayout.Label("Camera", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(sd.FindProperty("_camAdditionalDistance"));
        EditorGUI.indentLevel--;
        GUILayout.Label("Player", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(sd.FindProperty("_driftSpeed"));
        EditorGUILayout.PropertyField(sd.FindProperty("_driftToCenterSpeed"));
        EditorGUILayout.PropertyField(sd.FindProperty("_exitStrength"));
        EditorGUILayout.PropertyField(sd.FindProperty("_exitSpeed"));
        EditorGUI.indentLevel--;
        GUILayout.Label("Physics", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(sd.FindProperty("_acceleration"));
        EditorGUILayout.PropertyField(sd.FindProperty("_forwardForce"));
        EditorGUI.indentLevel--;
        GUILayout.Label("Advanced Physics", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
        EditorGUILayout.LabelField("Only modify these if you know what you're doing");
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(sd.FindProperty("_centeringForce"));
        EditorGUILayout.PropertyField(sd.FindProperty("_driftReactivity"));
        EditorGUILayout.PropertyField(sd.FindProperty("_centeringResistanceCoefficient"));
        EditorGUI.indentLevel--;
        GUILayout.Label("External Factors", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(sd.FindProperty("_reverseDirectionException"));
        EditorGUILayout.PropertyField(sd.FindProperty("_preventDirectionChange"));
        EditorGUI.indentLevel--;
        GUILayout.Label("Debug", new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold });
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(sd.FindProperty("_debugObject"));
        EditorGUILayout.PropertyField(sd.FindProperty("_showLines"));
        EditorGUILayout.PropertyField(sd.FindProperty("_debugDistance"));
        EditorGUI.indentLevel--;
        sd.ApplyModifiedProperties();
        if (GUILayout.Button("Update other BeamPush in scene to match"))
        {
            List<BeamPush> modified = new List<BeamPush>();
            foreach (var push in FindObjectsOfType<BeamPush>())
            {
                if (push != beampush)
                {
                    push.SetProperties(beampush._upDownAxis, beampush._leftRightAxis, beampush._inputImpact,
                        beampush._driftSpeed, beampush._driftToCenterSpeed, beampush._exitStrength,
                        beampush._forwardForce, beampush._centeringForce, beampush._acceleration,
                        beampush._debugObject, beampush._showLines, beampush._debugDistance,
                        beampush._doubleKeyPressDelayInMilliseconds, beampush._driftReactivity, beampush._jumpButton,
                        beampush._exitSpeed, beampush._centeringResistanceCoefficient, beampush._reverseDirectionException,
                        beampush._camAdditionalDistance, beampush._preventDirectionChange);
                    EditorUtility.SetDirty(push);
                }
            }
            Undo.RecordObjects(modified.ToArray(), "Updated other BeamPush in scene to match");
        }
    }
}
#endif