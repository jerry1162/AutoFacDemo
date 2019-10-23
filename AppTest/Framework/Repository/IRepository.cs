using System.Collections.Generic;
using AppTest.Framework.Model;

namespace AppTest.Framework.Repository
{
	public interface IRepository<T> where T : BaseModel
	{
		T GetById(int id);
		
		List<T> GetAll();

		bool Add(T item);
		
		bool Delete(T item);

		bool DeleteById(int id);
	}
}