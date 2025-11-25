using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ClemCAddons
{
    namespace Utilities
    {
        public class GameobjectMirror : MonoBehaviour
        {
            [SerializeField] private GameObject _toMirror;
            [SerializeField] private bool _MirrorX = true;
            [SerializeField] private bool _MirrorY = true;
            [SerializeField] private bool _MirrorZ = true;
            [SerializeField] private Vector3 _Offset = new Vector3();

            void Update()
            {
                if (_toMirror != null)
                {
                    transform.rotation = (new Vector3(
                        _MirrorX ? _toMirror.transform.localEulerAngles.x : 0,
                        _MirrorY ? _toMirror.transform.localEulerAngles.y : 0,
                        _MirrorZ ? _toMirror.transform.localEulerAngles.z : 0
                        )+_Offset).ToQuaternion();
                }
            }
        }
    }
}
