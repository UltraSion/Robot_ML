using UnityEngine;

namespace ETC
{
public class DirTest : MonoBehaviour
{
    public GameObject target;

    public void FixedUpdate()
    {
        var dir = transform.InverseTransformDirection(target.transform.position);
        var poi = transform.InverseTransformPoint(target.transform.position);


        Debug.Log($"Dir: {dir}, Poi: {poi}");
    }
}
}
