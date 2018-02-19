using Nez;

namespace MonoGameJam1.Scenes.SceneMapExtensions
{
    public class MoveToCredits : ISceneMapExtensionable
    {
        public Scene Scene { get; set; }

        private bool _startedTransition;
        private bool _deatachedEntities;

        public void initialize()
        {
        }

        public void update()
        {
            if (!_deatachedEntities)
            {
                _deatachedEntities = true;
                Scene.findEntity("player").detachFromScene();
                Scene.findEntity("hud").detachFromScene();
            }
            if (!Core.isOnTransition() && !_startedTransition)
            {
                _startedTransition = true;
                Core.startSceneTransition(new CrossFadeTransition(() => new SceneCredits()));
            }
        }

        public void receiveSceneMessage(string message)
        {
        }
    }
}
