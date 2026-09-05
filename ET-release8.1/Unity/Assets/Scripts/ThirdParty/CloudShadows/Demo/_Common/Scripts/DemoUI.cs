using UnityEngine;
using SSCS;
using UnityEngine.UI;

namespace SSCS_Demos {

    public class DemoUI : MonoBehaviour {
        public CloudShadows cloudShadows;
        public Toggle toggleClouds, toggleShadows;

        public void UpdateSettings() {
            cloudShadows.profile.showClouds = toggleClouds.isOn;
            cloudShadows.profile.showShadows = toggleShadows.isOn;

        }

    }
}
