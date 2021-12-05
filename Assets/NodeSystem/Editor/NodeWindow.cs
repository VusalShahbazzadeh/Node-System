using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class NodeWindow : ScriptableObject
{
    public NodeBase node;
    public Editor editor;
    public Rect rect;
    public bool draggingLeft;
    public bool draggingRight;
    public bool draggingUp;
    public bool draggingDown;
    public bool canDrag;

    public static NodeWindow Init(NodeBase node, Rect position)
    {
        var window = CreateInstance<NodeWindow>();
        window.node = node;
        window.rect = position;
        window.editor = Editor.CreateEditor(node);
        return window;
    }
}