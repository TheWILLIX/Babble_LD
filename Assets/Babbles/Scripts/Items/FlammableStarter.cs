using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;
using System.Linq;

public class FlammableStarter : MonoBehaviour
{
    [SerializeField] private float _flamingDistance = 1;
    [SerializeField] private GameObject _toDestroy;
    [Header("Notification")]
    [SerializeField] private Sprite _inventoryIcon = null;
    [SerializeField] private Item _flammableItemType;
    [Header("VFX")]
    [SerializeField] private GameObject _VFX;

    private static FlammableStarter _instance;
    public static FlammableStarter Instance { get => _instance;}

    void Start()
    {
        _instance = this;
        var r = Physics.OverlapSphere(transform.position, _flamingDistance);
        var flammables = new List<FlammableWallBuilder>();
        foreach (var collidingElement in r)
        {
            if (collidingElement.TryGetComponent<FlammableWallBuilder>(out var t))
                flammables.Add(t);
        }
        if(flammables.Count == 0)
        {
            _ = ClemCAddons.Utilities.GameTools.DelayedCall(1000, Redrop);
            return;
        }
        flammables.Sort(new FlammableWallComparer());
        // couldn't find a way to avoid that and only get a single result
        flammables[0].SetAblaze();
        AudioManager.Start3DSound("S_FleurBrulanteSurLeMur " + Random.Range(1, 3),GetComponent<AudioSource>());
        Instantiate(_VFX, transform.position, Quaternion.identity, transform);
        _ = ClemCAddons.Utilities.GameTools.DelayedCall(1000, EndChain);
    }
    private void Redrop()
    {
        InventoryManager.Instance.AddItem(_flammableItemType);
        var notificationData = new Notification.NotificationData()
        {
            Title = _flammableItemType.CleanName,
            Subtitle = "$Resource.Subtitle",
            Description = "$Resource.Fleur.Description",
            KeyIcon = _inventoryIcon,
            ElementIcon = _flammableItemType.Sprite,
            Background = UIManager.Instance.UIController.HUDBank.BackgroundNotification,
            ActionUponOpening = () => { UIManager.Instance.UIController.OpenInventory(true, false, _flammableItemType.Name); }
        };
        Notification.TriggerNotification(notificationData, 0);
        GetComponent<PickupEffect>().Pickup(() => { EndChain(); });
    }
    private void EndChain()
    {
        Destroy(_toDestroy);
    }
}

public class FlammableWallComparer : IComparer<FlammableWallBuilder>
{
    public int Compare(FlammableWallBuilder x, FlammableWallBuilder y)
    {
        return x.transform.position.Distance(FlammableStarter.Instance.transform.position).CompareTo(y.transform.position.Distance(FlammableStarter.Instance.transform.position));
    }
}