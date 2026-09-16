![](./FlappyBird/screenshot.png)

# Flappy Bird — Pocket Arcade

A Unity arcade game with responsive Canvas UI, procedural art, synthesized sound and particles.

Space or click to flap and restart; M or the speaker toggles sound.

- [Project and local build instructions](./FlappyBird/README.md)
- [Maintenance handoff and lessons learned](./docs/HANDOFF.md)
- CI runs release-validator tests on pushes and pull requests.
- WebGL: `bash scripts/build-webgl.sh` (Unity 6000.6.0f1 with WebGL support).

[Play in your browser](https://stevenli-phoenix-work.itch.io/flappy-bird).

Main-branch pushes trigger the GitHub-hosted Unity WebGL build and itch.io publishing workflow. Automatic publishing still requires Unity activation secrets and a refreshed butler publishing token; see the [setup status](./docs/HANDOFF.md#setup-status-2026-09-15).
