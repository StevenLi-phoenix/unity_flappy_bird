#!/usr/bin/env bash
set -euo pipefail
repo_root="$(cd "$(dirname "$0")/.." && pwd)"
unity_editor="${UNITY_EDITOR:-/Applications/Unity/Hub/Editor/6000.6.0f1-arm64/Unity.app/Contents/MacOS/Unity}"
build_log="${RUNNER_TEMP:-/tmp}/flappy-webgl-build.log"
test -x "$unity_editor" || { echo "Unity editor missing: $unity_editor" >&2; exit 1; }
echo "Building WebGL with Unity; log: $build_log"
"$unity_editor" -batchmode -nographics -projectPath "$repo_root/FlappyBird" \
  -buildTarget WebGL -executeMethod BuildGame.BuildWebGL -quit -logFile "$build_log" &
build_pid=$!
trap 'kill "$build_pid" 2>/dev/null || true' INT TERM
while kill -0 "$build_pid" 2>/dev/null; do
  echo "Unity build running ($SECONDS seconds elapsed)"
  sleep 15
done
if ! wait "$build_pid"; then tail -80 "$build_log"; exit 1; fi
grep -q BUILD_AND_TESTS_PASSED "$build_log"
node "$repo_root/scripts/check-webgl.mjs" "$repo_root/FlappyBird/Build/WebGL"
echo "WebGL build and Unity checks passed"
