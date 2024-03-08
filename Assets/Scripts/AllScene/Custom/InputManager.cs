/*
 * Copyright (c) 2023 Léonard Pannetier <leonard.pannetier@laposte.net>
 * 
 * This asset was create to make a Input System ease to use and powerful for project in Unity.
 * It's base on the XInputDotNetPure wrapper made by speps, see detail on their repo https://github.com/speps/XInputDotNet/releases
 * The license of the XInputDotNetPure is the same of this Package, and is include in the subfolder XInputDotNetPure.
 * This asset is also based on the build in Input System of Unity (the old input system).
 * 
 * This asset can be use in any projet, no obligation of credit, but it is appreciated.
 * See the license file for more details.
 * 
*/


#region Using

using System;
using System.Collections.Generic;
using XInputDotNetPure;
using System.Collections;
using System.Threading.Tasks;
using System.Text;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

#endregion

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

public enum BaseController
{
    Keyboard,
    Gamepad,
    KeyboardAndGamepad
}

public enum MouseWheelDirection
{
    Up,
    Down,
    none
}

public enum GeneralGamepadKey
{
    GPRT = -61,
    GPLT = -62,
    GPDPadUp = -63,
    GPDPadRight = -64,
    GPDPadDown = -65,
    GPDPadLeft = -66,
    GPTBSRUp = -67,
    GPTBSRDown = -68,
    GPTBSRRight = -69,
    GPTBSRLeft = -70,
    GPTBSLUp = -71,
    GPTBSLDown = -72,
    GPTBSLRight = -73,
    GPTBSLLeft = -74,
    GPGuide = -75,

    None = 0,

    GPA = 330,
    GPB = 331,
    GPX = 332,
    GPY = 333,
    GPL1 = 334,
    GPR1 = 335,
    GPBack = 336,
    GPStart = 337,
    GPTBSL = 338,
    GPTBSR = 339,
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
}

public enum GamepadKey
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
    GP1Guide = -15,

    GP2RT = -16,
    GP2LT = -17,
    GP2DPadUp = -18,
    GP2DPadRight = -19,
    GP2DPadDown = -20,
    GP2DPadLeft = -21,
    GP2TBSRUp = -22,
    GP2TBSRDown = -23,
    GP2TBSRRight = -24,
    GP2TBSRLeft = -25,
    GP2TBSLUp = -26,
    GP2TBSLDown = -27,
    GP2TBSLRight = -28,
    GP2TBSLLeft = -29,
    GP2Guide = -30,

    GP3RT = -31,
    GP3LT = -32,
    GP3DPadUp = -33,
    GP3DPadRight = -34,
    GP3DPadDown = -35,
    GP3DPadLeft = -36,
    GP3TBSRUp = -37,
    GP3TBSRDown = -38,
    GP3TBSRRight = -39,
    GP3TBSRLeft = -40,
    GP3TBSLUp = -41,
    GP3TBSLDown = -42,
    GP3TBSLRight = -43,
    GP3TBSLLeft = -44,
    GP3Guide = -45,

    GP4RT = -46,
    GP4LT = -47,
    GP4DPadUp = -48,
    GP4DPadRight = -49,
    GP4DPadDown = -50,
    GP4DPadLeft = -51,
    GP4TBSRUp = -52,
    GP4TBSRDown = -53,
    GP4TBSRRight = -54,
    GP4TBSRLeft = -55,
    GP4TBSLUp = -56,
    GP4TBSLDown = -57,
    GP4TBSLRight = -58,
    GP4TBSLLeft = -59,
    GP4Guide = -60,

    GPRT = -61,
    GPLT = -62,
    GPDPadUp = -63,
    GPDPadRight = -64,
    GPDPadDown = -65,
    GPDPadLeft = -66,
    GPTBSRUp = -67,
    GPTBSRDown = -68,
    GPTBSRRight = -69,
    GPTBSRLeft = -70,
    GPTBSLUp = -71,
    GPTBSLDown = -72,
    GPTBSLRight = -73,
    GPTBSLLeft = -74,
    GPGuide = -75,

    None = 0,

    GPA = 330,
    GPB = 331,
    GPX = 332,
    GPY = 333,
    GPL1 = 334,
    GPR1 = 335,
    GPBack = 336,
    GPStart = 337,
    GPTBSL = 338,
    GPTBSR = 339,
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
    GP1A = 350,
    GP1B = 351,
    GP1X = 352,
    GP1Y = 353,
    GP1L1 = 354,
    GP1R1 = 355,
    GP1Back = 356,
    GP1Start = 357,
    GP1TBSL = 358,
    GP1TBSR = 359,
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
    GP2A = 370,
    GP2B = 371,
    GP2X = 372,
    GP2Y = 373,
    GP2L1= 374,
    GP2R1 = 375,
    GP2Back = 376,
    GP2Start = 377,
    GP2TBSL = 378,
    GP2TBSR = 379,
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
    GP3A = 390,
    GP3B = 391,
    GP3X = 392,
    GP3Y = 393,
    GP3L1 = 394,
    GP3R1 = 395,
    GP3Back = 396,
    GP3Start = 397,
    GP3TBSL = 398,
    GP3TBSR = 399,
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
    GP4A = 410,
    GP4B = 411,
    GP4X = 412,
    GP4Y = 413,
    GP4L1 = 414,
    GP4R1 = 415,
    GP4Start = 416,
    GP4Back = 417,
    GP4TBSL = 418,
    GP4TBSR = 419,
    GP4Button10 = 420,
    GP4Button11 = 421,
    GP4Button12 = 422,
    GP4Button13 = 423,
    GP4Button14 = 424,
    GP4Button15 = 425,
    GP4Button16 = 426,
    GP4Button17 = 427,
    GP4Button18 = 428,
    GP4Button19 = 429
}

public enum KeyboardKey
{
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
    QuoteMark = 160,
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
    GP1Guide = -15,

    GP2RT = -16,
    GP2LT = -17,
    GP2DPadUp = -18,
    GP2DPadRight = -19,
    GP2DPadDown = -20,
    GP2DPadLeft = -21,
    GP2TBSRUp = -22,
    GP2TBSRDown = -23,
    GP2TBSRRight = -24,
    GP2TBSRLeft = -25,
    GP2TBSLUp = -26,
    GP2TBSLDown = -27,
    GP2TBSLRight = -28,
    GP2TBSLLeft = -29,
    GP2Guide = -30,

    GP3RT = -31,
    GP3LT = -32,
    GP3DPadUp = -33,
    GP3DPadRight = -34,
    GP3DPadDown = -35,
    GP3DPadLeft = -36,
    GP3TBSRUp = -37,
    GP3TBSRDown = -38,
    GP3TBSRRight = -39,
    GP3TBSRLeft = -40,
    GP3TBSLUp = -41,
    GP3TBSLDown = -42,
    GP3TBSLRight = -43,
    GP3TBSLLeft = -44,
    GP3Guide = -45,

    GP4RT = -46,
    GP4LT = -47,
    GP4DPadUp = -48,
    GP4DPadRight = -49,
    GP4DPadDown = -50,
    GP4DPadLeft = -51,
    GP4TBSRUp = -52,
    GP4TBSRDown = -53,
    GP4TBSRRight = -54,
    GP4TBSRLeft = -55,
    GP4TBSLUp = -56,
    GP4TBSLDown = -57,
    GP4TBSLRight = -58,
    GP4TBSLLeft = -59,
    GP4Guide = -60,

    GPRT = -61,
    GPLT = -62,
    GPDPadUp = -63,
    GPDPadRight = -64,
    GPDPadDown = -65,
    GPDPadLeft = -66,
    GPTBSRUp = -67,
    GPTBSRDown = -68,
    GPTBSRRight = -69,
    GPTBSRLeft = -70,
    GPTBSLUp = -71,
    GPTBSLDown = -72,
    GPTBSLRight = -73,
    GPTBSLLeft = -74,
    GPGuide = -75,

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
    QuoteMark = 160,
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
    GPA = 330,
    GPB = 331,
    GPX = 332,
    GPY = 333,
    GPL1 = 334,
    GPR1 = 335,
    GPBack = 336,
    GPStart = 337,
    GPTBSL = 338,
    GPTBSR = 339,
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
    GP1A = 350,
    GP1B = 351,
    GP1X = 352,
    GP1Y = 353,
    GP1L1 = 354,
    GP1R1 = 355,
    GP1Back = 356,
    GP1Start = 357,
    GP1TBSL = 358,
    GP1TBSR = 359,
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
    GP2A = 370,
    GP2B = 371,
    GP2X = 372,
    GP2Y = 373,
    GP2L1 = 374,
    GP2R1 = 375,
    GP2Back = 376,
    GP2Start = 377,
    GP2TBSL = 378,
    GP2TBSR = 379,
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
    GP3A = 390,
    GP3B = 391,
    GP3X = 392,
    GP3Y = 393,
    GP3L1 = 394,
    GP3R1 = 395,
    GP3Back = 396,
    GP3Start = 397,
    GP3TBSL = 398,
    GP3TBSR = 399,
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
    GP4A = 410,
    GP4B = 411,
    GP4X = 412,
    GP4Y = 413,
    GP4L1 = 414,
    GP4R1 = 415,
    GP4Start = 416,
    GP4Back = 417,
    GP4TBSL = 418,
    GP4TBSR = 419,
    GP4Button10 = 420,
    GP4Button11 = 421,
    GP4Button12 = 422,
    GP4Button13 = 423,
    GP4Button14 = 424,
    GP4Button15 = 425,
    GP4Button16 = 426,
    GP4Button17 = 427,
    GP4Button18 = 428,
    GP4Button19 = 429    
}

#endregion

#region InputManager

public static class InputManager
{
    #region Keys config

    //différents players controls
    private static InputData player1Keys = new InputData();
    private static InputData player2Keys = new InputData();
    private static InputData player3Keys = new InputData();
    private static InputData player4Keys = new InputData();
    private static InputData player5Keys = new InputData();

    //keyboard/gamepad controls
    private static InputData defaultKBKeys = new InputData();
    private static InputData defaultGPKeys = new InputData();
    private static InputData kbKeys = new InputData();
    private static InputData gpKeys = new InputData();

    #endregion

    #region Require

    private static GamePadState newGP1State, oldGP1State;
    private static GamePadState newGP2State, oldGP2State;
    private static GamePadState newGP3State, oldGP3State;
    private static GamePadState newGP4State, oldGP4State;

    private static Vector2 newGP1Triggers, newGP2Triggers, newGP3Triggers, newGP4Triggers;
    private static Vector2 newGP1RightStickPosition, newGP2RightStickPosition, newGP3RightStickPosition, newGP4RightStickPosition;
    private static Vector2 newGP1LeftStickPosition, newGP2LeftStickPosition, newGP3LeftStickPosition, newGP4LeftStickPosition;

    private static Vector2 _GP1RightThumbStickDeadZone = new Vector2(0.1f, 0.1f), _GP1LeftThumbStickDeadZone = new Vector2(0.1f, 0.1f), _GP1TriggersDeadZone = new Vector2(0.1f, 0.1f);
    private static Vector2 _GP2RightThumbStickDeadZone = new Vector2(0.1f, 0.1f), _GP2LeftThumbStickDeadZone = new Vector2(0.1f, 0.1f), _GP2TriggersDeadZone = new Vector2(0.1f, 0.1f);
    private static Vector2 _GP3RightThumbStickDeadZone = new Vector2(0.1f, 0.1f), _GP3LeftThumbStickDeadZone = new Vector2(0.1f, 0.1f), _GP3TriggersDeadZone = new Vector2(0.1f, 0.1f);
    private static Vector2 _GP4RightThumbStickDeadZone = new Vector2(0.1f, 0.1f), _GP4LeftThumbStickDeadZone = new Vector2(0.1f, 0.1f), _GP4TriggersDeadZone = new Vector2(0.1f, 0.1f);
    private static float _analogicButtonDownValue = 0.25f;

    public static Vector2 GP1RightThumbStickDeadZone { get => _GP1RightThumbStickDeadZone; set { _GP1RightThumbStickDeadZone = new Vector2(Mathf.Clamp(value.x, 0f, 0.45f), Mathf.Clamp(value.x, 0f, 0.45f)); } }
    public static Vector2 GP1LeftThumbStickDeadZone { get => _GP1LeftThumbStickDeadZone; set { _GP1LeftThumbStickDeadZone = new Vector2(Mathf.Clamp(value.x, 0f, 0.45f), Mathf.Clamp(value.x, 0f, 0.45f)); } }
    public static Vector2 GP1TriggersDeadZone { get => _GP1TriggersDeadZone; set { _GP1TriggersDeadZone = new Vector2(Mathf.Clamp(value.x, 0f, 0.45f), Mathf.Min(value.y, 0.5f)); } }
    public static Vector2 GP2RightThumbStickDeadZone { get => _GP2RightThumbStickDeadZone; set { _GP2RightThumbStickDeadZone = new Vector2(Mathf.Clamp(value.x, 0f, 0.45f), Mathf.Min(value.y, 0.5f)); } }
    public static Vector2 GP2LeftThumbStickDeadZone { get => _GP2LeftThumbStickDeadZone; set { _GP2LeftThumbStickDeadZone = new Vector2(Mathf.Clamp(value.x, 0f, 0.45f), Mathf.Min(value.y, 0.5f)); } }
    public static Vector2 GP2TriggersDeadZone { get => _GP2TriggersDeadZone; set { _GP2TriggersDeadZone = new Vector2(Mathf.Clamp(value.x, 0f, 0.45f), Mathf.Min(value.y, 0.5f)); } }
    public static Vector2 GP3RightThumbStickDeadZone { get => _GP3RightThumbStickDeadZone; set { _GP3RightThumbStickDeadZone = new Vector2(Mathf.Clamp(value.x, 0f, 0.45f), Mathf.Min(value.y, 0.5f)); } }
    public static Vector2 GP3LeftThumbStickDeadZone { get => _GP3LeftThumbStickDeadZone; set { _GP3LeftThumbStickDeadZone = new Vector2(Mathf.Clamp(value.x, 0f, 0.45f), Mathf.Min(value.y, 0.5f)); } }
    public static Vector2 GP3TriggersDeadZone { get => _GP3TriggersDeadZone; set { _GP3TriggersDeadZone = new Vector2(Mathf.Clamp(value.x, 0f, 0.45f), Mathf.Min(value.y, 0.5f)); } }
    public static Vector2 GP4RightThumbStickDeadZone { get => _GP4RightThumbStickDeadZone; set { _GP4RightThumbStickDeadZone = new Vector2(Mathf.Clamp(value.x, 0f, 0.45f), Mathf.Min(value.y, 0.5f)); } }
    public static Vector2 GP4LeftThumbStickDeadZone { get => _GP4LeftThumbStickDeadZone; set { _GP4LeftThumbStickDeadZone = new Vector2(Mathf.Clamp(value.x, 0f, 0.45f), Mathf.Min(value.y, 0.5f)); } }
    public static Vector2 GP4TriggersDeadZone { get => _GP4TriggersDeadZone; set { _GP4TriggersDeadZone = new Vector2(Mathf.Clamp(value.x, 0f, 0.45f), Mathf.Min(value.y, 0.5f)); } }
    public static float analogicButtonDownValue { get => _analogicButtonDownValue; set { _analogicButtonDownValue = Mathf.Clamp(value, 0.05f, 0.45f); } }

    private static Vector2 defaultGP1RightThumbStickDeadZone = new Vector2(0.1f, 0.1f), defaultGP1LeftThumbStickDeadZone = new Vector2(0.1f, 0.1f), defaultGP1TriggersDeadZone = new Vector2(0.1f, 0.1f);
    private static Vector2 defaultGP2RightThumbStickDeadZone = new Vector2(0.1f, 0.1f), defaultGP2LeftThumbStickDeadZone = new Vector2(0.1f, 0.1f), defaultGP2TriggersDeadZone = new Vector2(0.1f, 0.1f);
    private static Vector2 defaultGP3RightThumbStickDeadZone = new Vector2(0.1f, 0.1f), defaultGP3LeftThumbStickDeadZone = new Vector2(0.1f, 0.1f), defaultGP3TriggersDeadZone = new Vector2(0.1f, 0.1f);
    private static Vector2 defaultGP4RightThumbStickDeadZone = new Vector2(0.1f, 0.1f), defaultGP4LeftThumbStickDeadZone = new Vector2(0.1f, 0.1f), defaultGP4TriggersDeadZone = new Vector2(0.1f, 0.1f);

    private static readonly string[] letters = new string[36] { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

    private static readonly int[] keyCodeInt = { 0,8,9,12,13,19,27,32,33,34,35,36,37,38,39,40,41,42,43,44,45,46,47,48,49,50,51,52,53,54,55,56,57,58,59,60,61,62,63,64,92,93,94,95,96,97,98,99,100,101,102,103,104,105,106,107,108,109,110,111,112,113,114,115,116,
        117,118,119,120,121,122,123,124,125,126,127,160,256,257,258,259,260,261,262,263,264,265,266,267,268,269,270,271,272,273,274,275,276,277,278,279,280,281,282,283,284,285,286,287,288,289,290,291,292,293,294,295,296,300,301,302,303,304,305,306,307,308,
        309,310,311,312,313,314,315,316,317,318,319,323,324,325,326,327,328,329,330,331,332,333,334,335,336,337,338,339,340,341,342,343,344,345,346,347,348,349,350,351,352,353,354,355,356,357,358,359,360,361,362,363,364,365,366,367,368,369,370,371,372,
        373,374,375,376,377,378,379,380,381,382,383,384,385,386,387,388,389,390,391,392,393,394,395,396,397,398,399,400,401,402,403,404,405,406,407,408,409,410,411,412,413,414,415,416,417,418,419,420,421,422,423,424,425,426,427,428,429,430,431,432,433,
        434,435,436,437,438,439,440,441,442,443,444,445,446,447,448,449,450,451,452,453,454,455,456,457,458,459,460,461,462,463,464,465,466,467,468,469,470,471,472,473,474,475,476,477,478,479,480,481,482,483,484,485,486,487,488,489,490,491,492,493,494,
        495,496,497,498,499,500,501,502,503,504,505,506,507,508,509 };

    private static List<VibrationSetting> vibrationSettings = new List<VibrationSetting>();

    #region GetInputKey Down/Up/Pressed delegate

    private static readonly Func<bool>[] GetInputKeyDownDelegate = new Func<bool>[76]
    {
        () => false,
        () => { return oldGP1State.Triggers.Right <= _analogicButtonDownValue && newGP1State.Triggers.Right > _analogicButtonDownValue; },
        () => { return oldGP1State.Triggers.Left <= _analogicButtonDownValue && newGP1State.Triggers.Left > _analogicButtonDownValue; },
        () => { return oldGP1State.DPad.Up == ButtonState.Released && newGP1State.DPad.Up == ButtonState.Pressed; },
        () => { return oldGP1State.DPad.Right == ButtonState.Released && newGP1State.DPad.Right == ButtonState.Pressed; },
        () => { return oldGP1State.DPad.Down == ButtonState.Released && newGP1State.DPad.Down == ButtonState.Pressed; },
        () => { return oldGP1State.DPad.Left == ButtonState.Released && newGP1State.DPad.Left == ButtonState.Pressed; },
        () => { return oldGP1State.ThumbSticks.Right.Y <= _analogicButtonDownValue && newGP1State.ThumbSticks.Right.Y > _analogicButtonDownValue; },
        () => { return oldGP1State.ThumbSticks.Right.Y >= -_analogicButtonDownValue && newGP1State.ThumbSticks.Right.Y < -_analogicButtonDownValue; },
        () => { return oldGP1State.ThumbSticks.Right.X <= _analogicButtonDownValue && newGP1State.ThumbSticks.Right.X > _analogicButtonDownValue; },
        () => { return oldGP1State.ThumbSticks.Right.X >= -_analogicButtonDownValue && newGP1State.ThumbSticks.Right.X < -_analogicButtonDownValue; },
        () => { return oldGP1State.ThumbSticks.Left.Y <= _analogicButtonDownValue && newGP1State.ThumbSticks.Left.Y > _analogicButtonDownValue; },
        () => { return oldGP1State.ThumbSticks.Left.Y >= -_analogicButtonDownValue && newGP1State.ThumbSticks.Left.Y < -_analogicButtonDownValue; },
        () => { return oldGP1State.ThumbSticks.Left.X <= _analogicButtonDownValue && newGP1State.ThumbSticks.Left.X > _analogicButtonDownValue; },
        () => { return oldGP1State.ThumbSticks.Left.X >= -_analogicButtonDownValue && newGP1State.ThumbSticks.Left.X < -_analogicButtonDownValue; },
        () => { return oldGP1State.Buttons.Guide == ButtonState.Released && newGP1State.Buttons.Guide == ButtonState.Pressed; },

        () => { return oldGP2State.Triggers.Right <= _analogicButtonDownValue && newGP2State.Triggers.Right > _analogicButtonDownValue; },
        () => { return oldGP2State.Triggers.Left <= _analogicButtonDownValue && newGP2State.Triggers.Left > _analogicButtonDownValue; },
        () => { return oldGP2State.DPad.Up == ButtonState.Released && newGP2State.DPad.Up == ButtonState.Pressed; },
        () => { return oldGP2State.DPad.Right == ButtonState.Released && newGP2State.DPad.Right == ButtonState.Pressed; },
        () => { return oldGP2State.DPad.Down == ButtonState.Released && newGP2State.DPad.Down == ButtonState.Pressed; },
        () => { return oldGP2State.DPad.Left == ButtonState.Released && newGP2State.DPad.Left == ButtonState.Pressed; },
        () => { return oldGP2State.ThumbSticks.Right.Y <= _analogicButtonDownValue && newGP2State.ThumbSticks.Right.Y > _analogicButtonDownValue; },
        () => { return oldGP2State.ThumbSticks.Right.Y >= -_analogicButtonDownValue && newGP2State.ThumbSticks.Right.Y < -_analogicButtonDownValue; },
        () => { return oldGP2State.ThumbSticks.Right.X <= _analogicButtonDownValue && newGP2State.ThumbSticks.Right.X > _analogicButtonDownValue; },
        () => { return oldGP2State.ThumbSticks.Right.X >= -_analogicButtonDownValue && newGP2State.ThumbSticks.Right.X < -_analogicButtonDownValue; },
        () => { return oldGP2State.ThumbSticks.Left.Y <= _analogicButtonDownValue && newGP2State.ThumbSticks.Left.Y > _analogicButtonDownValue; },
        () => { return oldGP2State.ThumbSticks.Left.Y >= -_analogicButtonDownValue && newGP2State.ThumbSticks.Left.Y < -_analogicButtonDownValue; },
        () => { return oldGP2State.ThumbSticks.Left.X <= _analogicButtonDownValue && newGP2State.ThumbSticks.Left.X > _analogicButtonDownValue; },
        () => { return oldGP2State.ThumbSticks.Left.X >= -_analogicButtonDownValue && newGP2State.ThumbSticks.Left.X < -_analogicButtonDownValue; },
        () => { return oldGP2State.Buttons.Guide == ButtonState.Released && newGP2State.Buttons.Guide == ButtonState.Pressed; },

        () => { return oldGP3State.Triggers.Right <= _analogicButtonDownValue && newGP3State.Triggers.Right > _analogicButtonDownValue; },
        () => { return oldGP3State.Triggers.Left <= _analogicButtonDownValue && newGP3State.Triggers.Left > _analogicButtonDownValue; },
        () => { return oldGP3State.DPad.Up == ButtonState.Released && newGP3State.DPad.Up == ButtonState.Pressed; },
        () => { return oldGP3State.DPad.Right == ButtonState.Released && newGP3State.DPad.Right == ButtonState.Pressed; },
        () => { return oldGP3State.DPad.Down == ButtonState.Released && newGP3State.DPad.Down == ButtonState.Pressed; },
        () => { return oldGP3State.DPad.Left == ButtonState.Released && newGP3State.DPad.Left == ButtonState.Pressed; },
        () => { return oldGP3State.ThumbSticks.Right.Y <= _analogicButtonDownValue && newGP3State.ThumbSticks.Right.Y > _analogicButtonDownValue; },
        () => { return oldGP3State.ThumbSticks.Right.Y >= -_analogicButtonDownValue && newGP3State.ThumbSticks.Right.Y < -_analogicButtonDownValue; },
        () => { return oldGP3State.ThumbSticks.Right.X <= _analogicButtonDownValue && newGP3State.ThumbSticks.Right.X > _analogicButtonDownValue; },
        () => { return oldGP3State.ThumbSticks.Right.X >= -_analogicButtonDownValue && newGP3State.ThumbSticks.Right.X < -_analogicButtonDownValue; },
        () => { return oldGP3State.ThumbSticks.Left.Y <= _analogicButtonDownValue && newGP3State.ThumbSticks.Left.Y > _analogicButtonDownValue; },
        () => { return oldGP3State.ThumbSticks.Left.Y >= -_analogicButtonDownValue && newGP3State.ThumbSticks.Left.Y < -_analogicButtonDownValue; },
        () => { return oldGP3State.ThumbSticks.Left.X <= _analogicButtonDownValue && newGP3State.ThumbSticks.Left.X > _analogicButtonDownValue; },
        () => { return oldGP3State.ThumbSticks.Left.X >= -_analogicButtonDownValue && newGP3State.ThumbSticks.Left.X < -_analogicButtonDownValue; },
        () => { return oldGP3State.Buttons.Guide == ButtonState.Released && newGP3State.Buttons.Guide == ButtonState.Pressed; },

        () => { return oldGP4State.Triggers.Right <= _analogicButtonDownValue && newGP4State.Triggers.Right > _analogicButtonDownValue; },
        () => { return oldGP4State.Triggers.Left <= _analogicButtonDownValue && newGP4State.Triggers.Left > _analogicButtonDownValue; },
        () => { return oldGP4State.DPad.Up == ButtonState.Released && newGP4State.DPad.Up == ButtonState.Pressed; },
        () => { return oldGP4State.DPad.Right == ButtonState.Released && newGP4State.DPad.Right == ButtonState.Pressed; },
        () => { return oldGP4State.DPad.Down == ButtonState.Released && newGP4State.DPad.Down == ButtonState.Pressed; },
        () => { return oldGP4State.DPad.Left == ButtonState.Released && newGP4State.DPad.Left == ButtonState.Pressed; },
        () => { return oldGP4State.ThumbSticks.Right.Y <= _analogicButtonDownValue && newGP4State.ThumbSticks.Right.Y > _analogicButtonDownValue; },
        () => { return oldGP4State.ThumbSticks.Right.Y >= -_analogicButtonDownValue && newGP4State.ThumbSticks.Right.Y < -_analogicButtonDownValue; },
        () => { return oldGP4State.ThumbSticks.Right.X <= _analogicButtonDownValue && newGP4State.ThumbSticks.Right.X > _analogicButtonDownValue; },
        () => { return oldGP4State.ThumbSticks.Right.X >= -_analogicButtonDownValue && newGP4State.ThumbSticks.Right.X < -_analogicButtonDownValue; },
        () => { return oldGP4State.ThumbSticks.Left.Y <= _analogicButtonDownValue && newGP4State.ThumbSticks.Left.Y > _analogicButtonDownValue; },
        () => { return oldGP4State.ThumbSticks.Left.Y >= -_analogicButtonDownValue && newGP4State.ThumbSticks.Left.Y < -_analogicButtonDownValue; },
        () => { return oldGP4State.ThumbSticks.Left.X <= _analogicButtonDownValue && newGP4State.ThumbSticks.Left.X > _analogicButtonDownValue; },
        () => { return oldGP4State.ThumbSticks.Left.X >= -_analogicButtonDownValue && newGP4State.ThumbSticks.Left.X < -_analogicButtonDownValue; },
        () => { return oldGP4State.Buttons.Guide == ButtonState.Released && newGP4State.Buttons.Guide == ButtonState.Pressed; },

        () => { return (oldGP1State.Triggers.Right <= _analogicButtonDownValue && newGP1State.Triggers.Right > _analogicButtonDownValue)
            || (oldGP2State.Triggers.Right <= _analogicButtonDownValue && newGP2State.Triggers.Right > _analogicButtonDownValue)
            || (oldGP3State.Triggers.Right <= _analogicButtonDownValue && newGP3State.Triggers.Right > _analogicButtonDownValue)
            || (oldGP4State.Triggers.Right <= _analogicButtonDownValue && newGP4State.Triggers.Right > _analogicButtonDownValue); },
        () => { return (oldGP1State.Triggers.Left <= _analogicButtonDownValue && newGP1State.Triggers.Left > _analogicButtonDownValue)
            || (oldGP2State.Triggers.Left <= _analogicButtonDownValue && newGP2State.Triggers.Left > _analogicButtonDownValue)
            || (oldGP3State.Triggers.Left <= _analogicButtonDownValue && newGP3State.Triggers.Left > _analogicButtonDownValue)
            || (oldGP4State.Triggers.Left <= _analogicButtonDownValue && newGP4State.Triggers.Left > _analogicButtonDownValue); },
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
        () => { return (oldGP1State.ThumbSticks.Right.Y <= _analogicButtonDownValue && newGP1State.ThumbSticks.Right.Y > _analogicButtonDownValue)
            || (oldGP2State.ThumbSticks.Right.Y <= _analogicButtonDownValue && newGP2State.ThumbSticks.Right.Y > _analogicButtonDownValue)
            || (oldGP3State.ThumbSticks.Right.Y <= _analogicButtonDownValue && newGP3State.ThumbSticks.Right.Y > _analogicButtonDownValue)
            || (oldGP4State.ThumbSticks.Right.Y <= _analogicButtonDownValue && newGP4State.ThumbSticks.Right.Y > _analogicButtonDownValue); },
        () => { return (oldGP1State.ThumbSticks.Right.Y >= -_analogicButtonDownValue && newGP1State.ThumbSticks.Right.Y < -_analogicButtonDownValue)
            || (oldGP2State.ThumbSticks.Right.Y >= _analogicButtonDownValue && newGP2State.ThumbSticks.Right.Y < _analogicButtonDownValue)
            || (oldGP3State.ThumbSticks.Right.Y >= -_analogicButtonDownValue && newGP3State.ThumbSticks.Right.Y < -_analogicButtonDownValue)
            || (oldGP4State.ThumbSticks.Right.Y >= -_analogicButtonDownValue && newGP4State.ThumbSticks.Right.Y < -_analogicButtonDownValue); },
        () => { return (oldGP1State.ThumbSticks.Right.X <= _analogicButtonDownValue && newGP1State.ThumbSticks.Right.X > _analogicButtonDownValue)
            || (oldGP2State.ThumbSticks.Right.X <= _analogicButtonDownValue && newGP2State.ThumbSticks.Right.X > _analogicButtonDownValue)
            || (oldGP3State.ThumbSticks.Right.X <= _analogicButtonDownValue && newGP3State.ThumbSticks.Right.X > _analogicButtonDownValue)
            || (oldGP4State.ThumbSticks.Right.X <= _analogicButtonDownValue && newGP4State.ThumbSticks.Right.X > _analogicButtonDownValue); },
        () => { return (oldGP1State.ThumbSticks.Right.X >= -_analogicButtonDownValue && newGP1State.ThumbSticks.Right.X < -_analogicButtonDownValue)
            || (oldGP2State.ThumbSticks.Right.X >= -_analogicButtonDownValue && newGP2State.ThumbSticks.Right.X < -_analogicButtonDownValue)
            || (oldGP3State.ThumbSticks.Right.X >= -_analogicButtonDownValue && newGP3State.ThumbSticks.Right.X < -_analogicButtonDownValue)
            || (oldGP4State.ThumbSticks.Right.X >= -_analogicButtonDownValue && newGP4State.ThumbSticks.Right.X < -_analogicButtonDownValue); },
        () => { return (oldGP1State.ThumbSticks.Left.Y <= _analogicButtonDownValue && newGP1State.ThumbSticks.Left.Y > _analogicButtonDownValue)
            || (oldGP2State.ThumbSticks.Left.Y <= _analogicButtonDownValue && newGP2State.ThumbSticks.Left.Y > _analogicButtonDownValue)
            || (oldGP3State.ThumbSticks.Left.Y <= _analogicButtonDownValue && newGP3State.ThumbSticks.Left.Y > _analogicButtonDownValue)
            || (oldGP4State.ThumbSticks.Left.Y <= _analogicButtonDownValue && newGP4State.ThumbSticks.Left.Y > _analogicButtonDownValue); },
        () => { return (oldGP1State.ThumbSticks.Left.Y >= -_analogicButtonDownValue && newGP1State.ThumbSticks.Left.Y < -_analogicButtonDownValue)
            || (oldGP2State.ThumbSticks.Left.Y >= -_analogicButtonDownValue && newGP2State.ThumbSticks.Left.Y < -_analogicButtonDownValue)
            || (oldGP3State.ThumbSticks.Left.Y >= -_analogicButtonDownValue && newGP3State.ThumbSticks.Left.Y < -_analogicButtonDownValue)
            || (oldGP4State.ThumbSticks.Left.Y >= -_analogicButtonDownValue && newGP4State.ThumbSticks.Left.Y < -_analogicButtonDownValue); },
        () => { return (oldGP1State.ThumbSticks.Left.X <= _analogicButtonDownValue && newGP1State.ThumbSticks.Left.X > _analogicButtonDownValue)
            || (oldGP2State.ThumbSticks.Left.X <= _analogicButtonDownValue && newGP2State.ThumbSticks.Left.X > _analogicButtonDownValue)
            || (oldGP3State.ThumbSticks.Left.X <= _analogicButtonDownValue && newGP3State.ThumbSticks.Left.X > _analogicButtonDownValue)
            || (oldGP4State.ThumbSticks.Left.X <= _analogicButtonDownValue && newGP4State.ThumbSticks.Left.X > _analogicButtonDownValue); },
        () => { return (oldGP1State.ThumbSticks.Left.X >= -_analogicButtonDownValue && newGP1State.ThumbSticks.Left.X < -_analogicButtonDownValue)
            || (oldGP2State.ThumbSticks.Left.X >= -_analogicButtonDownValue && newGP2State.ThumbSticks.Left.X < -_analogicButtonDownValue)
            || (oldGP3State.ThumbSticks.Left.X >= -_analogicButtonDownValue && newGP3State.ThumbSticks.Left.X < -_analogicButtonDownValue)
            || (oldGP4State.ThumbSticks.Left.X >= -_analogicButtonDownValue && newGP4State.ThumbSticks.Left.X < -_analogicButtonDownValue); },
        () => { return (oldGP1State.Buttons.Guide == ButtonState.Released && newGP1State.Buttons.Guide == ButtonState.Pressed) ||
            (oldGP2State.Buttons.Guide == ButtonState.Released && newGP2State.Buttons.Guide == ButtonState.Pressed) ||
            (oldGP3State.Buttons.Guide == ButtonState.Released && newGP3State.Buttons.Guide == ButtonState.Pressed) ||
            (oldGP4State.Buttons.Guide == ButtonState.Released && newGP4State.Buttons.Guide == ButtonState.Pressed); }
    };

    private static readonly Func<bool>[] GetInputKeyUpDelegate = new Func<bool>[76]
    {
        () => { return false; },
        () => { return oldGP1State.Triggers.Right > _analogicButtonDownValue && newGP1State.Triggers.Right <= _analogicButtonDownValue; },
        () => { return oldGP1State.Triggers.Left > _analogicButtonDownValue && newGP1State.Triggers.Left <= _analogicButtonDownValue; },
        () => { return oldGP1State.DPad.Up == ButtonState.Pressed && newGP1State.DPad.Up == ButtonState.Released; },
        () => { return oldGP1State.DPad.Right == ButtonState.Pressed && newGP1State.DPad.Right == ButtonState.Released; },
        () => { return oldGP1State.DPad.Down == ButtonState.Pressed && newGP1State.DPad.Down == ButtonState.Released; },
        () => { return oldGP1State.DPad.Left == ButtonState.Pressed && newGP1State.DPad.Left == ButtonState.Released; },
        () => { return oldGP1State.ThumbSticks.Right.Y > _analogicButtonDownValue && newGP1State.ThumbSticks.Right.Y <= _analogicButtonDownValue; },
        () => { return oldGP1State.ThumbSticks.Right.Y < -_analogicButtonDownValue && newGP1State.ThumbSticks.Right.Y >= -_analogicButtonDownValue; },
        () => { return oldGP1State.ThumbSticks.Right.X > _analogicButtonDownValue && newGP1State.ThumbSticks.Right.X <= _analogicButtonDownValue; },
        () => { return oldGP1State.ThumbSticks.Right.X < -_analogicButtonDownValue && newGP1State.ThumbSticks.Right.X >= -_analogicButtonDownValue; },
        () => { return oldGP1State.ThumbSticks.Left.Y > _analogicButtonDownValue && newGP1State.ThumbSticks.Left.Y < _analogicButtonDownValue; },
        () => { return oldGP1State.ThumbSticks.Left.Y < -_analogicButtonDownValue && newGP1State.ThumbSticks.Left.Y > -_analogicButtonDownValue; },
        () => { return oldGP1State.ThumbSticks.Left.X > _analogicButtonDownValue && newGP1State.ThumbSticks.Left.X < _analogicButtonDownValue; },
        () => { return oldGP1State.ThumbSticks.Left.X < -_analogicButtonDownValue && newGP1State.ThumbSticks.Left.X > -_analogicButtonDownValue; },
        () => { return oldGP1State.Buttons.Guide == ButtonState.Pressed && newGP1State.Buttons.Guide == ButtonState.Released; },

        () => { return oldGP2State.Triggers.Right > _analogicButtonDownValue && newGP2State.Triggers.Right <= _analogicButtonDownValue; },
        () => { return oldGP2State.Triggers.Left > _analogicButtonDownValue && newGP2State.Triggers.Left <= _analogicButtonDownValue; },
        () => { return oldGP2State.DPad.Up == ButtonState.Pressed && newGP2State.DPad.Up == ButtonState.Released; },
        () => { return oldGP2State.DPad.Right == ButtonState.Pressed && newGP2State.DPad.Right == ButtonState.Released; },
        () => { return oldGP2State.DPad.Down == ButtonState.Pressed && newGP2State.DPad.Down == ButtonState.Released; },
        () => { return oldGP2State.DPad.Left == ButtonState.Pressed && newGP2State.DPad.Left == ButtonState.Released; },
        () => { return oldGP2State.ThumbSticks.Right.Y > _analogicButtonDownValue && newGP2State.ThumbSticks.Right.Y <= _analogicButtonDownValue; },
        () => { return oldGP2State.ThumbSticks.Right.Y < -_analogicButtonDownValue && newGP2State.ThumbSticks.Right.Y >= -_analogicButtonDownValue; },
        () => { return oldGP2State.ThumbSticks.Right.X > _analogicButtonDownValue && newGP2State.ThumbSticks.Right.X <= _analogicButtonDownValue; },
        () => { return oldGP2State.ThumbSticks.Right.X < -_analogicButtonDownValue && newGP2State.ThumbSticks.Right.X >= -_analogicButtonDownValue; },
        () => { return oldGP2State.ThumbSticks.Left.Y > _analogicButtonDownValue && newGP2State.ThumbSticks.Left.Y <= _analogicButtonDownValue; },
        () => { return oldGP2State.ThumbSticks.Left.Y < -_analogicButtonDownValue && newGP2State.ThumbSticks.Left.Y >= -_analogicButtonDownValue; },
        () => { return oldGP2State.ThumbSticks.Left.X > _analogicButtonDownValue && newGP2State.ThumbSticks.Left.X <= _analogicButtonDownValue; },
        () => { return oldGP2State.ThumbSticks.Left.X < -_analogicButtonDownValue && newGP2State.ThumbSticks.Left.X >= -_analogicButtonDownValue; },
        () => { return oldGP2State.Buttons.Guide == ButtonState.Pressed && newGP2State.Buttons.Guide == ButtonState.Released; },

        () => { return oldGP3State.Triggers.Right > _analogicButtonDownValue && newGP3State.Triggers.Right <= _analogicButtonDownValue; },
        () => { return oldGP3State.Triggers.Left > _analogicButtonDownValue && newGP3State.Triggers.Left <= _analogicButtonDownValue; },
        () => { return oldGP3State.DPad.Up == ButtonState.Pressed && newGP3State.DPad.Up == ButtonState.Released; },
        () => { return oldGP3State.DPad.Right == ButtonState.Pressed && newGP3State.DPad.Right == ButtonState.Released; },
        () => { return oldGP3State.DPad.Down == ButtonState.Pressed && newGP3State.DPad.Down == ButtonState.Released; },
        () => { return oldGP3State.DPad.Left == ButtonState.Pressed && newGP3State.DPad.Left == ButtonState.Released; },
        () => { return oldGP3State.ThumbSticks.Right.Y > _analogicButtonDownValue && newGP3State.ThumbSticks.Right.Y < _analogicButtonDownValue; },
        () => { return oldGP3State.ThumbSticks.Right.Y < -_analogicButtonDownValue && newGP3State.ThumbSticks.Right.Y > -_analogicButtonDownValue; },
        () => { return oldGP3State.ThumbSticks.Right.X > _analogicButtonDownValue && newGP3State.ThumbSticks.Right.X < _analogicButtonDownValue; },
        () => { return oldGP3State.ThumbSticks.Right.X < -_analogicButtonDownValue && newGP3State.ThumbSticks.Right.X > -_analogicButtonDownValue; },
        () => { return oldGP3State.ThumbSticks.Left.Y > _analogicButtonDownValue && newGP3State.ThumbSticks.Left.Y < _analogicButtonDownValue; },
        () => { return oldGP3State.ThumbSticks.Left.Y < -_analogicButtonDownValue && newGP3State.ThumbSticks.Left.Y > -_analogicButtonDownValue; },
        () => { return oldGP3State.ThumbSticks.Left.X > _analogicButtonDownValue && newGP3State.ThumbSticks.Left.X < _analogicButtonDownValue; },
        () => { return oldGP3State.ThumbSticks.Left.X < -_analogicButtonDownValue && newGP3State.ThumbSticks.Left.X > -_analogicButtonDownValue; },
        () => { return oldGP3State.Buttons.Guide == ButtonState.Pressed && newGP3State.Buttons.Guide == ButtonState.Released; },

        () => { return oldGP4State.Triggers.Right > _analogicButtonDownValue && newGP4State.Triggers.Right <= _analogicButtonDownValue; },
        () => { return oldGP4State.Triggers.Left > _analogicButtonDownValue && newGP4State.Triggers.Left <= _analogicButtonDownValue; },
        () => { return oldGP4State.DPad.Up == ButtonState.Pressed && newGP4State.DPad.Up == ButtonState.Released; },
        () => { return oldGP4State.DPad.Right == ButtonState.Pressed && newGP4State.DPad.Right == ButtonState.Released; },
        () => { return oldGP4State.DPad.Down == ButtonState.Pressed && newGP4State.DPad.Down == ButtonState.Released; },
        () => { return oldGP4State.DPad.Left == ButtonState.Pressed && newGP4State.DPad.Left == ButtonState.Released; },
        () => { return oldGP4State.ThumbSticks.Right.Y > _analogicButtonDownValue && newGP4State.ThumbSticks.Right.Y < _analogicButtonDownValue; },
        () => { return oldGP4State.ThumbSticks.Right.Y < -_analogicButtonDownValue && newGP4State.ThumbSticks.Right.Y > -_analogicButtonDownValue; },
        () => { return oldGP4State.ThumbSticks.Right.X > _analogicButtonDownValue && newGP4State.ThumbSticks.Right.X < _analogicButtonDownValue; },
        () => { return oldGP4State.ThumbSticks.Right.X < -_analogicButtonDownValue && newGP4State.ThumbSticks.Right.X > -_analogicButtonDownValue; },
        () => { return oldGP4State.ThumbSticks.Left.Y > _analogicButtonDownValue && newGP4State.ThumbSticks.Left.Y < _analogicButtonDownValue; },
        () => { return oldGP4State.ThumbSticks.Left.Y < -_analogicButtonDownValue && newGP4State.ThumbSticks.Left.Y > -_analogicButtonDownValue; },
        () => { return oldGP4State.ThumbSticks.Left.X > _analogicButtonDownValue && newGP4State.ThumbSticks.Left.X < _analogicButtonDownValue; },
        () => { return oldGP4State.ThumbSticks.Left.X < -_analogicButtonDownValue && newGP4State.ThumbSticks.Left.X > -_analogicButtonDownValue; },
        () => { return oldGP4State.Buttons.Guide == ButtonState.Pressed && newGP4State.Buttons.Guide == ButtonState.Released; },

        () => { return (oldGP1State.Triggers.Right > _analogicButtonDownValue && newGP1State.Triggers.Right <= _analogicButtonDownValue)
            || (oldGP2State.Triggers.Right > _analogicButtonDownValue && newGP2State.Triggers.Right <= _analogicButtonDownValue)
            || (oldGP3State.Triggers.Right > _analogicButtonDownValue && newGP3State.Triggers.Right <= _analogicButtonDownValue)
            || (oldGP4State.Triggers.Right > _analogicButtonDownValue && newGP4State.Triggers.Right <= _analogicButtonDownValue); },
        () => { return (oldGP1State.Triggers.Left > _analogicButtonDownValue && newGP1State.Triggers.Left <= _analogicButtonDownValue)
            || (oldGP2State.Triggers.Left > _analogicButtonDownValue && newGP2State.Triggers.Left <= _analogicButtonDownValue)
            || (oldGP3State.Triggers.Left > _analogicButtonDownValue && newGP3State.Triggers.Left <= _analogicButtonDownValue)
            || (oldGP4State.Triggers.Left > _analogicButtonDownValue && newGP4State.Triggers.Left <= _analogicButtonDownValue); },
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
        () => { return (oldGP1State.ThumbSticks.Right.Y > _analogicButtonDownValue && newGP1State.ThumbSticks.Right.Y <= _analogicButtonDownValue)
            || (oldGP2State.ThumbSticks.Right.Y > _analogicButtonDownValue && newGP2State.ThumbSticks.Right.Y <= _analogicButtonDownValue)
            || (oldGP3State.ThumbSticks.Right.Y > _analogicButtonDownValue && newGP3State.ThumbSticks.Right.Y <= _analogicButtonDownValue)
            || (oldGP4State.ThumbSticks.Right.Y > _analogicButtonDownValue && newGP4State.ThumbSticks.Right.Y <= _analogicButtonDownValue); },
        () => { return (oldGP1State.ThumbSticks.Right.Y < -_analogicButtonDownValue && newGP1State.ThumbSticks.Right.Y >= -_analogicButtonDownValue)
            || (oldGP2State.ThumbSticks.Right.Y < _analogicButtonDownValue && newGP2State.ThumbSticks.Right.Y >= _analogicButtonDownValue)
            || (oldGP3State.ThumbSticks.Right.Y < -_analogicButtonDownValue && newGP3State.ThumbSticks.Right.Y >= -_analogicButtonDownValue)
            || (oldGP4State.ThumbSticks.Right.Y < -_analogicButtonDownValue && newGP4State.ThumbSticks.Right.Y >= -_analogicButtonDownValue); },
        () => { return (oldGP1State.ThumbSticks.Right.X > _analogicButtonDownValue && newGP1State.ThumbSticks.Right.X <= _analogicButtonDownValue)
            || (oldGP2State.ThumbSticks.Right.X > _analogicButtonDownValue && newGP2State.ThumbSticks.Right.X <= _analogicButtonDownValue)
            || (oldGP3State.ThumbSticks.Right.X > _analogicButtonDownValue && newGP3State.ThumbSticks.Right.X <= _analogicButtonDownValue)
            || (oldGP4State.ThumbSticks.Right.X > _analogicButtonDownValue && newGP4State.ThumbSticks.Right.X <= _analogicButtonDownValue); },
        () => { return (oldGP1State.ThumbSticks.Right.X < -_analogicButtonDownValue && newGP1State.ThumbSticks.Right.X >= -_analogicButtonDownValue)
            || (oldGP2State.ThumbSticks.Right.X < -_analogicButtonDownValue && newGP2State.ThumbSticks.Right.X >= -_analogicButtonDownValue)
            || (oldGP3State.ThumbSticks.Right.X < -_analogicButtonDownValue && newGP3State.ThumbSticks.Right.X >= -_analogicButtonDownValue)
            || (oldGP4State.ThumbSticks.Right.X < -_analogicButtonDownValue && newGP4State.ThumbSticks.Right.X >= -_analogicButtonDownValue); },
        () => { return (oldGP1State.ThumbSticks.Left.Y > _analogicButtonDownValue && newGP1State.ThumbSticks.Left.Y <= _analogicButtonDownValue)
            || (oldGP2State.ThumbSticks.Left.Y > _analogicButtonDownValue && newGP2State.ThumbSticks.Left.Y <= _analogicButtonDownValue)
            || (oldGP3State.ThumbSticks.Left.Y > _analogicButtonDownValue && newGP3State.ThumbSticks.Left.Y <= _analogicButtonDownValue)
            || (oldGP4State.ThumbSticks.Left.Y > _analogicButtonDownValue && newGP4State.ThumbSticks.Left.Y <= _analogicButtonDownValue); },
        () => { return (oldGP1State.ThumbSticks.Left.Y < -_analogicButtonDownValue && newGP1State.ThumbSticks.Left.Y >= -_analogicButtonDownValue)
            || (oldGP2State.ThumbSticks.Left.Y < -_analogicButtonDownValue && newGP2State.ThumbSticks.Left.Y >= -_analogicButtonDownValue)
            || (oldGP3State.ThumbSticks.Left.Y < -_analogicButtonDownValue && newGP3State.ThumbSticks.Left.Y >= -_analogicButtonDownValue)
            || (oldGP4State.ThumbSticks.Left.Y < -_analogicButtonDownValue && newGP4State.ThumbSticks.Left.Y >= -_analogicButtonDownValue); },
        () => { return (oldGP1State.ThumbSticks.Left.X > _analogicButtonDownValue && newGP1State.ThumbSticks.Left.X <= _analogicButtonDownValue)
            || (oldGP2State.ThumbSticks.Left.X > _analogicButtonDownValue && newGP2State.ThumbSticks.Left.X <= _analogicButtonDownValue)
            || (oldGP3State.ThumbSticks.Left.X > _analogicButtonDownValue && newGP3State.ThumbSticks.Left.X <= _analogicButtonDownValue)
            || (oldGP4State.ThumbSticks.Left.X > _analogicButtonDownValue && newGP4State.ThumbSticks.Left.X <= _analogicButtonDownValue); },
        () => { return (oldGP1State.ThumbSticks.Left.X < -_analogicButtonDownValue && newGP1State.ThumbSticks.Left.X >= -_analogicButtonDownValue)
            || (oldGP2State.ThumbSticks.Left.X < -_analogicButtonDownValue && newGP2State.ThumbSticks.Left.X >= -_analogicButtonDownValue)
            || (oldGP3State.ThumbSticks.Left.X < -_analogicButtonDownValue && newGP3State.ThumbSticks.Left.X >= -_analogicButtonDownValue)
            || (oldGP4State.ThumbSticks.Left.X < -_analogicButtonDownValue && newGP4State.ThumbSticks.Left.X >= -_analogicButtonDownValue); },
        () => { return (oldGP1State.Buttons.Guide == ButtonState.Pressed && newGP1State.Buttons.Guide == ButtonState.Released) ||
            (oldGP2State.Buttons.Guide == ButtonState.Pressed && newGP2State.Buttons.Guide == ButtonState.Released) ||
            (oldGP3State.Buttons.Guide == ButtonState.Pressed && newGP3State.Buttons.Guide == ButtonState.Released) ||
            (oldGP4State.Buttons.Guide == ButtonState.Pressed && newGP4State.Buttons.Guide == ButtonState.Released); }
    };

    private static readonly Func<bool>[] GetInputKeyPressedDelegate = new Func<bool>[76]
    {
        () => { return false; },
        () => { return newGP1State.Triggers.Right > _analogicButtonDownValue; },
        () => { return newGP1State.Triggers.Left > _analogicButtonDownValue; },
        () => { return newGP1State.DPad.Up == ButtonState.Pressed; },
        () => { return newGP1State.DPad.Right == ButtonState.Pressed; },
        () => { return newGP1State.DPad.Down == ButtonState.Pressed; },
        () => { return newGP1State.DPad.Left == ButtonState.Pressed; },
        () => { return newGP1State.ThumbSticks.Right.Y > _analogicButtonDownValue; },
        () => { return newGP1State.ThumbSticks.Right.Y < -_analogicButtonDownValue; },
        () => { return newGP1State.ThumbSticks.Right.X > _analogicButtonDownValue; },
        () => { return newGP1State.ThumbSticks.Right.X < -_analogicButtonDownValue; },
        () => { return newGP1State.ThumbSticks.Left.Y > _analogicButtonDownValue; },
        () => { return newGP1State.ThumbSticks.Left.Y < -_analogicButtonDownValue; },
        () => { return newGP1State.ThumbSticks.Left.X > _analogicButtonDownValue; },
        () => { return newGP1State.ThumbSticks.Left.X < -_analogicButtonDownValue; },
        () => { return newGP1State.Buttons.Guide == ButtonState.Pressed; },

        () => { return newGP2State.Triggers.Right > _analogicButtonDownValue; },
        () => { return newGP2State.Triggers.Left > _analogicButtonDownValue; },
        () => { return newGP2State.DPad.Up == ButtonState.Pressed; },
        () => { return newGP2State.DPad.Right == ButtonState.Pressed; },
        () => { return newGP2State.DPad.Down == ButtonState.Pressed; },
        () => { return newGP2State.DPad.Left == ButtonState.Pressed; },
        () => { return newGP2State.ThumbSticks.Right.Y > _analogicButtonDownValue; },
        () => { return newGP2State.ThumbSticks.Right.Y < -_analogicButtonDownValue; },
        () => { return newGP2State.ThumbSticks.Right.X > _analogicButtonDownValue; },
        () => { return newGP2State.ThumbSticks.Right.X < -_analogicButtonDownValue; },
        () => { return newGP2State.ThumbSticks.Left.Y > _analogicButtonDownValue; },
        () => { return newGP2State.ThumbSticks.Left.Y < -_analogicButtonDownValue; },
        () => { return newGP2State.ThumbSticks.Left.X > _analogicButtonDownValue; },
        () => { return newGP2State.ThumbSticks.Left.X < -_analogicButtonDownValue; },
        () => { return newGP2State.Buttons.Guide == ButtonState.Pressed; },

        () => { return newGP3State.Triggers.Right > _analogicButtonDownValue; },
        () => { return newGP3State.Triggers.Left > _analogicButtonDownValue; },
        () => { return newGP3State.DPad.Up == ButtonState.Pressed; },
        () => { return newGP3State.DPad.Right == ButtonState.Pressed; },
        () => { return newGP3State.DPad.Down == ButtonState.Pressed; },
        () => { return newGP3State.DPad.Left == ButtonState.Pressed; },
        () => { return newGP3State.ThumbSticks.Right.Y > _analogicButtonDownValue; },
        () => { return newGP3State.ThumbSticks.Right.Y < -_analogicButtonDownValue; },
        () => { return newGP3State.ThumbSticks.Right.X > _analogicButtonDownValue; },
        () => { return newGP3State.ThumbSticks.Right.X < -_analogicButtonDownValue; },
        () => { return newGP3State.ThumbSticks.Left.Y > _analogicButtonDownValue; },
        () => { return newGP3State.ThumbSticks.Left.Y < -_analogicButtonDownValue; },
        () => { return newGP3State.ThumbSticks.Left.X > _analogicButtonDownValue; },
        () => { return newGP3State.ThumbSticks.Left.X < -_analogicButtonDownValue; },
        () => { return newGP3State.Buttons.Guide == ButtonState.Pressed; },

        () => { return newGP4State.Triggers.Right > _analogicButtonDownValue; },
        () => { return newGP4State.Triggers.Left > _analogicButtonDownValue; },
        () => { return newGP4State.DPad.Up == ButtonState.Pressed; },
        () => { return newGP4State.DPad.Right == ButtonState.Pressed; },
        () => { return newGP4State.DPad.Down == ButtonState.Pressed; },
        () => { return newGP4State.DPad.Left == ButtonState.Pressed; },
        () => { return newGP4State.ThumbSticks.Right.Y > _analogicButtonDownValue; },
        () => { return newGP4State.ThumbSticks.Right.Y < -_analogicButtonDownValue; },
        () => { return newGP4State.ThumbSticks.Right.X > _analogicButtonDownValue; },
        () => { return newGP4State.ThumbSticks.Right.X < -_analogicButtonDownValue; },
        () => { return newGP4State.ThumbSticks.Left.Y > _analogicButtonDownValue; },
        () => { return newGP4State.ThumbSticks.Left.Y < -_analogicButtonDownValue; },
        () => { return newGP4State.ThumbSticks.Left.X > _analogicButtonDownValue; },
        () => { return newGP4State.ThumbSticks.Left.X < -_analogicButtonDownValue; },
        () => { return newGP3State.Buttons.Guide == ButtonState.Pressed; },

        () => { return (newGP1State.Triggers.Right > _analogicButtonDownValue)
            || (newGP2State.Triggers.Right > _analogicButtonDownValue)
            || (newGP3State.Triggers.Right > _analogicButtonDownValue)
            || (newGP4State.Triggers.Right > _analogicButtonDownValue); },
        () => { return (newGP1State.Triggers.Left > _analogicButtonDownValue)
            || (newGP2State.Triggers.Left > _analogicButtonDownValue)
            || (newGP3State.Triggers.Left > _analogicButtonDownValue)
            || (newGP4State.Triggers.Left > _analogicButtonDownValue); },
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
        () => { return (newGP1State.ThumbSticks.Right.Y > _analogicButtonDownValue)
            || (newGP2State.ThumbSticks.Right.Y > _analogicButtonDownValue)
            || (newGP3State.ThumbSticks.Right.Y > _analogicButtonDownValue)
            || (newGP4State.ThumbSticks.Right.Y > _analogicButtonDownValue); },
        () => { return (newGP1State.ThumbSticks.Right.Y < -_analogicButtonDownValue)
            || (newGP2State.ThumbSticks.Right.Y < _analogicButtonDownValue)
            || (newGP3State.ThumbSticks.Right.Y < -_analogicButtonDownValue)
            || (newGP4State.ThumbSticks.Right.Y < -_analogicButtonDownValue); },
        () => { return (newGP1State.ThumbSticks.Right.X > _analogicButtonDownValue)
            || (newGP2State.ThumbSticks.Right.X > _analogicButtonDownValue)
            || (newGP3State.ThumbSticks.Right.X > _analogicButtonDownValue)
            || (newGP4State.ThumbSticks.Right.X > _analogicButtonDownValue); },
        () => { return (newGP1State.ThumbSticks.Right.X < -_analogicButtonDownValue)
            || (newGP2State.ThumbSticks.Right.X < -_analogicButtonDownValue)
            || (newGP3State.ThumbSticks.Right.X < -_analogicButtonDownValue)
            || (newGP4State.ThumbSticks.Right.X < -_analogicButtonDownValue); },
        () => { return (newGP1State.ThumbSticks.Left.Y > _analogicButtonDownValue)
            || (newGP2State.ThumbSticks.Left.Y > _analogicButtonDownValue)
            || (newGP3State.ThumbSticks.Left.Y > _analogicButtonDownValue)
            || (newGP4State.ThumbSticks.Left.Y > _analogicButtonDownValue); },
        () => { return (newGP1State.ThumbSticks.Left.Y < -_analogicButtonDownValue)
            || (newGP2State.ThumbSticks.Left.Y < -_analogicButtonDownValue)
            || (newGP3State.ThumbSticks.Left.Y < -_analogicButtonDownValue)
            || (newGP4State.ThumbSticks.Left.Y < -_analogicButtonDownValue); },
        () => { return (newGP1State.ThumbSticks.Left.X > _analogicButtonDownValue)
            || (newGP2State.ThumbSticks.Left.X > _analogicButtonDownValue)
            || (newGP3State.ThumbSticks.Left.X > _analogicButtonDownValue)
            || (newGP4State.ThumbSticks.Left.X > _analogicButtonDownValue); },
        () => { return (newGP1State.ThumbSticks.Left.X < -_analogicButtonDownValue)
            || (newGP2State.ThumbSticks.Left.X < -_analogicButtonDownValue)
            || (newGP3State.ThumbSticks.Left.X < -_analogicButtonDownValue)
            || (newGP4State.ThumbSticks.Left.X < -_analogicButtonDownValue); },
        () => { return newGP1State.Buttons.Guide == ButtonState.Pressed ||
            newGP2State.Buttons.Guide == ButtonState.Pressed ||
            newGP3State.Buttons.Guide == ButtonState.Pressed ||
            newGP4State.Buttons.Guide == ButtonState.Pressed; }
    };

    #endregion

    #endregion

    #region Class InputData

    [Serializable]
    private class InputData : IEnumerable<KeyValuePair<string, InputData.ListInt>>
    {
        public List<string> actions = new List<string>();
        public List<ListInt> keys = new List<ListInt>();
        [NonSerialized] public Dictionary<string, ListInt> controlsDic = new Dictionary<string, ListInt>();

        public InputData() { }

        public InputData(List<string> actions, List<ListInt> keys)
        {
            this.actions = actions;
            this.keys = keys;
            Build();
        }

        public InputData(Dictionary<string, ListInt> controls)
        {
            controlsDic = new Dictionary<string, ListInt>();
            foreach (KeyValuePair<string, ListInt> item in controls)
            {
                controlsDic.Add(item.Key, item.Value);
            }
        }

        public void Build()
        {
            if (actions.Count != keys.Count)
                return;

            controlsDic = new Dictionary<string, ListInt>();
            for (int i = 0; i < actions.Count; i++)
            {
                controlsDic.Add(actions[i], keys[i]);
            }
            ClearList();
        }

        public void UnBuild()
        {
            actions = new List<string>();
            keys = new List<ListInt>();

            foreach(KeyValuePair<string, ListInt> item in controlsDic)
            {
                actions.Add(item.Key);
                keys.Add(item.Value);
            }
        }

        public void ClearList()
        {
            actions.Clear();
            keys.Clear();
        }

        public void Clear()
        {
            ClearList();
            controlsDic.Clear();
        }

        public bool IsEmpty() => controlsDic.Count <= 0;

        public void AddAction(string action, int key)
        {
            if (controlsDic.ContainsKey(action))
                controlsDic[action].keys.Add(key);
            else
                controlsDic.Add(action, new ListInt(key));
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
            if (controlsDic.ContainsKey(action))
            {
                controlsDic[action] = new ListInt(key);
                return true;
            }
            return false;
        }

        public bool Contain(string action) => controlsDic.ContainsKey(action);

        public ListInt GetKeys(string action)
        {
            if(controlsDic.TryGetValue(action, out ListInt key))
                return key;
            return new ListInt(0);
        }

        public InputData Clone() => new InputData(controlsDic);

        public IEnumerator<KeyValuePair<string, ListInt>> GetEnumerator()
        {
            return controlsDic.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return controlsDic.GetEnumerator();
        }

        public InputData ToGeneralGamepadInputData()
        {
            InputData res = new InputData();
            foreach (KeyValuePair<string, ListInt> item in controlsDic)
            {
                foreach(int input in item.Value)
                {
                    if (IsGamepadKey((InputKey)input))
                    {
                        res.AddAction(item.Key, (int)ConvertGamepadKeyToGeneralKey((GamepadKey)input));
                    }
                }
            }

            return res;
        }

        public InputData ToGamepadInputData(ControllerType gamepadIndex)
        {
            InputData res = new InputData();
            foreach (KeyValuePair<string, ListInt> item in controlsDic)
            {
                foreach (int key in item.Value)
                {
                    if (IsGamepadKey((InputKey)key))
                    {
                        res.AddAction(item.Key, (int)ConvertGeneralKeyToGamepadKey((GamepadKey)key, gamepadIndex));
                    }
                }
            }
            return res;
        }

        public InputData ToKeyboardInputData()
        {
            InputData res = new InputData();
            foreach (KeyValuePair<string, ListInt> item in controlsDic)
            {
                foreach (int key in item.Value)
                {
                    if (IsKeyboardKey((InputKey)key))
                    {
                        res.AddAction(item.Key, key);
                    }
                }
            }
            return res;
        }

        [Serializable]
        public class ListInt : IEnumerable<int>
        {
            public List<int> keys = new List<int>();
            public int Count => keys.Count;

            public ListInt() { }

            public ListInt(List<int> keys)
            {
                this.keys = keys;
            }

            public ListInt(int key)
            {
                keys = new List<int> { key };
            }

            public ListInt Clone() => new ListInt(keys.Clone());

            public IEnumerator<int> GetEnumerator() => keys.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => keys.GetEnumerator();
        }
    }

    #endregion

    #region Start

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Start()
    {
        PlayerLoopSystem defaultSystems = PlayerLoop.GetDefaultPlayerLoop();
        PlayerLoopSystem preUpdateystem = FindSubSystem<PreUpdate.AIUpdate>(defaultSystems);
        PlayerLoopSystem systemRoot = new PlayerLoopSystem();
        systemRoot.subSystemList = new PlayerLoopSystem[]
        {
            defaultSystems,
            preUpdateystem,
            new PlayerLoopSystem()
            {
                updateDelegate = PreUpdate,
                type = typeof(InputManager)
            },
        };
        PlayerLoop.SetPlayerLoop(systemRoot);
    }

    private static PlayerLoopSystem FindSubSystem<T>(PlayerLoopSystem def)
    {
        if (def.type == typeof(T))
        {
            return def;
        }
        if (def.subSystemList != null)
        {
            foreach (PlayerLoopSystem s in def.subSystemList)
            {
                PlayerLoopSystem system = FindSubSystem<T>(s);
                if (system.type == typeof(T))
                {
                    return system;
                }
            }
        }
        return default(PlayerLoopSystem);
    }

    #endregion

    #region Key Convertion

    public static GeneralGamepadKey ConvertGamepadKeyToGeneralKey(GamepadKey key)
    {
        int k = (int)key;
        if ((0 <= k && k <= 349) || (-75 <= k && k <= -61))
            return (GeneralGamepadKey)k;
        if (k < 0)
            return (GeneralGamepadKey)(k - ((k + 75) / 15) * 15);
        return (GeneralGamepadKey)(k - ((k - 330) / 20) * 20);
    }

    private static Dictionary<ControllerType, int> CalculateOffsetNegKey = new Dictionary<ControllerType, int>
    {
        { ControllerType.Gamepad1, 60 }, { ControllerType.Gamepad2, 45 }, { ControllerType.Gamepad3, 30 }, { ControllerType.Gamepad4, 15 }
    };

    private static Dictionary<ControllerType, int> CalculateOffsetPosKey = new Dictionary<ControllerType, int>
    {
        { ControllerType.Gamepad1, 20 }, { ControllerType.Gamepad2, 40 }, { ControllerType.Gamepad3, 60 }, { ControllerType.Gamepad4, 80 }
    };

    public static GamepadKey ConvertGeneralKeyToGamepadKey(GamepadKey key, ControllerType gamepadIndex)
    {
        int k = (int)key;
        if ((-60 <= k && k <= 0) || k >= 350)
            return key;

        if (k < 0)
            return (GamepadKey)(k + CalculateOffsetNegKey[gamepadIndex]);
        return (GamepadKey)(k + CalculateOffsetPosKey[gamepadIndex]);
    }

    private static InputKey ConvertToGeneralGamepadKey(InputKey key)
    {
        int key2 = (int)key;
        if (key2 < 0 && key2 >= -56)
            return (InputKey)(key2 - ((key2 / 14) * 14));
        if (key2 >= 350)
            return (InputKey)(key2 - (((key2 - 350) / 20) * 20));
        return key;
    }

    public static bool IsGamepadKey(InputKey key)
    {
        int key2 = (int)key;
        return key2 <= 0 || key2 >= 330;
    }

    public static bool IsKeyboardKey(InputKey key) => !IsGamepadKey(key) || key == InputKey.None;

    public static bool IsGeneralGamepadKey(InputKey key)
    {
        int k = (int)key;
        return k <= -57 || (k >= 0 && k <= 349);
    }

    #endregion

    #region GamePad only

    #region SetVibration

    private static void PrivateSetVibration(float rightIntensity, float leftIntensity, ControllerType gamepadIndex)
    {
        if (gamepadIndex == ControllerType.All || gamepadIndex == ControllerType.GamepadAll)
        {
            if (newGP1State.IsConnected)
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

    public static void SetVibration(float intensity, ControllerType gamepadIndex = ControllerType.GamepadAll)
    {
        SetVibration(intensity, intensity, gamepadIndex);
    }

    public static void SetVibration(float rightIntensity = 1f, float leftIntensity = 1f, ControllerType gamepadIndex = ControllerType.GamepadAll)
    {
        //Handheld.Vibrate();//unity build-in poor version

        if (gamepadIndex == ControllerType.Keyboard)
        {
            Debug.LogWarning("Cannot vibrate the keyboard!");
            return;
        }
        PrivateSetVibration(rightIntensity, leftIntensity, gamepadIndex);
    }

    public static void SetVibration(float rightIntensity, float leftIntensity, float duration, ControllerType gamepadIndex = ControllerType.GamepadAll)
    {
        if (gamepadIndex == ControllerType.Keyboard)
        {
            Debug.LogWarning("Cannot vibrate the keyboard!");
            return;
        }

        vibrationSettings.Add(new VibrationSetting(gamepadIndex, duration, rightIntensity, leftIntensity));
    }

    public static void SetVibration(float rightIntensity, float leftIntensity, float duration, float delay, ControllerType gamepadIndex = ControllerType.GamepadAll)
    {
        if (gamepadIndex == ControllerType.Keyboard)
        {
            Debug.LogWarning("Cannot vibrate the keyboard!");
            return;
        }

        VibrationSetting setting = new VibrationSetting(gamepadIndex, duration, rightIntensity, leftIntensity);
        setting.timer = -delay;
        vibrationSettings.Add(setting);
    }

    public static void StopVibration(ControllerType gamepadIndex = ControllerType.GamepadAll)
    {
        if (gamepadIndex == ControllerType.Keyboard)
        {
            Debug.LogWarning("Cannot vibrate the keyboard!");
            return;
        }
        PrivateSetVibration(0f, 0f, gamepadIndex);
    }

    public static void CancelVibration()
    {
        StopVibration();
        vibrationSettings.Clear();
    }

    #endregion

    #region SetStickPosition

    private static void SetNewGamepadSticksAndTriggersPositions()
    {
        //ThumbStick : si vraiPos.x/y€[-deadZone.x/y, deadZone.x/y] => pos.x/y = 0, vraiPos.x/y€[-1, -1 + deadZone.x/y] U [1 - deadZone.x/y, 1] => pos.x/y = (vraiPos.x/y).Sign() * 1, sinon pos.x/y = vraiPos.x/y
        void CalculateGamepadStickAndTrigger(ref Vector2 rStickPos, ref Vector2 lStickPos, ref Vector2 triggerPos, in Vector2 lDeadZone, in Vector2 rDeadZone, in Vector2 triggerDeadZone, in GamePadState newState)
        {
            float Regression(float min, float max, float value)
            {
                if (Mathf.Abs(value) <= min)
                    return 0f;
                if (Mathf.Abs(value) >= max)
                    return value.Sign();
                if (min < value && value < max)
                    return (value - min) / (max - min);
                return (value + min) / (max - min);
            }

            float x = Mathf.Abs(newState.ThumbSticks.Right.X) <= rDeadZone.x ? 0f : (Mathf.Abs(newState.ThumbSticks.Right.X) >= (1f - rDeadZone.x) ? newState.ThumbSticks.Right.X.Sign() : newState.ThumbSticks.Right.X);
            float y = Mathf.Abs(newState.ThumbSticks.Right.Y) <= rDeadZone.x ? 0f : (Mathf.Abs(newState.ThumbSticks.Right.Y) >= (1f - rDeadZone.y) ? newState.ThumbSticks.Right.Y.Sign() : newState.ThumbSticks.Right.Y);
            rStickPos = new Vector2(Regression(rDeadZone.x, 1f - rDeadZone.x, x), Regression(rDeadZone.y, 1f - rDeadZone.y, y));
            if(rStickPos.sqrMagnitude > 1f)
                rStickPos = new Vector2(newState.ThumbSticks.Right.X, newState.ThumbSticks.Right.Y).normalized;

            x = Mathf.Abs(newState.ThumbSticks.Left.X) <= lDeadZone.x ? 0f : (Mathf.Abs(newState.ThumbSticks.Left.X) >= (1f - lDeadZone.x) ? newState.ThumbSticks.Left.X.Sign() : newState.ThumbSticks.Left.X);
            y = Mathf.Abs(newState.ThumbSticks.Left.Y) <= lDeadZone.x ? 0f : (Mathf.Abs(newState.ThumbSticks.Left.Y) >= (1f - lDeadZone.y) ? newState.ThumbSticks.Left.Y.Sign() : newState.ThumbSticks.Left.Y);
            lStickPos = new Vector2(Regression(lDeadZone.x, 1f - lDeadZone.x, x), Regression(lDeadZone.y, 1f - lDeadZone.y, y));
            if (lStickPos.sqrMagnitude > 1f)
                lStickPos = new Vector2(newState.ThumbSticks.Left.X, newState.ThumbSticks.Left.Y).normalized;

            x = newState.Triggers.Left <= triggerDeadZone.x ? 0f : (newState.Triggers.Left >= 1f - triggerDeadZone.x ? 1f : newState.Triggers.Left);
            y = newState.Triggers.Right <= triggerDeadZone.y ? 0f : (newState.Triggers.Right >= 1f - triggerDeadZone.y ? 1f : newState.Triggers.Right);
            triggerPos = new Vector2(Regression(triggerDeadZone.x, 1f - triggerDeadZone.x, x), Regression(triggerDeadZone.y, 1f - triggerDeadZone.y, y));
        }

        CalculateGamepadStickAndTrigger(ref newGP1RightStickPosition, ref newGP1LeftStickPosition, ref newGP1Triggers, GP1LeftThumbStickDeadZone, GP1RightThumbStickDeadZone, GP1TriggersDeadZone, newGP1State);
        CalculateGamepadStickAndTrigger(ref newGP2RightStickPosition, ref newGP2LeftStickPosition, ref newGP2Triggers, GP2LeftThumbStickDeadZone, GP2RightThumbStickDeadZone, GP2TriggersDeadZone, newGP2State);
        CalculateGamepadStickAndTrigger(ref newGP3RightStickPosition, ref newGP3LeftStickPosition, ref newGP3Triggers, GP3LeftThumbStickDeadZone, GP3RightThumbStickDeadZone, GP3TriggersDeadZone, newGP3State);
        CalculateGamepadStickAndTrigger(ref newGP4RightStickPosition, ref newGP4LeftStickPosition, ref newGP4Triggers, GP4LeftThumbStickDeadZone, GP4RightThumbStickDeadZone, GP4TriggersDeadZone, newGP4State);
    }

    #endregion

    public static bool IsAGamepadController(ControllerType controllerType)
    {
        return controllerType == ControllerType.Gamepad1 || controllerType == ControllerType.Gamepad2 || controllerType == ControllerType.Gamepad3 || controllerType == ControllerType.Gamepad4;
    }

    public static Vector2 GetGamepadStickPosition(ControllerType gamepadIndex, GamepadStick GamepadStick)
    {
        switch (gamepadIndex)
        {
            case ControllerType.Keyboard:
                Debug.LogWarning("Can't return the stick position of a keyboard!");
                return Vector2.zero;
            case ControllerType.Gamepad1:
                if(IsGamePadConnected(gamepadIndex))
                    return GamepadStick == GamepadStick.right ? newGP1RightStickPosition : newGP1LeftStickPosition;
                Debug.LogWarning("Gamepad1 is not connected!");
                return Vector2.zero;
            case ControllerType.Gamepad2:
                if (IsGamePadConnected(gamepadIndex))
                    return GamepadStick == GamepadStick.right ? newGP2RightStickPosition : newGP2LeftStickPosition;
                Debug.LogWarning("Gamepad2 is not connected!");
                return Vector2.zero;
            case ControllerType.Gamepad3:
                if (IsGamePadConnected(gamepadIndex))
                    return GamepadStick == GamepadStick.right ? newGP3RightStickPosition : newGP3LeftStickPosition;
                Debug.LogWarning("Gamepad3 is not connected!");
                return Vector2.zero;
            case ControllerType.Gamepad4:
                if (IsGamePadConnected(gamepadIndex))
                    return GamepadStick == GamepadStick.right ? newGP4RightStickPosition : newGP4LeftStickPosition;
                Debug.LogWarning("Gamepad4 is not connected!");
                return Vector2.zero;
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
    public static bool IsGamePadConnected(ControllerType gamepadIndex)
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

    public static void ResetGamepadDeadzone()
    {
        GP1RightThumbStickDeadZone = defaultGP1RightThumbStickDeadZone;
        GP1LeftThumbStickDeadZone = defaultGP1LeftThumbStickDeadZone;
        GP2RightThumbStickDeadZone = defaultGP2RightThumbStickDeadZone;
        GP2LeftThumbStickDeadZone = defaultGP2LeftThumbStickDeadZone;
        GP3RightThumbStickDeadZone = defaultGP3RightThumbStickDeadZone;
        GP3LeftThumbStickDeadZone = defaultGP3LeftThumbStickDeadZone;
        GP4RightThumbStickDeadZone = defaultGP4RightThumbStickDeadZone;
        GP4LeftThumbStickDeadZone = defaultGP4LeftThumbStickDeadZone;
    }

    #endregion

    #region GetInputKey

    private static InputKey[] ToArray(this InputData.ListInt keys)
    {
        InputKey[] res = new InputKey[keys.Count];
        for (int i = 0; i < res.Length; i++)
        {
            res[i] = (InputKey)keys.keys[i];
        }
        return res;
    }

    public static InputKey[] GetInputKey(string action, PlayerIndex player)
    {
        switch (player)
        {
            case PlayerIndex.One:
                return player1Keys.GetKeys(action).ToArray();
            case PlayerIndex.Two:
                return player2Keys.GetKeys(action).ToArray();
            case PlayerIndex.Three:
                return player3Keys.GetKeys(action).ToArray();
            case PlayerIndex.Four:
                return player4Keys.GetKeys(action).ToArray();
            case PlayerIndex.Five:
                return player5Keys.GetKeys(action).ToArray();
            case PlayerIndex.All:
                return player1Keys.GetKeys(action).ToArray().Merge(player2Keys.GetKeys(action).ToArray().Merge(player3Keys.GetKeys(action).ToArray().Merge(player4Keys.GetKeys(action).ToArray().Merge(player5Keys.GetKeys(action).ToArray()))));
            default:
                return new InputKey[0];
        }
    }

    public static InputKey[] GetInputKey(string action, BaseController controller, bool defaultConfig = false)
    {
        if(controller == BaseController.Keyboard)
        {
            return defaultConfig ? defaultKBKeys.GetKeys(action).ToArray() : kbKeys.GetKeys(action).ToArray();
        }
        if (controller == BaseController.Gamepad)
        {
            return defaultConfig ? defaultGPKeys.GetKeys(action).ToArray() : gpKeys.GetKeys(action).ToArray();
        }
        return defaultConfig ? defaultKBKeys.GetKeys(action).ToArray().Merge(defaultGPKeys.GetKeys(action).ToArray()) :
            kbKeys.GetKeys(action).ToArray().Merge(gpKeys.GetKeys(action).ToArray());
    }

    #endregion

    #region GetKeyDown / GetKeyUp / GetKey

    private static bool GetNegativeKeyDown(int key)
    {
        return GetInputKeyDownDelegate[-key].Invoke();
    }

    private static bool GetNegativeKeyUp(int key)
    {
        return GetInputKeyUpDelegate[-key].Invoke();
    }

    private static bool GetNegativeKeyPressed(int key)
    {
        return GetInputKeyPressedDelegate[-key].Invoke();
    }

    /// <returns> true during the frame when the key assigned with the action is pressed</returns>
    public static bool GetKeyDown(string action, PlayerIndex player)
    {
        switch (player)
        {
            case PlayerIndex.One:
                return GetKeysDown(player1Keys.GetKeys(action));
            case PlayerIndex.Two:
                return GetKeysDown(player2Keys.GetKeys(action));
            case PlayerIndex.Three:
                return GetKeysDown(player3Keys.GetKeys(action));
            case PlayerIndex.Four:
                return GetKeysDown(player4Keys.GetKeys(action));
            case PlayerIndex.Five:
                return GetKeysDown(player5Keys.GetKeys(action));
            case PlayerIndex.All:
                return GetKeysDown(player1Keys.GetKeys(action)) ||
                    GetKeysDown(player2Keys.GetKeys(action)) ||
                    GetKeysDown(player3Keys.GetKeys(action)) ||
                    GetKeysDown(player4Keys.GetKeys(action)) ||
                    GetKeysDown(player5Keys.GetKeys(action));
            default:
                return false;
        }
    }

    /// <returns> true during the frame when the key assigned with the action is pressed</returns>
    public static bool GetKeyDown(string action, PlayerIndex player, out PlayerIndex playerWhoPressesDown)
    {
        playerWhoPressesDown = player;
        switch (player)
        {
            case PlayerIndex.One:
                return GetKeysDown(player1Keys.GetKeys(action));
            case PlayerIndex.Two:
                return GetKeysDown(player2Keys.GetKeys(action));
            case PlayerIndex.Three:
                return GetKeysDown(player3Keys.GetKeys(action));
            case PlayerIndex.Four:
                return GetKeysDown(player4Keys.GetKeys(action));
            case PlayerIndex.Five:
                return GetKeysDown(player5Keys.GetKeys(action));
            case PlayerIndex.All:
                if(GetKeysDown(player1Keys.GetKeys(action)))
                {
                    playerWhoPressesDown = PlayerIndex.One;
                    return true;
                }
                if(GetKeysDown(player2Keys.GetKeys(action)))
                {
                    playerWhoPressesDown = PlayerIndex.Two;
                    return true;
                }
                if(GetKeysDown(player3Keys.GetKeys(action)))
                {
                    playerWhoPressesDown = PlayerIndex.Three;
                    return true;
                }
                if(GetKeysDown(player4Keys.GetKeys(action)))
                {
                    playerWhoPressesDown = PlayerIndex.Four;
                    return true;
                }
                if(GetKeysDown(player5Keys.GetKeys(action)))
                {
                    playerWhoPressesDown = PlayerIndex.Five;
                    return true;
                }
                return false;
            default:
                return false;
        }
    }

    public static bool GetKeyDown(string action, BaseController controller)
    {
        if(controller == BaseController.Keyboard)
            return GetKeysDown(kbKeys.GetKeys(action));
        if (controller == BaseController.Gamepad)
            return GetKeysDown(gpKeys.GetKeys(action));
        return GetKeysDown(kbKeys.GetKeys(action)) || GetKeysDown(gpKeys.GetKeys(action));
    }

    /// <returns> true during the frame when the key assigned with the action is unpressed</returns>
    public static bool GetKeyUp(string action, PlayerIndex player)
    {
        switch (player)
        {
            case PlayerIndex.One:
                return GetKeysUp(player1Keys.GetKeys(action));
            case PlayerIndex.Two:
                return GetKeysUp(player2Keys.GetKeys(action));
            case PlayerIndex.Three:
                return GetKeysUp(player3Keys.GetKeys(action));
            case PlayerIndex.Four:
                return GetKeysUp(player4Keys.GetKeys(action));
            case PlayerIndex.Five:
                return GetKeysUp(player5Keys.GetKeys(action));
            case PlayerIndex.All:
                return GetKeysUp(player1Keys.GetKeys(action)) ||
                    GetKeysUp(player2Keys.GetKeys(action)) ||
                    GetKeysUp(player3Keys.GetKeys(action)) ||
                    GetKeysUp(player4Keys.GetKeys(action)) ||
                    GetKeysUp(player5Keys.GetKeys(action));
            default:
                return false;
        }
    }

    /// <returns> true during the frame when the key assigned with the action is pressed up</returns>
    public static bool GetKeyUp(string action, PlayerIndex player, out PlayerIndex playerWhoPressesUp)
    {
        playerWhoPressesUp = player;
        switch (player)
        {
            case PlayerIndex.One:
                return GetKeysUp(player1Keys.GetKeys(action));
            case PlayerIndex.Two:
                return GetKeysUp(player2Keys.GetKeys(action));
            case PlayerIndex.Three:
                return GetKeysUp(player3Keys.GetKeys(action));
            case PlayerIndex.Four:
                return GetKeysUp(player4Keys.GetKeys(action));
            case PlayerIndex.Five:
                return GetKeysUp(player5Keys.GetKeys(action));
            case PlayerIndex.All:
                if (GetKeysUp(player1Keys.GetKeys(action)))
                {
                    playerWhoPressesUp = PlayerIndex.One;
                    return true;
                }
                if (GetKeysUp(player2Keys.GetKeys(action)))
                {
                    playerWhoPressesUp = PlayerIndex.Two;
                    return true;
                }
                if (GetKeysUp(player3Keys.GetKeys(action)))
                {
                    playerWhoPressesUp = PlayerIndex.Three;
                    return true;
                }
                if (GetKeysUp(player4Keys.GetKeys(action)))
                {
                    playerWhoPressesUp = PlayerIndex.Four;
                    return true;
                }
                if (GetKeysUp(player5Keys.GetKeys(action)))
                {
                    playerWhoPressesUp = PlayerIndex.Five;
                    return true;
                }
                return false;
            default:
                return false;
        }
    }

    public static bool GetKeyUp(string action, BaseController controller)
    {
        if (controller == BaseController.Keyboard)
            return GetKeysUp(kbKeys.GetKeys(action));
        if (controller == BaseController.Gamepad)
            return GetKeysUp(gpKeys.GetKeys(action));
        return GetKeysUp(kbKeys.GetKeys(action)) || GetKeysUp(gpKeys.GetKeys(action));
    }

    /// <returns> true when the key assigned with the action is pressed</returns>
    public static bool GetKey(string action, PlayerIndex player)
    {
        switch (player)
        {
            case PlayerIndex.One:
                return GetKeys(player1Keys.GetKeys(action));
            case PlayerIndex.Two:
                return GetKeys(player2Keys.GetKeys(action));
            case PlayerIndex.Three:
                return GetKeys(player3Keys.GetKeys(action));
            case PlayerIndex.Four:
                return GetKeys(player4Keys.GetKeys(action));
            case PlayerIndex.Five:
                return GetKeys(player5Keys.GetKeys(action));
            case PlayerIndex.All:
                return GetKeys(player1Keys.GetKeys(action)) ||
                    GetKeys(player2Keys.GetKeys(action)) ||
                    GetKeys(player3Keys.GetKeys(action)) ||
                    GetKeys(player4Keys.GetKeys(action)) ||
                    GetKeys(player5Keys.GetKeys(action));
            default:
                return false;
        }
    }

    /// <returns> true during while the key assigned with the action is pressed</returns>
    public static bool GetKey(string action, PlayerIndex player, out PlayerIndex playerWhoPressed)
    {
        playerWhoPressed = player;
        switch (player)
        {
            case PlayerIndex.One:
                return GetKeys(player1Keys.GetKeys(action));
            case PlayerIndex.Two:
                return GetKeys(player2Keys.GetKeys(action));
            case PlayerIndex.Three:
                return GetKeys(player3Keys.GetKeys(action));
            case PlayerIndex.Four:
                return GetKeys(player4Keys.GetKeys(action));
            case PlayerIndex.Five:
                return GetKeys(player5Keys.GetKeys(action));
            case PlayerIndex.All:
                if (GetKeys(player1Keys.GetKeys(action)))
                {
                    playerWhoPressed = PlayerIndex.One;
                    return true;
                }
                if (GetKeys(player2Keys.GetKeys(action)))
                {
                    playerWhoPressed = PlayerIndex.Two;
                    return true;
                }
                if (GetKeys(player3Keys.GetKeys(action)))
                {
                    playerWhoPressed = PlayerIndex.Three;
                    return true;
                }
                if (GetKeys(player4Keys.GetKeys(action)))
                {
                    playerWhoPressed = PlayerIndex.Four;
                    return true;
                }
                if (GetKeys(player5Keys.GetKeys(action)))
                {
                    playerWhoPressed = PlayerIndex.Five;
                    return true;
                }
                return false;
            default:
                return false;
        }
    }

    public static bool GetKey(string action, BaseController controller)
    {
        if (controller == BaseController.Keyboard)
            return GetKeys(kbKeys.GetKeys(action));
        if (controller == BaseController.Gamepad)
            return GetKeys(gpKeys.GetKeys(action));
        return GetKeys(kbKeys.GetKeys(action)) || GetKeys(gpKeys.GetKeys(action));
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

    public static bool GetKeyDown(string[] actions, BaseController controller)
    {
        foreach (string action in actions)
        {
            if (GetKeyDown(action, controller))
                return true;
        }
        return false;
    }

    /// <returns> true during the frame when a key assigned with one of the actions is unpressed</returns>
    public static bool GetKeyUp(string[] actions, PlayerIndex player)
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

    public static bool GetKeyUp(string[] actions, BaseController controller)
    {
        foreach (string action in actions)
        {
            if (GetKeyUp(action, controller))
                return true;
        }
        return false;
    }

    /// <returns> true when a key assigned with one of the actions is pressed</returns>
    public static bool GetKey(string[] actions, PlayerIndex player)
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

    public static bool GetKey(string[] actions, BaseController controller)
    {
        foreach (string action in actions)
        {
            if (GetKey(action, controller))
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
    public static bool GetKeyDown(KeyboardKey key) => GetKeyDown((int)key);
    /// <returns> true during the frame when key is unpressed</returns>
    public static bool GetKeyUp(KeyboardKey key) => GetKeyUp((int)key);
    /// <returns> true when the key is pressed</returns>
    public static bool GetKey(KeyboardKey key) => GetKey((int)key);
    /// <returns> true during the frame when the key is pressed</returns>
    public static bool GetKeyDown(GamepadKey key) => GetKeyDown((int)key);
    /// <returns> true during the frame when key is unpressed</returns>
    public static bool GetKeyUp(GamepadKey key) => GetKeyUp((int)key);
    /// <returns> true when the key is pressed</returns>
    public static bool GetKey(GamepadKey key) => GetKey((int)key);
    /// <returns> true during the frame when the key is pressed</returns>
    public static bool GetKeyDown(GeneralGamepadKey key) => GetKeyDown((int)key);
    /// <returns> true during the frame when key is unpressed</returns>
    public static bool GetKeyUp(GeneralGamepadKey key) => GetKeyUp((int)key);
    /// <returns> true when the key is pressed</returns>
    public static bool GetKey(GeneralGamepadKey key) => GetKey((int)key);
    /// <returns> true during the frame when the key is pressed</returns>
    public static bool GetKeyDown(int key) => key >= 0 ? Input.GetKeyDown((KeyCode)key) : GetNegativeKeyDown(key);
    /// <returns> true during the frame when key is unpressed</returns>
    public static bool GetKeyUp(int key) => key >= 0 ? Input.GetKeyUp((KeyCode)key) : GetNegativeKeyUp(key);
    /// <returns> true when the key is pressed</returns>
    public static bool GetKey(int key) => key >= 0 ? Input.GetKey((KeyCode)key) : GetNegativeKeyPressed(key);

    private static bool GetKeysDown(InputData.ListInt keys)
    {
        foreach (int key in keys.keys)
        {
            if (GetKeyDown(key))
                return true;
        }
        return false;
    }

    private static bool GetKeysUp(InputData.ListInt keys)
    {
        foreach (int key in keys.keys)
        {
            if (GetKeyUp(key))
                return true;
        }
        return false;
    }

    private static bool GetKeys(InputData.ListInt keys)
    {
        foreach (int key in keys.keys)
        {
            if (GetKey(key))
                return true;
        }
        return false;
    }

    #endregion

    #region Management controller

    #region Add/Replace/Remove action

    public static void AddInputAction(string action, KeyboardKey key, bool defaultConfig = false)
    {
        if(defaultConfig)
            defaultKBKeys.AddAction(action, (int)key);
        else
            kbKeys.AddAction(action, (int)key);
    }

    public static void AddInputsAction(string action, KeyboardKey[] keys, bool defaultConfig = false)
    {
        foreach (KeyboardKey key in keys)
        {
            AddInputAction(action, key, defaultConfig);
        }
    }

    public static void AddInputsActions(string[] actions, KeyboardKey[] keys, bool defaultConfig = false)
    {
        if(actions.Length != keys.Length)
        {
            Debug.LogWarning("actions and keys must have the same length! actions.length : " + actions.Length + " , keys.Length : " + keys.Length);
            return;
        }

        for (int i = 0; i < actions.Length; i++)
        {
            AddInputAction(actions[i], keys[i], defaultConfig);
        }
    }

    public static void AddInputAction(string action, GamepadKey key, bool defaultConfig = false)
    {
        if (defaultConfig)
            defaultGPKeys.AddAction(action, (int)key);
        else
            gpKeys.AddAction(action, (int)key);
    }

    public static void AddInputsAction(string action, GamepadKey[] keys, bool defaultConfig = false)
    {
        foreach (GamepadKey key in keys)
        {
            AddInputAction(action, key, defaultConfig);
        }
    }

    public static void AddInputsActions(string[] actions, GamepadKey[] keys, bool defaultConfig = false)
    {
        if (actions.Length != keys.Length)
        {
            Debug.LogWarning("actions and keys must have the same length! actions.length : " + actions.Length + " , keys.Length : " + keys.Length);
            return;
        }

        for (int i = 0; i < actions.Length; i++)
        {
            AddInputAction(actions[i], keys[i], defaultConfig);
        }
    }

    public static void AddInputAction(string action, GeneralGamepadKey key, bool defaultConfig = false)
    {
        if (defaultConfig)
            defaultGPKeys.AddAction(action, (int)key);
        else
            gpKeys.AddAction(action, (int)key);
    }

    public static void AddInputsAction(string action, GeneralGamepadKey[] keys, bool defaultConfig = false)
    {
        foreach (GeneralGamepadKey key in keys)
        {
            AddInputAction(action, key, defaultConfig);
        }
    }

    public static void AddInputsActions(string[] actions, GeneralGamepadKey[] keys, bool defaultConfig = false)
    {
        if (actions.Length != keys.Length)
        {
            Debug.LogWarning("actions and keys must have the same length! actions.length : " + actions.Length + " , keys.Length : " + keys.Length);
            return;
        }

        for (int i = 0; i < actions.Length; i++)
        {
            AddInputAction(actions[i], keys[i], defaultConfig);
        }
    }

    public static void AddInputAction(string action, InputKey key, BaseController controller, bool defaultConfig = false)
    {
        InputData kb = defaultConfig ? defaultKBKeys : kbKeys;
        InputData gp = defaultConfig ? defaultGPKeys : gpKeys;

        if (controller == BaseController.Keyboard)
        {
            if (IsKeyboardKey(key))
                kb.AddAction(action, (int)key);
            else
                Debug.LogWarning("Can't add " + KeyToString(key) + " to a keyboard controller because it's not a keyboard key!");
            return;
        }
        if (controller == BaseController.Gamepad)
        {
            if (IsGamepadKey(key))
                gp.AddAction(action, (int)ConvertToGeneralGamepadKey(key));
            else
                Debug.LogWarning("Can't add " + KeyToString(key) + " to a gamepad controller because it's not a gamepad key!");
            return;
        }
        if (IsKeyboardKey(key))
            kb.AddAction(action, (int)key);
        else
            Debug.LogWarning("Can't add " + KeyToString(key) + " to a keyboard controller because it's not a keyboard key!");
        if (IsGamepadKey(key))
            gp.AddAction(action, (int)ConvertToGeneralGamepadKey(key));
        else
            Debug.LogWarning("Can't add " + KeyToString(key) + " to a gamepad controller because it's not a gamepad key!");
    }

    public static void AddInputsAction(string action, InputKey[] keys, BaseController controller, bool defaultConfig = false)
    {
        foreach (InputKey key in keys)
        {
            AddInputAction(action, key, controller, defaultConfig);
        }
    }

    public static void AddInputsActions(string[] actions, InputKey[] keys, BaseController baseController, bool defaultConfig = false)
    {
        if (actions.Length != keys.Length)
        {
            Debug.LogWarning("actions and keys must have the same length! actions.length : " + actions.Length + " , keys.Length : " + keys.Length);
            return;
        }

        for (int i = 0; i < actions.Length; i++)
        {
            AddInputAction(actions[i], keys[i], baseController, defaultConfig);
        }
    }

    public static void AddInputAction(string action, KeyboardKey key, BaseController controller, bool defaultConfig = false)
    {
        AddInputAction(action, key, defaultConfig);
    }

    public static void AddInputsAction(string action, KeyboardKey[] keys, BaseController controller, bool defaultConfig = false)
    {
        AddInputsAction(action, keys, defaultConfig);   
    }

    public static void AddInputsActions(string[] actions, KeyboardKey[] keys, BaseController controller, bool defaultConfig = false)
    {
        AddInputsActions(actions, keys, defaultConfig);
    }

    public static void AddInputAction(string action, GamepadKey key, BaseController controller, bool defaultConfig = false)
    {
        AddInputAction(action, key, defaultConfig);
    }

    public static void AddInputsAction(string action, GamepadKey[] keys, BaseController controller, bool defaultConfig = false)
    {
        AddInputsAction(action, keys, defaultConfig);
    }

    public static void AddInputsActions(string[] actions, GamepadKey[] keys, BaseController controller, bool defaultConfig = false)
    {
        AddInputsActions(actions, keys, defaultConfig);
    }

    public static void AddInputAction(string action, GeneralGamepadKey key, BaseController controller, bool defaultConfig = false)
    {
        AddInputAction(action, key, defaultConfig);
    }

    public static void AddInputsAction(string action, GeneralGamepadKey[] keys, BaseController controller, bool defaultConfig = false)
    {
        AddInputsAction(action, keys, defaultConfig);
    }

    public static void AddInputsActions(string[] actions, GeneralGamepadKey[] keys, BaseController controller, bool defaultConfig = false)
    {
        AddInputsActions(actions, keys, defaultConfig);
    }

    /// <summary>
    /// Add an action to the InputManager system. Multiply action can have the same key.
    /// </summary>
    /// <param name="action"> The action</param>
    /// <param name="keyboardKey"> The keyboard key link with the action</param>
    public static void AddInputAction(string action, InputKey key, PlayerIndex player)
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

    public static void AddInputsAction(string action, InputKey[] keys, PlayerIndex player)
    {
        foreach (InputKey key in keys)
        {
            AddInputAction(action, key, player);
        }
    }

    public static void AddInputsActions(string[] actions, InputKey[] keys, PlayerIndex player)
    {
        if (actions.Length != keys.Length)
        {
            Debug.LogWarning("actions and keys must have the same length! actions.length : " + actions.Length + " , keys.Length : " + keys.Length);
            return;
        }

        for (int i = 0; i < actions.Length; i++)
        {
            AddInputAction(actions[i], keys[i], player);
        }
    }

    public static void AddInputAction(string action, KeyCode key, PlayerIndex player)
    {
        AddInputAction(action, (InputKey)key, player);
    }

    public static void AddInputsAction(string action, KeyCode[] keys, PlayerIndex player)
    {
        foreach (InputKey key in keys)
        {
            AddInputAction(action, key, player);
        }
    }

    public static void AddInputsActions(string[] actions, KeyCode[] keys, PlayerIndex player)
    {
        if (actions.Length != keys.Length)
        {
            Debug.LogWarning("actions and keys must have the same length! actions.length : " + actions.Length + " , keys.Length : " + keys.Length);
            return;
        }

        for (int i = 0; i < actions.Length; i++)
        {
            AddInputAction(actions[i], keys[i], player);
        }
    }

    public static void AddInputAction(string action, KeyboardKey key, PlayerIndex player)
    {
        AddInputAction(action, (InputKey)key, player);
    }

    public static void AddInputsAction(string action, KeyboardKey[] keys, PlayerIndex player)
    {
        foreach (InputKey key in keys)
        {
            AddInputAction(action, key, player);
        }
    }

    public static void AddInputsActions(string[] actions, KeyboardKey[] keys, PlayerIndex player)
    {
        if (actions.Length != keys.Length)
        {
            Debug.LogWarning("actions and keys must have the same length! actions.length : " + actions.Length + " , keys.Length : " + keys.Length);
            return;
        }

        for (int i = 0; i < actions.Length; i++)
        {
            AddInputAction(actions[i], keys[i], player);
        }
    }

    public static void AddInputAction(string action, GamepadKey key, PlayerIndex player)
    {
        AddInputAction(action, (InputKey)key, player);
    }

    public static void AddInputsAction(string action, GamepadKey[] keys, PlayerIndex player)
    {
        foreach (InputKey key in keys)
        {
            AddInputAction(action, key, player);
        }
    }

    public static void AddInputsActions(string[] actions, GamepadKey[] keys, PlayerIndex player)
    {
        if (actions.Length != keys.Length)
        {
            Debug.LogWarning("actions and keys must have the same length! actions.length : " + actions.Length + " , keys.Length : " + keys.Length);
            return;
        }

        for (int i = 0; i < actions.Length; i++)
        {
            AddInputAction(actions[i], (InputKey)keys[i], player);
        }
    }

    public static void AddInputAction(string action, GeneralGamepadKey key, PlayerIndex player)
    {
        AddInputAction(action, (InputKey)key, player);
    }

    public static void AddInputsAction(string action, GeneralGamepadKey[] keys, PlayerIndex player)
    {
        foreach (InputKey key in keys)
        {
            AddInputAction(action, key, player);
        }
    }

    public static void AddInputsActions(string[] actions, GeneralGamepadKey[] keys, PlayerIndex player)
    {
        if (actions.Length != keys.Length)
        {
            Debug.LogWarning("actions and keys must have the same length! actions.length : " + actions.Length + " , keys.Length : " + keys.Length);
            return;
        }

        for (int i = 0; i < actions.Length; i++)
        {
            AddInputAction(actions[i], keys[i], player);
        }
    }

    /// <summary>
    /// Change the keyboard key assigned to the action in param
    /// </summary>
    public static bool ReplaceAction(string action, InputKey newKey, PlayerIndex player)
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
                return b1 && b2 && b3 && b4 && b5;
            default:
                return false;
        }
    }

    public static bool ReplaceAction(string action, InputKey[] newKeys, PlayerIndex player)
    {
        if(newKeys.Length <= 0)
            return false;

        bool b = ReplaceAction(action, newKeys[0], player);
        if(b)
        {
            for (int i = 1; i < newKeys.Length; i++)
            {
                AddInputAction(action, newKeys[i], player);
            }
        }
        return b;
    }

    public static bool ReplaceAction(string action, KeyCode newKey, PlayerIndex player) => ReplaceAction(action, (InputKey)newKey, player);
    public static bool ReplaceAction(string action, KeyCode[] newKeys, PlayerIndex player)
    {
        if (newKeys.Length <= 0)
            return false;

        bool b = ReplaceAction(action, newKeys[0], player);
        if (b)
        {
            for (int i = 1; i < newKeys.Length; i++)
            {
                AddInputAction(action, newKeys[i], player);
            }
        }
        return b;
    }

    public static bool ReplaceAction(string action, KeyboardKey newKey, PlayerIndex player) => ReplaceAction(action, (InputKey)newKey, player);
    public static bool ReplaceAction(string action, KeyboardKey[] newKeys, PlayerIndex player)
    {
        if (newKeys.Length <= 0)
            return false;

        bool b = ReplaceAction(action, newKeys[0], player);
        if (b)
        {
            for (int i = 1; i < newKeys.Length; i++)
            {
                AddInputAction(action, newKeys[i], player);
            }
        }
        return b;
    }

    public static bool ReplaceAction(string action, GamepadKey newKey, PlayerIndex player) => ReplaceAction(action, (InputKey)newKey, player);
    public static bool ReplaceAction(string action, GamepadKey[] newKeys, PlayerIndex player)
    {
        if (newKeys.Length <= 0)
            return false;

        bool b = ReplaceAction(action, newKeys[0], player);
        if (b)
        {
            for (int i = 1; i < newKeys.Length; i++)
            {
                AddInputAction(action, newKeys[i], player);
            }
        }
        return b;
    }

    public static bool ReplaceAction(string action, InputKey newKey, BaseController controller, bool defaultConfig = false)
    {
        InputData kb = defaultConfig ? defaultKBKeys : kbKeys;
        InputData gp = defaultConfig ? defaultGPKeys : gpKeys;

        if (controller == BaseController.Keyboard)
        {
            if (IsKeyboardKey(newKey))
                return kb.ReplaceAction(action, (int)newKey);
            return false;
        }
        if (controller == BaseController.Gamepad)
        {
            if (IsGamepadKey(newKey))
                return gp.ReplaceAction(action, (int)ConvertToGeneralGamepadKey(newKey));
            return false;
        }
        bool b = false;
        if (IsKeyboardKey(newKey))
            b = kb.ReplaceAction(action, (int)newKey);
        if (IsGamepadKey(newKey))
            b = gp.ReplaceAction(action, (int)ConvertToGeneralGamepadKey(newKey)) && b;
        return b;
    }

    public static bool ReplaceAction(string action, InputKey[] newKeys, BaseController controller, bool defaultConfig = false)
    {
        if (newKeys.Length <= 0)
            return false;

        bool b = ReplaceAction(action, newKeys[0], controller, defaultConfig);
        if (b)
        {
            for (int i = 1; i < newKeys.Length; i++)
            {
                AddInputAction(action, newKeys[i], controller, defaultConfig);
            }
        }
        return b;
    }

    public static bool ReplaceAction(string action, KeyCode newKey, BaseController controller, bool defaultConfig = false) => ReplaceAction(action, (InputKey)newKey, controller, defaultConfig);
    public static bool ReplaceAction(string action, KeyCode[] newKeys, BaseController controller, bool defaultConfig = false)
    {
        if (newKeys.Length <= 0)
            return false;

        bool b = ReplaceAction(action, newKeys[0], controller, defaultConfig);
        if (b)
        {
            for (int i = 1; i < newKeys.Length; i++)
            {
                AddInputAction(action, (InputKey)newKeys[i], controller, defaultConfig);
}
        }
        return b;
    }

    public static bool ReplaceAction(string action, KeyboardKey newKey, BaseController controller, bool defaultConfig = false) => ReplaceAction(action, (InputKey)newKey, controller, defaultConfig);
    public static bool ReplaceAction(string action, KeyboardKey[] newKeys, BaseController controller, bool defaultConfig = false)
    {
        if (newKeys.Length <= 0)
            return false;

        bool b = ReplaceAction(action, newKeys[0], controller, defaultConfig);
        if (b)
        {
            for (int i = 1; i < newKeys.Length; i++)
            {
                AddInputAction(action, (InputKey)newKeys[i], controller, defaultConfig);
            }
        }
        return b;
    }

    public static bool ReplaceAction(string action, GamepadKey newKey, BaseController controller, bool defaultConfig = false) => ReplaceAction(action, (InputKey)newKey, controller, defaultConfig);
    public static bool ReplaceAction(string action, GamepadKey[] newKeys, BaseController controller, bool defaultConfig = false)
    {
        if (newKeys.Length <= 0)
            return false;

        bool b = ReplaceAction(action, newKeys[0], controller, defaultConfig);
        if (b)
        {
            for (int i = 1; i < newKeys.Length; i++)
            {
                AddInputAction(action, (InputKey)newKeys[i], controller, defaultConfig);
            }
        }
        return b;
    }

    /// <summary>
    /// Remove the action from the InputManager system
    /// </summary>
    /// <param name="action"> The action to remove.</param>
    /// <param name="controllerType">The controller where the action will be removed.</param>
    public static bool RemoveAction(string action, PlayerIndex player)
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
                return b1 && b2 && b3 && b4 && b5;
            default:
                return false;
        }
    }

    public static bool RemoveAction(string action, BaseController controller, bool defaultConfig = false)
    {
        InputData kb = defaultConfig ? defaultKBKeys : kbKeys;
        InputData gp = defaultConfig ? defaultGPKeys : gpKeys;

        if (controller == BaseController.Keyboard)
            return kb.RemoveAction(action);
        if (controller == BaseController.Gamepad)
            return gp.RemoveAction(action);
        bool b = kb.RemoveAction(action);
        return gp.RemoveAction(action) && b;
    }

    #endregion

    public static bool ActionExist(string action, PlayerIndex playerIndex)
    {
        switch (playerIndex)
        {
            case PlayerIndex.One:
                return player1Keys.Contain(action);
            case PlayerIndex.Two:
                return player2Keys.Contain(action);
            case PlayerIndex.Three:
                return player3Keys.Contain(action);
            case PlayerIndex.Four:
                return player4Keys.Contain(action);
            case PlayerIndex.Five:
                return player5Keys.Contain(action);
            case PlayerIndex.All:
                return player1Keys.Contain(action) && player2Keys.Contain(action) && player3Keys.Contain(action) && player4Keys.Contain(action) && player5Keys.Contain(action);
            default:
                return false;
        }
    }

    public static bool ActionExist(string action, BaseController baseController, bool defaultConfig = false)
    {
        switch (baseController)
        {
            case BaseController.Keyboard:
                return defaultConfig ? defaultKBKeys.Contain(action) : kbKeys.Contain(action);
            case BaseController.Gamepad:
                return defaultConfig ? defaultGPKeys.Contain(action) : gpKeys.Contain(action);
            case BaseController.KeyboardAndGamepad:
                return defaultConfig ? (defaultKBKeys.Contain(action) && defaultGPKeys.Contain(action)) : (kbKeys.Contain(action) && gpKeys.Contain(action));
            default:
                return false;
        }
    }

    public static void ClearAll()
    {
        ClearAllController();
        ClearDeadZone();
    }

    public static void ClearAllController()
    {
        ClearCurrentController();
        ClearDefaultController();
    }

    public static void ClearCurrentController()
    {
        player1Keys.Clear();
        player2Keys.Clear();
        player3Keys.Clear();
        player4Keys.Clear();
        player5Keys.Clear(); ;
        kbKeys.Clear();
        gpKeys.Clear();
    }

    public static void ClearDefaultController()
    {
        defaultKBKeys.Clear();
        defaultGPKeys.Clear();
    }

    public static void ClearPlayerController(PlayerIndex playerIndex)
    {
        switch (playerIndex)
        {
            case PlayerIndex.One:
                player1Keys.Clear();
                break;
            case PlayerIndex.Two:
                player2Keys.Clear();
                break;
            case PlayerIndex.Three:
                player3Keys.Clear();
                break;
            case PlayerIndex.Four:
                player4Keys.Clear();
                break;
            case PlayerIndex.Five:
                player5Keys.Clear();
                break;
            case PlayerIndex.All:
                player1Keys.Clear();
                player2Keys.Clear();
                player3Keys.Clear();
                player4Keys.Clear();
                player5Keys.Clear();
                break;
            default:
                break;
        }
    }

    public static void ClearCurrentController(BaseController controller, bool defaultTo = false)
    {
        if (controller == BaseController.Keyboard)
        {
            kbKeys.Clear();
            if (defaultTo)
                defaultKBKeys.Clear();
            return;
        }
        if (controller == BaseController.Gamepad)
        {
            gpKeys.Clear();
            if (defaultTo)
                defaultGPKeys.Clear();
            return;
        }
        kbKeys.Clear();
        gpKeys.Clear();
        if (defaultTo)
        {
            defaultKBKeys.Clear();
            defaultGPKeys.Clear();
        }
    }

    public static void ClearDeadZone()
    {
        GP1RightThumbStickDeadZone = defaultGP1RightThumbStickDeadZone;
        GP1LeftThumbStickDeadZone = defaultGP1LeftThumbStickDeadZone;
        GP1TriggersDeadZone = defaultGP1TriggersDeadZone;
        GP2RightThumbStickDeadZone = defaultGP2RightThumbStickDeadZone;
        GP2LeftThumbStickDeadZone = defaultGP2LeftThumbStickDeadZone;
        GP2TriggersDeadZone = defaultGP2TriggersDeadZone;
        GP3RightThumbStickDeadZone = defaultGP3RightThumbStickDeadZone;
        GP3LeftThumbStickDeadZone = defaultGP3LeftThumbStickDeadZone;
        GP3TriggersDeadZone = defaultGP3TriggersDeadZone;
        GP4RightThumbStickDeadZone = defaultGP4RightThumbStickDeadZone;
        GP4LeftThumbStickDeadZone = defaultGP4LeftThumbStickDeadZone;
        GP4TriggersDeadZone = defaultGP4TriggersDeadZone;
    }

    public static void ClearDeadZone(ControllerType gamepadIndex)
    {
        switch (gamepadIndex)
        {
            case ControllerType.Keyboard:
                Debug.Log("Can't mdify the deadzone of a keyboard!");
                break;
            case ControllerType.Gamepad1:
                GP1RightThumbStickDeadZone = defaultGP1RightThumbStickDeadZone;
                GP1LeftThumbStickDeadZone = defaultGP1LeftThumbStickDeadZone;
                GP1TriggersDeadZone = defaultGP1TriggersDeadZone;
                break;
            case ControllerType.Gamepad2:
                GP2RightThumbStickDeadZone = defaultGP2RightThumbStickDeadZone;
                GP2LeftThumbStickDeadZone = defaultGP2LeftThumbStickDeadZone;
                GP2TriggersDeadZone = defaultGP2TriggersDeadZone;
                break;
            case ControllerType.Gamepad3:
                GP3RightThumbStickDeadZone = defaultGP3RightThumbStickDeadZone;
                GP3LeftThumbStickDeadZone = defaultGP3LeftThumbStickDeadZone;
                GP3TriggersDeadZone = defaultGP3TriggersDeadZone;
                break;
            case ControllerType.Gamepad4:
                GP4RightThumbStickDeadZone = defaultGP4RightThumbStickDeadZone;
                GP4LeftThumbStickDeadZone = defaultGP4LeftThumbStickDeadZone;
                GP4TriggersDeadZone = defaultGP4TriggersDeadZone;
                break;
            case ControllerType.GamepadAll:
                ClearDeadZone();
                break;
            case ControllerType.All:
                ClearDeadZone();
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Set the default Control as the current configuration of a player
    /// </summary>
    /// <param name="controllerType"></param>
    /// <param name="gamepadIndex"></param>
    public static void SetDefaultController(BaseController controller)
    {
        if(controller == BaseController.Keyboard)
        {
            defaultKBKeys = kbKeys.ToKeyboardInputData();
            return;
        }
        if(controller == BaseController.Gamepad)
        {
            defaultGPKeys = gpKeys.ToGeneralGamepadInputData();
            return;
        }
        SetDefaultController();
    }

    public static void SetDefaultController()
    {
        defaultKBKeys = kbKeys.ToKeyboardInputData();
        defaultGPKeys = gpKeys.ToGeneralGamepadInputData();
    }

    public static void SetDefaultController(BaseController controller, PlayerIndex player)
    {
        InputData inputs = null;
        switch (player)
        {
            case PlayerIndex.One:
                inputs = player1Keys;
                break;
            case PlayerIndex.Two:
                inputs = player2Keys;
                break;
            case PlayerIndex.Three:
                inputs = player3Keys;
                break;
            case PlayerIndex.Four:
                inputs = player4Keys;
                break;
            case PlayerIndex.Five:
                inputs = player5Keys;
                break;
            case PlayerIndex.All:
                Debug.LogWarning("Can't set default control from multiple sources");
                break;
            default:
                break;
        }

        if(controller == BaseController.Keyboard)
        {
            defaultKBKeys = inputs.ToKeyboardInputData();
            return;
        }
        if (controller == BaseController.Keyboard)
        {
            defaultGPKeys = inputs.ToGeneralGamepadInputData();
            return;
        }
        defaultKBKeys = inputs.ToKeyboardInputData();
        defaultGPKeys = inputs.ToGeneralGamepadInputData();
    }

    /// <summary>
    /// Set the current Configuration of a player.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="controller"></param>
    public static void SetCurrentController(PlayerIndex player, BaseController controller, bool defaultConfig = false)
    {
        InputData inputs = null;
        switch (controller)
        {
            case BaseController.Keyboard:
                inputs = defaultConfig ? defaultKBKeys : kbKeys;
                break;
            case BaseController.Gamepad:
                inputs = (defaultConfig ? defaultGPKeys : gpKeys).ToGeneralGamepadInputData();
                break;
            case BaseController.KeyboardAndGamepad:
                Debug.LogWarning("Can't load multiple inputs system for one player!");
                return;
            default:
                return;
        }

        switch (player)
        {
            case PlayerIndex.One:
                player1Keys = inputs.Clone();
                break;
            case PlayerIndex.Two:
                player2Keys = inputs.Clone();
                break;
            case PlayerIndex.Three:
                player3Keys = inputs.Clone();
                break;
            case PlayerIndex.Four:
                player4Keys = inputs.Clone();
                break;
            case PlayerIndex.Five:
                player5Keys = inputs.Clone();
                break;
            case PlayerIndex.All:
                player1Keys = inputs.Clone();
                player2Keys = inputs.Clone();
                player3Keys = inputs.Clone();
                player4Keys = inputs.Clone();
                player5Keys = inputs.Clone();
                break;
            default:
                return;
        }
    }

    public static void SetCurrentController(BaseController controller)
    {
        if(controller == BaseController.Keyboard)
        {
            kbKeys = defaultKBKeys.Clone();
            return;
        }
        if(controller == BaseController.Gamepad)
        {
            gpKeys = defaultGPKeys.Clone().ToGeneralGamepadInputData();
            return;
        }
        kbKeys = defaultKBKeys.Clone();
        gpKeys = defaultGPKeys.Clone().ToGeneralGamepadInputData();
    }

    public static void SetCurrentControllerForGamepad(PlayerIndex player, ControllerType gamepadIndex, bool defaultConfig = false)
    {
        if (gamepadIndex == ControllerType.All || gamepadIndex == ControllerType.GamepadAll)
        {
            Debug.Log("Can't set the " + player.ToString() + " controllers for Gamepad with multiple defaults controllers!");
            return;
        }
        if (gamepadIndex == ControllerType.Keyboard)
        {
            Debug.Log("Can't set the " + player.ToString() + " controller for Gamepad with the keyboard default controller!");
            return;
        }

        InputData inputs = (defaultConfig ? defaultGPKeys : gpKeys).ToGamepadInputData(gamepadIndex);

        switch (player)
        {
            case PlayerIndex.One:
                player1Keys = inputs.Clone();
                break;
            case PlayerIndex.Two:
                player2Keys = inputs.Clone();
                break;
            case PlayerIndex.Three:
                player3Keys = inputs.Clone();
                break;
            case PlayerIndex.Four:
                player4Keys = inputs.Clone();
                break;
            case PlayerIndex.Five:
                player5Keys = inputs.Clone();
                break;
            case PlayerIndex.All:
                player1Keys = inputs.Clone();
                player2Keys = inputs.Clone();
                player3Keys = inputs.Clone();
                player4Keys = inputs.Clone();
                player5Keys = inputs.Clone();
                break;
            default:
                return;
        }
    }

    public static void SwitchController(PlayerIndex player1, PlayerIndex player2)
    {
        InputData GetInputData(PlayerIndex player)
        {
            switch (player)
            {
                case PlayerIndex.One:
                    return player1Keys;
                case PlayerIndex.Two:
                    return player2Keys;
                case PlayerIndex.Three:
                    return player3Keys;
                case PlayerIndex.Four:
                    return player4Keys;
                case PlayerIndex.Five:
                    return player5Keys;
                default:
                    Debug.LogWarning("Cannot switch the controller of multiple player controller!");
                    return null;
            }
        }

        InputData p1Data = GetInputData(player1);
        InputData p2Data = GetInputData(player2);
        if (p1Data == null || p2Data == null)
            return;

        InputData tmp = p1Data;
        p1Data = p2Data;
        p2Data = tmp;
    }

    #endregion

    #region SaveController

    [Serializable]
    private struct InputManagerConfigData
    {
        public InputData defaultKBKeys;
        public InputData defaultGPKeys;
        public InputData player1Keys;
        public InputData player2Keys;
        public InputData player3Keys;
        public InputData player4Keys;
        public InputData player5Keys;
        public InputData kbKeys;
        public InputData gpKeys;

        public Vector2 GP1RightThumbStickDeadZone, GP1LeftThumbStickDeadZone, GP1TriggersDeadZone;
        public Vector2 GP2RightThumbStickDeadZone, GP2LeftThumbStickDeadZone, GP2TriggersDeadZone;
        public Vector2 GP3RightThumbStickDeadZone, GP3LeftThumbStickDeadZone, GP3TriggersDeadZone;
        public Vector2 GP4RightThumbStickDeadZone, GP4LeftThumbStickDeadZone, GP4TriggersDeadZone;

        public InputManagerConfigData(InputData defaultKBKeys, InputData defaultGPKeys, InputData player1Keys, InputData player2Keys, InputData player3Keys, InputData player4Keys,
            InputData player5Keys, InputData kbKeys, InputData gpKeys, Vector2 gP1RightThumbStickDeadZone, Vector2 gP1LeftThumbStickDeadZone, Vector2 gP1TriggersDeadZone,
            Vector2 gP2RightThumbStickDeadZone, Vector2 gP2LeftThumbStickDeadZone, Vector2 gP2TriggersDeadZone, Vector2 gP3RightThumbStickDeadZone, Vector2 gP3LeftThumbStickDeadZone,
            Vector2 gP3TriggersDeadZone, Vector2 gP4RightThumbStickDeadZone, Vector2 gP4LeftThumbStickDeadZone, Vector2 gP4TriggersDeadZone)
        {
            this.defaultKBKeys = defaultKBKeys;
            this.defaultGPKeys = defaultGPKeys;
            this.player1Keys = player1Keys;
            this.player2Keys = player2Keys;
            this.player3Keys = player3Keys;
            this.player4Keys = player4Keys;
            this.player5Keys = player5Keys;
            this.kbKeys = kbKeys;
            this.gpKeys = gpKeys;
            GP1RightThumbStickDeadZone = gP1RightThumbStickDeadZone;
            GP1LeftThumbStickDeadZone = gP1LeftThumbStickDeadZone;
            GP1TriggersDeadZone = gP1TriggersDeadZone;
            GP2RightThumbStickDeadZone = gP2RightThumbStickDeadZone;
            GP2LeftThumbStickDeadZone = gP2LeftThumbStickDeadZone;
            GP2TriggersDeadZone = gP2TriggersDeadZone;
            GP3RightThumbStickDeadZone = gP3RightThumbStickDeadZone;
            GP3LeftThumbStickDeadZone = gP3LeftThumbStickDeadZone;
            GP3TriggersDeadZone = gP3TriggersDeadZone;
            GP4RightThumbStickDeadZone = gP4RightThumbStickDeadZone;
            GP4LeftThumbStickDeadZone = gP4LeftThumbStickDeadZone;
            GP4TriggersDeadZone = gP4TriggersDeadZone;
        }
    }

    /// <summary>
    /// Save all the current InputManager configuration (default and current actions and controllers keys link to the action) for all players in the file in param,
    /// can be load using the methode InputManager.LoadConfiguration(string fileName).
    /// </summary>
    public static bool SaveConfiguration(string fileName)
    {
        defaultKBKeys.UnBuild(); defaultGPKeys.UnBuild();kbKeys.UnBuild();gpKeys.UnBuild();
        player1Keys.UnBuild();player2Keys.UnBuild();player3Keys.UnBuild();player4Keys.UnBuild();player5Keys.UnBuild();

        InputManagerConfigData InputManagerConfig = new InputManagerConfigData(defaultKBKeys, defaultGPKeys, player1Keys, player2Keys, player3Keys, player4Keys, player5Keys, kbKeys, gpKeys, GP1RightThumbStickDeadZone,
            GP1LeftThumbStickDeadZone, GP1TriggersDeadZone, GP2RightThumbStickDeadZone, GP2LeftThumbStickDeadZone, GP2TriggersDeadZone, GP3RightThumbStickDeadZone, GP3LeftThumbStickDeadZone, GP3TriggersDeadZone, 
            GP4RightThumbStickDeadZone, GP4LeftThumbStickDeadZone, GP4TriggersDeadZone);
        bool res = Save.WriteJSONData(InputManagerConfig, fileName, true);
        defaultKBKeys.ClearList(); defaultGPKeys.ClearList(); kbKeys.ClearList(); gpKeys.ClearList();
        player1Keys.ClearList(); player2Keys.ClearList(); player3Keys.ClearList(); player4Keys.ClearList(); player5Keys.ClearList();
        return res;
    }

    /// <summary>
    /// Save all the default InputManager configuration (default actions and controllers keys link to the action) for all players in the file fikename in the game repertory,
    /// but don't save the current InputManager configuration.
    /// Can be load using the methode InputManager.LoadDefaultConfiguration(string fileName).
    /// </summary>
    public static bool SaveDefaultConfiguration(string fileName)
    {
        defaultKBKeys.UnBuild(); defaultGPKeys.UnBuild();
        bool res;
        if (!Save.ReadJSONData<InputManagerConfigData>(fileName, out InputManagerConfigData i))
        {
            InputManagerConfigData InputManagerConfig = new InputManagerConfigData(defaultKBKeys, defaultGPKeys, new InputData(), new InputData(), new InputData(), new InputData(), new InputData(), new InputData(), new InputData(), GP1RightThumbStickDeadZone,
                GP1LeftThumbStickDeadZone, GP1TriggersDeadZone, GP2RightThumbStickDeadZone, GP2LeftThumbStickDeadZone, GP2TriggersDeadZone, GP3RightThumbStickDeadZone, GP3LeftThumbStickDeadZone, GP3TriggersDeadZone,
                GP4RightThumbStickDeadZone, GP4LeftThumbStickDeadZone, GP4TriggersDeadZone);
            res = Save.WriteJSONData(InputManagerConfig, fileName, true);
            defaultKBKeys.ClearList(); defaultGPKeys.ClearList();
            return res;
        }
        InputManagerConfigData InputManagerConfig2 = new InputManagerConfigData(defaultKBKeys, defaultGPKeys, i.player1Keys, i.player2Keys, i.player3Keys, i.player4Keys, i.player5Keys, i.kbKeys, i.gpKeys, i.GP1RightThumbStickDeadZone,
            i.GP1LeftThumbStickDeadZone, i.GP1TriggersDeadZone, i.GP2RightThumbStickDeadZone, i.GP2LeftThumbStickDeadZone, i.GP2TriggersDeadZone, i.GP3RightThumbStickDeadZone, i.GP3LeftThumbStickDeadZone, i.GP3TriggersDeadZone,
            i.GP4RightThumbStickDeadZone, i.GP4LeftThumbStickDeadZone, i.GP4TriggersDeadZone);
        res = Save.WriteJSONData(InputManagerConfig2, fileName, true);
        defaultKBKeys.ClearList(); defaultGPKeys.ClearList();
        return res;
    }

    /// <summary>
    /// Save all the current InputManager configuration (current actions and controllers keys link to the action) for all players in the file fikename in the game repertory,
    /// but don't save the default InputManager configuration.
    /// Can be load using the methode InputManager.LoadCurrentConfiguration(string fileName).
    /// </summary>
    public static bool SaveCurrentConfiguration(string fileName)
    {
        kbKeys.UnBuild(); gpKeys.UnBuild();
        player1Keys.UnBuild(); player2Keys.UnBuild(); player3Keys.UnBuild(); player4Keys.UnBuild(); player5Keys.UnBuild();
        bool res;
        if (!Save.ReadJSONData<InputManagerConfigData>(fileName, out InputManagerConfigData i))
        {
            InputManagerConfigData InputManagerConfig = new InputManagerConfigData(new InputData(), new InputData(), player1Keys, player2Keys, player3Keys, player4Keys, player5Keys, kbKeys, gpKeys, GP1RightThumbStickDeadZone,
                GP1LeftThumbStickDeadZone, GP1TriggersDeadZone, GP2RightThumbStickDeadZone, GP2LeftThumbStickDeadZone, GP2TriggersDeadZone, GP3RightThumbStickDeadZone, GP3LeftThumbStickDeadZone, GP3TriggersDeadZone,
                GP4RightThumbStickDeadZone, GP4LeftThumbStickDeadZone, GP4TriggersDeadZone);
            res = Save.WriteJSONData(InputManagerConfig, fileName, true);

            kbKeys.ClearList(); gpKeys.ClearList();
            player1Keys.ClearList(); player2Keys.ClearList(); player3Keys.ClearList(); player4Keys.ClearList(); player5Keys.ClearList();
            return res;
        }
        InputManagerConfigData InputManagerConfig2 = new InputManagerConfigData(i.defaultKBKeys, i.defaultGPKeys, player1Keys, player2Keys, player3Keys, player4Keys, player5Keys, kbKeys, gpKeys, GP1RightThumbStickDeadZone,
            GP1LeftThumbStickDeadZone, GP1TriggersDeadZone, GP2RightThumbStickDeadZone, GP2LeftThumbStickDeadZone, GP2TriggersDeadZone, GP3RightThumbStickDeadZone, GP3LeftThumbStickDeadZone, GP3TriggersDeadZone,
            GP4RightThumbStickDeadZone, GP4LeftThumbStickDeadZone, GP4TriggersDeadZone);
        res = Save.WriteJSONData(InputManagerConfig2, fileName, true);
        kbKeys.ClearList(); gpKeys.ClearList();
        player1Keys.ClearList(); player2Keys.ClearList(); player3Keys.ClearList(); player4Keys.ClearList(); player5Keys.ClearList();
        return res;
    }

    /// <summary>
    /// Load from the file Save in the game repertory all the configuration of the InputManager system.
    /// </summary>
    public static bool LoadConfiguration(string fileName)
    {
        if (!Save.ReadJSONData<InputManagerConfigData>(fileName, out InputManagerConfigData i))
            return false;
        defaultKBKeys = i.defaultKBKeys;
        defaultGPKeys = i.defaultGPKeys;
        player1Keys = i.player1Keys;
        player2Keys = i.player2Keys;
        player3Keys = i.player3Keys;
        player4Keys = i.player4Keys;
        player5Keys = i.player5Keys;
        kbKeys = i.kbKeys;
        gpKeys = i.gpKeys;
        GP1RightThumbStickDeadZone = i.GP1RightThumbStickDeadZone;
        GP1LeftThumbStickDeadZone = i.GP1LeftThumbStickDeadZone;
        GP1TriggersDeadZone = i.GP1TriggersDeadZone;
        GP2RightThumbStickDeadZone = i. GP2RightThumbStickDeadZone;
        GP2LeftThumbStickDeadZone = i.GP2LeftThumbStickDeadZone;
        GP2TriggersDeadZone = i.GP2TriggersDeadZone;
        GP3RightThumbStickDeadZone = i.GP3RightThumbStickDeadZone;
        GP3LeftThumbStickDeadZone = i.GP3LeftThumbStickDeadZone;
        GP3TriggersDeadZone = i.GP3TriggersDeadZone;
        GP4RightThumbStickDeadZone = i.GP4RightThumbStickDeadZone;
        GP4LeftThumbStickDeadZone = i.GP4LeftThumbStickDeadZone;
        GP4TriggersDeadZone = i.GP4TriggersDeadZone;

        defaultKBKeys.Build(); defaultGPKeys.Build();player1Keys.Build();player2Keys.Build();player3Keys.Build();player4Keys.Build();player5Keys.Build();kbKeys.Build();gpKeys.Build();
        return true;
    }

    /// <summary>
    /// Load from the file Save in the game repertory all the configuration of the InputManager system.
    /// </summary>
    public static bool LoadControllerConfiguration(string fileName)
    {
        if (!Save.ReadJSONData<InputManagerConfigData>(fileName, out InputManagerConfigData i))
            return false;
        defaultKBKeys = i.defaultKBKeys;
        defaultGPKeys = i.defaultGPKeys;
        player1Keys = i.player1Keys;
        player2Keys = i.player2Keys;
        player3Keys = i.player3Keys;
        player4Keys = i.player4Keys;
        player5Keys = i.player5Keys;
        kbKeys = i.kbKeys;
        gpKeys = i.gpKeys;

        defaultKBKeys.Build(); defaultGPKeys.Build(); player1Keys.Build(); player2Keys.Build(); player3Keys.Build(); player4Keys.Build(); player5Keys.Build(); kbKeys.Build(); gpKeys.Build();
        return true;
    }

    /// <summary>
    /// Load from the file Save in the game repertory the default configuration of the InputManager system.
    /// </summary>
    public static bool LoadDefaultControllerConfiguration(string fileName)
    {
        if (!Save.ReadJSONData<InputManagerConfigData>(fileName, out InputManagerConfigData i))
            return false;
        defaultKBKeys = i.defaultKBKeys;
        defaultGPKeys = i.defaultGPKeys;
        defaultKBKeys.Build(); defaultGPKeys.Build();
        return true;
    }

    /// <summary>
    /// Load from the file Save in the game repertory the current configuration of the InputManager system.
    /// </summary>
    public static bool LoadNonDefaultControllerConfiguration(string fileName)
    {
        if (!Save.ReadJSONData<InputManagerConfigData>(fileName, out InputManagerConfigData i))
            return false;
        player1Keys = i.player1Keys;
        player2Keys = i.player2Keys;
        player3Keys = i.player3Keys;
        player4Keys = i.player4Keys;
        player5Keys = i.player5Keys;
        kbKeys = i.kbKeys;
        gpKeys = i.gpKeys;
        player1Keys.Build(); player2Keys.Build(); player3Keys.Build(); player4Keys.Build(); player5Keys.Build(); kbKeys.Build(); gpKeys.Build();
        return true;
    }

    /// <summary>
    /// Load from the file Save in the game repertory the current configuration of the InputManager system.
    /// </summary>
    public static bool LoadDeadZonesConfiguration(string fileName)
    {
        if (!Save.ReadJSONData<InputManagerConfigData>(fileName, out InputManagerConfigData i))
            return false;
        GP1RightThumbStickDeadZone = i.GP1RightThumbStickDeadZone;
        GP1LeftThumbStickDeadZone = i.GP1LeftThumbStickDeadZone;
        GP1TriggersDeadZone = i.GP1TriggersDeadZone;
        GP2RightThumbStickDeadZone = i.GP2RightThumbStickDeadZone;
        GP2LeftThumbStickDeadZone = i.GP2LeftThumbStickDeadZone;
        GP2TriggersDeadZone = i.GP2TriggersDeadZone;
        GP3RightThumbStickDeadZone = i.GP3RightThumbStickDeadZone;
        GP3LeftThumbStickDeadZone = i.GP3LeftThumbStickDeadZone;
        GP3TriggersDeadZone = i.GP3TriggersDeadZone;
        GP4RightThumbStickDeadZone = i.GP4RightThumbStickDeadZone;
        GP4LeftThumbStickDeadZone = i.GP4LeftThumbStickDeadZone;
        GP4TriggersDeadZone = i.GP4TriggersDeadZone;
        return true;
    }

    #endregion

    #region Save controller Async

    public static async Task<bool> SaveConfigurationAsync(string fileName, Action<bool> callback)
    {
        defaultKBKeys.UnBuild(); defaultGPKeys.UnBuild(); kbKeys.UnBuild(); gpKeys.UnBuild();
        player1Keys.UnBuild(); player2Keys.UnBuild(); player3Keys.UnBuild(); player4Keys.UnBuild(); player5Keys.UnBuild();

        InputManagerConfigData InputManagerConfig = new InputManagerConfigData(defaultKBKeys, defaultGPKeys, player1Keys, player2Keys, player3Keys, player4Keys, player5Keys, kbKeys, gpKeys, GP1RightThumbStickDeadZone,
            GP1LeftThumbStickDeadZone, GP1TriggersDeadZone, GP2RightThumbStickDeadZone, GP2LeftThumbStickDeadZone, GP2TriggersDeadZone, GP3RightThumbStickDeadZone, GP3LeftThumbStickDeadZone, GP3TriggersDeadZone,
            GP4RightThumbStickDeadZone, GP4LeftThumbStickDeadZone, GP4TriggersDeadZone);
        bool res = await Save.WriteJSONDataAsync(InputManagerConfig, fileName, callback, true);
        defaultKBKeys.ClearList(); defaultGPKeys.ClearList(); kbKeys.ClearList(); gpKeys.ClearList();
        player1Keys.ClearList(); player2Keys.ClearList(); player3Keys.ClearList(); player4Keys.ClearList(); player5Keys.ClearList();
        return res;
    }

    /// <summary>
    /// Save all the default InputManager configuration (default actions and controllers keys link to the action) for all players in the file fikename in the game repertory,
    /// but don't save the current InputManager configuration.
    /// Can be load using the methode InputManager.LoadDefaultConfiguration(string fileName).
    /// </summary>
    public static async Task<bool> SaveDefaultConfiguration(string fileName, Action<bool> callback)
    {
        defaultKBKeys.UnBuild(); defaultGPKeys.UnBuild();
        bool res;
        if (!Save.ReadJSONData<InputManagerConfigData>(fileName, out InputManagerConfigData i))
        {
            InputManagerConfigData InputManagerConfig = new InputManagerConfigData(defaultKBKeys, defaultGPKeys, new InputData(), new InputData(), new InputData(), new InputData(), new InputData(), new InputData(), new InputData(), GP1RightThumbStickDeadZone,
                GP1LeftThumbStickDeadZone, GP1TriggersDeadZone, GP2RightThumbStickDeadZone, GP2LeftThumbStickDeadZone, GP2TriggersDeadZone, GP3RightThumbStickDeadZone, GP3LeftThumbStickDeadZone, GP3TriggersDeadZone,
                GP4RightThumbStickDeadZone, GP4LeftThumbStickDeadZone, GP4TriggersDeadZone);
            res = await Save.WriteJSONDataAsync(InputManagerConfig, fileName, callback, true);
            defaultKBKeys.ClearList(); defaultGPKeys.ClearList();
            return res;
        }
        InputManagerConfigData InputManagerConfig2 = new InputManagerConfigData(defaultKBKeys, defaultGPKeys, i.player1Keys, i.player2Keys, i.player3Keys, i.player4Keys, i.player5Keys, i.kbKeys, i.gpKeys, i.GP1RightThumbStickDeadZone,
            i.GP1LeftThumbStickDeadZone, i.GP1TriggersDeadZone, i.GP2RightThumbStickDeadZone, i.GP2LeftThumbStickDeadZone, i.GP2TriggersDeadZone, i.GP3RightThumbStickDeadZone, i.GP3LeftThumbStickDeadZone, i.GP3TriggersDeadZone,
            i.GP4RightThumbStickDeadZone, i.GP4LeftThumbStickDeadZone, i.GP4TriggersDeadZone);
        res = await Save.WriteJSONDataAsync(InputManagerConfig2, fileName, callback, true);
        defaultKBKeys.ClearList(); defaultGPKeys.ClearList();
        return res;
    }

    /// <summary>
    /// Save all the current InputManager configuration (current actions and controllers keys link to the action) for all players in the file fikename in the game repertory,
    /// but don't save the default InputManager configuration.
    /// Can be load using the methode InputManager.LoadCurrentConfiguration(string fileName).
    /// </summary>
    public static async Task<bool> SaveCurrentConfigurationAsync(string fileName, Action<bool> callback)
    {
        kbKeys.UnBuild(); gpKeys.UnBuild();
        player1Keys.UnBuild(); player2Keys.UnBuild(); player3Keys.UnBuild(); player4Keys.UnBuild(); player5Keys.UnBuild();
        bool res;
        if (!Save.ReadJSONData<InputManagerConfigData>(fileName, out InputManagerConfigData i))
        {
            InputManagerConfigData InputManagerConfig = new InputManagerConfigData(new InputData(), new InputData(), player1Keys, player2Keys, player3Keys, player4Keys, player5Keys, kbKeys, gpKeys, GP1RightThumbStickDeadZone,
                GP1LeftThumbStickDeadZone, GP1TriggersDeadZone, GP2RightThumbStickDeadZone, GP2LeftThumbStickDeadZone, GP2TriggersDeadZone, GP3RightThumbStickDeadZone, GP3LeftThumbStickDeadZone, GP3TriggersDeadZone,
                GP4RightThumbStickDeadZone, GP4LeftThumbStickDeadZone, GP4TriggersDeadZone);
            res = await Save.WriteJSONDataAsync(InputManagerConfig, fileName, callback, true);

            kbKeys.ClearList(); gpKeys.ClearList();
            player1Keys.ClearList(); player2Keys.ClearList(); player3Keys.ClearList(); player4Keys.ClearList(); player5Keys.ClearList();
            return res;
        }
        InputManagerConfigData InputManagerConfig2 = new InputManagerConfigData(i.defaultKBKeys, i.defaultGPKeys, player1Keys, player2Keys, player3Keys, player4Keys, player5Keys, kbKeys, gpKeys, GP1RightThumbStickDeadZone,
            GP1LeftThumbStickDeadZone, GP1TriggersDeadZone, GP2RightThumbStickDeadZone, GP2LeftThumbStickDeadZone, GP2TriggersDeadZone, GP3RightThumbStickDeadZone, GP3LeftThumbStickDeadZone, GP3TriggersDeadZone,
            GP4RightThumbStickDeadZone, GP4LeftThumbStickDeadZone, GP4TriggersDeadZone);
        res = await Save.WriteJSONDataAsync(InputManagerConfig2, fileName, callback, true);
        kbKeys.ClearList(); gpKeys.ClearList();
        player1Keys.ClearList(); player2Keys.ClearList(); player3Keys.ClearList(); player4Keys.ClearList(); player5Keys.ClearList();
        return res;
    }

    /// <summary>
    /// Load from the file Save in the game repertory all the configuration of the InputManager system.
    /// </summary>
    public static async Task<bool> LoadConfigurationAsync(string fileName, Action<bool> callback)
    {
        void CallbackReadInputManagerConfigData(bool b, InputManagerConfigData i)
        {
            defaultKBKeys = i.defaultKBKeys;
            defaultGPKeys = i.defaultGPKeys;
            player1Keys = i.player1Keys;
            player2Keys = i.player2Keys;
            player3Keys = i.player3Keys;
            player4Keys = i.player4Keys;
            player5Keys = i.player5Keys;
            kbKeys = i.kbKeys;
            gpKeys = i.gpKeys;
            GP1RightThumbStickDeadZone = i.GP1RightThumbStickDeadZone;
            GP1LeftThumbStickDeadZone = i.GP1LeftThumbStickDeadZone;
            GP1TriggersDeadZone = i.GP1TriggersDeadZone;
            GP2RightThumbStickDeadZone = i.GP2RightThumbStickDeadZone;
            GP2LeftThumbStickDeadZone = i.GP2LeftThumbStickDeadZone;
            GP2TriggersDeadZone = i.GP2TriggersDeadZone;
            GP3RightThumbStickDeadZone = i.GP3RightThumbStickDeadZone;
            GP3LeftThumbStickDeadZone = i.GP3LeftThumbStickDeadZone;
            GP3TriggersDeadZone = i.GP3TriggersDeadZone;
            GP4RightThumbStickDeadZone = i.GP4RightThumbStickDeadZone;
            GP4LeftThumbStickDeadZone = i.GP4LeftThumbStickDeadZone;
            GP4TriggersDeadZone = i.GP4TriggersDeadZone;

            defaultKBKeys.Build(); defaultGPKeys.Build(); player1Keys.Build(); player2Keys.Build(); player3Keys.Build(); player4Keys.Build(); player5Keys.Build(); kbKeys.Build(); gpKeys.Build();
        }

        bool res = await Save.ReadJSONDataAsync<InputManagerConfigData>(fileName, CallbackReadInputManagerConfigData);
        callback.Invoke(res);
        return res;
    }

    /// <summary>
    /// Load from the file Save in the game repertory all the configuration of the InputManager system.
    /// </summary>
    public static async Task<bool> LoadControllerConfigurationAsync(string fileName, Action<bool> callback)
    {
        void CallbackReadInputManagerConfigData(bool b, InputManagerConfigData i)
        {
            defaultKBKeys = i.defaultKBKeys;
            defaultGPKeys = i.defaultGPKeys;
            player1Keys = i.player1Keys;
            player2Keys = i.player2Keys;
            player3Keys = i.player3Keys;
            player4Keys = i.player4Keys;
            player5Keys = i.player5Keys;
            kbKeys = i.kbKeys;
            gpKeys = i.gpKeys;

            defaultKBKeys.Build(); defaultGPKeys.Build(); player1Keys.Build(); player2Keys.Build(); player3Keys.Build(); player4Keys.Build(); player5Keys.Build(); kbKeys.Build(); gpKeys.Build();
        }

        bool res = await Save.ReadJSONDataAsync<InputManagerConfigData>(fileName, CallbackReadInputManagerConfigData);
        callback.Invoke(res);
        return res;
    }

    /// <summary>
    /// Load from the file Save in the game repertory the default configuration of the InputManager system.
    /// </summary>
    public static async Task<bool> LoadDefaultControllerConfigurationAsync(string fileName, Action<bool> callback)
    {
        void CallbackReadInputManagerConfigData(bool b, InputManagerConfigData i)
        {
            defaultKBKeys = i.defaultKBKeys;
            defaultGPKeys = i.defaultGPKeys;
            defaultKBKeys.Build(); defaultGPKeys.Build();
        }

        bool res = await Save.ReadJSONDataAsync<InputManagerConfigData>(fileName, CallbackReadInputManagerConfigData);
        callback.Invoke(res);
        return res;
    }

    /// <summary>
    /// Load from the file Save in the game repertory the current configuration of the InputManager system.
    /// </summary>
    public static async Task<bool> LoadNonDefaultControllerConfigurationAsync(string fileName, Action<bool> callBack)
    {
        void CallbackReadInputManagerConfigData(bool b, InputManagerConfigData i)
        {
            player1Keys = i.player1Keys;
            player2Keys = i.player2Keys;
            player3Keys = i.player3Keys;
            player4Keys = i.player4Keys;
            player5Keys = i.player5Keys;
            kbKeys = i.kbKeys;
            gpKeys = i.gpKeys;
            player1Keys.Build(); player2Keys.Build(); player3Keys.Build(); player4Keys.Build(); player5Keys.Build(); kbKeys.Build(); gpKeys.Build();
        }

        bool res = await Save.ReadJSONDataAsync<InputManagerConfigData>(fileName, CallbackReadInputManagerConfigData);
        callBack.Invoke(res);
        return res;
    }

    /// <summary>
    /// Load from the file Save in the game repertory the current configuration of the InputManager system.
    /// </summary>
    public static async Task<bool> LoadDeadZonesConfigurationAsync(string fileName, Action<bool> callback)
    {
        void CallbackReadInputManagerConfigData(bool b, InputManagerConfigData i)
        {
            GP1RightThumbStickDeadZone = i.GP1RightThumbStickDeadZone;
            GP1LeftThumbStickDeadZone = i.GP1LeftThumbStickDeadZone;
            GP1TriggersDeadZone = i.GP1TriggersDeadZone;
            GP2RightThumbStickDeadZone = i.GP2RightThumbStickDeadZone;
            GP2LeftThumbStickDeadZone = i.GP2LeftThumbStickDeadZone;
            GP2TriggersDeadZone = i.GP2TriggersDeadZone;
            GP3RightThumbStickDeadZone = i.GP3RightThumbStickDeadZone;
            GP3LeftThumbStickDeadZone = i.GP3LeftThumbStickDeadZone;
            GP3TriggersDeadZone = i.GP3TriggersDeadZone;
            GP4RightThumbStickDeadZone = i.GP4RightThumbStickDeadZone;
            GP4LeftThumbStickDeadZone = i.GP4LeftThumbStickDeadZone;
            GP4TriggersDeadZone = i.GP4TriggersDeadZone;
        }

        bool res = await Save.ReadJSONDataAsync<InputManagerConfigData>(fileName, CallbackReadInputManagerConfigData);
        callback.Invoke(res);
        return res;
    }

    #endregion

    #region Useful region

    public static bool IsConfigurationEmpty(PlayerIndex playerIndex)
    {
        switch (playerIndex)
        {
            case PlayerIndex.One:
                return player1Keys.IsEmpty();
            case PlayerIndex.Two:
                return player2Keys.IsEmpty();
            case PlayerIndex.Three:
                return player3Keys.IsEmpty();
            case PlayerIndex.Four:
                return player4Keys.IsEmpty();
            case PlayerIndex.Five:
                return player5Keys.IsEmpty();
            case PlayerIndex.All:
                return player1Keys.IsEmpty() && player2Keys.IsEmpty() && player3Keys.IsEmpty() && player4Keys.IsEmpty() && player5Keys.IsEmpty();
            default:
                return true;
        }
    }

    public static bool IsConfigurationEmpty(BaseController baseController, bool defaultConfig = false)
    {
        switch (baseController)
        {
            case BaseController.Keyboard:
                return defaultConfig ? defaultKBKeys.IsEmpty() : kbKeys.IsEmpty();
            case BaseController.Gamepad:
                return defaultConfig ? defaultGPKeys.IsEmpty() : gpKeys.IsEmpty();
            case BaseController.KeyboardAndGamepad:
                return defaultConfig? (defaultKBKeys.IsEmpty() && defaultGPKeys.IsEmpty()) : (kbKeys.IsEmpty() && gpKeys.IsEmpty());
            default:
                return true;
        }
    }

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

    /// <summary>
    /// Convert a key into a string.
    /// </summary>
    /// <param name="key"> the key to convert to a string</param>
    public static string KeyToString(InputKey key) => key.ToString();

    public static Vector2 mousePosition => Input.mousePosition;
    public static float mouseScrollDelta => Input.mouseScrollDelta.y;
    public static bool isAMouseConnected => Input.mousePresent;

    private static string KeysToString(InputData.ListInt keys)
    {
        StringBuilder sb = new StringBuilder();
        foreach (int key in keys)
        {
            sb.Append(KeyToString((InputKey)key) + ",");
        }
        sb.Remove(sb.Length - 1, 1);
        return sb.ToString();
    }

    /// <summary>
    /// Convert an action into the string who define the control of the action, according to the controller.
    /// </summary>
    public static string KeyToString(string action, PlayerIndex player)
    {
        InputData.ListInt keys;
        switch (player)
        {
            case PlayerIndex.One:
                keys = player1Keys.GetKeys(action);
                break;
            case PlayerIndex.Two:
                keys = player2Keys.GetKeys(action);
                break;
            case PlayerIndex.Three:
                keys = player3Keys.GetKeys(action);
                break;
            case PlayerIndex.Four:
                keys = player4Keys.GetKeys(action);
                break;
            case PlayerIndex.Five:
                keys = player5Keys.GetKeys(action);
                break;
            default:
                Debug.LogWarning("Cannot convert to string multiples Keys");
                return string.Empty;
        }
        return KeysToString(keys);
    }

    public static string KeyToString(string action, ControllerType controllerType)
    {
        if (controllerType == ControllerType.Keyboard)
            return(KeysToString(kbKeys.GetKeys(action)));
        if(controllerType == ControllerType.Gamepad1)
            return KeysToString(gpKeys.GetKeys(action));
        Debug.LogWarning("Cannot convert to string multiples Keys");
        return string.Empty;
    }

    public static string KeyToString(string action, BaseController controller)
    {
        if (controller == BaseController.Keyboard)
            return (KeysToString(kbKeys.GetKeys(action)));
        if (controller == BaseController.Gamepad)
            return KeysToString(gpKeys.GetKeys(action));
        Debug.LogWarning("Cannot convert to string multiples Keys");
        return string.Empty;
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
                for (int i = -15; i <= -1; i++)
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
                for (int i = -30; i <= -16; i++)
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
                for (int i = -45; i <= -31; i++)
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
                for (int i = -60; i <= -46; i++)
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
                for (int i = -75; i <= 0; i++)
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
                for (int i = -75; i <= 0; i++)
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

    public static bool Listen(BaseController controller, out InputKey key)
    {
        if(controller == BaseController.Gamepad)
        {
            return Listen(ControllerType.GamepadAll, out key);
        }
        if (controller == BaseController.Keyboard)
        {
            return Listen(ControllerType.Keyboard, out key);
        }
        return Listen(ControllerType.All, out key);
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
                for (int i = -15; i <= -1; i++)
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
                for (int i = -30; i <= -16; i++)
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
                for (int i = -45; i <= -31; i++)
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
                for (int i = -60; i <= -46; i++)
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
                for (int i = -75; i <= 0; i++)
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
                for (int i = -75; i <= 0; i++)
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

    public static bool ListenAll(BaseController controller, out InputKey[] resultKeys)
    {
        if (controller == BaseController.Gamepad)
        {
            return ListenAll(ControllerType.GamepadAll, out resultKeys);
        }
        if (controller == BaseController.Keyboard)
        {
            return ListenAll(ControllerType.Keyboard, out resultKeys);
        }
        return ListenAll(ControllerType.All, out resultKeys);
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
        newGP1State = GamePad.GetState(XInputDotNetPure.PlayerIndex.One);
        newGP2State = GamePad.GetState(XInputDotNetPure.PlayerIndex.Two);
        newGP3State = GamePad.GetState(XInputDotNetPure.PlayerIndex.Three);
        newGP4State = GamePad.GetState(XInputDotNetPure.PlayerIndex.Four);
        SetNewGamepadSticksAndTriggersPositions();

        //vibration
        List<VibrationSetting> stopSetting = new List<VibrationSetting>(); 
        for (int i = vibrationSettings.Count - 1; i >= 0; i--)
        {
            VibrationSetting setting = vibrationSettings[i];
            setting.timer += Time.deltaTime;
            if(setting.timer > setting.duration)
            {
                stopSetting.Add(setting);
                vibrationSettings.RemoveAt(i);
                continue;
            }

            if(setting.timer > 0f)
                PrivateSetVibration(setting.rightIntensity, setting.leftIntensity, setting.gamepadIndex);
        }

        foreach (VibrationSetting vib in stopSetting)
        {
            StopVibration(vib.gamepadIndex);
        }
    }

    #endregion

    #region Custom Struct

    #region GeneralInput

    [Serializable]
    public struct GeneralInput
    {
        public KeyboardKey[] keysKeyboard;
        public GamepadKey[] keyGamepad1;
        public GamepadKey[] keyGamepad2;
        public GamepadKey[] keyGamepad3;
        public GamepadKey[] keyGamepad4;
        public ControllerType controllerType;

        public GeneralInput(KeyboardKey[] keysKeyboard, GamepadKey[] keyGamepad1, GamepadKey[] keyGamepad2, GamepadKey[] keyGamepad3, GamepadKey[] keyGamepad4, ControllerType controllerType)
        {
            this.keysKeyboard = keysKeyboard;
            this.keyGamepad1 = keyGamepad1;
            this.keyGamepad2 = keyGamepad2;
            this.keyGamepad3 = keyGamepad3;
            this.keyGamepad4 = keyGamepad4;
            this.controllerType = controllerType;
        }

        private InputKey[] ConvertGamepadKeysToInputKeys(GamepadKey[] keys)
        {
            InputKey[] res = new InputKey[keys.Length];
            for (int i = 0; i < res.Length; i++)
            {
                res[i] = (InputKey)keys[i];
            }
            return res;
        }

        private InputKey[] ConvertKeyboardKeysToInputKeys(KeyboardKey[] keys)
        {
            InputKey[] res = new InputKey[keys.Length];
            for (int i = 0; i < res.Length; i++)
            {
                res[i] = (InputKey)keys[i];
            }
            return res;
        }

        private bool isKeySomething(Func<InputKey, bool> func)
        {
            switch (controllerType)
            {
                case ControllerType.Keyboard:
                    return GetKeySomething(func, ConvertKeyboardKeysToInputKeys(keysKeyboard));
                case ControllerType.Gamepad1:
                    return GetKeySomething(func, ConvertGamepadKeysToInputKeys(keyGamepad1));
                case ControllerType.Gamepad2:
                    return GetKeySomething(func, ConvertGamepadKeysToInputKeys(keyGamepad2));
                case ControllerType.Gamepad3:
                    return GetKeySomething(func, ConvertGamepadKeysToInputKeys(keyGamepad3));
                case ControllerType.Gamepad4:
                    return GetKeySomething(func, ConvertGamepadKeysToInputKeys(keyGamepad4));
                case ControllerType.GamepadAll:
                    return GetKeySomething(func, ConvertGamepadKeysToInputKeys(keyGamepad1)) || GetKeySomething(func, ConvertGamepadKeysToInputKeys(keyGamepad2))
                        || GetKeySomething(func, ConvertGamepadKeysToInputKeys(keyGamepad3)) || GetKeySomething(func, ConvertGamepadKeysToInputKeys(keyGamepad4));
                case ControllerType.All:
                    return GetKeySomething(func, ConvertKeyboardKeysToInputKeys(keysKeyboard)) || GetKeySomething(func, ConvertGamepadKeysToInputKeys(keyGamepad1)) || GetKeySomething(func, ConvertGamepadKeysToInputKeys(keyGamepad2))
                        || GetKeySomething(func, ConvertGamepadKeysToInputKeys(keyGamepad3)) || GetKeySomething(func, ConvertGamepadKeysToInputKeys(keyGamepad4));
                default:
                    return false;
            }

            bool GetKeySomething(Func<InputKey, bool> func, InputKey[] keys)
            {
                foreach (InputKey k in keys)
                {
                    if(func(k))
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

    #region VibrationSetting

    private class VibrationSetting : ICloneable<VibrationSetting>
    {
        public ControllerType gamepadIndex;
        public float duration, rightIntensity, leftIntensity, timer;

        public VibrationSetting(ControllerType gamepadIndex, float duration, float rightIntensity, float leftIntensity)
        {
            this.gamepadIndex = gamepadIndex;
            this.duration = duration;
            this.rightIntensity = rightIntensity;
            this.leftIntensity = leftIntensity;
            timer = 0f;
        }

        public VibrationSetting Clone() => new VibrationSetting(gamepadIndex, duration, rightIntensity, leftIntensity);
    }

    #endregion

    #endregion
}

#endregion
