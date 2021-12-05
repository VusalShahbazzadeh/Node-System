using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NodeGraphDerived))]
public class NodeGraphDerivedEditor : Editor
{
  private NodeGraphDerived NodeGraphDerived;
    private void Awake()
    {
        NodeGraphDerived = target as NodeGraphDerived;
    }

    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Open Editor Window"))
        {
            NodeGraphDerivedEditorWindow.Init(NodeGraphDerived);
        }
    }
}