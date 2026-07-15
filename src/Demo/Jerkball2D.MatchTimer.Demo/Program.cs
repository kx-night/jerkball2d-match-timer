using System;
using System.Diagnostics;
using System.Threading;
using Jerkball2D;

var timer = new MatchTimer(5f);

timer.OnCompleted += () => Console.WriteLine("\nMatch Finished!");
timer.Play();

var stopwatch = Stopwatch.StartNew();
float lastTime = 0f;

while (!timer.IsCompleted)
{
    float currentTime = (float)stopwatch.Elapsed.TotalSeconds;
    float deltaTime = currentTime - lastTime;
    lastTime = currentTime;

    timer.Update(deltaTime);
    Console.Write($"\rTime Remaining: {timer.DigitalClock}");
    Thread.Sleep(16);
}
