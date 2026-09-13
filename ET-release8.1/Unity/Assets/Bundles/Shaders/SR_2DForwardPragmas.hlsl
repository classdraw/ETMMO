#ifndef SR_2D_FORWARD_PRAGMAS_INCLUDED
#define SR_2D_FORWARD_PRAGMAS_INCLUDED

// 2D top-down forward pass: SH-only lighting, runtime cloud/fake-light toggles.
// Omit lightmap / cloud-3D / coverage / bounds multi_compile to reduce variant count.
#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
#pragma multi_compile _ _SSCS_RECEIVE
#pragma multi_compile _ _FAKE_ADDITIONAL_LIGHTS

#endif
