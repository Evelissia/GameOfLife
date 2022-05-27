using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GameOfLife
{
    public partial class ConwayMain : Form
    {
        Boolean InProgress;
        public ConwayMain()
        {
            InitializeComponent();
        }
        private void ConwayMain_Load(object sender, EventArgs e)
        {
            CreateGridSurface();
        }

        // этот метод создает сетку и ячейки
        private void CreateGridSurface()
        {
            Point locPoint;
            Cell newCell;
            Random random = new Random();

            // определение кол-ва строк и столбцов в сетке на основе размера ячейки, выбранного в форме
            int rows = (int)(pbGrid.Height / numSSize.Value);
            int cols = (int)(pbGrid.Width / numSSize.Value);

            // создание сетки в которой содержатся отдельные ячейки
            // ссылаюсь на PictureBox для определения размера
            using (Bitmap bmp = new Bitmap(pbGrid.Width, pbGrid.Height))

            // создание графического объекта, который будет обеспечивать поверхность рисования
            using (Graphics g = Graphics.FromImage(bmp))

            // кисть для рисования объекта на поверхности изображения
            using (SolidBrush cellBrush = new SolidBrush(Color.Pink))
            {
                // создание пустой поверхности и присвоение копии растового изображения
                g.Clear(Color.Black);
                pbGrid.Image = (Bitmap)bmp.Clone();

                Cell.gridCells.Clear();

                for (int y = 0; y < rows; y++)
                {
                    for (int x = 0; x < cols; x++)
                    {
                        locPoint = new Point((int)(x * numSSize.Value), (int)(y * numSSize.Value));
                        newCell = new Cell(locPoint, x, y);
                        newCell.IsAlive = (random.Next(100) < 15) ? true : false;
                        // объект ячейки добавляется в список
                        Cell.gridCells.Add(newCell);
                    }

                    // нанесение всех вновь созданных ячеек на сетку
                    foreach (Cell cell in Cell.gridCells)
                    {
                        // проверяется каждая ячейка в списке
                        // Если она живая, то наносится на сетку с помощью нового прямоугольника
                        if (cell.IsAlive)
                        {
                            g.FillRectangle(cellBrush, new Rectangle(cell.Location,
                                new Size((int)numSSize.Value - 1, (int)numSSize.Value - 1)));
                        }
                    }

                    // объект Bitmap клонируется в свойство изображения PictureBox
                    pbGrid.Image = (Bitmap)bmp.Clone();
                }
            }
        }

        // метод для вычисления следующих позоций ячеек и обновления сетки
        private void GetNextState()
        {
            // 1. Живая клетка с < 2 живыми соседками умирает от одиночества
            // 2. Живая клетка с 2 или 3 соседками живет до следующего поколения
            // 3. Живая клетка с > 3 живыми соседками умирает от перенасиления
            // 4. Любая мертвая клетка с ровно 3-мя живыми соседями становится живой

            // Вычисление следующего статуса каждой ячейки
            foreach (Cell cell in Cell.gridCells)
            {
                int activeCount = cell.LiveAdjacent();

                if (cell.IsAlive)
                {
                    if ((activeCount < 2) || (activeCount > 3))
                        cell.NextStatus = false;
                    else
                        cell.NextStatus = true;
                }
                else
                {
                    if (activeCount == 3)
                        cell.NextStatus = true;
                }

            }

            foreach(Cell cell in Cell.gridCells)
            {
                cell.IsAlive = cell.NextStatus;
            }    

            using (Bitmap bmp = new Bitmap(pbGrid.Width, pbGrid.Height))
            using(Graphics g = Graphics.FromImage(bmp))
            using(SolidBrush cellBrush = new SolidBrush(Color.Pink))
            {
                g.Clear(Color.Black);
                foreach (Cell cell in Cell.gridCells)
                {
                    if (cell.IsAlive)
                    {
                        g.FillRectangle(cellBrush, new Rectangle(cell.Location,
                                new Size((int)numSSize.Value - 1, (int)numSSize.Value - 1)));
                    }
                }

                pbGrid.Image.Dispose();
                pbGrid.Image = (Bitmap)bmp.Clone();
            }
        }



        // класс - чертеж для каждого объекта ячейки, который появляется в сетке
        public class Cell
        {
            public static List<Cell> gridCells = new List<Cell>();
            private Point cLocation;

            // св-ва cXPos и cYPos определяют положение ячеек в сетке
            private int cXPos;
            private int cYPos;
            private Boolean cIsAlive;
            private Boolean cNext;

            // конструктор для поиска местоположения в сетке в пикселях для новой ячейки
            // (используя класс Point и координаты х, у по отношению к другим ячейкам)
            public Cell(Point location, int X, int Y)
            {
                this.Location = location;
                this.YPos = Y;
                this.XPos = X;
            }

            public Point Location
            {
                get { return cLocation; }
                set { cLocation = value; }
            }

            public int XPos
            {
                get { return cXPos; }
                set { cXPos = value; }
            }

            public int YPos
            {
                get { return cYPos; }
                set { cYPos = value; }
            }

            // это св-во указывает, является ли отдельная ячейка живой или не живой (активная или нет)
            public Boolean IsAlive
            {
                get { return cIsAlive; }
                set { cIsAlive = value; }
            }

            // хранение следующих состояний ячеек
            public Boolean NextStatus
            {
                get { return cNext; }
                set { cNext = value; }
            }


            // функция для возврата количества активных соседних ячеек
            public int LiveAdjacent()
            {
                int liveAdjacent = 0;

                // перебор коллекции ячеек
                foreach (Cell cell in Cell.gridCells)
                {
                    // проверка является ли просматриваемая ячейка активной
                    if (cell.Location != this.Location && cell.IsAlive)
                    {
                        // значения X и Y ячейки должны накодится на расстоянии не боолее одной ячейки от текущей
                        if(Math.Abs(cell.XPos - this.XPos) < 2 && Math.Abs(cell.YPos - this.YPos) < 2)
                        {
                            // каждый раз, когда ф-ия находит активную ячейку радом с текущей, она увеличивает счетчик
                            liveAdjacent++;
                        }
                    }
                }
                return liveAdjacent;
            }
        }

        // событие сброса сетки
        private void btnReset_Click(object sender, EventArgs e)
        {
            CreateGridSurface();
        }

        private void btnAdvance_Click(object sender, EventArgs e)
        {
            GetNextState();
        }

        private void btnGo_Click(object sender, EventArgs e)
        {
            InProgress = !InProgress;
            btnGo.Text = InProgress ? "Остановить" : "Начать";

            while (InProgress)
            {
                GetNextState();
                Application.DoEvents();
            }

        }

    }
}
