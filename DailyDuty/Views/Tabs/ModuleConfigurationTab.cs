﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using DailyDuty.Abstracts;
using DailyDuty.Models.Enums;
using DailyDuty.System;
using DailyDuty.System.Localization;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility;
using ImGuiNET;
using KamiLib.Interfaces;
using KamiLib.Utility;

namespace DailyDuty.Views.Tabs;

public class ModuleConfigurationTab : ISelectionWindowTab
{
    public string TabName => Strings.ModuleConfiguration;
    public ISelectable? LastSelection { get; set; }
    
    private ModuleType? filterType; 
    
    public IEnumerable<ISelectable> GetTabSelectables()
    {
        return DailyDutySystem.ModuleController
            .GetModules(filterType)
            .Select(module => new ConfigurationSelectable(module))
            .OrderBy(module => module.Module.ModuleName.Label());
    }

    public void DrawTabExtras()
    {
        ImGui.PushItemWidth(ImGui.GetContentRegionAvail().X);
        
        if (ImGui.BeginCombo("##FilterCombo", filterType?.Label() ?? Strings.Show_All))
        {
            if (ImGui.Selectable(Strings.Show_All, filterType == null))
            {
                filterType = null;
            }
            
            foreach (var value in Enum.GetValues<ModuleType>())
            {
                if (ImGui.Selectable(value.Label(), filterType == value))
                {
                    filterType = value;
                }
            }

            ImGui.EndCombo();
        }
    }
}

public class ConfigurationSelectable : ISelectable, IDrawable
{
    public BaseModule Module;
    public string ID => Module.ModuleName.ToString();
    public IDrawable Contents => this;

    public ConfigurationSelectable(BaseModule module)
    {
        Module = module;
    }
    
    public void DrawLabel()
    {
        var labelRegion = ImGui.GetContentRegionAvail();
        var genericTextSize = ImGui.CalcTextSize("SizingText");
        var itemSpacing = ImGui.GetStyle().ItemSpacing;
        
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, Vector2.Zero);
        
        if (ImGui.BeginTable($"##ModuleNameTable{Module.ModuleName}", 2, ImGuiTableFlags.None, labelRegion with { Y = genericTextSize.Y + itemSpacing.Y } ))
        {
            ImGui.TableSetupColumn("##ModuleName", ImGuiTableColumnFlags.WidthStretch, 4);
            ImGui.TableSetupColumn("##ModuleStatus", ImGuiTableColumnFlags.WidthFixed, GetLongestModuleStatusLength());

            ImGui.TableNextColumn();
            var currentPosition = ImGui.GetCursorPos() + new Vector2(0.0f, - itemSpacing.Y + 1.0f);
            ImGui.SetCursorPos(currentPosition);
            ImGui.Text(Module.ModuleName.Label());

            ImGui.TableNextColumn();
            currentPosition = ImGui.GetCursorPos() + new Vector2(0.0f, - itemSpacing.Y + 1.0f);
            ImGui.SetCursorPos(currentPosition);
            var region = ImGui.GetContentRegionAvail();

            var text = Module.ModuleConfig.ModuleEnabled ? Strings.Enabled : Strings.Disabled;
            var color = Module.ModuleConfig.ModuleEnabled ? KnownColor.ForestGreen : KnownColor.OrangeRed;

            var textSize = ImGui.CalcTextSize(text);
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + region.X - textSize.X);
            ImGui.TextColored(color.Vector(), text);
            
            ImGui.EndTable();
        }
        
        ImGui.PopStyleVar();
        
    }
    
    public void Draw()
    {
        Module.DrawConfig();
    }
    
    private float GetLongestModuleStatusLength()
    {
        var longestStatus = Enum.GetValues<ModuleStatus>().Select(value => ImGui.CalcTextSize(value.Label())).Select(size => size.X).Prepend(0.0f).Max();
        
        var enabledLength = ImGui.CalcTextSize(Strings.Enabled);
        var disabledLength = ImGui.CalcTextSize(Strings.Disabled);

        var longestEnabledDisabled = MathF.Max(enabledLength.X, disabledLength.X);

        return MathF.Max(longestStatus, longestEnabledDisabled);
    }
}