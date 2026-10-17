#ifndef SR_2D_FORWARD_PRAGMAS_INCLUDED
#define SR_2D_FORWARD_PRAGMAS_INCLUDED

// 2D Forward：实时光（主光 + ShadowMap）与 Scene Shadow RT 叠加；云阴影 / FakeLight 可选。
#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
#pragma multi_compile_fragment _ _SHADOWS_SOFT
#pragma multi_compile _ _SSCS_RECEIVE
#pragma multi_compile _ _FAKE_ADDITIONAL_LIGHTS
#pragma shader_feature_local _ _RECEIVE_SHADOWS_OFF

#endif
