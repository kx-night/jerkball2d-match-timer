// SPDX-License-Identifier: MPL-2.0
//
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. See the LICENSE file in the repository root for more details.
//
// Copyright (c) 2026 kx-night

using System;
using BenchmarkDotNet.Attributes;
using Jerkball2D;
using Jerkball2D.TimerExtensions;
using static Jerkball2D.TimerExtensions.MatchTimerController;
using static Jerkball2D.TimerExtensions.TimerTypes;

namespace Jerkball2D.Benchmarks;

[MemoryDiagnoser]
[MediumRunJob]
public class TimerExtensionsBenchmark
{
    private const int LoopCount = 10_000;

    private MatchTimer _runningTimer = null!;
    private MatchTimer _pausedTimer = null!;
    private Action _cachedCallback = null!;

    private float[] _randomDeltas = null!;
    private int _index;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _runningTimer = new MatchTimer(90f);
        _runningTimer.Play();

        _pausedTimer = new MatchTimer(90f);
        _pausedTimer.Pause();

        _cachedCallback = static () => { };

        var random = new Random(42);
        _randomDeltas = new float[1024];
        for (int i = 0; i < _randomDeltas.Length; i++)
        {
            _randomDeltas[i] = random.NextSingle() * 2f;
        }
    }

    [IterationSetup]
    public void IterationSetup()
    {
        _runningTimer.Restart();
        _pausedTimer.ResetTo(90f);
        _pausedTimer.Pause();

        _index = 0;
    }

    [Benchmark(Baseline = true, OperationsPerInvoke = LoopCount)]
    public TimerState FSharp_Tick_Running()
    {
        TimerState finalState = TimerState.Idle;

        for (int i = 0; i < LoopCount; i++)
        {
            float delta = NextDelta() * 0.016f;
            finalState = tick(delta, _runningTimer);
        }

        return finalState;
    }

    [Benchmark(OperationsPerInvoke = LoopCount)]
    public TimerState FSharp_Tick_Paused()
    {
        TimerState finalState = TimerState.Idle;

        for (int i = 0; i < LoopCount; i++)
        {
            float delta = NextDelta() * 0.016f;
            finalState = tick(delta, _pausedTimer);
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

    private float NextDelta()
    {
        int index = _index++ & (_randomDeltas.Length - 1);
        return _randomDeltas[index];
    }
}
