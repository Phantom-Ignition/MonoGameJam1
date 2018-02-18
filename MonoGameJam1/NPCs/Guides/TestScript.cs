using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoGameJam1.NPCs
{
    class TestScript : NpcBase
    {
        public TestScript(string name) : base(name)
        {
            RunOnTouch = true;
            Invisible = true;
            Interactable = false;
        }

        protected override void loadTexture() { }

        protected override void createActionList()
        {
            playerMessage("Hello bois");
            closePlayerMessage();
            wait(0.5f);
            setGlobalSwitch("instructions1", true);
            executeAction(() => { Enabled = false; });
        }
    }
}
