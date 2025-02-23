using System.Collections.Generic;
using UnityEngine;

public class WalkChecker : MonoBehaviour
{
    [SerializeField]
    private bool isGround = false;

    public Unity.MLAgents.Agent agent;
    public List<WalkChecker> others;

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

            var isJumping = true;
            foreach (var checker in others)
            {
                if (checker.isGround)
                    isJumping = false;
            }

            if (isJumping)
            {
                others.ForEach(checker => checker.Reset());
                agent.EndEpisode();
            }
        }
    }

    public void Reset()
        => IsGround = false;
}
