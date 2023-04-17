WildPerception 是一个利用 [Unity Perception Package](https://github.com/Unity-Technologies/com.unity.perception) 来生成大规模多视角视频数据集的工具。

其允许用户导入自己的 Humanoid 人物模型，自定义场景，配合 [MultiviewX_Perception](https://github.com/TsingLoo/MultiviewX_Perception) 可以得到符合 Wildtrack 格式的数据集。 

注意：

1. 原 MultiviewX_FYP 现更名为 [MultiviewX_Perception](https://github.com/TsingLoo/MultiviewX_Perception)
2. 原 [CalibrateTool](http://www.tsingloo.com/2023/03/01/0a2bf39019914a06954a4506b9f0ca37/) 已集成到了此处，将不再独立导出
3. 开发使用的 Editor 版本为 2022.2.12，不保证之前版本（尤其是2022.2之前）的表现。 Perception 要求一定版本的 HDRP 。

# Import

有两种方法可以将 WildPerception 包添加到您的项目中。

注意：

1. 由于需要安装相关依赖，应该在联网环境中导入此包

## [Method 1] Add package from git URL

注意：

1. *这需要您的设备有 git 环境*，可以去这里安装：[git](https://git-scm.com/download/win) 。
1. 由于目前 Package Manager 的限制，若如此做不可打开示例场景 SampleScene_WildPerception，但不影响其他功能。

Unity Editor 中打开 Window -> Package Manager

![Add package from git URL](http://images.tsingloo.com/image-20230417090347906.png)

复制本项目地址，填写并添加

![复制本项目地址](http://images.tsingloo.com/image-20230417090700275.png)

![填写并添加](http://images.tsingloo.com/image-20230417090929050.png)

大约三到五分钟后，添加完成，Unity 开始导入新包。

## [Method 2] Add package from disk

通过 git clone 或者直接从 github 上下载ZIP文件，或者从此处[Download WildPerception] 下载包。

![直接从 github 上下载ZIP文件](http://images.tsingloo.com/image-20230417092638895.png)

将此ZIP文件解压，放到非项目Assets文件夹中（在项目文件夹外亦可）

![放到非项目Assets文件夹中](http://images.tsingloo.com/image-20230417092859578.png)

Unity Editor 中打开 Window -> Package Manager

![Add package from disk](http://images.tsingloo.com/image-20230417093010369.png)

将package.json选中并确定。

![将package.json选中](http://images.tsingloo.com/image-20230417093130256.png)

大约两分钟后，添加完成，Unity 开始导入新包。

# Setup

您可以很快赋予您的场景生成序列帧的能力，只需要简单的几步配置。

若您使用的是方法2来添加此包，不妨打开场景 Packages -> WildPerception -> Sample -> SampleScene_WildPerception。 这个场景中比较简洁，如下图所示，有预制体**SceneController** 和一些GameObject 

![需要用到的预制体及生成的GameObject](http://images.tsingloo.com/image-20230417093834342.png)

若您想使用自己搭建的场景，请为这个场景添加预制体 **SceneController**

![为这个场景添加预制体 SceneController](http://images.tsingloo.com/image-20230417094125273.png)

## SceneController

SceneController 集成了所有的配置与功能。

场景导入 SceneController 后，可以打开其Inspector面板，请按照您的情况配置。

### Main Controller

![MainController 是 SceneController上挂载的一个组件](http://images.tsingloo.com/image-20230417100723071.png)

点击 **Init Scene**，场景中会自动生成所需的GameObject，请在场景中将这些 GameObject 置于所需的位置，随后点击 **Assign Transfrom** 

请设置 MultiviewX_Perception 项目的**绝对路径**，

请设置 Human Models_Folder 的**绝对路径**，此**路径务必在一个名为Resources的文件夹下**，**且此文件夹中有且仅有人物模型的预制体(.prefab)，而非.fbx等模型文件**。若您的项目中还没有人物模型，可以使用示例模型，其在 *com.tsingloo.wildperception-main\Resources\Models* 下，请使用其绝对路径。

![设置相关路径](http://images.tsingloo.com/image-20230417101204775.png)

对于人物模型，仅仅要求其具有 Humanoid 骨骼，并带有 Animator组件，其 Runtime Animator Controller 可以为空，若为空，将会在其生成时使用您在 **People Manager** 中配置的默认Runtime Animator Controller。

![仅仅要求其具有 Humanoid 骨骼，并带有 Animator 组件](http://images.tsingloo.com/image-20230417102009639.png)

注意：

1. 请尽量**保证GridOrigin_OpenCV的纵坐标(Y)与Center_HumanSpawn_CameraLookAt的纵坐标(Y)相同**，否则可能会出现标定不准确的情况，这个问题可能会在后续工作中修复。
2. 请**确保场景中供人物模型行走的平面携带有 NavMeshSurface** 组件，**并已完成烘焙**，若您未在 Add Component 中找到这个组件，请前往安装 Package Manager -> Packages: Unity Registry -> AI Navigation，此处使用的版本是 1.1.1

![供人物模型行走的平面携带有 NavMeshSurface 组件](http://images.tsingloo.com/image-20230417100228008.png)



## Camera Manager

Camera Manager 管理并控制着相机相关的内容，您可以在这里配置将在场景运行多少帧后开始导出序列帧(Begin Frame Count)、相机的位置类型（自动生成 Ellipse_Auto 或者手动摆放 By Hand）、相机的自动生成参数 Ellipse_Auto Settings

### Camera Place Type 
若您希望程序自动生成相机，请使用Auto

若您希望手动放置相机，请使用 By Hand
并从菜单中添加相机，为这个场景添加相机预制体 **Camera_Perception**，并将预制体放在HandPlacedCameraParent下

![为这个场景添加相机预制体](http://images.tsingloo.com/image-20230417094125273.png)

![将相机预制体放在HandPlacedCameraParent下](http://images.tsingloo.com/image-20230417111802003.png)

### Ellipse_Auto Settings

此处您可以配置相机的自动生成参数。

| 参数名             | 备注                                                        |
| ------------------ | ----------------------------------------------------------- |
| Level              | 有几层相机                                                  |
| Nums Per Level     | 每层多少个相机                                              |
| Height First Level | 第一层离地(LookAt) 多少垂直高度（已经换算为了OpenCV下长度） |
| H Per Level        | 若有多层，每层层高（已经换算为了OpenCV下长度）              |
| Major Axis         | 椭圆主轴长度 （已经换算为了OpenCV下长度）                   |
| Minor Axis         | 椭圆副轴长度 （已经换算为了OpenCV下长度）                   |
| Camera Prefab      | 将会被自动生成的相机的预制体                                |



## People Manager 

此处您可以配置人物生成的相关参数。

| 参数名                  | 备注                                                         |
| ----------------------- | ------------------------------------------------------------ |
| Default Animator        | 人物模型使用的默认动画控制器                                 |
| Add_human_count         | 每次敲击空格将会新生成几个人物模型                           |
| Preset_humans           | 场景初始化后生成多少个模型                                   |
| Largest，Smallest，X，Y | 规定初始化生成模型的区域（绿色矩形辅助线）                   |
| Outter Bound Radius     | 人物模型回收边界，**请设置大一些，务必使此边界不与 Grid 相交**，否则可能出现Editor 卡死，这个问题会在后续工作中修复 |



## CalibrateTool 

用于相机的标定，为 MultiviewX_Perception 提供数据，请见：[CalibrateTool](http://www.tsingloo.com/2023/03/01/0a2bf39019914a06954a4506b9f0ca37/)