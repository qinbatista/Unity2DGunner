using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SORoomNodeTypeList", menuName = "ScriptableObjects/Dungeon/SORoomNodeTypeList")]
public class SORoomNodeTypeList: ScriptableObject
{
    #region  Header ROOM NODE TYPE LIST
    [Space(10)]
    [Header("FROM NODE TYPE LIST")]
    #endregion
    #region  Tooltip
    [Tooltip("This list should be populated with all the RoomNodeTypeSO's that are used in the game. This list is used to populate the RoomNodeType dropdown in the RoomNodeEditor.")]
    public List<SORoomNodeType> list;
    #endregion

    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(list), list);
    }
}