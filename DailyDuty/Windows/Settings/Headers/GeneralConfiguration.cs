﻿using DailyDuty.Interfaces;
using DailyDuty.Utilities;
using DailyDuty.Windows.Settings.Tabs;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using ImGuiNET;

namespace DailyDuty.Windows.Settings.Headers
{
    internal class GeneralConfiguration : ICollapsibleHeader
    {
        public void Dispose()
        {
            
        }

        public string HeaderText => "General Configuration";

        void ICollapsibleHeader.DrawContents()
        {
            ImGui.Indent(15 * ImGuiHelpers.GlobalScale);

            Draw.Checkbox("Temporary Edit Mode", HeaderText, ref ConfigurationTabItem.EditModeEnabled, 
                "Allows you to manually correct the values stored in each of Daily/Weekly tabs\n" +
                "Edit Mode automatically disables when you close this window\n" +
                "Only use Edit Mode to correct errors in other tabs");

            NotificationDelaySettings();

            Draw.Checkbox("Enable Chat Links", HeaderText, ref Service.Configuration.System.ClickableLinks, "Provides quick-action links for some DailyDuty notifications");

            OpacitySlider();

            Draw.Checkbox("Save Debug Printout", HeaderText, ref Service.Configuration.System.ShowSaveDebugInfo, "Enable to show a debug message whenever Daily Duty saves changes to DailyDuty.json");

            Draw.Checkbox("Enable Debug Output", HeaderText, ref Service.Configuration.System.EnableDebugOutput, "Enable Messages with the [Debug] tag");

            ImGui.Indent(-15 * ImGuiHelpers.GlobalScale);
        }

        private void OpacitySlider()
        {
            ImGui.Text("Opacity");
            ImGui.SameLine();

            ImGui.PushItemWidth(175 * ImGuiHelpers.GlobalScale);
            ImGui.DragFloat($"##{HeaderText}", ref Service.Configuration.SettingsWindowSettings.Opacity, 0.01f, 0.0f, 1.0f);
            ImGui.PopItemWidth();
        }

        private void NotificationDelaySettings()
        {
            ImGui.PushItemWidth(23 * ImGuiHelpers.GlobalScale);
            ImGui.InputInt($"##ZoneChangeDelay{HeaderText}",
                ref Service.Configuration.System.ZoneChangeDelayRate, 0, 0);

            ImGui.PopItemWidth();
            ImGui.SameLine();
            ImGui.Text("Zone Changes Before Resending Notifications");

            ImGuiComponents.HelpMarker("Prevents sending notifications until this many zone changes have happened\n" +
                                       "1: Notify on Every Zone Change\n" +
                                       "10: Notify on Every 10th Zone change\n" +
                                       "Minimum: 1\n" +
                                       "Maximum: 10");

            if (Service.Configuration.System.ZoneChangeDelayRate < 1)
            {
                Service.Configuration.System.ZoneChangeDelayRate = 1;
            }
            else if (Service.Configuration.System.ZoneChangeDelayRate > 10)
            {
                Service.Configuration.System.ZoneChangeDelayRate = 10;
            }
        }
    }
}