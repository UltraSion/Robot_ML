using System;
using UnityEngine;

namespace Weapons.ProjectileEntities
{
public class HarpoonProjectile : ProjectileEntity
{
    private FixedJoint fJoint;
    private SpringJoint sJoint;

    private ArticulationBody linkedBody;

    protected override void OnCollide(Collision other)
    {
        var targetRGBody = other.gameObject.GetComponent<Rigidbody>();
        var targetATBody = other.gameObject.GetComponent<ArticulationBody>();

        if (targetRGBody != null || targetATBody != null)
        {
            fJoint = gameObject.AddComponent<FixedJoint>();
            sJoint = gameObject.AddComponent<SpringJoint>();
        }

        if (targetRGBody != null)
            fJoint.connectedBody = targetRGBody;

        if (targetATBody != null)
            fJoint.connectedArticulationBody = targetATBody;
    }
}
}
