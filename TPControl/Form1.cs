using System;

using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Design;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;
using AMX_Controls;

using System.Xml;
namespace TPControl
{
   

    public partial class TPC : Form
    {

        public TPC()
        {
            DoubleBuffered = true;
            InitializeComponent();
        }
        string xmlFileName="",
            //папка проэкта, задается при создании, либо при открытии проэкта
            pathString="",//папка для картинок
            destFile="";//полный путь к элементу
        TabPage TabPanel = new TabPage();
        AMX_Button amxBtn=new AMX_Button();
        AMX_Level amxLvl=new AMX_Level();
        AMX_Level amx_lvl=new AMX_Level();
        Size tabSize=new Size(1032,796);
        string[,] history = new string [1,5];

        public bool stripLoading = false, aimDraw=false, dragButton=true,drawing=false;
        public Timer time = new Timer();
        public bool autosave = false;

        public  int i,action,pageNum=1,ResizableBorderWidth=5,itemsIndex=1,DDWidth=6;


        private void goBackForOneStep() {//считывает из массива истории собитий последнее и делает шаг назад
            hideDDs();
            if (history.Length/5>2)
            {
                string name = "";
                int x, y, w, h;
                name = history[history.Length / 5 - 2, 0];
                x = int.Parse(history[history.Length / 5 - 2, 1]);
                y = int.Parse(history[history.Length / 5 - 2, 2]);
                w = int.Parse(history[history.Length / 5 - 2, 3]);
                h = int.Parse(history[history.Length / 5 - 2, 4]);

                foreach (Control cnt in tab.SelectedTab.Controls)
                {

                    if (cnt.ToString() == "AMX_Controls.AMX_Button")
                    {
                        AMX_Button abs = cnt as AMX_Button;
                        if (abs.Name.ToString() == name)
                        {
                            abs.Location = new Point(x, y);
                            abs.Size = new Size(w, h);
                            //abs.Focus();
                            tab.SelectedTab.Refresh();
                        }
                    }

                    if (cnt.ToString() == "AMX_Controls.AMX_Level")
                    {
                        AMX_Level alx = cnt as AMX_Level;
                        if (alx.Name.ToString() == name)
                        {
                            amxLvl = (AMX_Level)alx;
                            amxLvl.Location = new Point(x, y);
                            amxLvl.Size = new Size(w, h);
                            //amxLvl.Focus();
                            tab.SelectedTab.Refresh();
                        }
                    }

                }

                string[,] buf = new string[history.Length / 5, 5];
                for (int i = 0; i < buf.Length / 5; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        buf[i, j] = history[i, j];
                    }
                }

                history = new string[buf.Length / 5 - 1, 5];

                for (int i = 0; i < (buf.Length / 5) - 1; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        history[i, j] = buf[i, j];
                    }
                }

            }

        }
        //запись элемента в историю событий для бекстепа
        private void saveElementToHistory(string name, int x, int y, int width, int height) {

            //проверить на повторение
            int change = 1;

            if (
            history[history.Length / 5 - change, 0] != name ||
            history[history.Length / 5 - change, 1] != x.ToString() ||
            history[history.Length / 5 - change, 2] != y.ToString() ||
            history[history.Length / 5 - change, 3] != width.ToString() ||
            history[history.Length / 5 - change, 4] != height.ToString())
            {
               
                    string[,] buf = new string[history.Length / 5, 5];
                    for (int i = 0; i < buf.Length / 5; i++)
                    {
                        for (int j = 0; j < 5; j++)
                        {
                            buf[i, j] = history[i, j];
                        }
                    }

                    history = new string[buf.Length / 5 + 1, 5];

                    for (int i = 0; i < buf.Length / 5; i++)
                    {
                        for (int j = 0; j < 5; j++)
                        {
                            history[i, j] = buf[i, j];
                        }
                    }
                    history[history.Length / 5 - 1, 0] = name;
                    history[history.Length / 5 - 1, 1] = x.ToString();
                    history[history.Length / 5 - 1, 2] = y.ToString();
                    history[history.Length / 5 - 1, 3] = width.ToString();
                    history[history.Length / 5 - 1, 4] = height.ToString();
                }
            
        }
        private void TPC_Load(object sender, EventArgs e)
        {

            time.Interval = 10;
            time.Enabled = true;
            time.Tick+=new EventHandler(time_Tick);
            time.Start();
            progress.Value = 0;
            initializeGrids();
            hideAllGrids();
            setActivePanel();
            progress.Value = 100;
            progress.Value = 0; 
            if(stripLoading)placeStrips();
            comboboxRefresh(0);
            tab.Location = new Point(5, 5);
            showPageGrid();
            
            initializeDD();
            writeInfoAbputCurrentPage();
            disactivateForm();
            placeStrips();
            history[0, 0] = "";
            history[0, 1] = "";
            history[0, 2] = "";
            history[0, 3] = "";
            history[0, 4] = "";
            setUpFlags();
            autosaveTimerSteps = autosaveTimer * 60 * (1000 / time.Interval);
            
        }
        private void setActivePanel()
        {
            TabPanel = tab.SelectedTab;
            TabPanel.Cursor = Cursors.Cross;
            TabPanel.MouseMove += new MouseEventHandler(TabPanel_MouseMove);
            TabPanel.MouseLeave += new EventHandler(TabPanel_MouseLeave);

            TabPanel.MouseDown+=new MouseEventHandler(TabPanel_MouseDown);
            TabPanel.MouseUp+=new MouseEventHandler(TabPanel_MouseUp);
        }
        
        Point drawing1, drawing2;
        public int drawingX=0, drawingY=0, drawingW=0, drawingH=0;
        bool drawingActive = false;
        private void TabPanel_MouseUp(object sender, MouseEventArgs e) {
            if (drawing)
            {
                
                createItemByDrawing();
                
                
            }
            drawingActive = false;
        }

        private void drawingXYWH() { 
        
        if (drawing1.X <= drawing2.X)
            {
                drawingX = drawing1.X;
                drawingW = drawing2.X - drawing1.X;
            }
            else {
                drawingX = drawing2.X;
                drawingW = drawing1.X - drawing2.X;
            }

            if (drawing1.Y <= drawing2.Y)
            {
                drawingY = drawing1.Y;
                drawingH = drawing2.Y - drawing1.Y;
            }
            else {
                drawingY = drawing2.Y;
                drawingH = drawing1.Y - drawing2.Y;
            }
        }//передает координаты прямоугольника после его рисования
        
        private void createItemByDrawing() {
            
            if (drawingH >= 5 && drawingW >= 5)
            {
                
                chooseButton.Show(TabPanel,drawing2.X-75,drawing2.Y-15);
            }

        }//вызывает контекстное меню выбора объекта(кнопка, уровень)

        private void TabPanel_MouseDown(object sender, MouseEventArgs e)
        {
            
            unfocusAll();
            drawingActive = true;
            Graphics g = tab.SelectedTab.CreateGraphics();
            g.Clear(tab.SelectedTab.BackColor);
            drawingW = 0;
            if (drawing)
            {
                drawing1 = e.Location;
                
            }
            else 
            { 
            selectItemsinComboItems(TabPanel.Name);
            showPageGrid();
            writeInfoAbputCurrentPage();
            }
            
        }
        public void setUpFlags() {

            aimCursorEnable.Checked = aimDraw;
            showAimToolStripMenuItem.Checked = aimDraw;
            if (aimDraw) aimCursorEnable.CheckState = CheckState.Checked;
            else aimCursorEnable.CheckState = CheckState.Unchecked;

            drawingToolStripMenuItem.Checked = drawing;
            drawingEnable.Checked = drawing;
        
        }
        private void TabPanel_MouseMove(object sender, MouseEventArgs e) {
            
            reDraw();
            if (drawing&&drawingActive) {
                drawing2 = e.Location;
                drawingXYWH();
                
                //MessageBox.Show();
            }
        }

        private void TabPanel_MouseLeave(object sender, EventArgs e)
        {

            if (aimDraw) drawAim(0, 0);
            if (drawing)drawRect();
            mousePosInfo.Text = "out";
        }

        public int autosaveTimer = 5;//minutes. steps to perform action is 5*60*(1000/time.interval)
        public int autosaveTimerSteps = 0;
        int timerSteps = 0;

        private void reDraw() {

            Point p = TabPanel.PointToClient(Cursor.Position);

            if (p.X >= 0 && p.Y >= 0 && p.X <= 1024 && p.Y <= 768)
            {
                mousePosInfo.Text = p.X.ToString() + "," + p.Y.ToString();
                if (aimDraw) drawAim(p.X, p.Y);
            }
            if (drawingActive) drawRect();

            }

        private void time_Tick(object sender, EventArgs e) {

            Point p = TabPanel.PointToClient(Cursor.Position);
            
            if (p.X >= 0 && p.Y >= 0 && p.X <= 1024 && p.Y <= 768)
            {
                mousePosInfo.Text = p.X.ToString() + "," + p.Y.ToString();
                //if(aimDraw)drawAim(p.X, p.Y);
            }
            
            timerSteps++;
            if (timerSteps >= autosaveTimerSteps) {
                timerSteps = 0;
                createXmlFile();
            }
            

        }
        //***********Initialization of grids*******
        private void initializeGrids() { 
            createPageGrid();
            createBtnGenGrid();
            createBtnProgGrid();
            createBtnOnStGrid();
            createBtnOffStGrid();           
            createLvlGenGrid();
            createLvlProgGrid();
            createLvlStGrid();

            
        }
        private void createPageGrid() {
            PageGrid.Rows.Clear();
            PageGrid.Rows.Add("Page name","");
            PageGrid.Rows.Add("Off-state pickturebox", "");
            PageGrid.Rows.Add("On-state pickturebox", "");
            PageGrid.Rows.Add("Background color", "");
        }

        private void createBtnGenGrid() {
            btnGenGrid.Rows.Clear();
            btnGenGrid.Rows.Add("Name", "");
            btnGenGrid.Rows.Add("Left", "");
            btnGenGrid.Rows.Add("Top", "");
            btnGenGrid.Rows.Add("Height", "");
            btnGenGrid.Rows.Add("Width", "");
            string[] states = new string[] { "OFF", "ON" };
            btnGenGrid.Rows.Add(createRow("Default state",states,btnGenGrid));
            btnGenGrid.Rows.Add("z-index", "");            

        }

        private void createBtnProgGrid() {
            btnProgGrid.Rows.Clear();
            string[] states = new string[] { "none", "channel","momentary" };
            btnProgGrid.Rows.Add(createRow("Feedback", states, btnProgGrid));
            
            btnProgGrid.Rows.Add("Address Port","");
            btnProgGrid.Rows.Add("Address Code", "");
            btnProgGrid.Rows.Add("Channel Port", "");
            btnProgGrid.Rows.Add("Channel Code", "");
            btnProgGrid.Rows.Add("String Output Port", "");
            btnProgGrid.Rows.Add("String Output", "");
            btnProgGrid.Rows.Add("Command Output Port", "");
            btnProgGrid.Rows.Add("Command Output", "");


        }

        private void createBtnOnStGrid() {
            btnOnStGrid.Rows.Clear();
            string[] states = new string[] { "none", "Use"};
            btnOnStGrid.Rows.Add(createRow("Picturebox", states, btnOnStGrid));
            btnOnStGrid.Rows.Add("Backgroung Color", "");
            btnOnStGrid.Rows.Add("Text Color", "");
            btnOnStGrid.Rows.Add("Font", "");
            btnOnStGrid.Rows.Add("Text", "");
            btnOnStGrid.Rows.Add("Bitmap", "");
            

        }

        private void createBtnOffStGrid()
        {
            btnOffStGrid.Rows.Clear();
            string[] states = new string[] { "none", "Use" };
            btnOffStGrid.Rows.Add(createRow("Picturebox", states, btnOffStGrid));
            btnOffStGrid.Rows.Add("Backgroung Color", "");
            btnOffStGrid.Rows.Add("Text Color", "");
            btnOffStGrid.Rows.Add("Font", "");
            btnOffStGrid.Rows.Add("Text", "");
            btnOffStGrid.Rows.Add("Bitmap", "");

        }
        
        private void createLvlGenGrid()
        {
            LvlGenGrid.Rows.Clear();
            LvlGenGrid.Rows.Add("Name", "");
            LvlGenGrid.Rows.Add("Left", "");
            LvlGenGrid.Rows.Add("Top", "");
            LvlGenGrid.Rows.Add("Height", "");
            LvlGenGrid.Rows.Add("Width", "");
            LvlGenGrid.Rows.Add("Minimum level", "");
            LvlGenGrid.Rows.Add("Maximum level", "");
            LvlGenGrid.Rows.Add("Default Level","");
            string[] states = new string[] {"vertical", "horizontal"};
            LvlGenGrid.Rows.Add(createRow("Orientation", states, LvlGenGrid));
            
            LvlGenGrid.Rows.Add("z-index", "");

        }

        private void createLvlProgGrid()
        {
            LvlProgGrid.Rows.Clear();
            string[] states = new string[] { "display", "active"};
            LvlProgGrid.Rows.Add(createRow("Level function", states, LvlProgGrid));
            LvlProgGrid.Rows.Add("Level Port", "");
            LvlProgGrid.Rows.Add("Level Code", "");

            LvlProgGrid.Rows.Add("Address Port", "");
            LvlProgGrid.Rows.Add("Address Code", "");

        }

        private void createLvlStGrid()
        {
            LvlStGrid.Rows.Clear();
            string[] states = new string[] { "none", "Use" };
            LvlStGrid.Rows.Add(createRow("Picturebox Off", states, LvlStGrid));
            LvlStGrid.Rows.Add(createRow("Picturebox On", states, LvlStGrid));
            LvlStGrid.Rows.Add("Backgroung Color Off", "");
            LvlStGrid.Rows.Add("Backgroung Color On", "");
            LvlStGrid.Rows.Add("Text Color", "");
            LvlStGrid.Rows.Add("Font", "");
            LvlStGrid.Rows.Add("Text", "");

        }

        private void unfocusAll() {
                        
            hideDDs();
        
        }
       
        //*****************************************
        private Color negativeColor(Color col) {

            int r=0, g=0, b=0, a=255;

            r = 255 - col.R;
            g = 255 - col.G;
            b = 255 - col.B;

           /* if (col.R < 130) r = 255;
            else r = 0;
            if (col.G < 130) g = 255;
            else g = 0;
            if (col.B < 130) b = 255;
            else b = 0;
            */
            return Color.FromArgb(a, r, g, b);           
             


        }

        private void drawAim(float xPos, float yPos)
        {
            
            Graphics g = tab.SelectedTab.CreateGraphics();
            
            if (xPos > 0 && yPos > 0)
            {

                g.Clear(tab.SelectedTab.BackColor);
                HatchBrush br = new HatchBrush(HatchStyle.Percent50, negativeColor(tab.SelectedTab.BackColor));
                Pen pen = new Pen(br);
                g.DrawLine(pen, xPos, 0, xPos, tab.SelectedTab.Height);
                g.DrawLine(pen, 0, yPos, tab.SelectedTab.Width, yPos);

            }

            else {
                g.Clear(tab.SelectedTab.BackColor);
            }

        }


        private void drawRect()//рисование  прямоугольника для создания элемента
        {

            Graphics g = tab.SelectedTab.CreateGraphics();
            g.Clear(tab.SelectedTab.BackColor);
            SolidBrush br = new SolidBrush(negativeColor(tab.SelectedTab.BackColor));
            g.FillRectangle(br, drawingX, drawingY, drawingW, drawingH);


        }

        private void amxBtn_MouseMove(object sender, MouseEventArgs e) {

            /*amxBtn = (AMX_Button)sender;
            int Xpos, Ypos;


            Xpos = amxBtn.Width;//praviy kray
            Ypos = amxBtn.Height;//nizniy kray

            if (!fixCursorType)
            {

                if ((e.Y < Ypos - ResizableBorderWidth) && (e.X < Xpos - ResizableBorderWidth)) //move
                {
                    amxBtn.Cursor = Cursors.SizeAll;
                    if (moveButton)
                    {
                        action = 0;
                        fixCursorType = true;
                    }
                }

                if ((e.X <= Xpos + ResizableBorderWidth) && (e.X >= (Xpos - ResizableBorderWidth)) && (e.Y <= Ypos + ResizableBorderWidth) && (e.Y >= (Ypos - ResizableBorderWidth)))
                { //praviy ugol

                    amxBtn.Cursor = Cursors.SizeNWSE;
                    
                    if (moveButton) 
                    { 
                        fixCursorType = true; 
                        action = 3;
                    }


                }

                else if (e.X <= Xpos && e.X >= Xpos - ResizableBorderWidth)
                { //pravaya storona
                    amxBtn.Cursor = Cursors.SizeWE;

                    if (moveButton)
                    {
                        action = 1;
                        fixCursorType = true;
                    }

                }

                else if (e.Y <= Ypos && e.Y >= Ypos - ResizableBorderWidth)//niz
                {
                    amxBtn.Cursor = Cursors.SizeNS;
                    if (moveButton)
                    {
                        action = 2;
                        fixCursorType = true;
                    }
                }
            }


            if (moveButton)
            {
                amxBtn = (AMX_Button)sender;

                if (action == 1)
                {
                    amxBtn.Width = e.X;
                }

                if (action == 2)
                {
                    amxBtn.Height = e.Y;
                }

                if (action == 3)
                {
                    amxBtn.Width = e.X;
                    amxBtn.Height = e.Y;
                }



                if (action == 0)
                {

                    amxBtn.Top += e.Y - p.Y;
                    amxBtn.Left += e.X - p.X;

                }
                tab.SelectedTab.Refresh();
                WriteToTextBoxes(amxBtn);

            }

            */
        }

        private AMX_Button createAmxButton(string name, int xPos, int yPos, int width, int height) {

            AMX_Button amxBtn = new AMX_Button();
            i = 0;
            do
            {
                i++;
            }
            while (scanForButton(i));

            amxBtn.Name = name+i;
            amxBtn.Left = xPos;
            amxBtn.Top = yPos;
            amxBtn.Width = width;
            amxBtn.Height = height;
            amxBtn.Parent = tab.SelectedTab;
            amxBtn.TabIndex = itemsIndex;
            amxBtn.BorderStyleOff = BorderStyle.None;
            amxBtn.BorderStyleOn = BorderStyle.None;
            itemsIndex++;
            amxBtn.MouseClick+=new MouseEventHandler(amxBtn_MouseClick);
            amxBtn.MouseDown+=new MouseEventHandler(amxBtn_MouseDown);
            amxBtn.MouseUp+=new MouseEventHandler(amxBtn_MouseUp);
           
            amxBtn.MouseMove +=new MouseEventHandler(amxBtn_MouseMove);
            amxBtn.GotFocus+=new EventHandler(amxBtn_GotFocus);
            comboboxRefresh(amxBtn.TabIndex);
            amxBtn.Focus();
            consoletext.Text = "Button "+amxBtn.Name + " was added on "+tab.SelectedTab.Name;
            return amxBtn;
        
        
        }

        private AMX_Level createAmxLevel(string name, int xPos, int yPos, int width, int height) {

            AMX_Level amxLvl = new AMX_Level();

            i = 0;
            do
            {
                i++;
            }
            while (scanForLevel(i));

            amxLvl.Name = name+i;
            amxLvl.TabIndex = itemsIndex;
            itemsIndex++;
            amxLvl.Location = new Point(xPos,yPos);
            amxLvl.Size = new Size(width,height);
            amxLvl.Parent = tab.SelectedTab;
            amxLvl.BorderStyle = BorderStyle.None;
            amxLvl.MouseClick+=new MouseEventHandler(amxLvl_MouseClick);
            amxLvl.GotFocus+=new EventHandler(amxLvl_GotFocus);    
            amxLvl.LostFocus+=new EventHandler(amxLvl_LostFocus);
            comboboxRefresh(amxLvl.TabIndex);
            consoletext.Text = "Level " + amxLvl.Name + " was added on "+tab.SelectedTab.Name;
            amxLvl.Focus();
            return amxLvl;
        
        }

        private void amxLvl_LostFocus(object sender, EventArgs e) {
            
        }
        private void amxLvl_MouseClick(object sender, MouseEventArgs e) 
        {
            amxLvl = (AMX_Level)sender;
            amxLvl.Focus();
            
        }

        private void amxLvl_GotFocus(object sender, EventArgs e) {

            unfocusAll();
            AMX_Level amx_lvl = (AMX_Level)sender;
            createDDControl(amx_lvl);
            selectItemsinComboItems(amx_lvl.Name.ToString());
            showLvlGrids();
            writeLvlInfo((AMX_Level)sender);
            saveElementToHistory(amx_lvl.Name, amx_lvl.Left, amx_lvl.Top, amx_lvl.Width, amx_lvl.Height);
        }

        private void addAmxButtonToPanel() 
        {
            tab.SelectedTab.Controls.Add(createAmxButton("P"+(tab.TabPages.IndexOf(tab.SelectedTab)+1).ToString()+"_Btn", 10, 10, 200, 200));
            sortItems();
        }        
             
        private void addAmxLevelToPanel()
        {

            tab.SelectedTab.Controls.Add(createAmxLevel("P" + (tab.TabPages.IndexOf(tab.SelectedTab) + 1).ToString() + "_Lvl", 10, 10, 100, 200));
            sortItems();
        }

        private bool scanForButton(int i)
        {

            try
            {
                foreach (Control cnt in tab.SelectedTab.Controls)
                {
                    if (cnt.ToString() == "AMX_Controls.AMX_Button")
                    {
                        AMX_Button ab = cnt as AMX_Button;
                        if (ab.Name.ToString() == ("P" + (tab.TabPages.IndexOf(tab.SelectedTab) + 1).ToString() + "_Btn" + i))
                        {
                            return true;
                        }
                    }


                }
            }
            catch { }
            return false;
        }

        private bool scanForLevel(int i)
        {

            try
            {
                foreach (Control cnt in tab.SelectedTab.Controls)
                {
                    if (cnt.ToString() == "AMX_Controls.AMX_Level")
                    {
                        AMX_Level al = cnt as AMX_Level;
                        if (al.Name.ToString() == ("P" + (tab.TabPages.IndexOf(tab.SelectedTab) + 1).ToString() + "_Lvl" + i))
                        {
                            return true;
                        }
                    }

                }
            }
            catch { }
            return false;
        }

        private void amxBtn_MouseClick(object sender, MouseEventArgs e) 
        {
            amxBtn = (AMX_Button)sender;
            amxBtn.Focus();
           
        }
        private void amxBtn_MouseDown(object sender, MouseEventArgs e)
        {
           /* amxBtn = (AMX_Button)sender;
            moveButton = true;
            p = e.Location;

            prevSize = amxBtn.Size;*/

        
        }
        private void amxBtn_GotFocus(object sender, EventArgs e) 
        {
            unfocusAll();
            amxBtn = (AMX_Button)sender;
            createDDControl(amxBtn);
          
            showBtnGrids();
            selectItemsinComboItems(amxBtn.Name.ToString());
            WriteToTextBoxes((AMX_Button)sender);
            saveElementToHistory(amxBtn.Name, amxBtn.Left, amxBtn.Top, amxBtn.Width, amxBtn.Height);         
            
        
        }
        
        private void amxBtn_MouseUp(object sender, MouseEventArgs e)
        {
         /*   amxBtn = (AMX_Button)sender;
            moveButton = false;
            if (amxBtn.Size.Width == 0 || amxBtn.Size.Height == 0) {
                amxBtn.Size = prevSize;            
            }
            try
            {
                WriteToTextBoxes((AMX_Button)sender);
            }
            catch {

                amxBtn.Size = prevSize;
                WriteToTextBoxes(amxBtn);
            }
            fixCursorType = false;*/
        }
        
        private DataGridViewRow createRow(string name,string[] states,DataGridView target){//создание в таблицах меню выбора
        
            DataGridViewRow row = new DataGridViewRow();
            row.CreateCells(btnGenGrid);
            DataGridViewComboBoxCell combobox = new DataGridViewComboBoxCell();
            
            combobox.Items.AddRange(states);
            row.HeaderCell.Value = name;
            row.Cells[1] = combobox;
            row.Cells[0].Value = name;
            row.Cells[1].Value = (row.Cells[1] as DataGridViewComboBoxCell).Items[0];



        return row;
        }

        private void newProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            createNewProject();
        }

        private void openProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
           
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = " XML Files|*.xml";
            if (ofd.ShowDialog() == DialogResult.OK)
            {

                tab.TabPages.Clear();
                string filePath = "";
                filePath = ofd.FileName;
                openXmlFile(filePath);
                activateForm();

            }
        }

        private void saveProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            createXmlFile();
        }

        private void addAmxButtonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            addAmxButtonToPanel();
        }

        private void setBackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tab.TabPages.Remove(plusPage);            
            TabPage page = addPage();
            tab.TabPages.Add(page);
            tab.TabPages.Add(plusPage);
            tab.SelectTab(page);
        }

        private void WriteToTextBoxes(AMX_Button amx_btn)
        {
            

            //-----General
            btnGenGrid.Rows[0].Cells[1].Value = amx_btn.Name.ToString();
            btnGenGrid.Rows[1].Cells[1].Value = amx_btn.Left.ToString();
            btnGenGrid.Rows[2].Cells[1].Value = amx_btn.Top.ToString();
            btnGenGrid.Rows[3].Cells[1].Value = amx_btn.Height.ToString();
            btnGenGrid.Rows[4].Cells[1].Value = amx_btn.Width.ToString();

            if (amxBtn.Pushed) btnGenGrid.Rows[5].Cells[1].Value = "ON";
            else btnGenGrid.Rows[5].Cells[1].Value = "OFF";
            btnGenGrid.Rows[6].Cells[1].Value = amx_btn.TabIndex.ToString();


            //-----Programming

            
            btnProgGrid.Rows[0].Cells[1].Value = amx_btn.Feedback.ToString();
            btnProgGrid.Rows[1].Cells[1].Value = amx_btn.Address_Port.ToString();
            btnProgGrid.Rows[2].Cells[1].Value = amx_btn.Address_Code.ToString();
            btnProgGrid.Rows[3].Cells[1].Value = amx_btn.Channel_Port.ToString();
            btnProgGrid.Rows[4].Cells[1].Value = amx_btn.Channel_Code.ToString();
            btnProgGrid.Rows[5].Cells[1].Value = amx_btn.String_Output_Port.ToString();
            btnProgGrid.Rows[6].Cells[1].Value = amx_btn.String_Output.ToString();
            btnProgGrid.Rows[7].Cells[1].Value = amx_btn.Command_Output_Port.ToString();
            btnProgGrid.Rows[8].Cells[1].Value = amx_btn.Command_Output.ToString();

            //-----States

            
            string textTowriteAboutFontOn = "";
            if (amx_btn.FontOn.Name != "") textTowriteAboutFontOn += amx_btn.FontOn.Name + ", " + amx_btn.FontOn.Size;
            if (amx_btn.FontOn.Bold) textTowriteAboutFontOn += ", b";
            if (amx_btn.FontOn.Italic) textTowriteAboutFontOn += ", i";
            if (amx_btn.FontOn.Underline) textTowriteAboutFontOn += ", u";

            if (amx_btn.PictureBoxOn != null) btnOnStGrid.Rows[0].Cells[1].Value = "Use";
            else btnOnStGrid.Rows[0].Cells[1].Value = "none";
            
            btnOnStGrid.Rows[1].Cells[1].Value = string.Format("{0:X6}", amx_btn.FillColorOn.ToArgb());
            btnOnStGrid.Rows[2].Cells[1].Value = string.Format("{0:X6}", amx_btn.TextColorOn.ToArgb());
            btnOnStGrid.Rows[3].Cells[1].Value = textTowriteAboutFontOn;
            btnOnStGrid.Rows[4].Cells[1].Value = amx_btn.TextOn.ToString();


            if (amx_btn.BitmapOn != null) {

                string pathToOn = "";
                pathToOn = Path.GetFullPath(System.IO.Directory.GetFiles(pathString, tab.SelectedTab.Name + "_" + amx_btn.Name + "_btm_on.*")[0].ToString()).ToString();

                if (File.Exists(pathToOn))
                {
                    btnOnStGrid.Rows[5].Cells[1].Value = "images\\" + Path.GetFileName(pathToOn);
                }
            
            
            }
            else btnOnStGrid.Rows[5].Cells[1].Value = "none";
            
            
            string textTowriteAboutFontOff = "";
            if (amx_btn.FontOff.Name != "") textTowriteAboutFontOff += amx_btn.FontOff.Name + ", " + amx_btn.FontOff.Size;
            if (amx_btn.FontOff.Bold) textTowriteAboutFontOff += ", b";
            if (amx_btn.FontOff.Italic) textTowriteAboutFontOff += ", i";
            if (amx_btn.FontOff.Underline) textTowriteAboutFontOff += ", u";

            if (amx_btn.PictureBoxOff != null) btnOffStGrid.Rows[0].Cells[1].Value = "Use";
            else btnOffStGrid.Rows[0].Cells[1].Value = "none";
            
            btnOffStGrid.Rows[1].Cells[1].Value = string.Format("{0:X6}", amx_btn.FillColorOff.ToArgb());
            btnOffStGrid.Rows[2].Cells[1].Value = string.Format("{0:X6}", amx_btn.TextColorOff.ToArgb());
            btnOffStGrid.Rows[3].Cells[1].Value = textTowriteAboutFontOff;
            btnOffStGrid.Rows[4].Cells[1].Value = amx_btn.TextOff.ToString();


            if (amx_btn.BitmapOff != null) {

                string pathToOff = "";

                pathToOff = Path.GetFullPath(System.IO.Directory.GetFiles(pathString, tab.SelectedTab.Name + "_" + amx_btn.Name + "_btm_off.*")[0].ToString()).ToString();

                if (File.Exists(pathToOff))
                {
                    btnOffStGrid.Rows[5].Cells[1].Value = "images\\" + Path.GetFileName(pathToOff);
                }
            
            
            }
            else btnOffStGrid.Rows[5].Cells[1].Value = "none";

        }

        private void writeLvlInfo(AMX_Level amx_lvl)       
        {
            //******General******


           
            LvlGenGrid.Rows[0].Cells[1].Value = amx_lvl.Name.ToString();
            LvlGenGrid.Rows[1].Cells[1].Value = amx_lvl.Left.ToString();
            LvlGenGrid.Rows[2].Cells[1].Value = amx_lvl.Top.ToString();
            LvlGenGrid.Rows[3].Cells[1].Value = amx_lvl.Height.ToString();
            LvlGenGrid.Rows[4].Cells[1].Value = amx_lvl.Width.ToString();
            LvlGenGrid.Rows[5].Cells[1].Value = amx_lvl.Minimum.ToString();
            LvlGenGrid.Rows[6].Cells[1].Value = amx_lvl.Maximum.ToString();
            LvlGenGrid.Rows[7].Cells[1].Value = amx_lvl.Level_Value.ToString();

            if (amx_lvl.Orientation == AMX_Level.T_Orientation.Horizontal) LvlGenGrid.Rows[8].Cells[1].Value = "horizontal";
            else LvlGenGrid.Rows[8].Cells[1].Value = "vertical";

            LvlGenGrid.Rows[9].Cells[1].Value = amx_lvl.TabIndex.ToString();

            //*****Programming******

            if (amx_lvl.Level_Function == AMX_Level.T_LevelFunction.Active) LvlProgGrid.Rows[0].Cells[1].Value = "active";
            else LvlProgGrid.Rows[0].Cells[1].Value = "display";

            LvlProgGrid.Rows[1].Cells[1].Value = amx_lvl.Level_Port.ToString();
            LvlProgGrid.Rows[2].Cells[1].Value = amx_lvl.Level_Code.ToString();
            LvlProgGrid.Rows[3].Cells[1].Value = amx_lvl.Address_Port.ToString();
            LvlProgGrid.Rows[4].Cells[1].Value = amx_lvl.Address_Code.ToString();

            

            //*****states***********          
 

            string textTowriteAboutLvlFont = "";

            if (amx_lvl.Font.Name != "") textTowriteAboutLvlFont += amx_lvl.Font.Name + ", " + amx_lvl.Font.Size;
            if (amx_lvl.Font.Bold) textTowriteAboutLvlFont += ", b";
            if (amx_lvl.Font.Italic) textTowriteAboutLvlFont += ", i";
            if (amx_lvl.Font.Underline) textTowriteAboutLvlFont += ", u";


            if (amx_lvl.PictureBoxOff != null) LvlStGrid.Rows[0].Cells[1].Value = "Use";
            else LvlStGrid.Rows[0].Cells[1].Value = "none";
            if (amx_lvl.PictureBoxOn != null) LvlStGrid.Rows[1].Cells[1].Value = "Use";
            else LvlStGrid.Rows[1].Cells[1].Value = "none";

            LvlStGrid.Rows[2].Cells[1].Value =string.Format("{0:X6}", amx_lvl.FillColorOff.ToArgb());
            LvlStGrid.Rows[3].Cells[1].Value = string.Format("{0:X6}", amx_lvl.FillColorOn.ToArgb());
            LvlStGrid.Rows[4].Cells[1].Value = string.Format("{0:X6}", amx_lvl.TextColor.ToArgb());
            LvlStGrid.Rows[5].Cells[1].Value = textTowriteAboutLvlFont;
            LvlStGrid.Rows[6].Cells[1].Value = amx_lvl.TextDisplay.ToString();

        }

        private void hideAllGrids() {
            try
            {
                tabProp.TabPages.Remove(pageProp);
                tabProp.TabPages.Remove(btnGeneral);
                tabProp.TabPages.Remove(btnProgramming);
                tabProp.TabPages.Remove(onStateProp);
                tabProp.TabPages.Remove(offStateProp);
                tabProp.TabPages.Remove(lvlGen);
                tabProp.TabPages.Remove(lvlProg);
                tabProp.TabPages.Remove(StatesLvl);


            }
            catch
            {

            }
        }

        private void showBtnGrids() {

            hideAllGrids();


            if(tabProp.TabPages.Contains(btnGeneral)){}
            else{
            
                tabProp.TabPages.Add(btnGeneral);
                tabProp.TabPages.Add(btnProgramming);
                tabProp.TabPages.Add(onStateProp);
                tabProp.TabPages.Add(offStateProp);
            }
        
        }   
        
        private void showLvlGrids() {

            hideAllGrids();


            if(tabProp.TabPages.Contains(lvlGen)){}
            else{
            
                tabProp.TabPages.Add(lvlGen);
                tabProp.TabPages.Add(lvlProg);
                tabProp.TabPages.Add(StatesLvl);

            }
        
        
        
        }

        private void showPageGrid() {

            hideAllGrids();

            if (tabProp.TabPages.Contains(pageProp)) { }
            else tabProp.TabPages.Add(pageProp);
        }

        private TabPage addPage()
        {
            TabPage myTabPage = new TabPage();            
            myTabPage.BackColor = tab.SelectedTab.BackColor;
            pageNum = 0;
            do{
            pageNum++;
            }
            while(tab.TabPages.ContainsKey("page"+pageNum));

            
            myTabPage.Name = "page" + pageNum;
            myTabPage.Size = new System.Drawing.Size(1024, 768);
            myTabPage.Text = "page" + pageNum;
            consoletext.Text = "Page " + myTabPage.Name + " added";
            return myTabPage;
        }
        
        private void addAmxLevelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            addAmxLevelToPanel();
        }
        private void PageGrid_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (PageGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value != null)
            {
                switch (e.RowIndex)
                {

                    case 0:
                        if (PageGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString() == "plusPage") MessageBox.Show("Please, enter another name of page. The name plusPage is reserved for system");
                        else
                        {
                            tab.SelectedTab.Name = PageGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                            tab.SelectedTab.Text = PageGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                            comboboxRefresh(0);
                            selectItemsinComboItems(tab.SelectedTab.Name);
                        }

                        break;




                }
            }
            else
            {
                MessageBox.Show("Enter data");
            }
        }
        
        private Color setBackgroudColor(Color previous) {
            
            ColorDialog colorDlg = new ColorDialog();

            colorDlg.AllowFullOpen = true;
            colorDlg.AnyColor = true;
            colorDlg.SolidColorOnly = false;
            colorDlg.Color = previous;

            if (colorDlg.ShowDialog() == DialogResult.OK)
            {
                
                return colorDlg.Color;

            }

            return previous;
        
        }

        private AMX_Button returnBtnByName(string name) {

            foreach (Control cnt in tab.SelectedTab.Controls)
            {
                if (cnt.ToString() == "AMX_Controls.AMX_Button")
                {
                    amxBtn = cnt as AMX_Button;
                    
                    if (amxBtn.Name.ToString() == name)
                    {
                        return amxBtn;
                    }
                }


            }

            return amxBtn;
        
        }

        private void btnGenGrid_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (btnGenGrid.Rows[e.RowIndex].Cells[1].Value != null)
            {
                amxBtn = returnBtnByName(comboItems.Text);
                switch (e.RowIndex)
                {

                    case 0:
                        amxBtn.Name = btnGenGrid.Rows[0].Cells[1].Value.ToString();
                        comboboxRefresh(0);
                        selectItemsinComboItems(amxBtn.Name);
                        break;
                    case 1:
                        amxBtn.Left = int.Parse(btnGenGrid.Rows[1].Cells[1].Value.ToString());
                        break;
                    case 2:
                        amxBtn.Top = int.Parse(btnGenGrid.Rows[2].Cells[1].Value.ToString());
                        break;
                    case 3:
                        amxBtn.Height = int.Parse(btnGenGrid.Rows[3].Cells[1].Value.ToString());
                        break;
                    case 4:
                        amxBtn.Width = int.Parse(btnGenGrid.Rows[4].Cells[1].Value.ToString());
                        break;
                    case 5:
                        if (btnGenGrid.Rows[5].Cells[1].Value.ToString() == "ON") amxBtn.Pushed = true;
                        if (btnGenGrid.Rows[5].Cells[1].Value.ToString() == "OFF") amxBtn.Pushed = false;
                        break;
                    case 6:
                        amxBtn.TabIndex = int.Parse(btnGenGrid.Rows[6].Cells[1].Value.ToString());
                        sortItems();
                        break;

                }
            }

            else {
                MessageBox.Show("Enter data");
            }


        }

        private void btnProgGrid_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            
                amxBtn = returnBtnByName(comboItems.Text);
                if (btnProgGrid.Rows[0].Cells[1].Value.ToString() == "channel") amxBtn.Feedback = AMX_Controls.Feedback.channel;
                if (btnProgGrid.Rows[0].Cells[1].Value.ToString() == "channel") amxBtn.Feedback = AMX_Controls.Feedback.momentary;
                if (btnProgGrid.Rows[0].Cells[1].Value.ToString() == "none") amxBtn.Feedback = AMX_Controls.Feedback.none;

                amxBtn.Address_Port = int.Parse(btnProgGrid.Rows[1].Cells[1].Value.ToString());
                amxBtn.Address_Code = int.Parse(btnProgGrid.Rows[2].Cells[1].Value.ToString());
                amxBtn.Channel_Port = int.Parse(btnProgGrid.Rows[3].Cells[1].Value.ToString());
                amxBtn.Channel_Code = int.Parse(btnProgGrid.Rows[4].Cells[1].Value.ToString());
                amxBtn.String_Output_Port = int.Parse(btnProgGrid.Rows[5].Cells[1].Value.ToString());

                if (btnProgGrid.Rows[6].Cells[1].Value != null) amxBtn.String_Output = btnProgGrid.Rows[6].Cells[1].Value.ToString();
                else amxBtn.String_Output = "";

                amxBtn.Command_Output_Port = int.Parse(btnProgGrid.Rows[7].Cells[1].Value.ToString());

                if (btnProgGrid.Rows[8].Cells[1].Value != null) amxBtn.Command_Output = btnProgGrid.Rows[8].Cells[1].Value.ToString();
                else amxBtn.Command_Output = "";
            
            

        }

        private void btnOnStGrid_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            
                amxBtn = returnBtnByName(comboItems.Text);
                switch (e.RowIndex)
                {

                    case 0:
                        if (btnOnStGrid.Rows[0].Cells[1].Value.ToString() == "Use")
                        {
                            try
                            {

                                if (File.Exists(Path.GetFullPath(System.IO.Directory.GetFiles(pathString, tab.SelectedTab.Name + "_picture_on.*")[0].ToString()).ToString()))
                                {
                                    PictureBox pcb_on = new PictureBox();
                                    pcb_on.Image = (Image)new Bitmap(Path.GetFullPath(System.IO.Directory.GetFiles(pathString, tab.SelectedTab.Name + "_picture_on.*")[0].ToString()).ToString());
                                    amxBtn.PictureBoxOn = pcb_on;

                                }
                            }
                            catch { }
                            //Поставить пикчебокс страницы
                        }
                        else amxBtn.PictureBoxOn = null;
                        break;

                    case 1:
                        break;
                    case 2:
                        break;
                    case 3:
                        break;
                    case 4:
                        if (btnOnStGrid.Rows[4].Cells[1].Value != null) amxBtn.TextOn = btnOnStGrid.Rows[4].Cells[1].Value.ToString();
                        else amxBtn.TextOn = "";
                        break;
                    case 5:

                        //pathString + "\\" + tab.SelectedTab.Name + "_"+amxBtn.Name+"_btm_on" + Path.GetExtension(ofd.FileName);
                        string onbtm = tab.SelectedTab.Name + "_" + amxBtn.Name + "_btm_on.*";
                        try
                        {
                            if (File.Exists(Path.GetFullPath(System.IO.Directory.GetFiles(pathString, onbtm)[0].ToString()).ToString()))
                            {
                                btnOnStGrid.Rows[5].Cells[1].Value = Path.GetFullPath(System.IO.Directory.GetFiles(pathString, onbtm)[0].ToString()).ToString();
                            }
                        }
                        catch { }

                        break;

                }

            
            
        }

        private void btnOffStGrid_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            
                amxBtn = returnBtnByName(comboItems.Text);
                switch (e.RowIndex)
                {

                    case 0:
                        if (btnOffStGrid.Rows[0].Cells[1].Value.ToString() == "Use")
                        {
                            try
                            {

                                if (File.Exists(Path.GetFullPath(System.IO.Directory.GetFiles(pathString, tab.SelectedTab.Name + "_picture_off.*")[0].ToString()).ToString()))
                                {
                                    PictureBox pcb_off = new PictureBox();
                                    pcb_off.Image = (Image)new Bitmap(Path.GetFullPath(System.IO.Directory.GetFiles(pathString, tab.SelectedTab.Name + "_picture_off.*")[0].ToString()).ToString());
                                    amxBtn.PictureBoxOff = pcb_off;

                                }
                            }
                            catch { }

                        }

                        else amxBtn.PictureBoxOff = null;
                        break;


                    case 4:
                        if (btnOffStGrid.Rows[4].Cells[1].Value != null) amxBtn.TextOff = btnOffStGrid.Rows[4].Cells[1].Value.ToString();
                        else amxBtn.TextOff = "";
                        break;
                    case 5:
                        string offbtm = tab.SelectedTab.Name + "_" + amxBtn.Name + "_btm_off.*";
                        try
                        {
                            if (File.Exists(Path.GetFullPath(System.IO.Directory.GetFiles(pathString, offbtm)[0].ToString()).ToString()))
                            {
                                btnOffStGrid.Rows[5].Cells[1].Value = Path.GetFullPath(System.IO.Directory.GetFiles(pathString, offbtm)[0].ToString()).ToString();
                            }
                        }
                        catch { }

                        break;

                }

            

        }

        private void btnOnStGrid_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            
                amxBtn = returnBtnByName(comboItems.Text);
                switch (e.RowIndex)
                {

                    case 1:
                        amxBtn.FillColorOn = setBackgroudColor(amxBtn.FillColorOn);
                        btnOnStGrid.Rows[1].Cells[1].Value = amxBtn.FillColorOn.ToString();
                        break;
                    case 2:
                        amxBtn.TextColorOn = setBackgroudColor(amxBtn.TextColorOn);
                        btnOnStGrid.Rows[2].Cells[1].Value = amxBtn.TextColorOn.ToString();
                        break;
                    case 0:
                        if (btnOnStGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString() == "Use") { }

                        else
                        {
                            amxBtn.PictureBoxOn = null;
                        }
                        break;
                    case 3:

                        amxBtn.FontOn = chooseFont(amxBtn.FontOn);
                        btnOnStGrid.Rows[3].Cells[1].Value = fontDescribe(amxBtn.FontOn);


                        break;
                    case 5:

                        OpenFileDialog ofd = new OpenFileDialog();
                        if (ofd.ShowDialog() == DialogResult.OK)
                        {

                            destFile = pathString + "\\" + tab.SelectedTab.Name + "_" + amxBtn.Name + "_btm_on" + Path.GetExtension(ofd.FileName);

                            try
                            {
                                amxBtn.BitmapOn = null;
                                File.Copy(ofd.FileName, destFile, true);
                                amxBtn.BitmapOn = new Bitmap(destFile);
                            }
                            catch { }

                        }


                        break;
                }
           
        }

        private void btnOffStGrid_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {

            amxBtn = returnBtnByName(comboItems.Text);
            switch (e.RowIndex)
            {

                case 1:
                    amxBtn.FillColorOff = setBackgroudColor(amxBtn.FillColorOff);
                    btnOffStGrid.Rows[1].Cells[1].Value = amxBtn.FillColorOff.ToString();
                    break;
                case 2:
                    amxBtn.TextColorOff = setBackgroudColor(amxBtn.TextColorOff);
                    btnOffStGrid.Rows[2].Cells[1].Value = amxBtn.TextColorOff.ToString();
                    break;
                case 0:
                    if (btnOffStGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString() == "Use") { }

                    else
                    {
                        amxBtn.PictureBoxOff = null;
                    }
                    break;
                case 3:
                        amxBtn.FontOff = chooseFont(amxBtn.FontOff);
                        btnOffStGrid.Rows[3].Cells[1].Value = fontDescribe(amxBtn.FontOff);

                    
                    break;
                case 5:

                    OpenFileDialog ofd = new OpenFileDialog();
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {

                        destFile = pathString + "\\" + tab.SelectedTab.Name + "_" + amxBtn.Name + "_btm_off" + Path.GetExtension(ofd.FileName);

                        try
                        {
                            amxBtn.BitmapOff = null;
                            File.Copy(ofd.FileName, destFile, true);
                            amxBtn.BitmapOff = new Bitmap(destFile);
                        }
                        catch { }
                    }

                    break;
                    
            }
            
                      
        }

        private void LvlGenGrid_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (LvlGenGrid.Rows[e.RowIndex].Cells[1].Value != null)
            {

                switch (e.RowIndex)
                {

                    case 0:
                        amxLvl.Name = LvlGenGrid.Rows[0].Cells[1].Value.ToString();
                        comboboxRefresh(0);
                        selectItemsinComboItems(amxLvl.Name);

                        break;
                    case 1:
                        amxLvl.Location = new Point(int.Parse(LvlGenGrid.Rows[1].Cells[1].Value.ToString()), int.Parse(LvlGenGrid.Rows[2].Cells[1].Value.ToString()));
                        break;
                    case 2:
                        amxLvl.Location = new Point(int.Parse(LvlGenGrid.Rows[1].Cells[1].Value.ToString()), int.Parse(LvlGenGrid.Rows[2].Cells[1].Value.ToString()));
                        break;
                    case 3:
                        amxLvl.Size = new Size(int.Parse(LvlGenGrid.Rows[4].Cells[1].Value.ToString()), int.Parse(LvlGenGrid.Rows[3].Cells[1].Value.ToString()));
                        break;
                    case 4:
                        amxLvl.Size = new Size(int.Parse(LvlGenGrid.Rows[4].Cells[1].Value.ToString()), int.Parse(LvlGenGrid.Rows[3].Cells[1].Value.ToString()));
                        break;
                    case 5:
                        amxLvl.Minimum = int.Parse(LvlGenGrid.Rows[5].Cells[1].Value.ToString());
                        break;
                    case 6:
                        amxLvl.Maximum = int.Parse(LvlGenGrid.Rows[6].Cells[1].Value.ToString());
                        break;
                    case 7:
                        amxLvl.Level_Value = int.Parse(LvlGenGrid.Rows[7].Cells[1].Value.ToString());
                        break;
                    case 8:
                        if (LvlGenGrid.Rows[8].Cells[1].Value.ToString() == "horizontal") amxLvl.Orientation = AMX_Level.T_Orientation.Horizontal;
                        else amxLvl.Orientation = AMX_Level.T_Orientation.Vertical;
                        break;
                    case 9:
                        amx_lvl.TabIndex = int.Parse(LvlGenGrid.Rows[9].Cells[1].Value.ToString());
                        sortItems();
                        break;
                }
            }
            else { MessageBox.Show("Enter data"); }

        }

        private void LvlProgGrid_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (LvlProgGrid.Rows[e.RowIndex].Cells[1].Value != null)
            {
            switch (e.RowIndex) { 
            
                case 0:
                    if (LvlProgGrid.Rows[0].Cells[1].Value.ToString() == "active") amxLvl.Level_Function = AMX_Level.T_LevelFunction.Active;
                    else amxLvl.Level_Function = AMX_Level.T_LevelFunction.Display;
                    break;
                case 1:
                    amxLvl.Level_Port = int.Parse(LvlProgGrid.Rows[1].Cells[1].Value.ToString());
                    break;
                case 2:
                    amxLvl.Level_Code= int.Parse(LvlProgGrid.Rows[2].Cells[1].Value.ToString());
                    break;
                case 3:
                    amxLvl.Address_Port = int.Parse(LvlProgGrid.Rows[3].Cells[1].Value.ToString());
                    break;
                case 4:
                    amxLvl.Address_Code = int.Parse(LvlProgGrid.Rows[4].Cells[1].Value.ToString());
                    break;
            }


        }
         else { MessageBox.Show("Enter data"); }
        }      

        private void LvlStGrid_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {

            switch (e.RowIndex) { 
                
                case 2:
                    amxLvl.FillColorOff = setBackgroudColor(amxLvl.FillColorOff);
                    LvlStGrid.Rows[2].Cells[1].Value = amxLvl.FillColorOff.ToString();
                    break;
                case 3:
                    amxLvl.FillColorOn = setBackgroudColor(amxLvl.FillColorOn);
                    LvlStGrid.Rows[3].Cells[1].Value = amxLvl.FillColorOn.ToString();
                    break;
                case 4:
                    amxLvl.TextColor = setBackgroudColor(amxLvl.TextColor);
                    LvlStGrid.Rows[4].Cells[1].Value = amxLvl.TextColor.ToString();
                    break;
                case 5:

                    amxLvl.Font = chooseFont(amxLvl.Font);
                    LvlStGrid.Rows[5].Cells[1].Value = fontDescribe(amxLvl.Font);

                   
                    break;

                case 6:

                    break;
            
            
            }

        }

        private void LvlStGrid_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            switch (e.RowIndex) { 

                case 0:
                     if (LvlStGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString() == "Use") { 
                         //off
                         try
                         {
                             if (File.Exists(Path.GetFullPath(System.IO.Directory.GetFiles(pathString, tab.SelectedTab.Name + "_picture_off.*")[0].ToString()).ToString()))
                             {
                                 PictureBox pcb_off = new PictureBox();
                                 pcb_off.Image = (Image)new Bitmap(Path.GetFullPath(System.IO.Directory.GetFiles(pathString, tab.SelectedTab.Name + "_picture_off.*")[0].ToString()).ToString());
                                 amxLvl.PictureBoxOff = pcb_off;
                             }
                         }
                         catch { }

                     }

                    else
                    {
                        amxLvl.PictureBoxOff = null;
                    }
                    break;
                case 1:
                    if (LvlStGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString() == "Use")
                    {
                        try
                        {
                            if (File.Exists(Path.GetFullPath(System.IO.Directory.GetFiles(pathString, tab.SelectedTab.Name + "_picture_on.*")[0].ToString()).ToString()))
                            {
                                PictureBox pcb_on = new PictureBox();
                                pcb_on.Image = (Image)new Bitmap(Path.GetFullPath(System.IO.Directory.GetFiles(pathString, tab.SelectedTab.Name + "_picture_on.*")[0].ToString()).ToString());
                                amxLvl.PictureBoxOn = pcb_on;
                            }
                        }
                        catch { }
                    }

                    else
                    {
                        amxLvl.PictureBoxOn = null;
                    }
                    break;
                case 6:
                    if(LvlStGrid.Rows[6].Cells[1].Value!=null) amxLvl.TextDisplay = LvlStGrid.Rows[6].Cells[1].Value.ToString();
                    else amxLvl.TextDisplay = "";
                    break;
            }
        }

        private Font chooseFont(Font fnt) {

            FontDialog fontDlg = new FontDialog();
            fontDlg.Font = fnt;

            if (fontDlg.ShowDialog() == DialogResult.OK)
            {

                fnt = fontDlg.Font;
            }
            return fnt;
        }
        private string fontDescribe(Font fnt) {
            string textToWrite="";

            if (fnt.Name != "") textToWrite += fnt.Name + ", " + fnt.Size;
            if (fnt.Bold) textToWrite += ", b";
            if (fnt.Italic) textToWrite += ", i";
            if (fnt.Underline) textToWrite += ", u";

            return textToWrite;
        }

        private void tab_Selecting(object sender, TabControlCancelEventArgs e)
        {
            hideDDs();
            if (tab.TabPages.Count > 0)
            {
                switch (e.TabPage.Name.ToString())
                {
                    case "plusPage":
                       
                        tab.TabPages.Remove(plusPage);
                        TabPage page = addPage();
                        tab.TabPages.Add(page);
                        tab.TabPages.Add(plusPage);
                        tab.SelectTab(page);
                        break;

                }
                setActivePanel();
                comboboxRefresh(0);
            }
            
        }

        private void placeStrips() {

            /*FileStrip.Parent=
            EditStrip.Location = new Point(94, 0);
            PanelStrip.Location = new Point(159, 0);
            PageStrip.Location = new Point(252, 0);
            ButtonStrip.Location = new Point(365, 0);
            LayoutStrip.Location = new Point(426, 0);*/

            if (File.Exists("strips.xml"))
            {
                
                    XmlDocument document = new XmlDocument();
                    document.Load("strips.xml");
                    XmlNodeList strips = document.GetElementsByTagName("strips");
                    foreach (XmlNode strip in strips[0].ChildNodes)
                    {

                        switch (strip.Attributes[0].Value)
                        {

                            case "FileStrip":
                                FileStrip.Location = new Point(int.Parse(strip.Attributes[1].Value), int.Parse(strip.Attributes[2].Value));
                                break;
                            case "EditStrip":
                                EditStrip.Location = new Point(int.Parse(strip.Attributes[1].Value), int.Parse(strip.Attributes[2].Value));
                                break;
                            case "PanelStrip":
                                PanelStrip.Location = new Point(int.Parse(strip.Attributes[1].Value), int.Parse(strip.Attributes[2].Value));
                                break;
                            case "PageStrip":
                                PageStrip.Location = new Point(int.Parse(strip.Attributes[1].Value), int.Parse(strip.Attributes[2].Value));
                                break;
                            case "ButtonStrip":
                                ButtonStrip.Location = new Point(int.Parse(strip.Attributes[1].Value), int.Parse(strip.Attributes[2].Value));
                                break;
                            case "LayoutStrip":
                                LayoutStrip.Location = new Point(int.Parse(strip.Attributes[1].Value), int.Parse(strip.Attributes[2].Value));
                                break;
                            case "flagsStrip":
                                flagsStrip.Location = new Point(int.Parse(strip.Attributes[1].Value), int.Parse(strip.Attributes[2].Value));
                                break;

                            case "stripLoading":
                                if (strip.Attributes[1].Value.ToLower() == "true") stripLoading = true;
                                else stripLoading = false;
                                break;
                            case "aimDraw":
                                if (strip.Attributes[1].Value.ToLower() == "true") aimDraw = true;
                                else aimDraw = false;
                                break;
                            case "dragButton":
                                if (strip.Attributes[1].Value.ToLower() == "true") dragButton = true;
                                else dragButton = false;
                                break;
                            case "autosave":
                                if (strip.Attributes[1].Value.ToLower() == "true") autosave = true;
                                else autosave = false;
                                break;
                            case "autosavetime":
                                if (strip.Attributes[1].Value.ToLower() != "") autosaveTimer = int.Parse(strip.Attributes[1].Value);
                                else autosaveTimer=0;
                                break;


                                
                        }



                    
                }
            }
            else {
                FileStrip.Location = new Point(3, 0);
                EditStrip.Location = new Point(94, 0);
                PanelStrip.Location = new Point(159, 0);
                PageStrip.Location = new Point(252, 0);
                ButtonStrip.Location = new Point(365, 0);
                LayoutStrip.Location = new Point(426, 0);
                flagsStrip.Location = new Point(520, 0);
            }
        
        }

        private void removePageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            removeCurrentPage();
        }

        private void removeCurrentPage() {

          DialogResult msbx=MessageBox.Show("Delete "+tab.SelectedTab.Name.ToString()+"?","Delete page",MessageBoxButtons.YesNo);

          if (msbx == DialogResult.Yes)
          {
              if (tab.TabCount > 1)
              {
                  consoletext.Text = "Page " + tab.SelectedTab.Name + " removed";
                  int index = tab.SelectedIndex;
                  tab.TabPages.Remove(tab.SelectedTab);
                  tab.SelectTab(index - 1);

              }
          }
        }

        private void setBackgroundColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tab.SelectedTab.BackColor = setBackgroudColor(tab.SelectedTab.BackColor);
        }

        private void deleteCurrentButtonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            deleteCurrentItem(comboItems.Text);
            comboboxRefresh(0);
        }

        private void deleteCurrentItem(string name) 
        {
            if (name != tab.SelectedTab.Name)
            {

                if (MessageBox.Show("Really delete " + name + "?", "Delete Item", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {

                    foreach (Control cnt in tab.SelectedTab.Controls)
                    {

                        if (cnt.ToString() == "AMX_Controls.AMX_Button")
                        {
                            AMX_Button abs = cnt as AMX_Button;
                            if (abs.Name.ToString() == name)
                            {
                                consoletext.Text = abs.Name+" removed";
                                abs.Dispose();
                                tab.SelectedTab.Refresh();
                            }
                        }

                        if (cnt.ToString() == "AMX_Controls.AMX_Level")
                        {
                            AMX_Level alx = cnt as AMX_Level;
                            if (alx.Name.ToString() == name)
                            {
                                consoletext.Text = alx.Name+" removed";
                                alx.Dispose();
                                tab.SelectedTab.Refresh();
                            }
                        }

                    }
                }
            }
        }

        private void comboboxRefresh(int index) {

            comboItems.Items.Clear();
            comboItems.Items.Add(new SelectData(0,tab.SelectedTab.Name.ToString()));

            comboItems.SelectedIndex = 0;

            foreach (Control cnt in tab.SelectedTab.Controls)
            {

                if (cnt.ToString() == "AMX_Controls.AMX_Button")
                {
                    AMX_Button abs = cnt as AMX_Button;
                    comboItems.Items.Add(new SelectData(abs.TabIndex, abs.Name.ToString()));
                }

                if (cnt.ToString() == "AMX_Controls.AMX_Level")
                {
                    AMX_Level alx = cnt as AMX_Level;
                    comboItems.Items.Add(new SelectData(alx.TabIndex, alx.Name.ToString()));
                    
                }

            }

            //selectItemsinComboItems(comboItems.Items.IndexOf(index).ToString());
        }

        private void comboItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Просканировать все элементы формы и найти с выбранным именем

            string name = comboItems.SelectedItem.ToString();
            if (tab.SelectedTab.Name.ToString()==name){
                unfocusAll();
                showPageGrid();
                writeInfoAbputCurrentPage();
            }
            foreach (Control cnt in tab.SelectedTab.Controls)
            {

                if (cnt.ToString() == "AMX_Controls.AMX_Button")
                {
                    AMX_Button abs = cnt as AMX_Button;
                    if (abs.Name.ToString() == name)
                    {
                        abs.Focus();
                        tab.SelectedTab.Refresh();
                    }
                }

                if (cnt.ToString() == "AMX_Controls.AMX_Level")
                {
                    AMX_Level alx = cnt as AMX_Level;
                    if (alx.Name.ToString() == name)
                    {
                        amxLvl = (AMX_Level)alx;
                        amxLvl.Focus();
                        //alx.Focus();
                        tab.SelectedTab.Refresh();
                    }
                }

            }
        }

        private void selectItemsinComboItems(string name) {

           // comboItems.SelectedValue = comboItems.Items.IndexOf(name);
            comboItems.Text = name;

        }

        private void writeInfoAbputCurrentPage() {

            PageGrid.Rows[0].Cells[1].Value = tab.SelectedTab.Name.ToString();
            string offPcb = "",onPcb="";
            onPcb = tab.SelectedTab.Name + "_picture_on.*";
            offPcb = tab.SelectedTab.Name + "_picture_off.*";
            try
            {
                if (File.Exists(Path.GetFullPath(System.IO.Directory.GetFiles(pathString, offPcb)[0].ToString()).ToString()))
                {
                    PageGrid.Rows[1].Cells[1].Value = Path.GetFullPath(System.IO.Directory.GetFiles(pathString, offPcb)[0].ToString()).ToString();
                }
                else {
                    PageGrid.Rows[1].Cells[1].Value = null;
                }
            }
            catch{
                
            }
            try
            {
                if (File.Exists(Path.GetFullPath(System.IO.Directory.GetFiles(pathString, onPcb)[0].ToString()).ToString()))
                {
                    PageGrid.Rows[2].Cells[1].Value = Path.GetFullPath(System.IO.Directory.GetFiles(pathString, onPcb)[0].ToString()).ToString();
                }
                else
                {
                    PageGrid.Rows[2].Cells[1].Value = null;
                }

            }
            catch {
                
            }
            
            PageGrid.Rows[3].Cells[1].Value = string.Format("{0:X6}", tab.SelectedTab.BackColor);
        }
       
        private void testToolStripMenuItem_Click(object sender, EventArgs e)
        {
            goBackForOneStep();
        }

        private void createXmlFile()
        {

            if (pathString != "")
            {
                xmlFileName = pathString + "\\XML.xml";
                XmlTextWriter textWritter = new XmlTextWriter(xmlFileName, Encoding.UTF8);
                textWritter.WriteStartDocument();
                textWritter.WriteStartElement("pages");
                textWritter.WriteEndElement();
                textWritter.Close();
            }

            writeToXml();
            progress.Value = 0;
        }

        private void writeToXml()
        {
            progress.Value = 0;
            progress.Step= progress.Width/(tab.TabCount - 1);
            XmlDocument document = new XmlDocument();
            if (xmlFileName != "")
            {
                document.Load(xmlFileName);


                foreach (TabPage tabPage in tab.TabPages)
                {
                    if (tabPage.Name != "plusPage")
                    {
                        progress.PerformStep();
                        XmlNode page = document.CreateElement("page");
                        document.DocumentElement.AppendChild(page);

                        XmlAttribute pagesWidth = document.CreateAttribute("width");
                        pagesWidth.Value = tabPage.Width.ToString();
                        document.DocumentElement.Attributes.Append(pagesWidth);

                        XmlAttribute pagesHeight = document.CreateAttribute("height");
                        pagesHeight.Value = tabPage.Height.ToString();
                        document.DocumentElement.Attributes.Append(pagesHeight);


                        XmlAttribute pageName = document.CreateAttribute("name");
                        pageName.Value = tabPage.Name.ToString();
                        page.Attributes.Append(pageName);

                        XmlAttribute pictureboxOff = document.CreateAttribute("pictureboxoff");
                        XmlAttribute pictureboxOn = document.CreateAttribute("pictureboxon");
                        string offPcb = "",onPcb="";
                        onPcb = tabPage.Name + "_picture_on.*";
                        offPcb = tabPage.Name + "_picture_off.*";

                                  
                        try
                        {
                            if (File.Exists(Path.GetFullPath(System.IO.Directory.GetFiles(pathString, offPcb)[0].ToString()).ToString()))
                            {
                                pictureboxOff.Value =Path.GetFileName(Path.GetFullPath(System.IO.Directory.GetFiles(pathString, offPcb)[0].ToString()).ToString());
                            }
                            else {
                                pictureboxOff.Value = null;
                            }
                        }

                        catch {
                
                        }

                        try
                        {
                        if (File.Exists(Path.GetFullPath(System.IO.Directory.GetFiles(pathString, onPcb)[0].ToString()).ToString()))
                            {
                                pictureboxOn.Value =Path.GetFileName(Path.GetFullPath(System.IO.Directory.GetFiles(pathString, onPcb)[0].ToString()).ToString());
                            }
                            else
                            {
                                pictureboxOn.Value = null;
                            }

                        }
                    
                        catch {
                
                        }

                        page.Attributes.Append(pictureboxOff);
                        page.Attributes.Append(pictureboxOn);


                        XmlAttribute fillColor = document.CreateAttribute("fillcolor");
                        fillColor.InnerText = string.Format("{0:X6}", tabPage.BackColor.ToArgb());
                        page.Attributes.Append(fillColor);



                        //--------------------------------------------------------------------------

                        foreach (Control cnt in tabPage.Controls)
                        {
                            if (cnt.ToString() == "AMX_Controls.AMX_Button")
                            {
                                AMX_Button ab = cnt as AMX_Button;
                                XmlNode button = document.CreateElement("button");
                                //-------------General------------------------------

                                XmlNode buttonName = document.CreateElement("name");
                                buttonName.InnerText = ab.Name.ToString();
                                button.AppendChild(buttonName);

                                XmlNode buttonLeft = document.CreateElement("left");
                                buttonLeft.InnerText = ab.Left.ToString();
                                button.AppendChild(buttonLeft);

                                XmlNode buttonTop = document.CreateElement("top");
                                buttonTop.InnerText = ab.Top.ToString();
                                button.AppendChild(buttonTop);

                                XmlNode buttonHeight = document.CreateElement("height");
                                buttonHeight.InnerText = ab.Height.ToString();
                                button.AppendChild(buttonHeight);

                                XmlNode buttonWidth = document.CreateElement("width");
                                buttonWidth.InnerText = ab.Width.ToString();
                                button.AppendChild(buttonWidth);

                                XmlNode buttonState = document.CreateElement("state");
                                if (ab.Pushed) buttonState.InnerText = "ON";
                                else buttonState.InnerText = "OFF";
                                button.AppendChild(buttonState);

                                XmlNode buttonZindex = document.CreateElement("zindex");
                                buttonZindex.InnerText = ab.TabIndex.ToString();
                                button.AppendChild(buttonZindex);

                                

                                //---------Programming----------------------

                                XmlNode buttonFeedback = document.CreateElement("feedback");
                                buttonFeedback.InnerText = ab.Feedback.ToString();
                                button.AppendChild(buttonFeedback);

                                XmlNode buttonAddressPort = document.CreateElement("address_port");
                                buttonAddressPort.InnerText = ab.Address_Port.ToString();
                                button.AppendChild(buttonAddressPort);

                                XmlNode buttonAddressCode = document.CreateElement("address_code");
                                buttonAddressCode.InnerText = ab.Address_Code.ToString();
                                button.AppendChild(buttonAddressCode);

                                XmlNode buttonChannelPort = document.CreateElement("channel_port");
                                buttonChannelPort.InnerText = ab.Channel_Port.ToString();
                                button.AppendChild(buttonChannelPort);

                                XmlNode buttonChannelCode = document.CreateElement("channel_code");
                                buttonChannelCode.InnerText = ab.Channel_Code.ToString();
                                button.AppendChild(buttonChannelCode);

                                XmlNode buttonStringOutputPort = document.CreateElement("string_output_port");
                                buttonStringOutputPort.InnerText = ab.String_Output_Port.ToString();
                                button.AppendChild(buttonStringOutputPort);

                                XmlNode buttonStringOutput = document.CreateElement("string_output");
                                buttonStringOutput.InnerText = ab.String_Output.ToString();
                                button.AppendChild(buttonStringOutput);

                                XmlNode buttonCommandOutputPort = document.CreateElement("command_output_port");
                                buttonCommandOutputPort.InnerText = ab.Command_Output_Port.ToString();
                                button.AppendChild(buttonCommandOutputPort);

                                XmlNode buttonCommandOutput = document.CreateElement("command_output");
                                buttonCommandOutput.InnerText = ab.Command_Output.ToString();
                                button.AppendChild(buttonCommandOutput);

                                //------------------offstate------------------------

                                XmlNode buttonPictureboxOff = document.CreateElement("picturebox_off");
                                if (ab.PictureBoxOff != null) buttonPictureboxOff.InnerText = "use";
                                else buttonPictureboxOff.InnerText = "none";
                                button.AppendChild(buttonPictureboxOff);

                                XmlNode buttonBackgroundColorOff = document.CreateElement("background_color_off");
                                buttonBackgroundColorOff.InnerText = string.Format("{0:X6}", ab.FillColorOff.ToArgb());
                                button.AppendChild(buttonBackgroundColorOff);

                                XmlNode buttonTextColorOff = document.CreateElement("text_color_off");
                                buttonTextColorOff.InnerText = string.Format("{0:X6}", ab.TextColorOff.ToArgb());
                                button.AppendChild(buttonTextColorOff);

                                XmlNode buttonFontOffFamily = document.CreateElement("font_off_family");
                                buttonFontOffFamily.InnerText = ab.FontOff.FontFamily.Name.ToString();
                                button.AppendChild(buttonFontOffFamily);

                                XmlNode buttonFontOffSize = document.CreateElement("font_off_size");
                                buttonFontOffSize.InnerText = ab.FontOff.Size.ToString();
                                button.AppendChild(buttonFontOffSize);

                                XmlNode buttonFontOffBold = document.CreateElement("font_off_bold");
                                buttonFontOffBold.InnerText = ab.FontOff.Bold.ToString();
                                button.AppendChild(buttonFontOffBold);

                                XmlNode buttonFontOffItalic = document.CreateElement("font_off_italic");
                                buttonFontOffItalic.InnerText = ab.FontOff.Italic.ToString();
                                button.AppendChild(buttonFontOffItalic);

                                XmlNode buttonFontOffUnderline = document.CreateElement("font_off_underline");
                                buttonFontOffUnderline.InnerText = ab.FontOff.Underline.ToString();
                                button.AppendChild(buttonFontOffUnderline);

                                XmlNode buttonTextOff = document.CreateElement("text_off");
                                buttonTextOff.InnerText = ab.TextOff.ToString();
                                button.AppendChild(buttonTextOff);


                                XmlNode buttonBitmapOff = document.CreateElement("bitmap_off");
                                if (ab.BitmapOff == null) buttonBitmapOff.InnerText = "none";
                                else 
                                {
                                    string pathToOff = "";
                                    
                                    pathToOff=Path.GetFullPath(System.IO.Directory.GetFiles(pathString, tabPage.Name + "_" + ab.Name + "_btm_off.*")[0].ToString()).ToString();

                                    if (File.Exists(pathToOff))
                                    {
                                        buttonBitmapOff.InnerText =Path.GetFileName(pathToOff);
                                    }
                                
                                }
                                button.AppendChild(buttonBitmapOff);

                                //------------onstate-----------------------------

                                XmlNode buttonPictureboxOn = document.CreateElement("picturebox_on");
                                if (ab.PictureBoxOn != null) buttonPictureboxOn.InnerText = "use";
                                else buttonPictureboxOn.InnerText = "none";
                                button.AppendChild(buttonPictureboxOn);

                                XmlNode buttonBackgroundColorOn = document.CreateElement("background_color_on");
                                buttonBackgroundColorOn.InnerText = string.Format("{0:X6}", ab.FillColorOn.ToArgb());
                                button.AppendChild(buttonBackgroundColorOn);

                                XmlNode buttonTextColorOn = document.CreateElement("text_color_on");
                                buttonTextColorOn.InnerText = string.Format("{0:X6}", ab.TextColorOn.ToArgb());
                                button.AppendChild(buttonTextColorOn);

                                XmlNode buttonFontOnFamily = document.CreateElement("font_on_family");
                                buttonFontOnFamily.InnerText = ab.FontOn.FontFamily.Name.ToString();
                                button.AppendChild(buttonFontOnFamily);

                                XmlNode buttonFontOnSize = document.CreateElement("font_on_size");
                                buttonFontOnSize.InnerText = ab.FontOn.Size.ToString();
                                button.AppendChild(buttonFontOnSize);

                                XmlNode buttonFontOnBold = document.CreateElement("font_on_bold");
                                buttonFontOnBold.InnerText = ab.FontOn.Bold.ToString();
                                button.AppendChild(buttonFontOnBold);

                                XmlNode buttonFontOnItalic = document.CreateElement("font_on_italic");
                                buttonFontOnItalic.InnerText = ab.FontOn.Italic.ToString();
                                button.AppendChild(buttonFontOnItalic);

                                XmlNode buttonFontOnUnderline = document.CreateElement("font_on_underline");
                                buttonFontOnUnderline.InnerText = ab.FontOn.Underline.ToString();
                                button.AppendChild(buttonFontOnUnderline);

                                XmlNode buttonTextOn = document.CreateElement("text_on");
                                buttonTextOn.InnerText = ab.Text.ToString();
                                button.AppendChild(buttonTextOn);


                                XmlNode buttonBitmapOn = document.CreateElement("bitmap_on");
                                if (ab.BitmapOn == null) buttonBitmapOn.InnerText = "none";
                                else
                                {
                                    string pathToOn = "";
                                    pathToOn = Path.GetFullPath(System.IO.Directory.GetFiles(pathString, tabPage.Name + "_" + ab.Name + "_btm_on.*")[0].ToString()).ToString();

                                    if (File.Exists(pathToOn))
                                    {
                                        buttonBitmapOn.InnerText =Path.GetFileName(pathToOn);
                                    }


                                }
                                button.AppendChild(buttonBitmapOn);


                                //-------------------------------------------------------------------
                                page.AppendChild(button);

                            }



                            else if (cnt.ToString() == "AMX_Controls.AMX_Level")
                            {
                                AMX_Level al = cnt as AMX_Level;
                                XmlNode level = document.CreateElement("level");
                                //----------------------------------------------------

                                XmlNode levelName = document.CreateElement("name");
                                XmlNode levelLeft = document.CreateElement("left");
                                XmlNode levelTop = document.CreateElement("top");
                                XmlNode levelHeight = document.CreateElement("height");
                                XmlNode levelWidth = document.CreateElement("width");
                                XmlNode levelMinimum = document.CreateElement("minimum");
                                XmlNode levelMaximum = document.CreateElement("maximum");
                                XmlNode levelValue = document.CreateElement("value");
                                XmlNode levelOrientation = document.CreateElement("orientation");
                                XmlNode levelZindex = document.CreateElement("zindex");
                                XmlNode levelFunction = document.CreateElement("function");
                                XmlNode levelLevelPort = document.CreateElement("level_port");
                                XmlNode levelLevelCode = document.CreateElement("level_code");
                                XmlNode levelAddressPort = document.CreateElement("address_port");
                                XmlNode levelAddressCode = document.CreateElement("address_code");
                                XmlNode levelFontFamily = document.CreateElement("font_family");    
                                XmlNode levelFontSize = document.CreateElement("font_size");
                                XmlNode levelFontBold = document.CreateElement("font_bold");
                                XmlNode levelFontItalic = document.CreateElement("font_italic");
                                XmlNode levelFontUnderline = document.CreateElement("font_underline");
                                XmlNode levelText = document.CreateElement("text");
                                XmlNode levelPictureboxOff = document.CreateElement("picturebox_off");
                                XmlNode levelPictureboxOn = document.CreateElement("picturebox_on");
                                XmlNode levelColorOff = document.CreateElement("color_off");
                                XmlNode levelColorOn = document.CreateElement("color_on");
                                XmlNode levelTextColor = document.CreateElement("text_color");

                                //---------------------------------------------------

                                levelName.InnerText = al.Name.ToString();
                                levelLeft.InnerText=al.Left.ToString();
                                levelTop.InnerText=al.Top.ToString();
                                levelHeight.InnerText=al.Height.ToString();
                                levelWidth.InnerText=al.Width.ToString();
                                levelMinimum.InnerText=al.Minimum.ToString();
                                levelMaximum.InnerText=al.Maximum.ToString();
                                levelValue.InnerText=al.Level_Value.ToString();
                                levelOrientation.InnerText = al.Orientation.ToString();
                                levelZindex.InnerText = al.TabIndex.ToString();
                                levelFunction.InnerText=al.Level_Function.ToString();
                                levelLevelPort.InnerText=al.Level_Port.ToString();
                                levelLevelCode.InnerText=al.Level_Code.ToString();
                                levelAddressPort.InnerText=al.Address_Port.ToString();
                                levelAddressCode.InnerText=al.Address_Code.ToString();
                                levelFontFamily.InnerText=al.Font.FontFamily.Name.ToString();
                                levelFontSize.InnerText=al.Font.Size.ToString();
                                levelFontBold.InnerText=al.Font.Bold.ToString();
                                levelFontItalic.InnerText=al.Font.Italic.ToString();
                                levelFontUnderline.InnerText=al.Font.Underline.ToString();
                                levelText.InnerText = al.TextDisplay.ToString();
                                if (al.PictureBoxOff != null) levelPictureboxOff.InnerText = "Use";
                                else levelPictureboxOff.InnerText = "";
                                if (al.PictureBoxOn != null) levelPictureboxOn.InnerText = "Use";
                                else levelPictureboxOn.InnerText = "";
                                levelColorOff.InnerText = string.Format("{0:X6}", al.FillColorOff.ToArgb());
                                levelColorOn.InnerText  = string.Format("{0:X6}", al.FillColorOn.ToArgb());
                                levelTextColor.InnerText = string.Format("{0:X6}", al.TextColor.ToArgb());

                                //---------------------------------------------------


                                level.AppendChild(levelName);
                                level.AppendChild(levelLeft);
                                level.AppendChild(levelTop);
                                level.AppendChild(levelHeight);
                                level.AppendChild(levelWidth);
                                level.AppendChild(levelMinimum);
                                level.AppendChild(levelMaximum);
                                level.AppendChild(levelValue);
                                level.AppendChild(levelOrientation);
                                level.AppendChild(levelZindex);
                                level.AppendChild(levelFunction);
                                level.AppendChild(levelLevelPort);
                                level.AppendChild(levelLevelCode);
                                level.AppendChild(levelAddressPort);
                                level.AppendChild(levelAddressCode);
                                level.AppendChild(levelFontFamily);
                                level.AppendChild(levelFontSize);
                                level.AppendChild(levelFontBold);
                                level.AppendChild(levelFontItalic);
                                level.AppendChild(levelFontUnderline);
                                level.AppendChild(levelText);
                                level.AppendChild(levelPictureboxOff);
                                level.AppendChild(levelPictureboxOn);
                                level.AppendChild(levelColorOff);
                                level.AppendChild(levelColorOn);
                                level.AppendChild(levelTextColor);
                                
                                //-------------------------------------------------


                                page.AppendChild(level);

                            }
                        }

                    }


                    document.Save(xmlFileName);
                    consoletext.Text = "Xml file saved at " + pathString;
                }

            }
        }

        private void sortItems() {

            
            for (int index = 1; index <= 1000; index++)
            {

                foreach (Control cnt in tab.SelectedTab.Controls)
                {
                    if (cnt.ToString() == "AMX_Controls.AMX_Button")
                    {
                        AMX_Button ab = cnt as AMX_Button;
                        if (ab.TabIndex == index)
                        {
                            ab.BringToFront();
                        }

                    }

                    else if (cnt.ToString() == "AMX_Controls.AMX_Level")
                    {
                        AMX_Level al = cnt as AMX_Level;
                        if (al.TabIndex == index)
                        {
                            al.BringToFront();
                        }

                    }
                }
            }
        }

        private void reverseStateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string name = comboItems.Text.ToString();

            if (name != tab.SelectedTab.Name)
            {

               foreach (Control cnt in tab.SelectedTab.Controls)
                    {

                        if (cnt.ToString() == "AMX_Controls.AMX_Button")
                        {
                            AMX_Button abs = cnt as AMX_Button;
                            if (abs.Name.ToString() == name)
                            {
                                amxBtn = abs;
                                amxBtn.Pushed = !amxBtn.Pushed;
 
                                tab.SelectedTab.Refresh();
                            }
                        }

                        if (cnt.ToString() == "AMX_Controls.AMX_Level")
                        {
                            AMX_Level alx = cnt as AMX_Level;
                            if (alx.Name.ToString() == name)
                            {
                                amxLvl = alx;
                                amxLvl.Level_Value = amxLvl.Maximum - amxLvl.Level_Value;
                                tab.SelectedTab.Refresh();
                            }
                        }

                    }
                }
            
        }

        private void sendBackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*
             * для того, что бы переместить элемент на 1 индекс назад
             * нужно сдвинуть все элементы на 1 вперед, кроме него
             * при этом нужно что бы элемент, который его догнал по индексу, 
             * перепрыгнул еще на 1 через него
             * 1) ищем индекс того, что надо оставить
             */
            string name = comboItems.Text.ToString();
            int indexOfFixed = 0; //тут получаем индекс, который надо сдвинуть на 1 назад
            if (name != tab.SelectedTab.Name)
            {

                foreach (Control cnt in tab.SelectedTab.Controls)
                {

                    if (cnt.ToString() == "AMX_Controls.AMX_Button")
                    {
                        AMX_Button abs = cnt as AMX_Button;
                        if (abs.Name.ToString() == name)
                        {
                            amxBtn = abs;
                            
                            if(amxBtn.TabIndex > 1) amxBtn.TabIndex = amxBtn.TabIndex - 1;
                            indexOfFixed = amxBtn.TabIndex;
                            WriteToTextBoxes(amxBtn);
                        }

                    }

                    if (cnt.ToString() == "AMX_Controls.AMX_Level")
                    {
                        AMX_Level alx = cnt as AMX_Level;
                        if (alx.Name.ToString() == name)
                        {
                            amxLvl = alx;
                            
                            if (amxLvl.TabIndex > 1) amxLvl.TabIndex = amxLvl.TabIndex - 1;
                            indexOfFixed = alx.TabIndex;
                            writeLvlInfo(amxLvl);
                        }

                    }

                }

                //indexOfFixed на этом этапе сохраняет индекс элемента, который надо подвинуть вперед
                // если у него имя отличается от того, что уже переместили
                //запускаем такой же цикл, который изменит элемент с этим индексом на 1 больше
                //если имя этого элемента отличается от перемещаемого
                foreach (Control cnt in tab.SelectedTab.Controls)
                {

                    if (cnt.ToString() == "AMX_Controls.AMX_Button")
                    {
                        AMX_Button abs = cnt as AMX_Button;
                        if (abs.Name.ToString() != name)
                        {
                            amxBtn = abs;
                            if(amxBtn.TabIndex==indexOfFixed) amxBtn.TabIndex = amxBtn.TabIndex + 1;
                            
                        }

                    }

                    if (cnt.ToString() == "AMX_Controls.AMX_Level")
                    {
                        AMX_Level alx = cnt as AMX_Level;
                        if (alx.Name.ToString() != name)
                        {
                            amxLvl = alx;

                            if (amxLvl.TabIndex == indexOfFixed) amxLvl.TabIndex = amxLvl.TabIndex + 1;
                            
                        }

                    }

                }

            }

            sortItems();

        }

        private void moveForvardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string name = comboItems.Text.ToString();
            int indexOfFixed = 0;
            if (name != tab.SelectedTab.Name)
            {

                foreach (Control cnt in tab.SelectedTab.Controls)
                {

                    if (cnt.ToString() == "AMX_Controls.AMX_Button")
                    {
                        AMX_Button abs = cnt as AMX_Button;
                        if (abs.Name.ToString() == name)
                        {
                            amxBtn = abs;
                            amxBtn.TabIndex = amxBtn.TabIndex + 1;
                            indexOfFixed = amxBtn.TabIndex;
                            WriteToTextBoxes(amxBtn);
                        }

                    }

                    if (cnt.ToString() == "AMX_Controls.AMX_Level")
                    {
                        AMX_Level alx = cnt as AMX_Level;
                        if (alx.Name.ToString() == name)
                        {
                            amxLvl = alx;
                            amxLvl.TabIndex = amxLvl.TabIndex + 1;
                            indexOfFixed = alx.TabIndex;
                            writeLvlInfo(amxLvl);
                        }

                    }

                }

               
                foreach (Control cnt in tab.SelectedTab.Controls)
                {

                    if (cnt.ToString() == "AMX_Controls.AMX_Button")
                    {
                        AMX_Button abs = cnt as AMX_Button;
                        if (abs.Name.ToString() != name)
                        {
                            amxBtn = abs;
                            if (amxBtn.TabIndex == indexOfFixed) amxBtn.TabIndex = amxBtn.TabIndex - 1;

                        }

                    }

                    if (cnt.ToString() == "AMX_Controls.AMX_Level")
                    {
                        AMX_Level alx = cnt as AMX_Level;
                        if (alx.Name.ToString() != name)
                        {
                            amxLvl = alx;

                            if (amxLvl.TabIndex == indexOfFixed) amxLvl.TabIndex = amxLvl.TabIndex - 1;

                        }

                    }

                }

            }

            sortItems();

        }

        public  DialogResult InputBox(string title, string promptText, ref string value)
        {
            Form form = new Form();
            Label label = new Label();
            TextBox textBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            label.Text = promptText;
            textBox.Text = value;

            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            label.SetBounds(9, 20, 372, 13);
            textBox.SetBounds(12, 36, 372, 20);
            buttonOk.SetBounds(228, 72, 75, 23);
            buttonCancel.SetBounds(309, 72, 75, 23);

            label.AutoSize = true;
            textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(396, 107);
            form.Controls.AddRange(new Control[] { label, textBox, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            value = textBox.Text;
            return dialogResult;
        }

        private void createNewProject() {

            string Path = "";
            

            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                Path = fbd.SelectedPath;
                
                string value = "Project1";
                if (InputBox("Create new project", "Enter project name:", ref value) == DialogResult.OK)
                {             
                pathString = System.IO.Path.Combine(Path, value);

                System.IO.Directory.CreateDirectory(pathString);

               // fs = new FileStream(pathString+"\\log.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
                File.Create(pathString + "\\log.txt",128,FileOptions.Asynchronous);
                consoletext.Text = "Successfully created at: " + pathString;
                activateForm();
                }
               
            }
        }

        private void setOnStatePictureBoxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            deleteOnStatePictureBoxToolStripMenuItem_Click(sender, e);
            setPictureBoxOn();
        }

        private void setOffStatePictureBoxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            deleteOffStatePictureBoxToolStripMenuItem_Click(sender, e);
            setPictureBoxOff();
        }
        private void freeTabResource() {
            if (tab.SelectedTab.BackgroundImage != null)
            {
                tab.SelectedTab.BackgroundImage.Dispose();
                tab.SelectedTab.BackgroundImage = null;
            }
            
        }

        private void setPictureBoxOff(){

            OpenFileDialog ofd = new OpenFileDialog();

            if (ofd.ShowDialog() == DialogResult.OK)
            {

                destFile = pathString + "\\" + tab.SelectedTab.Name + "_picture_off" + Path.GetExtension(ofd.FileName);

                
                try
                {
                    File.Copy(ofd.FileName, destFile, true);
                    tab.SelectedTab.BackgroundImage = new Bitmap(destFile);
                    
                }
                catch {
                    consoletext.Text = "Failed to set pictureBoxOff";       
                }

            }
            writeInfoAbputCurrentPage();


        }

        private void setPictureBoxOn()
        {

            OpenFileDialog ofd = new OpenFileDialog();

            if (ofd.ShowDialog() == DialogResult.OK)
            {

                destFile = pathString + "\\" + tab.SelectedTab.Name + "_picture_on" + Path.GetExtension(ofd.FileName);

                try
                {

                    File.Copy(ofd.FileName, destFile, true);
                    tab.SelectedTab.BackgroundImage = new Bitmap(destFile);
                }
                catch {

                    consoletext.Text = "Failed to set pictureBoxOn";
                }

            }
            writeInfoAbputCurrentPage();
        }

        private void PageGrid_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            switch (e.RowIndex)
            {

                case 1:
                    freeTabResource();
                    setPictureBoxOff();
                    break;

                case 2:
                    freeTabResource();
                    setPictureBoxOn();
                    break;
                case 3:
                    tab.SelectedTab.BackColor = setBackgroudColor(tab.SelectedTab.BackColor);
                    PageGrid.Rows[3].Cells[1].Value = string.Format("{0:X6}", tab.SelectedTab.BackColor);
                    break;

            }
        }

        private void viewPcbOff() {
            try
            {

                if (File.Exists(Path.GetFullPath(System.IO.Directory.GetFiles(pathString, tab.SelectedTab.Name + "_picture_off.*")[0].ToString()).ToString()))
                {
                    Bitmap bm = new Bitmap(Path.GetFullPath(System.IO.Directory.GetFiles(pathString, tab.SelectedTab.Name + "_picture_off.*")[0].ToString()).ToString());
                    tab.SelectedTab.BackgroundImage = bm;

                }
            }
            catch {
                consoletext.Text = "Failed to view pictureBoxOff";
            }
        }
        private void viewPcbOn() {
            try
            {

                if (File.Exists(Path.GetFullPath(System.IO.Directory.GetFiles(pathString, tab.SelectedTab.Name + "_picture_on.*")[0].ToString()).ToString()))
                {
                    Bitmap bm = new Bitmap(Path.GetFullPath(System.IO.Directory.GetFiles(pathString, tab.SelectedTab.Name + "_picture_on.*")[0].ToString()).ToString());
                    tab.SelectedTab.BackgroundImage = bm;

                }
            }
            catch {
                consoletext.Text = "Failed to view pictureBoxOn";
            }
        }

        private void PageGrid_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            if(e.ColumnIndex==0){

                    if (e.RowIndex == 1) {
                        freeTabResource();
                        viewPcbOff();
                    }
                    if (e.RowIndex == 2)
                    {
                        freeTabResource();
                        viewPcbOn();
                    }
                    if (e.RowIndex == 3) {
                        freeTabResource();
                        

                    }

            }

                        
        }

        private void deleteOnStatePictureBoxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            freeTabResource();
            foreach (Control cnt in tab.SelectedTab.Controls)
                    {

                        if (cnt.ToString() == "AMX_Controls.AMX_Button")
                        {
                            AMX_Button abs = cnt as AMX_Button;
                            abs.PictureBoxOn=null;
                        }

                        if (cnt.ToString() == "AMX_Controls.AMX_Level")
                        {
                            AMX_Level alx = cnt as AMX_Level;
                            alx.PictureBoxOn=null;
                        }

                    }
            try
            {

                if (File.Exists(Path.GetFullPath(System.IO.Directory.GetFiles(pathString, tab.SelectedTab.Name + "_picture_on.*")[0].ToString()).ToString()))
                {

                    File.Delete(Path.GetFullPath(System.IO.Directory.GetFiles(pathString, tab.SelectedTab.Name + "_picture_on.*")[0].ToString()).ToString());


                }
            }
            catch {
                consoletext.Text = "Failed to delete";
            }


                
        }

        private void deleteOffStatePictureBoxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            freeTabResource();
            foreach (Control cnt in tab.SelectedTab.Controls)
            {

                if (cnt.ToString() == "AMX_Controls.AMX_Button")
                {
                    AMX_Button abs = cnt as AMX_Button;
                    abs.PictureBoxOff = null;
                }

                if (cnt.ToString() == "AMX_Controls.AMX_Level")
                {
                    AMX_Level alx = cnt as AMX_Level;
                    alx.PictureBoxOff = null;
                }

            }

            try
            {

                if (File.Exists(Path.GetFullPath(System.IO.Directory.GetFiles(pathString, tab.SelectedTab.Name + "_picture_off.*")[0].ToString()).ToString()))
                {
                    
                  File.Delete(Path.GetFullPath(System.IO.Directory.GetFiles(pathString, tab.SelectedTab.Name + "_picture_off.*")[0].ToString()).ToString());


                }
            }
            catch {
                consoletext.Text = "Failed to delete";
            }
        }

        private void consoletext_Click(object sender, EventArgs e)
        {

        }

        private void consoletext_TextChanged(object sender, EventArgs e)
        {
            string toWrite="[ ";
            
            if(DateTime.Now.Day<10)toWrite+="0";
            toWrite+=DateTime.Now.Day+".";
            
            if(DateTime.Now.Month<10)toWrite+="0";
            toWrite+=DateTime.Now.Month+".";

            toWrite += DateTime.Now.Year+"  ";
            

            if(DateTime.Now.Hour<10)toWrite+="0";
            toWrite+=DateTime.Now.Hour+":";
            
            if (DateTime.Now.Minute < 10) toWrite += "0";
            toWrite += DateTime.Now.Minute + ":";
            
            if (DateTime.Now.Second < 10) toWrite += "0";
            toWrite += DateTime.Now.Second+"] ";
            
            toWrite+=consoletext.Text;
            
            try
            {
                StreamWriter sw = new StreamWriter(pathString+"\\log.txt",true);
                sw.WriteLine(toWrite);
                sw.Close();

            }
            catch { }

        }

        public void stripsSave() {

            XmlTextWriter textWritter = new XmlTextWriter("strips.xml", Encoding.UTF8);
            textWritter.WriteStartDocument();
            textWritter.WriteStartElement("strips");
            textWritter.WriteEndElement();
            textWritter.Close();

            XmlDocument document = new XmlDocument();

            document.Load("strips.xml");
            XmlNode strip = document.CreateElement("strip");
            document.DocumentElement.AppendChild(strip);
            XmlAttribute name = document.CreateAttribute("name");
            name.Value = FileStrip.Name.ToString();
            strip.Attributes.Append(name);
            XmlAttribute x = document.CreateAttribute("x");
            x.Value = FileStrip.Location.X.ToString();
            strip.Attributes.Append(x);
            XmlAttribute y = document.CreateAttribute("y");
            y.Value = FileStrip.Location.Y.ToString();
            strip.Attributes.Append(y);

            XmlNode strip2 = document.CreateElement("strip");
            document.DocumentElement.AppendChild(strip2);
            XmlAttribute name2 = document.CreateAttribute("name");
            name2.Value = EditStrip.Name.ToString();
            strip2.Attributes.Append(name2);
            XmlAttribute x2 = document.CreateAttribute("x");
            x2.Value = EditStrip.Location.X.ToString();
            strip2.Attributes.Append(x2);
            XmlAttribute y2 = document.CreateAttribute("y");
            y2.Value = EditStrip.Location.Y.ToString();
            strip2.Attributes.Append(y2);

            XmlNode strip3 = document.CreateElement("strip");
            document.DocumentElement.AppendChild(strip3);
            XmlAttribute name3 = document.CreateAttribute("name");
            name3.Value = PanelStrip.Name.ToString();
            strip3.Attributes.Append(name3);
            XmlAttribute x3 = document.CreateAttribute("x");
            x3.Value = PanelStrip.Location.X.ToString();
            strip3.Attributes.Append(x3);
            XmlAttribute y3 = document.CreateAttribute("y");
            y3.Value = PanelStrip.Location.Y.ToString();
            strip3.Attributes.Append(y3);

            XmlNode strip4 = document.CreateElement("strip");
            document.DocumentElement.AppendChild(strip4);
            XmlAttribute name4 = document.CreateAttribute("name");
            name4.Value = PageStrip.Name.ToString();
            strip4.Attributes.Append(name4);
            XmlAttribute x4 = document.CreateAttribute("x");
            x4.Value = PageStrip.Location.X.ToString();
            strip4.Attributes.Append(x4);
            XmlAttribute y4 = document.CreateAttribute("y");
            y4.Value = PageStrip.Location.Y.ToString();
            strip4.Attributes.Append(y4);

            XmlNode strip5 = document.CreateElement("strip");
            document.DocumentElement.AppendChild(strip5);
            XmlAttribute name5 = document.CreateAttribute("name");
            name5.Value = ButtonStrip.Name.ToString();
            strip5.Attributes.Append(name5);
            XmlAttribute x5 = document.CreateAttribute("x");
            x5.Value = ButtonStrip.Location.X.ToString();
            strip5.Attributes.Append(x5);
            XmlAttribute y5 = document.CreateAttribute("y");
            y5.Value = ButtonStrip.Location.Y.ToString();
            strip5.Attributes.Append(y5);

            XmlNode strip6 = document.CreateElement("strip");
            document.DocumentElement.AppendChild(strip6);
            XmlAttribute name6 = document.CreateAttribute("name");
            name6.Value = LayoutStrip.Name.ToString();
            strip6.Attributes.Append(name6);
            XmlAttribute x6 = document.CreateAttribute("x");
            x6.Value = LayoutStrip.Location.X.ToString();
            strip6.Attributes.Append(x6);
            XmlAttribute y6 = document.CreateAttribute("y");
            y6.Value = LayoutStrip.Location.Y.ToString();
            strip6.Attributes.Append(y6);

            XmlNode strip7 = document.CreateElement("strip");
            document.DocumentElement.AppendChild(strip7);
            XmlAttribute name7 = document.CreateAttribute("name");
            name7.Value = flagsStrip.Name.ToString();
            strip7.Attributes.Append(name7);
            XmlAttribute x7 = document.CreateAttribute("x");
            x7.Value = flagsStrip.Location.X.ToString();
            strip7.Attributes.Append(x7);
            XmlAttribute y7 = document.CreateAttribute("y");
            y7.Value = flagsStrip.Location.Y.ToString();
            strip7.Attributes.Append(y7);


            XmlNode flag = document.CreateElement("flag");
            document.DocumentElement.AppendChild(flag);

            XmlAttribute flag_name = document.CreateAttribute("name");
            flag_name.Value = "stripLoading";
            flag.Attributes.Append(flag_name);

            XmlAttribute state = document.CreateAttribute("state");
            state.Value = stripLoading.ToString();
            flag.Attributes.Append(state);

            XmlNode flag2 = document.CreateElement("flag");
            document.DocumentElement.AppendChild(flag2);

            XmlAttribute flag_name2 = document.CreateAttribute("name");
            flag_name2.Value = "aimDraw";
            flag2.Attributes.Append(flag_name2);

            XmlAttribute state2 = document.CreateAttribute("state");
            state2.Value = aimDraw.ToString();
            flag2.Attributes.Append(state2);

            XmlNode flag3 = document.CreateElement("flag");
            document.DocumentElement.AppendChild(flag3);

            XmlAttribute flag_name3 = document.CreateAttribute("name");
            flag_name3.Value = "dragButton";
            flag3.Attributes.Append(flag_name3);

            XmlAttribute state3 = document.CreateAttribute("state");
            state3.Value = dragButton.ToString();
            flag3.Attributes.Append(state3);

            XmlNode flag4 = document.CreateElement("flag");
            document.DocumentElement.AppendChild(flag4);

            XmlAttribute flag_name4 = document.CreateAttribute("name");
            flag_name4.Value = "autosave";
            flag4.Attributes.Append(flag_name4);

            XmlAttribute state4 = document.CreateAttribute("state");
            state4.Value = autosave.ToString();
            flag4.Attributes.Append(state4);

            XmlNode flag5 = document.CreateElement("flag");
            document.DocumentElement.AppendChild(flag5);

            XmlAttribute flag_name5 = document.CreateAttribute("name");
            flag_name5.Value = "autosavetime";
            flag5.Attributes.Append(flag_name5);

            XmlAttribute state5 = document.CreateAttribute("state");
            state5.Value = autosaveTimer.ToString();
            flag5.Attributes.Append(state5);


            document.Save("strips.xml");
        
        }
        private void TPC_FormClosing(object sender, FormClosingEventArgs e)
        {
            stripsSave();
        }

        private void movefwdStrip_Click(object sender, EventArgs e)
        {
            moveForvardToolStripMenuItem_Click(sender, e);

        }

        private void sendbckStrip_Click(object sender, EventArgs e)
        {
            sendBackToolStripMenuItem_Click(sender, e);
        }

        private void addBtnStrip_Click(object sender, EventArgs e)
        {
            addAmxButtonToPanel();
        }

        private void addLvlStrip_Click(object sender, EventArgs e)
        {
            addAmxLevelToPanel();
        }

        private void delBtnStrip_Click(object sender, EventArgs e)
        {
            deleteCurrentButtonToolStripMenuItem_Click(sender, e);
        }

        private void revStrip_Click(object sender, EventArgs e)
        {
            reverseStateToolStripMenuItem_Click(sender, e);
        }

        private void addPageStrip_Click(object sender, EventArgs e)
        {
            tab.TabPages.Remove(plusPage);
            TabPage page = addPage();
            tab.TabPages.Add(page);
            tab.TabPages.Add(plusPage);
            tab.SelectTab(page);
        }

        private void removePageStrip_Click(object sender, EventArgs e)
        {
            removeCurrentPage();
        }

        private void hidePcbStrip_Click(object sender, EventArgs e)
        {
            freeTabResource();
        }

        private void hidePictureBoxesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            freeTabResource();
        }

        private void newPrjStrip_Click(object sender, EventArgs e)
        {
            createNewProject();
        }

        private void openPrjStrip_Click(object sender, EventArgs e)
        {
            
            
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = " XML Files|*.xml";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                tab.TabPages.Clear();
                string filePath = "";
                filePath = ofd.FileName;
                openXmlFile(filePath);
                activateForm();
            }
        }

        private void savePrjStrip_Click(object sender, EventArgs e)
        {
            createXmlFile();
        }

        private void onStPcbStrip_Click(object sender, EventArgs e)
        {
            deleteOnStatePictureBoxToolStripMenuItem_Click(sender, e);
            setPictureBoxOn();
        }

        private void offStPcbStrip_Click(object sender, EventArgs e)
        {
            deleteOffStatePictureBoxToolStripMenuItem_Click(sender, e);
            setPictureBoxOff();
        }

        private void bgclrStrip_Click(object sender, EventArgs e)
        {
            tab.SelectedTab.BackColor = setBackgroudColor(tab.SelectedTab.BackColor);
        }

        private void delOnStPcb_Click(object sender, EventArgs e)
        {
            deleteOnStatePictureBoxToolStripMenuItem_Click(sender, e);
        }

        private void delOffStPcb_Click(object sender, EventArgs e)
        {
            deleteOffStatePictureBoxToolStripMenuItem_Click(sender, e);
        }
        private Color argbtoint(string argb)//преобразует строковое значение цвета формата ХХХХХХХХ в цвет
        {


            int alpha, red, green, blue;

            alpha = int.Parse("" + argb[0] + argb[1], System.Globalization.NumberStyles.HexNumber);
            red = int.Parse(argb[2].ToString() + argb[3].ToString(), System.Globalization.NumberStyles.HexNumber);
            green = int.Parse(argb[4].ToString() + argb[5].ToString(), System.Globalization.NumberStyles.HexNumber);
            blue = int.Parse(argb[6].ToString() + argb[7].ToString(), System.Globalization.NumberStyles.HexNumber);


            Color clr = Color.FromArgb(red, green, blue);

            return clr;
        }
        private void openXmlFile(string path) {
            progress.Value = 0;

            XmlDocument document = new XmlDocument();
            document.Load(path);
            pathString = path.Remove(path.Length - (Path.GetFileName(path).Length + 1));//отделение имени файла, остается только папка

            
            xmlFileName = Path.GetFileName(path);
            


            XmlNodeList pages = document.GetElementsByTagName("pages");
            progress.Step = (progress.Width / pages.Count);

            foreach (XmlAttribute attr in pages[0].Attributes) {

                switch (attr.Value) { 
                
                    case "width":
                        tab.Width = (int.Parse(attr.Value)+8);
                        break;

                    case "height":
                        tab.Height = (int.Parse(attr.Value)+28);
                        break;
                
                }

            }



            foreach (XmlNode page in pages[0].ChildNodes)
            {

                //создаем страницу
                TabPage myTabPage = new TabPage();
                //перебираем атрибуты
                myTabPage.Name = page.Attributes[0].Value;
                myTabPage.Text = myTabPage.Name;
                //MessageBox.Show(pathString + page.Attributes[1].Value);
                if (page.Attributes[1].Value != "") myTabPage.BackgroundImage = new Bitmap(pathString + "\\" + page.Attributes[1].Value);
                if (page.Attributes[2].Value != "") myTabPage.BackgroundImage = new Bitmap(pathString + "\\" + page.Attributes[2].Value);
                myTabPage.BackColor = argbtoint(page.Attributes[3].Value);
                myTabPage.Size = new System.Drawing.Size(1024, 768);
                myTabPage.MouseDown += new MouseEventHandler(TabPanel_MouseDown);
                tab.TabPages.Add(myTabPage);
                progress.PerformStep();


                

                foreach (XmlNode element in page.ChildNodes) {
                    if (element.Name == "button") {
                        //создаем кнопки
                        createButtonfromXml(element, myTabPage, page);
                    }

                    if (element.Name == "level") {
                        //создаем левелы
                        createLevelfromXml(element, myTabPage, page);
                    }
                
                
                }


             }

            consoletext.Text = "Project successfully opened at " + pathString;
            tab.TabPages.Add(plusPage);
            progress.Value = 0;
            setActivePanel();
        }

        
        private void createButtonfromXml(XmlNode button,TabPage myTabPage,XmlNode page){
            AMX_Button amx_button = new AMX_Button();
            string fontFamily = "";
            float fontSize = 0;
            bool bold=false, italic=false, underline=false;
            string fontFamilyOn = "";
            float fontSizeOn = 0;
            bool boldOn = false, italicOn = false, underlineOn = false;

            //Font myFont;
            foreach (XmlNode property in button.ChildNodes) {

                switch (property.Name) { 
                
                    case "name":
                        amx_button.Name = property.InnerText;
                        break;
                    case "left":
                        amx_button.Left = int.Parse(property.InnerText);
                        break;
                    case "top":
                        amx_button.Top = int.Parse(property.InnerText);
                        break;
                    case "width":
                        amx_button.Width = int.Parse(property.InnerText);
                        break;
                    case "height":
                        amx_button.Height = int.Parse(property.InnerText);
                        break;
                    case "state":
                        if(property.InnerText=="ON") amx_button.Pushed=true;
                        else amx_button.Pushed=false;
                        break;
                    case "zindex":
                        amx_button.TabIndex = int.Parse(property.InnerText);
                        break;
                        //*************************************************
                    case "feedback":
                        if (property.InnerText == "channel") amx_button.Feedback = AMX_Controls.Feedback.channel;
                        if (property.InnerText == "momentary") amx_button.Feedback = AMX_Controls.Feedback.momentary;
                        else amx_button.Feedback = AMX_Controls.Feedback.none;
                        break;
                    case "address_port":
                        amx_button.Address_Port = int.Parse(property.InnerText);
                        break;
                    case "address_code":
                        amx_button.Address_Code = int.Parse(property.InnerText);
                        break;
                    case "channel_port":
                        amx_button.Channel_Port = int.Parse(property.InnerText);
                        break;
                    case "channel_code":
                        amx_button.Channel_Code = int.Parse(property.InnerText);
                        break;
                    case "string_output_port":
                        amx_button.String_Output_Port = int.Parse(property.InnerText);
                        break;
                    case "string_output":
                        amx_button.String_Output = property.InnerText;
                        break;
                    case "command_output_port":
                        amx_button.Command_Output_Port = int.Parse(property.InnerText);
                        break;
                    case "command_output":
                        amx_button.Command_Output = property.InnerText;
                        break;
                        //**************************************************

                    case "picturebox_off":
                        
                        if (property.InnerText == "Use" && page.Attributes[1].Value != "") {
                            PictureBox pcboff = new PictureBox();
                            pcboff.Image = new Bitmap(pathString + "\\" + page.Attributes[1].Value);
                            amx_button.PictureBoxOff = pcboff;    
                        }
                        break;

                    case "background_color_off":
                        amx_button.FillColorOff = argbtoint(property.InnerText);
                        break;

                    case "text_color_off":
                        amx_button.TextColorOff = argbtoint(property.InnerText);
                        break;
                                           
                        /////////////////////////////////////
                    case "font_off_family":
                        fontFamily = property.InnerText;
                        break;
                    case "font_off_size":
                        fontSize = float.Parse(property.InnerText);
                        break;
                    case "font_off_bold":
                        if (property.InnerText == "True") bold = true;
                        break;
                    case "font_off_italic":
                        if (property.InnerText == "True") italic = true;
                        break;
                    case "font_off_underline":
                        if (property.InnerText == "True") underline = true;
                        break;
                        //////////////////////////////////////
                    case "text_off":
                        amx_button.TextOff = property.InnerText;
                        break;

                    case "bitmap_off":
                        if (property.InnerText != "none" && property.InnerText != "") amx_button.BitmapOff = new Bitmap(pathString + "\\" + property.InnerText);
                        break;
                        //***************************************************


                    case "picturebox_on":

                        if (property.InnerText == "Use" && page.Attributes[2].Value != "")
                        {
                            PictureBox pcbon = new PictureBox();
                            pcbon.Image = new Bitmap(pathString + "\\" + page.Attributes[2].Value);
                            amx_button.PictureBoxOn = pcbon;
                        }
                        break;

                    case "background_color_on":
                        amx_button.FillColorOn = argbtoint(property.InnerText);
                        break;

                    case "text_color_on":
                        amx_button.TextColorOn = argbtoint(property.InnerText);
                        break;

                    /////////////////////////////////////
                    case "font_on_family":
                        fontFamilyOn = property.InnerText;
                        break;
                    case "font_on_size":
                        fontSizeOn = float.Parse(property.InnerText);
                        break;
                    case "font_on_bold":
                        if (property.InnerText == "True") boldOn = true;
                        break;
                    case "font_on_italic":
                        if (property.InnerText == "True") italicOn = true;
                        break;
                    case "font_on_underline":
                        if (property.InnerText == "True") underlineOn = true;
                        break;
                    //////////////////////////////////////
                    case "text_on":
                        amx_button.TextOn = property.InnerText;
                        break;

                    case "bitmap_on":
                        if (property.InnerText != "none" && property.InnerText != "") amx_button.BitmapOn = new Bitmap(pathString + "\\" + property.InnerText);
                        break;
                    //***************************************************



                }
            
            }
            amx_button.FontOff = CreateFont(fontFamily, fontSize, bold, italic, underline);
            amx_button.FontOn = CreateFont(fontFamilyOn, fontSizeOn, boldOn, italicOn, underlineOn);
            amx_button.BorderStyleOff = BorderStyle.None;
            amx_button.BorderStyleOn = BorderStyle.None;

            amx_button.MouseClick += new MouseEventHandler(amxBtn_MouseClick);
            amx_button.MouseDown += new MouseEventHandler(amxBtn_MouseDown);
            amx_button.MouseUp += new MouseEventHandler(amxBtn_MouseUp);
            amx_button.MouseMove += new MouseEventHandler(amxBtn_MouseMove);
            amx_button.GotFocus += new EventHandler(amxBtn_GotFocus);



            myTabPage.Controls.Add(amx_button);
        
        }

        private void createLevelfromXml(XmlNode level, TabPage myTabPage, XmlNode page) {

            Size mySize = new Size();

            int height=0, width=0;

            string fontFamily = "";
            float fontSize = 0;
            bool bold = false, italic = false, underline = false;

            AMX_Level amx_level = new AMX_Level();

            foreach (XmlNode property in level.ChildNodes) {

                switch (property.Name) {

                    case "name":
                        amx_level.Name = property.InnerText;
                        break;
                    case "left":
                        amx_level.Left = int.Parse(property.InnerText);
                        break;
                    case "top":
                        amx_level.Top = int.Parse(property.InnerText);
                        break;
                    case "width":
                        width = int.Parse(property.InnerText);
                        break;
                    case "height":
                        height = int.Parse(property.InnerText);
                        break;
                    case "minimum":
                        amx_level.Minimum = int.Parse(property.InnerText);
                        break;
                    case "maximum":
                        amx_level.Maximum = int.Parse(property.InnerText);
                        break;
                    case "value":
                        amx_level.Level_Value = int.Parse(property.InnerText);
                        break;
                    case "orientation":
                        if (property.InnerText == "horizontal") amx_level.Orientation =AMX_Level.T_Orientation.Horizontal;
                        else if (property.InnerText == "vertical") amx_level.Orientation = AMX_Level.T_Orientation.Vertical;                        
                        break;
                    case "zindex":
                        amx_level.TabIndex = int.Parse(property.InnerText);
                        break;
                        //--------------------------------------------------------

                    case "function":
                        if (property.InnerText == "active") amx_level.Level_Function = AMX_Level.T_LevelFunction.Active;
                        else if (property.InnerText == "display") amx_level.Level_Function = AMX_Level.T_LevelFunction.Display;
                        break;

                    case "level_port":
                        amx_level.Level_Port = int.Parse(property.InnerText);
                        break;
                    case "level_code":
                        amx_level.Level_Code = int.Parse(property.InnerText);
                        break;
                    case "address_port":
                        amx_level.Address_Port = int.Parse(property.InnerText);
                        break;
                    case "address_code":
                        amx_level.Address_Code = int.Parse(property.InnerText);
                        break;

                        //-------------------------------------------------
                    case "font_family":
                        fontFamily = property.InnerText;
                        break;
                    case "font_size":
                        fontSize = float.Parse(property.InnerText);
                        break;
                    case "font_bold":
                        if (property.InnerText == "True") bold = true;
                        break;
                    case "font_italic":
                        if (property.InnerText == "True") italic = true;
                        break;
                    case "font_underline":
                        if (property.InnerText == "True") underline = true;
                        break;
                        //-----------------------------------------------
                    case "text":
                        amx_level.TextDisplay = property.InnerText;
                        break;


                    case "picturebox_off":

                        if (property.InnerText == "Use" && page.Attributes[1].Value != "")
                        {
                            PictureBox pcbon = new PictureBox();
                            pcbon.Image = new Bitmap(pathString + "\\" + page.Attributes[1].Value);
                            amx_level.PictureBoxOn = pcbon;
                        }
                        break;

                    case "picturebox_on":

                        if (property.InnerText == "Use" && page.Attributes[2].Value != "")
                        {
                            PictureBox pcbon = new PictureBox();
                            pcbon.Image = new Bitmap(pathString + "\\" + page.Attributes[2].Value);
                            amx_level.PictureBoxOn = pcbon;
                        }
                        break;

                    case "color_off":
                        amx_level.FillColorOff =argbtoint(property.InnerText);
                        break;
                    case "color_on":
                        amx_level.FillColorOn = argbtoint(property.InnerText);
                        break;
                    case "text_color":
                        amx_level.TextColor = argbtoint(property.InnerText);
                        break;
                           
                }
            
            
            
            }



            mySize.Height = height;
            mySize.Width = width;
            amx_level.Font = CreateFont(fontFamily, fontSize, bold, italic, underline);
            amx_level.Size = mySize;
            amx_level.BorderStyle = BorderStyle.None;
            amx_level.MouseClick += new MouseEventHandler(amxLvl_MouseClick);
            amx_level.GotFocus += new EventHandler(amxLvl_GotFocus);            
            myTabPage.Controls.Add(amx_level);

        }

        private Font CreateFont(string fontFamily, float fontSize, bool bold, bool italic, bool underline) {

            Font myFont;
            if (fontFamily != "Monotype Corsiva")
            {
                myFont = new Font(fontFamily, fontSize);
                
                if (bold)
                {
                    myFont = new Font(fontFamily, fontSize, FontStyle.Bold);
                    
                    if (italic && !(underline))
                    {
                        myFont = new Font(fontFamily, fontSize, FontStyle.Bold | FontStyle.Italic);

                    }


                    if (underline && !(italic))
                    {

                        myFont = new Font(fontFamily, fontSize, FontStyle.Bold | FontStyle.Underline);

                    }

                    if (underline && italic)
                    {

                        myFont = new Font(fontFamily, fontSize, FontStyle.Bold | FontStyle.Italic | FontStyle.Underline);

                    }

                }


                else if (italic)
                {

                    myFont = new Font(fontFamily, fontSize, FontStyle.Italic);

                    if (underline)
                    {

                        myFont = new Font(fontFamily, fontSize, FontStyle.Italic | FontStyle.Underline);

                    }

                }


                else if (underline)
                {

                    myFont = new Font(fontFamily, fontSize, FontStyle.Underline);

                }

                else
                {
                    myFont = new Font(fontFamily, fontSize);
                }

            }

            else
            {

                myFont = new Font(fontFamily, fontSize, FontStyle.Italic);


                if (bold)
                {
                    myFont = new Font(fontFamily, fontSize, FontStyle.Italic | FontStyle.Bold);

                }

            }








            return myFont;
        
        }

        private void splitContainer3_SplitterMoved(object sender, SplitterEventArgs e)
        {

        }

        private void preferencesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            preferences form = new preferences();
            form.Owner = this;
            form.ShowDialog();
        }

        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void preferencesStrip_Click(object sender, EventArgs e)
        {
            preferences form = new preferences();
            form.Owner = this;
            form.ShowDialog();
        }

        private void toolStripTextBox1_Click(object sender, EventArgs e)
        {
            
        }

        private void findBox_Enter(object sender, EventArgs e)
        {
            findBox.Text = "";
            findBox.ForeColor = Color.Black;
        }

        private void findBox_Leave(object sender, EventArgs e)
        {
            if (findBox.Text == "") {
                findBox.Text = "Find button..";
                findBox.ForeColor = Color.LightGray;
            }
        }

        private void searchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            findBox.Focus();
        }

        private bool search() {
            if (findBox.Text != "")
            {
                string btnName = findBox.Text;

                foreach (TabPage tabPage in tab.TabPages)
                {

                    foreach (Control cnt in tabPage.Controls)
                    {

                        if (cnt.ToString() == "AMX_Controls.AMX_Button")
                        {
                            AMX_Button ab = cnt as AMX_Button;
                            if (ab.Name == btnName)
                            {
                                tab.SelectTab(tabPage);
                                ab.Focus();
                                return true;
                            }

                        }

                        else if (cnt.ToString() == "AMX_Controls.AMX_Level")
                        {
                            AMX_Level al = cnt as AMX_Level;
                            if (al.Name == btnName)
                            {
                                tab.SelectTab(tabPage);
                                al.Focus();
                                return true;
                            }

                        }


                    }



                }



            }
            return false;
        }

        private void searchStrip_Click(object sender, EventArgs e)
        {
            if (!search()) MessageBox.Show("No such button or level");
        }

        
        private void findBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter&&findBox.Text!="") {                
                if (!search()) MessageBox.Show("No such button or level");
            }
        }

        private void aimCursorEnable_Click(object sender, EventArgs e)
        {
            aimCursorEnable.Checked = !aimCursorEnable.Checked;

            aimDraw = aimCursorEnable.Checked;
        }



        //------------------DragandDropControl-------------------------
       

            Point DDposition = new Point();
            Size DDSize = new Size();

            Size prevDDSize = new Size();
            private void initializeDD() {

               



                squareCenter.Cursor = Cursors.SizeAll;
                squareNE.Cursor = Cursors.SizeNWSE;
                squareNW.Cursor = Cursors.SizeNESW;
                squareSW.Cursor = Cursors.SizeNWSE;
                squareSE.Cursor = Cursors.SizeNESW;
                squareN.Cursor = Cursors.SizeNS;
                squareS.Cursor = Cursors.SizeNS;
                squareW.Cursor = Cursors.SizeWE;
                squareE.Cursor = Cursors.SizeWE;


                squareCenter.MouseDown += new MouseEventHandler(squareCenter_MouseDown);
                squareCenter.MouseUp += new MouseEventHandler(squareCenter_MouseUp);
                squareCenter.MouseMove += new MouseEventHandler(squareCenter_MouseMove);

                squareNE.MouseDown += new MouseEventHandler(squareNE_MouseDown);
                squareNE.MouseUp += new MouseEventHandler(squareNE_MouseUp);
                squareNE.MouseMove += new MouseEventHandler(squareNE_MouseMove);

                squareNW.MouseDown += new MouseEventHandler(squareNW_MouseDown);
                squareNW.MouseUp += new MouseEventHandler(squareNW_MouseUp);
                squareNW.MouseMove += new MouseEventHandler(squareNW_MouseMove);

                squareSW.MouseDown += new MouseEventHandler(squareSW_MouseDown);
                squareSW.MouseUp += new MouseEventHandler(squareSW_MouseUp);
                squareSW.MouseMove += new MouseEventHandler(squareSW_MouseMove);

                squareSE.MouseDown += new MouseEventHandler(squareSE_MouseDown);
                squareSE.MouseUp += new MouseEventHandler(squareSE_MouseUp);
                squareSE.MouseMove += new MouseEventHandler(squareSE_MouseMove);

                squareN.MouseDown += new MouseEventHandler(squareN_MouseDown);
                squareN.MouseUp += new MouseEventHandler(squareN_MouseUp);
                squareN.MouseMove += new MouseEventHandler(squareN_MouseMove);

                squareW.MouseDown += new MouseEventHandler(squareW_MouseDown);
                squareW.MouseUp += new MouseEventHandler(squareW_MouseUp);
                squareW.MouseMove += new MouseEventHandler(squareW_MouseMove);

                squareS.MouseDown += new MouseEventHandler(squareS_MouseDown);
                squareS.MouseUp += new MouseEventHandler(squareS_MouseUp);
                squareS.MouseMove += new MouseEventHandler(squareS_MouseMove);

                squareE.MouseDown += new MouseEventHandler(squareE_MouseDown);
                squareE.MouseUp += new MouseEventHandler(squareE_MouseUp);
                squareE.MouseMove += new MouseEventHandler(squareE_MouseMove);





                squareCenter.Size = new Size(DDWidth * 2, DDWidth * 2);
                squareNE.Size = new Size(DDWidth, DDWidth);
                squareNW.Size = new Size(DDWidth, DDWidth);
                squareSW.Size = new Size(DDWidth, DDWidth);
                squareSE.Size = new Size(DDWidth, DDWidth);
                squareN.Size = new Size(DDWidth, DDWidth);
                squareW.Size = new Size(DDWidth, DDWidth);
                squareS.Size = new Size(DDWidth, DDWidth);
                squareE.Size = new Size(DDWidth, DDWidth);


                

            }
            private void acceptDDtoControl() {

                    if (DDSize.Height <= 0 || DDSize.Width <= 0)
                    {
                        DDSize = prevDDSize;
                    }
                    else
                    {
                        prevDDSize = DDSize;
                    }

                if (btnType)//level
                {
                   
                    DDLevel.Location = DDposition;
                    DDLevel.Size = DDSize;
                    saveElementToHistory(DDLevel.Name, DDposition.X, DDposition.Y, DDSize.Width, DDSize.Height);
                }
                else { //button

                    DDButton.Location = DDposition;
                    DDButton.Size = DDSize;
                    saveElementToHistory(DDButton.Name, DDposition.X, DDposition.Y, DDSize.Width, DDSize.Height);
                }

                tab.SelectedTab.Refresh();

                

            }

            bool btnType = false;//false=button, true=level
            AMX_Level DDLevel = new AMX_Level();
            AMX_Button DDButton = new AMX_Button();
           // Pen pen;
            
        private void createDDControl(Control control) {
           
            if (control.ToString() == "AMX_Controls.AMX_Button")
            {
                 DDButton = control as AMX_Button;
                 DDposition.X = DDButton.Left;
                 DDposition.Y = DDButton.Top;

                 DDSize.Width = DDButton.Width;
                 DDSize.Height = DDButton.Height;

               
               
                btnType = false;
            }

            if (control.ToString() == "AMX_Controls.AMX_Level")
            {
                DDLevel = control as AMX_Level;
                DDposition.X = DDLevel.Left;
                DDposition.Y = DDLevel.Top;

                DDSize.Width = DDLevel.Width;
                DDSize.Height = DDLevel.Height;
              
                btnType = true;
            }

            prevDDSize = DDSize;
            
            squareCenter.BackColor = negativeColor(tab.SelectedTab.BackColor);
            squareNE.BackColor = negativeColor(tab.SelectedTab.BackColor);
            squareNW.BackColor = negativeColor(tab.SelectedTab.BackColor);
            squareSW.BackColor = negativeColor(tab.SelectedTab.BackColor);
            squareSE.BackColor = negativeColor(tab.SelectedTab.BackColor);
            squareN.BackColor = negativeColor(tab.SelectedTab.BackColor);
            squareW.BackColor = negativeColor(tab.SelectedTab.BackColor);
            squareS.BackColor = negativeColor(tab.SelectedTab.BackColor);
            squareE.BackColor = negativeColor(tab.SelectedTab.BackColor);
            DDinfo.BackColor = tab.SelectedTab.BackColor;
            DDinfo.ForeColor = negativeColor(DDinfo.BackColor);




            tab.SelectedTab.Controls.Add(squareCenter);
            tab.SelectedTab.Controls.Add(squareNE);
            tab.SelectedTab.Controls.Add(squareNW);
            tab.SelectedTab.Controls.Add(squareSW);
            tab.SelectedTab.Controls.Add(squareSE);
            tab.SelectedTab.Controls.Add(squareN);
            tab.SelectedTab.Controls.Add(squareW);
            tab.SelectedTab.Controls.Add(squareS);
            tab.SelectedTab.Controls.Add(squareE);

            drawDD();
           // g = tab.SelectedTab.CreateGraphics();
           
        }

        PictureBox squareCenter = new PictureBox();
        PictureBox squareNE = new PictureBox();
        PictureBox squareNW = new PictureBox();
        PictureBox squareSW = new PictureBox();
        PictureBox squareSE = new PictureBox();

        PictureBox squareN = new PictureBox();
        PictureBox squareW = new PictureBox();
        PictureBox squareS = new PictureBox();
        PictureBox squareE = new PictureBox();

        Point startMousePos = new Point();
        bool mousePress = false;

        /*поведение при нажатии на квадратики*/

        private void squareNE_MouseDown(object sender, MouseEventArgs e) 
        {
            startMousePos = e.Location;
            DDinfo.Visible = true;
            mousePress = true;
        
        }
        private void squareNE_MouseUp(object sender, MouseEventArgs e)
        {           
            mousePress = false;
            DDinfo.Visible = false;
            acceptDDtoControl();
        }
        private void squareNE_MouseMove(object sender, MouseEventArgs e)
        {
            if (mousePress) {

                squareNE.Left+=e.X - startMousePos.X;
                squareNE.Top+=e.Y - startMousePos.Y;
                int x, y;
                x=squareNE.Left - squareNW.Left - DDWidth;
                y=squareNE.Top - squareSE.Top - DDWidth;

                if (x >= 10) DDSize.Width = x;
                if (y >= 10) DDSize.Height = y;              
                
                
                
                
                drawDD();

            }
            
            tab.SelectedTab.Refresh();
        }

        private void squareNW_MouseDown(object sender, MouseEventArgs e)
        {
            startMousePos = e.Location;
            DDinfo.Visible = true;
            mousePress = true;

        }
        private void squareNW_MouseUp(object sender, MouseEventArgs e)
        {
            mousePress = false;
            DDinfo.Visible = false;
            acceptDDtoControl();
        }
        private void squareNW_MouseMove(object sender, MouseEventArgs e)
        {
            if (mousePress)
            {

                squareNW.Left += e.X - startMousePos.X;
                squareNW.Top += e.Y - startMousePos.Y;

                int x, y;
                x=squareNE.Left - squareNW.Left - DDWidth;
                y=squareNW.Top - squareSW.Top - DDWidth;
                if (x >= 10) {
                    DDposition.X += e.X - startMousePos.X;
                    DDSize.Width = x; 
                }
                if (y >= 10) DDSize.Height = y;     
                
                

                
               
                drawDD();

            }

            tab.SelectedTab.Refresh();
        }

        private void squareSW_MouseDown(object sender, MouseEventArgs e)
        {
            startMousePos = e.Location;
            DDinfo.Visible = true;
            mousePress = true;

        }
        private void squareSW_MouseUp(object sender, MouseEventArgs e)
        {
            mousePress = false;
            DDinfo.Visible = false;
            acceptDDtoControl();
        }
        private void squareSW_MouseMove(object sender, MouseEventArgs e)
        {
            if (mousePress)
            {

                squareSW.Left += e.X - startMousePos.X;
                squareSW.Top += e.Y - startMousePos.Y;
                int x, y;
                x=squareSE.Left - squareSW.Left - DDWidth;
                y=squareNW.Top - squareSW.Top - DDWidth;


                if (x >= 10)
                {
                    DDposition.X += e.X - startMousePos.X;
                    DDSize.Width = x;
                }
                if (y >= 10)
                {
                    DDposition.Y += e.Y - startMousePos.Y;
                    DDSize.Height = y;
                }
                
                drawDD();

            }

            tab.SelectedTab.Refresh();
        }

        private void squareSE_MouseDown(object sender, MouseEventArgs e)
        {
            startMousePos = e.Location;
            DDinfo.Visible = true;
            mousePress = true;

        }
        private void squareSE_MouseUp(object sender, MouseEventArgs e)
        {
            mousePress = false;
            DDinfo.Visible = false;
            acceptDDtoControl();
        }
        private void squareSE_MouseMove(object sender, MouseEventArgs e)
        {
            if (mousePress)
            {

                squareSE.Left += e.X - startMousePos.X;
                squareSE.Top += e.Y - startMousePos.Y;
                int x, y;

                x=squareSE.Left - squareSW.Left - DDWidth;
                y=squareNE.Top - squareSE.Top - DDWidth;

                if (x >= 10) {
                    DDSize.Width = x;
                }
                if (y >= 10) { 
                DDposition.Y += e.Y - startMousePos.Y;
                DDSize.Height = y;
                }
                
                drawDD();

            }

            tab.SelectedTab.Refresh();
        }

        private void squareN_MouseDown(object sender, MouseEventArgs e)
        {
            startMousePos = e.Location;
            DDinfo.Visible = true;
            mousePress = true;

        }
        private void squareN_MouseUp(object sender, MouseEventArgs e)
        {
            mousePress = false;
            DDinfo.Visible = false;
            acceptDDtoControl();
        }
        private void squareN_MouseMove(object sender, MouseEventArgs e)
        {
            if (mousePress)
            {
                squareNE.Top += e.Y - startMousePos.Y;
                int y;
                y=squareNE.Top - squareSE.Top - DDWidth;             
               

                if (y >= 10)DDSize.Height = y;
             
                drawDD();

            }

            tab.SelectedTab.Refresh();
        }

        private void squareW_MouseDown(object sender, MouseEventArgs e)
        {
            startMousePos = e.Location;
            DDinfo.Visible = true;
            mousePress = true;

        }
        private void squareW_MouseUp(object sender, MouseEventArgs e)
        {
            mousePress = false;
            DDinfo.Visible = false;
            acceptDDtoControl();
        }
        private void squareW_MouseMove(object sender, MouseEventArgs e)
        {
            if (mousePress)
            {
                int x;
                squareNW.Left += e.X - startMousePos.X;
                x=squareNE.Left - squareNW.Left - DDWidth;
                if (x >= 10) { 
                 DDposition.X += e.X - startMousePos.X;
                 DDSize.Width = x;
                }
                
             
                drawDD();

            }

            tab.SelectedTab.Refresh();
        }

        private void squareS_MouseDown(object sender, MouseEventArgs e)
        {
            startMousePos = e.Location;
            DDinfo.Visible = true;
            mousePress = true;

        }
        private void squareS_MouseUp(object sender, MouseEventArgs e)
        {
            mousePress = false;
            DDinfo.Visible = false;
            acceptDDtoControl();
        }
        private void squareS_MouseMove(object sender, MouseEventArgs e)
        {
            if (mousePress)
            {
                int y;
                squareSW.Top += e.Y - startMousePos.Y;

                y=squareNW.Top - squareSW.Top - DDWidth;
                if (y >= 10)
                {
                DDposition.Y += e.Y - startMousePos.Y;
                DDSize.Height = y;
                }
                
               
                drawDD();

            }

            tab.SelectedTab.Refresh();
        }

        private void squareE_MouseDown(object sender, MouseEventArgs e)
        {
            startMousePos = e.Location;
            DDinfo.Visible = true;
            mousePress = true;

        }
        private void squareE_MouseUp(object sender, MouseEventArgs e)
        {
            mousePress = false;
            DDinfo.Visible = false;
            acceptDDtoControl();
        }
        private void squareE_MouseMove(object sender, MouseEventArgs e)
        {
            if (mousePress)
            {
                int x;
                squareSE.Left += e.X - startMousePos.X;

                x=squareSE.Left - squareSW.Left - DDWidth;

                if (x >= 10) DDSize.Width = x; 

                drawDD();

            }

            tab.SelectedTab.Refresh();
        }
        
        private void squareCenter_MouseDown(object sender, MouseEventArgs e)
        {
            startMousePos = e.Location;
            DDinfo.Visible = true;
            mousePress = true;

        }
        private void squareCenter_MouseUp(object sender, MouseEventArgs e)
        {
            mousePress = false;
            DDinfo.Visible = false;
            acceptDDtoControl();
        }
        private void squareCenter_MouseMove(object sender, MouseEventArgs e)
        {
            if (mousePress)
            {

                DDposition.X += e.X - startMousePos.X;
                DDposition.Y += e.Y - startMousePos.Y;

                
                drawDD();

            }

            tab.SelectedTab.Refresh();
        }
        /* -------------------------------------- */
        private void drawDD() {
            
            DDinfo.Text = "Loc:" + DDposition.X + "," + DDposition.Y + "\nSize:" + DDSize.Width + "," + DDSize.Height;

            DDinfo.Left = DDposition.X + DDSize.Width+DDWidth;
            DDinfo.Top = DDposition.Y + DDSize.Height+DDWidth;


            squareCenter.Visible = true;
            squareNE.Visible = true;
            squareNW.Visible = true;
            squareSW.Visible = true;
            squareSE.Visible = true;
            squareN.Visible = true;
            squareW.Visible = true;
            squareS.Visible = true;
            squareE.Visible = true;

           

        squareNE.Left = DDposition.X + DDSize.Width;
        squareNE.Top = DDposition.Y + DDSize.Height;

        squareNW.Left = DDposition.X - DDWidth;
        squareNW.Top = DDposition.Y + DDSize.Height;

        squareSW.Left = DDposition.X - DDWidth;
        squareSW.Top = DDposition.Y - DDWidth;

        squareSE.Left = DDposition.X + DDSize.Width;
        squareSE.Top = DDposition.Y - DDWidth;

        squareN.Left = DDposition.X + DDSize.Width/2-DDWidth/2;
        squareN.Top = DDposition.Y + DDSize.Height;

        squareW.Left = DDposition.X - DDWidth;
        squareW.Top = DDposition.Y + DDSize.Height/2-DDWidth/2;

        squareS.Left = DDposition.X + DDSize.Width / 2 - DDWidth / 2;
        squareS.Top = DDposition.Y - DDWidth;

        squareE.Left = DDposition.X + DDSize.Width;
        squareE.Top = DDposition.Y + DDSize.Height / 2 - DDWidth / 2;

        squareCenter.Left = DDposition.X + DDSize.Width / 2 - DDWidth;
        squareCenter.Top = DDposition.Y + DDSize.Height / 2 - DDWidth;

  

        squareCenter.BringToFront();
        squareNE.BringToFront();
        squareNW.BringToFront();
        squareSE.BringToFront();
        squareSW.BringToFront();
        squareN.BringToFront();
        squareW.BringToFront();
        squareS.BringToFront();
        squareE.BringToFront();
        DDinfo.BringToFront();

       /* pen = new Pen(negativeColor(tab.SelectedTab.BackColor), 1);
        pen.DashStyle = DashStyle.Dot;

        g.DrawRectangle(pen,DDposition.X-1,DDposition.Y-1,DDSize.Width+2,DDSize.Height+2);
        */

        
        }

        private void hideDDs() {
           
            squareCenter.Visible = false;
            squareNE.Visible = false;
            squareNW.Visible = false;
            squareSW.Visible = false;
            squareSE.Visible = false;
            squareN.Visible = false;
            squareW.Visible = false;
            squareS.Visible = false;
            squareE.Visible = false;

        }

        private void page1_Click(object sender, EventArgs e)
        {

        }

        private void disactivateForm() {
            //helpToolStripMenuItem.Enabled = false;
            
            layoutToolStripMenuItem.Enabled = false;
            buttonToolStripMenuItem.Enabled = false;
            pageToolStripMenuItem.Enabled = false;
            panelToolStripMenuItem.Enabled = false;
            editToolStripMenuItem.Enabled = false;
            saveProjectToolStripMenuItem.Enabled = false;

            ButtonStrip.Enabled = false;
            PanelStrip.Enabled = false;
            PageStrip.Enabled = false;
            ButtonStrip.Enabled = false;
            LayoutStrip.Enabled = false;
            EditStrip.Enabled = false;
            savePrjStrip.Enabled = false;
            drawingEnable.Enabled = false;

        }

        private void activateForm() {

            helpToolStripMenuItem.Enabled        = true;            
            layoutToolStripMenuItem.Enabled      = true;
            buttonToolStripMenuItem.Enabled      = true;
            pageToolStripMenuItem.Enabled        = true;
            panelToolStripMenuItem.Enabled       = true;
            editToolStripMenuItem.Enabled        = true;
            saveProjectToolStripMenuItem.Enabled = true;

            ButtonStrip.Enabled   = true;
            PanelStrip.Enabled    = true;
            PageStrip.Enabled     = true;
            ButtonStrip.Enabled   = true;
            LayoutStrip.Enabled   = true;
            EditStrip.Enabled     = true;
            savePrjStrip.Enabled  = true;
            drawingEnable.Enabled = true;
        }

        private void backToolStripMenuItem_Click(object sender, EventArgs e)
        {
            goBackForOneStep();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            goBackForOneStep();
        }

        private void showAimToolStripMenuItem_Click(object sender, EventArgs e)
        {
            showAimToolStripMenuItem.Checked = !showAimToolStripMenuItem.Checked;
            aimDraw = aimCursorEnable.Checked;
        }

        private void drawingToolStripMenuItem_Click(object sender, EventArgs e)
        {            
            drawing = !drawing;
            drawingToolStripMenuItem.Checked = drawing;
            drawingEnable.Checked = drawing;
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            drawing = !drawing;
            drawingEnable.Checked = drawing;
            drawingToolStripMenuItem.Checked = drawing;
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }

        private void buttonToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            tab.SelectedTab.Controls.Add(createAmxButton("P" + (tab.TabPages.IndexOf(tab.SelectedTab) + 1).ToString() + "_Btn", drawingX, drawingY, drawingW, drawingH));
            sortItems();
            Graphics g = tab.SelectedTab.CreateGraphics();
            g.Clear(tab.SelectedTab.BackColor);
        }

        private void levelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            tab.SelectedTab.Controls.Add(createAmxLevel("P" + (tab.TabPages.IndexOf(tab.SelectedTab) + 1).ToString() + "_Lvl", drawingX, drawingY, drawingW, drawingH));
            sortItems();
            Graphics g = tab.SelectedTab.CreateGraphics();
            g.Clear(tab.SelectedTab.BackColor);
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Help form = new Help();
            form.Owner = this;
            form.ShowDialog();
        }

        

       

    }
    public class SelectData
    {
        public readonly int Value;
        public readonly string Text;

        public SelectData(int Value, string Text)
        {
            this.Value = Value;
            this.Text = Text;
        }

        public override string ToString()
        {
            return this.Text;
        }
    }
}
//разобраться с сохранением стрипсов