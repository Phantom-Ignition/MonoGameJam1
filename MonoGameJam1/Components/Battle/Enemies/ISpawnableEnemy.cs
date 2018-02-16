using Nez;

namespace MonoGameJam1.Components.Battle.Enemies
{
    public interface ISpawnableEnemy
    {
        RectangleF MovableArea { get; set; }
        void GoToSpawnState();
    }
}
