using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ClemCAddons;

public class Exploder : MonoBehaviour
{
    [SerializeField] private float _collapseDistance = 3;
    [SerializeField] private GameObject _toDestroy;
    [Header("Notification")]
    [SerializeField] private Item _grenadeItemType;
    [Header("VFX")]
    [SerializeField] private GameObject _VFXSource;
    [SerializeField] private GameObject _VFXPoint;

    void Start()
    {
        var r = Physics.OverlapSphere(transform.position, _collapseDistance);
        ExplosionShard collapsible = null;
        float distance = 9999;
        foreach (var collider in r)
        {
            var find = collider.GetComponentInParent<ExplosionShard>();
            if (find != null && find.transform.Distance(transform) < distance)
            {
                distance = find.transform.Distance(transform);
                collapsible = find;
            }
        }
        if (collapsible == null)
        {
            _ = ClemCAddons.Utilities.GameTools.DelayedCall(1000, Redrop);
            return;
        }
        _ = ClemCAddons.Utilities.GameTools.DelayedCall(1000, () =>
         {
             if (_VFXSource != null)
             {
                 var vfx = Instantiate(_VFXSource, transform.position, Quaternion.identity);
                 _ = ClemCAddons.Utilities.GameTools.DelayedCall(100, () => Destroy(vfx));
             }
             if (_VFXPoint != null)
             {
                 var vfx1 = Instantiate(_VFXPoint, transform.position, Quaternion.identity);
                 var vfx2 = Instantiate(_VFXPoint, Vector3.Lerp(transform.position, collapsible.ExplosionSource.transform.position + Vector3.up * 2, 0.5f), Quaternion.identity);
                 var vfx3 = Instantiate(_VFXPoint, collapsible.ExplosionSource.transform.position + Vector3.up * 2, Quaternion.identity);
                 _ = ClemCAddons.Utilities.GameTools.DelayedCall(5000, () => { Destroy(vfx1); Destroy(vfx2); Destroy(vfx3); });
             }
             BabblesVibration.CustomVibration(1f, 1f);
             AudioManager.Start3DSound("S_GrenadeQuiExplose", transform, true);

             Debug.Log("Explosion");
             collapsible.ExplodeAll(transform.position);
             _ = ClemCAddons.Utilities.GameTools.DelayedCall(100, () => { EndChain(); });
         });
    }

    private void Redrop()
    {
       // AudioManager.Start2DSound("S_DepopObjet");

        InventoryManager.Instance.AddItem(_grenadeItemType);
        var notificationData = new Notification.NotificationData()
        {
            Title = _grenadeItemType.CleanName,
            Subtitle = "$Resource.Subtitle",
            Description = "$Resource.Grenade.Description",
            KeyIcon = UIManager.Instance.UIController.HUDBank.InventoryInteractionKey,
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
