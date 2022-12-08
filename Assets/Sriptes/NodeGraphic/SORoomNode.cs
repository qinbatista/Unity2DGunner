using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
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
    public bool isLeftClickDragging = false;
    public bool isSelected = false;
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
        if (parentRoomNodeIDList.Count > 0 || roomNodeType.isEntrance)
        {
            EditorGUILayout.LabelField(roomNodeType.roomNodeTypeName);
        }
        else
        {

            int selected = roomNodeTypeList.list.FindIndex(x => x == roomNodeType);
            int selection = EditorGUILayout.Popup("", selected, GetRoomNodeTypesToDisplay());
            roomNodeType = roomNodeTypeList.list[selection];
            if (roomNodeTypeList.list[selected].isCorridor && !roomNodeTypeList.list[selection].isCorridor || !roomNodeTypeList.list[selected].isCorridor
             && roomNodeTypeList.list[selection].isCorridor || !roomNodeTypeList.list[selected].isBossRoom && roomNodeTypeList.list[selected].isBossRoom)
            {
                if (childRoomNodeIDList.Count > 0)
                {
                    for (int i = childRoomNodeIDList.Count - 1; i >= 0; i--)
                    {
                        SORoomNode childRoomNode = roomNodeGraphic.GetRoomNode(childRoomNodeIDList[i]);
                        if (childRoomNode != null)
                        {
                            RemoveChildRoomNodeIDFromRoomNode(childRoomNode.id);
                            childRoomNode.RemoveParentRoomNodeIDFromRoomNode(id);
                        }
                    }
                }
            }
        }
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
    public void ProcessEvents(Event currentEvent)
    {
        switch (currentEvent.type)
        {
            case EventType.MouseDown:
                ProcessMouseDownEvent(currentEvent);
                break;
            case EventType.MouseUp:
                ProcessMouseUpEvent(currentEvent);
                break;
            case EventType.MouseDrag:
                ProcessMouseDragEvent(currentEvent);
                break;
        }
    }


    private void ProcessMouseDownEvent(Event currentEvent)
    {
        if (currentEvent.button == 0)
        {
            ProcessLeftClickMouseDownEvent(currentEvent);
        }
        else if (currentEvent.button == 1)
        {
            ProcessRightClickMouseDownEvent(currentEvent);
        }
    }

    private void ProcessRightClickMouseDownEvent(Event currentEvent)
    {
        roomNodeGraphic.SetNodeToDrawConnectionLineFrom(this, currentEvent.mousePosition);
    }

    private void ProcessLeftClickMouseDownEvent(Event currentEvent)
    {
        isSelected = !isSelected;
    }
    private void ProcessMouseUpEvent(Event currentEvent)
    {
        if (currentEvent.button == 0)
        {
            ProcessLeftClickMouseUpEvent(currentEvent);
        }
    }

    private void ProcessLeftClickMouseUpEvent(Event currentEvent)
    {
        if (isLeftClickDragging == true)
        {
            isLeftClickDragging = false;
        }
    }
    private void ProcessMouseDragEvent(Event currentEvent)
    {
        if (currentEvent.button == 0)
        {
            ProcessLeftClickMouseDragEvent(currentEvent);
        }
    }

    private void ProcessLeftClickMouseDragEvent(Event currentEvent)
    {
        isLeftClickDragging = true;
        DragNode(currentEvent.delta);
        GUI.changed = true;
    }

    public void DragNode(Vector2 delta)
    {
        rect.position += delta;
        EditorUtility.SetDirty(this);
    }
    public bool AddChildRoomNodeIDToRoomNode(string childID)
    {
        if (isChildRoomValid(childID))
        {
            childRoomNodeIDList.Add(childID);
            return true;
        }
        return false;
    }

    private bool isChildRoomValid(string childID)
    {
        bool isConnectedBossNodeAlready = false;
        foreach (SORoomNode roomNode in roomNodeGraphic._roomNodeList)
        {
            if (roomNode.roomNodeType.isBossRoom && roomNode.parentRoomNodeIDList.Count > 0)
            {
                isConnectedBossNodeAlready = true;
            }
        }
        // if (roomNodeGraphic.GetRoomNode(childID).roomNodeType.isBossRoom && isConnectedBossNodeAlready)
        // {
        //     return false;
        // }
        // if (roomNodeGraphic.GetRoomNode(childID).roomNodeType.isNone)
        // {
        //     return false;
        // }
        // if (childRoomNodeIDList.Contains(childID))
        // {
        //     return false;
        // }
        // if (id == childID)
        // {
        //     return false;
        // }
        // if (parentRoomNodeIDList.Contains(childID))
        // {
        //     return false;
        // }
        // if (roomNodeGraphic.GetRoomNode(childID).parentRoomNodeIDList.Count > 0)
        // {
        //     return false;
        // }
        // if (roomNodeGraphic.GetRoomNode(childID).roomNodeType.isCorridor && roomNodeType.isCorridor)
        // {
        //     return false;
        // }
        // if (roomNodeGraphic.GetRoomNode(childID).roomNodeType.isCorridor && !roomNodeType.isCorridor)
        // {
        //     return false;
        // }
        // if (roomNodeGraphic.GetRoomNode(childID).roomNodeType.isCorridor && childRoomNodeIDList.Count>=Settings.maxChildCorridors)
        // {
        //     return false;
        // }
        // if (roomNodeGraphic.GetRoomNode(childID).roomNodeType.isEntrance)
        // {
        //     return false;
        // }
        // if (roomNodeGraphic.GetRoomNode(childID).roomNodeType.isCorridor && childRoomNodeIDList.Count>0)
        // {
        //     return false;
        // }
        return true;
    }

    public bool AddParentRoomNodeIDToRoomNode(string parentID)
    {
        parentRoomNodeIDList.Add(parentID);
        return true;
    }
    public bool RemoveChildRoomNodeIDFromRoomNode(string childID)
    {
        if (childRoomNodeIDList.Contains(childID))
        {
            childRoomNodeIDList.Remove(childID);
            return true;
        }
        return false;
    }
    public bool RemoveParentRoomNodeIDFromRoomNode(string parentID)
    {
        if (parentRoomNodeIDList.Contains(parentID))
        {
            parentRoomNodeIDList.Remove(parentID);
            return true;
        }
        return false;
    }
#endif

}
