************************************
*          CLOUD SHADOWS           *
*       Created by Krronnect       * 
*            README FILE           *
************************************


Quick help: how to use this asset?
----------------------------------

This asset will add simulated cloud shadows to your scene.

To use it just right click in the Hierarchy Window and select Effects -> Cloud Shadows,
or select from the Unity Editor top menu: GameObject -> Effects -> Cloud Shadows:
a gameobject with a Cloud Shadows script will be created which you can configure from the inspector.

The asset includes demo scenes for built-in and URP pipelines, as well as additional cloud textures in the demo folder.


Help & Support Forum
--------------------

Check the Documentation folder for detailed instructions:

Have any question or issue?
* Support-Web: https://kronnect.com/support
* Support-Discord: https://discord.gg/EH2GMaM
* Email: contact@kronnect.com
* Twitter: @Kronnect

If you like the asset, please rate it on the Asset Store. It encourages us to keep improving it! Thanks!



Future updates
--------------

All our assets follow an incremental development process by which a few beta releases are published on our support forum (kronnect.com).
We encourage you to signup and engage our forum. The forum is the primary support and feature discussions medium.

Of course, all updates of the asset will be eventually available on the Asset Store.



More Cool Assets!
-----------------
Check out our other assets here:
https://assetstore.unity.com/publishers/15018



Version history
---------------

3.0
- Added "Custom Bounds" option. Limits cloud shadows to a specific area.

2.5
- Added "Preserve Directional Shadows" option (URP only)

2.4.1
- Improved shadow rendering on already shadowed areas

2.4
- Added "GetShadowAttenuation()" function in CloudShadowsLibrary.hlsl for easier integration with other shaders

2.3
- Added "Render Queue" setting

2.2
- Added "Shadow Min Altitude" option. Use this new parameter to prevent cloud shadows rendering under certain altitude, for example, under water level.

2.1
- Support for orthographic camera

2.0
- New "Coverage Mask" feature which prevents shadows on specific places
- Added coverage mask painter option in the cloud shadows inspector

1.0.1
- [Fix] Fixed shadows rendering issue related to occlusion culling and fov

Sep/2023
- First version
