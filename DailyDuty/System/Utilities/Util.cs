﻿using System;
using System.Diagnostics;
using Dalamud.Game.ClientState.Party;
using Dalamud.Game.Text.SeStringHandling;
using FFXIVClientStructs.FFXIV.Client.Game.Object;

namespace DailyDuty.System.Utilities
{
    internal static class Util
    {
        public static unsafe bool IsTargetable(PartyMember partyMember)
        {
            var playerGameObject = partyMember.GameObject;
            if (playerGameObject == null) return false;

            var playerTargetable = ((GameObject*)playerGameObject.Address)->GetIsTargetable();

            return playerTargetable;
        }

        public static unsafe bool IsTargetable(Dalamud.Game.ClientState.Objects.Types.GameObject gameObject)
        {
            var playerTargetable = ((GameObject*)gameObject.Address)->GetIsTargetable();

            return playerTargetable;
        }

        public static void PrintMessage(string message)
        {
            var stringBuilder = new SeStringBuilder();
            stringBuilder.AddUiForeground(45);
            stringBuilder.AddText("[DailyDuty] ");
            stringBuilder.AddUiForegroundOff();
            stringBuilder.AddText(message);

            Service.Chat.Print(stringBuilder.BuiltString);
        }

        public static void PrintWondrousTails(string message)
        {
            var stringBuilder = new SeStringBuilder();
            stringBuilder.AddUiForeground(45);
            stringBuilder.AddText("[DailyDuty] ");
            stringBuilder.AddUiForegroundOff();
            stringBuilder.AddUiForeground(62);
            stringBuilder.AddText("[WondrousTails] ");
            stringBuilder.AddUiForegroundOff();
            stringBuilder.AddText(message);

            Service.Chat.Print(stringBuilder.BuiltString);
        }

        public static void PrintTreasureMap(string message)
        {
            var stringBuilder = new SeStringBuilder();
            stringBuilder.AddUiForeground(45);
            stringBuilder.AddText("[DailyDuty] ");
            stringBuilder.AddUiForegroundOff();
            stringBuilder.AddUiForeground(62);
            stringBuilder.AddText("[TreasureMap] ");
            stringBuilder.AddUiForegroundOff();
            stringBuilder.AddText(message);

            Service.Chat.Print(stringBuilder.BuiltString);
        }

        // Run function immediately, and prevent re-execution for TimeSpan delay time
        public static void UpdateDelayed(Stopwatch stopwatch, TimeSpan delayTime, Action function)
        {
            if (stopwatch.IsRunning && stopwatch.Elapsed >= delayTime)
            {
                stopwatch.Stop();
                stopwatch.Reset();
            }

            if (stopwatch.IsRunning == false)
            {
                stopwatch.Start();
                function();
            }
        }
    }
}