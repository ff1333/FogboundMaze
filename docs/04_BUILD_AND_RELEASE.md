# 构建与发布指南

本文以 `v1.0.0` 为准。项目使用 Unity `6000.3.18f1`，本机 Unity 路径为：

```text
E:\Program Files\Unity\Hub\Editor\6000.3.18f1\Editor\Unity.exe
```

## 1. 发布前检查

1. 用 Unity Hub 打开项目根目录 `FogboundMaze`。
2. 等待右下角导入和编译结束。
3. 打开 `Assets/Scenes/Main.unity`。
4. 确认 Console 没有红色错误。
5. 点击 Unity 顶部三角形 Play，至少完成一次选武器、进门、移动、攻击、切换视角和暂停。
6. 退出 Play Mode 后再开始构建。

## 2. 一键生成三个平台包

在项目根目录打开 PowerShell，运行：

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\build-release.ps1
```

脚本会依次执行发布边界验证，并生成：

```text
Builds/Windows/v1.0.0/FogboundMaze.exe
Builds/WebGL/v1.0.0/index.html
Builds/Android/FogboundMaze-v1.0.0.apk
```

每个平台日志最后都应包含 `result=Succeeded errors=0 warnings=0`。如果脚本中断，先看
`Builds/release-*.log` 最后 100 行，不要上传未完整生成的文件。

## 3. 生成 GitHub Release 附件

构建全部成功后运行：

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\package-release.ps1
```

最终附件都位于：

```text
Builds/Packages/v1.0.0/
```

必须有下面四个文件：

```text
FogboundMaze-Windows-v1.0.0.zip
FogboundMaze-WebGL-v1.0.0.zip
FogboundMaze-Android-v1.0.0.apk
SHA256SUMS.txt
```

`SHA256SUMS.txt` 用来确认文件在传输后没有损坏。重新构建或重新打包后，旧哈希会失效，
必须使用脚本新生成的文件。

## 4. 本地验收

### Windows

1. 解压 `FogboundMaze-Windows-v1.0.0.zip`。
2. 不要单独移动 exe；`FogboundMaze_Data` 等文件夹必须和 exe 放在一起。
3. 双击 `FogboundMaze.exe`。
4. 验证鼠标键盘、两种武器、第一/第三人称切换和至少一关通关。

自动烟测已经在真实 Windows Player 中完成，日志包含
`FOGBOUND_RUNTIME_SMOKE_PASS`，证据图是
`Builds/Windows/v1.0.0/release-smoke.png`。

### WebGL

WebGL 不能直接双击 `index.html`。解压后在该目录启动静态服务器：

```powershell
python -m http.server 8092
```

浏览器打开 `http://127.0.0.1:8092/`。本版本已启用 Gzip 和 Decompression Fallback，
普通静态服务器也能加载。最终浏览器烟测已通过；控制台只保留 Unity 自身的
`persistentDataPath` 弃用提示，不影响游戏。

### Android 真机

1. 把 `FogboundMaze-Android-v1.0.0.apk` 发送到安卓手机。
2. 在手机设置中仅对本次使用的文件管理器允许“安装未知应用”。
3. 安装 APK，保持横屏进入游戏。
4. 验证左侧摇杆、滑动视角、`FIRE`、`RUN`、`R`、`VIEW` 和暂停按钮。
5. 分别选择手枪和砍刀，至少完成第一关一次。
6. 确认文字没有被刘海、圆角或系统导航条遮挡。

只有完成这六项，才能把 Android 真机状态从 `PENDING` 改成 `PASS`。APK 构建成功不等于
已经完成真机验收。

## 5. 第一次推送到 GitHub

先在 GitHub 新建一个空仓库，建议名称 `FogboundMaze`。不要勾选自动创建 README、
`.gitignore` 或 License，因为本地仓库已经包含这些内容。然后在项目根目录运行：

```powershell
git remote add origin https://github.com/ff1333/FogboundMaze.git
git push -u origin main
git push origin v1.0.0
```

如果 `origin` 已存在，不要再次 `remote add`，先执行：

```powershell
git remote -v
```

确认地址正确后只执行两条 `git push`。

## 6. 创建 GitHub Release

1. 打开仓库首页，点击右侧 `Releases`。
2. 点击 `Draft a new release`。
3. 在 `Choose a tag` 中选择已经推送的 `v1.0.0`。
4. Release title 填 `Fogbound Maze v1.0.0`。
5. 正文使用 `docs/RELEASE_NOTES_v1.0.0.md`。
6. 上传 `Builds/Packages/v1.0.0` 下的四个附件。
7. 等待四个文件全部显示上传完成，再点击 `Publish release`。

## 7. WebGL 公开试玩

GitHub Release 里的 WebGL ZIP 是交付包，不会在页面内自动运行。作品集需要在线试玩时，
可以把 `FogboundMaze-WebGL-v1.0.0.zip` 上传到 itch.io，并勾选“该文件将在浏览器中运行”。
发布后实际打开公开页面再做一次键鼠和加载测试，然后把试玩链接放进 README 和简历。

