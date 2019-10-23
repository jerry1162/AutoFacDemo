using System.Web;
using System.Web.Mvc;
using Framework;
using Framework.Filters;
using Framework.Filters.Mvc;

namespace AppTest
{
	public class FilterConfig
	{
		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new HandleErrorAttribute());
			filters.Add(new ArgFilterAttribute());
		}
	}
}
