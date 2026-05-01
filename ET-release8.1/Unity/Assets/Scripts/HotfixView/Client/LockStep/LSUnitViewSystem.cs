using System;
using TrueSync;
using UnityEngine;

namespace ET.Client
{
    [EntitySystemOf(typeof(LSUnitView))]
    [LSEntitySystemOf(typeof(LSUnitView))]
    [FriendOf(typeof(LSUnitView))]
    public static partial class LSUnitViewSystem
    {
        [EntitySystem]
        private static void Awake(this LSUnitView self, GameObject go)
        {
            self.GameObject = go;
            self.Transform = go.transform;

        }

        [LSEntitySystem]
        private static void LSRollback(this LSUnitView self)
        {
            LSUnit unit = self.GetUnit();
            Vector3 p = unit.Position.ToVector();
            Quaternion r = unit.Rotation.ToQuaternion();
            self.Transform.position = p;
            self.Transform.rotation = r;
            self.Position = p;
            self.Rotation = r;
            self.t = 0;
            self.totalTime = 0;
        }

        [EntitySystem]
        private static void Update(this LSUnitView self)
        {
            LSUnit unit = self.GetUnit();

            Vector3 unitPos = unit.Position.ToVector();
            Quaternion unitRot = unit.Rotation.ToQuaternion();
            const float speed = 6f;
            float speed2 = speed;// * self.Room().SpeedMultiply

            if (unitPos != self.Position)
            {
                float distance = (unitPos - self.Position).magnitude;
                self.totalTime = Mathf.Max(distance / speed2, 1e-5f);
                self.t = 0;
                self.Position = unitPos;
                self.Rotation = unitRot;
            }
            else if (Quaternion.Angle(self.Rotation, unitRot) > 0.1f)
            {
                self.Rotation = unitRot;
                self.t = 0;
            }

            LSInput input = unit.GetComponent<LSInputComponent>().LSInput;
            if (input.V != TSVector2.zero)
            {
                self.GetComponent<LSAnimatorComponent>().SetFloatValue("Speed", speed2);
            }
            else
            {
                self.GetComponent<LSAnimatorComponent>().SetFloatValue("Speed", 0);
            }
            self.t += Time.deltaTime;
            self.Transform.rotation = Quaternion.Lerp(self.Transform.rotation, self.Rotation, Mathf.Clamp01(self.t / 1f));
            float posAlpha = self.totalTime > 1e-5f ? Mathf.Clamp01(self.t / self.totalTime) : 1f;
            self.Transform.position = Vector3.Lerp(self.Transform.position, self.Position, posAlpha);
        }

        private static LSUnit GetUnit(this LSUnitView self)
        {
            LSUnit unit = self.Unit;
            if (unit != null)
            {
                return unit;
            }

            self.Unit = (self.IScene as Room).LSWorld.GetComponent<LSUnitComponent>().GetChild<LSUnit>(self.Id);
            return self.Unit;
        }
    }
}