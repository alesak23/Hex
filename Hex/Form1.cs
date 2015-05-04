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
        GameEngine gameEngine;
        GraphicEngineWinforms graphicsEngine;

        public Form1()
        {
            this.SetStyle(
                System.Windows.Forms.ControlStyles.UserPaint |
                System.Windows.Forms.ControlStyles.AllPaintingInWmPaint |
                System.Windows.Forms.ControlStyles.OptimizedDoubleBuffer,
                true);

            gameEngine = new GameEngine();
            graphicsEngine = new GraphicEngineWinforms(this, gameEngine);
            gameEngine.graphicsEngine = graphicsEngine;

            

            InitializeComponent();

            gameEngine.Run();

        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
             graphicsEngine.Draw(e.Graphics);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }










    }
}
