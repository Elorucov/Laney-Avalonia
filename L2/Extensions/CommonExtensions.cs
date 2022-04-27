using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELOR.Laney.Extensions {
    public static class CommonExtensions {
        public static string HResultHEX(this Exception ex) {
            return $"0x{ex.HResult.ToString("x8")}";
        }
    }
}