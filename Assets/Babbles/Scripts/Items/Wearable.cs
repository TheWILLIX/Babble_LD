using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wearable : MonoBehaviour
{
    private Transform _player;
    private float _lifeTime = -1;

    public float LifeTime { get => _lifeTime; }

    public void Initialize(Transform player, float lifetime)
    {
        _player = player;
        _lifeTime = lifetime;
    }

    // Start is called before the first frame update
    void Start()
    {
        transform.parent = _player;
        transform.localPosition = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        if (_lifeTime == -1)
            return;
        _lifeTime -= Time.deltaTime;
        if (_lifeTime <= 0)
        {
            if(TryGetComponent<VFXPerfume>(out var perf))
            {
                foreach(var component in gameObject.GetComponents<Component>())
                {
                    if(component != perf && component != transform)
                        Destroy(component);
                }
                transform.parent = null;
                perf.StopEffect();
                _ = ClemCAddons.Utilities.GameTools.DelayedCall(30000, () => Destroy(gameObject));
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
