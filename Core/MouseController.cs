using Microsoft.Xna.Framework.Input;

namespace Hands.Core;

internal static class MouseController
{
    private static MouseState   _previousMouseState;
    
    public static void Update()
    {
        MouseState                  = Mouse.GetState();
        bool isMouseLeftClicked     = _previousMouseState.LeftButton    == ButtonState.Pressed && MouseState.LeftButton     == ButtonState.Released;
        bool isMouseRightClicked    = _previousMouseState.RightButton   == ButtonState.Pressed && MouseState.RightButton    == ButtonState.Released;

        MouseButton = (isMouseLeftClicked, isMouseRightClicked) switch
        {
            (true, false)   => MouseButtonEnum.Left,
            (false, true)   => MouseButtonEnum.Right,
            (true, true)    => MouseButtonEnum.Both,
            _               => MouseButtonEnum.None
        };


        int mouseWheel = MouseState.ScrollWheelValue;
        int previousMouseWheel = _previousMouseState.ScrollWheelValue;
        MouseWheel = MouseWheelEnum.None;
        if (mouseWheel > previousMouseWheel) MouseWheel = MouseWheelEnum.Up;
        if (mouseWheel < previousMouseWheel) MouseWheel = MouseWheelEnum.Down;

        _previousMouseState = MouseState;

    }

    public static Rectangle MouseBounds =>
        new((int)MousePosition.X, (int)MousePosition.Y, 1, 1);
    
    public static Vector2 MousePosition =>
        new Vector2(Mouse.GetState().X, Mouse.GetState().Y);

    

    public static MouseState        MouseState    { get; private set; }

    public static MouseButtonEnum   MouseButton   { get; private set; }

    public static MouseWheelEnum    MouseWheel    {  get; private set; }
}

internal enum MouseButtonEnum
{
    None,
    Left,
    Right,
    Both
}

internal enum MouseWheelEnum
{
    None,
    Up,
    Down
}
