# Flappy Bird：实现与发布复盘

记录日期：2026-09-16。本文不包含密码、API key、许可证内容或 OAuth 回调地址。

## 最重要的教训

- 代理造成的凭据暴露，由代理负责调查、准备修复并完成所有已授权且工具允许的步骤。不要反复把整套配置转交给用户。
- 把三个动作分开判断：撤销旧凭据、取得新凭据、把已有凭据保存到 GitHub。一个浏览器操作需要用户接手，不代表 CLI 保存 Secret 也需要用户手动完成。
- `gh secret set` 可以从文件或标准输入读取值，不必让值进入模型上下文、命令参数或日志。我们实际已用这种方式设置 Unity 密码和许可证。
- 工具限制应准确说明适用的动作。不要把某个浏览器工具的限制泛化成所有工具都不能做；同时也不能用另一种方式绕过明确适用的限制。
- 不要循环汇报同一个阻塞，也不要为了显得有进展反复触发必然失败的 CI。找新的证据或完成独立工作；确实需要用户操作时，只留下必要的最小步骤，并给出具体入口。
- 用户说“账号是你申请的”时，应核对历史，不能未经验证就认领注册行为。Hub 显示旧许可证激活日期，也不能据此断言账号是谁创建的。

## 凭据处理

用户要求留存以便复用后，项目根目录已创建本地 `.env`，权限为 `600`，并由 `.gitignore` 排除，保存 `UNITY_EMAIL` 和 `UNITY_PASSWORD`。只通过 shell 加载，不要用文件查看工具打开。许可证另外备份在 `.secret/Unity_lic.ulf`，目录权限 `700`、文件权限 `600`，已验证与默认路径源文件一致，整个 `.secret/` 已被 Git 忽略。也未保存暴露的旧 butler key。不要用此 `.env` 覆盖或清空尚未在本地保存的远端 Secret。

复用已保存的字段（不打印值）：

```bash
set -euo pipefail
source .env
printf '%s' "$UNITY_EMAIL" | gh secret set UNITY_EMAIL --repo StevenLi-phoenix/unity_flappy_bird
printf '%s' "$UNITY_PASSWORD" | gh secret set UNITY_PASSWORD --repo StevenLi-phoenix/unity_flappy_bird
gh secret set UNITY_LICENSE --repo StevenLi-phoenix/unity_flappy_bird < .secret/Unity_lic.ulf
```

安全传输示例（路径是占位符，不是实际凭据文件）：

```bash
set -euo pipefail
gh secret set BUTLER_API_KEY \
  --repo StevenLi-phoenix/unity_flappy_bird < /secure/path/new-butler-key
gh secret list --repo StevenLi-phoenix/unity_flappy_bird
```

只查看 Secret 名称和设置结果，不读取或打印值。不要使用 `set -x`，不要把值写入版本库、文档或截图。

本次 butler OAuth 将 token 放进回调 URL 的 fragment。即使当次输出已过滤，后续原生浏览器状态仍可能通过后台标签页标题泄漏 token。因此，过滤必须覆盖整个返回内容，包括所有标签页标题和地址；不要在敏感页面直接截图。优先让官方 CLI 接收回调并保存凭据，再通过文件/stdin 传输。

删除 GitHub Secret 或运行 `butler logout` 都不等于撤销服务端密钥。官方文档要求在 itch.io API keys 页面撤销暴露的 key。不要把本地清理说成完成轮换，也不要把旧 key 重新装回去后称为已修复。

## Unity 许可与云构建

- Unity 编译器在 CI 中也需要许可；免费 Personal 同样要激活。玩家运行 WebGL、itch.io 托管游戏本身不需要 Unity 账号。
- 本项目的 GameCI 工作流使用 `UNITY_EMAIL`、`UNITY_PASSWORD`、`UNITY_LICENSE`。这是当前工作流的要求，不应泛化为所有发布方式的要求。
- 本机 Hub 能构建，不代表 GitHub 临时机器已经获得授权。
- 早期标准目录没有 `.ulf`；用户批准 Personal 条款后进行了重新激活。之后官方 Unity CLI 1.0.0-beta.9 显示 Personal (ULF) 有效，重新检查发现 `/Library/Application Support/Unity/Unity_lic.ulf`。文件出现的具体原因未证实，不能声称 CLI 的只读检查生成了它。
- 不要把 entitlement XML 改名冒充 ULF。重新检查权威状态，而不是无限沿用早期“文件不存在”的结论。
- 再次复查默认路径时，找到了 `/Library/Application Support/Unity/Unity_lic.ulf`，随后直接复制到 `.secret/Unity_lic.ulf`，用 `cmp -s` 验证一致性，全程未显示内容。此前“许可证本地文件已不存在”的说法过于绝对：准确表述应是“该次检查未找到”。未查明出现或消失的原因时，不要推断许可证失效、文件被删除或由某个命令重新生成。
- 用户要求重新扫描默认位置时，先复查明确的默认路径；不要优先遍历整个用户目录和庞大的模拟器运行库。找到凭据后立即做用户已授权的受限权限备份，并同步修正旧文档中的当前状态。
- 官方 CLI 二进制从 Unity CDN 下载，并按官方 manifest 校验 SHA-256；没有安装常驻 runner。用户明确拒绝本机 runner，这个约束继续有效。

## 游戏和 WebGL 实现

- 缩放错误需要统一坐标体系。当前使用原生 Canvas、`CanvasScaler.ScaleWithScreenSize` 和 `Expand`，参考尺寸 1100×660；不要叠加第二套 GUI 缩放。
- UI 居中、游戏世界底部锚定；背景和地面覆盖实际边界，管道在可见边界之外生成与回收。点击命中测试必须使用同一坐标转换。
- 旋转先围绕鸟自己的中心组合，再应用屏幕缩放，避免点击时瞬移。文字与图标组合按实际几何尺寸居中。
- Unity WebGL 显式使用 OpenGLES3/WebGL 2，Gzip 配合 decompression fallback；自定义模板填满 iframe，并提供加载进度和错误提示。
- WebGL 的 `Application.dataPath` 不是可写本地路径，桌面截图逻辑不能直接用于浏览器。
- WebGLSupport 模块位于版本目录中、与 `Unity.app` 同级。先检查真实安装路径，不要猜。
- 上传压缩包必须在根目录包含 `index.html`。itch.io 项目类型设为 HTML，上传文件标记为浏览器可玩；后续更新同一个 `html5` channel。

## 验收与当前结果

必须分别验证：凭据存在 → Unity 激活 → 测试 → WebGL 产物 → 上传 → 公开页面实际运行 → itch.io 版本与触发提交一致。任何前一阶段成功，都不能代替后一阶段。

本次保存文档时，已重新查询 [GitHub run 35062603914](https://github.com/StevenLi-phoenix/unity_flappy_bird/actions/runs/35062603914)：

- `build` job：成功。此前日志确认许可激活成功、99 项 Unity 检查通过。
- `publish` job：失败，明确报错 `Missing BUTLER_API_KEY repository secret`。
- 该运行由手动 dispatch 触发，不能证明真实 main push 到 itch.io 的完整自动更新已经成功。
- [公开游戏](https://stevenli-phoenix-work.itch.io/flappy-bird)此前已验证可以游玩；它不证明这次云构建已发布。

后续应先完成暴露密钥的撤销及新凭据配置，再验证真实 main push 的 build/publish、butler channel 上的提交 SHA 和线上游戏。不要再把 Unity 凭据缺失当成当前阻塞。

## 参考

- [项目维护交接](HANDOFF.md)
- [GameCI 激活说明](https://game.ci/docs/github/activation/)
- [butler 登录、CI 与撤销说明](https://itch.io/docs/butler/login.html)
- [Unity 官方 CLI](https://docs.unity.com/en-us/unity-cli/use-unity-cli)
