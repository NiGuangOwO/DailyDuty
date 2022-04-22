﻿using System;
using System.Collections.Generic;
using System.Numerics;
using DailyDuty.Interfaces;
using DailyDuty.Localization;
using DailyDuty.Windows.DailyDutyWindow.SelectionTabBar;
using Dalamud.Interface;
using ImGuiNET;

namespace DailyDuty.Graphical
{
    internal class SelectionPane : IDisposable, IDrawable
    {
        private Vector2 AvailableArea => ImGui.GetContentRegionAvail();
        public float ScreenRatio { get; set; }
        public float Padding { get; set; }

        private ITab? selectedTab = null;

        private readonly List<ITab> tabs = new()
        {
            new FeaturesTab(),

        };

        public void Dispose()
        {

        }

        public void Draw()
        {
            var scaledPadding = Padding * ImGuiHelpers.GlobalScale;
            var moduleSelectionWidth = AvailableArea.X * ScreenRatio - scaledPadding;

            if (ImGui.BeginChild("SelectionPane", new Vector2(moduleSelectionWidth, AvailableArea.Y), true))
            {
                DrawSelectionPane();

                ImGui.EndChild();
            }

            ImGui.SameLine();

            if (ImGui.BeginChild("ConfigurationPane", new Vector2(AvailableArea.X - moduleSelectionWidth - scaledPadding, -1), true))
            {
                if (selectedTab?.SelectedTabItem != null)
                {
                    selectedTab?.SelectedTabItem?.DrawConfigurationPane();
                }
                else
                {
                    var available = ImGui.GetContentRegionAvail() / 2.0f;
                    var textSize = ImGui.CalcTextSize(Strings.Configuration.NoSelectionDescription) / 2.0f;
                    var center = new Vector2(available.X - textSize.X, available.Y - textSize.Y);

                    ImGui.SetCursorPos(center);
                    ImGui.Text(Strings.Configuration.NoSelectionDescription);
                }

                ImGui.EndChild();
            }
        }

        private void DrawSelectionPane()
        {
            ImGui.PushID("SelectionPane");

            if (ImGui.BeginTabBar("SelectionPaneTabBar", ImGuiTabBarFlags.Reorderable))
            {
                foreach (var tab in tabs)
                {
                    if (ImGui.BeginTabItem(tab.TabName))
                    {
                        ImGui.PushID(tab.TabName);

                        selectedTab = tab;

                        var fadedTextColor = GetFadedTextColor();
                        ImGui.TextColored(fadedTextColor, tab.Description);

                        ImGui.Spacing();

                        if (ImGui.BeginChild("SelectionPaneTabBarChild", new Vector2(0,0), false))
                        {
                            tab.DrawTabContents();
                            ImGui.EndChild();
                        }

                        ImGui.PopID();

                        ImGui.EndTabItem();
                    }
                }

                ImGui.EndTabBar();
            }

            ImGui.PopID();
        }

        private Vector4 GetFadedTextColor()
        {
            var userColor = ImGui.GetStyle().Colors[(int) ImGuiCol.Text];

            return userColor with {W = 0.5f};
        }
    }
}
