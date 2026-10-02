# Fogbound Maze v1.0.0

## 版本内容

- 完整的 10 关 3D 生存迷宫流程：选武器、进入迷宫、战斗、到达出口、解锁下一关。
- 第一人称和第三人称实时切换，支持键鼠与安卓触屏输入。
- 手枪命中扫描、弹匣与换弹系统，以及砍刀近战判定。
- 普通/精英僵尸、出生预警、网格 A*、状态机和对象池。
- 每关使用不同固定种子；候选迷宫按路线长度、转弯、死路和岔路评分。
- 后期关卡逐步加入雾、昼夜循环、精英怪、瘴气与安全灯。
- Windows、WebGL 和 Android 三平台交付包。

## 难度证据

十关主路线长度依次为：

```text
21, 31, 43, 57, 64, 79, 86, 96, 114, 128
```

关卡布局互不重复、全部存在起点到终点路径。完整指标见
`docs/02_CAMPAIGN_AND_BALANCE.md`。

## 验证结果

- EditMode：`8/8 PASS`。
- PlayMode：`5/5 PASS`。
- Windows Release：`Succeeded`，0 errors，0 warnings。
- WebGL Release：`Succeeded`，0 errors，0 warnings；浏览器加载烟测通过。
- Android APK：`Succeeded`，0 errors，0 warnings。
- Windows Player：运行时烟测通过，日志包含 `FOGBOUND_RUNTIME_SMOKE_PASS`。
- 三个发布附件的 SHA-256 与 `SHA256SUMS.txt` 一致。
- Android 最终 APK 真机烟测：`PENDING`，由发布者安装同一附件后确认。

## 已知限制

- 角色和僵尸使用程序化低多边形外观与代码驱动动作，没有商业动画资源。
- 敌人导航基于单层迷宫网格，不支持多层空间或任意地形。
- 当前平衡数据来自开发测试，不代表大规模玩家测试结果。
- WebGL 首次加载速度取决于静态托管服务和网络环境。

