using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PGTA_P1
{
    public partial class Form2 : Form
    {
        int Leng = 0; //0 ENG, 1 CAT, 2 ESP
        public Form2()
        {
            InitializeComponent();
            listBox_ACT();
        }

        public void CargarManual()
        {
            ManualVisorPanel.Visible = true;
            
        }

        private void ExitBTN_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void listBox_ACT()
        {
            if (Leng == 0)
            {
                //Items en ang
                listBox1.DataSource = null;

                List<string> SS = new List<string>();
                SS.Add("1. Load of data");
                SS.Add("2. HUD");
                SS.Add("3. Indicador d'estat");
                SS.Add("4. Top filters");
                SS.Add("5. Target view");
                SS.Add("6. Search in Text visor");
                SS.Add("7. Search in Map visor");
                SS.Add("8. Move in map");
                SS.Add("9. Track option");

                listBox1.DataSource = SS;
            }
            else if (Leng == 1)
            {
                //Items en cat
                listBox1.DataSource = null;
               
                List<string> SS = new List<string>();
                SS.Add("1. Càrrega de dades");
                SS.Add("2. Interfície");
                SS.Add("3. Indicador d'estat");
                SS.Add("4. Filtres superiors");
                SS.Add("5. Target view");
                SS.Add("6. Cerca en el Text visor");
                SS.Add("7. Cerca en el Map visor");
                SS.Add("8. Navegació pel mapa");
                SS.Add("9. Opció Track");

                listBox1.DataSource = SS;
            }
            else
            {
                //Items en esp
                listBox1.DataSource = null;

                List<string> SS = new List<string>();
                SS.Add("1. Carga de datos");
                SS.Add("2. Interfaz");
                SS.Add("3. Indicador de estado");
                SS.Add("4. Filtros superiores");
                SS.Add("5. Target view");
                SS.Add("6. Búsqueda en el Text visor");
                SS.Add("7. Búsqueda en el Map visor");
                SS.Add("8. Navegación por el mapa");
                SS.Add("9. Opción Track");

                listBox1.DataSource = SS;
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(Leng == 2)
            {
                if (listBox1.SelectedItem == null)
                { }
                else if (listBox1.SelectedItem.ToString() == "1. Carga de datos")
                    TextIMG.Image = Image.FromFile("CargaDatos.PNG");
                else if (listBox1.SelectedItem.ToString() == "2. Interfaz")
                    TextIMG.Image = Image.FromFile("Interfaz.PNG");
                else if (listBox1.SelectedItem.ToString() == "3. Indicador de estado")
                    TextIMG.Image = Image.FromFile("IndicadorEstado.PNG");
                else if (listBox1.SelectedItem.ToString() == "4. Filtros superiores")
                    TextIMG.Image = Image.FromFile("FiltrosSuperiores.PNG");
                else if (listBox1.SelectedItem.ToString() == "5. Target view")
                    TextIMG.Image = Image.FromFile("TargetViewES.PNG");
                else if (listBox1.SelectedItem.ToString() == "6. Búsqueda en el Text visor")
                    TextIMG.Image = Image.FromFile("BusquedaTEXT.PNG");
                else if (listBox1.SelectedItem.ToString() == "7. Búsqueda en el Map visor")
                    TextIMG.Image = Image.FromFile("BusquedaMAP.PNG");
                else if (listBox1.SelectedItem.ToString() == "8. Navegación por el mapa")
                    TextIMG.Image = Image.FromFile("Navegacion.PNG");
                else if (listBox1.SelectedItem.ToString() == "9. Opción Track")
                    TextIMG.Image = Image.FromFile("OpcionTrack.PNG");
            }
            
        }

        private void ENG_Click(object sender, EventArgs e)
        {
            Leng = 0;
            listBox_ACT();
        }

        private void ESP_Click(object sender, EventArgs e)
        {
            Leng = 2;
            listBox_ACT();
        }

        private void CAT_Click(object sender, EventArgs e)
        {
            Leng = 1;
            listBox_ACT();
        }
    }
}
