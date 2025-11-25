using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;

[ExecuteInEditMode]
public class ExplosionShard : MonoBehaviour
{
    [SerializeField] private Transform explosionSource;
    [SerializeField] private float explosionStrength = 10;

    [Header("Geyser Unlock Config")] //Only one of the rock has to be setted up with this
    [SerializeField] private bool _unlockGeyserAfterExplosion = false;
    [SerializeField] private GameObject[] _geyserObjects = null;
    [SerializeField] private PushScript _pushScript = null;


    private bool sinking = false;
    private Vector3 position;
    private Vector3 offset = Vector3.zero;

    public Transform ExplosionSource { get => explosionSource; }

    void Update()
    {
        if (!Application.isPlaying)
        {
            Debug.DrawLine(transform.position, transform.position + explosionSource.position.Direction(transform.position) * 5, Color.red);
            return;
        }
        if(TryGetComponent<Rigidbody>(out var rigidbody) && rigidbody.velocity == Vector3.zero && rigidbody.angularVelocity == Vector3.zero)
        {
            Destroy(rigidbody);
            position = transform.position;
            sinking = true;
        }
        if (!sinking)
            return;
        offset += Vector3.down * Time.deltaTime;
        transform.position = position + offset;
        if (offset.y.Abs() > GetComponentInChildren<Renderer>().bounds.extents.y * 2)
            Destroy(gameObject);
    }

    public void Explode(Vector3 position)
    {
        gameObject.AddComponent<Rigidbody>().velocity = Vector3.Lerp(position, explosionSource.position, 0.5f).Direction(transform.position) * 10;
        _ = ClemCAddons.Utilities.GameTools.DelayedCall(3000, () => { GetComponent<Rigidbody>().mass *= 10; });
       
        //Only one of the rock has to be setted up with this
        if(_unlockGeyserAfterExplosion == true && _geyserObjects.Length >= 1)
        {
            foreach(GameObject geyserObjects in _geyserObjects )
            {
                geyserObjects.SetActive(true);
            }
            _pushScript.GeyserActivated = true;
            UIManager.Instance.UIController.DialogueManager.VariableStorage.SetValue("$rocherDestroyed", true);

        }
    }

    public void ExplodeAll(Vector3 position)
    {
        foreach(Transform t in transform.parent)
        {
            if(t.TryGetComponent<ExplosionShard>(out var shard))
                shard.Explode(position);
        }
    }
}
