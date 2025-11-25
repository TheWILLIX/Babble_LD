using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OdinSerializer;
using System;

public class NPCPositionMemory : MonoBehaviour 
{


    [SerializeField] private MemoryPosStruct[] _memoryPos;

    [Serializable]
    public struct MemoryPosStruct
    {
        public string npcName;
        public Transform npcUIPosition;
    }

    public MemoryPosStruct[] MemoryPos => _memoryPos;
 


}
