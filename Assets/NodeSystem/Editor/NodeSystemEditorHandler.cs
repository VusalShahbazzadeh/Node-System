using System.IO;
using UnityEditor;
using UnityEngine;

namespace DartsGames.NodeSystem
{
    public class NodeSystemEditorHandler
    {
        public static string NodeDir = "";

        [MenuItem("DartsGames/Create/Node  Class")]
        public static void CreateNodeClass()
        {
            var className = "NodeDerived";
            var path = Path.Combine(NodeDir, "Scripts", "ScriptableObjects");
            CreateClass(className, "NodeBase", path, new [] {"DartsGames.NodeSystem"});
            AssetDatabase.Refresh();
        }
        
        [MenuItem("DartsGames/Create/Node Graph Class")]
        public static void CreateNodeGraphClass()
        {
            var className = "NodeGraphDerived";
            var path = Path.Combine(NodeDir, "Scripts", "ScriptableObjects");
            CreateClass(className, "NodeGraphBase", path, new[] {"DartsGames.NodeSystem", "UnityEngine"},
                new[]
                {
                    "CreateAssetMenu(fileName = \"new " + className + "\", menuName =\"NodeSystem/" + className +
                    " \")"
                });


            path = Path.Combine(NodeDir, "Scripts", "Editor", "ScriptableObjects");
            CreateClass(className + "Editor", "Editor", path, new[] {"UnityEditor", "UnityEngine"},
                new[] {"CustomEditor(typeof(" + className + "))"}, contents:
                @"  private " + className + " " + className + @";
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

            CreateClass(className + "EditorWindow", "NodeGraphEditorWindowBase", path,
                new[] {"System.Collections.Generic", "UnityEngine", "DartsGames.NodeSystem"},
                contents: @"    private " + className + @" " + className + @";
    public static void Init(" + className + @" " + className + @")
    {
        var window = CreateInstance<" + className + @"EditorWindow>();
        window.scrollSize = new Rect(0, 0, 1000, 1000);
        window.scrollPos = new Vector2();
        window." + className + @" = " + className + @";
        window.Show();
        window.nodes = new List<NodeWindow>();
        for (int i = 0; i < 1; i++)
        {
            window.nodes.Add(NodeWindow.Init(CreateInstance<NodeBase>(), new Rect(100, 100, 200, 200)));
        }
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