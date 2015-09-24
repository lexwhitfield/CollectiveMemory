namespace Harvester
{
    partial class Service1
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.TweetHarvestTimer = new System.Timers.Timer();
            this.StatisticGeneratorTimer = new System.Timers.Timer();
            ((System.ComponentModel.ISupportInitialize)(this.TweetHarvestTimer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.StatisticGeneratorTimer)).BeginInit();
            // 
            // TweetHarvestTimer
            // 
            this.TweetHarvestTimer.Enabled = true;
            this.TweetHarvestTimer.Interval = 7200000D;
            this.TweetHarvestTimer.Elapsed += new System.Timers.ElapsedEventHandler(this.TweetHarvestTimer_Elapsed);
            // 
            // StatisticGeneratorTimer
            // 
            this.StatisticGeneratorTimer.Enabled = true;
            this.StatisticGeneratorTimer.Interval = 14400000D;
            this.StatisticGeneratorTimer.Elapsed += new System.Timers.ElapsedEventHandler(this.StatisticGeneratorTimer_Elapsed);
            // 
            // Service1
            // 
            this.ServiceName = "Service1";
            ((System.ComponentModel.ISupportInitialize)(this.TweetHarvestTimer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.StatisticGeneratorTimer)).EndInit();

        }

        #endregion
        private System.Timers.Timer TweetHarvestTimer;
        private System.Timers.Timer StatisticGeneratorTimer;
    }
}
