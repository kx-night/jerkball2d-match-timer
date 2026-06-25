// SPDX-License-Identifier: MPL-2.0
//
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.
//
// Copyright (c) 2026 kx-night

// global using MyAwesomeGame = global::Jerkball2D;
// global using static global::Jerkball2D.TimerExtensions.MatchTimerController;
// global using static global::Jerkball2D.TimerExtensions.TimerTypes.TimerState;
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

    [<return: Struct>]
    let (|CurrentState|) (timer: MatchTimer) =
        if timer.IsCompleted then Completed
        elif timer.IsPaused then Paused
        elif timer.IsRunning then Running
        else Idle

module MatchTimerController =
    open TimerTypes

    let inline tick (deltaTime: single) (timer: MatchTimer) : TimerState =
        match timer with
        | CurrentState Running ->
            timer.Update deltaTime
            if timer.IsCompleted then Completed else Running
        | CurrentState state ->
            state

    let inline play (timer: MatchTimer) : MatchTimer =
        timer.Play()
        timer

    let inline pause (timer: MatchTimer) : MatchTimer =
        timer.Pause()
        timer

    let inline resetAndPlay (timer: MatchTimer) : MatchTimer =
        timer.ResetAndPlay()
        timer

    let onFinished (callback: unit -> unit) (timer: MatchTimer) : IDisposable =
        let handler = Action callback
        timer.OnCompleted.AddHandler handler

        { new IDisposable with
            member _.Dispose() =
                timer.OnCompleted.RemoveHandler handler }

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
