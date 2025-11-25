using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Yarn.Unity;

public class DialogueTestTool : MonoBehaviour
{

    #region Fields
    [Header("Yarn Spinner")]
    [SerializeField] private DialogueRunner _dialogueRunner = null;
    [SerializeField] private InMemoryVariableStorage _storage = null;


    [Header("Dialogue Start Configuration")]
    [SerializeField] private InputField _dialogueToStart = null;
    [SerializeField] private string _defaultNodeName = "Marline.Intro.FORET";


    [Header("Variable Modifcation")]
    [SerializeField] private EYarnVariableType _typeOfVariableToModify = EYarnVariableType.NONE;

    [SerializeField] private InputField _variableNameInputField = null;

    [SerializeField] private InputField _variableValueInputField = null;

    [Header("ModifyTextSpeed")]
    [SerializeField] private InputField _textSpeedInputField = null;
    [SerializeField] private BabblesLineView _lineView = null;

    [Header("CheckRelation")]
    [SerializeField] private InputField _relationInputField = null;
    [SerializeField] private TMP_Text _textRelation = null;

    [Header("Test avec Gaetan")]
    [SerializeField] private float _speedDialogueRate = 1.0f;

    [SerializeField] private bool _desactivateInputWhenStartingDialogue = false;
    #endregion Fields


    #region Methods
    private void Start()
    {
        _dialogueToStart.text = _defaultNodeName;
    }

    
    public void ShowDialogue()
   {
      //  try
       // {
            UIManager.Instance.UIController.StartDialogue(_dialogueToStart.text, _desactivateInputWhenStartingDialogue);
        //}
       // catch
        //{
        //    Debug.LogError(_dialogueToStart + " ; A error occured, check that you have specified the correct name both in Yarn Script and in the DialogueTestTool attributes");
        //}
   }

    public void ModifyVariable()
    {
        try
        {
            switch(_typeOfVariableToModify)
            {
                case EYarnVariableType.INT:
                    _storage.SetValue(_variableNameInputField.text, int.Parse(_variableValueInputField.text));
                    break;
                case EYarnVariableType.BOOL:
                    switch(_variableValueInputField.text)
                    {
                        case "true":
                        case "True":
                        case "TRUE":
                        case "tRUE":
                            _storage.SetValue(_variableNameInputField.text, true);
                            break;

                        case "false":
                        case "False":
                        case "FALSE":
                        case "fALSE":
                            _storage.SetValue(_variableNameInputField.text, false);
                            break;
                    }
                    break;
                case EYarnVariableType.STRING:
                    _storage.SetValue(_variableNameInputField.text, _variableValueInputField.text);
                    break;
                case EYarnVariableType.NONE:
                    Debug.LogWarning("Type of Variable is set to None, set it to the variable type you want to modify");
                    break;
            }
        }
        catch
        {
            Debug.LogError("A Error occured when trying to modify the variable");
        }
    }

    public void ModifyTextSpeed()
    {
        float textSpeed;
        textSpeed = float.Parse(_textSpeedInputField.text);

        _lineView.ChangeTypeWriterSpeed(textSpeed * _speedDialogueRate);

    }

    public void CheckRelation()
    {
        _textRelation.text = UIManager.Instance.UIController.DialogueManager.DebugRelation(_relationInputField.text);
    }

    #endregion Methods

}
