using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ClemCAddons;

using ClemCAddons.Utilities;
using System.Threading.Tasks;

public class TestAll : MonoBehaviour
{
    public float test = 0f;
    // Start is called before the first frame update
    void Start()
    {
        AsyncTest();
    }

    private async void AsyncTest()
    {
        await Task.Delay(10);
    }

    // Update is called once per frame
    void Update()
    {
    }
}
