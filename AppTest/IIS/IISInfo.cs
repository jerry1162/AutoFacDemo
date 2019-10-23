namespace AppTest.IIS
{

	public class IISInfo
	{
		/// <summary>
		/// 站点+端口
		/// </summary>
		public string DomainPort { get; set; }
		/// <summary>
		/// 应用程序池
		/// </summary>
		public string AppPool { get; set; }
		/// <summary>
		/// 网站名称
		/// </summary>
		public string ServerComment { get; set; }
		/// <summary>
		/// 物理路径
		/// </summary>
		public string physicalPath { get; set; }
	}
}