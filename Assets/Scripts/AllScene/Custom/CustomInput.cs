using System;
using System.Collections.Generic;
using XInputDotNetPure;
using UnityEngine;

#region Enums

public enum GamepadStick
{
    right,
    left
}

public enum GamepadTrigger
{
    right,
    left
}

public enum PlayerIndex
{
    One = 1,
    Two = 2,
    Three = 3,
    Four = 4,
    Five = 5,
    All = 6
}

public enum ControllerType
{
    Keyboard = 0,
    Gamepad1 = 1,
    Gamepad2 = 2,
    Gamepad3 = 3,
    Gamepad4 = 4,
    GamepadAll = 5,
    All = 6
}

public enum MouseWheelDirection
{
    Up,
    Down,
    none
}

public enum NegativeKeyCode
{
    None = 0,
    GP1RT = -1,
    GP1LT = -2,
    GP1DPadUp = -3,
    GP1DPadRight = -4,
    GP1DPadDown = -5,
    GP1DPadLeft = -6,
    GP1TBSRUp = -7,
    GP1TBSRDown = -8,
    GP1TBSRRight = -9,
    GP1TBSRLeft = -10,
    GP1TBSLUp = -11,
    GP1TBSLDown = -12,
    GP1TBSLRight = -13,
    GP1TBSLLeft = -14,

    GP2RT = -15,
    GP2LT = -16,
    GP2DPadUp = -17,
    GP2DPadRight = -18,
    GP2DPadDown = -19,
    GP2DPadLeft = -20,
    GP2TBSRUp = -21,
    GP2TBSRDown = -22,
    GP2TBSRRight = -23,
    GP2TBSRLeft = -24,
    GP2TBSLUp = -25,
    GP2TBSLDown = -26,
    GP2TBSLRight = -27,
    GP2TBSLLeft = -28,

    GP3RT = -29,
    GP3LT = -30,
    GP3DPadUp = -31,
    GP3DPadRight = -32,
    GP3DPadDown = -33,
    GP3DPadLeft = -34,
    GP3TBSRUp = -35,
    GP3TBSRDown = -36,
    GP3TBSRRight = -37,
    GP3TBSRLeft = -38,
    GP3TBSLUp = -39,
    GP3TBSLDown = -40,
    GP3TBSLRight = -41,
    GP3TBSLLeft = -42,

    GP4RT = -43,
    GP4LT = -44,
    GP4DPadUp = -45,
    GP4DPadRight = -46,
    GP4DPadDown = -47,
    GP4DPadLeft = -48,
    GP4TBSRUp = -49,
    GP4TBSRDown = -50,
    GP4TBSRRight = -51,
    GP4TBSRLeft = -52,
    GP4TBSLUp = -53,
    GP4TBSLDown = -54,
    GP4TBSLRight = -55,
    GP4TBSLLeft = -56,

    GPAllRT = -57,
    GPAllLT = -58,
    GPAllDPadUp = -59,
    GPAllDPadRight = -60,
    GPAllDPadDown = -61,
    GPAllDPadLeft = -62,
    GPAllTBSRUp = -63,
    GPAllTBSRDown = -64,
    GPAllTBSRRight = -65,
    GPAllTBSRLeft = -66,
    GPAllTBSLUp = -67,
    GPAllTBSLDown = -68,
    GPAllTBSLRight = -69,
    GPAllTBSLLeft = -70,
}

#endregion

public static class CustomInput
{
    #region Keys config

    //différents players controls
    private static InputData defaultKBKeys = new InputData();
    private static InputData defaultGB1Keys = new InputData();
    private static InputData defaultGB2Keys = new InputData();
    private static InputData defaultGB3Keys = new InputData();
    private static InputData defaultGB4Keys = new InputData();
    private static InputData player1Keys = new InputData();
    private static InputData player2Keys = new InputData();
    private static InputData player3Keys = new InputData();
    private static InputData player4Keys = new InputData();
    private static InputData player5Keys = new InputData();

    //keyboard/gamepad controls
    private static InputData kbKeys = new InputData();
    private static InputData defaultBGKeys = new InputData();
    private static InputData gbKeys = new InputData();

    #endregion

    #region require

    private static GamePadState newGP1State, oldGP1State;
    private static GamePadState newGP2State, oldGP2State;
    private static GamePadState newGP3State, oldGP3State;
    private static GamePadState newGP4State, oldGP4State;

    private static Vector2 oldGP1Triggers, oldGP2Triggers, oldGP3Triggers, oldGP4Triggers, newGP1Triggers, newGP2Triggers, newGP3Triggers, newGP4Triggers;
    private static Vector2 oldGP1RightStickPosition, oldGP2RightStickPosition, oldGP3RightStickPosition, oldGP4RightStickPosition;
    private static Vector2 oldGP1LeftStickPosition, oldGP2LeftStickPosition, oldGP3LeftStickPosition, oldGP4LeftStickPosition;
    private static Vector2 newGP1RightStickPosition, newGP2RightStickPosition, newGP3RightStickPosition, newGP4RightStickPosition;
    private static Vector2 newGP1LeftStickPosition, newGP2LeftStickPosition, newGP3LeftStickPosition, newGP4LeftStickPosition;

    //Trigger gauche/droite : si vraiXGauche/vraiXDroit <= deadZone.x/y => xGauche/xDroit = 0, si vraiXGauche/vraiXDroit >= 1 - deadZone.x/y => xGauche/xDroit = 1, sinon xGauche/xDroit = vraiXGauche/vraiXDroit
    //ThumbStick : si vraiPos.x/y€[-deadZone.x/y, deadZone.x/y] => pos.x/y = 0, vraiPos.x/y€[-1, 1-deadZone.x/y] U [1 - deadZone.x/y, 1] => pos.x/y = (vraiPos.x/y).Sign() * 1, sinon pos.x/y = vraiPos.x/y
    public static Vector2 GP1RightThumbStickDeadZone = new Vector2(0.1f, 0.1f), GP1LeftThumbStickDeadZone = new Vector2(0.1f, 0.1f), GP1TriggersDeadZone = new Vector2(0.1f, 0.1f);
    public static Vector2 GP2RightThumbStickDeadZone = new Vector2(0.1f, 0.1f), GP2LeftThumbStickDeadZone = new Vector2(0.1f, 0.1f), GP2TriggersDeadZone = new Vector2(0.1f, 0.1f);
    public static Vector2 GP3RightThumbStickDeadZone = new Vector2(0.1f, 0.1f), GP3LeftThumbStickDeadZone = new Vector2(0.1f, 0.1f), GP3TriggersDeadZone = new Vector2(0.1f, 0.1f);
    public static Vector2 GP4RightThumbStickDeadZone = new Vector2(0.1f, 0.1f), GP4LeftThumbStickDeadZone = new Vector2(0.1f, 0.1f), GP4TriggersDeadZone = new Vector2(0.1f, 0.1f);

    private static string[] letters = new string[36] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

    private static int[] keyCodeInt = { 0,8,9,12,13,19,27,32,33,34,35,36,37,38,39,40,41,42,43,44,45,46,47,48,49,50,51,52,53,54,55,56,57,58,59,60,61,62,63,64,92,93,94,95,96,97,98,99,100,101,102,103,104,105,106,107,108,108,110,111,112,113,114,115,116,
        117,118,119,120,121,122,123,124,125,126,127,256,257,258,259,260,261,262,263,264,265,266,267,268,269,270,271,272,273,274,275,276,277,278,279,280,281,282,283,284,285,286,287,288,289,290,291,292,293,294,295,296,300,301,302,303,304,305,306,307,308,
        309,310,311,312,313,314,315,316,317,318,319,323,324,325,326,327,328,329,330,331,332,333,334,335,336,337,338,339,340,341,342,343,344,345,346,347,348,349,350,351,352,353,354,355,356,357,358,359,360,361,362,363,364,365,366,367,368,369,370,371,372,
        373,374,375,376,377,378,379,380,381,382,383,384,385,386,387,388,389,390,391,392,393,394,395,396,397,398,399,400,401,402,403,404,405,406,407,408,409,410,411,412,413,414,415,416,417,418,419,420,421,422,423,424,425,426,427,428,429,430,431,432,433,
        434,435,436,437,438,439,440,441,442,443,444,445,446,447,448,449,450,451,452,453,454,455,456,457,458,459,460,461,462,463,464,465,466,467,468,469,470,471,472,473,474,475,476,477,478,479,480,481,482,483,484,485,486,487,488,489,490,491,492,493,494,
        495,496,497,498,499,500,501,502,503,504,505,506,507,508,509 };

    #region GetNegativeKeycode Down/Up/Pressed delegate

    private static readonly Func<bool>[] GetNegativeKeyCodeDownDelegate = new Func<bool>[71]
    {
        () => { return false; },
        () => { return oldGP1State.Triggers.Right <= GP1TriggersDeadZone.y && newGP1State.Triggers.Right > GP1TriggersDeadZone.y; },
        () => { return oldGP1State.Triggers.Left <= GP1TriggersDeadZone.x && newGP1State.Triggers.Left > GP1TriggersDeadZone.x; },
        () => { return oldGP1State.DPad.Up == ButtonState.Released && newGP1State.DPad.Up == ButtonState.Pressed; },
        () => { return oldGP1State.DPad.Right == ButtonState.Released && newGP1State.DPad.Right == ButtonState.Pressed; },
        () => { return oldGP1State.DPad.Down == ButtonState.Released && newGP1State.DPad.Down == ButtonState.Pressed; },
        () => { return oldGP1State.DPad.Left == ButtonState.Released && newGP1State.DPad.Left == ButtonState.Pressed; },
        () => { return oldGP1State.ThumbSticks.Right.Y <= GP1RightThumbStickDeadZone.y && newGP1State.ThumbSticks.Right.Y > GP1RightThumbStickDeadZone.y; },
        () => { return oldGP1State.ThumbSticks.Right.Y >= -GP1RightThumbStickDeadZone.y && newGP1State.ThumbSticks.Right.Y < -GP1RightThumbStickDeadZone.y; },
        () => { return oldGP1State.ThumbSticks.Right.X <= GP1RightThumbStickDeadZone.x && newGP1State.ThumbSticks.Right.X > GP1RightThumbStickDeadZone.x; },
        () => { return oldGP1State.ThumbSticks.Right.X >= -GP1RightThumbStickDeadZone.x && newGP1State.ThumbSticks.Right.X < -GP1RightThumbStickDeadZone.x; },
        () => { return oldGP1State.ThumbSticks.Left.Y <= GP1RightThumbStickDeadZone.y && newGP1State.ThumbSticks.Left.Y > GP1RightThumbStickDeadZone.y; },
        () => { return oldGP1State.ThumbSticks.Left.Y >= -GP1RightThumbStickDeadZone.y && newGP1State.ThumbSticks.Left.Y < -GP1RightThumbStickDeadZone.y; },
        () => { return oldGP1State.ThumbSticks.Left.X <= GP1RightThumbStickDeadZone.x && newGP1State.ThumbSticks.Left.X > GP1RightThumbStickDeadZone.x; },
        () => { return oldGP1State.ThumbSticks.Left.X >= -GP1RightThumbStickDeadZone.x && newGP1State.ThumbSticks.Left.X < -GP1RightThumbStickDeadZone.x; },

        () => { return oldGP2State.Triggers.Right <= GP2TriggersDeadZone.y && newGP2State.Triggers.Right > GP2TriggersDeadZone.y; },
        () => { return oldGP2State.Triggers.Left <= GP2TriggersDeadZone.x && newGP2State.Triggers.Left > GP2TriggersDeadZone.x; },
        () => { return oldGP2State.DPad.Up == ButtonState.Released && newGP2State.DPad.Up == ButtonState.Pressed; },
        () => { return oldGP2State.DPad.Right == ButtonState.Released && newGP2State.DPad.Right == ButtonState.Pressed; },
        () => { return oldGP2State.DPad.Down == ButtonState.Released && newGP2State.DPad.Down == ButtonState.Pressed; },
        () => { return oldGP2State.DPad.Left == ButtonState.Released && newGP2State.DPad.Left == ButtonState.Pressed; },
        () => { return oldGP2State.ThumbSticks.Right.Y <= GP2RightThumbStickDeadZone.y && newGP2State.ThumbSticks.Right.Y > GP2RightThumbStickDeadZone.y; },
        () => { return oldGP2State.ThumbSticks.Right.Y >= -GP2RightThumbStickDeadZone.y && newGP2State.ThumbSticks.Right.Y < -GP2RightThumbStickDeadZone.y; },
        () => { return oldGP2State.ThumbSticks.Right.X <= GP2RightThumbStickDeadZone.x && newGP2State.ThumbSticks.Right.X > GP2RightThumbStickDeadZone.x; },
        () => { return oldGP2State.ThumbSticks.Right.X >= -GP2RightThumbStickDeadZone.x && newGP2State.ThumbSticks.Right.X < -GP2RightThumbStickDeadZone.x; },
        () => { return oldGP2State.ThumbSticks.Left.Y <= GP2RightThumbStickDeadZone.y && newGP2State.ThumbSticks.Left.Y > GP2RightThumbStickDeadZone.y; },
        () => { return oldGP2State.ThumbSticks.Left.Y >= -GP2RightThumbStickDeadZone.y && newGP2State.ThumbSticks.Left.Y < -GP2RightThumbStickDeadZone.y; },
        () => { return oldGP2State.ThumbSticks.Left.X <= GP2RightThumbStickDeadZone.x && newGP2State.ThumbSticks.Left.X > GP2RightThumbStickDeadZone.x; },
        () => { return oldGP2State.ThumbSticks.Left.X >= -GP2RightThumbStickDeadZone.x && newGP2State.ThumbSticks.Left.X < -GP2RightThumbStickDeadZone.x; },

        () => { return oldGP3State.Triggers.Right <= GP3TriggersDeadZone.y && newGP3State.Triggers.Right > GP3TriggersDeadZone.y; },
        () => { return oldGP3State.Triggers.Left <= GP3TriggersDeadZone.x && newGP3State.Triggers.Left > GP3TriggersDeadZone.x; },
        () => { return oldGP3State.DPad.Up == ButtonState.Released && newGP3State.DPad.Up == ButtonState.Pressed; },
        () => { return oldGP3State.DPad.Right == ButtonState.Released && newGP3State.DPad.Right == ButtonState.Pressed; },
        () => { return oldGP3State.DPad.Down == ButtonState.Released && newGP3State.DPad.Down == ButtonState.Pressed; },
        () => { return oldGP3State.DPad.Left == ButtonState.Released && newGP3State.DPad.Left == ButtonState.Pressed; },
        () => { return oldGP3State.ThumbSticks.Right.Y <= GP3RightThumbStickDeadZone.y && newGP3State.ThumbSticks.Right.Y > GP3RightThumbStickDeadZone.y; },
        () => { return oldGP3State.ThumbSticks.Right.Y >= -GP3RightThumbStickDeadZone.y && newGP3State.ThumbSticks.Right.Y < -GP3RightThumbStickDeadZone.y; },
        () => { return oldGP3State.ThumbSticks.Right.X <= GP3RightThumbStickDeadZone.x && newGP3State.ThumbSticks.Right.X > GP3RightThumbStickDeadZone.x; },
        () => { return oldGP3State.ThumbSticks.Right.X >= -GP3RightThumbStickDeadZone.x && newGP3State.ThumbSticks.Right.X < -GP3RightThumbStickDeadZone.x; },
        () => { return oldGP3State.ThumbSticks.Left.Y <= GP3RightThumbStickDeadZone.y && newGP3State.ThumbSticks.Left.Y > GP3RightThumbStickDeadZone.y; },
        () => { return oldGP3State.ThumbSticks.Left.Y >= -GP3RightThumbStickDeadZone.y && newGP3State.ThumbSticks.Left.Y < -GP3RightThumbStickDeadZone.y; },
        () => { return oldGP3State.ThumbSticks.Left.X <= GP3RightThumbStickDeadZone.x && newGP3State.ThumbSticks.Left.X > GP3RightThumbStickDeadZone.x; },
        () => { return oldGP3State.ThumbSticks.Left.X >= -GP3RightThumbStickDeadZone.x && newGP3State.ThumbSticks.Left.X < -GP3RightThumbStickDeadZone.x; },

        () => { return oldGP4State.Triggers.Right <= GP4TriggersDeadZone.y && newGP4State.Triggers.Right > GP4TriggersDeadZone.y; },
        () => { return oldGP4State.Triggers.Left <= GP4TriggersDeadZone.x && newGP4State.Triggers.Left > GP4TriggersDeadZone.x; },
        () => { return oldGP4State.DPad.Up == ButtonState.Released && newGP4State.DPad.Up == ButtonState.Pressed; },
        () => { return oldGP4State.DPad.Right == ButtonState.Released && newGP4State.DPad.Right == ButtonState.Pressed; },
        () => { return oldGP4State.DPad.Down == ButtonState.Released && newGP4State.DPad.Down == ButtonState.Pressed; },
        () => { return oldGP4State.DPad.Left == ButtonState.Released && newGP4State.DPad.Left == ButtonState.Pressed; },
        () => { return oldGP4State.ThumbSticks.Right.Y <= GP4RightThumbStickDeadZone.y && newGP4State.ThumbSticks.Right.Y > GP4RightThumbStickDeadZone.y; },
        () => { return oldGP4State.ThumbSticks.Right.Y >= -GP4RightThumbStickDeadZone.y && newGP4State.ThumbSticks.Right.Y < -GP4RightThumbStickDeadZone.y; },
        () => { return oldGP4State.ThumbSticks.Right.X <= GP4RightThumbStickDeadZone.x && newGP4State.ThumbSticks.Right.X > GP4RightThumbStickDeadZone.x; },
        () => { return oldGP4State.ThumbSticks.Right.X >= -GP4RightThumbStickDeadZone.x && newGP4State.ThumbSticks.Right.X < -GP4RightThumbStickDeadZone.x; },
        () => { return oldGP4State.ThumbSticks.Left.Y <= GP4RightThumbStickDeadZone.y && newGP4State.ThumbSticks.Left.Y > GP4RightThumbStickDeadZone.y; },
        () => { return oldGP4State.ThumbSticks.Left.Y >= -GP4RightThumbStickDeadZone.y && newGP4State.ThumbSticks.Left.Y < -GP4RightThumbStickDeadZone.y; },
        () => { return oldGP4State.ThumbSticks.Left.X <= GP4RightThumbStickDeadZone.x && newGP4State.ThumbSticks.Left.X > GP4RightThumbStickDeadZone.x; },
        () => { return oldGP4State.ThumbSticks.Left.X >= -GP4RightThumbStickDeadZone.x && newGP4State.ThumbSticks.Left.X < -GP4RightThumbStickDeadZone.x; },

        () => { return (oldGP1State.Triggers.Right <= GP1TriggersDeadZone.y && newGP1State.Triggers.Right > GP1TriggersDeadZone.y)
            || (oldGP2State.Triggers.Right <= GP2TriggersDeadZone.y && newGP2State.Triggers.Right > GP2TriggersDeadZone.y)
            || (oldGP3State.Triggers.Right <= GP3TriggersDeadZone.y && newGP3State.Triggers.Right > GP3TriggersDeadZone.y)
            || (oldGP4State.Triggers.Right <= GP4TriggersDeadZone.y && newGP4State.Triggers.Right > GP4TriggersDeadZone.y); },
        () => { return (oldGP1State.Triggers.Left <= GP1TriggersDeadZone.x && newGP1State.Triggers.Left > GP1TriggersDeadZone.x)
            || (oldGP2State.Triggers.Left <= GP2TriggersDeadZone.x && newGP2State.Triggers.Left > GP2TriggersDeadZone.x)
            || (oldGP3State.Triggers.Left <= GP3TriggersDeadZone.x && newGP3State.Triggers.Left > GP2TriggersDeadZone.x)
            || (oldGP4State.Triggers.Left <= GP4TriggersDeadZone.x && newGP4State.Triggers.Left > GP4TriggersDeadZone.x); },
        () => { return (oldGP1State.DPad.Up == ButtonState.Released && newGP1State.DPad.Up == ButtonState.Pressed)
            || (oldGP2State.DPad.Up == ButtonState.Released && newGP2State.DPad.Up == ButtonState.Pressed)
            || (oldGP3State.DPad.Up == ButtonState.Released && newGP3State.DPad.Up == ButtonState.Pressed)
            || (oldGP4State.DPad.Up == ButtonState.Released && newGP4State.DPad.Up == ButtonState.Pressed); },
        () => { return (oldGP1State.DPad.Right == ButtonState.Released && newGP1State.DPad.Right == ButtonState.Pressed)
            || (oldGP2State.DPad.Right == ButtonState.Released && newGP2State.DPad.Right == ButtonState.Pressed)
            || (oldGP3State.DPad.Right == ButtonState.Released && newGP3State.DPad.Right == ButtonState.Pressed)
            || (oldGP4State.DPad.Right == ButtonState.Released && newGP4State.DPad.Right == ButtonState.Pressed); },
        () => { return (oldGP1State.DPad.Down == ButtonState.Released && newGP1State.DPad.Down == ButtonState.Pressed)
            || (oldGP2State.DPad.Down == ButtonState.Released && newGP2State.DPad.Down == ButtonState.Pressed)
            || (oldGP3State.DPad.Down == ButtonState.Released && newGP3State.DPad.Down == ButtonState.Pressed)
            || (oldGP4State.DPad.Down == ButtonState.Released && newGP4State.DPad.Down == ButtonState.Pressed); },
        () => { return (oldGP1State.DPad.Left == ButtonState.Released && newGP1State.DPad.Left == ButtonState.Pressed)
            || (oldGP2State.DPad.Left == ButtonState.Released && newGP2State.DPad.Left == ButtonState.Pressed)
            || (oldGP3State.DPad.Left == ButtonState.Released && newGP3State.DPad.Left == ButtonState.Pressed)
            || (oldGP4State.DPad.Left == ButtonState.Released && newGP4State.DPad.Left == ButtonState.Pressed); },
        () => { return (oldGP1State.ThumbSticks.Right.Y <= GP1RightThumbStickDeadZone.y && newGP1State.ThumbSticks.Right.Y > GP1RightThumbStickDeadZone.y)
            || (oldGP2State.ThumbSticks.Right.Y <= GP2RightThumbStickDeadZone.y && newGP2State.ThumbSticks.Right.Y > GP2RightThumbStickDeadZone.y)
            || (oldGP3State.ThumbSticks.Right.Y <= GP3RightThumbStickDeadZone.y && newGP3State.ThumbSticks.Right.Y > GP3RightThumbStickDeadZone.y)
            || (oldGP4State.ThumbSticks.Right.Y <= GP4RightThumbStickDeadZone.y && newGP4State.ThumbSticks.Right.Y > GP4RightThumbStickDeadZone.y); },
        () => { return (oldGP1State.ThumbSticks.Right.Y >= -GP1RightThumbStickDeadZone.y && newGP1State.ThumbSticks.Right.Y < -GP1RightThumbStickDeadZone.y)
            || (oldGP2State.ThumbSticks.Right.Y >= GP2RightThumbStickDeadZone.y && newGP2State.ThumbSticks.Right.Y < GP2RightThumbStickDeadZone.y)
            || (oldGP3State.ThumbSticks.Right.Y >= -GP3RightThumbStickDeadZone.y && newGP3State.ThumbSticks.Right.Y < -GP3RightThumbStickDeadZone.y)
            || (oldGP4State.ThumbSticks.Right.Y >= -GP4RightThumbStickDeadZone.y && newGP4State.ThumbSticks.Right.Y < -GP4RightThumbStickDeadZone.y); },
        () => { return (oldGP1State.ThumbSticks.Right.X <= GP1RightThumbStickDeadZone.x && newGP1State.ThumbSticks.Right.X > GP1RightThumbStickDeadZone.x)
            || (oldGP2State.ThumbSticks.Right.X <= GP2RightThumbStickDeadZone.x && newGP2State.ThumbSticks.Right.X > GP2RightThumbStickDeadZone.x)
            || (oldGP3State.ThumbSticks.Right.X <= GP3RightThumbStickDeadZone.x && newGP3State.ThumbSticks.Right.X > GP3RightThumbStickDeadZone.x)
            || (oldGP4State.ThumbSticks.Right.X <= GP4RightThumbStickDeadZone.x && newGP4State.ThumbSticks.Right.X > GP4RightThumbStickDeadZone.x); },
        () => { return (oldGP1State.ThumbSticks.Right.X >= -GP1RightThumbStickDeadZone.x && newGP1State.ThumbSticks.Right.X < -GP1RightThumbStickDeadZone.x)
            || (oldGP2State.ThumbSticks.Right.X >= -GP2RightThumbStickDeadZone.x && newGP2State.ThumbSticks.Right.X < -GP2RightThumbStickDeadZone.x)
            || (oldGP3State.ThumbSticks.Right.X >= -GP3RightThumbStickDeadZone.x && newGP3State.ThumbSticks.Right.X < -GP3RightThumbStickDeadZone.x)
            || (oldGP4State.ThumbSticks.Right.X >= -GP4RightThumbStickDeadZone.x && newGP4State.ThumbSticks.Right.X < -GP4RightThumbStickDeadZone.x); },
        () => { return (oldGP1State.ThumbSticks.Left.Y <= GP1RightThumbStickDeadZone.y && newGP1State.ThumbSticks.Left.Y > GP1RightThumbStickDeadZone.y)
            || (oldGP2State.ThumbSticks.Left.Y <= GP2RightThumbStickDeadZone.y && newGP2State.ThumbSticks.Left.Y > GP2RightThumbStickDeadZone.y)
            || (oldGP3State.ThumbSticks.Left.Y <= GP3RightThumbStickDeadZone.y && newGP3State.ThumbSticks.Left.Y > GP3RightThumbStickDeadZone.y)
            || (oldGP4State.ThumbSticks.Left.Y <= GP4RightThumbStickDeadZone.y && newGP4State.ThumbSticks.Left.Y > GP4RightThumbStickDeadZone.y); },
        () => { return (oldGP1State.ThumbSticks.Left.Y >= -GP1RightThumbStickDeadZone.y && newGP1State.ThumbSticks.Left.Y < -GP1RightThumbStickDeadZone.y)
            || (oldGP2State.ThumbSticks.Left.Y >= -GP2RightThumbStickDeadZone.y && newGP2State.ThumbSticks.Left.Y < -GP2RightThumbStickDeadZone.y)
            || (oldGP3State.ThumbSticks.Left.Y >= -GP3RightThumbStickDeadZone.y && newGP3State.ThumbSticks.Left.Y < -GP3RightThumbStickDeadZone.y)
            || (oldGP4State.ThumbSticks.Left.Y >= -GP4RightThumbStickDeadZone.y && newGP4State.ThumbSticks.Left.Y < -GP4RightThumbStickDeadZone.y); },
        () => { return (oldGP1State.ThumbSticks.Left.X <= GP1RightThumbStickDeadZone.x && newGP1State.ThumbSticks.Left.X > GP1RightThumbStickDeadZone.x)
            || (oldGP2State.ThumbSticks.Left.X <= GP2RightThumbStickDeadZone.x && newGP2State.ThumbSticks.Left.X > GP2RightThumbStickDeadZone.x)
            || (oldGP3State.ThumbSticks.Left.X <= GP3RightThumbStickDeadZone.x && newGP3State.ThumbSticks.Left.X > GP3RightThumbStickDeadZone.x)
            || (oldGP4State.ThumbSticks.Left.X <= GP4RightThumbStickDeadZone.x && newGP4State.ThumbSticks.Left.X > GP4RightThumbStickDeadZone.x); },
        () => { return (oldGP1State.ThumbSticks.Left.X >= -GP1RightThumbStickDeadZone.x && newGP1State.ThumbSticks.Left.X < -GP1RightThumbStickDeadZone.x)
            || (oldGP2State.ThumbSticks.Left.X >= -GP2RightThumbStickDeadZone.x && newGP2State.ThumbSticks.Left.X < -GP2RightThumbStickDeadZone.x)
            || (oldGP3State.ThumbSticks.Left.X >= -GP3RightThumbStickDeadZone.x && newGP3State.ThumbSticks.Left.X < -GP3RightThumbStickDeadZone.x)
            || (oldGP4State.ThumbSticks.Left.X >= -GP4RightThumbStickDeadZone.x && newGP4State.ThumbSticks.Left.X < -GP4RightThumbStickDeadZone.x); }
    };

    private static readonly Func<bool>[] GetNegativeKeyCodeUpDelegate = new Func<bool>[71]
    {
        () => { return false; },
        () => { return oldGP1State.Triggers.Right > GP1TriggersDeadZone.y && newGP1State.Triggers.Right <= GP1TriggersDeadZone.y; },
        () => { return oldGP1State.Triggers.Left > GP1TriggersDeadZone.x && newGP1State.Triggers.Left <= GP1TriggersDeadZone.x; },
        () => { return oldGP1State.DPad.Up == ButtonState.Pressed && newGP1State.DPad.Up == ButtonState.Released; },
        () => { return oldGP1State.DPad.Right == ButtonState.Pressed && newGP1State.DPad.Right == ButtonState.Released; },
        () => { return oldGP1State.DPad.Down == ButtonState.Pressed && newGP1State.DPad.Down == ButtonState.Released; },
        () => { return oldGP1State.DPad.Left == ButtonState.Pressed && newGP1State.DPad.Left == ButtonState.Released; },
        () => { return oldGP1State.ThumbSticks.Right.Y > GP1RightThumbStickDeadZone.y && newGP1State.ThumbSticks.Right.Y <= GP1RightThumbStickDeadZone.y; },
        () => { return oldGP1State.ThumbSticks.Right.Y < -GP1RightThumbStickDeadZone.y && newGP1State.ThumbSticks.Right.Y >= -GP1RightThumbStickDeadZone.y; },
        () => { return oldGP1State.ThumbSticks.Right.X > GP1RightThumbStickDeadZone.x && newGP1State.ThumbSticks.Right.X <= GP1RightThumbStickDeadZone.x; },
        () => { return oldGP1State.ThumbSticks.Right.X < -GP1RightThumbStickDeadZone.x && newGP1State.ThumbSticks.Right.X >= -GP1RightThumbStickDeadZone.x; },
        () => { return oldGP1State.ThumbSticks.Left.Y > GP1RightThumbStickDeadZone.y && newGP1State.ThumbSticks.Left.Y < GP1RightThumbStickDeadZone.y; },
        () => { return oldGP1State.ThumbSticks.Left.Y < -GP1RightThumbStickDeadZone.y && newGP1State.ThumbSticks.Left.Y > -GP1RightThumbStickDeadZone.y; },
        () => { return oldGP1State.ThumbSticks.Left.X > GP1RightThumbStickDeadZone.x && newGP1State.ThumbSticks.Left.X < GP1RightThumbStickDeadZone.x; },
        () => { return oldGP1State.ThumbSticks.Left.X < -GP1RightThumbStickDeadZone.x && newGP1State.ThumbSticks.Left.X > -GP1RightThumbStickDeadZone.x; },

        () => { return oldGP2State.Triggers.Right > GP2TriggersDeadZone.y && newGP2State.Triggers.Right <= GP2TriggersDeadZone.y; },
        () => { return oldGP2State.Triggers.Left > GP2TriggersDeadZone.x && newGP2State.Triggers.Left <= GP2TriggersDeadZone.x; },
        () => { return oldGP2State.DPad.Up == ButtonState.Pressed && newGP2State.DPad.Up == ButtonState.Released; },
        () => { return oldGP2State.DPad.Right == ButtonState.Pressed && newGP2State.DPad.Right == ButtonState.Released; },
        () => { return oldGP2State.DPad.Down == ButtonState.Pressed && newGP2State.DPad.Down == ButtonState.Released; },
        () => { return oldGP2State.DPad.Left == ButtonState.Pressed && newGP2State.DPad.Left == ButtonState.Released; },
        () => { return oldGP2State.ThumbSticks.Right.Y > GP2RightThumbStickDeadZone.y && newGP2State.ThumbSticks.Right.Y <= GP2RightThumbStickDeadZone.y; },
        () => { return oldGP2State.ThumbSticks.Right.Y < -GP2RightThumbStickDeadZone.y && newGP2State.ThumbSticks.Right.Y >= -GP2RightThumbStickDeadZone.y; },
        () => { return oldGP2State.ThumbSticks.Right.X > GP2RightThumbStickDeadZone.x && newGP2State.ThumbSticks.Right.X <= GP2RightThumbStickDeadZone.x; },
        () => { return oldGP2State.ThumbSticks.Right.X < -GP2RightThumbStickDeadZone.x && newGP2State.ThumbSticks.Right.X >= -GP2RightThumbStickDeadZone.x; },
        () => { return oldGP2State.ThumbSticks.Left.Y > GP2RightThumbStickDeadZone.y && newGP2State.ThumbSticks.Left.Y <= GP2RightThumbStickDeadZone.y; },
        () => { return oldGP2State.ThumbSticks.Left.Y < -GP2RightThumbStickDeadZone.y && newGP2State.ThumbSticks.Left.Y >= -GP2RightThumbStickDeadZone.y; },
        () => { return oldGP2State.ThumbSticks.Left.X > GP2RightThumbStickDeadZone.x && newGP2State.ThumbSticks.Left.X <= GP2RightThumbStickDeadZone.x; },
        () => { return oldGP2State.ThumbSticks.Left.X < -GP2RightThumbStickDeadZone.x && newGP2State.ThumbSticks.Left.X >= -GP2RightThumbStickDeadZone.x; },

        () => { return oldGP3State.Triggers.Right >  GP3TriggersDeadZone.y && newGP3State.Triggers.Right <= GP3TriggersDeadZone.y; },
        () => { return oldGP3State.Triggers.Left > GP3TriggersDeadZone.x && newGP3State.Triggers.Left <= GP3TriggersDeadZone.x; },
        () => { return oldGP3State.DPad.Up == ButtonState.Pressed && newGP3State.DPad.Up == ButtonState.Released; },
        () => { return oldGP3State.DPad.Right == ButtonState.Pressed && newGP3State.DPad.Right == ButtonState.Released; },
        () => { return oldGP3State.DPad.Down == ButtonState.Pressed && newGP3State.DPad.Down == ButtonState.Released; },
        () => { return oldGP3State.DPad.Left == ButtonState.Pressed && newGP3State.DPad.Left == ButtonState.Released; },
        () => { return oldGP3State.ThumbSticks.Right.Y > GP3RightThumbStickDeadZone.y && newGP3State.ThumbSticks.Right.Y < GP3RightThumbStickDeadZone.y; },
        () => { return oldGP3State.ThumbSticks.Right.Y < -GP3RightThumbStickDeadZone.y && newGP3State.ThumbSticks.Right.Y > -GP3RightThumbStickDeadZone.y; },
        () => { return oldGP3State.ThumbSticks.Right.X > GP3RightThumbStickDeadZone.x && newGP3State.ThumbSticks.Right.X < GP3RightThumbStickDeadZone.x; },
        () => { return oldGP3State.ThumbSticks.Right.X < -GP3RightThumbStickDeadZone.x && newGP3State.ThumbSticks.Right.X > -GP3RightThumbStickDeadZone.x; },
        () => { return oldGP3State.ThumbSticks.Left.Y > GP3RightThumbStickDeadZone.y && newGP3State.ThumbSticks.Left.Y < GP3RightThumbStickDeadZone.y; },
        () => { return oldGP3State.ThumbSticks.Left.Y < -GP3RightThumbStickDeadZone.y && newGP3State.ThumbSticks.Left.Y > -GP3RightThumbStickDeadZone.y; },
        () => { return oldGP3State.ThumbSticks.Left.X > GP3RightThumbStickDeadZone.x && newGP3State.ThumbSticks.Left.X < GP3RightThumbStickDeadZone.x; },
        () => { return oldGP3State.ThumbSticks.Left.X < -GP3RightThumbStickDeadZone.x && newGP3State.ThumbSticks.Left.X > -GP3RightThumbStickDeadZone.x; },

        () => { return oldGP4State.Triggers.Right > GP4TriggersDeadZone.y && newGP4State.Triggers.Right <= GP4TriggersDeadZone.y; },
        () => { return oldGP4State.Triggers.Left > GP4TriggersDeadZone.x && newGP4State.Triggers.Left <= GP4TriggersDeadZone.x; },
        () => { return oldGP4State.DPad.Up == ButtonState.Pressed && newGP4State.DPad.Up == ButtonState.Released; },
        () => { return oldGP4State.DPad.Right == ButtonState.Pressed && newGP4State.DPad.Right == ButtonState.Released; },
        () => { return oldGP4State.DPad.Down == ButtonState.Pressed && newGP4State.DPad.Down == ButtonState.Released; },
        () => { return oldGP4State.DPad.Left == ButtonState.Pressed && newGP4State.DPad.Left == ButtonState.Released; },
        () => { return oldGP4State.ThumbSticks.Right.Y > GP4RightThumbStickDeadZone.y && newGP4State.ThumbSticks.Right.Y < GP4RightThumbStickDeadZone.y; },
        () => { return oldGP4State.ThumbSticks.Right.Y < -GP4RightThumbStickDeadZone.y && newGP4State.ThumbSticks.Right.Y > -GP4RightThumbStickDeadZone.y; },
        () => { return oldGP4State.ThumbSticks.Right.X > GP4RightThumbStickDeadZone.x && newGP4State.ThumbSticks.Right.X < GP4RightThumbStickDeadZone.x; },
        () => { return oldGP4State.ThumbSticks.Right.X < -GP4RightThumbStickDeadZone.x && newGP4State.ThumbSticks.Right.X > -GP4RightThumbStickDeadZone.x; },
        () => { return oldGP4State.ThumbSticks.Left.Y > GP4RightThumbStickDeadZone.y && newGP4State.ThumbSticks.Left.Y < GP4RightThumbStickDeadZone.y; },
        () => { return oldGP4State.ThumbSticks.Left.Y < -GP4RightThumbStickDeadZone.y && newGP4State.ThumbSticks.Left.Y > -GP4RightThumbStickDeadZone.y; },
        () => { return oldGP4State.ThumbSticks.Left.X > GP4RightThumbStickDeadZone.x && newGP4State.ThumbSticks.Left.X < GP4RightThumbStickDeadZone.x; },
        () => { return oldGP4State.ThumbSticks.Left.X < -GP4RightThumbStickDeadZone.x && newGP4State.ThumbSticks.Left.X > -GP4RightThumbStickDeadZone.x; },

        () => { return (oldGP1State.Triggers.Right > GP1TriggersDeadZone.y && newGP1State.Triggers.Right <= GP1TriggersDeadZone.y)
            || (oldGP2State.Triggers.Right > GP2TriggersDeadZone.y && newGP2State.Triggers.Right <= GP2TriggersDeadZone.y)
            || (oldGP3State.Triggers.Right > GP3TriggersDeadZone.y && newGP3State.Triggers.Right <= GP3TriggersDeadZone.y)
            || (oldGP4State.Triggers.Right > GP4TriggersDeadZone.y && newGP4State.Triggers.Right <= GP4TriggersDeadZone.y); },
        () => { return (oldGP1State.Triggers.Left > GP1TriggersDeadZone.x && newGP1State.Triggers.Left <= GP1TriggersDeadZone.x)
            || (oldGP2State.Triggers.Left > GP2TriggersDeadZone.x && newGP2State.Triggers.Left <= GP2TriggersDeadZone.x)
            || (oldGP3State.Triggers.Left > GP3TriggersDeadZone.x && newGP3State.Triggers.Left <= GP3TriggersDeadZone.x)
            || (oldGP4State.Triggers.Left > GP4TriggersDeadZone.x && newGP4State.Triggers.Left <= GP4TriggersDeadZone.x); },
        () => { return (oldGP1State.DPad.Up == ButtonState.Pressed && newGP1State.DPad.Up == ButtonState.Released)
            || (oldGP2State.DPad.Up == ButtonState.Pressed && newGP2State.DPad.Up == ButtonState.Released)
            || (oldGP3State.DPad.Up == ButtonState.Pressed && newGP3State.DPad.Up == ButtonState.Released)
            || (oldGP4State.DPad.Up == ButtonState.Pressed && newGP4State.DPad.Up == ButtonState.Released); },
        () => { return (oldGP1State.DPad.Right == ButtonState.Pressed && newGP1State.DPad.Right == ButtonState.Released)
            || (oldGP2State.DPad.Right == ButtonState.Pressed && newGP2State.DPad.Right == ButtonState.Released)
            || (oldGP3State.DPad.Right == ButtonState.Pressed && newGP3State.DPad.Right == ButtonState.Released)
            || (oldGP4State.DPad.Right == ButtonState.Pressed && newGP4State.DPad.Right == ButtonState.Released); },
        () => { return (oldGP1State.DPad.Down == ButtonState.Released && newGP1State.DPad.Down == ButtonState.Released)
            || (oldGP2State.DPad.Down == ButtonState.Pressed && newGP2State.DPad.Down == ButtonState.Released)
            || (oldGP3State.DPad.Down == ButtonState.Pressed && newGP3State.DPad.Down == ButtonState.Released)
            || (oldGP4State.DPad.Down == ButtonState.Pressed && newGP4State.DPad.Down == ButtonState.Released); },
        () => { return (oldGP1State.DPad.Left == ButtonState.Pressed && newGP1State.DPad.Left == ButtonState.Released)
            || (oldGP2State.DPad.Left == ButtonState.Pressed && newGP2State.DPad.Left == ButtonState.Released)
            || (oldGP3State.DPad.Left == ButtonState.Pressed && newGP3State.DPad.Left == ButtonState.Released)
            || (oldGP4State.DPad.Left == ButtonState.Pressed && newGP4State.DPad.Left == ButtonState.Released); },
        () => { return (oldGP1State.ThumbSticks.Right.Y > GP1RightThumbStickDeadZone.y && newGP1State.ThumbSticks.Right.Y <= GP1RightThumbStickDeadZone.y)
            || (oldGP2State.ThumbSticks.Right.Y > GP2RightThumbStickDeadZone.y && newGP2State.ThumbSticks.Right.Y <= GP2RightThumbStickDeadZone.y)
            || (oldGP3State.ThumbSticks.Right.Y > GP3RightThumbStickDeadZone.y && newGP3State.ThumbSticks.Right.Y <= GP3RightThumbStickDeadZone.y)
            || (oldGP4State.ThumbSticks.Right.Y > GP4RightThumbStickDeadZone.y && newGP4State.ThumbSticks.Right.Y <= GP4RightThumbStickDeadZone.y); },
        () => { return (oldGP1State.ThumbSticks.Right.Y < -GP1RightThumbStickDeadZone.y && newGP1State.ThumbSticks.Right.Y >= -GP1RightThumbStickDeadZone.y)
            || (oldGP2State.ThumbSticks.Right.Y < GP2RightThumbStickDeadZone.y && newGP2State.ThumbSticks.Right.Y >= GP2RightThumbStickDeadZone.y)
            || (oldGP3State.ThumbSticks.Right.Y < -GP3RightThumbStickDeadZone.y && newGP3State.ThumbSticks.Right.Y >= -GP3RightThumbStickDeadZone.y)
            || (oldGP4State.ThumbSticks.Right.Y < -GP4RightThumbStickDeadZone.y && newGP4State.ThumbSticks.Right.Y >= -GP4RightThumbStickDeadZone.y); },
        () => { return (oldGP1State.ThumbSticks.Right.X > GP1RightThumbStickDeadZone.x && newGP1State.ThumbSticks.Right.X <= GP1RightThumbStickDeadZone.x)
            || (oldGP2State.ThumbSticks.Right.X > GP2RightThumbStickDeadZone.x && newGP2State.ThumbSticks.Right.X <= GP2RightThumbStickDeadZone.x)
            || (oldGP3State.ThumbSticks.Right.X > GP3RightThumbStickDeadZone.x && newGP3State.ThumbSticks.Right.X <= GP3RightThumbStickDeadZone.x)
            || (oldGP4State.ThumbSticks.Right.X > GP4RightThumbStickDeadZone.x && newGP4State.ThumbSticks.Right.X <= GP4RightThumbStickDeadZone.x); },
        () => { return (oldGP1State.ThumbSticks.Right.X < -GP1RightThumbStickDeadZone.x && newGP1State.ThumbSticks.Right.X >= -GP1RightThumbStickDeadZone.x)
            || (oldGP2State.ThumbSticks.Right.X < -GP2RightThumbStickDeadZone.x && newGP2State.ThumbSticks.Right.X >= -GP2RightThumbStickDeadZone.x)
            || (oldGP3State.ThumbSticks.Right.X < -GP3RightThumbStickDeadZone.x && newGP3State.ThumbSticks.Right.X >= -GP3RightThumbStickDeadZone.x)
            || (oldGP4State.ThumbSticks.Right.X < -GP4RightThumbStickDeadZone.x && newGP4State.ThumbSticks.Right.X >= -GP4RightThumbStickDeadZone.x); },
        () => { return (oldGP1State.ThumbSticks.Left.Y > GP1RightThumbStickDeadZone.y && newGP1State.ThumbSticks.Left.Y <= GP1RightThumbStickDeadZone.y)
            || (oldGP2State.ThumbSticks.Left.Y > GP2RightThumbStickDeadZone.y && newGP2State.ThumbSticks.Left.Y <= GP2RightThumbStickDeadZone.y)
            || (oldGP3State.ThumbSticks.Left.Y > GP3RightThumbStickDeadZone.y && newGP3State.ThumbSticks.Left.Y <= GP3RightThumbStickDeadZone.y)
            || (oldGP4State.ThumbSticks.Left.Y > GP4RightThumbStickDeadZone.y && newGP4State.ThumbSticks.Left.Y <= GP4RightThumbStickDeadZone.y); },
        () => { return (oldGP1State.ThumbSticks.Left.Y < -GP1RightThumbStickDeadZone.y && newGP1State.ThumbSticks.Left.Y >= -GP1RightThumbStickDeadZone.y)
            || (oldGP2State.ThumbSticks.Left.Y < -GP2RightThumbStickDeadZone.y && newGP2State.ThumbSticks.Left.Y >= -GP2RightThumbStickDeadZone.y)
            || (oldGP3State.ThumbSticks.Left.Y < -GP3RightThumbStickDeadZone.y && newGP3State.ThumbSticks.Left.Y >= -GP3RightThumbStickDeadZone.y)
            || (oldGP4State.ThumbSticks.Left.Y < -GP4RightThumbStickDeadZone.y && newGP4State.ThumbSticks.Left.Y >= -GP4RightThumbStickDeadZone.y); },
        () => { return (oldGP1State.ThumbSticks.Left.X > GP1RightThumbStickDeadZone.x && newGP1State.ThumbSticks.Left.X <= GP1RightThumbStickDeadZone.x)
            || (oldGP2State.ThumbSticks.Left.X > GP2RightThumbStickDeadZone.x && newGP2State.ThumbSticks.Left.X <= GP2RightThumbStickDeadZone.x)
            || (oldGP3State.ThumbSticks.Left.X > GP3RightThumbStickDeadZone.x && newGP3State.ThumbSticks.Left.X <= GP3RightThumbStickDeadZone.x)
            || (oldGP4State.ThumbSticks.Left.X > GP4RightThumbStickDeadZone.x && newGP4State.ThumbSticks.Left.X <= GP4RightThumbStickDeadZone.x); },
        () => { return (oldGP1State.ThumbSticks.Left.X < -GP1RightThumbStickDeadZone.x && newGP1State.ThumbSticks.Left.X >= -GP1RightThumbStickDeadZone.x)
            || (oldGP2State.ThumbSticks.Left.X < -GP2RightThumbStickDeadZone.x && newGP2State.ThumbSticks.Left.X >= -GP2RightThumbStickDeadZone.x)
            || (oldGP3State.ThumbSticks.Left.X < -GP3RightThumbStickDeadZone.x && newGP3State.ThumbSticks.Left.X >= -GP3RightThumbStickDeadZone.x)
            || (oldGP4State.ThumbSticks.Left.X < -GP4RightThumbStickDeadZone.x && newGP4State.ThumbSticks.Left.X >= -GP4RightThumbStickDeadZone.x); }
    };

    private static readonly Func<bool>[] GetNegativeKeyCodePressedDelegate = new Func<bool>[71]
    {
        () => { return false; },
        () => { return newGP1State.Triggers.Right > GP1TriggersDeadZone.y; },
        () => { return newGP1State.Triggers.Left > GP1TriggersDeadZone.x; },
        () => { return newGP1State.DPad.Up == ButtonState.Pressed; },
        () => { return newGP1State.DPad.Right == ButtonState.Pressed; },
        () => { return newGP1State.DPad.Down == ButtonState.Pressed; },
        () => { return newGP1State.DPad.Left == ButtonState.Pressed; },
        () => { return newGP1State.ThumbSticks.Right.Y > GP1LeftThumbStickDeadZone.y; },
        () => { return newGP1State.ThumbSticks.Right.Y < -GP1LeftThumbStickDeadZone.y; },
        () => { return newGP1State.ThumbSticks.Right.X > GP1LeftThumbStickDeadZone.x; },
        () => { return newGP1State.ThumbSticks.Right.X < -GP1LeftThumbStickDeadZone.x; },
        () => { return newGP1State.ThumbSticks.Left.Y > GP1LeftThumbStickDeadZone.y; },
        () => { return newGP1State.ThumbSticks.Left.Y < -GP1LeftThumbStickDeadZone.y; },
        () => { return newGP1State.ThumbSticks.Left.X > GP1LeftThumbStickDeadZone.x; },
        () => { return newGP1State.ThumbSticks.Left.X < -GP1LeftThumbStickDeadZone.x; },

        () => { return newGP2State.Triggers.Right > GP2TriggersDeadZone.y; },
        () => { return newGP2State.Triggers.Left > GP2TriggersDeadZone.x; },
        () => { return newGP2State.DPad.Up == ButtonState.Pressed; },
        () => { return newGP2State.DPad.Right == ButtonState.Pressed; },
        () => { return newGP2State.DPad.Down == ButtonState.Pressed; },
        () => { return newGP2State.DPad.Left == ButtonState.Pressed; },
        () => { return newGP2State.ThumbSticks.Right.Y > GP2LeftThumbStickDeadZone.y; },
        () => { return newGP2State.ThumbSticks.Right.Y < -GP2LeftThumbStickDeadZone.y; },
        () => { return newGP2State.ThumbSticks.Right.X > GP2LeftThumbStickDeadZone.x; },
        () => { return newGP2State.ThumbSticks.Right.X < -GP2LeftThumbStickDeadZone.x; },
        () => { return newGP2State.ThumbSticks.Left.Y > GP2LeftThumbStickDeadZone.y; },
        () => { return newGP2State.ThumbSticks.Left.Y < -GP2LeftThumbStickDeadZone.y; },
        () => { return newGP2State.ThumbSticks.Left.X > GP2LeftThumbStickDeadZone.x; },
        () => { return newGP2State.ThumbSticks.Left.X < -GP2LeftThumbStickDeadZone.x; },

        () => { return newGP3State.Triggers.Right > GP3TriggersDeadZone.y; },
        () => { return newGP3State.Triggers.Left > GP3TriggersDeadZone.x; },
        () => { return newGP3State.DPad.Up == ButtonState.Pressed; },
        () => { return newGP3State.DPad.Right == ButtonState.Pressed; },
        () => { return newGP3State.DPad.Down == ButtonState.Pressed; },
        () => { return newGP3State.DPad.Left == ButtonState.Pressed; },
        () => { return newGP3State.ThumbSticks.Right.Y > GP3LeftThumbStickDeadZone.y; },
        () => { return newGP3State.ThumbSticks.Right.Y < -GP3LeftThumbStickDeadZone.y; },
        () => { return newGP3State.ThumbSticks.Right.X > GP3LeftThumbStickDeadZone.x; },
        () => { return newGP3State.ThumbSticks.Right.X < -GP3LeftThumbStickDeadZone.x; },
        () => { return newGP3State.ThumbSticks.Left.Y > GP3LeftThumbStickDeadZone.y; },
        () => { return newGP3State.ThumbSticks.Left.Y < -GP3LeftThumbStickDeadZone.y; },
        () => { return newGP3State.ThumbSticks.Left.X > GP3LeftThumbStickDeadZone.x; },
        () => { return newGP3State.ThumbSticks.Left.X < -GP3LeftThumbStickDeadZone.x; },

        () => { return newGP4State.Triggers.Right > GP4TriggersDeadZone.y; },
        () => { return newGP4State.Triggers.Left > GP4TriggersDeadZone.x; },
        () => { return newGP4State.DPad.Up == ButtonState.Pressed; },
        () => { return newGP4State.DPad.Right == ButtonState.Pressed; },
        () => { return newGP4State.DPad.Down == ButtonState.Pressed; },
        () => { return newGP4State.DPad.Left == ButtonState.Pressed; },
        () => { return newGP4State.ThumbSticks.Right.Y > GP4LeftThumbStickDeadZone.y; },
        () => { return newGP4State.ThumbSticks.Right.Y < -GP4LeftThumbStickDeadZone.y; },
        () => { return newGP4State.ThumbSticks.Right.X > GP4LeftThumbStickDeadZone.x; },
        () => { return newGP4State.ThumbSticks.Right.X < -GP4LeftThumbStickDeadZone.x; },
        () => { return newGP4State.ThumbSticks.Left.Y > GP4LeftThumbStickDeadZone.y; },
        () => { return newGP4State.ThumbSticks.Left.Y < -GP4LeftThumbStickDeadZone.y; },
        () => { return newGP4State.ThumbSticks.Left.X > GP4LeftThumbStickDeadZone.x; },
        () => { return newGP4State.ThumbSticks.Left.X < -GP4LeftThumbStickDeadZone.x; },

        () => { return (newGP1State.Triggers.Right > GP1TriggersDeadZone.y)
            || (newGP2State.Triggers.Right > GP2TriggersDeadZone.y)
            || (newGP3State.Triggers.Right > GP3TriggersDeadZone.y)
            || (newGP4State.Triggers.Right > GP4TriggersDeadZone.y); },
        () => { return (newGP1State.Triggers.Left > GP1TriggersDeadZone.x)
            || (newGP2State.Triggers.Left > GP2TriggersDeadZone.x)
            || (newGP3State.Triggers.Left > GP3TriggersDeadZone.x)
            || (newGP4State.Triggers.Left > GP4TriggersDeadZone.x); },
        () => { return (newGP1State.DPad.Up == ButtonState.Pressed)
            || (newGP2State.DPad.Up == ButtonState.Pressed)
            || (newGP3State.DPad.Up == ButtonState.Pressed)
            || (newGP4State.DPad.Up == ButtonState.Pressed); },
        () => { return (newGP1State.DPad.Right == ButtonState.Pressed)
            || (newGP2State.DPad.Right == ButtonState.Pressed)
            || (newGP3State.DPad.Right == ButtonState.Pressed)
            || (newGP4State.DPad.Right == ButtonState.Pressed); },
        () => { return (newGP1State.DPad.Down == ButtonState.Pressed)
            || (newGP2State.DPad.Down == ButtonState.Pressed)
            || (newGP3State.DPad.Down == ButtonState.Pressed)
            || (newGP4State.DPad.Down == ButtonState.Pressed); },
        () => { return (newGP1State.DPad.Left == ButtonState.Pressed)
            || (newGP2State.DPad.Left == ButtonState.Pressed)
            || (newGP3State.DPad.Left == ButtonState.Pressed)
            || (newGP4State.DPad.Left == ButtonState.Pressed); },
        () => { return (newGP1State.ThumbSticks.Right.Y > GP1RightThumbStickDeadZone.y)
            || (newGP2State.ThumbSticks.Right.Y > GP2RightThumbStickDeadZone.y)
            || (newGP3State.ThumbSticks.Right.Y > GP3RightThumbStickDeadZone.y)
            || (newGP4State.ThumbSticks.Right.Y > GP4RightThumbStickDeadZone.y); },
        () => { return (newGP1State.ThumbSticks.Right.Y < -GP1RightThumbStickDeadZone.y)
            || (newGP2State.ThumbSticks.Right.Y < GP2RightThumbStickDeadZone.y)
            || (newGP3State.ThumbSticks.Right.Y < -GP3RightThumbStickDeadZone.y)
            || (newGP4State.ThumbSticks.Right.Y < -GP4RightThumbStickDeadZone.y); },
        () => { return (newGP1State.ThumbSticks.Right.X > GP1RightThumbStickDeadZone.x)
            || (newGP2State.ThumbSticks.Right.X > GP2RightThumbStickDeadZone.x)
            || (newGP3State.ThumbSticks.Right.X > GP3RightThumbStickDeadZone.x)
            || (newGP4State.ThumbSticks.Right.X > GP4RightThumbStickDeadZone.x); },
        () => { return (newGP1State.ThumbSticks.Right.X < -GP1RightThumbStickDeadZone.x)
            || (newGP2State.ThumbSticks.Right.X < -GP2RightThumbStickDeadZone.x)
            || (newGP3State.ThumbSticks.Right.X < -GP3RightThumbStickDeadZone.x)
            || (newGP4State.ThumbSticks.Right.X < -GP4RightThumbStickDeadZone.x); },
        () => { return (newGP1State.ThumbSticks.Left.Y > GP1RightThumbStickDeadZone.y)
            || (newGP2State.ThumbSticks.Left.Y > GP2RightThumbStickDeadZone.y)
            || (newGP3State.ThumbSticks.Left.Y > GP3RightThumbStickDeadZone.y)
            || (newGP4State.ThumbSticks.Left.Y > GP4RightThumbStickDeadZone.y); },
        () => { return (newGP1State.ThumbSticks.Left.Y < -GP1RightThumbStickDeadZone.y)
            || (newGP2State.ThumbSticks.Left.Y < -GP2RightThumbStickDeadZone.y)
            || (newGP3State.ThumbSticks.Left.Y < -GP3RightThumbStickDeadZone.y)
            || (newGP4State.ThumbSticks.Left.Y < -GP4RightThumbStickDeadZone.y); },
        () => { return (newGP1State.ThumbSticks.Left.X > GP1RightThumbStickDeadZone.x)
            || (newGP2State.ThumbSticks.Left.X > GP2RightThumbStickDeadZone.x)
            || (newGP3State.ThumbSticks.Left.X > GP3RightThumbStickDeadZone.x)
            || (newGP4State.ThumbSticks.Left.X > GP4RightThumbStickDeadZone.x); },
        () => { return (newGP1State.ThumbSticks.Left.X < -GP1RightThumbStickDeadZone.x)
            || (newGP2State.ThumbSticks.Left.X < -GP2RightThumbStickDeadZone.x)
            || (newGP3State.ThumbSticks.Left.X < -GP3RightThumbStickDeadZone.x)
            || (newGP4State.ThumbSticks.Left.X < -GP4RightThumbStickDeadZone.x); }
    };

    #endregion

    #endregion

    #region Class InputData

    [Serializable]
    private class InputData
    {
        public List<string> actions = new List<string>();
        public List<int> keys = new List<int>();

        public InputData() { }

        public InputData(List<string> actions, List<int> keys)
        {
            this.actions = actions;
            this.keys = keys;
        }

        public void AddAction(string action, in int key)
        {
            if (actions.Contains(action))
                ReplaceAction(action, key);
            else
            {
                actions.Add(action);
                keys.Add(key);
            }
        }

        public bool RemoveAction(string action)
        {
            if(actions.Contains(action))
            {
                keys.RemoveAt(actions.FindIndex((string s) => s == action));
                actions.Remove(action);
                return true;
            }
            return false;
        }

        public bool ReplaceAction(string action, in int key)
        {
            if (actions.Contains(action))
            {
                keys[actions.FindIndex((string s) => s == action)] = key;
                return true;
            }
            return false;
        }

        public bool Contain(string action) => actions.Contains(action);

        public int GetKey(string action)
        {
            return keys[actions.FindIndex((string s) => s == action)];
        }

        public InputData Clone() => new InputData(actions.Clone(), keys.Clone()); 
    }

    #endregion

    #region SetVibration

    public static void SetVibration(float intensity, ControllerType gamepadIndex = ControllerType.GamepadAll)
    {
        SetVibration(intensity, intensity, gamepadIndex);
    }

    public static void SetVibration(float rightIntensity = 1f, float leftIntensity = 1f, ControllerType gamepadIndex = ControllerType.GamepadAll)
    {
        //Handheld.Vibrate();//version unity eco plus

        if(gamepadIndex == ControllerType.Keyboard)
        {
            Debug.LogWarning("Cannot vibrate the keyboard!");
            return;
        }
        if(gamepadIndex == ControllerType.All || gamepadIndex == ControllerType.GamepadAll)
        {
            if(newGP1State.IsConnected)
                GamePad.SetVibration(XInputDotNetPure.PlayerIndex.One, rightIntensity, leftIntensity);
            if (newGP2State.IsConnected)
                GamePad.SetVibration(XInputDotNetPure.PlayerIndex.Two, rightIntensity, leftIntensity);
            if (newGP3State.IsConnected)
                GamePad.SetVibration(XInputDotNetPure.PlayerIndex.Three, rightIntensity, leftIntensity);
            if (newGP4State.IsConnected)
                GamePad.SetVibration(XInputDotNetPure.PlayerIndex.Four, rightIntensity, leftIntensity);
            return;
        }
        GamePad.SetVibration((XInputDotNetPure.PlayerIndex)((int)gamepadIndex - 1), rightIntensity, leftIntensity);
    }

    #endregion

    #region GamePad only

    #region SetStickPosition

    private static void SetOldGamepadSticksPositions()
    {
        oldGP1Triggers = newGP1Triggers;
        oldGP2Triggers = newGP2Triggers;
        oldGP3Triggers = newGP3Triggers;
        oldGP4Triggers = newGP4Triggers;
        oldGP1RightStickPosition = newGP1RightStickPosition;
        oldGP1LeftStickPosition = newGP1LeftStickPosition;
        oldGP2RightStickPosition = newGP2RightStickPosition;
        oldGP2LeftStickPosition = newGP2LeftStickPosition;
        oldGP3RightStickPosition = newGP3RightStickPosition;
        oldGP3LeftStickPosition = newGP3LeftStickPosition;
        oldGP4RightStickPosition = newGP4RightStickPosition;
        oldGP4LeftStickPosition = newGP4LeftStickPosition;
    }

    private static void SetNewGamepadStickPositions()
    {
        //ThumbStick : si vraiPos.x/y€[-deadZone.x/y, deadZone.x/y] => pos.x/y = 0, vraiPos.x/y€[-1, 1-deadZone.x/y] U [1 - deadZone.x/y, 1] => pos.x/y = (vraiPos.x/y).Sign() * 1, sinon pos.x/y = vraiPos.x/y
        float x = Mathf.Abs(newGP1State.ThumbSticks.Right.X) <= GP1RightThumbStickDeadZone.x ? 0f : (Mathf.Abs(newGP1State.ThumbSticks.Right.X) >= (1f - GP1RightThumbStickDeadZone.x) ? newGP1State.ThumbSticks.Right.X.Sign() : newGP1State.ThumbSticks.Right.X);
        float y = Mathf.Abs(newGP1State.ThumbSticks.Right.Y) <= GP1RightThumbStickDeadZone.x ? 0f : (Mathf.Abs(newGP1State.ThumbSticks.Right.Y) >= (1f - GP1RightThumbStickDeadZone.y) ? newGP1State.ThumbSticks.Right.Y.Sign() : newGP1State.ThumbSticks.Right.Y);
        newGP1RightStickPosition = new Vector2(x, y);
        x = Mathf.Abs(newGP1State.ThumbSticks.Left.X) <= GP1LeftThumbStickDeadZone.x ? 0f : (Mathf.Abs(newGP1State.ThumbSticks.Left.X) >= (1f - GP1LeftThumbStickDeadZone.x) ? newGP1State.ThumbSticks.Left.X.Sign() : newGP1State.ThumbSticks.Left.X);
        y = Mathf.Abs(newGP1State.ThumbSticks.Left.Y) <= GP1LeftThumbStickDeadZone.x ? 0f : (Mathf.Abs(newGP1State.ThumbSticks.Left.Y) >= (1f - GP1LeftThumbStickDeadZone.y) ? newGP1State.ThumbSticks.Left.Y.Sign() : newGP1State.ThumbSticks.Left.Y);
        newGP1LeftStickPosition = new Vector2(x, y);
        x = newGP1State.Triggers.Left <= GP1TriggersDeadZone.x ? 0f : (newGP1State.Triggers.Left >= 1f - GP1TriggersDeadZone.x ? 1f : newGP1State.Triggers.Left);
        y = newGP1State.Triggers.Right <= GP1TriggersDeadZone.y ? 0f : (newGP1State.Triggers.Right >= 1f - GP1TriggersDeadZone.y ? 1f : newGP1State.Triggers.Right);
        newGP1Triggers = new Vector2(x, y);


        x = Mathf.Abs(newGP2State.ThumbSticks.Right.X) <= GP2RightThumbStickDeadZone.x ? 0f : (Mathf.Abs(newGP2State.ThumbSticks.Right.X) >= (1f - GP2RightThumbStickDeadZone.x) ? newGP2State.ThumbSticks.Right.X.Sign() : newGP2State.ThumbSticks.Right.X);
        y = Mathf.Abs(newGP2State.ThumbSticks.Right.Y) <= GP2RightThumbStickDeadZone.x ? 0f : (Mathf.Abs(newGP2State.ThumbSticks.Right.Y) >= (1f - GP2RightThumbStickDeadZone.y) ? newGP2State.ThumbSticks.Right.Y.Sign() : newGP2State.ThumbSticks.Right.Y);
        newGP2RightStickPosition = new Vector2(x, y);
        x = Mathf.Abs(newGP2State.ThumbSticks.Left.X) <= GP2LeftThumbStickDeadZone.x ? 0f : (Mathf.Abs(newGP2State.ThumbSticks.Left.X) >= (1f - GP2LeftThumbStickDeadZone.x) ? newGP2State.ThumbSticks.Left.X.Sign() : newGP2State.ThumbSticks.Left.X);
        y = Mathf.Abs(newGP2State.ThumbSticks.Left.Y) <= GP2LeftThumbStickDeadZone.x ? 0f : (Mathf.Abs(newGP2State.ThumbSticks.Left.Y) >= (1f - GP2LeftThumbStickDeadZone.y) ? newGP2State.ThumbSticks.Left.Y.Sign() : newGP2State.ThumbSticks.Left.Y);
        newGP2LeftStickPosition = new Vector2(x, y);
        x = newGP2State.Triggers.Left <= GP2TriggersDeadZone.x ? 0f : (newGP2State.Triggers.Left >= 1f - GP2TriggersDeadZone.x ? 1f : newGP2State.Triggers.Left);
        y = newGP2State.Triggers.Right <= GP2TriggersDeadZone.y ? 0f : (newGP2State.Triggers.Right >= 1f - GP2TriggersDeadZone.y ? 1f : newGP2State.Triggers.Right);
        newGP2Triggers = new Vector2(x, y);

        x = Mathf.Abs(newGP3State.ThumbSticks.Right.X) <= GP3RightThumbStickDeadZone.x ? 0f : (Mathf.Abs(newGP3State.ThumbSticks.Right.X) >= (1f - GP3RightThumbStickDeadZone.x) ? newGP3State.ThumbSticks.Right.X.Sign() : newGP3State.ThumbSticks.Right.X);
        y = Mathf.Abs(newGP3State.ThumbSticks.Right.Y) <= GP3RightThumbStickDeadZone.x ? 0f : (Mathf.Abs(newGP3State.ThumbSticks.Right.Y) >= (1f - GP3RightThumbStickDeadZone.y) ? newGP3State.ThumbSticks.Right.Y.Sign() : newGP3State.ThumbSticks.Right.Y);
        newGP3RightStickPosition = new Vector2(x, y);
        x = Mathf.Abs(newGP3State.ThumbSticks.Left.X) <= GP3LeftThumbStickDeadZone.x ? 0f : (Mathf.Abs(newGP3State.ThumbSticks.Left.X) >= (1f - GP3LeftThumbStickDeadZone.x) ? newGP3State.ThumbSticks.Left.X.Sign() : newGP3State.ThumbSticks.Left.X);
        y = Mathf.Abs(newGP3State.ThumbSticks.Left.Y) <= GP3LeftThumbStickDeadZone.x ? 0f : (Mathf.Abs(newGP3State.ThumbSticks.Left.Y) >= (1f - GP3LeftThumbStickDeadZone.y) ? newGP3State.ThumbSticks.Left.Y.Sign() : newGP3State.ThumbSticks.Left.Y);
        newGP3LeftStickPosition = new Vector2(x, y);
        x = newGP3State.Triggers.Left <= GP3TriggersDeadZone.x ? 0f : (newGP3State.Triggers.Left >= 1f - GP3TriggersDeadZone.x ? 1f : newGP3State.Triggers.Left);
        y = newGP3State.Triggers.Right <= GP3TriggersDeadZone.y ? 0f : (newGP3State.Triggers.Right >= 1f - GP3TriggersDeadZone.y ? 1f : newGP3State.Triggers.Right);
        newGP3Triggers = new Vector2(x, y);

        x = Mathf.Abs(newGP4State.ThumbSticks.Right.X) <= GP4RightThumbStickDeadZone.x ? 0f : (Mathf.Abs(newGP4State.ThumbSticks.Right.X) >= (1f - GP4RightThumbStickDeadZone.x) ? newGP4State.ThumbSticks.Right.X.Sign() : newGP4State.ThumbSticks.Right.X);
        y = Mathf.Abs(newGP4State.ThumbSticks.Right.Y) <= GP4RightThumbStickDeadZone.x ? 0f : (Mathf.Abs(newGP4State.ThumbSticks.Right.Y) >= (1f - GP4RightThumbStickDeadZone.y) ? newGP4State.ThumbSticks.Right.Y.Sign() : newGP4State.ThumbSticks.Right.Y);
        newGP4RightStickPosition = new Vector2(x, y);
        x = Mathf.Abs(newGP4State.ThumbSticks.Left.X) <= GP4LeftThumbStickDeadZone.x ? 0f : (Mathf.Abs(newGP4State.ThumbSticks.Left.X) >= (1f - GP4LeftThumbStickDeadZone.x) ? newGP4State.ThumbSticks.Left.X.Sign() : newGP4State.ThumbSticks.Left.X);
        y = Mathf.Abs(newGP4State.ThumbSticks.Left.Y) <= GP4LeftThumbStickDeadZone.x ? 0f : (Mathf.Abs(newGP4State.ThumbSticks.Left.Y) >= (1f - GP4LeftThumbStickDeadZone.y) ? newGP4State.ThumbSticks.Left.Y.Sign() : newGP4State.ThumbSticks.Left.Y);
        newGP4LeftStickPosition = new Vector2(x, y);
        x = newGP4State.Triggers.Left <= GP4TriggersDeadZone.x ? 0f : (newGP4State.Triggers.Left >= 1f - GP4TriggersDeadZone.x ? 1f : newGP4State.Triggers.Left);
        y = newGP4State.Triggers.Right <= GP4TriggersDeadZone.y ? 0f : (newGP4State.Triggers.Right >= 1f - GP4TriggersDeadZone.y ? 1f : newGP4State.Triggers.Right);
        newGP4Triggers = new Vector2(x, y);
    }

    #endregion

    public static Vector2 GetGamepadStickPosition(in ControllerType gamepadIndex, in GamepadStick gamepadStick)
    {
        switch (gamepadIndex)
        {
            case ControllerType.Keyboard:
                Debug.LogWarning("Can't return the stick position of a keyboard!");
                return Vector2.zero;
            case ControllerType.Gamepad1:
                return gamepadStick == GamepadStick.right ? newGP1RightStickPosition : newGP1LeftStickPosition;
            case ControllerType.Gamepad2:
                return gamepadStick == GamepadStick.right ? newGP2RightStickPosition : newGP2LeftStickPosition;
            case ControllerType.Gamepad3:
                return gamepadStick == GamepadStick.right ? newGP3RightStickPosition : newGP3LeftStickPosition;
            case ControllerType.Gamepad4:
                return gamepadStick == GamepadStick.right ? newGP4RightStickPosition : newGP4LeftStickPosition;
            case ControllerType.GamepadAll:
                if (newGP1State.IsConnected)
                    return gamepadStick == GamepadStick.right ? newGP1RightStickPosition : newGP1LeftStickPosition;
                if (newGP2State.IsConnected)
                    return gamepadStick == GamepadStick.right ? newGP2RightStickPosition : newGP2LeftStickPosition;
                if (newGP3State.IsConnected)
                    return gamepadStick == GamepadStick.right ? newGP3RightStickPosition : newGP3LeftStickPosition;
                if (newGP4State.IsConnected)
                    return gamepadStick == GamepadStick.right ? newGP4RightStickPosition : newGP4LeftStickPosition;
                Debug.LogWarning("No GamePad is connected");
                return Vector2.zero;
            case ControllerType.All:
                if (newGP1State.IsConnected)
                    return gamepadStick == GamepadStick.right ? newGP1RightStickPosition : newGP1LeftStickPosition;
                if (newGP2State.IsConnected)
                    return gamepadStick == GamepadStick.right ? newGP2RightStickPosition : newGP2LeftStickPosition;
                if (newGP3State.IsConnected)
                    return gamepadStick == GamepadStick.right ? newGP3RightStickPosition : newGP3LeftStickPosition;
                if (newGP4State.IsConnected)
                    return gamepadStick == GamepadStick.right ? newGP4RightStickPosition : newGP4LeftStickPosition;
                Debug.LogWarning("No GamePad is connected");
                return Vector2.zero;
            default:
                return Vector2.zero;
        }
    }

    #region GetGamepadSTickUp/Down/Right/Left

    /// <summary>
    /// 
    /// </summary>
    /// <param name="controllerType"></param>
    /// <param name="gamepadStick"></param>
    /// <returns>true the frame when the gamepad stick reach the top (y value became 1f)</returns>
    public static bool GetGamepadStickUp(in ControllerType controllerType, in GamepadStick gamepadStick)
    {
        switch (controllerType)
        {
            case ControllerType.Keyboard:
                Debug.LogWarning("A keyboard does'nt have stick!");
                return false;
            case ControllerType.Gamepad1:
                return gamepadStick == GamepadStick.right ? oldGP1RightStickPosition.y < GP1RightThumbStickDeadZone.y && newGP1RightStickPosition.y >= GP1RightThumbStickDeadZone.y :
                    oldGP1LeftStickPosition.y < GP1LeftThumbStickDeadZone.y && newGP1LeftStickPosition.y >= GP1LeftThumbStickDeadZone.y;
            case ControllerType.Gamepad2:
                return gamepadStick == GamepadStick.right ? oldGP2RightStickPosition.y < GP2RightThumbStickDeadZone.y && newGP2RightStickPosition.y >= GP2RightThumbStickDeadZone.y :
                    oldGP2LeftStickPosition.y < GP2LeftThumbStickDeadZone.y && newGP2LeftStickPosition.y >= GP2LeftThumbStickDeadZone.y;
            case ControllerType.Gamepad3:
                return gamepadStick == GamepadStick.right ? oldGP3RightStickPosition.y < GP3RightThumbStickDeadZone.y && newGP3RightStickPosition.y >= GP3RightThumbStickDeadZone.y :
                    oldGP3LeftStickPosition.y < GP3LeftThumbStickDeadZone.y && newGP3LeftStickPosition.y >= GP3LeftThumbStickDeadZone.y;
            case ControllerType.Gamepad4:
                return gamepadStick == GamepadStick.right ? oldGP4RightStickPosition.y < GP4RightThumbStickDeadZone.y && newGP1RightStickPosition.y >= GP1RightThumbStickDeadZone.y :
                    oldGP1LeftStickPosition.y < GP1LeftThumbStickDeadZone.y && newGP1LeftStickPosition.y >= GP1LeftThumbStickDeadZone.y;
            case ControllerType.GamepadAll:
                if(GamePadIsConnected(ControllerType.Gamepad1))
                    return gamepadStick == GamepadStick.right ? oldGP1RightStickPosition.y < GP1RightThumbStickDeadZone.y && newGP1RightStickPosition.y >= GP1RightThumbStickDeadZone.y :
                    oldGP1LeftStickPosition.y < GP1LeftThumbStickDeadZone.y && newGP1LeftStickPosition.y >= GP1LeftThumbStickDeadZone.y;
                if(GamePadIsConnected(ControllerType.Gamepad2))
                    return gamepadStick == GamepadStick.right ? oldGP2RightStickPosition.y < GP2RightThumbStickDeadZone.y && newGP2RightStickPosition.y >= GP2RightThumbStickDeadZone.y :
                    oldGP2LeftStickPosition.y < GP2LeftThumbStickDeadZone.y && newGP2LeftStickPosition.y >= GP2LeftThumbStickDeadZone.y;
                if(GamePadIsConnected(ControllerType.Gamepad3))
                    return gamepadStick == GamepadStick.right ? oldGP3RightStickPosition.y < GP3RightThumbStickDeadZone.y && newGP3RightStickPosition.y >= GP3RightThumbStickDeadZone.y :
                    oldGP3LeftStickPosition.y < GP3LeftThumbStickDeadZone.y && newGP3LeftStickPosition.y >= GP3LeftThumbStickDeadZone.y;
                if(GamePadIsConnected(ControllerType.Gamepad4))
                    return gamepadStick == GamepadStick.right ? oldGP4RightStickPosition.y < GP4RightThumbStickDeadZone.y && newGP1RightStickPosition.y >= GP1RightThumbStickDeadZone.y :
                    oldGP1LeftStickPosition.y < GP1LeftThumbStickDeadZone.y && newGP1LeftStickPosition.y >= GP1LeftThumbStickDeadZone.y;
                Debug.LogWarning("No Gamepad is connected!");
                return false;
            case ControllerType.All:
                if (GamePadIsConnected(ControllerType.Gamepad1))
                    return gamepadStick == GamepadStick.right ? oldGP1RightStickPosition.y < GP1RightThumbStickDeadZone.y && newGP1RightStickPosition.y >= GP1RightThumbStickDeadZone.y :
                    oldGP1LeftStickPosition.y < GP1LeftThumbStickDeadZone.y && newGP1LeftStickPosition.y >= GP1LeftThumbStickDeadZone.y;
                if (GamePadIsConnected(ControllerType.Gamepad2))
                    return gamepadStick == GamepadStick.right ? oldGP2RightStickPosition.y < GP2RightThumbStickDeadZone.y && newGP2RightStickPosition.y >= GP2RightThumbStickDeadZone.y :
                    oldGP2LeftStickPosition.y < GP2LeftThumbStickDeadZone.y && newGP2LeftStickPosition.y >= GP2LeftThumbStickDeadZone.y;
                if (GamePadIsConnected(ControllerType.Gamepad3))
                    return gamepadStick == GamepadStick.right ? oldGP3RightStickPosition.y < GP3RightThumbStickDeadZone.y && newGP3RightStickPosition.y >= GP3RightThumbStickDeadZone.y :
                    oldGP3LeftStickPosition.y < GP3LeftThumbStickDeadZone.y && newGP3LeftStickPosition.y >= GP3LeftThumbStickDeadZone.y;
                if (GamePadIsConnected(ControllerType.Gamepad4))
                    return gamepadStick == GamepadStick.right ? oldGP4RightStickPosition.y < GP4RightThumbStickDeadZone.y && newGP1RightStickPosition.y >= GP1RightThumbStickDeadZone.y :
                    oldGP1LeftStickPosition.y < GP1LeftThumbStickDeadZone.y && newGP1LeftStickPosition.y >= GP1LeftThumbStickDeadZone.y;
                Debug.LogWarning("No Gamepad is connected!");
                return false;
            default:
                return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="controllerType"></param>
    /// <param name="gamepadStick"></param>
    /// <returns>true the frame when the gamepad stick reach the bottom (y value became -1f)</returns>
    public static bool GetGamepadStickDown(in ControllerType controllerType, in GamepadStick gamepadStick)
    {
        switch (controllerType)
        {
            case ControllerType.Keyboard:
                Debug.LogWarning("A keyboard does'nt have stick!");
                return false;
            case ControllerType.Gamepad1:
                return gamepadStick == GamepadStick.right ? oldGP1RightStickPosition.y > -GP1RightThumbStickDeadZone.y && newGP1RightStickPosition.y <= -GP1RightThumbStickDeadZone.y :
                    oldGP1LeftStickPosition.y > -GP1LeftThumbStickDeadZone.y && newGP1LeftStickPosition.y <= -GP1LeftThumbStickDeadZone.y;
            case ControllerType.Gamepad2:
                return gamepadStick == GamepadStick.right ? oldGP2RightStickPosition.y > -GP2RightThumbStickDeadZone.y && newGP2RightStickPosition.y <= -GP2RightThumbStickDeadZone.y :
                    oldGP2LeftStickPosition.y > -GP2LeftThumbStickDeadZone.y && newGP2LeftStickPosition.y <= -GP2LeftThumbStickDeadZone.y;
            case ControllerType.Gamepad3:
                return gamepadStick == GamepadStick.right ? oldGP3RightStickPosition.y > -GP3RightThumbStickDeadZone.y && newGP3RightStickPosition.y <= -GP3RightThumbStickDeadZone.y :
                    oldGP3LeftStickPosition.y > -GP3LeftThumbStickDeadZone.y && newGP3LeftStickPosition.y <= -GP3LeftThumbStickDeadZone.y;
            case ControllerType.Gamepad4:
                return gamepadStick == GamepadStick.right ? oldGP4RightStickPosition.y > -GP4RightThumbStickDeadZone.y && newGP4RightStickPosition.y <= -GP4RightThumbStickDeadZone.y :
                    oldGP4LeftStickPosition.y > -GP4LeftThumbStickDeadZone.y && newGP4LeftStickPosition.y <= -GP4LeftThumbStickDeadZone.y;
            case ControllerType.GamepadAll:
                if (GamePadIsConnected(ControllerType.Gamepad1))
                    return gamepadStick == GamepadStick.right ? oldGP1RightStickPosition.y > -GP1RightThumbStickDeadZone.y && newGP1RightStickPosition.y <= -GP1RightThumbStickDeadZone.y :
                        oldGP1LeftStickPosition.y > -GP1LeftThumbStickDeadZone.y && newGP1LeftStickPosition.y <= -GP1LeftThumbStickDeadZone.y;
                if (GamePadIsConnected(ControllerType.Gamepad2))
                    return gamepadStick == GamepadStick.right ? oldGP2RightStickPosition.y > -GP2RightThumbStickDeadZone.y && newGP2RightStickPosition.y <= -GP2RightThumbStickDeadZone.y :
                        oldGP2LeftStickPosition.y > -GP2LeftThumbStickDeadZone.y && newGP2LeftStickPosition.y <= -GP2LeftThumbStickDeadZone.y;
                if (GamePadIsConnected(ControllerType.Gamepad3))
                    return gamepadStick == GamepadStick.right ? oldGP3RightStickPosition.y > -GP3RightThumbStickDeadZone.y && newGP3RightStickPosition.y <= -GP3RightThumbStickDeadZone.y :
                        oldGP3LeftStickPosition.y > -GP3LeftThumbStickDeadZone.y && newGP3LeftStickPosition.y <= -GP3LeftThumbStickDeadZone.y;
                if (GamePadIsConnected(ControllerType.Gamepad4))
                    return gamepadStick == GamepadStick.right ? oldGP4RightStickPosition.y > -GP4RightThumbStickDeadZone.y && newGP4RightStickPosition.y <= -GP4RightThumbStickDeadZone.y :
                        oldGP4LeftStickPosition.y > -GP4LeftThumbStickDeadZone.y && newGP4LeftStickPosition.y <= -GP4LeftThumbStickDeadZone.y;
                Debug.LogWarning("No Gamepad is connected!");
                return false;
            case ControllerType.All:
                if (GamePadIsConnected(ControllerType.Gamepad1))
                    return gamepadStick == GamepadStick.right ? oldGP1RightStickPosition.y > -GP1RightThumbStickDeadZone.y && newGP1RightStickPosition.y <= -GP1RightThumbStickDeadZone.y :
                        oldGP1LeftStickPosition.y > -GP1LeftThumbStickDeadZone.y && newGP1LeftStickPosition.y <= -GP1LeftThumbStickDeadZone.y;
                if (GamePadIsConnected(ControllerType.Gamepad2))
                    return gamepadStick == GamepadStick.right ? oldGP2RightStickPosition.y > -GP2RightThumbStickDeadZone.y && newGP2RightStickPosition.y <= -GP2RightThumbStickDeadZone.y :
                        oldGP2LeftStickPosition.y > -GP2LeftThumbStickDeadZone.y && newGP2LeftStickPosition.y <= -GP2LeftThumbStickDeadZone.y;
                if (GamePadIsConnected(ControllerType.Gamepad3))
                    return gamepadStick == GamepadStick.right ? oldGP3RightStickPosition.y > -GP3RightThumbStickDeadZone.y && newGP3RightStickPosition.y <= -GP3RightThumbStickDeadZone.y :
                        oldGP3LeftStickPosition.y > -GP3LeftThumbStickDeadZone.y && newGP3LeftStickPosition.y <= -GP3LeftThumbStickDeadZone.y;
                if (GamePadIsConnected(ControllerType.Gamepad4))
                    return gamepadStick == GamepadStick.right ? oldGP4RightStickPosition.y > -GP4RightThumbStickDeadZone.y && newGP4RightStickPosition.y <= -GP4RightThumbStickDeadZone.y :
                        oldGP4LeftStickPosition.y > -GP4LeftThumbStickDeadZone.y && newGP4LeftStickPosition.y <= -GP4LeftThumbStickDeadZone.y;
                Debug.LogWarning("No Gamepad is connected!");
                return false;
            default:
                return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="controllerType"></param>
    /// <param name="gamepadStick"></param>
    /// <returns>true the frame when the gamepad stick reach the right (x value became 1f)</returns>
    public static bool GetGamepadStickRight(in ControllerType controllerType, in GamepadStick gamepadStick)
    {
        switch (controllerType)
        {
            case ControllerType.Keyboard:
                Debug.LogWarning("A keyboard does'nt have stick!");
                return false;
            case ControllerType.Gamepad1:
                return gamepadStick == GamepadStick.right ? oldGP1RightStickPosition.x < GP1RightThumbStickDeadZone.y && newGP1RightStickPosition.x >= GP1RightThumbStickDeadZone.y :
                    oldGP1LeftStickPosition.x < GP1LeftThumbStickDeadZone.y && newGP1LeftStickPosition.x >= GP1LeftThumbStickDeadZone.y;
            case ControllerType.Gamepad2:
                return gamepadStick == GamepadStick.right ? oldGP2RightStickPosition.x < GP2RightThumbStickDeadZone.y && newGP2RightStickPosition.x >= GP2RightThumbStickDeadZone.y :
                    oldGP2LeftStickPosition.x < GP2LeftThumbStickDeadZone.y && newGP2LeftStickPosition.x >= GP2LeftThumbStickDeadZone.y;
            case ControllerType.Gamepad3:
                return gamepadStick == GamepadStick.right ? oldGP3RightStickPosition.x < GP3RightThumbStickDeadZone.y && newGP3RightStickPosition.x >= GP3RightThumbStickDeadZone.y :
                    oldGP3LeftStickPosition.x < GP3LeftThumbStickDeadZone.y && newGP3LeftStickPosition.x >= GP3LeftThumbStickDeadZone.y;
            case ControllerType.Gamepad4:
                return gamepadStick == GamepadStick.right ? oldGP4RightStickPosition.x < GP4RightThumbStickDeadZone.y && newGP4RightStickPosition.x >= GP4RightThumbStickDeadZone.y :
                    oldGP4LeftStickPosition.x < GP4LeftThumbStickDeadZone.y && newGP4LeftStickPosition.x >= GP4LeftThumbStickDeadZone.y;
            case ControllerType.GamepadAll:
                if (GamePadIsConnected(ControllerType.Gamepad1))
                    return gamepadStick == GamepadStick.right ? oldGP1RightStickPosition.x < GP1RightThumbStickDeadZone.y && newGP1RightStickPosition.x >= GP1RightThumbStickDeadZone.y :
                        oldGP1LeftStickPosition.x < GP1LeftThumbStickDeadZone.y && newGP1LeftStickPosition.x >= GP1LeftThumbStickDeadZone.y;
                if (GamePadIsConnected(ControllerType.Gamepad2))
                    return gamepadStick == GamepadStick.right ? oldGP2RightStickPosition.x < GP2RightThumbStickDeadZone.y && newGP2RightStickPosition.x >= GP2RightThumbStickDeadZone.y :
                        oldGP2LeftStickPosition.x < GP2LeftThumbStickDeadZone.y && newGP2LeftStickPosition.x >= GP2LeftThumbStickDeadZone.y;
                if (GamePadIsConnected(ControllerType.Gamepad3))
                    return gamepadStick == GamepadStick.right ? oldGP3RightStickPosition.x < GP3RightThumbStickDeadZone.y && newGP3RightStickPosition.x >= GP3RightThumbStickDeadZone.y :
                        oldGP3LeftStickPosition.x < GP3LeftThumbStickDeadZone.y && newGP3LeftStickPosition.x >= GP3LeftThumbStickDeadZone.y;
                if (GamePadIsConnected(ControllerType.Gamepad4))
                    return gamepadStick == GamepadStick.right ? oldGP4RightStickPosition.x < GP4RightThumbStickDeadZone.y && newGP4RightStickPosition.x >= GP4RightThumbStickDeadZone.y :
                        oldGP4LeftStickPosition.x < GP4LeftThumbStickDeadZone.y && newGP4LeftStickPosition.x >= GP4LeftThumbStickDeadZone.y;
                Debug.LogWarning("No Gamepad is connected!");
                return false;
            case ControllerType.All:
                if (GamePadIsConnected(ControllerType.Gamepad1))
                    return gamepadStick == GamepadStick.right ? oldGP1RightStickPosition.x < GP1RightThumbStickDeadZone.y && newGP1RightStickPosition.x >= GP1RightThumbStickDeadZone.y :
                        oldGP1LeftStickPosition.x < GP1LeftThumbStickDeadZone.y && newGP1LeftStickPosition.x >= GP1LeftThumbStickDeadZone.y;
                if (GamePadIsConnected(ControllerType.Gamepad2))
                    return gamepadStick == GamepadStick.right ? oldGP2RightStickPosition.x < GP2RightThumbStickDeadZone.y && newGP2RightStickPosition.x >= GP2RightThumbStickDeadZone.y :
                        oldGP2LeftStickPosition.x < GP2LeftThumbStickDeadZone.y && newGP2LeftStickPosition.x >= GP2LeftThumbStickDeadZone.y;
                if (GamePadIsConnected(ControllerType.Gamepad3))
                    return gamepadStick == GamepadStick.right ? oldGP3RightStickPosition.x < GP3RightThumbStickDeadZone.y && newGP3RightStickPosition.x >= GP3RightThumbStickDeadZone.y :
                        oldGP3LeftStickPosition.x < GP3LeftThumbStickDeadZone.y && newGP3LeftStickPosition.x >= GP3LeftThumbStickDeadZone.y;
                if (GamePadIsConnected(ControllerType.Gamepad4))
                    return gamepadStick == GamepadStick.right ? oldGP4RightStickPosition.x < GP4RightThumbStickDeadZone.y && newGP4RightStickPosition.x >= GP4RightThumbStickDeadZone.y :
                        oldGP4LeftStickPosition.x < GP4LeftThumbStickDeadZone.y && newGP4LeftStickPosition.x >= GP4LeftThumbStickDeadZone.y;
                Debug.LogWarning("No Gamepad is connected!");
                return false;
            default:
                return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="controllerType"></param>
    /// <param name="gamepadStick"></param>
    /// <returns>true the frame when the gamepad stick reach the right (x value became 1f)</returns>
    public static bool GetGamepadStickLeft(in ControllerType controllerType, in GamepadStick gamepadStick)
    {
        switch (controllerType)
        {
            case ControllerType.Keyboard:
                Debug.LogWarning("A keyboard does'nt have stick!");
                return false;
            case ControllerType.Gamepad1:
                return gamepadStick == GamepadStick.right ? oldGP1RightStickPosition.x > -GP1RightThumbStickDeadZone.y && newGP1RightStickPosition.x <= -GP1RightThumbStickDeadZone.y :
                    oldGP1LeftStickPosition.x > -GP1LeftThumbStickDeadZone.y && newGP1LeftStickPosition.x <= -GP1LeftThumbStickDeadZone.y;
            case ControllerType.Gamepad2:
                return gamepadStick == GamepadStick.right ? oldGP2RightStickPosition.x > -GP2RightThumbStickDeadZone.y && newGP2RightStickPosition.x <= -GP2RightThumbStickDeadZone.y :
                    oldGP2LeftStickPosition.x > -GP2LeftThumbStickDeadZone.y && newGP2LeftStickPosition.x <= -GP2LeftThumbStickDeadZone.y;
            case ControllerType.Gamepad3:
                return gamepadStick == GamepadStick.right ? oldGP3RightStickPosition.x > -GP3RightThumbStickDeadZone.y && newGP3RightStickPosition.x <= -GP3RightThumbStickDeadZone.y :
                    oldGP3LeftStickPosition.x > -GP3LeftThumbStickDeadZone.y && newGP3LeftStickPosition.x <= -GP3LeftThumbStickDeadZone.y;
            case ControllerType.Gamepad4:
                return gamepadStick == GamepadStick.right ? oldGP4RightStickPosition.x > -GP4RightThumbStickDeadZone.y && newGP4RightStickPosition.x <= -GP4RightThumbStickDeadZone.y :
                    oldGP4LeftStickPosition.x > -GP4LeftThumbStickDeadZone.y && newGP4LeftStickPosition.x <= -GP4LeftThumbStickDeadZone.y;
            case ControllerType.GamepadAll:
                if (GamePadIsConnected(ControllerType.Gamepad1))
                    return gamepadStick == GamepadStick.right ? oldGP1RightStickPosition.x > -GP1RightThumbStickDeadZone.y && newGP1RightStickPosition.x <= -GP1RightThumbStickDeadZone.y :
                        oldGP1LeftStickPosition.x > -GP1LeftThumbStickDeadZone.y && newGP1LeftStickPosition.x <= -GP1LeftThumbStickDeadZone.y;
                if (GamePadIsConnected(ControllerType.Gamepad2))
                    return gamepadStick == GamepadStick.right ? oldGP2RightStickPosition.x > -GP2RightThumbStickDeadZone.y && newGP2RightStickPosition.x <= -GP2RightThumbStickDeadZone.y :
                        oldGP2LeftStickPosition.x > -GP2LeftThumbStickDeadZone.y && newGP2LeftStickPosition.x <= -GP2LeftThumbStickDeadZone.y;
                if (GamePadIsConnected(ControllerType.Gamepad3))
                    return gamepadStick == GamepadStick.right ? oldGP3RightStickPosition.x > -GP3RightThumbStickDeadZone.y && newGP3RightStickPosition.x <= -GP3RightThumbStickDeadZone.y :
                        oldGP3LeftStickPosition.x > -GP3LeftThumbStickDeadZone.y && newGP3LeftStickPosition.x <= -GP3LeftThumbStickDeadZone.y;
                if (GamePadIsConnected(ControllerType.Gamepad4))
                    return gamepadStick == GamepadStick.right ? oldGP4RightStickPosition.x > -GP4RightThumbStickDeadZone.y && newGP4RightStickPosition.x <= -GP4RightThumbStickDeadZone.y :
                        oldGP4LeftStickPosition.x > -GP4LeftThumbStickDeadZone.y && newGP4LeftStickPosition.x <= -GP4LeftThumbStickDeadZone.y;
                Debug.LogWarning("No Gamepad is connected!");
                return false;
            case ControllerType.All:
                if (GamePadIsConnected(ControllerType.Gamepad1))
                    return gamepadStick == GamepadStick.right ? oldGP1RightStickPosition.x > -GP1RightThumbStickDeadZone.y && newGP1RightStickPosition.x <= -GP1RightThumbStickDeadZone.y :
                        oldGP1LeftStickPosition.x > -GP1LeftThumbStickDeadZone.y && newGP1LeftStickPosition.x <= -GP1LeftThumbStickDeadZone.y;
                if (GamePadIsConnected(ControllerType.Gamepad2))
                    return gamepadStick == GamepadStick.right ? oldGP2RightStickPosition.x > -GP2RightThumbStickDeadZone.y && newGP2RightStickPosition.x <= -GP2RightThumbStickDeadZone.y :
                        oldGP2LeftStickPosition.x > -GP2LeftThumbStickDeadZone.y && newGP2LeftStickPosition.x <= -GP2LeftThumbStickDeadZone.y;
                if (GamePadIsConnected(ControllerType.Gamepad3))
                    return gamepadStick == GamepadStick.right ? oldGP3RightStickPosition.x > -GP3RightThumbStickDeadZone.y && newGP3RightStickPosition.x <= -GP3RightThumbStickDeadZone.y :
                        oldGP3LeftStickPosition.x > -GP3LeftThumbStickDeadZone.y && newGP3LeftStickPosition.x <= -GP3LeftThumbStickDeadZone.y;
                if (GamePadIsConnected(ControllerType.Gamepad4))
                    return gamepadStick == GamepadStick.right ? oldGP4RightStickPosition.x > -GP4RightThumbStickDeadZone.y && newGP4RightStickPosition.x <= -GP4RightThumbStickDeadZone.y :
                        oldGP4LeftStickPosition.x > -GP4LeftThumbStickDeadZone.y && newGP4LeftStickPosition.x <= -GP4LeftThumbStickDeadZone.y;
                Debug.LogWarning("No Gamepad is connected!");
                return false;
            default:
                return false;
        }
    }

    #endregion

    public static float GetGamepadTrigger(in ControllerType controllerType, in GamepadTrigger gamepadTrigger)
    {
        switch (controllerType)
        {
            case ControllerType.Keyboard:
                Debug.LogWarning("A keyboard does'nt have triggers");
                return 0f;
            case ControllerType.Gamepad1:
                return gamepadTrigger == GamepadTrigger.right ? newGP1Triggers.y : newGP1Triggers.x;
            case ControllerType.Gamepad2:
                return gamepadTrigger == GamepadTrigger.right ? newGP2Triggers.y : newGP2Triggers.x;
            case ControllerType.Gamepad3:
                return gamepadTrigger == GamepadTrigger.right ? newGP3Triggers.y : newGP3Triggers.x;
            case ControllerType.Gamepad4:
                return gamepadTrigger == GamepadTrigger.right ? newGP4Triggers.y : newGP4Triggers.x;
            case ControllerType.GamepadAll:
                return gamepadTrigger == GamepadTrigger.right ? Mathf.Max(newGP1Triggers.y, newGP2Triggers.y, newGP3Triggers.y, newGP4Triggers.y) : Mathf.Max(newGP1Triggers.x, newGP2Triggers.x, newGP3Triggers.x, newGP4Triggers.x);
            case ControllerType.All:
                return gamepadTrigger == GamepadTrigger.right ? Mathf.Max(newGP1Triggers.y, newGP2Triggers.y, newGP3Triggers.y, newGP4Triggers.y) : Mathf.Max(newGP1Triggers.x, newGP2Triggers.x, newGP3Triggers.x, newGP4Triggers.x);
            default:
                return 0f;
        }
    }

    /// <returns>true if the gamepad define by the gamepadIndex is connected, false otherwise </returns>
    public static bool GamePadIsConnected(in ControllerType gamepadIndex)
    {
        switch (gamepadIndex)
        {
            case ControllerType.Keyboard:
                Debug.Log("The keyboard is not a Gamepad!");
                return false;
            case ControllerType.Gamepad1:
                return newGP1State.IsConnected;
            case ControllerType.Gamepad2:
                return newGP2State.IsConnected;
            case ControllerType.Gamepad3:
                return newGP3State.IsConnected;
            case ControllerType.Gamepad4:
                return newGP4State.IsConnected;
            case ControllerType.GamepadAll:
                return newGP1State.IsConnected && newGP2State.IsConnected && newGP3State.IsConnected && newGP4State.IsConnected;
            case ControllerType.All:
                return newGP1State.IsConnected && newGP2State.IsConnected && newGP3State.IsConnected && newGP4State.IsConnected;
            default:
                return false;
        }
    }

    /// <returns>true if a gamepad is pluged at the current frame and return the gamepadIndex of the plugged gamepad, false otherwise </returns>
    public static bool GetGamepadPlugged(out ControllerType gamepadIndex)
    {
        if(newGP1State.IsConnected && !oldGP1State.IsConnected)
        {
            gamepadIndex = ControllerType.Gamepad1;
            return true;
        }
        if (newGP2State.IsConnected && !oldGP2State.IsConnected)
        {
            gamepadIndex = ControllerType.Gamepad2;
            return true;
        }
        if (newGP3State.IsConnected && !oldGP3State.IsConnected)
        {
            gamepadIndex = ControllerType.Gamepad3;
            return true;
        }
        if (newGP4State.IsConnected && !oldGP4State.IsConnected)
        {
            gamepadIndex = ControllerType.Gamepad4;
            return true;
        }
        gamepadIndex = ControllerType.Gamepad1;
        return false;
    }

    /// <returns>true if a gamepad is plugged at the current frame and return all the gamepadIndex of plugged gamepads, false otherwise </returns>
    public static bool GetGamepadPluggedAll(out ControllerType[] gamepadIndex)
    {
        List<ControllerType> res = null;
        bool b = false;
        if (newGP1State.IsConnected && !oldGP1State.IsConnected)
        {
            if (res == null)
                res = new List<ControllerType>();
            res.Add(ControllerType.Gamepad1);
            b = true;
        }
        if (newGP2State.IsConnected && !oldGP2State.IsConnected)
        {
            if (res == null)
                res = new List<ControllerType>();
            res.Add(ControllerType.Gamepad2);
            b = true; ;
        }
        if (newGP3State.IsConnected && !oldGP3State.IsConnected)
        {
            if (res == null)
                res = new List<ControllerType>();
            res.Add(ControllerType.Gamepad3);
            b = true;
        }
        if (newGP4State.IsConnected && !oldGP4State.IsConnected)
        {
            if (res == null)
                res = new List<ControllerType>();
            res.Add(ControllerType.Gamepad4);
            b = true;
        }
        if(b)
        {
            gamepadIndex = res.ToArray();
            return true;
        }
        gamepadIndex = null;
        return false;
    }

    /// <returns>true if a gamepad is unplugged at the current frame and return the gamepadIndex of the unplugged gamepad, false otherwise </returns>
    public static bool GetGamepadUnPlugged(out ControllerType gamepadIndex)
    {
        if (!newGP1State.IsConnected && oldGP1State.IsConnected)
        {
            gamepadIndex = ControllerType.Gamepad1;
            return true;
        }
        if (!newGP2State.IsConnected && oldGP2State.IsConnected)
        {
            gamepadIndex = ControllerType.Gamepad2;
            return true;
        }
        if (!newGP3State.IsConnected && oldGP3State.IsConnected)
        {
            gamepadIndex = ControllerType.Gamepad3;
            return true;
        }
        if (!newGP4State.IsConnected && oldGP4State.IsConnected)
        {
            gamepadIndex = ControllerType.Gamepad4;
            return true;
        }
        gamepadIndex = ControllerType.Gamepad1;
        return false;
    }

    /// <returns>true if a gamepad is unplugged at the current frame and return all the gamepadIndex of unplugged gamepads, false otherwise </returns>
    public static bool GetGamepadUnPluggedAll(out ControllerType[] gamepadIndex)
    {
        List<ControllerType> res = null;
        bool b = false;
        if (!newGP1State.IsConnected && oldGP1State.IsConnected)
        {
            if (res == null)
                res = new List<ControllerType>();
            res.Add(ControllerType.Gamepad1);
            b = true;
        }
        if (!newGP2State.IsConnected && oldGP2State.IsConnected)
        {
            if (res == null)
                res = new List<ControllerType>();
            res.Add(ControllerType.Gamepad2);
            b = true; ;
        }
        if (!newGP3State.IsConnected && oldGP3State.IsConnected)
        {
            if (res == null)
                res = new List<ControllerType>();
            res.Add(ControllerType.Gamepad3);
            b = true;
        }
        if (!newGP4State.IsConnected && oldGP4State.IsConnected)
        {
            if (res == null)
                res = new List<ControllerType>();
            res.Add(ControllerType.Gamepad4);
            b = true;
        }
        if (b)
        {
            gamepadIndex = res.ToArray();
            return true;
        }
        gamepadIndex = null;
        return false;
    }

    #endregion

    #region GetKeyDown / GetKeyUp / GetKey

    private static bool GetNegativeKeyCodeDown(in NegativeKeyCode key)
    {
        return GetNegativeKeyCodeDownDelegate[(-(int)key)].Invoke();
    }

    private static bool GetNegativeKeyCodeUp(in NegativeKeyCode key)
    {
        return GetNegativeKeyCodeUpDelegate[(-(int)key)].Invoke();
    }

    private static bool GetNegativeKeyCodePressed(in NegativeKeyCode key)
    {
        return GetNegativeKeyCodePressedDelegate[(-(int)key)].Invoke();
    }

    /// <returns> true during the frame when the key assigned with the action is pressed</returns>
    public static bool GetKeyDown(string action, in PlayerIndex player = PlayerIndex.All)
    {
        int key;
        switch (player)
        {
            case PlayerIndex.One:
                key = player1Keys.GetKey(action);
                return key >= 0 ? Input.GetKeyDown((KeyCode)key) : GetNegativeKeyCodeDown((NegativeKeyCode)key);
            case PlayerIndex.Two:
                key = player2Keys.GetKey(action);
                return key >= 0 ? Input.GetKeyDown((KeyCode)key) : GetNegativeKeyCodeDown((NegativeKeyCode)key);
            case PlayerIndex.Three:
                key = player3Keys.GetKey(action);
                return key >= 0 ? Input.GetKeyDown((KeyCode)key) : GetNegativeKeyCodeDown((NegativeKeyCode)key);
            case PlayerIndex.Four:
                key = player4Keys.GetKey(action);
                return key >= 0 ? Input.GetKeyDown((KeyCode)key) : GetNegativeKeyCodeDown((NegativeKeyCode)key);
            case PlayerIndex.Five:
                key = player5Keys.GetKey(action);
                return key >= 0 ? Input.GetKeyDown((KeyCode)key) : GetNegativeKeyCodeDown((NegativeKeyCode)key);
            case PlayerIndex.All:
                int key1 = player1Keys.GetKey(action);
                int key2 = player2Keys.GetKey(action);
                int key3 = player3Keys.GetKey(action);
                int key4 = player4Keys.GetKey(action);
                int key5 = player5Keys.GetKey(action);
                return key1 >= 0 ? Input.GetKeyDown((KeyCode)key1) : GetNegativeKeyCodeDown((NegativeKeyCode)key1) ||
                    key2 >= 0 ? Input.GetKeyDown((KeyCode)key2) : GetNegativeKeyCodeDown((NegativeKeyCode)key2) ||
                    key3 >= 0 ? Input.GetKeyDown((KeyCode)key3) : GetNegativeKeyCodeDown((NegativeKeyCode)key3) ||
                    key4 >= 0 ? Input.GetKeyDown((KeyCode)key4) : GetNegativeKeyCodeDown((NegativeKeyCode)key4) ||
                    key5 >= 0 ? Input.GetKeyDown((KeyCode)key5) : GetNegativeKeyCodeDown((NegativeKeyCode)key5); 
            default:
                return false;
        }
    }

    /// <returns> true during the frame when the key assigned with the action is pressed</returns>
    public static bool GetKeyDown(string action, in PlayerIndex player, out PlayerIndex playerWhoPressesDown)
    {
        playerWhoPressesDown = player;
        int key;
        switch (player)
        {
            case PlayerIndex.One:
                key = player1Keys.GetKey(action);
                return key >= 0 ? Input.GetKeyDown((KeyCode)key) : GetNegativeKeyCodeDown((NegativeKeyCode)key);
            case PlayerIndex.Two:
                key = player2Keys.GetKey(action);
                return key >= 0 ? Input.GetKeyDown((KeyCode)key) : GetNegativeKeyCodeDown((NegativeKeyCode)key);
            case PlayerIndex.Three:
                key = player3Keys.GetKey(action);
                return key >= 0 ? Input.GetKeyDown((KeyCode)key) : GetNegativeKeyCodeDown((NegativeKeyCode)key);
            case PlayerIndex.Four:
                key = player4Keys.GetKey(action);
                return key >= 0 ? Input.GetKeyDown((KeyCode)key) : GetNegativeKeyCodeDown((NegativeKeyCode)key);
            case PlayerIndex.Five:
                key = player5Keys.GetKey(action);
                return key >= 0 ? Input.GetKeyDown((KeyCode)key) : GetNegativeKeyCodeDown((NegativeKeyCode)key);
            case PlayerIndex.All:
                key = player1Keys.GetKey(action);
                if (key >= 0 ? Input.GetKeyDown((KeyCode)key) : GetNegativeKeyCodeDown((NegativeKeyCode)key))
                {
                    playerWhoPressesDown = PlayerIndex.One;
                    return true;
                }
                key = player2Keys.GetKey(action);
                if (key >= 0 ? Input.GetKeyDown((KeyCode)key) : GetNegativeKeyCodeDown((NegativeKeyCode)key))
                {
                    playerWhoPressesDown = PlayerIndex.Two;
                    return true;
                }
                key = player3Keys.GetKey(action);
                if (key >= 0 ? Input.GetKeyDown((KeyCode)key) : GetNegativeKeyCodeDown((NegativeKeyCode)key))
                {
                    playerWhoPressesDown = PlayerIndex.Three;
                    return true;
                }
                key = player4Keys.GetKey(action);
                if (key >= 0 ? Input.GetKeyDown((KeyCode)key) : GetNegativeKeyCodeDown((NegativeKeyCode)key))
                {
                    playerWhoPressesDown = PlayerIndex.Four;
                    return true;
                }
                key = player5Keys.GetKey(action);
                if (key >= 0 ? Input.GetKeyDown((KeyCode)key) : GetNegativeKeyCodeDown((NegativeKeyCode)key))
                {
                    playerWhoPressesDown = PlayerIndex.Five;
                    return true;
                }
                return false;
            default:
                return false;
        }
    }

    /// <returns> true during the frame when the key assigned with the action is unpressed</returns>
    public static bool GetKeyUp(string action, in PlayerIndex player = PlayerIndex.All)
    {
        int key;
        switch (player)
        {
            case PlayerIndex.One:
                key = player1Keys.GetKey(action);
                return key >= 0 ? Input.GetKeyUp((KeyCode)key) : GetNegativeKeyCodeUp((NegativeKeyCode)key);
            case PlayerIndex.Two:
                key = player2Keys.GetKey(action);
                return key >= 0 ? Input.GetKeyUp((KeyCode)key) : GetNegativeKeyCodeUp((NegativeKeyCode)key);
            case PlayerIndex.Three:
                key = player3Keys.GetKey(action);
                return key >= 0 ? Input.GetKeyUp((KeyCode)key) : GetNegativeKeyCodeUp((NegativeKeyCode)key);
            case PlayerIndex.Four:
                key = player4Keys.GetKey(action);
                return key >= 0 ? Input.GetKeyUp((KeyCode)key) : GetNegativeKeyCodeUp((NegativeKeyCode)key);
            case PlayerIndex.Five:
                key = player5Keys.GetKey(action);
                return key >= 0 ? Input.GetKeyUp((KeyCode)key) : GetNegativeKeyCodeUp((NegativeKeyCode)key);
            case PlayerIndex.All:
                int key1 = player1Keys.GetKey(action);
                int key2 = player2Keys.GetKey(action);
                int key3 = player3Keys.GetKey(action);
                int key4 = player4Keys.GetKey(action);
                int key5 = player5Keys.GetKey(action);
                return key1 >= 0 ? Input.GetKeyUp((KeyCode)key1) : GetNegativeKeyCodeUp((NegativeKeyCode)key1) ||
                    key2 >= 0 ? Input.GetKeyUp((KeyCode)key2) : GetNegativeKeyCodeUp((NegativeKeyCode)key2) ||
                    key3 >= 0 ? Input.GetKeyUp((KeyCode)key3) : GetNegativeKeyCodeUp((NegativeKeyCode)key3) ||
                    key4 >= 0 ? Input.GetKeyUp((KeyCode)key4) : GetNegativeKeyCodeUp((NegativeKeyCode)key4) ||
                    key5 >= 0 ? Input.GetKeyUp((KeyCode)key5) : GetNegativeKeyCodeUp((NegativeKeyCode)key5);
            default:
                return false;
        }
    }

    /// <returns> true during the frame when the key assigned with the action is pressed up</returns>
    public static bool GetKeyUp(string action, in PlayerIndex player, out PlayerIndex playerWhoPressesUp)
    {
        playerWhoPressesUp = player;
        int key;
        switch (player)
        {
            case PlayerIndex.One:
                key = player1Keys.GetKey(action);
                return key >= 0 ? Input.GetKeyUp((KeyCode)key) : GetNegativeKeyCodeUp((NegativeKeyCode)key);
            case PlayerIndex.Two:
                key = player2Keys.GetKey(action);
                return key >= 0 ? Input.GetKeyUp((KeyCode)key) : GetNegativeKeyCodeUp((NegativeKeyCode)key);
            case PlayerIndex.Three:
                key = player3Keys.GetKey(action);
                return key >= 0 ? Input.GetKeyUp((KeyCode)key) : GetNegativeKeyCodeUp((NegativeKeyCode)key);
            case PlayerIndex.Four:
                key = player4Keys.GetKey(action);
                return key >= 0 ? Input.GetKeyUp((KeyCode)key) : GetNegativeKeyCodeUp((NegativeKeyCode)key);
            case PlayerIndex.Five:
                key = player5Keys.GetKey(action);
                return key >= 0 ? Input.GetKeyUp((KeyCode)key) : GetNegativeKeyCodeUp((NegativeKeyCode)key);
            case PlayerIndex.All:
                key = player1Keys.GetKey(action);
                if (key >= 0 ? Input.GetKeyUp((KeyCode)key) : GetNegativeKeyCodeUp((NegativeKeyCode)key))
                {
                    playerWhoPressesUp = PlayerIndex.One;
                    return true;
                }
                key = player2Keys.GetKey(action);
                if (key >= 0 ? Input.GetKeyUp((KeyCode)key) : GetNegativeKeyCodeUp((NegativeKeyCode)key))
                {
                    playerWhoPressesUp = PlayerIndex.Two;
                    return true;
                }
                key = player3Keys.GetKey(action);
                if (key >= 0 ? Input.GetKeyUp((KeyCode)key) : GetNegativeKeyCodeUp((NegativeKeyCode)key))
                {
                    playerWhoPressesUp = PlayerIndex.Three;
                    return true;
                }
                key = player4Keys.GetKey(action);
                if (key >= 0 ? Input.GetKeyUp((KeyCode)key) : GetNegativeKeyCodeUp((NegativeKeyCode)key))
                {
                    playerWhoPressesUp = PlayerIndex.Four;
                    return true;
                }
                key = player5Keys.GetKey(action);
                if (key >= 0 ? Input.GetKeyUp((KeyCode)key) : GetNegativeKeyCodeUp((NegativeKeyCode)key))
                {
                    playerWhoPressesUp = PlayerIndex.Five;
                    return true;
                }
                return false;
            default:
                return false;
        }
    }

    /// <returns> true when the key assigned with the action is pressed</returns>
    public static bool GetKey(string action, in PlayerIndex player = PlayerIndex.All)
    {
        int key;
        switch (player)
        {
            case PlayerIndex.One:
                key = player1Keys.GetKey(action);
                return key >= 0 ? Input.GetKey((KeyCode)key) : GetNegativeKeyCodePressed((NegativeKeyCode)key);
            case PlayerIndex.Two:
                key = player2Keys.GetKey(action);
                return key >= 0 ? Input.GetKey((KeyCode)key) : GetNegativeKeyCodePressed((NegativeKeyCode)key);
            case PlayerIndex.Three:
                key = player3Keys.GetKey(action);
                return key >= 0 ? Input.GetKey((KeyCode)key) : GetNegativeKeyCodePressed((NegativeKeyCode)key);
            case PlayerIndex.Four:
                key = player4Keys.GetKey(action);
                return key >= 0 ? Input.GetKey((KeyCode)key) : GetNegativeKeyCodePressed((NegativeKeyCode)key);
            case PlayerIndex.Five:
                key = player5Keys.GetKey(action);
                return key >= 0 ? Input.GetKey((KeyCode)key) : GetNegativeKeyCodePressed((NegativeKeyCode)key);
            case PlayerIndex.All:
                int key1 = player1Keys.GetKey(action);
                int key2 = player2Keys.GetKey(action);
                int key3 = player3Keys.GetKey(action);
                int key4 = player4Keys.GetKey(action);
                int key5 = player5Keys.GetKey(action);
                return key1 >= 0 ? Input.GetKey((KeyCode)key1) : GetNegativeKeyCodePressed((NegativeKeyCode)key1) ||
                    key2 >= 0 ? Input.GetKey((KeyCode)key2) : GetNegativeKeyCodePressed((NegativeKeyCode)key2) ||
                    key3 >= 0 ? Input.GetKey((KeyCode)key3) : GetNegativeKeyCodePressed((NegativeKeyCode)key3) ||
                    key4 >= 0 ? Input.GetKey((KeyCode)key4) : GetNegativeKeyCodePressed((NegativeKeyCode)key4) ||
                    key5 >= 0 ? Input.GetKey((KeyCode)key5) : GetNegativeKeyCodePressed((NegativeKeyCode)key5);
            default:
                return false;
        }
    }

    /// <returns> true during while the key assigned with the action is pressed</returns>
    public static bool GetKey(string action, in PlayerIndex player, out PlayerIndex playerWhoPressed)
    {
        playerWhoPressed = player;
        int key;
        switch (player)
        {
            case PlayerIndex.One:
                key = player1Keys.GetKey(action);
                return key >= 0 ? Input.GetKey((KeyCode)key) : GetNegativeKeyCodePressed((NegativeKeyCode)key);
            case PlayerIndex.Two:
                key = player2Keys.GetKey(action);
                return key >= 0 ? Input.GetKey((KeyCode)key) : GetNegativeKeyCodePressed((NegativeKeyCode)key);
            case PlayerIndex.Three:
                key = player3Keys.GetKey(action);
                return key >= 0 ? Input.GetKey((KeyCode)key) : GetNegativeKeyCodePressed((NegativeKeyCode)key);
            case PlayerIndex.Four:
                key = player4Keys.GetKey(action);
                return key >= 0 ? Input.GetKey((KeyCode)key) : GetNegativeKeyCodePressed((NegativeKeyCode)key);
            case PlayerIndex.Five:
                key = player5Keys.GetKey(action);
                return key >= 0 ? Input.GetKey((KeyCode)key) : GetNegativeKeyCodePressed((NegativeKeyCode)key);
            case PlayerIndex.All:
                key = player1Keys.GetKey(action);
                if (key >= 0 ? Input.GetKey((KeyCode)key) : GetNegativeKeyCodePressed((NegativeKeyCode)key))
                {
                    playerWhoPressed = PlayerIndex.One;
                    return true;
                }
                key = player2Keys.GetKey(action);
                if (key >= 0 ? Input.GetKey((KeyCode)key) : GetNegativeKeyCodePressed((NegativeKeyCode)key))
                {
                    playerWhoPressed = PlayerIndex.Two;
                    return true;
                }
                key = player3Keys.GetKey(action);
                if (key >= 0 ? Input.GetKey((KeyCode)key) : GetNegativeKeyCodePressed((NegativeKeyCode)key))
                {
                    playerWhoPressed = PlayerIndex.Three;
                    return true;
                }
                key = player4Keys.GetKey(action);
                if (key >= 0 ? Input.GetKey((KeyCode)key) : GetNegativeKeyCodePressed((NegativeKeyCode)key))
                {
                    playerWhoPressed = PlayerIndex.Four;
                    return true;
                }
                key = player5Keys.GetKey(action);
                if (key >= 0 ? Input.GetKey((KeyCode)key) : GetNegativeKeyCodePressed((NegativeKeyCode)key))
                {
                    playerWhoPressed = PlayerIndex.Five;
                    return true;
                }
                return false;
            default:
                return false;
        }
    }

    /// <returns> true during the frame when a key assigned with one of the actions is pressed</returns>
    public static bool GetKeyDown(List<string> actions, in PlayerIndex player = PlayerIndex.All)
    {
        foreach (string action in actions)
        {
            if(GetKeyDown(action, player))
            {
                return true;
            }
        }
        return false;
    }

    /// <returns> true during the frame when a key assigned with one of the actions is unpressed</returns>
    public static bool GetKeyUp(List<string> actions, in PlayerIndex player = PlayerIndex.All)
    {
        foreach (string action in actions)
        {
            if (GetKeyUp(action, player))
            {
                return true;
            }
        }
        return false;
    }

    /// <returns> true when a key assigned with one of the actions is pressed</returns>
    public static bool GetKey(List<string> actions, in PlayerIndex player = PlayerIndex.All)
    {
        foreach (string action in actions)
        {
            if (GetKey(action, player))
            {
                return true;
            }
        }
        return false;
    }

    /// <returns> true during the frame when the key is pressed</returns>
    public static bool GetKeyDown(in KeyCode key) => Input.GetKeyDown(key);
    /// <returns> true during the frame when key is unpressed</returns>
    public static bool GetKeyUp(in KeyCode key) => Input.GetKeyDown(key);
    /// <returns> true when the key is pressed</returns>
    public static bool GetKey(in KeyCode key) => Input.GetKey(key);
    /// <returns> true during the frame when the key is pressed</returns>
    public static bool GetKeyDown(in NegativeKeyCode key) => GetNegativeKeyCodeDown(key);
    /// <returns> true during the frame when key is unpressed</returns>
    public static bool GetKeyUp(in NegativeKeyCode key) => GetNegativeKeyCodeUp(key);
    /// <returns> true when the key is pressed</returns>
    public static bool GetKey(in NegativeKeyCode key) => GetNegativeKeyCodePressed(key);
    /// <returns> true during the frame when the key is pressed</returns>
    public static bool GetKeyDown(in int key) => key >= 0 ? Input.GetKeyDown((KeyCode)key) : GetNegativeKeyCodeDown((NegativeKeyCode)key);
    /// <returns> true during the frame when key is unpressed</returns>
    public static bool GetKeyUp(in int key) => key >= 0 ? Input.GetKeyUp((KeyCode)key) : GetNegativeKeyCodeUp((NegativeKeyCode)key);
    /// <returns> true when the key is pressed</returns>
    public static bool GetKey(in int key) => key >= 0 ? Input.GetKey((KeyCode)key) : GetNegativeKeyCodePressed((NegativeKeyCode)key);

    #endregion

    #region Management controller

    /// <summary>
    /// Add an action to the CustomInput system. Multiply action can have the same key.
    /// </summary>
    /// <param name="action"> The action</param>
    /// <param name="keyboardKey"> The keyboard key link with the action</param>
    public static void AddInputAction(string action, in KeyCode key, in PlayerIndex player = PlayerIndex.All)
    {
        switch (player)
        {
            case PlayerIndex.One:
                player1Keys.AddAction(action, (int)key);
                break;
            case PlayerIndex.Two:
                player2Keys.AddAction(action, (int)key);
                break;
            case PlayerIndex.Three:
                player3Keys.AddAction(action, (int)key);
                break;
            case PlayerIndex.Four:
                player4Keys.AddAction(action, (int)key);
                break;
            case PlayerIndex.Five:
                player5Keys.AddAction(action, (int)key);
                break;
            case PlayerIndex.All:
                AddInputAction(action, key, PlayerIndex.One);
                AddInputAction(action, key, PlayerIndex.Two);
                AddInputAction(action, key, PlayerIndex.Three);
                AddInputAction(action, key, PlayerIndex.Four);
                AddInputAction(action, key, PlayerIndex.Five);
                break;
            default:
                break;
        }
    }

    public static void AddInputAction(List<string> actions, List<KeyCode> keys, in PlayerIndex player = PlayerIndex.All)
    {
        if (actions.Count != keys.Count)
            return;

        for (int i = 0; i < actions.Count; i++)
        {
            AddInputAction(actions[i], keys[i], player);
        }
    }


    /// <summary>
    /// Change the keyboard key assigned to the action in param
    /// </summary>
    public static bool ReplaceAction(string action, in KeyCode newKey, in PlayerIndex player = PlayerIndex.All)
    {
        switch (player)
        {
            case PlayerIndex.One:
                return player1Keys.ReplaceAction(action, (int)newKey);
            case PlayerIndex.Two:
                return player2Keys.ReplaceAction(action, (int)newKey);
            case PlayerIndex.Three:
                return player3Keys.ReplaceAction(action, (int)newKey);
            case PlayerIndex.Four:
                return player4Keys.ReplaceAction(action, (int)newKey);
            case PlayerIndex.Five:
                return player5Keys.ReplaceAction(action, (int)newKey);
            case PlayerIndex.All:
                bool b1 = player1Keys.ReplaceAction(action, (int)newKey);
                bool b2 = player2Keys.ReplaceAction(action, (int)newKey);
                bool b3 = player3Keys.ReplaceAction(action, (int)newKey);
                bool b4 = player4Keys.ReplaceAction(action, (int)newKey);
                bool b5 = player5Keys.ReplaceAction(action, (int)newKey);
                return b1 || b2 || b3 || b4 || b5;
            default:
                return false;
        }
    }

    /// <summary>
    /// Remove the action from the CustomInput system
    /// </summary>
    /// <param name="action"> The action to remove.</param>
    /// <param name="controllerType">The controller where the action will be removed.</param>
    public static bool RemoveAction(string action, in PlayerIndex player = PlayerIndex.All)
    {
        switch (player)
        {
            case PlayerIndex.One:
                return player1Keys.RemoveAction(action);
            case PlayerIndex.Two:
                return player2Keys.RemoveAction(action);
            case PlayerIndex.Three:
                return player3Keys.RemoveAction(action);
            case PlayerIndex.Four:
                return player4Keys.RemoveAction(action);
            case PlayerIndex.Five:
                return player5Keys.RemoveAction(action);
            case PlayerIndex.All:
                bool b1 = player1Keys.RemoveAction(action);
                bool b2 = player2Keys.RemoveAction(action);
                bool b3 = player3Keys.RemoveAction(action);
                bool b4 = player4Keys.RemoveAction(action);
                bool b5 = player5Keys.RemoveAction(action);
                return b1 || b2 || b3 || b4 || b5;
            default:
                return false;
        }
    }

    public static void ClearAll()
    {
        ClearCurrentConfiguration();
        ClearDefaultConfiguration();
    }

    public static void ClearCurrentConfiguration()
    {
        player1Keys = new InputData();
        player2Keys = new InputData();
        player3Keys = new InputData();
        player4Keys = new InputData();
        player5Keys = new InputData();
    }

    public static void ClearDefaultConfiguration()
    {
        defaultKBKeys = new InputData();
        defaultGB1Keys = new InputData();
        defaultGB2Keys = new InputData();
        defaultGB3Keys = new InputData();
        defaultGB4Keys = new InputData();
    }

    public static void ClearPlayerConfiguration(in PlayerIndex playerIndex, in bool defaultTo = false)
    {
        switch (playerIndex)
        {
            case PlayerIndex.One:
                player1Keys = new InputData();
                if(defaultTo)
                    defaultKBKeys = new InputData();
                break;
            case PlayerIndex.Two:
                player2Keys = new InputData();
                if (defaultTo)
                    defaultGB1Keys = new InputData();
                break;
            case PlayerIndex.Three:
                player3Keys = new InputData();
                if (defaultTo)
                    defaultGB2Keys = new InputData();
                break;
            case PlayerIndex.Four:
                player4Keys = new InputData();
                if (defaultTo)
                    defaultGB3Keys = new InputData();
                break;
            case PlayerIndex.Five:
                player5Keys = new InputData();
                if (defaultTo)
                    defaultGB4Keys = new InputData();
                break;
            case PlayerIndex.All:
                if (defaultTo)
                    ClearAll();
                else
                    ClearCurrentConfiguration();
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Set the default Control at the current configuration
    /// </summary>
    /// <param name="controllerType"></param>
    /// <param name="gamepadIndex"></param>
    public static void SetDefaultControler(in PlayerIndex player, in ControllerType controller)
    {
        InputData inputData = null;
        switch (player)
        {
            case PlayerIndex.One:
                inputData = player1Keys;
                break;
            case PlayerIndex.Two:
                inputData = player2Keys;
                break;
            case PlayerIndex.Three:
                inputData = player3Keys;
                break;
            case PlayerIndex.Four:
                inputData = player4Keys;
                break;
            case PlayerIndex.Five:
                inputData = player5Keys;
                break;
            default:
                Debug.LogWarning("Cannot set a default controller for multiple player inputs");
                return;
        }

        inputData = inputData.Clone();
        switch (controller)
        {
            case ControllerType.Keyboard:
                defaultKBKeys = inputData;
                break;
            case ControllerType.Gamepad1:
                defaultGB1Keys = inputData;
                break;
            case ControllerType.Gamepad2:
                defaultGB2Keys = inputData;
                break;
            case ControllerType.Gamepad3:
                defaultGB3Keys = inputData;
                break;
            case ControllerType.Gamepad4:
                defaultGB4Keys = inputData;
                break;
            default:
                Debug.LogWarning("Cannot set the default controller of multiple sources");
                return;
        }
    }


    /// <summary>
    /// Set the default Control at the current configuration
    /// </summary>
    /// <param name="controllerType"></param>
    /// <param name="gamepadIndex"></param>
    public static void SetDefaultControler()
    {
        defaultKBKeys = player1Keys.Clone();
        defaultGB1Keys = player2Keys.Clone();
        defaultGB2Keys = player3Keys.Clone();
        defaultGB3Keys = player4Keys.Clone();
        defaultGB4Keys = player5Keys.Clone();
    }

    /// <summary>
    /// Set the current Configuration as the default one.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="controller"></param>
    public static void LoadDefaultController(in PlayerIndex player, in ControllerType controller)
    {
        InputData inputData = null;
        switch (controller)
        {
            case ControllerType.Keyboard:
                inputData = defaultKBKeys;
                break;
            case ControllerType.Gamepad1:
                inputData = defaultGB1Keys;
                break;
            case ControllerType.Gamepad2:
                inputData = defaultGB2Keys;
                break;
            case ControllerType.Gamepad3:
                inputData = defaultGB3Keys;
                break;
            case ControllerType.Gamepad4:
                inputData = defaultGB4Keys;
                break;
            default:
                Debug.LogWarning("Cannot set the configuration as multiple sources");
                return;
        }

        inputData = inputData.Clone();
        switch (player)
        {
            case PlayerIndex.One:
                player1Keys = inputData;
                break;
            case PlayerIndex.Two:
                player2Keys = inputData;
                break;
            case PlayerIndex.Three:
                player3Keys = inputData;
                break;
            case PlayerIndex.Four:
                player4Keys = inputData;
                break;
            case PlayerIndex.Five:
                player5Keys = inputData;
                break;
            case PlayerIndex.All:
                player1Keys = inputData;
                player2Keys = inputData.Clone();
                player3Keys = inputData.Clone();
                player4Keys = inputData.Clone();
                player5Keys = inputData.Clone();
                break;
            default:
                return;
        }
    }

    #endregion

    #region SaveController

    [Serializable]
    private struct CustomInputConfigData
    {
        public InputData defaultKBKeys;
        public InputData defaultGB1Keys;
        public InputData defaultGB2Keys;
        public InputData defaultGB3Keys;
        public InputData defaultGB4Keys;
        public InputData player1Keys;
        public InputData player2Keys;
        public InputData player3Keys;
        public InputData player4Keys;
        public InputData player5Keys;

        public CustomInputConfigData(InputData defaultKBKeys, InputData defaultGB1Keys, InputData defaultGB2Keys, InputData defaultGB3Keys, 
            InputData defaultGB4Keys, InputData player1Keys, InputData player2Keys, InputData player3Keys, InputData player4Keys, InputData player5Keys)
        {
            this.defaultKBKeys = defaultKBKeys; this.defaultGB1Keys = defaultGB1Keys;
            this.defaultGB2Keys = defaultGB2Keys; this.defaultGB3Keys = defaultGB3Keys;
            this.defaultGB4Keys = defaultGB4Keys; this.player1Keys = player1Keys;
            this.player2Keys = player2Keys; this.player3Keys = player3Keys;
            this.player4Keys = player4Keys; this.player5Keys = player5Keys;
        }
    }

    /// <summary>
    /// Save all the current CustomInput configuration (default and current actions and controllers keys link to the action) for all players in the file in param,
    /// can be load using the methode CustomInput.LoadConfiguration(string fileName).
    /// </summary>
    public static bool SaveConfiguration(string fileName)
    {
        CustomInputConfigData CustomInputConfig = new CustomInputConfigData(defaultKBKeys.Clone(), defaultGB1Keys.Clone(), defaultGB2Keys.Clone(), defaultGB3Keys.Clone(), defaultGB4Keys.Clone(),
            player1Keys.Clone(), player2Keys.Clone(), player3Keys.Clone(), player4Keys.Clone(), player5Keys.Clone());
        return Save.WriteJSONData(CustomInputConfig, fileName);
    }

    /// <summary>
    /// Save all the default CustomInput configuration (default actions and controllers keys link to the action) for all players in the file fikename in the game repertory,
    /// but don't save the current CustomInput configuration.
    /// Can be load using the methode CustomInput.LoadDefaultConfiguration(string fileName).
    /// </summary>
    public static bool SaveDefaultConfiguration(string fileName)
    {
        if (!Save.ReadJSONData<CustomInputConfigData>(fileName, out CustomInputConfigData i))
            return false;
        CustomInputConfigData CustomInputConfig = new CustomInputConfigData(defaultKBKeys, defaultGB1Keys, defaultGB2Keys, defaultGB3Keys, defaultGB4Keys,
            i.player1Keys, i.player2Keys, i.player3Keys, i.player4Keys, i.player5Keys);
        return Save.WriteJSONData(CustomInputConfig, fileName);
    }

    /// <summary>
    /// Save all the current CustomInput configuration (current actions and controllers keys link to the action) for all players in the file fikename in the game repertory,
    /// but don't save the default CustomInput configuration.
    /// Can be load using the methode CustomInput.LoadCurrentConfiguration(string fileName).
    /// </summary>
    public static bool SaveCurrentConfiguration(string fileName)
    {
        if (!Save.ReadJSONData<CustomInputConfigData>(fileName, out CustomInputConfigData i))
            return false;
        CustomInputConfigData CustomInputConfig = new CustomInputConfigData(i.defaultKBKeys, i.defaultGB1Keys, i.defaultGB2Keys, i.defaultGB3Keys, i.defaultGB4Keys,
            player1Keys, player2Keys, player3Keys, player4Keys, player5Keys);
        return Save.WriteJSONData(CustomInputConfig, fileName);
    }

    /// <summary>
    /// Load from the file Save in the game repertory all the configuration of the CustomInput system.
    /// </summary>
    public static bool LoadConfiguration(string fileName)
    {
        if (!Save.ReadJSONData<CustomInputConfigData>(fileName, out CustomInputConfigData i))
            return false;
        defaultKBKeys = i.defaultKBKeys.Clone();
        defaultGB1Keys = i.defaultGB1Keys.Clone();
        defaultGB2Keys = i.defaultGB2Keys.Clone();
        defaultGB3Keys = i.defaultGB3Keys.Clone();
        defaultGB4Keys = i.defaultGB4Keys.Clone();
        player1Keys = i.player1Keys.Clone();
        player2Keys = i.player2Keys.Clone();
        player3Keys = i.player3Keys.Clone();
        player4Keys = i.player4Keys.Clone();
        player5Keys = i.player5Keys.Clone();
        return true;
    }

    /// <summary>
    /// Load from the file Save in the game repertory the default configuration of the CustomInput system.
    /// </summary>
    public static bool LoadDefaultConfiguration(string fileName)
    {
        if (!Save.ReadJSONData<CustomInputConfigData>(fileName, out CustomInputConfigData i))
            return false;
        defaultKBKeys = i.defaultKBKeys.Clone();
        defaultGB1Keys = i.defaultGB1Keys.Clone();
        defaultGB2Keys = i.defaultGB2Keys.Clone();
        defaultGB3Keys = i.defaultGB3Keys.Clone();
        defaultGB4Keys = i.defaultGB4Keys.Clone();
        return true;
    }
    /// <summary>
    /// Load from the file Save in the game repertory the current configuration of the CustomInput system.
    /// </summary>
    public static bool LoadCurrentConfiguration(string fileName)
    {
        if (!Save.ReadJSONData<CustomInputConfigData>(fileName, out CustomInputConfigData i))
            return false;
        player1Keys = i.player1Keys.Clone();
        player2Keys = i.player2Keys.Clone();
        player3Keys = i.player3Keys.Clone();
        player4Keys = i.player4Keys.Clone();
        player5Keys = i.player5Keys.Clone();
        return true;
    }

    #endregion

    #region Useful region

    /// <param name="direction"> the direction of the mousewheel return by the function </param>
    /// <returns> true during the frame where the mouse wheel is moved.</returns>
    public static bool MouseWheel(out MouseWheelDirection direction)
    {
        if(Input.mouseScrollDelta != Vector2.zero)
        {
            direction = Input.mouseScrollDelta.y > 0f ? MouseWheelDirection.Up : MouseWheelDirection.Down;
            return true;
        }
        direction = MouseWheelDirection.none;
        return false;
    }

    private static string[] negativeKeyToString = new string[15]
    {
        "None",
        "RT",
        "LT",
        "DpadUp",
        "DpadRight",
        "DpadDown",
        "DpadLeft",
        "TBSRUp",
        "TBSRDown",
        "TBSRRight",
        "TBSRLeft",
        "TBSLUp",
        "TBSLDown",
        "TBSLRight",
        "TBSLLeft"
    };

    /// <summary>
    /// Convert a key into a string.
    /// </summary>
    /// <param name="key"> the key to convert to a string</param>
    public static string KeyToString(int key)
    {
        return key >= 0 ? ((KeyCode)key).ToString() : negativeKeyToString[((-(key + 1)) % 14) + 1];
    }

    public static Vector2 mousePosition => Input.mousePosition;
    public static Vector2 mouseScrollDelta => Input.mouseScrollDelta;
    public static bool isAMouseConnected => Input.mousePresent;

    /// <summary>
    /// Convert an action into the string who define the control of the action, according to the controller.
    /// </summary>
    public static string KeyToString(string action, PlayerIndex player)
    {
        switch (player)
        {
            case PlayerIndex.One:
                return KeyToString(player1Keys.GetKey(action));
            case PlayerIndex.Two:
                return KeyToString(player2Keys.GetKey(action));
            case PlayerIndex.Three:
                return KeyToString(player3Keys.GetKey(action));
            case PlayerIndex.Four:
                return KeyToString(player4Keys.GetKey(action));
            case PlayerIndex.Five:
                return KeyToString(player5Keys.GetKey(action));
            case PlayerIndex.All:
                Debug.LogWarning("Cannot convert to string multiples Keys");
                return "";
            default:
                return "";
        }
    }

    /// <param name="key"> the key pressed, castable to an Keys, MouseButton or Buttons according to the controler type</param>
    /// <param name="gamepadIndex"></param>
    /// <returns> true if a key of the controler is pressed this frame, false otherwise </returns>
    public static bool Listen(ControllerType controller, out int key)
    {
        int beg = 0, end = 0;
        switch (controller)
        {
            case ControllerType.Keyboard:
                end = keyCodeInt.GetIndexOf(329);
                for (int i = beg; i <= end; i++)
                {
                    if (Input.GetKeyDown((KeyCode)keyCodeInt[i]))
                    {
                        key = keyCodeInt[i];
                        return true;
                    }
                }
                key = 0;
                return false;
            case ControllerType.Gamepad1:
                beg = keyCodeInt.GetIndexOf(350);
                end = keyCodeInt.GetIndexOf(369);
                for (int i = beg; i <= end; i++)
                {
                    if (Input.GetKeyDown((KeyCode)keyCodeInt[i]))
                    {
                        key = keyCodeInt[i];
                        return true;
                    }
                }
                for (int i = -14; i <= -1; i++)
                {
                    if (GetNegativeKeyCodeDown((NegativeKeyCode)i))
                    {
                        key = i;
                        return true;
                    }
                }

                key = 0;
                return false;
            case ControllerType.Gamepad2:
                beg = keyCodeInt.GetIndexOf(370);
                end = keyCodeInt.GetIndexOf(389);
                for (int i = beg; i <= end; i++)
                {
                    if (Input.GetKeyDown((KeyCode)keyCodeInt[i]))
                    {
                        key = keyCodeInt[i];
                        return true;
                    }
                }
                for (int i = -28; i <= -15; i++)
                {
                    if (GetNegativeKeyCodeDown((NegativeKeyCode)i))
                    {
                        key = i;
                        return true;
                    }
                }
                key = 0;
                return false;
            case ControllerType.Gamepad3:
                beg = keyCodeInt.GetIndexOf(390);
                end = keyCodeInt.GetIndexOf(409);
                for (int i = beg; i <= end; i++)
                {
                    if (Input.GetKeyDown((KeyCode)keyCodeInt[i]))
                    {
                        key = keyCodeInt[i];
                        return true;
                    }
                }
                for (int i = -42; i <= -29; i++)
                {
                    if (GetNegativeKeyCodeDown((NegativeKeyCode)i))
                    {
                        key = i;
                        return true;
                    }
                }
                key = 0;
                return false;
            case ControllerType.Gamepad4:
                beg = keyCodeInt.GetIndexOf(410);
                end = keyCodeInt.GetIndexOf(429);
                for (int i = beg; i <= end; i++)
                {
                    if (Input.GetKeyDown((KeyCode)keyCodeInt[i]))
                    {
                        key = keyCodeInt[i];
                        return true;
                    }
                }
                for (int i = -56; i <= -43; i++)
                {
                    if (GetNegativeKeyCodeDown((NegativeKeyCode)i))
                    {
                        key = i;
                        return true;
                    }
                }

                key = 0;
                return false;
            case ControllerType.GamepadAll:
                beg = keyCodeInt.GetIndexOf(330);
                end = keyCodeInt.GetIndexOf(349);
                for (int i = beg; i <= end; i++)
                {
                    if (Input.GetKeyDown((KeyCode)keyCodeInt[i]))
                    {
                        key = keyCodeInt[i];
                        return true;
                    }
                }
                for (int i = -70; i <= 0; i++)
                {
                    if (GetNegativeKeyCodeDown((NegativeKeyCode)i))
                    {
                        key = i;
                        return true;
                    }
                }
                key = 0;
                return false;
            case ControllerType.All:
                for (int i = 0; i < keyCodeInt.Length; i++)
                {
                    if (Input.GetKeyDown((KeyCode)keyCodeInt[i]))
                    {
                        key = keyCodeInt[i];
                        return true;
                    }
                }
                for (int i = -70; i <= 0; i++)
                {
                    if(GetNegativeKeyCodeDown((NegativeKeyCode)i))
                    {
                        key = i;
                        return true;
                    }
                }
                key = 0;
                return false;
            default:
                key = 0;
                return false;
        }
    }

    public static bool ListenAll(ControllerType controller, out int[] resultKeys)
    {
        List<int> res = new List<int>();
        int beg = 0, end = 0;
        switch (controller)
        {
            case ControllerType.Keyboard:
                end = keyCodeInt.GetIndexOf(329);
                for (int i = beg; i <= end; i++)
                {
                    if (Input.GetKeyDown((KeyCode)keyCodeInt[i]))
                    {
                        res.Add(keyCodeInt[i]);
                    }
                }
                break;
            case ControllerType.Gamepad1:
                beg = keyCodeInt.GetIndexOf(350);
                end = keyCodeInt.GetIndexOf(369);
                for (int i = beg; i <= end; i++)
                {
                    if (Input.GetKeyDown((KeyCode)keyCodeInt[i]))
                    {
                        res.Add(keyCodeInt[i]);
                    }
                }
                for (int i = -14; i <= -1; i++)
                {
                    if (GetNegativeKeyCodeDown((NegativeKeyCode)i))
                    {
                        res.Add(i);
                    }
                }
                break;
            case ControllerType.Gamepad2:
                beg = keyCodeInt.GetIndexOf(370);
                end = keyCodeInt.GetIndexOf(389);
                for (int i = beg; i <= end; i++)
                {
                    if (Input.GetKeyDown((KeyCode)keyCodeInt[i]))
                    {
                        res.Add(keyCodeInt[i]);
                    }
                }
                for (int i = -28; i <= -15; i++)
                    {
                    if (GetNegativeKeyCodeDown((NegativeKeyCode)i))
                    {
                        res.Add(i);
                    }
                }
                break;
            case ControllerType.Gamepad3:
                beg = keyCodeInt.GetIndexOf(390);
                end = keyCodeInt.GetIndexOf(409);
                for (int i = beg; i <= end; i++)
                {
                    if (Input.GetKeyDown((KeyCode)keyCodeInt[i]))
                    {
                        res.Add(keyCodeInt[i]);
                    }
                }
                for (int i = -42; i <= -29; i++)
                {
                    if (GetNegativeKeyCodeDown((NegativeKeyCode)i))
                    {
                        res.Add(i);
                    }
                }
                break;
            case ControllerType.Gamepad4:
                beg = keyCodeInt.GetIndexOf(410);
                end = keyCodeInt.GetIndexOf(429);
                for (int i = beg; i <= end; i++)
                {
                    if (Input.GetKeyDown((KeyCode)keyCodeInt[i]))
                    {
                        res.Add(keyCodeInt[i]);
                    }
                }
                for (int i = -56; i <= -43; i++)
                {
                    if (GetNegativeKeyCodeDown((NegativeKeyCode)i))
                    {
                        res.Add(i);
                    }
                }
                break;
            case ControllerType.GamepadAll:
                beg = keyCodeInt.GetIndexOf(330);
                end = keyCodeInt.GetIndexOf(349);
                for (int i = beg; i <= end; i++)
                {
                    if (Input.GetKeyDown((KeyCode)keyCodeInt[i]))
                    {
                        res.Add(keyCodeInt[i]);
                    }
                }
                for (int i = -70; i <= 0; i++)
                {
                    if (GetNegativeKeyCodeDown((NegativeKeyCode)i))
                    {
                        res.Add(i);
                    }
                }
                break;
            case ControllerType.All:
                for (int i = 0; i < keyCodeInt.Length; i++)
                {
                    if (Input.GetKeyDown((KeyCode)keyCodeInt[i]))
                    {
                        res.Add(keyCodeInt[i]);
                    }
                }
                for (int i = -70; i <= 0; i++)
                {
                    if (GetNegativeKeyCodeDown((NegativeKeyCode)i))
                    {
                        res.Add(i);
                    }
                }
                break;
            default:
                break;
        }
        if (res.Count > 0)
        {
            resultKeys = res.ToArray();
            return true;
        }
        resultKeys = null;
        return false;
    }

    /// <param name="letter"> the letter pressed this frame</param>
    /// <returns>true if a key of the letter of the keyboard controller is pressed this frame, false otherwise</returns>
    public static bool CharPressed(out string letter)
    {
        for (int i = 97; i <= 122; i++)
        {
            if (Input.GetKeyDown((KeyCode)i))
            {
                letter = letters[i - 97];
                if (Input.GetKey(KeyCode.RightShift) || Input.GetKey(KeyCode.LeftShift))
                    letter = letter.ToUpper();
                return true;
            }
        }
        for (int i = 256; i <= 265; i++)
        {
            if (Input.GetKeyDown((KeyCode)i))
            {
                letter = letters[i - 230];
                return true;
            }
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            letter = " ";
            return true;
        }
        letter = "";
        return false;
    }

    /// <param name="number"> the number pressed this frame</param>
    /// <returns>true if a key of the number of the keyboard controller is pressed this frame, false otherwise</returns>
    public static bool NumberPressed(out string number)
    {
        for (int i = 256; i <= 265; i++)
        {
            if (Input.GetKeyDown((KeyCode)i))
            {
                number = letters[i - 230];
                return true;
            }
        }
        number = "";
        return false;
    }

    #endregion

    #region Update

    public static void PreUpdate()
    {
        oldGP1State = newGP1State;
        oldGP2State = newGP2State;
        oldGP3State = newGP3State;
        oldGP4State = newGP4State;
        SetOldGamepadSticksPositions();
        newGP1State = GamePad.GetState(XInputDotNetPure.PlayerIndex.One);
        newGP2State = GamePad.GetState(XInputDotNetPure.PlayerIndex.Two);
        newGP3State = GamePad.GetState(XInputDotNetPure.PlayerIndex.Three);
        newGP4State = GamePad.GetState(XInputDotNetPure.PlayerIndex.Four);
        SetNewGamepadStickPositions();
    }

    #endregion

    #region Exeption

    [Serializable]
    private class InvalidCustomInputActionExeption : Exception
    {
        public InvalidCustomInputActionExeption(string dico, string action) : base("The action : '" + action + "' is not added to the " + dico +  " controler.")
        {

        }
    }

    #endregion

    #region Custom Struct

    [Serializable]
    public struct GeneralInput
    {
        public KeyCode[] keysKeyboard, keyGamepad1, keyGamepad2, keyGamepad3, keyGamepad4;
        public ControllerType controllerType;

        public GeneralInput(KeyCode[] keysKeyboard, KeyCode[] keyGamepad1, KeyCode[] keyGamepad2, KeyCode[] keyGamepad3, KeyCode[] keyGamepad4, ControllerType controllerType)
        {
            this.keysKeyboard = keysKeyboard;
            this.keyGamepad1 = keyGamepad1;
            this.keyGamepad2 = keyGamepad2;
            this.keyGamepad3 = keyGamepad3;
            this.keyGamepad4 = keyGamepad4;
            this.controllerType = controllerType;
        }

        private bool isKeySomething(Func<KeyCode, bool> func)
        {
            switch (controllerType)
            {
                case ControllerType.Keyboard:
                    return GetKeySomething(func, keysKeyboard);
                case ControllerType.Gamepad1:
                    return GetKeySomething(func, keyGamepad1);
                case ControllerType.Gamepad2:
                    return GetKeySomething(func, keyGamepad2);
                case ControllerType.Gamepad3:
                    return GetKeySomething(func, keyGamepad3);
                case ControllerType.Gamepad4:
                    return GetKeySomething(func, keyGamepad4);
                case ControllerType.GamepadAll:
                    return GetKeySomething(func, keyGamepad1) || GetKeySomething(func, keyGamepad2)
                        || GetKeySomething(func, keyGamepad3) || GetKeySomething(func, keyGamepad4);
                case ControllerType.All:
                    return GetKeySomething(func, keysKeyboard) || GetKeySomething(func, keyGamepad1) || GetKeySomething(func, keyGamepad2)
                        || GetKeySomething(func, keyGamepad3) || GetKeySomething(func, keyGamepad4);
                default:
                    return false;
            }

            bool GetKeySomething(Func<KeyCode, bool> func, KeyCode[] keyCodes)
            {
                foreach (KeyCode key in keyCodes)
                {
                    if (func(key))
                        return true;
                }
                return false;
            }
        }

        public bool IsPressedDown() => isKeySomething((KeyCode key) => GetKeyDown(key));
        public bool IsPressedUp() => isKeySomething((KeyCode key) => GetKeyUp(key));
        public bool IsPressed() => isKeySomething((KeyCode key) => GetKey(key));
    }

    #endregion
}
