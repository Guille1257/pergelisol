using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;


namespace simulateurPergelisol_alpha_0._1
{
    public partial class Option : Form
    {
        private Form1 m_formParent;
        private String m_langue;
        private string m_MoisDebut;

        public Option(Form1 parent, string langue, string MoisDebut, int vitesseSim, int opacite)
        {
            string[] files;
            m_formParent = parent;
            m_langue = langue;
            m_MoisDebut = MoisDebut;
            InitializeComponent();

            files = Directory.GetFiles("langage");
            int filecount = files.GetUpperBound(0) + 1;

            for (int i = 0; i < filecount; i++)
            {
                files[i] = System.IO.Path.GetFileNameWithoutExtension(files[i]);
                this.comboBoxLangue.Items.Add(files[i]);
            }

            comboBoxLangue.SelectedIndex = -1;
            comboBoxLangue.Text = langue;


            chargerLangage();

            comboBoxMoisDebut.SelectedIndex = -1;
            comboBoxMoisDebut.Text = MoisDebut;

            label1.Text = opacite.ToString() ;
            label2.Text = vitesseSim.ToString();
            hScrollBar1.Value = opacite;
            hScrollBar2.Value = vitesseSim;

        }


        #region Méthode initialisation

        private void chargerLangage()
        {
            string[] tabTexte = null;

            int indexFormOption;

            try
            {
                using (StreamReader sr = new StreamReader("langage/" + m_langue + ".txt", Encoding.GetEncoding("iso-8859-1")))
                {
                    tabTexte = sr.ReadToEnd().Split(';');
                    sr.Close();
                }

            }

            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                //TO DO
            }

            if (tabTexte != null)
            {

                indexFormOption = Array.IndexOf(tabTexte, "\r\nFormOptions");
                this.labelMoisDebut.Text = tabTexte[indexFormOption + 2];
                this.labelVitSimulation.Text = tabTexte[indexFormOption + 17];
                this.labelOpacite.Text = tabTexte[indexFormOption + 19];
                this.labelLangue.Text = tabTexte[indexFormOption + 21];

                //chargement des mois
                this.comboBoxMoisDebut.Items.Clear();
                for (int i = 0; i < 12; i++)
                {
                    this.comboBoxMoisDebut.Items.Add(tabTexte[indexFormOption + 4 + i]);
                }
            }

        }


        #endregion

        #region gestion évènement

        private void comboBoxLangue_SelectedIndexChanged(object sender, EventArgs e)
        {
                m_langue = comboBoxLangue.Text;
                chargerLangage();
                m_formParent.changerLangue(m_langue);
                comboBoxLangue.SelectedText = m_langue;
        }

        private void comboBoxMoisDebut_SelectedIndexChanged(object sender, EventArgs e)
        {
                m_MoisDebut = comboBoxMoisDebut.Text;
                m_formParent.changerMoisDebut(m_MoisDebut);
        }

        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            label1.Text = "" + hScrollBar1.Value;
            m_formParent.changerOpacite(hScrollBar1.Value);
        }

        private void hScrollBar2_Scroll(object sender, ScrollEventArgs e)
        {
            label2.Text = "" + hScrollBar2.Value;
            m_formParent.changerVitesseSim(31 - hScrollBar2.Value);
        }

        #endregion




    }
}
