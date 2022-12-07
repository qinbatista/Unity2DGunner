using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System;

public class RoomNodeGraphicEditor : EditorWindow
{
    static SORoomNodeGraphic _soRoomNodeGraphic;
    static SORoomNodeTypeList _soRoomNodeTypeList;
    GUIStyle roomNodeStyle;
    SORoomNode currentRoomNode;
    const float nodeWidth = 160f;
    const float nodeHeight = 75f;
    const int nodePadding = 25;
    const int nodeBorder = 12;
    [MenuItem("Room Node Graph Editor", menuItem = "Window/DungeonEditor/RoomNodeGraphicEditor")]
    static void OpenWindow()
    {
        GetWindow<RoomNodeGraphicEditor>("Room Node Graphic Editor");
        _soRoomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }
    [OnOpenAsset(1)]
    public static bool OnDoubleClickAssets(int instanceID, int line)
    {
        SORoomNodeGraphic roomNodeGraphic = EditorUtility.InstanceIDToObject(instanceID) as SORoomNodeGraphic;
        if (roomNodeGraphic != null)
        {
            OpenWindow();
            _soRoomNodeGraphic = roomNodeGraphic;
            return true;
        }
        return false;
    }
    private void OnEnable()
    {
        roomNodeStyle = new GUIStyle();
        roomNodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
        roomNodeStyle.normal.textColor = Color.white;
        roomNodeStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        roomNodeStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);
    }
    private void OnGUI()
    {
        if (_soRoomNodeGraphic != null)
        {
            ProcessEvents(Event.current);
            DrawRoomNodes();
        }
        if (GUI.changed)
            Repaint();
    }
    void ProcessEvents(Event currentEvent)
    {
        if (currentRoomNode == null || currentRoomNode.isLeftClickDragging == false)
        {
            currentRoomNode = IsMouseOverRoomNode(currentEvent);
        }
        if(currentRoomNode == null)
        {
            ProcessRoomNodeEvents(currentEvent);
        }
        else
        {
            currentRoomNode.ProcessEvents(currentEvent);
        }

    }

    private SORoomNode IsMouseOverRoomNode(Event currentEvent)
    {
        for (int i = _soRoomNodeGraphic._roomNodeList.Count - 1; i >= 0; i--)
        {
            if (_soRoomNodeGraphic._roomNodeList[i].rect.Contains(currentEvent.mousePosition))
            {
                return _soRoomNodeGraphic._roomNodeList[i];
            }
        }
        return null;
    }

    void ProcessRoomNodeEvents(Event currentEvent)
    {
        switch (currentEvent.type)
        {
            case EventType.MouseDown:
                ProcessMouseDownEvent(currentEvent);
                break;
        }
    }
    void ProcessMouseDownEvent(Event currentEvent)
    {
        //process right click mouse down on graph event
        if (currentEvent.button == 1)
        {
            ShowContentMenu(currentEvent.mousePosition);
        }
    }
    void ShowContentMenu(Vector2 mousePosition)
    {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("Add Room Node"), false, CreateRoomNode, mousePosition);
        menu.ShowAsContext();
    }
    void CreateRoomNode(object mousePosition)
    {
        Vector2 mousePos = (Vector2)mousePosition;
        SORoomNode roomNode = ScriptableObject.CreateInstance<SORoomNode>();
        _soRoomNodeGraphic._roomNodeList.Add(roomNode);
        roomNode.Initialize(new Rect(mousePos, new Vector2(nodeWidth, nodeHeight)), _soRoomNodeGraphic, _soRoomNodeTypeList.list.Find(x => x.isNone));
        AssetDatabase.AddObjectToAsset(roomNode, _soRoomNodeGraphic);
        AssetDatabase.SaveAssets();
    }
    void DrawRoomNodes()
    {
        foreach (var roomNode in _soRoomNodeGraphic._roomNodeList)
        {
            roomNode.Draw(roomNodeStyle);
        }
        GUI.changed = true;
    }
}
