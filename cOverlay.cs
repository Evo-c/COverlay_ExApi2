using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ExileCore2;
using ExileCore2.PoEMemory;
using ExileCore2.PoEMemory.Components;
using ExileCore2.PoEMemory.Elements.AtlasElements;
using ExileCore2.PoEMemory.MemoryObjects;
using ExileCore2.Shared;
using ExileCore2.Shared.Helpers;
using GameOffsets2.Native;
using ImGuiNET;
using Microsoft.VisualBasic.Devices;
using RectangleF = ExileCore2.Shared.RectangleF;
using Vector2 = System.Numerics.Vector2;

namespace cOverlay
{
    public partial class cOverlay : BaseSettingsPlugin<cOverlaySettings>
    {
        public State state = new State();
        private bool isAtlasPanelOpen = false;
        private HashSet<Node> nodesToDraw = new HashSet<Node>();
        private AtlasNodeDescription[] atlasNodes;
        private AtlasNodeDescription[] emptyTowersList;
        private HashSet<Node> processingNodes = new HashSet<Node>();
        private (Vector2i, Vector2i, Vector2i, Vector2i, Vector2i)[] atlasPoints;

        public Stopwatch contentSw = Stopwatch.StartNew();
        public Stopwatch swRun = Stopwatch.StartNew();
        private AtlasPanel atlasPanel;

        public Stopwatch swRefresh = Stopwatch.StartNew();
        public Stopwatch atlasRefresh = Stopwatch.StartNew();

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        public override bool Initialise()
        {
            state.Init();
            state.Load();
            atlasPanel = GameController.IngameState.IngameUi.WorldMap.AtlasPanel;

            return true;
        }

        public override void AreaChange(AreaInstance area)
        {
            atlasNodes = [];
            atlasPoints = [];
            processingNodes = [];
            //Perform once-per-zone processing here
            //For example, Radar builds the zone map texture here
        }

        public bool IsInScreen(AtlasNodeDescription node)
        {
            var nodeElement = node.Element.Center;
            return nodeElement.X > 0 && nodeElement.X < state.borderX && nodeElement.Y > 0 && nodeElement.Y < state.borderY;
        }

        private bool IsKeyPressed(Keys key)
        {
            return (GetAsyncKeyState((int)key) & 0x8000) != 0;
        }

        private static bool[] _previousKeyStates = new bool[256];

        public static bool IsKeyPressedOnce(Keys key)
        {
            int vKey = (int)key;
            bool isCurrentlyPressed = (GetAsyncKeyState(vKey) & 0x8000) != 0;

            // Key was just pressed (current=true, previous=false)
            bool triggered = isCurrentlyPressed && !_previousKeyStates[vKey];

            _previousKeyStates[vKey] = isCurrentlyPressed; // Update state
            return triggered;
        }

        public override void Tick()
        {
            isAtlasPanelOpen = atlasPanel.IsVisible;
            var counter = 0;

            if (isAtlasPanelOpen)
            {
                if (swRefresh.ElapsedMilliseconds > 5000 || counter == 0)
                {
                    var atlasNodeCount = atlasNodes.Count();
                    var atlasDescCount = atlasPanel.Descriptions.Count;
                    if (atlasNodeCount < atlasDescCount || atlasRefresh.ElapsedMilliseconds > 10000)
                    {
                        atlasNodes = atlasPanel.Descriptions.ToArray();
                        atlasPoints = atlasPanel.Points.ToArray();

                        counter++;
                        atlasRefresh.Restart();
                    }

                    swRefresh.Restart();
                }

                if (swRun.ElapsedMilliseconds > 250)
                {
                    foreach (var node in atlasNodes)
                    {
                        if (!IsInScreen(node))
                        {
                            //LogMessage($"Node at {node.Coordinate} IsInScreen:false. Removing");
                            processingNodes.RemoveWhere(x => x.NodeCoords == node.Coordinate);
                            continue;
                        }

                        if (!processingNodes.Any(x => x.NodeCoords == node.Coordinate)
                            && !node.Element.IsCompleted)
                        {
                            List<AtlasNodeDescription> nNodes = new List<AtlasNodeDescription>();

                            if (state.drawConnections)
                            {
                                var point = atlasPoints.FirstOrDefault(x => x.Item1 == node.Coordinate);
                                foreach (var nd in atlasNodes)
                                {
                                    if (nd.Coordinate == point.Item2 ||
                                        nd.Coordinate == point.Item3 ||
                                        nd.Coordinate == point.Item4 ||
                                        nd.Coordinate == point.Item5)
                                    {
                                        if (!nd.Element.IsCompleted)
                                            nNodes.Add(nd);
                                    }
                                }
                            }

                            var towers = atlasNodes.Where(x => x.Element.IsTower);
                            var nearbyTowers = towers.Where(x => Vector2.Distance(x.Coordinate, node.Coordinate) <= 11);
                            var affectedTowers = nearbyTowers.Where(x => atlasPanel.EffectSources.Any(y => x.Coordinate == y.Coordinate));
                            emptyTowersList = towers.Where(x => !atlasPanel.EffectSources.Any(y => x.Coordinate == y.Coordinate)).ToArray();

                            //LogMessage($"Node at {node.Coordinate} added to processing");
                            processingNodes.Add(new Node(node.Coordinate, node, nNodes.ToList(), nearbyTowers.Count(), affectedTowers.Count()));
                            nNodes.Clear();
                        };
                    }

                    swRun.Restart();
                }
            }
        }

        public override void Render()
        {
            if (state.ShowWaypointMenu)
            {
                DrawWaypointMenu();
            }
            //Any Imgui or Graphics calls go here.This is called after Tick
            Graphics.DrawText($"atlas nodes {atlasNodes.Count()} \ndraw nodes {processingNodes.Count} " +
                $"\nrender tick {PluginManager.Plugins.First(x => x.Name == "cOverlay").RenderDebugInformation.TickAverage}" +
                $"\ntick tick {PluginManager.Plugins.First(x => x.Name == "cOverlay").TickDebugInformation.TickAverage}" +
                $"\n atlasDesc {atlasPanel.Descriptions.Count}" +
                $"\n{atlasPanel.Camera.Snapshot.Matrix.Translation}", new Vector2(200, 200), Color.LightGreen);

            if (isAtlasPanelOpen)
            {
                if (state.drawConnections)
                {
                    foreach (var nodeObject in processingNodes)
                    {
                        DrawConnections(nodeObject);
                    }
                }

                foreach (var nodeObject in processingNodes)
                {
                    var node = nodeObject.NodeObject.Element;
                    if (!node.IsTower)
                    {
                        DrawNode(node);
                        DrawContent(node);
                        DrawNodeMain(nodeObject);
                        DrawNodeTower(nodeObject);
                    } else
                    {
                        Graphics.DrawTextWithBackground("I AM TOWER", new Vector2(node.Center.X, node.Center.Y), Color.Yellow, ExileCore2.Shared.Enums.FontAlign.Center | ExileCore2.Shared.Enums.FontAlign.VerticalCenter, Color.Black);
                    }

                    if (state.drawDebug)
                    {
                        DrawDebug(nodeObject);
                    }
                }

                foreach (var tower in emptyTowersList)
                {
                    if (tower.Element.IsCompleted)
                        Graphics.DrawTextWithBackground("MISSING TABLET", new Vector2(tower.Element.Center.X, tower.Element.Center.Y), Color.Red, ExileCore2.Shared.Enums.FontAlign.Center | ExileCore2.Shared.Enums.FontAlign.VerticalCenter, Color.Black);
                }

                if (IsKeyPressedOnce(Keys.F1))
                {
                    LogMessage($"Key {state.AddWaypointKey} pressed");
                    CreateWaypoint();
                }
            }
        }

        public void DrawNode(AtlasPanelNode node)
        {
            var nodeRadius = state.NodeRadius;
            Graphics.DrawCircleFilled(new Vector2(node.Center.X, node.Center.Y), nodeRadius, state.NodeColors[node.Area.Name], 20);
        }

        public void DrawNodeMain(Node _node)
        {
            var node = _node.NodeObject.Element;
            var textSize = Graphics.MeasureText(node.Area.Name.ToLower());
            var padding = state.paddingName;
            var rounding = state.nodeTextRounding;
            var backgroundRect = new RectangleF(
                node.Center.X - textSize.X / 2 - padding.X,
                node.Center.Y - textSize.Y / 2 - padding.Y,
                textSize.X + padding.X * 2,
                textSize.Y + padding.Y * 2);

            Graphics.DrawBox(
                backgroundRect,
                node.Content.Any(x => x.Name == "Irradiated" || x.Name == "Map Boss" && node.IsCorrupted) ? Color.White : state.BackgroundAreaColor,
                rounding);

            Graphics.DrawText(
                node.Area.Name,
                backgroundRect.Center,
                node.Content.Any(x => x.Name == "Irradiated" || x.Name == "Map Boss" && node.IsCorrupted) ? Color.Black : state.DrawTextSameColor ? state.AreaTextColor : state.NameColors[node.Area.Name],
                ExileCore2.Shared.Enums.FontAlign.VerticalCenter | ExileCore2.Shared.Enums.FontAlign.Center);

            DrawTraversal(_node, backgroundRect);
        }

        public void DrawNodeTower(Node node)
        {
            var nodeElement = node.NodeObject.Element;
            var resultText = nodeElement.IsTower ? "Tower" : $"{node.AffectedTowersCount}/{node.TowersCount}";
            var textSize = Graphics.MeasureText(resultText);
            var padding = state.paddingTower;
            var rounding = state.towerTextRounding;
            var backgroundRect = new RectangleF(
                nodeElement.Center.X - textSize.X / 2 - padding.X,
                nodeElement.Center.Y - textSize.Y / 2 - textSize.Y - padding.Y,
                textSize.X + padding.X * 2,
                textSize.Y + padding.Y * 2);

            Graphics.DrawBox(
                backgroundRect,
                node.TowersCount >= state.HighTowerAmountThreshold && !node.NodeObject.Element.IsTower ? state.HighTowerAmountColorBg : state.BackgroundTowerColor,
                rounding);

            Graphics.DrawText(
                resultText,
                backgroundRect.Center,
                node.TowersCount >= state.HighTowerAmountThreshold && !node.NodeObject.Element.IsTower ? state.HighTowerAmountColorTxt : state.TowerTextColor,
                ExileCore2.Shared.Enums.FontAlign.VerticalCenter | ExileCore2.Shared.Enums.FontAlign.Center);
        }

        public void DrawContent(AtlasPanelNode node)
        {
            var nodeContent = node.Content;
            if (node.Content != null)
            {
                var textSize = Graphics.MeasureText(node.Area.Name.ToLower());
                var padding = state.paddingName;
                var rounding = state.nodeTextRounding;

                int counter = 0;
                foreach (var content in nodeContent)
                {
                    if (contentSw.ElapsedMilliseconds > 10000)
                    {
                        if (!state.ContentCircleColor.ContainsKey(content.Name))
                        {
                            state.ContentCircleColor.Add(content.Name, Color.White);
                        }
                        if (!state.ContentCircleThickness.ContainsKey(content.Name))
                        {
                            state.ContentCircleThickness.Add(content.Name, 4);
                        }
                        if (!state.ContentToggle.ContainsKey(content.Name))
                        {
                            state.ContentToggle.Add(content.Name, false);
                        }
                        contentSw.Restart();
                    }

                    if (state.ContentToggle[content.Name])
                    {
                        counter++;
                        Graphics.DrawFrame(
                            new Vector2(node.Center.X - textSize.X / 2 - padding.X / 2 - counter * state.ContentCircleThickness[content.Name], node.Center.Y - textSize.Y / 2 - padding.Y / 2 - counter * state.ContentCircleThickness[content.Name]),
                            new Vector2(node.Center.X + textSize.X / 2 + padding.X / 2 + counter * state.ContentCircleThickness[content.Name], node.Center.Y + textSize.Y / 2 + padding.Y / 2 + counter * state.ContentCircleThickness[content.Name]),
                            state.ContentCircleColor[content.Name],
                            state.ContentCircleThickness[content.Name]);
                    }
                }
            }
        }

        public void DrawDebug(Node node)
        {
            var nodeCoords = node.NodeObject.Coordinate;
            var nodeElement = node.NodeObject.Element;
            var nodeCenter = nodeElement.Center;
            Graphics.DrawTextWithBackground(
                $"x{nodeCoords.X} y{nodeCoords.Y}" +
                //$"\nx{nodeElement.X} y{nodeElement.Y}" +
                //$"\ncenter x{nodeCenter.X} y{nodeCenter.Y}" +
                $"\nnodeContent{node.NodeObject.Element.Content.ToList().Count()}",
                new Vector2(nodeCenter.X, nodeCenter.Y + 18),
                Color.Black,
                ExileCore2.Shared.Enums.FontAlign.Center | ExileCore2.Shared.Enums.FontAlign.VerticalCenter,
                Color.Green);
        }

        public void DrawConnections(Node node)
        {
            foreach (var ne in node.neighbourNodes)
            {
                Graphics.DrawLine(new Vector2(node.NodeObject.Element.Center.X, node.NodeObject.Element.Center.Y), new Vector2(ne.Element.Center.X, ne.Element.Center.Y), state.ConnectionsThickness, state.ConnectionsColor);
            }
        }

        public void DrawTraversal(Node node, RectangleF rect)
        {
            if (node.NodeObject.Element.CanTraverse)
            {
                Graphics.DrawLine(rect.TopLeft, rect.TopRight, 5, Color.Aqua);
            }
        }

        public void CreateWaypoint()
        {
            var node = GetClosestToCursorNode();
            LogMessage(node.NodeObject.Coordinate.ToString());
            var PositionX = atlasPanel.Camera.Snapshot.Matrix.Translation.X;
            var PositionY = atlasPanel.Camera.Snapshot.Matrix.Translation.Y;
            var Name = node.NodeObject.Element.Area.Name;
            var TowersCount = node.TowersCount;

            Waypoint waypoint = new Waypoint(PositionX, PositionY, Name, TowersCount, node.NodeCoords);

            state.WaypointList.Add(waypoint);
        }

        public void DeleteWaypoint(Vector2 position)
        {
            state.WaypointList.RemoveAll(x => new Vector2(x.PositionX, x.PositionY) == position);
        }

        public Node GetClosestToCursorNode()
        {
            var cursorPos = new Vector2(GameController.IngameState.MousePosX, GameController.IngameState.MousePosY);
            var closestNode = processingNodes.OrderBy(x => Vector2.Distance(new Vector2(x.NodeObject.Element.Center.X, x.NodeObject.Element.Center.Y), cursorPos)).FirstOrDefault();

            return closestNode;
        }

        public override void EntityAdded(Entity entity)
        {
            //If you have a reason to process every entity only once,
            //this is a good place to do so.
            //You may want to use a queue and run the actual
            //processing (if any) inside the Tick method.
        }
    }
}