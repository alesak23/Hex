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
    class GameEngine
    {
        private Timer timer;
        private System.Diagnostics.Stopwatch stopwatch;
        private long lastFrameTime;

        private DrawingEngine _graphicsEngine;
        public DrawingEngine graphicsEngine { get { return _graphicsEngine; } set { value.gameLogicToDraw = gameLogic; _graphicsEngine = value; } }



        private GameLogic gameLogic;




        public GameEngine()
        {

            gameLogic = new GameLogic(20, 11, new Random().Next());
           
        }

        public void Run()
        {
            stopwatch = System.Diagnostics.Stopwatch.StartNew();
            lastFrameTime = stopwatch.ElapsedMilliseconds;
            graphicsEngine.SetGameLogicToDraw(gameLogic);
            //graphicsEngine.DrawTerrainToConsole();
            
            timer = new Timer();
            timer.Interval = 20;
            timer.Tick += Tick;
            timer.Start();
        }


        private void MainLoop()
        {
            int elapsedMilliseconds = (int)(stopwatch.ElapsedMilliseconds - lastFrameTime);


            graphicsEngine.SetGameLogicToDraw(gameLogic.CreateCompleteCopy());
            graphicsEngine.Draw();
            lastFrameTime = stopwatch.ElapsedMilliseconds;
        }

        private void Tick(object sender, EventArgs e)
        {
            MainLoop();
        }




    }
}
