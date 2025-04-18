using UnityEngine;

public static class PunchData
{
    public static readonly Punch[] RegularPunches = new Punch[]
    {
        new Punch(damage: 15, duration: 0.33f, hitSecond: 0.26f, direction: new Vector2(1, 0)), // Side punch
        new Punch(damage: 15, duration: 0.43f, hitSecond: 0.26f, direction: new Vector2(1, -0.2f)), // Slightly upward punch
        new Punch(damage: 15, duration: 0.4f, hitSecond: 0.26f, direction: new Vector2(1, 0.2f)) // Slightly upward punch
    };

    public static readonly Punch[] HeavyPunches = new Punch[]
    {
        new Punch(damage: 30, duration: 1.05f, hitSecond: 1, direction: new Vector2(1, 0.5f)), // Upward heavy punch
        new Punch(damage: 35, duration: 1.35f, hitSecond: 1, direction: new Vector2(1, -0.5f)), // Downward heavy punch
        new Punch(damage: 40, duration: 1.35f, hitSecond: 1, direction: new Vector2(1, 0)) // Side heavy punch
    };
}