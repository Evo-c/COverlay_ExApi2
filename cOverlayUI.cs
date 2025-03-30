using System.Linq.Expressions;
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