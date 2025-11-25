using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ClemCAddons;
using TMPro;
using Luminosity.IO;

public class Notification : MonoBehaviour
{
    [SerializeField] private int _instanceID;
    [SerializeField] private NotificationData _notification;
    [SerializeField] private NotificationElements _notificationElements;
    [SerializeField] private NotificationSettings _notificationSettings;
    [SerializeField] private List<NotificationData> _waitingList;


    private RectTransform _rectTransform;

    private NotificationSteps _currentStep = NotificationSteps.Ready;

    private static Notification[] _instances = new Notification[2];

    public static NotificationSteps CurrentStep(int instanceID) { return _instances[instanceID]._currentStep;}

    public enum NotificationSteps
    {
        Ready,
        Opening,
        Open,
        Closing,
        Closed
    }
    [Serializable]
    private class NotificationSettings
    {
        public Vector2 OpenPosition = new Vector2(-100, 0);
        public Vector2 ClosedPosition = new Vector2(0, 0);
        public float OpeningSpeed = 2;
        public float ClosingSpeed = 1;
        public int OpenDelayMs = 5000;
        public int ReopeningDelayMs = 500;
    }

    [Serializable]
    private class NotificationElements
    {
        public TMP_Text Title;
        public TMP_Text Subtitle;
        public TMP_Text Description;
        public Image KeyIcon;
        public Image Background;
        public Image ElementIcon;

    }

    [Serializable]
    public class NotificationData
    {
        public string Title;
        public string Subtitle;
        public string Description;
        public Sprite KeyIcon;
        public Sprite Background;
        public Sprite ElementIcon;
        public Action ActionUponOpening;

        public NotificationData(string title, string subtitle, string description, Sprite keyIcon, Sprite background, Action actionUponOpening)
        {
            Title = title;
            Subtitle = subtitle;
            Description = description;
            KeyIcon = keyIcon;
            Background = background;
            ActionUponOpening = actionUponOpening;
        }
        public NotificationData()
        {

        }
    }

    void Start()
    {
        if(_instances.Length > _instanceID)
            _instances[_instanceID] = this;
        else
        {
            Array.Resize(ref _instances, _instanceID + 1);
            _instances[_instanceID] = this;
        }
        _rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        switch (_currentStep)
        {
            case NotificationSteps.Ready:
                if (_notification == null || _notification.Title == null || _notification.Title == "")
                {
                    if (_waitingList.Count > 0)
                    {
                        _notification = _waitingList[0];
                        _waitingList.RemoveAt(0);
                        UpdateElements();
                        _currentStep++;
                    }
                } else
                {
                    _currentStep++;
                }
                break;
            case NotificationSteps.Opening:
                _rectTransform.anchoredPosition = _rectTransform.anchoredPosition.ClampedConstantInterpolation(_notificationSettings.OpenPosition, Time.deltaTime * _notificationSettings.OpenPosition.Distance(_notificationSettings.ClosedPosition) * _notificationSettings.OpeningSpeed);
                if (InputManager.GetButtonDown("MenuOpen") && _notification.ActionUponOpening != null)
                {
                    _notification.ActionUponOpening.Invoke();
                    _currentStep += 2;
                    break;
                }
                if (_rectTransform.anchoredPosition == _notificationSettings.OpenPosition)
                {
                    _currentStep++;
                }
                break;
            case NotificationSteps.Open:
                if (InputManager.GetButtonDown("MenuOpen") && _notification.ActionUponOpening != null)
                {
                    _notification.ActionUponOpening.Invoke();
                    _currentStep++;
                }
                if (ClemCAddons.Utilities.Timer.MinimumDelay("Notification".GetHashCode(), (_notificationSettings.OpenDelayMs * (1 - _waitingList.Count.Min(1) * 0.25f)).Round()) || InputManager.GetButtonDown("MenuDismiss"))
                {
                    if(_waitingList.Count > 0)
                    {
                        _notification = _waitingList[0];
                        _waitingList.RemoveAt(0);
                        Shake();
                    }
                    else
                        _currentStep++;
                }
                break;
            case NotificationSteps.Closing:
                _rectTransform.anchoredPosition = _rectTransform.anchoredPosition.ClampedConstantInterpolation(_notificationSettings.ClosedPosition, Time.deltaTime * _notificationSettings.OpenPosition.Distance(_notificationSettings.ClosedPosition) * _notificationSettings.ClosingSpeed);
                if (InputManager.GetButtonDown("MenuOpen") && _notification.ActionUponOpening != null)
                {
                    _notification.ActionUponOpening.Invoke();
                }
                if (_rectTransform.anchoredPosition == _notificationSettings.ClosedPosition)
                {
                    _notification = null;
                    _currentStep++;
                }
                break;
            case NotificationSteps.Closed:
                if (ClemCAddons.Utilities.Timer.MinimumDelay("Notification".GetHashCode(), _notificationSettings.ReopeningDelayMs))
                {
                    _currentStep = NotificationSteps.Ready;
                }
                break;
            default:
                break;
        }
    }

    private async void Shake()
    {
        var pos = _rectTransform.anchoredPosition;
        var rot = _rectTransform.rotation;
        for(int i = 0; i < 10; i++)
        {
            _rectTransform.anchoredPosition = pos + Vector2.up * UnityEngine.Random.Range(-10,10);
            _rectTransform.rotation = rot * Quaternion.Euler(0, 0, UnityEngine.Random.Range(-5, 5));
            await System.Threading.Tasks.Task.Delay(20);
        }
        _rectTransform.anchoredPosition = pos;
        _rectTransform.rotation = rot;
    }

    private void UpdateElements()
    {
        if (_notification == null) return;
        _notificationElements.Title.text = _notification.Title;
        _notificationElements.Description.text = _notification.Description;
        _notificationElements.Subtitle.text = _notification.Subtitle;
        _notificationElements.KeyIcon.sprite = _notification.KeyIcon;
        _notificationElements.Background.sprite = _notification.Background;
        _notificationElements.ElementIcon.sprite = _notification.ElementIcon;
    }

    public static void TriggerNotification(NotificationData data, int instanceID)
    {
        _instances[instanceID]._waitingList.Add(data);
    }
}
