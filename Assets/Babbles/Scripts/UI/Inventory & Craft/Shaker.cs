using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Luminosity.IO;
using ClemCAddons;
using ClemCAddons.Utilities;
using UnityEngine.UI;

public class Shaker : MonoBehaviour, IPointerDownHandler, ISelectHandler
{
    [SerializeField] private int requiredShakes = 4;
    [SerializeField] private float cancelDelay = 3;
    private BananeManager _bananeManager = null;
    private float slide = 0;
    private bool sliding = false;
    private int target = 0;
    private int counter = 0;
    private Vector3 originalPos;
    private float cancel = 0;
    private bool canceling;
    private int currentDirection;
    private int step = 0;
    private Vector2 joystickSave;
    private Vector2 currentIndicatorDirection = Vector2.down;
    private Quaternion defaultRotation = Quaternion.identity;
    private Vector2 previousExtremity = Vector2.zero;
    private int shakingType = 0;
    private ShakerAnimation currentAnimation = ShakerAnimation.Neutral;
    private Vector3 targetPosition = Vector3.zero;
    private Quaternion targetRotation = Quaternion.identity;
    private bool disableUpdate = false;
    private AudioSource _shakerAudioSource = null;
    private bool _hasStartedShakingSound = false;

    private enum ShakerAnimation
    {
        Neutral,
        Shaking,
        ToCenter,
        AtCenter,
        Opening,
        Ending,
        Sorting
    }
    private readonly Vector2[] allDirections = new Vector2[]
    { 
        Vector2.down,                   // bottom
        Vector2.down + Vector2.right,   // bottom right
        Vector2.right,                  // right
        Vector2.right + Vector2.up,     // top right
        Vector2.up,                     // top
        Vector2.up + Vector2.left,      // top left
        Vector2.left,                   // left
        Vector2.left + Vector2.down     // bottom left
    };

    // Start is called before the first frame update
    void Start()
    {
        _bananeManager = GetComponentInParent<BananeManager>();
        originalPos = targetPosition = transform.localPosition;
        defaultRotation = targetRotation = transform.rotation;
        _shakerAudioSource = GetComponent<AudioSource>();
    }

    public void Reset()
    {
        transform.localPosition = originalPos;
        transform.rotation = defaultRotation;
        disableUpdate = false;
        slide = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if ((sliding && slide != 0).OnceIfTrueGate("ShakerEmpty".GetHashCode()))
        {
            transform.FindDeep("CraftMask1").FindDeep(t => t.name.ToLower() == _bananeManager.FirstItemInShaker.Name.ToLower()).gameObject.SetActive(false);
            transform.FindDeep("CraftMask2").FindDeep(t => t.name.ToLower() == _bananeManager.SecondItemInShaker.Name.ToLower()).gameObject.SetActive(false);
            _bananeManager.CloseShakerFeedback();
        }
        if (!disableUpdate)
            switch (_bananeManager.ControlMode)
            {
                case BananeManager.InventoryControlMode.Cursor:
                    SlidingUpdate();
                    break;
                case BananeManager.InventoryControlMode.Slots:
                    SlotUpdate();
                    break;
                default:
                    break;
            }
        Animate();
    }

    private float shakingAmount = 5;
    private float direction = 1;
    private float howFar = 0;
    private Transform faker;
    private RectTransform slot;
    private float inactivity = 0;
    private float inactivityCheck = 0;
    private void Animate()
    {
        if (!disableUpdate)
        {
            if (GetPercentage() < 0.5f)
            {
                currentAnimation = ShakerAnimation.Neutral;
            }
            else
            {
                currentAnimation = ShakerAnimation.Shaking;
            }
        }
        switch (currentAnimation)
        {
            case ShakerAnimation.Neutral:
                transform.localPosition = targetPosition;
                transform.rotation = targetRotation;
                howFar = 0;
                break;
            case ShakerAnimation.Shaking:
                transform.localPosition = originalPos + Vector3.right * direction * Random.Range(0,shakingAmount);
                transform.rotation = defaultRotation * Quaternion.Euler(0, 0, Random.Range(0, shakingAmount) * direction);
                if (inactivityCheck != step)
                    inactivity = 2;
                else
                    inactivity -= Time.deltaTime;
                inactivityCheck = step;
                BabblesVibration.StartConstantVibration((1 - ((GetPercentage() - 0.5f) * 2)) * inactivity.Clamp01(), true);
                if (ClemCAddons.Utilities.Timer.MinimumDelay("ShakerAnimation".GetHashCode(), 100))
                {
                    direction *= -1 * inactivityCheck.Clamp01();
                }
                if (disableUpdate)
                {
                    howFar += Time.deltaTime;
                    if(howFar > 1)
                    {
                        AudioManager.Instance?.StopUniqueSound(ESoundType.REPETITIVE2D, "S_ShakerShaking");
                        BabblesVibration.StopConstantVibration();
                        howFar = 0;
                        currentAnimation++;
                    }
                }

                if (_hasStartedShakingSound == false)
                {
                    AudioManager.Start2DSound("S_ShakerShaking");
                    _hasStartedShakingSound = true;
                }
                break;
            case ShakerAnimation.ToCenter:
                transform.parent.GetComponent<Canvas>().overrideSorting = true;
                transform.localPosition = Vector3.Lerp(originalPos, Vector3.zero, howFar);
                transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.75f, howFar);
                transform.rotation = defaultRotation * Quaternion.Euler(0, 0, howFar * 2f * 360);
                howFar += Time.deltaTime / 1.5f;

                if (howFar >= 1)
                {
                    howFar = 0;
                    currentAnimation++;
                    AudioManager.Start2DSound("S_JingleCraft");
                }
                break;
            case ShakerAnimation.AtCenter:
                howFar += Time.deltaTime;
                if(howFar > 1)
                {
                    howFar = 0;
                    currentAnimation++;
                }
                break;
            case ShakerAnimation.Opening:
                if (howFar == 0)
                {
                    AudioManager.Start2DSound("S_ShakerOuverture");
                    _bananeManager.OpenShakerFeedback(true);
                    //play a pop sound
                    var result = _bananeManager.GetCraftResult();
                    BabblesVibration.CustomVibration(1, 1f);
                    faker = Instantiate(_bananeManager.ItemFakerPrefab, transform.parent).transform;
                    faker.GetComponentInChildren<Image>().sprite = result.Sprite;
                    faker.localScale = Vector3.one * 3;
                    faker.position = transform.position;
                    Debug.Log("Needs to play a pop sound later on");
                }
                howFar += Time.deltaTime;
                if (howFar > 1)
                {
                    howFar = 0;
                    slot = _bananeManager.GetCraftedSlot().GetComponent<RectTransform>();
                    _bananeManager.CloseShakerFeedback();
                    AudioManager.Start2DSound("S_ShakerFermeture");
                    currentAnimation++;
                }
                break;
            case ShakerAnimation.Ending:
                howFar += Time.deltaTime;
                transform.localPosition = Vector3.Lerp(transform.localPosition, originalPos, howFar);
                transform.rotation = Quaternion.Lerp(transform.rotation, defaultRotation, howFar);
                transform.localScale = Vector3.Lerp(transform.localScale, Vector2.one, howFar);
                if (transform.localPosition == originalPos)
                {
                    howFar = 0;
                    currentAnimation++;
                }
                break;
            case ShakerAnimation.Sorting:
                faker.position = Vector3.Lerp(faker.position, slot.position, howFar);
                faker.localScale = Vector3.Lerp(faker.localScale, Vector3.one, howFar);
                howFar += Time.deltaTime;
                if(faker.position.ApproximatelyEqual(slot.position, 0.01f))
                {
                    Destroy(faker.gameObject);
                    howFar = 0;
                    transform.parent.GetComponent<Canvas>().overrideSorting = false;
                    _bananeManager.CraftCheck();
                    Release();
                    currentAnimation = 0;
                }
                break;
        }
    }
    private bool CheckIfDone()
    {
        if (counter >= requiredShakes)
        {
            if (_bananeManager.CraftValid())
            {
                disableUpdate = true;
                JoystickVisualizer.Hide();
            }
            else
            {
                _bananeManager.CraftCheck();
                Release();
                JoystickVisualizer.Hide();
                BabblesVibration.StopConstantVibration();
                BabblesVibration.CustomVibration(0.2f, 1);
            }
            return true;
        }
        return false;
    }
    private void Release()
    {
        Lerper.ConstantLerp(ref slide, 0, 0.3f);
        sliding = false;
        disableUpdate = false;
        counter = 0;
        target = 0;
        currentDirection = 0;
        currentIndicatorDirection = allDirections[0];
        targetPosition = originalPos;
        targetRotation = defaultRotation;
        ReleaseCancel();
    }
    private void ReleaseCancel()
    {
        cancel = 0;
        canceling = false;
        CircularProgress.SetProgress(0);
        _hasStartedShakingSound = false;
    }

    private bool CheckForCancel()
    {
        switch (_bananeManager.ControlMode)
        {
            case BananeManager.InventoryControlMode.Cursor:
                if (canceling)
                {
                    if (InputManager.GetMouseButtonUp(0) || InputManager.GetButton("UI_Cancel")) // add or controller when adapting to controller inputs
                    {
                        Release();
                        return true;
                    }
                    cancel += Time.deltaTime;
                    if (cancel > cancelDelay / 3)
                    {
                        CircularProgress.Setup(BabblesCursor.GetPosition(), new Vector2(50, 50));
                        CircularProgress.SetProgress((cancel - (cancelDelay / 3)) / (cancelDelay - (cancelDelay / 3)));
                    }
                    if (cancel >= cancelDelay)
                    {
                        BabblesVibration.StopConstantVibration();
                        Release();
                        _bananeManager.CancelCraft();
                        return true;
                    }
                }
                return false;
            case BananeManager.InventoryControlMode.Slots:
                if (canceling)
                {
                    if (cancel >= cancelDelay)
                    {
                        Release();
                        JoystickVisualizer.Hide();
                        _bananeManager.CancelCraft();
                        return true;
                    }
                    if (InputManager.GetButtonUp("UI_Cancel")) // add or controller when adapting to controller inputs
                    {
                        ReleaseCancel();
                        JoystickVisualizer.Show(transform.GetChild(0).position, true, allDirections[currentDirection]);
                        return false;
                    }
                    cancel += Time.deltaTime;
                    CircularProgress.Setup(transform.GetChild(0).position, new Vector2(50, 50));
                    CircularProgress.SetProgress(cancel / cancelDelay);
                    return true;
                }
                return false;
            default:
                return false;
        }
        
    }
    private void UpdatePosToSlide(bool smoothMode = false)
    {
        if (smoothMode)
        {
            targetRotation = Quaternion.Slerp(targetRotation, defaultRotation * Quaternion.Euler(0, 0, slide * 30), Time.deltaTime * 3);
            targetPosition = Vector3.Lerp(targetPosition, originalPos + new Vector3(slide * -20, 0), Time.deltaTime * 3);
        }
        else
        {
            transform.rotation = targetRotation = defaultRotation * Quaternion.Euler(0, 0, slide * 30);
            transform.localPosition = targetPosition = originalPos + new Vector3(slide * -20, 0);
        }
    }
    #region Cursor
    private void SlidingUpdate()
    {
        if (!sliding)
        {
            if (CheckForCancel())
                return;
            if (slide == 0)
                return;
            UpdatePosToSlide();
            return;
        }
        if (CheckIfDone())
            return;
        if (InputManager.GetMouseButtonUp(0) || InputManager.GetButton("UI_Cancel")) // add or controller when adapting to controller inputs
        {
            Release();
            return;
        }
        float preSlide = slide;
        slide += (transform.position.x - InputManager.mousePosition.x) * Time.deltaTime;
        if (slide != 0 && target == 0)
        {
            target = slide.Sign().Round();
            cancel = 0;
        }
        if (slide >= 1 && target == 1)
        {
            target = -1;
            counter++;
            cancel = 0;
        }
        if (slide <= -1 && target == -1)
        {
            target = 1;
            counter++;
            cancel = 0;
        }
        slide = slide.Clamp(-1, 1);
        UpdatePosToSlide();
        if (slide != preSlide)
            cancel = 0;
        else
        {
            cancel += Time.deltaTime;
            if (cancel > cancelDelay / 3)
            {
                CircularProgress.Setup(BabblesCursor.GetPosition(), new Vector2(50, 50));
                CircularProgress.SetProgress((cancel - (cancelDelay / 3)) / (cancelDelay - (cancelDelay / 3)));
            }
        }
        if (cancel >= cancelDelay)
        {
            Release();
            _bananeManager.CancelCraft();
        }
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (_bananeManager.CanCraft())
        {
            sliding = true;
        }
        else if (!_bananeManager.CraftEmpty())
        {
            canceling = true;
        }
    }

    public void ShakerUsed()
    {
        if (!InputManager.IsUsingController)
            return;
        if (!_bananeManager.CanCraft())
        {
            _bananeManager.PlaceRessource();
            return;
        }
        sliding = true;
    }

    #endregion Cursor

    private void SlotUpdate()
    {
        if (!sliding)
        {
            if (slide == 0)
                return;
            UpdatePosToSlide(true);
            return;
        }
        if (sliding)
        {
            if (InputManager.GetButtonDown("UI_Cancel"))
            {
                canceling = true;
                JoystickVisualizer.Hide();
            }
            if (CheckForCancel())
                return;
            Vector2 currentJoystick = new Vector2(InputManager.GetAxis("Horizontal") + InputManager.GetAxis("LookHorizontal"), InputManager.GetAxis("Vertical") + InputManager.GetAxis("LookVertical"));
            // direction of movement:
            var da = allDirections[currentDirection].Direction(allDirections[NextDirection()]);
            var db = allDirections[currentDirection].Direction(allDirections[PreviousDirection()]);
            var d2 = joystickSave.Direction(currentJoystick);
            // position related to center:
            var d3 = allDirections[currentDirection].Direction(Vector2.zero);
            var d4 = currentJoystick.Direction(Vector2.zero);
            // ensure they have the same direction (precision relative to the number of possible directions)
            // and are on the same side of the circle
            var indic = -1;
            if (currentJoystick.magnitude > 0.90)
            {
                if (Vector2.Dot(currentJoystick, previousExtremity) < - 1 + (1f / allDirections.Length))
                {
                    indic = (int)JoystickVisualizer.IndicatorTargetInfo.Both;
                    for (int i = 0; i < (allDirections.Length / 2); i++)
                    {
                        shakingType = 2;
                        step = NextStep();
                        currentDirection = NextDirection();
                        if (step == 0)
                            counter++;
                        BabblesVibration.CustomVibration(0.3f * GetShakingValue(), GetShakingValue()); //Vibration
                        var quarter = allDirections.Length / 4f;
                        if (step <= quarter)
                            slide = Mathf.Lerp(0, 1, quarter / step);
                        else if (step <= quarter * 2)
                            slide = Mathf.Lerp(1, 0, quarter / (step - quarter));
                        else if (step <= quarter * 3)
                            slide = Mathf.Lerp(0, -1, quarter / (step - quarter * 2));
                        else if (step <= quarter * 4)
                            slide = Mathf.Lerp(-1, 0, quarter / (step - quarter * 3));
                        if (CheckIfDone())
                            return;
                    }
                }
                previousExtremity = currentJoystick;
            }
            if(Vector2.Dot(da, d2) > 1-(1f / allDirections.Length) && Vector2.Dot(d3, d4) > 0)
            {
                indic = (int)JoystickVisualizer.IndicatorTargetInfo.Left;
                shakingType--;
                currentDirection = NextDirection();
                step = NextStep();
                if(step == 0 || step == allDirections.Length / 2)
                    BabblesVibration.CustomVibration(0.3f * GetShakingValue(), GetShakingValue());
                if (step == 0)
                    counter++;
                var quarter = allDirections.Length / 4f;
                if (step <= quarter)
                    slide = Mathf.Lerp(0, 1, quarter / step);
                else if (step <= quarter * 2)
                    slide = Mathf.Lerp(1, 0, quarter / (step-quarter));
                else if (step <= quarter * 3)
                    slide = Mathf.Lerp(0, -1, quarter / (step-quarter*2));
                else if (step <= quarter * 4)
                    slide = Mathf.Lerp(-1, 0, quarter / (step-quarter*3));
                if (CheckIfDone())
                    return;
            } else if (Vector2.Dot(db, d2) > 1 - (1f / allDirections.Length) && Vector2.Dot(d3, d4) > 0)
            {
                indic = (int)JoystickVisualizer.IndicatorTargetInfo.Right;
                shakingType--;
                currentDirection = PreviousDirection();
                step = PreviousStep();
                if (step == 0 || step == allDirections.Length / 2)
                    BabblesVibration.CustomVibration(0.3f * GetShakingValue(), GetShakingValue());
                if (step == 0)
                    counter++;
                var quarter = allDirections.Length / 4f;
                if (step <= quarter)
                    slide = Mathf.Lerp(0, 1, quarter / step);
                else if (step <= quarter * 2)
                    slide = Mathf.Lerp(1, 0, quarter / (step - quarter));
                else if (step <= quarter * 3)
                    slide = Mathf.Lerp(0, -1, quarter / (step - quarter * 2));
                else if (step <= quarter * 4)
                    slide = Mathf.Lerp(-1, 0, quarter / (step - quarter * 3));
                if (CheckIfDone())
                    return;
            }
            if (shakingType > 0)
            {
                indic = (int)JoystickVisualizer.IndicatorTargetInfo.Both;

                if (Vector2.Dot(currentIndicatorDirection, previousExtremity) < 0)
                {
                    currentIndicatorDirection = previousExtremity.normalized;
                }
                else
                {
                    currentIndicatorDirection = Vector3.Slerp(currentIndicatorDirection, previousExtremity.normalized, Time.deltaTime * 10).normalized;
                }
            }
            else
            {
                currentIndicatorDirection = Vector3.Slerp(currentIndicatorDirection, allDirections[currentDirection], Time.deltaTime * 10).normalized;
            }
            if(indic != -1)
            {
                if(((JoystickVisualizer.IndicatorTargetInfo)indic) != JoystickVisualizer.IndicatorTargetInfo.Both)
                {
                    JoystickVisualizer.BindFirstIndicator = true;
                    JoystickVisualizer.UpdateIndicator(Vector2.zero, (JoystickVisualizer.IndicatorTargetInfo)indic, currentIndicatorDirection);
                }
                else
                {
                    JoystickVisualizer.BindFirstIndicator = false;
                    JoystickVisualizer.UpdateIndicator(Vector2.zero, (JoystickVisualizer.IndicatorTargetInfo)indic);
                }
            }
            joystickSave = currentJoystick;
            UpdatePosToSlide(true);
        }
    }
    
    private float GetShakingValue()
    {
        var v = (GetPercentage() * 2).Min(1);
        return v;
    }

    private float GetPercentage()
    {
        return (counter + step / (float)allDirections.Length) / requiredShakes;
    }

    private int NextStep()
    {
        if (step + 1 >= allDirections.Length)
            return 0;
        return step + 1;
    }
    private int PreviousStep()
    {
        if (step == 0)
            return allDirections.Length - 1;
        return step - 1;
    }
    private int NextDirection()
    {
        if (currentDirection + 1 >= allDirections.Length)
            return 0;
        return currentDirection + 1;
    }
    private int PreviousDirection()
    {
        if (currentDirection == 0)
            return allDirections.Length - 1;
        return currentDirection - 1;
    }

    public void OnSelect(BaseEventData eventData)
    {
        slide = 0;
        sliding = true;
        JoystickVisualizer.Show(transform.GetChild(0).position, true, allDirections[0], isFirstIndicatorBinded: true);
    }
}
