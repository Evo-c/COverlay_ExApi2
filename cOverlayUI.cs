using ImGuiNET;

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
                ImGui.Checkbox("Toggle overlay", ref state.OverlayToggle);
                if (ImGui.Button("Save settings"))
                {
                    state.Save();
                }
                ImGui.SliderInt("BorderX", ref state.borderX, 500, 2500);
                ImGui.SliderInt("BorderX", ref state.borderY, 500, 1400);
            }

            if (ImGui.CollapsingHeader("Style settings"))
            {
                DrawStyleSettings();
            }

            if (ImGui.CollapsingHeader("Keybinds"))
            {
                DrawKeybindLine("AddWaypointKey", ref state.AddWaypointKey, ref state.AddWaypointToggle);
                DrawKeybindLine("RemoveWaypointKey", ref state.RemoveWaypointKey, ref state.RemoveWaypointToggle);
                DrawKeybindLine("ShowWaypointPanelKey", ref state.ShowWaypointPanelKey, ref state.ShowWaypointPanelToggle);
            }

            if (ImGui.CollapsingHeader("Map settings"))
            {
                DrawMapSettings();
            }

            if (ImGui.CollapsingHeader("Content settings"))
            {
                DrawContentSettings();
            }
        }

        private void DrawStyleSettings()
        {
            var bgAreaColor = ColorConverter.ToVector4(state.BackgroundAreaColor);
            var bgTowerColor = ColorConverter.ToVector4(state.BackgroundTowerColor);
            var textAreaColor = ColorConverter.ToVector4(state.AreaTextColor);
            var textTowerColor = ColorConverter.ToVector4(state.TowerTextColor);
            var highTowerAmountColorBg = ColorConverter.ToVector4(state.HighTowerAmountColorBg);
            var highTowerAmountColorTxt = ColorConverter.ToVector4(state.HighTowerAmountColorTxt);

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

            state.HighTowerAmountColorBg = ColorConverter.FromVector4(highTowerAmountColorBg);
            state.HighTowerAmountColorTxt = ColorConverter.FromVector4(highTowerAmountColorTxt);
            state.BackgroundAreaColor = ColorConverter.FromVector4(bgAreaColor);
            state.BackgroundTowerColor = ColorConverter.FromVector4(bgTowerColor);
            state.AreaTextColor = ColorConverter.FromVector4(textAreaColor);
            state.TowerTextColor = ColorConverter.FromVector4(textTowerColor);
        }

        private void RenderOverlay()
        {
            var wFlags = ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysAutoResize;
            ImGui.Begin("Overlay", wFlags);
            ImGui.Text("Atlas node 1");
            ImGui.SameLine();
            ImGui.Button("Move to atlas node");

            ImGui.Text("Atlas node 2");
            ImGui.SameLine();
            ImGui.Button("Move to atlas node");

            ImGui.Text("Atlas node 3");
            ImGui.SameLine();
            ImGui.Button("Move to atlas node");

            ImGui.End();
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
            if (ImGui.BeginTable("MapSettings", 3, tFlags))
            {
                var cFlags = ImGuiTableColumnFlags.NoHide | ImGuiTableColumnFlags.DefaultSort;
                ImGui.TableSetupColumn("Name", cFlags);
                ImGui.TableSetupColumn("Node color", cFlags);
                ImGui.TableSetupColumn("Name color", cFlags);
                ImGui.TableHeadersRow();

                foreach (var map in state.NodeColors)
                {
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
                }
            }
            ImGui.EndTable();
        }

        private void DrawContentSettings()
        {
            var tFlags = ImGuiTableFlags.RowBg | ImGuiTableFlags.NoHostExtendX | ImGuiTableFlags.PreciseWidths;
            if (ImGui.BeginTable("MapSettings", 3, tFlags))
            {
                var cFlags = ImGuiTableColumnFlags.NoHide | ImGuiTableColumnFlags.DefaultSort;
                ImGui.TableSetupColumn("Name", cFlags);
                ImGui.TableSetupColumn("Content color", cFlags);
                ImGui.TableSetupColumn("Outline width", cFlags);
                ImGui.TableHeadersRow();

                for (int row = 0; row < state.ContentSettings.Count; row++)
                {
                    ImGui.TableNextRow();
                    for (int column = 0; column < 3; column++)
                    {
                        ImGui.TableSetColumnIndex(column);
                        ImGui.PushID(state.ContentSettings[row].Type.ToString());
                        switch (column)
                        {
                            case 0:
                                {
                                    ImGui.Text(state.ContentSettings[row].Type.ToString());
                                    break;
                                }

                            case 1:
                                {
                                    var color = ColorConverter.ToVector4(state.ContentSettings[row].ContentColor);

                                    var bFlags = ImGuiColorEditFlags.AlphaPreview | ImGuiColorEditFlags.DisplayRGB | ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoLabel;
                                    ImGui.ColorEdit4("Node Color", ref color, bFlags);

                                    state.ContentSettings[row].ContentColor = ColorConverter.FromVector4(color);

                                    break;
                                }
                            case 2:
                                {
                                    var width = state.ContentSettings[row].ContentWidth;

                                    ImGui.SliderFloat("Width", ref width, 1, 5);

                                    state.ContentSettings[row].ContentWidth = width;

                                    break;
                                }

                            default:
                                break;
                        }
                        ImGui.PopID();
                    }
                }
                ImGui.EndTable();
            }
        }
    }
}