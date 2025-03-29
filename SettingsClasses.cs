using System.Collections.Generic;
using System.Drawing;
using ExileCore2.PoEMemory;
using ExileCore2.PoEMemory.Elements.AtlasElements;
using ExileCore2.PoEMemory.FilesInMemory.Atlas;

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
        { this.ID = address; }

        public Node(AtlasPanelNode element, long address, Color _nodeColor, Color _nameColor)
        {
            this.Name = element.Area.Name;
            this.NodeElement = element;
            this.ID = address;
            this.NodeColor = _nodeColor;
            this.NameColor = _nameColor;
            this.IsCorrupted = element.IsCorrupted;
        }

        public string Name { get; set; }
        public int TowersCount { get; set; }
        public int AffectedTowersCount { get; set; }
        public AtlasPanelNode NodeElement;
        public long ID { get; set; }
        public Color NodeColor { get; set; }
        public Color NameColor { get; set; }
        public List<Content> Content { get; set; }
        public bool IsCorrupted;
    }
}