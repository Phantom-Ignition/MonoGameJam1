using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameJam1.Components.Colliders;
using Nez;
using Nez.Sprites;
using Nez.Textures;

namespace MonoGameJam1.Components.Sprites
{
    public class AnimatedSprite : Sprite, IUpdatable
    {
        //--------------------------------------------------
        // Frames stuff

        private int _currentFrame;
        public int CurrentFrame => _currentFrame;

        private string _currentFrameList;
        public string CurrentAnimation => _currentFrameList;

        private bool _looped;
        public bool Looped => _looped;

        //--------------------------------------------------
        // Animations

        private Dictionary<string, FramesList> _animations;
        public Dictionary<string, FramesList> animations => _animations;
        private float _delayTick;

        //----------------------//------------------------//

        public AnimatedSprite(Texture2D texture, string initialFrame) : base(texture)
        {
            _currentFrame = 0;
            _currentFrameList = initialFrame;
            _delayTick = 0;
            _animations = new Dictionary<string, FramesList>();
            _looped = false;
        }
        
        public void CreateAnimation(string animation, float delay)
        {
            _animations[animation] = new FramesList(delay);
        }

        public void CreateAnimation(string animation, float delay, bool reset)
        {
            _animations[animation] = new FramesList(delay);
            _animations[animation].Reset = reset;
        }

        public void CreateAnimation(string animation)
        {
            _animations[animation] = new FramesList(0);
        }

        public void ResetCurrentAnimation()
        {
            _currentFrame = 0;
            _looped = false;
            _delayTick = 0;
        }

        public void AddFrames(string animation, List<Rectangle> frames, int[] offsetX, int[] offsetY)
        {
            for (var i = 0; i < frames.Count; i++)
            {
                var frameSubtexture = new Subtexture(subtexture.texture2D, frames[i]);
                _animations[animation].Frames.Add(new FrameInfo(frameSubtexture, offsetX[i], offsetY[i]));
            }
        }

        public void AddFrames(string animation, List<Rectangle> frames)
        {
            var offsetX = new int[frames.Count];
            var offsetY = new int[frames.Count];
            AddFrames(animation, frames, offsetX, offsetY);
        }

        public void AddAttackCollider(string name, List<List<Rectangle>> rectangleFrames)
        {
            for (var i = 0; i < rectangleFrames.Count; i++)
            {
                for (var j = 0; j < rectangleFrames[i].Count; j++)
                {
                    var collider = new AttackCollider(rectangleFrames[i][j].X, rectangleFrames[i][j].Y, rectangleFrames[i][j].Width, rectangleFrames[i][j].Height);
                    entity.addComponent(collider);
                    _animations[name].Frames[i].AttackColliders.Add(collider);
                }
            }
        }

        public void AddFramesToAttack(string name, params int[] frames)
        {
            for (var i = 0; i < frames.Length; i++)
                _animations[name].FramesToAttack.Add(frames[i]);
        }

        public void CloneAnimationsFrom(AnimatedSprite animatedSprite, Texture2D tex)
        {
            _animations = new Dictionary<string, FramesList>();
            foreach (var animation in animatedSprite.animations)
            {
                CreateAnimation(animation.Key, animation.Value.Delay);
                var frames = animation.Value.Frames;
                var rectList = new Rectangle[frames.Count];
                var offsetListX = new int[frames.Count];
                var offsetListY = new int[frames.Count];
                for (var i = 0; i < frames.Count; i++)
                {
                    rectList[i] = frames[i].Subtexture.sourceRect;
                    offsetListX[i] = frames[i].OffsetX;
                    offsetListY[i] = frames[i].OffsetY;
                }
                AddFrames(animation.Key, rectList.ToList(), offsetListX, offsetListY);
            }
        }

        void IUpdatable.update()
        {
            foreach (var frame in _animations[_currentFrameList].Frames)
            {
                foreach (var collider in frame.AttackColliders)
                {
                    var offsetX = 0f;
                    if (spriteEffects == SpriteEffects.FlipHorizontally)
                        offsetX = -2.0f * collider.X;
                    collider.ApplyOffset(offsetX, 0);
                }
            }

            if (_animations[_currentFrameList].Loop)
            {
                var currentAnimation = _animations[_currentFrameList];
                _delayTick += Time.deltaTime;
                if (_delayTick > currentAnimation.Delay)
                {
                    _delayTick -= currentAnimation.Delay;
                    _currentFrame++;
                    if (_currentFrame == currentAnimation.Frames.Count)
                    {
                        if (!currentAnimation.Reset)
                        {
                            _currentFrame--;
                            currentAnimation.Loop = false;
                        }
                        else _currentFrame = 0;
                        if (!_looped) _looped = true;
                    }
                }
                var currentFrame = currentAnimation.Frames[_currentFrame];
                var rsubtexture = currentFrame.Subtexture;
                setSubtexture(rsubtexture);
                _localOffset = new Vector2(currentFrame.OffsetX, currentFrame.OffsetY);
            }
        }

        public bool isOnCombableFrame()
        {
            return _currentFrame >= _animations[_currentFrameList].Frames.Count - 3;
        }

        public int getDirection()
        {
            return spriteEffects == SpriteEffects.FlipHorizontally ? -1 : 1;
        }

        public FramesList getCurrentAnimation()
        {
            return _animations[_currentFrameList];
        }

        public FrameInfo getCurrentFrame()
        {
            return _animations[_currentFrameList].Frames[_currentFrame];
        }

        public void play(string animation)
        {
            if (_currentFrameList == animation) return;
            _currentFrame = 0;
            _delayTick = 0;
            _currentFrameList = animation;
            _looped = false;
            if (!_animations[_currentFrameList].Reset)
            {
                _animations[_currentFrameList].Loop = true;
            }
        }

        public override void debugRender(Graphics graphics)
        {
            base.debugRender(graphics);
            
            foreach (var collider in _animations[_currentFrameList].Frames[_currentFrame].AttackColliders)
            {
                collider.debugRender(graphics, true);
            }
        }
    }
}
