// SPDX-License-Identifier: MPL-2.0
//
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, See the LICENSE file in the repository root for more details.
//
// Copyright (c) 2026 kx-night

namespace Jerkball2D.TimerExtensions

open System
open Jerkball2D

module TimerTypes =
    [<RequireQualifiedAccess>]
    type TimerState =
        | Idle
        | Running
        | Paused
        | Completed

[<AutoOpen>]
module MatchTimerActivePatterns =
    open TimerTypes

    let (|CurrentState|) (timer: MatchTimer) =
        if timer.IsCompleted then TimerState.Completed
        elif timer.IsPaused then TimerState.Paused
        elif timer.IsRunning then TimerState.Running
        else TimerState.Idle

module MatchTimerController =
    open TimerTypes

    let inline tick (deltaTime: single) (timer: MatchTimer) : TimerState =
        match timer with
        | CurrentState TimerState.Running ->
            timer.Update deltaTime
            if timer.IsCompleted then TimerState.Completed else TimerState.Running
        | CurrentState state ->
            state

    let inline play (timer: MatchTimer) : MatchTimer =
        timer.Play()
        timer

    let inline pause (timer: MatchTimer) : MatchTimer =
        timer.Pause()
        timer

    let inline restart (timer: MatchTimer) : MatchTimer =
        timer.Restart()
        timer

    let onFinished (callback: Action) (timer: MatchTimer) : IDisposable =
        timer.add_OnCompleted callback

        { new IDisposable with
            member _.Dispose() =
                timer.remove_OnCompleted callback }

    let tryResetTo (seconds: single) (timer: MatchTimer) : Result<MatchTimer, string> =
        try
            timer.ResetTo seconds
            Ok timer
        with
        | :? ArgumentOutOfRangeException as ex ->
            let paramInfo =
                if String.IsNullOrWhiteSpace ex.ParamName then ""
                else $" ({ex.ParamName})"

            Error $"Match duration must be between 1 second and 99 minutes.{paramInfo}"
