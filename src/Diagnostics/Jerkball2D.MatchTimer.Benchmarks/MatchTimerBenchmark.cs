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

namespace Jerkball2D.Benchmarks;

[MemoryDiagnoser]
[MediumRunJob(RunStrategy.Throughput)]
public class MatchTimerBenchmark
{
    private const int LoopCount = 100_000;
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
