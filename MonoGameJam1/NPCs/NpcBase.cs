using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameJam1.Components;
using MonoGameJam1.Components.Sprites;
using MonoGameJam1.Components.Windows;
using MonoGameJam1.Managers;
using Nez;

namespace MonoGameJam1.NPCs
{
    public abstract class NpcBase : Component
    {
        //--------------------------------------------------
        // Sprite

        public AnimatedSprite sprite;
        public bool FlipX { get; set; }

        //--------------------------------------------------
        // Platform Object

        private PlatformerObject _platformerObject;

        //--------------------------------------------------
        // Text Window Component

        private TextWindowComponent _textWindowComponent;
        public TextWindowComponent TextWindowComponent => _textWindowComponent;

        //--------------------------------------------------
        // Name

        public string Name { get; set; }

        //--------------------------------------------------
        // Enabled

        public bool Enabled { get; set; }

        //--------------------------------------------------
        // Render Layer

        private int _renderLayer;

        //--------------------------------------------------
        // Commands

        private List<NpcCommand> _commands;

        //--------------------------------------------------
        // Local Switches

        private Dictionary<string, bool> _switches;
        public Dictionary<string, bool> Switches => _switches;

        //--------------------------------------------------
        // Local Variables

        private Dictionary<string, int> _variables;
        public Dictionary<string, int> Variables => _variables;

        //--------------------------------------------------
        // Texture

        protected Texture2D _texture;
        protected string TextureName
        {
            set => _texture = entity.scene.content.Load<Texture2D>(value);
        }

        //--------------------------------------------------
        // Object Rect

        public Rectangle ObjectRect { get; set; }

        //--------------------------------------------------
        // Invisible

        public bool Invisible { get; set; }

        //--------------------------------------------------
        // Run on touch

        public bool RunOnTouch { get; set; }

        //--------------------------------------------------
        // Interactable

        public bool Interactable { get; set; }

        //----------------------//------------------------//

        protected NpcBase(string name)
        {
            Enabled = true;
            Name = name;
            _commands = new List<NpcCommand>();
            _switches = new Dictionary<string, bool>();
            _variables = new Dictionary<string, int>();
        }

        public override void onAddedToEntity()
        {
            _platformerObject = entity.getComponent<PlatformerObject>();
            _textWindowComponent = entity.getComponent<TextWindowComponent>();

            loadTexture();
            createCollider();
            if (!Invisible)
            {
                createSprite();
                createAnimations();
                sprite.play("stand");
            }
        }

        protected abstract void loadTexture();
        protected abstract void createActionList();

        protected virtual void createCollider()
        {
            if (Invisible)
            {
                entity.addComponent(new BoxCollider(0, 0, ObjectRect.Width, ObjectRect.Height));
            }
            else
            {
                entity.addComponent(new BoxCollider(-14, -24, 32, 44));
            }
        }

        private void createSprite()
        {
            sprite = entity.addComponent(new AnimatedSprite(_texture, "stand"));
            sprite.renderLayer = _renderLayer;
            sprite.flipX = FlipX;
        }

        public void setRenderLayer(int renderLayer)
        {
            _renderLayer = renderLayer;
        }

        protected virtual void createAnimations()
        {
            sprite.CreateAnimation("stand");
            sprite.AddFrames("stand", new List<Rectangle>()
            {
                new Rectangle(0, 0, 64, 64)
            }, new int[] { 0 }, new int[] { -12 });
        }

        private IEnumerator actionList()
        {
            var index = 0;
            if (_commands.Count == 0)
            {
                Core.getGlobalManager<InputManager>().IsBusy = false;
                if (!Invisible)
                {
                    sprite.flipX = FlipX;
                }
                yield break;
            }
            while (true)
            {
                Input.update();
                var command = _commands[index];
                command.start();
                while (!command.update())
                {
                    yield return 0;
                }
                if (++index >= _commands.Count)
                {
                    Core.getGlobalManager<InputManager>().IsBusy = false;
                    if (!Invisible)
                    {
                        sprite.flipX = FlipX;
                    }
                    yield break;
                }
            }
        }

        public void executeActionList(bool turnToPlayer)
        {
            if (turnToPlayer)
            {
                var player = Core.getGlobalManager<SystemManager>().playerEntity;
                var differece = transform.position - player.position;
                if (!Invisible)
                {
                    sprite.flipX = differece.X > 0;
                }
            }
            _commands.Clear();
            createActionList();
            Core.startCoroutine(actionList());
        }

        #region NPC Commands

        protected void message(string message, float maxWidth = -1.0f)
        {
            _commands.Add(new NpcMessageCommand(this, message, maxWidth));
        }

        protected void playerMessage(string message, float maxWidth = -1.0f)
        {
            var player = entity.scene.findEntity("player");
            _commands.Add(new NpcMessageTargetCommand(this, player, message, maxWidth));
        }

        protected void closeMessage()
        {
            _commands.Add(new NpcCloseMessageCommand(this));
        }

        protected void closePlayerMessage()
        {
            var player = entity.scene.findEntity("player");
            _commands.Add(new NpcCloseTargetMessageCommand(this, player));
        }

        protected void wait(float duration)
        {
            _commands.Add(new NpcWaitCommand(this, duration));
        }

        protected void setSwitch(string name, bool value)
        {
            _commands.Add(new NpcSetSwitchCommand(this, true, name, value));
        }

        protected bool getSwitch(string name)
        {
            return _switches.ContainsKey(name) ? _switches[name] : false;
        }

        protected void setGlobalSwitch(string name, bool value)
        {
            _commands.Add(new NpcSetSwitchCommand(this, false, name, value));
        }

        protected bool getGlobalSwitch(string name)
        {
            var globalSwitches = Core.getGlobalManager<SystemManager>().switches;
            return globalSwitches.ContainsKey(name) ? globalSwitches[name] : false;
        }

        protected void setVariable(string name, int value)
        {
            _commands.Add(new NpcSetVariableCommand(this, true, name, value));
        }

        protected int getVariable(string name)
        {
            return _variables.ContainsKey(name) ? _variables[name] : 0;
        }

        protected void setGlobalVariable(string name, int value)
        {
            _commands.Add(new NpcSetVariableCommand(this, false, name, value));
        }

        protected int getGlobalVariable(string name)
        {
            var globalVariables = Core.getGlobalManager<SystemManager>().variables;
            return globalVariables.ContainsKey(name) ? globalVariables[name] : 0;
        }

        protected void focusCamera(Entity target)
        {
            _commands.Add(new NpcFocusCameraCommand(this, target));
        }

        protected void cinematicIn(float amount, float duration)
        {
            _commands.Add(new NpcCinematicCommand(this, amount, duration, true));
        }

        protected void cinematicOut(float amount, float duration)
        {
            _commands.Add(new NpcCinematicCommand(this, amount, duration, false));
        }

        protected void movePlayer(Vector2 velocity)
        {
            _commands.Add(new NpcMovePlayerCommand(this, velocity));
        }

        protected void hideTexture()
        {
            _commands.Add(new NpcHideTextureCommand(this, true));
        }

        protected void mapTransfer(int mapId, int mapX, int mapY)
        {
            _commands.Add(new NpcMapTransferCommand(this, mapId, mapX, mapY));
        }

        protected void executeAction(Action action)
        {
            _commands.Add(new NpcExecuteActionCommand(this, action));
        }

        #endregion
    }
}
