using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesactivationTrigger : MonoBehaviour
{

    #region Fields



    private bool _triggerEnabled = false;

    [SerializeField] private bool _desactivateSomeStuff = false;
    [SerializeField] private GameObject[] _stuffToDesactivate = null;
    #endregion Fields



    #region Methods



    void OnTriggerEnter(Collider collider)
    {
        if (collider.tag == "Player")
        {
            if (_triggerEnabled == false)
            {
                if (_desactivateSomeStuff == true && _stuffToDesactivate.Length >= 1)
                {
                    foreach (GameObject gameObject in _stuffToDesactivate)
                    {
                        gameObject.SetActive(false);
                    }
                }
                _triggerEnabled = true;
            }

        }
    }

    #endregion Methods


}
