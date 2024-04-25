using Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Helpers
{
    public static class Helper
    {
        public static double RoundToOneDigit(this double value)
        {
            return Math.Round(value, 1);
        }
        
    }
}
