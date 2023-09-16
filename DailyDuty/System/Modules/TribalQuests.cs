﻿using DailyDuty.Abstracts;
using DailyDuty.Models;
using DailyDuty.Models.Enums;
using DailyDuty.Models.ModuleData;
using DailyDuty.System.Localization;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace DailyDuty.System;

public unsafe class TribalQuests : Module.DailyModule
{
    public override ModuleName ModuleName => ModuleName.TribalQuests;

    public override IModuleConfigBase ModuleConfig { get; protected set; } = new TribalQuestsConfig();
    public override IModuleDataBase ModuleData { get; protected set; } = new TribalQuestsData();
    private TribalQuestsConfig Config => ModuleConfig as TribalQuestsConfig ?? new TribalQuestsConfig();
    private TribalQuestsData Data => ModuleData as TribalQuestsData ?? new TribalQuestsData();

    public override void Update()
    {
        Data.RemainingAllowances = TryUpdateData(Data.RemainingAllowances, QuestManager.Instance()->GetBeastTribeAllowance());
        
        base.Update();
    }

    public override void Reset()
    {
        Data.RemainingAllowances = 12;
        
        base.Reset();
    }

    protected override ModuleStatus GetModuleStatus() => Config.ComparisonMode switch
    {
        ComparisonMode.LessThan when Config.NotificationThreshold > Data.RemainingAllowances => ModuleStatus.Complete,
        ComparisonMode.EqualTo when Config.NotificationThreshold == Data.RemainingAllowances => ModuleStatus.Complete,
        ComparisonMode.LessThanOrEqual when Config.NotificationThreshold >= Data.RemainingAllowances => ModuleStatus.Complete,
        _ => ModuleStatus.Incomplete
    };
    
    protected override StatusMessage GetStatusMessage() => new()
    {
        Message = $"{Data.RemainingAllowances} {Strings.AllowancesRemaining}",
    };
}