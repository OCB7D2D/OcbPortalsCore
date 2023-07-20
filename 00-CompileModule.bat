@echo off

call MC7D2D PortalsCore.dll ^
/reference:"%PATH_7D2D_MANAGED%\Assembly-CSharp.dll" ^
	Harmony\*.cs Source\*cs Utils\*.cs && ^
echo Successfully compiled PortalsCore.dll

pause