using UnityEngine;

public class WalkChecker : MonoBehaviour
{
    [SerializeField]
    private bool isGround = true;

    public bool IsGround
    {
        get => isGround;
        private set => isGround = value;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.transform.CompareTag("Ground"))
            IsGround = true;
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.transform.CompareTag("Ground"))
        {
            IsGround = false;
        }
    }

    public void Reset()
        => IsGround = true;
}