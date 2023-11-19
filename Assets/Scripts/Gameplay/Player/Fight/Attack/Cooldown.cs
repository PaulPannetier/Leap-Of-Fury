using UnityEngine;

[System.Serializable]
public class Cooldown
{
    private float counter;

    [HideInInspector] public bool isActive => counter >= duration;
    public float duration;
    [HideInInspector] public float percentage => Mathf.Min(1f, counter / duration);

    public Cooldown(in float duration)
    {
        this.duration = duration;
        counter = duration;
    }

    public void Update()
    {
        counter += Time.deltaTime;
    }

    public void ForceActivate()
    {
        counter = duration;
    }

    public void Reset()
    {
        counter = 0f;
    }
}
