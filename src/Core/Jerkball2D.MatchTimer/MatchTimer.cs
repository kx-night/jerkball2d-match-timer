// SPDX-License-Identifier: MPL-2.0
//
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
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

/// <summary>
/// Countdown timer for tracking match duration and playback states.
/// </summary>
/// <remarks>
/// WARNING: This timer maintains mutable state and is not thread-safe.
/// Use it from a single thread or protect access with external synchronization.
///
/// Usage guidelines:
/// 1. Read the clock string before numeric values. When updating UI,
///    read <see cref="DigitalClock"/> before accessing the numeric properties.
/// 2. Avoid using numeric properties for game logic. The <c>_minutes</c> and
///    <c>_seconds</c> fields are updated only inside the <see cref="DigitalClock"/>
///    property. Reading <see cref="Minutes"/> or <see cref="Seconds"/> without
///    reading <see cref="DigitalClock"/> first may return stale values.
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

    public int Minutes => _minutes;
    public int Seconds => _seconds;

    public string DigitalClock
    {
        get
        {
            int currentSeconds = TotalRemainingSeconds;

            if (currentSeconds != _lastDisplayedSecond)
            {
                _lastDisplayedSecond = currentSeconds;

                _minutes = Math.Min(currentSeconds / 60, MaxClockMinutes);
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

            return _cachedClockText;
        }
    }

    public void Play()
    {
        if (IsCompleted)
        {
            ResetAndPlay();
            return;
        }

        IsRunning = true;
        IsPaused = false;
    }

    public void Pause()
    {
        (IsRunning, IsPaused) = (false, true);
    }

    public void ResetAndPlay()
    {
        (Elapsed, IsRunning, IsPaused) = (0f, true, false);
        _lastDisplayedSecond = -1;
    }

    public void ResetTo(float seconds)
    {
        Duration = SanitizeDuration(seconds);
        ResetAndPlay();
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
