using Nez;

namespace MonoGameJam1.Scenes.SceneMapExtensions
{
    public interface ISceneMapExtensionable
    {
        Scene Scene { get; set; }
        void initialize();
        void update();
        void receiveSceneMessage(string message);
    }
}
