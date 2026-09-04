using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MayNghien.Infrastructures.Helpers
{
    public class SearchHelper
    {
        public static int CalculateNumOfPages(int numOfItems, int? pageSize)
        {
            int num = pageSize ?? 10;
            int num2 = numOfItems / num;
            if (numOfItems % num != 0)
            {
                num2++;
            }

            return num2;
        }
    }
}
