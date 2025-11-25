using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;
using ClemCAddons.Utilities;
using UnityEngine.UI;
using Luminosity.IO;
using System.Linq;
using System;

public class Camp : MonoBehaviour
{
    [SerializeField] private BezierSpline _curve;
    [SerializeField] private float _speed = 0.1f;
    [SerializeField] private int _fadeToBlackDelay = 3000;
    [SerializeField] private float _fadeToBlackSpeed = 0.5f;
    [Header("Collisions")]
    [SerializeField] private int _collisionTests = 100;
    [SerializeField] private int _collisionPasses = 100;
    [SerializeField] private LayerMask _layermask;
    [Header("UI")]
    [SerializeField] private GameObject _uiPosition;
    [SerializeField] private string _useText = "Camp";
    [Header("Dialogues")]
    [SerializeField] private CampDialogue[] _dialogues;

    private static List<Camp> _allInRange = new List<Camp>();

    private bool _lock;
    private Collider _player;

    public GameObject UIPosition { get => _uiPosition; }
    public string UseText { get => _useText; }

    private bool _alternative = false;

    [Serializable]
    public class CampDialogue
    {
        public ELevelType Level;
        public string Name;
    }

    void Start()
    {
        RotateCamp();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && _lock == false)
        {
            _allInRange.Add(this);
            InteractButtonUpdater.Instance.StartInteraction(this);
            _player = other;
            return;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            _allInRange.Remove(this);
            InteractButtonUpdater.Instance.EndInteraction(this);
            _player = null;
        }
    }

    void Update()
    {
        if (_player != null)
        {
            var closest = _allInRange[0];
            float closestDistance = _allInRange[0].transform.Distance(_player.transform);
            foreach (Camp camp in _allInRange)
            {
                if (camp.transform.Distance(_player.transform) < closestDistance)
                {
                    closest = camp;
                    closestDistance = camp.transform.Distance(_player.transform);
                }
            }
            if (closest != this)
                return;
            if (_lock == true)
                return;
            if (InputManager.GetButtonDown("Interaction") || InputManager.GetButtonDown("UI_Submit"))
            {
                _lock = true;
                InteractButtonUpdater.Instance.EndInteraction(this);
                if (_alternative)
                    StartCoroutine(StartCampAlt());
                else
                    StartCoroutine(StartCamp());
            }
        }
    }

    private async void RotateCamp()
    {
        var baseRot = _curve.transform.rotation;
        for (int i = 0; i < _collisionTests; i++)
        {
            bool nohit = true;
            for (int t = 0; t < _collisionPasses && nohit; t++)
            {
                var p1 = _curve.GetPoint(t / (_collisionPasses + 1f));
                var p2 = _curve.GetPoint((t + 1) / (_collisionPasses + 1f));
                Debug.DrawLine(p1, p2, Color.red, 100);
                if (Physics.Raycast(p1, p1.Direction(p2), p1.Distance(p2), _layermask))
                {
                    nohit = false;
                    break;
                }
            }
            if (nohit)
            {
                break;
            }
            _curve.transform.rotation = baseRot * Quaternion.Euler(0, i / (float)_collisionTests * 360, 0);
            await System.Threading.Tasks.Task.Delay(1); // just to give the computer some breathing room, as we don't need it to execute instantly.
            if(i == _collisionTests - 1) // did a 360
            {
                _alternative = true;
            }
        }
    }

    IEnumerator StartCampAlt()
    {
        AudioManager.Start2DSound("S_Campement");

        _lock = true;
        var tpsCam = FindObjectOfType<ClemCAddons.CameraAndNodes.TPSCameraWithNodeSupport>();
        var cam = transform.GetChild(0);
        tpsCam.gameObject.SetActive(false);
        cam.gameObject.SetActive(true);

        var rawImage = transform.GetComponentInChildren<RawImage>();

        float progress = 0;
        float delay = 0;

        var interestPoints = FindObjectsOfType<InterestPoint>();

        interestPoints = interestPoints.OrderBy(t => t.transform.position.Distance(transform.position)).ToArray();

        InterestPoint closestInterestPoint = null;
        if(interestPoints.Length > 0)
            closestInterestPoint = interestPoints[0];


        while (rawImage.color != Color.black)
        {
            delay += Time.deltaTime;
            progress += Time.deltaTime * _speed;
            cam.position = Vector3.Lerp(transform.position + Vector3.up * 3, transform.position + Vector3.up * 100, progress);
            if (delay * 1000 < _fadeToBlackDelay / 2f || closestInterestPoint == null)
                cam.LookAt(transform.position);
            else
                cam.rotation = Quaternion.Lerp(cam.rotation, Quaternion.LookRotation(cam.position.Direction(closestInterestPoint.transform.position)), Time.deltaTime);
            yield return new WaitForEndOfFrame();
            if (delay * 1000 > _fadeToBlackDelay)
            {
                rawImage.color = Color.black.SetA((rawImage.color.a + Time.deltaTime * _fadeToBlackSpeed).Min(1));
            }
        }
        StartCampDialogue();
        yield return new WaitForSeconds(1);
        StartCoroutine(EndCamp(rawImage, tpsCam.gameObject, cam.gameObject));
    }

    IEnumerator StartCamp()
    {
        AudioManager.Start2DSound("S_Campement");

        _lock = true;
        var tpsCam = FindObjectOfType<ClemCAddons.CameraAndNodes.TPSCameraWithNodeSupport>();
        var cam = transform.GetChild(0);
        tpsCam.gameObject.SetActive(false);
        cam.gameObject.SetActive(true);

        var rawImage = transform.GetComponentInChildren<RawImage>();

        float progress = 0;
        float delay = 0;

        while (rawImage.color != Color.black)
        {
            delay += Time.deltaTime;
            progress += Time.deltaTime * _speed;
            cam.position = _curve.GetPoint(progress);
            cam.LookAt(transform.position);
            yield return new WaitForEndOfFrame();
            if(delay * 1000 > _fadeToBlackDelay)
            {
                rawImage.color = Color.black.SetA((rawImage.color.a + Time.deltaTime * _fadeToBlackSpeed).Min(1));
            }
        }
        StartCampDialogue();
        yield return new WaitForSeconds(1);
        StartCoroutine(EndCamp(rawImage, tpsCam.gameObject, cam.gameObject));
    }

    IEnumerator EndCamp(RawImage rawImage, GameObject tpsCam, GameObject cam)
    {
        while (DialogueManager.Instance.IsInDialog)
        {
            yield return new WaitForSeconds(0.1f);
        }
        Seum.Instance.ChangeSeum(-50);
        tpsCam.SetActive(true);
        cam.GetComponent<Camera>().enabled = false;;
        float delay = 1;
        while (delay > 0)
        {
            rawImage.color = Color.black.SetA(delay);
            delay -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        cam.SetActive(false);
        cam.GetComponent<Camera>().enabled = true;
        rawImage.color = Color.black.SetA(0);
    }

    private void StartCampDialogue()
    {
        var possibleDialogues = _dialogues.Where(t => t.Level == SceneLoader.CurrentSceneElevelType).Select(t => t.Name).ToArray();
        var r = UnityEngine.Random.Range(0, possibleDialogues.Length);
        UIManager.Instance.UIController.StartDialogue(possibleDialogues[r], false);
    }
}
