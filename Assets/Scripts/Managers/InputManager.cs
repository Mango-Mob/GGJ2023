using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

/// <summary>
/// Michael Jordan
/// </summary>
/// 

#region InputEnums
public enum KeyType
{
    Q, W, E, R, T, Y, U, I, O, P,
    A, S, D, F, G, H, J, K, L,
    Z, X, C, V, B, N, M,
    ALP_ONE, ALP_TWO, ALP_THREE, ALP_FOUR, ALP_FIVE, ALP_SIX, ALP_SEVEN, ALP_EIGHT, ALP_NINE, ALP_ZERO,
    NUM_ONE, NUM_TWO, NUM_THREE, NUM_FOUR, NUM_FIVE, NUM_SIX, NUM_SEVEN, NUM_EIGHT, NUM_NINE, NUM_ZERO,
    L_SHIFT, L_CTRL, L_ALT, TAB, ESC, SPACE,
    R_SHIFT, R_CTRL, R_ALT, ENTER,
    TILDE, UP, LEFT, RIGHT, DOWN, NONE
}
public enum MouseButton
{
    LEFT, MIDDLE, RIGHT, NONE
}
public enum ButtonType
{
    NORTH, SOUTH, EAST, WEST,
    START, SELECT, 
    LT, LB, LS, RT, RB, RS,
    UP, RIGHT, DOWN, LEFT,
    NONE
}

public enum StickType
{
    LEFT,
    RIGHT,
    NONE
}

#endregion

public class InputManager : SingletonPersistent<InputManager>
{
    protected Mouse mouse;
    protected Keyboard keyboard = Keyboard.current;
    protected int gamepadCount;

    public bool bindHasUpdated = true;
    [Header("Keyboard Sprites")]
    [SerializeField] protected Sprite m_leftClick;
    [SerializeField] protected Sprite m_rightClick;
    [SerializeField] protected Sprite m_space;
    [SerializeField] protected Sprite m_backspace;
    [SerializeField] protected Sprite m_enter;
    [SerializeField] protected Sprite m_lShift;
    [SerializeField] protected Sprite m_lCtrl;
    [SerializeField] protected Sprite m_lAlt;
    [SerializeField] protected Sprite m_rShift;
    [SerializeField] protected Sprite m_rCtrl;
    [SerializeField] protected Sprite m_rAlt;
    [SerializeField] protected Sprite m_tab;
    [SerializeField] protected Sprite m_escape;
    [SerializeField] protected Sprite m_tilde;

    [Header("Gamepad Sprites")]
    [SerializeField] protected Sprite m_north;
    [SerializeField] protected Sprite m_south;
    [SerializeField] protected Sprite m_east;
    [SerializeField] protected Sprite m_west;

    [SerializeField] protected Sprite m_dpadUp;
    [SerializeField] protected Sprite m_dpadDown;
    [SerializeField] protected Sprite m_dpadLeft;
    [SerializeField] protected Sprite m_dpadRight;

    [SerializeField] protected Sprite m_leftBumper;
    [SerializeField] protected Sprite m_leftTrigger;
    [SerializeField] protected Sprite m_leftStick;
    [SerializeField] protected Sprite m_leftStickMove;

    [SerializeField] protected Sprite m_rightBumper;
    [SerializeField] protected Sprite m_rightTrigger;
    [SerializeField] protected Sprite m_rightStick;
    [SerializeField] protected Sprite m_rightStickMove;

    [SerializeField] protected Sprite m_start;
    [SerializeField] protected Sprite m_select;

    public class Bind
    {
        public Bind(Type _type, int _value) { enumType = _type; value = _value; }
        public Bind(int _type, int _value) { enumType = Bind.GetTypeFromID(_type); value = _value; }

        public Type enumType;
        public int value;

        public static int GetTypeID(Type _type)
        {
            if (_type == typeof(KeyType)) { return 0; }
            if (_type == typeof(MouseButton)) { return 1; }
            if (_type == typeof(ButtonType)) { return 2; }
            if (_type == typeof(StickType)) { return 3; }
            return -1;
        }

        public static Type GetTypeFromID(int _type)
        {
            switch (_type)
            {
                default:
                case 0: return typeof(KeyType);
                case 1: return typeof(MouseButton);
                case 2: return typeof(ButtonType);
                case 3: return typeof(StickType);
            }
        }
        public override bool Equals(object obj)
        {
            return (enumType == (obj as Bind).enumType) && (value == (obj as Bind).value);
        }
    }

    private Dictionary<string, Bind[]> m_binds;

    public bool isInGamepadMode { get; private set; } = false;
    protected override void Awake()
    {
        base.Awake();
        mouse = Mouse.current;
        Debug.Log($"{mouse.displayName} has connected as a PRIMARY_MOUSE to the InputManager.");
        keyboard = Keyboard.current;
        Debug.Log($"{keyboard.displayName} has connected as a PRIMARY_KEYBOARD to the InputManager.");
        gamepadCount = Gamepad.all.Count;

        if(gamepadCount > 0)
        {
            for (int i = 0; i < gamepadCount; i++)
            {
                Debug.Log($"{Gamepad.all[i].displayName} has connected as a GAMEPAD (ID: {i}) to the InputManager.");
            }
        }

        m_binds = new Dictionary<string, Bind[]>();
        int keyCount = PlayerPrefs.GetInt("BindCount", 0);
        if(keyCount == 0)
        {
            SetDefaultKeyBinds();
        }
        else
        {
            LoadBinds();
        }
    }

    private void Update()
    {
        if(gamepadCount < Gamepad.all.Count)
        {
            for (int i = gamepadCount; i < Gamepad.all.Count; i++)
            {
                Debug.Log($"{Gamepad.all[i].displayName} has connected as a GAMEPAD (ID: {i}) to the InputManager.");
            }
            gamepadCount = Gamepad.all.Count;
        }
        if (IsAnyKeyDown() != KeyType.NONE || IsAnyMouseButtonDown() != MouseButton.NONE)
        {
            isInGamepadMode = false;
        }
        else
        {
            int gamepadID = GetAnyGamePad();
            if (gamepadID != -1)
            {
                isInGamepadMode = true;
            }
        }
    }

    private void LateUpdate()
    {
        if (!bindHasUpdated)
            return;

        for (int i = m_binds.Count - 1; i >= 0; i--)
        {
            string key = m_binds.Keys.ToList()[i];
            Bind[] array = m_binds[key];

            int newSize = (array != null) ? array.Length : 0;

            if (newSize == 0)
                continue;

            foreach (var bind in array)
            {
                if (bind == null)
                {
                    newSize--;
                }
            }

            if (newSize == array.Length)
                continue;

            if (newSize > 0)
            {
                Bind[] newArray = new Bind[newSize];
                int k = 0;
                for (int j = 0; j < array.Length; j++)
                {
                    if (array[j] != null)
                        newArray[k++] = array[j];
                }

                m_binds[key] = newArray;
            }
            else
            {
                m_binds[key] = null;
            }
        }

        bindHasUpdated = false;
    }

    #region Binds
    public void SetDefaultKeyBinds()
    {
        m_binds.Clear();

        //Movement
        m_binds.Add("Move", new Bind[] { new Bind(typeof(StickType), (int)StickType.LEFT) });
        m_binds.Add("Move_Forward", new Bind[] { new Bind(typeof(KeyType), (int)KeyType.W) });
        m_binds.Add("Move_Backward", new Bind[] { new Bind(typeof(KeyType), (int)KeyType.S) });
        m_binds.Add("Move_Left", new Bind[] { new Bind(typeof(KeyType), (int)KeyType.A) }); ;
        m_binds.Add("Move_Right", new Bind[] { new Bind(typeof(KeyType), (int)KeyType.D) });
        m_binds.Add("Roll", new Bind[] { new Bind(typeof(KeyType), (int)KeyType.SPACE), new Bind(typeof(ButtonType), (int)ButtonType.EAST) });

        //Attack
        m_binds.Add("Aim", new Bind[] { new Bind(typeof(StickType), (int)StickType.RIGHT) });
        m_binds.Add("Toggle_Lockon", new Bind[] { new Bind(typeof(KeyType), (int)KeyType.L_ALT), new Bind(typeof(ButtonType), (int)ButtonType.RS) });
        m_binds.Add("Left_Attack", new Bind[] { new Bind(typeof(ButtonType), (int)ButtonType.LB), new Bind(typeof(MouseButton), (int)MouseButton.RIGHT) });
        m_binds.Add("Right_Attack", new Bind[] { new Bind(typeof(MouseButton), (int)MouseButton.LEFT), new Bind(typeof(ButtonType), (int)ButtonType.RB) });
        m_binds.Add("Left_Ability", new Bind[] { new Bind(typeof(KeyType), (int)KeyType.Q), new Bind(typeof(ButtonType), (int)ButtonType.LT) });
        m_binds.Add("Right_Ability", new Bind[] { new Bind(typeof(KeyType), (int)KeyType.E), new Bind(typeof(ButtonType), (int)ButtonType.RT) });

        //Other
        m_binds.Add("Interact", new Bind[] { new Bind(typeof(KeyType), (int)KeyType.X), new Bind(typeof(ButtonType), (int)ButtonType.WEST) });
        m_binds.Add("Switch", new Bind[] { new Bind(typeof(KeyType), (int)KeyType.Y), new Bind(typeof(ButtonType), (int)ButtonType.UP) });
        m_binds.Add("Consume", new Bind[] { new Bind(typeof(KeyType), (int)KeyType.V), new Bind(typeof(ButtonType), (int)ButtonType.NORTH) });
        m_binds.Add("Pause", new Bind[] { new Bind(typeof(KeyType), (int)KeyType.ESC), new Bind(typeof(ButtonType), (int)ButtonType.START) });
        m_binds.Add("Toggle_Zoom", new Bind[] { new Bind(typeof(KeyType), (int)KeyType.Z), new Bind(typeof(ButtonType), (int)ButtonType.LS) });
        m_binds.Add("Right_Pickup", new Bind[] { new Bind(typeof(MouseButton), (int)MouseButton.LEFT), new Bind(typeof(ButtonType), (int)ButtonType.LEFT) });
        m_binds.Add("Left_Pickup", new Bind[] { new Bind(typeof(MouseButton), (int)MouseButton.RIGHT), new Bind(typeof(ButtonType), (int)ButtonType.RIGHT) });

        //SkillTree
        m_binds.Add("Skill_Select", new Bind[] { new Bind(typeof(MouseButton), (int)MouseButton.LEFT), new Bind(typeof(ButtonType), (int)ButtonType.SOUTH) });
        m_binds.Add("Skill_Back", new Bind[] { new Bind(typeof(KeyType), (int)KeyType.Q), new Bind(typeof(ButtonType), (int)ButtonType.EAST) });
        m_binds.Add("Skill_Reset", new Bind[] { new Bind(typeof(KeyType), (int)KeyType.R), new Bind(typeof(ButtonType), (int)ButtonType.NORTH) });
        SaveBinds();
    }

    public void SaveBinds()
    {
        PlayerPrefs.SetInt("BindCount", m_binds.Count);

        int i = 0;
        foreach (var bind in m_binds)
        {
            PlayerPrefs.SetString($"Bind{i}Name", bind.Key);
            PlayerPrefs.SetInt($"Bind{i}Count", (bind.Value != null) ? bind.Value.Length : 0);
            for (int j = 0; j < bind.Value.Length; j++)
            {
                if(bind.Value[j] == null)
                {
                    PlayerPrefs.SetInt($"Bind{i}.{j}.TypeID", -1);
                    PlayerPrefs.SetInt($"Bind{i}.{j}.Value", -1);
                }
                else
                {
                    PlayerPrefs.SetInt($"Bind{i}.{j}.TypeID", Bind.GetTypeID(bind.Value[j].enumType));
                    PlayerPrefs.SetInt($"Bind{i}.{j}.Value", bind.Value[j].value);
                }
            }
            i++;
        }
    }
    public void LoadBinds()
    {
        int bindCount = PlayerPrefs.GetInt("BindCount");
        for (int i = 0; i < bindCount; i++)
        {
            string name = PlayerPrefs.GetString($"Bind{i}Name");
            int count = PlayerPrefs.GetInt($"Bind{i}Count");

            List<Bind> list = new List<Bind>();
            for (int j = 0; j < count; j++)
            {
                int type = PlayerPrefs.GetInt($"Bind{i}.{j}.TypeID");
                int value = PlayerPrefs.GetInt($"Bind{i}.{j}.Value");

                if(type == -1 || value == -1)
                {
                    list.Add(null);
                }
                else
                {
                    list.Add(new Bind(type, value));
                }
                
            }
            m_binds.Add(name, list.ToArray());
        }
    }
    public Bind[] GetBinds(string _id)
    {
        if (m_binds.ContainsKey(_id))
            return m_binds[_id];

        return null;
    }

    public string GetBindString(string _id)
    {
        Bind[] result = null;
        if (m_binds.ContainsKey(_id))
            result = m_binds[_id];

        if(result != null)
        {
            foreach (var bind in result)
            {
                if (bind.enumType == typeof(KeyType))
                {
                    return GetKeyString((KeyType)bind.value);
                }
                else if (bind.enumType == typeof(MouseButton))
                {
                    return GetMouseButtonString((MouseButton)bind.value);
                }
            }
        }
        
        return "";
    }

    public Sprite GetBindImage(string _id, bool includeGamepad = true)
    {
        Bind[] result = null;
        if (m_binds.ContainsKey(_id))
            result = m_binds[_id];

        Sprite resultImage = null;
        if (result != null)
        {
            foreach (var bind in result)
            {
                if(includeGamepad)
                {
                    if (bind.enumType == typeof(ButtonType))
                    {
                        resultImage = GetGamepadSprite((ButtonType)bind.value);
                    }
                    else if (bind.enumType == typeof(StickType))
                    {
                        resultImage = GetGameStickSprite((StickType)bind.value);
                    }
                }
                else
                {
                    if (bind.enumType == typeof(KeyType))
                    {
                        resultImage = GetKeyImage((KeyType)bind.value);
                    }
                    else if (bind.enumType == typeof(MouseButton))
                    {
                        resultImage = GetMouseButtonSprite((MouseButton)bind.value);
                    }
                }

                if (resultImage != null)
                    break;
            }

        }

        return resultImage;
    }

    public bool DoesBindContainNull(string _id)
    {
        Bind[] list;
        if(m_binds.TryGetValue(_id, out list))
        {
            foreach (var bind in list)
            {
                if(bind == null)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void SetBinds(string _id, Bind[] _binds)
    {
        if (m_binds.ContainsKey(_id))
            m_binds[_id] = _binds;

        bindHasUpdated = true;

        //foreach (var item in m_binds)
        //{
        //    if(item.Key != _id && item.Value != null)
        //    {
        //        for (int i = 0; i < item.Value.Length; i++)
        //        {
        //            foreach (var newBinds in _binds)
        //            {
        //                if (newBinds.Equals(item.Value[i])) 
        //                {
        //                    //Found Dupe, clearing old dupe
        //                    item.Value[i] = null;
        //                    break;
        //                }
        //            }
        //        }
        //    }
        //}
    }

    public bool IsBindDown(string bindName, int gamepadID = 0)
    {
        Bind[] temp;
        if(m_binds.TryGetValue(bindName, out temp))
        {
            bool isDown = false;
            foreach (var item in temp)
            {
                switch (Bind.GetTypeID(item.enumType))
                {
                    
                    case 0: 
                        isDown = isDown || IsKeyDown((KeyType)item.value);
                        break;
                    case 1: 
                        isDown = isDown || IsMouseButtonDown((MouseButton)item.value); 
                        break;
                    case 2: 
                        isDown = isDown || IsGamepadButtonDown((ButtonType)item.value, gamepadID);
                        break;

                    case 3:
                    default:
                        continue;
                }
            }
            return isDown;
        }

        Debug.LogWarning($"Bind of {bindName} does not exist.");
        return false;
    }
    public bool IsBindUp(string bindName, int gamepadID = 0)
    {
        Bind[] temp;
        if (m_binds.TryGetValue(bindName, out temp))
        {
            bool isDown = false;
            foreach (var item in temp)
            {
                switch (Bind.GetTypeID(item.enumType))
                {

                    case 0:
                        isDown = isDown || IsKeyUp((KeyType)item.value);
                        break;
                    case 1:
                        isDown = isDown || IsMouseButtonUp((MouseButton)item.value);
                        break;
                    case 2:
                        isDown = isDown || IsGamepadButtonUp((ButtonType)item.value, gamepadID);
                        break;

                    case 3:
                    default:
                        continue;
                }
            }
            return isDown;
        }

        Debug.LogWarning($"Bind of {bindName} does not exist.");
        return false;
    }

    public bool IsBindPressed(string bindName, int gamepadID = 0)
    {
        Bind[] temp;
        if (m_binds.TryGetValue(bindName, out temp))
        {
            bool isDown = false;
            foreach (var item in temp)
            {
                switch (Bind.GetTypeID(item.enumType))
                {

                    case 0:
                        isDown = isDown || IsKeyPressed((KeyType)item.value);
                        break;
                    case 1:
                        isDown = isDown || IsMouseButtonPressed((MouseButton)item.value);
                        break;
                    case 2:
                        isDown = isDown || IsGamepadButtonPressed((ButtonType)item.value, gamepadID);
                        break;

                    case 3:
                    default:
                        continue;
                }
            }
            return isDown;
        }

        Debug.LogWarning($"Bind of {bindName} does not exist.");
        return false;
    }

    public bool IsBindReleased(string bindName, int gamepadID = 0)
    {
        Bind[] temp;
        if (m_binds.TryGetValue(bindName, out temp))
        {
            bool isDown = false;
            foreach (var item in temp)
            {
                switch (Bind.GetTypeID(item.enumType))
                {

                    case 0:
                        isDown = isDown || IsKeyReleased((KeyType)item.value);
                        break;
                    case 1:
                        isDown = isDown || IsMouseButtonReleased((MouseButton)item.value);
                        break;
                    case 2:
                        isDown = isDown || IsGamepadButtonReleased((ButtonType)item.value, gamepadID);
                        break;

                    case 3:
                    default:
                        continue;
                }
            }
            return isDown;
        }

        Debug.LogWarning($"Bind of {bindName} does not exist.");
        return false;
    }

    public Vector2 GetBindStick(string bindName, int gamepadID = 0)
    {
        Bind[] temp;
        if (m_binds.TryGetValue(bindName, out temp))
        {
            Vector2 stick = Vector2.zero;
            foreach (var item in temp)
            {
                switch (Bind.GetTypeID(item.enumType))
                {
                    case 3:
                        stick = GetGamepadStick((StickType)item.value, gamepadID);
                        break;
                    case 0:
                    case 1:
                    case 2:
                    default:
                        continue;
                }
            }
            return stick;
        }

        Debug.LogWarning($"Bind of {bindName} does not exist.");
        return Vector2.zero;
    }
    #endregion

    #region GamePad
    public int GetAnyGamePad()
    {
        for (int i = 0; i < Gamepad.all.Count; i++)
        {
            if (IsAnyGamePadInput(i) != ButtonType.NONE || IsAnyGameStickInput(i) != StickType.NONE)
            {
                return i;
            }
        }
        return -1;
    }

    public ButtonType IsAnyGamePadInput(int padID)
    {
        for (int i = 0; i < Enum.GetNames(typeof(ButtonType)).Length; i++)
        {
            if (IsGamepadButtonPressed((ButtonType)i, padID))
            {
                return (ButtonType)i;
            }
        }

        return ButtonType.NONE;
    }

    public StickType IsAnyGameStickInput(int padID)
    {
        for (int j = 0; j < Enum.GetNames(typeof(StickType)).Length; j++)
        {
            if (GetGamepadStick((StickType)j, padID) != Vector2.zero)
            {
                return (StickType)j;
            }
        }
        return StickType.NONE;
    }

    public ButtonControl GetGamepadButton(ButtonType button, int padID)
    {
        Gamepad pad = (padID < 0 || padID >= Gamepad.all.Count) ? Gamepad.current : Gamepad.all[padID];

        if (pad == null)
            return null;

        switch (button)
        {
            case ButtonType.NORTH: { return pad.buttonNorth; }
            case ButtonType.SOUTH: { return pad.buttonSouth; }
            case ButtonType.EAST: { return pad.buttonEast; }
            case ButtonType.WEST: { return pad.buttonWest; }
            case ButtonType.START: { return pad.startButton; }
            case ButtonType.SELECT: { return pad.selectButton; }
            case ButtonType.LT: { return pad.leftTrigger; }
            case ButtonType.LB: { return pad.leftShoulder; }
            case ButtonType.LS: { return pad.leftStickButton; }
            case ButtonType.RT: { return pad.rightTrigger; }
            case ButtonType.RB: { return pad.rightShoulder; }
            case ButtonType.RS: { return pad.rightStickButton; }
            case ButtonType.UP: { return pad.dpad.up; }
            case ButtonType.DOWN: { return pad.dpad.down; }
            case ButtonType.LEFT: { return pad.dpad.left; }
            case ButtonType.RIGHT: { return pad.dpad.right; }

            default:
                return null;
        }
    }

    public Vector2 GetGamepadStick(StickType stick, int padID)
    {
        Gamepad pad = (padID < 0 || padID >= Gamepad.all.Count) ? Gamepad.current : Gamepad.all[padID];

        if (pad == null)
            return Vector2.zero;

        switch (stick)
        {
            case StickType.LEFT: { return new Vector2(pad.leftStick.x.ReadValue(), pad.leftStick.y.ReadValue()); }
            case StickType.RIGHT: { return new Vector2(pad.rightStick.x.ReadValue(), pad.rightStick.y.ReadValue()); }

            default:
                return Vector2.zero;
        }
    }
    
    public bool IsGamepadButtonDown(ButtonType button, int padID)
    {
        ButtonControl control = GetGamepadButton(button, padID);

        if (control == null)
            return false;

        return control.wasPressedThisFrame;
    }

    public bool IsGamepadButtonUp(ButtonType button, int padID)
    {
        ButtonControl control = GetGamepadButton(button, padID);

        if (control == null)
            return false;

        return control.wasReleasedThisFrame;
    }

    public bool IsGamepadButtonPressed(ButtonType button, int padID)
    {
        ButtonControl control = GetGamepadButton(button, padID);

        if (control == null)
            return false;

        return control.isPressed;
    }

    public bool IsGamepadButtonReleased(ButtonType button, int padID)
    {
        ButtonControl control = GetGamepadButton(button, padID);

        if (control == null)
            return false;

        return !(control.isPressed);
    }

    public Sprite GetGamepadSprite(ButtonType _button)
    {
        switch (_button)
        {
            case ButtonType.NORTH:  {return m_north; }
            case ButtonType.SOUTH:  { return m_south; }
            case ButtonType.EAST:   { return m_east; }
            case ButtonType.WEST:   { return m_west; }
                                      
            case ButtonType.START:  { return m_start; }
            case ButtonType.SELECT: { return m_select; }
                                      
            case ButtonType.LT:     { return m_leftTrigger; }
            case ButtonType.LB:     { return m_leftBumper; }
            case ButtonType.LS:     { return m_leftStick; }
                                      
            case ButtonType.RT:     { return m_rightTrigger; }
            case ButtonType.RB:     { return m_rightBumper; }
            case ButtonType.RS:     { return m_rightStick; }
                                      
            case ButtonType.UP:     { return m_dpadUp; }
            case ButtonType.DOWN:   { return m_dpadDown; }
            case ButtonType.LEFT:   { return m_dpadLeft; }
            case ButtonType.RIGHT:  { return m_dpadRight; }

            default:
            case ButtonType.NONE:   { return null; }
        }
    }

    public Sprite GetGameStickSprite(StickType _stick)
    {
        switch (_stick)
        {
            case StickType.LEFT: { return m_leftStickMove; }
            case StickType.RIGHT: { return m_rightStickMove; }
            
            default:
            case StickType.NONE: { return null; }
        }
    }
    #endregion

    #region Keyboard

        public KeyType IsAnyKeyPressed()
    {
        foreach (var keyType in Enum.GetValues(typeof(KeyType)).Cast<KeyType>())
        {
            if (IsKeyPressed(keyType))
            {
                return keyType;
            }
        }
        return KeyType.NONE;
    }

    public KeyType IsAnyKeyDown()
    {
        foreach (var keyType in Enum.GetValues(typeof(KeyType)).Cast<KeyType>())
        {
            if(IsKeyDown(keyType))
            {
                return keyType;
            }
        }
        return KeyType.NONE;
    }

    public MouseButton IsAnyMouseButtonDown()
    {
        foreach (var mouseButton in Enum.GetValues(typeof(MouseButton)).Cast<MouseButton>())
        {
            if (IsMouseButtonDown(mouseButton))
            {
                return mouseButton;
            }
        }
        return MouseButton.NONE;
    }

    public KeyControl GetKey(KeyType key)
    {
        switch (key)
        {
            case KeyType.Q: { return keyboard.qKey; }
            case KeyType.W: { return keyboard.wKey; }
            case KeyType.E: { return keyboard.eKey; }
            case KeyType.R: { return keyboard.rKey; }
            case KeyType.T: { return keyboard.tKey; }
            case KeyType.Y: { return keyboard.yKey; }
            case KeyType.U: { return keyboard.uKey; }
            case KeyType.I: { return keyboard.iKey; }
            case KeyType.O: { return keyboard.oKey; }
            case KeyType.P: { return keyboard.pKey; }

            case KeyType.A: { return keyboard.aKey; }
            case KeyType.S: { return keyboard.sKey; }
            case KeyType.D: { return keyboard.dKey; }
            case KeyType.F: { return keyboard.fKey; }
            case KeyType.G: { return keyboard.gKey; }
            case KeyType.H: { return keyboard.hKey; }
            case KeyType.J: { return keyboard.jKey; }
            case KeyType.K: { return keyboard.kKey; }
            case KeyType.L: { return keyboard.lKey; }

            case KeyType.Z: { return keyboard.zKey; }
            case KeyType.X: { return keyboard.xKey; }
            case KeyType.C: { return keyboard.cKey; }
            case KeyType.V: { return keyboard.vKey; }
            case KeyType.B: { return keyboard.bKey; }
            case KeyType.N: { return keyboard.nKey; }
            case KeyType.M: { return keyboard.mKey; }

            case KeyType.NUM_ONE: { return keyboard.numpad1Key; }
            case KeyType.NUM_TWO: { return keyboard.numpad2Key; }
            case KeyType.NUM_THREE: { return keyboard.numpad3Key; }
            case KeyType.NUM_FOUR: { return keyboard.numpad4Key; }
            case KeyType.NUM_FIVE: { return keyboard.numpad5Key; }
            case KeyType.NUM_SIX: { return keyboard.numpad6Key; }
            case KeyType.NUM_SEVEN: { return keyboard.numpad7Key; }
            case KeyType.NUM_EIGHT: { return keyboard.numpad8Key; }
            case KeyType.NUM_NINE: { return keyboard.numpad9Key; }
            case KeyType.NUM_ZERO: { return keyboard.numpad0Key; }

            case KeyType.ALP_ONE: { return keyboard.digit1Key; }
            case KeyType.ALP_TWO: { return keyboard.digit2Key; }
            case KeyType.ALP_THREE: { return keyboard.digit3Key; }
            case KeyType.ALP_FOUR: { return keyboard.digit4Key; }
            case KeyType.ALP_FIVE: { return keyboard.digit5Key; }
            case KeyType.ALP_SIX: { return keyboard.digit6Key; }
            case KeyType.ALP_SEVEN: { return keyboard.digit7Key; }
            case KeyType.ALP_EIGHT: { return keyboard.digit8Key; }
            case KeyType.ALP_NINE: { return keyboard.digit9Key; }
            case KeyType.ALP_ZERO: { return keyboard.digit0Key; }

            case KeyType.L_SHIFT: { return keyboard.leftShiftKey; }
            case KeyType.L_CTRL: { return keyboard.leftCtrlKey; }
            case KeyType.L_ALT: { return keyboard.leftAltKey; }
            case KeyType.TAB: { return keyboard.tabKey; }
            case KeyType.ESC: { return keyboard.escapeKey; }
            case KeyType.SPACE: { return keyboard.spaceKey; }

            case KeyType.R_SHIFT: { return keyboard.rightShiftKey; }
            case KeyType.R_CTRL: { return keyboard.rightCtrlKey; }
            case KeyType.R_ALT: { return keyboard.rightAltKey; }
            case KeyType.ENTER: { return keyboard.endKey; }
            case KeyType.TILDE: { return keyboard.backquoteKey; }
            case KeyType.UP: { return keyboard.upArrowKey; }
            case KeyType.LEFT: { return keyboard.leftArrowKey; }
            case KeyType.RIGHT: { return keyboard.rightArrowKey; }
            case KeyType.DOWN: { return keyboard.downArrowKey; }
            default:
                return null;
        }
    }

    public bool IsKeyDown(KeyType key)
    {
        if(key == KeyType.NONE)
            return false;

        return GetKey(key).wasPressedThisFrame;
    }

    public bool IsKeyUp(KeyType key)
    {
        if (key == KeyType.NONE)
            return false;

        return GetKey(key).wasReleasedThisFrame;
    }

    public bool IsKeyPressed(KeyType key)
    {
        if (key == KeyType.NONE)
            return false;

        return GetKey(key).isPressed;
    }
    public bool IsKeyReleased(KeyType key)
    {
        if (key == KeyType.NONE)
            return false;

        return !(GetKey(key).isPressed);
    }

    public string GetKeyString(KeyType key)
    {
        switch (key)
        {
            case KeyType.Q: { return "Q"; }
            case KeyType.W: { return "W"; }
            case KeyType.E: { return "E"; }
            case KeyType.R: { return "R"; }
            case KeyType.T: { return "T"; }
            case KeyType.Y: { return "Y"; }
            case KeyType.U: { return "U"; }
            case KeyType.I: { return "I"; }
            case KeyType.O: { return "O"; }
            case KeyType.P: { return "P"; }
                              
            case KeyType.A: { return "A"; }
            case KeyType.S: { return "S"; }
            case KeyType.D: { return "D"; }
            case KeyType.F: { return "F"; }
            case KeyType.G: { return "G"; }
            case KeyType.H: { return "H"; }
            case KeyType.J: { return "J"; }
            case KeyType.K: { return "K"; }
            case KeyType.L: { return "L"; }
                              
            case KeyType.Z: { return "Z"; }
            case KeyType.X: { return "X"; }
            case KeyType.C: { return "C"; }
            case KeyType.V: { return "V"; }
            case KeyType.B: { return "B"; }
            case KeyType.N: { return "N"; }
            case KeyType.M: { return "M"; }

            case KeyType.ALP_ONE:   { return "1"; }
            case KeyType.ALP_TWO:   { return "2"; }
            case KeyType.ALP_THREE: { return "3"; }
            case KeyType.ALP_FOUR:  { return "4"; }
            case KeyType.ALP_FIVE:  { return "5"; }
            case KeyType.ALP_SIX:   { return "6"; }
            case KeyType.ALP_SEVEN: { return "7"; }
            case KeyType.ALP_EIGHT: { return "8"; }
            case KeyType.ALP_NINE:  { return "9"; }
            case KeyType.ALP_ZERO:  { return "0"; }
                                      
            case KeyType.NUM_ONE:   { return "NUM_1"; }
            case KeyType.NUM_TWO:   { return "NUM_2"; }
            case KeyType.NUM_THREE: { return "NUM_3"; }
            case KeyType.NUM_FOUR:  { return "NUM_4"; }
            case KeyType.NUM_FIVE:  { return "NUM_5"; }
            case KeyType.NUM_SIX:   { return "NUM_6"; }
            case KeyType.NUM_SEVEN: { return "NUM_7"; }
            case KeyType.NUM_EIGHT: { return "NUM_8"; }
            case KeyType.NUM_NINE:  { return "NUM_9"; }
            case KeyType.NUM_ZERO:  { return "NUM_0"; }
                                      
            case KeyType.L_SHIFT:   { return "L Shift"; }
            case KeyType.L_CTRL:    { return "L Ctrl";  }
            case KeyType.L_ALT:     { return "L Alt"; }
                                      
            case KeyType.TAB:       { return "Tab";  }
            case KeyType.ESC:       { return "Esc";  }
            case KeyType.SPACE:     { return "Space"; }
                                      
            case KeyType.R_SHIFT:   { return "R Shift"; }
            case KeyType.R_CTRL:    { return "R Ctrl"; }
            case KeyType.R_ALT:     { return "R Alt"; }
                                      
            case KeyType.ENTER:     { return "Enter"; }

            default:
            
            case KeyType.TILDE:{ return "Tilde"; }
            case KeyType.NONE: { return ""; }
        }
    }
    public Sprite GetKeyImage(KeyType key)
    {
        switch (key)
        {
            case KeyType.Q: { return null; }
            case KeyType.W: { return null; }
            case KeyType.E: { return null; }
            case KeyType.R: { return null; }
            case KeyType.T: { return null; }
            case KeyType.Y: { return null; }
            case KeyType.U: { return null; }
            case KeyType.I: { return null; }
            case KeyType.O: { return null; }
            case KeyType.P: { return null; }
                                     
            case KeyType.A: { return null; }
            case KeyType.S: { return null; }
            case KeyType.D: { return null; }
            case KeyType.F: { return null; }
            case KeyType.G: { return null; }
            case KeyType.H: { return null; }
            case KeyType.J: { return null; }
            case KeyType.K: { return null; }
            case KeyType.L: { return null; }
                                     
            case KeyType.Z: { return null; }
            case KeyType.X: { return null; }
            case KeyType.C: { return null; }
            case KeyType.V: { return null; }
            case KeyType.B: { return null; }
            case KeyType.N: { return null; }
            case KeyType.M: { return null; }

            case KeyType.ALP_ONE:   { return null; }
            case KeyType.ALP_TWO:   { return null; }
            case KeyType.ALP_THREE: { return null; }
            case KeyType.ALP_FOUR:  { return null; }
            case KeyType.ALP_FIVE:  { return null; }
            case KeyType.ALP_SIX:   { return null; }
            case KeyType.ALP_SEVEN: { return null; }
            case KeyType.ALP_EIGHT: { return null; }
            case KeyType.ALP_NINE:  { return null; }
            case KeyType.ALP_ZERO:  { return null; }

            case KeyType.NUM_ONE:   { return null; }
            case KeyType.NUM_TWO:   { return null; }
            case KeyType.NUM_THREE: { return null; }
            case KeyType.NUM_FOUR:  { return null; }
            case KeyType.NUM_FIVE:  { return null; }
            case KeyType.NUM_SIX:   { return null; }
            case KeyType.NUM_SEVEN: { return null; }
            case KeyType.NUM_EIGHT: { return null; }
            case KeyType.NUM_NINE:  { return null; }
            case KeyType.NUM_ZERO:  { return null; }

            case KeyType.L_SHIFT:   { return m_lShift; }
            case KeyType.L_CTRL:    { return m_lCtrl; }
            case KeyType.L_ALT:     { return m_lAlt; }

            case KeyType.TAB:   { return m_tab; }
            case KeyType.ESC:   { return m_escape; }
            case KeyType.SPACE: { return m_space; }

            case KeyType.R_SHIFT:  { return m_rShift; }
            case KeyType.R_CTRL:   { return m_rCtrl; }
            case KeyType.R_ALT: { return m_rAlt; }

            case KeyType.ENTER: { return m_enter; }

            default:

            case KeyType.TILDE: { return m_tilde; }
            case KeyType.NONE: { return null; }
        }
    }
    #endregion

    #region Mouse

    public bool IsAnyMousePressed()
    {
        return mouse.press.isPressed;
    }

    public ButtonControl GetMouseButton(MouseButton button)
    {
        switch (button)
        {
            case MouseButton.LEFT:      { return mouse.leftButton; }
            case MouseButton.MIDDLE:    { return mouse.middleButton; }
            case MouseButton.RIGHT:     { return mouse.rightButton; }
           
            default:
                return null;
        }
    }

    public bool IsMouseButtonDown(MouseButton button)
    {
        if (button == MouseButton.NONE)
            return false;

        return GetMouseButton(button).wasPressedThisFrame;
    }

    public bool IsMouseButtonUp(MouseButton button)
    {
        if (button == MouseButton.NONE)
            return false;

        return GetMouseButton(button).wasReleasedThisFrame;
    }

    public bool IsMouseButtonPressed(MouseButton button)
    {
        if (button == MouseButton.NONE)
            return false;

        return GetMouseButton(button).isPressed;
    }

    public bool IsMouseButtonReleased(MouseButton button)
    {
        if (button == MouseButton.NONE)
            return false;

        return !(GetMouseButton(button).isPressed);
    }

    /*
    * GetMousePositionInScreen by Michael Jordan
    * Description:
    *  Returns the position of the mouse where 0,0 is the bottem left of the screen.
    *
    * Return: 
    *  Vector2 - Mouse position
    */
    public Vector2 GetMousePositionInScreen()
    {
        //Convert to 0,0 middle
        //Vector3 screenPoint = new Vector2(mouse.position.x.ReadValue(), mouse.position.y.ReadValue());
        //new Vector2(screenPoint.x - Camera.main.scaledPixelWidth / 2, screenPoint.y - Camera.main.scaledPixelHeight / 2);
        
        return new Vector2(mouse.position.x.ReadValue(), mouse.position.y.ReadValue());
    }

    public Vector2 GetMouseDelta()
    {
        return new Vector2(mouse.delta.x.ReadValue(), mouse.delta.y.ReadValue());
    }

    public float GetMouseScrollDelta()
    {
        return mouse.scroll.y.ReadValue();
    }

    /// <summary>
    /// Checking if the mouse button is down
    /// </summary>
    /// <param name="button"></param>
    /// <returns></returns>
    public bool GetMouseDown(MouseButton button)
    {
        switch (button)
        {
            default:
                Debug.LogWarning($"Unsupported mouse button type in GetMouseDown.");
                return false;
            case MouseButton.LEFT:
                return mouse.leftButton.wasPressedThisFrame;

            case MouseButton.RIGHT:
                return mouse.rightButton.wasPressedThisFrame;
        }

    }
    /// <summary>
    /// Checking if the mouse button is pressed
    /// </summary>
    /// <param name="button"></param>
    /// <returns></returns>
    public bool GetMousePress(MouseButton button)
    {
        switch (button)
        {
            default:
                Debug.LogWarning($"Unsupported mouse button type in GetMouseDown.");
                return false;
            case MouseButton.LEFT:
                return mouse.leftButton.isPressed;

            case MouseButton.RIGHT:
                return mouse.rightButton.isPressed;
        }
    }

    public string GetMouseButtonString(MouseButton button)
    {
        switch (button)
        {
            case MouseButton.LEFT:      { return "L Click"; }
            case MouseButton.MIDDLE:    { return "M Click"; }
            case MouseButton.RIGHT:     { return "R Click"; }

            default:
            case MouseButton.NONE:{ return ""; }
        }
    }
    public Sprite GetMouseButtonSprite(MouseButton button)
    {
        switch (button)
        {
            case MouseButton.LEFT: { return m_leftClick; }
            case MouseButton.MIDDLE: { return null; }
            case MouseButton.RIGHT: { return m_rightClick; }

            default:
            case MouseButton.NONE: { return null; }
        }
    }
    #endregion
}
