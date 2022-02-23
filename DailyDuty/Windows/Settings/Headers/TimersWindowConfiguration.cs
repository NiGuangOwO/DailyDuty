﻿using DailyDuty.Components.Graphical;
using DailyDuty.Data.Graphical;
using DailyDuty.Data.SettingsObjects.Timers;
using DailyDuty.Data.SettingsObjects.WindowSettings;
using DailyDuty.Interfaces;
using DailyDuty.Utilities;
using Dalamud.Interface;
using ImGuiNET;

namespace DailyDuty.Windows.Settings.Headers
{
    internal class TimersWindowConfiguration : ICollapsibleHeader
    {
        private TimersWindowSettings Settings => Service.Configuration.TimersWindowSettings;

        private readonly CountdownTimers countdownTimers;
        public TimersWindowConfiguration()
        {
            var timersList = Service.TimerManager.GetTimersWindowTimers();

            countdownTimers = new CountdownTimers(timersList);
        }

        public void Dispose()
        {
            
        }

        public string HeaderText => "Timers Window Configuration";

        void ICollapsibleHeader.DrawContents()
        {
            ImGui.Indent(15 * ImGuiHelpers.GlobalScale);

            ShowHideWindow();

            DisableEnableClickThrough();

            HideInDuty();

            OpacitySlider();

            if (ImGui.BeginTable("TimersWindowConfigurationTable", 7))
            {
                ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthFixed, 125f * ImGuiHelpers.GlobalScale);
                ImGui.TableSetupColumn("  Fg", ImGuiTableColumnFlags.WidthFixed, 25f * ImGuiHelpers.GlobalScale);
                ImGui.TableSetupColumn("  Bg", ImGuiTableColumnFlags.WidthFixed, 25f * ImGuiHelpers.GlobalScale);
                ImGui.TableSetupColumn(" Txt", ImGuiTableColumnFlags.WidthFixed, 25f * ImGuiHelpers.GlobalScale);
                ImGui.TableSetupColumn(" Style Options", ImGuiTableColumnFlags.WidthFixed, 200f * ImGuiHelpers.GlobalScale);
                ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed, 175f * ImGuiHelpers.GlobalScale);
                ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed, 100f * ImGuiHelpers.GlobalScale);

                ImGui.TableHeadersRow();

                foreach (var timer in countdownTimers.Timers)
                {
                    DrawTimerSettings(timer);
                }
                
                ImGui.EndTable();
            }

            ImGui.Indent(-15 * ImGuiHelpers.GlobalScale);
        }

        private void DrawTimerSettings(ITimer timer)
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            Draw.Checkbox($"{timer.Name}", $"{HeaderText}{timer.Name}Enable", ref timer.Settings.Enabled);

            ImGui.TableNextColumn();
            ImGui.ColorEdit4($"##{HeaderText}{timer.Name}ForegroundColor", ref timer.Settings.TimerStyle.ForegroundColor, ImGuiColorEditFlags.NoInputs);
        
            ImGui.TableNextColumn();
            ImGui.ColorEdit4($"##{HeaderText}{timer.Name}BackgroundColor", ref timer.Settings.TimerStyle.BackgroundColor, ImGuiColorEditFlags.NoInputs);

            ImGui.TableNextColumn();
            ImGui.ColorEdit4($"##{HeaderText}{timer.Name}TextColor", ref timer.Settings.TimerStyle.TextColor, ImGuiColorEditFlags.NoInputs);

            ImGui.TableNextColumn();
            ImGui.SetNextItemWidth(200);

            if (ImGui.BeginCombo($"##{HeaderText}{timer.Name}FormatDropdown", timer.Settings.TimerStyle.Options.Format() ?? "Null!", ImGuiComboFlags.PopupAlignLeft))
            {
                foreach (var options in TimerOptions.OptionsSamples)
                {
                    bool isSelected = options == timer.Settings.TimerStyle.Options;
                    if (ImGui.Selectable(options.Format(), isSelected))
                    {
                        timer.Settings.TimerStyle.Options = options;
                    }

                    if (isSelected)
                    {
                        ImGui.SetItemDefaultFocus();
                    }
                }

                ImGui.EndCombo();
            }

            ImGui.TableNextColumn();
            ImGui.PushItemWidth(175 * ImGuiHelpers.GlobalScale);
            ImGui.SliderInt($"{HeaderText}{timer.Name}Size", ref timer.Settings.TimerStyle.Size, 86, 600);
            ImGui.PopItemWidth();

            ImGui.TableNextColumn();
            if (ImGui.Button($"Reset##{HeaderText}{timer.Name}", ImGuiHelpers.ScaledVector2(100, 25)))
            {
                timer.Settings.TimerStyle = new TimerStyle();
                Service.Configuration.Save();
            }
        }
        
        private void OpacitySlider()
        {
            ImGui.PushItemWidth(150);
            ImGui.DragFloat($"Opacity##{HeaderText}", ref Settings.Opacity, 0.01f, 0.0f, 1.0f);
            ImGui.PopItemWidth();
        }

        private void HideInDuty()
        {
            ImGui.Checkbox("Hide when Bound By Duty", ref Settings.HideInDuty);
        }

        private void DisableEnableClickThrough()
        {
            Draw.Checkbox("Enable Click-through", HeaderText, ref Settings.ClickThrough, "Enables/Disables the ability to move the Timers Window");
        }

        private void ShowHideWindow()
        {
            Draw.Checkbox("Show Timers Window", HeaderText, ref Settings.Open, "Shows/Hides the Timers Window");
        }
    }
}