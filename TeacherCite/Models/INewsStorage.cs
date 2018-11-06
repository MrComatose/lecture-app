using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KovalukApp.Models
{
    public interface INewsStorage
    {
         void AddNews(News value);
        void DeleteNews(News value);
        IEnumerable<News> GetSixLatestNews();
        IQueryable<News> AllNews { get; }
    }
}
