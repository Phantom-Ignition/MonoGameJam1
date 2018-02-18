namespace MonoGameJam1.NPCs
{
    public class GuideOne : NpcBase
    {
        public GuideOne(string name) : base(name)
        {
            RunOnTouch = true;
            Invisible = true;
            Interactable = false;
        }

        protected override void loadTexture() { }

        protected override void createActionList()
        {
            setGlobalSwitch("guide1", true);
            executeAction(() => { Enabled = false; });
        }
    }
}
