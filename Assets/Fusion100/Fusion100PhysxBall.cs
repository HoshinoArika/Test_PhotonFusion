using Fusion;
using UnityEngine;

public sealed class Fusion100PhysxBall : NetworkBehaviour
{
    [Networked] private TickTimer Life { get; set; }

    public void Init(Vector3 forward)
    {
        Life = TickTimer.CreateFromSeconds(Runner, 5.0f);
        GetComponent<Rigidbody>().velocity = forward;
    }

    public override void FixedUpdateNetwork()
    {
        if (Life.Expired(Runner))
            Runner.Despawn(Object);
    }
}