using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DashingWanderer.Extensions
{
    public static class IntExtension
    {
        /// <summary>
        /// Returns null if the integer value is zero. 
        /// Otherwise, returns the integer's value.
        /// </summary>
        /// <param name="number"></param>
        /// <returns>Returns null if zero, otherwise returns the number.</returns>
        public static int? NullIfZero(this int number)
        {
            if (number == 0)
            {
                return null;
            }

            return number;
        }
    }
}
