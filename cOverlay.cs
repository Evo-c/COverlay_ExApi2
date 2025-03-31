using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using ExileCore2;
using ExileCore2.PoEMemory.Elements.AtlasElements;
using ExileCore2.PoEMemory.MemoryObjects;
using ExileCore2.Shared;
using GameOffsets2.Native;
using RectangleF = ExileCore2.Shared.RectangleF;
using Vector2 = System.Numerics.Vector2;

namespace cOverlay
{
    public partial class cOverlay : BaseSettingsPlugin<cOverlaySettings>
    {
        public State state = new State();
        private bool isAtlasPanelOpen = false;
        private HashSet<Node> nodesToDraw = new HashSet<Node>();
        private HashSet<AtlasNodeDescription> atlasNodes = new HashSet<AtlasNodeDescription>();
        private HashSet<Node> processingNodes = new HashSet<Node>();
        private HashSet<(Vector2i, Vector2i, Vector2i, Vector2i, Vector2i)> atlasPoints = new HashSet<(Vector2i, Vector2i, Vector2i, Vector2i, Vector2i)>();
        public Stopwatch swRefresh = Stopwatch.StartNew();
        public Stopwatch contentSw = Stopwatch.StartNew();
        public Stopwatch swRun = Stopwatch.StartNew();
        private AtlasPanel atlasPanel;

        public override bool Initialise()
        {
            state.Init();
            state.Load();
            atlasPanel = GameController.IngameState.IngameUi.WorldMap.AtlasPanel;

            return true;
        }

        public override void AreaChange(AreaInstance area)
        {
            atlasNodes.Clear();
            processingNodes.Clear();
            //Perform once-per-zone processing here
            //For example, Radar builds the zone map texture here
        }

        public bool IsInScreen(AtlasNodeDescription node)
        {
            var nodeElement = node.Element.Center;
            return nodeElement.X > 0 && nodeElement.X < state.borderX && nodeElement.Y > 0 && nodeElement.Y < state.borderY;
        }
        public void ClearAtlasNodes(ref HashSet<AtlasNodeDescription> _atlasNodes)
        {
            _atlasNodes.Clear();
        }
        public override void Tick()
        {
            isAtlasPanelOpen = atlasPanel.IsVisible;
            var counter = 0;
            if (isAtlasPanelOpen)
            {
                if (swRefresh.ElapsedMilliseconds > 5000 || counter == 0)
                {
                    var atlasNodeCount = atlasNodes.Count;
                    var atlasDescCount = atlasPanel.Descriptions.Count;
                    if (atlasNodeCount < atlasDescCount)
                    {
                        atlasNodes = new HashSet<AtlasNodeDescription>(atlasPanel.Descriptions);
                        atlasPoints = new HashSet<(Vector2i, Vector2i, Vector2i, Vector2i, Vector2i)>(atlasPanel.Points);

                        counter++;
                    }

                    //if (atlasNodeCount > atlasDescCount)
                    //{
                    //    atlasNodes.Clear();
                    //    atlasPoints.Clear();
                    //}

                    swRefresh.Restart();
                }

                if (swRun.ElapsedMilliseconds > 250)
                {
                    foreach (var node in atlasNodes)
                    {
                        if (!IsInScreen(node))
                        {
                            processingNodes.RemoveWhere(x => x.ID == node.Address);
                            continue;
                        }

                        if (!processingNodes.Any(x => x.ID == node.Address)
                            && !node.Element.IsCompleted)
                        {
                            List<AtlasNodeDescription> nNodes = new List<AtlasNodeDescription>();

                            var point = atlasPoints.FirstOrDefault(x => x.Item1 == node.Coordinate);
                            foreach (var nd in atlasNodes)
                            {
                                if (nd.Coordinate == point.Item2 ||
                                    nd.Coordinate == point.Item3 ||
                                    nd.Coordinate == point.Item4 ||
                                    nd.Coordinate == point.Item5)
                                {
                                    nNodes.Add(nd);
                                }
                            }

                            var nearbyTowers = atlasNodes.Where(x => x.Element.IsTower && Vector2.Distance(x.Coordinate, node.Coordinate) <= 11);
                            var affectedTowers = nearbyTowers.Where(x => atlasPanel.EffectSources.Any(y => x.Coordinate == y.Coordinate));

                            //LogMessage($"Node at {node.Coordinate} added to processing");
                            processingNodes.Add(new Node(node.Address, node, nNodes.ToList(), nearbyTowers.Count(), affectedTowers.Count()));
                            nNodes.Clear();
                        };
                    }

                    swRun.Restart();
                }
            }
        }

        //Perform non-render-related work here, e.g. position calculation.
        //var a = Math.Sqrt(7);

        public override void Render()
        {
            Vector2 v1 = new Vector2(-291, -73);
            Vector2 v2 = new Vector2(-281, -68);
            //Any Imgui or Graphics calls go here.This is called after Tick
            Graphics.DrawText($"atlas nodes {atlasNodes.Count} \ndraw nodes {processingNodes.Count} " +
                $"\nrender tick {PluginManager.Plugins.First(x => x.Name == "cOverlay").RenderDebugInformation.TickAverage}" +
                $"\ntick tick {PluginManager.Plugins.First(x => x.Name == "cOverlay").TickDebugInformation.TickAverage}" +
                $"\n{Vector2.Distance(v1, v2)}" +
                $"\n atlasDesc {atlasPanel.Descriptions.Count}", new Vector2(200, 200), Color.LightGreen);

            if (isAtlasPanelOpen)
            {
                foreach (var nodeObject in processingNodes)
                {
                    DrawConnections(nodeObject);
                }

                foreach (var nodeObject in processingNodes)
                {
                    var node = nodeObject.NodeObject.Element;
                    DrawNode(node);
                    DrawContent(node);
                    DrawNodeMain(nodeObject);
                    //DrawNodeBackground(node);
                    //DrawNodeText(node);
                    DrawNodeTower(nodeObject);
                    //DrawNodeTowerText(nodeObject);
                    //DrawTraversal(nodeObject);
                    if (state.drawDebug)
                    {
                        DrawDebug(nodeObject);
                    }
                }

                if (state.OverlayToggle)
                {
                    RenderOverlay();
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

            //DrawTraversal(node, backgroundRect);
        }

        //public void DrawNodeTowerBackground(Node node)
        //{
        //    var nodeElement = node.NodeObject.Element;
        //    var resultText = nodeElement.IsTower ? "Tower" : $"{node.AffectedTowersCount}/{node.TowersCount}";
        //    var textSize = Graphics.MeasureText(resultText);
        //    var padding = state.paddingTower;
        //    var rounding = state.towerTextRounding;
        //    Graphics.DrawBox(
        //        new Vector2(nodeElement.Center.X - textSize.X / 2 - padding.X, nodeElement.Center.Y - textSize.Y / 2 - textSize.Y - padding.Y),
        //        new Vector2(nodeElement.Center.X + textSize.X / 2 + padding.X, nodeElement.Center.Y + textSize.Y / 2 - textSize.Y + padding.Y),
        //        node.AffectedTowersCount >= state.HighTowerAmountThreshold ? state.HighTowerAmountColorBg : state.BackgroundTowerColor,
        //        rounding);
        //}

        //public void DrawNodeTowerText(Node node)
        //{
        //    var nodeElement = node.NodeObject.Element;
        //    var resultText = nodeElement.IsTower ? "Tower" : $"{node.AffectedTowersCount}/{node.TowersCount}";
        //    var textSize = Graphics.MeasureText(resultText);
        //    var padding = new Vector2(3, 2);
        //    Graphics.DrawText(
        //        resultText,
        //        new Vector2(nodeElement.Center.X, nodeElement.Center.Y - textSize.Y),
        //        node.AffectedTowersCount >= state.HighTowerAmountThreshold ? state.HighTowerAmountColorTxt : state.TowerTextColor,
        //        ExileCore2.Shared.Enums.FontAlign.VerticalCenter | ExileCore2.Shared.Enums.FontAlign.Center);
        //}

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
                $"\nx{nodeElement.X} y{nodeElement.Y}" +
                $"\ncenter x{nodeCenter.X} y{nodeCenter.Y}",
                new Vector2(nodeCenter.X, nodeCenter.Y + 18),
                Color.Black,
                ExileCore2.Shared.Enums.FontAlign.Center | ExileCore2.Shared.Enums.FontAlign.VerticalCenter,
                Color.Green);
        }

        public void DrawConnections(Node node)
        {
            foreach (var ne in node.neighbourNodes)
            {
                if (!ne.Element.IsCompleted)
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

        public override void EntityAdded(Entity entity)
        {
            //If you have a reason to process every entity only once,
            //this is a good place to do so.
            //You may want to use a queue and run the actual
            //processing (if any) inside the Tick method.
        }
    }
}