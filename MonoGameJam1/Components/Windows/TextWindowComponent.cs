using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Tweens;

namespace MonoGameJam1.Components.Windows
{
    public class TextWindowComponent : Component, IUpdatable
    {
        //--------------------------------------------------
        // Text

        private string _text;

        //--------------------------------------------------
        // Index

        private int _index;

        //--------------------------------------------------
        // Markup Component

        private MarkupText _markupComponent;

        //--------------------------------------------------
        // Delay

        private float _delay;
        public float TextDelay { get; set; }

        //--------------------------------------------------
        // Playing

        private bool _playing;
        public bool Playing => _playing;
        
        //--------------------------------------------------
        // Window States

        private bool _active;
        private bool _opening;
        private bool _closing;
        
        //--------------------------------------------------
        // Sizes

        private float _maxWidth;
        private Vector2 _textSize;

        //--------------------------------------------------
        // Window Sprite

        private WindowSprite _window;

        // Tween related variables
        private float _windowAlpha;
        private float _windowPositionIncreaseY;

        //----------------------//------------------------//

        public TextWindowComponent()
        {
            _maxWidth = 200;
            _textSize.X = 200;

            _markupComponent = new MarkupText() { renderLayer = -1 };
            _markupComponent.setTextWidth(_maxWidth);

            TextDelay = 0.03f;
        }

        public override void onAddedToEntity()
        {
            createWindow();
            entity.addComponent(_markupComponent);
            updateWindowPlacement();
        }

        private void createWindow()
        {
            var texture = entity.scene.content.Load<Texture2D>(Content.System.windowskin);
            _window = entity.addComponent(new WindowSprite(texture));
        }

        public void start(string text, float maxWidth = -1.0f)
        {
            if (maxWidth < 0) maxWidth = 150.0f;
            _maxWidth = maxWidth;
            
            _opening = string.IsNullOrEmpty(_text);
            _text = "";
            _index = 0;
            _playing = true;
            _active = true;
            _closing = false;
            _delay = 0;

            _windowAlpha = 0.0f;
            _window.color = Color.White * 0.0f;
            this.tween("_windowAlpha", 1.0f, 0.4f).setEaseType(EaseType.SineOut).start();
            if (_opening)
            {
                _windowPositionIncreaseY = 30.0f;
                this.tween("_windowPositionIncreaseY", 0.0f, 0.4f).setEaseType(EaseType.SineOut).start();
            }

            var ms = Graphics.instance.bitmapFont.measureString(text);
            var maxTextWidth = ms.X;
            _textSize.X = Math.Min(_maxWidth, maxTextWidth);
            _markupComponent.setTextWidth(_textSize.X);
            _textSize = measureTextSize(text);

            updateWindowPlacement();

            _text = text;
            _window.setVisible(true);
        }

        public void close()
        {
            this.tween("_windowAlpha", 0.0f, 0.4f).setEaseType(EaseType.SineIn).start();
            this.tween("_windowPositionIncreaseY", 30.0f, 0.4f).setEaseType(EaseType.SineIn).start();
            _closing = true;
        }

        public void forceFinish()
        {
            _index = _text.Length - 1;
            _delay = TextDelay;
        }

        void IUpdatable.update()
        {
            if (!_active) return;
            updateTweens();
            if (!_playing) return;

            if (_windowAlpha < 1.0f) return;

            _delay += Time.deltaTime;
            if (_delay >= TextDelay)
            {
                _delay = 0;
                _index++;
                onTextChange();
                if (_index >= _text.Length)
                {
                    _playing = false;
                }
            }
        }

        private void updateTweens()
        {
            if (_markupComponent.alpha != _windowAlpha)
            {
                _window.color = Color.White * _windowAlpha;

                _markupComponent.setAlpha(_windowAlpha);
                _markupComponent.compile();

                updateWindowPlacement();

                if (_closing && _windowAlpha <= 0.0f)
                {
                    _active = false;
                    _window.setVisible(false);
                    _text = "";
                }
            }
        }

        private void onTextChange()
        {
            var text = wrapText(_text.Substring(0, _index));
            _markupComponent.setText(text);
            _markupComponent.compile();
        }

        private void updateWindowPlacement()
        {
            var collider = entity.getComponent<Collider>();
            var spacementX = _window.Windowskin.EdgeSize.X * 2 + 5;
            var spacementY = _window.Windowskin.EdgeSize.Y * 2 + 5;

            var windowSize = new Vector2(_textSize.X + spacementX, _textSize.Y);
            _window.Size = new Vector2(windowSize.X, windowSize.Y);

            var x = (collider.bounds.width - windowSize.X) / 2;
            var y = -(windowSize.Y + collider.bounds.height / 2 + spacementY) + _windowPositionIncreaseY;
            var localOffset = new Vector2(x, y);
            _markupComponent.localOffset = localOffset;
            _markupComponent.setTextWidth(windowSize.X - spacementX);
            
            var windowX = localOffset.X - _window.Windowskin.EdgeSize.X - 6 / 2;
            var windowY = localOffset.Y + _window.Windowskin.EdgeSize.Y - (_textSize.Y <= 20 ? 1 : 0);
            _window.localOffset = new Vector2(windowX, windowY);
        }

        private Vector2 measureTextSize(string text)
        {
            _markupComponent.setText(wrapText(text));
            _markupComponent.compile();
            var vect = new Vector2(_markupComponent.resultTextWidth, _markupComponent.height);
            _markupComponent.setText(wrapText(_text));
            _markupComponent.compile();
            return vect;
        }

        private string wrapText(string text)
        {
            var model = "<markuptext face='default' color='#ffffff' align='left'><p>{0}</p></markuptext>";
            return string.Format(model, text);
        }
    }
}
