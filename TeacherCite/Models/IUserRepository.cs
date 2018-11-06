using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KovalukApp.Models
{
    public interface IUserRepository
    {
        IQueryable<IUser> UsersData { get; }
        IUser FindUserByName(string name);
    }
}
