This is a Unity 5 project for visualizing code as blocks in 3D space.
It's meant to be used with my codeanalyzer Java project, which analyzes the
code and produces the textures which are used to create, size, and decorate
the 3D blocks.

This is so utterly a work-in-progress and code is not clean and not expected
to be for quite a while.  The path to the texture images lives outside
the Unity project folder, which means it won't even "build and run" as codevis.app
because the textures come up missing due to application sandboxing or something.
Works fine interactively in Unity though.  Note that the path is hardcoded.

This was created on Mac Yosemite and may not cooperate fully on Windows.

