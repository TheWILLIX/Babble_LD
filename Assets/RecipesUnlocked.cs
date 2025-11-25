using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ClemCAddons;
using ClemCAddons.Utilities;
using Luminosity.IO;

public class RecipesUnlocked : MonoBehaviour
{
    [SerializeField] private Image _background;
    [SerializeField] private TMPro.TMP_Text _title;
    [SerializeField] private TMPro.TMP_Text _description;
    [SerializeField] private Image _sprite;
    [SerializeField] private Image _icon;


    private static RecipesUnlocked _instance;
    
    private string _titleText = "$Recipe.Title";

    private bool _active;

    public static RecipesUnlocked Instance { get => _instance; }

    public void Unlock(Recipe recipe)
    {
        Unlock(recipe.ResultItem);
    }

    public void Unlock(Item item)
    {
        UIManager.Instance.UIController.BananeManager.LockExit = true;
        Luminosity.IO.StandaloneInputModule.LockInput = true; //We lock the input of the banane until the UI_Submit button has been pressed

        _background.color = _background.color.SetA(0);
        _title.color = _title.color.SetA(0);
        _description.color = _description.color.SetA(0);
        _sprite.color = _sprite.color.SetA(0);
        _icon.color = _icon.color.SetA(0);


        _title.text = _titleText;
        _description.text = item.CleanName + " !";
        _sprite.sprite = item.Sprite;
        _icon.sprite = UIManager.Instance.UIController.HUDBank.UIValidationKey;


        _title.GetComponent<UIEffects>().Activate();
        Lerper.ConstantLerp(0, 1, 1, (f) => _title.color = _title.color.SetA(f));
        _ = GameTools.DelayedCall(500, () =>
        {
            _description.GetComponent<UIEffects>().Activate();
            _sprite.GetComponent<UIEffects>().Activate();
            Lerper.ConstantLerp(0, 1, 1, (f) => _description.color = _description.color.SetA(f));
            Lerper.ConstantLerp(0, 1, 1, (f) => _sprite.color = _sprite.color.SetA(f));

            Lerper.ConstantLerp(0, 0.75f, 1, (f) => _background.color = _background.color.SetA(f));
        });
        _ = GameTools.DelayedCall(1000, () =>
        {
            _icon.GetComponent<UIEffects>().Activate();
            Lerper.ConstantLerp(0, 1, 0.5f, (f) => _icon.color = _icon.color.SetA(f));
            _active = true;
        });

        AudioManager.Start2DSound("S_RecetteDebloquee");

    }

    void Start()
    {
        _instance = this;
        
        // TESTING:

        //_ = GameTools.DelayedCall(3000, () =>
        //{
        //    Unlock(InventoryManager.Instance.RecipeDB[0]);
        //});
    }

    void Update()
    {
        if(!_active)
        {
            return;
        }
        if (InputManager.GetButtonDown("UI_Submit"))
        {
            Luminosity.IO.StandaloneInputModule.LockInput = false; //We Unlock the input of the banane 

            AudioManager.Start2DSound("S_MenuYes");
           // AudioManager.Start2DSound("S_Recolte");


            _title.GetComponent<UIEffects>().Desactivate();
            _description.GetComponent<UIEffects>().Desactivate();
            _sprite.GetComponent<UIEffects>().Desactivate();
            _icon.GetComponent<UIEffects>().Desactivate();

            _title.GetComponent<UIEffects>().enabled = false;
            _description.GetComponent<UIEffects>().enabled = false;
            _icon.GetComponent<UIEffects>().enabled = false;

            var saveT = _title.GetComponent<RectTransform>().anchoredPosition;
            var saveD = _description.GetComponent<RectTransform>().anchoredPosition;
            var saveI = _icon.GetComponent<RectTransform>().anchoredPosition;

            Lerper.ConstantLerp(saveT, saveT + Vector2.left * 640, 0.5f, (v) =>
            {
                _title.GetComponent<RectTransform>().anchoredPosition = v;
            });
            Lerper.ConstantLerp(saveD, saveD + Vector2.right * 640, 0.5f, (v) =>
            {
                _description.GetComponent<RectTransform>().anchoredPosition = v;
            });
            Lerper.ConstantLerp(1, 0, 0.5f, (f) => _sprite.color = _sprite.color.SetA(f));
            Lerper.ConstantLerp(saveI, saveI + Vector2.down * 70, 0.5f, (v) =>
            {
                _icon.GetComponent<RectTransform>().anchoredPosition = v;
            });
            Lerper.ConstantLerp(0.75f, 0, 1, (f) => _background.color = _background.color.SetA(f));

            _ = GameTools.DelayedCall(1000, () =>
            {
                _title.color = _title.color.SetA(0);
                _description.color = _description.color.SetA(0);
                _sprite.color = _sprite.color.SetA(0);
                _icon.color = _icon.color.SetA(0);
                _background.color = _background.color.SetA(0);
                _title.GetComponent<RectTransform>().anchoredPosition = saveT;
                _description.GetComponent<RectTransform>().anchoredPosition = saveD;
                _icon.GetComponent<RectTransform>().anchoredPosition = saveI;
                _title.GetComponent<UIEffects>().enabled = true;
                _description.GetComponent<UIEffects>().enabled = true;
                _icon.GetComponent<UIEffects>().enabled = true;

                UIManager.Instance.UIController.BananeManager.LockExit = false;
                if(UIManager.Instance.UIController.InventoryOpen)
                    UIManager.Instance.UIController.BananeManager.RessourceInteractions[0].GetComponent<Button>().Select();
            });
            
            _active = false;
        }
    }
}
