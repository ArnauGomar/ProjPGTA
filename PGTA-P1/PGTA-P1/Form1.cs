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
        List<DataBlock> DataBlockList = new List<DataBlock>();
        List<DataTable> DataTable1000;
        List<Target> TargetList = new List<Target>();
        DataTable TargetTable = new DataTable();
        int numDTable = 0;

        //Cats, sources y Ids
        string IdView = "All";

        bool SMR = false;
        bool MULT = false;
        bool ADSB = false;

        bool CAT10 = false;
        bool CAT21 = false;
        
        //GMaps
        double BCNLat = 41.2972361111;
        double BCNLon = 2.0783333333;
        int TrackTime = 0;

        //Timer sets
        TimeSpan interval = new TimeSpan(0, 0, 1);
        string velocitat = "x 1";
        bool Play = false;
        TimeSpan Temps = new TimeSpan(8, 00, 00);

        //Aparició de targets
        List<Target> ViewTargetListADSB = new List<Target>();
        List<Target> ViewTargetListMULTI = new List<Target>();
        List<Target> ViewTargetListSMR = new List<Target>();
        List<Target> ViewTargetListShow = new List<Target>();
        int countNOACTU = 0;
        string FromMarker = "";

        //Carregar imatges
        Bitmap[] Mark_Images = new Bitmap[5];
        Image[] But_Images = new Image[10];

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

            //Images vectors
            Mark_Images[0] = (Bitmap)Image.FromFile("Test.png");
            Mark_Images[1] = (Bitmap)Image.FromFile("Test2.png");
            Mark_Images[2] = (Bitmap)Image.FromFile("Test3.png");
            Mark_Images[3] = (Bitmap)Image.FromFile("MULTI.png");
            Mark_Images[4] = (Bitmap)Image.FromFile("SMR.png");

            But_Images[0] = Image.FromFile("Menus(I).png");
            But_Images[1] = Image.FromFile("Menus(II).png");
            But_Images[2] = Image.FromFile("Mes(I).png");
            But_Images[3] = Image.FromFile("Mes(II).png");
            But_Images[4] = Image.FromFile("Refresh2(1).png");
            But_Images[5] = Image.FromFile("Refresh2(II).png");
            But_Images[6] = Image.FromFile("Pause (I).png");
            But_Images[7] = Image.FromFile("Pause(II).png");
            But_Images[8] = Image.FromFile("Play(I).png");
            But_Images[9] = Image.FromFile("Play(II).png");
        }

        //Filtrar per categoria (te en compte la pagina DataTable)
        private DataView FiltrarCatSour()
        {
            DataTable Inicial = DataTable1000[numDTable];
            DataRow[] F = new DataRow[999];
            DataTable Filtrada = new DataTable();
            Filtrada.Columns.Add("Category");
            Filtrada.Columns.Add("Source");
            Filtrada.Columns.Add("Target ID");
            Filtrada.Columns.Add("Track Number");
            Filtrada.Columns.Add("Vehicle Fleet");
            Filtrada.Columns.Add("DataBlock Id");

            if (((!CAT10)&&(!CAT21)) && ((!SMR)&&(!MULT)&&(!ADSB)))
                Filtrada = Inicial;
            else if ((!CAT10) && (!CAT21)) //Radar no es all
            {
                string SouView = "";
                if (SMR)
                    SouView = "SMR";
                else if (MULT)
                    SouView = "Multi.";
                else if (ADSB)
                    SouView = "ADS-B";

                F = Inicial.Select("Source = '" + SouView + "'");
                int i = 0;
                while (i < F.Count())
                {
                    Filtrada.ImportRow(F[i]);
                    i++;
                }
            }
            else if ((!SMR) && (!MULT) && (!ADSB)) //Cat no es all
            {
                string CatView = "";
                if (CAT10)
                    CatView = "10";
                else
                    CatView = "21";

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
                string SouView = "";
                if (SMR)
                    SouView = "SMR";
                else if (MULT)
                    SouView = "Multi.";
                else if (ADSB)
                    SouView = "ADS-B";

                string CatView = "";
                if (CAT10)
                    CatView = "10";
                else
                    CatView = "21";

                F = Inicial.Select("Category = '" + CatView + "' AND Source = '" + SouView + "'");
                int i = 0;
                while (i < F.Count())
                {
                    Filtrada.ImportRow(F[i]);
                    i++;
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
            while (numDTable < DataTable1000.Count())
            {
                DataTable Input = FiltrarCatSour().ToTable();
                DataRow[] F = new DataRow[999];
                F = Input.Select("[Target ID] LIKE '" + IdView + "%'");
                if (F.Count() == 0)
                {
                    F = Input.Select("[Track Number] LIKE '" + IdView + "%'");
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

            Max.Text = "1";
            previousBTT.Visible = false;
            nextBTN.Visible = false;

            return ret;
        }

        private void GroupTNumb()
        {
            int h = 0;
            while (h < TargetList.Count())
            {
                if ((TargetList[h].CoordenadesSMR.Count == 1) && (TargetList[h].From == "SMR"))
                    TargetList.Remove(TargetList[h]);
                TargetList[h].Bucle_TNum();
                TargetList[h].Bucle_From();
                h++;
            }

            PGB1.Refresh();
            DataInf.Text = "Grouping Targets...";
            DataInf.Refresh();
            PGB1.Maximum = TargetList.Count();
            PGB1.Value = 1;
            PGB1.Step = 1;

            int i = 0;
            while(i< TargetList.Count())
            {
                if (TargetList[i].From != "SMR")
                {
                    int DT = TargetList[i].DataBlocks.Count();
                    //Estem en ADSB o MULTI, busquem semblances amb T_Number
                    int j = 0;
                    while (j < TargetList.Count())
                    {
                        if ((TargetList[j].From != "SMR") && (i != j))
                        {
                            int k = 0;
                            bool enc = false;
                            while ((k < TargetList[j].T_NumberMult.Count)&&(!enc))
                            {
                                string T_NumberTargetList = TargetList[j].T_NumberMult[k];
                                List<string> Iguals = TargetList[i].T_NumberMult.Where(x => x == T_NumberTargetList).ToList();

                                if (Iguals.Count != 0) //Hem trobat una igualtat, podem ingresar dades al target de copia
                                {
                                    TargetList[i].DataBlocks.AddRange(TargetList[j].DataBlocks);
                                    TargetList.Remove(TargetList[j]);
                                    
                                    enc = true;
                                }
                                k++;
                            }
                        }
                        j++;
                    }
                }

                TargetList[i].ReLoad();
                TargetTable.Rows.Add(TargetList[i].StringLin());
                PGB1.PerformStep();
                i++;
            }
        }

        private byte[] Load3AST()
        {
            byte[] Bytes1 = File.ReadAllBytes("201002-lebl-080001_adsb.ast");
            byte[] Bytes2 = File.ReadAllBytes("201002-lebl-080001_mlat.ast");
            byte[] Bytes3 = File.ReadAllBytes("201002-lebl-080001_smr.ast");
            List<byte> ret = new List<byte>();
            int i = 0;
            int j = 0;
            int k = 0;
            while (i < Bytes1.Count())
            {
                ret.Add(Bytes1[i]);
                i++;
            }
            
            while (j < Bytes2.Count())
            {
                ret.Add(Bytes2[j]);
                j++;
            }
            
            while (k < Bytes3.Count())
            {
                ret.Add(Bytes3[k]);
                k++;
            }

            return ret.ToArray();
        }

        //Actualització de DGV DataBlocks
        private void DataBlocksDGV_Act()
        {
            DataInf.Text = "Loading...";
            DataInf.ForeColor = Color.DarkGray;
            DataInf.Refresh();
            DataView Filtrada = FiltrarCatSour();
            if (Filtrada.Count == 0)
            {
                numDTable = 0;
                while ((numDTable < DataTable1000.Count) && (Filtrada.Count == 0))
                {
                    Filtrada = FiltrarCatSour();
                    numDTable++;
                }
                Current.Text = Convert.ToString(numDTable + 1);
            }
            if (IdView != "All")
                Filtrada = FiltrarID();

            this.Cursor = Cursors.WaitCursor;
            DataBlocksAll.DataSource = Filtrada.ToTable();
            DataBlocksAll.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            DataBlocksAll.RowHeadersVisible = false;
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
            TargetsShow.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

            if (IdView == "All")
                NewTargetTable = TargetTable;
            else
            {
                DataRow[] F = new DataRow[999];
                if (IdView != "-")
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
            TargetsShow.RowHeadersVisible = false;
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
            DataBlocViwer.RowHeadersVisible = false;
            DataBlocViwer.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

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
        }

        private void CurrentTargets_Act()
        {
            CurrenTargets.Columns.Clear();
            CurrenTargets.Rows.Clear();
            CurrenTargets.ColumnCount = 1;
            CurrenTargets.Columns[0].Name = "ID";
            CurrenTargets.RowHeadersVisible = false;

            foreach (Target T in ViewTargetListShow)
            {
                if (T.T_ID != "-")
                    CurrenTargets.Rows.Add(T.T_ID);
                else
                    CurrenTargets.Rows.Add(T.T_Number);

                if (T.From == "ADS-B ")
                    CurrenTargets.Rows[CurrenTargets.Rows.Count - 2].DefaultCellStyle.BackColor = Color.Red;
                else if (T.From == "Multi. ")
                    CurrenTargets.Rows[CurrenTargets.Rows.Count - 2].DefaultCellStyle.BackColor = Color.Green;
                else if (T.From == "SMR")
                {
                    CurrenTargets.Rows[CurrenTargets.Rows.Count - 2].DefaultCellStyle.BackColor = Color.Blue;
                    CurrenTargets.Rows[CurrenTargets.Rows.Count - 2].DefaultCellStyle.ForeColor = Color.White;
                }
                else
                    CurrenTargets.Rows[CurrenTargets.Rows.Count - 2].DefaultCellStyle.BackColor = Color.White;
            }
        }

        private Target BusquedaMetode(string Text)
        {
            List<Target> T = ViewTargetListShow.Where(x => x.T_ID == Text).ToList();
            if (T.Count() == 0)
                T = ViewTargetListShow.Where(x => x.T_Number == Text).ToList();
                
            if (T.Count != 0)
            {
                IdView = Text;
                CurrenTargets.Size = new Size(178, 256);
                ID_TXT.Text = Text; ID_TXT.Visible = true;
                ShowInfo.Visible = true;
                Center.Visible = true;
                Center.Checked = true;
                ShowInfo.Size = new Size(300, 140);

                ShowInfo.Columns.Clear();
                ShowInfo.Rows.Clear();
                ShowInfo.ColumnCount = 1;
                ShowInfo.Columns[0].Name = "Info";
                ShowInfo.ColumnHeadersVisible = false;
                ShowInfo.RowHeadersVisible = false;
                ShowInfo.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

                foreach (Target L in T)
                {
                    ShowInfo.Rows.Add("Infro from "+L.From+"");
                    ShowInfo.Rows.Add("T. number: "+L.StringLin()[1]+"");
                    ShowInfo.Rows.Add("Time on:"+L.Inici.ToString()+"");
                    ShowInfo.Rows.Add("Time off:"+L.Final.ToString()+"");
                    ShowInfo.Rows.Add("Type: "+L.V+"");
                    ShowInfo.Rows.Add();

                }
                SearchTxT2.Text = Text;

                return T[0];
            }
            else
            {
                MessageBox.Show("Target not found");
                return null;
            }
        }

        //BTN sortida
        private void Exit_Click(object sender, EventArgs e)
        {
            Timer.Enabled = false;
            Timer.Stop();
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

            Timer.Stop();
            Play = false;
            CatPanel.Visible = false;
            SouPanel.Visible = false;
            PanelControlSuperior.Visible = false;

            try
            {
                PlayPause.Image = But_Images[8];
            }
            catch (FileNotFoundException ex)
            {
                MessageBox.Show(ex.ToString());
            }

            

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
                Temps = new TimeSpan(8, 00, 00);
                TempsLBL.Text = Temps.ToString("c");
                TempsLBL.Refresh();
                Play = false;
                PlayPause.Image = But_Images[8];

                Map.Overlays.Clear();
                Map.Refresh();
                foreach (Target T in TargetList)
                {
                    T.ResetOverlays();
                }

                ViewTargetListADSB = new List<Target>();
                ViewTargetListMULTI = new List<Target>();
                ViewTargetListSMR = new List<Target>();
                ViewTargetListShow = new List<Target>();

                TextVisorBTN.BorderStyle = BorderStyle.None;
                PanelControlSuperior.Visible = false;
                TextVisorPanel.Visible = false;
                this.DataBlockList = new List<DataBlock>();//lista con paquetes separados
                this.DataTable1000 = new List<DataTable>();
                this.Cursor = Cursors.WaitCursor;
                PGB1.Visible = true;
                //byte[] Bytes = File.ReadAllBytes(openFileDialog.FileName); //vector bytes todos juntos, sin separar ni nada
                byte[] Bytes = Load3AST();
                CatLib[] Cat = Hertz_Hülsmeyer.CarregarCategories();
                PGB1.Maximum = Bytes.Count();
                int H = 0; //Contador delements no inscrits a CAT10 i CAT21
                PGB1.Value = 1;

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
                        DataBlock ADD = new DataBlock(BytesSave, Cat, DataBlockList.Count());
                        DataBlockList.Add(ADD); //Afegim a la llista general

                        //Optimització groupTarget
                        if (TargetList.Count() == 0)
                        {
                            Target T = new Target();
                            T.DataBlocks.Add(ADD);
                            T.T_ID = ADD.T_ID;
                            T.T_Number = ADD.T_Number;

                            TargetList.Add(T);
                        }
                        else if (ADD.T_ID != "-")
                        {
                            int u = 0;
                            Boolean N = false;
                            while ((u < TargetList.Count()) && (!N))
                            {
                                if (TargetList[u].T_ID == ADD.T_ID)
                                {
                                    N = true;
                                    TargetList[u].DataBlocks.Add(ADD);
                                }
                                u++;
                            }
                            if (!N)
                            {
                                Target T = new Target();
                                T.DataBlocks.Add(ADD);
                                T.T_ID = ADD.T_ID;
                                T.T_Number = ADD.T_Number;

                                TargetList.Add(T);
                            }
                        }
                        else
                        {
                            int u = 0;
                            bool enc = false;
                            while ((u < TargetList.Count()) && (!enc))
                            {
                                if ((ADD.From == "SMR") && (TargetList[u].DataBlocks.First().From == "SMR"))
                                {
                                    if (TargetList[u].T_Number == ADD.T_Number)
                                    {
                                        enc = true;
                                        TargetList[u].DataBlocks.Add(ADD);
                                    }
                                }
                                else if ((ADD.From != "SMR") && (TargetList[u].DataBlocks.First().From != "SMR"))
                                {
                                    if (TargetList[u].T_Number == ADD.T_Number)
                                    {
                                        enc = true;
                                        TargetList[u].DataBlocks.Add(ADD);
                                    }
                                }

                                    u++;
                            }
                            if (!enc)
                            {
                                Target T = new Target();
                                T.DataBlocks.Add(ADD);
                                T.T_ID = ADD.T_ID;
                                T.T_Number = ADD.T_Number;

                                TargetList.Add(T);
                            }
                        }

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
                GroupTNumb();
                
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
                SouPanel.Visible = true;
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
                    PanelControlSuperior.Visible = true;
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
            MapVisorPanel.Visible = false;
            TempsPanel.Visible = false;
            CatPanel.Visible = true;
            TextVisorBTN.BorderStyle = BorderStyle.FixedSingle;
            MapVisor.BorderStyle = BorderStyle.None;
            Timer.Stop();
            Play = false;
            PlayPause.Image = Image.FromFile("Play(I).png");

            if (IdView != "All")
            {
                Buscar.Text = IdView;
                DataInf.Text = "Loading...";
                DataInf.ForeColor = Color.DarkGray;
                DataInf.Refresh();
                this.Cursor = Cursors.WaitCursor;
                if (IdView == "")
                    this.IdView = "All";
                DataBlocksDGV_Act();
                TargetShow_Act();
                this.Cursor = Cursors.Default;
                DataInf.Text = "Data loaded";
                DataInf.ForeColor = Color.Green;
            }
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
            TempsPanel.Visible = true;
            MapVisorPanel.Visible = true;
            MapVisorPanel.BringToFront();
            CatPanel.Visible = false;

            if (IdView != "All")
            {
                SearchTxT2.Text = IdView;
            }
        }
        private void MapVisor_MouseHover(object sender, EventArgs e)
        {
            MapVisor.BackColor = Color.FromArgb(0, 66, 108);
        }
        private void MapVisor_MouseLeave(object sender, EventArgs e)
        {
            MapVisor.BackColor = Color.FromArgb(209, 222, 230);
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

        //Obrir info al DGV DataBlock view
        private void DataBlocksAll_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            DataBlocksAll.CurrentRow.Selected = true;
            this.Cursor = Cursors.WaitCursor;
            if (e.RowIndex >= 0)
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

                NamT.Text = "D.Blocks";
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

                NamT.Text = "Targets";
            }
        }

        private void TargetsShow_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            DataInf.Text = "Loading...";
            DataInf.ForeColor = Color.DarkGray;
            DataInf.Refresh();
            string ID = DataBlocksAll.Rows[e.RowIndex].Cells["Target ID"].FormattedValue.ToString();
            if (ID == "-")
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

        //Control de temps
        private void Timer_Tick(object sender, EventArgs e)
        {
            Map.Overlays.Clear();
            GMapPolygon Area = null;

            //Augmentem temps
            Temps = Temps.Add(interval);
            TempsLBL.Text = Temps.ToString("c");
            TempsLBL.Refresh();

            //Aparició nous targets
            foreach (Target T in TargetList)
            {
                if ((Hertz_Hülsmeyer.Round(T.Inici) == Temps) && ((T.From == "ADS-B ")||(T.From == "ADS-B Multi. ")))
                {
                    ViewTargetListADSB.Add(T);
                    ViewTargetListShow.Add(T);
                }
                if ((Hertz_Hülsmeyer.Round(T.Inici) == Temps) && ((T.From == "Multi. ") || (T.From == "ADS-B Multi. ")))
                {
                    ViewTargetListMULTI.Add(T);
                    if (T.From == "Multi. ")
                        ViewTargetListShow.Add(T);
                }
                else if ((Hertz_Hülsmeyer.Round(T.Inici) == Temps) && (T.From == "SMR"))
                {
                    ViewTargetListSMR.Add(T);
                    ViewTargetListShow.Add(T);
                }
                //else if ((T.Inici == Temps) && (T.From == "ADS-B Multi. "))
                //{
                //    ViewTargetListADSB.Add(T);
                //    ViewTargetListMULTI.Add(T);
                //    ViewTargetListShow.Add(T);
                //}
            }

            //Mostrar targets al mapa
            //Multi.
            foreach (Target T in ViewTargetListMULTI)
            {
                if (((IdView == "All") || (IdView == T.T_ID) || (IdView == T.T_Number)) && ((MULT) || ((!ADSB) && (!MULT) && (!SMR))))
                {
                    Map.Overlays.Add(T.CapaMULTI);
                }

                Coordenada Mostrar = new Coordenada();
                bool enc = false;
                int i = 0;
                while ((i < T.CoordenadesMULTI.Count()) && (enc == false))
                {
                    if (Hertz_Hülsmeyer.Round(T.CoordenadesMULTI[i].Moment) == Temps)
                    {
                        enc = true;
                        Mostrar = T.CoordenadesMULTI[i];
                    }
                    i++;
                }

                if (enc == true)
                {
                    T.MapTravel(Mostrar, "Multi.", TrackBox.Checked, TrackTime, Mark_Images);
                    T.CapaMULTI.Polygons.Last().IsVisible = false;
                    
                }
                else if (TrackBox.Checked == true)
                    T.BorrarTraza(Temps, "Multi.", TrackTime);
                else
                    T.CapaMULTI.Routes.Clear();
                if ((Center.Checked == true) && ((IdView == T.T_ID) || (IdView == T.T_Number)) && (enc == true))
                {
                    if (FromMarker == "MULTI")
                        Map.Position = Mostrar.PointMap;
                    Area = T.CapaMULTI.Polygons.First();
                }
            }
            //SMR
            foreach (Target T in ViewTargetListSMR)
            {
                if (((IdView == "All") || (IdView == T.T_ID) || (IdView == T.T_Number)) && ((SMR) || ((!ADSB) && (!MULT) && (!SMR))))
                {
                    Map.Overlays.Add(T.CapaSMR);
                }

                Coordenada Mostrar = new Coordenada();
                bool enc = false;
                int i = 0;
                while ((i < T.CoordenadesSMR.Count()) && (enc == false))
                {
                    if (Hertz_Hülsmeyer.Round(T.CoordenadesSMR[i].Moment) == Temps)
                    {
                        enc = true;
                        Mostrar = T.CoordenadesSMR[i];
                    }
                    i++;
                }
                if (Area != null)
                {
                    if(Area.IsInside(Mostrar.PointMap) == true)
                    {
                        //Map.Overlays.Add(T.CapaSMR);
                        if(T.T_ID!= Area.Name)
                        {
                            T.T_ID = Area.Name;
                            BusquedaMetode(T.T_ID);
                        }
                    }
                }
                if (enc == true)
                {
                    T.MapTravel(Mostrar, "SMR", TrackBox.Checked, TrackTime, Mark_Images);
                }
                else if (TrackBox.Checked == true)
                    T.BorrarTraza(Temps, "SMR", TrackTime);
                else
                    T.CapaSMR.Routes.Clear();
                if ((Center.Checked == true) && ((IdView == T.T_ID) || (IdView == T.T_Number)) && (enc == true) && (FromMarker == "SMR"))
                {
                    Map.Position = Mostrar.PointMap;
                }
            }
            //ADSB
            foreach (Target T in ViewTargetListADSB)
            {
                if (((IdView == "All") || (IdView == T.T_ID) || (IdView == T.T_Number)) && ((ADSB) || ((!ADSB) && (!MULT) && (!SMR))))
                {
                    Map.Overlays.Add(T.CapaADSB);
                }

                Coordenada Mostrar = new Coordenada();
                bool enc = false;
                int i = 0;
                while ((i < T.CoordenadesADSB.Count()) && (enc == false))
                {
                    if (Hertz_Hülsmeyer.Round(T.CoordenadesADSB[i].Moment) == Temps)
                    {
                        enc = true;
                        Mostrar = T.CoordenadesADSB[i];
                    }
                    i++;
                }

                if (enc == true)
                {
                    T.MapTravel(Mostrar, "ADS-B", TrackBox.Checked, TrackTime, Mark_Images);
                }
                else if (TrackBox.Checked == true)
                    T.BorrarTraza(Temps, "ADS-B", TrackTime);
                else
                    T.CapaADSB.Routes.Clear();
                if ((Center.Checked == true) && ((IdView == T.T_ID) || (IdView == T.T_Number)) && (enc == true) && (FromMarker == "ADS-B"))
                {
                    Map.Position = Mostrar.PointMap;
                }
            }


            //Mostrem targets al DGV
            if (countNOACTU != ViewTargetListShow.Count())
            {
                CurrentTargets_Act();
                countNOACTU = ViewTargetListShow.Count();
                Map.Refresh();
            }

            //Eliminar de les llistes en el moment+1 final
            //ADSB
            int j = 0;
            while (j < ViewTargetListADSB.Count())
            {
                if (Hertz_Hülsmeyer.Round(ViewTargetListADSB[j].Final) < Temps)
                {
                    ViewTargetListADSB.Remove(ViewTargetListADSB[j]);
                }
                j++;
            }
            //Multi
            j = 0;
            while (j < ViewTargetListMULTI.Count())
            {
                if (Hertz_Hülsmeyer.Round(ViewTargetListMULTI[j].Final) < Temps)
                {
                    ViewTargetListMULTI.Remove(ViewTargetListMULTI[j]);
                }
                j++;
            }
            //SMR
            while (j < ViewTargetListSMR.Count())
            {
                if (Hertz_Hülsmeyer.Round(ViewTargetListSMR[j].Final) < Temps)
                {
                    ViewTargetListSMR.Remove(ViewTargetListSMR[j]);
                }
                j++;
            }
            //Show
            j = 0;
            while (j < ViewTargetListShow.Count())
            {
                if (Hertz_Hülsmeyer.Round(ViewTargetListShow[j].Final) < Temps)
                {
                    ViewTargetListShow.Remove(ViewTargetListShow[j]);
                }
                j++;
            }
        }

        private void PlayPause_Click(object sender, EventArgs e)
        {
            if (!Play)
            {
                Play = true;
                PlayPause.Image = But_Images[7];
                PlayPause.Refresh();
                Timer.Enabled = true;
                Timer.Start();

                TempsLBL.Text = Temps.ToString("c");
                TempsLBL.Refresh();
            }
            else
            {
                Play = false;
                PlayPause.Image = But_Images[9];
                Timer.Stop();
            }
            Map.Refresh();
        }
        private void PlayPause_MouseHover(object sender, EventArgs e)
        {
            if (!Play)
                PlayPause.Image = But_Images[9];
            else
                PlayPause.Image = But_Images[7];
        }
        private void PlayPause_MouseLeave(object sender, EventArgs e)
        {
            if (!Play)
                PlayPause.Image = But_Images[8];
            else
                PlayPause.Image = But_Images[6];
        }

        private void Refresh_Click(object sender, EventArgs e)
        {
            Temps = new TimeSpan(8, 00, 00);
            TempsLBL.Text = Temps.ToString("c");
            TempsLBL.Refresh();
            Play = false;
            PlayPause.Image = But_Images[8];
            Timer.Stop();

            Map.Overlays.Clear();
            Map.Refresh();
            foreach (Target T in TargetList)
            {
                T.ResetOverlays();
            }

            ViewTargetListADSB = new List<Target>();
            ViewTargetListMULTI = new List<Target>();
            ViewTargetListSMR = new List<Target>();
            ViewTargetListShow = new List<Target>();
        }
        private void Refresh_MouseHover(object sender, EventArgs e)
        {
            Refresh.Image = But_Images[5];
        }
        private void Refresh_MouseLeave(object sender, EventArgs e)
        {
            Refresh.Image = But_Images[4];
        }

        private void MesV_MouseHover(object sender, EventArgs e)
        {
            MesV.Image = But_Images[3];
        }
        private void MesV_MouseLeave(object sender, EventArgs e)
        {
            MesV.Image = But_Images[2];
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
            MenysV.Image = But_Images[1];
        }
        private void MenysV_MouseLeave(object sender, EventArgs e)
        {
            MenysV.Image = But_Images[0];
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

        private void TrackBox_CheckedChanged(object sender, EventArgs e)
        {
            if (TrackBox.Checked == true)
            {
                Rad5min.Visible = true;
                Rad15min.Visible = true;
                Rad30min.Visible = true;
                Rad1h.Visible = true;
                RadTot.Visible = true;
            }
            else
            {
                Rad5min.Visible = false;
                Rad15min.Visible = false;
                Rad30min.Visible = false;
                Rad1h.Visible = false;
                RadTot.Visible = false;
            }
        }

        private void Rad5min_CheckedChanged(object sender, EventArgs e)
        {
            if (Rad5min.Checked == true)
                TrackTime = 1;
        }
        private void Rad15min_CheckedChanged(object sender, EventArgs e)
        {
            if (Rad15min.Checked == true)
                TrackTime = 2;
        }
        private void Rad30min_CheckedChanged(object sender, EventArgs e)
        {
            if (Rad30min.Checked == true)
                TrackTime = 5;
        }
        private void Rad1h_CheckedChanged(object sender, EventArgs e)
        {
            if (Rad1h.Checked == true)
                TrackTime = 10;
        }
        private void RadTot_CheckedChanged(object sender, EventArgs e)
        {
            if (RadTot.Checked == true)
                TrackTime = 0;
        }

        private void CurrenTargets_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            CurrenTargets.CurrentRow.Selected = true;
            if (e.RowIndex >= 0)
            {
                string ID_I = CurrenTargets.Rows[e.RowIndex].Cells["ID"].FormattedValue.ToString();
                Target T = BusquedaMetode(ID_I);
                if (T != null)
                    if (T.From == "SMR")
                    {
                        FromMarker = "SMR";
                    }
                    else
                        FromMarker = "Multi";
            }
        }

        private void SearchBTN2_Click(object sender, EventArgs e)
        {
            string Text = SearchTxT2.Text;
            if (Text == "")
            {
                IdView = "All";
            }
            else
            {
                Target T = BusquedaMetode(Text);
                if (T!= null)
                    if (T.From == "SMR")
                    {
                        FromMarker = "SMR";
                    }
                    else
                        FromMarker = "Multi";
            }
        }
        private void SearchBTN2_MouseHover(object sender, EventArgs e)
        {
            SearchBTN2.BackColor = Color.FromArgb(0, 66, 108);
        }
        private void SearchBTN2_MouseLeave(object sender, EventArgs e)
        {
            SearchBTN2.BackColor = Color.FromArgb(209, 222, 230);
        }
        private void SearchTxT2_TextChanged(object sender, EventArgs e)
        {
            if (SearchTxT2.Text == "")
            {
                this.IdView = "All";
                CurrenTargets.Size = new Size(178, 434);
                ShowInfo.Size = new Size(178, 140);
                ID_TXT.Visible = false;
                ShowInfo.Visible = false;
                Center.Visible = false;
                Center.Checked = false;
            }
        }

        private void Map_OnMarkerClick(GMapMarker item, MouseEventArgs e)
        {
            string ID_I = item.Tag.ToString().Split('_')[0];
            FromMarker = item.Tag.ToString().Split('_')[1];

            BusquedaMetode(ID_I);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string ID = SearchTxT2.Text;
            System.IO.StreamWriter file = new System.IO.StreamWriter("" + ID + ".txt");
            file.Close();

            //Busquem target
            List<Target> Encontrado = TargetList.Where(x => x.T_ID == ID).ToList();
            if (Encontrado.Count() == 0)
            {
                Encontrado = TargetList.Where(x => x.T_Number == ID).ToList();
            }

            if (Encontrado.Count() != 0)
            {
                StreamWriter W = new StreamWriter("" + ID + ".txt");
                int Max = Encontrado[0].CoordenadesMULTI.Count();
                if (Max != 0)
                {
                    W.WriteLine(Max);
                    foreach (Coordenada C in Encontrado[0].CoordenadesMULTI)
                    {
                        W.WriteLine(string.Join("_", C.RetrunSysCart()));
                    }
                    W.Close();
                }
                else
                {
                    Max = Encontrado[0].CoordenadesMULTI.Count();
                    if (Max != 0)
                    {
                        W.WriteLine(Max);
                        foreach (Coordenada C in Encontrado[0].CoordenadesMULTI)
                        {
                            W.WriteLine(string.Join("_", C.RetrunSysCart()));
                        }
                        W.Close();
                    }
                    else
                    {
                        Max = Encontrado[0].CoordenadesSMR.Count();
                        if (Max != 0)
                        {
                            W.WriteLine(Max);
                            foreach (Coordenada C in Encontrado[0].CoordenadesSMR)
                            {
                                W.WriteLine(string.Join("_", C.RetrunSysCart()));
                            }
                            W.Close();
                        }
                    }
                }
                MessageBox.Show("Exported");
            }
        }

        private void R_But_Cat10_CheckedChanged(object sender, EventArgs e)
        {
            if (R_But_Cat10.Checked == true)
            {
                CAT21 = false;
                CAT10 = true;
                DataBlocksDGV_Act();
            }
        }
        private void R_But_Cat21_CheckedChanged(object sender, EventArgs e)
        {
            if (R_But_Cat21.Checked == true)
            {
                CAT21 = true;
                CAT10 = false;
                DataBlocksDGV_Act();
            }
        }
        private void R_But_CatAll_CheckedChanged(object sender, EventArgs e)
        {
            if (R_But_CatAll.Checked == true)
            {
                CAT21 = false;
                CAT10 = false;
                DataBlocksDGV_Act();
            } 
        }

        private void R_But_SMR_CheckedChanged(object sender, EventArgs e)
        {
            if (R_But_SMR.Checked == true)
            {
                SMR = true;
                MULT = false;
                ADSB = false;
                DataBlocksDGV_Act();
            }
        }
        private void R_But_Multi_CheckedChanged(object sender, EventArgs e)
        {
            if (R_But_Multi.Checked == true)
            {
                SMR = false;
                MULT =true;
                ADSB = false;
                DataBlocksDGV_Act();
            }
        }
        private void R_But_ADSB_CheckedChanged(object sender, EventArgs e)
        {
            if (R_But_ADSB.Checked == true)
            {
                SMR = false;
                MULT = false;
                ADSB = true;
                DataBlocksDGV_Act();
            }
        }
        private void R_But_SouAll_CheckedChanged(object sender, EventArgs e)
        {
            if (R_But_SouAll.Checked == true)
            {
                SMR = false;
                MULT = false;
                ADSB = false;
                DataBlocksDGV_Act();
            }
        }
    }
}
