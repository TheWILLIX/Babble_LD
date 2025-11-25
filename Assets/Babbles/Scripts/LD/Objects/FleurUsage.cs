using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FleurUsage : MonoBehaviour
{
    private float _timeStamp = 0;
    [SerializeField] private float _timerAutoDestruction = 5f;
    private void OnTriggerEnter(Collider collider)
    {
        Debug.Log("collider found");
        if (collider.tag == "MurAlgue")
        {
            Debug.Log("AlgueWall found");

            collider.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        _timeStamp += Time.smoothDeltaTime;

        if(_timeStamp >= _timerAutoDestruction)
        {
            Destroy(this.gameObject);
        }
    }
}
