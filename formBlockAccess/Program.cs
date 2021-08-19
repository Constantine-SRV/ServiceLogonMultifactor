using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace formBlockAccess
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
       
        static void Main(string[] args)
        {
         string param1 = "5";
         int timeBlockSec = 30;  
         if (args.Length> 0 && args[0]!=null) {param1 = args[0]; }
         int.TryParse(param1, out timeBlockSec);
       
         Application.EnableVisualStyles();
         Application.SetCompatibleTextRenderingDefault(false);
         
         BlockAndDisplay.DrawFormsOnAllScreens();
         Task.Factory.StartNew(() => { BlockAndDisplay.BlockInputEvery500MsForIsec(timeBlockSec); });
         Application.Run();
        
        }

       
    }
}
