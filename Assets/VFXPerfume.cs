using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXPerfume : MonoBehaviour
{
    [SerializeField] private GameObject _AlgaePerfume;
    [SerializeField] private GameObject _FruitPerfume;
    [SerializeField] private GameObject _VanillaPerfume;

    public PerfumeType Type;

    private GameObject _vfx;

    public enum PerfumeType
    {
        Algae,
        Fruit,
        Vanilla
    }

    public void StopEffect()
    {
        var main = _vfx.GetComponent<ParticleSystem>().main;
        main.loop = false;

    }

    void Start()
    {
        GameObject target;
        switch (Type)
        {
            case PerfumeType.Algae:
                target = _AlgaePerfume;
                break;
            case PerfumeType.Fruit:
                target = _FruitPerfume;
                break;
            case PerfumeType.Vanilla:
                target = _VanillaPerfume;
                break;
            default:
                target = null;
                break;
        }
        _vfx = Instantiate(target, transform.position, Quaternion.identity, transform);
    }
}
