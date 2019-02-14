unity3d_nav_critterai
=============

## 原名cai-nav
KBEngine在u3d项目中的演示使用, 对导出部分做了一点修改。

## 编译(Unity): (注意：unity3d-x-64bit中已经有编译好的文件， 没有做源码修改的话无需重新编译，直接使用即可)
	
	1: vs2013及以上打开sources\build\unity\cai-navigation-u3d.sln

	2: 设置每个子项目的References，添加Unity库引用:
		Unity\Editor\Data\Managed\UnityEditor.dll
		Unity\Editor\Data\Managed\UnityEngine.dll

	3: 编译，并且将相关文件拷贝到unity3d_nav_critterai\unity3d-x.x（具体文件参考已经编译好的unity3d-5.x-64bit中的内容）

## 使用方法参考项目:
	
	1：将unity3d_nav_critterai\unity3d-x.x\Assets拷贝到Unity项目对应的Assets中
	2：打开Unity3D创建一个新的3D游戏项目并且在游戏场景中添加一个地形与天空盒子，地形创建后在项目中资源名称叫“New Terrain.asset”
	3：将unity3d/Assets目录下的所有目录与文件拷贝到你的Unity3D游戏项目对应的Assets下，现在我们的编辑器效果与游戏资产库文件夹中内容如下图

![cainav1](https://kbengine.github.io/assets/img/screenshots/cainav1.jpg)

	4：在游戏项目菜单中选择（CritterAI->Create NMGen Assets->Navmesh Build : Standard）初始化，初始化完毕后
	项目目录中将出现几个文件，他们如下:
		CAIBakedNavmesh.asset
		MeshCompiler.asset
		NavmeshBuild.asset
	
	5：添加一个能生成地形寻路网格的Compiler，（CritterAI->Create NMGen Assets->Compiler : Terrain）

![cainav2](https://kbengine.github.io/assets/img/screenshots/cainav2.jpg)

	我们还需要将我们之前创建的地形绑定到TerrainCompiler上。

![cainav3](https://kbengine.github.io/assets/img/screenshots/cainav3.jpg)

	6：开始生成Navmesh

![cainav4](https://kbengine.github.io/assets/img/screenshots/cainav4.jpg)

	7：导出为文件，此时会出现2个文件，其中“srv_”开头的文件用于服务端寻路，另一个则可用于客户端使用该插件来寻路。

![cainav5](https://kbengine.github.io/assets/img/screenshots/cainav5.jpg)

	(注意: 生成完毕后建议删除Unity3D项目中Assets\Plugins下关于CAINav的文件，否则启动游戏会造成无法导出游戏的错误，原因未知。)

	8：将“srv_”这个文件拷贝到服务端资产目录，例如：“D:\kbe\kbengine\kbengine_demos_assets\res\spaces\xinshoucun”
	重启服务端后，服务端该场景会加载这个资源用于寻路（注意：要正确寻路服务端实体必须在有效坐标范围内，即必须在Navmesh地表上）

	（更多功能请参考该插件官网：http://www.critterai.org/projects/cainav/）


## 演示项目参考：

	https://github.com/kbengine/kbengine_unity3d_warring




