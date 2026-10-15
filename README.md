# Cosmic Batter / cat-space-ship 组员交接

一个使用 Unity 6 制作的60秒太空猫街机射击原型，支持食材收集、限时增益、敌人追踪和冲刺。

## 打开与运行
1. Unity Hub 添加项目文件夹，用 **6000.0.66f2** 打开。
2. 等待编译，打开 `Assets/Scenes/CosmicBatter.unity`，点击 Play → LET'S BAKE!。
3. **WASD 或方向键四方向移动，按住空格连射**。点击 Game 视图后操作。
4. 60 秒结束显示得分、评级和 PLAY AGAIN。重开会清空敌人、子弹和所有临时效果。

场景和引用已经配置好，正常使用无需运行搭建工具。更新期间如果编辑器仍显示旧场景，请保存自己需要的改动到另一文件后重新打开 CosmicBatter 场景。

## 当前规则
- 牛奶：+10 分及浮字，定期掉落。
- 蛋：移动速度 +30%，持续5秒；再次拾取刷新到5秒，不增加倍率，不额外加分。
- 糖：三发扇形弹幕，射击间隔从0.12秒缩短到0.065秒，持续2秒；再次拾取刷新到2秒，不叠层，不额外加分。
- 罚单：缓慢追踪；触碰使玩家减速40%，持续3秒，罚单消失；再次碰到罚单刷新减速，不额外扣分。射杀+10分。
- 新鸽子：较快追踪，每约2.8秒向玩家当前位置直线冲刺0.35秒。触碰-50分，有全局1.5秒鸽子碰撞冷却，多个鸽子也不能同时重复扣分。鸽子接触后仍存在，可继续射杀，射杀+20分。
- 陨石保留：撞到-30分并使用原受伤冷却；会挡住子弹，不能击毁。
- 子弹穿过食材；分数最低为0。
- 加速与减速分别计时，同时生效时倍率相乘：1.3 × 0.6 = 0.78。每个效果到期只移除自身倍率。
- HUD 以绿色、黄色、蓝色分别显示加速、重火力、减速的剩余秒数；玩家变色和星点反馈辅助提示。
- 前15秒每1.7秒生成一名敌人，随后每15秒改为1.4、1.1、0.85秒。最后15秒鸽子概率从30%增加至65%。最多22名敌人；生成位置位于屏幕上部，会避开玩家和已有碰撞体，拥挤时跳过一次生成。

## 组员修改入口
- **PlayerController.cs / 场景 Player**：移动速度、射速、扇形角度，以及三种效果的倍率和时长。
- **FallingObject.cs / 敌人和道具预制体**：Touch Effect 决定触碰效果，Movement 决定下落/追踪/冲刺；Points 是碰撞分数，Kill Points 是击杀得分。
- **ObjectSpawner.cs / 场景 Spawner**：Stage Intervals 四档间隔、两种鸽子概率、Max Enemies、食材间隔与速度。
- **Bullet.cs / PlayerBullet.prefab**：子弹速度、命中半径和外观；扫过运动路径检测命中，避免高速穿透。
- **GameManager.cs**：时长、计分、评级、结束与重开。
- **GameUI.cs / Canvas**：现有开始/结果页与新增状态提示。
- **ScoreEffect.cs**：得分浮字和小爆炸。

推荐分工：一人负责玩家与子弹，一人负责敌人和生成，一人负责美术与UI。尽量不要同时修改同一个 .unity 场景。常用参数可直接在 Inspector 调，不必改代码；代码与 .meta 文件要一起交接。

## 素材与预制体
新增原图 `Assets/Art/SpeedingTicket.png`、`EvilPigeon.png`；保持图片比例，没有覆盖 PlayerShip.png、DonutBird.png 或其他原美术。
新增预制体 `SpeedingTicket.prefab`、`EvilPigeon.prefab`，复用原 FallingObject 脚本、Kinematic Rigidbody2D 和触发碰撞体。旧 DonutBird 预制体仍保留，目前生成器使用新敌人。
替换图片：导入PNG，Texture Type设为Sprite (2D and UI)，拖到对应预制体的Sprite Renderer；保持X/Y等比例缩放，根据实际图案调碰撞半径。

## 编辑器工具
- **Tools → Cosmic Batter → Update Items And Enemies**：对现有 CosmicBatter 场景配置本次更新，可重复运行，不重复生成HUD指示器；会恢复本版默认平衡参数，组员调参后不必再运行。
- **Build Prototype** 是旧的完整重建工具，会覆盖原型场景/物品预制体。正常继续编辑请直接打开现有场景。

## 验证
Unity 6000.0.66f2 独立验证副本已通过编译与无画面 Play Mode 自动检查：实际道具碰撞、效果刷新与到期、加减速同时生效、三发不同方向子弹、普通射击恢复、两只鸽子共享冷却、两种敌人击杀分数、模拟WASD/方向键移动与空格连射、边界、鸽子追踪与冲刺触发、阶段间隔、生成、重开和计时结束清理。
结束流程用缩短时长触发，交付场景仍为60秒。验证副本使用本机已有 Input System 和 UGUI 包；未修改原项目的包或渲染设置。尚未人工视觉验收、实机帧率测试或Web导出测试。
建议组员运行完整60秒，确认Game视图16:9、两种新图案显示正确、三种状态提示清晰、最终15秒难度合适。

## 给组员与导出
在仓库点击 Code → Download ZIP，解压后将含 Assets、Packages、ProjectSettings 的文件夹添加到 Unity Hub。也可以用 Git 克隆仓库；首次打开需等待资源导入。公开仓库允许所有人查看和下载，但直接推送修改仍需仓库所有者添加协作者。
Web导出：在Unity Hub安装对应版本的Web Build Support，File → Build Profiles → Web，加入且只勾选CosmicBatter场景，Switch Platform后Build。用HTTP服务或游戏平台打开导出结果，不要双击HTML。


