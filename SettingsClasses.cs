using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using ExileCore2.PoEMemory.Elements.AtlasElements;

namespace cOverlay
{
    public class Content
    {
        public Content()
        { }

        public Content(string type, float contentWidth = 1)
        {
            this.Type = type;
            this.ContentColor = Color.Black;
            this.ContentWidth = contentWidth;
        }

        public Content(string type, Color _contentColor, float _contentWidth = 1)
        {
            this.Type = type;
            this.ContentColor = _contentColor;
            this.ContentWidth = _contentWidth;
        }

        public string Type { get; set; } = "";
        public Color ContentColor { get; set; } = Color.White;
        public float ContentWidth { get; set; }

        public static HashSet<string> ContentType = new HashSet<string> {
            "Breach",
            "Ritual",
            "Expedition",
            "Irradiated",
            "Map Boss",
            "Delirium"
        };
    }

    public class Node
    {
        public Node(long address)
        {
            this.ID = address;
        }

        public Node(long iD, AtlasNodeDescription _node, List<AtlasNodeDescription> neighbourNodes, int towersCount, int affectedTowersCount) : this(iD)
        {
            NodeObject = _node;
            this.neighbourNodes = neighbourNodes;
            TowersCount = towersCount;
            AffectedTowersCount = affectedTowersCount;
        }

        public long ID { get; set; }
        public AtlasNodeDescription NodeObject { get; set; }
        public List<AtlasNodeDescription> neighbourNodes { get; set; } = new List<AtlasNodeDescription>();
        public int TowersCount { get; set; }
        public int AffectedTowersCount { get; set; }
    }
}