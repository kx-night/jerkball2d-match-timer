// SPDX-License-Identifier: MPL-2.0
//
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at [https://mozilla.org/MPL/2.0/](https://mozilla.org/MPL/2.0/).
//
// Copyright (c) 2026 kx-night

using System;
using Xunit;
using Jerkball2D;

namespace Jerkball2D.MatchTimer.UnitTests;

public sealed class MatchTimerTest
{
    [Theory]
    [InlineData(0f)]
    [InlineData(-1f)]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    [InlineData(float.NegativeInfinity)]
    [InlineData(6000f)]
    public void Ctor_InvalidDuration_Throws(float invalidDuration) =>
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new MatchTimer(invalidDuration));

    [Fact]
    public void Ctor_InitializesStopped()
    {
        var timer = new MatchTimer(10f);

        Assert.Equal(10f, timer.Duration);
        Assert.Equal(0f, timer.Elapsed);
        Assert.False(timer.IsRunning);
        Assert.False(timer.IsPaused);
        Assert.False(timer.IsCompleted);
        Assert.Equal("00:10", timer.DigitalClock);
    }

    [Fact]
    public void Play_StartsTimer()
    {
        var timer = new MatchTimer(5f);

        timer.Play();

        Assert.True(timer.IsRunning);
        Assert.False(timer.IsPaused);
        Assert.False(timer.IsCompleted);
    }

    [Fact]
    public void Pause_PausesTimer()
    {
        var timer = new MatchTimer(5f);

        timer.Play();
        timer.Pause();

        Assert.False(timer.IsRunning);
        Assert.True(timer.IsPaused);
        Assert.False(timer.IsCompleted);
    }

    [Fact]
    public void Play_WhenCompleted_RestartsTimer()
    {
        var timer = new MatchTimer(5f);

        timer.Play();
        timer.Update(5f);

        timer.Play();

        Assert.True(timer.IsRunning);
        Assert.False(timer.IsCompleted);
        Assert.False(timer.IsPaused);
        Assert.Equal(0f, timer.Elapsed, precision: 5);
        Assert.Equal("00:05", timer.DigitalClock);
    }

    [Fact]
    public void RestartsTimer()
    {
        var timer = new MatchTimer(10f);

        timer.Play();
        timer.Update(3f);

        timer.Restart();

        Assert.True(timer.IsRunning);
        Assert.False(timer.IsPaused);
        Assert.False(timer.IsCompleted);
        Assert.Equal(0f, timer.Elapsed, precision: 5);
        Assert.Equal("00:10", timer.DigitalClock);
    }

    [Fact]
    public void ResetTo_ReconfiguresAndStarts()
    {
        var timer = new MatchTimer(10f);

        timer.Play();
        timer.Update(3f);

        timer.ResetTo(20f);

        Assert.Equal(20f, timer.Duration);
        Assert.Equal(0f, timer.Elapsed, precision: 5);
        Assert.True(timer.IsRunning);
        Assert.False(timer.IsPaused);
        Assert.False(timer.IsCompleted);
        Assert.Equal("00:20", timer.DigitalClock);
    }

    [Theory]
    [InlineData(0f)]
    [InlineData(-1f)]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    [InlineData(float.NegativeInfinity)]
    [InlineData(6000f)]
    public void ResetTo_InvalidDuration_Throws(float invalidDuration)
    {
        var timer = new MatchTimer(10f);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => timer.ResetTo(invalidDuration));
    }

    [Fact]
    public void Update_Ignored_WhenNotRunning()
    {
        var timer = new MatchTimer(10f);

        timer.Update(1f);

        Assert.False(timer.IsRunning);
        Assert.False(timer.IsPaused);
        Assert.False(timer.IsCompleted);
        Assert.Equal(0f, timer.Elapsed, precision: 5);
        Assert.Equal("00:10", timer.DigitalClock);
    }

    [Theory]
    [InlineData(0f)]
    [InlineData(-1f)]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    [InlineData(float.NegativeInfinity)]
    public void Update_IgnoresInvalidDelta(float invalidDelta)
    {
        var timer = new MatchTimer(10f);

        timer.Play();
        timer.Update(invalidDelta);

        Assert.True(timer.IsRunning);
        Assert.False(timer.IsPaused);
        Assert.False(timer.IsCompleted);
        Assert.Equal(0f, timer.Elapsed, precision: 5);
        Assert.Equal("00:10", timer.DigitalClock);
    }

    [Fact]
    public void Update_AdvancesElapsed()
    {
        var timer = new MatchTimer(5f);

        timer.Play();

        timer.Update(2f);
        timer.Update(2f);

        Assert.True(timer.IsRunning);
        Assert.False(timer.IsCompleted);
        Assert.Equal(4f, timer.Elapsed, precision: 5);
        Assert.Equal("00:01", timer.DigitalClock);
    }

    [Fact]
    public void Update_CompletesWhenExpired()
    {
        var timer = new MatchTimer(1f);
        int completedCount = 0;

        timer.OnCompleted += () => completedCount++;
        timer.Play();

        timer.Update(0.5f);
        timer.Update(0.5f);

        Assert.False(timer.IsRunning);
        Assert.False(timer.IsPaused);
        Assert.True(timer.IsCompleted);
        Assert.Equal(1f, timer.Elapsed, precision: 5);
        Assert.Equal(1, completedCount);
        Assert.Equal("00:00", timer.DigitalClock);
    }

    [Fact]
    public void DigitalClock_ValuesAreConsistent()
    {
        var timer = new MatchTimer(90f);

        timer.Play();
        timer.Update(30f);

        Assert.Equal(60f, timer.Remaining, precision: 5);
        Assert.Equal(60, timer.TotalRemainingSeconds);
        Assert.Equal(1, timer.Minutes);
        Assert.Equal(0, timer.Seconds);
        Assert.Equal("01:00", timer.DigitalClock);
    }

    [Fact]
    public void DigitalClock_CachesUntilSecondChanges()
    {
        var timer = new MatchTimer(90f);

        timer.Play();
        timer.Update(30f);

        string firstRead = timer.DigitalClock;
        string secondRead = timer.DigitalClock;

        Assert.Equal("01:00", firstRead);
        Assert.Same(firstRead, secondRead);

        timer.Update(0.016f);
        string subSecondRead = timer.DigitalClock;

        Assert.Same(firstRead, subSecondRead);

        timer.Update(1f);
        string nextSecondRead = timer.DigitalClock;

        Assert.NotSame(firstRead, nextSecondRead);
        Assert.Equal("00:59", nextSecondRead);
    }

    [Fact]
    public void DigitalClock_AllowsMaxDuration()
    {
        float maxValidSeconds = 99f * 60f;
        var timer = new MatchTimer(maxValidSeconds);

        string clock = timer.DigitalClock;

        Assert.Equal("99:00", clock);
    }

    [Fact]
    public void DigitalClock_ThrowsWhenDurationExceedsMax()
    {
        float aboveMaxSeconds = (99f * 60f) + 60f;

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new MatchTimer(aboveMaxSeconds));
    }
}
