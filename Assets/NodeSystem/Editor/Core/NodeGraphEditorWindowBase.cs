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
        protected Vector2 scrollPos;
        protected Rect scrollSize;
        protected List<NodeWindow> nodes;
        protected NodeWindow selectedNode;

        protected virtual void OnGUI()
        {
            ProcessUserInput();

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
        }

        private void RightClick(Event e)
        {
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
            menu.AddItem(new GUIContent("Connect"),false,()=>);
            menu.ShowAsContext();
        }

        private void CreateNode(Event e, Type t)
        {
            var newNode = NodeWindow.Init(Activator.CreateInstance(t) as NodeBase,
                new Rect(0, 0, 200, 200));
            newNode.rect.position = e.mousePosition;
            nodes.Add(newNode);
        }


        private void DeleteNode(NodeWindow node)
        {
            nodes.Remove(node);
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
    }
}