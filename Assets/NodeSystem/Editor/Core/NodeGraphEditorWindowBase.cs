using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;

namespace DartsGames.NodeSystem
{
    public abstract class NodeGraphEditorWindowBase : EditorWindow
    {
        protected NodeGraphBase NodeGraphBase;
        protected Vector2 scrollPos;
        protected Rect scrollSize;
        protected List<NodeWindow> nodes;
        protected NodeWindow selectedNode;

        private NodeWindow linkNode = null;

        protected virtual void OnGUI()
        {
            ProcessUserInput();
            DrawLines();

            GUILayout.BeginArea(new Rect(0, 0, position.width, position.height));
            scrollPos = GUI.BeginScrollView(new Rect(0, 0, position.width, position.height), scrollPos, scrollSize);

            BeginWindows();
            for (int i = 0; i < nodes.Count; i++)
            {
                for (var dir = 0; dir < 4; dir++)
                {
                    nodes[i].rect = Resizer(nodes[i], dir);
                }

                nodes[i].rect = GUI.Window(i, nodes[i].rect, DrawNode, new GUIContent(""));
            }

            EndWindows();
            GUI.EndScrollView();
            GUILayout.EndArea();
        }

        private void DrawLines()
        {
            for (var i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].nextNode is null)
                {
                    continue;
                }

                DrawLine(nodes[i].rect, nodes[i].nextNode.rect);
            }

            if (linkNode is { })
            {
                var startPos = new Vector3(linkNode.rect.x + linkNode.rect.width,
                    linkNode.rect.y + linkNode.rect.height / 2, 0);
                DrawLine(startPos, Event.current.mousePosition);
                Repaint();
            }
        }

        private void DrawLine(Rect start, Rect end)
        {
            var startPos = new Vector3(start.x + start.width, start.y + start.height / 2, 0);
            var endPos = new Vector3(end.x, end.y + end.height / 2, 0);
            DrawLine(startPos, endPos);
        }


        private void DrawLine(Vector3 start, Vector3 end)
        {
            var startTan = start + Vector3.right * 50;
            var endTan = end + Vector3.left * 50;

            Handles.DrawBezier(start, end, startTan, endTan, Color.white, null, 2);
        }

        private void DrawNode(int i)
        {
            nodes[i].editor.DrawDefaultInspector();

            GUI.DragWindow(new Rect(8, 8, nodes[i].rect.width - 16, nodes[i].rect.height - 16));
        }

        private void ProcessUserInput()
        {
            var e = Event.current;

            if (e.type == EventType.MouseDown &&
                e.button == 1)
            {
                RightClick(e);
            }

            if (e.type == EventType.MouseDown &&
                e.button == 0)
            {
                LeftClick(e);
            }
        }

        private void LeftClick(Event e)
        {
            if (linkNode is null)
            {
                return;
            }

            selectedNode = nodes.FirstOrDefault(x => x.rect.Contains(e.mousePosition));
            FinishConnectNode();
        }

        private void RightClick(Event e)
        {
            linkNode = null;
            selectedNode = nodes.FirstOrDefault(x => x.rect.Contains(e.mousePosition));
            if (selectedNode is null)
            {
                ShowContextMenu(e);
            }
            else
                ShowNodeContextMenu(selectedNode);
        }

        private void ShowContextMenu(Event e)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("NodeBase"), false,
                () => CreateNode(e, typeof(NodeBase)));
            foreach (Type t in AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsSubclassOf(typeof(NodeBase))))
            {
                menu.AddItem(new GUIContent(t.Name), false,
                    () => CreateNode(e, t));
            }

            menu.ShowAsContext();
        }

        private void ShowNodeContextMenu(NodeWindow node)
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Delete Node"), false, () => DeleteNode(node));
            menu.AddItem(new GUIContent("Connect"), false, () => StartConnectNode(node));
            menu.ShowAsContext();
        }

        private void StartConnectNode(NodeWindow node)
        {
            linkNode = node;
        }

        private void FinishConnectNode()
        {
            if (selectedNode is null || selectedNode == linkNode)
            {
                linkNode.node.nextId = 0;
                linkNode.nextNode = null;
                linkNode = null;
            }
            else
            {
                if (selectedNode.prevNode is { })
                {
                    selectedNode.prevNode.nextNode = null;
                }

                if (linkNode.nextNode is { })
                {
                    linkNode.nextNode.prevNode = null;
                }

                linkNode.nextNode = selectedNode;
                selectedNode.prevNode = linkNode;
                linkNode = null;
            }
        }

        private void CreateNode(Event e, Type t)
        {
            var newNode = NodeWindow.Init(CreateInstance(t) as NodeBase,
                new Rect(0, 0, 200, 200));
            newNode.rect.position = e.mousePosition;

            int i = 1;
            while (nodes.Any(n => n.node.Id == i))
            {
                i++;
            }

            newNode.node.Id = i;

            nodes.Add(newNode);
            NodeGraphBase.nodes.Add(newNode.node);
            NodeGraphBase.nodePos.Add(newNode.rect);
            NodeGraphBase.AssemblyQualifiedNames.Add(t.AssemblyQualifiedName);
        }


        private void DeleteNode(NodeWindow node)
        {
            nodes.Remove(node);
            if (node.nextNode is { })
            {
                node.nextNode.prevNode = null;
                node.nextNode = null;
            }

            if (node.prevNode is { })
            {
                node.prevNode.nextNode = null;
                node.prevNode = null;
            }

            var index = NodeGraphBase.nodes.IndexOf(node.node);
            NodeGraphBase.nodes.RemoveAt(index);
            NodeGraphBase.nodePos.RemoveAt(index);
            NodeGraphBase.AssemblyQualifiedNames.RemoveAt(index);
        }

        private Rect Resizer(NodeWindow nodeWindow, int dir = 0, float detectionRange = 16f)
        {
            Rect window = nodeWindow.rect;
            detectionRange *= .5f;
            Rect resizer = window;

            if (dir == 0)
            {
                // left
                resizer.xMax = resizer.xMin + detectionRange;
                resizer.xMin -= detectionRange;
            }
            else if (dir == 1)
            {
                // right
                resizer.xMin = resizer.xMax - detectionRange;
                resizer.xMax += detectionRange;
            }
            else if (dir == 2)
            {
                // up
                resizer.yMax = resizer.yMin + detectionRange;
                resizer.yMin -= detectionRange;
            }
            else
            {
                // down
                resizer.yMin = resizer.yMax - detectionRange;
                resizer.yMax += detectionRange;
            }

            //Debug.Log("Dir: " + dir + "\n(" + resizer.xMin + ", " + resizer.xMax + ")\n(" + resizer.yMin + ", " + resizer.yMax + ")");

            Event current = Event.current;
            if (dir == 0 ||
                dir == 1)
            {
                EditorGUIUtility.AddCursorRect(resizer, MouseCursor.ResizeHorizontal);
            }
            else
            {
                EditorGUIUtility.AddCursorRect(resizer, MouseCursor.ResizeVertical);
            }

            if (current.type == EventType.MouseUp)
            {
                nodeWindow.draggingLeft = false;
                nodeWindow.draggingRight = false;
                nodeWindow.draggingUp = false;
                nodeWindow.draggingDown = false;
                nodeWindow.canDrag = false;
            }

            bool inXRange = current.mousePosition.x >= resizer.xMin && current.mousePosition.x <= resizer.xMax;
            bool inYRange = current.mousePosition.y >= resizer.yMin && current.mousePosition.y <= resizer.yMax;


            if (current.type == EventType.MouseDown &&
                (inXRange && inYRange))
            {
                nodeWindow.canDrag = true;
            }

            if (nodeWindow.canDrag)
            {
                if (inXRange && inYRange &&
                    current.type == EventType.MouseDrag &&
                    current.button == 0 ||
                    nodeWindow.draggingLeft ||
                    nodeWindow.draggingRight)
                {
                    if ((dir == 1) &&
                        !(nodeWindow.draggingRight || nodeWindow.draggingUp || nodeWindow.draggingDown))
                    {
                        window.width += current.delta.x / 2f;
                        window.width = Mathf.Max(100f, window.width);
                        Repaint();
                        nodeWindow.draggingLeft = true;
                    }

                    if ((dir == 0) &&
                        !(nodeWindow.draggingLeft || nodeWindow.draggingUp || nodeWindow.draggingDown))
                    {
                        window.x += current.delta.x / 2f;
                        window.width -= current.delta.x / 2f;
                        window.width = Mathf.Max(100f, window.width);
                        Repaint();
                        nodeWindow.draggingRight = true;
                    }
                }

                if (inXRange && inYRange &&
                    current.type == EventType.MouseDrag &&
                    current.button == 0 ||
                    nodeWindow.draggingUp ||
                    nodeWindow.draggingDown)
                {
                    if ((dir == 2) &&
                        !(nodeWindow.draggingDown || nodeWindow.draggingLeft || nodeWindow.draggingRight))
                    {
                        window.y += current.delta.y / 2f;
                        window.height -= current.delta.y / 2f;
                        window.height = Mathf.Max(50f, window.height);
                        Repaint();
                        nodeWindow.draggingUp = true;
                    }

                    if ((dir == 3) &&
                        !(nodeWindow.draggingUp || nodeWindow.draggingLeft || nodeWindow.draggingRight))
                    {
                        window.height += current.delta.y / 2f;
                        window.height = Mathf.Max(50f, window.height);
                        Repaint();
                        nodeWindow.draggingDown = true;
                    }
                }
            }

            return window;
        }

        private void OnDisable()
        {
            for (var i = 0; i < nodes.Count; i++)
            {
                NodeGraphBase.nodes[i] = nodes[i].node;
                NodeGraphBase.nodePos[i] = nodes[i].rect;
            }
        }

        public void Init()
        {
            nodes = new List<NodeWindow>();
            for (int i = 0; i < NodeGraphBase.nodes.Count; i++)
            {
                var nodeWindow = NodeWindow.Init(NodeGraphBase.nodes[i], NodeGraphBase.nodePos[i]); 
                nodes.Add(nodeWindow);
            }
            
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                node.prevNode = nodes.FirstOrDefault(n => n.node.Id == node.node.prevId);
                node.nextNode = nodes.FirstOrDefault(n => n.node.Id == node.node.nextId);
            }

        }
        
    }
}