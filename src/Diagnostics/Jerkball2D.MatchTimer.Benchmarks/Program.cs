// SPDX-License-Identifier: MPL-2.0
//
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. See the LICENSE file in the repository root for more details.
//
// Copyright (c) 2026 kx-night

using BenchmarkDotNet.Running;

namespace Jerkball2D.Benchmarks;

public static class Program
{
    public static void Main(string[] args)
    {
        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}
