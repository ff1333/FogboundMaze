# v1.2.0 验证索引

- EditMode.xml：10/10；PlayMode.xml：23/23。
- build-summary.txt：最终三平台 BuildReport 摘要。Android 的 5 条告警为 SDK 清单联网/等待提示，无构建错误。
- v1.2.0-*.log：独立 Windows Player 的菜单/战斗反馈截图运行记录。
- fogbound-web-result.json：本机 Edge（Chromium 内核）浏览器交互，无捕获的 JS/Unity 异常。
- SHA256SUMS.txt：三份发布附件校验值。
- 截图位于 ../../images/；自动摆放的外观检查场景并非正常刷怪时序。

Windows 检查 1280×720，图鉴另查 854×480；浏览器 viewport 1280×800，游戏画布 960×600。
这不是手机截图，没有把新版 Android 真机标为通过。自动化烟测也不代表整场长时间平衡验收。

浏览器脚本：`tools/verify-web.cjs`，需要 Node.js、Playwright 与本机 Edge。
运行形式：`node tools/verify-web.cjs fogbound Builds/WebGL/v1.2.0 Builds/WebVerification`。
脚本只开本地临时 HTTP 服务，结束时关闭服务和浏览器；结果需结合截图人工审查。
