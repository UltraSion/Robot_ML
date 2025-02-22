using UnityEngine;

namespace Weapons.ProjectileEntities
{
public abstract class ProjectileEntity : MonoBehaviour
{
    private bool used = false;
    private void OnCollisionEnter(Collision other)
    {
        if (used)
            return;
        OnCollide(other);
        used = true;
    }

    protected abstract void OnCollide(Collision other);
}
}
