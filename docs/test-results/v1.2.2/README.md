# v1.2.2 验证索引

日期：2026-10-04。自动执行和截图审查：Codex 辅助，不是作者本人手测。

| 检查 | 结果 | 证据 |
| --- | --- | --- |
| EditMode | 15/15 通过 | v1.2.2-EditMode.xml |
| PlayMode | 34/34 通过 | v1.2.2-PlayMode.xml |
| Windows 正式包 | 版本 1.2.2；图鉴、装弹就绪、第十关加载通过 | v1.2.2-guide/guide-small/reload/level10.log |
| WebGL / Edge | 开始、图鉴、选关、选武器、移动、装弹、暂停；无捕获异常 | fogbound-web-result.json |
| Windows / WebGL / Android 构建 | 三者均 0 errors / 0 warnings | build-summary.txt |
| APK 元数据 | versionName 1.2.2、versionCode 8、ARM64 | build-summary.txt |
| 三份发布附件 | SHA-256 已生成并复核 | SHA256SUMS.txt |
| 新版 Android 真机 | 待作者安装试玩 | 不复用旧版验收结论 |

## 截图

图片在 `../../images/v1.2.2-*.png`：Windows 图鉴 1280×720 和 960×540；
第一人称 READY 提示和第十关入口灯柱 1280×720；浏览器 viewport 1280×800、canvas 960×600。
图鉴文字、READY 及换弹进度均已对截图检查，没有发现本次文字溢出。
Windows 截图为实际相机 RenderTexture 渲染；WebGL 截图为 Edge/Playwright 交互所得。

## 测试覆盖和限制

灯柱新增测试验证默认四个后期关卡的同种子复现、唯一合法位置、支路/死路分布、危险格与通行耗血。
耗血按中心线每 0.1 米采样，含终点到出口外侧通道，不计战斗与走错路。实际数据见 XML 和关卡平衡文档。

装弹测试验证完成补满 30 发、READY 显示与消退、开火清除 READY、装弹期间不能射击、暂停冻结、死亡/切武器取消。
音效样本检查确认完成声与开始声不同、含两段非静音脉冲且幅值没有超出范围；没有将此写成听觉质量验收。

本轮无新增外部美术或软件依赖。Android SDK 远程清单预先通过现有代理刷新，本次 BuildReport 未再出现上一版的联网告警。
构建成功不代表商店审核通过，也不代表所有手机上的帧率、音量或触控体验均通过。
本次灯柱数量增加，长时间战斗及手机灯光性能仍需实际试玩观察。
