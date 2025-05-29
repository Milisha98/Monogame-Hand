using Microsoft.Xna.Framework.Input;
using System.Linq;

namespace Hands.Core;

internal static class KeyboardController
{
    private static Keys[] _lastKeysDown = [];

    public static Vector2 CheckInput()
    {
        var v = Vector2.Zero;

        if (CheckKeyDown(Keys.Left, Keys.A, Keys.Q, Keys.Z, Keys.NumPad7, Keys.NumPad4, Keys.NumPad1))
        {
            v += new Vector2(-1, 0);
        }

        if (CheckKeyDown(Keys.Right, Keys.D, Keys.E, Keys.C, Keys.NumPad9, Keys.NumPad6, Keys.NumPad3))
        {
            v += new Vector2(1, 0);
        }

        if (CheckKeyDown(Keys.Up, Keys.W, Keys.Q, Keys.E, Keys.NumPad7, Keys.NumPad8, Keys.NumPad9))
        {
            v += new Vector2(0, -1);
        }

        if (CheckKeyDown(Keys.Down, Keys.S, Keys.Z, Keys.X, Keys.C, Keys.NumPad1, Keys.NumPad2, Keys.NumPad3))
        {
            v += new Vector2(0, 1);
        }       

        return v;
    }

    public static bool CheckKeyDown(params Keys[] keys)
    {
        var keyboardState = Keyboard.GetState();
        return keys.Any(x => keyboardState.IsKeyDown(x));
    }

    public static Keys[] KeysPressed()
    {
        var keyboardState = Keyboard.GetState();
        var currentKeys = keyboardState.GetPressedKeys();
        var returnKeys = currentKeys.Except(_lastKeysDown).ToArray();
        _lastKeysDown = currentKeys;
        return returnKeys;
    }

}
