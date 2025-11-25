using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuPathRunner : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private int starterPath = -1;
    private int previous = -1;

    void Start()
    {
        if (starterPath != -1)
            transform.GetChild(starterPath).GetComponent<MainMenuPath>().Play(Reroll);
        else
            Reroll();
    }

    public void Reroll()
    {
        if (gameObject == null)
            return;
        if (transform.childCount == 0)
            return;
        else if (transform.childCount == 1)
            previous = 0;
        else
        {
            int res = previous;
            while (res == previous)
                res = Random.Range(0, transform.childCount);
            previous = res;
        }
        transform.GetChild(previous).GetComponent<MainMenuPath>().Play(Reroll);
    }
}
