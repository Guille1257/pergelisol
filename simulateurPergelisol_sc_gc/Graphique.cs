using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace simulateurPergelisol_alpha_0._1
{
    public partial class Graphique : UserControl
    {

        //Variables d'affichage
        private float m_grandeurPixelAxeX,
                        m_grandeurPixelAxeY,
                        m_margePixelX,
                        m_margePixelY,
                        m_graduationPixelX,
                        m_graduationPixelY,
                        m_deltaY,
                        nbPixelQuadUn,
                        nbPixelQuadQuatre;

        private int m_valGraduationX,
                    m_valGraduationY,
                    m_dernierPoint,
                    m_prochainPoint;

        private Bitmap m_background,
                       m_backGroundThread;

        private string m_nomGraphique;
        private string m_imgPath;

        //Variable de points
        private float[] m_pointY;
        private string[] m_pointX;
        private float[] m_origine;

        //Variable de simulation
        private float m_zero,
                      m_coefficient;

        private bool m_enterDeuxPoints,
                     m_overideTracage,
                     m_tracer;

        private int m_compteur,
                    m_vitesseTrace;

        private string m_moisSurligner;
        private long m_ticksSimulation;

        private delegate void dessin();
        private dessin callBackDessin;
        private object lockSimThread;

        public Graphique(System.Drawing.Point location, System.Drawing.Size grosseur, float[] CoordPointY, string[] nomPointX, string nomGraphique)
        {
            InitializeComponent();
            m_imgPath = "image/backgroundGraphe.png";
            m_nomGraphique = nomGraphique;
            m_pointY = CoordPointY;
            m_pointX = nomPointX;
            this.Size = grosseur;
            this.Location = location;
            calculPopriete();
            creerImageBackground();
            callBackDessin = new dessin(invalidateControl);
            lockSimThread = new Object();
            calculVitesseTracage();
        }

        public float[] getOrigine()
        {
            return m_origine;
        }

        public float getEspaceParGraduationX()
        {
            return m_graduationPixelX;
        }

        public float getGrandeurAxeX()
        {
            return m_grandeurPixelAxeX;
        }

        public Bitmap getBrackGroundImage()
        {
            return m_background;   
        }

        public void updateDonnee(float[] CoordPointY, string[] nomPointX, string nomGraphique)
        {
            if (CoordPointY.Length == nomPointX.Length)
            {
                Array.Clear(m_pointY, 0, m_pointY.Length);
                Array.Clear(m_pointX, 0, m_pointX.Length);
                m_dernierPoint = 0;
                m_enterDeuxPoints = false;

                for (int i = 0; i < CoordPointY.Length; i++)
                {
                    m_pointY[i] = CoordPointY[i];
                    m_pointX[i] = nomPointX[i];
                }
                calculPopriete();
                creerImageBackground();
                bufferNouveauGraphique();
            }
        }


        public void switchBackground(int type)
        {
            switch (type)
            {
                case 0:
                    m_imgPath = "image/backgroundGraphe.png";
                    break;
                case 1:
                    m_imgPath = "image/lichen.png";
                    break;
                case 2:
                    m_imgPath = "image/low.png";
                    break;
                case 3:
                    m_imgPath = "image/high.png";
                    break;
            }

            creerImageBackground();
            bufferNouveauGraphique();
        }

        public void nettoyer()
        {
            m_tracer = false;
            m_dernierPoint = m_prochainPoint = 0;
            this.Invoke(callBackDessin);
        }

        public void changerMoisGras(string mois)
        {
            m_moisSurligner = mois;
            creerImageBackground();
            bufferNouveauGraphique();
        }

        #region Méthode override

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (m_tracer)
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.DrawImage(m_background, 0, 0);

                SolidBrush brushPoint = new SolidBrush(Color.Black);
                Pen pen = new Pen(Color.Black, 1);
                Pen pen2 = new Pen(Color.Black, 2);
                float xCourant,
                      xProchain,
                      pointX1,
                      pointY1,
                      pointX2,
                      pointY2;

                for (int i = 0; i <= m_prochainPoint; i++)
                {

                    if (!m_overideTracage)
                    {

                        if (i != m_prochainPoint)
                        {
                            g.FillEllipse(brushPoint, m_margePixelX + (m_graduationPixelX / m_valGraduationX) * i - 3,
                        m_origine[1] - 3 - ((m_graduationPixelY / m_valGraduationY) * m_pointY[i]), 6, 6);

                            g.DrawEllipse(pen, (m_margePixelX + (m_graduationPixelX / m_valGraduationX) * i) - 3,
                                           m_origine[1] - ((m_graduationPixelY / m_valGraduationY) * m_pointY[i]) - 3, 6, 6);
                        }

                        if (i == m_prochainPoint && m_prochainPoint == m_pointY.Length - 1 && m_enterDeuxPoints == false)
                        {

                            g.FillEllipse(brushPoint, m_margePixelX + (m_graduationPixelX / m_valGraduationX) * i - 3,
                                            m_origine[1] - 3 - ((m_graduationPixelY / m_valGraduationY) * m_pointY[i]), 6, 6);

                            g.DrawEllipse(pen, (m_margePixelX + (m_graduationPixelX / m_valGraduationX) * i) - 3,
                                           m_origine[1] - ((m_graduationPixelY / m_valGraduationY) * m_pointY[i]) - 3, 6, 6);
                        }

                        if (i != 0)
                        {

                            if (i == m_prochainPoint && m_enterDeuxPoints == true)
                            {

                                xCourant = m_margePixelX + (m_graduationPixelX / m_valGraduationX) * (i - 1) + m_compteur * (m_graduationPixelX / m_vitesseTrace);
                                xProchain = m_margePixelX + (m_graduationPixelX / m_valGraduationX) * (i - 1) + (m_compteur + 1) * (m_graduationPixelX / m_vitesseTrace);

                                pointX1 = m_margePixelX + (m_graduationPixelX / m_valGraduationX) * (i - 1);
                                pointY1 = m_origine[1] - ((m_graduationPixelY / m_valGraduationY) * m_pointY[i - 1]);
                                pointX2 = m_margePixelX + (m_graduationPixelX / m_valGraduationX) * i;
                                pointY2 = m_origine[1] - ((m_graduationPixelY / m_valGraduationY) * m_pointY[i]);

                                calculNouveauXY(ref pointX1, ref pointY1, ref pointX2, ref pointY2);

                                g.DrawLine(pen2, pointX1,
                                                                     pointY1,
                                                                     xCourant,
                                                                   (m_coefficient * xCourant) + m_zero);

                                g.DrawLine(pen2, xCourant,
                                                                   (m_coefficient * xCourant) + m_zero,
                                                                   xProchain,
                                                                   (m_coefficient * xProchain) + m_zero);
                            }

                            else
                            {
                                pointX1 = m_margePixelX + (m_graduationPixelX / m_valGraduationX) * (i - 1);
                                pointY1 = m_origine[1] - ((m_graduationPixelY / m_valGraduationY) * m_pointY[i - 1]);
                                pointX2 = m_margePixelX + (m_graduationPixelX / m_valGraduationX) * i;
                                pointY2 = m_origine[1] - ((m_graduationPixelY / m_valGraduationY) * m_pointY[i]);

                                calculNouveauXY(ref pointX1, ref pointY1, ref pointX2, ref pointY2);
                                g.DrawLine(pen2, pointX1, pointY1, pointX2, pointY2);

                            }

                        }

                    }

                    else
                    {
                        if (i != 0)
                        {
                            pointX1 = m_margePixelX + (m_graduationPixelX / m_valGraduationX) * (i - 1);
                            pointY1 = m_origine[1] - ((m_graduationPixelY / m_valGraduationY) * m_pointY[i - 1]);
                            pointX2 = m_margePixelX + (m_graduationPixelX / m_valGraduationX) * i;
                            pointY2 = m_origine[1] - ((m_graduationPixelY / m_valGraduationY) * m_pointY[i]);

                            calculNouveauXY(ref pointX1, ref pointY1, ref pointX2, ref pointY2);
                            g.DrawLine(pen2, pointX1, pointY1, pointX2, pointY2);
                        }

                        g.FillEllipse(brushPoint, m_margePixelX + (m_graduationPixelX / m_valGraduationX) * i - 3,
                                      m_origine[1] - 3 - ((m_graduationPixelY / m_valGraduationY) * m_pointY[i]), 6, 6);

                        g.DrawEllipse(pen, (m_margePixelX + (m_graduationPixelX / m_valGraduationX) * i) - 3,
                                       m_origine[1] - ((m_graduationPixelY / m_valGraduationY) * m_pointY[i]) - 3, 6, 6);

                    }
                }
                g.Dispose();
                brushPoint.Dispose();
                pen.Dispose();
                pen2.Dispose();
            }

            else
            {
                e.Graphics.DrawImage(m_background, new Point(0, 0));
            }

        }

        #endregion

        #region Méthode simulation

        private void calculVitesseTracage()
        {
            long tickTime = DateTime.Now.Ticks;
            bufferNouveauGraphique();
            m_ticksSimulation = DateTime.Now.Ticks - tickTime;
        }

        private void invalidateControl()
        {
            this.Invalidate();
            this.Refresh();
            this.Update();
        }

        public void sequenceDessin(int prochainPoint, int vitesseTrace, bool overrideTracage)
        {
            m_tracer = true;
            m_overideTracage = overrideTracage;

            if (prochainPoint == 1)
            {
                calculVitesseTracage();
            }

            if (!m_overideTracage)
            {
                double lol = TimeSpan.FromTicks(m_ticksSimulation).TotalSeconds;
                m_vitesseTrace = (int)((vitesseTrace/11) / TimeSpan.FromTicks(m_ticksSimulation).TotalSeconds);
                m_dernierPoint = prochainPoint - 1;
                m_prochainPoint = prochainPoint;
                trouverRegle();
                m_enterDeuxPoints = true;

                while (m_compteur < m_vitesseTrace)
                {
                    bufferNouveauGraphique();
                    m_compteur++;
                }

                m_compteur = 0;
                m_enterDeuxPoints = false;

            }

            else
            {
                m_dernierPoint = 10;
                m_prochainPoint = 11;
                bufferNouveauGraphique();
            }

            if (prochainPoint == m_pointY.Length - 1)
            {
                bufferNouveauGraphique();
            }

            m_overideTracage = true;

        }

        private void bufferNouveauGraphique()
        {
            Bitmap buffer = new Bitmap(this.Size.Width, this.Size.Height);

            using (Graphics gDone = this.CreateGraphics())
            {
                Graphics g = Graphics.FromImage(buffer);
                if (m_tracer)
                {
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                    lock (lockSimThread)
                    {
                        g.DrawImage(m_backGroundThread, new Point(0, 0));
                    }
                    SolidBrush brushPoint = new SolidBrush(Color.Black);
                    Pen pen = new Pen(Color.Black, 1);
                    Pen pen2 = new Pen(Color.Black, 2);
                    float xCourant,
                          xProchain,
                          pointX1,
                          pointY1,
                          pointX2,
                          pointY2;

                    for (int i = 0; i <= m_prochainPoint; i++)
                    {

                        if (!m_overideTracage)
                        {

                            if (i != m_prochainPoint)
                            {
                                g.FillEllipse(brushPoint, m_margePixelX + (m_graduationPixelX / m_valGraduationX) * i - 3,
                            m_origine[1] - 3 - ((m_graduationPixelY / m_valGraduationY) * m_pointY[i]), 6, 6);

                                g.DrawEllipse(pen, (m_margePixelX + (m_graduationPixelX / m_valGraduationX) * i) - 3,
                                               m_origine[1] - ((m_graduationPixelY / m_valGraduationY) * m_pointY[i]) - 3, 6, 6);
                            }

                            if (i == m_prochainPoint && m_prochainPoint == m_pointY.Length - 1 && m_enterDeuxPoints == false)
                            {

                                g.FillEllipse(brushPoint, m_margePixelX + (m_graduationPixelX / m_valGraduationX) * i - 3,
                                                m_origine[1] - 3 - ((m_graduationPixelY / m_valGraduationY) * m_pointY[i]), 6, 6);

                                g.DrawEllipse(pen, (m_margePixelX + (m_graduationPixelX / m_valGraduationX) * i) - 3,
                                               m_origine[1] - ((m_graduationPixelY / m_valGraduationY) * m_pointY[i]) - 3, 6, 6);
                            }

                            if (i != 0)
                            {

                                if (i == m_prochainPoint && m_enterDeuxPoints == true)
                                {

                                    pointX1 = m_margePixelX + (m_graduationPixelX / m_valGraduationX) * (i - 1);
                                    pointY1 = m_origine[1] - ((m_graduationPixelY / m_valGraduationY) * m_pointY[i - 1]);
                                    pointX2 = m_margePixelX + (m_graduationPixelX / m_valGraduationX) * i;
                                    pointY2 = m_origine[1] - ((m_graduationPixelY / m_valGraduationY) * m_pointY[i]);
                                    calculNouveauXY(ref pointX1, ref pointY1, ref pointX2, ref pointY2);

                                    xCourant = pointX1 + m_compteur * (m_graduationPixelX / m_vitesseTrace);
                                    xProchain = pointX1 + (m_compteur + 1) * (m_graduationPixelX / m_vitesseTrace);

                                    g.DrawLine(pen2, pointX1,
                                                     pointY1,
                                                     xCourant,
                                                     (m_coefficient * xCourant) + m_zero);

                                    g.DrawLine(pen2, xCourant,
                                                     (m_coefficient * xCourant) + m_zero,
                                                      xProchain,
                                                      (m_coefficient * xProchain) + m_zero);
                                }

                                else
                                {
                                    pointX1 = m_margePixelX + (m_graduationPixelX / m_valGraduationX) * (i - 1);
                                    pointY1 = m_origine[1] - ((m_graduationPixelY / m_valGraduationY) * m_pointY[i - 1]);
                                    pointX2 = m_margePixelX + (m_graduationPixelX / m_valGraduationX) * i;
                                    pointY2 = m_origine[1] - ((m_graduationPixelY / m_valGraduationY) * m_pointY[i]);

                                    calculNouveauXY(ref pointX1, ref pointY1, ref pointX2, ref pointY2);
                                    g.DrawLine(pen2, pointX1, pointY1, pointX2, pointY2);

                                }

                            }

                        }

                        else
                        {
                            if (i != 0)
                            {
                                pointX1 = m_margePixelX + (m_graduationPixelX / m_valGraduationX) * (i - 1);
                                pointY1 = m_origine[1] - ((m_graduationPixelY / m_valGraduationY) * m_pointY[i - 1]);
                                pointX2 = m_margePixelX + (m_graduationPixelX / m_valGraduationX) * i;
                                pointY2 = m_origine[1] - ((m_graduationPixelY / m_valGraduationY) * m_pointY[i]);

                                calculNouveauXY(ref pointX1, ref pointY1, ref pointX2, ref pointY2);
                                g.DrawLine(pen2, pointX1, pointY1, pointX2, pointY2);
                            }

                            g.FillEllipse(brushPoint, m_margePixelX + (m_graduationPixelX / m_valGraduationX) * i - 3,
                                          m_origine[1] - 3 - ((m_graduationPixelY / m_valGraduationY) * m_pointY[i]), 6, 6);

                            g.DrawEllipse(pen, (m_margePixelX + (m_graduationPixelX / m_valGraduationX) * i) - 3,
                                           m_origine[1] - ((m_graduationPixelY / m_valGraduationY) * m_pointY[i]) - 3, 6, 6);

                        }
                    }

                    gDone.DrawImage(buffer, 0, 0);
                    buffer.Dispose();
                    gDone.Dispose();
                    g.Dispose();
                    brushPoint.Dispose();
                    pen.Dispose();
                    pen2.Dispose();
                }

                else
                {
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    lock (lockSimThread)
                    {
                        g.DrawImage(m_backGroundThread, new Point(0, 0));
                    }

                    gDone.DrawImage(buffer, 0, 0);
                    buffer.Dispose();
                    gDone.Dispose();
                    g.Dispose();
                }

            }

        }

        private void trouverRegle()
        {
            float[] coordDernierPoint,
                    coordProchainPoint;

            coordDernierPoint = new[]{m_margePixelX + (m_graduationPixelX / m_valGraduationX) * m_dernierPoint,
                                m_origine[1] - ((m_graduationPixelY / m_valGraduationY) * m_pointY[m_dernierPoint])};

            coordProchainPoint = new[]{m_margePixelX + (m_graduationPixelX / m_valGraduationX) * m_prochainPoint,
                                m_origine[1] - ((m_graduationPixelY / m_valGraduationY) * m_pointY[m_prochainPoint])};


            m_coefficient = (coordProchainPoint[1] - coordDernierPoint[1]) / (coordProchainPoint[0] - coordDernierPoint[0]);
            m_zero = coordDernierPoint[1] - m_coefficient * coordDernierPoint[0];

        }

        private void calculNouveauXY(ref float pointX1, ref float pointY1, ref float pointX2, ref float pointY2)
        {
            double distance, teta;
            float nPointX1, nPointY1, nPointX2, nPointY2;

            distance = Math.Sqrt(Math.Pow(pointY2 - pointY1, 2) + Math.Pow(pointX2 - pointX1, 2));
            teta = Math.Asin((pointY2 - pointY1) / distance);
            nPointX1 = pointX1 + 3;
            nPointY1 = (float)(Math.Tan(teta) * (nPointX1 - pointX1) + pointY1);

            nPointX2 = pointX2 - 3;
            nPointY2 = (float)(Math.Tan(teta) * (nPointX2 - pointX2) + pointY2);
            pointX1 = nPointX1;
            pointX2 = nPointX2;
            pointY1 = nPointY1;
            pointY2 = nPointY2;

        }

        #endregion

        #region Méthode d'initialisation

        private void calculPopriete()
        {
            //Calcul les dimensions du graphiques en général

            //Calcul des grandeurs des axes

            const int graduationLIMIT = 25;

            float ptsMaxY = int.MinValue,
                  ptsMinY = int.MaxValue;
            float yCourant;

            m_margePixelX = (float)(this.Size.Width * 0.1);
            m_margePixelY = (float)(this.Size.Height * 0.15);

            m_grandeurPixelAxeX = this.Size.Width - 2 * m_margePixelX;
            m_grandeurPixelAxeY = this.Size.Height - 2 * m_margePixelY - (this.Size.Height / 4);


            //Trouve les points max et min en Y

            for (int i = 0; i < m_pointY.Length; i++)
            {
                yCourant = m_pointY[i];

                if (yCourant > ptsMaxY)
                {
                    ptsMaxY = yCourant;
                }

                if (yCourant < ptsMinY)
                {
                    ptsMinY = yCourant;
                }
            }

            ptsMaxY = (float)(Math.Round(ptsMaxY));
            ptsMinY = (float)(Math.Round(ptsMinY));
            m_deltaY = (int)(Math.Round(ptsMaxY - ptsMinY));

            //Déterminer valeur de graduation

            m_origine = new float[2];
            m_origine[0] = m_margePixelX;
            m_valGraduationX = 1; //un mois à la fois
            m_graduationPixelX = (float)(m_grandeurPixelAxeX / 11); // 11 - 0 = deltaX

            m_valGraduationY = 7;
            m_graduationPixelY = (int)(m_grandeurPixelAxeY / m_valGraduationY);

            //Nb de pixel utilisé pour chaque quadrant
            nbPixelQuadUn = (float)((m_graduationPixelY / m_valGraduationY) * Math.Abs(14));
            nbPixelQuadQuatre = (float)((m_graduationPixelY / m_valGraduationY) * Math.Abs(-28));

            m_origine[1] = (m_margePixelY + nbPixelQuadUn);

        }

        private void creerImageBackground()
        {
            m_background = new Bitmap(this.Size.Width, this.Size.Height);
            Graphics g = Graphics.FromImage(m_background);
            g.DrawImage(Image.FromFile(m_imgPath, true), 0, 0, this.Size.Width, this.Size.Height);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            Pen pen = new System.Drawing.Pen(Brushes.Black, 1F);
            Font fontGraduation = new Font(FontFamily.GenericSansSerif, 10);
            Font fontTitre = new Font(FontFamily.GenericSansSerif, 12);
            StringFormat formatGraduation = new StringFormat();
            SolidBrush brushPoint = new SolidBrush(Color.Black);
            formatGraduation.Alignment = StringAlignment.Far;
            float nbGraduationQuadUn,
                nbGraduationQuadQuatre,
                nbGraduationX,
                grandeurBarreGrad = m_margePixelY / 4,
                gradVal;

            //nom graphique

            SizeF rect = calculTailleString(m_nomGraphique, fontTitre);
            g.DrawString(m_nomGraphique, fontTitre, new SolidBrush(Color.Black), new PointF(this.Size.Width / 2 - rect.Width / 2, 15));

            //traçage des axes

            g.DrawLine(pen, m_margePixelX - grandeurBarreGrad, m_origine[1], this.Size.Width - m_margePixelX, m_origine[1]); //x (0)
            g.DrawLine(pen, m_margePixelX, m_origine[1] - nbPixelQuadUn, m_margePixelX, m_origine[1] + nbPixelQuadQuatre); //y


            // traçage de la graduation en Y et écriture pas graduation

            g.DrawString("0", fontGraduation, new SolidBrush(Color.Black), new RectangleF(m_margePixelX / 4, m_origine[1] - 7, m_margePixelX / 2, 50), formatGraduation);

            nbGraduationQuadUn = (float)Math.Round((m_origine[1] - m_margePixelY) / m_graduationPixelY);
            nbGraduationQuadQuatre = (float)(Math.Round((nbPixelQuadQuatre / m_graduationPixelY)));

            for (int i = 1; i <= nbGraduationQuadUn; i++)
            {
                gradVal = i * m_valGraduationY;

                g.DrawString(gradVal.ToString(), fontGraduation, new SolidBrush(Color.Black), new RectangleF(m_margePixelX / 4, m_origine[1] - i * m_graduationPixelY - 7, m_margePixelX / 2, 25), formatGraduation);
                g.DrawLine(pen, m_margePixelX - grandeurBarreGrad, m_origine[1] - i * m_graduationPixelY, m_margePixelX, m_origine[1] - i * m_graduationPixelY);
                g.DrawLine(pen, m_margePixelX, m_origine[1] - i * m_graduationPixelY, this.Size.Width - m_margePixelX, m_origine[1] - i * m_graduationPixelY);

            }

            for (int i = 1; i <= nbGraduationQuadQuatre; i++)
            {
                gradVal = -(i * m_valGraduationY);
                g.DrawLine(pen, m_margePixelX - grandeurBarreGrad, m_origine[1] + i * m_graduationPixelY, m_margePixelX, m_origine[1] + i * m_graduationPixelY);
                g.DrawString(gradVal.ToString(), fontGraduation, new SolidBrush(Color.Black), new RectangleF(m_margePixelX / 4, m_origine[1] + i * m_graduationPixelY - 7, m_margePixelX / 2, 50), formatGraduation);
                g.DrawLine(pen, m_margePixelX, m_origine[1] + i * m_graduationPixelY, this.Size.Width - m_margePixelX, m_origine[1] + i * m_graduationPixelY);
            }

            // traçage de la graduation en X et écriture pas graduation
            nbGraduationX = (float)Math.Round((m_grandeurPixelAxeX / m_graduationPixelX));
            nbGraduationQuadQuatre = (float)(Math.Round((nbPixelQuadQuatre / m_graduationPixelY))); //Obligé de ré-écrire sinon MVS détecte une erreur (non initialisée)

            for (int i = 0; i <= nbGraduationX; i++)
            {
                g.DrawLine(pen, m_margePixelX + i * m_graduationPixelX, m_origine[1] + nbGraduationQuadQuatre * m_graduationPixelY + grandeurBarreGrad, m_margePixelX + i * m_graduationPixelX,
                    m_origine[1] + nbGraduationQuadQuatre * m_graduationPixelY);
            }

            for (int i = 0; i <= nbGraduationX; i++)
            {
                ecrireNomRotationAxeX(ref g, m_pointX[i], new[] { i * m_graduationPixelX + m_margePixelX, m_origine[1] + grandeurBarreGrad + nbGraduationQuadQuatre * m_graduationPixelY }, -25);
            }

            m_backGroundThread = new Bitmap(m_background);
            //libère mémoire
            pen.Dispose();
            fontGraduation.Dispose();
            formatGraduation.Dispose();
            brushPoint.Dispose();
            g.Dispose();

        }

        private SizeF calculTailleString(string s, Font f)
        {
            System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(new Bitmap(1, 1));
            return graphics.MeasureString(s, f);
        }

        private void ecrireNomRotationAxeX(ref Graphics g, string texte, float[] coordOrigniale, int angleRotation)
        {
            const double PI = 3.14159;
            double x, beta, mu, teta;
            int posX, posY;
            Font fontGraduation;

            if (String.Compare(texte, m_moisSurligner) == 0)
            {
                fontGraduation = new Font(FontFamily.GenericSansSerif, 14);
            }

            else
            {
                fontGraduation = new Font(FontFamily.GenericSansSerif, 10);
            }

            StringFormat formatGraduation = new StringFormat();
            formatGraduation.Alignment = StringAlignment.Far;
            SizeF rect = calculTailleString(texte, fontGraduation);


            g.RotateTransform(angleRotation);
            teta = (PI * angleRotation) / 180;
            coordOrigniale[1] = ((float)(coordOrigniale[1] - Math.Sin(teta) * rect.Width));
            coordOrigniale[0] = ((float)(coordOrigniale[0] - Math.Cos(teta) * rect.Width));

            x = Math.Sqrt(coordOrigniale[0] * coordOrigniale[0] + coordOrigniale[1] * coordOrigniale[1]);
            beta = Math.Asin(coordOrigniale[1] / x);
            mu = beta - teta;

            posX = (int)(Math.Cos(mu) * x);
            posY = (int)(Math.Sin(mu) * x);

            g.DrawString(texte, fontGraduation, Brushes.Black, new RectangleF(posX, posY, rect.Width, rect.Height));
            g.ResetTransform();
            fontGraduation.Dispose();

        }

        #endregion

    }
}
