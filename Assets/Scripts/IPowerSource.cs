public interface IPowerSource
{
    public float UsableForce { get; }

    public float ReleaseForce(float requestedForceAmount);
}