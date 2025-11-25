using ClemCAddons.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectifsUpdate : MonoBehaviour
{


    #region Fields
    [SerializeField] private ObjectiveData[] _objDatas = null;

    private Dictionary<string, ObjectiveData> _questDatas = null;

    private List<ObjectiveData> _questFoundList = null;
    #endregion Fields
    #region Properties
    #endregion Properties
    #region Methods

    void Start()
    {
        _questDatas = new Dictionary<string, ObjectiveData>();
        _questFoundList = new List<ObjectiveData>();


        if (_objDatas.Length >= 1)    //CHECK IF ANY OBJ EXIST
        {
            for (int i = 0; i < _objDatas.Length; i++)    //SETUP THE DICTIONARY CORRECTLY
            {
                _questDatas.Add(_objDatas[i].QuestType.ToString() + _objDatas[i].StadeQuest, _objDatas[i]);
            }
        }
    }


    public void NotificationObjective(EQuestType questType, int questStade)
    {
        //try
        //{
            /*foreach (ObjectiveData quest in _questFoundList)
            {
                Debug.Log(quest.CardName);
            }*/


            if (_questDatas[questType.ToString() + questStade.ToString()] == null) //We Check if this quest exist, if not it's probably not correctly configured
            {
                Debug.LogError("Quest Data Not Found");
                return;
            }
            else
            {

                ObjectiveData objData = _questDatas[questType.ToString() + questStade.ToString()]; //We get the ObjectiveData

                string cardName = objData.CardName;


                string oldCardName = "Objective." + questType.ToString() + (questStade-1).ToString();

                bool replaceOldCard = false;



                foreach (ObjectiveData objectives in _questFoundList)
                {
                    if (cardName == objectives.CardName)
                    {
                        Debug.LogError("The Quest : " + cardName + " you are trying to add has already been found/completed");
                        return;
                    }

                    if (oldCardName == objectives.CardName)
                    {
                        Debug.Log("We found a older Quest : " + oldCardName);
                        replaceOldCard = true;
                    } 
                }


                Debug.Log(objData.CardName + " Popup should appear");


                if (replaceOldCard == true)
                {
                    UIManager.Instance.UIController.Paging.ModifyCard(oldCardName , cardName, "Objectives");
                    Debug.Log("Card Replace (Old) : " + cardName);

                }
                else 
                {
                    UIManager.Instance.UIController.Paging.AddCard(cardName, "Objectives"); //We add the new card
                    Debug.Log("Card Added : " + cardName);
                }



                var notificationData = new Notification.NotificationData()
                {
                    Subtitle = objData.ObjectiveNotifText,
                    Description = "$Carnet.MoreDetails",
                    KeyIcon = UIManager.Instance.UIController.HUDBank.NotebookKey,
                    Background = UIManager.Instance.UIController.HUDBank.BackgroundObjectifUpdate,
                    ActionUponOpening = () =>
                    {
                        _ = GameTools.DelayedCall(10, () =>
                        {
                            UIManager.Instance.UIController.BananeManager.ShowNotebookByCard(cardName);
                        });
                    }
                };
                Notification.TriggerNotification(notificationData, 2);


                _questFoundList.Add(objData); //We Add the quest to the already Discovered Quests

            }
       /* }
        catch
        {
            Debug.LogError("Error in NotificationObjective");
        }*/

      
    }




    public void NotificationObjectiveWithoutCard(EQuestType questType, int questStade)
    {

        if (_questDatas[questType.ToString() + questStade.ToString()] == null) //We Check if this quest exist, if not it's probably not correctly configured
        {
            Debug.LogError("Quest Data Not Found");
            return;
        }
        else
        {
            ObjectiveData objData = _questDatas[questType.ToString() + questStade.ToString()]; //We get the ObjectiveData

            string cardName = objData.CardName;



            var notificationData = new Notification.NotificationData()
            {
                Subtitle = objData.ObjectiveNotifText,
                Description = "$Carnet.MoreDetails",
                KeyIcon = UIManager.Instance.UIController.HUDBank.NotebookKey,
                Background = UIManager.Instance.UIController.HUDBank.BackgroundObjectifUpdate,
                ActionUponOpening = () =>
                {
                    _ = GameTools.DelayedCall(10, () =>
                    {
                        UIManager.Instance.UIController.BananeManager.ShowNotebookByCard(cardName);
                    });
                }
            };
            Notification.TriggerNotification(notificationData, 2);
        }


    }





    public void UpdateCard(EQuestType questType, int questStade)
    {
        Debug.Log("Should only trigger once !!");

        if (_questDatas[questType.ToString() + questStade.ToString()] == null)
        {
            Debug.LogError("Quest Data Not Found");
            return;
        }
        else
        {
            ObjectiveData objData = _questDatas[questType.ToString() + questStade.ToString()];

            string cardName = objData.CardName;

            string oldCardName = "Objective." + questType.ToString() + (questStade - 1).ToString();

            bool replaceOldCard = false;


            foreach (ObjectiveData objectives in _questFoundList)
            {
                if (cardName == objectives.CardName)
                {
                    Debug.LogError("The Quest : " + cardName + " you are trying to add has already been found/completed");
                    return;
                }

                if (oldCardName == objectives.CardName)
                {
                    Debug.Log("We found a older Quest : " + oldCardName);
                    replaceOldCard = true;
                }
            }

            if (replaceOldCard == true)
            {
                UIManager.Instance.UIController.Paging.ModifyCard(oldCardName, cardName, "Objectives");
                Debug.Log("Card Replace (Old) : " + cardName);

            }
            else
            {
                UIManager.Instance.UIController.Paging.AddCard(cardName, "Objectives"); //We add the new card
                Debug.Log("Card Added : " + cardName);
            }

            _questFoundList.Add(objData); //We Add the quest to the already Discovered Quests


        }



    }


  
    #endregion Methods
}
