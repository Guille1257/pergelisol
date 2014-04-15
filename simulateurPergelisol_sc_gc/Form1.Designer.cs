namespace simulateurPergelisol_alpha_0._1
{
    using System.ComponentModel.Design;
    partial class Form1
    {
        /// <summary>
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur Windows Form

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.panelGraphique = new System.Windows.Forms.Panel();
            this.buttonDemarrer = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.villageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.typeDeSolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.couvertureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.buttonFin = new System.Windows.Forms.Button();
            this.buttonDebut = new System.Windows.Forms.Button();
            this.panelTableau = new System.Windows.Forms.Panel();
            this.menuStrip1.SuspendLayout();
            this.panelTableau.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelGraphique
            // 
            this.panelGraphique.BackColor = System.Drawing.Color.DarkRed;
            this.panelGraphique.ForeColor = System.Drawing.SystemColors.ControlText;
            this.panelGraphique.Location = new System.Drawing.Point(0, 24);
            this.panelGraphique.Margin = new System.Windows.Forms.Padding(0);
            this.panelGraphique.Name = "panelGraphique";
            this.panelGraphique.Size = new System.Drawing.Size(799, 261);
            this.panelGraphique.TabIndex = 1;
            // 
            // buttonDemarrer
            // 
            this.buttonDemarrer.Location = new System.Drawing.Point(408, 291);
            this.buttonDemarrer.Name = "buttonDemarrer";
            this.buttonDemarrer.Size = new System.Drawing.Size(102, 37);
            this.buttonDemarrer.TabIndex = 3;
            this.buttonDemarrer.UseVisualStyleBackColor = true;
            this.buttonDemarrer.Click += new System.EventHandler(this.button1_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.villageToolStripMenuItem,
            this.typeDeSolToolStripMenuItem,
            this.couvertureToolStripMenuItem,
            this.optionsToolStripMenuItem});
            this.menuStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(799, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // villageToolStripMenuItem
            // 
            this.villageToolStripMenuItem.Name = "villageToolStripMenuItem";
            this.villageToolStripMenuItem.Size = new System.Drawing.Size(54, 20);
            this.villageToolStripMenuItem.Text = "Village";
            // 
            // typeDeSolToolStripMenuItem
            // 
            this.typeDeSolToolStripMenuItem.Name = "typeDeSolToolStripMenuItem";
            this.typeDeSolToolStripMenuItem.Size = new System.Drawing.Size(79, 20);
            this.typeDeSolToolStripMenuItem.Text = "Type de sol";
            // 
            // couvertureToolStripMenuItem
            // 
            this.couvertureToolStripMenuItem.Name = "couvertureToolStripMenuItem";
            this.couvertureToolStripMenuItem.Size = new System.Drawing.Size(78, 20);
            this.couvertureToolStripMenuItem.Text = "Couverture";
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.optionsToolStripMenuItem.Text = "Options";
            this.optionsToolStripMenuItem.Click += new System.EventHandler(this.optionsToolStripMenuItem_Click);
            // 
            // buttonFin
            // 
            this.buttonFin.Location = new System.Drawing.Point(516, 291);
            this.buttonFin.Name = "buttonFin";
            this.buttonFin.Size = new System.Drawing.Size(102, 37);
            this.buttonFin.TabIndex = 3;
            this.buttonFin.UseVisualStyleBackColor = true;
            // 
            // buttonDebut
            // 
            this.buttonDebut.Location = new System.Drawing.Point(300, 291);
            this.buttonDebut.Name = "buttonDebut";
            this.buttonDebut.Size = new System.Drawing.Size(102, 37);
            this.buttonDebut.TabIndex = 3;
            this.buttonDebut.UseVisualStyleBackColor = true;
            // 
            // panelTableau
            // 
            this.panelTableau.BackColor = System.Drawing.Color.White;
            this.panelTableau.Controls.Add(this.buttonDemarrer);
            this.panelTableau.Controls.Add(this.buttonFin);
            this.panelTableau.Controls.Add(this.buttonDebut);
            this.panelTableau.Location = new System.Drawing.Point(0, 285);
            this.panelTableau.Margin = new System.Windows.Forms.Padding(0);
            this.panelTableau.Name = "panelTableau";
            this.panelTableau.Size = new System.Drawing.Size(799, 332);
            this.panelTableau.TabIndex = 4;
            this.panelTableau.Paint += new System.Windows.Forms.PaintEventHandler(this.panelTableau_draw);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.ClientSize = new System.Drawing.Size(799, 617);
            this.Controls.Add(this.panelTableau);
            this.Controls.Add(this.panelGraphique);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "Simulateur";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.panelTableau.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panelGraphique;
        private System.Windows.Forms.Button buttonDemarrer;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem villageToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem typeDeSolToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem couvertureToolStripMenuItem;
        private System.Windows.Forms.Button buttonFin;
        private System.Windows.Forms.Button buttonDebut;
        private System.Windows.Forms.Panel panelTableau;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
    }
}

