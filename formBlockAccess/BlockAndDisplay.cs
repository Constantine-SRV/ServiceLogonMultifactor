using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace formBlockAccess
{
    public static class BlockAndDisplay
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool BlockInput(bool fBlockIt);

        static  List<Form> forms = new List<Form>();
        static public void DrawFormsOnAllScreens()
        {
            var a = Screen.AllScreens.Length;

            for (int i = 0; i <= Screen.AllScreens.Length - 1; i++)
            {
                var screen = Screen.AllScreens[i];
                var bounds = screen.Bounds;
                Form f = new FormBlock();
                f.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                f.SetBounds(bounds.X, bounds.Y, bounds.Width, bounds.Height);
                f.StartPosition = FormStartPosition.Manual;
                Label l = ((Label)f.Controls["label1"]);
                f.TopMost = true;
                f.Show();
                l.Left = (f.ClientSize.Width - l.Size.Width) / 2;
                l.Top = (f.ClientSize.Height - l.Size.Height) / 2;
                forms.Add(f);
            }

        }

       public static void BlockInputEvery500MsForIsec(int timeBlockSec)
        {
            for (int i = timeBlockSec; i >= 0; i--)
            {
                DrawTime(i);
                Thread.Sleep(500);
                BlockInput(true);
                Thread.Sleep(500);
                BlockInput(true);
            }
            BlockInput(false);
            Application.Exit();
        }
        static void DrawTime(int i)
        {
            foreach (var f in forms)
            {
                Label l = ((Label)f.Controls["label1"]);
                if (l.InvokeRequired) l.Invoke(new Action(() =>
                {
                    l.Text = $"Disconnect in {i} seconds";
                }));
            }
        }


    }
}
