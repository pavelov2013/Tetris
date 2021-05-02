using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.IO;

namespace tetris000
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

        }
       protected List<PictureBox> boxes = new List<PictureBox>();
        protected List<int[]> coords = new List<int[]>();
        private List<PictureBox> StaticObj = new List<PictureBox>();
        private List<int[]> StaticCoords = new List<int[]>();
        public int gamescore;
        private string currentColor;
        private List<string> FigureColors = new List<string>();
        private List<int[]> FCoords = new List<int[]>();
        private  int FiguresCount; //sync with figureColor
        private int speed = 15 ;
        private void Form1_Load(object sender, EventArgs e)
        {
            InitFigures();
            timer1.Interval = 600 / speed;
            timer1.Enabled = false;
            SpeedBox.Enabled = false;
            button1.Enabled = false;
        }
        private void NewGame(object sender, MouseEventArgs e)
        {
            boxes.Clear();
            coords.Clear();
            StaticCoords.Clear();
            StaticObj.Clear();
            canvas.Controls.Clear();

            timer1.Enabled = true;
            CreateFigure();
        }
        public void CreateFigure() //For spawning create Picbox & Spawn(name,x,y)
        {
            Random FigureGeneration = new Random();
            int SpawningFigure = FigureGeneration.Next(0, FiguresCount);
            int[] Spawn_coords = FCoords[SpawningFigure];
            currentColor = FigureColors[SpawningFigure];
            PictureBox[] Created = new PictureBox[Spawn_coords.Length/2];
            for (int i = 0; i < Spawn_coords.Length/2; i++)
            {
                Created[i] = new PictureBox();
            }
            label1.Text = Created.Length.ToString();
            for (int i = 0; i < Created.Count(); i++)
            {
                Spawn(Created[i],Spawn_coords[2*i],Spawn_coords[2*i+1],currentColor);
            }
        }
        public void InitFigures()
        {
            if(!File.Exists("figures.fc"))
            {
                ErrorLoad("File Not exists");
                return;
            }
            List<string> data = new List<string>();
             data = File.ReadLines("figures.fc").ToList();
            FiguresCount = data.Count();
            if (FiguresCount <= 0)
            {
                ErrorLoad("count = 0");
            }
            List<string> GetColor = new List<string>();
              List<int[]> FiguresCoords = new List<int[]>();
            for (int i = 0; i < data.Count(); i++)
            {
                string[] addition = data[i].Split(new char[] {'/'});
                if (addition.Length != 2)
                {
                    ErrorLoad("length not equals 2");
                    return;
                }
                GetColor.Add(addition[0]);

                    string[] crds = addition[1].Split(new char[] { ';' });
                int[] cords = new int[crds.Length];
                    for (int t = 0; t < crds.Length; t++)
                    {
                        try
                        {
                            cords[t] = int.Parse(crds[t]); 
                        }
                        catch (Exception)
                        {

                            ErrorLoad("not int coords"  );
                            return;
                        }
                    }

                  FiguresCoords.Add(cords);

            }

            for (int i = 0; i < FiguresCoords.Count(); i++)
            {
                if (FiguresCoords[i].Length % 2 != 0)
                {
                    ErrorLoad("Some coords is not pair. this %2 != 0");
                    return;
                }
                if (FiguresCoords[i].Length > 36*2)
                {
                    ErrorLoad("Too many objects");
                    return;
                }
            }
            for (int i = 0; i < FiguresCoords.Count(); i++)
            {
                for (int q = 0; q < FiguresCoords[i].Length; q+=2)
                {
                    if (FiguresCoords[i][q] % 30 != 0 || FiguresCoords[i][q] < 60 || FiguresCoords[i][q]>210)
                    {
                        ErrorLoad("Xcoord can't multiply of base size");
                        return;
                    }
                }
                for (int q = 1; q < FiguresCoords[i].Length; q+=2)
                {
                    if (FiguresCoords[i][q] > 150 || FiguresCoords[i][q] < 0)
                    {
                        ErrorLoad("Ycoord is not in range");
                        return;
                    }
                }
            }
            string[] ColorTypes = Enum.GetNames(typeof(KnownColor));
            for (int i = 0; i < GetColor.Count(); i++)
            {
                bool truly = false;
                for (int q = 0; q < ColorTypes.Length; q++)
                {

                    if (GetColor[i] == ColorTypes[q])
                    {
                        truly = true;
                        break;
                    }
                }
                if (!truly)
                {
                    ErrorLoad("Color is not exist");
                    return;
                }
            }
            FCoords = FiguresCoords;
            FigureColors = GetColor;
        }
        public void Spawn(PictureBox box, int x, int y,string color)                                       
        {
              if (IsGameOver(x,y))
              {
                GameOver();
                  return;
              }

              box.BackColor = Color.FromName(color);
              box.Location = new Point(x, y);
              box.Name = "box";
              box.Size = new Size(30, 30);
              int[] output = { x, y };
              boxes.Add(box);
              coords.Add(output);
              canvas.Controls.Add(box);
            
            
        }
        public void DrawMoving(PictureBox box, int x, int y,string color)
        {
            box.BackColor = Color.FromName(color);
            box.Location = new Point(x, y);
            box.Name = "box";
            box.Size = new Size(30, 30);
            canvas.Controls.Add(box);
        }
        public void DrawMoving(PictureBox box, int x, int y)
        {
            box.Location = new Point(x, y);
            box.Name = "box";
            box.Size = new Size(30, 30);
            canvas.Controls.Add(box);
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            string ent = keyData.ToString();
            ent = ent.ToLower();
            if ( ent == "left" || ent == "right" || ent == "down" || ent == "space")
            { MoveBox(ent); return base.ProcessCmdKey(ref msg, keyData); }
            else { return false; }
        }
        public void MoveBox(string key)
        {

            if(timer1.Enabled)
            {
                int minX = canvas.Size.Width;
                int minID = 0;
                for (int z = 0; z < boxes.Count; z++)
                {
                    if (coords[z][0] <= minX)
                    {
                        minID = z;
                        minX = coords[z][0];
                    }
                }
                int maxX = 0;
                int maxID = 0;
                for (int z = 0; z < boxes.Count; z++)
                {
                    if (coords[z][0] >= maxX)
                    {
                        maxID = z;
                        maxX = coords[z][0];
                    }
                }
                switch (key)
                {

                    case "left":
                        {
                            if (coords.Count != 0 && coords[minID][0] >= 30 && CanMoveLeft(key))
                            {
                                
                                for(int i=0;i< boxes.Count;i++)
                                {
                                      coords[i][0] -= 30;
                                      boxes[i].Location = new Point(coords[i][0], coords[i][1]);
                                      DrawMoving(boxes[i], coords[i][0], coords[i][1],currentColor);
                                }
                            }
                            break;
                        }
                    case "right":
                        {

                            if (coords.Count != 0 && coords[maxID][0]+30 < canvas.Size.Width && CanMoveRight(key))
                            {
                                for (int i = 0; i < boxes.Count; i++)
                                {
                                    coords[i][0] += 30;
                                    boxes[i].Location = new Point(coords[i][0], coords[i][1]);
                                    DrawMoving(boxes[i], coords[i][0], coords[i][1],currentColor);
                                }
                            }
                            break;
                        }
                    case "down":
                        {
                            while (CanFall_AllObj())
                            {
                                for (int i = 0; i < boxes.Count(); i++)
                                {
                                    coords[i][1] += 5;
                                    boxes[i].Location = new Point(coords[i][0], coords[i][1]);
                                }
                            }
                            for (int i = 0; i < boxes.Count(); i++)
                            {
                                DrawMoving(boxes[i], coords[i][0], coords[i][1],currentColor);
                            }
                                foreach (PictureBox element in boxes)
                                {
                                    StaticObj.Add(element);
                                }
                                foreach (int[] element in coords)
                                {
                                    StaticCoords.Add(element);
                                }
                                boxes.Clear();
                                coords.Clear();

                                CreateFigure();
                            
                            break;
                        }
                    case "space":
                        {
                            RotateFigure();
                            break;
                        }

                }
            }
            
        }
        private void Tick(object sender, EventArgs e)
        {   timer1.Interval = 600 / speed;
            status.Text = "Текущие объекты: " + boxes.Count.ToString() + " Всего объектов: " + StaticObj.Count.ToString();
            score.Text = gamescore.ToString();
            if(CanFall_AllObj())
            {
                for (int i = 0; i < boxes.Count(); i++)
                {
                    coords[i][1] += 5;
                    boxes[i].Location = new Point(coords[i][0], coords[i][1]);
                    DrawMoving(boxes[i], coords[i][0], coords[i][1],currentColor);
                }
            }
            else
            {
                foreach (PictureBox element in boxes)
                {
                    StaticObj.Add(element);
                }
                foreach (int[] element in coords)
                {
                    StaticCoords.Add(element);
                }
                boxes.Clear();
                coords.Clear();

                CreateFigure();
            }
            DeleteLine();
        }
        private bool CanFall_AllObj()
        {
            for(int i = 0; i < boxes.Count(); i++)
            {

                if (coords[i][1] + 5 <= canvas.Size.Height - 30 && CanFall(i))
                {
                   
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
        private bool CanFall(int i)
        {
            bool output = true;
            for (int z = 0; z < StaticObj.Count; z++)
            {
                if ( !CollisionInFall(coords[i],StaticCoords[z]))
                {
                    output = true;
                }
                else
                {
                    output = false;
                    break;
                }
            
            }
            return output;

        }
        private bool CollisionInFall( int[] Coord , int[] stCoords)
        {
            if(Coord[1]+30 >= stCoords[1] && Coord[1] <= stCoords[1]+30 && Coord[0]<stCoords[0]+30 && Coord[0]+30 > stCoords[0])
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool CanMoveLeft(string key)
        {
            int[] ObjLeft;
            ObjLeft = FindX(key);
            
            foreach (int elem in ObjLeft)
            {
                for(int z=0;z<StaticObj.Count;z++)
                {
                    if(coords[elem][0] == StaticCoords[z][0]+30 && coords[elem][1] < StaticCoords[z][1]+30 && coords[elem][1]+30 > StaticCoords[z][1])
                    {
                        return false;
                    }
                }
                
            }
            return true;
        }
        public bool CanMoveRight(string key)
        {
            int[] Obj;
            Obj = FindX(key);

            foreach (int elem in Obj)
            {
                for (int z = 0; z < StaticObj.Count; z++)
                {
                    if (coords[elem][0]+30 == StaticCoords[z][0] && coords[elem][1] < StaticCoords[z][1] + 30 && coords[elem][1] + 30 > StaticCoords[z][1])
                    {
                        return false;
                    }
                }

            }
            return true;
        }
        private int[] FindX( string key)
        {
            List<int> Ucoords = new List<int>();
            for(int i=0;i<boxes.Count;i++)
            {
                Ucoords.Add(coords[i][1]);
            }
            Ucoords = Ucoords.Distinct().ToList();
            List<int[]> UniqID = new List<int[]>();
            for (int i = 0; i < Ucoords.Count(); i++)
            {
                List<int> UCoordsToID = new List<int>();
                for (int z = 0; z < boxes.Count() ; z++)
                {
                    if(coords[z][1] == Ucoords[i])
                    {
                        UCoordsToID.Add(z);
                        
                    }
                }
                UniqID.Add(UCoordsToID.ToArray());
            }
            List<int> output = new List<int>();
            if(key == "left")
            {
                for (int i = 0; i < UniqID.Count(); i++)
                {
                    int minXID = 0;
                    int minX = canvas.Size.Width;
                    for (int z = 0; z < UniqID[i].Length; z++)
                    {


                        if (coords[UniqID[i][z]][0] < minX)
                        {
                            minX = coords[UniqID[i][z]][0];
                            minXID = UniqID[i][z];
                        }
                    }
                    output.Add(minXID);

                }
                
            }
            else if(key == "right")
            {
                for (int i = 0; i < UniqID.Count(); i++)
                {
                    int maxXID = 0;
                    int maxX = 0;
                    for (int z = 0; z < UniqID[i].Length; z++)
                    {
                        if (coords[UniqID[i][z]][0] > maxX)
                        {
                            maxX = coords[UniqID[i][z]][0];
                            maxXID = UniqID[i][z];
                        }
                    }
                    output.Add(maxXID);

                }
            }
            return output.ToArray();
        }
        private void Pause(object sender, MouseEventArgs e)
        {
            if (timer1.Enabled)
            {
                timer1.Enabled = false;
                pause_button.Text = "Unpause";
            }
            else
            {
                timer1.Enabled = true;
                pause_button.Text = "pause";
            }
        }
        public void DeleteLine()
        {

            List<int> UcoordsY = new List<int>();
            for (int i = 0; i < StaticObj.Count; i++)
            {
                UcoordsY.Add(StaticCoords[i][1]);
            }
            UcoordsY = UcoordsY.Distinct().ToList();
            UcoordsY.Sort();
            UcoordsY.Reverse();
            List<List<int>> IdstObj = new List<List<int>>(); //Id объектов уникальных линий
            foreach (int el in UcoordsY)
            {

                List<int> Idst = new List<int>();
                for (int i = 0; i < StaticObj.Count; i++)
                {

                    if (StaticCoords[i][1] == el)
                    {

                        Idst.Add(i);
                    }
                }
                IdstObj.Add(Idst);
            }
            foreach(List<int> elem in IdstObj)
            {
                elem.Reverse();
            }
            for(int i=0;i<IdstObj.Count();i++)
            {
                if(IdstObj[i].Count == 10)
                {
                    for(int z=0;z< IdstObj[i].Count();z++)
                    {
                        StaticCoords.RemoveAt(IdstObj[i][z]);
                        StaticObj.RemoveAt(IdstObj[i][z]);
                        canvas.Controls.RemoveAt(IdstObj[i][z]);
                    }
                    DropUpperLines(UcoordsY[i]);
                    gamescore++;
                    break;
                }
                
            }
        }
        public void DropUpperLines(int lineY)
        {
            List<int> linesToMove = new List<int>();
            for(int i=0;i<StaticObj.Count();i++)
            {
               if(StaticCoords[i][1]<lineY)
               {
                    linesToMove.Add(StaticCoords[i][1]);
               }
            }
            linesToMove.Distinct();
            linesToMove.Sort();
            linesToMove.Reverse();
            for(int i=0;i<linesToMove.Count();i++)
            {
                for(int z=0;z<StaticCoords.Count();z++)
                {
                    if(StaticCoords[z][1] == linesToMove[i])
                    {
                        StaticCoords[z][1] += 30;
                        StaticObj[z].Location = new Point(StaticCoords[z][0], StaticCoords[z][1]);
                    }
                }
            }
            DrawAllStaticObj();
        }
        private void DrawAllStaticObj()
        {
            for(int i=0;i< StaticObj.Count();i++)
            {
                  DrawMoving(StaticObj[i], StaticCoords[i][0], StaticCoords[i][1]);
            }
        }
        private void RotateFigure()
        {
            int[] min = new int[2];
            int[] max = new int[2];
            min = FindExtremeFirst();
            max = FindExtremeLast();
            int range1 = max[0] - min[0]+1 ;
            int range2 = max[1] - min[1]+1 ;
            int[,] boxCanv = new int[range1, range2];
           // label1.Text = range1 + " " + range2;
            for (int i = 0; i < boxes.Count(); i++)
            {
                boxCanv[coords[i][0] - min[0], coords[i][1] - min[1]] = i + 1;
                //label1.Text += Environment.NewLine + (coords[i][0] - min[0]) + " " + (coords[i][1] - min[1]);
            }
           int[,] boxCanvNew = RotateMatrix(boxCanv, range1, range2);
            List<int[]> rotated = new List<int[]>();
            for (int t = 0; t < boxes.Count()+1; t++)
            {
                for (int i = 0; i < range2; i++)
                {
                    for (int z = 0; z < range1; z++)
                    {
                        if (boxCanvNew[i, z] != 0 && boxCanvNew[i, z] == t)
                        {
                            int[] mass = { i+min[0],z+min[1]}; 
                            rotated.Add(mass); 
                        }
                    }
                }
            }
            //label4.Text = CanRotate(rotated).ToString();
            if (CanRotate(rotated))
            {
                for (int z = 0; z < rotated.Count(); z++)
                {
                    coords[z] = rotated[z];
                }
            }
            
            
            

            for (int i = 0; i < rotated.Count(); i++)
            {
                boxes[i].Location = new Point(coords[i][0], coords[i][1]);
                DrawMoving(boxes[i], coords[i][0], coords[i][1]);
            }
        }
        private int[,] RotateMatrix(int[,] matrix,int range1,int range2)
        {
            int[,] rotated = new int[range2, range1];
            if (range2 >= range1)
            {
                for (int x = 0; x < range2; x++)
                {
                    for (int y = 0; y < range1; y++)
                    {
                        rotated[x, y] = matrix[y,range2-1- x];
                    }

                }
            }
            else
            {
                for (int x = 0; x < range1; x++)
                {
                    for (int y = 0; y < range2; y++)
                    {
                        rotated[y,x] = matrix[x,range2-1-y];
                    }

                }
            }
            return rotated;
        }
        private int[] FindExtremeFirst()
        {
            int y0 = canvas.Size.Height;
            int x0 = canvas.Size.Width;
            foreach (int[] elem in coords)
            {
                if(elem[1] < y0)
                {
                    y0 = elem[1];
                }

            }
            foreach (int[] elem in coords)
            {
                if (elem[0] < x0)
                {
                    x0 = elem[0];
                }

            }
            int[] output = {x0,y0};
            return output;
        }
        private int[] FindExtremeLast()
        {
            int y0 = 0;
            int x0 = 0;
            foreach (int[] elem in coords)
            {
                if (elem[1] > y0)
                {
                    y0 = elem[1];
                }

            }
            foreach (int[] elem in coords)
            {
                if (elem[0] > x0)
                {
                    x0 = elem[0];
                }

            }
            int[] output = { x0, y0 };
            return output;
        }
        public bool CanRotate(List<int[]> rotated)
        {
            for (int i = 0; i < rotated.Count(); i++)
            {
                if (rotated[i][0]+30>canvas.Size.Width || rotated[i][0] < 0 || rotated[i][1] < 0 || rotated[i][1]+30>canvas.Size.Height)
                {
                    return false;
                }
                for (int z = 0; z < StaticCoords.Count(); z++)
                {
                    if ((StaticCoords[z][0] + 30 > rotated[i][0] && StaticCoords[z][0] < rotated[i][0] + 30) && (StaticCoords[z][1] + 30 > rotated[i][1] && StaticCoords[z][1] < rotated[i][1] + 30))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        public bool IsGameOver(int x,int y)
        {
            for (int i = 0; i < StaticObj.Count(); i++)
            {
                if (StaticCoords[i][0]+30>x && StaticCoords[i][0]<x+30 && StaticCoords[i][1]+30>y && StaticCoords[i][1]<y+30)
                {
                    return true;
                }
            }
            return false;
        }
        private void GameOver()
        {
            timer1.Enabled = false;
            MessageBox.Show("Game Over" + Environment.NewLine + "Your score: " + gamescore);
            canvas.Controls.Clear();
            NewGame(start_button,null);
            return;
        }
        private void Speed_Click(object sender, MouseEventArgs e)
        {
            int enter = 0;
            try
            {
                enter = int.Parse(SpeedBox.Text);
            }
            catch (Exception)
            {

                MessageBox.Show("Text input exception");
                return;
            }
            if (enter > 0 && enter <= 100)
            {
                speed = enter;
            }
            else
            {
                MessageBox.Show("Out of range speed");
                return;
            }
        }
        public void ErrorLoad(string text)
        {
            MessageBox.Show("Can't load Figures" + Environment.NewLine + "Check Figure Constructor." + Environment.NewLine + "Error: " + text);
            Application.Exit();
        }
    }
}
//System.Diagnostics.Process Proc = new System.Diagnostics.Process();
//Proc.StartInfo.FileName = "ConsoleConstructor.exe";
           // Proc.Start();
