using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class RoomNodeGraphicEditor : EditorWindow
{
    [MenuItem("Room Node Graph Editor",menuItem = "Window/DungeonEditor/RoomNodeGraphicEditor")]
    public static void OpenWindow()
    {
        GetWindow<RoomNodeGraphicEditor>("Room Node Graphic Editor");
    }

}
