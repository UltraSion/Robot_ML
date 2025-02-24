using System.Collections.Generic;
using UnityEngine;

public class WalkChecker : MonoBehaviour
{
    [SerializeField] private bool isGround = false;

    [SerializeField] private bool isReady = false;

    public Unity.MLAgents.Agent agent;
    public List<WalkChecker> others;

    public bool IsGround
    {
        get
        {
            if (!isReady)
                return true;

            return isGround;
        }
        private set => isGround = value;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!other.transform.CompareTag("Ground"))
            return;


        if (!isReady)
        {
            isReady = true;
            others.ForEach(checker => checker.isReady = true);
        }

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
    {
        IsGround = false;
        isReady = false;
    }
}
