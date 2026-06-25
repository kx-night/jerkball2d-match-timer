// SPDX-License-Identifier: MPL-2.0
//
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at [https://mozilla.org/MPL/2.0/](https://mozilla.org/MPL/2.0/).
//
// Copyright (c) 2026 kx-night

using System;
using Jerkball2D;
using Xunit;

namespace Jerkball2dMatchTimer.Tests;

public sealed class MatchTimerTest
{
    [Theory]
    [InlineData(0f)]
    [InlineData(-1f)]
    [InlineData(float.NaN)]
    [InlineData(float.PositiveInfinity)]
    [InlineData(float.NegativeInfinity)]
    [InlineData(1e-45f)]
    [InlineData(6000f)]
    public void Ctor_InvalidDuration_Throws(float invalidDuration)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new MatchTimer(invalidDuration));
    }

    [Fact]
    public void Ctor_ValidDuration_InitializesAsStopped()
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
    public void Play_TransitionsToRunning()
    {
        var timer = new MatchTimer(5f);

        timer.Play();

        Assert.True(timer.IsRunning);
        Assert.False(timer.IsPaused);
        Assert.False(timer.IsCompleted);
    }

    [Fact]
    public void Pause_TransitionsToPaused()
    {
        var timer = new MatchTimer(5f);

        timer.Play();
        timer.Pause();

        Assert.False(timer.IsRunning);
        Assert.True(timer.IsPaused);
        Assert.False(timer.IsCompleted);
    }

    [Fact]
    public void Play_WhenCompleted_Restarts()
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
    public void ResetAndPlay_RestartsTimer()
    {
        var timer = new MatchTimer(10f);

        timer.Play();
        timer.Update(3f);

        timer.ResetAndPlay();

        Assert.True(timer.IsRunning);
        Assert.False(timer.IsPaused);
        Assert.False(timer.IsCompleted);
        Assert.Equal(0f, timer.Elapsed, precision: 5);
        Assert.Equal("00:10", timer.DigitalClock);
    }

    [Fact]
    public void ResetTo_ReconfiguresTimer()
    {
        var timer = new MatchTimer(10f);

        timer.Play();
        timer.Update(3f);

        timer.ResetTo(20f);

        Assert.Equal(20f, timer.Duration);
        Assert.Equal(0f, timer.Elapsed, precision: 5);
        Assert.False(timer.IsRunning);
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
    [InlineData(1e-45f)]
    [InlineData(6000f)]
    public void ResetTo_InvalidDuration_Throws(float invalidDuration)
    {
        var timer = new MatchTimer(10f);

        Assert.Throws<ArgumentOutOfRangeException>(() => timer.ResetTo(invalidDuration));
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
    public void Update_Ignored_WhenDeltaIsInvalid(float invalidDelta)
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
    public void Update_AdvancesElapsed_WhenRunning()
    {
        var timer = new MatchTimer(5f);

        timer.Play();

        timer.Update(2f);
        timer.Update(2f);

        Assert.True(timer.IsRunning);
        Assert.False(timer.IsCompleted);
        Assert.Equal(4f, timer.Elapsed, precision: 5);
        Assert.Equal("00:01", timer.DigitalClock); // 5s - 4s = 1s display
    }

    [Fact]
    public void Update_WhenExpired_CompletesMatch()
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
    public void DigitalClock_Properties_AreConsistent()
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
    public void DigitalClock_CachesString_UntilSecondChanges()
    {
        var timer = new MatchTimer(90f);

        timer.Play();
        timer.Update(30f);

        string firstRead = timer.DigitalClock;
        string secondRead = timer.DigitalClock;

        Assert.Equal("01:00", firstRead);
        Assert.Same(firstRead, secondRead); // Verifies the internal cache hit

        timer.Update(0.016f);
        string subSecondRead = timer.DigitalClock;

        Assert.Same(firstRead, subSecondRead); // Verifies the cache stays hot during sub-second frame ticks

        timer.Update(1f);
        string nextSecondRead = timer.DigitalClock;

        Assert.NotSame(firstRead, nextSecondRead); // Verifies the old cache was safely evicted
        Assert.Equal("00:59", nextSecondRead);     // Verifies the new zero-allocation text layouts match
    }

    [Fact]
    public void DigitalClock_ClampsToMaxLimit()
    {
        float maxValidSeconds = (99f * 60f) + 59f;
        var timer = new MatchTimer(maxValidSeconds);

        string clock = timer.DigitalClock;

        Assert.Equal("99:59", clock);
    }
}
