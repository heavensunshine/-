// Decompiled with JetBrains decompiler
// Type: HCHO_TEST.Form1
// Assembly: HCHO_TEST, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BC436C84-C200-4B69-921B-D64C128EA012
// Assembly location: C:\Users\heave\Desktop\hcho上位机\HCHO_TEST.exe

using HCHO_TEST.Properties;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Text;

namespace HCHO_TEST
{
    public class Form1 : Form
    {
        private DateTime TimeStart = DateTime.Now;
        private int Baund = 9600;
        private double[] a = new double[3];
        private double[] w = new double[3];
        private double[] Angle = new double[3];
        private double[] LastTime = new double[3];
        private byte[] RxBuffer = new byte[1000];
        private FileStream HchoAccelerate = null;// new FileStream( ("甲醛"+DateTime.Now.ToString("yyyyMMddHHmmss")+".txt"), FileMode.Create); 

        private bool bListening;
        private bool bClosing;
        private long N_Sec = 60;
        //private double Temperature;
        //private ushort usRxLength;
        private StreamWriter swAccelerate;

        private IContainer components;
        private ToolStrip toolStrip1;
        private ToolStripDropDownButton toolStripComSet;
        private ToolStripButton toolStripButton3;
        private SerialPort spSerialPort;
        private System.Windows.Forms.Timer timer1;
        private ToolStripButton toolStripButton1;
        private ToolStripButton toolStripButtonStart;
        private ToolStripDropDownButton toolStripDropDownButton1;
        private ToolStripMenuItem toolStripMenuItem2;
        private ToolStripMenuItem toolStripMenuItem3;
        private ToolStripStatusLabel Status;
        private Chart chart1;
        private TextBox textBox2;
        private Button button1;
        private Label label1;
        private Label label2;
        private Button button2;
        private TextBox textBox3;
        private Button button3;
        private Button button4;
        private Label label3;
        private Button button5;
        private TextBox textBox4;
        private CheckBox checkBox1;
        private Label label4;
        private TextBox textBox5;
        private Button button6;
        private Panel panel1;
        private Panel panel2;
        private Panel panel3;
        private Label label7;
        private Label label6;
        private Panel panel4;
        private Panel panel5;
        private Label label8;
        private Label label11;
        private Label label10;
        private Label label9;
        private ToolStripLabel toolStripLabel1;
        private ToolStripTextBox toolStripTextBox1;
        private Panel panel6;
        private Button button7;
        private ToolStripStatusLabel toolStripStatusLabel1;
        private ToolStripStatusLabel ErrorCounts;
        private StatusStrip statusStrip1;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            foreach (string portName in SerialPort.GetPortNames())
                toolStripComSet.DropDownItems.Add(portName, null, new EventHandler(PortSelect));
            toolStripComSet.DropDownItems.Add(new ToolStripSeparator());
            toolStripComSet.DropDownItems.Add("关闭所有", null, new EventHandler(PortClose));
            //HchoAccelerate.Close();

            if (!File.Exists("Config.ini"))
            {
                FileStream fileStream = new FileStream("Config.ini", FileMode.Create);
                StreamWriter streamWriter = new StreamWriter(fileStream);
                streamWriter.WriteLine("9600");
                Baund = 9600;


                streamWriter.Close();
                fileStream.Close();
            }
            else
            {
                StreamReader streamReader = new StreamReader(new FileStream("Config.ini", FileMode.Open));
                Baund = int.Parse(streamReader.ReadLine());

            }
            if (Baund == 115200)
            {
                toolStripMenuItem2.Checked = true;
                toolStripMenuItem3.Checked = false;
            }
            else
            {
                toolStripMenuItem2.Checked = false;
                toolStripMenuItem3.Checked = true;
            }
        }

        private void PortSelect(object sender, EventArgs e)
        {
            ToolStripMenuItem toolStripMenuItem = (ToolStripMenuItem)sender;
            for (int index = 0; index < toolStripComSet.DropDownItems.Count - 2; ++index)
                ((ToolStripMenuItem)toolStripComSet.DropDownItems[index]).Checked = false;
            sender.ToString();
            try
            {
                if(toolStripTextBox1.Text=="")
                {
                    MessageBox.Show("设备地址是无效字符", "错误");
                    return;
                }
                toolStripTextBox1.Enabled = false;
                PortClose(null, null);
                spSerialPort.ReadTimeout = 3000;
                spSerialPort.PortName = toolStripMenuItem.Text;
                spSerialPort.BaudRate = !toolStripMenuItem2.Checked ? 9600 : 115200;
                spSerialPort.Open();
                toolStripMenuItem.Checked = true;
                bClosing = false;
                this.timer1.Enabled = true;
            }
            catch (Exception ex)
            {
                toolStripMenuItem.Checked = false;
            }
        }

        private void PortClose(object sender, EventArgs e)
        {
            for (int index = 0; index < toolStripComSet.DropDownItems.Count - 2; ++index)
                ((ToolStripMenuItem)toolStripComSet.DropDownItems[index]).Checked = false;
            if (!spSerialPort.IsOpen)
                return;
            bClosing = true;
            while (bListening)
                Application.DoEvents();
            this.timer1.Enabled = false;
            spSerialPort.Dispose();
            spSerialPort.Close();
        }

       

        //private void DecodeData(byte[] byteTemp)
        //{
        //    double[] Data = new double[5];
        //    TimeSpan tss = DateTime.Now.Subtract(TimeStart);
        //    TimeSpan num = tss;
            
        //    Data[0] = ((byteTemp[1] << 8) + byteTemp[2] - 10000) / 1000.0;
        //    if (Data[0] == 0)
        //        return ;
        //    Data[4] = ((byteTemp[3] << 8) + byteTemp[4] - 10000) / 1000.0;
        //    Data[2] = ((byteTemp[5] << 8) + byteTemp[6] ) / 10.0;
        //    Data[3] = (byteTemp[7] << 8) + byteTemp[8] ;

        //            textBox1.Text = "系统时间:" + DateTime.Now.ToLongTimeString() + "\r\n\r\n相对时间:\n" + num.ToString() + "\r\n\r\n检测到HCHO传感器！" + "\r\n\r\n\r\n" + "甲醛:"+Data[0].ToString("f3") + "mg/m3" + "\r\n\r\n\r\n" + "温度:" + Data[2].ToString("f1") + "℃" + "\r\n\r\n\r\n" + "湿度:" + Data[3].ToString("f0") + "%RH";
        //            long Time_X = num.Ticks / 10000000;
        //            //if (Time_X>=120)
        //            //{

        //            //}
        //            chart1.Series[0].Points.AddXY(Time_X, Data[0]);
        //            chart1.Series[1].Points.AddXY(Time_X, Data[4]);

        //            if ( (Time_X >= 60)&&(!checkBox1.Checked))
        //            {
        //                //if(Time_X/30>5)
        //                //chart1.Series[0].Points.RemoveAt(0);
        //                chart1.ChartAreas[0].AxisX.Maximum = Time_X;
        //                chart1.ChartAreas[0].AxisX.Minimum = Time_X-N_Sec;
        //            }
        //            else
        //            {
        //                chart1.ChartAreas[0].AxisX.Maximum = Time_X;
        //               // chart1.ChartAreas[0].AxisX.Minimum = 0;
        //            }


        //            if (HchoAccelerate != null)
        //            {
        //                if (HchoAccelerate.CanWrite)
        //                    swAccelerate.WriteLine(DateTime.Now.ToLongTimeString() + "\t" + Data[0].ToString("f4") + "\t");
        //            }
        //            //if (num - LastTime[0] < 0.1)
        //            //    LastTime[0] = num;

        //            //if (num>=200.0)
        //            //// if (chart1.Series["HCHO"].Points.Count > 59)
        //            //{
        //            //    //chart1.Series["HCHO"].Points.RemoveAt(0);
        //            //    chart1.Series[0].Points.Clear();
        //            //}
        //            chart1.ChartAreas[0].RecalculateAxesScale();
        //            //   UpdateIMUData("Accelerate", num, Data);
        //}

        //private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        //{
        //    byte[] numArray = new byte[1000];
        //    if (bClosing)
        //        return;
        //    try
        //    {
        //        bListening = true;
        //        usRxLength += (ushort)spSerialPort.Read(RxBuffer, usRxLength, 1);
        //        try
        //        {
                    
        //            int tt = 0;
        //            for (; tt < 200; tt++)

        //                while (usRxLength==21)
        //                {
        //                    Form1.UpdateData updateData = new Form1.UpdateData(DecodeData);
        //                    usRxLength = 0;
        //                    for (int num = 0; num < 11; num++)
        //                        numArray[num] = RxBuffer[tt + num];
        //                   // RxBuffer.CopyTo(numArray, tt);
        //                    Invoke(updateData, numArray);
        //                    spSerialPort.DiscardInBuffer();
        //                    for (int num = 0; num < 11; num++)
        //                    RxBuffer[num] = 00;
        //                }
        //        }
        //        finally
        //        { }
        //    }
        //    finally
        //    {
        //        bListening = false;
        //    }
        //}

        private sbyte sbSumCheck(byte[] byteData, byte byteLength)
        {
            byte num = 0;
            for (byte index = 0; index < byteLength - 2; ++index)
                num += byteData[index];
            return byteData[(int)byteLength - 1] == num ? (sbyte)0 : (sbyte)-1;
        }

        public sbyte SendMessage(byte[] byteSend)
        {
            if (!spSerialPort.IsOpen)
            {
                int num = (int)MessageBox.Show("串口未打开！", "错误");
                return -1;
            }
            try
            {
                spSerialPort.Write(byteSend, 0, byteSend.Length);
                return 0;
            }
            catch (Exception ex)
            {
                int num = (int)MessageBox.Show(ex.Message);
                return -1;
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            Process.Start("https://shop72000001.taobao.com/?spm=a230r.7195193.1997079397.70.C13ina");
        }

        private void toolStripButtonStart_Click(object sender, EventArgs e)
        {
            byte[] byteSend = new byte[1];
            if (toolStripButtonStart.Text == "重新测量")
            {
                byteSend[0] = 97;
                if (SendMessage(byteSend) != 0)
                    return;
                toolStripButtonStart.Text = "重新测量";
                Status.Text = "测量已开始！";
                chart1.Series[0].Points.Clear();
                chart1.Series[1].Points.Clear();
                AxisXY_Change();
                //chart1.Series[1].Points.Clear();
                //chart1.Series[2].Points.Clear();
                TimeStart = DateTime.Now;
            }
            else
            {
                byteSend[0] = 98;
                if (SendMessage(byteSend) != 0)
                    return;
                Status.Text = "测量已开始";
                chart1.Series[0].Points.Clear();
                AxisXY_Change();
                toolStripButtonStart.Text = "重新测量";
            }
            LastTime[0] = 0.0;
        }

        private void WriteConfig()
        {
            try
            {
                FileStream fileStream = new FileStream("Config.ini", FileMode.Create);
                StreamWriter streamWriter = new StreamWriter(fileStream);
                if (toolStripMenuItem2.Checked)
                    streamWriter.WriteLine("115200");
                else
                    streamWriter.WriteLine("9600");
                streamWriter.Close();
                fileStream.Close();
            }
            catch
            {
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (toolStripButton3.Text == "停止")
                {
                    swAccelerate.Flush();
                    swAccelerate.Close();
                    HchoAccelerate.Close();
                }
                //WriteConfig();
                PortClose(null, null);
            }
            catch
            {
            }
        }

        int LogStartTime = Environment.TickCount;
        private void ToolStripButton3_Click(object sender, EventArgs e)
        {
            if (toolStripButton3.Text == "记录")
            {
                this.toolStripButton3.ForeColor = System.Drawing.Color.Red;
                toolStripButton3.Text = "停止";
                HchoAccelerate = new FileStream(("甲醛" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv"), 
                    FileMode.Create);
                swAccelerate = new StreamWriter(HchoAccelerate,  Encoding.GetEncoding("gb2312"));

                swAccelerate.WriteLine("起始时间：" + TimeStart.ToString());
                swAccelerate.WriteLine("时间(s)" + "," +"x甲醛");
               
            }
            else
            {
                this.toolStripButton3.ForeColor = System.Drawing.Color.Lime;
                toolStripButton3.Text = "记录";
                swAccelerate.Flush();
                swAccelerate.Close();
                HchoAccelerate.Close();

            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            byte[] byteSend = new byte[1] { 99 };
            spSerialPort.BaudRate = 9600;
            if (SendMessage(byteSend) != 0)
                return;
            Thread.Sleep(200);
            spSerialPort.BaudRate = 115200;
            if (SendMessage(byteSend) != 0)
                return;
            toolStripMenuItem2.Checked = true;
            toolStripMenuItem3.Checked = false;
            Status.Text = "波特率已设置为115200！";
            //WriteConfig();
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            byte[] byteSend = new byte[1] { 100 };
            spSerialPort.BaudRate = 115200;
            if (SendMessage(byteSend) != 0)
                return;
            Thread.Sleep(200);
            spSerialPort.BaudRate = 9600;
            if (SendMessage(byteSend) != 0)
                return;
            toolStripMenuItem3.Checked = true;
            toolStripMenuItem2.Checked = false;
            Status.Text = "波特率已设置为9600！";
            //WriteConfig();
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Series series2 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Title title1 = new System.Windows.Forms.DataVisualization.Charting.Title();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripComSet = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripButtonStart = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton3 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripTextBox1 = new System.Windows.Forms.ToolStripTextBox();
            this.spSerialPort = new System.IO.Ports.SerialPort(this.components);
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.Status = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.ErrorCounts = new System.Windows.Forms.ToolStripStatusLabel();
            this.chart1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.button3 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.button5 = new System.Windows.Forms.Button();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox5 = new System.Windows.Forms.TextBox();
            this.button6 = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel6 = new System.Windows.Forms.Panel();
            this.button7 = new System.Windows.Forms.Button();
            this.label11 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.panel4 = new System.Windows.Forms.Panel();
            this.panel5 = new System.Windows.Forms.Panel();
            this.toolStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).BeginInit();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel6.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel5.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.BackColor = System.Drawing.SystemColors.ButtonShadow;
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripComSet,
            this.toolStripDropDownButton1,
            this.toolStripButtonStart,
            this.toolStripButton3,
            this.toolStripButton1,
            this.toolStripLabel1,
            this.toolStripTextBox1});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Margin = new System.Windows.Forms.Padding(2);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Padding = new System.Windows.Forms.Padding(0);
            this.toolStrip1.Size = new System.Drawing.Size(1350, 27);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripComSet
            // 
            this.toolStripComSet.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripComSet.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.toolStripComSet.Name = "toolStripComSet";
            this.toolStripComSet.Size = new System.Drawing.Size(78, 24);
            this.toolStripComSet.Text = "串口选择";
            // 
            // toolStripDropDownButton1
            // 
            this.toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem2,
            this.toolStripMenuItem3});
            this.toolStripDropDownButton1.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            this.toolStripDropDownButton1.Size = new System.Drawing.Size(64, 24);
            this.toolStripDropDownButton1.Text = "波特率";
            this.toolStripDropDownButton1.Visible = false;
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Checked = true;
            this.toolStripMenuItem2.CheckState = System.Windows.Forms.CheckState.Checked;
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(126, 24);
            this.toolStripMenuItem2.Text = "115200";
            this.toolStripMenuItem2.Click += new System.EventHandler(this.toolStripMenuItem2_Click);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(126, 24);
            this.toolStripMenuItem3.Text = "9600";
            this.toolStripMenuItem3.Click += new System.EventHandler(this.toolStripMenuItem3_Click);
            // 
            // toolStripButtonStart
            // 
            this.toolStripButtonStart.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButtonStart.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.toolStripButtonStart.Name = "toolStripButtonStart";
            this.toolStripButtonStart.Size = new System.Drawing.Size(69, 24);
            this.toolStripButtonStart.Text = "重新测量";
            this.toolStripButtonStart.Click += new System.EventHandler(this.toolStripButtonStart_Click);
            // 
            // toolStripButton3
            // 
            this.toolStripButton3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton3.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.toolStripButton3.ForeColor = System.Drawing.Color.Lime;
            this.toolStripButton3.Name = "toolStripButton3";
            this.toolStripButton3.Size = new System.Drawing.Size(41, 24);
            this.toolStripButton3.Text = "记录";
            this.toolStripButton3.Click += new System.EventHandler(this.ToolStripButton3_Click);
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton1.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(69, 24);
            this.toolStripButton1.Text = "我要购买";
            this.toolStripButton1.Click += new System.EventHandler(this.toolStripButton1_Click);
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.toolStripLabel1.ForeColor = System.Drawing.Color.Maroon;
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(68, 24);
            this.toolStripLabel1.Text = "设备地址:";
            // 
            // toolStripTextBox1
            // 
            this.toolStripTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.toolStripTextBox1.Name = "toolStripTextBox1";
            this.toolStripTextBox1.Size = new System.Drawing.Size(33, 27);
            this.toolStripTextBox1.Text = "242";
            // 
            // timer1
            // 
            this.timer1.Interval = 2000;
            this.timer1.Tick += new System.EventHandler(this.Timer1_Tick);
            // 
            // Status
            // 
            this.Status.Name = "Status";
            this.Status.Size = new System.Drawing.Size(0, 18);
            // 
            // statusStrip1
            // 
            this.statusStrip1.AutoSize = false;
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.Status,
            this.toolStripStatusLabel1,
            this.ErrorCounts});
            this.statusStrip1.Location = new System.Drawing.Point(0, 706);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1350, 23);
            this.statusStrip1.Stretch = false;
            this.statusStrip1.TabIndex = 4;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(52, 18);
            this.toolStripStatusLabel1.Text = "ERROR:";
            // 
            // ErrorCounts
            // 
            this.ErrorCounts.Name = "ErrorCounts";
            this.ErrorCounts.Size = new System.Drawing.Size(15, 18);
            this.ErrorCounts.Text = "0";
            // 
            // chart1
            // 
            this.chart1.BackColor = System.Drawing.SystemColors.Control;
            chartArea1.AlignmentOrientation = ((System.Windows.Forms.DataVisualization.Charting.AreaAlignmentOrientations)((System.Windows.Forms.DataVisualization.Charting.AreaAlignmentOrientations.Vertical | System.Windows.Forms.DataVisualization.Charting.AreaAlignmentOrientations.Horizontal)));
            chartArea1.AxisX.MajorGrid.Interval = 1D;
            chartArea1.AxisX.MajorGrid.IntervalType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Number;
            chartArea1.AxisX.MinorGrid.Enabled = true;
            chartArea1.AxisX.MinorGrid.Interval = 1D;
            chartArea1.AxisX.MinorGrid.IntervalType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Number;
            chartArea1.AxisX.MinorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea1.AxisY.MinorGrid.Enabled = true;
            chartArea1.AxisY.MinorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chartArea1.BackColor = System.Drawing.Color.DimGray;
            chartArea1.BorderDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Solid;
            chartArea1.Name = "ChartArea1";
            this.chart1.ChartAreas.Add(chartArea1);
            this.chart1.Dock = System.Windows.Forms.DockStyle.Fill;
            legend1.DockedToChartArea = "ChartArea1";
            legend1.Name = "Legend1";
            this.chart1.Legends.Add(legend1);
            this.chart1.Location = new System.Drawing.Point(0, 0);
            this.chart1.Name = "chart1";
            this.chart1.Palette = System.Windows.Forms.DataVisualization.Charting.ChartColorPalette.Fire;
            series1.BorderWidth = 2;
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
            series1.IsVisibleInLegend = false;
            series1.Legend = "Legend1";
            series1.Name = "HCHO";
            series2.ChartArea = "ChartArea1";
            series2.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
            series2.IsVisibleInLegend = false;
            series2.Legend = "Legend1";
            series2.Name = "Series2";
            this.chart1.Series.Add(series1);
            this.chart1.Series.Add(series2);
            this.chart1.Size = new System.Drawing.Size(1168, 679);
            this.chart1.TabIndex = 0;
            this.chart1.Text = "chart1";
            title1.Name = "Title1";
            this.chart1.Titles.Add(title1);
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(79, 39);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(44, 21);
            this.textBox2.TabIndex = 5;
            this.textBox2.Text = "3";
            this.textBox2.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextBox2_KeyPress);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(1, 38);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(72, 23);
            this.button1.TabIndex = 6;
            this.button1.Text = "Y轴上限";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Times New Roman", 12F);
            this.label1.Location = new System.Drawing.Point(129, 40);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 19);
            this.label1.TabIndex = 7;
            this.label1.Text = "mg/m3";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Times New Roman", 12F);
            this.label2.Location = new System.Drawing.Point(129, 67);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(50, 19);
            this.label2.TabIndex = 10;
            this.label2.Text = "mg/m3";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(1, 65);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(72, 23);
            this.button2.TabIndex = 9;
            this.button2.Text = "Y轴下限";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(79, 66);
            this.textBox3.Name = "textBox3";
            this.textBox3.Size = new System.Drawing.Size(44, 21);
            this.textBox3.TabIndex = 8;
            this.textBox3.Text = "0";
            this.textBox3.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox3_KeyPress);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(3, 11);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(60, 23);
            this.button3.TabIndex = 11;
            this.button3.Text = "复位Y轴";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(3, 146);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(60, 23);
            this.button4.TabIndex = 18;
            this.button4.Text = "复位X轴";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Times New Roman", 12F);
            this.label3.Location = new System.Drawing.Point(129, 121);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(23, 19);
            this.label3.TabIndex = 17;
            this.label3.Text = "t/s";
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(1, 119);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(72, 23);
            this.button5.TabIndex = 16;
            this.button5.Text = "X轴下限";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // textBox4
            // 
            this.textBox4.Location = new System.Drawing.Point(79, 120);
            this.textBox4.Name = "textBox4";
            this.textBox4.Size = new System.Drawing.Size(44, 21);
            this.textBox4.TabIndex = 15;
            this.textBox4.Text = "0";
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(79, 150);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(60, 16);
            this.checkBox1.TabIndex = 20;
            this.checkBox1.Text = "自动XY";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Times New Roman", 12F);
            this.label4.Location = new System.Drawing.Point(129, 94);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(15, 19);
            this.label4.TabIndex = 14;
            this.label4.Text = "s";
            // 
            // textBox5
            // 
            this.textBox5.Location = new System.Drawing.Point(79, 93);
            this.textBox5.Name = "textBox5";
            this.textBox5.Size = new System.Drawing.Size(44, 21);
            this.textBox5.TabIndex = 12;
            this.textBox5.Text = "60";
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(1, 92);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(72, 23);
            this.button6.TabIndex = 13;
            this.button6.Text = "X轴区间";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.chart1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(182, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1168, 679);
            this.panel1.TabIndex = 23;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.panel1);
            this.panel2.Controls.Add(this.panel3);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 27);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1350, 679);
            this.panel2.TabIndex = 24;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.panel6);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(182, 679);
            this.panel3.TabIndex = 24;
            // 
            // panel6
            // 
            this.panel6.Controls.Add(this.button7);
            this.panel6.Controls.Add(this.button1);
            this.panel6.Controls.Add(this.label11);
            this.panel6.Controls.Add(this.button3);
            this.panel6.Controls.Add(this.label4);
            this.panel6.Controls.Add(this.label8);
            this.panel6.Controls.Add(this.textBox4);
            this.panel6.Controls.Add(this.label10);
            this.panel6.Controls.Add(this.textBox2);
            this.panel6.Controls.Add(this.label7);
            this.panel6.Controls.Add(this.button6);
            this.panel6.Controls.Add(this.label9);
            this.panel6.Controls.Add(this.button4);
            this.panel6.Controls.Add(this.label6);
            this.panel6.Controls.Add(this.label2);
            this.panel6.Controls.Add(this.textBox3);
            this.panel6.Controls.Add(this.label3);
            this.panel6.Controls.Add(this.checkBox1);
            this.panel6.Controls.Add(this.button2);
            this.panel6.Controls.Add(this.button5);
            this.panel6.Controls.Add(this.textBox5);
            this.panel6.Controls.Add(this.label1);
            this.panel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel6.Location = new System.Drawing.Point(0, 0);
            this.panel6.Name = "panel6";
            this.panel6.Size = new System.Drawing.Size(182, 679);
            this.panel6.TabIndex = 29;
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(32, 474);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(75, 23);
            this.button7.TabIndex = 28;
            this.button7.Text = "button7";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("宋体", 10.5F);
            this.label11.Location = new System.Drawing.Point(60, 341);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(21, 14);
            this.label11.TabIndex = 27;
            this.label11.Text = "-1";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("宋体", 10.5F);
            this.label8.Location = new System.Drawing.Point(12, 341);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(42, 14);
            this.label8.TabIndex = 25;
            this.label8.Text = "湿度:";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("宋体", 10.5F);
            this.label10.Location = new System.Drawing.Point(60, 319);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(21, 14);
            this.label10.TabIndex = 26;
            this.label10.Text = "-1";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("宋体", 10.5F);
            this.label7.Location = new System.Drawing.Point(12, 319);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(42, 14);
            this.label7.TabIndex = 24;
            this.label7.Text = "温度:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("宋体", 10.5F);
            this.label9.Location = new System.Drawing.Point(60, 298);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(21, 14);
            this.label9.TabIndex = 25;
            this.label9.Text = "-1";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("宋体", 10.5F);
            this.label6.Location = new System.Drawing.Point(12, 296);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(42, 14);
            this.label6.TabIndex = 23;
            this.label6.Text = "甲醛:";
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.panel5);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel4.Location = new System.Drawing.Point(0, 0);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(1350, 27);
            this.panel4.TabIndex = 25;
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.toolStrip1);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel5.Location = new System.Drawing.Point(0, 0);
            this.panel5.Margin = new System.Windows.Forms.Padding(0);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(1350, 27);
            this.panel5.TabIndex = 26;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(1350, 729);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.statusStrip1);
            this.Name = "Form1";
            this.Text = "HCHO";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chart1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel6.ResumeLayout(false);
            this.panel6.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            this.ResumeLayout(false);

        }

        private void AxisXY_Change()
        {
            chart1.ChartAreas[0].AxisX.Maximum = 60;
            chart1.ChartAreas[0].AxisX.Minimum = 0;
            chart1.ChartAreas[0].AxisY.Maximum = 3D;
            chart1.ChartAreas[0].AxisY.Minimum = 0;
        }

        //private delegate void UpdateData(byte[] byteData);

        private void TextBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (((int)e.KeyChar < 48 || (int)e.KeyChar > 57) && (int)e.KeyChar != 8 && (int)e.KeyChar != 46)
                e.Handled = true;
            //小数点的处理。
            if (e.KeyChar == 46) //小数点
            {
                if (textBox2.Text.Length <= 0)
                    e.Handled = true; //小数点不能在第一位
                else
                {
                    float f;
                    float oldf;
                    bool b1 = false, b2 = false;
                    b1 = float.TryParse(textBox2.Text, out oldf);
                    b2 = float.TryParse(textBox2.Text + e.KeyChar.ToString(), out f);
                    if (b2 == false)
                    {
                        if (b1 == true)
                            e.Handled = true;
                        else
                            e.Handled = false;
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if((this.textBox2.TextLength > 0)&&(double.Parse(this.textBox2.Text) > chart1.ChartAreas[0].AxisY.Minimum) )
            chart1.ChartAreas[0].AxisY.Maximum = double.Parse(this.textBox2.Text);
        }

        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (((int)e.KeyChar < 48 || (int)e.KeyChar > 57) && (int)e.KeyChar != 8 && (int)e.KeyChar != 46)
                e.Handled = true;
            //小数点的处理。
            if ((int)e.KeyChar == 46) //小数点
            {
                if (textBox3.Text.Length <= 0)
                    e.Handled = true; //小数点不能在第一位
                else
                {
                    float f;
                    float oldf;
                    bool b1 = false, b2 = false;
                    b1 = float.TryParse(textBox3.Text, out oldf);
                    b2 = float.TryParse(textBox3.Text + e.KeyChar.ToString(), out f);
                    if (b2 == false)
                    {
                        if (b1 == true)
                            e.Handled = true;
                        else
                            e.Handled = false;
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if((this.textBox3.TextLength>0)&&(double.Parse(this.textBox3.Text) < chart1.ChartAreas[0].AxisY.Maximum))
            chart1.ChartAreas[0].AxisY.Minimum = double.Parse(this.textBox3.Text);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //chart1.ChartAreas[0].AxisX.Maximum = 60;
            //chart1.ChartAreas[0].AxisX.Minimum = 0;
            chart1.ChartAreas[0].AxisY.Maximum = 3D;
            chart1.ChartAreas[0].AxisY.Minimum = 0;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if ((this.textBox5.TextLength > 0) && (double.Parse(this.textBox5.Text) < chart1.ChartAreas[0].AxisX.Maximum))
                N_Sec = long.Parse(this.textBox5.Text);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if ((this.textBox4.TextLength > 0) && (double.Parse(this.textBox4.Text) < chart1.ChartAreas[0].AxisX.Maximum))
                chart1.ChartAreas[0].AxisX.Minimum  = double.Parse(this.textBox4.Text);
        }

        private void button4_Click(object sender, EventArgs e)
        {
           // chart1.ChartAreas[0].AxisX.Maximum = 60;
            chart1.ChartAreas[0].AxisX.Minimum = 0;
        }
        //bool MotorEn = false;
        bool SpIsBusy = false; UInt32 err = 0;
        private void Timer1_Tick(object sender, EventArgs e)
        {
            if (SpIsBusy)
                return;
            SpIsBusy = true;
            byte[] readCmd0x525 = new byte[8]
                {
                   1,
                   3,
                   5,
                   0x25,
                   0,
                   8,
                   196,
                   11
                };
            try
            {
                readCmd0x525[0] = byte.Parse(this.toolStripTextBox1.Text);
            }
            catch
            {
                this.timer1.Enabled = false;
                toolStripTextBox1.Enabled = true;
                MessageBox.Show("请检查设备地址,设备地址只能是10进制数字", "叮咚");
                return;
            }
            ushort checkCode1 = GetCheckCode(readCmd0x525, readCmd0x525.Length - 2);
            readCmd0x525[readCmd0x525.Length - 2] = (byte)(checkCode1 & (uint)byte.MaxValue);
            readCmd0x525[readCmd0x525.Length - 1] = (byte)((uint)checkCode1 >> 8);
            spSerialPort.DiscardInBuffer();
            spSerialPort.Write(readCmd0x525, 0, readCmd0x525.Length);
            byte[] numArray = new byte[100];
            int offset = 0;
            bool flag = false;
            do
            {
                try
                {
                       offset += spSerialPort.Read(numArray, offset, numArray.Length - offset);
                    if (offset == 21)
                        flag = true;
                }
                catch(Exception ex)
                {
                    while (bListening)
                        Application.DoEvents();
                    //this.timer1.Enabled = false;
                    toolStripTextBox1.Enabled = true;
                    ErrorCounts.Text = (++err).ToString();
                    //MessageBox.Show("读取超时,请检查接线或设备地址","叮咚");
                    SpIsBusy = false;
                    return;
                }
            }
            while (!flag || offset <= 0);
            ushort checkCode2 = GetCheckCode(numArray, 19);
            if (numArray[19] == (byte)(checkCode2 & (uint)byte.MaxValue) && numArray[20] == (byte)((uint)checkCode2 >> 8))
            {

                byte[] HCHO_HEX = new byte[4];
                HCHO_HEX[3] = numArray[3];
                HCHO_HEX[2] = numArray[4];
                HCHO_HEX[1] = numArray[5];
                HCHO_HEX[0] = numArray[6];
                float num1 = BitConverter.ToSingle(HCHO_HEX, 0);
                HCHO_HEX[3] = numArray[7];
                HCHO_HEX[2] = numArray[8];
                HCHO_HEX[1] = numArray[9];
                HCHO_HEX[0] = numArray[10];
                float temp = BitConverter.ToSingle(HCHO_HEX, 0);
                HCHO_HEX[3] = numArray[11];
                HCHO_HEX[2] = numArray[12];
                HCHO_HEX[1] = numArray[13];
                HCHO_HEX[0] = numArray[14];
                float humi = BitConverter.ToSingle(HCHO_HEX, 0);
                Form1.UpdateData updateData = new Form1.UpdateData(DecodeData);
                Invoke(updateData, num1, temp, humi);
            }
            SpIsBusy = false;
            if(NextRun)
                MotorRun();
        }
        private void DecodeData(float hcho,float temp,float humi)
        {
         //   double[] Data = new double[5];
            TimeSpan tss = DateTime.Now.Subtract(TimeStart);
            TimeSpan num = tss;

            //Data[0] = ((byteTemp[1] << 8) + byteTemp[2] - 10000) / 1000.0;
            //if (Data[0] == 0)
            //    return;
            //Data[4] = ((byteTemp[3] << 8) + byteTemp[4] - 10000) / 1000.0;
            //Data[2] = ((byteTemp[5] << 8) + byteTemp[6]) / 10.0;
            //Data[3] = (byteTemp[7] << 8) + byteTemp[8];
            label9.Text = hcho.ToString("f3") + "mg/m3";
            label10.Text = temp.ToString("f1") + "℃";
            label11.Text = humi.ToString("f0") + "%RH";
            //if (toolStripTextBox1.Enabled)
            //    toolStripTextBox1.Enabled = false;
            //textBox1.Text = "系统时间:" + DateTime.Now.ToLongTimeString() + "\r\n\r\n相对时间:\n" + num.ToString() 
            //    + "\r\n\r\n检测到HCHO传感器！" + "\r\n\r\n\r\n" + "甲醛:" + hcho.ToString("f3") + "mg/m3" 
            //    + "\r\n\r\n\r\n" + "温度:" + temp.ToString("f1") + "℃" + "\r\n\r\n\r\n" + "湿度:" + humi.ToString("f0") + "%RH";
            long Time_X = num.Ticks / 10000000;
            //if (Time_X>=120)
            //{

            //}
            chart1.Series[0].Points.AddXY(Time_X, hcho);
         //   chart1.Series[1].Points.AddXY(Time_X, Data[4]);

            if ((Time_X >= 60) && (!checkBox1.Checked))
            {
                //if(Time_X/30>5)
                //chart1.Series[0].Points.RemoveAt(0);
                chart1.ChartAreas[0].AxisX.Maximum = Time_X;
                chart1.ChartAreas[0].AxisX.Minimum = Time_X - N_Sec;
            }
            else
            {
                chart1.ChartAreas[0].AxisX.Maximum = Time_X;
                // chart1.ChartAreas[0].AxisX.Minimum = 0;
            }


            if (HchoAccelerate != null)
            {
                if (HchoAccelerate.CanWrite)
                {
                    swAccelerate.WriteLine(DateTime.Now.ToLongTimeString() + "," + hcho.ToString("f4"));
                    if ((Environment.TickCount - LogStartTime) > 20000)
                    {
                        LogStartTime = Environment.TickCount;
                        swAccelerate.Flush();
                    }
                }
            }
            
            //if (num - LastTime[0] < 0.1)
            //    LastTime[0] = num;

            //if (num>=200.0)
            //// if (chart1.Series["HCHO"].Points.Count > 59)
            //{
            //    //chart1.Series["HCHO"].Points.RemoveAt(0);
            //    chart1.Series[0].Points.Clear();
            //}
            chart1.ChartAreas[0].RecalculateAxesScale();
            //   UpdateIMUData("Accelerate", num, Data);
        }
        private delegate void UpdateData(float hcho, float temp, float humi);
        private ushort GetCheckCode(byte[] pSendBuf, int nEnd)
        {
            ushort num = ushort.MaxValue;
            for (int index1 = 0; index1 < nEnd; ++index1)
            {
                num ^= pSendBuf[index1];
                for (int index2 = 0; index2 < 8; ++index2)
                {
                    if ((num & 1) == 1)
                        num = (ushort)((ushort)((uint)num >> 1) ^ 40961U);
                    else
                        num >>= 1;
                }
            }
            return num;
        }

        bool NextRun = false;
        private void button7_Click(object sender, EventArgs e)
        {
            if(SpIsBusy)
            {
                NextRun = true;
                return;
            }
            MotorRun();
        }

        private void MotorRun()
        {
            SpIsBusy = true;
            byte[] StartPump = new byte[8]
                {
                   1,
                   9,
                   5,
                   0x25,
                   0,
                   8,
                   196,
                   11
                };
            StartPump[0] = byte.Parse(this.toolStripTextBox1.Text);
            ushort checkCode1 = GetCheckCode(StartPump, StartPump.Length - 2);
            StartPump[StartPump.Length - 2] = (byte)(checkCode1 & (uint)byte.MaxValue);
            StartPump[StartPump.Length - 1] = (byte)((uint)checkCode1 >> 8);
            spSerialPort.DiscardInBuffer();
            spSerialPort.Write(StartPump, 0, StartPump.Length);
            Thread.Sleep(100);
            //MotorEn = false;
            SpIsBusy = false;
        }
    }
}
