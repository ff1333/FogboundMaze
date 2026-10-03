# 07：开始菜单、战役进度与动画角色

日期：2026-10-03。版本：v1.1.0。实现与自动验证：Codex 辅助完成；用户新版手动验收：待确认。

## 反馈与验收目标

用户指出启动直接进入选武器，没有开始页和关卡选择；角色、怪物仍是简单几何体。
目标：开始页 → 1–10 选关 → 对应关卡选武器 → 进入迷宫，顺序通关解锁并本地保存；
替换为许可明确、带骨骼动画的角色素材，保留双视角、对象池和旧版修复。

## 实现

1. GamePhase 增加 Title/LevelSelect，GameHud.Campaign 构建开始页、十关网格和返回路径。
2. CampaignProgress 保存十关完成位标记，只接受连续已完成前缀。第一关默认可进，通关 N 解锁 N+1；死亡和进入不算通关。
3. 从旧 UnlockedLevel 迁移真实解锁记录，不信任 SelectedLevel。旧版第十关解锁只能证明前九关完成，不能推断第十关已通关。
4. 返回菜单清敌人、清触控残留、恢复 timeScale、释放鼠标；选关方法增加阶段/解锁校验。
5. 出生预警回调携带关卡代数，离开该局后失效，防止旧回调在新局生成敌人。
6. 从 Quaternius 官方源导入 CC0 生存者、普通/大型僵尸、枪刀、色板贴图及动画。缓存放 E 盘，来源和许可随代码保留。
7. 使用 Legacy Animation 片段切换，CharacterController 驱动位移；第三人称使用原骨骼武器挂点，第一人称独立武器预制体。
8. 对象池同时预热普通/精英外观，复用时重置碰撞体、生命、攻击计时、外观及动画；闪白用属性块并清除恢复原色。

## 集成中处理的问题

- 项目采用 Standard 材质，贴图应赋给 mainTexture；不能只按其他渲染管线的 _BaseMap 属性赋值。
- FBX 自带单位换算，保留原始比例。无图形环境的采样包围盒与实际 Player 显示不一致，不能据此缩小模型；改用实际 Player 外观和骨骼姿态变化验证。
- MaterialPropertyBlock 在 Awake 初始化，避免 Unity API 在线程不合适的字段初始化时调用。
- UI Button 明确设置 targetGraphic，锁定态反馈与交互状态一致。

## 自动验证结果

| 范围 | 结果 | 证据 |
| --- | --- | --- |
| EditMode | 10/10 PASS | ../test-results/v1.1.0/campaign-EditMode.xml |
| PlayMode | 21/21 PASS | ../test-results/v1.1.0/campaign-PlayMode.xml |
| Windows 正式包 | 成功，BuildReport 0 errors / 0 warnings | build-summary.txt |
| WebGL 正式包 | 成功，BuildReport 0 errors / 0 warnings | build-summary.txt |
| Android 正式包 | 成功，BuildReport 0 errors / 0 warnings | build-summary.txt |
| 实际 Player 菜单/角色 | 五组截图及退出码 0 | v1.1.0-title/levels/loadout/small-levels/actors.log |
| 第一人称枪/刀 | 各走前六格，WALK_PASS、SMOKE_PASS、退出码 0 | v1.1.0-pistol/knife.log |

表内未带路径的证据均在 `docs/test-results/v1.1.0/`，截图在 `docs/images/`。
PlayMode 覆盖锁关、存档重载、旧记录迁移、暂停返回、完整十关、动画骨骼运动及敌人复用，
也保留血条、死亡、迷宫分岔和十关实际碰撞通路回归。
测试备份并恢复原 PlayerPrefs；Player 烟测使用独立进度键。
角色展示烟测把两类敌人临时摆在入口用于外观检查，不是正常刷怪位置。

Unity 构建启动日志有授权客户端握手消息，但随后构建完成且最终 BuildReport 成功。
上述“0 errors / 0 warnings”专指 BuildReport，不代表原始日志完全没有 Error 字样。

## 交付与边界

发布附件在 `Builds/Packages/v1.1.0/`，含 Windows ZIP、WebGL ZIP、Android APK 和 SHA256SUMS。
包和源码同版本，Android versionCode=5。没有替用户发布 GitHub Release。
新版浏览器交互、Android 真机和长时间平衡体验仍待人工验收，不能冒充用户已测。
角色风格为低多边形，尚未制作完整第一人称手臂、IK、Animator 混合树或写实画面。
这些是明确的作品集范围，第三方模型和动画不得声称原创。
