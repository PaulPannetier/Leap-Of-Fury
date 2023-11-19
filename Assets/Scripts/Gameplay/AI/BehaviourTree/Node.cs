using System.Collections;
using System.Collections.Generic;

namespace CustomAI
{
    public enum NodeState
    {
        RUNNING,
        SUCCESS,
        FAILURE
    }

    public class Node
    {
        private Dictionary<string, object> dataContext = new Dictionary<string, object>();

        protected NodeState state;

        public Node parent;
        protected List<Node> children = new List<Node>();

        public Node()
        {
            parent = null;
        }

        public Node(List<Node> children)
        {
            foreach (Node child in children)
                Attach(child);
        }

        private void Attach(Node node)
        {
            node.parent = this;
            children.Add(node);
        }

        public virtual NodeState Evaluate() => NodeState.FAILURE;

        public void SetData(string key, object value)
        {
            dataContext[key] = value;
        }

        public object GetData(string key)
        {
            object val = null;
            if (dataContext.TryGetValue(key, out val))
                return val;

            Node node = parent;
            while (node != null)
            {
                if (node.dataContext.TryGetValue(key, out val))
                    return val;
                node = node.parent;
            }
            return val;
        }

        public bool ClearData(string key)
        {
            if (dataContext.ContainsKey(key))
            {
                dataContext.Remove(key);
                return true;
            }

            Node node = parent;
            while (node != null)
            {
                if (node.dataContext.ContainsKey(key))
                {
                    dataContext.Remove(key);
                    return true;
                }
                node = node.parent;
            }
            return false;
        }
    }
}
