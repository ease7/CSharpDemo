using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace IdiomsSolitaire.Core
{
    public class ChengYuDalService: IChengYuDalService
    {
        private readonly TestDbContext _dbContext;

        private static Random random = new Random(DateTime.Now.Millisecond);

        public ChengYuDalService(TestDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public ChengYu GetByFirstName(string name)
        {
            var list = _dbContext.ChengYu.Where(o => o.Name.StartsWith(name)).ToList();

            if(list.Count > 0)
            {
                var index = random.Next(0, list.Count - 1);

                return list[index];
            }

            return null;
        }

        public ChengYu GetByName(string name)
        {
            return _dbContext.ChengYu.FirstOrDefault(o => o.Name == name);
        }

        public List<ChengYu> Search(string expression)
        {
            if (expression.Contains("%"))
            {
                return _dbContext.ChengYu.Where(o => EF.Functions.Like(o.Name, expression)).ToList();

            }
            else
            {
                return _dbContext.ChengYu.Where(o => o.Name.Contains(expression)).ToList();
            }

        }

    }
}
