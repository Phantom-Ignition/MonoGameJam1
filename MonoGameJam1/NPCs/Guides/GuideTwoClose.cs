namespace MonoGameJam1.NPCs.Guides
{
    public class GuideTwoClose : NpcBase
    {
        public GuideTwoClose(string name) : base(name)
        {
            RunOnTouch = true;
            Invisible = true;
            Interactable = false;
        }

        protected override void loadTexture() { }

        protected override void createActionList()
        {
            setGlobalSwitch("guide2", false);
            executeAction(() => { Enabled = false; });
        }
    }
}
