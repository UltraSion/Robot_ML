using UnityEngine;

public class BatteryTest : MonoBehaviour
{
    public Battery battery;

    private void Start()
    {
        battery = GetComponent<Battery>();
    }

    private void FixedUpdate()
    {
        battery.Charge(10f * Time.fixedDeltaTime);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            float force = battery.ReleaseForce(100f);
            Debug.Log(force);
        }

    }
}