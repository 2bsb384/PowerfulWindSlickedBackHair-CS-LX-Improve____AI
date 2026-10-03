# 强 风 大 背 头（Windows 版）— AI 修改版

> 用一堆 Windows 窗口表演"強風オールバック"。B 站刷到的视频，程序原作者说源码不小心搞丢了，好在可以反编译，本项目由反编译恢复而来，并在此基础上做了进一步修改。

## 致谢（原作者声明）

- **程序原作者：CS-LX** — https://github.com/CS-LX
- **反编译重建 / 改进作者：SunnyDesignor** — https://github.com/SunnyDesignor
- **原项目仓库**：https://github.com/SunnyDesignor/PowerfulWindSlickedBackHairCS-LX_Improve

本项目为在原项目基础上修改的独立版本，两位原作者的署名与贡献请以上方链接为准。

## 本版本（AI 修改版）改动说明

> 本版本的以下修改由 AI 辅助完成。

1. **运行时锁定桌面**：程序启动后自动隐藏桌面图标与任务栏，并禁用鼠标键盘输入；正常退出时全部自动恢复（需要管理员权限，首次运行会弹 UAC 提示）。
2. **单文件 exe**：所有素材（图片、字体、BGM、音效库）全部嵌入一个 exe，无需任何附带文件。运行时会将少量必需文件（SoundTouch 变声库、字体、音频）临时释放到 exe 旁和 `%TEMP%\PWBSBH` 目录，**退出后自动清理删除，只保留 exe 本身**。
3. **移除结束弹窗**：播放结束不再弹出作者信息窗口，动画播完直接自动退出并恢复桌面。
4. **日志不再残留**：运行日志写入临时目录，随退出清理一并删除，不会在 exe 旁边生成 log.txt。
5. 壁纸玩法保留原作设计：启动时切换黄色壁纸、部分场景蓝/黄切换、结束恢复你原来的壁纸（使用第三方壁纸软件如 Wallpaper Engine 也能正常恢复）。

## 使用方法

1. 从 Releases 下载 exe（或自行编译，见下）。
2. 双击运行，在 UAC 提示中选择"是"。
3. 动画期间鼠标键盘被锁定、任务栏与图标隐藏，属正常现象；播放结束自动恢复并退出。

小贴士：

- 若任务管理器强制结束后桌面未恢复：重启 `explorer.exe`，并在 设置 → 个性化 → 背景 里重新选择一次壁纸即可。
- 音画不同步时，可在 exe 同目录编辑 `Offset.txt` 调整参数（见原项目说明）。

## 构建说明（无 Visual Studio 环境）

使用 .NET SDK 的 msbuild + .NET Framework 4.0 引用程序集即可编译：

```powershell
# 1. 先转换 resx（本项目内脚本，避免新版 SDK 的 Resource 生成器不兼容）
powershell -File ConvertResx.ps1
# 2. 编译（FrameworkPathOverride 指向本机 .NET Framework 4.0）
dotnet msbuild PowerfulWindSlickedBackHair.csproj /p:Configuration=Release /p:FrameworkPathOverride=C:\Windows\Microsoft.NET\Framework64\v4.0.30319
```

## 免责声明

- 本程序仅供学习、娱乐与交流使用，请勿用于任何不当用途。
- 程序运行期间会暂时锁定鼠标键盘输入、隐藏任务栏与桌面图标、更改桌面壁纸，请知悉后再运行。
- 程序内含的动画素材、音乐、字体等版权归原作者所有；若涉及任何版权问题，请联系原作者或删除本仓库。
- 使用本程序可能造成的任何直接或间接后果，本项目作者与 AI 不承担任何责任。

## 相关链接

- B 站视频：https://www.bilibili.com/video/BV1Kz4y1p7sz
- 原视频：https://youtu.be/D6DVTLvOupE