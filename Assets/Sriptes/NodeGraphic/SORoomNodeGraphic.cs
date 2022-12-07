
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SORoomNodeGraphic", menuName = "ScriptableObjects/Dungeon/SORoomNodeGraphic", order = 1)]
public class SORoomNodeGraphic : ScriptableObject
{
    public List<SORoomNode> _roomNodeList = new List<SORoomNode>();

}