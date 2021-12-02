using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Compilation;
using UnityEditor.Experimental;
using UnityEngine;

namespace DartsGames.NodeSystem
{
    public class NodeSystemEditorHandler
    {
        public static string NodeDir = "";

        [MenuItem("DartsGames/Create/Node Editor Class")]
        public static void CreateNodeEditorClass()
        {
            var className = "NodeEditors";
            var path = Path.Combine(NodeDir, "Scripts", "ScriptableObjects");
            CreateClass(className, "NodeEditorBase", path, new[] {"DartsGames.NodeSystem", "UnityEngine"},
                new[]
                {
                    "CreateAssetMenu(fileName = \"new " + className + "\", menuName =\"NodeSystem/" + className +
                    " \")"
                });


            path = Path.Combine(NodeDir, "Scripts", "Editor", "ScriptableObjects");
            CreateClass(className + "Editor", "Editor", path, new[] {"UnityEditor", "UnityEngine"},
                new[] {"CustomEditor(typeof(" + className + "))"}, contents:
                @"private " + className + " " + className + @";
    private void Awake()
    {
        " + className + @" = target as " + className + @";
    }

    public override void OnInspectorGUI()
    {
        if (GUILayout.Button(""Open Editor Window""))
        {
            " + className + "EditorWindow" + @".Init(" + className + @");
        }
    }");

            path = Path.Combine(NodeDir, "Scripts", "Editor", "Windows");

            CreateClass(className + "EditorWindow", "EditorWindow", path, new[] {"UnityEditor","UnityEngine"},
                contents: @"private "+className+@" "+className+@";
    private Vector2 scrollPos;
    private Rect scrollSize;

    public static void Init("+className+@" "+className+@")
    {
        var window = new "+className+@"EditorWindow();
        window.scrollSize = new Rect(0, 0, 1000, 1000);
        window.scrollPos = new Vector2();
        window."+className+@" = "+className+@";
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(0, 0, position.width, position.height));
        scrollPos = GUI.BeginScrollView(new Rect(0, 0, position.width, position.height), scrollPos, scrollSize);
        GUI.EndScrollView();
        GUILayout.EndArea();
    }");
            AssetDatabase.Refresh();
        }

        public static void CreateClass(string name, string baseClass = "MonoBehaviour",
            string directory = "", string[] includes = null, string[] attributes = null, string contents = "")
        {
            var fullDir = Path.Combine(Application.dataPath, directory);
            var fileDir = Path.Combine(fullDir, name + ".cs");

            if (!Directory.Exists(fullDir))
            {
                Directory.CreateDirectory(fullDir);
            }

            string content = "";
            if (includes is { })
                foreach (string include in includes)
                {
                    content += "using " + include + ";\n";
                }

            content += "\n";

            if (attributes is { })
                foreach (string attribute in attributes)
                {
                    content += "[" + attribute + "]\n";
                }

            content +=
                "public class " + name + " : " + baseClass + "\n" +
                "{\n" +
                contents + "\n" +
                "}";

            if (!File.Exists(fileDir))
            {
                var stream = File
                    .CreateText(fileDir);
                stream.Write(content);
                stream.Close();
            }
            else
            {
                File.WriteAllText(fileDir, content);
            }
        }
    }
}