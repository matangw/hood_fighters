using UnityEngine;

public static class PunchData
{
    public static readonly Punch[] RegularPunches = new Punch[]
    {
        new Punch(damage: 15, duration: 0.33f, hitSecond: 0.17f),
        new Punch(damage: 15, duration: 0.43f, hitSecond: 0.17f),
        new Punch(damage: 15, duration: 0.4f, hitSecond: 0.17f)
    };

    public static readonly Punch[] HeavyPunches = new Punch[]
    {
        new Punch(damage: 30, duration: 1.05f, hitSecond: 0.47f),
        new Punch(damage: 35, duration: 1.35f, hitSecond: 0.47f),
        new Punch(damage: 40, duration: 1.35f, hitSecond: 0.47f)
    };
}