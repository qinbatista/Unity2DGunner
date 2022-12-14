using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System;
using System.Collections.Generic;

public class RoomNodeGraphicEditor : EditorWindow
{
    static SORoomNodeGraphic _currentRoomNodeGraphic;
    Vector2 graphOffset;
    Vector2 graphDrag;
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
    const float gridLarge = 100f;
    const float gridSmall = 25f;

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
            DrawBackgroundGrid(gridSmall, 0.2f, Color.gray);
            DrawBackgroundGrid(gridLarge, 0.3f, Color.gray);
            DrawDraggedLine();
            ProcessEvents(Event.current);
            DrawRoomConnections();
            DrawRoomNodes();
        }
        if (GUI.changed)
            Repaint();
    }

    private void DrawBackgroundGrid(float gridSize, float gridOpacity, Color gray)
    {
        int verticalLineCount = Mathf.CeilToInt((position.width + gridSize) / gridSize);
        int horizontalLineCount = Mathf.CeilToInt((position.height + gridSize) / gridSize);
        Handles.color = new Color(gray.r, gray.g, gray.b, gridOpacity);
        graphOffset += graphDrag * 0.5f;
        Vector3 gridOffset = new Vector3(graphOffset.x % gridSize, graphOffset.y % gridSize, 0);
        for (int i = 0; i < verticalLineCount; i++)
        {
            Handles.DrawLine(new Vector3(gridSize * i, -gridSize, 0) + gridOffset, new Vector3(gridSize * i, position.height + gridSize, 0f) + gridOffset);
        }
        for (int i = 0; i < horizontalLineCount; i++)
        {
            Handles.DrawLine(new Vector3(-gridSize, gridSize * i, 0) + gridOffset, new Vector3(position.width + gridSize, gridSize * i, 0f) + gridOffset);
        }
        Handles.color = Color.white;
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
        graphDrag = Vector2.zero;
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
        else if (currentEvent.button == 0)
        {
            ProcessLeftMouseDragEvent(currentEvent.delta);
        }
    }

    private void ProcessLeftMouseDragEvent(Vector2 delta)
    {
        graphDrag = delta;
        for (int i = 0; i < _currentRoomNodeGraphic._roomNodeList.Count; i++)
        {
            _currentRoomNodeGraphic._roomNodeList[i].DragNode(delta);
        }
        GUI.changed = true;
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
        menu.AddItem(new GUIContent("Delete Selected Room Nodes Links"), false, DeleteSelectedRoomNodesLinks);
        menu.AddItem(new GUIContent("Delete Selected Room Nodes"), false, DeleteSelectedRoomNodes);
        menu.ShowAsContext();
    }

    private void DeleteSelectedRoomNodes()
    {
        Queue<SORoomNode> roomNodeDeletionQueue = new Queue<SORoomNode>();
        foreach (SORoomNode roomNode in _currentRoomNodeGraphic._roomNodeList)
        {
            if (roomNode.isSelected && !roomNode.roomNodeType.isEntrance)
            {
                roomNodeDeletionQueue.Enqueue(roomNode);
                foreach (string childRoomNode in roomNode.childRoomNodeIDList)
                {
                    SORoomNode childRoomNodeObject = _currentRoomNodeGraphic.GetRoomNode(childRoomNode);
                    if (childRoomNodeObject != null)
                    {
                        childRoomNodeObject.RemoveParentRoomNodeIDFromRoomNode(roomNode.id);
                    }
                }
                foreach (string parentRoomNodeID in roomNode.parentRoomNodeIDList)
                {
                    SORoomNode parentRoomNodeObject = _currentRoomNodeGraphic.GetRoomNode(parentRoomNodeID);
                    if (parentRoomNodeObject != null)
                    {
                        parentRoomNodeObject.RemoveChildRoomNodeIDFromRoomNode(roomNode.id);
                    }
                }
            }
        }
        while (roomNodeDeletionQueue.Count > 0)
        {
            SORoomNode roomNodeToDelete = roomNodeDeletionQueue.Dequeue();
            _currentRoomNodeGraphic._roomNodeDictionary.Remove(roomNodeToDelete.id);
            _currentRoomNodeGraphic._roomNodeList.Remove(roomNodeToDelete);
            DestroyImmediate(roomNodeToDelete, true);
            AssetDatabase.SaveAssets();
        }
    }

    private void DeleteSelectedRoomNodesLinks()
    {
        foreach (SORoomNode roomNode in _currentRoomNodeGraphic._roomNodeList)
        {
            if (roomNode.isSelected && roomNode.childRoomNodeIDList.Count > 0)
            {
                for (int i = roomNode.childRoomNodeIDList.Count - 1; i >= 0; i--)
                {
                    SORoomNode childRoomNode = _currentRoomNodeGraphic.GetRoomNode(roomNode.childRoomNodeIDList[i]);
                    if (childRoomNode != null && childRoomNode.isSelected)
                    {
                        roomNode.RemoveChildRoomNodeIDFromRoomNode(childRoomNode.id);
                        childRoomNode.RemoveParentRoomNodeIDFromRoomNode(roomNode.id);
                    }
                }
            }
        }
        ClearAllSelectedRoomNodes();
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
