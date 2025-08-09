# 墓园生成器 – 程序化墓地模拟 | Graveyard Generator – Procedural Cemetery Simulation

[Itch.io 在线试玩 / Play Online](https://selfer-li.itch.io/cemetery-simulator)  
[YouTube 视频演示 / Watch on YouTube](https://youtu.be/CzQLpmNBs8s)  

## Table of Contents
- [中文介绍](#中文介绍)
- [English Overview](#english-overview)

---

## 中文介绍

墓园生成器 是一个使用 Unity 制作的 程序化 3D 墓地生成与模拟项目，包含动态环境创建、季节变化、访客 AI 行为和夜间幽灵系统。本项目在课程作业基础上延伸为个人独立开发版本。  

### 主要玩法与功能
- 程序化生成：可调节墓园尺寸、围栏样式、树木/花草密度与季节，自动生成完整布局。  
- 访客 AI 系统：白天访客进入墓园，寻找墓碑、献上贡品并离开。  
- 供奉与幽灵机制：贡品价值越高，夜晚对应墓碑生成幽灵的概率越大。  
- 幽灵互动系统：幽灵会接受贡品，并根据贡品类型说出不同台词；只有接受特定贡品组合的幽灵才会触发特殊台词，从而解锁特殊幽灵图鉴。  
- 季节变化与环境特效：不同季节生成不同类型的树木与花草，搭配落叶、雪花等粒子效果。  
- 任务与事件系统：链式任务结构，每个阶段包含交互触发器与可视物体。  

### 开发信息
- 引擎：Unity  
- 编程语言：C#  
- 开发周期：课程项目延伸的个人独立作品  

### 我的职责（个人作品）
- 环境生成：实现墓碑、围栏、地面、季节性树木与花草的程序化生成逻辑。  
- AI 系统：基于状态机实现访客与幽灵的行为控制，结合 NavMesh 实现路径规划与游荡。  
- 交互与任务逻辑：链式任务结构、供奉系统、贡品概率计算与触发事件控制。  
- 特殊事件：实现幽灵交互对话、接受贡品、根据贡品类型触发不同台词，以及通过特定贡品组合解锁特殊幽灵与“死者图鉴”。  
- 用户界面（UI）：开发参数调整面板、昼夜切换、信息显示与图鉴查看功能。  
- 游戏流程：管理昼夜循环、访客与幽灵切换、事件触发与重置机制。  

### 代码结构与模块

#### 场景生成 / 导航
- GraveyardGenerator.cs：墓园主生成器，支持地面、围栏、季节植被、石板路、Spawn/Exit 点生成，以及双 NavMesh 烘焙（幽灵仅内层、访客内外层）。  
- FenceStyleSet.cs：定义直段、门段、拐角等围栏样式集。  
- NavMeshAreaSetter.cs：编辑器工具，批量为物体设置 NavMesh 区域索引。  

#### 墓碑与装饰
- GraveyardContentGenerator.cs：按权重生成普通/特殊墓碑与装饰；首行交替布局，棋盘错位排列；自动添加 GraveData。  
- GraveData.cs：记录墓碑参数与贡品信息；供奉品按墓碑朝向在 120 度弧形区域摆放。  

#### 白天访客系统（AI / 交互）
- VisitorManager.cs：控制访客生成频率、最大数量、强制离场等逻辑。  
- DayVisitorAgent.cs：访客 AI 状态机（Idle → 选墓 → 前往 → 供奉 → 离场）；加权随机选墓，供奉概率判定，支持外部召回。  
- DayVisitorAnimatorController.cs：控制访客行走与供奉动画。  

#### 夜晚幽灵系统（AI / 事件）
- NightModeManager.cs：根据白天贡品数量生成幽灵（3 个及以上必出，2 个 70%，1 个 35%），绑定墓碑与游荡范围，生成始终朝向相机的对白 UI。  
- GhostController.cs：幽灵 AI 状态机（Idle、Wander、ReceiveOffering、TalkWithGhost、Vanish）；接受不同贡品会说出不同台词；特定贡品组合触发特殊台词并解锁“死者图鉴”。  
- FaceCamera.cs：对白 UI 始终朝向相机。  

#### 运行时 UI / 游戏流程
- GraveyardUIController.cs：运行时参数面板（墓园尺寸、植被密度、围栏样式、季节、昼夜滑条）；确保尺寸为奇数；冬季隐藏花密度选项；支持开始/返回切换、暂停时间、清空幽灵与访客。  

---

## English Overview

Graveyard Generator is a procedural 3D cemetery generation and simulation project built in Unity, featuring dynamic environment creation, seasonal variation, AI-driven NPC visitors, and nighttime ghost interactions. This project began as a coursework assignment and was further developed into a solo project.  

### Main Features
- Procedural Generation: Adjustable cemetery size, fence style, tree/flower density, and season to generate a complete environment automatically.  
- Visitor AI System: Visitors enter during the day, select graves, place offerings, and leave.  
- Offering and Ghost Mechanics: Higher offering value increases ghost spawn probability at night.  
- Ghost Interaction System: Ghosts respond with different dialogue lines depending on the type of offerings they receive; only ghosts receiving specific offering combinations will trigger special dialogue that unlocks unique Ghost Codex entries.  
- Seasonal Variation and Effects: Different vegetation for each season, plus effects like falling leaves and snow.  
- Task and Event System: Chain-based task progression with interactive triggers and visual objects.  

### Development
- Engine: Unity  
- Programming Language: C#  
- Development Time: Extended solo version of a coursework project  

### My Contributions (Solo Project)
- Procedural Environment: Graves, fences, ground, seasonal trees, and flowers.  
- AI Systems: State machine–driven visitors and ghosts with NavMesh pathfinding and wandering logic.  
- Interaction and Task Logic: Chained tasks, offering system, and probability-based events.  
- Special Events: Implemented ghost dialogues that change based on offerings, and special offering combinations that unlock unique ghosts in the Ghost Codex.  
- User Interface (UI): Parameter panels, day-night cycle controls, information displays, and codex viewing.  
- Game Flow: Day-night transitions, visitor/ghost switching, event triggers, and reset functions.  

### Code Structure and Modules

#### Scene Generation / Navigation
- GraveyardGenerator.cs: Main generator for ground, fences, seasonal vegetation, stone paths, spawn/exit points; dual NavMesh for ghosts (inner area) and visitors (inner and outer areas).  
- FenceStyleSet.cs: Defines fence style sets for straight, gate, and corner segments.  
- NavMeshAreaSetter.cs: Editor utility for assigning NavMesh area indexes.  

#### Graves and Decorations
- GraveyardContentGenerator.cs: Weighted random selection of graves and decorations; alternating row placement; automatic GraveData attachment.  
- GraveData.cs: Stores grave settings and offerings; arranges offerings in a 120 degree arc in front of the grave.  

#### Daytime Visitor System (AI / Interaction)
- VisitorManager.cs: Controls visitor spawn frequency, maximum concurrent visitors, and forced removal.  
- DayVisitorAgent.cs: Visitor AI state machine (Idle → SelectGrave → NavigateToGrave → Offer → Exit); weighted random grave selection; probability-based offerings; forced recall support.  
- DayVisitorAnimatorController.cs: Controls walking and offering animations.  

#### Nighttime Ghost System (AI / Events)
- NightModeManager.cs: Spawns ghosts based on offerings (3 or more guaranteed, 2 = 70%, 1 = 35%); assigns graves and wander bounds; creates always-facing-camera dialogue bubbles.  
- GhostController.cs: Ghost AI state machine (Idle, Wander, ReceiveOffering, TalkWithGhost, Vanish); different offerings trigger different dialogue lines; specific offering combinations trigger special lines that unlock unique Ghost Codex entries.  
- FaceCamera.cs: Keeps dialogue UI facing the camera.  

#### Runtime UI / Game Flow
- GraveyardUIController.cs: Parameter panel for size, vegetation density, fence style, season, and day-night cycle; ensures odd-number dimensions; hides flower slider in winter; manages start/return transitions, pausing, and cleanup of visitors and ghosts.  
