using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;

public class Collapser : MonoBehaviour
{
    [SerializeField] private float _collapseDistance = 3;
    [SerializeField] private GameObject _toDestroy;
    [Header("Notification")]
    [SerializeField] private Sprite _inventoryIcon = null;
    [SerializeField] private Item _grenadeItemType;

    void Start()
    {
        var r = Physics.OverlapSphere(transform.position, _collapseDistance);
        var collapsibles = new List<CollapsibleWall>();
        foreach(var collider in r)
        {
            var find = collider.GetComponentInParent<CollapsibleWall>();
            if (find != null)
                collapsibles.Add(find);
        }
        if(collapsibles.Count == 0)
        {
            _ = ClemCAddons.Utilities.GameTools.DelayedCall(1000, Redrop);
            return;
        }
        foreach (var collapsible in collapsibles)
        {
            _ = ClemCAddons.Utilities.GameTools.DelayedCall(1000, () => { collapsible.Collapse(transform.position); EndChain(); });
        }
    }

    private void Redrop()
    {
        InventoryManager.Instance.AddItem(_grenadeItemType);
        var notificationData = new Notification.NotificationData()
        {
            Title = _grenadeItemType.CleanName,
            Subtitle = "$Resource.Subtitle",
            Description = "$Resource.Grenade.Description",
            KeyIcon = _inventoryIcon,
            ElementIcon = _grenadeItemType.Sprite,
            Background = UIManager.Instance.UIController.HUDBank.BackgroundNotification,
            ActionUponOpening = () => { UIManager.Instance.UIController.OpenInventory(true, false, _grenadeItemType.Name); }
        };
        Notification.TriggerNotification(notificationData, 0);
        GetComponent<PickupEffect>().Pickup(() => { EndChain(); });
    }

    private void EndChain()
    {
        Destroy(_toDestroy);
    }
}
