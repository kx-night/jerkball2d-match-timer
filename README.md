<h1 align="center">⏱️ Jerkball2D.MatchTimer</h1>

<p align="center">
  <strong>Developer‑friendly match timer for C# game loops.</strong>
  <br />
  <sub>Unity • Godot • raylib • MonoGame • Stride • Custom Loops</sub>
</p>

<p align="center">
  <a href="LICENSE"><img src="https://img.shields.io/badge/License-MPL--2.0-blue.svg" alt="License: MPL-2.0"></a>
  <a href="https://dotnet.microsoft.com/"><img src="https://img.shields.io/badge/.NET-6%2B-5C2D91.svg?logo=.net&logoColor=white" alt=".NET 6+"></a>
  <a href="https://github.com/kx-night/jerkball2d-match-timer/actions"><img src="https://github.com/kx-night/jerkball2d-match-timer/actions/workflows/ci.yml/badge.svg" alt="Build Status"></a>
  <a href="https://github.com/kx-night/jerkball2d-match-timer/actions"><img src="https://github.com/kx-night/jerkball2d-match-timer/actions/workflows/bench.yml/badge.svg" alt="Benchmark Status"></a>
  <img src="https://img.shields.io/badge/coverage-passing-brightgreen.svg" alt="Coverage">
  <a href="https://github.com/kx-night/jerkball2d-match-timer"><img src="https://img.shields.io/github/stars/kx-night/jerkball2d-match-timer.svg?style=social&label=Star" alt="GitHub stars"></a>
  <a href="https://replit.com/github/kx-night/jerkball2d-match-timer"><img src="https://replit.com/badge/github/kx-night/jerkball2d-match-timer" alt="Run on Replit"></a>
</p>

<p align="center">
  <a href="https://unity.com/"><img src="https://img.shields.io/badge/Unity-supported-000000.svg?logo=unity&logoColor=white" alt="Unity"></a>
  <a href="https://godotengine.org/"><img src="https://img.shields.io/badge/Godot-supported-478CBF.svg?logo=godot-engine&logoColor=white" alt="Godot"></a>
  <a href="https://www.raylib.com/"><img src="https://img.shields.io/badge/raylib-supported-000000.svg" alt="raylib"></a>
  <a href="https://www.monogame.net/"><img src="https://img.shields.io/badge/MonoGame-supported-E73C00.svg" alt="MonoGame"></a>
  <a href="https://stride3d.net/"><img src="https://img.shields.io/badge/Stride-supported-FF6A00.svg" alt="Stride"></a>
</p>

## Table of Contents

<details>
  <summary><b>Expand to navigate</b></summary>

- [Overview](#overview)
- [Requirements & Compatibility](#requirements--compatibility)
- [Quick Start](#quick-start)
- [F# Functional Wrapper](#f-functional-wrapper)
  - [What It Adds](#what-it-adds)
  - [Using It from C#](#using-it-from-c)
- [Engine Examples](#engine-examples)
  - [Unity](#unity-integration)
  - [Godot 4](#godot-4-integration)
  - [Raylib](#raylib-integration)
  - [Stride](#stride-integration)
- [License](#license)
- [Contributors](#contributors)

</details>

---

## **Overview**

`Jerkball2D.MatchTimer` is a engine-agnostic timer library for C# game loops. It keeps match timing separate from any specific engine, so it works with Unity, Godot, MonoGame, Stride, raylib, or custom .NET loops.

Originally built for the *Jerkball2D* football prototype, it was later extracted into a standalone package so the same timing logic can be reused across projects and runtimes.

---

## F# Functional Wrapper

For developers who prefer functional programming paradigms, the `Jerkball2D.TimerExtensions` module provides a lightweight functional wrapper over the core `MatchTimer`. It introduces active patterns, type-safe state transitions, and a `Result`-based error handling model, all while remaining fully interoperable with C#.

### What It Adds

| Feature | Description |
| :--- | :--- |
| **Active Patterns** | Use `\|CurrentState\|` to pattern-match on timer states (`Idle`, `Running`, `Paused`, `Completed`) for readable state handling. |
| **Functional Tick** | `tick` composes state transitions cleanly and returns a new timer value, modeling behavior as a pure function of the current state and delta time. |
| **Result-Based Validation** | `tryResetTo` returns `Result<MatchTimer, string>` instead of throwing exceptions, keeping invalid input out of exception flow. |
| **Clean Event Subscription** | `onFinished` returns an `IDisposable` token for deterministic cleanup and unsubscription. |
| **Method Chaining** | Operations like `play`, `pause`, and `restart` return the timer instance, enabling fluent composition. |

---

### Using It from C#

Here is how you can leverage the F# wrapper directly from your C# codebase:

```csharp
using System;
using Jerkball2D;
using Jerkball2D.TimerExtensions; // Import the F# extension module

// Create the timer
var timer = new MatchTimer(60f);

// Functional play & subscribe to completion
IDisposable subscription = TimerController.onFinished(
    new Action(() => Console.WriteLine("\nMatch finished!")),
    TimerController.play(timer)
);

// Game loop simulation
float deltaTime = 0.016f;
while (true)
{
    // Tick the timer and get the new state
    TimerTypes.TimerState state = TimerController.tick(deltaTime, timer);

    // Pattern match on the state (C# 8.0+ switch expression)
    switch (state)
    {
        case TimerTypes.TimerState.Running:
            Console.Write($"\rTime Remaining: {timer.DigitalClock}");
            break;
        case TimerTypes.TimerState.Completed:
            Console.WriteLine("\nTimer reached zero!");
            goto exit;
        case TimerTypes.TimerState.Paused:
            Console.WriteLine("\nTimer paused");
            break;
        case TimerTypes.TimerState.Idle:
            Console.WriteLine("\nTimer idle");
            break;
    }
}

exit:
// Clean up the event subscription automatically
subscription.Dispose();

// --- Safe reset with F# Result type ---
var resetResult = TimerController.tryResetTo(30f, timer);

if (resetResult.IsOk)
{
    Console.WriteLine("Timer reset to 30 seconds!");
    // Access the verified timer instance: resetResult.ResultValue
}
else
{
    Console.WriteLine($"Reset failed: {resetResult.ErrorValue}");
}
```

---

## Requirements & Compatibility

| Component | Target Framework Support | Minimum Language Version |
| :--- | :--- | :--- |
| **C# Core Library** (`Jerkball2D`) | .NET Standard 2.0+ or .NET 6.0+ | C# 10.0+ |
| **F# Extensions** (`Jerkball2D.TimerExtensions`) | .NET Standard 2.0+ or .NET 6.0+ | F# 6.0+ |

---

## Quick Start

```csharp
using System;
using System.Diagnostics;
using System.Threading;
using Jerkball2D;

var timer = new MatchTimer(5f); // 5 seconds for quick testing

timer.OnCompleted += () => Console.WriteLine("\nMatch Finished!");
timer.Play();

var stopwatch = Stopwatch.StartNew();
float lastTime = 0f;

// Active game loop simulation
while (!timer.IsCompleted)
{
    float currentTime = (float)stopwatch.Elapsed.TotalSeconds;
    float deltaTime = currentTime - lastTime;
    lastTime = currentTime;

    timer.Update(deltaTime);

    // Render formatted "MM:SS" time directly to console
    Console.Write($"\rTime Remaining: {timer.DigitalClock}");

    Thread.Sleep(16); // ~60 FPS update tick rate
}
```

---

## Engine Examples

The examples below all follow the same basic flow: create the timer, start it, update it with delta time, and show `DigitalClock`.

<details>
<summary><b>Unity</b></summary>

### Unity Integration

Attach this component directly to a GameObject containing a `TextMeshProUGUI` or `TextMeshPro` component. It acts as a wrapper bridging the core timer library to Unity's lifecycle and event system.

```csharp
// SPDX-License-Identifier: MPL-2.0
//
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. See the LICENSE file in the repository root for more details.
//
// Copyright (c) 2026 kx-night

using TMPro;
using UnityEngine;
using UnityEngine.Events;
using Jerkball2D;

[DisallowMultipleComponent]
[RequireComponent(typeof(TMP_Text))]
public sealed class MatchTimerUI : MonoBehaviour
{
    [SerializeField, Min(0f)] private float _matchDuration = 300f;
    [SerializeField] private bool _autoStart = true;
    [SerializeField] private UnityEvent _onMatchCompleted = new();

    private TMP_Text _label = null!;
    private MatchTimer _timer = null!;
    private string _cachedText = string.Empty;

    private void Awake()
    {
        _label = GetComponent<TMP_Text>();
        _timer = new MatchTimer(_matchDuration);
    }

    private void OnEnable()
    {
        _timer.OnCompleted += HandleCompleted;

        if (_autoStart)
        {
            _timer.Play();
        }

        RefreshText();
    }

    private void Update()
    {
        _timer.Update(Time.deltaTime);
        RefreshText();
    }

    private void OnDisable()
    {
        _timer.OnCompleted -= HandleCompleted;
    }

    public void Play() => _timer.Play();
    public void Pause() => _timer.Pause();

    public void Restart()
    {
        _timer.Restart();
        RefreshText();
    }

    public void ResetTo(float seconds)
    {
        if (seconds < 0f)
        {
            Debug.LogError($"[{nameof(MatchTimerUI)}] ResetTo failed: seconds must be non-negative.", this);
            return;
        }

        _timer.ResetTo(seconds);
        RefreshText();
    }

    private void HandleCompleted()
    {
        _onMatchCompleted.Invoke();
        RefreshText();
    }

    private void RefreshText()
    {
        string text = _timer.DigitalClock;

        if (_cachedText != text)
        {
            _cachedText = text;
            _label.text = text;
        }
    }
}
```

</details>

<details>
<summary><b>Godot 4 (C#)</b></summary>

### Godot 4 Integration

Attach this script to any `Control` node. It exposes the timer's core properties directly to the Godot Inspector and efficiently bridges the library to Godot's frame processing steps.

```csharp
// SPDX-License-Identifier: MPL-2.0
//
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. See the LICENSE file in the repository root for more details.
//
// Copyright (c) 2026 kx-night

using Godot;
using Jerkball2D;

public partial class MatchHud : Control
{
    [Export] private Label _timerLabel = null!;
    [Export] private float _matchDuration = 300f;
    [Export] private bool _autoStart = true;

    private MatchTimer _timer = null!;
    private string _cachedText = string.Empty;

    public override void _Ready()
    {
        _timer = new MatchTimer(_matchDuration);

        _timer.OnCompleted += HandleCompleted;

        if (_autoStart)
        {
            _timer.Play();
        }

        RefreshText();
    }

    public override void _Process(double delta)
    {
        _timer.Update((float)delta);
        RefreshText();
    }

    public override void _ExitTree()
    {
        _timer.OnCompleted -= HandleCompleted;
    }

    public void Play() => _timer.Play();
    public void Pause() => _timer.Pause();

    public void Restart()
    {
        _timer.Restart();
        RefreshText();
    }

    public void ResetTo(float seconds)
    {
        if (seconds < 0f)
        {
            GD.PushError($"[{nameof(MatchHud)}] ResetTo failed: seconds must be non-negative.");
            return;
        }

        _timer.ResetTo(seconds);
        RefreshText();
    }

    private void HandleCompleted()
    {
        RefreshText();
    }

    private void RefreshText()
    {
        string text = _timer.DigitalClock;

        if (_cachedText != text)
        {
            _cachedText = text;
            _timerLabel.Text = text;
        }
    }
}
```

</details>

<details>
<summary><b>Raylib (C#)</b></summary>

### Raylib Integration

A lightweight, immediate-mode boilerplate showing how to run and render the timer directly inside a standard custom game loop using the `Raylib-cs` bindings.

```csharp
// SPDX-License-Identifier: MPL-2.0
//
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. See the LICENSE file in the repository root for more details.
//
// Copyright (c) 2026 kx-night

using Raylib_cs;
using Jerkball2D;

public static class Program
{
    public static void Main()
    {
        Raylib.InitWindow(800, 450, "Jerkball2D.MatchTimer");
        Raylib.SetTargetFPS(60);

        var timer = new MatchTimer(300f);
        timer.Play();

        string cachedText = string.Empty;

        while (!Raylib.WindowShouldClose())
        {
            timer.Update(Raylib.GetFrameTime());

            string text = timer.DigitalClock;
            if (cachedText != text)
            {
                cachedText = text;
            }

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.BLACK);
            Raylib.DrawText(cachedText, 350, 200, 40, Color.WHITE);
            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }
}
```

</details>

<details>
<summary><b>Stride (SyncScript)</b></summary>

### Stride Integration

Attach this script to an Entity in your scene to handle frame updates. This example uses Stride's built-in `SyncScript` lifecycle and renders the output directly to the screen using the game's native `DebugText` utility.

```csharp
// SPDX-License-Identifier: MPL-2.0
//
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. See the LICENSE file in the repository root for more details.
//
// Copyright (c) 2026 kx-night

using Stride.Core.Mathematics;
using Stride.Engine;
using Jerkball2D;

public class MatchTimerHud : SyncScript
{
    private const float MatchDuration = 300f;
    private readonly Int2 _textPosition = new(10, 10);

    private MatchTimer _timer = null!;
    private string _cachedText = string.Empty;

    public override void Start()
    {
        _timer = new MatchTimer(MatchDuration);
        _timer.Play();
        RefreshText();
    }

    public override void Update()
    {
        _timer.Update((float)Game.UpdateTime.Elapsed.TotalSeconds);
        RefreshText();
    }

    private void RefreshText()
    {
        string text = _timer.DigitalClock;

        if (_cachedText != text)
        {
            _cachedText = text;
        }

        DebugText.Print(_cachedText, _textPosition);
    }
}
```

</details>

---

## License

This project is licensed under the **Mozilla Public License 2.0** (MPL-2.0, SPDX identifier `MPL-2.0`). See the [`LICENSE`](LICENSE) file for details.

---

## Contributors

<p align="center">
  <a href="https://github.com/kx-night/jerkball2d-match-timer/graphs/contributors">
    <img src="https://contrib.rocks/image?repo=kx-night/jerkball2d-match-timer" alt="jerkball2d-match-timer contributors" />
  </a>
</p>

<p align="center">
  <sub>Made with ❤️ by the contributors. Click the image to view the contributor graph.</sub>
</p>
