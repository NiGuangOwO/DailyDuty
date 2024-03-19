﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using DailyDuty.Models;
using DailyDuty.Models.Enums;
using DailyDuty.System.Localization;
using DailyDuty.Views.Components;
using Dalamud.Game.Text;
using KamiLib.AutomaticUserInterface;
using KamiLib.FileIO;
using KamiLib.Game;
using Lumina.Excel;

namespace DailyDuty.Abstracts;

public abstract class BaseModule : IDisposable
{
    public abstract IModuleDataBase ModuleData { get; protected set; }
    public abstract IModuleConfigBase ModuleConfig { get; protected set; }
    public abstract ModuleName ModuleName { get; }
    public abstract ModuleType ModuleType { get; }
    public ModuleStatus ModuleStatus => ModuleConfig.Suppressed ? ModuleStatus.Suppressed : GetModuleStatus();
    public virtual void Dispose() { }
    
    protected abstract DateTime GetNextReset();
    protected abstract ModuleStatus GetModuleStatus();
    protected abstract StatusMessage GetStatusMessage();
    protected bool DataChanged;
    protected bool ConfigChanged;
    
    protected XivChatType GetChatChannel() => ModuleConfig.UseCustomChannel ? ModuleConfig.MessageChatChannel : Service.PluginInterface.GeneralChatType;
    private readonly Stopwatch statusMessageLockout = new();
    
    protected virtual void UpdateTaskLists() { }
    public virtual bool HasTooltip { get; protected set; } = false;
    public virtual string TooltipText { get; protected set; } = string.Empty;
    public virtual bool HasClickableLink { get; protected set; } = false;
    public virtual PayloadId ClickableLinkPayloadId { get; protected set; } = PayloadId.Unknown;

    public void DrawConfig()
    {
        DrawableAttribute.DrawAttributes(ModuleConfig, () => {
            SaveConfig();
            ModuleConfig.StyleChanged = true;
        });
    }

    public void DrawData()
    {
        ModuleStatusView.Draw(this);
        DrawableAttribute.DrawAttributes(ModuleData);
        ModuleSuppressionView.Draw(ModuleConfig, SaveConfig);
    }

    public virtual void Update()
    {
        if(DataChanged) SaveData();
        if(ConfigChanged) SaveConfig();

        DataChanged = false;
        ConfigChanged = false;
    }

    public virtual void Load()
    {
        Service.Log.Debug($"[{ModuleName}] Loading Module");
        ModuleData = LoadData();
        ModuleConfig = LoadConfig();

        if (DateTime.UtcNow > ModuleData.NextReset)
        {
            Reset();
        }
        
        UpdateTaskLists();
        
        Update();
        
        if (ModuleConfig is { OnLoginMessage: true, ModuleEnabled: true, Suppressed: false })
        {
            SendStatusMessage();
        }
    }

    public virtual void Unload()
    {
        Service.Log.Debug($"[{ModuleName}] Unloading Module");
        
        statusMessageLockout.Stop();
        statusMessageLockout.Reset();
    }

    public virtual void Reset()
    {
        Service.Log.Debug($"[{ModuleName}] Resetting Module, Next Reset: {GetNextReset().ToLocalTime()}");

        SendResetMessage();
        
        ModuleData.NextReset = GetNextReset();
        SaveData();
        
        ModuleConfig.Suppressed = false;
        SaveConfig();
    }

    public virtual void ZoneChange(uint newZone)
    {
        if (ModuleConfig is { OnZoneChangeMessage: true, ModuleEnabled: true, Suppressed: false })
        {
            SendStatusMessage();
        }
    }
    
    private IModuleConfigBase LoadConfig() => CharacterFileController.LoadFile<IModuleConfigBase>($"{ModuleName}.config.json", ModuleConfig);
    private IModuleDataBase LoadData() => CharacterFileController.LoadFile<IModuleDataBase>($"{ModuleName}.data.json", ModuleData);
    public void SaveConfig() => CharacterFileController.SaveFile($"{ModuleName}.config.json", ModuleConfig.GetType(), ModuleConfig);
    public void SaveData() => CharacterFileController.SaveFile($"{ModuleName}.data.json", ModuleData.GetType(), ModuleData);

    private void SendStatusMessage()
    {
        if (GetModuleStatus() is not (ModuleStatus.Incomplete or ModuleStatus.Unknown)) return;
        if (Condition.IsBoundByDuty()) return;
        if (statusMessageLockout.Elapsed < TimeSpan.FromMinutes(5) && statusMessageLockout.IsRunning)
        {
            Service.Log.Debug($"[{ModuleName}] Suppressing Status Message: {TimeSpan.FromMinutes(5) - statusMessageLockout.Elapsed}");
            return;
        }
        
        Service.Log.Debug($"[{ModuleName}] Sending Status Message");
        
        var statusMessage = GetStatusMessage();
        if (ModuleConfig.UseCustomStatusMessage && GetModuleStatus() != ModuleStatus.Unknown)
        {
            statusMessage.Message = ModuleConfig.CustomStatusMessage;
        }
        statusMessage.SourceModule = ModuleName;
        statusMessage.MessageChannel = GetChatChannel();
        statusMessage.PrintMessage();
        
        statusMessageLockout.Restart();
    }
    
    private void SendResetMessage()
    {
        if (ModuleConfig is not { ResetMessage: true, ModuleEnabled: true, Suppressed: false }) return;
        if (DateTime.UtcNow - ModuleData.NextReset >= TimeSpan.FromMinutes(5)) return;
        
        var statusMessage = GetStatusMessage();
        statusMessage.Message = ModuleConfig.UseCustomResetMessage ? ModuleConfig.CustomResetMessage : Strings.ModuleReset;
        statusMessage.SourceModule = ModuleName;
        statusMessage.MessageChannel = GetChatChannel();
        statusMessage.PrintMessage();
    }

    protected T TryUpdateData<T>(T value, T newValue) where T : IEquatable<T>
    {
        if (!value.Equals(newValue))
        {
            DataChanged = true;
            return newValue;
        }

        return value;
    }
    
    protected static int GetIncompleteCount<T>(LuminaTaskConfigList<T> config, LuminaTaskDataList<T> data) where T : ExcelRow
    {
        if (config.Count != data.Count) throw new Exception("Task and Data array size are mismatched. Unable to calculate IncompleteCount.");

        var count = 0;
        for (var i = 0; i < config.Count; i++)
        {
            var configTask = config.ConfigList[i];
            var dataTask = data.DataList[i];

            if (configTask.RowId != dataTask.RowId) throw new Exception($"Task and Data rows are mismatched. Unable to calculate IncompleteCount.\nConfig RowId: {configTask.RowId} Data RowId: {dataTask.RowId}.");

            if (configTask.Enabled)
            {
                var isCountableTaskIncomplete = configTask.TargetCount != 0 && dataTask.CurrentCount < configTask.TargetCount;
                var isNonCountableTaskIncomplete = configTask.TargetCount == 0 && !dataTask.Complete;
                
                if (isCountableTaskIncomplete || isNonCountableTaskIncomplete) count++;
            }
        }

        return count;
    }

    protected static IEnumerable<string> GetIncompleteRows<T>(LuminaTaskConfigList<T> config, LuminaTaskDataList<T> data) where T : ExcelRow
    {
        if (config.Count != data.Count) throw new Exception("Task and Data array size are mismatched. Unable to calculate IncompleteCount.");

        for (var i = 0; i < config.Count; i++)
        {
            var configTask = config.ConfigList[i];
            var dataTask = data.DataList[i];

            if (configTask.RowId != dataTask.RowId) throw new Exception($"Task and Data rows are mismatched. Unable to calculate IncompleteCount.\nConfig RowId: {configTask.RowId} Data RowId: {dataTask.RowId}.");

            if (configTask.Enabled)
            {
                var isCountableTaskIncomplete = configTask.TargetCount != 0 && dataTask.CurrentCount < configTask.TargetCount;
                var isNonCountableTaskIncomplete = configTask.TargetCount == 0 && !dataTask.Complete;
                
                if (isCountableTaskIncomplete || isNonCountableTaskIncomplete) yield return configTask.Label();
            }
        }
    }
}