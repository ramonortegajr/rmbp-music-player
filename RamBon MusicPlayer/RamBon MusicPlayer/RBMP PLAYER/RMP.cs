using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Speech.Synthesis;
using System.Media;
using System.Diagnostics;

namespace RamBon_MusicPlayer
{
    //**********************************
    //DEVELOPER : RAMON ORTEGA JR//
    //GMAIL : ortegaram22@gmail.com//
    //**********************************

    public partial class RMP : Form
    {
        SpeechSynthesizer ss = new SpeechSynthesizer();
        System.Timers.Timer timer;
        public RMP()
        {

            InitializeComponent();
            ss.SelectVoiceByHints(VoiceGender.Female);
            song.Visible = false;
            lblstatus.Visible = false;
            muted.Visible = false;
            btnvolmuted.Visible = false;
            btnvol1.Visible = false;
            btnvoldown.Visible = false;
            panelalarm.Visible = false;
            pnlPlaylist.Visible = false;
        }

        //kani na code para ni sa alarm panel nga kung open na sya e close niya og close sya e open ni sya
        private void showPanel(Panel pnl)
        {
            if (pnl.Visible == false)
            {
                pnl.Visible = true;
            }
            else
            {
                pnl.Visible = false;
            }
        }


        //global variable para sa listahan sa mga kanta nato
        String[] path, files;
       
        private void Form1_Load(object sender, EventArgs e)
        {
            btnpause.Visible = false;
            ss.Rate = 0;
            ss.Volume = 100;
            ss.SpeakAsync("Welcome to Ram Bon Music Player");
            wp.settings.volume = 50;
            lblvolume.Text = trackBar1.Value.ToString() + "%";
            #region alarm_code
            timer = new System.Timers.Timer();
            timer.Interval = 1000;
            timer.Elapsed += Timer_Elapse;
            btnAlarmInOff.Visible = true;
            btnAlarmOff.Visible = false;
            #endregion
        }

        delegate void UpdateLabel(Label lbl, string value);
        void UpdateDataLabel(Label lbl, string value)
        {
            lbl.Text = value;
        }

        //code sa time elapse para sa atong alarm
        private void Timer_Elapse(object sender, System.Timers.ElapsedEventArgs e)
        {
            DateTime currentTime = DateTime.Now;
            DateTime userTime = alrmtime.Value;

            if (currentTime.Hour == userTime.Hour && currentTime.Minute == userTime.Minute && currentTime.Second == userTime.Second)
            {
                timer.Stop();

                try
                {
                    UpdateLabel upd = UpdateDataLabel;
                    if (alrmstatus.InvokeRequired)
                        Invoke(upd, alrmstatus, "OFF");

                    //kill the application using this alarm
                    killRMP("RamBon MusicPlayer");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        //mao ni mag close sa application nato
        public static void killRMP(string s)
        {
            System.Diagnostics.Process[] procs = null;
            try
            {
                procs = Process.GetProcessesByName(s);
                Process prog = procs[0];
                if (!prog.HasExited)
                {
                    prog.Kill();
                }
            }
            finally
            {
                if (procs != null)
                {
                    foreach (Process p in procs)
                    {
                        p.Dispose();
                    }
                }
            }
        }

        //VOLUME UP AND VOLUME  DOWN CODE
        private const int APPCOMMAND_VOLUME_UP = 0xA0000;
        private const int APPCOMMAND_VOLUME_DOWN = 0x90000;
        private const int WP_APPCOMMAND = 0x319;

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        //para ma hide atoang form
        public void MinimizeForm(System.Windows.Forms.Form frmName)
        {
            if (frmName.WindowState == System.Windows.Forms.FormWindowState.Normal)
            {
                frmName.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            }
            else if (frmName.WindowState == System.Windows.Forms.FormWindowState.Maximized)
            {
                frmName.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            }
        }

        //para mudako atoang form
        public void MaximizedForm(System.Windows.Forms.Form FrmName)
        {
            if (FrmName.WindowState == System.Windows.Forms.FormWindowState.Normal)
            {
                FrmName.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            }
            else if (FrmName.WindowState == System.Windows.Forms.FormWindowState.Maximized)
            {
                FrmName.WindowState = System.Windows.Forms.FormWindowState.Normal;
            }
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            //para ni sa pag close sa application
            this.Close();
        }

        //make playlist
        string myPlaylist = "";
        private void makePlaylist()
        {
            WMPLib.IWMPPlaylist playlist = wp.playlistCollection.newPlaylist(myPlaylist);
            WMPLib.IWMPMedia media;

            ofdd.Multiselect = true;
            if (ofdd.ShowDialog() == DialogResult.OK)
            {
                foreach (string file in ofdd.FileNames)
                {
                    media = wp.newMedia(file);
                    playlist.appendItem(media);
                }

                ListSong.Items.Clear();
                files = ofdd.SafeFileNames; //code ni para paglista sa title sa kanta adto sa file array
                path = ofdd.FileNames; //code ni para sa file path to array

                //display ag title sa kanta sa listbox
                for (int i = 0; i < files.Length; i++)
                {
                    ListSong.Items.Add(files[i]); //display title sa kanta
                }
            }

            ListSong.SelectedIndex = ListSong.SelectedIndex + 1;
            wp.currentPlaylist = playlist;
            wp.Ctlcontrols.play();
            txtplaylist.Text = "";
            song.Visible = true;
            song.Text = ListSong.SelectedItem.ToString();
            btnpause.Visible = true;
            btnplay.Visible = false;
        }

        //browse for a song
        private void bunifuButton1_Click(object sender, EventArgs e)
        {
            showPanel(pnlPlaylist);
        }

        //kani nga code para ni sa pag play sa kanta inag change index
        private void ListSong_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void ListSong_Click(object sender, EventArgs e)
        {
            try
            {
                if (ListSong.Text == "")
                {
                    //og way sulod atong listbox e open niya ag file dialog para pili tag kanta
                    try
                    {

                        using (OpenFileDialog ofd = new OpenFileDialog())
                        {
                            ofd.Filter = "Mp3 Files|*.mp3|Mpeg4-Audio Files|*.m4a|Wav File|*.wav";
                            ofd.Multiselect = true;
                            if (ofd.ShowDialog() == DialogResult.OK)
                            {
                                ListSong.Items.Clear();
                                files = ofd.SafeFileNames; //code ni para paglista sa title sa kanta adto sa file array
                                path = ofd.FileNames; //code ni para sa file path to array

                                //display ag title sa kanta sa listbox
                                for (int i = 0; i < files.Length; i++)
                                {
                                    ListSong.Items.Add(files[i]); //display title sa kanta
                                    lblstatus.Visible = true;

                                    if ((ListSong.Items.Count == 1))
                                    {
                                        lblstatus.Text = (ListSong.Items.Count.ToString()) + " song";
                                    }
                                    else
                                    {
                                        lblstatus.Text = (ListSong.Items.Count.ToString()) + " songs";
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    this.ActiveControl = txtsearch;
                }

                if (btnvisual1.Visible == true)
                {
                    wp.URL = path[ListSong.SelectedIndex];
                    song.Visible = true;
                    song.Text = ListSong.SelectedItem.ToString();
                    visualize1.Visible = false;
                    pds.Visible = false;
                    pd.Visible = false;
                    defolt.Visible = false;
                    wp.Visible = true;
                    btnplay.Visible = false;
                    btnpause.Visible = true;
                }
                else
                {
                    wp.URL = path[ListSong.SelectedIndex];
                    song.Visible = true;
                    song.Text = ListSong.SelectedItem.ToString();
                    visualize1.Visible = true;
                    pds.Visible = false;
                    pd.Visible = false;
                    defolt.Visible = false;
                    wp.Visible = false;
                    btnplay.Visible = false;
                    btnpause.Visible = true;

                }
            }
            catch (Exception)
            {

            }
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            MinimizeForm(this);
        }

        private void wd_Enter(object sender, EventArgs e)
        {

        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {

        }
        private void timer1_Tick(object sender, EventArgs e)
        {
        }

        private void timer2_Tick(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {
        }

        private void bunifuButton2_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Press Ctrl + A to multiselect song.", "RBMP", MessageBoxButtons.OK, MessageBoxIcon.Information);

            try
            {

                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Filter = "Mp3 Files|*.mp3|Mpeg4-Audio Files|*.m4a|Wav File|*.wav";
                    ofd.Multiselect = true;
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        ListSong.Items.Clear();
                        files = ofd.SafeFileNames; //code ni para paglista sa title sa kanta adto sa file array
                        path = ofd.FileNames; //code ni para sa file path to array

                        //display ag title sa kanta sa listbox
                        for (int i = 0; i < files.Length; i++)
                        {
                            ListSong.Items.Add(files[i]); //display title sa kanta
                            lblstatus.Visible = true;
                            if ((ListSong.Items.Count == 1))
                            {
                                lblstatus.Text = (ListSong.Items.Count.ToString()) + " song";
                            }
                            else
                            {
                                lblstatus.Text = (ListSong.Items.Count.ToString()) + " songs";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            this.ActiveControl = txtsearch;

        }

        //fast reverse sa kanta
        private void bunifuImageButton1_Click(object sender, EventArgs e)
        {
          
        }

        private void bunifuImageButton3_Click(object sender, EventArgs e)
        {
            try
            {
                if (ListSong.Text == "")
                {
                    MessageBox.Show("Please select song to play", "RBMP", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    if (btnvisual.Visible == true)
                    {
                        wp.Ctlcontrols.play();
                        song.Visible = true;
                        song.Text = ListSong.SelectedItem.ToString();
                        btnplay.Visible = false;
                        btnpause.Visible = true;
                        visualize1.Visible = true;
                        pds.Visible = false;
                        pd.Visible = false;
                    }
                    else
                    {
                        wp.Ctlcontrols.play();
                        song.Visible = true;
                        song.Text = ListSong.SelectedItem.ToString();
                        btnplay.Visible = false;
                        btnpause.Visible = true;
                        visualize1.Visible = false;
                        pds.Visible = false;
                        pd.Visible = false;
                        defolt.Visible = false;
                        wp.Visible = true;
                    }

                }

            }
            catch (Exception)
            {

            }
        }

        private void bunifuImageButton2_Click(object sender, EventArgs e)
        {
            //kani nga code para sa pag previous ni sya
            if (ListSong.Items.Count == 0)
            {
                MessageBox.Show("There is no song in the list.", "RMP", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
            else if (ListSong.SelectedIndex > 0)
            {
                ListSong.SelectedIndex = ListSong.SelectedIndex - 1;
                wp.URL = path[ListSong.SelectedIndex];
                song.Visible = true;
                song.Text = ListSong.SelectedItem.ToString();
                txtsearch.Text = "";
                lblstatus.Visible = true;
                lblstatus.Text = (ListSong.Items.Count.ToString()) + " songs";
            }
        }

        private void bunifuImageButton4_Click(object sender, EventArgs e)
        {
            //kani nga code para ni sa pag next
            if (ListSong.Items.Count == 0)
            {
                MessageBox.Show("There is no song in the list.", "RMP", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
            else if (ListSong.SelectedIndex < ListSong.Items.Count - 1)
            {
                ListSong.SelectedIndex = ListSong.SelectedIndex + 1;
                wp.URL = path[ListSong.SelectedIndex];
                song.Visible = true;
                song.Text = ListSong.SelectedItem.ToString();
                btnpause.Visible = true;
                btnplay.Visible = false;
                txtsearch.Text = "";
                lblstatus.Visible = true;
                lblstatus.Text = (ListSong.Items.Count.ToString()) + " songs";
            }
        }

        private void bunifuImageButton6_Click(object sender, EventArgs e)
        {
            //kani na code sa pag stop ni sya
            wp.Ctlcontrols.stop();
            song.Text = "No Song Playing";
            btnpause.Visible = false;
            btnplay.Visible = true;
            visualize1.Visible = false;
            pds.Visible = true;
            pd.Visible = true;
            uptime.Text = "00:00";
        }



        private void bunifuImageButton7_Click(object sender, EventArgs e)
        {
        }

        private void rectangleShape2_Click(object sender, EventArgs e)
        {

        }

        private void btnvolup_Click(object sender, EventArgs e)
        {
            //SendMessage(this.Handle, WP_APPCOMMAND, this.Handle, (IntPtr)APPCOMMAND_VOLUME_UP);
        }

        private void btnvoldown_Click(object sender, EventArgs e)
        {
            wp.settings.volume = 0;
            muted.Visible = true;
            btnvolmuted.Visible = true;

        }

        private void wp_Enter(object sender, EventArgs e)
        {

        }

        private void btnpause_Click(object sender, EventArgs e)
        {
          
            //kani na code sa pag pause ni sya
            if (btnvisual.Visible == true)
            {
                wp.Ctlcontrols.pause();
                btnplay.Visible = true;
                btnpause.Visible = false;
                visualize1.Visible = false;
                pds.Visible = true;
                pd.Visible = true;
            }
            else
            {
                wp.Ctlcontrols.pause();
                btnplay.Visible = true;
                btnpause.Visible = false;
                visualize1.Visible = false;
                pds.Visible = false;
                pd.Visible = false;
                defolt.Visible = false;
                wp.Visible = true;
            }

        }

        private void bunifuImageButton8_Click(object sender, EventArgs e)
        {
        }

        private void bunifuImageButton5_Click(object sender, EventArgs e)
        {

        }
        private void wp_PlayStateChange(object sender, AxWMPLib._WMPOCXEvents_PlayStateChangeEvent e)
        {
            //para ni sa progress bar sa pagdagan niya para mailhan og asa na ag kanta dapit
            if (wp.playState == WMPLib.WMPPlayState.wmppsPlaying)
            {
                //  progressBar1.Maximum = (int)wp.Ctlcontrols.currentItem.duration;
                pb1.MaximumValue = (int)wp.Ctlcontrols.currentItem.duration;
                timer1.Start();

                if (btnvisual1.Visible == true)
                {

                    visualize1.Visible = false;
                    pds.Visible = false;
                    pd.Visible = false;
                    defolt.Visible = false;
                    wp.Visible = true;

                }
                else
                {

                    visualize1.Visible = true;
                    pds.Visible = false;
                    pd.Visible = false;
                    defolt.Visible = false;
                    wp.Visible = false;


                }
            }
            //code ni nga muhnong pod ag progressbar og imo e pause
            else if (wp.playState == WMPLib.WMPPlayState.wmppsPaused)
            {
                timer1.Stop();
            }
            //para ni sa pag stopped nga muhonong pati oras og progress bar
            else if (wp.playState == WMPLib.WMPPlayState.wmppsStopped)
            {
                timer1.Stop();
                pb1.Value = 0;
                uptime.Text = "00:00";
            }

            else if (wp.playState == WMPLib.WMPPlayState.wmppsMediaEnded)
            {
                if (btnrepeatall.Visible == true)
                {
                    timer3.Enabled = true;
                }
                else
                {
                    timer4.Enabled = true;
                }
            }
        }

        private void timer1_Tick_1(object sender, EventArgs e)
        {
            uptime.Text = wp.Ctlcontrols.currentPositionString;
            endtime.Text = wp.Ctlcontrols.currentItem.durationString.ToString();

            if (wp.playState == WMPLib.WMPPlayState.wmppsPlaying)
            {
                pb1.Value = (int)wp.Ctlcontrols.currentPosition;
            }
        }

        private void bunifuButton3_Click(object sender, EventArgs e)
        {
            //open nato ag alarm panel
            showPanel(panelalarm);
        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }

        private void bunifuButton4_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Equilizer is available in Version 2.0", "RBMP PLAYER", MessageBoxButtons.OK, MessageBoxIcon.Information);//no code
        }

        private void rectangleShape1_Click(object sender, EventArgs e)
        {

        }

        //kani nga code mao atoang volume 
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            wp.settings.volume = trackBar1.Value;
            lblvolume.Text = trackBar1.Value.ToString() + "%";
            muted.Visible = false;
            btnvolmuted.Visible = false;
            if (trackBar1.Value == 0)
            {
                btnvolmuted.Visible = true;
                btnvoldown.Visible = false;
                btnvol1.Visible = false;
                btnvol2.Visible = false;
            }
            else if (trackBar1.Value == 1)
            {
                btnvolmuted.Visible = false;
                btnvol1.Visible = false;
                btnvol2.Visible = false;
                btnvoldown.Visible = true;
            }
            else if (trackBar1.Value == 5)
            {
                btnvolmuted.Visible = false;
                btnvol1.Visible = false;
                btnvol2.Visible = false;
                btnvoldown.Visible = true;
            }
            else if (trackBar1.Value == 20)
            {
                btnvolmuted.Visible = false;
                btnvol1.Visible = false;
                btnvol2.Visible = false;
                btnvoldown.Visible = true;
            }
            else if (trackBar1.Value == 50)
            {
                btnvoldown.Visible = false;
                btnvolmuted.Visible = false;
                btnvol1.Visible = false;
                btnvol2.Visible = true;
            }

            else if (trackBar1.Value == 100)
            {
                btnvoldown.Visible = false;
                btnvolmuted.Visible = false;
                btnvol1.Visible = true;
                btnvol2.Visible = false;
            }

        }

        private void ListSong_DoubleClick(object sender, EventArgs e)
        {

        }

        private void ListSong_SelectedValueChanged(object sender, EventArgs e)
        {

        }


        private void bunifuImageButton5_Click_1(object sender, EventArgs e)
        {

        }

        private void pb1_progressChanged(object sender, EventArgs e)
        {

        }

        private void endtime_TextChanged(object sender, EventArgs e)
        {

        }

        private void pictureBox5_Click_1(object sender, EventArgs e)
        {
            MaximizedForm(this);
        }

        private void visualize1_Click(object sender, EventArgs e)
        {

        }

        private void endtime_Click(object sender, EventArgs e)
        {

        }


        private void pb1_MouseClick(object sender, MouseEventArgs e)
        {

        }

        private void trckbar_ValueChanged(object sender, EventArgs e)
        {

        }

        private void trckbar_KeyUp(object sender, KeyEventArgs e)
        {


        }

        private void trckbar_Move(object sender, EventArgs e)
        {

        }

        private void wp_MediaChange(object sender, AxWMPLib._WMPOCXEvents_MediaChangeEvent e)
        {

        }

        //para ni sa pag search nato sa kanta nga naa sa listbox
        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void bunifuButton5_Click(object sender, EventArgs e)
        {
            FrmDeveloper dev = new FrmDeveloper();
            dev.Show();
        }

        private void txtsearch_KeyUp(object sender, KeyEventArgs e)
        {

        }
        private void txtsearch_TextChanged(object sender, EventArgs e)
        {

        }

        //fast forward sa kanta 
        private void bunifuImageButton3_Click_1(object sender, EventArgs e)
        {
            try
            {
                wp.Ctlcontrols.fastForward();
                pb1.Value = (int)wp.Ctlcontrols.currentPosition;
                btnpause.Visible = false;
                btnplay.Visible = true;
            }
            catch (Exception)
            {

            }
        }


        //visualizer ni nato sa screen
        private void bunifuImageButton5_Click_2(object sender, EventArgs e)
        {
            btnvisual.Visible = false;
            btnvisual1.Visible = true;

            try
            {

                visualize1.Visible = false;
                pds.Visible = false;
                pd.Visible = false;
                defolt.Visible = false;
                wp.Visible = true;

            }
            catch (Exception)
            {

            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (txtsearch.Text == "")
                {
                    MessageBox.Show("Textbox must filled.", "RBMP", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    ListSong.SelectedItems.Clear();
                    for (int i = ListSong.Items.Count - 1; i >= 0; i--)
                    {
                        if (ListSong.Items[i].ToString().ToLower().Contains(txtsearch.Text.ToLower()))
                        {
                            ListSong.SetSelected(i, true);
                        }
                    }
                }
                lblstatus.Visible = true;
                lblstatus.Text = ListSong.SelectedItems.Count.ToString() + " song found";
            }

            catch (Exception)
            {

            }
            return;
        }

        //timer nato ni nga mag play automatically sa atong song okey
        private void timer3_Tick(object sender, EventArgs e)
        {
            try
            {
                ListSong.SelectedIndex = ListSong.SelectedIndex + 1;//kani na code mag increment ni sa kanta aron mu next
                wp.URL = path[ListSong.SelectedIndex];//kani na code mao ni mag trigger para mo play ag selected index song
                song.Visible = true;
                song.Text = ListSong.SelectedItem.ToString();
                txtsearch.Clear();// clear sa txtsearch song
                lblstatus.Visible = false;
                timer3.Enabled = false;
            }
            catch (Exception)
            {

            }
        }

        private void RBMP_KeyDown(object sender, KeyEventArgs e)
        {
        }

        private void label7_Click(object sender, EventArgs e)
        {

        }

        private void btnvolmuted_Click(object sender, EventArgs e)
        {
            if (muted.Visible == true)
            {
                wp.settings.volume = trackBar1.Value;
                btnvolmuted.Visible = false;
                btnvol2.Visible = true;
                btnvol1.Visible = false;
                muted.Visible = false;
            }
            else
            {
                wp.settings.volume = 0;
                btnvolmuted.Visible = true;
                btnvol2.Visible = false;
                btnvol1.Visible = false;
                muted.Visible = true;
            }
        }

        private void btnvol2_Click(object sender, EventArgs e)
        {
            wp.settings.volume = 0;
            muted.Visible = true;
            btnvolmuted.Visible = true;
        }

        private void btnvoldown_Click_1(object sender, EventArgs e)
        {
            if (muted.Visible == true)
            {
                wp.settings.volume = trackBar1.Value;
                btnvolmuted.Visible = false;
                btnvol2.Visible = true;
                btnvol1.Visible = false;
                muted.Visible = false;
            }
            else
            {
                wp.settings.volume = 0;
                btnvolmuted.Visible = true;
                btnvol2.Visible = false;
                btnvol1.Visible = false;
                muted.Visible = true;
            }
        }

        private void btnrepeatonce_Click(object sender, EventArgs e)
        {
            btnrepeatonce.Visible = false;
            btnrepeatall.Visible = true;
        }

        //repeat the one song until its loop
        private void timer4_Tick(object sender, EventArgs e)
        {
            try
            {
                ListSong.SelectedIndex = ListSong.SelectedIndex;//kani na code mag increment ni sa kanta aron mu next
                wp.URL = path[ListSong.SelectedIndex];//kani na code mao ni mag trigger para mo play ag selected index song
                song.Visible = true;
                song.Text = ListSong.SelectedItem.ToString();
                txtsearch.Clear();// clear sa txtsearch song
                lblstatus.Visible = false;
                timer4.Enabled = false;
            }
            catch (Exception)
            {

            }

        }

        //repeat all song tanan ng dretso2x
        private void btnrepeatall_Click(object sender, EventArgs e)
        {
            btnrepeatonce.Visible = true;
            btnrepeatall.Visible = false;
        }

        private void btnunshuffle_Click(object sender, EventArgs e)
        {
            if (cbcolor.Visible == false)
            {
                cbcolor.Visible = true;
            }
            else
            {
                cbcolor.Visible = false;
            }
        }

        private void btnshuffle_Click(object sender, EventArgs e)
        {

        }

        private void btnvisual1_Click(object sender, EventArgs e)
        {
            btnvisual.Visible = true;
            btnvisual1.Visible = false;

            try
            {

                visualize1.Visible = true;
                pds.Visible = false;
                pd.Visible = false;
                defolt.Visible = false;
                wp.Visible = false;

            }
            catch (Exception)
            {

            }
        }
        //check if forms is na open na or wla pa
        private void openfrm(Form frm)
        {
            frm.Show();
        }

        private void bunifuButton2_MouseHover(object sender, EventArgs e)
        {
            panelalarm.Visible = false;
            pnlPlaylist.Visible = false;
        }

        private void bunifuButton1_MouseHover(object sender, EventArgs e)
        {
            panelalarm.Visible = false;

        }

        private void bunifuButton3_MouseHover(object sender, EventArgs e)
        {
            panelalarm.Visible = false;
            pnlPlaylist.Visible = false;
        }

        private void bunifuButton4_MouseHover(object sender, EventArgs e)
        {
            panelalarm.Visible = false;
            pnlPlaylist.Visible = false;
        }

        //alarm on
        private void btnAlarmOn_Click(object sender, EventArgs e)
        {
            timer.Start();
            btnAlarmInOn.Visible = true;
            btnAlarmOn.Visible = false;
            alrmstatus.Text = "ON";
            btnAlarmInOff.Visible = false;
            btnAlarmOff.Visible = true;
            ss.Rate = 0;
            ss.Volume = 100;
            ss.SpeakAsync("Alarm is on");
            MessageBox.Show("RBMP PLAYER WILL STOP AT " + alrmtime.Value.ToLongTimeString(), "RBMP PLAYER", MessageBoxButtons.OK, MessageBoxIcon.Information);
            panelalarm.Visible = false;
        }

        //alarm off
        private void btnAlarmOff_Click(object sender, EventArgs e)
        {
            timer.Stop();
            btnAlarmOff.Visible = false;
            btnAlarmInOff.Visible = true;
            alrmstatus.Text = "OFF";
            btnAlarmInOn.Visible = false;
            btnAlarmOn.Visible = true;
            ss.Rate = 0;
            ss.Volume = 100;
            ss.SpeakAsync("Alarm is off");
            MessageBox.Show("ALARM STOP", "RBMP PLAYER", MessageBoxButtons.OK, MessageBoxIcon.Information);
            panelalarm.Visible = false;
        }

        private void btnAlarmInOn_Click(object sender, EventArgs e)
        {
            btnAlarmInOff.Visible = false;
            btnAlarmOff.Visible = true;
        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Playlist location Music\'Playlist", "RBMP", MessageBoxButtons.OK, MessageBoxIcon.Information);
            try
            {

                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    ofd.Filter = "RBMP Playlist|*.wpl";
                    ofd.Multiselect = true;
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        ListSong.Items.Clear();
                        files = ofd.SafeFileNames; //code ni para paglista sa title sa kanta adto sa file array
                        path = ofd.FileNames; //code ni para sa file path to array

                        //display ag title sa kanta sa listbox
                        for (int i = 0; i < files.Length; i++)
                        {
                            ListSong.Items.Add(files[i]); //display title sa kanta
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            pnlPlaylist.Visible = false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (txtplaylist.Text == "")
            {
                MessageBox.Show("Playlist title required.", "RBMP", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                myPlaylist = txtplaylist.Text + " RBMP Playlist";
                makePlaylist();
                pnlPlaylist.Visible = false;
            }
        }

        private void panelalarm_Paint(object sender, PaintEventArgs e)
        {

        }


        //kani na code para ni sa theme
        private void cbcolor_SelectedIndexChanged_1(object sender, EventArgs e)
        {

            string corol = cbcolor.Text;

            switch (corol)
            {
                case "Crimson":
                    panel1.BackColor = Color.Crimson;
                    ListSong.ForeColor = Color.Crimson;
                    label6.ForeColor = Color.White;
                    song.ForeColor = Color.White;
                    panel2.BackColor = Color.Crimson;
                    rectangleShape12.BackColor = Color.Crimson;
                    rectangleShape1.BackColor = Color.Crimson;
                    this.BackColor = Color.Crimson;
                    ListSong.BackColor = Color.White;
                    cbcolor.BackColor = Color.Crimson;
                    cbcolor.ItemBackColor = Color.Crimson;
                    txtsearch.BackColor = Color.White;
                    txtsearch.ForeColor = Color.Crimson;
                    contrl.ForeColor = Color.White;
                    cpyrt.ForeColor = Color.White;
                    break;

                case "Brown":
                    panel1.BackColor = Color.Brown;
                    ListSong.ForeColor = Color.Brown;
                    label6.ForeColor = Color.White;
                    song.ForeColor = Color.White;
                    panel2.BackColor = Color.Brown;
                    rectangleShape12.BackColor = Color.Brown;
                    rectangleShape1.BackColor = Color.Brown;
                    this.BackColor = Color.Brown;
                    ListSong.BackColor = Color.White;
                    cbcolor.BackColor = Color.Brown;
                    cbcolor.ItemBackColor = Color.Brown;
                    txtsearch.BackColor = Color.White;
                    txtsearch.ForeColor = Color.Brown;
                        contrl.ForeColor = Color.White;
                    cpyrt.ForeColor = Color.White;
                    break;


                case "DarkSlateGray":
                    panel1.BackColor = Color.DarkSlateGray;
                    ListSong.ForeColor = Color.DarkSlateGray;
                    label6.ForeColor = Color.White;
                    song.ForeColor = Color.White;
                    panel2.BackColor = Color.DarkSlateGray;
                    rectangleShape12.BackColor = Color.DarkSlateGray;
                    rectangleShape1.BackColor = Color.DarkSlateGray;
                    this.BackColor = Color.DarkSlateGray;
                    ListSong.BackColor = Color.White;
                    cbcolor.BackColor = Color.DarkSlateGray;
                    cbcolor.ItemBackColor = Color.DarkSlateGray;
                    txtsearch.BackColor = Color.White;
                    txtsearch.ForeColor = Color.DarkSlateGray;
                        contrl.ForeColor = Color.White;
                    cpyrt.ForeColor = Color.White;
                    break;

                case "Red":
                    panel1.BackColor = Color.Red;
                    ListSong.ForeColor = Color.White;
                    label6.ForeColor = Color.White;
                    song.ForeColor = Color.White;
                    panel2.BackColor = Color.Red;
                    rectangleShape12.BackColor = Color.Red;
                    rectangleShape1.BackColor = Color.Red;
                    this.BackColor = Color.Red;
                      cbcolor.BackColor = Color.Red;
                    cbcolor.ItemBackColor = Color.Red;
                    ListSong.BackColor = Color.Red;
                    txtsearch.BackColor = Color.Red;
                    txtsearch.ForeColor = Color.White;
                        contrl.ForeColor = Color.White;
                    cpyrt.ForeColor = Color.White;
                    break;

                case "Blue":
                    panel1.BackColor = Color.Blue;
                    ListSong.ForeColor = Color.White;
                    label6.ForeColor = Color.White;
                    song.ForeColor = Color.White;
                    panel2.BackColor = Color.Blue;
                    rectangleShape12.BackColor = Color.Blue;
                    rectangleShape1.BackColor = Color.Blue;
                    this.BackColor = Color.Blue;
                      cbcolor.BackColor = Color.Blue;
                      cbcolor.ItemBackColor = Color.Blue;
                    ListSong.BackColor = Color.Blue;
                    txtsearch.BackColor = Color.Blue;
                    txtsearch.ForeColor = Color.White;
                        contrl.ForeColor = Color.White;
                    cpyrt.ForeColor = Color.White;
                    break;

                case "Black":
                    panel1.BackColor = Color.Black;
                    ListSong.ForeColor = Color.White;
                    label6.ForeColor = Color.White;
                    song.ForeColor = Color.White;
                    panel2.BackColor = Color.Black;
                    rectangleShape12.BackColor = Color.Black;
                    rectangleShape1.BackColor = Color.Black;
                    this.BackColor = Color.Black;
                    cbcolor.BackColor = Color.Black;
                    cbcolor.ItemBackColor = Color.Black;
                    ListSong.BackColor = Color.Black;
                    txtsearch.BackColor = Color.Black;
                    txtsearch.ForeColor = Color.White;
                        contrl.ForeColor = Color.White;
                    cpyrt.ForeColor = Color.White;
                    break;

                case "Gray":
                    panel1.BackColor = Color.Gray;
                    ListSong.ForeColor = Color.White;
                    label6.ForeColor = Color.White;
                    song.ForeColor = Color.White;
                    panel2.BackColor = Color.Gray;
                    rectangleShape12.BackColor = Color.Gray;
                    rectangleShape1.BackColor = Color.Gray;
                    this.BackColor = Color.Gray;
                    cbcolor.BackColor = Color.Gray;
                    cbcolor.ItemBackColor = Color.Gray;
                    ListSong.BackColor = Color.Gray;
                    txtsearch.BackColor = Color.Gray;
                    txtsearch.ForeColor = Color.White;
                        contrl.ForeColor = Color.White;
                    cpyrt.ForeColor = Color.White;
                    break;

                case "DarkGray":
                    panel1.BackColor = Color.DarkGray;
                    ListSong.ForeColor = Color.White;
                    label6.ForeColor = Color.White;
                    song.ForeColor = Color.White;
                    panel2.BackColor = Color.DarkGray;
                    rectangleShape12.BackColor = Color.DarkGray;
                    rectangleShape1.BackColor = Color.DarkGray;
                    this.BackColor = Color.DarkGray;
                    cbcolor.BackColor = Color.DarkGray;
                    cbcolor.ItemBackColor = Color.DarkGray;
                    ListSong.BackColor = Color.DarkGray;
                    txtsearch.BackColor = Color.DarkGray;
                    txtsearch.ForeColor = Color.White;
                        contrl.ForeColor = Color.White;
                    cpyrt.ForeColor = Color.White;
                    break;


                case "Silver":
                    panel1.BackColor = Color.Silver;
                    ListSong.ForeColor = Color.White;
                    label6.ForeColor = Color.White;
                    song.ForeColor = Color.White;
                    panel2.BackColor = Color.Silver;
                    rectangleShape12.BackColor = Color.Silver;
                    rectangleShape1.BackColor = Color.Silver;
                    this.BackColor = Color.Silver;
                    cbcolor.BackColor = Color.Silver;
                    cbcolor.ItemBackColor = Color.Silver;
                    ListSong.BackColor = Color.Silver;
                    txtsearch.BackColor = Color.Silver;
                    txtsearch.ForeColor = Color.White;
                        contrl.ForeColor = Color.White;
                    cpyrt.ForeColor = Color.White;
                    break;


                case "Teal":
                    panel1.BackColor = Color.Teal;
                    ListSong.ForeColor = Color.White;
                    label6.ForeColor = Color.White;
                    song.ForeColor = Color.White;
                    panel2.BackColor = Color.Teal;
                    rectangleShape12.BackColor = Color.Teal;
                    rectangleShape1.BackColor = Color.Teal;
                    this.BackColor = Color.Teal;
                    cbcolor.BackColor = Color.Teal;
                    cbcolor.ItemBackColor = Color.Teal;
                    ListSong.BackColor = Color.Teal;
                    txtsearch.BackColor = Color.Teal;
                    txtsearch.ForeColor = Color.White;
                        contrl.ForeColor = Color.White;
                    cpyrt.ForeColor = Color.White;
                    break;

                case "Green":
                    panel1.BackColor = Color.Green;
                    ListSong.ForeColor = Color.White;
                    label6.ForeColor = Color.White;
                    song.ForeColor = Color.White;
                    panel2.BackColor = Color.Green;
                    rectangleShape12.BackColor = Color.Green;
                    rectangleShape1.BackColor = Color.Green;
                    this.BackColor = Color.Green;
                    cbcolor.BackColor = Color.Green;
                    cbcolor.ItemBackColor = Color.Green;
                    ListSong.BackColor = Color.Green;
                    txtsearch.BackColor = Color.Green;
                    txtsearch.ForeColor = Color.White;
                        contrl.ForeColor = Color.White;
                    cpyrt.ForeColor = Color.White;
                    break;

                case "Orange":
                    panel1.BackColor = Color.Orange;
                    ListSong.ForeColor = Color.White;
                    label6.ForeColor = Color.White;
                    song.ForeColor = Color.White;
                    panel2.BackColor = Color.Orange;
                    rectangleShape12.BackColor = Color.Orange;
                    rectangleShape1.BackColor = Color.Orange;
                    this.BackColor = Color.Orange;
                    cbcolor.BackColor = Color.Orange;
                    cbcolor.ItemBackColor = Color.Orange;
                    ListSong.BackColor = Color.Orange;
                    txtsearch.BackColor = Color.Orange;
                    txtsearch.ForeColor = Color.White;
                        contrl.ForeColor = Color.White;
                    cpyrt.ForeColor = Color.White;
                    break;

                case "Violet":
                    panel1.BackColor = Color.Violet;
                    ListSong.ForeColor = Color.White;
                    label6.ForeColor = Color.White;
                    song.ForeColor = Color.White;
                    panel2.BackColor = Color.Violet;
                    rectangleShape12.BackColor = Color.Violet;
                    rectangleShape1.BackColor = Color.Violet;
                    this.BackColor = Color.Violet;
                    cbcolor.BackColor = Color.Violet;
                    cbcolor.ItemBackColor = Color.Violet;
                    ListSong.BackColor = Color.Violet;
                    txtsearch.BackColor = Color.Violet;
                    txtsearch.ForeColor = Color.White;
                        contrl.ForeColor = Color.White;
                    cpyrt.ForeColor = Color.White;
                    break;

                case "Pink":
                    panel1.BackColor = Color.Pink;
                    ListSong.ForeColor = Color.White;
                    label6.ForeColor = Color.White;
                    song.ForeColor = Color.White;
                    panel2.BackColor = Color.Pink;
                    rectangleShape12.BackColor = Color.Pink;
                    rectangleShape1.BackColor = Color.Pink;
                    this.BackColor = Color.Pink;
                    cbcolor.BackColor = Color.Pink;
                    cbcolor.ItemBackColor = Color.Pink;
                    ListSong.BackColor = Color.Pink;
                    txtsearch.BackColor = Color.Pink;
                    txtsearch.ForeColor = Color.White;
                        contrl.ForeColor = Color.White;
                    cpyrt.ForeColor = Color.White;
                    break;

                case "Maroon":
                    panel1.BackColor = Color.Maroon;
                    ListSong.ForeColor = Color.White;
                    label6.ForeColor = Color.White;
                    song.ForeColor = Color.White;
                    panel2.BackColor = Color.Maroon;
                    rectangleShape12.BackColor = Color.Maroon;
                    rectangleShape1.BackColor = Color.Maroon;
                    this.BackColor = Color.Maroon;
                    cbcolor.BackColor = Color.Maroon;
                    cbcolor.ItemBackColor = Color.Maroon;
                    ListSong.BackColor = Color.Maroon;
                    txtsearch.BackColor = Color.Maroon;
                    txtsearch.ForeColor = Color.White;
                        contrl.ForeColor = Color.White;
                    cpyrt.ForeColor = Color.White;
                    break;

                case "Default":
                    panel1.BackColor = Color.FromArgb(17,17,17);
                    ListSong.ForeColor = Color.White;
                    label6.ForeColor = Color.White;
                    song.ForeColor = Color.White;
                    panel2.BackColor = Color.FromArgb(17, 17, 17);
                    rectangleShape12.BackColor = Color.FromArgb(17, 17, 17);
                    rectangleShape1.BackColor = Color.FromArgb(17, 17, 17);
                    this.BackColor = Color.FromArgb(17, 17, 17);
                    cbcolor.BackColor = Color.FromArgb(17, 17, 17);
                    cbcolor.ItemBackColor = Color.FromArgb(17, 17, 17);
                    ListSong.BackColor = Color.Black;
                    txtsearch.BackColor = Color.Black;
                    txtsearch.ForeColor = Color.White;
                    break;


            }

            cbcolor.Visible = false;
        }

        private void rectangleShape1_MouseHover(object sender, EventArgs e)
        {
            cbcolor.Visible = false;
        }
    }
}