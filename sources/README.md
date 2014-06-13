>> Directory Layout <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

build

    Contains Visual Studio projects and build related source files for active 
    projects.

doc

    Documentation project and source files.

legacy

    Older projects that are no longer being actively worked on.

src

    Source for active projects.

    The source directory layout is a bit odd.  Most C# source is located
    under  /src/main/Assets/CAI.  This layout mimics the layout within
    a Unity project.  It is structured in this way to support easy source
    deployment to non-Windows Unity projects which can't use the pre-compiled
    libraries.

>> .NET Notes <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

No automatic builds or packaging.
Sandcastle for documentation build.

DOCUMENTATION BUILD NOTES

The following tools are required to build the API documentation:

Sandcastle: http://sandcastle.codeplex.com/
Sandcastle Help File Builder: http://shfb.codeplex.com/

Sandcastle Styles Patch

http://sandcastlestyles.codeplex.com/ - Home Page
http://sandcastlestyles.codeplex.com/releases/view/47767 - Patch Used

>> Unity Notes <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

Libraries and namespaces containing 'u3d' depend on Unity Pro 
(http://unity3d.com/). The free version of unity is not supported.

The Visual Studio projects were created on Windows 64-bit, so the Unity DLL 
reference will be broken on 32-bit Windows.

For Unity 3.x, the normal location of the DLL for both operating systems is 
as follows:

Windows 32-bit: C:\Program Files\Unity\Editor\Data\Managed
Windows 64-bit: C:\Program Files (x86)\Unity\Editor\Data\Managed\


