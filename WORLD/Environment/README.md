# FM0 static daylight
Sky3D 2.1 owns the atmosphere and sun in environment.tscn. Fixed 13:30, time progression/clouds/screen-space Sky3D fog disabled. daylight.gd applies modest native distance haze and SSAO after Sky3D's internal initialization, avoiding its early environment setter dependency. Environment values live in this small wrapper and the owning scene; no weather/day-cycle manager.

Sun shadow distance is 100m with blended splits; LOD3 never casts. Adjust sun direction/energy, ambient energy and haze after eye-level visual review. Shader appearance, shadow quality, distant haze and GPU performance require local Forward+ testing. Sky3D remains an external addon; its upstream deprecation warning is not suppressed.
