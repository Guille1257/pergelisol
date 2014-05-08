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

        public Option(Form1 parent, string langue, int vitesseSim, int opacite)
        {
            string[] files;
            m_formParent = parent;
            m_langue = langue;
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

            label1.Text = opacite.ToString() ;
            label2.Text = (vitesseSim - 20).ToString();
            hScrollBar1.Value = opacite;
            hScrollBar2.Value = vitesseSim - 20;

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
                this.labelVitSimulation.Text = tabTexte[indexFormOption + 17];
                this.labelOpacite.Text = tabTexte[indexFormOption + 19];
                this.labelLangue.Text = tabTexte[indexFormOption + 21];
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

        private void hScrollBar1_Scroll(object sender, ScrollEventArgs e)
        {
            label1.Text = "" + hScrollBar1.Value;
            m_formParent.changerOpacite(hScrollBar1.Value);
        }

        private void hScrollBar2_Scroll(object sender, ScrollEventArgs e)
        {
            label2.Text = "" + hScrollBar2.Value;
            m_formParent.changerVitesseSim(hScrollBar2.Value + 20);
        }

        #endregion

    }
}
