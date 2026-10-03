# Fogbound Maze v1.1.0

增加完整开始流程：开始页 → 十关选择 → 关卡内选武器 → 进入迷宫。
第一关默认解锁，活着到达终点后保存本地完成记录并解锁下一关；已完成关卡可重复游玩。
暂停、武器和结算页可返回选关，进度重启后保留，兼容旧版真正的解锁记录。

玩家、普通僵尸和大型精英更换为 Quaternius CC0 模型和骨骼动画，替换枪刀模型。
保留双视角、探索地图、受击反馈、对象池、血条同步及分岔迷宫修复。

## 验证

- EditMode 10/10、PlayMode 21/21 PASS。
- Windows 正式包：菜单、选关、角色、小窗口选关、第一人称枪刀和前六格实体行走检查通过。
- Windows/WebGL/Android 构建成功，BuildReport 均为 0 errors / 0 warnings。
- 新版 WebGL 浏览器交互和 Android 真机验收：PENDING，发布前请根据实际试玩更新此项。

## 附件

- FogboundMaze-Windows-v1.1.0.zip：完整解压后运行 exe，保留同目录 DLL 和 Data。
- FogboundMaze-WebGL-v1.1.0.zip：需要通过网页服务托管，不能直接双击 index.html。
- FogboundMaze-Android-v1.1.0.apk：Android 安装包，versionCode 5。
- SHA256SUMS.txt：附件校验值。

本地存档不提供跨平台云同步。模型与动画为第三方 CC0 美术，来源见 THIRD_PARTY_NOTICES.md。
