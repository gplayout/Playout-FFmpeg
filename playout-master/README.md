Playout compile guideance:

Tool: Visual Studio 2013

1- Install .Net Framework 4  
2- Install Microsoft Windows SDK for Windows 7 from the following link:  
https://www.microsoft.com/en-us/download/details.aspx?id=8279  
3- Open directshow/pushsource.sln in VS2013  
4- Set Directshow 7 libraries paths in pushsource's projects settings.  
5- Compile and build pushsource.sln  
6- Open Playout/playout.sln in Visual Studio  
7- Compile and build playout solution  
8- Copy pushsource/Debug/HamedSource.ax to playout/playout.ui/bin/debug/  
9- Copy pushsource/Debug/HamedDecklink.ax to playout/playout.ui/bin/debug/  
10- Copy pushsource/Debug/HamedVirtualCamera.ax,HamedVirtualSound.ax to playout/playout.ui/bin/debug/VCam  
11- Install all *.ax files with RegSvr32.exe in Windows Command Prompt  
12- Run playout/Playout.ui project  