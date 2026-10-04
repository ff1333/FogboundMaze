# v1.2.1 验证索引

日期：2026-10-04。工具执行与画面审查：Codex 辅助。

- v1.2.1-EditMode.xml：10/10，通过。
- v1.2.1-PlayMode.xml：31/31，通过，包含 8 项新增攻击/握持/遮挡回归。
- v1.2.1-title/loadout/smg/blade/right-hand.log：独立 Windows Player，包内版本确认为 1.2.1。
- 对应截图在 ../../images/v1.2.1-*.png；实际相机 RenderTexture 渲染，分辨率 1280×720。
- fogbound-web-result.json：Edge 浏览器开始→图鉴→选关→选武器→移动→暂停，无捕获的 JS/Unity 异常。
- 浏览器截图在 ../../images/v1.2.1-fogbound-web-*.png，viewport 1280×800、canvas 960×600。
- build-summary.txt：最终 Windows/WebGL/Android 构建摘要，不使用已中止的中间构建记录。
- SHA256SUMS.txt：最终三个发布附件的校验值，另保存在 Builds/Packages/v1.2.1。

新增攻击回归明确验证：按住连射遵守冷却、换弹期间不能射击、换满 30 发；墙后和超射程目标不扣血；
镜头看得到但枪口被挡也不穿墙；长刀前摇、3.2 米处命中、3.6 米处不命中；同敌人多个 Collider 只伤害一次；
身后/墙后目标不受近战伤害；僵尸不能隔墙扣玩家血；右手握持；退出菜单或死亡取消挥砍前摇。

Windows 和 WebGL 构建为零错误、零告警。Android 的结果和告警数量以最终摘要为准；
SDK 远程清单下载/等待告警属于构建环境联网检查，不等于已完成手机渲染、触控或性能验收。

安卓新版真机仍待作者实际安装检查，不沿用旧包的真机结论。没有把自动化测试写成作者本人手测。
本次未宣称全部十关长时间平衡、所有手机或应用商店审核通过。
