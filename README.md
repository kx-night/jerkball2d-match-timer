# Jerkball2D.MatchTimer

A small match timer for C# game loops. It has no engine dependency and works with **Unity**, **Godot**, **raylib**, **MonoGame**, **Stride**, or any custom loop in a .NET project.

<!-- Project & build status -->
[![License: MPL-2.0](https://img.shields.io/badge/License-MPL--2.0-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-6%2B-5C2D91.svg?logo=.net&logoColor=white)](https://dotnet.microsoft.com/)
[![Build](https://github.com/KXnight/jerkball2d-match-timer/actions/workflows/build.yml/badge.svg)](https://github.com/KXnight/jerkball2d-match-timer/actions)
[![Coverage](https://img.shields.io/badge/coverage-passing-brightgreen.svg)](https://github.com/KXnight/jerkball2d-match-timer/actions)
[![GitHub stars](https://img.shields.io/github/stars/KXnight/jerkball2d-match-timer.svg?style=social&label=Star)](https://github.com/KXnight/jerkball2d-match-timer)

<!-- Engine / framework support -->
[![Unity](https://img.shields.io/badge/Unity-supported-000000.svg?logo=unity&logoColor=white)](https://unity.com/)
[![Godot](https://img.shields.io/badge/Godot-supported-478CBF.svg?logo=godot-engine&logoColor=white)](https://godotengine.org/)
[![raylib](https://img.shields.io/badge/raylib-supported-000000.svg)](https://www.raylib.com/)
[![MonoGame](https://img.shields.io/badge/MonoGame-supported-E73C00.svg)](https://www.monogame.net/)
[![Stride](https://img.shields.io/badge/Stride-supported-FF6A00.svg)](https://stride3d.net/)

---

## Features

- Zero allocations after construction.
- Works with any source of delta time.
- `DigitalClock` output for display.
- Pause, resume, and completion state.
- Single small type, no extra framework.

---

## Requirements & Compatibility

| Requirement      | Specification                                        |
| :--------------- | :--------------------------------------------------- |
| **C# Language**  | C# 10.0 or newer                                    |
| **.NET / Unity** | .NET Standard 2.1+, .NET 6.0+, or Unity 2021.3 LTS+ |

---

## Quick start

```csharp
using Jerkball2D;

var timer = new MatchTimer(300f); // 5 Minutes
timer.Play();

while (!timer.IsCompleted)
{
    var delta = GetDeltaTime(); // Supply your own delta time
    timer.Update(delta);
    Console.WriteLine(timer.DigitalClock);
}
```

---

## Engine examples

The examples below all follow the same basic flow: create the timer, start it, update it with delta time, and show `DigitalClock`.

<details>
<summary><b>Unity (TextMeshPro)</b></summary>

```csharp
using UnityEngine;
using TMPro;
using Jerkball2D;

[RequireComponent(typeof(TMP_Text))]
public sealed class MatchTimerUI : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float _matchDuration = 300f;

    private TMP_Text _label = null!;
    private MatchTimer _timer = null!;

    private void Awake()
    {
        _label = GetComponent<TMP_Text>();
        _timer = new MatchTimer(_matchDuration);
        _timer.Play();
        _label.text = _timer.DigitalClock;
    }

    private void Update()
    {
        _timer.Update(Time.deltaTime);
        _label.text = _timer.DigitalClock;
    }
}
```

</details>

<details>
<summary><b>Godot 4 (C#)</b></summary>

```csharp
using Godot;
using Jerkball2D;

public partial class MatchHud : Control
{
    [Export] private Label _timerLabel = null!;
    [Export] private float _matchDuration = 300f;

    private MatchTimer _timer = null!;

    public override void _Ready()
    {
        _timer = new MatchTimer(_matchDuration);
        _timer.Play();
        _timerLabel.Text = _timer.DigitalClock;
    }

    public override void _Process(double delta)
    {
        _timer.Update((float)delta);
        _timerLabel.Text = _timer.DigitalClock;
    }
}
```

</details>

<details>
<summary><b>raylib (C# binding)</b></summary>

```csharp
using Jerkball2D;
using Raylib_cs;

public static class Program
{
    public static void Main()
    {
        Raylib.InitWindow(800, 450, "Jerkball2D.MatchTimer + raylib");
        Raylib.SetTargetFPS(60);

        var timer = new MatchTimer(300f);
        timer.Play();

        while (!Raylib.WindowShouldClose())
        {
            var delta = Raylib.GetFrameTime();
            timer.Update(delta);

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.BLACK);
            Raylib.DrawText(timer.DigitalClock, 350, 200, 40, Color.WHITE);
            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
    }
}
```

</details>

<details>
<summary><b>Stride (SyncScript)</b></summary>

```csharp
using Jerkball2D;
using Stride.Engine;

public class MatchTimerHud : SyncScript
{
    private MatchTimer _timer = null!;

    public override void Start()
    {
        _timer = new MatchTimer(300f);
        _timer.Play();
    }

    public override void Update()
    {
        var delta = (float)Game.UpdateTime.Elapsed.TotalSeconds;
        _timer.Update(delta);

        Stride.Engine.DebugText.Print(
            _timer.DigitalClock,
            new Stride.Core.Mathematics.Int2(10, 10)
        );
    }
}
```

</details>

---

## License

This project is licensed under the **Mozilla Public License 2.0** (MPL‑2.0, SPDX identifier `MPL-2.0`). See the [`LICENSE`](LICENSE) file for details.