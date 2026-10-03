# v1.0.1 首次试玩修复包：验收与发布

> 后续试玩已确认本版存在入口墙体堵路和边缘掉落问题。本页为历史记录，不再按本页发布 v1.0.1。请改看 `07_V1_0_2_ENTRY_FIX.md`。

本页只适用于 `v1.0.1`。`v1.0.0` 的原始包和说明保留，不用旧版附件覆盖新版，也不要把旧版测试记录当作本次证据。

## 1. 当前进度

- 首次试玩问题、原因和处理见 `devlogs/04-first-playtest-bugfix.md`。
- EditMode `9/9 PASS`，PlayMode `10/10 PASS`；三平台正式构建均为 `0 errors / 0 warnings`。
- Windows Release Player 烟测 PASS；三个发布附件的 SHA-256 与校验单匹配。
- 三个附件和 `SHA256SUMS.txt` 已在主项目 `Builds/Packages/v1.0.1/`，可以直接上传，不需重建。
- WebGL 本地 HTTP 服务返回 200，但浏览器内加载与操作仍为 PENDING。
- Android APK 构建成功不等于真机已验收。必须安装**本次 v1.0.1 APK** 后再确认。
- WebGL 构建成功不等于浏览器交互已验收。必须打开**本次 v1.0.1 页面**复测。

## 2. 在 Unity 中人工复测

1. 打开 `Assets/Scenes/Main.unity`，按顶部三角形 Play。
2. 在选武器界面确认下方能看到电脑或手机对应的操作说明。
3. 选手枪后，确认绿色入口框与提示；从入口走入迷宫。进门前按 `Esc` 应打开暂停菜单并恢复鼠标箭头。
4. 点 `RESUME` 后，鼠标重新成为准心；按 `V` 切换第一/第三人称，观察镜头高度。
5. 准心对准僵尸按鼠标左键：弹药减少、命中有反馈；按 `R` 换弹。
6. 暂停并点 `BACK TO LOADOUT`，选砍刀；靠近僵尸持续按左键，确认挥砍动作与伤害。
7. 再按 `Esc`，确认 Windows/Android 的 `QUIT GAME`；在 Unity 编辑器内点击会停止 Play Mode。WebGL 不显示退出按钮，返回选武器后关闭网页即可。
8. 看顶栏血条、时间、关卡、击杀和武器文字是否清晰、无遮挡。
9. 看右上角探索小地图：起初仅有起点，走过岔路后格子逐步点亮；转动视角时白色朝向标记跟着转。重试后轨迹应清空，不能提前看见完整迷宫和终点。

不要仅凭自动截图把第 3-9 项写成自己已经亲手测过。尤其砍刀手感、镜头眩晕和安卓触控需要实际试玩判断。

## 3. 构建与附件

本轮附件已经生成。下面仅用于学习或需要重新构建时复现；先关闭原工程的 Unity 编辑器，
再在项目根目录的 PowerShell 执行：

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\build-release.ps1
powershell -ExecutionPolicy Bypass -File .\tools\package-release.ps1
```

脚本产物：

```text
Builds/Windows/v1.0.1/FogboundMaze.exe
Builds/WebGL/v1.0.1/index.html
Builds/Android/FogboundMaze-v1.0.1.apk
Builds/Packages/v1.0.1/FogboundMaze-Windows-v1.0.1.zip
Builds/Packages/v1.0.1/FogboundMaze-WebGL-v1.0.1.zip
Builds/Packages/v1.0.1/FogboundMaze-Android-v1.0.1.apk
Builds/Packages/v1.0.1/SHA256SUMS.txt
```

本轮实际在独立验证副本构建，四个附件已复制到**主项目**的
`Builds/Packages/v1.0.1/`，不需再次在原工程构建。不要把验证副本的 `Library`、
`Temp` 或生成物提交进 Git。若将来重建卡在 `Detecting Android SDK`，先确认本机
Clash Verge 的 `127.0.0.1:7897` 正在监听，然后只在该 PowerShell 窗口设置：

```powershell
$env:JAVA_TOOL_OPTIONS = '-Dhttps.proxyHost=127.0.0.1 -Dhttps.proxyPort=7897 -Dhttp.proxyHost=127.0.0.1 -Dhttp.proxyPort=7897'
powershell -ExecutionPolicy Bypass -File .\tools\build-release.ps1
```

该变量只影响从这个窗口启动的构建进程，不会修改系统代理。如果端口不再是 `7897`，
先在代理软件里看实际端口，不能照抄旧值。

## 4. 各平台的最后确认

- Windows：解压 ZIP，运行 exe，完成第 2 节的完整人工复测。不要只复制 exe，旁边的 `_Data` 文件夹必须保留。
- WebGL：解压 ZIP，在 `index.html` 同目录运行 `python -m http.server 8092`，打开 `http://127.0.0.1:8092/`。验证加载、选武器、鼠标锁定/释放、手枪和砍刀；浏览器关闭标签页就是退出。
- Android：把 **v1.0.1** APK 发到真机并安装，横屏验证虚拟摇杆、滑动视角、攻击、切镜头、暂停、返回选武器和退出。还要看刘海/圆角有无压住顶栏。

只把实际完成的项目写为 PASS；没测试的保留 PENDING。

## 5. 推送及 GitHub Release

本地仓库目前没有 `origin`；`ff1333/FogboundMaze` 远端地址查询结果是“Repository not found”。
先在 GitHub 账号 `ff1333` 下新建**空仓库** `FogboundMaze`，不要勾选自动生成 README、
`.gitignore` 或 License。完成后，在本项目根目录的 PowerShell 运行一次：

```powershell
git remote add origin https://github.com/ff1333/FogboundMaze.git
git remote -v
```

确认地址显示正确，再推送本地 `main` 和 `v1.0.1`：

```powershell
git status
git push -u origin main
git push origin v1.0.1
```

进入 GitHub 仓库右侧 `Releases`，点 `Draft a new release`，在 `Choose a tag` 选择 `v1.0.1`，标题填 `Fogbound Maze v1.0.1`，正文参考 `docs/RELEASE_NOTES_v1.0.1.md`，上传上述三个包和 `SHA256SUMS.txt`。四个文件上传完成后点 `Publish release`。不要重新创建一个指向 v1.0.0 的标签。

若做 itch.io 浏览器试玩，上传 WebGL ZIP、设置为浏览器运行，然后打开公开页面复测。GitHub Release 自身只提供下载，不会在线运行游戏。
