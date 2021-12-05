using System;
using System.Collections.Generic;
using UnityEngine;

namespace DartsGames.NodeSystem
{
    public abstract class NodeGraphBase : ScriptableObject, ISerializationCallbackReceiver
    {
        [NonSerialized]
        public List<NodeBase> nodes;
        [NonSerialized]
        public List<Rect> nodePos;
        public List<string> AssemblyQualifiedNames;

        public string[] serializedData;
        public int dataCount;

        public NodeGraphBase()
        {
            nodes = new List<NodeBase>();
            nodePos = new List<Rect>();
            AssemblyQualifiedNames = new List<string>();
        }

        public void OnBeforeSerialize()
        {
            dataCount = nodes.Count;
            Debug.Log(dataCount );
            serializedData = new string[dataCount*2];
            for (var i = 0; i < dataCount; i++)
            {
                serializedData[i] = JsonUtility.ToJson(nodes[i]);
                serializedData[i + dataCount] = JsonUtility.ToJson(new SerializableRect(nodePos[i].x,nodePos[i].y,nodePos[i].width,nodePos[i].height));
            }
        }

        public void OnAfterDeserialize()
        {
            Debug.Log(dataCount );
            for (var i = 0; i < dataCount; i++)
            {
                Debug.Log(serializedData[i] ); 
                NodeBase node = new NodeBase();
                 JsonUtility.FromJsonOverwrite(serializedData[i], node);
                var nodePosition = JsonUtility.FromJson<SerializableRect>(serializedData[i + dataCount]);
                 nodes.Add(node);
                nodePos.Add(new Rect(nodePosition.x,nodePosition.y,nodePosition.width,nodePosition.height));
            }
        }

        [System.Serializable]
        private class SerializableRect
        {
            public float x, y, width, height;

            public SerializableRect(float x, float y, float width, float height)
            {
                this.x = x;
                this.y = y;
                this.width = width;
                this.height = height;
            }
        }
    }
}