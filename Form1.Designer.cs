using System.Drawing;
using System.Windows.Forms;

namespace GPSTracker
{
    partial class Form1
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.mapCard = new System.Windows.Forms.PictureBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.mapCard)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 50;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // mapCard
            // 
            this.mapCard.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.mapCard.Location = new System.Drawing.Point(12, 9);
            this.mapCard.MinimumSize = new System.Drawing.Size(255, 255);
            this.mapCard.Name = "mapCard";
            this.mapCard.Size = new System.Drawing.Size(765, 765);
            this.mapCard.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.mapCard.TabIndex = 11;
            this.mapCard.TabStop = false;
            this.mapCard.Click += new System.EventHandler(this.mapCard_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox1.Location = new System.Drawing.Point(12, 39);
            this.pictureBox1.MinimumSize = new System.Drawing.Size(255, 255);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(765, 765);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 12;
            this.pictureBox1.TabStop = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(787, 780);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.mapCard);
            this.DoubleBuffered = true;
            this.Name = "Form1";
            this.Text = "GPSTracker";
            ((System.ComponentModel.ISupportInitialize)(this.mapCard)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ToolTip toolTip1;

        private System.Windows.Forms.PictureBox mapCard;
        private PictureBox pictureBox1;
    }
}

