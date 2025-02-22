using UnityEngine;

namespace Weapons
{
public class ObjectShooter : MonoBehaviour
{
    public GameObject toShoot;

    [SerializeField]
    private float shootForce;

    public void Shoot()
    {
        var shootDir = transform.forward;
        var projectile = Instantiate(toShoot, transform.position, transform.rotation);
        var rgBody = projectile.GetComponent<Rigidbody>();
        rgBody.AddForce(shootDir * shootForce);
    }
}
}
