using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeamAccelerator : MonoBehaviour
{
    [SerializeField] private float _boostStrength;

    private static BeamAccelerator _acceleratorReservation;


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _acceleratorReservation = this;
            BeamPush.BoostAmount += _boostStrength;// so if you enter a new one while inside the previous, additions
        }
    }

    void OnTriggerExit(Collider other)
    {
        if(_acceleratorReservation == this)
        {
            BeamPush.BoostAmount = 0;
            // you never know when float can have a few imprecisions, so while this is technically unnecessary, could avoid always being a few epsilons from 0
            // such a meagre amount wouldn't matter in the short term, but in the long run, float imprecisions could pile up by the thousands, eventually reaching
            // a point where it could be felt, though the normal distribution would tend to 0 if the probabilities were equal in both directions, because it's
            // fractions, it's not really linear and I don't know. Better safe than sorry.
        }
        else
        {
            BeamPush.BoostAmount -= _boostStrength;
        }
    }
}
