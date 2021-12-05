using System;
using System.Collections.Generic;
using UnityEngine;

namespace DartsGames.NodeSystem
{
    public abstract class NodeGraphBase : ScriptableObject
    {
        public List<NodeBase> nodes;
        public List<Rect> nodePos;

        private void OnEnable()
        {
            nodes = new List<NodeBase>
            {
                CreateInstance<NodeBase>()
            };
            nodePos = new List<Rect>
            {
                new Rect(0, 0, 100, 100)
            };
        }
    }
}