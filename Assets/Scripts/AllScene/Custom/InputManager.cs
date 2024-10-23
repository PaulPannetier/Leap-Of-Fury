/*
 * Copyright (c) 2023 Léonard Pannetier <email>
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
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XInput;
using UnityEngine.InputSystem.DualShock;
using System.Collections;
using System.Threading.Tasks;
using System.Text;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using UnityEngine.InputSystem.Utilities;
using System.Runtime.CompilerServices;
using UnityEngine.InputSystem.Controls;

#endregion

#region Enums

public enum ControllerModel
{
    Keyboard,
    PSVita,
    PS3,
    PS4,
    PS5,
    SteamDeck,
    Switch,
    XBox360,
    XBoxOne,
    XBoxSeries,
    AmazonLuna,
    GoogleStadia,
    Ouya,
    None
}

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
    GamepadAny = 5,
    Any = 6
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
    GPRT = -57,
    GPLT = -58,
    GPDPadUp = -59,
    GPDPadRight = -60,
    GPDPadDown = -61,
    GPDPadLeft = -62,
    GPTBSRUp = -63,
    GPTBSRDown = -64,
    GPTBSRRight = -65,
    GPTBSRLeft = -66,
    GPTBSLUp = -67,
    GPTBSLDown = -68,
    GPTBSLRight = -69,
    GPTBSLLeft = -70,

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

    GPRT = -57,
    GPLT = -58,
    GPDPadUp = -59,
    GPDPadRight = -60,
    GPDPadDown = -61,
    GPDPadLeft = -62,
    GPTBSRUp = -63,
    GPTBSRDown = -64,
    GPTBSRRight = -65,
    GPTBSRLeft = -66,
    GPTBSLUp = -67,
    GPTBSLDown = -68,
    GPTBSLRight = -69,
    GPTBSLLeft = -70,

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

    GP1A = 340,
    GP1B = 341,
    GP1X = 342,
    GP1Y = 343,
    GP1L1 = 344,
    GP1R1 = 345,
    GP1Back = 346,
    GP1Start = 347,
    GP1TBSL = 348,
    GP1TBSR = 349,

    GP2A = 350,
    GP2B = 351,
    GP2X = 352,
    GP2Y = 353,
    GP2L1 = 354,
    GP2R1 = 355,
    GP2Back = 356,
    GP2Start = 357,
    GP2TBSL = 358,
    GP2TBSR = 359,

    GP3A = 360,
    GP3B = 361,
    GP3X = 362,
    GP3Y = 363,
    GP3L1 = 364,
    GP3R1 = 365,
    GP3Back = 366,
    GP3Start = 367,
    GP3TBSL = 368,
    GP3TBSR = 369,

    GP4A = 370,
    GP4B = 371,
    GP4X = 372,
    GP4Y = 373,
    GP4L1 = 374,
    GP4R1 = 375,
    GP4Start = 376,
    GP4Back = 377,
    GP4TBSL = 378,
    GP4TBSR = 379
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

public enum GeneralKey
{
    GPRT = -57,
    GPLT = -58,
    GPDPadUp = -59,
    GPDPadRight = -60,
    GPDPadDown = -61,
    GPDPadLeft = -62,
    GPTBSRUp = -63,
    GPTBSRDown = -64,
    GPTBSRRight = -65,
    GPTBSRLeft = -66,
    GPTBSLUp = -67,
    GPTBSLDown = -68,
    GPTBSLRight = -69,
    GPTBSLLeft = -70,

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

    GPRT = -57,
    GPLT = -58,
    GPDPadUp = -59,
    GPDPadRight = -60,
    GPDPadDown = -61,
    GPDPadLeft = -62,
    GPTBSRUp = -63,
    GPTBSRDown = -64,
    GPTBSRRight = -65,
    GPTBSRLeft = -66,
    GPTBSLUp = -67,
    GPTBSLDown = -68,
    GPTBSLRight = -69,
    GPTBSLLeft = -70,

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

    GP1A = 340,
    GP1B = 341,
    GP1X = 342,
    GP1Y = 343,
    GP1L1 = 344,
    GP1R1 = 345,
    GP1Back = 346,
    GP1Start = 347,
    GP1TBSL = 348,
    GP1TBSR = 349,

    GP2A = 350,
    GP2B = 351,
    GP2X = 352,
    GP2Y = 353,
    GP2L1 = 354,
    GP2R1 = 355,
    GP2Back = 356,
    GP2Start = 357,
    GP2TBSL = 358,
    GP2TBSR = 359,

    GP3A = 360,
    GP3B = 361,
    GP3X = 362,
    GP3Y = 363,
    GP3L1 = 364,
    GP3R1 = 365,
    GP3Back = 366,
    GP3Start = 367,
    GP3TBSL = 368,
    GP3TBSR = 369,

    GP4A = 370,
    GP4B = 371,
    GP4X = 372,
    GP4Y = 373,
    GP4L1 = 374,
    GP4R1 = 375,
    GP4Start = 376,
    GP4Back = 377,
    GP4TBSL = 378,
    GP4TBSR = 379
}

#endregion

#region InputManager

public static class InputManager
{
    #region Keys config

    private static InputData player1Keys;
    private static InputData player2Keys;
    private static InputData player3Keys;
    private static InputData player4Keys;
    private static InputData player5Keys;

    private static InputData defaultKBKeys;
    private static InputData defaultGPKeys;
    private static InputData kbKeys;
    private static InputData gpKeys;

    #endregion

    #region Require

    private static GamePadState newGP1State, oldGP1State;
    private static GamePadState newGP2State, oldGP2State;
    private static GamePadState newGP3State, oldGP3State;
    private static GamePadState newGP4State, oldGP4State;

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

    private static List<VibrationSettings> vibrationSettings = new List<VibrationSettings>();

    #region GetInputKey Down/Up/Pressed delegate

    #region Negative Input

    private static readonly Func<bool>[] getInputKeyDownDelegate = new Func<bool>[71]
    {
        () => false,
        () => { return oldGP1State.triggers.right <= _analogicButtonDownValue && newGP1State.triggers.right > _analogicButtonDownValue; },
        () => { return oldGP1State.triggers.left <= _analogicButtonDownValue && newGP1State.triggers.left > _analogicButtonDownValue; },
        () => { return oldGP1State.dPad.up == ButtonState.Released && newGP1State.dPad.up == ButtonState.Pressed; },
        () => { return oldGP1State.dPad.right == ButtonState.Released && newGP1State.dPad.right == ButtonState.Pressed; },
        () => { return oldGP1State.dPad.down == ButtonState.Released && newGP1State.dPad.down == ButtonState.Pressed; },
        () => { return oldGP1State.dPad.left == ButtonState.Released && newGP1State.dPad.left == ButtonState.Pressed; },
        () => { return oldGP1State.thumbSticks.right.y <= _analogicButtonDownValue && newGP1State.thumbSticks.right.y > _analogicButtonDownValue; },
        () => { return oldGP1State.thumbSticks.right.y >= -_analogicButtonDownValue && newGP1State.thumbSticks.right.y < -_analogicButtonDownValue; },
        () => { return oldGP1State.thumbSticks.right.x <= _analogicButtonDownValue && newGP1State.thumbSticks.right.x > _analogicButtonDownValue; },
        () => { return oldGP1State.thumbSticks.right.x >= -_analogicButtonDownValue && newGP1State.thumbSticks.right.x < -_analogicButtonDownValue; },
        () => { return oldGP1State.thumbSticks.left.y <= _analogicButtonDownValue && newGP1State.thumbSticks.left.y > _analogicButtonDownValue; },
        () => { return oldGP1State.thumbSticks.left.y >= -_analogicButtonDownValue && newGP1State.thumbSticks.left.y < -_analogicButtonDownValue; },
        () => { return oldGP1State.thumbSticks.left.x <= _analogicButtonDownValue && newGP1State.thumbSticks.left.x > _analogicButtonDownValue; },
        () => { return oldGP1State.thumbSticks.left.x >= -_analogicButtonDownValue && newGP1State.thumbSticks.left.x < -_analogicButtonDownValue; },

        () => { return oldGP2State.triggers.right <= _analogicButtonDownValue && newGP2State.triggers.right > _analogicButtonDownValue; },
        () => { return oldGP2State.triggers.left <= _analogicButtonDownValue && newGP2State.triggers.left > _analogicButtonDownValue; },
        () => { return oldGP2State.dPad.up == ButtonState.Released && newGP2State.dPad.up == ButtonState.Pressed; },
        () => { return oldGP2State.dPad.right == ButtonState.Released && newGP2State.dPad.right == ButtonState.Pressed; },
        () => { return oldGP2State.dPad.down == ButtonState.Released && newGP2State.dPad.down == ButtonState.Pressed; },
        () => { return oldGP2State.dPad.left == ButtonState.Released && newGP2State.dPad.left == ButtonState.Pressed; },
        () => { return oldGP2State.thumbSticks.right.y <= _analogicButtonDownValue && newGP2State.thumbSticks.right.y > _analogicButtonDownValue; },
        () => { return oldGP2State.thumbSticks.right.y >= -_analogicButtonDownValue && newGP2State.thumbSticks.right.y < -_analogicButtonDownValue; },
        () => { return oldGP2State.thumbSticks.right.x <= _analogicButtonDownValue && newGP2State.thumbSticks.right.x > _analogicButtonDownValue; },
        () => { return oldGP2State.thumbSticks.right.x >= -_analogicButtonDownValue && newGP2State.thumbSticks.right.x < -_analogicButtonDownValue; },
        () => { return oldGP2State.thumbSticks.left.y <= _analogicButtonDownValue && newGP2State.thumbSticks.left.y > _analogicButtonDownValue; },
        () => { return oldGP2State.thumbSticks.left.y >= -_analogicButtonDownValue && newGP2State.thumbSticks.left.y < -_analogicButtonDownValue; },
        () => { return oldGP2State.thumbSticks.left.x <= _analogicButtonDownValue && newGP2State.thumbSticks.left.x > _analogicButtonDownValue; },
        () => { return oldGP2State.thumbSticks.left.x >= -_analogicButtonDownValue && newGP2State.thumbSticks.left.x < -_analogicButtonDownValue; },

        () => { return oldGP3State.triggers.right <= _analogicButtonDownValue && newGP3State.triggers.right > _analogicButtonDownValue; },
        () => { return oldGP3State.triggers.left <= _analogicButtonDownValue && newGP3State.triggers.left > _analogicButtonDownValue; },
        () => { return oldGP3State.dPad.up == ButtonState.Released && newGP3State.dPad.up == ButtonState.Pressed; },
        () => { return oldGP3State.dPad.right == ButtonState.Released && newGP3State.dPad.right == ButtonState.Pressed; },
        () => { return oldGP3State.dPad.down == ButtonState.Released && newGP3State.dPad.down == ButtonState.Pressed; },
        () => { return oldGP3State.dPad.left == ButtonState.Released && newGP3State.dPad.left == ButtonState.Pressed; },
        () => { return oldGP3State.thumbSticks.right.y <= _analogicButtonDownValue && newGP3State.thumbSticks.right.y > _analogicButtonDownValue; },
        () => { return oldGP3State.thumbSticks.right.y >= -_analogicButtonDownValue && newGP3State.thumbSticks.right.y < -_analogicButtonDownValue; },
        () => { return oldGP3State.thumbSticks.right.x <= _analogicButtonDownValue && newGP3State.thumbSticks.right.x > _analogicButtonDownValue; },
        () => { return oldGP3State.thumbSticks.right.x >= -_analogicButtonDownValue && newGP3State.thumbSticks.right.x < -_analogicButtonDownValue; },
        () => { return oldGP3State.thumbSticks.left.y <= _analogicButtonDownValue && newGP3State.thumbSticks.left.y > _analogicButtonDownValue; },
        () => { return oldGP3State.thumbSticks.left.y >= -_analogicButtonDownValue && newGP3State.thumbSticks.left.y < -_analogicButtonDownValue; },
        () => { return oldGP3State.thumbSticks.left.x <= _analogicButtonDownValue && newGP3State.thumbSticks.left.x > _analogicButtonDownValue; },
        () => { return oldGP3State.thumbSticks.left.x >= -_analogicButtonDownValue && newGP3State.thumbSticks.left.x < -_analogicButtonDownValue; },

        () => { return oldGP4State.triggers.right <= _analogicButtonDownValue && newGP4State.triggers.right > _analogicButtonDownValue; },
        () => { return oldGP4State.triggers.left <= _analogicButtonDownValue && newGP4State.triggers.left > _analogicButtonDownValue; },
        () => { return oldGP4State.dPad.up == ButtonState.Released && newGP4State.dPad.up == ButtonState.Pressed; },
        () => { return oldGP4State.dPad.right == ButtonState.Released && newGP4State.dPad.right == ButtonState.Pressed; },
        () => { return oldGP4State.dPad.down == ButtonState.Released && newGP4State.dPad.down == ButtonState.Pressed; },
        () => { return oldGP4State.dPad.left == ButtonState.Released && newGP4State.dPad.left == ButtonState.Pressed; },
        () => { return oldGP4State.thumbSticks.right.y <= _analogicButtonDownValue && newGP4State.thumbSticks.right.y > _analogicButtonDownValue; },
        () => { return oldGP4State.thumbSticks.right.y >= -_analogicButtonDownValue && newGP4State.thumbSticks.right.y < -_analogicButtonDownValue; },
        () => { return oldGP4State.thumbSticks.right.x <= _analogicButtonDownValue && newGP4State.thumbSticks.right.x > _analogicButtonDownValue; },
        () => { return oldGP4State.thumbSticks.right.x >= -_analogicButtonDownValue && newGP4State.thumbSticks.right.x < -_analogicButtonDownValue; },
        () => { return oldGP4State.thumbSticks.left.y <= _analogicButtonDownValue && newGP4State.thumbSticks.left.y > _analogicButtonDownValue; },
        () => { return oldGP4State.thumbSticks.left.y >= -_analogicButtonDownValue && newGP4State.thumbSticks.left.y < -_analogicButtonDownValue; },
        () => { return oldGP4State.thumbSticks.left.x <= _analogicButtonDownValue && newGP4State.thumbSticks.left.x > _analogicButtonDownValue; },
        () => { return oldGP4State.thumbSticks.left.x >= -_analogicButtonDownValue && newGP4State.thumbSticks.left.x < -_analogicButtonDownValue; },

        () => { return (oldGP1State.triggers.right <= _analogicButtonDownValue && newGP1State.triggers.right > _analogicButtonDownValue)
            || (oldGP2State.triggers.right <= _analogicButtonDownValue && newGP2State.triggers.right > _analogicButtonDownValue)
            || (oldGP3State.triggers.right <= _analogicButtonDownValue && newGP3State.triggers.right > _analogicButtonDownValue)
            || (oldGP4State.triggers.right <= _analogicButtonDownValue && newGP4State.triggers.right > _analogicButtonDownValue); },
        () => { return (oldGP1State.triggers.left <= _analogicButtonDownValue && newGP1State.triggers.left > _analogicButtonDownValue)
            || (oldGP2State.triggers.left <= _analogicButtonDownValue && newGP2State.triggers.left > _analogicButtonDownValue)
            || (oldGP3State.triggers.left <= _analogicButtonDownValue && newGP3State.triggers.left > _analogicButtonDownValue)
            || (oldGP4State.triggers.left <= _analogicButtonDownValue && newGP4State.triggers.left > _analogicButtonDownValue); },
        () => { return (oldGP1State.dPad.up == ButtonState.Released && newGP1State.dPad.up == ButtonState.Pressed)
            || (oldGP2State.dPad.up == ButtonState.Released && newGP2State.dPad.up == ButtonState.Pressed)
            || (oldGP3State.dPad.up == ButtonState.Released && newGP3State.dPad.up == ButtonState.Pressed)
            || (oldGP4State.dPad.up == ButtonState.Released && newGP4State.dPad.up == ButtonState.Pressed); },
        () => { return (oldGP1State.dPad.right == ButtonState.Released && newGP1State.dPad.right == ButtonState.Pressed)
            || (oldGP2State.dPad.right == ButtonState.Released && newGP2State.dPad.right == ButtonState.Pressed)
            || (oldGP3State.dPad.right == ButtonState.Released && newGP3State.dPad.right == ButtonState.Pressed)
            || (oldGP4State.dPad.right == ButtonState.Released && newGP4State.dPad.right == ButtonState.Pressed); },
        () => { return (oldGP1State.dPad.down == ButtonState.Released && newGP1State.dPad.down == ButtonState.Pressed)
            || (oldGP2State.dPad.down == ButtonState.Released && newGP2State.dPad.down == ButtonState.Pressed)
            || (oldGP3State.dPad.down == ButtonState.Released && newGP3State.dPad.down == ButtonState.Pressed)
            || (oldGP4State.dPad.down == ButtonState.Released && newGP4State.dPad.down == ButtonState.Pressed); },
        () => { return (oldGP1State.dPad.left == ButtonState.Released && newGP1State.dPad.left == ButtonState.Pressed)
            || (oldGP2State.dPad.left == ButtonState.Released && newGP2State.dPad.left == ButtonState.Pressed)
            || (oldGP3State.dPad.left == ButtonState.Released && newGP3State.dPad.left == ButtonState.Pressed)
            || (oldGP4State.dPad.left == ButtonState.Released && newGP4State.dPad.left == ButtonState.Pressed); },
        () => { return (oldGP1State.thumbSticks.right.y <= _analogicButtonDownValue && newGP1State.thumbSticks.right.y > _analogicButtonDownValue)
            || (oldGP2State.thumbSticks.right.y <= _analogicButtonDownValue && newGP2State.thumbSticks.right.y > _analogicButtonDownValue)
            || (oldGP3State.thumbSticks.right.y <= _analogicButtonDownValue && newGP3State.thumbSticks.right.y > _analogicButtonDownValue)
            || (oldGP4State.thumbSticks.right.y <= _analogicButtonDownValue && newGP4State.thumbSticks.right.y > _analogicButtonDownValue); },
        () => { return (oldGP1State.thumbSticks.right.y >= -_analogicButtonDownValue && newGP1State.thumbSticks.right.y < -_analogicButtonDownValue)
            || (oldGP2State.thumbSticks.right.y >= _analogicButtonDownValue && newGP2State.thumbSticks.right.y < _analogicButtonDownValue)
            || (oldGP3State.thumbSticks.right.y >= -_analogicButtonDownValue && newGP3State.thumbSticks.right.y < -_analogicButtonDownValue)
            || (oldGP4State.thumbSticks.right.y >= -_analogicButtonDownValue && newGP4State.thumbSticks.right.y < -_analogicButtonDownValue); },
        () => { return (oldGP1State.thumbSticks.right.x <= _analogicButtonDownValue && newGP1State.thumbSticks.right.x > _analogicButtonDownValue)
            || (oldGP2State.thumbSticks.right.x <= _analogicButtonDownValue && newGP2State.thumbSticks.right.x > _analogicButtonDownValue)
            || (oldGP3State.thumbSticks.right.x <= _analogicButtonDownValue && newGP3State.thumbSticks.right.x > _analogicButtonDownValue)
            || (oldGP4State.thumbSticks.right.x <= _analogicButtonDownValue && newGP4State.thumbSticks.right.x > _analogicButtonDownValue); },
        () => { return (oldGP1State.thumbSticks.right.x >= -_analogicButtonDownValue && newGP1State.thumbSticks.right.x < -_analogicButtonDownValue)
            || (oldGP2State.thumbSticks.right.x >= -_analogicButtonDownValue && newGP2State.thumbSticks.right.x < -_analogicButtonDownValue)
            || (oldGP3State.thumbSticks.right.x >= -_analogicButtonDownValue && newGP3State.thumbSticks.right.x < -_analogicButtonDownValue)
            || (oldGP4State.thumbSticks.right.x >= -_analogicButtonDownValue && newGP4State.thumbSticks.right.x < -_analogicButtonDownValue); },
        () => { return (oldGP1State.thumbSticks.left.y <= _analogicButtonDownValue && newGP1State.thumbSticks.left.y > _analogicButtonDownValue)
            || (oldGP2State.thumbSticks.left.y <= _analogicButtonDownValue && newGP2State.thumbSticks.left.y > _analogicButtonDownValue)
            || (oldGP3State.thumbSticks.left.y <= _analogicButtonDownValue && newGP3State.thumbSticks.left.y > _analogicButtonDownValue)
            || (oldGP4State.thumbSticks.left.y <= _analogicButtonDownValue && newGP4State.thumbSticks.left.y > _analogicButtonDownValue); },
        () => { return (oldGP1State.thumbSticks.left.y >= -_analogicButtonDownValue && newGP1State.thumbSticks.left.y < -_analogicButtonDownValue)
            || (oldGP2State.thumbSticks.left.y >= -_analogicButtonDownValue && newGP2State.thumbSticks.left.y < -_analogicButtonDownValue)
            || (oldGP3State.thumbSticks.left.y >= -_analogicButtonDownValue && newGP3State.thumbSticks.left.y < -_analogicButtonDownValue)
            || (oldGP4State.thumbSticks.left.y >= -_analogicButtonDownValue && newGP4State.thumbSticks.left.y < -_analogicButtonDownValue); },
        () => { return (oldGP1State.thumbSticks.left.x <= _analogicButtonDownValue && newGP1State.thumbSticks.left.x > _analogicButtonDownValue)
            || (oldGP2State.thumbSticks.left.x <= _analogicButtonDownValue && newGP2State.thumbSticks.left.x > _analogicButtonDownValue)
            || (oldGP3State.thumbSticks.left.x <= _analogicButtonDownValue && newGP3State.thumbSticks.left.x > _analogicButtonDownValue)
            || (oldGP4State.thumbSticks.left.x <= _analogicButtonDownValue && newGP4State.thumbSticks.left.x > _analogicButtonDownValue); },
        () => { return (oldGP1State.thumbSticks.left.x >= -_analogicButtonDownValue && newGP1State.thumbSticks.left.x < -_analogicButtonDownValue)
            || (oldGP2State.thumbSticks.left.x >= -_analogicButtonDownValue && newGP2State.thumbSticks.left.x < -_analogicButtonDownValue)
            || (oldGP3State.thumbSticks.left.x >= -_analogicButtonDownValue && newGP3State.thumbSticks.left.x < -_analogicButtonDownValue)
            || (oldGP4State.thumbSticks.left.x >= -_analogicButtonDownValue && newGP4State.thumbSticks.left.x < -_analogicButtonDownValue); }
    };

    private static readonly Func<bool>[] getInputKeyUpDelegate = new Func<bool>[71]
    {
        () => { return false; },
        () => { return oldGP1State.triggers.right > _analogicButtonDownValue && newGP1State.triggers.right <= _analogicButtonDownValue; },
        () => { return oldGP1State.triggers.left > _analogicButtonDownValue && newGP1State.triggers.left <= _analogicButtonDownValue; },
        () => { return oldGP1State.dPad.up == ButtonState.Pressed && newGP1State.dPad.up == ButtonState.Released; },
        () => { return oldGP1State.dPad.right == ButtonState.Pressed && newGP1State.dPad.right == ButtonState.Released; },
        () => { return oldGP1State.dPad.down == ButtonState.Pressed && newGP1State.dPad.down == ButtonState.Released; },
        () => { return oldGP1State.dPad.left == ButtonState.Pressed && newGP1State.dPad.left == ButtonState.Released; },
        () => { return oldGP1State.thumbSticks.right.y > _analogicButtonDownValue && newGP1State.thumbSticks.right.y <= _analogicButtonDownValue; },
        () => { return oldGP1State.thumbSticks.right.y < -_analogicButtonDownValue && newGP1State.thumbSticks.right.y >= -_analogicButtonDownValue; },
        () => { return oldGP1State.thumbSticks.right.x > _analogicButtonDownValue && newGP1State.thumbSticks.right.x <= _analogicButtonDownValue; },
        () => { return oldGP1State.thumbSticks.right.x < -_analogicButtonDownValue && newGP1State.thumbSticks.right.x >= -_analogicButtonDownValue; },
        () => { return oldGP1State.thumbSticks.left.y > _analogicButtonDownValue && newGP1State.thumbSticks.left.y < _analogicButtonDownValue; },
        () => { return oldGP1State.thumbSticks.left.y < -_analogicButtonDownValue && newGP1State.thumbSticks.left.y > -_analogicButtonDownValue; },
        () => { return oldGP1State.thumbSticks.left.x > _analogicButtonDownValue && newGP1State.thumbSticks.left.x < _analogicButtonDownValue; },
        () => { return oldGP1State.thumbSticks.left.x < -_analogicButtonDownValue && newGP1State.thumbSticks.left.x > -_analogicButtonDownValue; },

        () => { return oldGP2State.triggers.right > _analogicButtonDownValue && newGP2State.triggers.right <= _analogicButtonDownValue; },
        () => { return oldGP2State.triggers.left > _analogicButtonDownValue && newGP2State.triggers.left <= _analogicButtonDownValue; },
        () => { return oldGP2State.dPad.up == ButtonState.Pressed && newGP2State.dPad.up == ButtonState.Released; },
        () => { return oldGP2State.dPad.right == ButtonState.Pressed && newGP2State.dPad.right == ButtonState.Released; },
        () => { return oldGP2State.dPad.down == ButtonState.Pressed && newGP2State.dPad.down == ButtonState.Released; },
        () => { return oldGP2State.dPad.left == ButtonState.Pressed && newGP2State.dPad.left == ButtonState.Released; },
        () => { return oldGP2State.thumbSticks.right.y > _analogicButtonDownValue && newGP2State.thumbSticks.right.y <= _analogicButtonDownValue; },
        () => { return oldGP2State.thumbSticks.right.y < -_analogicButtonDownValue && newGP2State.thumbSticks.right.y >= -_analogicButtonDownValue; },
        () => { return oldGP2State.thumbSticks.right.x > _analogicButtonDownValue && newGP2State.thumbSticks.right.x <= _analogicButtonDownValue; },
        () => { return oldGP2State.thumbSticks.right.x < -_analogicButtonDownValue && newGP2State.thumbSticks.right.x >= -_analogicButtonDownValue; },
        () => { return oldGP2State.thumbSticks.left.y > _analogicButtonDownValue && newGP2State.thumbSticks.left.y <= _analogicButtonDownValue; },
        () => { return oldGP2State.thumbSticks.left.y < -_analogicButtonDownValue && newGP2State.thumbSticks.left.y >= -_analogicButtonDownValue; },
        () => { return oldGP2State.thumbSticks.left.x > _analogicButtonDownValue && newGP2State.thumbSticks.left.x <= _analogicButtonDownValue; },
        () => { return oldGP2State.thumbSticks.left.x < -_analogicButtonDownValue && newGP2State.thumbSticks.left.x >= -_analogicButtonDownValue; },

        () => { return oldGP3State.triggers.right > _analogicButtonDownValue && newGP3State.triggers.right <= _analogicButtonDownValue; },
        () => { return oldGP3State.triggers.left > _analogicButtonDownValue && newGP3State.triggers.left <= _analogicButtonDownValue; },
        () => { return oldGP3State.dPad.up == ButtonState.Pressed && newGP3State.dPad.up == ButtonState.Released; },
        () => { return oldGP3State.dPad.right == ButtonState.Pressed && newGP3State.dPad.right == ButtonState.Released; },
        () => { return oldGP3State.dPad.down == ButtonState.Pressed && newGP3State.dPad.down == ButtonState.Released; },
        () => { return oldGP3State.dPad.left == ButtonState.Pressed && newGP3State.dPad.left == ButtonState.Released; },
        () => { return oldGP3State.thumbSticks.right.y > _analogicButtonDownValue && newGP3State.thumbSticks.right.y < _analogicButtonDownValue; },
        () => { return oldGP3State.thumbSticks.right.y < -_analogicButtonDownValue && newGP3State.thumbSticks.right.y > -_analogicButtonDownValue; },
        () => { return oldGP3State.thumbSticks.right.x > _analogicButtonDownValue && newGP3State.thumbSticks.right.x < _analogicButtonDownValue; },
        () => { return oldGP3State.thumbSticks.right.x < -_analogicButtonDownValue && newGP3State.thumbSticks.right.x > -_analogicButtonDownValue; },
        () => { return oldGP3State.thumbSticks.left.y > _analogicButtonDownValue && newGP3State.thumbSticks.left.y < _analogicButtonDownValue; },
        () => { return oldGP3State.thumbSticks.left.y < -_analogicButtonDownValue && newGP3State.thumbSticks.left.y > -_analogicButtonDownValue; },
        () => { return oldGP3State.thumbSticks.left.x > _analogicButtonDownValue && newGP3State.thumbSticks.left.x < _analogicButtonDownValue; },
        () => { return oldGP3State.thumbSticks.left.x < -_analogicButtonDownValue && newGP3State.thumbSticks.left.x > -_analogicButtonDownValue; },

        () => { return oldGP4State.triggers.right > _analogicButtonDownValue && newGP4State.triggers.right <= _analogicButtonDownValue; },
        () => { return oldGP4State.triggers.left > _analogicButtonDownValue && newGP4State.triggers.left <= _analogicButtonDownValue; },
        () => { return oldGP4State.dPad.up == ButtonState.Pressed && newGP4State.dPad.up == ButtonState.Released; },
        () => { return oldGP4State.dPad.right == ButtonState.Pressed && newGP4State.dPad.right == ButtonState.Released; },
        () => { return oldGP4State.dPad.down == ButtonState.Pressed && newGP4State.dPad.down == ButtonState.Released; },
        () => { return oldGP4State.dPad.left == ButtonState.Pressed && newGP4State.dPad.left == ButtonState.Released; },
        () => { return oldGP4State.thumbSticks.right.y > _analogicButtonDownValue && newGP4State.thumbSticks.right.y < _analogicButtonDownValue; },
        () => { return oldGP4State.thumbSticks.right.y < -_analogicButtonDownValue && newGP4State.thumbSticks.right.y > -_analogicButtonDownValue; },
        () => { return oldGP4State.thumbSticks.right.x > _analogicButtonDownValue && newGP4State.thumbSticks.right.x < _analogicButtonDownValue; },
        () => { return oldGP4State.thumbSticks.right.x < -_analogicButtonDownValue && newGP4State.thumbSticks.right.x > -_analogicButtonDownValue; },
        () => { return oldGP4State.thumbSticks.left.y > _analogicButtonDownValue && newGP4State.thumbSticks.left.y < _analogicButtonDownValue; },
        () => { return oldGP4State.thumbSticks.left.y < -_analogicButtonDownValue && newGP4State.thumbSticks.left.y > -_analogicButtonDownValue; },
        () => { return oldGP4State.thumbSticks.left.x > _analogicButtonDownValue && newGP4State.thumbSticks.left.x < _analogicButtonDownValue; },
        () => { return oldGP4State.thumbSticks.left.x < -_analogicButtonDownValue && newGP4State.thumbSticks.left.x > -_analogicButtonDownValue; },

        () => { return (oldGP1State.triggers.right > _analogicButtonDownValue && newGP1State.triggers.right <= _analogicButtonDownValue)
            || (oldGP2State.triggers.right > _analogicButtonDownValue && newGP2State.triggers.right <= _analogicButtonDownValue)
            || (oldGP3State.triggers.right > _analogicButtonDownValue && newGP3State.triggers.right <= _analogicButtonDownValue)
            || (oldGP4State.triggers.right > _analogicButtonDownValue && newGP4State.triggers.right <= _analogicButtonDownValue); },
        () => { return (oldGP1State.triggers.left > _analogicButtonDownValue && newGP1State.triggers.left <= _analogicButtonDownValue)
            || (oldGP2State.triggers.left > _analogicButtonDownValue && newGP2State.triggers.left <= _analogicButtonDownValue)
            || (oldGP3State.triggers.left > _analogicButtonDownValue && newGP3State.triggers.left <= _analogicButtonDownValue)
            || (oldGP4State.triggers.left > _analogicButtonDownValue && newGP4State.triggers.left <= _analogicButtonDownValue); },
        () => { return (oldGP1State.dPad.up == ButtonState.Pressed && newGP1State.dPad.up == ButtonState.Released)
            || (oldGP2State.dPad.up == ButtonState.Pressed && newGP2State.dPad.up == ButtonState.Released)
            || (oldGP3State.dPad.up == ButtonState.Pressed && newGP3State.dPad.up == ButtonState.Released)
            || (oldGP4State.dPad.up == ButtonState.Pressed && newGP4State.dPad.up == ButtonState.Released); },
        () => { return (oldGP1State.dPad.right == ButtonState.Pressed && newGP1State.dPad.right == ButtonState.Released)
            || (oldGP2State.dPad.right == ButtonState.Pressed && newGP2State.dPad.right == ButtonState.Released)
            || (oldGP3State.dPad.right == ButtonState.Pressed && newGP3State.dPad.right == ButtonState.Released)
            || (oldGP4State.dPad.right == ButtonState.Pressed && newGP4State.dPad.right == ButtonState.Released); },
        () => { return (oldGP1State.dPad.down == ButtonState.Pressed && newGP1State.dPad.down == ButtonState.Released)
            || (oldGP2State.dPad.down == ButtonState.Pressed && newGP2State.dPad.down == ButtonState.Released)
            || (oldGP3State.dPad.down == ButtonState.Pressed && newGP3State.dPad.down == ButtonState.Released)
            || (oldGP4State.dPad.down == ButtonState.Pressed && newGP4State.dPad.down == ButtonState.Released); },
        () => { return (oldGP1State.dPad.left == ButtonState.Pressed && newGP1State.dPad.left == ButtonState.Released)
            || (oldGP2State.dPad.left == ButtonState.Pressed && newGP2State.dPad.left == ButtonState.Released)
            || (oldGP3State.dPad.left == ButtonState.Pressed && newGP3State.dPad.left == ButtonState.Released)
            || (oldGP4State.dPad.left == ButtonState.Pressed && newGP4State.dPad.left == ButtonState.Released); },
        () => { return (oldGP1State.thumbSticks.right.y > _analogicButtonDownValue && newGP1State.thumbSticks.right.y <= _analogicButtonDownValue)
            || (oldGP2State.thumbSticks.right.y > _analogicButtonDownValue && newGP2State.thumbSticks.right.y <= _analogicButtonDownValue)
            || (oldGP3State.thumbSticks.right.y > _analogicButtonDownValue && newGP3State.thumbSticks.right.y <= _analogicButtonDownValue)
            || (oldGP4State.thumbSticks.right.y > _analogicButtonDownValue && newGP4State.thumbSticks.right.y <= _analogicButtonDownValue); },
        () => { return (oldGP1State.thumbSticks.right.y < -_analogicButtonDownValue && newGP1State.thumbSticks.right.y >= -_analogicButtonDownValue)
            || (oldGP2State.thumbSticks.right.y < _analogicButtonDownValue && newGP2State.thumbSticks.right.y >= _analogicButtonDownValue)
            || (oldGP3State.thumbSticks.right.y < -_analogicButtonDownValue && newGP3State.thumbSticks.right.y >= -_analogicButtonDownValue)
            || (oldGP4State.thumbSticks.right.y < -_analogicButtonDownValue && newGP4State.thumbSticks.right.y >= -_analogicButtonDownValue); },
        () => { return (oldGP1State.thumbSticks.right.x > _analogicButtonDownValue && newGP1State.thumbSticks.right.x <= _analogicButtonDownValue)
            || (oldGP2State.thumbSticks.right.x > _analogicButtonDownValue && newGP2State.thumbSticks.right.x <= _analogicButtonDownValue)
            || (oldGP3State.thumbSticks.right.x > _analogicButtonDownValue && newGP3State.thumbSticks.right.x <= _analogicButtonDownValue)
            || (oldGP4State.thumbSticks.right.x > _analogicButtonDownValue && newGP4State.thumbSticks.right.x <= _analogicButtonDownValue); },
        () => { return (oldGP1State.thumbSticks.right.x < -_analogicButtonDownValue && newGP1State.thumbSticks.right.x >= -_analogicButtonDownValue)
            || (oldGP2State.thumbSticks.right.x < -_analogicButtonDownValue && newGP2State.thumbSticks.right.x >= -_analogicButtonDownValue)
            || (oldGP3State.thumbSticks.right.x < -_analogicButtonDownValue && newGP3State.thumbSticks.right.x >= -_analogicButtonDownValue)
            || (oldGP4State.thumbSticks.right.x < -_analogicButtonDownValue && newGP4State.thumbSticks.right.x >= -_analogicButtonDownValue); },
        () => { return (oldGP1State.thumbSticks.left.y > _analogicButtonDownValue && newGP1State.thumbSticks.left.y <= _analogicButtonDownValue)
            || (oldGP2State.thumbSticks.left.y > _analogicButtonDownValue && newGP2State.thumbSticks.left.y <= _analogicButtonDownValue)
            || (oldGP3State.thumbSticks.left.y > _analogicButtonDownValue && newGP3State.thumbSticks.left.y <= _analogicButtonDownValue)
            || (oldGP4State.thumbSticks.left.y > _analogicButtonDownValue && newGP4State.thumbSticks.left.y <= _analogicButtonDownValue); },
        () => { return (oldGP1State.thumbSticks.left.y < -_analogicButtonDownValue && newGP1State.thumbSticks.left.y >= -_analogicButtonDownValue)
            || (oldGP2State.thumbSticks.left.y < -_analogicButtonDownValue && newGP2State.thumbSticks.left.y >= -_analogicButtonDownValue)
            || (oldGP3State.thumbSticks.left.y < -_analogicButtonDownValue && newGP3State.thumbSticks.left.y >= -_analogicButtonDownValue)
            || (oldGP4State.thumbSticks.left.y < -_analogicButtonDownValue && newGP4State.thumbSticks.left.y >= -_analogicButtonDownValue); },
        () => { return (oldGP1State.thumbSticks.left.x > _analogicButtonDownValue && newGP1State.thumbSticks.left.x <= _analogicButtonDownValue)
            || (oldGP2State.thumbSticks.left.x > _analogicButtonDownValue && newGP2State.thumbSticks.left.x <= _analogicButtonDownValue)
            || (oldGP3State.thumbSticks.left.x > _analogicButtonDownValue && newGP3State.thumbSticks.left.x <= _analogicButtonDownValue)
            || (oldGP4State.thumbSticks.left.x > _analogicButtonDownValue && newGP4State.thumbSticks.left.x <= _analogicButtonDownValue); },
        () => { return (oldGP1State.thumbSticks.left.x < -_analogicButtonDownValue && newGP1State.thumbSticks.left.x >= -_analogicButtonDownValue)
            || (oldGP2State.thumbSticks.left.x < -_analogicButtonDownValue && newGP2State.thumbSticks.left.x >= -_analogicButtonDownValue)
            || (oldGP3State.thumbSticks.left.x < -_analogicButtonDownValue && newGP3State.thumbSticks.left.x >= -_analogicButtonDownValue)
            || (oldGP4State.thumbSticks.left.x < -_analogicButtonDownValue && newGP4State.thumbSticks.left.x >= -_analogicButtonDownValue); }
    };

    private static readonly Func<bool>[] getInputKeyPressedDelegate = new Func<bool>[71]
    {
        () => { return false; },
        () => { return newGP1State.triggers.right > _analogicButtonDownValue; },
        () => { return newGP1State.triggers.left > _analogicButtonDownValue; },
        () => { return newGP1State.dPad.up == ButtonState.Pressed; },
        () => { return newGP1State.dPad.right == ButtonState.Pressed; },
        () => { return newGP1State.dPad.down == ButtonState.Pressed; },
        () => { return newGP1State.dPad.left == ButtonState.Pressed; },
        () => { return newGP1State.thumbSticks.right.y > _analogicButtonDownValue; },
        () => { return newGP1State.thumbSticks.right.y < -_analogicButtonDownValue; },
        () => { return newGP1State.thumbSticks.right.x > _analogicButtonDownValue; },
        () => { return newGP1State.thumbSticks.right.x < -_analogicButtonDownValue; },
        () => { return newGP1State.thumbSticks.left.y > _analogicButtonDownValue; },
        () => { return newGP1State.thumbSticks.left.y < -_analogicButtonDownValue; },
        () => { return newGP1State.thumbSticks.left.x > _analogicButtonDownValue; },
        () => { return newGP1State.thumbSticks.left.x < -_analogicButtonDownValue; },

        () => { return newGP2State.triggers.right > _analogicButtonDownValue; },
        () => { return newGP2State.triggers.left > _analogicButtonDownValue; },
        () => { return newGP2State.dPad.up == ButtonState.Pressed; },
        () => { return newGP2State.dPad.right == ButtonState.Pressed; },
        () => { return newGP2State.dPad.down == ButtonState.Pressed; },
        () => { return newGP2State.dPad.left == ButtonState.Pressed; },
        () => { return newGP2State.thumbSticks.right.y > _analogicButtonDownValue; },
        () => { return newGP2State.thumbSticks.right.y < -_analogicButtonDownValue; },
        () => { return newGP2State.thumbSticks.right.x > _analogicButtonDownValue; },
        () => { return newGP2State.thumbSticks.right.x < -_analogicButtonDownValue; },
        () => { return newGP2State.thumbSticks.left.y > _analogicButtonDownValue; },
        () => { return newGP2State.thumbSticks.left.y < -_analogicButtonDownValue; },
        () => { return newGP2State.thumbSticks.left.x > _analogicButtonDownValue; },
        () => { return newGP2State.thumbSticks.left.x < -_analogicButtonDownValue; },

        () => { return newGP3State.triggers.right > _analogicButtonDownValue; },
        () => { return newGP3State.triggers.left > _analogicButtonDownValue; },
        () => { return newGP3State.dPad.up == ButtonState.Pressed; },
        () => { return newGP3State.dPad.right == ButtonState.Pressed; },
        () => { return newGP3State.dPad.down == ButtonState.Pressed; },
        () => { return newGP3State.dPad.left == ButtonState.Pressed; },
        () => { return newGP3State.thumbSticks.right.y > _analogicButtonDownValue; },
        () => { return newGP3State.thumbSticks.right.y < -_analogicButtonDownValue; },
        () => { return newGP3State.thumbSticks.right.x > _analogicButtonDownValue; },
        () => { return newGP3State.thumbSticks.right.x < -_analogicButtonDownValue; },
        () => { return newGP3State.thumbSticks.left.y > _analogicButtonDownValue; },
        () => { return newGP3State.thumbSticks.left.y < -_analogicButtonDownValue; },
        () => { return newGP3State.thumbSticks.left.x > _analogicButtonDownValue; },
        () => { return newGP3State.thumbSticks.left.x < -_analogicButtonDownValue; },

        () => { return newGP4State.triggers.right > _analogicButtonDownValue; },
        () => { return newGP4State.triggers.left > _analogicButtonDownValue; },
        () => { return newGP4State.dPad.up == ButtonState.Pressed; },
        () => { return newGP4State.dPad.right == ButtonState.Pressed; },
        () => { return newGP4State.dPad.down == ButtonState.Pressed; },
        () => { return newGP4State.dPad.left == ButtonState.Pressed; },
        () => { return newGP4State.thumbSticks.right.y > _analogicButtonDownValue; },
        () => { return newGP4State.thumbSticks.right.y < -_analogicButtonDownValue; },
        () => { return newGP4State.thumbSticks.right.x > _analogicButtonDownValue; },
        () => { return newGP4State.thumbSticks.right.x < -_analogicButtonDownValue; },
        () => { return newGP4State.thumbSticks.left.y > _analogicButtonDownValue; },
        () => { return newGP4State.thumbSticks.left.y < -_analogicButtonDownValue; },
        () => { return newGP4State.thumbSticks.left.x > _analogicButtonDownValue; },
        () => { return newGP4State.thumbSticks.left.x < -_analogicButtonDownValue; },

        () => { return (newGP1State.triggers.right > _analogicButtonDownValue)
            || (newGP2State.triggers.right > _analogicButtonDownValue)
            || (newGP3State.triggers.right > _analogicButtonDownValue)
            || (newGP4State.triggers.right > _analogicButtonDownValue); },
        () => { return (newGP1State.triggers.left > _analogicButtonDownValue)
            || (newGP2State.triggers.left > _analogicButtonDownValue)
            || (newGP3State.triggers.left > _analogicButtonDownValue)
            || (newGP4State.triggers.left > _analogicButtonDownValue); },
        () => { return (newGP1State.dPad.up == ButtonState.Pressed)
            || (newGP2State.dPad.up == ButtonState.Pressed)
            || (newGP3State.dPad.up == ButtonState.Pressed)
            || (newGP4State.dPad.up == ButtonState.Pressed); },
        () => { return (newGP1State.dPad.right == ButtonState.Pressed)
            || (newGP2State.dPad.right == ButtonState.Pressed)
            || (newGP3State.dPad.right == ButtonState.Pressed)
            || (newGP4State.dPad.right == ButtonState.Pressed); },
        () => { return (newGP1State.dPad.down == ButtonState.Pressed)
            || (newGP2State.dPad.down == ButtonState.Pressed)
            || (newGP3State.dPad.down == ButtonState.Pressed)
            || (newGP4State.dPad.down == ButtonState.Pressed); },
        () => { return (newGP1State.dPad.left == ButtonState.Pressed)
            || (newGP2State.dPad.left == ButtonState.Pressed)
            || (newGP3State.dPad.left == ButtonState.Pressed)
            || (newGP4State.dPad.left == ButtonState.Pressed); },
        () => { return (newGP1State.thumbSticks.right.y > _analogicButtonDownValue)
            || (newGP2State.thumbSticks.right.y > _analogicButtonDownValue)
            || (newGP3State.thumbSticks.right.y > _analogicButtonDownValue)
            || (newGP4State.thumbSticks.right.y > _analogicButtonDownValue); },
        () => { return (newGP1State.thumbSticks.right.y < -_analogicButtonDownValue)
            || (newGP2State.thumbSticks.right.y < _analogicButtonDownValue)
            || (newGP3State.thumbSticks.right.y < -_analogicButtonDownValue)
            || (newGP4State.thumbSticks.right.y < -_analogicButtonDownValue); },
        () => { return (newGP1State.thumbSticks.right.x > _analogicButtonDownValue)
            || (newGP2State.thumbSticks.right.x > _analogicButtonDownValue)
            || (newGP3State.thumbSticks.right.x > _analogicButtonDownValue)
            || (newGP4State.thumbSticks.right.x > _analogicButtonDownValue); },
        () => { return (newGP1State.thumbSticks.right.x < -_analogicButtonDownValue)
            || (newGP2State.thumbSticks.right.x < -_analogicButtonDownValue)
            || (newGP3State.thumbSticks.right.x < -_analogicButtonDownValue)
            || (newGP4State.thumbSticks.right.x < -_analogicButtonDownValue); },
        () => { return (newGP1State.thumbSticks.left.y > _analogicButtonDownValue)
            || (newGP2State.thumbSticks.left.y > _analogicButtonDownValue)
            || (newGP3State.thumbSticks.left.y > _analogicButtonDownValue)
            || (newGP4State.thumbSticks.left.y > _analogicButtonDownValue); },
        () => { return (newGP1State.thumbSticks.left.y < -_analogicButtonDownValue)
            || (newGP2State.thumbSticks.left.y < -_analogicButtonDownValue)
            || (newGP3State.thumbSticks.left.y < -_analogicButtonDownValue)
            || (newGP4State.thumbSticks.left.y < -_analogicButtonDownValue); },
        () => { return (newGP1State.thumbSticks.left.x > _analogicButtonDownValue)
            || (newGP2State.thumbSticks.left.x > _analogicButtonDownValue)
            || (newGP3State.thumbSticks.left.x > _analogicButtonDownValue)
            || (newGP4State.thumbSticks.left.x > _analogicButtonDownValue); },
        () => { return (newGP1State.thumbSticks.left.x < -_analogicButtonDownValue)
            || (newGP2State.thumbSticks.left.x < -_analogicButtonDownValue)
            || (newGP3State.thumbSticks.left.x < -_analogicButtonDownValue)
            || (newGP4State.thumbSticks.left.x < -_analogicButtonDownValue); }
    };

    #endregion

    #region Positive Input

    private static readonly Func<bool>[] getInputKeyDownPositiveDelegate = new Func<bool>[]
    {
        () => (oldGP1State.buttons.a == ButtonState.Released && newGP1State.buttons.a == ButtonState.Pressed) ||
              (oldGP2State.buttons.a == ButtonState.Released && newGP2State.buttons.a == ButtonState.Pressed) ||
              (oldGP3State.buttons.a == ButtonState.Released && newGP3State.buttons.a == ButtonState.Pressed) ||
              (oldGP4State.buttons.a == ButtonState.Released && newGP4State.buttons.a == ButtonState.Pressed),
        () => (oldGP1State.buttons.b == ButtonState.Released && newGP1State.buttons.b == ButtonState.Pressed) ||
              (oldGP2State.buttons.b == ButtonState.Released && newGP2State.buttons.b == ButtonState.Pressed) ||
              (oldGP3State.buttons.b == ButtonState.Released && newGP3State.buttons.b == ButtonState.Pressed) ||
              (oldGP4State.buttons.b == ButtonState.Released && newGP4State.buttons.b == ButtonState.Pressed),
        () => (oldGP1State.buttons.x == ButtonState.Released && newGP1State.buttons.x == ButtonState.Pressed) ||
              (oldGP2State.buttons.x == ButtonState.Released && newGP2State.buttons.x == ButtonState.Pressed) ||
              (oldGP3State.buttons.x == ButtonState.Released && newGP3State.buttons.x == ButtonState.Pressed) ||
              (oldGP4State.buttons.x == ButtonState.Released && newGP4State.buttons.x == ButtonState.Pressed),
        () => (oldGP1State.buttons.y == ButtonState.Released && newGP1State.buttons.y == ButtonState.Pressed) ||
              (oldGP2State.buttons.y == ButtonState.Released && newGP2State.buttons.y == ButtonState.Pressed) ||
              (oldGP3State.buttons.y == ButtonState.Released && newGP3State.buttons.y == ButtonState.Pressed) ||
              (oldGP4State.buttons.y == ButtonState.Released && newGP4State.buttons.y == ButtonState.Pressed),
        () => (oldGP1State.buttons.leftShoulder == ButtonState.Released && newGP1State.buttons.leftShoulder == ButtonState.Pressed) ||
              (oldGP2State.buttons.leftShoulder == ButtonState.Released && newGP2State.buttons.leftShoulder == ButtonState.Pressed) ||
              (oldGP3State.buttons.leftShoulder == ButtonState.Released && newGP3State.buttons.leftShoulder == ButtonState.Pressed) ||
              (oldGP4State.buttons.leftShoulder == ButtonState.Released && newGP4State.buttons.leftShoulder == ButtonState.Pressed),
        () => (oldGP1State.buttons.rightShoulder == ButtonState.Released && newGP1State.buttons.rightShoulder == ButtonState.Pressed) ||
              (oldGP2State.buttons.rightShoulder == ButtonState.Released && newGP2State.buttons.rightShoulder == ButtonState.Pressed) ||
              (oldGP3State.buttons.rightShoulder == ButtonState.Released && newGP3State.buttons.rightShoulder == ButtonState.Pressed) ||
              (oldGP4State.buttons.rightShoulder == ButtonState.Released && newGP4State.buttons.rightShoulder == ButtonState.Pressed),
        () => (oldGP1State.buttons.back == ButtonState.Released && newGP1State.buttons.back == ButtonState.Pressed) ||
              (oldGP2State.buttons.back == ButtonState.Released && newGP2State.buttons.back == ButtonState.Pressed) ||
              (oldGP3State.buttons.back == ButtonState.Released && newGP3State.buttons.back == ButtonState.Pressed) ||
              (oldGP4State.buttons.back == ButtonState.Released && newGP4State.buttons.back == ButtonState.Pressed),
        () => (oldGP1State.buttons.start == ButtonState.Released && newGP1State.buttons.start == ButtonState.Pressed) ||
              (oldGP2State.buttons.start == ButtonState.Released && newGP2State.buttons.start == ButtonState.Pressed) ||
              (oldGP3State.buttons.start == ButtonState.Released && newGP3State.buttons.start == ButtonState.Pressed) ||
              (oldGP4State.buttons.start == ButtonState.Released && newGP4State.buttons.start == ButtonState.Pressed),
        () => (oldGP1State.buttons.leftStick == ButtonState.Released && newGP1State.buttons.leftStick == ButtonState.Pressed) ||
              (oldGP2State.buttons.leftStick == ButtonState.Released && newGP2State.buttons.leftStick == ButtonState.Pressed) ||
              (oldGP3State.buttons.leftStick == ButtonState.Released && newGP3State.buttons.leftStick == ButtonState.Pressed) ||
              (oldGP4State.buttons.leftStick == ButtonState.Released && newGP4State.buttons.leftStick == ButtonState.Pressed),
        () => (oldGP1State.buttons.rightStick == ButtonState.Released && newGP1State.buttons.rightStick == ButtonState.Pressed) ||
              (oldGP2State.buttons.rightStick == ButtonState.Released && newGP2State.buttons.rightStick == ButtonState.Pressed) ||
              (oldGP3State.buttons.rightStick == ButtonState.Released && newGP3State.buttons.rightStick == ButtonState.Pressed) ||
              (oldGP4State.buttons.rightStick == ButtonState.Released && newGP4State.buttons.rightStick == ButtonState.Pressed),

        () => oldGP1State.buttons.a == ButtonState.Released && newGP1State.buttons.a == ButtonState.Pressed,
        () => oldGP1State.buttons.b == ButtonState.Released && newGP1State.buttons.b == ButtonState.Pressed,
        () => oldGP1State.buttons.x == ButtonState.Released && newGP1State.buttons.x == ButtonState.Pressed,
        () => oldGP1State.buttons.y == ButtonState.Released && newGP1State.buttons.y == ButtonState.Pressed,
        () => oldGP1State.buttons.leftShoulder == ButtonState.Released && newGP1State.buttons.leftShoulder == ButtonState.Pressed,
        () => oldGP1State.buttons.rightShoulder == ButtonState.Released && newGP1State.buttons.rightShoulder == ButtonState.Pressed,
        () => oldGP1State.buttons.back == ButtonState.Released && newGP1State.buttons.back == ButtonState.Pressed,
        () => oldGP1State.buttons.start == ButtonState.Released && newGP1State.buttons.start == ButtonState.Pressed,
        () => oldGP1State.buttons.leftStick == ButtonState.Released && newGP1State.buttons.leftStick == ButtonState.Pressed,
        () => oldGP1State.buttons.rightStick == ButtonState.Released && newGP1State.buttons.rightStick == ButtonState.Pressed,

        () => oldGP2State.buttons.a == ButtonState.Released && newGP2State.buttons.a == ButtonState.Pressed,
        () => oldGP2State.buttons.b == ButtonState.Released && newGP2State.buttons.b == ButtonState.Pressed,
        () => oldGP2State.buttons.x == ButtonState.Released && newGP2State.buttons.x == ButtonState.Pressed,
        () => oldGP2State.buttons.y == ButtonState.Released && newGP2State.buttons.y == ButtonState.Pressed,
        () => oldGP2State.buttons.leftShoulder == ButtonState.Released && newGP2State.buttons.leftShoulder == ButtonState.Pressed,
        () => oldGP2State.buttons.rightShoulder == ButtonState.Released && newGP2State.buttons.rightShoulder == ButtonState.Pressed,
        () => oldGP2State.buttons.back == ButtonState.Released && newGP2State.buttons.back == ButtonState.Pressed,
        () => oldGP2State.buttons.start == ButtonState.Released && newGP2State.buttons.start == ButtonState.Pressed,
        () => oldGP2State.buttons.leftStick == ButtonState.Released && newGP2State.buttons.leftStick == ButtonState.Pressed,
        () => oldGP2State.buttons.rightStick == ButtonState.Released && newGP2State.buttons.rightStick == ButtonState.Pressed,

        () => oldGP3State.buttons.a == ButtonState.Released && newGP3State.buttons.a == ButtonState.Pressed,
        () => oldGP3State.buttons.b == ButtonState.Released && newGP3State.buttons.b == ButtonState.Pressed,
        () => oldGP3State.buttons.x == ButtonState.Released && newGP3State.buttons.x == ButtonState.Pressed,
        () => oldGP3State.buttons.y == ButtonState.Released && newGP3State.buttons.y == ButtonState.Pressed,
        () => oldGP3State.buttons.leftShoulder == ButtonState.Released && newGP3State.buttons.leftShoulder == ButtonState.Pressed,
        () => oldGP3State.buttons.rightShoulder == ButtonState.Released && newGP3State.buttons.rightShoulder == ButtonState.Pressed,
        () => oldGP3State.buttons.back == ButtonState.Released && newGP3State.buttons.back == ButtonState.Pressed,
        () => oldGP3State.buttons.start == ButtonState.Released && newGP3State.buttons.start == ButtonState.Pressed,
        () => oldGP3State.buttons.leftStick == ButtonState.Released && newGP3State.buttons.leftStick == ButtonState.Pressed,
        () => oldGP3State.buttons.rightStick == ButtonState.Released && newGP3State.buttons.rightStick == ButtonState.Pressed,

        () => oldGP4State.buttons.a == ButtonState.Released && newGP4State.buttons.a == ButtonState.Pressed,
        () => oldGP4State.buttons.b == ButtonState.Released && newGP4State.buttons.b == ButtonState.Pressed,
        () => oldGP4State.buttons.x == ButtonState.Released && newGP4State.buttons.x == ButtonState.Pressed,
        () => oldGP4State.buttons.y == ButtonState.Released && newGP4State.buttons.y == ButtonState.Pressed,
        () => oldGP4State.buttons.leftShoulder == ButtonState.Released && newGP4State.buttons.leftShoulder == ButtonState.Pressed,
        () => oldGP4State.buttons.rightShoulder == ButtonState.Released && newGP4State.buttons.rightShoulder == ButtonState.Pressed,
        () => oldGP4State.buttons.back == ButtonState.Released && newGP4State.buttons.back == ButtonState.Pressed,
        () => oldGP4State.buttons.start == ButtonState.Released && newGP4State.buttons.start == ButtonState.Pressed,
        () => oldGP4State.buttons.leftStick == ButtonState.Released && newGP4State.buttons.leftStick == ButtonState.Pressed,
        () => oldGP4State.buttons.rightStick == ButtonState.Released && newGP4State.buttons.rightStick == ButtonState.Pressed
    };

    private static readonly Func<bool>[] getInputKeyUpPositiveDelegate = new Func<bool>[]
    {
        () => (oldGP1State.buttons.a == ButtonState.Pressed && newGP1State.buttons.a == ButtonState.Released) ||
              (oldGP2State.buttons.a == ButtonState.Pressed && newGP2State.buttons.a == ButtonState.Released) ||
              (oldGP3State.buttons.a == ButtonState.Pressed && newGP3State.buttons.a == ButtonState.Released) ||
              (oldGP4State.buttons.a == ButtonState.Pressed && newGP4State.buttons.a == ButtonState.Released),
        () => (oldGP1State.buttons.b == ButtonState.Pressed && newGP1State.buttons.b == ButtonState.Released) ||
              (oldGP2State.buttons.b == ButtonState.Pressed && newGP2State.buttons.b == ButtonState.Released) ||
              (oldGP3State.buttons.b == ButtonState.Pressed && newGP3State.buttons.b == ButtonState.Released) ||
              (oldGP4State.buttons.b == ButtonState.Pressed && newGP4State.buttons.b == ButtonState.Released),
        () => (oldGP1State.buttons.x == ButtonState.Pressed && newGP1State.buttons.x == ButtonState.Released) ||
              (oldGP2State.buttons.x == ButtonState.Pressed && newGP2State.buttons.x == ButtonState.Released) ||
              (oldGP3State.buttons.x == ButtonState.Pressed && newGP3State.buttons.x == ButtonState.Released) ||
              (oldGP4State.buttons.x == ButtonState.Pressed && newGP4State.buttons.x == ButtonState.Released),
        () => (oldGP1State.buttons.y == ButtonState.Pressed && newGP1State.buttons.y == ButtonState.Released) ||
              (oldGP2State.buttons.y == ButtonState.Pressed && newGP2State.buttons.y == ButtonState.Released) ||
              (oldGP3State.buttons.y == ButtonState.Pressed && newGP3State.buttons.y == ButtonState.Released) ||
              (oldGP4State.buttons.y == ButtonState.Pressed && newGP4State.buttons.y == ButtonState.Released),
        () => (oldGP1State.buttons.leftShoulder == ButtonState.Pressed && newGP1State.buttons.leftShoulder == ButtonState.Released) ||
              (oldGP2State.buttons.leftShoulder == ButtonState.Pressed && newGP2State.buttons.leftShoulder == ButtonState.Released) ||
              (oldGP3State.buttons.leftShoulder == ButtonState.Pressed && newGP3State.buttons.leftShoulder == ButtonState.Released) ||
              (oldGP4State.buttons.leftShoulder == ButtonState.Pressed && newGP4State.buttons.leftShoulder == ButtonState.Released),
        () => (oldGP1State.buttons.rightShoulder == ButtonState.Pressed && newGP1State.buttons.rightShoulder == ButtonState.Released) ||
              (oldGP2State.buttons.rightShoulder == ButtonState.Pressed && newGP2State.buttons.rightShoulder == ButtonState.Released) ||
              (oldGP3State.buttons.rightShoulder == ButtonState.Pressed && newGP3State.buttons.rightShoulder == ButtonState.Released) ||
              (oldGP4State.buttons.rightShoulder == ButtonState.Pressed && newGP4State.buttons.rightShoulder == ButtonState.Released),
        () => (oldGP1State.buttons.back == ButtonState.Pressed && newGP1State.buttons.back == ButtonState.Released) ||
              (oldGP2State.buttons.back == ButtonState.Pressed && newGP2State.buttons.back == ButtonState.Released) ||
              (oldGP3State.buttons.back == ButtonState.Pressed && newGP3State.buttons.back == ButtonState.Released) ||
              (oldGP4State.buttons.back == ButtonState.Pressed && newGP4State.buttons.back == ButtonState.Released),
        () => (oldGP1State.buttons.start == ButtonState.Pressed && newGP1State.buttons.start == ButtonState.Released) ||
              (oldGP2State.buttons.start == ButtonState.Pressed && newGP2State.buttons.start == ButtonState.Released) ||
              (oldGP3State.buttons.start == ButtonState.Pressed && newGP3State.buttons.start == ButtonState.Released) ||
              (oldGP4State.buttons.start == ButtonState.Pressed && newGP4State.buttons.start == ButtonState.Released),
        () => (oldGP1State.buttons.leftStick == ButtonState.Pressed && newGP1State.buttons.leftStick == ButtonState.Released) ||
              (oldGP2State.buttons.leftStick == ButtonState.Pressed && newGP2State.buttons.leftStick == ButtonState.Released) ||
              (oldGP3State.buttons.leftStick == ButtonState.Pressed && newGP3State.buttons.leftStick == ButtonState.Released) ||
              (oldGP4State.buttons.leftStick == ButtonState.Pressed && newGP4State.buttons.leftStick == ButtonState.Released),
        () => (oldGP1State.buttons.rightStick == ButtonState.Pressed && newGP1State.buttons.rightStick == ButtonState.Released) ||
              (oldGP2State.buttons.rightStick == ButtonState.Pressed && newGP2State.buttons.rightStick == ButtonState.Released) ||
              (oldGP3State.buttons.rightStick == ButtonState.Pressed && newGP3State.buttons.rightStick == ButtonState.Released) ||
              (oldGP4State.buttons.rightStick == ButtonState.Pressed && newGP4State.buttons.rightStick == ButtonState.Released),

        () => oldGP1State.buttons.a == ButtonState.Pressed && newGP1State.buttons.a == ButtonState.Released,
        () => oldGP1State.buttons.b == ButtonState.Pressed && newGP1State.buttons.b == ButtonState.Released,
        () => oldGP1State.buttons.x == ButtonState.Pressed && newGP1State.buttons.x == ButtonState.Released,
        () => oldGP1State.buttons.y == ButtonState.Pressed && newGP1State.buttons.y == ButtonState.Released,
        () => oldGP1State.buttons.leftShoulder == ButtonState.Pressed && newGP1State.buttons.leftShoulder == ButtonState.Released,
        () => oldGP1State.buttons.rightShoulder == ButtonState.Pressed && newGP1State.buttons.rightShoulder == ButtonState.Released,
        () => oldGP1State.buttons.back == ButtonState.Pressed && newGP1State.buttons.back == ButtonState.Released,
        () => oldGP1State.buttons.start == ButtonState.Pressed && newGP1State.buttons.start == ButtonState.Released,
        () => oldGP1State.buttons.leftStick == ButtonState.Pressed && newGP1State.buttons.leftStick == ButtonState.Released,
        () => oldGP1State.buttons.rightStick == ButtonState.Pressed && newGP1State.buttons.rightStick == ButtonState.Released,

        () => oldGP2State.buttons.a == ButtonState.Pressed && newGP2State.buttons.a == ButtonState.Released,
        () => oldGP2State.buttons.b == ButtonState.Pressed && newGP2State.buttons.b == ButtonState.Released,
        () => oldGP2State.buttons.x == ButtonState.Pressed && newGP2State.buttons.x == ButtonState.Released,
        () => oldGP2State.buttons.y == ButtonState.Pressed && newGP2State.buttons.y == ButtonState.Released,
        () => oldGP2State.buttons.leftShoulder == ButtonState.Pressed && newGP2State.buttons.leftShoulder == ButtonState.Released,
        () => oldGP2State.buttons.rightShoulder == ButtonState.Pressed && newGP2State.buttons.rightShoulder == ButtonState.Released,
        () => oldGP2State.buttons.back == ButtonState.Pressed && newGP2State.buttons.back == ButtonState.Released,
        () => oldGP2State.buttons.start == ButtonState.Pressed && newGP2State.buttons.start == ButtonState.Released,
        () => oldGP2State.buttons.leftStick == ButtonState.Pressed && newGP2State.buttons.leftStick == ButtonState.Released,
        () => oldGP2State.buttons.rightStick == ButtonState.Pressed && newGP2State.buttons.rightStick == ButtonState.Released,

        () => oldGP3State.buttons.a == ButtonState.Pressed && newGP3State.buttons.a == ButtonState.Released,
        () => oldGP3State.buttons.b == ButtonState.Pressed && newGP3State.buttons.b == ButtonState.Released,
        () => oldGP3State.buttons.x == ButtonState.Pressed && newGP3State.buttons.x == ButtonState.Released,
        () => oldGP3State.buttons.y == ButtonState.Pressed && newGP3State.buttons.y == ButtonState.Released,
        () => oldGP3State.buttons.leftShoulder == ButtonState.Pressed && newGP3State.buttons.leftShoulder == ButtonState.Released,
        () => oldGP3State.buttons.rightShoulder == ButtonState.Pressed && newGP3State.buttons.rightShoulder == ButtonState.Released,
        () => oldGP3State.buttons.back == ButtonState.Pressed && newGP3State.buttons.back == ButtonState.Released,
        () => oldGP3State.buttons.start == ButtonState.Pressed && newGP3State.buttons.start == ButtonState.Released,
        () => oldGP3State.buttons.leftStick == ButtonState.Pressed && newGP3State.buttons.leftStick == ButtonState.Released,
        () => oldGP3State.buttons.rightStick == ButtonState.Pressed && newGP3State.buttons.rightStick == ButtonState.Released,

        () => oldGP4State.buttons.a == ButtonState.Pressed && newGP4State.buttons.a == ButtonState.Released,
        () => oldGP4State.buttons.b == ButtonState.Pressed && newGP4State.buttons.b == ButtonState.Released,
        () => oldGP4State.buttons.x == ButtonState.Pressed && newGP4State.buttons.x == ButtonState.Released,
        () => oldGP4State.buttons.y == ButtonState.Pressed && newGP4State.buttons.y == ButtonState.Released,
        () => oldGP4State.buttons.leftShoulder == ButtonState.Pressed && newGP4State.buttons.leftShoulder == ButtonState.Released,
        () => oldGP4State.buttons.rightShoulder == ButtonState.Pressed && newGP4State.buttons.rightShoulder == ButtonState.Released,
        () => oldGP4State.buttons.back == ButtonState.Pressed && newGP4State.buttons.back == ButtonState.Released,
        () => oldGP4State.buttons.start == ButtonState.Pressed && newGP4State.buttons.start == ButtonState.Released,
        () => oldGP4State.buttons.leftStick == ButtonState.Pressed && newGP4State.buttons.leftStick == ButtonState.Released,
        () => oldGP4State.buttons.rightStick == ButtonState.Pressed && newGP4State.buttons.rightStick == ButtonState.Released
    };

    private static readonly Func<bool>[] getInputKeyPositiveDelegate = new Func<bool>[]
    {
        () => newGP1State.buttons.a == ButtonState.Pressed || newGP2State.buttons.a == ButtonState.Pressed || newGP3State.buttons.a == ButtonState.Pressed || newGP4State.buttons.a == ButtonState.Pressed,
        () => newGP1State.buttons.b == ButtonState.Pressed || newGP2State.buttons.b == ButtonState.Pressed || newGP3State.buttons.b == ButtonState.Pressed || newGP4State.buttons.b == ButtonState.Pressed,
        () => newGP1State.buttons.x == ButtonState.Pressed || newGP2State.buttons.x == ButtonState.Pressed || newGP3State.buttons.x == ButtonState.Pressed || newGP4State.buttons.x == ButtonState.Pressed,
        () => newGP1State.buttons.y == ButtonState.Pressed || newGP2State.buttons.y == ButtonState.Pressed || newGP3State.buttons.y == ButtonState.Pressed || newGP4State.buttons.y == ButtonState.Pressed,
        () => newGP1State.buttons.leftShoulder == ButtonState.Pressed || newGP2State.buttons.leftShoulder == ButtonState.Pressed || newGP3State.buttons.leftShoulder == ButtonState.Pressed || newGP4State.buttons.leftShoulder == ButtonState.Pressed,
        () => newGP1State.buttons.rightShoulder == ButtonState.Pressed || newGP2State.buttons.rightShoulder == ButtonState.Pressed || newGP3State.buttons.rightShoulder == ButtonState.Pressed || newGP4State.buttons.rightShoulder == ButtonState.Pressed,
        () => newGP1State.buttons.back == ButtonState.Pressed || newGP2State.buttons.back == ButtonState.Pressed || newGP3State.buttons.back == ButtonState.Pressed || newGP4State.buttons.back == ButtonState.Pressed,
        () => newGP1State.buttons.start == ButtonState.Pressed || newGP2State.buttons.start == ButtonState.Pressed || newGP3State.buttons.start == ButtonState.Pressed || newGP4State.buttons.start == ButtonState.Pressed,
        () => newGP1State.buttons.leftStick == ButtonState.Pressed || newGP2State.buttons.leftStick == ButtonState.Pressed || newGP3State.buttons.leftStick == ButtonState.Pressed || newGP4State.buttons.leftStick == ButtonState.Pressed,
        () => newGP1State.buttons.rightStick == ButtonState.Pressed || newGP2State.buttons.rightStick == ButtonState.Pressed || newGP3State.buttons.rightStick == ButtonState.Pressed || newGP4State.buttons.rightStick == ButtonState.Pressed,

        () => newGP1State.buttons.a == ButtonState.Pressed,
        () => newGP1State.buttons.b == ButtonState.Pressed,
        () => newGP1State.buttons.x == ButtonState.Pressed,
        () => newGP1State.buttons.y == ButtonState.Pressed,
        () => newGP1State.buttons.leftShoulder == ButtonState.Pressed,
        () => newGP1State.buttons.rightShoulder == ButtonState.Pressed,
        () => newGP1State.buttons.back == ButtonState.Pressed,
        () => newGP1State.buttons.start == ButtonState.Pressed,
        () => newGP1State.buttons.leftStick == ButtonState.Pressed,
        () => newGP1State.buttons.rightStick == ButtonState.Pressed,

        () => newGP2State.buttons.a == ButtonState.Pressed,
        () => newGP2State.buttons.b == ButtonState.Pressed,
        () => newGP2State.buttons.x == ButtonState.Pressed,
        () => newGP2State.buttons.y == ButtonState.Pressed,
        () => newGP2State.buttons.leftShoulder == ButtonState.Pressed,
        () => newGP2State.buttons.rightShoulder == ButtonState.Pressed,
        () => newGP2State.buttons.back == ButtonState.Pressed,
        () => newGP2State.buttons.start == ButtonState.Pressed,
        () => newGP2State.buttons.leftStick == ButtonState.Pressed,
        () => newGP2State.buttons.rightStick == ButtonState.Pressed,

        () => newGP3State.buttons.a == ButtonState.Pressed,
        () => newGP3State.buttons.b == ButtonState.Pressed,
        () => newGP3State.buttons.x == ButtonState.Pressed,
        () => newGP3State.buttons.y == ButtonState.Pressed,
        () => newGP3State.buttons.leftShoulder == ButtonState.Pressed,
        () => newGP3State.buttons.rightShoulder == ButtonState.Pressed,
        () => newGP3State.buttons.back == ButtonState.Pressed,
        () => newGP3State.buttons.start == ButtonState.Pressed,
        () => newGP3State.buttons.leftStick == ButtonState.Pressed,
        () => newGP3State.buttons.rightStick == ButtonState.Pressed,

        () => newGP4State.buttons.a == ButtonState.Pressed,
        () => newGP4State.buttons.b == ButtonState.Pressed,
        () => newGP4State.buttons.x == ButtonState.Pressed,
        () => newGP4State.buttons.y == ButtonState.Pressed,
        () => newGP4State.buttons.leftShoulder == ButtonState.Pressed,
        () => newGP4State.buttons.rightShoulder == ButtonState.Pressed,
        () => newGP4State.buttons.back == ButtonState.Pressed,
        () => newGP4State.buttons.start == ButtonState.Pressed,
        () => newGP4State.buttons.leftStick == ButtonState.Pressed,
        () => newGP4State.buttons.rightStick == ButtonState.Pressed
    };

    #endregion

    #endregion

    #endregion

    #region Class InputData

    private class InputData : IEnumerable<KeyValuePair<string, InputData.ListInt>>, ICloneable<InputData>
    {
        private Dictionary<string, ListInt> controlsDic;

        private ControllerType _controllerType;
        public ControllerType controllerType
        {
            get => _controllerType;
            set
            {
                if (controllerType == ControllerType.Any)
                {
                    string errorMsg = "An input data is for keyboard or gamepads not both!";
                    Debug.LogError(errorMsg);
                    LogManager.instance.AddLog(errorMsg, "InputManager.InputData.controllerTypeSetter");
                    return;
                }

                _controllerType = value;
                controlsDic = ConvertControlsToSpesificControls(controlsDic);
            }
        }

        public InputData()
        {
            controlsDic = new Dictionary<string, ListInt>();
            _controllerType = ControllerType.Any;
        }

        public InputData(ControllerType controllerType)
        {
            if(controllerType == ControllerType.Any)
            {
                string errorMsg = "An input data is for keyboard or gamepads not both!";
                Debug.LogError(errorMsg);
                LogManager.instance.AddLog(errorMsg, "InputManager.InputData.InputData(ControllerType controllerType)");
            }

            controlsDic = new Dictionary<string, ListInt>();
            _controllerType = controllerType;
        }

        public InputData(ControllerType controllerType, string[] actions, GeneralKey[] keys)
        {
#if UNITY_EDITOR
            if(actions.Length != keys.Length)
            {
                string errorMsg = $"actions and keys must have the same length! actions length{actions.Length}, key length : {keys.Length}";
                Debug.LogWarning(errorMsg);
                LogManager.instance.AddLog(errorMsg, "InputManager.InputData.InputData(ControllerType controllerType, string[] actions, GeneralKey[] keys)", actions, keys);
                return;
            }
#endif

            if (controllerType == ControllerType.Any)
            {
                string errorMsg = "An input data is for keyboard or gamepads not both!";
                Debug.LogError(errorMsg);
                LogManager.instance.AddLog(errorMsg, "InputManager.InputData.InputData(ControllerType controllerType)");
            }

            _controllerType = controllerType;
            controlsDic = new Dictionary<string, ListInt>();
            for (int i = 0; i < actions.Length; i++)
            {
                if (VerifyKey(keys[i], out int newKey))
                {
                    controlsDic.Add(actions[i], new ListInt(newKey));
                }
            }
        }

        public InputData(ControllerType controllerType, in InputDataRaw inputDataRaw)
        {
            if (controllerType == ControllerType.Any)
            {
                string errorMsg = "An input data is for keyboard or gamepads not both!";
                Debug.LogError(errorMsg);
                LogManager.instance.AddLog(errorMsg, "InputManager.InputData.InputData(ControllerType controllerType, in InputDataRaw inputDataRaw)");
            }

            _controllerType = controllerType;
            Dictionary<string, ListInt> controlsDic = new Dictionary<string, ListInt>();
            for (int i = 0; i < inputDataRaw.actions.Length; i++)
            {
                controlsDic.Add(inputDataRaw.actions[i], inputDataRaw.keys[i]);
            }
            this.controlsDic = ConvertControlsToSpesificControls(controlsDic);
        }

        private InputData(ControllerType controllerType, Dictionary<string, ListInt> controlsDic)
        {
            this._controllerType = controllerType;
            this.controlsDic = new Dictionary<string, ListInt>(controlsDic);
        }

        private Dictionary<string, ListInt> ConvertControlsToSpesificControls(Dictionary<string, ListInt> controls)
        {
            Dictionary<string, ListInt> newControlsDic = new Dictionary<string, ListInt>(controls.Count);
            foreach (KeyValuePair<string, ListInt> inputs in controls)
            {
                ListInt keys = new ListInt();
                keys.capacity = inputs.Value.Count;
                foreach (int currentKey in inputs.Value)
                {
                    GeneralKey generalKey = ConvertInputKeyToGeneralInputKey((InputKey)currentKey);
                    if (controllerType == ControllerType.Keyboard)
                    {
                        if (IsKeyboardKey((InputKey)generalKey))
                        {
                            keys.Add((int)generalKey);
                        }
                    }
                    else
                    {
                        if (IsGamepadKey((InputKey)generalKey))
                        {
                            GamepadKey gpKey = ConvertGeneralGamepadKeyToGamepadKey((GeneralGamepadKey)generalKey, controllerType);
                            keys.Add((int)gpKey);
                        }
                    }
                }
                newControlsDic.Add(inputs.Key, keys);
            }
            return newControlsDic;
        }

        public InputDataRaw GetRawData()
        {
            string[] actions = new string[controlsDic.Count];
            ListInt[] keys = new ListInt[controlsDic.Count];

            int i = 0;
            foreach (KeyValuePair<string, ListInt> control in controlsDic)
            {
                actions[i] = control.Key;
                keys[i] = control.Value.Clone();
                for (int j = 0; j < keys[i].Count; j++)
                {
                    keys[i][j] = (int)ConvertInputKeyToGeneralInputKey((InputKey)keys[i][j]);
                }
                i++;
            }

            return new InputDataRaw(actions, keys);
        }

        public void Clear()
        {
            controlsDic.Clear();
        }

        public bool IsEmpty() => controlsDic.Count <= 0;

        private bool VerifyKey(GeneralKey key, out int newKey)
        {
            if (controllerType == ControllerType.Keyboard)
            {
                if (IsKeyboardKey((InputKey)key))
                {
                    newKey = (int)key;
                    return true;
                }
            }
            else if (controllerType == ControllerType.GamepadAny)
            {
                if (IsGamepadKey((InputKey)key))
                {
                    newKey = (int)key;
                    return true;
                }
            }
            else if (controllerType == ControllerType.Any)
            {
                newKey = (int)key;
                return true;
            }
            else
            {
                if (IsGamepadKey((InputKey)key))
                {
                    newKey = (int)ConvertGeneralGamepadKeyToGamepadKey((GeneralGamepadKey)key, controllerType);
                    return true;
                }
            }
            newKey = 0;
            return false;
        }

        public void AddAction(string action, GeneralKey key)
        {
            if(VerifyKey(key, out int newKey))
            {
                if (controlsDic.ContainsKey(action))
                    controlsDic[action].Add(newKey);
                else
                    controlsDic.Add(action, new ListInt(newKey));
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

        public bool ReplaceAction(string action, GeneralKey key)
        {
            if (controlsDic.ContainsKey(action))
            {
                controlsDic.Remove(action);
                AddAction(action, key);
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

        public InputData Clone()
        {
            Dictionary<string, ListInt> cloneDict = new Dictionary<string, ListInt>();
            foreach (KeyValuePair<string, ListInt> inputs in controlsDic)
            {
                cloneDict.Add(inputs.Key, inputs.Value.Clone());
            }
            return new InputData(controllerType, cloneDict);
        }

        public IEnumerator<KeyValuePair<string, ListInt>> GetEnumerator()
        {
            return controlsDic.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return controlsDic.GetEnumerator();
        }

        [Serializable]
        public class ListInt : IEnumerable<int>
        {
            [SerializeField] private List<int> keys;
            public int Count => keys.Count;
            public int capacity
            {
                get => keys.Capacity;
                set => keys.Capacity = value;
            }

            public ListInt()
            {
                keys = new List<int>();
            }

            public ListInt(List<int> keys)
            {
                this.keys = keys;
            }

            public ListInt(int key)
            {
                keys = new List<int> { key };
            }

            public int this[int i]
            {
                get => keys[i];
                set => keys[i] = value;
            } 

            public void Add(int key) => keys.Add(key);

            public ListInt Clone() => new ListInt(new List<int>(keys));

            public IEnumerator<int> GetEnumerator() => keys.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => keys.GetEnumerator();

            public InputKey[] ToArray()
            {
                InputKey[] res = new InputKey[keys.Count];
                for (int i = 0; i < res.Length; i++)
                {
                    res[i] = (InputKey)keys[i];
                }
                return res;
            }
        }

        [Serializable]
        public struct InputDataRaw
        {
            public string[] actions;
            public ListInt[] keys;

            public InputDataRaw(string[] actions, ListInt[] keys)
            {
                this.keys = keys;
                this.actions = actions;
            }
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

        player1Keys = new InputData();
        player2Keys = new InputData();
        player3Keys = new InputData();
        player4Keys = new InputData();
        player5Keys = new InputData();
        defaultKBKeys = new InputData();
        defaultGPKeys = new InputData();
        kbKeys = new InputData();
        gpKeys = new InputData();
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
                    return system;
            }
        }

        return default(PlayerLoopSystem);
    }

    #endregion

    #region Key Convertion

    private static Dictionary<ControllerType, int> offsetNegKey = new Dictionary<ControllerType, int>
    {
        { ControllerType.Gamepad1, 56 }, { ControllerType.Gamepad2, 42 }, { ControllerType.Gamepad3, 28 }, { ControllerType.Gamepad4, 14 }
    };

    public static GeneralGamepadKey ConvertGamepadKeyToGeneralGamepadKey(GamepadKey key)
    {
        int k = (int)key;
        if (k < 0)
            return (GeneralGamepadKey)(k - ((k + 70) / 14) * 14);
        if(k >= 340)
            return (GeneralGamepadKey)(k - ((k - 330) / 10) * 10);
        return (GeneralGamepadKey)k;
    }

    public static GamepadKey ConvertGeneralGamepadKeyToGamepadKey(GeneralGamepadKey key, ControllerType gamepadIndex)
    {
        if(gamepadIndex == ControllerType.GamepadAny)
            return (GamepadKey)key;

        int k = (int)key;
        if (k < 0)
            return (GamepadKey)(k + offsetNegKey[gamepadIndex]);
        return (GamepadKey)(k + (10 * ((int)gamepadIndex)));
    }

    private static GeneralKey ConvertInputKeyToGeneralInputKey(InputKey key)
    {
        if (IsKeyboardKey(key))
            return (GeneralKey)key;
        return (GeneralKey)ConvertGamepadKeyToGeneralGamepadKey((GamepadKey)key);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsGamepadKey(InputKey key)
    {
        int key2 = (int)key;
        return key2 <= 0 || key2 >= 330;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsKeyboardKey(InputKey key) => !IsGamepadKey(key) || key == InputKey.None;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsGeneralGamepadKey(GamepadKey key)
    {
        int k = (int)key;
        return k <= -57 || (k >= 0 && k <= 339);
    }

    #endregion

    #region GamePad Only

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetCurrentGamepadName() => Gamepad.current.displayName;

    private static GamePadState GetState(ControllerType gamepadIndex)
    {
        switch (gamepadIndex)
        {
            case ControllerType.Gamepad1:
                return newGP1State;
            case ControllerType.Gamepad2:
                return newGP2State;
            case ControllerType.Gamepad3:
                return newGP3State;
            case ControllerType.Gamepad4:
                return newGP4State;
            default:
                return default(GamePadState);
        }
    }

    private static InputData GetInputData(PlayerIndex player)
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
                Debug.LogWarning("Cannot get the input data of multiple player controller!");
                return null;
        }
    }

    private static GamePadState GetState(PlayerIndex playerIndex) => GetState(GetInputData(playerIndex).controllerType);

    private static ControllerType GetControllerType(PlayerIndex playerIndex)
    {
        switch (playerIndex)
        {
            case PlayerIndex.One:
                return player1Keys.controllerType;
            case PlayerIndex.Two:
                return player2Keys.controllerType;
            case PlayerIndex.Three:
                return player3Keys.controllerType;
            case PlayerIndex.Four:
                return player4Keys.controllerType;
            case PlayerIndex.Five:
                return player5Keys.controllerType;
            default:
                return ControllerType.Any;
        }
    }

    #region SetVibration

    private static void SetVibrationInternal(float lowFrequency, float hightFrequency, ControllerType gamepadIndex)
    {
        lowFrequency = Mathf.Clamp01(lowFrequency);
        hightFrequency = Mathf.Clamp01(hightFrequency);
        if (gamepadIndex == ControllerType.Any || gamepadIndex == ControllerType.GamepadAny)
        {
            foreach (Gamepad gamepad in Gamepad.all)
            {
                gamepad.SetMotorSpeeds(lowFrequency, hightFrequency);
            }
        }
        else
        {
            GamePadState gamePadState = GetState(gamepadIndex);
            foreach (Gamepad gamepad in Gamepad.all)
            {
                if(gamePadState.deviceId == gamepad.deviceId)
                {
                    gamepad.SetMotorSpeeds(lowFrequency, hightFrequency);
                    break;
                }
            }
        }
    }

    public static void SetVibration(float intensity, ControllerType gamepadIndex = ControllerType.GamepadAny)
    {
        SetVibration(intensity, intensity, gamepadIndex);
    }

    public static void SetVibration(float lowFrequency = 1f, float highFrequency = 1f, ControllerType gamepadIndex = ControllerType.GamepadAny)
    {
        if (gamepadIndex == ControllerType.Keyboard)
        {
            Debug.LogWarning("Cannot vibrate the keyboard!");
            return;
        }
        SetVibrationInternal(lowFrequency, highFrequency, gamepadIndex);
    }

    public static void SetVibration(float lowFrequency, float highFrequency, float duration, ControllerType gamepadIndex = ControllerType.GamepadAny)
    {
        SetVibration(lowFrequency, highFrequency, duration, 0f, gamepadIndex);
    }

    public static void SetVibration(float lowFrequency, float highFrequency, float duration, float delay, ControllerType gamepadIndex = ControllerType.GamepadAny)
    {
        if (gamepadIndex == ControllerType.Keyboard)
        {
            Debug.LogWarning("Cannot vibrate the keyboard!");
            return;
        }

        VibrationSettings setting = new VibrationSettings(gamepadIndex, duration, lowFrequency, highFrequency);
        setting.timer = -delay;
        vibrationSettings.Add(setting);
    }

    public static void SetVibration(float intensity, PlayerIndex playerIndex) => SetVibration(intensity, intensity, GetControllerType(playerIndex));
    public static void SetVibration(float lowFrequency, float highFrequency, PlayerIndex playerIndex) => SetVibration(lowFrequency, highFrequency, GetControllerType(playerIndex));
    public static void SetVibration(float lowFrequency, float highFrequency, float duration, PlayerIndex playerIndex) => SetVibration(lowFrequency, highFrequency, duration, GetControllerType(playerIndex));
    public static void SetVibration(float lowFrequency, float highFrequency, float duration, float delay, PlayerIndex playerIndex) => SetVibration(lowFrequency, highFrequency, duration, delay, GetControllerType(playerIndex));

    public static void StopVibration(ControllerType gamepadIndex)
    {
        if (gamepadIndex == ControllerType.Keyboard)
        {
            Debug.LogWarning("Cannot vibrate the keyboard!");
            return;
        }
        SetVibrationInternal(0f, 0f, gamepadIndex);
    }

    public static void CancelVibration()
    {
        StopVibration(ControllerType.GamepadAny);
        vibrationSettings.Clear();
    }

    #endregion

    #region SetStickPosition

    private static void SetNewGamepadSticksAndTriggersPositions()
    {
        //ThumbStick : si vraiPos.x/y€[-deadZone.x/y, deadZone.x/y] => pos.x/y = 0, vraiPos.x/y€[-1, -1 + deadZone.x/y] U [1 - deadZone.x/y, 1] => pos.x/y = (vraiPos.x/y).Sign() * 1, sinon pos.x/y = vraiPos.x/y
        void CalculateGamepadStickAndTrigger(ref GamePadState newState, in Vector2 lDeadZone, in Vector2 rDeadZone, in Vector2 triggerDeadZone)
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

            float norm = newState.thumbSticks.right.x * newState.thumbSticks.right.x + newState.thumbSticks.right.y * newState.thumbSticks.right.y;
            float trueX = newState.thumbSticks.right.x;
            float trueY = newState.thumbSticks.right.y;
            if (norm >= 1f)
            {
                norm = Mathf.Sqrt(norm);
                trueX /= norm;
                trueY /= norm;
            }

            float x = Mathf.Abs(trueX) <= rDeadZone.x ? 0f : (Mathf.Abs(trueX) >= (1f - rDeadZone.x) ? trueX.Sign() : trueX);
            float y = Mathf.Abs(trueY) <= rDeadZone.x ? 0f : (Mathf.Abs(trueY) >= (1f - rDeadZone.y) ? trueY.Sign() : trueY);
            newState.thumbSticks.right = new Vector2(Regression(rDeadZone.x, 1f - rDeadZone.x, x), Regression(rDeadZone.y, 1f - rDeadZone.y, y));

            norm = newState.thumbSticks.left.x * newState.thumbSticks.left.x + newState.thumbSticks.left.y * newState.thumbSticks.left.y;
            trueX = newState.thumbSticks.left.x;
            trueY = newState.thumbSticks.left.y;
            if (norm >= 1f)
            {
                norm = Mathf.Sqrt(norm);
                trueX /= norm;
                trueY /= norm;
            }

            x = Mathf.Abs(trueX) <= rDeadZone.x ? 0f : (Mathf.Abs(trueX) >= (1f - rDeadZone.x) ? trueX.Sign() : trueX);
            y = Mathf.Abs(trueY) <= rDeadZone.x ? 0f : (Mathf.Abs(trueY) >= (1f - rDeadZone.y) ? trueY.Sign() : trueY);
            newState.thumbSticks.left = new Vector2(Regression(rDeadZone.x, 1f - rDeadZone.x, x), Regression(rDeadZone.y, 1f - rDeadZone.y, y));

            trueX = Mathf.Clamp01(newState.triggers.left);
            trueY = Mathf.Clamp01(newState.triggers.right);
            x = trueX <= triggerDeadZone.x ? 0f : (trueX >= 1f - triggerDeadZone.x ? 1f : trueX);
            y = trueY <= triggerDeadZone.y ? 0f : (trueY >= 1f - triggerDeadZone.y ? 1f : trueY);
            newState.triggers.left = Regression(triggerDeadZone.x, 1f - triggerDeadZone.x, x);
            newState.triggers.right = Regression(triggerDeadZone.y, 1f - triggerDeadZone.y, y);
        }

        CalculateGamepadStickAndTrigger(ref newGP1State, GP1LeftThumbStickDeadZone, GP1RightThumbStickDeadZone, GP1TriggersDeadZone);
        CalculateGamepadStickAndTrigger(ref newGP2State, GP2LeftThumbStickDeadZone, GP2RightThumbStickDeadZone, GP2TriggersDeadZone);
        CalculateGamepadStickAndTrigger(ref newGP3State, GP3LeftThumbStickDeadZone, GP3RightThumbStickDeadZone, GP3TriggersDeadZone);
        CalculateGamepadStickAndTrigger(ref newGP4State, GP4LeftThumbStickDeadZone, GP4RightThumbStickDeadZone, GP4TriggersDeadZone);
    }

    #endregion

    #region Gamepad Stick/Trigger/IsPlug/Unplug

    public static bool IsAGamepadController(ControllerType controllerType)
    {
        return controllerType == ControllerType.Gamepad1 || controllerType == ControllerType.Gamepad2 || controllerType == ControllerType.Gamepad3 || controllerType == ControllerType.Gamepad4;
    }

    public static Vector2 GetGamepadStickPosition(ControllerType gamepadIndex, GamepadStick gamepadStick)
    {
        if(gamepadIndex == ControllerType.Keyboard)
        {
            string errorMsg = "Can't return the stick position of a keyboard!";
            Debug.LogWarning(errorMsg);
            LogManager.instance.AddLog(errorMsg, "InputManager.GetGamepadStickPosition(ControllerType gamepadIndex, GamepadStick GamepadStick)");
            return Vector2.zero;
        }

        if (gamepadIndex == ControllerType.Any || gamepadIndex == ControllerType.GamepadAny)
        {
            string errorMsg = "Can't return the stick position of multiple device";
            Debug.LogWarning(errorMsg);
            LogManager.instance.AddLog(errorMsg, "InputManager.GetGamepadStickPosition(ControllerType gamepadIndex, GamepadStick GamepadStick)");
            return Vector2.zero;
        }

        GamePadState gamePadState = GetState(gamepadIndex);
        return gamepadStick == GamepadStick.right ? gamePadState.thumbSticks.right : gamePadState.thumbSticks.left;
    }

    public static float GetGamepadTrigger(ControllerType gamepadIndex, GamepadTrigger gamepadTrigger)
    {
        if (gamepadIndex == ControllerType.Keyboard)
        {
            string errorMsg = "Can't return the trigger of a keyboard!";
            Debug.LogWarning(errorMsg);
            LogManager.instance.AddLog(errorMsg, "InputManager.GetGamepadTrigger(ControllerType gamepadIndex, GamepadTrigger gamepadTrigger)");
            return 0f;
        }

        if (gamepadIndex == ControllerType.Any || gamepadIndex == ControllerType.GamepadAny)
        {
            string errorMsg = "Can't return the trigger of multiple device";
            Debug.LogWarning(errorMsg);
            LogManager.instance.AddLog(errorMsg, "InputManager.GetGamepadTrigger(ControllerType gamepadIndex, GamepadTrigger gamepadTrigger)");
            return 0f;
        }

        GamePadState gamePadState = GetState(gamepadIndex);
        return gamepadTrigger == GamepadTrigger.right ? gamePadState.triggers.right : gamePadState.triggers.left;
    }

    /// <returns>true if the gamepad define by the gamepadIndex is connected, false otherwise </returns>
    public static bool IsGamePadConnected(ControllerType gamepadIndex)
    {
        if (gamepadIndex == ControllerType.Keyboard)
            return true;

        if (gamepadIndex == ControllerType.Any || gamepadIndex == ControllerType.GamepadAny)
            return newGP1State.isConnected && newGP2State.isConnected && newGP3State.isConnected && newGP4State.isConnected;

        return GetState(gamepadIndex).isConnected;
    }

    /// <returns>true if a gamepad is pluged at the current frame and return the gamepadIndex of the plugged gamepad, false otherwise </returns>
    public static bool GetGamepadPlugged(out ControllerType gamepadIndex)
    {
        if(newGP1State.isConnected && !oldGP1State.isConnected)
        {
            gamepadIndex = ControllerType.Gamepad1;
            return true;
        }
        if (newGP2State.isConnected && !oldGP2State.isConnected)
        {
            gamepadIndex = ControllerType.Gamepad2;
            return true;
        }
        if (newGP3State.isConnected && !oldGP3State.isConnected)
        {
            gamepadIndex = ControllerType.Gamepad3;
            return true;
        }
        if (newGP4State.isConnected && !oldGP4State.isConnected)
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
        if (newGP1State.isConnected && !oldGP1State.isConnected)
        {
            if (res == null)
                res = new List<ControllerType>();
            res.Add(ControllerType.Gamepad1);
            b = true;
        }
        if (newGP2State.isConnected && !oldGP2State.isConnected)
        {
            if (res == null)
                res = new List<ControllerType>();
            res.Add(ControllerType.Gamepad2);
            b = true; ;
        }
        if (newGP3State.isConnected && !oldGP3State.isConnected)
        {
            if (res == null)
                res = new List<ControllerType>();
            res.Add(ControllerType.Gamepad3);
            b = true;
        }
        if (newGP4State.isConnected && !oldGP4State.isConnected)
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
        if (!newGP1State.isConnected && oldGP1State.isConnected)
        {
            gamepadIndex = ControllerType.Gamepad1;
            return true;
        }
        if (!newGP2State.isConnected && oldGP2State.isConnected)
        {
            gamepadIndex = ControllerType.Gamepad2;
            return true;
        }
        if (!newGP3State.isConnected && oldGP3State.isConnected)
        {
            gamepadIndex = ControllerType.Gamepad3;
            return true;
        }
        if (!newGP4State.isConnected && oldGP4State.isConnected)
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
        if (!newGP1State.isConnected && oldGP1State.isConnected)
        {
            if (res == null)
                res = new List<ControllerType>();
            res.Add(ControllerType.Gamepad1);
            b = true;
        }
        if (!newGP2State.isConnected && oldGP2State.isConnected)
        {
            if (res == null)
                res = new List<ControllerType>();
            res.Add(ControllerType.Gamepad2);
            b = true; ;
        }
        if (!newGP3State.isConnected && oldGP3State.isConnected)
        {
            if (res == null)
                res = new List<ControllerType>();
            res.Add(ControllerType.Gamepad3);
            b = true;
        }
        if (!newGP4State.isConnected && oldGP4State.isConnected)
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

    #endregion

    #region Keyboard Only

    public static Vector2 mousePosition => Input.mousePosition;
    public static float mouseScrollDelta => Input.mouseScrollDelta.y;
    public static bool isAMouseConnected => Input.mousePresent;

    /// <param name="direction"> the direction of the mousewheel return by the function </param>
    /// <returns> true during the frame where the mouse wheel is moved.</returns>
    public static bool MouseWheel(out MouseWheelDirection direction)
    {
        if (Input.mouseScrollDelta != Vector2.zero)
        {
            direction = Input.mouseScrollDelta.y > 0f ? MouseWheelDirection.Up : MouseWheelDirection.Down;
            return true;
        }
        direction = MouseWheelDirection.none;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetMouseCursor(Texture2D texture, in Vector2 hotpost, CursorMode cursorMode)
    {
        Cursor.SetCursor(texture, hotpost, cursorMode);
    }

    public static void HideMouseCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public static void ShowMouseCursor()
    {
        Cursor.visible = true;
#if UNITY_EDITOR
        Cursor.lockState = CursorLockMode.None;
#else
        Cursor.lockState = CursorLockMode.Confined;
#endif
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsMouseKey(InputKey key)
    {
        int keyInt = (int)key;
        return keyInt >= 323 && keyInt <= 329;
    }

#endregion

    #region GetInputKey

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool GetNegativeKeyDown(int key)
    {
        return getInputKeyDownDelegate[-key].Invoke();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool GetNegativeKeyUp(int key)
    {
        return getInputKeyUpDelegate[-key].Invoke();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool GetNegativeKeyPressed(int key)
    {
        return getInputKeyPressedDelegate[-key].Invoke();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool GetPositiveKeyDown(int key)
    {
        return getInputKeyDownPositiveDelegate[key - 330].Invoke();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool GetPositiveKeyUp(int key)
    {
        return getInputKeyUpPositiveDelegate[key - 330].Invoke();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool GetPositiveKeyPressed(int key)
    {
        return getInputKeyPositiveDelegate[key - 330].Invoke();
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
    public static bool GetKeyDown(string[] actions, PlayerIndex player)
    {
        foreach (string action in actions)
        {
            if(GetKeyDown(action, player))
                return true;
        }
        return false;
    }

    /// <returns> true during the frame when a key assigned with one of the actions is pressed</returns>
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
                return true;
        }
        return false;
    }

    /// <returns> true during the frame when a key assigned with one of the actions is unpressed</returns>
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
                return true;
        }
        return false;
    }

    /// <returns> true when a key assigned with one of the actions is pressed</returns>
    public static bool GetKey(string[] actions, BaseController controller)
    {
        foreach (string action in actions)
        {
            if (GetKey(action, controller))
                return true;
        }
        return false;
    }

    private static bool GetKeyDown(int key)
    {
        if (key < 0)
            return GetNegativeKeyDown(key);
        if (key <= 329)
            return Input.GetKeyDown((KeyCode)key);
        return GetPositiveKeyDown(key);
    }

    private static bool GetKeyUp(int key)
    {
        if (key < 0)
            return GetNegativeKeyUp(key);
        if (key < 329)
            return Input.GetKeyUp((KeyCode)key);
        return GetPositiveKeyUp(key);
    }

    private static bool GetKey(int key)
    {
        if (key < 0)
            return GetNegativeKeyPressed(key);
        if (key <= 329)
            return Input.GetKey((KeyCode)key);
        return GetPositiveKeyPressed(key);
    }

    /// <returns> true during the frame when the key is pressed</returns>
    public static bool GetKeyDown(KeyCode key) => GetKeyDown((int)key);
    /// <returns> true during the frame when key is unpressed</returns>
    public static bool GetKeyUp(KeyCode key) => GetKeyUp((int)key);
    /// <returns> true when the key is pressed</returns>
    public static bool GetKey(KeyCode key) => GetKey((int)key);
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

    private static bool GetKeysDown(InputData.ListInt keys)
    {
        foreach (int key in keys)
        {
            if (GetKeyDown(key))
                return true;
        }
        return false;
    }

    private static bool GetKeysUp(InputData.ListInt keys)
    {
        foreach (int key in keys)
        {
            if (GetKeyUp(key))
                return true;
        }
        return false;
    }

    private static bool GetKeys(InputData.ListInt keys)
    {
        foreach (int key in keys)
        {
            if (GetKey(key))
                return true;
        }
        return false;
    }

    #endregion

    #region Management Controller

    #region Add/Replace/Remove action

    public static void AddInputAction(string action, KeyboardKey key, bool defaultConfig = false)
    {
        if(defaultConfig)
            defaultKBKeys.AddAction(action, (GeneralKey)key);
        else
            kbKeys.AddAction(action, (GeneralKey)key);
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
            string errorMsg = $"actions and keys must have the same length! actions.length : {actions.Length}, keys.Length : {keys.Length}";
            Debug.LogWarning(errorMsg);
            LogManager.instance.AddLog(errorMsg, "InputManager.AddInputsActions(string[] actions, KeyboardKey[] keys, bool defaultConfig = false)", actions, keys, defaultConfig);
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
            defaultGPKeys.AddAction(action, (GeneralKey)key);
        else
            gpKeys.AddAction(action, (GeneralKey)key);
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
            string errorMsg = $"actions and keys must have the same length! actions.length : {actions.Length}, keys.Length : {keys.Length}";
            Debug.LogWarning(errorMsg);
            LogManager.instance.AddLog(errorMsg, "InputManager.AddInputsActions(string[] actions, GamepadKey[] keys, bool defaultConfig = false)", actions, keys, defaultConfig);
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
            defaultGPKeys.AddAction(action, (GeneralKey)key);
        else
            gpKeys.AddAction(action, (GeneralKey)key);
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
            string errorMsg = $"actions and keys must have the same length! actions.length : {actions.Length}, keys.Length : {keys.Length}";
            Debug.LogWarning(errorMsg);
            LogManager.instance.AddLog(errorMsg, "InputManager.AddInputsActions(string[] actions, GeneralGamepadKey[] keys, bool defaultConfig = false)", actions, keys, defaultConfig);
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
                kb.AddAction(action, (GeneralKey)key);
            else
                Debug.LogWarning("Can't add " + KeyToString(key) + " to a keyboard controller because it's not a keyboard key!");
            return;
        }
        if (controller == BaseController.Gamepad)
        {
            if (IsGamepadKey(key))
                gp.AddAction(action, (GeneralKey)ConvertGamepadKeyToGeneralGamepadKey((GamepadKey)key));
            else
                Debug.LogWarning("Can't add " + KeyToString(key) + " to a gamepad controller because it's not a gamepad key!");
            return;
        }
        if (IsKeyboardKey(key))
            kb.AddAction(action, (GeneralKey)key);
        else
            Debug.LogWarning("Can't add " + KeyToString(key) + " to a keyboard controller because it's not a keyboard key!");
        if (IsGamepadKey(key))
            gp.AddAction(action, (GeneralKey)ConvertGamepadKeyToGeneralGamepadKey((GamepadKey)key));
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
            string errorMsg = $"actions and keys must have the same length! actions.length : {actions.Length}, keys.Length : {keys.Length}";
            Debug.LogWarning(errorMsg);
            LogManager.instance.AddLog(errorMsg, "InputManager.AddInputsActions(string[] actions,  BaseController baseController, bool defaultConfig = false)", actions, keys, defaultConfig);
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
                player1Keys.AddAction(action, (GeneralKey)key);
                break;
            case PlayerIndex.Two:
                player2Keys.AddAction(action, (GeneralKey)key);
                break;
            case PlayerIndex.Three:
                player3Keys.AddAction(action, (GeneralKey)key);
                break;
            case PlayerIndex.Four:
                player4Keys.AddAction(action, (GeneralKey)key);
                break;
            case PlayerIndex.Five:
                player5Keys.AddAction(action, (GeneralKey)key);
                break;
            case PlayerIndex.All:
                player1Keys.AddAction(action, (GeneralKey)key);
                player2Keys.AddAction(action, (GeneralKey)key);
                player3Keys.AddAction(action, (GeneralKey)key);
                player4Keys.AddAction(action, (GeneralKey)key);
                player5Keys.AddAction(action, (GeneralKey)key);
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
            string errorMsg = $"actions and keys must have the same length! actions.length : {actions.Length}, keys.Length : {keys.Length}";
            Debug.LogWarning(errorMsg);
            LogManager.instance.AddLog(errorMsg, "InputManager.AddInputsActions(string[] actions, InputKey[] keys, bool defaultConfig = false)", actions, keys, player);
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
            string errorMsg = $"actions and keys must have the same length! actions.length : {actions.Length}, keys.Length : {keys.Length}";
            Debug.LogWarning(errorMsg);
            LogManager.instance.AddLog(errorMsg, "InputManager.AddInputsActions(string[] actions, KeyboardKey[] keys, bool defaultConfig = false)", actions, keys, player);
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
            string errorMsg = $"actions and keys must have the same length! actions.length : {actions.Length}, keys.Length : {keys.Length}";
            Debug.LogWarning(errorMsg);
            LogManager.instance.AddLog(errorMsg, "InputManager.AddInputsActions(string[] actions, GamepadKey[] keys, bool defaultConfig = false)", actions, keys, player);
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
            string errorMsg = $"actions and keys must have the same length! actions.length : {actions.Length}, keys.Length : {keys.Length}";
            Debug.LogWarning(errorMsg);
            LogManager.instance.AddLog(errorMsg, "InputManager.AddInputsActions(string[] actions, GeneralGamepadKey[] keys, bool defaultConfig = false)", actions, keys, player);
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
        GeneralKey key = ConvertInputKeyToGeneralInputKey(newKey);
        switch (player)
        {
            case PlayerIndex.One:
                return player1Keys.ReplaceAction(action, key);
            case PlayerIndex.Two:
                return player2Keys.ReplaceAction(action, key);
            case PlayerIndex.Three:
                return player3Keys.ReplaceAction(action, key);
            case PlayerIndex.Four:
                return player4Keys.ReplaceAction(action, key);
            case PlayerIndex.Five:
                return player5Keys.ReplaceAction(action, key);
            case PlayerIndex.All:
                bool b1 = player1Keys.ReplaceAction(action, key);
                b1 = b1 & player2Keys.ReplaceAction(action, key);
                b1 = b1 & player3Keys.ReplaceAction(action, key);
                b1 = b1 & player4Keys.ReplaceAction(action, key);
                b1 = b1 & player5Keys.ReplaceAction(action, key);
                return b1;
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
        GeneralKey key = ConvertInputKeyToGeneralInputKey(newKey);

        if (controller == BaseController.Keyboard)
        {
            if (IsKeyboardKey(newKey))
                return kb.ReplaceAction(action, key);
            return false;
        }
        if (controller == BaseController.Gamepad)
        {
            if (IsGamepadKey((InputKey)key))
                return gp.ReplaceAction(action, (GeneralKey)ConvertGamepadKeyToGeneralGamepadKey((GamepadKey)key));
            return false;
        }

        bool b = false;
        if (IsKeyboardKey(newKey))
            b = kb.ReplaceAction(action, key);
        if (IsGamepadKey(newKey))
            b = gp.ReplaceAction(action, (GeneralKey)ConvertGamepadKeyToGeneralGamepadKey((GamepadKey)key)) && b;
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
                bool b = player1Keys.RemoveAction(action);
                b = b & player2Keys.RemoveAction(action);
                b = b & player3Keys.RemoveAction(action);
                b = b & player4Keys.RemoveAction(action);
                b = b & player5Keys.RemoveAction(action);
                return b;
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

    #endregion

    #region Clear

    public static void ClearAll()
    {
        player1Keys = new InputData();
        player2Keys = new InputData();
        player3Keys = new InputData();
        player4Keys = new InputData();
        player5Keys = new InputData();
        kbKeys = new InputData();
        gpKeys = new InputData();
        defaultKBKeys = new InputData();
        defaultGPKeys = new InputData();

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
        player5Keys.Clear();
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
                Debug.Log("Can't modify the deadzone of a keyboard!");
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
            case ControllerType.GamepadAny:
                ClearDeadZone();
                break;
            case ControllerType.Any:
                ClearDeadZone();
                break;
            default:
                break;
        }
    }

    #endregion

    #region SetController

    /// <summary>
    /// Set the default Control as the current configuration of a player
    /// </summary>
    /// <param name="controllerType"></param>
    /// <param name="gamepadIndex"></param>
    public static void SetDefaultController(BaseController controller)
    {
        if(controller == BaseController.Keyboard)
        {
            defaultKBKeys = kbKeys.Clone();
            defaultKBKeys.controllerType = ControllerType.Keyboard;
            return;
        }
        if(controller == BaseController.Gamepad)
        {
            defaultGPKeys = gpKeys.Clone();
            defaultGPKeys.controllerType = ControllerType.GamepadAny;
            return;
        }
        SetDefaultController();
    }

    public static void SetDefaultController()
    {
        defaultKBKeys = kbKeys.Clone();
        defaultKBKeys.controllerType = ControllerType.Keyboard;
        defaultGPKeys = gpKeys.Clone();
        defaultGPKeys.controllerType = ControllerType.GamepadAny;
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
            defaultKBKeys = inputs.Clone();
            defaultKBKeys.controllerType = ControllerType.Keyboard;
            return;
        }
        if (controller == BaseController.Keyboard)
        {
            defaultGPKeys = inputs.Clone();
            defaultGPKeys.controllerType = ControllerType.GamepadAny;
            return;
        }
        defaultKBKeys = inputs.Clone();
        defaultKBKeys.controllerType = ControllerType.Keyboard;
        defaultGPKeys = inputs.Clone();
        defaultGPKeys.controllerType = ControllerType.GamepadAny;
    }

    /// <summary>
    /// Set the current Configuration of a player.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="controller"></param>
    public static void SetCurrentController(PlayerIndex player, ControllerType controllerType, bool defaultConfig = false)
    {
        if(player == PlayerIndex.All)
        {
            string errorMsg = "Can't assign multiple player's inputs";
            Debug.LogWarning(errorMsg);
            string methodSignature = "InputManger.SetCurrentController(PlayerIndex player, ControllerType controllerType, bool defaultConfig = false)";
            LogManager.instance.AddLog(errorMsg, methodSignature, player, controllerType, defaultConfig);
            return;
        }

        if (controllerType == ControllerType.Any)
        {
            string errorMsg = "Can't assign from multiples inputs";
            Debug.LogWarning(errorMsg);
            string methodSignature = "InputManger.SetCurrentController(PlayerIndex player, ControllerType controllerType, bool defaultConfig = false)";
            LogManager.instance.AddLog(errorMsg, methodSignature, player, controllerType, defaultConfig);
            return;
        }

        InputData inputs = null;
        if(controllerType == ControllerType.Keyboard)
            inputs = defaultConfig ? defaultKBKeys : kbKeys;
        else
            inputs = defaultConfig ? defaultGPKeys : gpKeys;

        switch (player)
        {
            case PlayerIndex.One:
                player1Keys = inputs.Clone();
                player1Keys.controllerType = controllerType;
                break;
            case PlayerIndex.Two:
                player2Keys = inputs.Clone();
                player2Keys.controllerType = controllerType;
                break;
            case PlayerIndex.Three:
                player3Keys = inputs.Clone();
                player3Keys.controllerType = controllerType;
                break;
            case PlayerIndex.Four:
                player4Keys = inputs.Clone();
                player4Keys.controllerType = controllerType;
                break;
            case PlayerIndex.Five:
                player5Keys = inputs.Clone();
                player5Keys.controllerType = controllerType;
                break;
            default:
                break;
        }
    }

    public static void SetCurrentController(BaseController controller)
    {
        if(controller == BaseController.Keyboard)
        {
            kbKeys = defaultKBKeys.Clone();
            kbKeys.controllerType = ControllerType.Keyboard;
            return;
        }
        if(controller == BaseController.Gamepad)
        {
            gpKeys = defaultGPKeys.Clone();
            gpKeys.controllerType = ControllerType.GamepadAny;
            return;
        }
        kbKeys = defaultKBKeys.Clone();
        kbKeys.controllerType = ControllerType.Keyboard;
        gpKeys = defaultGPKeys.Clone();
        gpKeys.controllerType = ControllerType.GamepadAny;
    }

    #endregion

    #endregion

    #region SaveController

    [Serializable]
    private struct InputManagerConfigData
    {
        public InputData.InputDataRaw defaultKBKeys;
        public InputData.InputDataRaw defaultGPKeys;
        public InputData.InputDataRaw kbKeys;
        public InputData.InputDataRaw gpKeys;

        public Vector2 GP1RightThumbStickDeadZone, GP1LeftThumbStickDeadZone, GP1TriggersDeadZone;
        public Vector2 GP2RightThumbStickDeadZone, GP2LeftThumbStickDeadZone, GP2TriggersDeadZone;
        public Vector2 GP3RightThumbStickDeadZone, GP3LeftThumbStickDeadZone, GP3TriggersDeadZone;
        public Vector2 GP4RightThumbStickDeadZone, GP4LeftThumbStickDeadZone, GP4TriggersDeadZone;

        public InputManagerConfigData(InputData.InputDataRaw defaultKBKeys, InputData.InputDataRaw defaultGPKeys, InputData.InputDataRaw kbKeys, InputData.InputDataRaw gpKeys, Vector2 gP1RightThumbStickDeadZone, Vector2 gP1LeftThumbStickDeadZone, Vector2 gP1TriggersDeadZone,
            Vector2 gP2RightThumbStickDeadZone, Vector2 gP2LeftThumbStickDeadZone, Vector2 gP2TriggersDeadZone, Vector2 gP3RightThumbStickDeadZone, Vector2 gP3LeftThumbStickDeadZone,
            Vector2 gP3TriggersDeadZone, Vector2 gP4RightThumbStickDeadZone, Vector2 gP4LeftThumbStickDeadZone, Vector2 gP4TriggersDeadZone)
        {
            this.defaultKBKeys = defaultKBKeys;
            this.defaultGPKeys = defaultGPKeys;
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
        InputManagerConfigData InputManagerConfig = new InputManagerConfigData(defaultKBKeys.GetRawData(), defaultGPKeys.GetRawData(), kbKeys.GetRawData(), gpKeys.GetRawData(), GP1RightThumbStickDeadZone,
            GP1LeftThumbStickDeadZone, GP1TriggersDeadZone, GP2RightThumbStickDeadZone, GP2LeftThumbStickDeadZone, GP2TriggersDeadZone, GP3RightThumbStickDeadZone, GP3LeftThumbStickDeadZone, GP3TriggersDeadZone, 
            GP4RightThumbStickDeadZone, GP4LeftThumbStickDeadZone, GP4TriggersDeadZone);
        return Save.WriteJSONData(InputManagerConfig, fileName, true);
    }

    /// <summary>
    /// Save all the default InputManager configuration (default actions and controllers keys link to the action) for all players in the file fikename in the game repertory,
    /// but don't save the current InputManager configuration.
    /// Can be load using the methode InputManager.LoadDefaultConfiguration(string fileName).
    /// </summary>
    public static bool SaveDefaultConfiguration(string fileName)
    {
        if (!Save.ReadJSONData<InputManagerConfigData>(fileName, out InputManagerConfigData i))
        {
            InputManagerConfigData InputManagerConfig = new InputManagerConfigData(defaultKBKeys.GetRawData(), defaultGPKeys.GetRawData(), new InputData.InputDataRaw(Array.Empty<string>(), Array.Empty<InputData.ListInt>()),
                new InputData.InputDataRaw(Array.Empty<string>(), Array.Empty<InputData.ListInt>()), GP1RightThumbStickDeadZone, GP1LeftThumbStickDeadZone, GP1TriggersDeadZone, GP2RightThumbStickDeadZone,
                GP2LeftThumbStickDeadZone, GP2TriggersDeadZone, GP3RightThumbStickDeadZone, GP3LeftThumbStickDeadZone, GP3TriggersDeadZone, GP4RightThumbStickDeadZone, GP4LeftThumbStickDeadZone, GP4TriggersDeadZone);
            return Save.WriteJSONData(InputManagerConfig, fileName, true);
        }

        InputManagerConfigData InputManagerConfig2 = new InputManagerConfigData(defaultKBKeys.GetRawData(), defaultGPKeys.GetRawData(), i.kbKeys, i.gpKeys,
            i.GP1RightThumbStickDeadZone, i.GP1LeftThumbStickDeadZone, i.GP1TriggersDeadZone, i.GP2RightThumbStickDeadZone, i.GP2LeftThumbStickDeadZone, 
            i.GP2TriggersDeadZone, i.GP3RightThumbStickDeadZone, i.GP3LeftThumbStickDeadZone, i.GP3TriggersDeadZone, i.GP4RightThumbStickDeadZone, i.GP4LeftThumbStickDeadZone, i.GP4TriggersDeadZone);
        return Save.WriteJSONData(InputManagerConfig2, fileName, true);
    }

    /// <summary>
    /// Save all the current InputManager configuration (current actions and controllers keys link to the action) for all players in the file fikename in the game repertory,
    /// but don't save the default InputManager configuration.
    /// Can be load using the methode InputManager.LoadCurrentConfiguration(string fileName).
    /// </summary>
    public static bool SaveCurrentConfiguration(string fileName)
    {
        if (!Save.ReadJSONData<InputManagerConfigData>(fileName, out InputManagerConfigData i))
        {
            InputManagerConfigData InputManagerConfig = new InputManagerConfigData(new InputData.InputDataRaw(Array.Empty<string>(), Array.Empty<InputData.ListInt>()), new InputData.InputDataRaw(Array.Empty<string>(), Array.Empty<InputData.ListInt>()),
                kbKeys.GetRawData(), gpKeys.GetRawData(), GP1RightThumbStickDeadZone, GP1LeftThumbStickDeadZone, GP1TriggersDeadZone, GP2RightThumbStickDeadZone, GP2LeftThumbStickDeadZone, 
                GP2TriggersDeadZone, GP3RightThumbStickDeadZone, GP3LeftThumbStickDeadZone, GP3TriggersDeadZone, GP4RightThumbStickDeadZone, GP4LeftThumbStickDeadZone, GP4TriggersDeadZone);
            return Save.WriteJSONData(InputManagerConfig, fileName, true);
        }

        InputManagerConfigData InputManagerConfig2 = new InputManagerConfigData(i.defaultKBKeys, i.defaultGPKeys, kbKeys.GetRawData(), gpKeys.GetRawData(), GP1RightThumbStickDeadZone,
            GP1LeftThumbStickDeadZone, GP1TriggersDeadZone, GP2RightThumbStickDeadZone, GP2LeftThumbStickDeadZone, GP2TriggersDeadZone, GP3RightThumbStickDeadZone, GP3LeftThumbStickDeadZone, GP3TriggersDeadZone,
            GP4RightThumbStickDeadZone, GP4LeftThumbStickDeadZone, GP4TriggersDeadZone);
        return Save.WriteJSONData(InputManagerConfig2, fileName, true);
    }

    /// <summary>
    /// Load from the file Save in the game repertory all the configuration of the InputManager system.
    /// </summary>
    public static bool LoadConfiguration(string fileName)
    {
        if (!Save.ReadJSONData<InputManagerConfigData>(fileName, out InputManagerConfigData data))
            return false;
        
        defaultKBKeys = new InputData(ControllerType.Keyboard, data.defaultKBKeys);
        defaultGPKeys = new InputData(ControllerType.GamepadAny, data.defaultGPKeys);
        kbKeys = new InputData(ControllerType.Keyboard, data.kbKeys);
        gpKeys = new InputData(ControllerType.GamepadAny, data.gpKeys);
        GP1RightThumbStickDeadZone = data.GP1RightThumbStickDeadZone;
        GP1LeftThumbStickDeadZone = data.GP1LeftThumbStickDeadZone;
        GP1TriggersDeadZone = data.GP1TriggersDeadZone;
        GP2RightThumbStickDeadZone = data. GP2RightThumbStickDeadZone;
        GP2LeftThumbStickDeadZone = data.GP2LeftThumbStickDeadZone;
        GP2TriggersDeadZone = data.GP2TriggersDeadZone;
        GP3RightThumbStickDeadZone = data.GP3RightThumbStickDeadZone;
        GP3LeftThumbStickDeadZone = data.GP3LeftThumbStickDeadZone;
        GP3TriggersDeadZone = data.GP3TriggersDeadZone;
        GP4RightThumbStickDeadZone = data.GP4RightThumbStickDeadZone;
        GP4LeftThumbStickDeadZone = data.GP4LeftThumbStickDeadZone;
        GP4TriggersDeadZone = data.GP4TriggersDeadZone;
        return true;
    }

    /// <summary>
    /// Load from the file Save in the game repertory the default configuration of the InputManager system.
    /// </summary>
    public static bool LoadDefaultControllerConfiguration(string fileName)
    {
        if (!Save.ReadJSONData<InputManagerConfigData>(fileName, out InputManagerConfigData data))
            return false;

        defaultKBKeys = new InputData(ControllerType.Keyboard, data.defaultKBKeys);
        defaultGPKeys = new InputData(ControllerType.GamepadAny, data.defaultGPKeys);
        return true;
    }

    /// <summary>
    /// Load from the file Save in the game repertory the current configuration of the InputManager system.
    /// </summary>
    public static bool LoadNonDefaultControllerConfiguration(string fileName)
    {
        if (!Save.ReadJSONData<InputManagerConfigData>(fileName, out InputManagerConfigData data))
            return false;

        kbKeys = new InputData(ControllerType.Keyboard, data.kbKeys);
        gpKeys = new InputData(ControllerType.GamepadAny, data.gpKeys);
        GP1RightThumbStickDeadZone = data.GP1RightThumbStickDeadZone;
        GP1LeftThumbStickDeadZone = data.GP1LeftThumbStickDeadZone;
        GP1TriggersDeadZone = data.GP1TriggersDeadZone;
        GP2RightThumbStickDeadZone = data.GP2RightThumbStickDeadZone;
        GP2LeftThumbStickDeadZone = data.GP2LeftThumbStickDeadZone;
        GP2TriggersDeadZone = data.GP2TriggersDeadZone;
        GP3RightThumbStickDeadZone = data.GP3RightThumbStickDeadZone;
        GP3LeftThumbStickDeadZone = data.GP3LeftThumbStickDeadZone;
        GP3TriggersDeadZone = data.GP3TriggersDeadZone;
        GP4RightThumbStickDeadZone = data.GP4RightThumbStickDeadZone;
        GP4LeftThumbStickDeadZone = data.GP4LeftThumbStickDeadZone;
        GP4TriggersDeadZone = data.GP4TriggersDeadZone;
        return true;
    }

    /// <summary>
    /// Load from the file Save in the game repertory the current configuration of the InputManager system.
    /// </summary>
    public static bool LoadDeadZonesConfiguration(string fileName)
    {
        if (!Save.ReadJSONData<InputManagerConfigData>(fileName, out InputManagerConfigData data))
            return false;

        GP1RightThumbStickDeadZone = data.GP1RightThumbStickDeadZone;
        GP1LeftThumbStickDeadZone = data.GP1LeftThumbStickDeadZone;
        GP1TriggersDeadZone = data.GP1TriggersDeadZone;
        GP2RightThumbStickDeadZone = data.GP2RightThumbStickDeadZone;
        GP2LeftThumbStickDeadZone = data.GP2LeftThumbStickDeadZone;
        GP2TriggersDeadZone = data.GP2TriggersDeadZone;
        GP3RightThumbStickDeadZone = data.GP3RightThumbStickDeadZone;
        GP3LeftThumbStickDeadZone = data.GP3LeftThumbStickDeadZone;
        GP3TriggersDeadZone = data.GP3TriggersDeadZone;
        GP4RightThumbStickDeadZone = data.GP4RightThumbStickDeadZone;
        GP4LeftThumbStickDeadZone = data.GP4LeftThumbStickDeadZone;
        GP4TriggersDeadZone = data.GP4TriggersDeadZone;
        return true;
    }

    #endregion

    #region Save controller Async

    public static async Task<bool> SaveConfigurationAsync(string fileName, Action<bool> callback)
    {
        InputManagerConfigData InputManagerConfig = new InputManagerConfigData(defaultKBKeys.GetRawData(), defaultGPKeys.GetRawData(), kbKeys.GetRawData(), gpKeys.GetRawData(), GP1RightThumbStickDeadZone,
            GP1LeftThumbStickDeadZone, GP1TriggersDeadZone, GP2RightThumbStickDeadZone, GP2LeftThumbStickDeadZone, GP2TriggersDeadZone, GP3RightThumbStickDeadZone, GP3LeftThumbStickDeadZone, GP3TriggersDeadZone,
            GP4RightThumbStickDeadZone, GP4LeftThumbStickDeadZone, GP4TriggersDeadZone);

        return await Save.WriteJSONDataAsync(InputManagerConfig, fileName, callback, true);
    }

    /// <summary>
    /// Save all the default InputManager configuration (default actions and controllers keys link to the action) for all players in the file fikename in the game repertory,
    /// but don't save the current InputManager configuration.
    /// Can be load using the methode InputManager.LoadDefaultConfiguration(string fileName).
    /// </summary>
    public static async Task<bool> SaveDefaultConfiguration(string fileName, Action<bool> callback)
    {
        if (!Save.ReadJSONData<InputManagerConfigData>(fileName, out InputManagerConfigData i))
        {
            InputManagerConfigData InputManagerConfig = new InputManagerConfigData(defaultKBKeys.GetRawData(), defaultGPKeys.GetRawData(), new InputData.InputDataRaw(Array.Empty<string>(), Array.Empty<InputData.ListInt>()),
                new InputData.InputDataRaw(Array.Empty<string>(), Array.Empty<InputData.ListInt>()), GP1RightThumbStickDeadZone, GP1LeftThumbStickDeadZone, GP1TriggersDeadZone, GP2RightThumbStickDeadZone, 
                GP2LeftThumbStickDeadZone, GP2TriggersDeadZone, GP3RightThumbStickDeadZone, GP3LeftThumbStickDeadZone, GP3TriggersDeadZone, GP4RightThumbStickDeadZone, GP4LeftThumbStickDeadZone, GP4TriggersDeadZone);
            return await Save.WriteJSONDataAsync(InputManagerConfig, fileName, callback, true);
        }

        InputManagerConfigData InputManagerConfig2 = new InputManagerConfigData(defaultKBKeys.GetRawData(), defaultGPKeys.GetRawData(), i.kbKeys, i.gpKeys, i.GP1RightThumbStickDeadZone, i.GP1LeftThumbStickDeadZone, i.GP1TriggersDeadZone, i.GP2RightThumbStickDeadZone, i.GP2LeftThumbStickDeadZone, 
            i.GP2TriggersDeadZone, i.GP3RightThumbStickDeadZone, i.GP3LeftThumbStickDeadZone, i.GP3TriggersDeadZone, i.GP4RightThumbStickDeadZone, i.GP4LeftThumbStickDeadZone, i.GP4TriggersDeadZone);
        return await Save.WriteJSONDataAsync(InputManagerConfig2, fileName, callback, true);
    }

    /// <summary>
    /// Save all the current InputManager configuration (current actions and controllers keys link to the action) for all players in the file fikename in the game repertory,
    /// but don't save the default InputManager configuration.
    /// Can be load using the methode InputManager.LoadCurrentConfiguration(string fileName).
    /// </summary>
    public static async Task<bool> SaveCurrentConfigurationAsync(string fileName, Action<bool> callback)
    {
        if (!Save.ReadJSONData<InputManagerConfigData>(fileName, out InputManagerConfigData i))
        {
            InputManagerConfigData InputManagerConfig = new InputManagerConfigData(new InputData.InputDataRaw(Array.Empty<string>(), Array.Empty<InputData.ListInt>()), new InputData.InputDataRaw(Array.Empty<string>(), 
                Array.Empty<InputData.ListInt>()), kbKeys.GetRawData(), gpKeys.GetRawData(), GP1RightThumbStickDeadZone,
                GP1LeftThumbStickDeadZone, GP1TriggersDeadZone, GP2RightThumbStickDeadZone, GP2LeftThumbStickDeadZone, GP2TriggersDeadZone, GP3RightThumbStickDeadZone, GP3LeftThumbStickDeadZone, GP3TriggersDeadZone,
                GP4RightThumbStickDeadZone, GP4LeftThumbStickDeadZone, GP4TriggersDeadZone);
            return await Save.WriteJSONDataAsync(InputManagerConfig, fileName, callback, true);
        }

        InputManagerConfigData InputManagerConfig2 = new InputManagerConfigData(i.defaultKBKeys, i.defaultGPKeys, kbKeys.GetRawData(), gpKeys.GetRawData(), GP1RightThumbStickDeadZone,
            GP1LeftThumbStickDeadZone, GP1TriggersDeadZone, GP2RightThumbStickDeadZone, GP2LeftThumbStickDeadZone, GP2TriggersDeadZone, GP3RightThumbStickDeadZone, GP3LeftThumbStickDeadZone, GP3TriggersDeadZone,
            GP4RightThumbStickDeadZone, GP4LeftThumbStickDeadZone, GP4TriggersDeadZone);
        return await Save.WriteJSONDataAsync(InputManagerConfig2, fileName, callback, true);
    }

    /// <summary>
    /// Load from the file Save in the game repertory all the configuration of the InputManager system.
    /// </summary>
    public static async Task<bool> LoadConfigurationAsync(string fileName, Action<bool> callback)
    {
        void CallbackReadInputManagerConfigData(bool b, InputManagerConfigData data)
        {
            defaultKBKeys = new InputData(ControllerType.Keyboard, data.defaultKBKeys);
            defaultGPKeys = new InputData(ControllerType.GamepadAny, data.defaultGPKeys);
            kbKeys = new InputData(ControllerType.Keyboard, data.kbKeys);
            gpKeys = new InputData(ControllerType.GamepadAny, data.gpKeys);
            GP1RightThumbStickDeadZone = data.GP1RightThumbStickDeadZone;
            GP1LeftThumbStickDeadZone = data.GP1LeftThumbStickDeadZone;
            GP1TriggersDeadZone = data.GP1TriggersDeadZone;
            GP2RightThumbStickDeadZone = data.GP2RightThumbStickDeadZone;
            GP2LeftThumbStickDeadZone = data.GP2LeftThumbStickDeadZone;
            GP2TriggersDeadZone = data.GP2TriggersDeadZone;
            GP3RightThumbStickDeadZone = data.GP3RightThumbStickDeadZone;
            GP3LeftThumbStickDeadZone = data.GP3LeftThumbStickDeadZone;
            GP3TriggersDeadZone = data.GP3TriggersDeadZone;
            GP4RightThumbStickDeadZone = data.GP4RightThumbStickDeadZone;
            GP4LeftThumbStickDeadZone = data.GP4LeftThumbStickDeadZone;
            GP4TriggersDeadZone = data.GP4TriggersDeadZone;
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
        void CallbackReadInputManagerConfigData(bool b, InputManagerConfigData data)
        {
            defaultKBKeys = new InputData(ControllerType.Keyboard, data.defaultKBKeys);
            defaultGPKeys = new InputData(ControllerType.GamepadAny, data.defaultGPKeys);
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
        void CallbackReadInputManagerConfigData(bool b, InputManagerConfigData data)
        {
            kbKeys = new InputData(ControllerType.Keyboard, data.kbKeys);
            gpKeys = new InputData(ControllerType.GamepadAny, data.gpKeys);
            GP1RightThumbStickDeadZone = data.GP1RightThumbStickDeadZone;
            GP1LeftThumbStickDeadZone = data.GP1LeftThumbStickDeadZone;
            GP1TriggersDeadZone = data.GP1TriggersDeadZone;
            GP2RightThumbStickDeadZone = data.GP2RightThumbStickDeadZone;
            GP2LeftThumbStickDeadZone = data.GP2LeftThumbStickDeadZone;
            GP2TriggersDeadZone = data.GP2TriggersDeadZone;
            GP3RightThumbStickDeadZone = data.GP3RightThumbStickDeadZone;
            GP3LeftThumbStickDeadZone = data.GP3LeftThumbStickDeadZone;
            GP3TriggersDeadZone = data.GP3TriggersDeadZone;
            GP4RightThumbStickDeadZone = data.GP4RightThumbStickDeadZone;
            GP4LeftThumbStickDeadZone = data.GP4LeftThumbStickDeadZone;
            GP4TriggersDeadZone = data.GP4TriggersDeadZone;
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
        void CallbackReadInputManagerConfigData(bool b, InputManagerConfigData data)
        {
            GP1RightThumbStickDeadZone = data.GP1RightThumbStickDeadZone;
            GP1LeftThumbStickDeadZone = data.GP1LeftThumbStickDeadZone;
            GP1TriggersDeadZone = data.GP1TriggersDeadZone;
            GP2RightThumbStickDeadZone = data.GP2RightThumbStickDeadZone;
            GP2LeftThumbStickDeadZone = data.GP2LeftThumbStickDeadZone;
            GP2TriggersDeadZone = data.GP2TriggersDeadZone;
            GP3RightThumbStickDeadZone = data.GP3RightThumbStickDeadZone;
            GP3LeftThumbStickDeadZone = data.GP3LeftThumbStickDeadZone;
            GP3TriggersDeadZone = data.GP3TriggersDeadZone;
            GP4RightThumbStickDeadZone = data.GP4RightThumbStickDeadZone;
            GP4LeftThumbStickDeadZone = data.GP4LeftThumbStickDeadZone;
            GP4TriggersDeadZone = data.GP4TriggersDeadZone;
        }

        bool res = await Save.ReadJSONDataAsync<InputManagerConfigData>(fileName, CallbackReadInputManagerConfigData);
        callback.Invoke(res);
        return res;
    }

    #endregion

    #region Useful region

    #region Controller Name/Model

    public static string GetControllerName(ControllerType controller)
    {
        if (controller == ControllerType.Any)
        {
            string errorMsg = "Can't get the name of multiple controller";
            Debug.LogWarning(errorMsg);
            LogManager.instance.AddLog(errorMsg, "InputManager.GetControllerName", controller);
            return string.Empty;
        }

        if (controller == ControllerType.Keyboard)
            return "Keyboard";

        if(controller == ControllerType.GamepadAny)
            return GetCurrentGamepadName();


        int id = GetState(controller).deviceId;
        foreach(Gamepad gamepad in Gamepad.all)
        {
            if(gamepad.deviceId == id)
                return gamepad.displayName;
        }
        return string.Empty;
    }

    public static ControllerModel GetControllerModel(PlayerIndex playerIndex) => GetControllerModel(GetInputData(playerIndex).controllerType);

    public static ControllerModel GetControllerModel(ControllerType controllerType)
    {
        if(controllerType == ControllerType.Any || controllerType == ControllerType.GamepadAny)
        {
            string errorMsg = "Can't get the model of multiple devices";
            Debug.LogWarning(errorMsg);
            LogManager.instance.AddLog(errorMsg, "InputManager.GetGamepadModel(ControllerType controllerType)", controllerType);
            return ControllerModel.None;
        }

        if (controllerType == ControllerType.Keyboard)
            return ControllerModel.Keyboard;

        int id = GetState(controllerType).deviceId;
        foreach (Gamepad gamepad in Gamepad.all)
        {
            if (gamepad.deviceId == id)
                return GetGamepadControllerType(gamepad);
        }
        return ControllerModel.None;
    }

    public static ControllerModel GetCurrentGamepadModel()
    {
        return Gamepad.current != null ? GetGamepadControllerType(Gamepad.current) : ControllerModel.None;
    }

    private static ControllerModel GetGamepadControllerType(Gamepad gamepad)
    {
        bool CheckGamepad(Gamepad gamePad, string name)
        {
            string[] strToTests = new string[]
            {
                gamepad.name,
                gamepad.displayName,
                gamepad.shortDisplayName,
                gamepad.description.product,
                gamepad.description.manufacturer,
                gamepad.description.deviceClass,
                gamepad.description.serial,
                gamepad.description.interfaceName
            };

            StringComparison sc = StringComparison.OrdinalIgnoreCase;
            for (int i = 0; i < strToTests.Length; i++)
            {
                string str = strToTests[i];
                if (!string.IsNullOrEmpty(str) && str.Replace(" ", "").Contains(name, sc))
                    return true;
            }
            return false;
        }

        if (gamepad is XInputController)
        {
            if (CheckGamepad(gamepad, "Xbox360"))
                return ControllerModel.XBox360;
            if (CheckGamepad(gamepad, "XboxOne"))
                return ControllerModel.XBoxOne;
            return ControllerModel.XBoxSeries;
        }
        else if (gamepad is DualShockGamepad)
        {
            if (CheckGamepad(gamepad, "PS3"))
                return ControllerModel.PS3;
            if (CheckGamepad(gamepad, "PS4"))
                return ControllerModel.PS4;
            return ControllerModel.PS5;
        }
        else if (CheckGamepad(gamepad, "SteamDeck"))
            return ControllerModel.SteamDeck;
        else if (CheckGamepad(gamepad, "Switch"))
            return ControllerModel.Switch;
        else if (CheckGamepad(gamepad, "Luna"))
            return ControllerModel.AmazonLuna;
        else if (CheckGamepad(gamepad, "Stadia"))
            return ControllerModel.GoogleStadia;
        else if (CheckGamepad(gamepad, "Ouya"))
            return ControllerModel.Ouya;
        return ControllerModel.XBoxSeries;
    }

    #endregion

    #region Other

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

    #endregion

    #region KeyToString

    /// <summary>
    /// Convert a key into a string.
    /// </summary>
    /// <param name="key"> the key to convert to a string</param>
    public static string KeyToString(InputKey key) => key.ToString();

    private static string KeysToString(InputData.ListInt keys)
    {
        StringBuilder sb = new StringBuilder();
        foreach (int key in keys)
        {
            sb.Append(KeyToString((InputKey)key) + ",");
        }

        if(keys.Count >= 1)
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

    #endregion

    #region Listen

    #region ListenDown

    /// <param name="key"> the key pressed, castable to an Keys, MouseButton or Buttons according to the controler type</param>
    /// <param name="gamepadIndex"></param>
    /// <returns> true if a key of the controler is pressed this frame, false otherwise </returns>
    public static bool Listen(ControllerType controller, out InputKey key)
    {
        int i;
        switch (controller)
        {
            case ControllerType.Keyboard:
                foreach (KeyboardKey keyboardKey in Enum.GetValues(typeof(KeyboardKey)))
                {
                    if (Input.GetKeyDown((KeyCode)keyboardKey))
                    {
                        key = (InputKey)keyboardKey;
                        return true;
                    }
                }
                key = 0;
                return false;
            case ControllerType.Gamepad1:
                for (i = 340; i <= 349; i++)
                {
                    if (GetPositiveKeyDown(i))
                    {
                        key = (InputKey)i;
                        return true;
                    }
                }

                for (i = -14; i <= -1; i++)
                {
                    if(GetNegativeKeyDown(i))
                    {
                        key = (InputKey)i;
                        return true;
                    }
                }

                key = 0;
                return false;
            case ControllerType.Gamepad2:
                for (i = 350; i <= 359; i++)
                {
                    if (GetPositiveKeyDown(i))
                    {
                        key = (InputKey)i;
                        return true;
                    }
                }

                for (i = -28; i <= -15; i++)
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
                for (i = 360; i <= 369; i++)
                {
                    if (GetPositiveKeyDown(i))
                    {
                        key = (InputKey)i;
                        return true;
                    }
                }

                for (i = -42; i <= -29; i++)
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
                for (i = 370; i <= 379; i++)
                {
                    if (GetPositiveKeyDown(i))
                    {
                        key = (InputKey)i;
                        return true;
                    }
                }

                for (i = -56; i <= -43; i++)
                {
                    if (GetNegativeKeyDown(i))
                    {
                        key = (InputKey)i;
                        return true;
                    }
                }

                key = 0;
                return false;
            case ControllerType.GamepadAny:
                for (i = 330; i <= 339; i++)
                {
                    if (GetPositiveKeyDown(i))
                    {
                        key = (InputKey)i;
                        return true;
                    }
                }

                for (i = -70; i <= -57; i++)
                {
                    if (GetNegativeKeyDown(i))
                    {
                        key = (InputKey)i;
                        return true;
                    }
                }

                key = 0;
                return false;
            case ControllerType.Any:
                foreach (KeyboardKey keyboardKey in Enum.GetValues(typeof(KeyboardKey)))
                {
                    if (Input.GetKeyDown((KeyCode)keyboardKey))
                    {
                        key = (InputKey)keyboardKey;
                        return true;
                    }
                }

                for (i = 330; i <= 339; i++)
                {
                    if (GetPositiveKeyDown(i))
                    {
                        key = (InputKey)i;
                        return true;
                    }
                }

                for (i = -70; i <= -57; i++)
                {
                    if (GetNegativeKeyDown(i))
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
            return Listen(ControllerType.GamepadAny, out key);
        }
        if (controller == BaseController.Keyboard)
        {
            return Listen(ControllerType.Keyboard, out key);
        }
        return Listen(ControllerType.Any, out key);
    }

    public static bool ListenAll(ControllerType controller, out InputKey[] resultKeys)
    {
        List<InputKey> res = new List<InputKey>();
        int i;
        switch (controller)
        {
            case ControllerType.Keyboard:
                foreach (KeyboardKey keyboardKey in Enum.GetValues(typeof(KeyboardKey)))
                {
                    if (Input.GetKeyDown((KeyCode)keyboardKey))
                        res.Add((InputKey)keyboardKey);
                }
                break;
            case ControllerType.Gamepad1:
                for (i = 340; i <= 349; i++)
                {
                    if (GetPositiveKeyDown(i))
                        res.Add((InputKey)i);
                }

                for (i = -14; i <= -1; i++)
                {
                    if (GetNegativeKeyDown(i))
                        res.Add((InputKey)i);
                }
                break;
            case ControllerType.Gamepad2:
                for (i = 350; i <= 359; i++)
                {
                    if (GetPositiveKeyDown(i))
                        res.Add((InputKey)i);
                }

                for (i = -28; i <= -15; i++)
                {
                    if (GetNegativeKeyDown(i))
                        res.Add((InputKey)i);
                }
                break;
            case ControllerType.Gamepad3:
                for (i = 360; i <= 369; i++)
                {
                    if (GetPositiveKeyDown(i))
                        res.Add((InputKey)i);
                }

                for (i = -42; i <= -29; i++)
                {
                    if (GetNegativeKeyDown(i))
                        res.Add((InputKey)i);
                }
                break;
            case ControllerType.Gamepad4:
                for (i = 370; i <= 379; i++)
                {
                    if (GetPositiveKeyDown(i))
                        res.Add((InputKey)i);
                }

                for (i = -56; i <= -43; i++)
                {
                    if (GetNegativeKeyDown(i))
                        res.Add((InputKey)i);
                }
                break;
            case ControllerType.GamepadAny:
                for (i = 330; i <= 339; i++)
                {
                    if (GetPositiveKeyDown(i))
                        res.Add((InputKey)i);
                }

                for (i = -70; i <= -57; i++)
                {
                    if (GetNegativeKeyDown(i))
                        res.Add((InputKey)i);
                }
                break;
            case ControllerType.Any:
                foreach (KeyboardKey keyboardKey in Enum.GetValues(typeof(KeyboardKey)))
                {
                    if (Input.GetKeyDown((KeyCode)keyboardKey))
                        res.Add((InputKey)keyboardKey);
                }

                for (i = 330; i <= 339; i++)
                {
                    if (GetPositiveKeyDown(i))
                        res.Add((InputKey)i);
                }

                for (i = -70; i <= -57; i++)
                {
                    if (GetNegativeKeyDown(i))
                        res.Add((InputKey)i);
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
            return ListenAll(ControllerType.GamepadAny, out resultKeys);
        }
        if (controller == BaseController.Keyboard)
        {
            return ListenAll(ControllerType.Keyboard, out resultKeys);
        }
        return ListenAll(ControllerType.Any, out resultKeys);
    }

    #endregion

    #region ListenUp

    /// <param name="key"> the key pressed, castable to an Keys, MouseButton or Buttons according to the controler type</param>
    /// <param name="gamepadIndex"></param>
    /// <returns> true if a key of the controler is pressed this frame, false otherwise </returns>
    public static bool ListenUp(ControllerType controller, out InputKey key)
    {
        int i;
        switch (controller)
        {
            case ControllerType.Keyboard:
                foreach (KeyboardKey keyboardKey in Enum.GetValues(typeof(KeyboardKey)))
                {
                    if (Input.GetKeyUp((KeyCode)keyboardKey))
                    {
                        key = (InputKey)keyboardKey;
                        return true;
                    }
                }
                key = 0;
                return false;
            case ControllerType.Gamepad1:
                for (i = 340; i <= 349; i++)
                {
                    if (GetPositiveKeyUp(i))
                    {
                        key = (InputKey)i;
                        return true;
                    }
                }

                for (i = -14; i <= -1; i++)
                {
                    if (GetNegativeKeyUp(i))
                    {
                        key = (InputKey)i;
                        return true;
                    }
                }

                key = 0;
                return false;
            case ControllerType.Gamepad2:
                for (i = 350; i <= 359; i++)
                {
                    if (GetPositiveKeyUp(i))
                    {
                        key = (InputKey)i;
                        return true;
                    }
                }

                for (i = -28; i <= -15; i++)
                {
                    if (GetNegativeKeyUp(i))
                    {
                        key = (InputKey)i;
                        return true;
                    }
                }

                key = 0;
                return false;
            case ControllerType.Gamepad3:
                for (i = 360; i <= 369; i++)
                {
                    if (GetPositiveKeyUp(i))
                    {
                        key = (InputKey)i;
                        return true;
                    }
                }

                for (i = -42; i <= -29; i++)
                {
                    if (GetNegativeKeyUp(i))
                    {
                        key = (InputKey)i;
                        return true;
                    }
                }

                key = 0;
                return false;
            case ControllerType.Gamepad4:
                for (i = 370; i <= 379; i++)
                {
                    if (GetPositiveKeyUp(i))
                    {
                        key = (InputKey)i;
                        return true;
                    }
                }

                for (i = -56; i <= -43; i++)
                {
                    if (GetNegativeKeyUp(i))
                    {
                        key = (InputKey)i;
                        return true;
                    }
                }

                key = 0;
                return false;
            case ControllerType.GamepadAny:
                for (i = 330; i <= 339; i++)
                {
                    if (GetPositiveKeyUp(i))
                    {
                        key = (InputKey)i;
                        return true;
                    }
                }

                for (i = -70; i <= -57; i++)
                {
                    if (GetNegativeKeyUp(i))
                    {
                        key = (InputKey)i;
                        return true;
                    }
                }

                key = 0;
                return false;
            case ControllerType.Any:
                foreach (KeyboardKey keyboardKey in Enum.GetValues(typeof(KeyboardKey)))
                {
                    if (Input.GetKeyUp((KeyCode)keyboardKey))
                    {
                        key = (InputKey)keyboardKey;
                        return true;
                    }
                }

                for (i = 330; i <= 339; i++)
                {
                    if (GetPositiveKeyUp(i))
                    {
                        key = (InputKey)i;
                        return true;
                    }
                }

                for (i = -70; i <= -57; i++)
                {
                    if (GetNegativeKeyUp(i))
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

    public static bool ListenUp(BaseController controller, out InputKey key)
    {
        if (controller == BaseController.Gamepad)
        {
            return ListenUp(ControllerType.GamepadAny, out key);
        }
        if (controller == BaseController.Keyboard)
        {
            return ListenUp(ControllerType.Keyboard, out key);
        }
        return ListenUp(ControllerType.Any, out key);
    }

    public static bool ListenUpAll(ControllerType controller, out InputKey[] resultKeys)
    {
        List<InputKey> res = new List<InputKey>();
        int i;
        switch (controller)
        {
            case ControllerType.Keyboard:
                foreach (KeyboardKey keyboardKey in Enum.GetValues(typeof(KeyboardKey)))
                {
                    if (Input.GetKeyUp((KeyCode)keyboardKey))
                        res.Add((InputKey)keyboardKey);
                }
                break;
            case ControllerType.Gamepad1:
                for (i = 340; i <= 349; i++)
                {
                    if (GetPositiveKeyUp(i))
                        res.Add((InputKey)i);
                }

                for (i = -14; i <= -1; i++)
                {
                    if (GetNegativeKeyUp(i))
                        res.Add((InputKey)i);
                }
                break;
            case ControllerType.Gamepad2:
                for (i = 350; i <= 359; i++)
                {
                    if (GetPositiveKeyUp(i))
                        res.Add((InputKey)i);
                }

                for (i = -28; i <= -15; i++)
                {
                    if (GetNegativeKeyUp(i))
                        res.Add((InputKey)i);
                }
                break;
            case ControllerType.Gamepad3:
                for (i = 360; i <= 369; i++)
                {
                    if (GetPositiveKeyUp(i))
                        res.Add((InputKey)i);
                }

                for (i = -42; i <= -29; i++)
                {
                    if (GetNegativeKeyUp(i))
                        res.Add((InputKey)i);
                }
                break;
            case ControllerType.Gamepad4:
                for (i = 370; i <= 379; i++)
                {
                    if (GetPositiveKeyUp(i))
                        res.Add((InputKey)i);
                }

                for (i = -56; i <= -43; i++)
                {
                    if (GetNegativeKeyUp(i))
                        res.Add((InputKey)i);
                }
                break;
            case ControllerType.GamepadAny:
                for (i = 330; i <= 339; i++)
                {
                    if (GetPositiveKeyUp(i))
                        res.Add((InputKey)i);
                }

                for (i = -70; i <= -57; i++)
                {
                    if (GetNegativeKeyUp(i))
                        res.Add((InputKey)i);
                }
                break;
            case ControllerType.Any:
                foreach (KeyboardKey keyboardKey in Enum.GetValues(typeof(KeyboardKey)))
                {
                    if (Input.GetKeyUp((KeyCode)keyboardKey))
                        res.Add((InputKey)keyboardKey);
                }

                for (i = 330; i <= 339; i++)
                {
                    if (GetPositiveKeyUp(i))
                        res.Add((InputKey)i);
                }

                for (i = -70; i <= -57; i++)
                {
                    if (GetNegativeKeyUp(i))
                        res.Add((InputKey)i);
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

    public static bool ListenUpAll(BaseController controller, out InputKey[] resultKeys)
    {
        if (controller == BaseController.Gamepad)
        {
            return ListenUpAll(ControllerType.GamepadAny, out resultKeys);
        }
        if (controller == BaseController.Keyboard)
        {
            return ListenUpAll(ControllerType.Keyboard, out resultKeys);
        }
        return ListenUpAll(ControllerType.Any, out resultKeys);
    }

    #endregion

    private static bool IsPrintableKey(UnityEngine.InputSystem.Controls.KeyControl key, out string keyString)
    {
        if (key == Keyboard.current.spaceKey)
        {
            keyString = " ";
            return true;
        }

        string displayName = key.displayName;
        if (displayName.Length == 1 || char.IsPunctuation(displayName[0]) || char.IsSymbol(displayName[0]))
        {
            keyString = displayName;
            return true;
        }

        keyString = string.Empty;
        return false;
    }

    /// <param name="letter"> the letter pressed this frame</param>
    /// <returns>true if a key of the letter of the keyboard controller is pressed this frame, false otherwise</returns>
    public static bool CharPressed(out string letter)
    {
        if (Keyboard.current == null)
        {
            letter = string.Empty;
            return false;
        }

        foreach (UnityEngine.InputSystem.Controls.KeyControl key in Keyboard.current.allKeys)
        {
            if (key.wasPressedThisFrame)
            {
                if (IsPrintableKey(key, out string keyStr))
                {
                    letter = keyStr;
                    return true;
                }
            }
        }

        letter = string.Empty;
        return false;
    }

    /// <param name="number"> the number pressed this frame</param>
    /// <returns>true if a key of the number of the keyboard controller is pressed this frame, false otherwise</returns>
    public static bool NumberPressed(out string number)
    {
        if (Keyboard.current == null)
        {
            number = string.Empty;
            return false;
        }

        foreach (UnityEngine.InputSystem.Controls.KeyControl key in Keyboard.current.allKeys)
        {
            if (key.wasPressedThisFrame)
            {
                if (IsPrintableKey(key, out string keyStr) && char.IsDigit(keyStr[0]))
                {
                    number = keyStr;
                    return true;
                }
            }
        }

        number = string.Empty;
        return false;
    }

    #endregion

    #endregion

    #region Update

    public static void PreUpdate()
    {
        GamePadButtons GetButtonsFromGamepad(Gamepad gamepad)
        {
            ButtonState start = gamepad.startButton.isPressed ? ButtonState.Pressed : ButtonState.Released;
            ButtonState back = gamepad.selectButton.isPressed ? ButtonState.Pressed : ButtonState.Released;
            ButtonState leftShoulder = gamepad.leftShoulder.isPressed ? ButtonState.Pressed : ButtonState.Released;
            ButtonState rightShoulder = gamepad.rightShoulder.isPressed ? ButtonState.Pressed : ButtonState.Released;
            ButtonState a = gamepad.aButton.isPressed ? ButtonState.Pressed : ButtonState.Released;
            ButtonState x = gamepad.xButton.isPressed ? ButtonState.Pressed : ButtonState.Released;
            ButtonState b = gamepad.bButton.isPressed ? ButtonState.Pressed : ButtonState.Released;
            ButtonState y = gamepad.yButton.isPressed ? ButtonState.Pressed : ButtonState.Released;
            ButtonState guide = ButtonState.Released;
            ButtonState leftStick = gamepad.leftStickButton.isPressed ? ButtonState.Pressed : ButtonState.Released;
            ButtonState rightStick = gamepad.rightStickButton.isPressed ? ButtonState.Pressed : ButtonState.Released;
            return new GamePadButtons(start, back, leftStick, rightStick, leftShoulder, rightShoulder, guide, a, b, x, y);
        }

        GamePadDPad GetDPadFromGamepad(Gamepad gamepad)
        {
            ButtonState up = gamepad.dpad.up.isPressed ? ButtonState.Pressed : ButtonState.Released;
            ButtonState down = gamepad.dpad.down.isPressed ? ButtonState.Pressed : ButtonState.Released;
            ButtonState right = gamepad.dpad.right.isPressed ? ButtonState.Pressed : ButtonState.Released;
            ButtonState left = gamepad.dpad.left.isPressed ? ButtonState.Pressed : ButtonState.Released;
            return new GamePadDPad(up, down, left, right);
        }

        GamePadThumbSticks GetThumbSticksFromGamepad(Gamepad gamepad)
        {
            Vector2 right = new Vector2(gamepad.rightStick.x.value, gamepad.rightStick.y.value);
            Vector2 left = new Vector2(gamepad.leftStick.x.value, gamepad.leftStick.y.value);
            return new GamePadThumbSticks(left, right);
        }

        GamePadTriggers GetTriggersFromGamepad(Gamepad gamepad)
        {
            return new GamePadTriggers(gamepad.leftTrigger.value, gamepad.rightTrigger.value);
        }

        GamePadState GetGamepadStateFromGamepad(Gamepad gamepad)
        {
            int deviceId = gamepad.deviceId;
            GamePadButtons buttons = GetButtonsFromGamepad(gamepad);
            GamePadDPad dPad = GetDPadFromGamepad(gamepad);
            GamePadThumbSticks thumbSticks = GetThumbSticksFromGamepad(gamepad);
            GamePadTriggers triggers = GetTriggersFromGamepad(gamepad);
            return new GamePadState(true, deviceId, buttons, dPad, thumbSticks, triggers);
        }

        oldGP1State = newGP1State;
        oldGP2State = newGP2State;
        oldGP3State = newGP3State;
        oldGP4State = newGP4State;

        GamePadState[] gamepadsStateTemp = new GamePadState[4]
        {
            GamePadState.Empty,
            GamePadState.Empty,
            GamePadState.Empty,
            GamePadState.Empty
        };

        ReadOnlyArray<Gamepad> gamepads = Gamepad.all;
        for (int i = 0; i < gamepads.Count; i++)
        {
            int deviceId = gamepads[i].deviceId;
            if(oldGP1State.deviceId == deviceId || oldGP2State.deviceId == deviceId || oldGP3State.deviceId == deviceId || oldGP4State.deviceId == deviceId)
            {
                if(oldGP1State.deviceId == deviceId || oldGP2State.deviceId == deviceId)
                {
                    if (oldGP1State.deviceId == deviceId)
                        gamepadsStateTemp[0] = GetGamepadStateFromGamepad(gamepads[i]);
                    else
                        gamepadsStateTemp[1] = GetGamepadStateFromGamepad(gamepads[i]);
                }
                else
                {
                    if (oldGP3State.deviceId == deviceId)
                        gamepadsStateTemp[2] = GetGamepadStateFromGamepad(gamepads[i]);
                    else
                        gamepadsStateTemp[3] = GetGamepadStateFromGamepad(gamepads[i]);
                }
                continue;
            }
        }

        for (int i = 0; i < gamepads.Count; i++)
        {
            int deviceId = gamepads[i].deviceId;
            if (oldGP1State.deviceId != deviceId && oldGP2State.deviceId != deviceId && oldGP3State.deviceId != deviceId && oldGP4State.deviceId != deviceId)
            {
                if (!gamepadsStateTemp[0].isConnected)
                    gamepadsStateTemp[0] = GetGamepadStateFromGamepad(gamepads[i]);
                else if (!gamepadsStateTemp[1].isConnected)
                    gamepadsStateTemp[1] = GetGamepadStateFromGamepad(gamepads[i]);
                else if (!gamepadsStateTemp[2].isConnected)
                    gamepadsStateTemp[2] = GetGamepadStateFromGamepad(gamepads[i]);
                else if (!gamepadsStateTemp[3].isConnected)
                    gamepadsStateTemp[3] = GetGamepadStateFromGamepad(gamepads[i]);
            }
        }

        newGP1State = gamepadsStateTemp[0];
        newGP2State = gamepadsStateTemp[1];
        newGP3State = gamepadsStateTemp[2];
        newGP4State = gamepadsStateTemp[3];

        SetNewGamepadSticksAndTriggersPositions();

        //vibration
        List<VibrationSettings> stopSetting = new List<VibrationSettings>(); 
        for (int i = vibrationSettings.Count - 1; i >= 0; i--)
        {
            VibrationSettings setting = vibrationSettings[i];
            setting.timer += Time.deltaTime;
            if(setting.timer > setting.duration)
            {
                stopSetting.Add(setting);
                vibrationSettings.RemoveAt(i);
                continue;
            }

            if(setting.timer > 0f)
                SetVibrationInternal(setting.lowFrequency, setting.highFrequency, setting.gamepadIndex);
        }

        foreach (VibrationSettings vib in stopSetting)
        {
            StopVibration(vib.gamepadIndex);
        }
    }

    #endregion

    #region Custom Struct

    #region GamepadState

    private enum ButtonState
    {
        Pressed,
        Released
    }

    private struct GamePadButtons
    {
        public ButtonState start;
        public ButtonState back;
        public ButtonState leftStick;
        public ButtonState rightStick;
        public ButtonState leftShoulder;
        public ButtonState rightShoulder;
        public ButtonState guide;
        public ButtonState a;
        public ButtonState b;
        public ButtonState x;
        public ButtonState y;

        public GamePadButtons(ButtonState start, ButtonState back, ButtonState leftStick, ButtonState rightStick, ButtonState leftShoulder, ButtonState rightShoulder, ButtonState guide, ButtonState a, ButtonState b, ButtonState x, ButtonState y)
        {
            this.start = start;
            this.back = back;
            this.leftStick = leftStick;
            this.rightStick = rightStick;
            this.leftShoulder = leftShoulder;
            this.rightShoulder = rightShoulder;
            this.guide = guide;
            this.a = a;
            this.b = b;
            this.x = x;
            this.y = y;
        }
    }

    private struct GamePadDPad
    {
        public ButtonState up;
        public ButtonState down;
        public ButtonState left;
        public ButtonState right;

        public GamePadDPad(ButtonState up, ButtonState down, ButtonState left, ButtonState right)
        {
            this.up = up;
            this.down = down;
            this.left = left;
            this.right = right;
        }
    }

    private struct GamePadTriggers
    {
        public float left;
        public float right;

        public GamePadTriggers(float left, float right)
        {
            this.left = left;
            this.right = right;
        }
    }

    private struct GamePadThumbSticks
    {
        public Vector2 left;
        public Vector2 right;

        public GamePadThumbSticks(in Vector2 left, in Vector2 right)
        {
            this.left = left;
            this.right = right;
        }
    }

    private struct GamePadState
    {
        public static readonly GamePadState Empty = new GamePadState(
            false,
            0,
            new GamePadButtons(ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released),
            new GamePadDPad(ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released),
            new GamePadThumbSticks(Vector2.zero, Vector2.zero),
            new GamePadTriggers(0f, 0f));

        public bool isConnected;
        public int deviceId;
        public GamePadButtons buttons;
        public GamePadDPad dPad;
        public GamePadThumbSticks thumbSticks;
        public GamePadTriggers triggers;

        public GamePadState(bool isConnected, int deviceId, GamePadButtons buttons, GamePadDPad dPad, GamePadThumbSticks thumbSticks, GamePadTriggers triggers)
        {
            this.isConnected = isConnected;
            this.deviceId = deviceId;
            this.buttons = buttons;
            this.dPad = dPad;
            this.thumbSticks = thumbSticks;
            this.triggers = triggers;
        }
    }

    #endregion

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
                case ControllerType.GamepadAny:
                    return GetKeySomething(func, ConvertGamepadKeysToInputKeys(keyGamepad1)) || GetKeySomething(func, ConvertGamepadKeysToInputKeys(keyGamepad2))
                        || GetKeySomething(func, ConvertGamepadKeysToInputKeys(keyGamepad3)) || GetKeySomething(func, ConvertGamepadKeysToInputKeys(keyGamepad4));
                case ControllerType.Any:
                    return GetKeySomething(func, ConvertKeyboardKeysToInputKeys(keysKeyboard)) || GetKeySomething(func, ConvertGamepadKeysToInputKeys(keyGamepad1)) || GetKeySomething(func, ConvertGamepadKeysToInputKeys(keyGamepad2))
                        || GetKeySomething(func, ConvertGamepadKeysToInputKeys(keyGamepad3)) || GetKeySomething(func, ConvertGamepadKeysToInputKeys(keyGamepad4));
                default:
                    return false;
            }

            bool GetKeySomething(Func<InputKey, bool> predicate, InputKey[] keys)
            {
                foreach (InputKey k in keys)
                {
                    if(predicate(k))
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

    private class VibrationSettings : ICloneable<VibrationSettings>
    {
        public ControllerType gamepadIndex;
        public float duration, lowFrequency, highFrequency, timer;

        public VibrationSettings(ControllerType gamepadIndex, float duration, float lowFrequency, float highFrequency)
        {
            this.gamepadIndex = gamepadIndex;
            this.duration = duration;
            this.lowFrequency = lowFrequency;
            this.highFrequency = highFrequency;
            timer = 0f;
        }

        public VibrationSettings Clone() => new VibrationSettings(gamepadIndex, duration, lowFrequency, highFrequency);
    }

    #endregion

    #endregion
}

#endregion
