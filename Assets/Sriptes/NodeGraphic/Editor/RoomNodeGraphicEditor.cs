using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System;

public class RoomNodeGraphicEditor : EditorWindow
{
    static SORoomNodeGraphic _currentRoomNodeGraphic;
    static SORoomNodeTypeList _soRoomNodeTypeList;
    GUIStyle roomNodeStyle;
    GUIStyle roomNodeSelectedStyle;
    SORoomNode currentRoomNode;
    const float nodeWidth = 160f;
    const float nodeHeight = 75f;
    const int nodePadding = 25;
    const int nodeBorder = 12;
    const float connectingLineWidth = 3f;
    const float connectingLineArrowSize = 10f;
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
            _currentRoomNodeGraphic = roomNodeGraphic;
            return true;
        }
        return false;
    }
    private void OnEnable()
    {
        Selection.selectionChanged += InspectorSelectionChanged;
        roomNodeStyle = new GUIStyle();
        roomNodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
        roomNodeStyle.normal.textColor = Color.white;
        roomNodeStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        roomNodeStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);

        roomNodeSelectedStyle = new GUIStyle();
        roomNodeSelectedStyle.normal.background = EditorGUIUtility.Load("node1 on") as Texture2D;
        roomNodeSelectedStyle.normal.textColor = Color.red;
        roomNodeSelectedStyle.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        roomNodeSelectedStyle.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);

        _soRoomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }
    private void OnDisable()
    {
        Selection.selectionChanged -= InspectorSelectionChanged;
    }

    private void InspectorSelectionChanged()
    {
        SORoomNodeGraphic roomNodeGraphic = Selection.activeObject as SORoomNodeGraphic;
        if (roomNodeGraphic != null)
        {
            _currentRoomNodeGraphic = roomNodeGraphic;
            GUI.changed = true;
        }
    }

    private void OnGUI()
    {
        if (_currentRoomNodeGraphic != null)
        {
            DrawDraggedLine();
            ProcessEvents(Event.current);
            DrawRoomConnections();
            DrawRoomNodes();
        }
        if (GUI.changed)
            Repaint();
    }

    private void DrawRoomConnections()
    {
        foreach (var roomNode in _currentRoomNodeGraphic._roomNodeList)
        {
            if (roomNode.childRoomNodeIDList.Count > 0)
            {
                foreach (var childRoomNodeID in roomNode.childRoomNodeIDList)
                {
                    if (_currentRoomNodeGraphic._roomNodeDictionary.ContainsKey(childRoomNodeID))
                    {
                        DrawConnectionLine(roomNode, _currentRoomNodeGraphic._roomNodeDictionary[childRoomNodeID]);
                        GUI.changed = true;
                    }
                }
            }
        }
    }

    private void DrawConnectionLine(SORoomNode parentRoomNode, SORoomNode childRoomNode)
    {
        Vector2 startPosition = parentRoomNode.rect.center;
        Vector2 endPosition = childRoomNode.rect.center;
        Vector2 midPosition = (endPosition + startPosition) / 2f;
        Vector2 direction = endPosition - startPosition;
        Vector2 arrowTailPoint1 = midPosition - new Vector2(direction.y, -direction.x).normalized * connectingLineArrowSize;
        Vector2 arrowTailPoint2 = midPosition + new Vector2(direction.y, -direction.x).normalized * connectingLineArrowSize;
        Vector2 arrowHeadPoint = midPosition + direction.normalized * connectingLineArrowSize;

        Handles.DrawBezier(
            arrowHeadPoint,
            arrowTailPoint1,
            arrowHeadPoint,
            arrowTailPoint1,
            Color.white,
            null,
            connectingLineWidth
        );

        Handles.DrawBezier(
            arrowHeadPoint,
            arrowTailPoint2,
            arrowHeadPoint,
            arrowTailPoint2,
            Color.white,
            null,
            connectingLineWidth
        );

        Handles.DrawBezier(
            startPosition,
            endPosition,
            startPosition,
            endPosition,
            Color.white,
            null,
            connectingLineWidth
        );
        GUI.changed = true;
    }

    private void DrawDraggedLine()
    {
        if (_currentRoomNodeGraphic.linePosition != Vector2.zero)
        {
            Handles.DrawBezier(
                _currentRoomNodeGraphic._roomNodeToDrawLineFrom.rect.center,
                _currentRoomNodeGraphic.linePosition,
                _currentRoomNodeGraphic._roomNodeToDrawLineFrom.rect.center,
                _currentRoomNodeGraphic.linePosition,
                Color.white,
                null,
                connectingLineWidth
            );
            GUI.changed = true;
        }
    }

    void ProcessEvents(Event currentEvent)
    {
        if (currentRoomNode == null || currentRoomNode.isLeftClickDragging == false)
        {
            currentRoomNode = IsMouseOverRoomNode(currentEvent);
        }
        if (currentRoomNode == null || _currentRoomNodeGraphic._roomNodeToDrawLineFrom != null)
        {
            ProcessRoomNodeGraphicEvents(currentEvent);
        }
        else
        {
            currentRoomNode.ProcessEvents(currentEvent);
        }
    }

    private SORoomNode IsMouseOverRoomNode(Event currentEvent)
    {
        for (int i = _currentRoomNodeGraphic._roomNodeList.Count - 1; i >= 0; i--)
        {
            if (_currentRoomNodeGraphic._roomNodeList[i].rect.Contains(currentEvent.mousePosition))
            {
                return _currentRoomNodeGraphic._roomNodeList[i];
            }
        }
        return null;
    }
    void ProcessRoomNodeGraphicEvents(Event currentEvent)
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

    private void ProcessMouseUpEvent(Event currentEvent)
    {
        if (currentEvent.button == 1 && _currentRoomNodeGraphic._roomNodeToDrawLineFrom != null)
        {
            SORoomNode roomNode = IsMouseOverRoomNode(currentEvent);
            if (roomNode != null)
            {
                if (_currentRoomNodeGraphic._roomNodeToDrawLineFrom.AddChildRoomNodeIDToRoomNode(roomNode.id))
                {
                    roomNode.AddParentRoomNodeIDToRoomNode(_currentRoomNodeGraphic._roomNodeToDrawLineFrom.id);
                }
            }
            ClearLineDrag();
        }
    }

    private void ClearLineDrag()
    {
        _currentRoomNodeGraphic._roomNodeToDrawLineFrom = null;
        _currentRoomNodeGraphic.linePosition = Vector2.zero;
        GUI.changed = true;
    }

    private void ProcessMouseDragEvent(Event currentEvent)
    {
        if (currentEvent.button == 1)
        {
            ProcessRightMouseDragEvent(currentEvent);
        }
    }

    private void ProcessRightMouseDragEvent(Event currentEvent)
    {
        if (_currentRoomNodeGraphic._roomNodeToDrawLineFrom != null)
        {
            DragConnectingLine(currentEvent.delta);
            GUI.changed = true;
        }
    }

    private void DragConnectingLine(Vector2 delta)
    {
        _currentRoomNodeGraphic.linePosition += delta;
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
        else if (currentEvent.button == 0)
        {
            ClearLineDrag();
            ClearAllSelectedRoomNodes();
        }
    }

    private void ClearAllSelectedRoomNodes()
    {
        foreach (SORoomNode roomNode in _currentRoomNodeGraphic._roomNodeList)
        {
            if (roomNode.isSelected)
            {
                roomNode.isSelected = false;
                GUI.changed = true;
            }
        }
    }

    void ShowContentMenu(Vector2 mousePosition)
    {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("Add Room Node"), false, CreateRoomNode, mousePosition);
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Select All Room Nodes"), false, SelectAllRoomNodes);
        menu.ShowAsContext();
    }

    private void SelectAllRoomNodes()
    {
        foreach (SORoomNode roomNode in _currentRoomNodeGraphic._roomNodeList)
        {
            roomNode.isSelected = true;
        }
        GUI.changed = true;
    }

    void CreateRoomNode(object mousePositionObject)
    {
        if (_currentRoomNodeGraphic._roomNodeList.Count == 0)
        {
            CreateRoomNode(new Vector2(200f, 200f), _soRoomNodeTypeList.list.Find(x => x.isEntrance));
        }
        CreateRoomNode(mousePositionObject, _soRoomNodeTypeList.list.Find(x => x.isNone));
    }
    void CreateRoomNode(object mousePosition, SORoomNodeType roomNodeType)
    {
        Vector2 mousePos = (Vector2)mousePosition;
        SORoomNode roomNode = ScriptableObject.CreateInstance<SORoomNode>();
        _currentRoomNodeGraphic._roomNodeList.Add(roomNode);
        roomNode.Initialize(new Rect(mousePos, new Vector2(nodeWidth, nodeHeight)), _currentRoomNodeGraphic, roomNodeType);
        AssetDatabase.AddObjectToAsset(roomNode, _currentRoomNodeGraphic);
        AssetDatabase.SaveAssets();
        _currentRoomNodeGraphic.OnValidate();
    }
    void DrawRoomNodes()
    {
        foreach (var roomNode in _currentRoomNodeGraphic._roomNodeList)
        {
            if (roomNode.isSelected)
            {
                roomNode.Draw(roomNodeSelectedStyle);
            }
            else
            {
                roomNode.Draw(roomNodeStyle);
            }
        }
        GUI.changed = true;
    }
}
