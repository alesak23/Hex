using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace Hex
{
    public enum HexPoints { UpLeft, UpRight, Right, DownRight, DownLeft, Left }

    /// <summary>
    /// Drawing engine handles rendering all aspects of game including GUI and also handles user input form screen.
    /// </summary>
    public abstract class DrawingEngine
    {
        public GameLogic gameLogicToDraw { get; set; }



        public virtual void SetGameLogicToDraw(GameLogic gamelogic)
        {
            gameLogicToDraw = gamelogic;
        }

        public abstract void Draw();

        //TODO: start new game with given parameters
        public virtual void StartNewGame()
        {

        }

    }



    class GraphicEngineWinforms : DrawingEngine
    {




        private Form1 form;
        private GameEngine gameEngine;
        private GameScreens currentGameScreen;

        /// <summary>
        /// Coordinates of hexagon points, hexagon is centered on (0, 0) and has radius 1, coordinate axes are standart screen coordinates.
        /// </summary>
        private PointF[] Hexagon;
        private float median = (float)Math.Sqrt(3) / 2;

        private RectangleF playingArea;
        private float hexRadiusX;
        private float hexRadiusY;

        private System.Diagnostics.Stopwatch stopwatch;
        private long lastFrameTime;
        private long elapsedMilliseconds;

        public Color[] FactionColors;


        enum GameScreens { Menu, Game, EndOfGame }

        public GraphicEngineWinforms(Form1 form, GameEngine gameEngine)
        {
            this.form = form;
            this.gameEngine = gameEngine;
            currentGameScreen = GameScreens.Game;

            float radius = 1;
            float median = radius * (float)Math.Sqrt(3) / 2;

            Hexagon = new PointF[6];
            /*
            Hexagon[(int)HexPoints.UpLeft] = new PointF(-radius / 2, -median);
            Hexagon[(int)HexPoints.UpRight] = new PointF(radius / 2, -median);
            Hexagon[(int)HexPoints.Right] = new PointF(radius, 0);
            Hexagon[(int)HexPoints.DownRight] = new PointF(radius / 2, median);
            Hexagon[(int)HexPoints.DownLeft] = new PointF(-radius / 2, median);
            Hexagon[(int)HexPoints.Left] = new PointF(-radius, 0);
             */

            Hexagon[(int)HexPoints.UpLeft] = new PointF(-radius / 2, -1);
            Hexagon[(int)HexPoints.UpRight] = new PointF(radius / 2, -1);
            Hexagon[(int)HexPoints.Right] = new PointF(radius, 0);
            Hexagon[(int)HexPoints.DownRight] = new PointF(radius / 2, 1);
            Hexagon[(int)HexPoints.DownLeft] = new PointF(-radius / 2, 1);
            Hexagon[(int)HexPoints.Left] = new PointF(-radius, 0);

            playingArea = new Rectangle(form.Location, form.Size);
            playingArea.X = playingArea.X + 10;
            playingArea.Y = playingArea.Y + 10;

            playingArea.Width -= 30;
            playingArea.Height -= 50;

            FactionColors = new Color[] { Color.BurlyWood, Color.Red, Color.Fuchsia, Color.Yellow };

            stopwatch = System.Diagnostics.Stopwatch.StartNew();
            lastFrameTime = stopwatch.ElapsedMilliseconds;

        }


        public override void Draw()
        {
            elapsedMilliseconds = stopwatch.ElapsedMilliseconds - lastFrameTime;
            lastFrameTime = stopwatch.ElapsedMilliseconds;

            form.Refresh();


        }

        public void Draw(Graphics graphics)
        {
            form.BackColor = Color.Black;

            if (elapsedMilliseconds != 0)
                graphics.DrawString((1000 / elapsedMilliseconds).ToString(),
    new Font(FontFamily.GenericSansSerif, 10), Brushes.WhiteSmoke, new PointF(10, 10));


            switch (currentGameScreen)
            {
                case GameScreens.Menu:
                    DrawMenu(graphics);
                    break;
                case GameScreens.Game:
                    DrawGame(graphics);
                    break;
                case GameScreens.EndOfGame:
                    DrawEndOfGame(graphics);
                    break;
                default:
                    throw new ArgumentException("Wanted game screen is not implemented.");

            }
        }

        private void DrawEndOfGame(Graphics graphics)
        {
            throw new NotImplementedException();
        }

        private void DrawGame(Graphics graphics)
        {
            //graphics.Clear(Color.Black);

            playingArea = new Rectangle(new Point(0, 0), form.Size);
            playingArea.X = playingArea.X + 10;
            playingArea.Y = playingArea.Y + 10;

            playingArea.Width -= 30;
            playingArea.Height -= 50;

            graphics.DrawRectangle(new Pen(Brushes.White), playingArea.X, playingArea.Y, playingArea.Width, playingArea.Height);


            for (int i = 0; i < gameLogicToDraw.SizeY; i++)
            {
                for (int j = 0; j < gameLogicToDraw.SizeX; j++)
                {
                    float centerPointX = playingArea.X + GetCenterPoint(j, i).X;
                    float centerPointY = playingArea.Y + GetCenterPoint(j, i).Y;

                    int tileX = ConvertScreenToTileCoordsX(j);
                    int tileY = ConvertScreenToTileCoordsY(i);

                    Tile thisTile = gameLogicToDraw.Tiles[tileX, tileY];

                    for (int k = 0; k < 6; k++)
                    {
                        System.Drawing.Drawing2D.Matrix m = new System.Drawing.Drawing2D.Matrix();
                        PointF p1 = Hexagon[k];
                        PointF p2 = Hexagon[(k + 1) % 6];
                        m.Translate(centerPointX, centerPointY);
                        m.Scale(hexRadiusX, hexRadiusY);
                        p1 = m.TransformPoint(p1);
                        p2 = m.TransformPoint(p2);

                        int penWidth = 4;

                        Pen pen1;

                        Tuple<int, int> neighbouringTile = gameLogicToDraw.GetNeighbouringTile(Tuple.Create(tileX, tileY), (HexDirections)k, true);
                        Faction thisFaction = gameLogicToDraw.GetTile(Tuple.Create(tileX, tileY)).OccupiedByFaction;

                        if (thisFaction == Faction.Empty)
                        {
                            if (gameLogicToDraw.GetTile(gameLogicToDraw.GetNeighbouringTile(Tuple.Create(tileX, tileY), (HexDirections)k)).OccupiedByFaction == Faction.Empty)
                            {
                                pen1 = new Pen(Color.Gray, penWidth);
                                graphics.DrawLine(pen1, p1, p2);
                            }
                        }
                        else
                        {
                            if (gameLogicToDraw.IsOutOfBounds(neighbouringTile))
                            {
                                pen1 = new Pen(FactionColors[thisFaction.ID], penWidth);
                                graphics.DrawLine(pen1, p1, p2);
                            }
                            else
                            {
                                Faction neighbourFaction = gameLogicToDraw.GetTile(neighbouringTile).OccupiedByFaction;
                                if (neighbourFaction == Faction.Empty)
                                {
                                    //if (k == (int)(HexDirections.Up))

                                    pen1 = new Pen(FactionColors[thisFaction.ID], penWidth);
                                    graphics.DrawLine(pen1, p1, p2);

                                }
                                else if (neighbourFaction == thisFaction)
                                {
                                    continue;
                                }
                                else
                                {
                                    pen1 = new Pen(FactionColors[thisFaction.ID], penWidth);
                                    Pen pen2 = new Pen(FactionColors[neighbourFaction.ID], penWidth);
                                    graphics.DrawLineTwoColour(pen1, pen2, p1, p2, 5);
                                }
                            }
                        }

                    }

                    string tileString = "X";
                    Pen stringPen = new Pen(Color.Red);
                    Pen fillPen = new Pen(Color.ForestGreen);
                    switch (thisTile.Type)
                    {
                        case TerrainTypes.Land:
                            tileString = " ";
                            stringPen.Brush = Brushes.SandyBrown;
                            break;
                        case TerrainTypes.Water:
                            tileString = " ";
                            stringPen.Brush = Brushes.Aqua;
                            fillPen.Color = Color.DeepSkyBlue;
                            break;
                        case TerrainTypes.City:
                            tileString = "c";
                            stringPen.Brush = Brushes.LightGray;
                            //fillPen.Color=Color.SandyBrown;
                            break;
                        case TerrainTypes.Harbor:
                            tileString = "H";
                            stringPen.Brush = Brushes.Yellow;
                            break;
                        case TerrainTypes.Capital:
                            tileString = "C";
                            stringPen.Brush = Brushes.DarkGray;
                            break;
                        default:
                            tileString = "X";
                            break;
                    }

                    RectangleF rect = new RectangleF(centerPointX - hexRadiusX * 0.8f, centerPointY - hexRadiusY * 0.8f, hexRadiusX * 1.6f, hexRadiusY * 1.6f);
                    graphics.FillEllipse(fillPen.Brush, rect);

                    graphics.DrawString(tileString,
    new Font(FontFamily.GenericSansSerif, 25), stringPen.Brush, new PointF(centerPointX - 15, centerPointY - 18));


                    graphics.DrawString(tileX.ToString() + ", " + tileY.ToString() + ";" + gameLogicToDraw.GetTile(Tuple.Create(tileX, tileY)).OccupiedByFaction.ID.ToString(),
                        new Font(FontFamily.GenericMonospace, 7), Brushes.White, new PointF(centerPointX - 20, centerPointY - 7));

                }
            }

            /*GraphicsPath path = new GraphicsPath();
            GraphicsPath path2 = new GraphicsPath();
            //Point[] points = new Point[] { new Point(0, 50), new Point(25, 100), new Point(75, 100), new Point(100, 50), new Point(75, 0), new Point(25, 0) };

            Point point1 = new Point(-50, 50);
            point1 = m.TransformPoint(point1);

            Point point2 = new Point(50, -50);
            point2 = m.TransformPoint(point2);

            graphics.DrawLineTwoColour(pen1, pen2, point1, point2, 14);*/

            //path.CloseFigure();

        }

        private void DrawMenu(Graphics graphics)
        {
            throw new NotImplementedException();
        }


        private void DrawHex(Tuple<int, int> tile)
        {

        }

        public int ConvertScreenToTileCoordsX(int x)
        {
            return x;
        }

        public int ConvertScreenToTileCoordsY(int y)
        {
            return gameLogicToDraw.SizeY - 1 - y;
        }



        public override void SetGameLogicToDraw(GameLogic gamelogic)
        {
            base.SetGameLogicToDraw(gamelogic);

            if (gamelogic.SizeX % 2 == 0)
                hexRadiusX = (playingArea.Width / (1.5f * (float)gamelogic.SizeX + 0.5f));
            else
                hexRadiusX = (playingArea.Width / (2f * ((float)gamelogic.SizeX) - gamelogic.SizeX / 2));

            hexRadiusY = (playingArea.Height / (2 * (float)gamelogic.SizeY + 1f));


        }

        /* public HexPoints[] HexDirectionToPoints(HexDirections dir)
         {
             HexPoints[] result = new HexPoints[2];
             switch (dir)
             {
                 case HexDirections.Up:

                     break;
                 case HexDirections.UpRight:
                     break;
                 case HexDirections.DownRight:
                     break;
                 case HexDirections.Down:
                     break;
                 case HexDirections.DownLeft:
                     break;
                 case HexDirections.LeftUp:
                     break;
                 default:
                     break;
             }
         }*/

        private PointF GetCenterPoint(int x, int y)
        {
            PointF result = new PointF();
            result.X = hexRadiusX * ((float)x * 1.5f + 1f);

            if (x % 2 == 0)
                result.Y = hexRadiusY * ((float)y * 2f + 2);
            else
                result.Y = hexRadiusY * ((float)y * 2f + 1f);

            return result;
        }



        public void DrawTerrainToConsole()
        {
            if (gameLogicToDraw == null)
                return;

            for (int i = 0; i < gameLogicToDraw.SizeY; i++)
            {
                for (int j = 0; j < gameLogicToDraw.SizeX; j++)
                {
                    string tile = "X";

                    switch (gameLogicToDraw.Tiles[j, i].Type)
                    {
                        case TerrainTypes.Capital:
                            tile = "C";
                            break;
                        case TerrainTypes.City:
                            tile = "c";
                            break;
                        case TerrainTypes.Harbor:
                            tile = "H";
                            break;
                        case TerrainTypes.Water:
                            tile = " ";
                            break;
                        case TerrainTypes.Land:
                            tile = "L";
                            break;
                        default:
                            tile = "X";
                            break;
                    }
                    Console.Write(tile);
                }
                Console.WriteLine();
            }
        }




    }

    /// <summary>
    /// Draws unconnected lines from given array. Array must have even length. 
    /// </summary>
    public static class GraphicsExtensions
    {
        public static void DrawLinesSeparately(this Graphics graphics, Pen pen, PointF[] lines)
        {
            if (lines.Length % 2 != 0)
            {
                throw new ArgumentException("Size of input array must be even.");
            }

            for (int i = 0; i < lines.Length; i = i + 2)
            {
                graphics.DrawLine(pen, lines[i], lines[i + 1]);
            }
        }

        public static void DrawLineTwoColour(this Graphics graphics, Pen pen1, Pen pen2, PointF from, PointF to, int numberOfSegments)
        {
            float lengthX = (-from.X + to.X) / numberOfSegments;
            float lengthY = (-from.Y + to.Y) / numberOfSegments;

            int sizeLines1;
            if (numberOfSegments % 2 == 0)
                sizeLines1 = numberOfSegments / 2 * 2;
            else
                sizeLines1 = (numberOfSegments / 2 + 1) * 2;

            PointF[] lines1 = new PointF[sizeLines1];
            PointF[] lines2 = new PointF[numberOfSegments / 2 * 2];

            for (int i = 0; i < lines1.Length - 1; i = i + 2)
            {
                lines1[i].X = from.X + lengthX * i;
                lines1[i].Y = from.Y + lengthY * i;

                lines1[i + 1].X = from.X + (lengthX * (i + 1));
                lines1[i + 1].Y = from.Y + (lengthY * (i + 1));
            }

            for (int i = 0; i < lines2.Length - 1; i = i + 2)
            {
                lines2[i].X = from.X + (lengthX * (i + 1));
                lines2[i].Y = from.Y + (lengthY * (i + 1));

                lines2[i + 1].X = from.X + (lengthX * (i + 2));
                lines2[i + 1].Y = from.Y + (lengthY * (i + 2));
            }

            graphics.DrawLinesSeparately(pen1, lines1);
            graphics.DrawLinesSeparately(pen2, lines2);
        }

        public static PointF TransformPoint(this Matrix matrix, PointF point)
        {
            PointF[] ps = new PointF[] { new PointF(point.X, point.Y) };
            matrix.TransformPoints(ps);
            return ps[0];
        }

        public static PointF[] CopyAndTransform(this Matrix matrix, PointF[] points)
        {
            PointF[] result = new PointF[points.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = points[i];
            }
            matrix.TransformPoints(result);
            return result;
        }
    }
}
