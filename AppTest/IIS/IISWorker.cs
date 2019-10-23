using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
 
namespace AppTest.IIS
{
    public class IISWorker
    {
        #region 获取IIS版本
        /// <summary>
        /// 获取本地IIS版本
        /// </summary>
        /// <returns></returns>
        public static string GetIIsVersion()
        {
            try
            {
                DirectoryEntry entry = new DirectoryEntry("IIS://localhost/W3SVC/INFO");
                string version = entry.Properties["MajorIISVersionNumber"].Value.ToString();
                return version;
            }
            catch (Exception se)
            {
                //说明一点:IIS5.0中没有(int)entry.Properties["MajorIISVersionNumber"].Value;属性，将抛出异常 证明版本为 5.0
                return string.Empty;
            }
        }
        #endregion
 
        #region 获取SiteID
        /// <summary>
        /// 获取最小SiteId，越小越好
        /// </summary>
        /// <returns></returns>
        public static int SiteId()
        {
            DirectoryEntry root = new DirectoryEntry("IIS://localhost/W3SVC");
            // Find unused ID value for new web site
            int siteID = 1;
            foreach (DirectoryEntry e in root.Children)
            {
                if (e.SchemaClassName == "IIsWebServer")
                {
                    int ID = Convert.ToInt32(e.Name);
                    if (ID >= siteID)
                    {
                        siteID = ID + 1;
                    }
                }
            }
            return siteID;
        }
        #endregion
 
        #region 建IIS站点
        /// <summary>
        /// IIS站点
        /// </summary>
        /// <param name="webSiteName">站点名称</param>
        /// <param name="siteID">站点ID</param>
        /// <param name="port">站点端口</param>
        /// <param name="siteExplain">域名</param>
        /// <param name="defaultDoc">默认文档</param>
        /// <param name="pathToRoot">物理路径：d:\\iis\8001</param>
        /// <param name="UserId">应用程序池名称，如果没有自动创建</param>
        public static int CreateSite(string webSiteName,int siteID, string port, string siteExplain, string defaultDoc, string pathToRoot, string UserId)
        {
            int mark = 0;
            try
            {
                // createAppPool(siteExplain);
                DirectoryEntry de = new DirectoryEntry("IIS://localhost/" + "w3svc");   //从活动目录中获取IIS对象。
                
                object[] prams = new object[2] { "IIsWebServer", siteID };
                
                DirectoryEntry site = (DirectoryEntry)de.Invoke("Create", prams); //创建IISWebServer对象。  
                site.Properties["KeyType"][0] = "IIsWebServer";
                site.Properties["ServerComment"][0] = webSiteName; //站点名称  
                site.Properties["ServerState"][0] = 2; //站点初始状态，1.停止，2.启动，3  
                site.Properties["ServerSize"][0] = 1;
                site.Properties["ServerBindings"].Add(":" + port + ":" + siteExplain); //站点端口  
                site.CommitChanges(); //保存改变
                de.CommitChanges();
                DirectoryEntry root = site.Children.Add("Root", "IIsWebVirtualDir");   //添加虚拟目录对象  
                root.Invoke("AppCreate", true); //创建IIS应用程序  
                root.Invoke("AppCreate3", new object[] { 2, UserId, true });  //创建应用程序池，并指定应用程序池为"HostPool","true"表示如果HostPool不存在，则自动创建
                root.Properties["path"][0] = pathToRoot; //虚拟目录指向的物理目录  
                root.Properties["EnableDirBrowsing"][0] = true;//目录浏览  
                root.Properties["AuthAnonymous"][0] = true;
                root.Properties["AccessExecute"][0] = true;   //可执行权限  
                root.Properties["AccessRead"][0] = true;
                root.Properties["AccessWrite"][0] = true;
                root.Properties["AccessScript"][0] = true;//纯脚本  
                root.Properties["AccessSource"][0] = false;
                root.Properties["FrontPageWeb"][0] = false;
                root.Properties["KeyType"][0] = "IIsWebVirtualDir";
                root.Properties["AppFriendlyName"][0] = siteExplain; //应用程序名   
                root.Properties["AppIsolated"][0] = 2;
                root.Properties["DefaultDoc"][0] = defaultDoc; //默认文档  
                root.Properties["EnableDefaultDoc"][0] = true; //是否启用默认文档  
                root.CommitChanges();
                site.CommitChanges();
                root.Close();
                site.Close();
                de.CommitChanges(); //保存  
                site.Invoke("Start", null); //除了在创建过程中置初始状态外，也可在此调用方法改变状态  
                mark = 1;
            }
            catch(Exception ex)
            {
                mark = 0;
            }
            return mark;
        }
        #endregion
 
        #region 删除站点
        public static void DelSite(string siteName)
        {
            string siteNum = GetWebSiteNum(siteName);
            string siteEntPath = String.Format("IIS://{0}/w3svc/{1}", "localhost", siteNum);
            DirectoryEntry siteEntry = new DirectoryEntry(siteEntPath);
            string rootPath = String.Format("IIS://{0}/w3svc", "localhost");
            DirectoryEntry rootEntry = new DirectoryEntry(rootPath);
            rootEntry.Children.Remove(siteEntry);
            rootEntry.CommitChanges();
        }
        #endregion
 
        #region 域名绑定方法
        public static int AddHostHeader(int siteid, string ip, int port, string domain)//增加主机头（站点编号.ip.端口.域名）
        {
            int mark = 0;
            try
            {
                DirectoryEntry site = new DirectoryEntry("IIS://localhost/W3SVC/" + siteid);
                PropertyValueCollection serverBindings = site.Properties["ServerBindings"];
                string headerStr = string.Format("{0}:{1}:{2}", ip, port, domain);
                if (!serverBindings.Contains(headerStr))
                {
                    serverBindings.Add(headerStr);
                }
                site.CommitChanges();
                mark = 1;
            }
            catch
            {
                mark = 0;
            }
            return mark;
        }
        #endregion
 
        #region 删除主机头
        public static void DeleteHostHeader(int siteid, string ip, int port, string domain)//删除主机头（站点编号.ip.端口.域名）
        {
            DirectoryEntry site = new DirectoryEntry("IIS://localhost/W3SVC/" + siteid);
            PropertyValueCollection serverBindings = site.Properties["ServerBindings"];
            string headerStr = string.Format("{0}:{1}:{2}", ip, port, domain);
            if (serverBindings.Contains(headerStr))
            {
                serverBindings.Remove(headerStr);
            }
            site.CommitChanges();
        }
        #endregion
        
        #region 创建应用程序池
        static void createAppPool(string AppPoolName)
        {
            DirectoryEntry newpool;
            DirectoryEntry apppools = new DirectoryEntry("IIS://localhost/W3SVC/AppPools");
            newpool = apppools.Children.Add(AppPoolName, "IIsApplicationPool");
            newpool.CommitChanges();
        }
        #endregion
 
        #region 删除应用程序池
        public void deleteAppPool(string AppPoolName)
        {
            bool ExistAppPoolFlag = false;
            try
            {
                DirectoryEntry apppools = new DirectoryEntry("IIS://localhost/W3SVC/AppPools");
                foreach (DirectoryEntry a in apppools.Children)
                {
                    if (a.Name == AppPoolName)
                    {
                        ExistAppPoolFlag = true;
                        a.DeleteTree();
                        // MessageBox.Show("应用程序池名称删除成功", "删除成功");
                    }
                }
                if (ExistAppPoolFlag == false)
                {
                    // MessageBox.Show("应用程序池未找到", "删除失败");
                }
            }
            catch
            {
                //MessageBox.Show(ex.Message, "错误");
            }
        }
        #endregion
 
        #region 获取指定网站siteID
        /// <summary>
        /// 获取指定网站siteID
        /// </summary>
        /// <param name="siteName">站点名称</param>
        /// <returns></returns>
        public static string GetWebSiteNum(string siteName)
        {
            Regex regex = new Regex(siteName);
            string tmpStr;
            string entPath = String.Format("IIS://{0}/w3svc", "localhost");
            DirectoryEntry ent = new DirectoryEntry(entPath);
            foreach (DirectoryEntry child in ent.Children)
            {
                if (child.SchemaClassName == "IIsWebServer")
                {
                    if (child.Properties["ServerBindings"].Value != null)
                    {
                        tmpStr = child.Properties["ServerBindings"].Value.ToString();
                        if (regex.Match(tmpStr).Success)
                        {
                            return child.Name;
                        }
                    }
                    if (child.Properties["ServerComment"].Value != null)
                    {
                        tmpStr = child.Properties["ServerComment"].Value.ToString();
                        if (regex.Match(tmpStr).Success)
                        {
                            return child.Name;
                        }
                    }
                }
            }
            return "没有找到要删除的站点";
        }
        #endregion
 
        #region 获取IIS站点列表
        /// <summary>
        /// 获取站点列表
        /// </summary>
        public static List<IISInfo> GetServerBindings()
        {
            List<IISInfo> iisList = new List<IISInfo>();
            string entPath = "IIS://localhost/w3svc";
            DirectoryEntry ent = new DirectoryEntry(entPath);
            foreach (DirectoryEntry child in ent.Children)
            {
                if (child.SchemaClassName.Equals("IIsWebServer", StringComparison.OrdinalIgnoreCase))
                {
                    if (child.Properties["ServerBindings"].Value != null)
                    {
                        object objectArr = child.Properties["ServerBindings"].Value;
                        string serverBindingStr = string.Empty;
                        if (objectArr is Array)//如果有多个绑定站点时
                        {
                            object[] objectToArr = (object[])objectArr;
                            serverBindingStr = objectToArr[0].ToString();
                        }
                        else//只有一个绑定站点
                        {
                            serverBindingStr = child.Properties["ServerBindings"].Value.ToString();
                        }
                        IISInfo iisInfo = new IISInfo();
                        iisInfo.DomainPort = serverBindingStr;
                        iisInfo.AppPool = child.Properties["AppPoolId"].Value.ToString();//应用程序池
                        iisInfo.ServerComment = child.Properties["ServerComment"].Value.ToString();
 
                        iisInfo.physicalPath = GetWebsitePhysicalPath(child);
                        iisList.Add(iisInfo);
                    }
                }
            }
            return iisList;
        }
        #endregion
 
        #region 获取网站的物理路径
        /// <summary>
        /// 得到网站的物理路径
        /// </summary>
        /// <param name="rootEntry">网站节点</param>
        /// <returns></returns>
        public static string GetWebsitePhysicalPath(DirectoryEntry rootEntry)
        {
            string physicalPath = "";
            foreach (DirectoryEntry childEntry in rootEntry.Children)
            {
                if ((childEntry.SchemaClassName == "IIsWebVirtualDir") && (childEntry.Name.ToLower() == "root"))
                {
                    if (childEntry.Properties["Path"].Value != null)
                    {
                        physicalPath = childEntry.Properties["Path"].Value.ToString();
                    }
                    else
                    {
                        physicalPath = "";
                    }
                }
            }
            return physicalPath;
        }
        #endregion
 
        #region 判断端口是否被占用
        /// <summary>
        /// 判断端口是否被占用
        /// </summary>
        /// <param name="port">端口号</param>
        /// <returns></returns>
        public static bool PortInUse(int port)
        {
            bool inUse = false;
 
            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] ipEndPoints = ipProperties.GetActiveTcpListeners();
 
            foreach (IPEndPoint endPoint in ipEndPoints)
            {
                if (endPoint.Port == port)
                {
                    inUse = true;
                    break;
                }
            }
            return inUse;
        }
        #endregion
    }
}