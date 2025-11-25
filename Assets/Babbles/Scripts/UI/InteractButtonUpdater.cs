using ClemCAddons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractButtonUpdater : MonoBehaviour
{
    [SerializeField] GameObject[] _interactButtonPrefabs = null;
    private ResourcePickUp[] _pickups = new ResourcePickUp[0];
    private CollectiblePickUp[] _collectiblesPickups = new CollectiblePickUp[0];
    private Camp[] _campPickups = new Camp[0];
    private ElevatorButton[] _buttonPickups = new ElevatorButton[0];
    private CinemaButton[] _cinButtonPickups = new CinemaButton[0];
    private PortalButton[] _portalButtonPickups = new PortalButton[0];

    public static InteractButtonUpdater Instance;

    public void StartInteraction(ElevatorButton buttonPickup)
    {
        _buttonPickups = _buttonPickups.Add(buttonPickup);
    }
    public void StartInteraction(CinemaButton buttonPickup)
    {
        _cinButtonPickups = _cinButtonPickups.Add(buttonPickup);
    }

    public void StartInteraction(ResourcePickUp resourcePickUp)
    {
        _pickups = _pickups.Add(resourcePickUp);
    }

    public void StartInteraction(CollectiblePickUp collectiblePickup)
    {
        _collectiblesPickups = _collectiblesPickups.Add(collectiblePickup);
    }


    public void StartInteraction(PortalButton collectiblePickup)
    {
        _portalButtonPickups = _portalButtonPickups.Add(collectiblePickup);
    }


    public void EndInteraction(ResourcePickUp resourcePickUp)
    {
        _pickups = _pickups.RemoveAll(resourcePickUp);
    }


    public void EndInteraction(ElevatorButton buttonPickup)
    {
        _buttonPickups = _buttonPickups.RemoveAll(buttonPickup);
    }
    public void EndInteraction(CinemaButton buttonPickup)
    {
        _cinButtonPickups = _cinButtonPickups.RemoveAll(buttonPickup);
    }


    public void EndInteraction(CollectiblePickUp collectiblePickup)
    {
        _collectiblesPickups = _collectiblesPickups.RemoveAll(collectiblePickup);
    }

    public void EndInteraction(PortalButton collectiblePickup)
    {
        _portalButtonPickups = _portalButtonPickups.RemoveAll(collectiblePickup);
    }

    public void StartInteraction(Camp camp)
    {
        _campPickups = _campPickups.Add(camp);
    }

    public void EndInteraction(Camp camp)
    {
        _campPickups = _campPickups.RemoveAll(camp);
    }

    void Start()
    {
        Instance = this;
    }

    void Update()
    {
        while(transform.childCount < 6)
        {
            new GameObject().transform.parent = transform;
        }
        var child0 = transform.GetChild(0);
        var child1 = transform.GetChild(1);
        var child2 = transform.GetChild(2);
        var child3 = transform.GetChild(3);
        var child4 = transform.GetChild(4);
        var child5 = transform.GetChild(5);
        var length0 = _pickups.Length;
        var length1 = _campPickups.Length;
        var length2 = _collectiblesPickups.Length;
        var length3 = _buttonPickups.Length;
        var length4 = _cinButtonPickups.Length;
        var length5 = _portalButtonPickups.Length;
        if (length0 != child0.childCount)
        {
            while (child0.childCount < length0)
            {
                var r = Instantiate(_interactButtonPrefabs[0], child0);
                SetPickupData(r.transform, _pickups[child0.childCount - 1]);
            }
            for (int i = child0.childCount - 1; i >= length0; i--)
                Destroy(child0.GetChild(i).gameObject);
        }
        if (length1 != child1.childCount)
        {
            while (child1.childCount < length1)
            {
                var r = Instantiate(_interactButtonPrefabs[1], child1);
                SetPickupData(r.transform, _campPickups[child1.childCount - 1]);
            }
            for (int i = child1.childCount - 1; i >= length1; i--)
                Destroy(child1.GetChild(i).gameObject);
        }
        if (length2 != child2.childCount)
        {
            while (child2.childCount < length2)
            {
                var r = Instantiate(_interactButtonPrefabs[0], child2);
                SetPickupData(r.transform, _collectiblesPickups[child2.childCount - 1]);
            }
            for (int i = child2.childCount - 1; i >= length2; i--)
                Destroy(child2.GetChild(i).gameObject);
        }
        if (length3 != child3.childCount)
        {
            while (child3.childCount < length3)
            {
                var r = Instantiate(_interactButtonPrefabs[0], child3);
                SetPickupData(r.transform, _buttonPickups[child3.childCount - 1]);
            }
            for (int i = child3.childCount - 1; i >= length3; i--)
                Destroy(child3.GetChild(i).gameObject);
        }
        if (length4 != child4.childCount)
        {
            while (child4.childCount < length4)
            {
                var r = Instantiate(_interactButtonPrefabs[0], child4);
                SetPickupData(r.transform, _interactButtonPrefabs[child4.childCount - 1]);
            }
            for (int i = child4.childCount - 1; i >= length4; i--)
                Destroy(child4.GetChild(i).gameObject);
        }
        if (length5 != child5.childCount)
        {
            while (child5.childCount < length5)
            {
                var r = Instantiate(_interactButtonPrefabs[0], child5);
                SetPickupData(r.transform, _interactButtonPrefabs[child5.childCount - 1]);
            }
            for (int i = child5.childCount - 1; i >= length5; i--)
                Destroy(child5.GetChild(i).gameObject);
        }
        for (int i = 0; i < length0; i++)
        {
            var child = child0.GetChild(i);
            var pickup = _pickups[i];
            child.position = Camera.allCameras[0].WorldToScreenPoint(pickup.UIInteractPosition.transform.position);
        }
        for (int i = 0; i < length1; i++)
        {
            var child = child1.GetChild(i);
            var pickup = _campPickups[i];
            child.position = Camera.allCameras[0].WorldToScreenPoint(pickup.UIPosition.transform.position);
        }
        for (int i = 0; i < length2; i++)
        {
            var child = child2.GetChild(i);
            var pickup = _collectiblesPickups[i];
            child.position = Camera.allCameras[0].WorldToScreenPoint(pickup.UIPosition.transform.position);
        }
        for (int i = 0; i < length3; i++)
        {
            var child = child3.GetChild(i);
            var pickup = _buttonPickups[i];
            child.position = Camera.allCameras[0].WorldToScreenPoint(pickup.UIPosition.transform.position);
        }
        for (int i = 0; i < length4; i++)
        {
            var child = child4.GetChild(i);
            var pickup = _cinButtonPickups[i];
            child.position = Camera.allCameras[0].WorldToScreenPoint(pickup.UIPosition.transform.position);
        }
        for (int i = 0; i < length5; i++)
        {
            var child = child5.GetChild(i);
            var pickup = _portalButtonPickups[i];
            child.position = Camera.allCameras[0].WorldToScreenPoint(pickup.UIPosition.transform.position);
        }
    }
    
    private void SetPickupData<T>(Transform child, T pickup)
    {
        switch (pickup)
        {
            case ResourcePickUp resourcePickUp:
                SetAndTranslate(child.GetComponentInChildren<TMPro.TMP_Text>(), resourcePickUp.Resource.CleanName);
                child.GetComponentInChildren<Image>().sprite = UIManager.Instance.UIController.HUDBank.RessourceInteractionKey;
                break;
            case Camp campPickUp:
                SetAndTranslate(child.GetComponentInChildren<TMPro.TMP_Text>(), campPickUp.UseText);
                child.GetComponentInChildren<Image>().sprite = UIManager.Instance.UIController.HUDBank.RessourceInteractionKey;
                break;
            case CollectiblePickUp _:
                child.GetComponentInChildren<TMPro.TMP_Text>().text = "Souvenir";
                child.GetComponentInChildren<Image>().sprite = UIManager.Instance.UIController.HUDBank.RessourceInteractionKey;
                break;
            case ElevatorButton elevatorPickUp:
                SetAndTranslate(child.GetComponentInChildren<TMPro.TMP_Text>(), elevatorPickUp.Text);
                child.GetComponentInChildren<Image>().sprite = UIManager.Instance.UIController.HUDBank.RessourceInteractionKey;
                break;
            case CinemaButton cinemaPickUp:
                SetAndTranslate(child.GetComponentInChildren<TMPro.TMP_Text>(), cinemaPickUp.Text);
                child.GetComponentInChildren<Image>().sprite = UIManager.Instance.UIController.HUDBank.RessourceInteractionKey;
                break;
            case PortalButton portalPickUp:
                SetAndTranslate(child.GetComponentInChildren<TMPro.TMP_Text>(), portalPickUp.Text);
                child.GetComponentInChildren<Image>().sprite = UIManager.Instance.UIController.HUDBank.RessourceInteractionKey;
                break;
        }
    }

    private void SetAndTranslate(TMPro.TMP_Text text, string value)
    {
        if (value.StartsWith("$") || value.Contains("%%"))
        {
            text.text = Multilang.TryInstantTranslation(value);
            if(TryGetComponent<MultilangAutoNode>(out var auto))
            {
                auto.Setup(value);
            }
        }
        else
            text.text = value;
    }
}
