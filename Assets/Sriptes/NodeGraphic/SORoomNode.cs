using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CreateAssetMenu(fileName = "SORoomNode", menuName = "ScriptableObjects/Dungeon/SORoomNode")]
public class SORoomNode : ScriptableObject
{
    public string id;
    public List<string> parentRoomNodeIDList = new List<string>();
    public List<string> childRoomNodeIDList = new List<string>();
    public SORoomNodeGraphic roomNodeGraphic;
    public SORoomNodeType roomNodeType;
    public SORoomNodeTypeList roomNodeTypeList;
#if UNITY_EDITOR
    [HideInInspector] public Rect rect;
    public void Initialize(Rect rect, SORoomNodeGraphic roomNodeGraphic, SORoomNodeType roomNodeType)
    {
        this.rect = rect;
        this.id = System.Guid.NewGuid().ToString();
        this.name = "RoomNode";
        this.roomNodeGraphic = roomNodeGraphic;
        this.roomNodeType = roomNodeType;
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }
    public void Draw(GUIStyle nodeStyle)
    {
        GUILayout.BeginArea(rect, nodeStyle);
        EditorGUI.BeginChangeCheck();
        int selected = roomNodeTypeList.list.FindIndex(x => x == roomNodeType);
        int selection = EditorGUILayout.Popup("", selected, GetRoomNodeTypesToDisplay());
        roomNodeType = roomNodeTypeList.list[selection];
        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(this);
        }
        GUILayout.EndArea();
    }
    public string[] GetRoomNodeTypesToDisplay()
    {
        string[] roomArray = new string[roomNodeTypeList.list.Count];
        for (int i = 0; i < roomNodeTypeList.list.Count; i++)
        {
            if (roomNodeTypeList.list[i].displayInNodeGraphEditor)
            {
                roomArray[i] = roomNodeTypeList.list[i].roomNodeTypeName;
            }
        }
        return roomArray;
    }
#endif

}
