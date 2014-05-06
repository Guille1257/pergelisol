using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Timers;

namespace simulateurPergelisol_alpha_0._1
{
    public partial class Form1 : Form
    {
        private Graphique m_graphique;
        private panelTransparent m_tableauActif;
        private dataWriter m_writeData;
        private neigeContainer m_neige;

        //Variables du menu
        private List<ToolStripMenuItem> m_listeVillage;
        private List<ToolStripMenuItem> m_listeCoverType;
        private List<ToolStripMenuItem> m_listeSol;
        private ToolStripMenuItem m_villageItem,
                                  m_coverTypeItem,
                                  m_solItem;

        //Variables gestion chargement
        private bool m_langueCharger,
                     m_overrideSimulation,
                     m_finiTracer,
                     m_pasTracer;
        private Dictionary<string, string> m_equivalentSol;
        private Dictionary<string, string> m_equivalentCouverture;

        //Threading
        private Thread m_simulation;
        private bool m_killThread;

        //String pour langage
        private string m_nomGraphique,
                       m_moisDebut,
                       m_langue,
                       m_nomAxe;

        private Dictionary<string, string> m_equivalentMois;
        private Option m_formOption;
        private about m_formabout;

        //Paramètre
        private int m_indexMoisDebut;
        private int m_vitesseSimulation;
        private int m_opacite;

        public Form1()
        {
            InitializeComponent();
            this.FormClosing += new FormClosingEventHandler(this.form_close);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            m_listeVillage = new List<ToolStripMenuItem>();
            m_listeCoverType = new List<ToolStripMenuItem>();
            m_listeSol = new List<ToolStripMenuItem>();
            m_equivalentSol = new Dictionary<string, string>();
            m_equivalentCouverture = new Dictionary<string, string>();
            m_equivalentMois = new Dictionary<string, string>();
            m_langue = "Français";
            chargerLangage(m_langue);


            //paramètre
            m_pasTracer = true;
            m_vitesseSimulation = 20;
            m_opacite = 200;
            
            m_villageItem = m_listeVillage[0];
            m_coverTypeItem = m_listeCoverType[0];
            m_solItem = m_listeSol[0];
            m_villageItem.Checked = m_coverTypeItem.Checked = m_solItem.Checked = true;
            this.panelTableau.BackgroundImage = Image.FromFile("image/Argile.png");
            this.panelTableau.BackColor = Color.FromArgb(255, 111, 82, 64);

            this.panelTableau.BackgroundImageLayout = ImageLayout.Stretch;

            genererGraphique(new Point(0, 0), new Size(this.panelGraphique.Size.Width, this.panelGraphique.Size.Height), m_nomGraphique,m_nomAxe);
            initialiserBoutonSimulation();
            genererTableauActif(new Point((int)(m_graphique.getOrigine()[0] - m_graphique.getEspaceParGraduationX() / 2), 0), new Size((int)m_graphique.getGrandeurAxeX(), this.panelTableau.Size.Height));
            genererNeigeContainer();

            m_finiTracer = true;
            this.panelGraphique.BackColor = Color.AliceBlue;

            //form option
            m_formOption = new Option(this, m_langue, m_moisDebut, m_vitesseSimulation, m_opacite);
            m_formabout = new about(m_langue);
            initialiserDatawrite();
        }

        #region Méthode publique

        public void changerMoisGrasGraphique(int indexMois)
        {
            if (indexMois >= 0)
            {
                m_graphique.changerMoisGras(indexMois, m_equivalentMois.ElementAt(indexMois).Key);
            }
        }

        public void annulerMoisGrasGraphique()
        {
            m_graphique.changerMoisGras(12, null);
        }

        public void changerLangue(string langue)
        {
            m_langue = langue;
            chargerLangage(m_langue);
            this.panelGraphique.Controls.Remove(m_graphique);
            genererGraphique(new Point(0, 0), new Size(this.panelGraphique.Size.Width, this.panelGraphique.Size.Height), m_nomGraphique,m_nomAxe);
            genererNeigeContainer();
            this.panelTableau.Controls.Remove(m_tableauActif);
            genererTableauActif(new Point((int)(m_graphique.getOrigine()[0] - m_graphique.getEspaceParGraduationX() / 2), 0), new Size((int)m_graphique.getGrandeurAxeX(), this.panelTableau.Size.Height));
            if (!m_pasTracer)
            {
                m_overrideSimulation = true;
                m_simulation = new Thread(this.sequenceDessin);
                m_simulation.Start();
            }
        }

        public void changerVitesseSim(int i)
        {
            m_vitesseSimulation = i;
        }

        public void changerOpacite(int i)
        {
            m_opacite = i;
            this.m_tableauActif.changerOpacite(i);
        }

        public void changerMoisDebut(string i)
        {
            m_moisDebut = i;
            this.panelGraphique.Controls.Remove(m_graphique);
            genererGraphique(new Point(0, 0), new Size(this.panelGraphique.Size.Width, this.panelGraphique.Size.Height), m_nomGraphique,m_nomAxe);
            this.panelTableau.Controls.Remove(m_tableauActif);
            genererTableauActif(new Point((int)(m_graphique.getOrigine()[0] - m_graphique.getEspaceParGraduationX() / 2), 0), new Size((int)m_graphique.getGrandeurAxeX(), this.panelTableau.Size.Height));

            switch (m_equivalentCouverture[this.m_coverTypeItem.Text])
            {
                case "none":
                    m_tableauActif.changerSolType(0);
                    m_graphique.switchBackground(0);
                    break;

                case "lichen":
                    m_tableauActif.changerSolType(1);
                    m_graphique.switchBackground(1);
                    break;

                case "low":
                    m_tableauActif.changerSolType(2);
                    m_graphique.switchBackground(2);
                    break;

                case "high":
                    m_tableauActif.changerSolType(3);
                    m_graphique.switchBackground(3);
                    break;
            }
            if (!m_pasTracer)
            {
                m_overrideSimulation = true;
                m_simulation = new Thread(this.sequenceDessin);
                m_simulation.Start();
            }
        }

        public void changerDataToolTip(double temp, double prof)
        {
            m_writeData.updataData(temp, prof);
        }

        public void toolTipVisible(bool i)
        {
            m_writeData.Visible = i;
        }

        #endregion

        #region Méthode simulation

        private void sequenceDessin()
        {
            int dernierPoint,
                prochainPoint;

            m_finiTracer = false;

            dernierPoint = 0;
            prochainPoint = 1;

            if (!m_overrideSimulation)
            {
                while (prochainPoint < 12 && !m_killThread)
                {

                    if (dernierPoint == 0)
                    {
                        m_neige.start();
                    }

                    else if (dernierPoint == 7)
                    {
                        m_neige.finishThread();
                    }

                    dernierPoint = prochainPoint;
                    m_tableauActif.setProchainMois(prochainPoint);
                    m_graphique.sequenceDessin(prochainPoint, m_vitesseSimulation,false);
                    prochainPoint += 1;
                }

                m_tableauActif.setProchainMois(prochainPoint);
            }

            else
            {
                prochainPoint = 12;
                m_tableauActif.setProchainMois(prochainPoint);
                m_graphique.sequenceDessin(prochainPoint, m_vitesseSimulation, true);
            }

            if (m_killThread)
            {
                clearSimulation();
                m_killThread = false;
            }

            m_finiTracer = true;
        }

        private void clearSimulation()
        {
            m_tableauActif.nettoyer();
            m_graphique.nettoyer();
            m_finiTracer = true;
        }

        #endregion

        #region méthode initialisation du form

        private void initialiserDatawrite()
        {
            m_writeData = new dataWriter(new Point(m_tableauActif.Location.X, 373), new Size(148, 37));
            panelTableau.Controls.Add(m_writeData);
            m_writeData.Visible = false;
        }
        private void initialiserBoutonSimulation()
        {
            this.buttonFin.Click += new System.EventHandler(this.buttonFin_Click);
            this.buttonDebut.Click += new System.EventHandler(this.buttonDebut_Click);

        }

        private void genererNeigeContainer()
        {
            m_neige = new neigeContainer(150, ref m_graphique);
            m_neige.Location = new Point(0, 0);
            m_neige.Size = this.panelGraphique.Size;
            m_neige.BackColor = Color.Transparent;
            m_graphique.Controls.Add(m_neige);
            m_neige.Hide();
            // m_graphique.Hide();
            //this.panelGraphique.Hide();
        }

        private void genererGraphique(Point location, Size size, string nomGraphique,string nomAxe)
        {
            float[] coordY = new float[12];
            string[] nomX = new string[12];
            lireAirTemperature(ref coordY, ref nomX);
            this.m_graphique = new Graphique(location, size, coordY, nomX, nomGraphique,nomAxe);
            this.panelGraphique.Controls.Add(this.m_graphique);
        }

        private void genererTableauActif(Point location, Size size)
        {
            this.m_tableauActif = new panelTransparent(lecturetemp(), m_indexMoisDebut, m_opacite, this);
            this.m_tableauActif.Location = location;
            this.m_tableauActif.Size = size;
            this.m_tableauActif.Height -= 40;
            this.m_tableauActif.Height = this.m_tableauActif.Height - (this.m_tableauActif.Height % 13);
            this.m_tableauActif.Width = (int)(m_graphique.getEspaceParGraduationX()) * 12;
            this.m_tableauActif.Cursor = System.Windows.Forms.Cursors.Cross;
            this.m_tableauActif.changerSolType(0);
            this.panelTableau.Controls.Add(this.m_tableauActif);
        }

        private void chargerLangage(String langage)
        {
            string[] tabTexte = null;
            string[] nomSol = { "clay", "peat", "roc", "sand", "till" };
            string[] nomCouverture = { "none", "lichen", "low", "high" };
            string[] nomMois = {"October", "November", "December", "January", "February", "March", "April", "May", "June", "July", "August", "September" };

            int indexTypeSol,
                indexCouverture,
                indexOption,
                indexBouton,
                indexNomMois,
                indexNomGraphe,
                indexDatawrite;
            try
            {
                using (StreamReader sr = new StreamReader("langage/" + langage + ".txt", Encoding.GetEncoding("iso-8859-1")))
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

            if (this.m_langueCharger)
            {
                m_equivalentCouverture.Clear();
                m_equivalentSol.Clear();
            }

            if (tabTexte != null)
            {
                indexTypeSol = Array.IndexOf(tabTexte, "\r\nType de sol");
                indexCouverture = Array.IndexOf(tabTexte, "\r\nCouverture");
                indexBouton = Array.IndexOf(tabTexte, "\r\nBouton");
                indexOption = Array.IndexOf(tabTexte, "\r\nOptions");
                indexNomMois = Array.IndexOf(tabTexte, "\r\ncomboxBoxMois");
                indexNomGraphe = Array.IndexOf(tabTexte, "\r\nnomGraphe");
                indexDatawrite=Array.IndexOf(tabTexte,"\r\ndatawrite");

                //Chargement nom des villages
                for (int i = 2; i < indexTypeSol; i++)
                {
                    if (!this.m_langueCharger)
                    {
                        this.m_listeVillage.Add(new ToolStripMenuItem());
                        this.m_listeVillage[i - 2].Click += new System.EventHandler(this.ToolStripMenuItem_click);
                    }
                    this.m_listeVillage[i - 2].Name = tabTexte[i];
                    this.m_listeVillage[i - 2].Text = tabTexte[i];
                    this.villageToolStripMenuItem.DropDownItems.Add(this.m_listeVillage[i - 2]);
                }

                //Chargement type de sol
                for (int i = indexTypeSol + 2; i < indexCouverture; i++)
                {
                    if (!this.m_langueCharger)
                    {
                        this.m_listeSol.Add(new ToolStripMenuItem());
                        this.m_listeSol[i - 2 - indexTypeSol].Click += new System.EventHandler(this.ToolStripMenuItem_click);
                    }
                    this.m_equivalentSol.Add(tabTexte[i], nomSol[i - indexTypeSol - 2]);
                    this.m_listeSol[i - 2 - indexTypeSol].Name = tabTexte[i];
                    this.m_listeSol[i - 2 - indexTypeSol].Text = tabTexte[i];
                    this.typeDeSolToolStripMenuItem.DropDownItems.Add(this.m_listeSol[i - 2 - indexTypeSol]);
                }

                //Chargement couverure
                for (int i = indexCouverture + 2; i < indexOption; i++)
                {
                    if (!this.m_langueCharger)
                    {
                        this.m_listeCoverType.Add(new ToolStripMenuItem());
                        this.m_listeCoverType[i - 2 - indexCouverture].Click += new System.EventHandler(this.ToolStripMenuItem_click);
                    }
                    this.m_equivalentCouverture.Add(tabTexte[i], nomCouverture[i - indexCouverture - 2]);
                    this.m_listeCoverType[i - 2 - indexCouverture].Name = tabTexte[i];
                    this.m_listeCoverType[i - 2 - indexCouverture].Text = tabTexte[i];
                    this.couvertureToolStripMenuItem.DropDownItems.Add(this.m_listeCoverType[i - 2 - indexCouverture]);
                }


                if (this.m_langueCharger)
                {
                    this.m_equivalentMois.Clear();
                }

                //Chargement nom mois
                for (int i = indexNomMois + 1; i < indexNomMois + 13; i++)
                {
                    this.m_equivalentMois.Add(tabTexte[i], nomMois[i - 1 - indexNomMois]);

                }


                this.villageToolStripMenuItem.Text = tabTexte[1];
                this.typeDeSolToolStripMenuItem.Text = tabTexte[indexTypeSol + 1];
                this.couvertureToolStripMenuItem.Text = tabTexte[indexCouverture + 1];
                this.optionsToolStripMenuItem.Text = tabTexte[indexOption + 1];

                //changement nom boutton
                this.buttonDemarrer.Text = tabTexte[indexBouton + 1];
                this.buttonDebut.Text = tabTexte[indexBouton + 2];
                this.buttonFin.Text = tabTexte[indexBouton + 3];

                //changement datawrite
            if(m_writeData!=null)
                m_writeData.upLangue(tabTexte[indexDatawrite+1],tabTexte[indexDatawrite+2]);
         
               this.m_langueCharger = true;
                m_nomGraphique = tabTexte[indexNomGraphe + 1];
                m_nomAxe = tabTexte[indexNomGraphe + 2];
                m_moisDebut = m_equivalentMois.ElementAt(0).Key;
            }
        }

        #endregion

        #region méthode lecture fichier

        public string[,] lecturetemp()
        {
            string[,] tableau = new string[56, 13];
            string[] ligne;
            int counter = 0;
            int cnb = 0;
            string line;

            System.IO.StreamReader file = new System.IO.StreamReader("village/" + m_villageItem.Text + "/" + m_villageItem.Text + m_equivalentSol[m_solItem.Text] + ".csv");

            while ((line = file.ReadLine()) != null)
            {
                ligne = line.Split(';');
                cnb = 0;
                while (cnb < 13)
                {
                    tableau[counter, cnb] = ligne[cnb];
                    cnb++;
                }
                counter++;
            }

            file.Close();
            return tableau;
        }

        private void lireAirTemperature(ref float[] coordY, ref string[] nomX)
        {
            string[] tabTexte = null;
            int compteur = 0;

            try
            {
                using (StreamReader sr = new StreamReader("village/" + m_villageItem.Text + "/airTemperature.csv"))
                {
                    tabTexte = sr.ReadToEnd().Split(new string[] { ";", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
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
                m_indexMoisDebut = Array.IndexOf(tabTexte, m_equivalentMois[m_moisDebut]);
                if (m_indexMoisDebut != 0)
                {
                    for (int i = m_indexMoisDebut; i < 12; i++)
                    {
                        nomX[compteur] = m_equivalentMois.ElementAt(i).Key;
                        coordY[compteur] = Single.Parse(tabTexte[i + 12]);
                        compteur++;
                    }

                    for (int i = 0; i <= m_indexMoisDebut - 1; i++)
                    {
                        nomX[compteur] = m_equivalentMois.ElementAt(i).Key;
                        coordY[compteur] = Single.Parse(tabTexte[i + 12]);
                        compteur++;
                    }

                }
                else
                {
                    for (int i = 0; i < 12; i++)
                    {
                        nomX[i] = m_equivalentMois.ElementAt(i).Key;
                        coordY[i] = Single.Parse(tabTexte[i + 12]);
                    }
                }
            }


        }

        #endregion

        #region Gestion evènement du form

        private void panelTableau_draw(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            SolidBrush brushPoint = new SolidBrush(Color.Black);
            Pen penDraw = new Pen(Brushes.Black);
            penDraw.Width = 1;
            brushPoint.Color = Color.FromArgb(255, 120, 96, 74);
            e.Graphics.FillRectangle(brushPoint, new Rectangle(0, 0, this.Size.Width, m_tableauActif.Size.Height/13));
            //e.Graphics.DrawImage(Image.FromFile("image/top_panel.png"), new Point(0, 0));
            e.Graphics.DrawLine(penDraw, new Point(0, 0), new Point(this.Size.Width, 0));
            penDraw.Width = 1;
            e.Graphics.DrawLine(penDraw, new Point(0, m_tableauActif.Size.Height / 13), new Point(this.Size.Width, m_tableauActif.Size.Height / 13));
            brushPoint.Dispose();
            penDraw.Dispose();
        }

        private void form_close(object sender, EventArgs e)
        {
            try
            {
                m_simulation.Abort();
            }
            catch(Exception ep)
             {

             }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            m_pasTracer = false;
            if (m_finiTracer)
            {
                m_overrideSimulation = false;
                m_simulation = new Thread(this.sequenceDessin);
                m_simulation.Start();
            }
        }

        private void buttonFin_Click(object sender, EventArgs e)
        {

            m_overrideSimulation = true;
            m_pasTracer = false;

            if (m_finiTracer)
            {
                m_simulation = new Thread(this.sequenceDessin);
                m_simulation.Start();

            }

            else
            {
                m_simulation.Abort();
                m_simulation = new Thread(this.sequenceDessin);
                m_simulation.Start();
            }

            if (!m_neige.done)
            {
                m_neige.killThread();
            }

            m_neige.update();
        }

        private void buttonDebut_Click(object sender, EventArgs e)
        {
            if (m_simulation.IsAlive)
            {
                m_killThread = true;
                m_graphique.killSimulation();
            }

            else
            {
                clearSimulation();
            }

            if (!m_neige.done)
            {
                m_neige.killThread();
            }

            m_pasTracer = true;
        }


        private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_formOption.IsDisposed)
            {
                m_formOption = new Option(this, m_langue, m_moisDebut, m_vitesseSimulation, m_opacite);
                m_formOption.Visible = true;
            }
            else
            {
                m_formOption.Visible = true;
            }
            Application.OpenForms["Option"].BringToFront();
        }

        private void ToolStripMenuItem_click(object sender, EventArgs e)
        {
            ToolStripMenuItem temp = (ToolStripMenuItem)sender;

            if (temp.OwnerItem == this.villageToolStripMenuItem)
            {
                float[] coordY = new float[12];
                string[] nomX = new string[12];
                this.m_villageItem.CheckState = CheckState.Unchecked;                
                temp.CheckState = CheckState.Checked;
                this.m_villageItem = temp;
                lireAirTemperature(ref coordY, ref nomX);
                this.m_graphique.updateDonnee(coordY, nomX, "Température en fonction du mois");
                this.m_tableauActif.changerDonne(lecturetemp());
            }

            else if (temp.OwnerItem == this.typeDeSolToolStripMenuItem)
            {   
                this.m_solItem.CheckState = CheckState.Unchecked;
                temp.CheckState = CheckState.Checked;
                this.m_solItem = temp;

                switch (m_equivalentSol[m_solItem.Text])
                {
                    case "clay":
                        this.panelTableau.BackgroundImage = Image.FromFile("image/Argile.png");
                        break;

                    case "peat":
                        this.panelTableau.BackgroundImage = Image.FromFile("image/Tourbe.png");
                        break;

                    case "roc":
                        this.panelTableau.BackgroundImage = Image.FromFile("image/Roc.png");
                        break;

                    case"sand":
                        this.panelTableau.BackgroundImage = Image.FromFile("image/Sable.png");
                        break;

                    case "till":
                        this.panelTableau.BackgroundImage = Image.FromFile("image/Till.png");
                        break;

                }

                this.m_tableauActif.changerDonne(lecturetemp());
            }

            else if (temp.OwnerItem == this.couvertureToolStripMenuItem)
            {
                this.m_coverTypeItem.CheckState = CheckState.Unchecked;
                temp.CheckState = CheckState.Checked;
                this.m_coverTypeItem = temp;

                switch (m_equivalentCouverture[this.m_coverTypeItem.Text])
                {
                    case "none":
                        m_tableauActif.changerSolType(0);
                        m_graphique.switchBackground(0);
                        break;

                    case "lichen":
                        m_tableauActif.changerSolType(1);
                        m_graphique.switchBackground(1);
                        break;

                    case "low":
                        m_tableauActif.changerSolType(2);
                        m_graphique.switchBackground(2);
                        break;

                    case "high":
                        m_tableauActif.changerSolType(3);
                        m_graphique.switchBackground(3);
                        break;
                }

            }

        }

      private void toolStripAbout_Click(object sender, EventArgs e)
      {
     if (m_formabout.IsDisposed)
                {
                    m_formabout = new about(m_langue);
                    m_formabout.Visible = true;
                }
                else
                {
                    m_formabout.Visible = true;
                }
                Application.OpenForms["about"].BringToFront();
      }
        #endregion
    }

    public class panelTransparent : PictureBox
    {
        private Form1 m_formParent;
        private string[,] m_tableau = new string[57, 14];

        private float m_espaceX;
        private int m_moisEnCours,
                    m_typeSol;
        private delegate void dessin();
        private dessin callBackDessin;
        private bool m_commencerDessin;
        private int m_indexMoisDebut;
        private int m_indexMoisEnCours;

        private int opacite;

        private readonly double[] PROFONDEUR = new double[] { 0, -0.25, -0.5, -0.75, -1, -1.5, -2,-2.5, -3, -3.5, -4, -4.5, -5 };

        #region Constructeurs

        public panelTransparent(string[,] tableau, int indexMoisDebut, int opac, Form1 parent)
            : base()
        {
            m_formParent = parent;
            m_tableau = tableau;
            opacite = opac;
            m_indexMoisDebut = indexMoisDebut + 1;
            this.MouseEnter += new System.EventHandler(this.mouseEnter);
            this.MouseLeave += new System.EventHandler(this.mouseLeave);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.mouseMove);
            m_moisEnCours = 0;

            callBackDessin = new dessin(this.invalidateControl);
            this.BackColor = Color.Transparent;
            m_commencerDessin = false;
            m_indexMoisEnCours = -1; //valeur initilisation

        }

        #endregion

        #region Méthode public

        public void changerSolType(int type)
        {
            m_typeSol = type;
            this.Invalidate();
            this.Refresh();
            this.Update();
        }

        public void changerDonne(string[,] tableau)
        {
            m_tableau = tableau;
            this.Invalidate();
            this.Refresh();
            this.Update();
        }

        public void changerOpacite(int i)
        {
            opacite = i;
            this.Invalidate();
            this.Refresh();
            this.Update();
        }

        public void nettoyer()
        {
            m_commencerDessin = false;
            this.Invoke(callBackDessin);
        }

        public void setEspaceX(float i)
        {
            m_espaceX = i;
        }

        public void setProchainMois(int mois)
        {
            m_commencerDessin = true;
            m_moisEnCours = mois;
            this.Invoke(callBackDessin);
        }

        #endregion

        private void invalidateControl()
        {
            this.Invalidate();
            this.Refresh();
            this.Update();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (m_commencerDessin)
            {
                if (m_commencerDessin)
                {
                    m_espaceX = this.Width / 12;
                    Pen pen = new Pen(Color.Black);
                    SolidBrush brush;
                    SolidBrush tomatoBrush = new SolidBrush(Color.Tomato);
                    Font fontToolTip = new Font(FontFamily.GenericSansSerif, 8);
                    for (int x = 0; x < m_moisEnCours; x++)
                    {
                        for (int y = 0; y < 13; y++)
                        {
                            if (x + m_indexMoisDebut > 12)
                            {
                                brush = getBrush(Convert.ToDouble(m_tableau[(y + 1) + m_typeSol * 14, -(12 - m_indexMoisDebut) + (x)]), opacite);

                            }

                            else
                            {
                                brush = getBrush(Convert.ToDouble(m_tableau[(y + 1) + m_typeSol * 14, x + m_indexMoisDebut]), opacite);
                            }
                            e.Graphics.FillRectangle(brush, (float)(x * m_espaceX), (float)(y * this.Height / 13), (float)(m_espaceX), (float)(this.Height / 13));
                            brush.Dispose();
                            e.Graphics.DrawRectangle(pen, (float)(x * m_espaceX), (float)(y * this.Height / 13), (float)(m_espaceX), (float)(this.Height / 13));
                        }
                    }

                    if (m_moisEnCours == 12)
                        e.Graphics.DrawRectangle(pen, 0, 0, m_moisEnCours * m_espaceX - 1, this.Size.Height - 1);
                    else
                       e.Graphics.DrawRectangle(pen, 0, 0, m_moisEnCours * m_espaceX, this.Size.Height - 1);

                    pen.Dispose();
                }
            }
            base.OnPaint(e);
        }

        private SolidBrush getBrush(double val, int transparence)
        {
            SolidBrush brushReturn = new SolidBrush(Color.FromArgb(150, 0, 0, 255));
            double[] upperBound = {25,20,15,13,11,9,7,5,4,3,2,1.5,1,9.5,-0.49,-0.5,-1,-1.5,-2,-3,-4,-5,-7,-9,-11,-13,-15,-20,-25};
            string[] rgbValueTab = {"230-0-0", "232-39-0", "237-59-0","240-80-0", "242-97-0", "245-110-0", "247-128-0", "250-142-0", "252-160-0", "252-173-0",
                                    "255-191-0", "255-204-0","255-221-0","255-238-0","255-255-0","115-222-217","109-211-214","102-199-212","96-187-209","89-175-207",
                                    "82-165-204", "75-155-201", "68-144-199" ,"61-135-196" ,"52-125-194" ,"43-114-189" ,"35-106-186" ,"24-96-184" ,"2-88-181"};
            string[] rgbValue;

            for (int i = 0; i < upperBound.Length; i++)
            {
                if (val >= upperBound[0])
                {
                    rgbValue = rgbValueTab[0].Split('-');
                    brushReturn.Color = Color.FromArgb(transparence, Convert.ToInt32(rgbValue[0]), Convert.ToInt32(rgbValue[1]), Convert.ToInt32(rgbValue[2]));
                    break;
                }

                else if (val <= upperBound[upperBound.Length - 1])
                {
                    rgbValue = rgbValueTab[upperBound.Length - 1].Split('-');
                    brushReturn.Color = Color.FromArgb(transparence, Convert.ToInt32(rgbValue[0]), Convert.ToInt32(rgbValue[1]), Convert.ToInt32(rgbValue[2]));
                    break;
                }

                else
                {
                    if (i != 0 && i != upperBound.Length - 1)
                    {
                        if (val < upperBound[i] && val >= upperBound[i + 1])
                        {
                            rgbValue = rgbValueTab[i].Split('-');
                            brushReturn.Color = Color.FromArgb(transparence, Convert.ToInt32(rgbValue[0]), Convert.ToInt32(rgbValue[1]), Convert.ToInt32(rgbValue[2]));
                            break;
                        }
                    }
                }
            }
            return brushReturn;
        }

        #region Gestion évènement

        private void mouseMove(object sender, EventArgs e)
        {

            int positiony = (((this.PointToClient(Cursor.Position).Y) / (this.Height / 13)) + 1);
            int positionx = (((this.PointToClient(Cursor.Position).X) / (this.Width / 12)));
            double result = 0;

            if (m_indexMoisEnCours != positionx && m_commencerDessin && positionx <= m_moisEnCours)
            {
                m_indexMoisEnCours = positionx;

                if (positionx + m_indexMoisDebut > 12)
                {
                    m_formParent.changerMoisGrasGraphique(-(12 - m_indexMoisDebut) + (positionx) - 1);
                }

                else
                {
                    m_formParent.changerMoisGrasGraphique(positionx + m_indexMoisDebut - 1);
                }

            }

            if ((positiony < 14) && (positiony > 0) && (positionx >= 0) && (positionx < 13))
            {

                if (m_commencerDessin && positionx <= m_moisEnCours)
                {
                    m_formParent.toolTipVisible(true);

                    if (positionx + m_indexMoisDebut > 12)
                    {
                        result = Convert.ToDouble(m_tableau[positiony + m_typeSol * 14, -(12 - m_indexMoisDebut) + (positionx)]);
                    }

                    else
                    {
                        result = Convert.ToDouble(m_tableau[positiony + m_typeSol * 14, positionx + m_indexMoisDebut]);
                    }


                    m_formParent.changerDataToolTip(result, PROFONDEUR[positiony - 1]);
                }

                else
                {
                    m_formParent.toolTipVisible(false);
                }

            }

        }

        private void mouseEnter(object sender, EventArgs e)
        {
            if (m_commencerDessin)
            {
                int positiony = (((this.PointToClient(Cursor.Position).Y) / (this.Height / 13)) + 1);
                int positionx = (((this.PointToClient(Cursor.Position).X) / (this.Width / 12)));

                m_indexMoisEnCours = positionx;
                if (positionx <= m_moisEnCours)
                {
                    if (positionx + m_indexMoisDebut > 12)
                    {
                        m_formParent.changerMoisGrasGraphique(-(12 - m_indexMoisDebut) + (positionx) - 1);
                    }

                    else
                    {
                        m_formParent.changerMoisGrasGraphique(positionx + m_indexMoisDebut - 1);
                    }
                }
            }
        }


        private void mouseLeave(object sender, EventArgs e)
        {
            if (m_commencerDessin)
            {
                m_formParent.toolTipVisible(false);
                m_formParent.annulerMoisGrasGraphique();
            }
        }

        #endregion
    }

    public class dataWriter : PictureBox
    {

        private double  m_temp,
                        m_profondeur;
        private string  m_sTemp,
                        m_sProfondeur;

        public dataWriter(Point position, Size grosseur)
        {
            this.Size = grosseur;
            this.Location = position;
            m_temp = 10.5;
            m_profondeur = -1.25;
            m_sTemp = "Température:";
            m_sProfondeur = "Profondeur:";
        }

        public void updataData(double temp, double profondeur)
        {
            m_temp = temp;
            m_profondeur = profondeur;
            this.Invalidate();
            this.Update();
            this.Refresh();
        }
        public void upLangue(string stemp, string sprofondeur)
        {
            m_sTemp = stemp;
            m_sProfondeur = sprofondeur; 
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
            Pen contour = new Pen(Brushes.Black);
            Font writing = new Font(FontFamily.GenericSansSerif, 10);
            StringFormat formatWriting = new StringFormat();
            formatWriting.Alignment = StringAlignment.Far;
            SolidBrush writingBrush = new SolidBrush(Color.Black);
            System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(new Bitmap(1, 1));
            int largeurTemp = (int)graphics.MeasureString(m_sTemp, writing).Width;

            if(m_temp < 0)
                pe.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, 4,130,255)), new Rectangle(0, 0, this.Size.Width, this.Size.Height));
            else
                pe.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, 224, 205, 14)), new Rectangle(0, 0, this.Size.Width, this.Size.Height));

            pe.Graphics.DrawLine(contour, 0, 0, this.Size.Width, 0);
            pe.Graphics.DrawLine(contour, 0, 0, 0, this.Size.Height);
            pe.Graphics.DrawLine(contour, this.Size.Width - 1, 0, this.Size.Width - 1, this.Size.Height);
            pe.Graphics.DrawLine(contour, 0, this.Size.Height - 1, this.Size.Width, this.Size.Height - 1);

            pe.Graphics.DrawString(m_sTemp, writing, writingBrush, new Rectangle(0, 1, largeurTemp + 2, 20), formatWriting);
            pe.Graphics.DrawString(m_sProfondeur, writing, writingBrush, new Rectangle(0, 20, largeurTemp, 20), formatWriting);

            pe.Graphics.DrawString(string.Format("{0: 0.00} °C", m_temp), writing, writingBrush, new Point(largeurTemp, 0));
            pe.Graphics.DrawString(string.Format("{0: 0.00} m", m_profondeur), writing, writingBrush, new Point(largeurTemp, 20));


            contour.Dispose();
            writing.Dispose();
            writingBrush.Dispose();

        }

    }

    public class neigeContainer : PictureBox
    {
        private Flocon[] m_arrayFlocons;
        private Random rng;
        private Thread m_simThread;
        private delegate void voidDelegate();
        private delegate void voidDelegateArgs(bool a);
        private voidDelegate delInvalidate;
        private voidDelegateArgs hideDel;
        private bool started;
        private Bitmap solIMG;
        private int nbFlocon;
        private object lockIMG;
        private Graphique m_controlParent;
        private bool m_done;
        private int m_doneCount;

        public neigeContainer(int nbFloconPerThread, ref Graphique parent)
        {
            m_controlParent = parent;
            delInvalidate = invalidateControl;
            hideDel = hideControl;
            lockIMG = new object();
            nbFlocon = nbFloconPerThread;
            m_simThread = new Thread(simulationSequence);
            m_done = false;
            rng = new Random();
            this.Size = parent.Size;
            solIMG = new Bitmap(this.Size.Width, this.Size.Height);
            initSol();
            creerFlocons(nbFloconPerThread);
        }

        public void start()
        {
            try
            {
                m_simThread.Start();
            }
            catch (Exception e)
            {
                creerFlocons(nbFlocon);
                try
                {
                    m_simThread.Abort();
                }
                catch(Exception f)
                 {

                 }
                m_simThread = new Thread(simulationSequence);
                m_simThread.Start();
            }
            started = true;
        }

        public void finishThread()
        {
            m_done = true;
        }

        public void update()
        {
            this.Invoke(delInvalidate);
        }

        public void killThread()
        {
            m_simThread.Abort();
            started = false;
            this.Invoke(delInvalidate);
        }

        private void hideControl(bool a)
        {
            this.Visible = a;
            m_controlParent.forceUpdate();
        }

        private void creerFlocons(int nombre)
        {
            int tempGen;
            m_arrayFlocons = new Flocon[nombre];
            for (int i = 0; i < m_arrayFlocons.Length; i++)
            {
                tempGen = rng.Next(-100, this.Size.Width);
                if (rng.Next(0, 50) > 10)
                {
                    m_arrayFlocons[i] = new Flocon(new int[] { tempGen, rng.Next(0, this.Size.Height) }, new int[] { rng.Next(tempGen, tempGen + 275), this.Size.Height },
                                                    this.Size.Height - 5);
                }
                else
                {
                    m_arrayFlocons[i] = new Flocon(new int[] { tempGen, rng.Next(0, this.Size.Height) }, new int[] { rng.Next(tempGen - 100, tempGen), this.Size.Height },
                                this.Size.Height - 5);
                }
            }
        }

        private void invalidateControl()
        {
            this.Invalidate();
            this.Update();
            this.Refresh();
        }

        private void bufferNouveauBackground()
        {
            Bitmap buffer = new Bitmap(this.Size.Width, this.Size.Height);

            using (Graphics g = Graphics.FromImage(buffer))
            {
                using (Graphics gDone = this.CreateGraphics())
                {
                    //m_controlParent.forceUpdate();
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    gDone.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                    lock (lockIMG)
                    {
                        // g.DrawImage(backgroundIMG, new Point(0, 0));
                        // g.DrawImage(solIMG, new Point(0, 0));
                    }

                    foreach (Flocon f in m_arrayFlocons)
                    {
                        g.FillEllipse(Brushes.White, f.coordinates[0], f.coordinates[1], f.taille, f.taille);
                    }
                    gDone.DrawImage(buffer, new Point(0, 0));
                }
            }
            buffer.Dispose();
        }

        private void simulationSequence()
        {
            m_done = false;
            m_doneCount = 0;
            this.Invoke(hideDel, true);
            while (!m_done || m_doneCount != nbFlocon)
            {
                calculNouveauFlocon();
                this.Invoke(delInvalidate);
                Thread.Sleep(5);
            }
        }

        private void calculNouveauFlocon()
        {
            int tempGen;
            foreach (Flocon f in m_arrayFlocons)
            {
                f.calculNextPoint(rng);
                if (f.coordinates[1] >= f.limitCollision)
                {
                    ajouterFloconSol(f);
                    tempGen = rng.Next(-100, this.Size.Width);

                    if (rng.Next(0, 50) > 10)
                    {
                        if (!m_done && !f.done)
                        {
                            f.reset(new int[] { tempGen, 0 }, new int[] { rng.Next(tempGen, tempGen + 275), this.Size.Height },
                                  this.Size.Height - 5);
                        }
                    }

                    else
                    {
                        if (!m_done && !f.done)
                        {
                            f.reset(new int[] { tempGen, 0 }, new int[] { rng.Next(tempGen - 100, tempGen), this.Size.Height },
                                  this.Size.Height - 5);
                        }
                    }

                    if (m_done && !f.done)
                    {
                        f.done = true;
                        m_doneCount++;
                    }
                }
            }
        }

        private void ajouterFloconSol(Flocon f)
        {
            lock (lockIMG)
            {
                using (Graphics g = Graphics.FromImage(solIMG))
                {
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    g.FillEllipse(Brushes.White, f.coordinates[0], f.coordinates[1], f.taille, f.taille);
                }
            }
        }

        private void initSol()
        {
            using (Graphics g = Graphics.FromImage(solIMG))
            {
                Brush b = new SolidBrush(Color.FromArgb(255, 80, 80, 80));
                g.FillRectangle(b, new Rectangle(0, 0, this.Size.Width, this.Size.Height));
                b.Dispose();
            }
            solIMG.MakeTransparent(Color.FromArgb(255, 80, 80, 80));
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
            if (started)
            {
                pe.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                foreach (Flocon f in m_arrayFlocons)
                {
                    pe.Graphics.FillEllipse(Brushes.White, f.coordinates[0], f.coordinates[1], f.taille, f.taille);
                }
            }
        }

        public bool done
        {
            get { return m_done; }
        }

    }

    public class Flocon
    {
        private int[] m_coordDebut,
                        m_coordFin,
                        m_currentCoord;

        private double m_coefficient,
                        m_zero;

        private int m_taille,
                        m_compteur,
                        m_limit;

        private bool m_inverser;
        private bool m_done;

        private Random rng;

        public Flocon(int[] coordDebut, int[] coordFin, int limitCollision)
        {
            m_coordDebut = m_currentCoord = coordDebut;
            m_coordFin = coordFin;
            m_taille = taille;
            rng = new Random();
            m_limit = limitCollision;
            m_compteur = 0;
            calculRegle();
            genererTaille();
            m_done = false;
        }

        public void calculNextPoint(Random rnd)
        {

            if (m_inverser)
                m_compteur--;
            else
                m_compteur++;

            if (m_coefficient != 0)
            {
                m_currentCoord = new int[] { m_coordDebut[0] + m_compteur, (int)(m_coefficient * (m_compteur + m_coordDebut[0]) + m_zero) };
            }
            else
            {
                m_currentCoord = new int[] { m_coordDebut[0], m_coordDebut[1] + m_compteur };
            }
        }

        public void reset(int[] coordDebut, int[] coordFin, int limitCollision)
        {
            m_coordDebut = m_currentCoord = coordDebut;
            m_coordFin = coordFin;
            m_limit = limitCollision;
            m_compteur = 0;
            m_inverser = false;
            calculRegle();
            genererTaille();
        }

        private void calculRegle()
        {
            if (m_coordDebut[0] != m_coordFin[0])
            {
                m_coefficient = ((double)m_coordFin[1] - (double)m_coordDebut[1]) / ((double)m_coordFin[0] - (double)m_coordDebut[0]);
                m_zero = m_coordDebut[1] - m_coordDebut[0] * m_coefficient;
            }
            else
            {
                m_coefficient = 0;
                m_zero = 0;
            }

            if (m_coordFin[0] < m_coordDebut[0])
            {
                m_inverser = true;
            }
        }

        private void genererTaille()
        {
            double[] bound = { 15, 5, 2.5, 1.3, 0.8, 0.3 }; //5 interval
            int[,] tailleInterval = new int[,] { { 4, 7 }, { 2, 7 }, { 2, 5 }, { 2, 4 }, { 2, 3 }, { 2, 2 } };

            if (m_coefficient < bound[0])
            {
                for (int i = 0; i < bound.Length; i++)
                {
                    if (m_coefficient >= bound[i] && m_coefficient <= bound[i - 1])
                    {
                        m_taille = rng.Next(tailleInterval[i, 0], tailleInterval[i, 1]);
                        break;
                    }
                }
            }
            else
            {
                m_taille = rng.Next(tailleInterval[0, 0], tailleInterval[0, 1]);
            }

            if (m_taille == 0)
            {
                m_taille = 1;
            }

        }

        public int limitCollision
        {
            get { return m_limit; }
        }

        public int taille
        {
            get { return m_taille; }
        }

        public bool done
        {
            get { return m_done; }
            set { m_done = value; }
        }

        public int[] coordinates
        {
            get { return m_currentCoord; }
        }

    }

}
