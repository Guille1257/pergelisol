using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
namespace simulateurPergelisol_alpha_0._1
{
    public partial class about : Form
    {
        private String m_langue;
        public about(string langue)
        {
            string[] tabTexte = null;
            m_langue = langue;

            try
            {
                using (StreamReader sr = new StreamReader("langage/" + m_langue + ".txt", Encoding.GetEncoding("iso-8859-1")))
                {
                    tabTexte = sr.ReadToEnd().Split(';');
                    sr.Close();
                    richTextBox1.Text += tabTexte[1];
                }

            }

            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                //TO DO
            }

          

            InitializeComponent();
        }
    }
}
