using System;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using TEngine;

namespace ET.Client
{
    [ComponentOf(typeof(Scene))]
    public class CameraPlayComponent : Entity, IAwake, IDestroy
    {
        public string MainCameraPath = "Assets/Bundles/Tools/MainCamera.prefab";
        public string CameraRootPath = "Assets/Bundles/Tools/CameraRoot.prefab";
        public GameObject MainCameraObj;
        public GameObject CameraRootObj;

        public CinemachineFreeLook CinemachineFreeLook;//后面可能会有很多

        public GameObject PlayerObject;//主角

    }
}