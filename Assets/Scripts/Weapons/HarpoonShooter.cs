using UnityEngine;
using Weapons.ProjectileEntities;

namespace Weapons
{
public class HarpoonShooter : ObjectShooter
{
    public override void Shoot()
    {
        var shootDir = transform.forward;
        var projectile = Instantiate(toShoot, transform.position, transform.rotation);
        var rgBody = projectile.GetComponent<Rigidbody>();
        rgBody.AddForce(shootDir * shootForce);
    }

    public void Pull()
    {
        
    }
}
}
