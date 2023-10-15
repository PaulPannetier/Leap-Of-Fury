using UnityEngine;

public class Boomerang : MonoBehaviour
{
    private enum State
    {
        go,
        getBack
    }

    private new Transform transform;
    private ToricObject toricObject;
    private Animator animator;
    private BoomerangAttack sender;
    private Vector2 dir;
    private AnimationCurve speedCurvePhase1, accelerationCurvePhase2;
    private float maxSpeedPhase1, durationPhase1, accelerationDurationPhase2;
    private float maxSpeedPhase2, recuperationRange;
    private State state;
    private float timeLaunch = -10f;
    private Vector2 velocity;

    private void Awake()
    {
        this.transform = base.transform;
        toricObject = GetComponent<ToricObject>();
        animator = GetComponent<Animator>();
    }

    public void Launch(in BoomerangLaunchData boomerangLauchData)
    {
        Builder(boomerangLauchData);

        state = State.go;
        velocity = maxSpeedPhase1 * speedCurvePhase1.Evaluate(0f) * dir;
        timeLaunch = Time.time;

        void Builder(in BoomerangLaunchData data)
        {
            dir = data.dir; 
            speedCurvePhase1 = data.speedCurvePhase1;
            maxSpeedPhase1 = data.maxSpeedPhase1;
            durationPhase1 = data.durationPhase1;
            sender = data.sender;
            maxSpeedPhase2 = data.maxSpeedPhase2;
            accelerationCurvePhase2 = data.accelerationCurvePhase2;
            maxSpeedPhase2 = data.maxSpeedPhase2;
            recuperationRange = data.recuperationRange;
        }
    }

    private void Update()
    {
        switch (state)
        {
            case State.go:
                HandleGoState();
                break;
            case State.getBack:
                HandleGetBackState();
                break;
            default:
                break;
        }

        Vector2 pos = transform.position + (Vector3)(velocity * Time.deltaTime);
        transform.SetPositionAndRotation(pos, Quaternion.Euler(0f, 0f, (velocity.Angle(Vector2.right) + Mathf.PI) * Mathf.Rad2Deg));
    }

    private void HandleGoState()
    {
        if(Time.time - timeLaunch > durationPhase1)
        {
            state = State.getBack;
            velocity = velocity.normalized * speedCurvePhase1.Evaluate(1f);
            timeLaunch = Time.time;
            return;
        }

        velocity = maxSpeedPhase1 * speedCurvePhase1.Evaluate((Time.time - timeLaunch) / durationPhase1) * dir;
    }

    private void HandleGetBackState()
    {
        dir = PhysicsToric.Direction(transform.position, sender.transform.position);

        if (Time.time - timeLaunch < accelerationDurationPhase2)
        {
            velocity = maxSpeedPhase2 * accelerationCurvePhase2.Evaluate(1f) * dir;
        }
        else
        {
            velocity = maxSpeedPhase2 * dir;
        }

        if(PhysicsToric.Distance(transform.position, sender.transform.position) < recuperationRange)
        {
            sender.GetBack();
            animator.SetTrigger("destroy");
            if (animator.GetAnimationLength("destroy", out float length))
            {
                Invoke(nameof(Destroy), length);
            }
            else
                Destroy();
        }
    }

    private void Destroy()
    {
        toricObject.RemoveClones();
        Destroy(gameObject);
    }

    public struct BoomerangLaunchData
    {
        public Vector2 dir;
        public AnimationCurve speedCurvePhase1, accelerationCurvePhase2;
        public float maxSpeedPhase1, durationPhase1, accelerationDurationPhase2;
        public BoomerangAttack sender;
        public float maxSpeedPhase2;
        public float recuperationRange;

        public BoomerangLaunchData(in Vector2 dir, AnimationCurve speedCurvePhase1, AnimationCurve accelerationCurvePhase2, float maxSpeedPhase1, float durationPhase1, float accelerationDurationPhase2, BoomerangAttack sender, float maxSpeedPhase2, float recuperationRange)
        {
            this.dir = dir;
            this.speedCurvePhase1 = speedCurvePhase1;
            this.accelerationCurvePhase2 = accelerationCurvePhase2;
            this.maxSpeedPhase1 = maxSpeedPhase1;
            this.durationPhase1 = durationPhase1;
            this.accelerationDurationPhase2 = accelerationDurationPhase2;
            this.sender = sender;
            this.maxSpeedPhase2 = maxSpeedPhase2;
            this.recuperationRange = recuperationRange;
        }
    }
}
