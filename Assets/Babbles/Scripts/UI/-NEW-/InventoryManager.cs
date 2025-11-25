using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luminosity.IO;
using ClemCAddons.Utilities;
using ClemTimer = ClemCAddons.Utilities.Timer;
using ClemCAddons;
using UnityEditor;
using UnityEngine.Serialization;
using System;
using System.Linq;

public class InventoryManager : Singleton<InventoryManager>
{
    #region Fields


    [SerializeField] private Item[] _itemsDB = null;
    [SerializeField] private Recipe[] _recipesDB = null;
    [SerializeField] private List<int> _defaultInventory = new List<int>();
    


    private Dictionary<string, int> _itemData = null;
    private Dictionary<string, bool> _itemDiscovered = null;

    private Dictionary<ELevelType, int> _collectibleDictionary = null;


    private List<Item> _ressourceList = null;
    private List<Item> _objectList = null; 

    private Dictionary<string, bool> _recipeLocked = null; //Useless ??? (ItemDiscovered used instead)

    private Transform _player = null;

    private Item[] _quickUseList = new Item[4];

    [SerializeField,FormerlySerializedAs("_usageContaineer")] private Transform _usageContainer;

    #endregion Fields

    #region Properties
    public Item[] ItemDB => _itemsDB;
    public Recipe[] RecipeDB => _recipesDB;

    public Dictionary<ELevelType, int> CollectibleDictionary { get => _collectibleDictionary; set => _collectibleDictionary = value; }

    public List<Item> RessourceList => _ressourceList; //USELESS ?
    public Dictionary<string, bool> RecipeLocked
    {
        get
        {
          return  _recipeLocked;
        }
    }

    public Transform Player {
        get
        {
            if (_player == null)
            {
                Debug.LogWarning("Lost player reference, finding player in scene");
                _player = FindObjectOfType<ClemCAddons.Player.CharacterMovement>().transform;
            } // if the reference is dropped for some reason, find it again
            return _player;
        }
        set => _player = value; }

    public Item[] QuickUseItems { get => _quickUseList; set => _quickUseList = value; }
    #endregion Properties


    #region Methods
    new void Awake()
    {
        _itemData = new Dictionary<string, int>();
        _itemDiscovered = new Dictionary<string, bool>();

        _ressourceList = new List<Item>();
        _collectibleDictionary = new Dictionary<ELevelType, int>();

        _recipeLocked = new Dictionary<string, bool>();

        _quickUseList[0] = Array.Find(_itemsDB, t => t.Name == "Algue");
        _quickUseList[1] = Array.Find(_itemsDB, t => t.Name == "Fruit");
        _quickUseList[2] = Array.Find(_itemsDB, t => t.Name == "Meduse");
        _quickUseList[3] = Array.Find(_itemsDB, t => t.Name == "Poulpe");

        if (_itemsDB.Length >= 1)
        {
            for (int i = 0; i < _itemsDB.Length; i++)
            {
#if(UNITY_EDITOR)
                _itemData.Add(_itemsDB[i].Name, _itemsDB[i].DefaultQuantity);   //WE CREATE TWO DICTIONNARY TO BE ABLE TO STORE DATA ON THOSE BECAUSE MODIFYING THE SCRIPTABLE OBJECT IS NOT POSSIBLE
                _itemDiscovered.Add(_itemsDB[i].Name, _itemsDB[i].IsDiscoveredByDefault);
# endif
#if(!UNITY_EDITOR)
                _itemData.Add(_itemsDB[i].Name, 0);
                _itemDiscovered.Add(_itemsDB[i].Name, _itemsDB[i].IsDiscoveredByDefault);
#endif


                if (_itemsDB[i].IsRessource == true) //Si l'item est une ressource on l'ajoute à une deuxième liste (Sert pour de l'optimisation de code)
                {
                    _ressourceList.Add(_itemsDB[i]);
                }
                else
                {
                    //Il y avais une liste uniquement d'object craftés mais pas vraiment nécessaire je crois
                }
            }
        }

        if(_recipesDB.Length >= 1)
        {
            for (int i = 0; i < _recipesDB.Length; i++)
            {
                if(_recipesDB[i] == null)
                {
                    Debug.LogError("Missing recipe " + i);
                    continue;
                }
                _recipeLocked.Add(_recipesDB[i].name, _recipesDB[i].Locked);
            }
        }

        if(Player == null)
        {
            Debug.LogWarning("Missing player reference, finding player in scene");
            _player = FindObjectOfType<ClemCAddons.Player.CharacterMovement>().transform;
        }
    }

    public void AddItem(Item item, int number = 1)
    {
        if (item == null)
            return;
        _itemData[item.Name] = InventoryManager.Instance.GetItemData(item) + number;
    }

    public void RemoveItem(Item item, int number = 1)
    {
        _itemData[item.Name] = InventoryManager.Instance.GetItemData(item) - number;
    }

    #region Recipes
    public void UnlockRecipeByString(string recipeName) //FUNCTION NOT WORKING ??????????!!!!!!!
    {
        _recipeLocked[recipeName] = false;

        for (int i = 0; i < _recipesDB.Length; i++)
        {
            if(_recipesDB[i].RecipeName == recipeName)
            {
                _recipeLocked[_recipesDB[i].LinkedRecipe.name] = false;
                Debug.Log("Recipe Unlocked Successfully");
                SetItemDiscovered(_recipesDB[i].ResultItem, true); //Result Item is also discovered
                RecipesUnlocked.Instance.Unlock(_recipesDB[i]);
                Paging.Instance.AddCard(recipeName, "Recipes");
                AudioManager.Start2DSound("S_RecetteDebloquee"); //Audio For Unlocking Recipe

                return;
            }
        }
    }

    public void UnlockRecipeByRecipe(Recipe recipe)
    {
        try
        {
            _recipeLocked[recipe.name] = false;
            _recipeLocked[recipe.LinkedRecipe.name] = false;
            SetItemDiscovered(recipe.ResultItem, true); //Result Item is also discovered
            RecipesUnlocked.Instance.Unlock(recipe);
            Debug.Log("Recipe Unlocked Successfully");
            Paging.Instance.AddCard(recipe.ResultItem.Name, "Recipes");
            AudioManager.Start2DSound("S_RecetteDebloquee"); //Audio For Unlocking Recipe
        }
        catch
        {
            Debug.Log("Recipe Unlocked Failed");
            return;
        }
    }

    #endregion Recipes

    #region Ressources Discovered
    public bool isItemDiscovered(Item ressource)
    {
        bool t = _itemDiscovered.TryGetValue(ressource.Name, out var r);
        if (!t) // if missing in dictionary
        {
            Debug.LogError("Missing " + ressource.Name + " in ItemDiscovered: " + _itemDiscovered);
            return false;
        }
        return r;
    }

    public void SetItemDiscovered(Item ressource, bool value)
    {
        bool alreadyDiscovered = _itemDiscovered[ressource.Name];
        if (alreadyDiscovered)
            return;
        _itemDiscovered[ressource.Name] = value;
        if(ressource.IsRessource == true)
        {
            Paging.Instance.AddCard(ressource.Name + "Card", "Resources");
        }
        else
        {
            Paging.Instance.AddCard(ressource.Name, "Recipes");
            RecipesUnlocked.Instance.Unlock(ressource);
        }
    }

    #endregion Ressources Discovered


    #region GetDatas

    public int GetItemData(Item item)
    {
        bool t = _itemData.TryGetValue(item.Name, out var r);
        if (!t) // if missing in dictionary
        {
            Debug.LogError("Missing " + item.Name + " in ItemData: " + _itemData);
            return -1;
        }
        return r;
    }

    public Item GetItemDataByString(string itemName)
    {
        foreach (Item item in _itemsDB)
        {
            if (item.Name == itemName)
            {
                return item;
            }
        }

        Debug.Log(itemName + " is impossible to find, are you sure this is the correct item name ?");
        return null;
    }
    #endregion GetDatas



    #region Use Item
    [Obsolete("UseItem is part the legacy system that was kept for backwards compatibility reasons. Any use of it should be replaced by TryUseItem and associated systems.")]
    public void UseItem(Item itemUsed)
    {
        Debug.LogError("This is the legacy system that was kept for backwards compatibility reasons. Any use of it should be replaced by TryUseItem and associated systems");

        // Récupère l'item de l'objet/ressource qu'on utilisé et qu'on a drag vers l'extérieur

        switch (itemUsed.UsageType)        //APPEL DE LA FONCTION DE L'ITEM

        {
            case EItemUsageType.PLACEMENT:
                InventoryManager.Instance.PlaceItem(itemUsed);
                break;

            case EItemUsageType.PARFUME:
                InventoryManager.Instance.Perfume(itemUsed);
                break;

            case EItemUsageType.LIGHT:
                InventoryManager.Instance.Light(itemUsed);
                break;

            case EItemUsageType.HEAL:
                InventoryManager.Instance.HealSeum(itemUsed);
                break;

            case EItemUsageType.FAILEFFECT:
                InventoryManager.Instance.FailEffect(itemUsed);
                break;

            case EItemUsageType.OTHERS:
                InventoryManager.Instance.OtherUsage(itemUsed);
                break;

            case EItemUsageType.UNUSABLE:
                Debug.LogError("Can't refund with this system.");
                break;

            case EItemUsageType.NONE:
                Debug.LogWarning("Item usage type is None. If the item should be unusable, use \"Unusable\" instead");
                break;

            default:
                Debug.LogError("Item usage type is Invalid");
                break;
        }

        BananeManager.ItemSelected = null; 


    }

    public bool TryUseItem(Item item)
    {
        if(item == null)
        {
            Debug.LogError("Item is null");
            return true; // so it doesn't try to refund
        }
        switch (item.UsageType)
        {
            case EItemUsageType.PLACEMENT:
                InventoryManager.Instance.PlaceItem(item);
                UIManager.Instance.UIController.CloseInventory(true);
                break;

            case EItemUsageType.PARFUME:
                if(item.LinkedObject == null)
                {
                    Debug.LogError("Missing linked object (parfume) in item, refunding...");
                    return false;
                }
                AudioManager.Start2DSound("S_UtilisationParfum");
                InventoryManager.Instance.Perfume(item);
                UIManager.Instance.UIController.CloseInventory(true);
                break;

            case EItemUsageType.LIGHT:
                if (item.LinkedObject == null)
                {
                    Debug.LogError("Missing linked object (light) in item, refunding...");
                    return false;
                }
                AudioManager.Start2DSound("S_UtilisationLumiere");
                Instance.Light(item);
                UIManager.Instance.UIController.CloseInventory(true);
                break;

            case EItemUsageType.HEAL:
                UIManager.Instance.UIController.CloseInventory(true);
                _ = GameTools.DelayedCall(500, () =>
                {
                    HealSeum(item);
                    AudioManager.Start2DSound("S_BulleMangeSushis");
                    FreshBulleAnimation.StartHandAnimation(FreshBulleAnimation.HandType.Eating);
                });
                break;

            case EItemUsageType.WEARABLE:
                if (item.LinkedObject == null)
                {
                    Debug.LogError("Missing linked object (light) in item, refunding...");
                    return false;
                }
                AudioManager.Start2DSound("S_BulleLook");
                InventoryManager.Instance.WearItem(item);
                UIManager.Instance.UIController.CloseInventory(true);
                break;

            case EItemUsageType.FAILEFFECT:
                InventoryManager.Instance.FailEffect(item);
                break;

            case EItemUsageType.OTHERS:
                InventoryManager.Instance.OtherUsage(item);
                break;

            case EItemUsageType.UNUSABLE:
                return false; // tell caller to refund

            case EItemUsageType.DIALOGUE:
                UIManager.Instance.UIController.StartDialogue(item.CleanName + "Usage", true);
                break;

            case EItemUsageType.NONE:
                Debug.LogWarning("Item usage type is None, will be consumed. If the item should be unusable, use \"Unusable\" instead");
                break;

            default:
                Debug.LogError("Item usage type is Invalid, will be refunded");
                return false;
        }
        return true;
    }
    #endregion Use Item


    #region Collectible
    public int AddCollectible(ELevelType levelType)
    {
        var r = _collectibleDictionary.FirstOrDefault(t => t.Key == levelType).Value + 1;
        _collectibleDictionary.AddOrReplace(levelType, r);
        return _collectibleDictionary.Values.Sum();
    }
    #endregion Collectible


    //FONCTION DES DIFFÉRENTS ITEMS ?
    #region Items Usage Methods
    public void PlaceItem(Item item)
    {
        //Linked Object is the prefab to instantiate
        //Linked Value is the distance of the object from Bulle (Huge difference of size between 'MéduseCoussin' and 'FleurBrulante' for exemple)
        GameObject prefabToInstantiate = item.LinkedObject;
        float distance = item.LinkedValue;

        Vector3 basePos = Player.position + (Player.forward * distance);
        GameObject spawnedItem = Instantiate(prefabToInstantiate, basePos, Quaternion.identity, _usageContainer);
        spawnedItem.GetComponentInChildren<ItemPlacer>().ItemType = item;

    }

    public void WearItem(Item item)
    {
        AudioManager.Start2DSound("S_BulleLook");

        var wearable = Instantiate(item.LinkedObject, Player.position, Quaternion.identity);
        wearable.GetComponent<Wearable>().Initialize(Player, item.LinkedValue);
        VFXSpawner.Spawn("Skin", _player.transform, _player.transform.position);
    }

    public void HealSeum(Item item)
    {
        AudioManager.Start2DSound("S_BulleMangeSushis");

        Seum.Instance.ChangeSeum(-item.LinkedValue.Abs());
        // no matter whether the seum is negative or positive, should be assumed it is meant to lower its value
        Debug.Log("Healing Seum, missing feedback");
        //Linked Object could eventually be used for a visual feedback
        //Linked Value is the value of how much Seum is being lowered (The healing value)

        VFXSpawner.Spawn("Manger", UIManager.Instance.UIController.Character.transform.position);
    }

    private GameObject _currentLight;
    // reference to the current spawned light, if any

    public void Light(Item item)
    {

        AudioManager.Start2DSound("S_UtilisationLumiere");

        ClearLight(); // just in case, as effects cannot and shouldn't overlap
        _currentLight = Instantiate(item.LinkedObject, Player.position, Quaternion.identity);
        if (_currentLight.TryGetComponent<Wearable>(out var wearable))
        {
            wearable.Initialize(Player, item.LinkedValue);
        }
        else
        {
            _ = GameTools.DelayedCall((item.LinkedValue * 1000).Round(), ClearLight);
        }

        //Linked Object is the object emitting the light instantiated next to Bulle
        //Linked Value is the timer, the time the light will last (Value highly change between a simple use of a 'Méduse' and the precious 'Ampoule de Fleur' 
    }

    private void ClearLight()
    {
        if (_currentLight != null)
        {
            Destroy(_currentLight);
            _currentLight = null;
        }
    }
    
    private GameObject _currentPerfume;
    // reference to the current spawned perfume, if any

    public void Perfume(Item item)
    {
        AudioManager.Start2DSound("S_UtilisationParfum");

        ClearPerfume();
        _currentPerfume = Instantiate(item.LinkedObject, Player.position, Quaternion.identity);
        if (_currentPerfume.TryGetComponent<VFXPerfume>(out var perfume))
        {
            switch (item.Name)
            {
                case "Algue":
                    perfume.Type = VFXPerfume.PerfumeType.Algae;
                    break;
                case "ParfumAloa":
                    perfume.Type = VFXPerfume.PerfumeType.Vanilla;
                    break;
                case "ParfumFruite":
                    perfume.Type = VFXPerfume.PerfumeType.Fruit;
                    break;
            }
        }

        if (_currentPerfume.TryGetComponent<Wearable>(out var wearable))
        {
            wearable.Initialize(Player, item.LinkedValue);
        }
        else
        {
            _ = GameTools.DelayedCall((item.LinkedValue * 1000).Round(), ClearPerfume);
        }

        //Linked Object is the visual feedback (probably a VFX) of the parfume
        //Linked Value is the timer, the time the effect of the parfume will last
    }

    private void ClearPerfume()
    {

        if (_currentPerfume != null)
        {
            Destroy(_currentPerfume);
            _currentPerfume = null;
        }
    }

    public void FailEffect(Item item)
    {
        if(item.LinkedObject == null)
        {
            UIManager.Instance.UIController.CloseInventory(true);
            UIManager.Instance.UIController.StartDialogue("Failed_" + item.Name, false);
            return;
        }

        Instance.PlaceItem(item);
        UIManager.Instance.UIController.CloseInventory(true);
        //Linked Object is the visual feedback of the fail item
        //Linked Value can be usefull depending on the situation, up to you
    }

    public void OtherUsage(Item item) //In case none of the above Methods suit the item you are trying to create use the OtherUsage function here
    {
        GameObject prefabToInstantiate = item.LinkedObject;
        float value = item.LinkedValue;

        switch (item.Name)
        {
            case "BrassartSlide":
                //We Start the Brassart Slide Animation
                
                break;


            case "Campement":
                //If we do this one it will be complex and be unique so no need to create his own function
                break;
            
        }
    }

    #endregion Items Usage Methods

    #endregion Methods


}

#if(UNITY_EDITOR)
[CustomEditor(typeof(InventoryManager))]
public class InventoryManagerInspector : Editor
{
    private InventoryManager manager;
    private int page = 0;
    private bool showPageMenu;
    private string[] titles = { "Inventory", "Config", "Temporary values" };
    private List<Item> items = new List<Item>();
    private List<Recipe> recipes = new List<Recipe>();

    public override void OnInspectorGUI()
    {
        manager = target as InventoryManager;
        var sd = new SerializedObject(manager);

        if (items.Count == 0)
            items.AddRange(manager.ItemDB);
        if(recipes.Count == 0)
            recipes.AddRange(manager.RecipeDB);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("<<"))
        {
            page = (page-1).Max(0);
            showPageMenu = false;
        }
        if (GUILayout.Button("Page: " + page.ToString()))
        {
            showPageMenu = !showPageMenu;
        }
        if (GUILayout.Button(">>"))
        {
           page = (page+1).Min(titles.Length-1);
        }
        EditorGUILayout.EndHorizontal();

        if (showPageMenu)
        {
            for(int i = 0; i < titles.Length;)
            {
                EditorGUILayout.BeginHorizontal();
                for (int j = 0; j < Mathf.CeilToInt(titles.Length / Mathf.Ceil(titles.Length / 2f)); j++, i++)
                {
                    if (i >= titles.Length)
                        break;
                    if (GUILayout.Button((i).ToString()))
                    {
                        page = i;
                        showPageMenu = false;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        EditorGUILayout.LabelField(new GUIContent(titles[page]), new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, fontSize = 20, alignment = TextAnchor.UpperCenter, clipping = TextClipping.Overflow});
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        switch (page)
        {
            case 0:
                for(int i = 0; i < items.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(items[i].Name);
                    var r = EditorGUILayout.IntField(items[i].DefaultQuantity);
                    if (r != items[i].DefaultQuantity)
                    {
                        manager.ItemDB[i].DefaultQuantity = r;
                        items[i].DefaultQuantity = r;
                    }
                    EditorGUILayout.EndHorizontal();
                }
                break;
            case 1:
                EditorGUILayout.PropertyField(sd.FindProperty("_itemsDB"));
                EditorGUILayout.PropertyField(sd.FindProperty("_recipesDB"));
                break;
            case 2:
                EditorGUILayout.PropertyField(sd.FindProperty("_usageContainer"));
                break;
            default:
                break;
        }
        sd.Update();
    }
}
#endif