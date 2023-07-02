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

public enum InputKey
{
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

    None = 0,
    Backspace = 8,
    Tab = 9,
    Clear = 12,
    Return = 13,
    Pause = 19,
    Escape = 27,
    Space = 32,
    Exclaim = 33,
    DoubleQuote = 34,
    Hash = 35,
    Dollar = 36,
    Percent = 37,
    Ampersand = 38,
    Quote = 39,
    LeftParen = 40,
    RightParen = 41,
    Asterisk = 42,
    Plus = 43,
    Comma = 44,
    Minus = 45,
    Period = 46,
    Slash = 47,
    Alpha0 = 48,
    Alpha1 = 49,
    Alpha2 = 50,
    Alpha3 = 51,
    Alpha4 = 52,
    Alpha5 = 53,
    Alpha6 = 54,
    Alpha7 = 55,
    Alpha8 = 56,
    Alpha9 = 57,
    Colon = 58,
    Semicolon = 59,
    Less = 60,
    Equals = 61,
    Greater = 62,
    Question = 63,
    At = 64,
    LeftBracket = 91,
    Backslash = 92,
    RightBracket = 93,
    Caret = 94,
    Underscore = 95,
    BackQuote = 96,
    A = 97,
    B = 98,
    C = 99,
    D = 100,
    E = 101,
    F = 102,
    G = 103,
    H = 104,
    I = 105,
    J = 106,
    K = 107,
    L = 108,
    M = 109,
    N = 110,
    O = 111,
    P = 112,
    Q = 113,
    R = 114,
    S = 115,
    T = 116,
    U = 117,
    V = 118,
    W = 119,
    X = 120,
    Y = 121,
    Z = 122,
    LeftCurlyBracket = 123,
    Pipe = 124,
    RightCurlyBracket = 125,
    Tilde = 126,
    Delete = 127,
    Keypad0 = 256,
    Keypad1 = 257,
    Keypad2 = 258,
    Keypad3 = 259,
    Keypad4 = 260,
    Keypad5 = 261,
    Keypad6 = 262,
    Keypad7 = 263,
    Keypad8 = 264,
    Keypad9 = 265,
    KeypadPeriod = 266,
    KeypadDivide = 267,
    KeypadMultiply = 268,
    KeypadMinus = 269,
    KeypadPlus = 270,
    KeypadEnter = 271,
    KeypadEquals = 272,
    UpArrow = 273,
    DownArrow = 274,
    RightArrow = 275,
    LeftArrow = 276,
    Insert = 277,
    Home = 278,
    End = 279,
    PageUp = 280,
    PageDown = 281,
    F1 = 282,
    F2 = 283,
    F3 = 284,
    F4 = 285,
    F5 = 286,
    F6 = 287,
    F7 = 288,
    F8 = 289,
    F9 = 290,
    F10 = 291,
    F11 = 292,
    F12 = 293,
    F13 = 294,
    F14 = 295,
    F15 = 296,
    Numlock = 300,
    CapsLock = 301,
    ScrollLock = 302,
    RightShift = 303,
    LeftShift = 304,
    RightControl = 305,
    LeftControl = 306,
    RightAlt = 307,
    LeftAlt = 308,
    LeftMeta = 310,
    LeftCommand = 310,
    LeftApple = 310,
    LeftWindows = 311,
    RightMeta = 309,
    RightCommand = 309,
    RightApple = 309,
    RightWindows = 312,
    AltGr = 313,
    Help = 315,
    Print = 316,
    SysReq = 317,
    Break = 318,
    Menu = 319,
    Mouse0 = 323,
    Mouse1 = 324,
    Mouse2 = 325,
    Mouse3 = 326,
    Mouse4 = 327,
    Mouse5 = 328,
    Mouse6 = 329,
    GPButton0 = 330,
    GPButton1 = 331,
    GPButton2 = 332,
    GPButton3 = 333,
    GPButton4 = 334,
    GPButton5 = 335,
    GPButton6 = 336,
    GPButton7 = 337,
    GPButton8 = 338,
    GPButton9 = 339,
    GPButton10 = 340,
    GPButton11 = 341,
    GPButton12 = 342,
    GPButton13 = 343,
    GPButton14 = 344,
    GPButton15 = 345,
    GPButton16 = 346,
    GPButton17 = 347,
    GPButton18 = 348,
    GPButton19 = 349,
    GP1Button0 = 350,
    GP1Button1 = 351,
    GP1Button2 = 352,
    GP1Button3 = 353,
    GP1Button4 = 354,
    GP1Button5 = 355,
    GP1Button6 = 356,
    GP1Button7 = 357,
    GP1Button8 = 358,
    GP1Button9 = 359,
    GP1Button10 = 360,
    GP1Button11 = 361,
    GP1Button12 = 362,
    GP1Button13 = 363,
    GP1Button14 = 364,
    GP1Button15 = 365,
    GP1Button16 = 366,
    GP1Button17 = 367,
    GP1Button18 = 368,
    GP1Button19 = 369,
    GP2Button0 = 370,
    GP2Button1 = 371,
    GP2Button2 = 372,
    GP2Button3 = 373,
    GP2Button4 = 374,
    GP2Button5 = 375,
    GP2Button6 = 376,
    GP2Button7 = 377,
    GP2Button8 = 378,
    GP2Button9 = 379,
    GP2Button10 = 380,
    GP2Button11 = 381,
    GP2Button12 = 382,
    GP2Button13 = 383,
    GP2Button14 = 384,
    GP2Button15 = 385,
    GP2Button16 = 386,
    GP2Button17 = 387,
    GP2Button18 = 388,
    GP2Button19 = 389,
    GP3Button0 = 390,
    GP3Button1 = 391,
    GP3Button2 = 392,
    GP3Button3 = 393,
    GP3Button4 = 394,
    GP3Button5 = 395,
    GP3Button6 = 396,
    GP3Button7 = 397,
    GP3Button8 = 398,
    GP3Button9 = 399,
    GP3Button10 = 400,
    GP3Button11 = 401,
    GP3Button12 = 402,
    GP3Button13 = 403,
    GP3Button14 = 404,
    GP3Button15 = 405,
    GP3Button16 = 406,
    GP3Button17 = 407,
    GP3Button18 = 408,
    GP3Button19 = 409,
    GP4Button0 = 410,
    GP4Button1 = 411,
    GP4Button2 = 412,
    GP4Button3 = 413,
    GP4Button4 = 414,
    GP4Button5 = 415,
    GP4Button6 = 416,
    GP4Button7 = 417,
    GP4Button8 = 418,
    GP4Button9 = 419,
    GP4Button10 = 420,
    GP4Button11 = 421,
    GP4Button12 = 422,
    GP4Button13 = 423,
    GP4Button14 = 424,
    GP4Button15 = 425,
    GP4Button16 = 426,
    GP4Button17 = 427,
    GP4Button18 = 428,
    GP4Button19 = 429,
    GP5Button0 = 430,
    GP5Button1 = 431,
    GP5Button2 = 432,
    GP5Button3 = 433,
    GP5Button4 = 434,
    GP5Button5 = 435,
    GP5Button6 = 436,
    GP5Button7 = 437,
    GP5Button8 = 438,
    GP5Button9 = 439,
    GP5Button10 = 440,
    GP5Button11 = 441,
    GP5Button12 = 442,
    GP5Button13 = 443,
    GP5Button14 = 444,
    GP5Button15 = 445,
    GP5Button16 = 446,
    GP5Button17 = 447,
    GP5Button18 = 448,
    GP5Button19 = 449,
    GP6Button0 = 450,
    GP6Button1 = 451,
    GP6Button2 = 452,
    GP6Button3 = 453,
    GP6Button4 = 454,
    GP6Button5 = 455,
    GP6Button6 = 456,
    GP6Button7 = 457,
    GP6Button8 = 458,
    GP6Button9 = 459,
    GP6Button10 = 460,
    GP6Button11 = 461,
    GP6Button12 = 462,
    GP6Button13 = 463,
    GP6Button14 = 464,
    GP6Button15 = 465,
    GP6Button16 = 466,
    GP6Button17 = 467,
    GP6Button18 = 468,
    GP6Button19 = 469,
    GP7Button0 = 470,
    GP7Button1 = 471,
    GP7Button2 = 472,
    GP7Button3 = 473,
    GP7Button4 = 474,
    GP7Button5 = 475,
    GP7Button6 = 476,
    GP7Button7 = 477,
    GP7Button8 = 478,
    GP7Button9 = 479,
    GP7Button10 = 480,
    GP7Button11 = 481,
    GP7Button12 = 482,
    GP7Button13 = 483,
    GP7Button14 = 484,
    GP7Button15 = 485,
    GP7Button16 = 486,
    GP7Button17 = 487,
    GP7Button18 = 488,
    GP7Button19 = 489,
    GP8Button0 = 490,
    GP8Button1 = 491,
    GP8Button2 = 492,
    GP8Button3 = 493,
    GP8Button4 = 494,
    GP8Button5 = 495,
    GP8Button6 = 496,
    GP8Button7 = 497,
    GP8Button8 = 498,
    GP8Button9 = 499,
    GP8Button10 = 500,
    GP8Button11 = 501,
    GP8Button12 = 502,
    GP8Button13 = 503,
    GP8Button14 = 504,
    GP8Button15 = 505,
    GP8Button16 = 506,
    GP8Button17 = 507,
    GP8Button18 = 508,
    GP8Button19 = 509
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
    private static InputData defaultGPKeys = new InputData();
    private static InputData gpKeys = new InputData();

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
    private class InputData : ICloneable<InputData>
    {
        public List<string> actions = new List<string>();
        public List<int> keys = new List<int>();
        [NonSerialized] public Dictionary<string, int> controlsDic = new Dictionary<string, int>();

        public InputData() { }

        public InputData(List<string> actions, List<int> keys)
        {
            this.actions = actions;
            this.keys = keys;
            Build();
        }

        public void Build()
        {
            if (actions.Count != keys.Count)
                return;

            controlsDic = new Dictionary<string, int>();
            for (int i = 0; i < actions.Count; i++)
            {
                controlsDic.Add(actions[i], keys[i]);
            }
            actions.Clear();
            keys.Clear();
        }

        public void PrepareSerialization()
        {
            actions = new List<string>();
            keys = new List<int>();

            foreach (KeyValuePair<string, int> item in controlsDic)
            {
                actions.Add(item.Key);
                keys.Add(item.Value);
            }
        }

        public void AddAction(string action, int key)
        {
            if (controlsDic.ContainsKey(action))
                ReplaceAction(action, key);
            else
            {
                controlsDic.Add(action, key);
            }
        }

        public bool RemoveAction(string action)
        {
            if(controlsDic.ContainsKey(action))
            {
                controlsDic.Remove(action);
                return true;
            }
            return false;
        }

        public bool ReplaceAction(string action, int key)
        {
            if (actions.Contains(action))
            {
                controlsDic[action] = key;
                return true;
            }
            return false;
        }

        public bool Contain(string action) => controlsDic.ContainsKey(action);

        public int GetKey(string action)
        {
            if(controlsDic.TryGetValue(action, out int key))
                return key;
            return 0;
        }

        public InputData Clone() => new InputData(actions, keys); 
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

    public static Vector2 GetGamepadStickPosition(ControllerType gamepadIndex, GamepadStick GamepadStick)
    {
        switch (gamepadIndex)
        {
            case ControllerType.Keyboard:
                Debug.LogWarning("Can't return the stick position of a keyboard!");
                return Vector2.zero;
            case ControllerType.Gamepad1:
                return GamepadStick == GamepadStick.right ? newGP1RightStickPosition : newGP1LeftStickPosition;
            case ControllerType.Gamepad2:
                return GamepadStick == GamepadStick.right ? newGP2RightStickPosition : newGP2LeftStickPosition;
            case ControllerType.Gamepad3:
                return GamepadStick == GamepadStick.right ? newGP3RightStickPosition : newGP3LeftStickPosition;
            case ControllerType.Gamepad4:
                return GamepadStick == GamepadStick.right ? newGP4RightStickPosition : newGP4LeftStickPosition;
            case ControllerType.GamepadAll:
                if (newGP1State.IsConnected)
                    return GamepadStick == GamepadStick.right ? newGP1RightStickPosition : newGP1LeftStickPosition;
                if (newGP2State.IsConnected)
                    return GamepadStick == GamepadStick.right ? newGP2RightStickPosition : newGP2LeftStickPosition;
                if (newGP3State.IsConnected)
                    return GamepadStick == GamepadStick.right ? newGP3RightStickPosition : newGP3LeftStickPosition;
                if (newGP4State.IsConnected)
                    return GamepadStick == GamepadStick.right ? newGP4RightStickPosition : newGP4LeftStickPosition;
                Debug.LogWarning("No GamePad is connected");
                return Vector2.zero;
            case ControllerType.All:
                if (newGP1State.IsConnected)
                    return GamepadStick == GamepadStick.right ? newGP1RightStickPosition : newGP1LeftStickPosition;
                if (newGP2State.IsConnected)
                    return GamepadStick == GamepadStick.right ? newGP2RightStickPosition : newGP2LeftStickPosition;
                if (newGP3State.IsConnected)
                    return GamepadStick == GamepadStick.right ? newGP3RightStickPosition : newGP3LeftStickPosition;
                if (newGP4State.IsConnected)
                    return GamepadStick == GamepadStick.right ? newGP4RightStickPosition : newGP4LeftStickPosition;
                Debug.LogWarning("No GamePad is connected");
                return Vector2.zero;
            default:
                return Vector2.zero;
        }
    }

    #region GetGamepadStickUp/Down/Right/Left

    /// <summary>
    /// 
    /// </summary>
    /// <param name="controllerType"></param>
    /// <param name="GamepadStick"></param>
    /// <returns>true the frame when the gamepad stick reach the top (y value became 1f)</returns>
    public static bool GetGamepadStickUp(ControllerType controllerType, GamepadStick GamepadStick)
    {
        switch (controllerType)
        {
            case ControllerType.Keyboard:
                Debug.LogWarning("A keyboard does'nt have stick!");
                return false;
            case ControllerType.Gamepad1:
                return GamepadStick == GamepadStick.right ? oldGP1RightStickPosition.y < GP1RightThumbStickDeadZone.y && newGP1RightStickPosition.y >= GP1RightThumbStickDeadZone.y :
                    oldGP1LeftStickPosition.y < GP1LeftThumbStickDeadZone.y && newGP1LeftStickPosition.y >= GP1LeftThumbStickDeadZone.y;
            case ControllerType.Gamepad2:
                return GamepadStick == GamepadStick.right ? oldGP2RightStickPosition.y < GP2RightThumbStickDeadZone.y && newGP2RightStickPosition.y >= GP2RightThumbStickDeadZone.y :
                    oldGP2LeftStickPosition.y < GP2LeftThumbStickDeadZone.y && newGP2LeftStickPosition.y >= GP2LeftThumbStickDeadZone.y;
            case ControllerType.Gamepad3:
                return GamepadStick == GamepadStick.right ? oldGP3RightStickPosition.y < GP3RightThumbStickDeadZone.y && newGP3RightStickPosition.y >= GP3RightThumbStickDeadZone.y :
                    oldGP3LeftStickPosition.y < GP3LeftThumbStickDeadZone.y && newGP3LeftStickPosition.y >= GP3LeftThumbStickDeadZone.y;
            case ControllerType.Gamepad4:
                return GamepadStick == GamepadStick.right ? oldGP4RightStickPosition.y < GP4RightThumbStickDeadZone.y && newGP1RightStickPosition.y >= GP1RightThumbStickDeadZone.y :
                    oldGP1LeftStickPosition.y < GP1LeftThumbStickDeadZone.y && newGP1LeftStickPosition.y >= GP1LeftThumbStickDeadZone.y;
            case ControllerType.GamepadAll:
                if(GamePadIsConnected(ControllerType.Gamepad1))
                    return GamepadStick == GamepadStick.right ? oldGP1RightStickPosition.y < GP1RightThumbStickDeadZone.y && newGP1RightStickPosition.y >= GP1RightThumbStickDeadZone.y :
                    oldGP1LeftStickPosition.y < GP1LeftThumbStickDeadZone.y && newGP1LeftStickPosition.y >= GP1LeftThumbStickDeadZone.y;
                if(GamePadIsConnected(ControllerType.Gamepad2))
                    return GamepadStick == GamepadStick.right ? oldGP2RightStickPosition.y < GP2RightThumbStickDeadZone.y && newGP2RightStickPosition.y >= GP2RightThumbStickDeadZone.y :
                    oldGP2LeftStickPosition.y < GP2LeftThumbStickDeadZone.y && newGP2LeftStickPosition.y >= GP2LeftThumbStickDeadZone.y;
                if(GamePadIsConnected(ControllerType.Gamepad3))
                    return GamepadStick == GamepadStick.right ? oldGP3RightStickPosition.y < GP3RightThumbStickDeadZone.y && newGP3RightStickPosition.y >= GP3RightThumbStickDeadZone.y :
                    oldGP3LeftStickPosition.y < GP3LeftThumbStickDeadZone.y && newGP3LeftStickPosition.y >= GP3LeftThumbStickDeadZone.y;
                if(GamePadIsConnected(ControllerType.Gamepad4))
                    return GamepadStick == GamepadStick.right ? oldGP4RightStickPosition.y < GP4RightThumbStickDeadZone.y && newGP1RightStickPosition.y >= GP1RightThumbStickDeadZone.y :
                    oldGP1LeftStickPosition.y < GP1LeftThumbStickDeadZone.y && newGP1LeftStickPosition.y >= GP1LeftThumbStickDeadZone.y;
                Debug.LogWarning("No Gamepad is connected!");
                return false;
            case ControllerType.All:
                if (GamePadIsConnected(ControllerType.Gamepad1))
                    return GamepadStick == GamepadStick.right ? oldGP1RightStickPosition.y < GP1RightThumbStickDeadZone.y && newGP1RightStickPosition.y >= GP1RightThumbStickDeadZone.y :
                    oldGP1LeftStickPosition.y < GP1LeftThumbStickDeadZone.y && newGP1LeftStickPosition.y >= GP1LeftThumbStickDeadZone.y;
                if (GamePadIsConnected(ControllerType.Gamepad2))
                    return GamepadStick == GamepadStick.right ? oldGP2RightStickPosition.y < GP2RightThumbStickDeadZone.y && newGP2RightStickPosition.y >= GP2RightThumbStickDeadZone.y :
                    oldGP2LeftStickPosition.y < GP2LeftThumbStickDeadZone.y && newGP2LeftStickPosition.y >= GP2LeftThumbStickDeadZone.y;
                if (GamePadIsConnected(ControllerType.Gamepad3))
                    return GamepadStick == GamepadStick.right ? oldGP3RightStickPosition.y < GP3RightThumbStickDeadZone.y && newGP3RightStickPosition.y >= GP3RightThumbStickDeadZone.y :
                    oldGP3LeftStickPosition.y < GP3LeftThumbStickDeadZone.y && newGP3LeftStickPosition.y >= GP3LeftThumbStickDeadZone.y;
                if (GamePadIsConnected(ControllerType.Gamepad4))
                    return GamepadStick == GamepadStick.right ? oldGP4RightStickPosition.y < GP4RightThumbStickDeadZone.y && newGP1RightStickPosition.y >= GP1RightThumbStickDeadZone.y :
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
    /// <param name="GamepadStick"></param>
    /// <returns>true the frame when the gamepad stick reach the bottom (y value became -1f)</returns>
    public static bool GetGamepadStickDown(ControllerType controllerType, GamepadStick GamepadStick)
    {
        switch (controllerType)
        {
            case ControllerType.Keyboard:
                Debug.LogWarning("A keyboard does'nt have stick!");
                return false;
            case ControllerType.Gamepad1:
                return GamepadStick == GamepadStick.right ? oldGP1RightStickPosition.y > -GP1RightThumbStickDeadZone.y && newGP1RightStickPosition.y <= -GP1RightThumbStickDeadZone.y :
                    oldGP1LeftStickPosition.y > -GP1LeftThumbStickDeadZone.y && newGP1LeftStickPosition.y <= -GP1LeftThumbStickDeadZone.y;
            case ControllerType.Gamepad2:
                return GamepadStick == GamepadStick.right ? oldGP2RightStickPosition.y > -GP2RightThumbStickDeadZone.y && newGP2RightStickPosition.y <= -GP2RightThumbStickDeadZone.y :
                    oldGP2LeftStickPosition.y > -GP2LeftThumbStickDeadZone.y && newGP2LeftStickPosition.y <= -GP2LeftThumbStickDeadZone.y;
            case ControllerType.Gamepad3:
                return GamepadStick == GamepadStick.right ? oldGP3RightStickPosition.y > -GP3RightThumbStickDeadZone.y && newGP3RightStickPosition.y <= -GP3RightThumbStickDeadZone.y :
                    oldGP3LeftStickPosition.y > -GP3LeftThumbStickDeadZone.y && newGP3LeftStickPosition.y <= -GP3LeftThumbStickDeadZone.y;
            case ControllerType.Gamepad4:
                return GamepadStick == GamepadStick.right ? oldGP4RightStickPosition.y > -GP4RightThumbStickDeadZone.y && newGP4RightStickPosition.y <= -GP4RightThumbStickDeadZone.y :
                    oldGP4LeftStickPosition.y > -GP4LeftThumbStickDeadZone.y && newGP4LeftStickPosition.y <= -GP4LeftThumbStickDeadZone.y;
            case ControllerType.GamepadAll:
                if (GamePadIsConnected(ControllerType.Gamepad1))
                    return GamepadStick == GamepadStick.right ? oldGP1RightStickPosition.y > -GP1RightThumbStickDeadZone.y && newGP1RightStickPosition.y <= -GP1RightThumbStickDeadZone.y :
                        oldGP1LeftStickPosition.y > -GP1LeftThumbStickDeadZone.y && newGP1LeftStickPosition.y <= -GP1LeftThumbStickDeadZone.y;
                if (GamePadIsConnected(ControllerType.Gamepad2))
                    return GamepadStick == GamepadStick.right ? oldGP2RightStickPosition.y > -GP2RightThumbStickDeadZone.y && newGP2RightStickPosition.y <= -GP2RightThumbStickDeadZone.y :
                        oldGP2LeftStickPosition.y > -GP2LeftThumbStickDeadZone.y && newGP2LeftStickPosition.y <= -GP2LeftThumbStickDeadZone.y;
                if (GamePadIsConnected(ControllerType.Gamepad3))
                    return GamepadStick == GamepadStick.right ? oldGP3RightStickPosition.y > -GP3RightThumbStickDeadZone.y && newGP3RightStickPosition.y <= -GP3RightThumbStickDeadZone.y :
                        oldGP3LeftStickPosition.y > -GP3LeftThumbStickDeadZone.y && newGP3LeftStickPosition.y <= -GP3LeftThumbStickDeadZone.y;
                if (GamePadIsConnected(ControllerType.Gamepad4))
                    return GamepadStick == GamepadStick.right ? oldGP4RightStickPosition.y > -GP4RightThumbStickDeadZone.y && newGP4RightStickPosition.y <= -GP4RightThumbStickDeadZone.y :
                        oldGP4LeftStickPosition.y > -GP4LeftThumbStickDeadZone.y && newGP4LeftStickPosition.y <= -GP4LeftThumbStickDeadZone.y;
                Debug.LogWarning("No Gamepad is connected!");
                return false;
            case ControllerType.All:
                if (GamePadIsConnected(ControllerType.Gamepad1))
                    return GamepadStick == GamepadStick.right ? oldGP1RightStickPosition.y > -GP1RightThumbStickDeadZone.y && newGP1RightStickPosition.y <= -GP1RightThumbStickDeadZone.y :
                        oldGP1LeftStickPosition.y > -GP1LeftThumbStickDeadZone.y && newGP1LeftStickPosition.y <= -GP1LeftThumbStickDeadZone.y;
                if (GamePadIsConnected(ControllerType.Gamepad2))
                    return GamepadStick == GamepadStick.right ? oldGP2RightStickPosition.y > -GP2RightThumbStickDeadZone.y && newGP2RightStickPosition.y <= -GP2RightThumbStickDeadZone.y :
                        oldGP2LeftStickPosition.y > -GP2LeftThumbStickDeadZone.y && newGP2LeftStickPosition.y <= -GP2LeftThumbStickDeadZone.y;
                if (GamePadIsConnected(ControllerType.Gamepad3))
                    return GamepadStick == GamepadStick.right ? oldGP3RightStickPosition.y > -GP3RightThumbStickDeadZone.y && newGP3RightStickPosition.y <= -GP3RightThumbStickDeadZone.y :
                        oldGP3LeftStickPosition.y > -GP3LeftThumbStickDeadZone.y && newGP3LeftStickPosition.y <= -GP3LeftThumbStickDeadZone.y;
                if (GamePadIsConnected(ControllerType.Gamepad4))
                    return GamepadStick == GamepadStick.right ? oldGP4RightStickPosition.y > -GP4RightThumbStickDeadZone.y && newGP4RightStickPosition.y <= -GP4RightThumbStickDeadZone.y :
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
    /// <param name="GamepadStick"></param>
    /// <returns>true the frame when the gamepad stick reach the right (x value became 1f)</returns>
    public static bool GetGamepadStickRight(ControllerType controllerType, GamepadStick GamepadStick)
    {
        switch (controllerType)
        {
            case ControllerType.Keyboard:
                Debug.LogWarning("A keyboard does'nt have stick!");
                return false;
            case ControllerType.Gamepad1:
                return GamepadStick == GamepadStick.right ? oldGP1RightStickPosition.x < GP1RightThumbStickDeadZone.y && newGP1RightStickPosition.x >= GP1RightThumbStickDeadZone.y :
                    oldGP1LeftStickPosition.x < GP1LeftThumbStickDeadZone.y && newGP1LeftStickPosition.x >= GP1LeftThumbStickDeadZone.y;
            case ControllerType.Gamepad2:
                return GamepadStick == GamepadStick.right ? oldGP2RightStickPosition.x < GP2RightThumbStickDeadZone.y && newGP2RightStickPosition.x >= GP2RightThumbStickDeadZone.y :
                    oldGP2LeftStickPosition.x < GP2LeftThumbStickDeadZone.y && newGP2LeftStickPosition.x >= GP2LeftThumbStickDeadZone.y;
            case ControllerType.Gamepad3:
                return GamepadStick == GamepadStick.right ? oldGP3RightStickPosition.x < GP3RightThumbStickDeadZone.y && newGP3RightStickPosition.x >= GP3RightThumbStickDeadZone.y :
                    oldGP3LeftStickPosition.x < GP3LeftThumbStickDeadZone.y && newGP3LeftStickPosition.x >= GP3LeftThumbStickDeadZone.y;
            case ControllerType.Gamepad4:
                return GamepadStick == GamepadStick.right ? oldGP4RightStickPosition.x < GP4RightThumbStickDeadZone.y && newGP4RightStickPosition.x >= GP4RightThumbStickDeadZone.y :
                    oldGP4LeftStickPosition.x < GP4LeftThumbStickDeadZone.y && newGP4LeftStickPosition.x >= GP4LeftThumbStickDeadZone.y;
            case ControllerType.GamepadAll:
                if (GamePadIsConnected(ControllerType.Gamepad1))
                    return GamepadStick == GamepadStick.right ? oldGP1RightStickPosition.x < GP1RightThumbStickDeadZone.y && newGP1RightStickPosition.x >= GP1RightThumbStickDeadZone.y :
                        oldGP1LeftStickPosition.x < GP1LeftThumbStickDeadZone.y && newGP1LeftStickPosition.x >= GP1LeftThumbStickDeadZone.y;
                if (GamePadIsConnected(ControllerType.Gamepad2))
                    return GamepadStick == GamepadStick.right ? oldGP2RightStickPosition.x < GP2RightThumbStickDeadZone.y && newGP2RightStickPosition.x >= GP2RightThumbStickDeadZone.y :
                        oldGP2LeftStickPosition.x < GP2LeftThumbStickDeadZone.y && newGP2LeftStickPosition.x >= GP2LeftThumbStickDeadZone.y;
                if (GamePadIsConnected(ControllerType.Gamepad3))
                    return GamepadStick == GamepadStick.right ? oldGP3RightStickPosition.x < GP3RightThumbStickDeadZone.y && newGP3RightStickPosition.x >= GP3RightThumbStickDeadZone.y :
                        oldGP3LeftStickPosition.x < GP3LeftThumbStickDeadZone.y && newGP3LeftStickPosition.x >= GP3LeftThumbStickDeadZone.y;
                if (GamePadIsConnected(ControllerType.Gamepad4))
                    return GamepadStick == GamepadStick.right ? oldGP4RightStickPosition.x < GP4RightThumbStickDeadZone.y && newGP4RightStickPosition.x >= GP4RightThumbStickDeadZone.y :
                        oldGP4LeftStickPosition.x < GP4LeftThumbStickDeadZone.y && newGP4LeftStickPosition.x >= GP4LeftThumbStickDeadZone.y;
                Debug.LogWarning("No Gamepad is connected!");
                return false;
            case ControllerType.All:
                if (GamePadIsConnected(ControllerType.Gamepad1))
                    return GamepadStick == GamepadStick.right ? oldGP1RightStickPosition.x < GP1RightThumbStickDeadZone.y && newGP1RightStickPosition.x >= GP1RightThumbStickDeadZone.y :
                        oldGP1LeftStickPosition.x < GP1LeftThumbStickDeadZone.y && newGP1LeftStickPosition.x >= GP1LeftThumbStickDeadZone.y;
                if (GamePadIsConnected(ControllerType.Gamepad2))
                    return GamepadStick == GamepadStick.right ? oldGP2RightStickPosition.x < GP2RightThumbStickDeadZone.y && newGP2RightStickPosition.x >= GP2RightThumbStickDeadZone.y :
                        oldGP2LeftStickPosition.x < GP2LeftThumbStickDeadZone.y && newGP2LeftStickPosition.x >= GP2LeftThumbStickDeadZone.y;
                if (GamePadIsConnected(ControllerType.Gamepad3))
                    return GamepadStick == GamepadStick.right ? oldGP3RightStickPosition.x < GP3RightThumbStickDeadZone.y && newGP3RightStickPosition.x >= GP3RightThumbStickDeadZone.y :
                        oldGP3LeftStickPosition.x < GP3LeftThumbStickDeadZone.y && newGP3LeftStickPosition.x >= GP3LeftThumbStickDeadZone.y;
                if (GamePadIsConnected(ControllerType.Gamepad4))
                    return GamepadStick == GamepadStick.right ? oldGP4RightStickPosition.x < GP4RightThumbStickDeadZone.y && newGP4RightStickPosition.x >= GP4RightThumbStickDeadZone.y :
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
    /// <param name="GamepadStick"></param>
    /// <returns>true the frame when the gamepad stick reach the right (x value became 1f)</returns>
    public static bool GetGamepadStickLeft(ControllerType controllerType, GamepadStick GamepadStick)
    {
        switch (controllerType)
        {
            case ControllerType.Keyboard:
                Debug.LogWarning("A keyboard does'nt have stick!");
                return false;
            case ControllerType.Gamepad1:
                return GamepadStick == GamepadStick.right ? oldGP1RightStickPosition.x > -GP1RightThumbStickDeadZone.y && newGP1RightStickPosition.x <= -GP1RightThumbStickDeadZone.y :
                    oldGP1LeftStickPosition.x > -GP1LeftThumbStickDeadZone.y && newGP1LeftStickPosition.x <= -GP1LeftThumbStickDeadZone.y;
            case ControllerType.Gamepad2:
                return GamepadStick == GamepadStick.right ? oldGP2RightStickPosition.x > -GP2RightThumbStickDeadZone.y && newGP2RightStickPosition.x <= -GP2RightThumbStickDeadZone.y :
                    oldGP2LeftStickPosition.x > -GP2LeftThumbStickDeadZone.y && newGP2LeftStickPosition.x <= -GP2LeftThumbStickDeadZone.y;
            case ControllerType.Gamepad3:
                return GamepadStick == GamepadStick.right ? oldGP3RightStickPosition.x > -GP3RightThumbStickDeadZone.y && newGP3RightStickPosition.x <= -GP3RightThumbStickDeadZone.y :
                    oldGP3LeftStickPosition.x > -GP3LeftThumbStickDeadZone.y && newGP3LeftStickPosition.x <= -GP3LeftThumbStickDeadZone.y;
            case ControllerType.Gamepad4:
                return GamepadStick == GamepadStick.right ? oldGP4RightStickPosition.x > -GP4RightThumbStickDeadZone.y && newGP4RightStickPosition.x <= -GP4RightThumbStickDeadZone.y :
                    oldGP4LeftStickPosition.x > -GP4LeftThumbStickDeadZone.y && newGP4LeftStickPosition.x <= -GP4LeftThumbStickDeadZone.y;
            case ControllerType.GamepadAll:
                if (GamePadIsConnected(ControllerType.Gamepad1))
                    return GamepadStick == GamepadStick.right ? oldGP1RightStickPosition.x > -GP1RightThumbStickDeadZone.y && newGP1RightStickPosition.x <= -GP1RightThumbStickDeadZone.y :
                        oldGP1LeftStickPosition.x > -GP1LeftThumbStickDeadZone.y && newGP1LeftStickPosition.x <= -GP1LeftThumbStickDeadZone.y;
                if (GamePadIsConnected(ControllerType.Gamepad2))
                    return GamepadStick == GamepadStick.right ? oldGP2RightStickPosition.x > -GP2RightThumbStickDeadZone.y && newGP2RightStickPosition.x <= -GP2RightThumbStickDeadZone.y :
                        oldGP2LeftStickPosition.x > -GP2LeftThumbStickDeadZone.y && newGP2LeftStickPosition.x <= -GP2LeftThumbStickDeadZone.y;
                if (GamePadIsConnected(ControllerType.Gamepad3))
                    return GamepadStick == GamepadStick.right ? oldGP3RightStickPosition.x > -GP3RightThumbStickDeadZone.y && newGP3RightStickPosition.x <= -GP3RightThumbStickDeadZone.y :
                        oldGP3LeftStickPosition.x > -GP3LeftThumbStickDeadZone.y && newGP3LeftStickPosition.x <= -GP3LeftThumbStickDeadZone.y;
                if (GamePadIsConnected(ControllerType.Gamepad4))
                    return GamepadStick == GamepadStick.right ? oldGP4RightStickPosition.x > -GP4RightThumbStickDeadZone.y && newGP4RightStickPosition.x <= -GP4RightThumbStickDeadZone.y :
                        oldGP4LeftStickPosition.x > -GP4LeftThumbStickDeadZone.y && newGP4LeftStickPosition.x <= -GP4LeftThumbStickDeadZone.y;
                Debug.LogWarning("No Gamepad is connected!");
                return false;
            case ControllerType.All:
                if (GamePadIsConnected(ControllerType.Gamepad1))
                    return GamepadStick == GamepadStick.right ? oldGP1RightStickPosition.x > -GP1RightThumbStickDeadZone.y && newGP1RightStickPosition.x <= -GP1RightThumbStickDeadZone.y :
                        oldGP1LeftStickPosition.x > -GP1LeftThumbStickDeadZone.y && newGP1LeftStickPosition.x <= -GP1LeftThumbStickDeadZone.y;
                if (GamePadIsConnected(ControllerType.Gamepad2))
                    return GamepadStick == GamepadStick.right ? oldGP2RightStickPosition.x > -GP2RightThumbStickDeadZone.y && newGP2RightStickPosition.x <= -GP2RightThumbStickDeadZone.y :
                        oldGP2LeftStickPosition.x > -GP2LeftThumbStickDeadZone.y && newGP2LeftStickPosition.x <= -GP2LeftThumbStickDeadZone.y;
                if (GamePadIsConnected(ControllerType.Gamepad3))
                    return GamepadStick == GamepadStick.right ? oldGP3RightStickPosition.x > -GP3RightThumbStickDeadZone.y && newGP3RightStickPosition.x <= -GP3RightThumbStickDeadZone.y :
                        oldGP3LeftStickPosition.x > -GP3LeftThumbStickDeadZone.y && newGP3LeftStickPosition.x <= -GP3LeftThumbStickDeadZone.y;
                if (GamePadIsConnected(ControllerType.Gamepad4))
                    return GamepadStick == GamepadStick.right ? oldGP4RightStickPosition.x > -GP4RightThumbStickDeadZone.y && newGP4RightStickPosition.x <= -GP4RightThumbStickDeadZone.y :
                        oldGP4LeftStickPosition.x > -GP4LeftThumbStickDeadZone.y && newGP4LeftStickPosition.x <= -GP4LeftThumbStickDeadZone.y;
                Debug.LogWarning("No Gamepad is connected!");
                return false;
            default:
                return false;
        }
    }

    #endregion

    public static float GetGamepadTrigger(ControllerType controllerType, GamepadTrigger gamepadTrigger)
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
    public static bool GamePadIsConnected(ControllerType gamepadIndex)
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

    private static bool GetNegativeKeyDown(int key)
    {
        return GetNegativeKeyCodeDownDelegate[-key].Invoke();
    }

    private static bool GetNegativeKeyUp(int key)
    {
        return GetNegativeKeyCodeUpDelegate[-key].Invoke();
    }

    private static bool GetNegativeKeyPressed(int key)
    {
        return GetNegativeKeyCodePressedDelegate[-key].Invoke();
    }

    /// <returns> true during the frame when the key assigned with the action is pressed</returns>
    public static bool GetKeyDown(string action, PlayerIndex player = PlayerIndex.All)
    {
        int key;
        switch (player)
        {
            case PlayerIndex.One:
                key = player1Keys.GetKey(action);
                return key >= 0 ? Input.GetKeyDown((KeyCode)key) : GetNegativeKeyDown(key);
            case PlayerIndex.Two:
                key = player2Keys.GetKey(action);
                return key >= 0 ? Input.GetKeyDown((KeyCode)key) : GetNegativeKeyDown(key);
            case PlayerIndex.Three:
                key = player3Keys.GetKey(action);
                return key >= 0 ? Input.GetKeyDown((KeyCode)key) : GetNegativeKeyDown(key);
            case PlayerIndex.Four:
                key = player4Keys.GetKey(action);
                return key >= 0 ? Input.GetKeyDown((KeyCode)key) : GetNegativeKeyDown(key);
            case PlayerIndex.Five:
                key = player5Keys.GetKey(action);
                return key >= 0 ? Input.GetKeyDown((KeyCode)key) : GetNegativeKeyDown(key);
            case PlayerIndex.All:
                int key1 = player1Keys.GetKey(action);
                int key2 = player2Keys.GetKey(action);
                int key3 = player3Keys.GetKey(action);
                int key4 = player4Keys.GetKey(action);
                int key5 = player5Keys.GetKey(action);
                return key1 >= 0 ? Input.GetKeyDown((KeyCode)key1) : GetNegativeKeyDown(key1) ||
                    key2 >= 0 ? Input.GetKeyDown((KeyCode)key2) : GetNegativeKeyDown(key2) ||
                    key3 >= 0 ? Input.GetKeyDown((KeyCode)key3) : GetNegativeKeyDown(key3) ||
                    key4 >= 0 ? Input.GetKeyDown((KeyCode)key4) : GetNegativeKeyDown(key4) ||
                    key5 >= 0 ? Input.GetKeyDown((KeyCode)key5) : GetNegativeKeyDown(key5); 
            default:
                return false;
        }
    }

    /// <returns> true during the frame when the key assigned with the action is pressed</returns>
    public static bool GetKeyDown(string action, PlayerIndex player, out PlayerIndex playerWhoPressesDown)
    {
        playerWhoPressesDown = player;
        int key;
        switch (player)
        {
            case PlayerIndex.One:
                key = player1Keys.GetKey(action);
                return key >= 0 ? Input.GetKeyDown((KeyCode)key) : GetNegativeKeyDown(key);
            case PlayerIndex.Two:
                key = player2Keys.GetKey(action);
                return key >= 0 ? Input.GetKeyDown((KeyCode)key) : GetNegativeKeyDown(key);
            case PlayerIndex.Three:
                key = player3Keys.GetKey(action);
                return key >= 0 ? Input.GetKeyDown((KeyCode)key) : GetNegativeKeyDown(key);
            case PlayerIndex.Four:
                key = player4Keys.GetKey(action);
                return key >= 0 ? Input.GetKeyDown((KeyCode)key) : GetNegativeKeyDown(key);
            case PlayerIndex.Five:
                key = player5Keys.GetKey(action);
                return key >= 0 ? Input.GetKeyDown((KeyCode)key) : GetNegativeKeyDown(key);
            case PlayerIndex.All:
                key = player1Keys.GetKey(action);
                if (key >= 0 ? Input.GetKeyDown((KeyCode)key) : GetNegativeKeyDown(key))
                {
                    playerWhoPressesDown = PlayerIndex.One;
                    return true;
                }
                key = player2Keys.GetKey(action);
                if (key >= 0 ? Input.GetKeyDown((KeyCode)key) : GetNegativeKeyDown(key))
                {
                    playerWhoPressesDown = PlayerIndex.Two;
                    return true;
                }
                key = player3Keys.GetKey(action);
                if (key >= 0 ? Input.GetKeyDown((KeyCode)key) : GetNegativeKeyDown(key))
                {
                    playerWhoPressesDown = PlayerIndex.Three;
                    return true;
                }
                key = player4Keys.GetKey(action);
                if (key >= 0 ? Input.GetKeyDown((KeyCode)key) : GetNegativeKeyDown(key))
                {
                    playerWhoPressesDown = PlayerIndex.Four;
                    return true;
                }
                key = player5Keys.GetKey(action);
                if (key >= 0 ? Input.GetKeyDown((KeyCode)key) : GetNegativeKeyDown(key))
                {
                    playerWhoPressesDown = PlayerIndex.Five;
                    return true;
                }
                return false;
            default:
                return false;
        }
    }

    public static bool GetKeyDown(string action, ControllerType controllerType)
    {
        if(controllerType == ControllerType.Keyboard)
            return GetKeyDown(kbKeys.GetKey(action));
        if (controllerType == ControllerType.All)
            return GetKeyDown(kbKeys.GetKey(action)) || GetKeyDown(gpKeys.GetKey(action));
        return GetKeyDown(gpKeys.GetKey(action));
    }

    /// <returns> true during the frame when the key assigned with the action is unpressed</returns>
    public static bool GetKeyUp(string action, PlayerIndex player = PlayerIndex.All)
    {
        int key;
        switch (player)
        {
            case PlayerIndex.One:
                key = player1Keys.GetKey(action);
                return key >= 0 ? Input.GetKeyUp((KeyCode)key) : GetNegativeKeyUp(key);
            case PlayerIndex.Two:
                key = player2Keys.GetKey(action);
                return key >= 0 ? Input.GetKeyUp((KeyCode)key) : GetNegativeKeyUp(key);
            case PlayerIndex.Three:
                key = player3Keys.GetKey(action);
                return key >= 0 ? Input.GetKeyUp((KeyCode)key) : GetNegativeKeyUp(key);
            case PlayerIndex.Four:
                key = player4Keys.GetKey(action);
                return key >= 0 ? Input.GetKeyUp((KeyCode)key) : GetNegativeKeyUp(key);
            case PlayerIndex.Five:
                key = player5Keys.GetKey(action);
                return key >= 0 ? Input.GetKeyUp((KeyCode)key) : GetNegativeKeyUp(key);
            case PlayerIndex.All:
                int key1 = player1Keys.GetKey(action);
                int key2 = player2Keys.GetKey(action);
                int key3 = player3Keys.GetKey(action);
                int key4 = player4Keys.GetKey(action);
                int key5 = player5Keys.GetKey(action);
                return key1 >= 0 ? Input.GetKeyUp((KeyCode)key1) : GetNegativeKeyUp(key1) ||
                    key2 >= 0 ? Input.GetKeyUp((KeyCode)key2) : GetNegativeKeyUp(key2) ||
                    key3 >= 0 ? Input.GetKeyUp((KeyCode)key3) : GetNegativeKeyUp(key3) ||
                    key4 >= 0 ? Input.GetKeyUp((KeyCode)key4) : GetNegativeKeyUp(key4) ||
                    key5 >= 0 ? Input.GetKeyUp((KeyCode)key5) : GetNegativeKeyUp(key5);
            default:
                return false;
        }
    }

    /// <returns> true during the frame when the key assigned with the action is pressed up</returns>
    public static bool GetKeyUp(string action, PlayerIndex player, out PlayerIndex playerWhoPressesUp)
    {
        playerWhoPressesUp = player;
        int key;
        switch (player)
        {
            case PlayerIndex.One:
                key = player1Keys.GetKey(action);
                return key >= 0 ? Input.GetKeyUp((KeyCode)key) : GetNegativeKeyUp(key);
            case PlayerIndex.Two:
                key = player2Keys.GetKey(action);
                return key >= 0 ? Input.GetKeyUp((KeyCode)key) : GetNegativeKeyUp(key);
            case PlayerIndex.Three:
                key = player3Keys.GetKey(action);
                return key >= 0 ? Input.GetKeyUp((KeyCode)key) : GetNegativeKeyUp(key);
            case PlayerIndex.Four:
                key = player4Keys.GetKey(action);
                return key >= 0 ? Input.GetKeyUp((KeyCode)key) : GetNegativeKeyUp(key);
            case PlayerIndex.Five:
                key = player5Keys.GetKey(action);
                return key >= 0 ? Input.GetKeyUp((KeyCode)key) : GetNegativeKeyUp(key);
            case PlayerIndex.All:
                key = player1Keys.GetKey(action);
                if (key >= 0 ? Input.GetKeyUp((KeyCode)key) : GetNegativeKeyUp(key))
                {
                    playerWhoPressesUp = PlayerIndex.One;
                    return true;
                }
                key = player2Keys.GetKey(action);
                if (key >= 0 ? Input.GetKeyUp((KeyCode)key) : GetNegativeKeyUp(key))
                {
                    playerWhoPressesUp = PlayerIndex.Two;
                    return true;
                }
                key = player3Keys.GetKey(action);
                if (key >= 0 ? Input.GetKeyUp((KeyCode)key) : GetNegativeKeyUp(key))
                {
                    playerWhoPressesUp = PlayerIndex.Three;
                    return true;
                }
                key = player4Keys.GetKey(action);
                if (key >= 0 ? Input.GetKeyUp((KeyCode)key) : GetNegativeKeyUp(key))
                {
                    playerWhoPressesUp = PlayerIndex.Four;
                    return true;
                }
                key = player5Keys.GetKey(action);
                if (key >= 0 ? Input.GetKeyUp((KeyCode)key) : GetNegativeKeyUp(key))
                {
                    playerWhoPressesUp = PlayerIndex.Five;
                    return true;
                }
                return false;
            default:
                return false;
        }
    }

    public static bool GetKeyUp(string action, ControllerType controllerType)
    {
        if (controllerType == ControllerType.Keyboard)
            return GetKeyUp(kbKeys.GetKey(action));
        if (controllerType == ControllerType.All)
            return GetKeyUp(kbKeys.GetKey(action)) || GetKeyUp(gpKeys.GetKey(action));
        return GetKeyUp(gpKeys.GetKey(action));
    }

    /// <returns> true when the key assigned with the action is pressed</returns>
    public static bool GetKey(string action, PlayerIndex player = PlayerIndex.All)
    {
        int key;
        switch (player)
        {
            case PlayerIndex.One:
                key = player1Keys.GetKey(action);
                return key >= 0 ? Input.GetKey((KeyCode)key) : GetNegativeKeyPressed(key);
            case PlayerIndex.Two:
                key = player2Keys.GetKey(action);
                return key >= 0 ? Input.GetKey((KeyCode)key) : GetNegativeKeyPressed(key);
            case PlayerIndex.Three:
                key = player3Keys.GetKey(action);
                return key >= 0 ? Input.GetKey((KeyCode)key) : GetNegativeKeyPressed(key);
            case PlayerIndex.Four:
                key = player4Keys.GetKey(action);
                return key >= 0 ? Input.GetKey((KeyCode)key) : GetNegativeKeyPressed(key);
            case PlayerIndex.Five:
                key = player5Keys.GetKey(action);
                return key >= 0 ? Input.GetKey((KeyCode)key) : GetNegativeKeyPressed(key);
            case PlayerIndex.All:
                int key1 = player1Keys.GetKey(action);
                int key2 = player2Keys.GetKey(action);
                int key3 = player3Keys.GetKey(action);
                int key4 = player4Keys.GetKey(action);
                int key5 = player5Keys.GetKey(action);
                return key1 >= 0 ? Input.GetKey((KeyCode)key1) : GetNegativeKeyPressed(key1) ||
                    key2 >= 0 ? Input.GetKey((KeyCode)key2) : GetNegativeKeyPressed(key2) ||
                    key3 >= 0 ? Input.GetKey((KeyCode)key3) : GetNegativeKeyPressed(key3) ||
                    key4 >= 0 ? Input.GetKey((KeyCode)key4) : GetNegativeKeyPressed(key4) ||
                    key5 >= 0 ? Input.GetKey((KeyCode)key5) : GetNegativeKeyPressed(key5);
            default:
                return false;
        }
    }

    /// <returns> true during while the key assigned with the action is pressed</returns>
    public static bool GetKey(string action, PlayerIndex player, out PlayerIndex playerWhoPressed)
    {
        playerWhoPressed = player;
        int key;
        switch (player)
        {
            case PlayerIndex.One:
                key = player1Keys.GetKey(action);
                return key >= 0 ? Input.GetKey((KeyCode)key) : GetNegativeKeyPressed(key);
            case PlayerIndex.Two:
                key = player2Keys.GetKey(action);
                return key >= 0 ? Input.GetKey((KeyCode)key) : GetNegativeKeyPressed(key);
            case PlayerIndex.Three:
                key = player3Keys.GetKey(action);
                return key >= 0 ? Input.GetKey((KeyCode)key) : GetNegativeKeyPressed(key);
            case PlayerIndex.Four:
                key = player4Keys.GetKey(action);
                return key >= 0 ? Input.GetKey((KeyCode)key) : GetNegativeKeyPressed(key);
            case PlayerIndex.Five:
                key = player5Keys.GetKey(action);
                return key >= 0 ? Input.GetKey((KeyCode)key) : GetNegativeKeyPressed(key);
            case PlayerIndex.All:
                key = player1Keys.GetKey(action);
                if (key >= 0 ? Input.GetKey((KeyCode)key) : GetNegativeKeyPressed(key))
                {
                    playerWhoPressed = PlayerIndex.One;
                    return true;
                }
                key = player2Keys.GetKey(action);
                if (key >= 0 ? Input.GetKey((KeyCode)key) : GetNegativeKeyPressed(key))
                {
                    playerWhoPressed = PlayerIndex.Two;
                    return true;
                }
                key = player3Keys.GetKey(action);
                if (key >= 0 ? Input.GetKey((KeyCode)key) : GetNegativeKeyPressed(key))
                {
                    playerWhoPressed = PlayerIndex.Three;
                    return true;
                }
                key = player4Keys.GetKey(action);
                if (key >= 0 ? Input.GetKey((KeyCode)key) : GetNegativeKeyPressed(key))
                {
                    playerWhoPressed = PlayerIndex.Four;
                    return true;
                }
                key = player5Keys.GetKey(action);
                if (key >= 0 ? Input.GetKey((KeyCode)key) : GetNegativeKeyPressed(key))
                {
                    playerWhoPressed = PlayerIndex.Five;
                    return true;
                }
                return false;
            default:
                return false;
        }
    }

    public static bool GetKey(string action, ControllerType controllerType)
    {
        if (controllerType == ControllerType.Keyboard)
            return GetKey(kbKeys.GetKey(action));
        if (controllerType == ControllerType.All)
            return GetKey(kbKeys.GetKey(action)) || GetKey(gpKeys.GetKey(action));
        return GetKey(gpKeys.GetKey(action));
    }

    /// <returns> true during the frame when a key assigned with one of the actions is pressed</returns>
    public static bool GetKeyDown(string[] actions, PlayerIndex player = PlayerIndex.All)
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

    public static bool GetKeyDown(string[] actions, ControllerType controllerType)
    {
        foreach (string action in actions)
        {
            if (GetKeyDown(action, controllerType))
                return true;
        }
        return false;
    }

    /// <returns> true during the frame when a key assigned with one of the actions is unpressed</returns>
    public static bool GetKeyUp(string[] actions, PlayerIndex player = PlayerIndex.All)
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

    public static bool GetKeyUp(string[] actions, ControllerType controllerType)
    {
        foreach (string action in actions)
        {
            if (GetKeyUp(action, controllerType))
                return true;
        }
        return false;
    }

    /// <returns> true when a key assigned with one of the actions is pressed</returns>
    public static bool GetKey(string[] actions, PlayerIndex player = PlayerIndex.All)
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

    public static bool GetKey(string[] actions, ControllerType controllerType)
    {
        foreach (string action in actions)
        {
            if (GetKey(action, controllerType))
                return true;
        }
        return false;
    }

    /// <returns> true during the frame when the key is pressed</returns>
    public static bool GetKeyDown(KeyCode key) => Input.GetKeyDown(key);
    /// <returns> true during the frame when key is unpressed</returns>
    public static bool GetKeyUp(KeyCode key) => Input.GetKeyDown(key);
    /// <returns> true when the key is pressed</returns>
    public static bool GetKey(KeyCode key) => Input.GetKey(key);
    /// <returns> true during the frame when the key is pressed</returns>
    public static bool GetKeyDown(InputKey key) => GetKeyDown((int)key);
    /// <returns> true during the frame when key is unpressed</returns>
    public static bool GetKeyUp(InputKey key) => GetKeyUp((int)key);
    /// <returns> true when the key is pressed</returns>
    public static bool GetKey(InputKey key) => GetKey((int)key);
    /// <returns> true during the frame when the key is pressed</returns>
    public static bool GetKeyDown(int key) => key >= 0 ? Input.GetKeyDown((KeyCode)key) : GetNegativeKeyDown(key);
    /// <returns> true during the frame when key is unpressed</returns>
    public static bool GetKeyUp(int key) => key >= 0 ? Input.GetKeyUp((KeyCode)key) : GetNegativeKeyUp(key);
    /// <returns> true when the key is pressed</returns>
    public static bool GetKey(int key) => key >= 0 ? Input.GetKey((KeyCode)key) : GetNegativeKeyPressed(key);

    #endregion

    #region Management controller

    /// <summary>
    /// Add an action to the CustomInput system. Multiply action can have the same key.
    /// </summary>
    /// <param name="action"> The action</param>
    /// <param name="keyboardKey"> The keyboard key link with the action</param>
    public static void AddInputAction(string action, InputKey key, PlayerIndex player = PlayerIndex.All)
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

    public static void AddInputAction(string action, KeyCode key, PlayerIndex player = PlayerIndex.All)
    {
        AddInputAction(action, (InputKey)key, player);
    }

    private static InputKey ConvertToGeneralGamepadKey(InputKey key)
    {
        int key2 = (int)key;
        if(key2 < 0 && key2 >= -56)
        {
            do
            {
                key2 -= 14;
            } while (key2 >= -56);
            return (InputKey)key2;
        }
        if(key2 >= 350)
        {
            do
            {
                key2 -= 20;
            } while (key2 >= 350);
            return (InputKey)key2;
        }
        return key;
    }

    private static bool IsGamepadKey(InputKey key)
    {
        int key2 = (int)key;
        return key2 <= 0 || key2 >= 330;
    }

    private static bool IsKeyboardKey(InputKey key) => !IsGamepadKey(key) || key == InputKey.None;

    public static void AddInputAction(string action, InputKey key, ControllerType controllerType)
    {
        if(controllerType == ControllerType.Keyboard)
        {
            if(IsKeyboardKey(key))
                kbKeys.AddAction(action, (int)key);
            return;
        }
        if(controllerType != ControllerType.All)
        {
            if(IsGamepadKey(key))
                gpKeys.AddAction(action, (int)ConvertToGeneralGamepadKey(key));
            return;
        }
        if (IsKeyboardKey(key))
            kbKeys.AddAction(action, (int)key);
        if (IsGamepadKey(key))
            gpKeys.AddAction(action, (int)ConvertToGeneralGamepadKey(key));
    }

    public static void AddInputAction(string[] actions, InputKey[] keys, PlayerIndex player = PlayerIndex.All)
    {
        if (actions.Length != keys.Length)
            return;

        for (int i = 0; i < actions.Length; i++)
        {
            AddInputAction(actions[i], keys[i], player);
        }
    }

    public static void AddInputAction(string[] actions, KeyCode[] keys, PlayerIndex player = PlayerIndex.All)
    {
        InputKey[] keys2 = new InputKey[keys.Length];
        for (int i = 0; i < keys2.Length; i++)
        {
            keys2[i] = (InputKey)keys[i];
        }
        AddInputAction(actions, keys2, player);
    }

    public static void AddInputAction(string[] actions, InputKey[] keys, ControllerType controllerType)
    {
        if (actions.Length != keys.Length)
            return;

        for (int i = 0; i < actions.Length; i++)
        {
            AddInputAction(actions[i], keys[i], controllerType);
        }
    }

    /// <summary>
    /// Change the keyboard key assigned to the action in param
    /// </summary>
    public static bool ReplaceAction(string action, InputKey newKey, PlayerIndex player = PlayerIndex.All)
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

    public static bool ReplaceAction(string action, KeyCode newKey, PlayerIndex player = PlayerIndex.All) => ReplaceAction(action, (InputKey)newKey, player);

    public static bool ReplaceAction(string action, InputKey newKey, ControllerType controllerType)
    {
        if (controllerType == ControllerType.Keyboard)
        {
            if (IsKeyboardKey(newKey))
                return kbKeys.ReplaceAction(action, (int)newKey);
            return false;
        }
        if (controllerType != ControllerType.All)
        {
            if (IsGamepadKey(newKey))
                return gpKeys.ReplaceAction(action, (int)ConvertToGeneralGamepadKey(newKey));
            return false;
        }
        bool b = false;
        if (IsKeyboardKey(newKey))
            b = kbKeys.ReplaceAction(action, (int)newKey);
        if (IsGamepadKey(newKey))
            b = gpKeys.ReplaceAction(action, (int)ConvertToGeneralGamepadKey(newKey)) || b;
        return b;
    }

    public static bool ReplaceAction(string action, KeyCode newKey, ControllerType controllerType) => ReplaceAction(action, (InputKey)newKey, controllerType);

    /// <summary>
    /// Remove the action from the CustomInput system
    /// </summary>
    /// <param name="action"> The action to remove.</param>
    /// <param name="controllerType">The controller where the action will be removed.</param>
    public static bool RemoveAction(string action, PlayerIndex player = PlayerIndex.All)
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

    public static bool RemoveAction(string action, ControllerType controllerType)
    {
        if (controllerType == ControllerType.Keyboard)
            return kbKeys.RemoveAction(action);
        if (controllerType != ControllerType.All)
            return gpKeys.RemoveAction(action);
        bool b = kbKeys.RemoveAction(action);
        return gpKeys.RemoveAction(action) || b;
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
        kbKeys = new InputData();
        gpKeys = new InputData();
    }

    public static void ClearDefaultConfiguration()
    {
        defaultKBKeys = new InputData();
        defaultGB1Keys = new InputData();
        defaultGB2Keys = new InputData();
        defaultGB3Keys = new InputData();
        defaultGB4Keys = new InputData();
        defaultGPKeys = new InputData();
    }

    public static void ClearPlayerConfiguration(PlayerIndex playerIndex, bool defaultTo = false)
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

    public static void ClearControllerConfiguration(ControllerType controllerType, bool defaultTo = false)
    {
        if (controllerType == ControllerType.Keyboard)
        {
            kbKeys = new InputData();
            if (defaultTo)
                defaultKBKeys = new InputData();
            return;
        }
        if (controllerType != ControllerType.All)
        {
            gpKeys = new InputData();
            if (defaultTo)
                defaultGPKeys = new InputData();
            return;
        }
        kbKeys = new InputData();
        gpKeys = new InputData();
        if (defaultTo)
        {
            defaultKBKeys = new InputData();
            defaultGPKeys = new InputData();
        }
    }

    /// <summary>
    /// Set the default Control as the current configuration of a player
    /// </summary>
    /// <param name="controllerType"></param>
    /// <param name="gamepadIndex"></param>
    public static void SetDefaultControl(PlayerIndex player, ControllerType controller)
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

    public static void SetDefaultControl()
    {
        defaultKBKeys = kbKeys.Clone();
        defaultGPKeys = gpKeys.Clone();
    }

    public static void SetDefaultControl(ControllerType controller)
    {
        if(controller == ControllerType.Keyboard)
        {
            defaultKBKeys = kbKeys.Clone();
            return;
        }
        if (controller != ControllerType.All)
        {
            defaultGPKeys = gpKeys.Clone();
            return;
        }
        defaultKBKeys = kbKeys.Clone();
        defaultGPKeys = gpKeys.Clone();
    }

    /// <summary>
    /// Set the current Configuration as the default one.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="controller"></param>
    public static void LoadDefaultController(PlayerIndex player, ControllerType controller)
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
            case ControllerType.GamepadAll:
                inputData = defaultGPKeys;
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
        public InputData defaultGPKeys;
        public InputData defaultG1Keys;
        public InputData defaultGP2Keys;
        public InputData defaultGP3Keys;
        public InputData defaultGP4Keys;
        public InputData player1Keys;
        public InputData player2Keys;
        public InputData player3Keys;
        public InputData player4Keys;
        public InputData player5Keys;
        public InputData kbKeys;
        public InputData gpKeys;

        public CustomInputConfigData(InputData defaultKBKeys, InputData defaultGPKeys, InputData defaultGP1Keys, InputData defaultGP2Keys, InputData defaultGP3Keys,
            InputData defaultGP4Keys, InputData player1Keys, InputData player2Keys, InputData player3Keys, InputData player4Keys, InputData player5Keys, InputData kbKeys, InputData gpKeys)
        {
            this.defaultKBKeys = defaultKBKeys;
            this.defaultGPKeys = defaultGPKeys;
            this.defaultG1Keys = defaultGP1Keys;
            this.defaultGP2Keys = defaultGP2Keys;
            this.defaultGP3Keys = defaultGP3Keys;
            this.defaultGP4Keys = defaultGP4Keys;
            this.player1Keys = player1Keys;
            this.player2Keys = player2Keys;
            this.player3Keys = player3Keys;
            this.player4Keys = player4Keys;
            this.player5Keys = player5Keys;
            this.kbKeys = kbKeys;
            this.gpKeys = gpKeys;
        }
    }

    /// <summary>
    /// Save all the current CustomInput configuration (default and current actions and controllers keys link to the action) for all players in the file in param,
    /// can be load using the methode CustomInput.LoadConfiguration(string fileName).
    /// </summary>
    public static bool SaveConfiguration(string fileName)
    {
        defaultKBKeys.PrepareSerialization(); defaultGPKeys.PrepareSerialization(); defaultGB1Keys.PrepareSerialization();defaultGB2Keys.PrepareSerialization();
        defaultGB3Keys.PrepareSerialization();defaultGB4Keys.PrepareSerialization();player1Keys.PrepareSerialization();player2Keys.PrepareSerialization();
        player3Keys.PrepareSerialization();player4Keys.PrepareSerialization();player5Keys.PrepareSerialization();kbKeys.PrepareSerialization();gpKeys.PrepareSerialization();

        CustomInputConfigData CustomInputConfig = new CustomInputConfigData(defaultKBKeys, defaultGPKeys, defaultGB1Keys, defaultGB2Keys, defaultGB3Keys, defaultGB4Keys,
            player1Keys, player2Keys, player3Keys, player4Keys, player5Keys, kbKeys, gpKeys);
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
        defaultKBKeys.PrepareSerialization(); defaultGPKeys.PrepareSerialization(); defaultGB1Keys.PrepareSerialization(); defaultGB2Keys.PrepareSerialization();
        defaultGB3Keys.PrepareSerialization(); defaultGB4Keys.PrepareSerialization();
        CustomInputConfigData CustomInputConfig = new CustomInputConfigData(defaultKBKeys, defaultGPKeys, defaultGB1Keys, defaultGB2Keys, defaultGB3Keys, defaultGB4Keys,
            i.player1Keys, i.player2Keys, i.player3Keys, i.player4Keys, i.player5Keys, i.kbKeys, i.gpKeys);
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
        player1Keys.PrepareSerialization(); player2Keys.PrepareSerialization();
        player3Keys.PrepareSerialization(); player4Keys.PrepareSerialization(); player5Keys.PrepareSerialization(); kbKeys.PrepareSerialization(); gpKeys.PrepareSerialization();
        CustomInputConfigData CustomInputConfig = new CustomInputConfigData(i.defaultKBKeys, i.defaultGPKeys, i.defaultG1Keys, i.defaultGP2Keys, i.defaultGP3Keys, i.defaultGP4Keys,
            player1Keys, player2Keys, player3Keys, player4Keys, player5Keys, kbKeys, gpKeys);
        return Save.WriteJSONData(CustomInputConfig, fileName);
    }

    /// <summary>
    /// Load from the file Save in the game repertory all the configuration of the CustomInput system.
    /// </summary>
    public static bool LoadConfiguration(string fileName)
    {
        if (!Save.ReadJSONData<CustomInputConfigData>(fileName, out CustomInputConfigData i))
            return false;
        defaultKBKeys = i.defaultKBKeys;
        defaultGPKeys = i.defaultGPKeys;
        defaultGB1Keys = i.defaultG1Keys;
        defaultGB2Keys = i.defaultGP2Keys;
        defaultGB3Keys = i.defaultGP3Keys;
        defaultGB4Keys = i.defaultGP4Keys;
        player1Keys = i.player1Keys;
        player2Keys = i.player2Keys;
        player3Keys = i.player3Keys;
        player4Keys = i.player4Keys;
        player5Keys = i.player5Keys;
        kbKeys = i.kbKeys;
        gpKeys = i.gpKeys;

        defaultKBKeys.Build(); defaultGPKeys.Build(); defaultGB1Keys.Build(); defaultGB2Keys.Build();
        defaultGB3Keys.Build();defaultGB4Keys.Build();player1Keys.Build();player2Keys.Build();player3Keys.Build();player4Keys.Build();player5Keys.Build();kbKeys.Build();gpKeys.Build();
        return true;
    }

    /// <summary>
    /// Load from the file Save in the game repertory the default configuration of the CustomInput system.
    /// </summary>
    public static bool LoadDefaultConfiguration(string fileName)
    {
        if (!Save.ReadJSONData<CustomInputConfigData>(fileName, out CustomInputConfigData i))
            return false;
        defaultKBKeys = i.defaultKBKeys;
        defaultGB1Keys = i.defaultG1Keys;
        defaultGB2Keys = i.defaultGP2Keys;
        defaultGB3Keys = i.defaultGP3Keys;
        defaultGB4Keys = i.defaultGP4Keys;
        defaultGPKeys = i.defaultGPKeys;
        defaultKBKeys.Build(); defaultGPKeys.Build(); defaultGB1Keys.Build();
        defaultGB2Keys.Build(); defaultGB3Keys.Build(); defaultGB4Keys.Build();
        return true;
    }
    /// <summary>
    /// Load from the file Save in the game repertory the current configuration of the CustomInput system.
    /// </summary>
    public static bool LoadCurrentConfiguration(string fileName)
    {
        if (!Save.ReadJSONData<CustomInputConfigData>(fileName, out CustomInputConfigData i))
            return false;
        player1Keys = i.player1Keys;
        player2Keys = i.player2Keys;
        player3Keys = i.player3Keys;
        player4Keys = i.player4Keys;
        player5Keys = i.player5Keys;
        gpKeys = i.gpKeys;
        kbKeys = i.kbKeys;

        player1Keys.Build(); player2Keys.Build(); player3Keys.Build(); player4Keys.Build(); player5Keys.Build(); kbKeys.Build(); gpKeys.Build();
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
        return key >= 0 ? ((InputKey)key).ToString() : negativeKeyToString[((-(key + 1)) % 14) + 1];
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

    public static string KeyToString(string action, ControllerType controllerType)
    {
        if (controllerType == ControllerType.Keyboard)
            return KeyToString(kbKeys.GetKey(action));
        if(gpKeys.Contain(action))
            return KeyToString(gpKeys.GetKey(action));
        return KeyToString(kbKeys.GetKey(action));
    }

    public static bool Listen(ControllerType controller, out int key)
    {
        if(Listen(controller, out InputKey tmp))
        {
            key = (int)tmp;
            return true;
        }
        key = 0;
        return false;
    }

    /// <param name="key"> the key pressed, castable to an Keys, MouseButton or Buttons according to the controler type</param>
    /// <param name="gamepadIndex"></param>
    /// <returns> true if a key of the controler is pressed this frame, false otherwise </returns>
    public static bool Listen(ControllerType controller, out InputKey key)
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
                        key = (InputKey)keyCodeInt[i];
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
                        key = (InputKey)keyCodeInt[i];
                        return true;
                    }
                }
                for (int i = -14; i <= -1; i++)
                {
                    if (GetNegativeKeyDown(i))
                    {
                        key = (InputKey)i;
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
                        key = (InputKey)keyCodeInt[i];
                        return true;
                    }
                }
                for (int i = -28; i <= -15; i++)
                {
                    if (GetNegativeKeyDown(i))
                    {
                        key = (InputKey)i;
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
                        key = (InputKey)keyCodeInt[i];
                        return true;
                    }
                }
                for (int i = -42; i <= -29; i++)
                {
                    if (GetNegativeKeyDown(i))
                    {
                        key = (InputKey)i;
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
                        key = (InputKey)keyCodeInt[i];
                        return true;
                    }
                }
                for (int i = -56; i <= -43; i++)
                {
                    if (GetNegativeKeyDown(i))
                    {
                        key = (InputKey)i;
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
                        key = (InputKey)keyCodeInt[i];
                        return true;
                    }
                }
                for (int i = -70; i <= 0; i++)
                {
                    if (GetNegativeKeyDown(i))
                    {
                        key = (InputKey)i;
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
                        key = (InputKey)keyCodeInt[i];
                        return true;
                    }
                }
                for (int i = -70; i <= 0; i++)
                {
                    if(GetNegativeKeyDown(i))
                    {
                        key = (InputKey)i;
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

    public static bool ListenAll(ControllerType controller, out InputKey[] resultKeys)
    {
        List<InputKey> res = new List<InputKey>();
        int beg = 0, end = 0;
        switch (controller)
        {
            case ControllerType.Keyboard:
                end = keyCodeInt.GetIndexOf(329);
                for (int i = beg; i <= end; i++)
                {
                    if (Input.GetKeyDown((KeyCode)keyCodeInt[i]))
                    {
                        res.Add((InputKey)keyCodeInt[i]);
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
                        res.Add((InputKey)keyCodeInt[i]);
                    }
                }
                for (int i = -14; i <= -1; i++)
                {
                    if (GetNegativeKeyDown(i))
                    {
                        res.Add((InputKey)i);
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
                        res.Add((InputKey)keyCodeInt[i]);
                    }
                }
                for (int i = -28; i <= -15; i++)
                    {
                    if (GetNegativeKeyDown(i))
                    {
                        res.Add((InputKey)i);
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
                        res.Add((InputKey)keyCodeInt[i]);
                    }
                }
                for (int i = -42; i <= -29; i++)
                {
                    if (GetNegativeKeyDown(i))
                    {
                        res.Add((InputKey)i);
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
                        res.Add((InputKey)keyCodeInt[i]);
                    }
                }
                for (int i = -56; i <= -43; i++)
                {
                    if (GetNegativeKeyDown(i))
                    {
                        res.Add((InputKey)i);
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
                        res.Add((InputKey)keyCodeInt[i]);
                    }
                }
                for (int i = -70; i <= 0; i++)
                {
                    if (GetNegativeKeyDown(i))
                    {
                        res.Add((InputKey)i);
                    }
                }
                break;
            case ControllerType.All:
                for (int i = 0; i < keyCodeInt.Length; i++)
                {
                    if (Input.GetKeyDown((KeyCode)keyCodeInt[i]))
                    {
                        res.Add((InputKey)keyCodeInt[i]);
                    }
                }
                for (int i = -70; i <= 0; i++)
                {
                    if (GetNegativeKeyDown(i))
                    {
                        res.Add((InputKey)i);
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
        public InputKey[] keysKeyboard, keyGamepad1, keyGamepad2, keyGamepad3, keyGamepad4;
        public ControllerType controllerType;

        public GeneralInput(InputKey[] keysKeyboard, InputKey[] keyGamepad1, InputKey[] keyGamepad2, InputKey[] keyGamepad3, InputKey[] keyGamepad4, ControllerType controllerType)
        {
            this.keysKeyboard = keysKeyboard;
            this.keyGamepad1 = keyGamepad1;
            this.keyGamepad2 = keyGamepad2;
            this.keyGamepad3 = keyGamepad3;
            this.keyGamepad4 = keyGamepad4;
            this.controllerType = controllerType;
        }

        private bool isKeySomething(Func<InputKey, bool> func)
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

            bool GetKeySomething(Func<InputKey, bool> func, InputKey[] keyCodes)
            {
                foreach (InputKey key in keyCodes)
                {
                    if (func(key))
                        return true;
                }
                return false;
            }
        }

        public bool IsPressedDown() => isKeySomething((InputKey key) => GetKeyDown(key));
        public bool IsPressedUp() => isKeySomething((InputKey key) => GetKeyUp(key));
        public bool IsPressed() => isKeySomething((InputKey key) => GetKey(key));
    }

    #endregion
}
