using ET;
using GameLogic;
using UnityEngine;
using UnityEngine.UI;
using XEngine.UI;


namespace ET.Client
{
    [EntitySystemOf(typeof(UIMainComponent))]
    [FriendOf(typeof(UIMainComponent))]
    [FriendOf(typeof(CameraPlayComponent))]
    public static partial class UIMainComponentSystem
    {
        private const float JoystickMoveDistance = 2f;
        private const float JoystickDeadZoneSqr = 0.04f;
        private const float JoystickDirectionKeepAngle = 5f;
        private const long JoystickMoveInterval = 200;
        [EntitySystem]
        private static void Awake(this UIMainComponent self)
        {
            UIBindComponent m_bindComponent = self.GetParent<UI>().GameObject.GetComponent<UIBindComponent>();
            self.m_sliderHeadHP = m_bindComponent.GetComponent<Slider>(0);
            self.m_sliderHeadMP = m_bindComponent.GetComponent<Slider>(1);
            self.m_sliderHP = m_bindComponent.GetComponent<Slider>(2);
            self.m_sliderMP = m_bindComponent.GetComponent<Slider>(3);
            self.m_btnBag = m_bindComponent.GetComponent<Button>(4);
            self.m_btnBag.onClick.AddListener(() => { self.OnBag(); });

            self.m_joystick = self.GetParent<UI>().GameObject.GetComponentInChildren<JoystickCtrl>(true);
            if (self.m_joystick == null)
            {
                Log.Error("UIMain 未找到 JoystickCtrl");
                return;
            }

            self.m_joystick.SetPointerDownAction(value => { self.OnJoystickPointerDown(value); });
            self.m_joystick.SetValueChangeAction(value => { self.OnJoystickValueChanged(value); });
            self.m_joystick.SetPointerUpAction(value => { self.OnJoystickPointerUp(value); });

            self.RefreshHpAndSp();
        }
        
        [EntitySystem]
        private static void Destroy(this UIMainComponent self)
        {
            if (self.m_joystick == null)
            {
                return;
            }

            self.m_joystick.SetPointerDownAction(null);
            self.m_joystick.SetValueChangeAction(null);
            self.m_joystick.SetPointerUpAction(null);
            self.m_joystick = null;
        }

        /// <summary>根据主角数值刷新头像血蓝条与底部血蓝条。</summary>
        public static void RefreshHpAndSp(this UIMainComponent self)
        {
            if (self == null || self.IsDisposed)
            {
                return;
            }

            Scene currentScene = self.Root().CurrentScene();
            if (currentScene == null)
            {
                return;
            }

            Unit myUnit = UnitHelper.GetMyUnitFromCurrentScene(currentScene);
            if (myUnit == null || myUnit.IsDisposed)
            {
                return;
            }

            NumericComponent numericComponent = myUnit.GetComponent<NumericComponent>();
            if (numericComponent == null)
            {
                return;
            }

            long hp = numericComponent[NumericType.Hp];
            long sp = numericComponent[NumericType.Sp];
            long maxHp = GetDisplayMax(numericComponent, NumericType.MaxHp, NumericType.MaxHpBase, hp);
            long maxSp = GetDisplayMax(numericComponent, NumericType.MaxSp, NumericType.MaxSpBase, sp);
            ApplyHpAndSpToSliders(self, hp, sp, maxHp, maxSp);
        }

        public static void RefreshHpMpSlider(this UIMainComponent self) => self.RefreshHpAndSp();

        private static void ApplyHpAndSpToSliders(UIMainComponent self, long hp, long sp, long maxHp, long maxSp)
        {
            float hpRatio = maxHp > 0 ? hp / (float)maxHp : 0f;
            float spRatio = maxSp > 0 ? sp / (float)maxSp : 0f;
            SetRatioSlider(self.m_sliderHeadHP, hpRatio);
            SetRatioSlider(self.m_sliderHeadMP, spRatio);
            SetResourceSlider(self.m_sliderHP, hp, maxHp);
            SetResourceSlider(self.m_sliderMP, sp, maxSp);
        }

        private static void SetRatioSlider(Slider slider, float ratio)
        {
            if (slider == null)
            {
                return;
            }

            slider.minValue = 0;
            slider.maxValue = 1;
            slider.wholeNumbers = false;
            slider.value = Mathf.Clamp01(ratio);
        }

        private static long GetDisplayMax(NumericComponent numeric, int maxFinal, int maxBase, long current)
        {
            long max = numeric[maxFinal];
            if (max > 0)
            {
                return max;
            }

            max = numeric.GetByKey(maxBase);
            if (max > 0)
            {
                return max;
            }

            return current > 0 ? current : 1;
        }

        private static void SetResourceSlider(Slider slider, long current, long max)
        {
            if (slider == null)
            {
                return;
            }

            long safeMax = max > 0 ? max : 1;
            float targetValue = Mathf.Clamp(current, 0, safeMax);

            slider.minValue = 0;
            slider.maxValue = safeMax;
            slider.wholeNumbers = true;
            // 先归零再赋值，否则仅改 max 时 FillRect 可能不刷新
            slider.value = 0;
            slider.value = targetValue;
        }

        public static void OnBag(this UIMainComponent self)
        {
            Scene scene = self.Root().CurrentScene();
            if (scene == null)
            {
                Log.Error("打开背包失败：未进入地图场景");
                return;
            }

            UIBagHelper.Open(scene);
        }

        public static void OnJoystickPointerDown(this UIMainComponent self, Vector2 value)
        {
            //Log.Info($"Joystick PointerDown: {value}");
        }

        public static void OnJoystickValueChanged(this UIMainComponent self, Vector2 value)
        {
            if (value.sqrMagnitude < JoystickDeadZoneSqr)
            {
                self.StopJoystickMove();
                return;
            }

            Vector3 move = self.GetJoystickMoveDirection(value);
            if (move.sqrMagnitude < JoystickDeadZoneSqr)
            {
                self.StopJoystickMove();
                return;
            }

            move.Normalize();
            long now = TimeInfo.Instance.ClientNow();
            bool directionChanged = !self.HasJoystickMoveDirection ||
                    Vector3.Angle(self.JoystickMoveDirection, move) > JoystickDirectionKeepAngle;
            if (directionChanged)
            {
                self.JoystickMoveDirection = move;
                self.LastJoystickValue = value;
                self.HasJoystickMoveDirection = true;
                self.SendJoystickMoveTarget(now);
                return;
            }

            if (now - self.LastJoystickMoveTime < JoystickMoveInterval)
            {
                return;
            }

            self.SendJoystickMoveTarget(now);
        }

        private static void SendJoystickMoveTarget(this UIMainComponent self, long now)
        {
            Unit myUnit = UnitHelper.GetMyUnitFromCurrentScene(self.Root().CurrentScene());
            if (myUnit == null || myUnit.IsDisposed || myUnit.IsCasting() || myUnit.IsForbidMove())
            {
                return;
            }

            Vector3 target = (Vector3)myUnit.Position + self.JoystickMoveDirection * JoystickMoveDistance;
            target.y = myUnit.Position.y;

            self.LastJoystickMoveTime = now;
            C2M_PathfindingResult c2MPathfindingResult = C2M_PathfindingResult.Create();
            c2MPathfindingResult.Position = target;
            self.Root().GetComponent<ClientSenderComponent>().Send(c2MPathfindingResult);
        }

        private static void ClearJoystickMoveDirection(this UIMainComponent self)
        {
            self.HasJoystickMoveDirection = false;
            self.LastJoystickValue = Vector2.zero;
            self.JoystickMoveDirection = Vector3.zero;
        }

        /// <summary>摇杆向上对应画面上方，向右对应画面右方，投影到地面。</summary>
        private static Vector3 GetJoystickMoveDirection(this UIMainComponent self, Vector2 value)
        {
            Vector3 forward = Vector3.forward;
            Vector3 right = Vector3.right;
            Camera camera = self.GetPlayCamera();
            if (camera != null)
            {
                forward = camera.transform.forward;
                right = camera.transform.right;
                forward.y = 0f;
                right.y = 0f;
            }

            if (forward.sqrMagnitude < 0.0001f)
            {
                forward = Vector3.forward;
            }
            else
            {
                forward.Normalize();
            }

            if (right.sqrMagnitude < 0.0001f)
            {
                right = Vector3.right;
            }
            else
            {
                right.Normalize();
            }

            return right * value.x + forward * value.y;
        }

        private static Camera GetPlayCamera(this UIMainComponent self)
        {
            CameraPlayComponent cameraPlay = self.Root().GetComponent<CameraPlayComponent>();
            if (cameraPlay != null && cameraPlay.MainCameraObj != null)
            {
                Camera playCamera = cameraPlay.MainCameraObj.GetComponent<Camera>();
                if (playCamera != null)
                {
                    return playCamera;
                }
            }

            return Camera.main;
        }

        public static void OnJoystickPointerUp(this UIMainComponent self, Vector2 value)
        {
            self.StopJoystickMove();
        }

        private static void StopJoystickMove(this UIMainComponent self)
        {
            if (!self.HasJoystickMoveDirection)
            {
                return;
            }

            self.ClearJoystickMoveDirection();
            C2M_Stop c2MStop = C2M_Stop.Create();
            self.Root().GetComponent<ClientSenderComponent>().Send(c2MStop);
        }
    }
}