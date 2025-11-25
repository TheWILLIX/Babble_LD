using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SuckFystem : MonoBehaviour
{
    void Awake()
    {
        if(SceneManager.sceneCount > 1)
            gameObject.SetActive(false);
    }
}
