using UnityEngine;
using Weapons.ProjectileEntities;

namespace Weapons
{
public abstract class ObjectShooter : MonoBehaviour
{
    public ProjectileEntity toShoot;

    [SerializeField]
    protected float shootForce;

    public abstract void Shoot();
}
}
