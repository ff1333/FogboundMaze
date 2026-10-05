# v1.3.0：中英文与声音设置

日期：2026-10-05。此版本保留十关迷宫、第一/第三人称、冲锋枪装弹、长刀、地图探索、全图避难灯和之前的试玩修复。首领战只加入 Neon Arena，没有修改本游戏的胜利条件。

## 玩家操作

1. 打开 `Assets/Scenes/Main.unity`，点击 Unity 顶部三角形；也可以打开本次 Windows 包中的 FogboundMaze.exe。
2. 在开始页点“设置”，不是去 Unity 的 Project Settings。
3. 点“简体中文”或 English。无设置记录时默认中文；设置页、选关、图鉴、武器选择和动态 HUD 随语言切换。
4. 勾选/取消声音开关；拖动总音量、音乐音量、音效音量。音效音量也影响射击、挥刀、命中和装弹完成提示。
5. 点返回后重新进入设置确认数值；关闭程序再开确认保留。静音后再打开声音，原滑块值仍保留。
6. 点开始、选择已解锁关卡、选择武器进入游戏。WASD 移动，鼠标瞄准，左键攻击，Shift 冲刺，R 装弹，V 切视角，Esc 暂停并释放鼠标。

WebGL 的声音需要用户与页面交互后由浏览器解锁；初次没有点页面时无声不代表音量失效。Android 仍需在真实手机上听声音和操作触控。

## 实现说明

| 文件 | 责任 |
| --- | --- |
| `Assets/Scripts/PortfolioSettings.cs` | 默认值、PlayerPrefs、音量范围限制、设置变更事件 |
| `PortfolioSettingsMenu.cs` | 开始页入口、安全区设置界面、语言按钮、开关、滑块和环境音 |
| `LocalizedLabel.cs` | 保留源文本、中文映射、UGUI/TMP 字体与动态刷新 |
| `Resources/Localization/Strings.json` | 英文源句和中文翻译，含十关机制说明 |
| `UI/GameHud.cs` | 创建文字时挂接本地化标签，保留原 HUD/血条/操作流程 |
| `Combat/WeaponController.cs` | 将音效分类音量应用到武器声音，销毁时退订事件 |
| `Tests/PlayMode/SettingsTests.cs` | 切换语言、中文图鉴、静音、音量以及换场景后的保留 |

设置只属于本机，不是云存档、账号系统或防作弊。关卡进度使用原有独立 key；设置不会清除通关记录。两个独立产品由各自的 PlayerPrefs 存储隔离。

## 开发记录

第一轮接入共享设置层，补充中文词条、Noto 字体和程序生成环境音，并在打开设置时阻止开始页快捷键穿透。

第二轮用独立 Player 检查 960×540 和 1280×720 画面：修复滑块手柄过长，文字创建时直接挂接本地化。自动截图原先在本帧 LateUpdate 之前抓取，导致刚切换的图鉴说明仍显示英文；截图前显式刷新，避免误判。生产界面仍按每帧刷新处理动态文本。

第三轮重跑原有生成、寻路、战斗、存档、环境、重载、镜头和装弹测试，加入设置持久化回归，生成 Windows/WebGL/Android 包。原始 XML、截图与构建信息见 [v1.3.0 证据](test-results/v1.3.0/README.md)。

本轮字体有 OFL 授权，详见根目录 `THIRD_PARTY_NOTICES.md`。此前角色仍为 Quaternius CC0 素材，不能在面试中说角色建模和动画都是自己手工制作。

## 自己复盘时做什么

先找到 PlayerPrefs 的默认中文值；跟一次 SetLanguage → Changed → 标签刷新的调用，再跟一次音量滑块 → SetVolume → AudioListener/AudioSource。最后解释静音为什么不把三条滑块全部设为零，以及事件为什么要在 OnDestroy 里退订。

本轮没有声称实测所有机型性能，也没有将自动运行当成主观听音或十关手动通关。新版手机验收仍按总交付指南进行。
