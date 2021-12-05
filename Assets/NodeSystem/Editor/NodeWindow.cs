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

    public NodeWindow nextNode
    {
        get => _nextNode;
        set
        {
            if (value is null)
            {
                node.nextId = 0;
                _nextNode = null;
                return;
            }
            node.nextId = value.node.Id;
            _nextNode = value;
        }
    }

    public NodeWindow prevNode
    {
        get => _prevNode;
        set
        {
            if (value is null)
            {
                _prevNode = null;
                node.prevId = 0;
                return;
            }
            _prevNode = value;
            node.prevId = value.node.Id;
        }
    }

    private NodeWindow _nextNode;
    private NodeWindow _prevNode;

    public static NodeWindow Init(NodeBase node, Rect position)
    {
        var window = CreateInstance<NodeWindow>();
        window.node = node;
        window.rect = position;
        window.editor = Editor.CreateEditor(node);
        return window;
    }
}