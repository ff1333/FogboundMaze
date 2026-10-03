# v1.0.2：入口与防掉落修复

> 历史版本。本版后来发现血条与死亡数字显示问题、迷宫岔路不足，当前请看 `08_V1_0_3_HUD_AND_BRANCHES.md`。

## 1. 先确认打开的是新版

旧版 v1.0.1 的入口有已确认的缺陷，不再作为可发布候选。旧附件保留供追溯，不覆盖旧标签。

在 Unity 中：

1. 如果顶部三角形 Play 仍亮着，先点它停止运行。
2. 确认打开的工程目录是 `D:\software\documents\unity_games\projects\learning\FogboundMaze`。
3. 等待 Unity 右下角的导入或编译结束；Console 不应有红色编译错误。
4. 在 Project 面板双击 `Assets/Scenes/Main.unity`，再点顶部 Play。迷宫是运行时生成的，不需要手动调整场景内墙体。

## 2. 重点复测

1. 选手枪，沿绿色入口进入，然后继续沿走廊转弯，不要只停在计时开始的位置。
2. 出生区左右两侧应紧贴墙，不应再有通向门外黑暗处的侧边通道；贴墙前行也不应掉落。
3. 按 V 切换第一/第三人称，各走过几个转角；右上角小地图应逐步记录已走格子。
4. 按 Esc，点击 BACK TO LOADOUT，选砍刀再试。Esc 应能随时暂停、释放鼠标。
5. 重试后角色在出生区落地站稳，不应继承之前的下落速度。

这次不需要手动改脚本或墙的位置。若打开的是之前解压的 exe，它不会随 Unity 工程自动更新，必须换用新版包。

## 3. 自动验证及交付状态

- PlayMode：13/13 PASS，包含全部十关路线、开放岔路、平台边界与掉落恢复。
- EditMode：9/9 PASS。
- Windows 正式包：第一人称手枪、第三人称砍刀各实际连续走六格 PASS，截图与日志已保存。
- Windows / WebGL 构建成功，各 0 errors / 0 warnings。
- Android 构建成功，0 errors / 5 warnings；警告来自 SDK 远程清单下载失败或等待，不是 C# 编译错误。APK 版本名 1.0.2，版本号 3，ARM64。
- 三个平台附件已生成，SHA-256 已逐一核对；Windows 压缩包包含 exe、UnityPlayer.dll、Data，WebGL 包包含 index.html 与构建资源。
- WebGL 浏览器交互、Android 真机及用户本次手动验收：待执行。

完整根因及测试证据见 `devlogs/05-entry-geometry-regression.md`。

## 4. 新版附件与 GitHub

新版附件已放在主工程 `Builds/Packages/v1.0.2/`，无需再次构建：

- `FogboundMaze-Windows-v1.0.2.zip`：解压到一个新的文件夹，打开里面的 FogboundMaze.exe；不能只复制 exe。
- `FogboundMaze-WebGL-v1.0.2.zip`：交给支持 Unity WebGL 的网页托管平台，不能直接双击 index.html 代替部署测试。
- `FogboundMaze-Android-v1.0.2.apk`：传到手机安装，重新验证移动、攻击、暂停和进门。
- `SHA256SUMS.txt`：附件完整性校验单。

若你此前已经上传旧版本，复测新版后在同一个仓库新建 `v1.0.2` Release，保留历史版本；不要把新文件放到 v1.0.1 下冒充旧包。
本次没有自动推送 GitHub，也没有替你创建线上 Release。
