@echo off

IF NOT EXIST "%1" GOTO NOTARGETDIR


SET TARGET=%1

mklink /d Common.Core %TARGET%\src\Common.Core
mklink /d Ecs.Core %TARGET%\src\Ecs.Core
mklink /d Game.Simulation.Client %TARGET%\src\Game.Simulation.Client 
mklink /d Game.Simulation.Core %TARGET%\src\Game.Simulation.Core 
mklink /d Networking.Core %TARGET%\src\Networking.Core 
mklink /d VolatilePhysics %TARGET%\src\VolatilePhysics 

goto DONE

:NOTARGETDIR

echo Target directory does not exist

:DONE