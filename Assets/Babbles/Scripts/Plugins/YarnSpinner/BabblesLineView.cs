using System;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Luminosity.IO;
using ClemCAddons;

namespace Yarn.Unity
{
    /// <summary>
    /// A Dialogue View that presents lines of dialogue, using Unity UI
    /// elements.
    /// </summary>
    public class BabblesLineView : DialogueViewBase
    {
        #region Fields
        /// <summary>
        /// The canvas group that contains the UI elements used by this Line
        /// View.
        /// </summary>
        /// <remarks>
        /// If <see cref="useFadeEffect"/> is true, then the alpha value of this
        /// <see cref="CanvasGroup"/> will be animated during line presentation
        /// and dismissal.
        /// </remarks>
        /// <seealso cref="useFadeEffect"/>
        [SerializeField]
        internal CanvasGroup canvasGroup;

        /// <summary>
        /// Controls whether the line view should fade in when lines appear, and
        /// fade out when lines disappear.
        /// </summary>
        /// <remarks><para>If this value is <see langword="true"/>, the <see
        /// cref="canvasGroup"/> object's alpha property will animate from 0 to
        /// 1 over the course of <see cref="fadeInTime"/> seconds when lines
        /// appear, and animate from 1 to zero over the course of <see
        /// cref="fadeOutTime"/> seconds when lines disappear.</para>
        /// <para>If this value is <see langword="false"/>, the <see
        /// cref="canvasGroup"/> object will appear instantaneously.</para>
        /// </remarks>
        /// <seealso cref="canvasGroup"/>
        /// <seealso cref="fadeInTime"/>
        /// <seealso cref="fadeOutTime"/>
        [SerializeField]
        internal bool useFadeEffect = true;

        /// <summary>
        /// The time that the fade effect will take to fade lines in.
        /// </summary>
        /// <remarks>This value is only used when <see cref="useFadeEffect"/> is
        /// <see langword="true"/>.</remarks>
        /// <seealso cref="useFadeEffect"/>
        [SerializeField]
        [Min(0)]
        internal float fadeInTime = 0.25f;

        /// <summary>
        /// The time that the fade effect will take to fade lines out.
        /// </summary>
        /// <remarks>This value is only used when <see cref="useFadeEffect"/> is
        /// <see langword="true"/>.</remarks>
        /// <seealso cref="useFadeEffect"/>
        [SerializeField]
        [Min(0)]
        internal float fadeOutTime = 0.05f;

        /// <summary>
        /// The <see cref="TextMeshProUGUI"/> object that displays the text of
        /// dialogue lines.
        /// </summary>
        [SerializeField]
        internal TextMeshProUGUI lineText = null;

        /// <summary>
        /// Controls whether the <see cref="lineText"/> object will show the
        /// character name present in the line or not.
        /// </summary>
        /// <remarks>
        /// <para style="note">This value is only used if <see
        /// cref="characterNameText"/> is <see langword="null"/>.</para>
        /// <para>If this value is <see langword="true"/>, any character names
        /// present in a line will be shown in the <see cref="lineText"/>
        /// object.</para>
        /// <para>If this value is <see langword="false"/>, character names will
        /// not be shown in the <see cref="lineText"/> object.</para>
        /// </remarks>
        [SerializeField]
        [UnityEngine.Serialization.FormerlySerializedAs("showCharacterName")]
        internal bool showCharacterNameInLineView = true;

        /// <summary>
        /// The <see cref="TextMeshProUGUI"/> object that displays the character
        /// names found in dialogue lines.
        /// </summary>
        /// <remarks>
        /// If the <see cref="LineView"/> receives a line that does not contain
        /// a character name, this object will be left blank.
        /// </remarks>
        [SerializeField]
        internal TextMeshProUGUI characterNameText = null;

        /// <summary>
        /// Controls whether the text of <see cref="lineText"/> should be
        /// gradually revealed over time.
        /// </summary>
        /// <remarks><para>If this value is <see langword="true"/>, the <see
        /// cref="lineText"/> object's <see
        /// cref="TMP_Text.maxVisibleCharacters"/> property will animate from 0
        /// to the length of the text, at a rate of <see
        /// cref="typewriterEffectSpeed"/> letters per second when the line
        /// appears. <see cref="onCharacterTyped"/> is called for every new
        /// character that is revealed.</para>
        /// <para>If this value is <see langword="false"/>, the <see
        /// cref="lineText"/> will all be revealed at the same time.</para>
        /// <para style="note">If <see cref="useFadeEffect"/> is <see
        /// langword="true"/>, the typewriter effect will run after the fade-in
        /// is complete.</para>
        /// </remarks>
        /// <seealso cref="lineText"/>
        /// <seealso cref="onCharacterTyped"/>
        /// <seealso cref="typewriterEffectSpeed"/>
        [SerializeField]
        internal bool useTypewriterEffect = false;

        /// <summary>
        /// A Unity Event that is called each time a character is revealed
        /// during a typewriter effect.
        /// </summary>
        /// <remarks>
        /// This event is only invoked when <see cref="useTypewriterEffect"/> is
        /// <see langword="true"/>.
        /// </remarks>
        /// <seealso cref="useTypewriterEffect"/>
        [SerializeField]
        internal UnityEngine.Events.UnityEvent onCharacterTyped;

        /// <summary>
        /// The number of characters per second that should appear during a
        /// typewriter effect.
        /// </summary>
        /// <seealso cref="useTypewriterEffect"/>
        [SerializeField]
        [Min(0)]
        internal float typewriterEffectSpeed = 0f;

        /// <summary>
        /// The game object that represents an on-screen button that the user
        /// can click to continue to the next piece of dialogue.
        /// </summary>
        /// <remarks>
        /// <para>This game object will be made inactive when a line begins
        /// appearing, and active when the line has finished appearing.</para>
        /// <para>
        /// This field will generally refer to an object that has a <see
        /// cref="Button"/> component on it that, when clicked, calls <see
        /// cref="OnContinueClicked"/>. However, if your game requires specific
        /// UI needs, you can provide any object you need.</para>
        /// </remarks>
        /// <seealso cref="autoAdvance"/>
        [SerializeField]
        internal GameObject continueButton = null;

        /// <summary>
        /// The amount of time to wait after any lin
        /// </summary>
        [SerializeField]
        [Min(0)]
        internal float holdTime = 1f;

        /// <summary>
        /// Controls whether this Line View will wait for user input before
        /// indicating that it has finished presenting a line.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If this value is true, the Line View will not report that it has
        /// finished presenting its lines. Instead, it will wait until the <see
        /// cref="UserRequestedViewAdvancement"/> method is called.
        /// </para>
        /// <para style="note"><para>The <see cref="DialogueRunner"/> will not
        /// proceed to the next piece of content (e.g. the next line, or the
        /// next options) until all Dialogue Views have reported that they have
        /// finished presenting their lines. If a <see cref="LineView"/> doesn't
        /// report that it's finished until it receives input, the <see
        /// cref="DialogueRunner"/> will end up pausing.</para>
        /// <para>
        /// This is useful for games in which you want the player to be able to
        /// read lines of dialogue at their own pace, and give them control over
        /// when to advance to the next line.</para></para>
        /// </remarks>
        [SerializeField]
        public bool autoAdvance = false;

        /// <summary>
        /// The current <see cref="LocalizedLine"/> that this line view is
        /// displaying.
        /// </summary>
        LocalizedLine currentLine = null;

        /// <summary>
        /// A stop token that is used to interrupt the current animation.
        /// </summary>
        Effects.CoroutineInterruptToken currentStopToken = new Effects.CoroutineInterruptToken();

        [SerializeField] private DialogueManager _dialogueManager = null;

        [SerializeField] private string _skipInputController = null;
        [SerializeField] private string _skipInputPC = null;
        private bool _blockSkip = false;





        [Header("DialogueSkip")]
        [SerializeField] private float _timeBeforeSkipStart = 1f;
        private float _dialogueSkipTimeStamp = 0f;
        [SerializeField] private float _skippingSpeed = 0.5f; //The Smaller the faster it will skip dialogue
        private bool _skipButtonRelease = false;
        private bool _inSkip = false;
        //For later updates -> two skippingSpeed a starting one and then it increment until it reach a maximumSkippingSpeed

        #region Pannel Fields
        [Header("DialogBox Mode")]
        private EDialogBoxType _dialogBoxType = EDialogBoxType.CLASSIC;
        [SerializeField] private GameObject _currentDialogBox = null;
        [SerializeField] private GameObject _currentSkipButton = null;



        [Header("Classic Pannel")] //Might be a good idea to make a Pannel Class to store those datas
        [SerializeField] private GameObject _classicDialogBox = null;
        private TextMeshProUGUI _classicText = null;
        [SerializeField] private Image _classicSkipButton = null;

        [Header("Astere Pannel")]
        [SerializeField] private GameObject _astereDialogBox = null;
        [SerializeField] private TextMeshProUGUI _astereLineText = null;
        [SerializeField] private Image _astereSkipButton = null;

        [Header("Panneau Pannel")]
        [SerializeField] private GameObject _panneauDialogBox = null;
        [SerializeField] private TextMeshProUGUI _panneauLineText = null;
        [SerializeField] private Image _panneauSkipButton = null;

        [Header("Jeunes Pannel")]
        [SerializeField] private GameObject _jeunesDialogBox = null;
        [SerializeField] private TextMeshProUGUI _jeunesLineText = null;
        [SerializeField] private Image _jeunesSkipButton = null;

        [Header("FullScreen Pannel")]
        [SerializeField] private GameObject _fullscreenDialogBox = null;
        [SerializeField] private TextMeshProUGUI _fullscreenLineText = null;
        [SerializeField] private Image _fullscreenSkipButton = null;
        [SerializeField] private Image _fullscreenContour = null;
        [SerializeField] private Image _fullscreenCharacterFace = null;

        #endregion Pannel Fiels


        #endregion Fields

        #region Properties
        public EDialogBoxType DialogBoxType => _dialogBoxType;

        public bool BlockSkip
        {
            get
            {
                return _blockSkip;
            }
            set
            {
                _blockSkip = value;
            }
        }

        #region Pannel Properties
        public GameObject CurrentDialogBox
        {
            get
            {
                return _currentDialogBox;
            }
            set
            {
                _currentDialogBox = value;
            }
        }

        public GameObject CurrentSkipButton
        {
            get
            {
                return _currentSkipButton;
            }
            set
            {
                _currentSkipButton = value;
            }
        }

        public Image FullscreenCharacterFace { get => _fullscreenCharacterFace; set => _fullscreenCharacterFace = value; }


        #endregion Pannel Properties



        #endregion Properties


        #region Methods
        public void TypeWriterSpeedActivation(bool condition)
        {
            useTypewriterEffect = condition;
        }
        public void ChangeTypeWriterSpeed(float speed)
        {
            typewriterEffectSpeed = speed;
        }

        #region Pannel Methods

        public void ClassicMode()
        {
            _classicDialogBox.SetActive(true); //We Activate / Desactivate the correct dialogBox
            _astereDialogBox.SetActive(false);
            _panneauDialogBox.SetActive(false);
            _jeunesDialogBox.SetActive(false);


            _currentDialogBox = _classicDialogBox; //We set the currentDialogBox Used to the Classic one

            lineText = _classicText; //We set up the correct line Text


            _currentSkipButton = _classicSkipButton.gameObject;
        }

        public void AstereMode()
        {
            _classicDialogBox.SetActive(false); //We Activate / Desactivate the correct dialogBox
            _panneauDialogBox.SetActive(false);
            _astereDialogBox.SetActive(true);
            _jeunesDialogBox.SetActive(false);
            _fullscreenDialogBox.SetActive(false);


            _currentDialogBox = _astereDialogBox; //We set the currentDialogBox Used to the Astere one

            lineText = _astereLineText; //We set up the correct line Text


            _currentSkipButton = _astereSkipButton.gameObject;

        }
      
        public void JeunesMode()
        {
            _classicDialogBox.SetActive(false); //We Activate / Desactivate the correct dialogBox
            _astereDialogBox.SetActive(false);
            _panneauDialogBox.SetActive(false);
            _jeunesDialogBox.SetActive(true);
            _fullscreenDialogBox.SetActive(false);


            _currentDialogBox = _jeunesDialogBox; //We set the currentDialogBox Used to the Classic one

            lineText = _jeunesLineText; //We set up the correct line Text


            _currentSkipButton = _jeunesSkipButton.gameObject;
        }

        public void PanneauMode()
        {
            _classicDialogBox.SetActive(false); //We Activate / Desactivate the correct dialogBox
            _astereDialogBox.SetActive(false);
            _panneauDialogBox.SetActive(true);
            _jeunesDialogBox.SetActive(false);


            _currentDialogBox = _panneauDialogBox; //We set the currentDialogBox Used to the Classic one

            lineText = _panneauLineText; //We set up the correct line Text


            _currentSkipButton = _panneauSkipButton.gameObject;
        }

        public void FullScreenMode(bool activate)
        {
            if(activate == false)
            {
                UIManager.Instance.UIController.DialogueManager.ModeFullScreen = false;


                _fullscreenDialogBox.SetActive(false);
                _classicDialogBox.SetActive(true);

                _currentDialogBox = _classicDialogBox; //We set the currentDialogBox Used to the Classic one

                lineText = _classicText; //We set up the correct line Text

                _currentSkipButton = _classicSkipButton.gameObject;

            }
            else
            {

                UIManager.Instance.UIController.DialogueManager.ModeFullScreen = true;



                _classicDialogBox.SetActive(false); //We Activate / Desactivate the correct dialogBox
                _astereDialogBox.SetActive(false);
                _panneauDialogBox.SetActive(false);
                _jeunesDialogBox.SetActive(false);
                _fullscreenDialogBox.SetActive(true);



                _currentDialogBox = _fullscreenDialogBox; //We set the currentDialogBox Used to the Classic one

                lineText = _fullscreenLineText; //We set up the correct line Text


                _currentSkipButton = _fullscreenSkipButton.gameObject;

                //Setup Contour Couleur
                ColorUtility.TryParseHtmlString(UIManager.Instance.UIController.DialogueManager.GetCurrentSpeaker().TextColorID, out Color hexColor); //On récupère la couleur du speaker

                _fullscreenContour.color = hexColor;

            }
       
        }

        #endregion Pannel Methods

        private void Update()
        {

            if(_blockSkip != true)
            {
                if (InputManager.GetButtonUp(_skipInputController) || InputManager.GetButtonUp(_skipInputPC))
                {
                    _skipButtonRelease = true;
                    _dialogueSkipTimeStamp = 0;
                }

                // if (is in dialog && getbuttondown
                // isSkipping = true

                // if getbuttonup
                // isSkipping = false

                if (InputManager.GetButton(_skipInputController) || InputManager.GetButton(_skipInputPC))
                {
                    _skipButtonRelease = false;


                    _dialogueSkipTimeStamp += Time.deltaTime;

                    if (_dialogueSkipTimeStamp >= _timeBeforeSkipStart)
                    {
                        Debug.Log("SkipStart");
                        OnContinueClicked();
                        _dialogueSkipTimeStamp = _dialogueSkipTimeStamp - _skippingSpeed;
                        _inSkip = true;
                    }

                }

                if (InputManager.GetButtonDown(_skipInputController) || InputManager.GetButtonDown(_skipInputPC))
                {
                    OnContinueClicked();
                }
            }


        }

        private void Start()
        {
            canvasGroup.alpha = 0;
            _classicText = lineText;
        }

        private void Reset()
        {
            canvasGroup = GetComponentInParent<CanvasGroup>();
        }

        /// <inheritdoc/>
        public override void DismissLine(Action onDismissalComplete)
        {
            currentLine = null;

            StartCoroutine(DismissLineInternal(onDismissalComplete));
        }

        private IEnumerator DismissLineInternal(Action onDismissalComplete)
        {

            canvasGroup.interactable = false;

            // If we're using a fade effect, run it, and wait for it to finish.
            if (useFadeEffect)
            {
                yield return StartCoroutine(Effects.FadeAlpha(canvasGroup, 1, 0, fadeOutTime, currentStopToken));
                currentStopToken.Complete();
            }

            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;
            onDismissalComplete();

            _dialogueManager.EndLine(); //--- End Of Line

        }

        /// <inheritdoc/>
        public override void InterruptLine(LocalizedLine dialogueLine, Action onInterruptLineFinished)
        {
            currentLine = dialogueLine;

            // Cancel all coroutines that we're currently running. This will
            // stop the RunLineInternal coroutine, if it's running.
            StopAllCoroutines();

            // for now we are going to just immediately show everything
            // later we will make it fade in
            lineText.gameObject.SetActive(true);
            canvasGroup.gameObject.SetActive(true);

            int length;

            if (characterNameText == null)
            {
                if (showCharacterNameInLineView)
                {
                    lineText.text = dialogueLine.Text.Text;
                    length = dialogueLine.Text.Text.Length;
                }
                else
                {
                    lineText.text = dialogueLine.TextWithoutCharacterName.Text;
                    length = dialogueLine.TextWithoutCharacterName.Text.Length;
                }
            }
            else
            {
                characterNameText.text = dialogueLine.CharacterName;
                lineText.text = dialogueLine.TextWithoutCharacterName.Text;
                length = dialogueLine.TextWithoutCharacterName.Text.Length;
            }

            // Show the entire line's text immediately.
            lineText.maxVisibleCharacters = length;

            // Make the canvas group fully visible immediately, too.
            canvasGroup.alpha = 1;

            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            onInterruptLineFinished();

            _dialogueManager.EndLine(); //--- End Of Line
        }

        /// <inheritdoc/>
        public override void RunLine(LocalizedLine dialogueLine, Action onDialogueLineFinished)
        {
            // Stop any coroutines currently running on this line view (for
            // example, any other RunLine that might be running)
            StopAllCoroutines();

            // Begin running the line as a coroutine.
            StartCoroutine(RunLineInternal(dialogueLine, onDialogueLineFinished));
        }

        private IEnumerator RunLineInternal(LocalizedLine dialogueLine, Action onDialogueLineFinished)
        {
            IEnumerator PresentLine()
            {
                lineText.gameObject.SetActive(true);
                canvasGroup.gameObject.SetActive(true);

                // Hide the continue button until presentation is complete (if
                // we have one).
             
                if (continueButton != null)
                {
                    continueButton.SetActive(false);
                }

                if (characterNameText != null)
                {
                    // If we have a character name text view, show the character
                    // name in it, and show the rest of the text in our main
                    // text view.
                    characterNameText.text = dialogueLine.CharacterName;

                    UIManager.Instance.UIController.DialogueManager.UpdateSpeaker(); // !! DANGEROUS LINE BUT USEFULL FOR WHEN NEW DIALOGUE BOX ARE BEING CREATED
                    lineText.text = dialogueLine.TextWithoutCharacterName.Text;

                }
                else
                {
                    // We don't have a character name text view. Should we show
                    // the character name in the main text view?
                    if (showCharacterNameInLineView)
                    {
                        // Yep! Show the entire text.
                        lineText.text = dialogueLine.Text.Text;
                    }
                    else
                    {
                        // Nope! Show just the text without the character name.
                        lineText.text = dialogueLine.TextWithoutCharacterName.Text;
                    }
                }

                if (useTypewriterEffect)
                {
                    // If we're using the typewriter effect, hide all of the
                    // text before we begin any possible fade (so we don't fade
                    // in on visible text).
                    lineText.maxVisibleCharacters = 0;
                }
                else
                {
                    // Ensure that the max visible characters is effectively
                    // unlimited.
                    lineText.maxVisibleCharacters = int.MaxValue;
                }

                // If we're using the fade effect, start it, and wait for it to
                // finish.
                if (useFadeEffect)
                {
                    yield return StartCoroutine(Effects.FadeAlpha(canvasGroup, 0, 1, fadeInTime, currentStopToken));
                    if (currentStopToken.WasInterrupted)
                    {
                        // The fade effect was interrupted. Stop this entire
                        // coroutine.
                        yield break;
                    }
                }


                // If we're using the typewriter effect, start it, and wait for
                // it to finish.
                if (useTypewriterEffect)
                {
                    yield return StartCoroutine(
                        Effects.Typewriter(
                            lineText,
                            typewriterEffectSpeed,
                            () => onCharacterTyped.Invoke(),
                            currentStopToken
                        )
                    );
                    if (currentStopToken.WasInterrupted)
                    {
                        // The typewriter effect was interrupted. Stop this
                        // entire coroutine.
                        yield break;
                    }
                }
            }
            currentLine = dialogueLine;

            // Run any presentations as a single coroutine. If this is stopped,
            // which UserRequestedViewAdvancement can do, then we will stop all
            // of the animations at once.
            yield return StartCoroutine(PresentLine());

            currentStopToken.Complete();

            _dialogueManager.VoiceSFXDialogue.StopLettersSFX(); //We stop the letters apparition sounds

            // All of our text should now be visible.
            lineText.maxVisibleCharacters = int.MaxValue;

            // Our view should at be at full opacity.
            canvasGroup.alpha = 1f;

            // Show the continue button, if we have one.
            if (continueButton != null)
            {
                continueButton.SetActive(true);
            }

            // If we have a hold time, wait that amount of time, and then
            // continue.
            if (holdTime > 0)
            {
                yield return new WaitForSeconds(holdTime);
            }

            if (autoAdvance == false)
            {
                // The line is now fully visible, and we've been asked to not
                // auto-advance to the next line. Stop here, and don't call the
                // completion handler - we'll wait for a call to
                // UserRequestedViewAdvancement, which will interrupt this
                // coroutine.
                yield break;
            }

            // Our presentation is complete; call the completion handler.
            onDialogueLineFinished();
        }

        /// <inheritdoc/>
        public override void UserRequestedViewAdvancement()
        {
            // We received a request to advance the view. If we're in the middle of
            // an animation, skip to the end of it. If we're not current in an
            // animation, interrupt the line so we can skip to the next one.

            // we have no line, so the user just mashed randomly
            if (currentLine == null)
            {
                return;
            }

            // Is an animation running that we can stop?
            if (currentStopToken.CanInterrupt)
            {
                // Stop the current animation, and skip to the end of whatever
                // started it.
                currentStopToken.Interrupt();
            }
            else
            {
                // No animation is currently running. Signal that we want to
                // interrupt the line instead.
                requestInterrupt?.Invoke();
            }
        }

        /// <summary>
        /// Called when the <see cref="continueButton"/> is clicked.
        /// </summary>
        public void OnContinueClicked()
        {
            // When the Continue button is clicked, we'll do the same thing as
            // if we'd received a signal from any other part of the game (for
            // example, if a DialogueAdvanceInput had signalled us.)
            UserRequestedViewAdvancement();
        }


    
    }
    #endregion Methods
}
