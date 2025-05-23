﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;
using AutoMapper;

namespace cOverlay
{
    public class State
    {
        public State()
        {
        }

        public static string settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "MDK", "cOverlay");

        // Keybinds

        public Keys KeybindCreateWaypoint = Keys.F1;
        public Keys KeybindShowWaypointPanel = Keys.F2;
        public Keys KeybindSaveSettings = Keys.F3;
        public Keys KeybindRefreshNodes = Keys.F4;

        // Draw toggles

        public bool drawDebug = false;
        public bool drawConnections = false;
        public bool RecolorHighTowersAmount = false;
        public bool DrawTowerRange = false;

        // Colors

        public Color AreaTextColor = Color.White;
        public Color ConnectionsColor = Color.AliceBlue;
        public Color BackgroundAreaColor = Color.Black;
        public Color TowerTextColor = Color.White;
        public Color HighTowerAmountColorBg = Color.White;
        public Color HighTowerAmountColorTxt = Color.Black;
        public Color traversalColor = Color.FromArgb(255, 108, 180, 235);
        public Color untraversalColor = Color.Gray;

        // Style settings

        public int HighTowerAmountThreshold = 5;
        public Vector2 paddingName = new Vector2(3, 2);
        public int ConnectionsThickness = 2;
        public int nodeTextRounding = 3;
        public int NodeRadius = 20;
        public int traversalTransparency = 90;
        public int nodeRadiusTrash = 15;
        public bool showTowersAtName = true;
        public float towersRange = 12;

        // Node settings

        public Dictionary<string, Color> NodeColors = new Dictionary<string, Color>();
        public Dictionary<string, Color> NameColors = new Dictionary<string, Color>();

        // General Settings

        public int borderX = 2400;
        public int borderY = 1300;
        public int atlasRefreshRate = 10000;
        public int screnRefreshRate = 1000;

        // Content

        public bool ToggleGemling = true;
        public bool ToggleCorruptedMaps = true;
        public Color GemlingColor = Color.Orange;
        public int ContentCircleThickness = 2;
        public int GapRadius = 5;
        public Dictionary<string, bool> ContentToggle = new Dictionary<string, bool>();
        public Dictionary<string, Color> ContentCircleColor = new Dictionary<string, Color>();
        public List<Content> ContentSettings { get; set; } = new List<Content>();

        // Waypoints

        public bool ShowWaypointMenu = false;
        public Vector2 WaypointWindowPos = new Vector2(100, 100);
        public List<Waypoint> WaypointList { get; set; } = new List<Waypoint>();

        // Debug

        public bool DebugDrawPerfomance = false;
        public bool DebugDrawCoordinates = false;
        public bool DebugDrawContentAmount = false;
        public bool DebugDrawContentNames = false;
        public bool DebugDrawAttempted = false;
        public bool DebugDrawNodePosition = false;
        public bool DebugDrawNodeCenterPosition = false;

        // ---------------

        public void Load()
        {
            if (File.Exists(Path.Combine(settingsPath, "StateSettings.json")))
            {
                var config = new MapperConfiguration(cfg => cfg.CreateMap<State, State>());
                var mapper = config.CreateMapper();

                var obj = SerializationApi.Deserialize<State>(Path.Combine(settingsPath, "StateSettings.json"));
                if (obj != null)
                {
                    mapper.Map(obj, this);
                    NodeColors = obj.NodeColors.OrderBy(x => x.Key).ToDictionary<string, Color>();
                    NameColors = obj.NameColors.OrderBy(x => x.Key).ToDictionary<string, Color>();
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