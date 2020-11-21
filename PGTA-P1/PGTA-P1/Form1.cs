using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;

namespace PGTA_P1
{
    public partial class Form1 : Form
    {
        List<DataBlock> DataBlockList;
        List<DataTable> DataTable1000;
        List<Target> TargetList = new List<Target>();
        DataTable TargetTable = new DataTable();
        int numDTable = 0;

        //Cats i sources
        string CatView = "All";
        string[] SourceViewV = new string[2];
        bool SMR = false;
        bool MULT = false;
        bool ADSB = false;
        bool CAT10 = false;
        string SourView = "All";
        string IdView = "All";

        //GMaps
        double BCNLat = 41.2972361111;
        double BCNLon = 2.0783333333;

        //Timer sets
        TimeSpan interval = new TimeSpan(0,0,1);
        string velocitat = "x 1";
        bool Play = false;
        TimeSpan Temps = new TimeSpan(8, 0, 0);

        //Moure ventana
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        [DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        //Constructor
        public Form1()
        {
            InitializeComponent();

            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();

            PGB1.Minimum = 1;

            SourceViewV[0] = "All";
            SourceViewV[1] = "-";

            //GMaps
            Map.DragButton = MouseButtons.Left;
            Map.CanDragMap = true;
            Map.MapProvider = GMapProviders.GoogleMap;
            Map.Position = new PointLatLng(BCNLat, BCNLon);
            Map.MaxZoom = 24;
            Map.MinZoom = 0;
            Map.Zoom = 14;
            Map.AutoScroll = true;

            //TimerSets
            Velo.Text = velocitat;
        }

        //Filtrar per categoria (te en compte la pagina DataTable)
        private DataView FiltrarCatSour()
        {
            //DataTable Inicial = DataTable1000[numDTable];
            //DataRow[] F = new DataRow[999];
            //DataTable Filtrada = new DataTable();
            //Filtrada.Columns.Add("Category");
            //Filtrada.Columns.Add("Source");
            //Filtrada.Columns.Add("Target ID");
            //Filtrada.Columns.Add("Track Number");
            //Filtrada.Columns.Add("Vehicle Fleet");
            //Filtrada.Columns.Add("DataBlock Id");

            //if ((CatView == "All") && (SourView == "All"))
            //    Filtrada = Inicial;
            //else if (CatView == "All") //Radar no es all
            //{
            //    F = Inicial.Select("Source = '" + SourView + "'");
            //    int i = 0;
            //    while (i < F.Count())
            //    {
            //        Filtrada.ImportRow(F[i]);
            //        i++;
            //    }
            //}
            //else if (SourView == "All") //Cat no es all
            //{
            //    F = Inicial.Select("Category = '" + CatView + "'");
            //    int i = 0;
            //    while (i < F.Count())
            //    {
            //        Filtrada.ImportRow(F[i]);
            //        i++;
            //    }
            //}
            //else //Cap es all
            //{
            //    F = Inicial.Select("Category = '" + CatView + "' AND Source = '" + SourView + "'");
            //    int i = 0;
            //    while (i < F.Count())
            //    {
            //        Filtrada.ImportRow(F[i]);
            //        i++;
            //    }
            //}
            //DataView ret = Filtrada.DefaultView;

            //previousBTT.Visible = true;
            //nextBTN.Visible = true;
            //Max.Text = Convert.ToString(this.DataTable1000.Count());

            //return ret;

            DataTable Inicial = DataTable1000[numDTable];
            DataRow[] F = new DataRow[999];
            DataTable Filtrada = new DataTable();
            Filtrada.Columns.Add("Category");
            Filtrada.Columns.Add("Source");
            Filtrada.Columns.Add("Target ID");
            Filtrada.Columns.Add("Track Number");
            Filtrada.Columns.Add("Vehicle Fleet");
            Filtrada.Columns.Add("DataBlock Id");

            if ((CatView == "All") && (SourceViewV[0] == "All") && (SourceViewV[1] == "-"))
                Filtrada = Inicial;
            else if (CatView == "All") //Radar no es all
            {
                F = Inicial.Select("Source = '" + SourceViewV[0] + "'");
                int i = 0;
                while (i < F.Count())
                {
                    Filtrada.ImportRow(F[i]);
                    i++;
                }
                if (SourceViewV[1] != "-")
                {
                    F = Inicial.Select("Source = '" + SourceViewV[1] + "'");
                    i = 0;
                    while (i < F.Count())
                    {
                        Filtrada.ImportRow(F[i]);
                        i++;
                    }
                }
            }
            else if ((SourceViewV[0] == "All") && (SourceViewV[1] == "-")) //Cat no es all
            {
                F = Inicial.Select("Category = '" + CatView + "'");
                int i = 0;
                while (i < F.Count())
                {
                    Filtrada.ImportRow(F[i]);
                    i++;
                }
            }
            else //Cap es all
            {
                F = Inicial.Select("Category = '" + CatView + "' AND Source = '" + SourceViewV[0] + "'");
                int i = 0;
                while (i < F.Count())
                {
                    Filtrada.ImportRow(F[i]);
                    i++;
                }
                if (SourceViewV[1] != "-")
                {
                    F = Inicial.Select("Category = '" + CatView + "' AND Source = '" + SourceViewV[1] + "'");
                    i = 0;
                    while (i < F.Count())
                    {
                        Filtrada.ImportRow(F[i]);
                        i++;
                    }
                }
            }
            DataView ret = Filtrada.DefaultView;

            previousBTT.Visible = true;
            nextBTN.Visible = true;
            Max.Text = Convert.ToString(this.DataTable1000.Count());

            return ret;
        }

        //filtrar per nom (només una taula gran)
        private DataView FiltrarID()
        {
            DataTable Final = new DataTable();
            Final.Columns.Add("Category");
            Final.Columns.Add("Source");
            Final.Columns.Add("Target ID");
            Final.Columns.Add("Track Number");
            Final.Columns.Add("Vehicle Fleet");
            Final.Columns.Add("DataBlock Id");
            numDTable = 0;
            string Sort = "";
            while (numDTable < DataTable1000.Count())
            {
                DataTable Input = FiltrarCatSour().ToTable();
                DataRow[] F = new DataRow[999];
                F = Input.Select("[Target ID] LIKE '" + IdView + "%'");
                Sort = "[Target ID]";
                if (F.Count() == 0)
                {
                    F = Input.Select("[Track Number] LIKE '" + IdView + "%'");
                    Sort = "[Track Number]";
                }
                int j = 0;
                while (j < F.Count())
                {
                    Final.ImportRow(F[j]);
                    j++;
                }
                numDTable++;
            }
            numDTable = 0;

            DataView ret = Final.DefaultView;
            //ret.Sort = Sort;

            Max.Text = "1";
            previousBTT.Visible = false;
            nextBTN.Visible = false;

            return ret;
        }

        private void TargetGroup()
        {
            //Creem targets i poblem datagrid targets
            PGB1.Value = 1;
            PGB1.Refresh();
            DataInf.Text = "Grouping Targets...";
            DataInf.Refresh();
            List<DataBlock> Copia = DataBlockList.ToList();
            PGB1.Maximum = Copia.Count();

            //Bucle ADS-B
            bool adsb_fin = false; int i = 0;
            List<DataBlock> ADSB = Copia.Where(x => x.From == "ADS-B").ToList();
            while ((Copia.Count != 0)&&(adsb_fin == false)&& (ADSB.Count != 0))
            {
                DataBlock Evaluat = ADSB.First();

                List<DataBlock> Filtrados = new List<DataBlock>();

                if (Evaluat.T_ID != "-")
                {
                    Filtrados = Copia.Where(x => x.T_ID == Evaluat.T_ID).ToList();
                    Copia.RemoveAll(x => x.T_ID == Evaluat.T_ID);
                    ADSB.RemoveAll(x => x.T_ID == Evaluat.T_ID);
                }
                else
                {
                    Filtrados = Copia.Where(x => x.T_Number == Evaluat.T_Number).ToList();
                    Copia.RemoveAll(x => x.T_Number == Evaluat.T_Number);
                    ADSB.RemoveAll(x => x.T_Number == Evaluat.T_Number);
                }

                TargetList.Add(new Target(Filtrados));

                PGB1.Step = Filtrados.Count();
                PGB1.PerformStep();

                TargetTable.Rows.Add(TargetList.Last().StringLin());
                i++;
                if (ADSB.Count == 0)
                    adsb_fin = true;
            }

            //Bucle SMR
            List<DataBlock> SMR = Copia.Where(x => x.From == "SMR").ToList();
            bool smr = false;
            while ((smr == false)&& (SMR.Count() != 0)) 
            {
                DataBlock Evaluat = SMR.First();

                List<DataBlock> Filtrados = SMR.Where(x => x.T_Number == Evaluat.T_Number).ToList();
                TargetList.Add(new Target(Filtrados));

                SMR.RemoveAll(x => x.T_Number == Evaluat.T_Number);

                PGB1.Step = Filtrados.Count();
                PGB1.PerformStep();

                TargetTable.Rows.Add(TargetList.Last().StringLin());

                if (SMR.Count == 0)
                    smr = true;
            }

            //Bucle Multi
            bool multi_fin = false;
            List<DataBlock> Multi = Copia.Where(x => x.From == "Multi.").ToList();
            while ((multi_fin == false) && (Multi.Count() != 0)) 
            {
                DataBlock Evaluat = Multi.First();

                List<DataBlock> Filtrados = new List<DataBlock>();
                if (Evaluat.T_ID != "-")
                {
                    Filtrados = Multi.Where(x => x.T_ID == Evaluat.T_ID).ToList();
                    Multi.RemoveAll(x => x.T_ID == Evaluat.T_ID);
                }
                else
                {
                    Filtrados = Multi.Where(x => x.T_Number == Evaluat.T_Number).ToList();
                    Multi.RemoveAll(x => x.T_Number == Evaluat.T_Number);
                }
                        
                TargetList.Add(new Target(Filtrados));

                PGB1.Step = Filtrados.Count();
                PGB1.PerformStep();

                TargetTable.Rows.Add(TargetList.Last().StringLin());
     
                if (Multi.Count == 0)
                    multi_fin = true;
            }
            TargetShow_Act();
        }

        //Actualització de DGV DataBlocks
        private void DataBlocksDGV_Act()
        {
            DataInf.Text = "Loading...";
            DataInf.ForeColor = Color.DarkGray;
            DataInf.Refresh();
            DataView Filtrada = FiltrarCatSour();
            if (IdView != "All")
                Filtrada = FiltrarID();

            this.Cursor = Cursors.WaitCursor;
            DataBlocksAll.DataSource = Filtrada.ToTable();
            DataBlocksAll.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            this.Cursor = Cursors.Default;
            DataInf.Text = "Data loaded";
            DataInf.ForeColor = Color.Green;
            DataInf.Refresh();
        }

        //Actualitzar el DGV dels targets
        private void TargetShow_Act()
        {
            DataTable NewTargetTable = new DataTable();
            NewTargetTable.Columns.Add("Target ID");
            NewTargetTable.Columns.Add("Track Number");
            NewTargetTable.Columns.Add("Vehicle Fleet");
            NewTargetTable.Columns.Add("Source");
            NewTargetTable.Columns.Add("N. DataBlocks");

            if (IdView == "All")
                NewTargetTable = TargetTable;
            else
            {
                DataRow[] F = new DataRow[999];
                if(IdView != "-")
                    F = TargetTable.Select("[Target ID] LIKE '" + IdView + "%'");
                if (F.Count() == 0)
                {
                    F = TargetTable.Select("[Track Number] LIKE '" + IdView + "%'");
                }
                int j = 0;
                while (j < F.Count())
                {
                    NewTargetTable.ImportRow(F[j]);
                    j++;
                }
            }

            TargetsShow.DataSource = NewTargetTable;
        }

        //Actualització de DGV DataBlockView
        private void DataBlockViwerDGV_Act(DataBlock Element)
        {
            DataBlocViwer.Columns.Clear();
            DataBlocViwer.Rows.Clear();
            DataBlocViwer.ColumnCount = 3;
            DataBlocViwer.Columns[0].Name = "Item name";
            DataBlocViwer.Columns[1].Name = "Message (DeCod)";
            DataBlocViwer.Columns[2].Name = "Units";


            //Obrim els datafields
            int i = 0;
            while (i < Element.DataFields.Count())
            {
                DataField Visio = Element.DataFields[i];
                if (Visio.DeCode.Count != 0)
                {
                    DataBlocViwer.Rows.Add(Visio.LinVectNom());
                    int h = 1;
                    while (h < Visio.DeCode.Count)
                    {
                        DataBlocViwer.Rows.Add(Visio.LinVect(h));
                        h++;
                    }
                }
                i++;
            }

            DataBlocViwer.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
        }

        //BTN sortida
        private void Exit_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void pictureBox1_MouseHover(object sender, EventArgs e)
        {
            //pictureBox1.Image = Image.FromFile("S3(hover).png");
        }
        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            //pictureBox1.Image = Image.FromFile("S3.png");
        }

        //BTN Load
        private void LoadBTN_MouseHover(object sender, EventArgs e)
        {
            LoadBTN.BackColor = Color.FromArgb(0, 66, 108);
        }
        private void LoadBTN_MouseLeave(object sender, EventArgs e)
        {
            LoadBTN.BackColor = Color.FromArgb(209, 222, 230);
        }
        private void LoadBTN_Click(object sender, EventArgs e)
        {
            TargetList = new List<Target>();
            TargetTable = new DataTable();
            TargetTable.Columns.Add("Target ID");
            TargetTable.Columns.Add("Track Number");
            TargetTable.Columns.Add("Vehicle Fleet");
            TargetTable.Columns.Add("Source");
            TargetTable.Columns.Add("N. DataBlocks");

            DataInf.Text = "Loading Data...";
            DataInf.ForeColor = Color.DarkGray;
            pictureBox5.BringToFront();
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                InitialDirectory = @"D:\",
                Title = "Browse Files",

                CheckFileExists = true,
                CheckPathExists = true,
                Filter = "ast files (*.ast)|*.ast",
                DefaultExt = ".ast",
                FilterIndex = 2,
                RestoreDirectory = true,

                ReadOnlyChecked = true,
                ShowReadOnly = true
            };


            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                TextVisorBTN.BorderStyle = BorderStyle.None;
                PanelControlSuperior.Visible = false;
                TextVisorPanel.Visible = false;
                this.DataBlockList = new List<DataBlock>();//lista con paquetes separados
                this.DataTable1000 = new List<DataTable>();
                this.Cursor = Cursors.WaitCursor;
                PGB1.Visible = true;
                byte[] Bytes = File.ReadAllBytes(openFileDialog.FileName); //vector bytes todos juntos, sin separar ni nada
                CatLib[] Cat = Hertz_Hülsmeyer.CarregarCategories();
                PGB1.Maximum = Bytes.Count();
                int H = 0; //Contador delements no inscrits a CAT10 i CAT21
                PGB1.Value = 1;

                bool Cat10 = false; bool Cat21 = false; bool Psr = false; bool Multi = false;

                DataTable DT = new DataTable();
                DT.Columns.Add("Category");
                DT.Columns.Add("Source");
                DT.Columns.Add("Target ID");
                DT.Columns.Add("Track Number");
                DT.Columns.Add("Vehicle Fleet");
                DT.Columns.Add("DataBlock Id");

                int i = 0; int numDT = 0;
                while (i < Bytes.Count())
                {
                    //Obtenirm dades inicials del block
                    string CAT = Bytes[i].ToString();
                    if (CAT == "10")
                        Cat10 = true;
                    else
                        Cat21 = true;
                    int Long = Convert.ToInt32(Bytes[i + 2].ToString());
                    Queue<byte> BytesSave = new Queue<byte>();


                    //Introduim tots els bytes dins d'una queue per crear el DataBlock
                    int j = 0;
                    while (j < Long)
                    {
                        BytesSave.Enqueue(Bytes[j + i]); //Afegim a la llista local
                        j++;
                    }

                    //Si es de la categoria desitjada l'enllistem a la llista general
                    if ((CAT == "10") || (CAT == "21"))
                    {
                        DataBlockList.Add(new DataBlock(BytesSave, Cat, DataBlockList.Count())); //Afegim a la llista general

                        if (DataBlockList.Last().From == "Multi.")
                            Multi = true;
                        else if (DataBlockList.Last().From == "SMR")
                            Psr = true;
                            numDT++;
                        if (numDT == 999)
                        {
                            this.DataTable1000.Add(DT);
                            DT = new DataTable();
                            DT.Columns.Add("Category");
                            DT.Columns.Add("Source");
                            DT.Columns.Add("Target ID");
                            DT.Columns.Add("Track Number");
                            DT.Columns.Add("Vehicle Fleet");
                            DT.Columns.Add("DataBlock Id");
                            numDT = 0;
                        }

                        DT.Rows.Add(DataBlockList.Last().StringLin());
                    }
                    else
                    {
                        H++;
                    }

                    i = i + j;
                    PGB1.Step = j;
                    PGB1.PerformStep();
                }

                //Agrupar Targets
                TargetGroup();

                this.DataTable1000.Add(DT);
                this.Cursor = Cursors.Default;
                PGB1.Visible = false;
                PanelControlSuperior.Visible = true;
                DataInf.Text = "Data loaded";
                DataInf.ForeColor = Color.Green;
                FileName.Text = "(File: " + openFileDialog.FileName + ")";
                FileName.Visible = true;
                Current.Text = "1";
                Max.Text = Convert.ToString(this.DataTable1000.Count());
                TextVisorPanel.BringToFront();

                if ((Cat10 == true) && (Cat21 == true))
                {
                    Cat010BTN.Visible = true;
                    AllCatBTN.Visible = true;
                    Cat021BTN.Visible = true;
                    AdsBTN.Visible = true;
                    if ((Multi == true) && (Psr == true))
                    {
                        MultiBTN.Visible = true;
                        AllSBTN.Visible = true;
                        PSRBTN.Visible = true;
                    }
                    else if (Multi == true)
                    {
                        MultiBTN.Visible = true;
                        AllSBTN.Visible = true;
                        PSRBTN.Visible = false;
                    }
                    else
                    {
                        MultiBTN.Visible = false;
                        AllSBTN.Visible = true;
                        PSRBTN.Visible = true;
                    }
                }
                else if (Cat10 == true)
                {
                    Cat010BTN.Visible = true;
                    AllCatBTN.Visible = false;
                    Cat021BTN.Visible = false;
                    AdsBTN.Visible = false;
                    Cat010BTN.BorderStyle = BorderStyle.FixedSingle;
                    if ((Multi == true) && (Psr == true))
                    {
                        MultiBTN.Visible = true;
                        PSRBTN.Visible = true;
                        AllSBTN.Visible = true;
                    }
                    else if (Multi == true)
                    {
                        MultiBTN.Visible = true;
                        PSRBTN.Visible = false;
                        AllSBTN.Visible = false;
                        MultiBTN.BorderStyle = BorderStyle.FixedSingle;
                    }
                    else
                    {
                        MultiBTN.Visible = false;
                        PSRBTN.Visible = true;
                        AllSBTN.Visible = false;
                        PSRBTN.BorderStyle = BorderStyle.FixedSingle;
                    }
                }
                else
                {
                    Cat010BTN.Visible = false;
                    MultiBTN.Visible = false;
                    PSRBTN.Visible = false;
                    AllCatBTN.Visible = false;
                    Cat021BTN.Visible = true;
                    AdsBTN.Visible = true;
                    AllSBTN.Visible = false;
                    Cat021BTN.BorderStyle = BorderStyle.FixedSingle;
                    AdsBTN.BorderStyle = BorderStyle.FixedSingle;
                }
            }
            else
            {
                DataInf.Text = "No data loaded";
                DataInf.ForeColor = Color.Red;
                TextVisorPanel.BringToFront();
                if (DataBlockList.Count != 0)
                {
                    DataInf.Text = "Data loaded";
                    DataInf.ForeColor = Color.Green;
                }
            }
        }

        //Moure finestra
        private void BarraSuperior_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        //BTN TextVisor
        private void TextVisorBTN_Click(object sender, EventArgs e)
        {
            DataBlocksDGV_Act();
            TextVisorPanel.Visible = true;
            PanelSources1.Visible = true;
            MapVisorPanel.Visible = false;
            TempsPanel.Visible = false;
            TextVisorBTN.BorderStyle = BorderStyle.FixedSingle;
            MapVisor.BorderStyle = BorderStyle.None;
        }
        private void TextVisorBTN_MouseHover(object sender, EventArgs e)
        {
            TextVisorBTN.BackColor = Color.FromArgb(0, 66, 108);
        }
        private void TextVisorBTN_MouseLeave(object sender, EventArgs e)
        {
            TextVisorBTN.BackColor = Color.FromArgb(209, 222, 230);
        }

        //BTN MapVisor
        private void MapVisor_Click(object sender, EventArgs e)
        {
            TextVisorBTN.BorderStyle = BorderStyle.None;
            MapVisor.BorderStyle = BorderStyle.FixedSingle;
            TextVisorPanel.Visible = false;
            PanelSources1.Visible = true;
            TempsPanel.Visible = true;
            MapVisorPanel.Visible = true;
            MapVisorPanel.BringToFront();
        }
        private void MapVisor_MouseHover(object sender, EventArgs e)
        {
            MapVisor.BackColor = Color.FromArgb(0, 66, 108);
        }
        private void MapVisor_MouseLeave(object sender, EventArgs e)
        {
            MapVisor.BackColor = Color.FromArgb(209, 222, 230);
        }

        //BTN Cat010
        private void Cat010BTN_Click(object sender, EventArgs e)
        {
            CatView = "10";
            Cat010BTN.BorderStyle = BorderStyle.FixedSingle;
            Cat021BTN.BorderStyle = BorderStyle.None;
            AllCatBTN.BorderStyle = BorderStyle.None;
            CAT10 = true;

            DataBlocksDGV_Act();
        }
        private void Cat010BTN_MouseHover(object sender, EventArgs e)
        {
            Cat010BTN.BackColor = Color.FromArgb(0,66, 108);

        }
        private void Cat010BTN_MouseLeave(object sender, EventArgs e)
        {
            Cat010BTN.BackColor = Color.FromArgb(209, 222, 230);
        }

        //BTN Cat021
        private void Cat021BTN_Click(object sender, EventArgs e)
        {
            CatView = "21";
            Cat010BTN.BorderStyle = BorderStyle.None;
            Cat021BTN.BorderStyle = BorderStyle.FixedSingle;
            AllCatBTN.BorderStyle = BorderStyle.None;
            CAT10 = false;

            DataBlocksDGV_Act();
        }
        private void Cat021BTN_MouseHover(object sender, EventArgs e)
        {
            Cat021BTN.BackColor = Color.FromArgb(0, 66, 108);
        }
        private void Cat021BTN_MouseLeave(object sender, EventArgs e)
        {
            Cat021BTN.BackColor = Color.FromArgb(209, 222, 230);
        }

        //BTN AllCat
        private void AllCatBTN_Click(object sender, EventArgs e)
        {
            CatView = "All";
            Cat010BTN.BorderStyle = BorderStyle.None;
            Cat021BTN.BorderStyle = BorderStyle.None;
            AllCatBTN.BorderStyle = BorderStyle.FixedSingle;
            CAT10 = false;

            DataBlocksDGV_Act();
        }
        private void AllCatBTN_MouseHover(object sender, EventArgs e)
        {
            AllCatBTN.BackColor = Color.FromArgb(0, 66, 108);
        }
        private void AllCatBTN_MouseLeave(object sender, EventArgs e)
        {
            AllCatBTN.BackColor = Color.FromArgb(209, 222, 230);
        }

        //BTN Next
        private void nextBTN_Click(object sender, EventArgs e)
        {
            if (numDTable < DataTable1000.Count())
                numDTable++;
            DataBlocksDGV_Act();
            Current.Text = Convert.ToString(numDTable + 1);
        }
        private void nextBTN_MouseHover(object sender, EventArgs e)
        {
            nextBTN.BackColor = Color.FromArgb(0, 66, 108);
        }
        private void nextBTN_MouseLeave(object sender, EventArgs e)
        {
            nextBTN.BackColor = Color.FromArgb(209, 222, 230);
        }

        //BTN previous
        private void previousBTT_Click(object sender, EventArgs e)
        {
            if (numDTable != 0)
                numDTable--;
            DataBlocksDGV_Act();
            Current.Text = Convert.ToString(numDTable + 1);
        }
        private void previousBTT_MouseHover(object sender, EventArgs e)
        {
            previousBTT.BackColor = Color.FromArgb(0, 66, 108);
        }
        private void previousBTT_MouseLeave(object sender, EventArgs e)
        {
            previousBTT.BackColor = Color.FromArgb(209, 222, 230);
        }

        //BTN Multi (S)
        private void MultiBTN_Click(object sender, EventArgs e)
        {
            if ((!MULT) && (SMR) && (ADSB))
            {
                if ((MULT) && (SMR) && (CatView == "10"))
                {
                    CatView = "All";
                }

                MULT = false;
                SMR = false;
                ADSB = false;

                SourceViewV[0] = "All";
                SourceViewV[1] = "-";

                MultiBTN.BorderStyle = BorderStyle.None;
                PSRBTN.BorderStyle = BorderStyle.None;
                AdsBTN.BorderStyle = BorderStyle.None;
                AllSBTN.BorderStyle = BorderStyle.FixedSingle;
            }
            else if ((!MULT) && (!SMR))
            {
                if (SourceViewV[0] == "All")
                {
                    SourceViewV[0] = "Multi.";
                }
                else if (SourceViewV[0] != "Multi.")
                {
                    SourceViewV[1] = "Multi.";
                }
                MultiBTN.BorderStyle = BorderStyle.FixedSingle;
                AllSBTN.BorderStyle = BorderStyle.None;
                MULT = true;
            }
            else if (MULT)
            {
                if (SMR)
                {
                    SourceViewV[0] = "SMR";
                    SourceViewV[1] = "-";
                    CatView = "All";
                }
                else
                {
                    if (SourceViewV[0] == "Multi.")
                    {
                        SourceViewV[0] = "-";
                    }
                    else if (SourceViewV[1] == "Multi.")
                    {
                        SourceViewV[1] = "-";
                    }
                }
                MultiBTN.BorderStyle = BorderStyle.None;
                MULT = false;

                if ((!SMR) && (!ADSB))
                {
                    SourceViewV[0] = "All";
                    SourceViewV[1] = "-";
                    AllSBTN.BorderStyle = BorderStyle.FixedSingle;
                }
            }
            else if ((!MULT) && (SMR))
            {
                if(CatView != "21")
                {
                    SourceViewV[0] = "All";
                    SourceViewV[1] = "-";
                    CatView = "10";
                }
                if (CAT10)
                {
                    MULT = false;
                    SMR = false;
                    ADSB = false;

                    MultiBTN.BorderStyle = BorderStyle.None;
                    PSRBTN.BorderStyle = BorderStyle.None;
                    AdsBTN.BorderStyle = BorderStyle.None;
                    AllSBTN.BorderStyle = BorderStyle.FixedSingle;
                }
                else
                {
                    MULT = true;
                    MultiBTN.BorderStyle = BorderStyle.FixedSingle;
                }
            }

            DataBlocksDGV_Act();
        }
        private void MultiBTN_MouseLeave(object sender, EventArgs e)
        {
            MultiBTN.BackColor = Color.FromArgb(209, 222, 230);
        }
        private void MultiBTN_MouseHover(object sender, EventArgs e)
        {

            MultiBTN.BackColor = Color.FromArgb(0, 66, 108);
        }

        //BTN Psr (S)
        private void PSRBTN_Click(object sender, EventArgs e)
        {
            if ((MULT) && (!SMR) && (ADSB))
            {
                if ((MULT) && (SMR) && (CatView == "10"))
                {
                    CatView = "All";
                }

                MULT = false;
                SMR = false;
                ADSB = false;

                SourceViewV[0] = "All";
                SourceViewV[1] = "-";

                MultiBTN.BorderStyle = BorderStyle.None;
                PSRBTN.BorderStyle = BorderStyle.None;
                AdsBTN.BorderStyle = BorderStyle.None;
                AllSBTN.BorderStyle = BorderStyle.FixedSingle;
            }
            
            else if ((!MULT) && (!SMR))
            {
                if (SourceViewV[0] == "All")
                {
                    SourceViewV[0] = "SMR";
                }
                else if (SourceViewV[0] != "SMR")
                {
                    SourceViewV[1] = "SMR";
                }
                PSRBTN.BorderStyle = BorderStyle.FixedSingle;
                AllSBTN.BorderStyle = BorderStyle.None;
                SMR = true;
            }
            else if(SMR)
            {
                if (MULT)
                {
                    SourceViewV[0] = "Multi.";
                    SourceViewV[1] = "-";
                    CatView = "All";
                }
                else
                {
                    if (SourceViewV[0] == "SMR")
                    {
                        SourceViewV[0] = "-";
                    }
                    else if (SourceViewV[1] == "SMR")
                    {
                        SourceViewV[1] = "-";
                    }
                }
                
                PSRBTN.BorderStyle = BorderStyle.None;
                SMR = false;

                if ((!MULT) && (!ADSB))
                {
                    SourceViewV[0] = "All";
                    SourceViewV[1] = "-";
                    AllSBTN.BorderStyle = BorderStyle.FixedSingle;
                    SMR = false;
                }
            }
            else if ((MULT) && (!SMR))
            {
                if (CatView != "21")
                {
                    SourceViewV[0] = "All";
                    SourceViewV[1] = "-";
                    CatView = "10";
                }
                if (CAT10)
                {
                    MULT = false;
                    SMR = false;
                    ADSB = false;

                    MultiBTN.BorderStyle = BorderStyle.None;
                    PSRBTN.BorderStyle = BorderStyle.None;
                    AdsBTN.BorderStyle = BorderStyle.None;
                    AllSBTN.BorderStyle = BorderStyle.FixedSingle;
                }
                else
                {
                    SMR = true;
                    PSRBTN.BorderStyle = BorderStyle.FixedSingle;
                }
            }

            DataBlocksDGV_Act();
        }
        private void PSRBTN_MouseHover(object sender, EventArgs e)
        {
            PSRBTN.BackColor = Color.FromArgb(0, 66, 108);
        }
        private void PSRBTN_MouseLeave(object sender, EventArgs e)
        {
            PSRBTN.BackColor = Color.FromArgb(209, 222, 230);
        }

        //BTN Ads (S)
        private void AdsBTN_Click(object sender, EventArgs e)
        {
            //this.SourView = "ADS-B";
            //MultiBTN.BorderStyle = BorderStyle.None;
            //PSRBTN.BorderStyle = BorderStyle.None;
            //AdsBTN.BorderStyle = BorderStyle.FixedSingle;
            //AllSBTN.BorderStyle = BorderStyle.None;

            if ((MULT) && (SMR) && (!ADSB))
            {
                if ((MULT) && (SMR) && (CatView == "10") && (!CAT10))
                {
                    CatView = "All";
                }

                MULT = false;
                SMR = false;
                ADSB = false;

                SourceViewV[0] = "All";
                SourceViewV[1] = "-";

                MultiBTN.BorderStyle = BorderStyle.None;
                PSRBTN.BorderStyle = BorderStyle.None;
                AdsBTN.BorderStyle = BorderStyle.None;
                AllSBTN.BorderStyle = BorderStyle.FixedSingle;
            }
            else if (!ADSB)
            {
                if (SourceViewV[0] == "All")
                {
                    SourceViewV[0] = "ADS-B";
                }
                else if (SourceViewV[0] != "ADS-B")
                {
                    SourceViewV[1] = "ADS-B";
                }
                AdsBTN.BorderStyle = BorderStyle.FixedSingle;
                AllSBTN.BorderStyle = BorderStyle.None;
                ADSB = true;
            }
            else if (ADSB)
            {
                if (SourceViewV[0] == "ADS-B")
                {
                    SourceViewV[0] = "-";
                }
                else if (SourceViewV[1] == "ADS-B")
                {
                    SourceViewV[1] = "-";
                }
                AdsBTN.BorderStyle = BorderStyle.None;
                ADSB = false;

                if ((!MULT) && (!SMR))
                {
                    SourceViewV[0] = "All";
                    SourceViewV[1] = "-";
                    AllSBTN.BorderStyle = BorderStyle.FixedSingle;
                }
            }

            DataBlocksDGV_Act();
        }
        private void AdsBTN_MouseHover(object sender, EventArgs e)
        {
            AdsBTN.BackColor = Color.FromArgb(0, 66, 108);
        }
        private void AdsBTN_MouseLeave(object sender, EventArgs e)
        {
            AdsBTN.BackColor = Color.FromArgb(209, 222, 230);
        }

        //BTN All (S)
        private void AllSBTN_Click(object sender, EventArgs e)
        {
            if ((MULT) && (SMR) && (CatView == "10"))
            {
                CatView = "All";
            }

            SourceViewV[0] = "All";
            SourceViewV[1] = "-";
            MULT = false;
            SMR = false;
            ADSB = false;
            MultiBTN.BorderStyle = BorderStyle.None;
            PSRBTN.BorderStyle = BorderStyle.None;
            AdsBTN.BorderStyle = BorderStyle.None;
            AllSBTN.BorderStyle = BorderStyle.FixedSingle;

            DataBlocksDGV_Act();
        }
        private void AllSBTN_MouseLeave(object sender, EventArgs e)
        {
            AllSBTN.BackColor = Color.FromArgb(209, 222, 230);
        }
        private void AllSBTN_MouseHover(object sender, EventArgs e)
        {
            AllSBTN.BackColor = Color.FromArgb(0, 66, 108);
        }

        //Obrir info al DGV DataBlock view
        private void DataBlocksAll_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            DataBlocksAll.CurrentRow.Selected = true;
            this.Cursor = Cursors.WaitCursor;
            if(e.RowIndex >= 0)
            {
                string ID_I = DataBlocksAll.Rows[e.RowIndex].Cells["DataBlock Id"].FormattedValue.ToString();
                int i = 0; bool en = false;
                while ((i < DataBlockList.Count()) && (en == false))
                {
                    if (DataBlockList[i].ID_Intern == ID_I)
                    {
                        DataBlockViwerDGV_Act(DataBlockList[i]);
                        en = true;
                    }
                    i++;
                }
                this.Cursor = Cursors.Default;
            }
        }

        //BTN Buscar + TextBox Buscar
        private void BuscarBTN_Click(object sender, EventArgs e)
        {
            DataInf.Text = "Loading...";
            DataInf.ForeColor = Color.DarkGray;
            DataInf.Refresh();
            this.Cursor = Cursors.WaitCursor;
            this.IdView = Buscar.Text;
            if (IdView == "")
                this.IdView = "All";
            DataBlocksDGV_Act();
            TargetShow_Act();
            this.Cursor = Cursors.Default;
            DataInf.Text = "Data loaded";
            DataInf.ForeColor = Color.Green;
        }
        private void BuscarBTN_MouseHover(object sender, EventArgs e)
        {
            BuscarBTN.BackColor = Color.FromArgb(0, 66, 108);
        }
        private void BuscarBTN_MouseLeave(object sender, EventArgs e)
        {
            BuscarBTN.BackColor = Color.FromArgb(209, 222, 230);
        }
        private void Buscar_TextChanged(object sender, EventArgs e)
        {
            if (Buscar.Text == "")
            {
                this.IdView = "All";
                DataBlocksDGV_Act();
                TargetShow_Act();
            }
        }

        //BTN target
        private void TargetBTN_Click(object sender, EventArgs e)
        {
            this.IdView = "All";
            DataBlocksDGV_Act();
            TargetShow_Act();
            Buscar.Text = "";

            if (TargetsShow.Visible == false)
            {
                TargetBTN.BorderStyle = BorderStyle.FixedSingle;
                TargetBTN.BackColor = Color.FromArgb(0, 66, 108);
                TargetsShow.Visible = true;
                DataBlocksAll.Visible = false;
                nextBTN.Visible = false;
                previousBTT.Visible = false;
                label13.Visible = false;
                Max.Visible = false;
                Current.Visible = false;
                TargetShow_Act();
            }
            else
            {
                TargetBTN.BorderStyle = BorderStyle.None;
                TargetBTN.BackColor = Color.FromArgb(209, 222, 230);
                TargetsShow.Visible = false;
                DataBlocksAll.Visible = true;
                nextBTN.Visible = true;
                previousBTT.Visible = true;
                label13.Visible = true;
                Max.Visible = true;
                Current.Visible = true;

                NamT.Text = "D.Blocks";

            }
        }

        //private void Export_Click(object sender, EventArgs e)
        //{
        //    string ID = Buscar.Text;
        //    System.IO.StreamWriter file = new System.IO.StreamWriter("" + ID + ".txt");
        //    file.Close();

        //    //Busquem target
        //    List<Target> Encontrado = TargetList.Where(x => x.T_ID == ID).ToList();
        //    if (Encontrado.Count() == 0)
        //    {
        //        Encontrado = TargetList.Where(x => x.T_Number == ID).ToList();
        //    }

        //    if (Encontrado.Count() != 0)
        //    {
        //        StreamWriter W = new StreamWriter("" + ID + ".txt");
        //        int Max = Encontrado[0].CoordenadesADSB.Count();
        //        if (Max != 0)
        //        {
        //            W.WriteLine(Max);
        //            foreach (Coordenada C in Encontrado[0].CoordenadesADSB)
        //            {
        //                W.WriteLine(string.Join("_", C.Retrun()));
        //            }
        //            W.Close();
        //        }
        //        else
        //        {
        //            Max = Encontrado[0].CoordenadesMULTI.Count();
        //            if (Max != 0)
        //            {
        //                W.WriteLine(Max);
        //                foreach (Coordenada C in Encontrado[0].CoordenadesMULTI)
        //                {
        //                    W.WriteLine(string.Join("_", C.Retrun()));
        //                }
        //                W.Close();
        //            }
        //            else
        //            {
        //                Max = Encontrado[0].CoordenadesSMR.Count();
        //                if (Max != 0)
        //                {
        //                    W.WriteLine(Max);
        //                    foreach (Coordenada C in Encontrado[0].CoordenadesSMR)
        //                    {
        //                        W.WriteLine(string.Join("_", C.Retrun()));
        //                    }
        //                    W.Close();
        //                }
        //            }
        //        }
        //        MessageBox.Show("Exported");
        //    }
        //}

        private void TargetsShow_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            DataInf.Text = "Loading...";
            DataInf.ForeColor = Color.DarkGray;
            DataInf.Refresh();
            string ID = DataBlocksAll.Rows[e.RowIndex].Cells["Target ID"].FormattedValue.ToString();
            if(ID == "-")
                ID = DataBlocksAll.Rows[e.RowIndex].Cells["Track Number"].FormattedValue.ToString();
            this.IdView = ID;
            if (IdView == "")
                this.IdView = "All";
            DataBlocksDGV_Act();
            TargetShow_Act();

            Buscar.Text = ID;

            TargetBTN.BorderStyle = BorderStyle.None;
            TargetBTN.BackColor = Color.FromArgb(209, 222, 230);
            TargetsShow.Visible = false;
            DataBlocksAll.Visible = true;
            nextBTN.Visible = true;
            previousBTT.Visible = true;
            label13.Visible = true;
            Max.Visible = true;
            Current.Visible = true;

            this.Cursor = Cursors.Default;
            DataInf.Text = "Data loaded";
            DataInf.ForeColor = Color.Green;

            Max.Text = "1";
            previousBTT.Visible = false;
            nextBTN.Visible = false;
        }

        //Mapa
        //Marcador de la coordenada (TESTE MAPA1)
        private void Map_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            double lat = Map.FromLocalToLatLng(e.X, e.Y).Lat;
            double lon = Map.FromLocalToLatLng(e.X, e.Y).Lng;
            GMapOverlay Overlay = new GMapOverlay();
            GMarkerGoogle M = new GMarkerGoogle(new PointLatLng(0, 0), GMarkerGoogleType.green);
            M.Position = new PointLatLng(lat, lon);
            M.ToolTipMode = MarkerTooltipMode.Always;
            M.ToolTipText = string.Format("LAT: " + lat + "; LNG: " + lon + "");
            Overlay.Markers.Add(M);
            Map.Overlays.Add(Overlay);
        }

        //Control de temps
        private void Timer_Tick(object sender, EventArgs e)
        {
            Temps = Temps.Add(interval);
            TempsLBL.Text = Temps.ToString("c");
            TempsLBL.Refresh();


        }

        private void PlayPause_Click(object sender, EventArgs e)
        {
            if (!Play)
            {
                Play = true;
                PlayPause.Image = Image.FromFile("Pause(II).png");
                PlayPause.Refresh();
                Timer.Enabled = true;
                Timer.Start();

                TempsLBL.Text = Temps.ToString("c");
                TempsLBL.Refresh();
            }
            else
            {
                Play = false;
                PlayPause.Image = Image.FromFile("Play(II).png");
                Timer.Stop();
            }
        }
        private void PlayPause_MouseHover(object sender, EventArgs e)
        {
            if (!Play)
                PlayPause.Image = Image.FromFile("Play(II).png");
            else
                PlayPause.Image = Image.FromFile("Pause(II).png");
        }
        private void PlayPause_MouseLeave(object sender, EventArgs e)
        {
            if (!Play)
                PlayPause.Image = Image.FromFile("Play(I).png");
            else
                PlayPause.Image = Image.FromFile("Pause (I).png");
        }

        private void Refresh_Click(object sender, EventArgs e)
        {
            Temps = new TimeSpan(8, 0, 0);
            TempsLBL.Text = Temps.ToString("c");
            TempsLBL.Refresh();
            Play = false;
            PlayPause.Image = Image.FromFile("Play(I).png");
            Timer.Stop();
        }
        private void Refresh_MouseHover(object sender, EventArgs e)
        {
            Refresh.Image = Image.FromFile("Refresh2(II).png");
        }
        private void Refresh_MouseLeave(object sender, EventArgs e)
        {
            Refresh.Image = Image.FromFile("Refresh2(1).png");
        }

        private void TempsLBL_Click(object sender, EventArgs e)
        {

        }

        private void MesV_MouseHover(object sender, EventArgs e)
        {
            MesV.Image = Image.FromFile("Mes(II).png");
        }
        private void MesV_MouseLeave(object sender, EventArgs e)
        {
            MesV.Image = Image.FromFile("Mes(I).png");
        }
        private void MesV_Click(object sender, EventArgs e)
        {
            if (velocitat == "x 1")
            {
                Timer.Interval = 750;
                velocitat = "x 2";
                Velo.Text = velocitat;
            }
            else if (velocitat == "x 2")
            {
                Timer.Interval = 500;
                velocitat = "x 3";
                Velo.Text = velocitat; ;
            }
            else if (velocitat == "x 3")
            {
                Timer.Interval = 250;
                velocitat = "x 4";
                Velo.Text = velocitat;
            }
            else if (velocitat == "x 4")
            {
                Timer.Interval = 100;
                velocitat = "x 5";
                Velo.Text = velocitat;
            }
            else if (velocitat == "x 5")
            {
                Timer.Interval = 10;
                velocitat = "x 10";
                Velo.Text = velocitat;
            }
        }

        private void MenysV_MouseHover(object sender, EventArgs e)
        {
            MenysV.Image = Image.FromFile("Menus(II).png");
        }
        private void MenysV_MouseLeave(object sender, EventArgs e)
        {
            MenysV.Image = Image.FromFile("Menus(I).png");
        }
        private void MenysV_Click(object sender, EventArgs e)
        {
            if (velocitat == "x 2")
            {
                Timer.Interval = 1000;
                velocitat = "x 1";
                Velo.Text = velocitat;
            }
            else if (velocitat == "x 3")
            {
                Timer.Interval = 750;
                velocitat = "x 2";
                Velo.Text = velocitat;
            }
            else if (velocitat == "x 4")
            {
                Timer.Interval = 500;
                velocitat = "x 3";
                Velo.Text = velocitat;
            }
            else if (velocitat == "x 5")
            {
                Timer.Interval = 250;
                velocitat = "x 4";
                Velo.Text = velocitat;
            }
            else if (velocitat == "x 10")
            {
                Timer.Interval = 100;
                velocitat = "x 5";
                Velo.Text = velocitat;
            }
        }
    }
}
