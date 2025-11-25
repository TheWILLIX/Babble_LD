using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ClemCAddons;

namespace Yarn.Unity
{
    public class BabblesOptionsListView : DialogueViewBase
    {
        [SerializeField] CanvasGroup canvasGroup;

        [SerializeField] BabblesOptionView optionViewPrefab;

        [SerializeField] TextMeshProUGUI lastLineText;

        [SerializeField] float fadeTime = 0.1f;

        [SerializeField] bool showUnavailableOptions = false;

        [SerializeField] Color _seumAnswerColor = Color.red;
        [SerializeField] Color _freshAnswerColor = Color.cyan;

        // A cached pool of OptionView objects so that we can reuse them
        List<BabblesOptionView> optionViews = new List<BabblesOptionView>();

        // The method we should call when an option has been selected.
        Action<int> OnOptionSelected;

        // The line we saw most recently.
        LocalizedLine lastSeenLine;

        public void Start()
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        public void Reset()
        {
            canvasGroup = GetComponentInParent<CanvasGroup>();
        }

        public override void RunLine(LocalizedLine dialogueLine, Action onDialogueLineFinished)
        {
            // Don't do anything with this line except note it and
            // immediately indicate that we're finished with it. RunOptions
            // will use it to display the text of the previous line.
            lastSeenLine = dialogueLine;
            onDialogueLineFinished();
        }

        public void ExitOptions()
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        public override void RunOptions(DialogueOption[] dialogueOptions, Action<int> onOptionSelected)
        {
            //Security

            if(UIManager.Instance.UIController.IsInGiveSituation == true)
            {
                Debug.LogError("We should not be creating new options since we are in a Give Situation");
                return;
            }
            else if(UIManager.Instance.UIController.DialogueManager.IsInDialog == false)
            {
                Debug.Log("Creating Options outside of a dialogue");
                return;
            }
            else if (UIManager.Instance.UIController.DialogueManager.IsInDialog == true)
            {
                Debug.Log("Creating Options and in dialogue");

            }



            // Hide all existing option views
            foreach (BabblesOptionView optionView in optionViews)
            {
                optionView.gameObject.SetActive(false);
            }

            // If we don't already have enough option views, create more
            while (dialogueOptions.Length > optionViews.Count)
            {
                var optionView = CreateNewOptionView();
                optionView.gameObject.SetActive(false);
            }

            // Set up all of the option views
            int optionViewsCreated = 0;


            for (int i = 0; i < dialogueOptions.Length; i++)
            {
                var optionView = optionViews[i];
                var option = dialogueOptions[i];

                //Check si l'option

                Debug.Log("OPTIONS CHOICES CREATION");

                #region Seum Answer
                string ID = option.TextID.Substring(5, 1);
               
                if (ID == "1")
                {
                  optionView.GetComponentInChildren<RawImage>().color = _seumAnswerColor;
                  Debug.Log("Réponse Salé");
                    optionView.SeumAnswerType = ESeumAnswerType.SALE;
                }
                else if (ID == "2")
                {
                    optionView.GetComponentInChildren<RawImage>().color = _freshAnswerColor;
                    Debug.Log("Réponse Bonne Humeur");
                    optionView.SeumAnswerType = ESeumAnswerType.FRESH;

                }
                else if ((ID != "2" || ID != "1") && optionView.GetComponent<Image>().color != Color.white)
                {
                    optionView.GetComponentInChildren<RawImage>().color = Color.white;
                    optionView.SeumAnswerType = ESeumAnswerType.NONE;

                }

                #endregion Seum Answer



                if (option.IsAvailable == false)
                {
                    //  optionView.interactable = false;
                    optionView.BlockInteraction(true);
                    Debug.Log("Non");
                }
                else
                {
                    optionView.BlockInteraction(false);
                }


                if (option.IsAvailable == false && showUnavailableOptions == false)
                {
                    // Don't show this option.
                    continue;
                }

                optionView.gameObject.SetActive(true);

                optionView.Option = option;

                // The first available option is selected by default
                if (optionViewsCreated == 0)
                {
                    optionView.Select();
                }

                optionViewsCreated += 1;
            }


            // Update the last line, if one is configured
            if (lastLineText != null)
            {
                if (lastSeenLine != null)
                {
                    lastLineText.gameObject.SetActive(true);
                    lastLineText.text = lastSeenLine.Text.Text;
                }
                else
                {
                    lastLineText.gameObject.SetActive(false);
                }
            }

            // Note the delegate to call when an option is selected
            OnOptionSelected = onOptionSelected;

            // Fade it all in
            StartCoroutine(Effects.FadeAlpha(canvasGroup, 0, 1, fadeTime));


            // Initialize navigation
            GenerateNavigation(optionViews.ToArray());

            /// <summary>
            /// Creates and configures a new <see cref="BabblesOptionView"/>, and adds
            /// it to <see cref="optionViews"/>.
            /// </summary>
            BabblesOptionView CreateNewOptionView()
            {
                BabblesOptionView optionView = Instantiate(optionViewPrefab);
                optionView.transform.SetParent(transform, false);
                optionView.transform.SetAsLastSibling();

                optionView.OnOptionSelected = OptionViewWasSelected;
                optionViews.Add(optionView);

                return optionView;
            }


            /// <summary>
            /// Called by <see cref="OptionView"/> objects.
            /// </summary>
            void OptionViewWasSelected(DialogueOption option)
            {
                StartCoroutine(OptionViewWasSelectedInternal(option));

                //Conditions pour voir si c'est une réponse salé / réponse bonne humeur
                string ID = option.TextID.Substring(5, 1);
                if (ID == "1")
                {
                    Debug.Log("Réponse Salé Selectionné");
                }
                else if (ID == "2")
                {
                    Debug.Log("Réponse Bonne Humeur Selectionné");
                }


                AudioManager.Start2DSound("S_ReponseSelection");


                IEnumerator OptionViewWasSelectedInternal(DialogueOption selectedOption)
                {
                    yield return StartCoroutine(Effects.FadeAlpha(canvasGroup, 1, 0, fadeTime));
                    OnOptionSelected(selectedOption.DialogueOptionID);
                }
            }

            void GenerateNavigation(BabblesOptionView[] views)
            {
                for(int i = 0; i < views.Length; i++)
                {
                    var view = views[i];
                    var nav = new Navigation();
                    nav.mode = Navigation.Mode.Explicit;
                    var left = GetLeft(views, view);
                    var right = GetRight(views, view);
                    var up = i;
                    var down = i;
                    nav.selectOnLeft = views[left];
                    nav.selectOnRight = views[right];
                    nav.selectOnUp = views[up];
                    nav.selectOnDown = views[down];
                    view.navigation = nav;
                }
            }

            int GetLeft(BabblesOptionView[] views, BabblesOptionView target)
            {
                int id = views.FindIndex(target);
                if (id == 0)
                    return views.Length - 1;
                else
                    return id - 1;
            }

            int GetRight(BabblesOptionView[] views, BabblesOptionView target)
            {
                int id = views.FindIndex(target);
                if (id == views.Length - 1)
                    return 0;
                else
                    return id + 1;
            }
        }
    }
}
