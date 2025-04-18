using UnityEngine;

public class Punch
{
    public float damage;
    public float duration;
    public float hitSecond;

    public Punch(float damage, float duration, float hitSecond)
    {
        this.damage = damage;
        this.duration = duration;
        this.hitSecond = hitSecond;
    }
}
