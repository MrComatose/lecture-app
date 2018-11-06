using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KovalukApp.Models
{
    public class NewsRepository : INewsStorage
    {
        ApplicationContext Context;
        public NewsRepository(ApplicationContext ctx)
        {
            Context = ctx;
        }
        public IQueryable<News> AllNews => Context.News;

        public void AddNews(News value)
        {
            Context.Add(value);
            Context.SaveChanges();
        }

        public void DeleteNews(News value)
        {
            Context.Remove(value);
            Context.SaveChanges();
        }

        public IEnumerable<News> GetSixLatestNews()
        {
            return Context.News.AsNoTracking().OrderByDescending(x => x.PublicationDate).Take(6);
        }
    }
}
