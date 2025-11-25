using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;
using Cinemachine.Utility;

public class PlaceAboveTarget : MonoBehaviour
{
    #region Fields

    [SerializeField] private Collider _target;
    [SerializeField] private DialogueManager _dialogueManager = null;
    [SerializeField] private Vector2 _startPosition;
    [SerializeField] private Transform _character = null;
    [SerializeField] private float _bulleOffset = 5;
    [SerializeField] private float _astereOnTopOfBulleOffset = 5;


    #endregion Fields

    #region Properties
    public Collider Target { get => _target; set => _target = value; }
    public Transform Character { get => _character; set => _character = value; }
    #endregion Properties

    #region Methods

    void Start()
    {
        _startPosition = GetComponent<RectTransform>().anchoredPosition;
    }

    void Update()
    {
        if (Target != null)
        {
            UpdateTextPosition();
        }
    }

    public void UpdateTextPosition()
    {
        if (DialogueManager.Instance.ModeFullScreen == true)
        {
            GetComponent<RectTransform>().anchoredPosition = _startPosition;
        }
        else if(DialogueManager.Instance.IsInDialog == true && DialogueManager.Instance.CharacterCurrentlySpeaking == "Bulle")
        {
            var usedPos = _character.GetComponent<Collider>().bounds.center
                  .SetY(_character.position.y + _character.GetComponent<Collider>().bounds.extents.y);
            if (Vector3.Dot(usedPos - Camera.allCameras[0].transform.position, Camera.allCameras[0].transform.forward) < 0)
                    GetComponent<RectTransform>().position = Vector2.one * 100000;
            else
                GetComponent<RectTransform>().position = Camera.allCameras[0].WorldToScreenPoint(usedPos).SetZ(0);
        }
        else if (DialogueManager.Instance.IsInDialog == true && DialogueManager.Instance.CharacterCurrentlySpeaking == "Astère")
        {
            var usedPos = _character.position
                .SetY(_character.position.y + _character.GetComponent<Collider>().bounds.extents.y);
            if (Vector3.Dot(usedPos - Camera.allCameras[0].transform.position, Camera.allCameras[0].transform.forward) < 0)
                GetComponent<RectTransform>().position = Vector2.one * 100000;
            else
                GetComponent<RectTransform>().position = Camera.allCameras[0].WorldToScreenPoint(usedPos).SetZ(0).SetZ(0)
                + Vector3.zero.SetY(_astereOnTopOfBulleOffset);
        }
        else if (DialogueManager.Instance.IsInDialog == true && (DialogueManager.Instance.CharacterCurrentlySpeaking == "Gumpy" || DialogueManager.Instance.CharacterCurrentlySpeaking == "Dumbo" || DialogueManager.Instance.CharacterCurrentlySpeaking == "Jeunes")) //There is a exeption with Gumpy, Dumbo and Jeunes since it's part of the same NPC_Interaction as Ablette
        {
           // Debug.Log("TESTTTTTT" + DialogueManager.Instance.CharacterCurrentlySpeaking);

            if(DialogueManager.Instance.CharacterCurrentlySpeaking == "Gumpy")
            {
                RegularSetAboveTarget("Position_Gumpy");
            }
            else if(DialogueManager.Instance.CharacterCurrentlySpeaking == "Dumbo")
            {
                RegularSetAboveTarget("Position_Dumbo");
            }
            else if(DialogueManager.Instance.CharacterCurrentlySpeaking == "Jeunes")
            {
                RegularSetAboveTarget("Position_Jeunes");
            }

        }
        else if (Target != null)
        {
            RegularSetAboveTarget();
        }
        else
        {
            GetComponent<RectTransform>().position = new Vector3();
            Debug.LogWarning("Something is fucked up");
            return;
        }
    }


    private void RegularSetAboveTarget(string target = "UIPosition")
    {
        Vector3 targetPosition = _target.transform.position;
        // default position should anything after not be found

        //Debug.Log(_target);
        //Debug.Log(_target.transform.parent);

        var r = _target.transform.parent.FindDeep(target, true);
        if (r == null)
            r = _target.transform.parent.FindDeep("UI_Position", true);
        // find child of parent starting with name
        Collider t = null;
        if (r != null) // if found
        {
            targetPosition = r.position;
            // want to place it there
        }
        else
        {
            t = _target.transform.parent.GetComponent<Collider>();
            // if no uiposition, parent (NPC) might still have a collider
        }
        float offset = 0.25f;
        // base offset is 0.25
        if (r == null && t == null)
        {
            offset += _target.bounds.extents.y;
            // if no uiposition && parent has no collider, use the detection bounds to account for the potential height of the NPC
        }
        else if (t != null)
        {
            offset += t.bounds.extents.y;
            // if no ui position but parent has a collider, use that to account for the potential height of the npc
        }
        targetPosition += Vector3.zero.SetY(offset);
        if (Vector3.Dot(targetPosition - Camera.allCameras[0].transform.position, Camera.allCameras[0].transform.forward) < 0)
            GetComponent<RectTransform>().position = Vector2.one * 100000;
        else
            GetComponent<RectTransform>().position = Camera.allCameras[0].WorldToScreenPoint(targetPosition).SetZ(0);
        // set position (bottom)
    }


    #endregion Methods
}
