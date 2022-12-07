using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SORoomNodeType", menuName = "ScriptableObjects/Dungeon/SORoomNodeType")]
public class SORoomNodeType : ScriptableObject
{
    public string roomNodeTypeName;
    public bool displayInNodeGraphEditor = true;
    public bool isCorridor;
    public bool isCorridorNS;
    public bool isCorridorEW;
    public bool isEntrance;
    public bool isBossRoom;
    public bool isNone;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!HelperUtilities.ValidateCheckEmptyString(this, nameof(roomNodeTypeName), roomNodeTypeName))
        {
            Debug.Log("roomNodeTypeName is empty and must contain a value in object " + this.name.ToString());
        }
    }
    #endif
}
