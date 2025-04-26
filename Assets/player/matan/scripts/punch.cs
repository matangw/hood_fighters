using UnityEngine;

public class Punch
{
    public float damage;
    public float duration;
    public float hitSecond;
    public Vector2 direction;

    public Punch(float damage, float duration, float hitSecond, Vector2 direction)
    {
        this.damage = damage;
        this.duration = duration;
        this.hitSecond = hitSecond;
        this.direction = direction;
    }
}
