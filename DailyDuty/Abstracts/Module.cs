﻿using System;
using DailyDuty.Models.Enums;
using KamiLib.Utilities;
using Lumina.Excel;

namespace DailyDuty.Abstracts;

public static class Module
{
    public abstract class DailyModule : BaseModule
    {
        public override ModuleType ModuleType => ModuleType.Daily;
        protected override DateTime GetNextReset() => Time.NextDailyReset();
    }

    public abstract class WeeklyModule : BaseModule
    {
        public override ModuleType ModuleType => ModuleType.Weekly;
        protected override DateTime GetNextReset() => Time.NextWeeklyReset();
    }

    public abstract class SpecialModule : BaseModule
    {
        public override ModuleType ModuleType => ModuleType.Special;
    }

    public abstract class DailyTaskModule<T> : DailyModule where T : ExcelRow
    {
        public override IModuleDataBase ModuleData { get; protected set; } = new ModuleTaskDataBase<T>();
        public override IModuleConfigBase ModuleConfig { get; protected set; } = new ModuleTaskConfigBase<T>();
        protected ModuleTaskDataBase<T> Data => ModuleData as ModuleTaskDataBase<T> ?? new  ModuleTaskDataBase<T>();
        protected ModuleTaskConfigBase<T> Config => ModuleConfig as ModuleTaskConfigBase<T> ?? new ModuleTaskConfigBase<T>();

        public override bool HasTooltip => true;
        public override string TooltipText => string.Join("\n", GetIncompleteRows(Config.TaskConfig, Data.TaskData));
    }

    public abstract class WeeklyTaskModule<T> : WeeklyModule where T : ExcelRow
    {
        public override IModuleDataBase ModuleData { get; protected set; } = new ModuleTaskDataBase<T>();
        public override IModuleConfigBase ModuleConfig { get; protected set; } = new ModuleTaskConfigBase<T>();
        protected ModuleTaskDataBase<T> Data => ModuleData as ModuleTaskDataBase<T> ?? new  ModuleTaskDataBase<T>();
        protected ModuleTaskConfigBase<T> Config => ModuleConfig as ModuleTaskConfigBase<T> ?? new ModuleTaskConfigBase<T>();
        
        public override bool HasTooltip => true;
        public override string TooltipText => string.Join("\n", GetIncompleteRows(Config.TaskConfig, Data.TaskData));
    }

    public abstract class SpecialTaskModule<T> : SpecialModule where T : ExcelRow
    {
        public override IModuleDataBase ModuleData { get; protected set; } = new ModuleTaskDataBase<T>();
        public override IModuleConfigBase ModuleConfig { get; protected set; } = new ModuleTaskConfigBase<T>();
        protected ModuleTaskDataBase<T> Data => ModuleData as ModuleTaskDataBase<T> ?? new  ModuleTaskDataBase<T>();
        protected ModuleTaskConfigBase<T> Config => ModuleConfig as ModuleTaskConfigBase<T> ?? new ModuleTaskConfigBase<T>();
        
        public override bool HasTooltip => true;
        public override string TooltipText => string.Join("\n", GetIncompleteRows(Config.TaskConfig, Data.TaskData));
    }
}

