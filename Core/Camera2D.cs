using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Hands.Core;
internal class Camera2D
{
    private readonly Rectangle _bounds;

    public Camera2D() : this(null)
    {
    }
    public Camera2D(Viewport? viewport)
    {
        var defaultViewPort = new Viewport(0, 0, Global.Graphics.RenderWidth, Global.Graphics.RenderHeight);
        _bounds = (viewport ?? defaultViewPort).Bounds;
    }

    public float Zoom { get; set; } = 1f;
    public Vector2 Position { get; set; }
    public float Rotation { get; set; } = 0f;
    public Rectangle Bounds => new Rectangle(_bounds.X, _bounds.Y, _bounds.X + Global.Graphics.ScreenWidth, _bounds.Y + Global.Graphics.ScreenHeight);

    public Vector2 Center
    {
        get => new(Position.X + (_bounds.Width * 0.5f), Position.Y + (_bounds.Height * 0.5f));
    }

    public void LookAt(Vector2 position)
    {
        Position = position - new Vector2(_bounds.Width * 0.5f, _bounds.Height * 0.5f);
    }

    public Vector2 WorldToScreenPosition(Vector2 position)
    {
        return Vector2.Transform(position, ViewMatrix);
    }

    public Matrix ViewMatrix
    {
        get =>
                    Matrix.CreateTranslation(new Vector3(-Position.X, -Position.Y, 0)) *
                    Matrix.CreateRotationZ(Rotation) *
                    Matrix.CreateScale(Zoom) *
                    Matrix.CreateTranslation(new Vector3(_bounds.Width * 0.5f, _bounds.Height * 0.5f, 0));
    }

}

