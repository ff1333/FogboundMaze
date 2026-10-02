# 架构说明

## 启动流程

`Main.unity` 只保存一个 `GameDirector`。运行后按以下顺序创建系统：

1. 创建输入、迷宫、敌人池和环境控制器。
2. 创建玩家、双视角相机和武器控制器。
3. 创建 HUD 与移动端触控层。
4. 读取存档并加载对应关卡。
5. 生成迷宫、预热对象池，进入选武器阶段。

运行时生成使十关共享同一份代码和场景，关卡差异集中在
`LevelDefinition`，避免复制场景后参数逐渐失控。

## 主要模块

| 模块 | 关键脚本 | 职责 |
| --- | --- | --- |
| 流程 | `GameDirector` | 阶段、关卡、存档、生成、胜负 |
| 迷宫 | `MazeGenerator` | 固定种子的递归回溯生成 |
| 难度 | `MazeComplexity` | 路线、转弯、死路和岔路评分 |
| 寻路 | `MazePathfinder` | 网格 A* 与路径重建 |
| 世界 | `MazeWorld` | 生成碰撞体、门、灯和坐标转换 |
| 玩家 | `PlayerController` | CharacterController 移动与重力 |
| 相机 | `CameraRig` | 第一/第三人称切换和墙体避让 |
| 战斗 | `WeaponController` | Hitscan 手枪、弹匣、近战检测 |
| AI | `EnemyAgent` | Wander/Chase/Attack/Dead 状态 |
| 性能 | `EnemyPool` | 按关卡上限预热并复用敌人 |
| 环境 | `EnvironmentController` | 雾、光照与昼夜循环 |
| 输入 | `GameInput` | Input System 与触控输入统一 |
| UI | `GameHud` | HUD、武器选择、暂停与结算 |

## 为什么不用 NavMesh

迷宫在运行时生成。使用 NavMesh 需要运行时重新烘焙，并引入额外平台与包依赖。
敌人直接使用迷宫的逻辑网格做 A*，路径天然不会穿墙，算法也可以在 EditMode
中独立测试。当前上限只有 14 个敌人，每个敌人按 0.45-0.7 秒间隔重算路径，
不需要每帧寻路。

## 第一/第三人称

两种视角共用同一个 `CameraRig`、准星和攻击方向。第三人称使用球形检测缩短
相机距离，避免墙体挡住镜头；第一人称把相机放在眼位。角色移动始终根据相机
水平朝向计算，因此切换视角不会改变操作语义。

## 数据流

输入 -> 玩家/相机 -> 武器命中或移动 -> Health/AI -> GameDirector -> HUD。

系统之间优先通过公开状态和事件通信，例如 `Health.Died`，而不是在多个脚本里
重复查询场景对象。

