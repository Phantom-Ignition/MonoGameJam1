using System;
using Microsoft.Xna.Framework;
using MonoGameJam1.Components.Player;
using MonoGameJam1.Components.Windows;
using MonoGameJam1.Managers;
using MonoGameJam1.Scenes;
using MonoGameJam1.Systems;
using Nez;

namespace MonoGameJam1.NPCs
{
    public abstract class NpcCommand
    {
        protected NpcBase _npc;

        public NpcCommand(NpcBase npc)
        {
            _npc = npc;
        }

        public abstract void start();
        public abstract bool update();
    }

    public class NpcMessageCommand : NpcCommand
    {
        private string _text;
        private float _maxWidth;

        public NpcMessageCommand(NpcBase npc, string text, float maxWidth) : base(npc)
        {
            _text = text;
            _maxWidth = maxWidth;
        }

        public override void start()
        {
            _npc.TextWindowComponent.start(_text, _maxWidth);
        }

        public override bool update()
        {
            if (Core.getGlobalManager<InputManager>().InteractionButton.isPressed)
            {
                if (_npc.TextWindowComponent.Playing)
                {
                    _npc.TextWindowComponent.forceFinish();
                }
                else
                {
                    return true;
                }
            }
            return false;
        }
    }

    public class NpcMessageTargetCommand : NpcCommand
    {
        private TextWindowComponent _textWindowComponent;
        private string _text;
        private float _maxWidth;

        public NpcMessageTargetCommand(NpcBase npc, Entity target, string text, float maxWidth) : base(npc)
        {
            _text = text;
            _maxWidth = maxWidth;
            _textWindowComponent = target.getComponent<TextWindowComponent>();
        }

        public override void start()
        {
            _textWindowComponent.start(_text, _maxWidth);
        }

        public override bool update()
        {
            if (Core.getGlobalManager<InputManager>().InteractionButton.isPressed)
            {
                if (_textWindowComponent.Playing)
                {
                    _textWindowComponent.forceFinish();
                }
                else
                {
                    return true;
                }
            }
            return false;
        }
    }

    public class NpcCloseMessageCommand : NpcCommand
    {
        public NpcCloseMessageCommand(NpcBase npc) : base(npc)
        { }

        public override void start()
        {
            _npc.TextWindowComponent.close();
        }

        public override bool update()
        {
            return true;
        }
    }

    public class NpcCloseTargetMessageCommand : NpcCommand
    {
        private TextWindowComponent _textWindowComponent;

        public NpcCloseTargetMessageCommand(NpcBase npc, Entity target) : base(npc)
        {
            _textWindowComponent = target.getComponent<TextWindowComponent>();
        }

        public override void start()
        {
            _textWindowComponent.close();
        }

        public override bool update()
        {
            return true;
        }
    }

    public class NpcWaitCommand : NpcCommand
    {
        private float _duration;
        private float _current;

        public NpcWaitCommand(NpcBase npc, float duration) : base(npc)
        {
            _duration = duration;
        }

        public override void start()
        { }

        public override bool update()
        {
            _current += Time.deltaTime;
            if (_current >= _duration)
            {
                return true;
            }
            return false;
        }
    }

    public class NpcSetSwitchCommand : NpcCommand
    {
        private bool _isLocal;
        private string _name;
        private bool _value;

        public NpcSetSwitchCommand(NpcBase npc, bool isLocal, string name, bool value) : base(npc)
        {
            _isLocal = isLocal;
            _name = name;
            _value = value;
        }

        public override void start()
        {
            if (_isLocal)
            {
                _npc.Switches[_name] = _value;
            }
            else
            {
                Core.getGlobalManager<SystemManager>().switches[_name] = _value;
            }
        }

        public override bool update()
        {
            return true;
        }
    }

    public class NpcSetVariableCommand : NpcCommand
    {
        private bool _isLocal;
        private string _name;
        private int _value;

        public NpcSetVariableCommand(NpcBase npc, bool isLocal, string name, int value) : base(npc)
        {
            _isLocal = isLocal;
            _name = name;
            _value = value;
        }

        public override void start()
        {
            if (_isLocal)
            {
                _npc.Variables[_name] = _value;
            }
            else
            {
                Core.getGlobalManager<SystemManager>().variables[_name] = _value;
            }
        }

        public override bool update()
        {
            return true;
        }
    }
    
    public class NpcFocusCameraCommand : NpcCommand
    {
        private Entity _target;

        public NpcFocusCameraCommand(NpcBase npc, Entity target) : base(npc)
        {
            _target = target;
        }

        public override void start()
        {
            _npc.entity.scene.getEntityProcessor<CameraSystem>().SetTarget(_target);
        }

        public override bool update()
        {
            return true;
        }
    }

    public class NpcCinematicCommand : NpcCommand
    {
        private float _amount;
        private float _duration;
        private bool _isIn;

        public NpcCinematicCommand(NpcBase npc, float amount, float duration, bool isIn) : base(npc)
        {
            _amount = amount;
            _duration = duration;
            _isIn = isIn;
        }

        public override void start()
        {
            var sysManager = Core.getGlobalManager<SystemManager>();
            if (_isIn)
            {
                Core.startCoroutine(sysManager.cinematicLetterboxPostProcessor.animateIn(_amount, _duration));
            }
            else
            {
                Core.startCoroutine(sysManager.cinematicLetterboxPostProcessor.animateOut(_duration));
            }
            /*
            var sys = Core.getGlobalManager<SystemManager>();
            sys.tween("cinematicAmount", _amount, _duration).setEaseType(Nez.Tweens.EaseType.SineInOut).start(); 
            */
        }

        public override bool update()
        {
            return true;
        }
    }

    public class NpcMovePlayerCommand : NpcCommand
    {
        private Vector2 _velocity;
        private float _duration;

        public NpcMovePlayerCommand(NpcBase npc, Vector2 velocity) : base(npc)
        {
            _velocity = velocity;
        }

        public override void start()
        {
            var player = _npc.entity.scene.findEntity("player");
            player.getComponent<PlayerComponent>().forceMovement(_velocity);
        }

        public override bool update()
        {
            return true;
        }
    }

    public class NpcHideTextureCommand : NpcCommand
    {
        private bool _hide;

        public NpcHideTextureCommand(NpcBase npc, bool hide) : base(npc)
        {
            _hide = hide;
        }

        public override void start()
        {
            _npc.sprite.setEnabled(!_hide);
        }

        public override bool update()
        {
            return true;
        }
    }

    public class NpcMapTransferCommand : NpcCommand
    {
        private int _mapId;
        private int _mapX;
        private int _mapY;

        public NpcMapTransferCommand(NpcBase npc, int mapId, int mapX, int mapY) : base(npc)
        {
            _mapId = mapId;
            _mapX = mapX;
            _mapY = mapY;
        }

        public override void start()
        {
            var mapScene = (SceneMap)_npc.entity.scene;
            mapScene.reserveTransfer(_mapId, _mapX, _mapY);
        }

        public override bool update()
        {
            return true;
        }
    }

    public class NpcExecuteActionCommand : NpcCommand
    {
        private Action _action;

        public NpcExecuteActionCommand(NpcBase npc, Action action) : base(npc)
        {
            _action = action;
        }

        public override void start()
        {
            _action.Invoke();
        }

        public override bool update()
        {
            return true;
        }
    }
}
