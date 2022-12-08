
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SORoomNodeGraphic", menuName = "ScriptableObjects/Dungeon/SORoomNodeGraphic", order = 1)]
public class SORoomNodeGraphic : ScriptableObject
{
    public SORoomNodeTypeList _soRoomNodeTypeList;
    public List<SORoomNode> _roomNodeList = new List<SORoomNode>();
    public Dictionary<string, SORoomNode> _roomNodeDictionary = new Dictionary<string, SORoomNode>();
#if UNITY_EDITOR
    public SORoomNode _roomNodeToDrawLineFrom = null;
    public Vector2 linePosition;
    public void SetNodeToDrawConnectionLineFrom(SORoomNode roomNode, Vector2 position)
    {
        _roomNodeToDrawLineFrom = roomNode;
        linePosition = position;
    }
    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        LoadRoomNodeDictionary();
    }

    public void OnValidate()
    {
        LoadRoomNodeDictionary();
    }
    private void LoadRoomNodeDictionary()
    {
        _roomNodeDictionary.Clear();
        foreach (var roomNode in _roomNodeList)
        {
            _roomNodeDictionary[roomNode.id] = roomNode;
        }
    }
    public SORoomNode GetRoomNode(string roomNodeID)
    {
        if (_roomNodeDictionary.TryGetValue(roomNodeID, out SORoomNode roomNode))
        {
            return roomNode;
        }
        return null;
    }
#endif
}