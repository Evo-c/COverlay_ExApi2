using System.Numerics;
using System.Runtime.CompilerServices;
using ImGuiNET;
using static System.Net.Mime.MediaTypeNames;

namespace cOverlay
{
    public partial class cOverlay
    {
        public override void DrawSettings()
        {
            BuildSettings();
        }

        private void BuildSettings()
        {
            if (ImGui.CollapsingHeader("General"))
            {
                if (ImGui.Button("Save settings"))
                {
                    state.Save();
                }
                ImGui.SliderInt("BorderX", ref state.borderX, 500, 2500);
                ImGui.SliderInt("BorderY", ref state.borderY, 500, 1400);
            }

            if (ImGui.CollapsingHeader("Style settings"))
            {
                DrawStyleSettings();
            }

            if (ImGui.CollapsingHeader("Keybinds"))
            {
                ImGui.Text("Add waypoint key " + state.AddWaypointKey.ToString());
                ImGui.Text("Remove waypoint key " + state.RemoveWaypointKey.ToString());
            }

            if (ImGui.CollapsingHeader("Map settings"))
            {
                DrawMapSettings();
            }

            if (ImGui.CollapsingHeader("Content settings"))
            {
                DrawContentSettings();
            }

            if (ImGui.CollapsingHeader("Waypoints"))
            {
                ImGui.SliderFloat("Waypoint menu position x", ref state.WaypointWindowPos.X, 0, 1920, "x = %.0f");
                ImGui.SliderFloat("Waypoint menu position y", ref state.WaypointWindowPos.Y, 0, 1080, "y = %.0f");
                ImGui.Checkbox("Show waypoint menu", ref state.ShowWaypointMenu);
            }
        }

        private void CenterText(string text)
        {
            //ImGui.TableSetColumnIndex(ImGui.GetColumnIndex() + 1);
            var textSize = ImGui.CalcTextSize(text);
            var cellSize = new Vector2(ImGui.GetColumnWidth(), 35);
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + (cellSize.X - textSize.X) * 0.5f);
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + (cellSize.Y - textSize.Y) * 0.5f);
        }

        private void DrawWaypointMenu()
        {
            var mFlags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize;
            var window = GameController.Window.GetWindowRectangleTimeCache.TopRight;

            if (ImGui.Begin("Waypoint menu", ref state.ShowWaypointMenu, mFlags))
            {
                ImGui.SetWindowPos(state.WaypointWindowPos);
                var tFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.RowBg;
                if (ImGui.BeginTable("Waypoints", 6, tFlags))
                {
                    var cFlags = ImGuiTableColumnFlags.NoClip | ImGuiTableColumnFlags.WidthFixed;
                    ImGui.TableSetupColumn("   imbo   ", cFlags);

                    ImGui.TableSetupColumn(" Towers ", cFlags);

                    ImGui.TableSetupColumn("       Name       ", cFlags);

                    ImGui.TableSetupColumn("Dist", cFlags);

                    ImGui.TableSetupColumn("", cFlags);

                    ImGui.TableSetupColumn(" ", cFlags);

                    ImGui.TableHeadersRow();

                    foreach (var waypoint in state.WaypointList)
                    {
                        ImGui.PushID(waypoint.PositionX.ToString());

                        ImGui.TableNextColumn();
                        CenterText("Move to");
                        if (ImGui.Button("Move to"))
                        {
                            DeleteWaypoint(new Vector2(waypoint.PositionX, waypoint.PositionY));
                        }

                        ImGui.TableNextColumn();
                        CenterText(waypoint.TowersCount.ToString());
                        ImGui.Text(waypoint.TowersCount.ToString());

                        ImGui.TableNextColumn();
                        CenterText(waypoint.Name);
                        ImGui.Text(waypoint.Name);

                        ImGui.TableNextColumn();
                        DrawWorldDirectionIndicator(
                            new Vector2(waypoint.PositionX, -waypoint.PositionY),
                            new Vector2(atlasPanel.Camera.Snapshot.Matrix.Translation.X, -atlasPanel.Camera.Snapshot.Matrix.Translation.Y)
                            );
                        ImGui.TableNextColumn();

                        CenterText("[X]");
                        if (ImGui.Button("[X]"))
                        {
                            DeleteWaypoint(new Vector2(waypoint.PositionX, waypoint.PositionY));
                        }

                        ImGui.TableNextRow();

                        ImGui.PopID();
                    }
                }
                ImGui.EndTable();
            }

            ImGui.End();
        }

        public void DrawWorldDirectionIndicator(Vector2 worldCoord1, Vector2 worldCoord2)
        {
            // Calculate direction vector
            Vector2 direction = worldCoord2 - worldCoord1;
            float distance = direction.Length();
            direction = Vector2.Normalize(direction);
            CenterText($"{distance / 1000:N0}");
            ImGui.Text($"{distance / 1000:N0}");
            ImGui.TableNextColumn();
            ImGui.TableSetColumnIndex(4);
            // Draw small arrow widget
            Vector2 arrowSize = new Vector2(45, 45); // Size of our indicator
            Vector2 center = ImGui.GetCursorScreenPos() + arrowSize / 2;

            ImDrawListPtr drawList = ImGui.GetWindowDrawList();

            // Main direction line
            Vector2 arrowEnd = center + direction * (arrowSize.X / 2 - 5);
            drawList.AddLine(center, arrowEnd, ImGui.GetColorU32(ImGuiCol.Text), 2f);

            // Arrowhead
            Vector2 perp = new Vector2(-direction.Y, direction.X);
            drawList.AddTriangleFilled(
                arrowEnd,
                arrowEnd - direction * 10 + perp * 5,
                arrowEnd - direction * 10 - perp * 5,
                ImGui.GetColorU32(ImGuiCol.Text)
            );
            ImGui.Dummy(new Vector2(30, 40));
        }

        private void DrawStyleSettings()
        {
            var bgAreaColor = ColorConverter.ToVector4(state.BackgroundAreaColor);
            var bgTowerColor = ColorConverter.ToVector4(state.BackgroundTowerColor);
            var textAreaColor = ColorConverter.ToVector4(state.AreaTextColor);
            var textTowerColor = ColorConverter.ToVector4(state.TowerTextColor);
            var highTowerAmountColorBg = ColorConverter.ToVector4(state.HighTowerAmountColorBg);
            var highTowerAmountColorTxt = ColorConverter.ToVector4(state.HighTowerAmountColorTxt);
            var connectionsColor = ColorConverter.ToVector4(state.ConnectionsColor);

            ImGui.SliderInt("Node radius", ref state.NodeRadius, 1, 30);
            ImGui.SliderFloat("Area text padding X", ref state.paddingName.X, 0, 10);
            ImGui.SliderFloat("Area text padding Y", ref state.paddingName.Y, 0, 10);
            ImGui.SliderInt("Area background rounding", ref state.nodeTextRounding, 0, 10);
            ImGui.SliderFloat("Tower text padding X", ref state.paddingTower.X, 0, 10);
            ImGui.SliderFloat("Tower text padding Y", ref state.paddingTower.Y, 0, 10);
            ImGui.SliderInt("Tower background rounding", ref state.towerTextRounding, 0, 10);

            var bFlags = ImGuiColorEditFlags.NoInputs;

            ImGui.SliderInt("High towers amount threshold", ref state.HighTowerAmountThreshold, 2, 10);
            ImGui.ColorEdit4("HighTowerAmountColorBg", ref highTowerAmountColorBg, bFlags);
            ImGui.ColorEdit4("High Tower Amount Text Color", ref highTowerAmountColorTxt, bFlags);
            ImGui.ColorEdit4("Background Area Color", ref bgAreaColor, bFlags);
            ImGui.ColorEdit4("Background Tower Color", ref bgTowerColor, bFlags);
            ImGui.Checkbox("All area text same color", ref state.DrawTextSameColor);
            if (state.DrawTextSameColor)
                ImGui.ColorEdit4("Area Text Color", ref textAreaColor, bFlags);
            ImGui.ColorEdit4("Tower Text Color", ref textTowerColor, bFlags);
            ImGui.SliderInt("Connections thickness", ref state.ConnectionsThickness, 1, 5);
            ImGui.ColorEdit4("Connections Color", ref connectionsColor, bFlags);

            state.HighTowerAmountColorBg = ColorConverter.FromVector4(highTowerAmountColorBg);
            state.HighTowerAmountColorTxt = ColorConverter.FromVector4(highTowerAmountColorTxt);
            state.BackgroundAreaColor = ColorConverter.FromVector4(bgAreaColor);
            state.BackgroundTowerColor = ColorConverter.FromVector4(bgTowerColor);
            state.AreaTextColor = ColorConverter.FromVector4(textAreaColor);
            state.TowerTextColor = ColorConverter.FromVector4(textTowerColor);
            state.ConnectionsColor = ColorConverter.FromVector4(connectionsColor);
        }

        private void DrawKeybindLine(string description, ref ImGuiKey keybind, ref bool menuToggle)
        {
            ImGui.Text(description);
            ImGui.SameLine();

            if (ImGui.Button(keybind.ToString()))
            {
                menuToggle = true;
            }

            if (menuToggle)
            {
                DrawKeybindingUI(ref keybind, ref menuToggle);
            }
        }

        private void DrawKeybindingUI(ref ImGuiKey keyToChange, ref bool menuToggle)
        {
            var wFlags = ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize;
            ImGui.Begin("Keybindings", wFlags);

            ImGui.Text("Press any key...");

            for (ImGuiKey key = ImGuiKey.NamedKey_BEGIN; key < ImGuiKey.NamedKey_END; key++)
            {
                ImGui.SameLine();
                if (ImGui.IsKeyPressed(key) && key != ImGuiKey.MouseLeft)
                {
                    keyToChange = key;
                    menuToggle = false;
                    ImGui.End();

                    return;
                }
            }

            ImGui.End();
        }

        private void DrawMapSettings()
        {
            var tFlags = ImGuiTableFlags.RowBg | ImGuiTableFlags.NoHostExtendX | ImGuiTableFlags.PreciseWidths;
            ImGui.Checkbox("Draw Debug", ref state.drawDebug);
            ImGui.Checkbox("Draw Connections", ref state.drawConnections);
            if (ImGui.BeginTable("MapSettings", 3, tFlags))
            {
                var cFlags = ImGuiTableColumnFlags.NoHide | ImGuiTableColumnFlags.DefaultSort;
                ImGui.TableSetupColumn("Name", cFlags);
                ImGui.TableSetupColumn("Node color", cFlags);
                ImGui.TableSetupColumn("Name color", cFlags);
                ImGui.TableHeadersRow();

                foreach (var map in state.NodeColors)
                {
                    ImGui.PushID(map.Key);
                    var nodeColor = ColorConverter.ToVector4(map.Value);
                    var nameColor = ColorConverter.ToVector4(state.NameColors[map.Key]);
                    var bFlags = ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel;

                    ImGui.TableNextColumn();
                    ImGui.Text($"{map.Key}");
                    ImGui.TableNextColumn();
                    ImGui.ColorEdit4("Node color", ref nodeColor, bFlags);
                    ImGui.TableNextColumn();
                    ImGui.ColorEdit4("Name color", ref nameColor, bFlags);
                    ImGui.TableNextRow();

                    state.NodeColors[map.Key] = ColorConverter.FromVector4(nodeColor);
                    state.NameColors[map.Key] = ColorConverter.FromVector4(nameColor);
                    ImGui.PopID();
                }
            }
            ImGui.EndTable();
        }

        private void DrawContentSettings()
        {
            var tFlags = ImGuiTableFlags.RowBg | ImGuiTableFlags.NoHostExtendX | ImGuiTableFlags.PreciseWidths;
            if (ImGui.BeginTable("ContentSettings", 4, tFlags))
            {
                var cFlags = ImGuiTableColumnFlags.NoHide | ImGuiTableColumnFlags.DefaultSort;
                ImGui.TableSetupColumn("Enabled", cFlags);
                ImGui.TableSetupColumn("Name", cFlags);
                ImGui.TableSetupColumn("Content color", cFlags);
                ImGui.TableSetupColumn("Circle thickness", cFlags);
                ImGui.TableHeadersRow();

                foreach (var content in state.ContentCircleColor)
                {
                    ImGui.PushID(content.Key);
                    var contentColor = ColorConverter.ToVector4(content.Value);
                    var thickness = state.ContentCircleThickness[content.Key];
                    var toggle = state.ContentToggle[content.Key];
                    var bFlags = ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel;

                    ImGui.TableNextColumn();
                    ImGui.Checkbox("Enabled", ref toggle);
                    ImGui.TableNextColumn();
                    ImGui.Text($"{content.Key}");
                    ImGui.TableNextColumn();
                    ImGui.ColorEdit4("Circle color", ref contentColor, bFlags);
                    ImGui.TableNextColumn();
                    ImGui.SliderInt("Circle thickness", ref thickness, 1, 10);
                    ImGui.TableNextRow();

                    state.ContentCircleColor[content.Key] = ColorConverter.FromVector4(contentColor);
                    state.ContentCircleThickness[content.Key] = thickness;
                    state.ContentToggle[content.Key] = toggle;
                    ImGui.PopID();
                }
            }
            ImGui.EndTable();
        }
    }
}