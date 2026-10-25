using ET;
using UnityEngine;
using UnityEngine.UI;
using XEngine.UI;

namespace ET.Client
{
    [ComponentOf(typeof(UI))]
    public class UIMainComponent: Entity, IAwake,IDestroy
    {
        public Button m_btnBag;
        public JoystickCtrl m_joystick;
        public long LastJoystickMoveTime;
        public bool HasJoystickMoveDirection;
        public Vector2 LastJoystickValue;
        public Vector3 JoystickMoveDirection;
    }
}