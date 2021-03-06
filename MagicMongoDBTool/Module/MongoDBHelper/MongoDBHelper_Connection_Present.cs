﻿using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TreeViewColumnsProject;

namespace MagicMongoDBTool.Module
{
    public static partial class MongoDBHelper
    {
        /// <summary>
        /// get current Server Information
        /// </summary>
        /// <returns></returns>
        public static String GetCurrentSvrInfo()
        {
            String rtnSvrInfo = String.Empty;
            MongoServer mongosvr = SystemManager.GetCurrentServer();
            rtnSvrInfo = "IsArbiter：" + mongosvr.Instance.IsArbiter.ToString() + System.Environment.NewLine;
            rtnSvrInfo += "IsPrimary：" + mongosvr.Instance.IsPrimary.ToString() + System.Environment.NewLine;
            rtnSvrInfo += "IsSecondary：" + mongosvr.Instance.IsSecondary.ToString() + System.Environment.NewLine;
            rtnSvrInfo += "Address：" + mongosvr.Instance.Address.ToString() + System.Environment.NewLine;
            if (mongosvr.Instance.BuildInfo != null)
            {
                //Before mongo2.0.2 BuildInfo will be null without auth
                rtnSvrInfo += "VersionString：" + mongosvr.Instance.BuildInfo.VersionString + System.Environment.NewLine;
                rtnSvrInfo += "SysInfo：" + mongosvr.Instance.BuildInfo.SysInfo + System.Environment.NewLine;
            }
            return rtnSvrInfo;
        }

        #region"展示数据库结构 WebForm"
        /// <summary>
        /// 获取JSON
        /// </summary>
        /// <param name="ConnectionName"></param>
        /// <returns></returns>
        public static String GetConnectionzTreeJSON()
        {
            TreeView tree = new TreeView();
            FillConnectionToTreeView(tree);
            return ConvertTreeViewTozTreeJson(tree);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="RootName"></param>
        /// <param name="doc"></param>
        /// <param name="IsOpen"></param>
        /// <returns></returns>
        public static String ConvertBsonTozTreeJson(String RootName, BsonDocument doc, Boolean IsOpen)
        {
            TreeViewColumns trvStatus = new TreeViewColumns();
            MongoDBHelper.FillDataToTreeView(RootName, trvStatus, doc);
            if (IsOpen)
            {
                trvStatus.TreeView.Nodes[0].Expand();
            }
            return ConvertTreeViewTozTreeJson(trvStatus.TreeView);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        public static String ConvertTreeViewTozTreeJson(TreeView tree)
        {
            BsonArray array = new BsonArray();
            foreach (TreeNode item in tree.Nodes)
            {
                array.Add(ConvertTreeNodeTozTreeBsonDoc(item));
            }
            return array.ToJson(SystemManager.JsonWriterSettings);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="SubNode"></param>
        /// <returns></returns>
        private static BsonDocument ConvertTreeNodeTozTreeBsonDoc(TreeNode SubNode)
        {
            BsonDocument SingleNode = new BsonDocument();
            SingleNode.Add("name", SubNode.Text + GetTagText(SubNode));
            if (SubNode.Nodes.Count == 0)
            {
                SingleNode.Add("icon", "MainTreeImage" + String.Format("{0:00}", SubNode.ImageIndex) + ".png");
            }
            else
            {
                BsonArray ChildrenList = new BsonArray();
                foreach (TreeNode item in SubNode.Nodes)
                {
                    ChildrenList.Add(ConvertTreeNodeTozTreeBsonDoc(item));
                }
                SingleNode.Add("children", ChildrenList);
                SingleNode.Add("icon", "MainTreeImage" + String.Format("{0:00}", SubNode.ImageIndex) + ".png");
            }
            if (SubNode.IsExpanded)
            {
                SingleNode.Add("open", "true");
            }
            if (SubNode.Tag != null)
            {
                SingleNode.Add("click", "ShowData('" + SystemManager.GetTagType(SubNode.Tag.ToString()) + "','" + SystemManager.GetTagData(SubNode.Tag.ToString()) + "')");
            }
            return SingleNode;
        }
        /// <summary>
        /// 展示数据值和类型
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private static string GetTagText(TreeNode node)
        {
            string strColumnText = String.Empty;
            BsonElement Element = node.Tag as BsonElement;
            if (Element != null && !Element.Value.IsBsonDocument && !Element.Value.IsBsonArray)
            {
                strColumnText = ":" + Element.Value.ToString();
                strColumnText += "[" + Element.Value.GetType().Name.Substring(4) + "]";
            }
            return strColumnText;
        }
        #endregion

        #region"展示数据库结构 Winform"
        /// <summary>
        /// 将Mongodb的服务器在树形控件中展示
        /// </summary>
        /// <param name="trvMongoDB"></param>
        public static void FillConnectionToTreeView(TreeView trvMongoDB)
        {
            trvMongoDB.Nodes.Clear();
            foreach (String mongoConnKey in _mongoConnSvrLst.Keys)
            {
                MongoServer mongoConn = _mongoConnSvrLst[mongoConnKey];
                TreeNode ConnectionNode = new TreeNode();
                try
                {
                    //ReplSetName只能使用在虚拟的Replset服务器，Sharding体系等无效。虽然一个Sharding可以看做一个ReplSet
                    ConfigHelper.MongoConnectionConfig config = SystemManager.ConfigHelperInstance.ConnectionList[mongoConnKey];
                    ConnectionNode.SelectedImageIndex = (int)GetSystemIcon.MainTreeImageType.Connection;
                    ConnectionNode.ImageIndex = (int)GetSystemIcon.MainTreeImageType.Connection;
                    //ReplSet服务器需要Connect才能连接。可能因为这个是虚拟的服务器，没有Mongod实体。
                    //不过现在改为全部显示的打开连接
                    mongoConn.Connect();
                    ///mongoSvr.ReplicaSetName只有在连接后才有效，但是也可以使用Config.ReplsetName
                    ConnectionNode.Text = mongoConnKey;
                    ConnectionNode.Nodes.Add(GetInstanceNode(mongoConnKey, config, mongoConn, null, mongoConn));
                    if (mongoConn.ReplicaSetName != null)
                    {
                        ConnectionNode.Tag = CONNECTION_REPLSET_TAG + ":" + config.ConnectionName;
                        TreeNode ServerListNode = new TreeNode("Servers");
                        ServerListNode.SelectedImageIndex = (int)GetSystemIcon.MainTreeImageType.Servers;
                        ServerListNode.ImageIndex = (int)GetSystemIcon.MainTreeImageType.Servers;
                        foreach (MongoServerInstance ServerInstace in mongoConn.Instances)
                        {
                            ServerListNode.Nodes.Add(GetInstanceNode(mongoConnKey, config, mongoConn, ServerInstace, null));
                        }
                        ConnectionNode.Nodes.Add(ServerListNode);
                        config.ServerRole = ConfigHelper.SvrRoleType.ReplsetSvr;
                    }
                    else
                    {
                        BsonDocument ServerStatusDoc = ExecuteMongoSvrCommand(serverStatus_Command, mongoConn).Response;
                        if (ServerStatusDoc.GetElement("process").Value == ServerStatus_PROCESS_MONGOS)
                        {
                            //Shard的时候，必须将所有服务器的ReadPreferred设成可读
                            config.ServerRole = ConfigHelper.SvrRoleType.ShardSvr;
                            ConnectionNode.Tag = CONNECTION_CLUSTER_TAG + ":" + config.ConnectionName;
                            TreeNode ShardListNode = new TreeNode("Shards");
                            ShardListNode.SelectedImageIndex = (int)GetSystemIcon.MainTreeImageType.Servers;
                            ShardListNode.ImageIndex = (int)GetSystemIcon.MainTreeImageType.Servers;
                            foreach (var lst in GetShardInfo(mongoConn, "host"))
                            {
                                TreeNode ShardNode = new TreeNode();
                                ShardNode.Text = lst.Key;
                                ShardNode.SelectedImageIndex = (int)GetSystemIcon.MainTreeImageType.Servers;
                                ShardNode.ImageIndex = (int)GetSystemIcon.MainTreeImageType.Servers;
                                String strHostList = lst.Value.ToString();
                                String[] strAddress = strHostList.Split("/".ToCharArray());
                                String strAddresslst;
                                if (strAddress.Length == 2)
                                {
                                    //#1  replset/host:port,host:port
                                    ShardNode.Text += "[Replset:" + strAddress[0] + "]";
                                    strAddresslst = strAddress[1];
                                }
                                else
                                {
                                    //#2  host:port,host:port
                                    strAddresslst = strHostList;
                                }
                                foreach (String item in strAddresslst.Split(",".ToCharArray()))
                                {
                                    MongoClientSettings tinySetting = new MongoClientSettings();
                                    tinySetting.ConnectionMode = ConnectionMode.Direct;
                                    //防止无法读取Sharding状态。Sharding可能是一个Slaver
                                    tinySetting.ReadPreference = ReadPreference.PrimaryPreferred;
                                    tinySetting.ReplicaSetName = strAddress[0];
                                    MongoServerAddress tinyAddr;
                                    if (item.Split(":".ToCharArray()).Length == 2)
                                    {
                                        tinyAddr = new MongoServerAddress(item.Split(":".ToCharArray())[0], Convert.ToInt32(item.Split(":".ToCharArray())[1]));
                                    }
                                    else
                                    {
                                        tinyAddr = new MongoServerAddress(item.Split(":".ToCharArray())[0]);
                                    }
                                    tinySetting.Server = tinyAddr;
                                    //MongoServer tiny = MongoServer.Create(tinySetting);
                                    MongoServer tiny = new MongoClient(tinySetting).GetServer();
                                    ShardNode.Nodes.Add(GetInstanceNode(mongoConnKey, config, mongoConn, tiny.Instance, null));
                                }
                                ShardListNode.Nodes.Add(ShardNode);
                            }
                            ConnectionNode.Nodes.Add(ShardListNode);
                        }
                        else
                        {
                            ///Server Status mongod
                            ///Master - Slave 的判断
                            BsonElement replElement;
                            ServerStatusDoc.TryGetElement("repl", out replElement);
                            if (replElement == null)
                            {
                                config.ServerRole = ConfigHelper.SvrRoleType.DataSvr;
                            }
                            else
                            {
                                if (replElement.Value.AsBsonDocument.GetElement("ismaster").Value == BsonBoolean.True)
                                {
                                    config.ServerRole = ConfigHelper.SvrRoleType.MasterSvr;
                                }
                                else
                                {
                                    //ismaster 的值不一定是True和False...
                                    config.ServerRole = ConfigHelper.SvrRoleType.SlaveSvr;
                                }
                            }
                            ConnectionNode.Tag = CONNECTION_TAG + ":" + config.ConnectionName;
                        }
                    }
                    //设定是否可用
                    config.Health = true;
                    //设定版本
                    if (mongoConn.BuildInfo != null)
                    {
                        config.MongoDBVersion = mongoConn.BuildInfo.Version;
                    }
                    SystemManager.ConfigHelperInstance.ConnectionList[mongoConnKey] = config;
                    switch (config.ServerRole)
                    {
                        case ConfigHelper.SvrRoleType.DataSvr:
                            ConnectionNode.Text = "[Data]  " + ConnectionNode.Text;
                            break;
                        case ConfigHelper.SvrRoleType.ShardSvr:
                            ConnectionNode.Text = "[Cluster]  " + ConnectionNode.Text;
                            break;
                        case ConfigHelper.SvrRoleType.ReplsetSvr:
                            ConnectionNode.Text = "[Replset]  " + ConnectionNode.Text;
                            break;
                        case ConfigHelper.SvrRoleType.MasterSvr:
                            ConnectionNode.Text = "[Master]  " + ConnectionNode.Text;
                            break;
                        case ConfigHelper.SvrRoleType.SlaveSvr:
                            ConnectionNode.Text = "[Slave]  " + ConnectionNode.Text;
                            break;
                        default:
                            break;
                    }
                    trvMongoDB.Nodes.Add(ConnectionNode);
                }
                catch (MongoAuthenticationException ex)
                {
                    //需要验证的数据服务器，没有Admin权限无法获得数据库列表
                    if (!SystemManager.IsUseDefaultLanguage)
                    {
                        ConnectionNode.Text += "[" + SystemManager.mStringResource.GetText(StringResource.TextType.Exception_AuthenticationException) + "]";
                        SystemManager.ExceptionDeal(ex, SystemManager.mStringResource.GetText(StringResource.TextType.Exception_AuthenticationException),
                                                       SystemManager.mStringResource.GetText(StringResource.TextType.Exception_AuthenticationException_Note));
                    }
                    else
                    {
                        ConnectionNode.Text += "[MongoAuthenticationException]";
                        SystemManager.ExceptionDeal(ex, "MongoAuthenticationException:", "Please check UserName and Password");
                    }
                    ConnectionNode.Tag = CONNECTION_EXCEPTION_TAG + ":" + mongoConnKey;
                    trvMongoDB.Nodes.Add(ConnectionNode);
                }
                catch (Exception ex)
                {
                    //暂时不处理任何异常，简单跳过
                    //无法连接的理由：
                    //1.服务器没有启动
                    //2.认证模式不正确
                    if (!SystemManager.IsUseDefaultLanguage)
                    {
                        ConnectionNode.Text += "[" + SystemManager.mStringResource.GetText(StringResource.TextType.Exception_NotConnected) + "]";
                        SystemManager.ExceptionDeal(ex, SystemManager.mStringResource.GetText(StringResource.TextType.Exception_NotConnected),
                                                       SystemManager.mStringResource.GetText(StringResource.TextType.Exception_NotConnected_Note));
                    }
                    else
                    {
                        ConnectionNode.Text += "[Exception]";
                        SystemManager.ExceptionDeal(ex, "Not Connected", "Mongo Server may not Startup or Auth Mode is not correct");
                    }
                    ConnectionNode.Tag = CONNECTION_EXCEPTION_TAG + ":" + mongoConnKey;
                    trvMongoDB.Nodes.Add(ConnectionNode);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mongoConnKey"></param>
        /// <param name="config"></param>
        /// <param name="mongoConn"></param>
        /// <param name="mServerInstace"></param>
        /// <param name="mServer"></param>
        /// <returns></returns>
        private static TreeNode GetInstanceNode(String mongoConnKey, ConfigHelper.MongoConnectionConfig config,
                                                MongoServer mongoConn, MongoServerInstance mServerInstace, MongoServer mServer)
        {
            Boolean isServer = false;
            //无论如何，都改为主要服务器读优先
            if (mServerInstace == null)
            {
                isServer = true;
            }
            TreeNode SvrInstanceNode = new TreeNode();
            String ConnSvrKey;
            if (isServer)
            {
                ConnSvrKey = mongoConnKey + "/" + mongoConnKey;
            }
            else
            {
                ConnSvrKey = mongoConnKey + "/" + mServerInstace.Address.ToString().Replace(":", "@");
            }
            SvrInstanceNode.SelectedImageIndex = (int)GetSystemIcon.MainTreeImageType.WebServer;
            SvrInstanceNode.ImageIndex = (int)GetSystemIcon.MainTreeImageType.WebServer;
            if (isServer)
            {
                SvrInstanceNode.Text = "Connection";
            }
            else
            {
                SvrInstanceNode.Text = "Server[" + mServerInstace.Address.ToString() + "]";
            }
            if ((!String.IsNullOrEmpty(config.UserName)) & (!String.IsNullOrEmpty(config.Password)))
            {
                config.AuthMode = true;
            }
            //获取ReadOnly
            config.IsReadOnly = false;
            List<String> databaseNameList = new List<String>();
            if (!String.IsNullOrEmpty(config.DataBaseName))
            {
                //单数据库模式
                TreeNode mongoSingleDBNode;
                if (isServer)
                {
                    mongoSingleDBNode = FillDataBaseInfoToTreeNode(config.DataBaseName, mServer, mongoConnKey + "/" + mongoConnKey);
                }
                else
                {
                    mongoSingleDBNode = FillDataBaseInfoToTreeNode(config.DataBaseName, mServerInstace.Server, mongoConnKey + "/" + mServerInstace.Address.ToString());
                }
                mongoSingleDBNode.Tag = SINGLE_DATABASE_TAG + ":" + ConnSvrKey + "/" + config.DataBaseName;
                mongoSingleDBNode.SelectedImageIndex = (int)GetSystemIcon.MainTreeImageType.Database;
                mongoSingleDBNode.ImageIndex = (int)GetSystemIcon.MainTreeImageType.Database;
                SvrInstanceNode.Nodes.Add(mongoSingleDBNode);
                SvrInstanceNode.Tag = SINGLE_DB_SERVER_TAG + ":" + ConnSvrKey;
                if (config.AuthMode)
                {
                    config.IsReadOnly = mongoConn.GetDatabase(config.DataBaseName).FindUser(config.UserName).IsReadOnly;
                }
            }
            else
            {
                MongoServer InstantSrv;
                if (isServer)
                {
                    InstantSrv = mServer;
                    databaseNameList = mServer.GetDatabaseNames().ToList<String>();
                }
                else
                {
                    MongoServerSettings setting = mongoConn.Settings.Clone();
                    setting.ConnectionMode = ConnectionMode.Direct;
                    //When Replset Case,Application need to read admin DB information
                    //if Primary,there will be exception
                    setting.ReadPreference = ReadPreference.PrimaryPreferred;
                    setting.Server = mServerInstace.Address;
                    InstantSrv = new MongoServer(setting);
                    databaseNameList = InstantSrv.GetDatabaseNames().ToList<String>();
                }
                foreach (String strDBName in databaseNameList)
                {
                    TreeNode mongoDBNode;
                    try
                    {
                        mongoDBNode = FillDataBaseInfoToTreeNode(strDBName, InstantSrv, ConnSvrKey);
                        mongoDBNode.ImageIndex = (int)GetSystemIcon.MainTreeImageType.Database;
                        mongoDBNode.SelectedImageIndex = (int)GetSystemIcon.MainTreeImageType.Database;
                        SvrInstanceNode.Nodes.Add(mongoDBNode);
                        if (strDBName == MongoDBHelper.DATABASE_NAME_ADMIN)
                        {
                            if (config.AuthMode)
                            {
                                config.IsReadOnly = mongoConn.GetDatabase(strDBName).FindUser(config.UserName).IsReadOnly;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        SystemManager.ExceptionDeal(ex, strDBName + "Exception", strDBName + "Exception");
                        mongoDBNode = new TreeNode(strDBName + " (Exception)");
                        mongoDBNode.ImageIndex = (int)GetSystemIcon.MainTreeImageType.Database;
                        mongoDBNode.SelectedImageIndex = (int)GetSystemIcon.MainTreeImageType.Database;
                        SvrInstanceNode.Nodes.Add(mongoDBNode);
                    }
                }
                if (isServer)
                {
                    SvrInstanceNode.Tag = SERVER_TAG + ":" + mongoConnKey + "/" + mongoConnKey;
                }
                else
                {
                    if (mongoConn.ReplicaSetName != null)
                    {
                        SvrInstanceNode.Tag = SERVER_REPLSET_MEMBER_TAG + ":" + mongoConnKey + "/" + mServerInstace.Address.ToString().Replace(":", "@");
                    }
                }
            }
            if (_mongoInstanceLst.ContainsKey(ConnSvrKey))
            {
                _mongoInstanceLst.Remove(ConnSvrKey);
            }
            if (!isServer)
            {
                _mongoInstanceLst.Add(ConnSvrKey, mServerInstace);
            }
            return SvrInstanceNode;
        }
        /// <summary>
        /// 获得一个表示数据库结构的节点
        /// </summary>
        /// <param name="strDBName"></param>
        /// <param name="mongoSvr"></param>
        /// <param name="mongoSvrKey"></param>
        /// <returns></returns>
        private static TreeNode FillDataBaseInfoToTreeNode(String strDBName, MongoServer mongoSvr, String mongoSvrKey)
        {
            String strShowDBName = strDBName;
            if (!SystemManager.IsUseDefaultLanguage)
            {
                if (SystemManager.mStringResource.LanguageType == "Chinese")
                {
                    switch (strDBName)
                    {
                        case "admin":
                            strShowDBName = "管理员权限(admin)";
                            break;
                        case "local":
                            strShowDBName = "本地(local)";
                            break;
                        case "config":
                            strShowDBName = "配置(config)";
                            break;
                        default:
                            break;
                    }
                }
            }
            TreeNode mongoDBNode = new TreeNode(strShowDBName);
            mongoDBNode.Tag = DATABASE_TAG + ":" + mongoSvrKey + "/" + strDBName;
            MongoDatabase mongoDB = mongoSvr.GetDatabase(strDBName);

            TreeNode UserNode = new TreeNode("User", (int)GetSystemIcon.MainTreeImageType.UserIcon, (int)GetSystemIcon.MainTreeImageType.UserIcon);
            UserNode.Tag = USER_LIST_TAG + ":" + mongoSvrKey + "/" + mongoDB.Name + "/" + COLLECTION_NAME_USER;
            mongoDBNode.Nodes.Add(UserNode);

            TreeNode JsNode = new TreeNode("JavaScript", (int)GetSystemIcon.MainTreeImageType.JavaScriptList, (int)GetSystemIcon.MainTreeImageType.JavaScriptList);
            JsNode.Tag = JAVASCRIPT_TAG + ":" + mongoSvrKey + "/" + mongoDB.Name + "/" + COLLECTION_NAME_JAVASCRIPT;
            mongoDBNode.Nodes.Add(JsNode);

            TreeNode GFSNode = new TreeNode("Grid File System", (int)GetSystemIcon.MainTreeImageType.GFS, (int)GetSystemIcon.MainTreeImageType.GFS);
            GFSNode.Tag = GRID_FILE_SYSTEM_TAG + ":" + mongoSvrKey + "/" + mongoDB.Name + "/" + COLLECTION_NAME_GFS_FILES;
            mongoDBNode.Nodes.Add(GFSNode);

            TreeNode mongoSysColListNode = new TreeNode("Collections(System)", (int)GetSystemIcon.MainTreeImageType.SystemCol, (int)GetSystemIcon.MainTreeImageType.SystemCol);
            mongoSysColListNode.Tag = SYSTEM_COLLECTION_LIST_TAG + ":" + mongoSvrKey + "/" + mongoDB.Name;
            mongoDBNode.Nodes.Add(mongoSysColListNode);

            TreeNode mongoColListNode = new TreeNode("Collections(General)", (int)GetSystemIcon.MainTreeImageType.CollectionList, (int)GetSystemIcon.MainTreeImageType.CollectionList);
            mongoColListNode.Tag = COLLECTION_LIST_TAG + ":" + mongoSvrKey + "/" + mongoDB.Name;
            List<String> colNameList = mongoDB.GetCollectionNames().ToList<String>();
            foreach (String strColName in colNameList)
            {
                switch (strColName)
                {
                    case COLLECTION_NAME_USER:
                        //system.users,fs,system.js这几个系统级别的Collection不需要放入
                        break;
                    case COLLECTION_NAME_JAVASCRIPT:
                        foreach (BsonDocument t in mongoDB.GetCollection(COLLECTION_NAME_JAVASCRIPT).FindAll())
                        {
                            TreeNode js = new TreeNode(t.GetValue(KEY_ID).ToString());
                            js.ImageIndex = (int)GetSystemIcon.MainTreeImageType.JsDoc;
                            js.SelectedImageIndex = (int)GetSystemIcon.MainTreeImageType.JsDoc;
                            js.Tag = JAVASCRIPT_DOC_TAG + ":" + mongoSvrKey + "/" + mongoDB.Name + "/" + COLLECTION_NAME_JAVASCRIPT + "/" + t.GetValue(KEY_ID).ToString();
                            JsNode.Nodes.Add(js);
                        }
                        break;
                    default:
                        TreeNode mongoColNode = new TreeNode();
                        try
                        {
                            mongoColNode = FillCollectionInfoToTreeNode(strColName, mongoDB, mongoSvrKey);
                        }
                        catch (Exception ex)
                        {
                            mongoColNode = new TreeNode(strColName + "[exception:]");
                            mongoColNode.ImageIndex = (int)GetSystemIcon.MainTreeImageType.Err;
                            mongoColNode.SelectedImageIndex = (int)GetSystemIcon.MainTreeImageType.Err;
                            SystemManager.ExceptionDeal(ex);
                        }
                        if (IsSystemCollection(mongoDB.Name, strColName))
                        {
                            switch (strColName)
                            {
                                case COLLECTION_NAME_GFS_CHUNKS:
                                case COLLECTION_NAME_GFS_FILES:
                                    GFSNode.Nodes.Add(mongoColNode);
                                    break;
                                default:
                                    mongoSysColListNode.Nodes.Add(mongoColNode);
                                    break;
                            }
                        }
                        else
                        {
                            mongoColListNode.Nodes.Add(mongoColNode);
                        }
                        break;
                }
            }
            mongoDBNode.Nodes.Add(mongoColListNode);


            mongoDBNode.ImageIndex = (int)GetSystemIcon.MainTreeImageType.Database;
            mongoDBNode.SelectedImageIndex = (int)GetSystemIcon.MainTreeImageType.Database;
            return mongoDBNode;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="strShowColName"></param>
        /// <param name="mongoDB"></param>
        /// <param name="mongoConnSvrKey"></param>
        /// <returns></returns>
        private static TreeNode FillCollectionInfoToTreeNode(String strColName, MongoDatabase mongoDB, String mongoConnSvrKey)
        {
            String strShowColName = strColName;
            if (!SystemManager.IsUseDefaultLanguage)
            {
                switch (strShowColName)
                {
                    case "chunks":
                        if (mongoDB.Name == "config")
                        {
                            strShowColName = SystemManager.mStringResource.GetText(StringResource.TextType.SYSTEMC_COLNAME_chunks) + "(" + strShowColName + ")";
                        }
                        break;
                    case "collections":
                        if (mongoDB.Name == "config")
                        {
                            strShowColName = SystemManager.mStringResource.GetText(StringResource.TextType.SYSTEMC_COLNAME_collections) + "(" + strShowColName + ")";
                        }
                        break;
                    case "changelog":
                        if (mongoDB.Name == "config")
                        {
                            strShowColName = SystemManager.mStringResource.GetText(StringResource.TextType.SYSTEMC_COLNAME_changelog) + "(" + strShowColName + ")";
                        }
                        break;
                    case "databases":
                        if (mongoDB.Name == "config")
                        {
                            strShowColName = SystemManager.mStringResource.GetText(StringResource.TextType.SYSTEMC_COLNAME_databases) + "(" + strShowColName + ")";
                        }
                        break;
                    case "lockpings":
                        if (mongoDB.Name == "config")
                        {
                            strShowColName = SystemManager.mStringResource.GetText(StringResource.TextType.SYSTEMC_COLNAME_lockpings) + "(" + strShowColName + ")";
                        }
                        break;
                    case "locks":
                        if (mongoDB.Name == "config")
                        {
                            strShowColName = SystemManager.mStringResource.GetText(StringResource.TextType.SYSTEMC_COLNAME_locks) + "(" + strShowColName + ")";
                        }
                        break;
                    case "mongos":
                        if (mongoDB.Name == "config")
                        {
                            strShowColName = SystemManager.mStringResource.GetText(StringResource.TextType.SYSTEMC_COLNAME_mongos) + "(" + strShowColName + ")";
                        }
                        break;
                    case "settings":
                        if (mongoDB.Name == "config")
                        {
                            strShowColName = SystemManager.mStringResource.GetText(StringResource.TextType.SYSTEMC_COLNAME_settings) + "(" + strShowColName + ")";
                        }
                        break;
                    case "shards":
                        if (mongoDB.Name == "config")
                        {
                            strShowColName = SystemManager.mStringResource.GetText(StringResource.TextType.SYSTEMC_COLNAME_shards) + "(" + strShowColName + ")";
                        }
                        break;
                    case "tags":
                        //ADD: 2013/01/04 Mongo2.2.2开始支持ShardTag了 
                        if (mongoDB.Name == "config")
                        {
                            strShowColName = SystemManager.mStringResource.GetText(StringResource.TextType.SYSTEMC_COLNAME_tags) + "(" + strShowColName + ")";
                        }
                        break;
                    case "version":
                        if (mongoDB.Name == "config")
                        {
                            strShowColName = SystemManager.mStringResource.GetText(StringResource.TextType.SYSTEMC_COLNAME_version) + "(" + strShowColName + ")";
                        }
                        break;
                    case "me":
                        if (mongoDB.Name == "local")
                        {
                            strShowColName = SystemManager.mStringResource.GetText(StringResource.TextType.SYSTEMC_COLNAME_me) + "(" + strShowColName + ")";
                        }
                        break;
                    case "sources":
                        if (mongoDB.Name == "local")
                        {
                            strShowColName = SystemManager.mStringResource.GetText(StringResource.TextType.SYSTEMC_COLNAME_sources) + "(" + strShowColName + ")";
                        }
                        break;
                    case "slaves":
                        if (mongoDB.Name == "local")
                        {
                            strShowColName = SystemManager.mStringResource.GetText(StringResource.TextType.SYSTEMC_COLNAME_slaves) + "(" + strShowColName + ")";
                        }
                        break;
                    case COLLECTION_NAME_GFS_CHUNKS:
                        strShowColName = SystemManager.mStringResource.GetText(StringResource.TextType.COLLECTION_NAME_GFS_CHUNKS) + "(" + strShowColName + ")";
                        break;
                    case COLLECTION_NAME_GFS_FILES:
                        strShowColName = SystemManager.mStringResource.GetText(StringResource.TextType.COLLECTION_NAME_GFS_FILES) + "(" + strShowColName + ")";
                        break;
                    case COLLECTION_NAME_OPERATION_LOG:
                        strShowColName = SystemManager.mStringResource.GetText(StringResource.TextType.COLLECTION_NAME_OPERATION_LOG) + "(" + strShowColName + ")";
                        break;
                    case COLLECTION_NAME_SYSTEM_INDEXES:
                        strShowColName = SystemManager.mStringResource.GetText(StringResource.TextType.COLLECTION_NAME_SYSTEM_INDEXES) + "(" + strShowColName + ")";
                        break;
                    case COLLECTION_NAME_JAVASCRIPT:
                        strShowColName = SystemManager.mStringResource.GetText(StringResource.TextType.COLLECTION_NAME_JAVASCRIPT) + "(" + strShowColName + ")";
                        break;
                    case COLLECTION_NAME_SYSTEM_REPLSET:
                        strShowColName = SystemManager.mStringResource.GetText(StringResource.TextType.COLLECTION_NAME_SYSTEM_REPLSET) + "(" + strShowColName + ")";
                        break;
                    case COLLECTION_NAME_REPLSET_MINVALID:
                        strShowColName = SystemManager.mStringResource.GetText(StringResource.TextType.COLLECTION_NAME_REPLSET_MINVALID) + "(" + strShowColName + ")";
                        break;
                    case COLLECTION_NAME_USER:
                        strShowColName = SystemManager.mStringResource.GetText(StringResource.TextType.COLLECTION_NAME_USER) + "(" + strShowColName + ")";
                        break;
                    case COLLECTION_NAME_SYSTEM_PROFILE:
                        strShowColName = SystemManager.mStringResource.GetText(StringResource.TextType.COLLECTION_NAME_SYSTEM_PROFILE) + "(" + strShowColName + ")";
                        break;
                    default:
                        break;
                }
            }
            TreeNode mongoColNode;
            mongoColNode = new TreeNode(strShowColName);
            switch (strColName)
            {
                case COLLECTION_NAME_GFS_FILES:
                    mongoColNode.Tag = GRID_FILE_SYSTEM_TAG + ":" + mongoConnSvrKey + "/" + mongoDB.Name + "/" + strColName;
                    break;
                case COLLECTION_NAME_USER:
                    mongoColNode.Tag = USER_LIST_TAG + ":" + mongoConnSvrKey + "/" + mongoDB.Name + "/" + strColName;
                    break;
                default:
                    mongoColNode.Tag = COLLECTION_TAG + ":" + mongoConnSvrKey + "/" + mongoDB.Name + "/" + strColName;
                    break;
            }

            MongoCollection mongoCol = mongoDB.GetCollection(strColName);

            //Start ListIndex
            TreeNode mongoIndexes = new TreeNode("Indexes");
            GetIndexesResult indexList = mongoCol.GetIndexes();
            foreach (IndexInfo indexDoc in indexList.ToList<IndexInfo>())
            {
                TreeNode mongoIndex = new TreeNode();
                if (!SystemManager.IsUseDefaultLanguage)
                {
                    mongoIndex.Text = (SystemManager.mStringResource.GetText(StringResource.TextType.Index_Name) + ":" + indexDoc.Name);
                    mongoIndex.Nodes.Add(String.Empty, SystemManager.mStringResource.GetText(StringResource.TextType.Index_Keys) + ":" +
                        GetKeyString(indexDoc.Key), (int)GetSystemIcon.MainTreeImageType.KeyInfo, (int)GetSystemIcon.MainTreeImageType.KeyInfo);
                    mongoIndex.Nodes.Add(String.Empty, SystemManager.mStringResource.GetText(StringResource.TextType.Index_RepeatDel) + ":" + indexDoc.DroppedDups.ToString(), (int)GetSystemIcon.MainTreeImageType.KeyInfo, (int)GetSystemIcon.MainTreeImageType.KeyInfo);
                    mongoIndex.Nodes.Add(String.Empty, SystemManager.mStringResource.GetText(StringResource.TextType.Index_Background) + ":" + indexDoc.IsBackground.ToString(), (int)GetSystemIcon.MainTreeImageType.KeyInfo, (int)GetSystemIcon.MainTreeImageType.KeyInfo);
                    mongoIndex.Nodes.Add(String.Empty, SystemManager.mStringResource.GetText(StringResource.TextType.Index_Sparse) + ":" + indexDoc.IsSparse.ToString(), (int)GetSystemIcon.MainTreeImageType.KeyInfo, (int)GetSystemIcon.MainTreeImageType.KeyInfo);
                    mongoIndex.Nodes.Add(String.Empty, SystemManager.mStringResource.GetText(StringResource.TextType.Index_Unify) + ":" + indexDoc.IsUnique.ToString(), (int)GetSystemIcon.MainTreeImageType.KeyInfo, (int)GetSystemIcon.MainTreeImageType.KeyInfo);
                    mongoIndex.Nodes.Add(String.Empty, SystemManager.mStringResource.GetText(StringResource.TextType.Index_NameSpace) + ":" + indexDoc.Namespace.ToString(), (int)GetSystemIcon.MainTreeImageType.KeyInfo, (int)GetSystemIcon.MainTreeImageType.KeyInfo);
                    mongoIndex.Nodes.Add(String.Empty, SystemManager.mStringResource.GetText(StringResource.TextType.Index_Version) + ":" + indexDoc.Version.ToString(), (int)GetSystemIcon.MainTreeImageType.KeyInfo, (int)GetSystemIcon.MainTreeImageType.KeyInfo);
                    if (indexDoc.TimeToLive == TimeSpan.MaxValue)
                    {
                        mongoIndex.Nodes.Add(String.Empty, SystemManager.mStringResource.GetText(StringResource.TextType.Index_ExpireData) + ":Not Set", (int)GetSystemIcon.MainTreeImageType.KeyInfo, (int)GetSystemIcon.MainTreeImageType.KeyInfo);
                    }
                    else
                    {
                        mongoIndex.Nodes.Add(String.Empty, SystemManager.mStringResource.GetText(StringResource.TextType.Index_ExpireData) + ":" + indexDoc.TimeToLive.TotalSeconds.ToString(), (int)GetSystemIcon.MainTreeImageType.KeyInfo, (int)GetSystemIcon.MainTreeImageType.KeyInfo);
                    }
                }
                else
                {
                    mongoIndex.Text = "IndexName:" + indexDoc.Name;
                    mongoIndex.Nodes.Add(String.Empty, "Keys:" + GetKeyString(indexDoc.Key), (int)GetSystemIcon.MainTreeImageType.KeyInfo, (int)GetSystemIcon.MainTreeImageType.KeyInfo);
                    mongoIndex.Nodes.Add(String.Empty, "DroppedDups :" + indexDoc.DroppedDups.ToString(), (int)GetSystemIcon.MainTreeImageType.KeyInfo, (int)GetSystemIcon.MainTreeImageType.KeyInfo);
                    mongoIndex.Nodes.Add(String.Empty, "IsBackground:" + indexDoc.IsBackground.ToString(), (int)GetSystemIcon.MainTreeImageType.KeyInfo, (int)GetSystemIcon.MainTreeImageType.KeyInfo);
                    mongoIndex.Nodes.Add(String.Empty, "IsSparse:" + indexDoc.IsSparse.ToString(), (int)GetSystemIcon.MainTreeImageType.KeyInfo, (int)GetSystemIcon.MainTreeImageType.KeyInfo);
                    mongoIndex.Nodes.Add(String.Empty, "IsUnique:" + indexDoc.IsUnique.ToString(), (int)GetSystemIcon.MainTreeImageType.KeyInfo, (int)GetSystemIcon.MainTreeImageType.KeyInfo);
                    mongoIndex.Nodes.Add(String.Empty, "NameSpace:" + indexDoc.Namespace.ToString(), (int)GetSystemIcon.MainTreeImageType.KeyInfo, (int)GetSystemIcon.MainTreeImageType.KeyInfo);
                    mongoIndex.Nodes.Add(String.Empty, "Version:" + indexDoc.Version.ToString(), (int)GetSystemIcon.MainTreeImageType.KeyInfo, (int)GetSystemIcon.MainTreeImageType.KeyInfo);
                    if (indexDoc.TimeToLive == TimeSpan.MaxValue)
                    {
                        mongoIndex.Nodes.Add(String.Empty, "Expire Data:Not Set", (int)GetSystemIcon.MainTreeImageType.KeyInfo, (int)GetSystemIcon.MainTreeImageType.KeyInfo);
                    }
                    else
                    {
                        mongoIndex.Nodes.Add(String.Empty, "Expire Data(sec):" + indexDoc.TimeToLive.TotalSeconds.ToString(), (int)GetSystemIcon.MainTreeImageType.KeyInfo, (int)GetSystemIcon.MainTreeImageType.KeyInfo);
                    }
                }
                mongoIndex.ImageIndex = (int)GetSystemIcon.MainTreeImageType.DBKey;
                mongoIndex.SelectedImageIndex = (int)GetSystemIcon.MainTreeImageType.DBKey;
                mongoIndex.Tag = INDEX_TAG + ":" + mongoConnSvrKey + "/" + mongoDB.Name + "/" + strColName + "/" + indexDoc.Name;
                mongoIndexes.Nodes.Add(mongoIndex);
            }
            mongoIndexes.ImageIndex = (int)GetSystemIcon.MainTreeImageType.Keys;
            mongoIndexes.SelectedImageIndex = (int)GetSystemIcon.MainTreeImageType.Keys;
            mongoIndexes.Tag = INDEXES_TAG + ":" + mongoConnSvrKey + "/" + mongoDB.Name + "/" + strColName;
            mongoColNode.Nodes.Add(mongoIndexes);
            //End ListIndex

            mongoColNode.ToolTipText = strColName + System.Environment.NewLine;
            mongoColNode.ToolTipText += "IsCapped:" + mongoCol.GetStats().IsCapped.ToString();

            if (strColName == COLLECTION_NAME_USER)
            {
                mongoColNode.ImageIndex = (int)GetSystemIcon.MainTreeImageType.UserIcon;
                mongoColNode.SelectedImageIndex = (int)GetSystemIcon.MainTreeImageType.UserIcon;
            }
            else
            {
                mongoColNode.ImageIndex = (int)GetSystemIcon.MainTreeImageType.Collection;
                mongoColNode.SelectedImageIndex = (int)GetSystemIcon.MainTreeImageType.Collection;
            }
            //End Data
            return mongoColNode;
        }

        public static string GetKeyString(IndexKeysDocument keys)
        {
            String KeyString = string.Empty;
            foreach (BsonElement key in keys.Elements)
            {
                KeyString += key.Name + ":";
                switch (key.Value.ToString())
                {
                    case "1":
                        KeyString += MongoDBHelper.IndexType.Ascending.ToString();
                        break;
                    case "-1":
                        KeyString += MongoDBHelper.IndexType.Descending.ToString();
                        break;
                    case "2d":
                        KeyString += MongoDBHelper.IndexType.GeoSpatial.ToString();
                        break;
                    default:
                        break;
                }
                KeyString += ";";
            }
            KeyString = "[" + KeyString.TrimEnd(";".ToArray()) + "]";
            return KeyString;
        }
        #endregion
    }
}
