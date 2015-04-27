using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Hex
{
    public partial class Form1 : Form
    {
        private Timer timer;
        private System.Diagnostics.Stopwatch stopwatch;
        private long lastFrameTime;


        public Form1()
        {
            this.SetStyle(
                System.Windows.Forms.ControlStyles.UserPaint |
                System.Windows.Forms.ControlStyles.AllPaintingInWmPaint |
                System.Windows.Forms.ControlStyles.OptimizedDoubleBuffer,
                true);

            InitializeComponent();

            stopwatch = System.Diagnostics.Stopwatch.StartNew();

            timer = new Timer();
            timer.Interval = 20;
            timer.Tick += Tick;
            timer.Start();
        }


        private void MainLoop()
        {
            int elapsedMilliseconds = (int)(stopwatch.ElapsedMilliseconds - lastFrameTime);

            

            this.Refresh();
            lastFrameTime = stopwatch.ElapsedMilliseconds;
        }


        private void Draw(Graphics graphics)
        {
            this.BackColor = Color.White;
            graphics.FillEllipse(Brushes.Red, new Rectangle(PointToClient(Cursor.Position).X - 50, PointToClient(Cursor.Position).Y - 50, 100, 100));
        }



        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Draw(e.Graphics);
        }



        private void Tick(object sender, EventArgs e)
        {
            MainLoop();
        }






    }
}
