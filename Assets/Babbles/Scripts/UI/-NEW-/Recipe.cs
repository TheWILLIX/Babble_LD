using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Recipe", menuName = "Database/Engine/Recipe")]

public class Recipe : ScriptableObject
{
    #region Fields
    [SerializeField] private string _recipeName = string.Empty;

    [SerializeField] private Item _firstItem = null;

    [SerializeField] private Item _secondItem = null;

    [SerializeField] private Item _resultItem = null;

    [SerializeField] private Recipe _linkedRecipe = null;

    [SerializeField] private bool _locked = false;
    #endregion Fields

    #region Properties
    public string RecipeName => _recipeName;

    public Item FirstItem => _firstItem;

    public Item SecondItem => _secondItem;

    public Item ResultItem => _resultItem;

    public Recipe LinkedRecipe => _linkedRecipe;

    public bool Locked => _locked;


    #endregion Properties

}
