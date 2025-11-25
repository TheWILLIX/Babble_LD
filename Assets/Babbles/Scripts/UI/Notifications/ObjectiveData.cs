using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ObjectiveData", menuName = "Database/Engine/ObjectiveData")]
public class ObjectiveData : ScriptableObject
{

    [SerializeField] private EQuestType _questType = EQuestType.NONE;

    [SerializeField] private string _cardName = string.Empty;

    [SerializeField] private int _stadeQuest = 0;

    [TextArea]
    [SerializeField] private string _objectiveNotifText = string.Empty;

    public EQuestType QuestType { get => _questType; }
    public string CardName { get => _cardName; }
    public int StadeQuest { get => _stadeQuest; }
    public string ObjectiveNotifText { get => _objectiveNotifText; }
}
