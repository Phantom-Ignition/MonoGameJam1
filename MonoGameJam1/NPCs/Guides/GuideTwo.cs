namespace MonoGameJam1.NPCs
{
    public class GuideTwo : NpcBase
    {
        public GuideTwo(string name) : base(name)
        {
            RunOnTouch = true;
            Invisible = true;
            Interactable = false;
        }

        protected override void loadTexture() { }

        protected override void createActionList()
        {
            setGlobalSwitch("guide1", false);
            setGlobalSwitch("guide2", true);
            executeAction(() => { Enabled = false; });
        }
    }
}
