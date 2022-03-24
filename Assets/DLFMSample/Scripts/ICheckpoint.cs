using static Level.Line;

namespace Level
{
    public interface ICheckpoint
    {
        float Time { get; }
        bool IsAvailable { get; }
        LineRespawnAttributes[] LineRespawnAttributes { get; }
        void Respawn();
    }
}
