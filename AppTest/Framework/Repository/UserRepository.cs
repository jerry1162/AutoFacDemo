using System.Collections.Generic;
using System.Linq;
using AppTest.Framework.Model;

namespace AppTest.Framework.Repository
{
	public class UserRepository : IRepository<UserModel>
	{
		private List<UserModel> UserList;

		public UserRepository()
		{
			UserList = new List<UserModel>
			{
				new UserModel() {Id = 0, Name = "Jerry"},
				new UserModel() {Id = 1, Name = "Tom"},
				new UserModel() {Id = 2, Name = "Equinox"}
			};
		}

		public UserModel GetById(int id)
		{
			return UserList.FirstOrDefault(it => it.Id == id);
		}

		public List<UserModel> GetAll()
		{
			return UserList;
		}

		public bool Add(UserModel item)
		{
			if (item == null)
			{
				return false;
			}
			UserList.Add(item);
			return true;
		}

		public bool Delete(UserModel item)
		{
			return item != null && UserList.Remove(item);
		}

		public bool DeleteById(int id)
		{
			return false;
//			var user=UserList.Find()
		}
	}
}