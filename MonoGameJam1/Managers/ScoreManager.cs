using Nez;
using Nez.Tweens;

namespace MonoGameJam1.Managers
{
    public class ScoreManager : IUpdatableManager
    {
        //--------------------------------------------------
        // Score

        public int Score { get; private set; }
        public float ScaleMultiplier { get; private set; }

        private ITweenable _scaleTween;

        //----------------------//------------------------//

        public ScoreManager()
        {
            ScaleMultiplier = 1.0f;
        }

        public void GetHostageRescuePoints()
        {
            AnimateScaleMultiplier();
            Score += 500;
        }

        public void GetComboPoints(float currentDamageMultiplier)
        {
            AnimateScaleMultiplier();
            Score += (int)(100 * currentDamageMultiplier);
        }

        private void AnimateScaleMultiplier()
        {
            _scaleTween?.stop();

            ScaleMultiplier = 1.4f;
            _scaleTween = this.tween("ScaleMultiplier", 1f, 0.4f).setEaseType(EaseType.ExpoOut);
            _scaleTween.start();
        }

        public void update() { }
    }
}
