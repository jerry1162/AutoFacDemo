using System;
using AppTest.IIS;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AppTest.Tests.IIS
{
	[TestClass]
	public class IISTest
	{
		[TestMethod]
		public void IISWorkerTest()
		{
			Console.WriteLine(IISWorker.GetIIsVersion());
			var list = IISWorker.GetServerBindings();
			Console.WriteLine(list);
		}
	}
}