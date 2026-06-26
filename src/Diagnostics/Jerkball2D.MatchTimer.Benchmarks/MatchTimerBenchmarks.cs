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
using BenchmarkDotNet.Running;
using Jerkball2D;
using Jerkball2D.TimerExtensions;
using static Jerkball2D.TimerExtensions.MatchTimerController;
using static Jerkball2D.TimerExtensions.TimerTypes;

namespace Jerkball2DMatchTimer.Benchmarks;

public static class Program
{
    public static void Main(string[] args)
    {
        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}

[MemoryDiagnoser]
[SimpleJob(RunStrategy.Throughput, launchCount: 1, warmupCount: 5, iterationCount: 20)]
public sealed class MatchTimerBenchmark
{
    private const int LoopCount = 1000;
    private readonly Random _random = new(42);

    private MatchTimer[] _timers = null!;
    private float[] _randomDeltas = null!;
    private int _index;

    [Params(1, 100, 1000)]
    public int EntityCount { get; set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        _timers = new MatchTimer[EntityCount];
        for (int i = 0; i < _timers.Length; i++)
        {
            _timers[i] = new MatchTimer(90f);
        }

        _randomDeltas = new float[1024];
        for (int i = 0; i < _randomDeltas.Length; i++)
        {
            _randomDeltas[i] = _random.NextSingle() * 2f;
        }
    }

    [IterationSetup]
    public void IterationSetup()
    {
        _index = 0;

        for (int i = 0; i < _timers.Length; i++)
        {
            _timers[i].ResetTo(90f);
        }
    }

    [Benchmark(OperationsPerInvoke = LoopCount)]
    public float UpdateOnly_SmallDelta()
    {
        float totalElapsed = 0f;

        for (int loop = 0; loop < LoopCount; loop++)
        {
            float delta = NextDelta() * 0.016f;
            for (int i = 0; i < _timers.Length; i++)
            {
                _timers[i].Update(delta);
                totalElapsed += _timers[i].Elapsed;
            }
        }

        return totalElapsed;
    }

    [Benchmark(OperationsPerInvoke = LoopCount)]
    public float UpdateOnly_LargeDelta()
    {
        float totalElapsed = 0f;

        for (int loop = 0; loop < LoopCount; loop++)
        {
            float delta = NextDelta();
            for (int i = 0; i < _timers.Length; i++)
            {
                _timers[i].Update(delta);
                totalElapsed += _timers[i].Elapsed;
            }
        }

        return totalElapsed;
    }

    [Benchmark(OperationsPerInvoke = LoopCount)]
    public float SimulateOneSecond()
    {
        float totalElapsed = 0f;

        for (int loop = 0; loop < LoopCount; loop++)
        {
            for (int frame = 0; frame < 60; frame++)
            {
                float delta = NextDelta() * 0.016f;
                for (int i = 0; i < _timers.Length; i++)
                {
                    _timers[i].Update(delta);
                    totalElapsed += _timers[i].Elapsed;
                }
            }
        }

        return totalElapsed;
    }

    [Benchmark(OperationsPerInvoke = LoopCount)]
    public int ReadDigitalClock_Cached()
    {
        int totalLength = 0;

        for (int loop = 0; loop < LoopCount; loop++)
        {
            for (int i = 0; i < _timers.Length; i++)
            {
                totalLength += _timers[i].DigitalClock.Length;
            }
        }

        return totalLength;
    }

    [Benchmark(OperationsPerInvoke = LoopCount)]
    public int ReadDigitalClock_WithFormatting()
    {
        int totalLength = 0;

        for (int loop = 0; loop < LoopCount; loop++)
        {
            float delta = NextDelta();
            for (int i = 0; i < _timers.Length; i++)
            {
                _timers[i].Update(delta);
                totalLength += _timers[i].DigitalClock.Length;
            }
        }

        return totalLength;
    }

    [Benchmark(OperationsPerInvoke = LoopCount)]
    public int Restart()
    {
        int totalLength = 0;

        for (int loop = 0; loop < LoopCount; loop++)
        {
            for (int i = 0; i < _timers.Length; i++)
            {
                _timers[i].Restart();
                totalLength += _timers[i].DigitalClock.Length;
            }
        }

        return totalLength;
    }

    [Benchmark(OperationsPerInvoke = LoopCount)]
    public int ResetTo()
    {
        int totalLength = 0;

        for (int loop = 0; loop < LoopCount; loop++)
        {
            float targetTime = NextDelta() * 60f;
            for (int i = 0; i < _timers.Length; i++)
            {
                _timers[i].ResetTo(targetTime);
                totalLength += _timers[i].DigitalClock.Length;
            }
        }

        return totalLength;
    }

    private float NextDelta()
    {
        int index = _index++ & (_randomDeltas.Length - 1);
        return _randomDeltas[index];
    }
}

[MemoryDiagnoser]
[SimpleJob(RunStrategy.Throughput, launchCount: 1, warmupCount: 5, iterationCount: 20)]
public sealed class TimerExtensionsBenchmark
{
    private const int LoopCount = 1000;

    private MatchTimer _runningTimer = null!;
    private MatchTimer _pausedTimer = null!;
    private Action _cachedCallback = null!;

    [GlobalSetup]
    public void Setup()
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
