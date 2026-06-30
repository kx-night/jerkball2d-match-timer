// SPDX-License-Identifier: MPL-2.0
//
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. See the LICENSE file in the repository root for more details.
//
// Copyright (c) 2026 kx-night

// Enable nullable reference types so the `Action?` delegate compiles and nullable warnings are reported.
// Required if `<Nullable>enable</Nullable>` is missing from your project configuration.
#nullable enable

using System;
using System.Globalization;

// To use this timer in your game without having the "Jerkball2D"
// namespace scattered across your codebase, you do not need to modify this file.
//
// Instead, add a 'GlobalUsings.cs' file to your own project and include:
//
//     global using MyAwesomeGame = global::Jerkball2D;
//
// This allows you to use 'MyAwesomeGame.MatchTimer' anywhere in your project
// while keeping this source file completely unchanged.
namespace Jerkball2D;

#if !NETSTANDARD2_1_OR_GREATER && !NETCOREAPP3_0_OR_GREATER
internal static class MathF
{
    public static float Max(float a, float b) => Math.Max(a, b);
    public static float Min(float a, float b) => Math.Min(a, b);
    public static float Ceiling(float x) => (float)Math.Ceiling(x);
}
#endif

/// <summary>
/// Countdown timer for tracking match duration and playback states.
/// </summary>
/// <remarks>
/// This timer maintains mutable state and is not thread-safe.
/// Use it from a single thread or protect access with external synchronization.
/// </remarks>
public sealed class MatchTimer
{
    private const int MaxClockMinutes = 99;

    private int _lastDisplayedSecond = -1;
    private string _cachedClockText = "00:00";

    private int _minutes;
    private int _seconds;

    public MatchTimer(float seconds) =>
        Duration = SanitizeDuration(seconds);

    public event Action? OnCompleted;

    public float Duration { get; private set; }
    public float Elapsed { get; private set; }
    public bool IsRunning { get; private set; }
    public bool IsPaused { get; private set; }

    public bool IsCompleted => Elapsed >= Duration;
    public float Remaining => MathF.Max(0f, Duration - Elapsed);
    public int TotalRemainingSeconds => (int)MathF.Ceiling(Remaining);

    public int Minutes
    {
        get
        {
            SyncTime();
            return _minutes;
        }
    }

    public int Seconds
    {
        get
        {
            SyncTime();
            return _seconds;
        }
    }

    public string DigitalClock
    {
        get
        {
            SyncTime();
            return _cachedClockText;
        }
    }

    public void Play()
    {
        if (IsCompleted)
            return;

        IsRunning = true;
        IsPaused = false;
    }

    public void Pause()
    {
        (IsRunning, IsPaused) = (false, true);
    }

    public void Restart()
    {
        (Elapsed, IsRunning, IsPaused) = (0f, true, false);
        _lastDisplayedSecond = -1;
    }

    public void ResetTo(float seconds)
    {
        Duration = SanitizeDuration(seconds);
        Restart();
    }

    public void Update(float deltaTime)
    {
        if (!IsRunning || IsPaused)
            return;

        if (deltaTime <= 0f || !float.IsFinite(deltaTime))
            return;

        Elapsed = MathF.Min(Duration, Elapsed + deltaTime);

        if (IsCompleted)
        {
            IsRunning = false;
            OnCompleted?.Invoke();
        }
    }

    private void SyncTime()
    {
        int currentSeconds = TotalRemainingSeconds;

        if (currentSeconds == _lastDisplayedSecond)
            return;

        _lastDisplayedSecond = currentSeconds;

        _minutes = currentSeconds / 60;
        _seconds = currentSeconds % 60;

        _cachedClockText = string.Create(
            5,
            (_minutes, _seconds),
            static (clockBuffer, time) =>
            {
                time.Item1.TryFormat(clockBuffer[..2], out _, "D2", CultureInfo.InvariantCulture);
                clockBuffer[2] = ':';
                time.Item2.TryFormat(clockBuffer[3..], out _, "D2", CultureInfo.InvariantCulture);
            });
    }

    private static float SanitizeDuration(float seconds) =>
        seconds switch
        {
            _ when seconds <= 0f || !float.IsFinite(seconds) =>
                throw new ArgumentOutOfRangeException(
                    nameof(seconds),
                    "Duration must be a finite value greater than zero."
                ),
            _ when seconds > MaxClockMinutes * 60f =>
                throw new ArgumentOutOfRangeException(
                    nameof(seconds),
                    $"Duration cannot exceed {MaxClockMinutes} minutes."
                ),
            _ => seconds
        };
}
