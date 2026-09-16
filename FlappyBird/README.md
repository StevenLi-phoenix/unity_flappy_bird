![](./screenshot.png)

# Flappy Bird — Pocket Arcade

A playable Unity 6 recreation with procedural pixel art, scrolling pipes, gravity and flap movement, collision detection, score tracking, persistent best score, and instant restart.

Open `Build/Flappy Bird.app` to play. Press **Space** or **left click** to start, flap, or restart after a crash. Click the speaker icon or press **M** to toggle sound (saved between sessions). Drag a window edge to resize. Flaps, scores, and crashes have synthesized arcade sounds and pixel particles; the bird tilts with its velocity, scores pulse, and crashes produce a short impact flash. Rounded buttons and clouds sit directly in the game world; the title fades away on takeoff and the score card enters with a gentle overshoot, then fades out on retry.

To edit, open this folder in Unity 6000.6.0f1 and open `Assets/FlappyBird.unity`. Rendering uses Universal Render Pipeline (URP) 17.6.0 and Canvas UI 2.6.0. Controls use Input System 1.20.0 with the legacy Input Manager disabled.

To test and build, run the Unity editor with `-batchmode -nographics -projectPath <project-path> -executeMethod BuildGame.Build -quit`. The build entry point checks 97 audio, particle, input, rendering, flight, collision, animation, resizing, icon, and layout cases before generating the scene and producing the macOS app. A shared Canvas scale keeps text and artwork aligned. UI stays centered while the world extends to the window edges and the ground stays at the bottom. Pipes spawn and recycle beyond the actual visible edges.

Run the app with `--layout-check` for rendered regression screenshots at five actual window sizes, including portrait and ultrawide. Results are saved to `Build/LayoutChecks`; fixture scores do not change saved progress.

The app captures `Build/screenshot.png` after its first 100 frames. Art is drawn procedurally; Unity Package Manager restores URP and its dependencies from the checked-in package manifest and lockfile.

WebGL builds use `BuildGame.BuildWebGL` or `bash scripts/build-webgl.sh` from the repository root. Output is `Build/WebGL`, with an iframe-filling template and Gzip decompression fallback. Desktop screenshot writing is excluded from browser builds. See [the maintenance handoff](../docs/HANDOFF.md) for architecture, verification, and publishing details.
