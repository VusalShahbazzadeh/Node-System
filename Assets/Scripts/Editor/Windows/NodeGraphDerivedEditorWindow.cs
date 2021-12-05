using System.Collections.Generic;
using UnityEngine;
using DartsGames.NodeSystem;

public class NodeGraphDerivedEditorWindow : NodeGraphEditorWindowBase
{
    private NodeGraphDerived NodeGraphDerived;
    public static void Init(NodeGraphDerived NodeGraphDerived)
    {
        var window = CreateInstance<NodeGraphDerivedEditorWindow>();
        window.scrollSize = new Rect(0, 0, 1000, 1000);
        window.scrollPos = new Vector2();
        window.NodeGraphDerived = NodeGraphDerived;
        window.Show();
        window.nodes = new List<NodeWindow>();
        for (int i = 0; i < 1; i++)
        {
            window.nodes.Add(NodeWindow.Init(CreateInstance<NodeBase>(), new Rect(100, 100, 200, 200)));
        }
    }
}