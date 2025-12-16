using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TagRunner
{
    public class Aspose许可授权
    {
        public static void Authorize()
        {
            string licPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "asposekey.txt");
            var se = new Aspose.Words.License();
            se.SetLicense(licPath);
        }
    }
}
