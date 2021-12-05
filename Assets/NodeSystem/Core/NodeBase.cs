using UnityEngine;

[System.Serializable]
public class NodeBase : ScriptableObject
{
    [HideInInspector]
    public int Id;
    [HideInInspector]
    public int nextId;
    [HideInInspector]
    public int prevId;
}