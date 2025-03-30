using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ExileCore2;
using ExileCore2.PoEMemory.Elements.AtlasElements;
using ExileCore2.PoEMemory.MemoryObjects;
using Vector2 = System.Numerics.Vector2;

namespace cOverlay
{
    public partial class cOverlay : BaseSettingsPlugin<cOverlaySettings>
    {
        public State state = new State();
        private bool isAtlasPanelOpen = false;
        private HashSet<Node> nodesToDraw = new HashSet<Node>();
        private HashSet<AtlasNodeDescription> atlasNodes = new HashSet<AtlasNodeDescription>();

        public override bool Initialise()
        {
            state.Init();
            state.Load();

            return true;
        }

        public override void AreaChange(AreaInstance area)
        {
            atlasNodes.Clear();
            nodesToDraw.Clear();
            //Perform once-per-zone processing here
            //For example, Radar builds the zone map texture here
        }

        public override void Tick()
        {
            var atlasPanel = GameController.IngameState.IngameUi.WorldMap.AtlasPanel;
            isAtlasPanelOpen = atlasPanel.IsVisible;

            if (isAtlasPanelOpen)
            {
                if (nodesToDraw.Where(x => x.NodeElement.X > state.borderX || x.NodeElement.X < 0 || x.NodeElement.Y > state.borderY || x.NodeElement.Y < 0).Count() > 10 || nodesToDraw.Count < 15)
                {
                    foreach (var node in atlasPanel.Descriptions)
                    {
                        atlasNodes.Add(node);
                    }

                    //foreach (var effect in atlasPanel.EffectSources)
                    //{
                    //    effectSources.Add(effect);
                    //}

                    //foreach (var effectNode in effectSources)
                    //{
                    //    effectNodes = atlasPanel.Descriptions.Where(x => x.Coordinate == effectNode.Coordinate).ToList();
                    //}

                    foreach (var node in atlasNodes)
                    {
                        LogMessage($"{state.NodeColors.Count}");
                        if (!state.NodeColors.ContainsKey(node.Element.Area.Name))
                        {
                            state.NodeColors.Add(node.Element.Area.Name, Color.Red);
                            state.NameColors.Add(node.Element.Area.Name, Color.Green);
                        }
                        var nodeObject = new Node(node.Element, node.Address, state.NodeColors[node.Element.Area.Name], state.NameColors[node.Element.Area.Name]);

                        //if (nodeObject.NodeElement.IsCompleted && !nodeObject.NodeElement.IsTower)
                        //    continue;

                        if (nodeObject.NodeElement.X > 0
                            && nodeObject.NodeElement.X < state.borderX
                            && nodeObject.NodeElement.Y > 0
                            && nodeObject.NodeElement.Y < state.borderY)
                        {
                            if (!nodesToDraw.Any(x =>
                                x.ID == node.Address))
                            {
                                var nearbyTowers = atlasNodes.Where(x => x.Element.IsTower && Vector2.Distance(x.Coordinate, node.Coordinate) <= 11);
                                var affectedTowers = nearbyTowers.Where(x => atlasPanel.EffectSources.Any(y => x.Coordinate == y.Coordinate));
                                nodeObject.TowersCount = nearbyTowers.Count();
                                nodeObject.AffectedTowersCount = affectedTowers.Count();
                                nodesToDraw.Add(nodeObject);
                            }
                        }
                        else
                        {
                            nodesToDraw.RemoveWhere(x => x.ID == node.Address);
                        }
                    }
                }
            }
        }

        //Perform non-render-related work here, e.g. position calculation.
        //var a = Math.Sqrt(7);

        public override void Render()
        {
            //Any Imgui or Graphics calls go here.This is called after Tick
            Graphics.DrawTextWithBackground($"{atlasNodes.Count} {nodesToDraw.Count}", new Vector2(200, 200), Color.DarkGray, Color.Red);

            if (isAtlasPanelOpen)
            {
                foreach (var nodeObject in nodesToDraw)
                {
                    var node = nodeObject.NodeElement;
                    DrawNode(node);
                    DrawContent(node);
                    DrawNodeBackground(node);
                    DrawNodeText(node);
                    DrawNodeTowerBackground(nodeObject);
                    DrawNodeTowerText(nodeObject);
                    var areaName = node.Area.Name;
                }

                if (state.OverlayToggle)
                {
                    RenderOverlay();
                }
            }
        }

        public void DrawNodeBackground(AtlasPanelNode node)
        {
            var textSize = Graphics.MeasureText(node.Area.Name.ToLower());
            var padding = state.paddingName;
            var rounding = state.nodeTextRounding;
            Graphics.DrawBox(
                new Vector2(node.Center.X - textSize.X / 2 - padding.X, node.Center.Y - textSize.Y / 2 - padding.Y),
                new Vector2(node.Center.X + textSize.X / 2 + padding.X, node.Center.Y + textSize.Y / 2 + padding.Y),
                node.Content.Any(x => x.Name == "Irradiated" || x.Name == "Map Boss" && node.IsCorrupted) ? Color.White : state.BackgroundAreaColor, rounding);
        }

        public void DrawNodeText(AtlasPanelNode node)
        {
            Graphics.DrawText(
                node.Area.Name,
                new Vector2(node.Center.X, node.Center.Y),
                node.Content.Any(x => x.Name == "Irradiated" || x.Name == "Map Boss" && node.IsCorrupted) ? Color.Black : state.DrawTextSameColor ? state.AreaTextColor : state.NameColors[node.Area.Name],
                ExileCore2.Shared.Enums.FontAlign.VerticalCenter | ExileCore2.Shared.Enums.FontAlign.Center);
        }

        public void DrawNode(AtlasPanelNode node)
        {
            var nodeRadius = state.NodeRadius;
            Graphics.DrawCircleFilled(new Vector2(node.Center.X, node.Center.Y), nodeRadius, state.NodeColors[node.Area.Name], 20);
        }

        public void DrawNodeTowerBackground(Node node)
        {
            var nodeElement = node.NodeElement;
            var resultText = nodeElement.IsTower ? "Tower" : $"{node.AffectedTowersCount}/{node.TowersCount}";
            var textSize = Graphics.MeasureText(resultText);
            var padding = state.paddingTower;
            var rounding = state.towerTextRounding;
            Graphics.DrawBox(
                new Vector2(nodeElement.Center.X - textSize.X / 2 - padding.X, nodeElement.Center.Y - textSize.Y / 2 - textSize.Y - padding.Y),
                new Vector2(nodeElement.Center.X + textSize.X / 2 + padding.X, nodeElement.Center.Y + textSize.Y / 2 - textSize.Y + padding.Y),
                node.AffectedTowersCount >= state.HighTowerAmountThreshold ? state.HighTowerAmountColorBg : state.BackgroundTowerColor,
                rounding);
        }

        public void DrawNodeTowerText(Node node)
        {
            var nodeElement = node.NodeElement;
            var resultText = nodeElement.IsTower ? "Tower" : $"{node.AffectedTowersCount}/{node.TowersCount}";
            var textSize = Graphics.MeasureText(resultText);
            var padding = new Vector2(3, 2);
            Graphics.DrawText(
                resultText,
                new Vector2(nodeElement.Center.X, nodeElement.Center.Y - textSize.Y),
                node.AffectedTowersCount >= state.HighTowerAmountThreshold ? state.HighTowerAmountColorTxt : state.TowerTextColor,
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

        public override void EntityAdded(Entity entity)
        {
            //If you have a reason to process every entity only once,
            //this is a good place to do so.
            //You may want to use a queue and run the actual
            //processing (if any) inside the Tick method.
        }
    }
}