using System;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ClemCAddons;



namespace Yarn.Unity
{

    public class BabblesOptionView : UnityEngine.UI.Selectable, ISubmitHandler, IPointerClickHandler, IPointerEnterHandler, ISelectHandler
    {
        [SerializeField] TextMeshProUGUI text;
        [SerializeField] bool showCharacterName = false;
        [SerializeField] RawImage _contourBulle = null;
        [Header("Key Hint")]
        [SerializeField] Image _hintTouche = null;
        [SerializeField] Color _lockColor;

        [Header("Seum Hint")]
        [SerializeField] Image _seumHint = null;
        [SerializeField] Sprite _saleSpriteHint = null;
        [SerializeField] Sprite _freshSpriteHint = null;

        private ESeumAnswerType _seumAnswerType = ESeumAnswerType.NONE;
        public ESeumAnswerType SeumAnswerType
        {
            get
            {
              return  _seumAnswerType;
            }
            set
            {
                _seumAnswerType = value;
            }
        }
        public Action<DialogueOption> OnOptionSelected;

        DialogueOption _option;

        bool hasSubmittedOptionSelection = false;

        bool _blockInteraction = false;

        public DialogueOption Option
        {
            get => _option;

            set
            {
                _option = value;

                hasSubmittedOptionSelection = false;

                // When we're given an Option, use its text and update our
                // interactibility.
                if (showCharacterName)
                {
                    text.text = value.Line.Text.Text;
                }
                else
                {
                    text.text = value.Line.TextWithoutCharacterName.Text;
                }
                interactable = value.IsAvailable;

                _hintTouche.gameObject.SetActive(false);
            }
        }

        // If we receive a submit or click event, invoke our "we just selected
        // this option" handler.

        public void OnSubmit(BaseEventData eventData)
        {
            if (_blockInteraction == false)
            {
                EventSystem.current.SetSelectedGameObject(null);
                InvokeOptionSelected();
            }
        }

        public void InvokeOptionSelected()
        {
            // We only want to invoke this once, because it's an error to
            // submit an option when the Dialogue Runner isn't expecting it. To
            // prevent this, we'll only invoke this if the flag hasn't been cleared already.
            if (hasSubmittedOptionSelection == false)
            {
                OnOptionSelected.Invoke(Option);
                hasSubmittedOptionSelection = true;
            }
        }

        

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_blockInteraction == false)
            {
                InvokeOptionSelected();
            }
        }

        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            _hintTouche.gameObject.SetActive(true);
            if(this._blockInteraction == true)
            {
                _hintTouche.color = Color.gray;
            }
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);
            _hintTouche.gameObject.SetActive(false);
            _hintTouche.color = Color.white; //We reset the color

          //  if(_seumAnswerType == ESeumAnswerType == S)

        }

        // If we mouse-over, we're telling the UI system that this element is
        // the currently 'selected' (i.e. focused) element. 
        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.Select();
            _hintTouche.gameObject.SetActive(true);
        }

        #region Custom Made
      /*  public void SetReponseBonneHumeur()
        {
            GetComponent<Image>().color = Color.cyan;
        }
        public void SetReponseSale()
        {
            GetComponent<Image>().color = Color.red;
        }*/
        public void BlockInteraction(bool value)
        {
            _blockInteraction = value;
        }

        #endregion Custom Made

    }
}