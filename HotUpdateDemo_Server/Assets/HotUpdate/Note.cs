using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 打包之后Assets/StreamingAssets存放AssetBundle资源
// 打包之后StreamingAssets只能读不能写
// 热更新之前,检查StreamingAssets目录中的文件,再比较缓存中的文件
// 如果缓存没有或者版本一致,那么比较网络文件,如果网络文件版本高,那么下载网络文件到缓存
// 如果缓存中版本高,那么比较网络文件,如果网络文件版本与缓存版本一致,则使用缓存文件
// 缓存中存储打包后更新的文件,并不拥有所有AssetBundle


// VersionInfo
// 0 如果缓存中没有VersionInfo,那么复制本地VersionInfo到缓存中
//      对于初次启动游戏,缓存中没有VersionInfo的两种策略: 
//          1 复制本地到缓存,使后面流程正常
//          2 直接下载远端VersionInfo,FileInfo和所有的AssetBundle(不可取,因为本地已经有资源,不必下载所有资源)
// 1 加载缓存VersionInfo
// 2 下载远端VersionInfo(文件流中的文本,暂时不保存)
// 3 比较两者版本号
//      3.1 大版本号小于远端,应用更新
//      3.2 小版本号小于远端,热更新(有任何改动都要增加版号,即不存在版号一致而资源不一致的情况)

// FileInfo 
// 如果热更新,那么进行下面步骤
// 0 如果缓存中没有FileInfo,那么复制本地FileInfo到缓存中
// 1 加载缓存FileInfo并解析每个文件的Hash和Size
// 2 下载远端FileInfo并解析每个文件的Hash和Size(文件流中的文本,暂时不保存)
// 3 比较Hash和Size,有改动则将该文件URL加入下载队列

// AssetBundles
// 如果下载队列中有文件,那么进行以下步骤
// 1 下载文件并保存
// 2 等待下载队列清空
// 3 保存VersionInfo和FileInfo
// 4 热更结束,启动游戏

// 加载AssetBundle
// 1 根据Manifest获取依赖项AssetBundle名称
// 2 根据名称在上面步骤中的FileInfo中获取文件路径(本地或缓存)
// 3 加载需要的AssetBundle
// 4 实例化GameObject
// 5 卸载已经实例化的AssetBundle


//TJQ:
//1.load local version file
//2.request remote version file
//3.compare version to see we should use cold update or hot update or pass

//(hot update procedure)
//a.compare filesize and hash of all files to get a ready-to-update list
//b.download updated files if necessary
//c.waiting for download task and update progress
//d.launch game if hot update is finished

// 这个可能是大版本更新
//(cold update procedure)
//a.using message box to prevent users from entering game
//b.click "update", then redirect user to app store or other app market to update game
namespace HotFix
{

}
