using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KovalukApp.Models
{
    public class UserDataRepository : IUserRepository
    {
        private ApplicationContext context;

        public UserDataRepository(ApplicationContext cnt)
        {
            context = cnt;
        }

        public IQueryable<IUser> UsersData => context.Users;

        public IUser FindUserByName(string name) => UsersData.FirstOrDefault(user=>user.UserName==name);
    }
}
