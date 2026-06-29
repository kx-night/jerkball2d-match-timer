// SPDX-License-Identifier: MPL-2.0
//
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
//
// Copyright (c) 2026 kx-night

using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Jerkball2D;
using Jerkball2D.TimerExtensions;
using static Jerkball2D.TimerExtensions.MatchTimerController;
using static Jerkball2D.TimerExtensions.TimerTypes;

namespace Jerkball2D.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RunStrategy.Throughput, launchCount: 1, warmupCount: 5, iterationCount: 20)]
public sealed class TimerExtensionsBenchmark
{
    private const int LoopCount = 1000;

    private MatchTimer _runningTimer = null!;
    private MatchTimer _pausedTimer = null!;
    private Action _cachedCallback = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _runningTimer = new MatchTimer(90f);
        _runningTimer.Play();

        _pausedTimer = new MatchTimer(90f);
        _pausedTimer.Pause();

        _cachedCallback = static () => { };
    }

    [IterationSetup]
    public void IterationSetup()
    {
        _runningTimer.Restart();
        _pausedTimer.ResetTo(90f);
        _pausedTimer.Pause();
    }

    [Benchmark(OperationsPerInvoke = LoopCount)]
    public TimerState FSharp_Tick_Running()
    {
        TimerState finalState = TimerState.Idle;
        const float deltaTime = 0.016f;

        for (int i = 0; i < LoopCount; i++)
        {
            finalState = tick(deltaTime, _runningTimer);
        }

        return finalState;
    }

    [Benchmark(OperationsPerInvoke = LoopCount)]
    public TimerState FSharp_Tick_Paused()
    {
        TimerState finalState = TimerState.Idle;
        const float deltaTime = 0.016f;

        for (int i = 0; i < LoopCount; i++)
        {
            finalState = tick(deltaTime, _pausedTimer);
        }

        return finalState;
    }

    [Benchmark(OperationsPerInvoke = LoopCount)]
    public int FSharp_Fluent_Chains()
    {
        int activeCount = 0;

        for (int i = 0; i < LoopCount; i++)
        {
            MatchTimer timer = restart(_runningTimer);
            timer = pause(timer);
            timer = play(timer);

            if (timer.IsRunning)
            {
                activeCount++;
            }
        }

        return activeCount;
    }

    [Benchmark(OperationsPerInvoke = LoopCount)]
    public int FSharp_Subscribe_And_Dispose()
    {
        int lifecycleCount = 0;

        for (int i = 0; i < LoopCount; i++)
        {
            using (onFinished(_cachedCallback, _runningTimer))
            {
                lifecycleCount++;
            }
        }

        return lifecycleCount;
    }
}
