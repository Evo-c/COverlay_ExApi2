using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;
using ExileCore2;
using ImGuiNET;
using Microsoft.CSharp;
using Microsoft.VisualBasic.Logging;
using static System.Windows.Forms.AxHost;


namespace cOverlay
{
    public class State
    {
        public State()
        {
        }

        public static string settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "MDK", "cOverlay");


        public ImGuiKey RefreshAtlasKey = ImGuiKey.F4;
        public ImGuiKey OpenKey = ImGuiKey.F12;
        public ImGuiKey AddWaypointKey = ImGuiKey.F1;
        public ImGuiKey RemoveWaypointKey = ImGuiKey.F2;
        public ImGuiKey ShowWaypointPanelKey = ImGuiKey.F3;
        public List<Content> ContentSettings { get; set; } = new List<Content>();
        public bool ShowWaypointMenu = false;
        public List<Waypoint> WaypointList { get; set; } = new List<Waypoint>();
        public bool drawDebug = false;
        public bool drawConnections = false;
        public Vector2 WaypointWindowPos = new Vector2(100, 100);

        public int NodeRadius = 20;
        public int borderX = 2400;
        public int borderY = 1300;
        public Vector2 paddingName = new Vector2(3, 2);
        public Vector2 paddingTower  = new Vector2(3, 2);
        public int nodeTextRounding = 3;
        public int towerTextRounding = 3;

        public int HighTowerAmountThreshold = 5;
        public Color HighTowerAmountColorBg = Color.White;
        public Color HighTowerAmountColorTxt = Color.Black;

        public Color BackgroundAreaColor = Color.Black;
        public Color BackgroundTowerColor = Color.Black;
        public Color TowerTextColor = Color.White;
        public bool DrawTextSameColor = false;
        public Color AreaTextColor = Color.White;
        public Color ConnectionsColor = Color.AliceBlue;
        public int ConnectionsThickness = 2;

        public Dictionary<string, Color> NodeColors = new Dictionary<string, Color>();
        public Dictionary<string, Color> NameColors = new Dictionary<string, Color>();

        public Dictionary<string, int> ContentCircleThickness = new Dictionary<string, int>();
        public Dictionary<string, Color> ContentCircleColor = new Dictionary<string, Color>();
        public Dictionary<string, bool> ContentToggle = new Dictionary<string, bool>();

        public void Load()
        {
            if (File.Exists(Path.Combine(settingsPath, "StateSettings.json")))
            {
                var obj = SerializationApi.Deserialize<State>(Path.Combine(settingsPath, "StateSettings.json"));
                if (obj != null)
                {     
                    RefreshAtlasKey = obj.RefreshAtlasKey;
                    OpenKey = obj.OpenKey;
                    AddWaypointKey = obj.AddWaypointKey;
                    RemoveWaypointKey = obj.RemoveWaypointKey;
                    ShowWaypointPanelKey = obj.ShowWaypointPanelKey;
                    ShowWaypointMenu = obj.ShowWaypointMenu;
                    WaypointWindowPos = obj.WaypointWindowPos;

                    borderX = obj.borderX;
                    borderY = obj.borderY;

                    drawConnections = obj.drawConnections;
                    drawDebug = obj.drawDebug;
                    WaypointList = obj.WaypointList;

                    paddingName = obj.paddingName;
                    paddingTower = obj.paddingTower;
                    nodeTextRounding = obj.nodeTextRounding;
                    towerTextRounding = obj.towerTextRounding;
                    NodeRadius = obj.NodeRadius;

                    ContentSettings = obj.ContentSettings;
                    NodeColors = obj.NodeColors;
                    NameColors = obj.NameColors;

                    ConnectionsColor = obj.ConnectionsColor;
                    ConnectionsThickness = obj.ConnectionsThickness;
                    ContentCircleThickness = obj.ContentCircleThickness;
                    ContentCircleColor = obj.ContentCircleColor;
                    ContentToggle = obj.ContentToggle;
                }
            }
            else
            {
                File.Create(Path.Combine(settingsPath, "StateSettings.json"));
            }
        }

        public void Save()
        {
            if (File.Exists(Path.Combine(settingsPath, "StateSettings.json")))
            {
                SerializationApi.Serialize<State>(this, Path.Combine(Path.Combine(settingsPath, "StateSettings.json")));
            }
            else
            {
                File.Create(Path.Combine(settingsPath, "StateSettings.json"));
            }
        }

        public void Init()
        {
            if (!File.Exists(Path.Combine(settingsPath, "StateSettings.json")))
            {
                File.Create(Path.Combine(settingsPath, "StateSettings.json"));

                SerializationApi.Serialize<State>(this, Path.Combine(Path.Combine(settingsPath, "StateSettings.json")));
            }

            var obj = SerializationApi.Deserialize<State>(Path.Combine(settingsPath, "StateSettings.json"));
            if (obj != null)
            {

                if (obj.ContentSettings.Count < (Content.ContentType.Count()))
                {
                    foreach (var content in Content.ContentType)
                    {
                        if (!obj.ContentSettings.Any(x => x.Type == content))
                        {
                            obj.ContentSettings.Add(new Content(content));
                        }
                    }
                }

                SerializationApi.Serialize<State>(obj, Path.Combine(Path.Combine(settingsPath, "StateSettings.json")));
            }
        }

    }
}
