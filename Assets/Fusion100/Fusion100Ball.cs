using Fusion;
using UnityEngine;

public sealed class Fusion100Ball : NetworkBehaviour
{
    [Networked] private TickTimer Life { get; set; }


    public void Init()
    {
        Life = TickTimer.CreateFromSeconds(Runner, 5.0f);
    }

    public override void FixedUpdateNetwork()
    {
        if (Life.Expired(Runner))
        {
            Runner.Despawn(Object);
            return;
        }

        var thisTransform = transform;
        thisTransform.position += 5 * thisTransform.forward * Runner.DeltaTime;
    }
}