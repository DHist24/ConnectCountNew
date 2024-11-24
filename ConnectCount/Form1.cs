using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Data.SqlClient;
using System.Windows.Forms.DataVisualization.Charting;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using System.Data.Common;

//ver 07.03.24 Условие,если пустые поля
//ver 11.03.24 Свойства формы: поверх всех окон
//             Условие, если закрыли программу через крестик
//             Построение графика
//ver 15.03.24 Обновление графика при нажатии на старт
//ver 22.03.24 По оси X  рисуется график а не сетка+ожидание 5 минут
//ver 24.11.24 Запрет изменения размера формы,  запрет разворота на весь экран
//             Нет доступа к базе данных

namespace ConnectCount

{
    delegate void SendCountDel(string t);
    

    public partial class Form1 : Form
    {
        SendCountDel sCD;
        Thread countingThread;
        Boolean Pressed;
        Boolean EmptyAddressInfo;
        Boolean NoAccessDataBaseInfo;
        int countForClear;
        





        public  Form1()
        {
            InitializeComponent();
            StartPosition = FormStartPosition.CenterScreen;
            

        }

     

        private void Form1_Load_1(object sender, EventArgs e)
        {
            button2.Enabled = false;
            textBox1.ReadOnly = true;
            chart1.ChartAreas[0].AxisX.Maximum = 300;
            


        }

        private void EmptyAddress()
        {

            var servertext = textBoxSer.Text;
            var databasename = textBoxBase.Text;

            if (string.IsNullOrEmpty(servertext) || string.IsNullOrEmpty(databasename))
            {
                MessageBox.Show("Не удалось подключиться к базе данных. Заполните адрес подключения.", "Ошибка подключения", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                EmptyAddressInfo = true;
                return;
                
            }
            else
            {
                EmptyAddressInfo = false;

            }
        }

        

        


        private void RefreshDataGrid()
        {
            sCD = new SendCountDel(SendCounts);

            var servertext = textBoxSer.Text;
            var databasename = textBoxBase.Text;


            while (true && Pressed == false)
            {

                SqlConnection sqlConn = new SqlConnection();
                sqlConn.ConnectionString = (@"Data Source='" + servertext + "';Initial Catalog='" + databasename + "';User ID=operator;Password=operator");
                SqlCommand sqlComm = new SqlCommand();

                try
                {
                    sqlComm.Connection = sqlConn;
                    sqlConn.Open();



                    sqlComm.CommandText = "select count(dbid)  from sys.sysprocesses where dbid in (select dbid from  master.sys.sysdatabases where name = '" + databasename + "') group by dbid";

                    object result = sqlComm.ExecuteScalar();
                    if (result != null)

                    {

                        SendCounts(result.ToString());

                    }
                    sqlConn.Close();


                    Thread.Sleep(1000);
                    countForClear++;

                }
                catch
                {


                    MessageBox.Show("Не удалось подключиться к базе данных.", "Ошибка подключения", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                    
                   
                    return;
                }

                
                                
            }
           
        }

        private void CreateChart(string result)
        {

            if (countForClear == 300) //5 минут
            {
                chart1.Series["Connections"].Points.Clear();
                countForClear = 0;
            }
            chart1.Series["Connections"].Points.AddY(result);

        }


        private void SendCounts(string result)//метод 2
        {
            if (textBox1.InvokeRequired)
            {
                Invoke(sCD, new object[] { result });

            }
            else
            {
                textBox1.Text = result;
                if (checkBox1.Checked == false)
                { CreateChart(result); }
                

            }

        }



        private void button1_Click(object sender, EventArgs e)
        {
            EmptyAddress();
            
            if (EmptyAddressInfo ==false)
            {
                chart1.Series["Connections"].Points.Clear();
                countForClear = 0;
                Pressed = false;
                button2.Enabled = true;
                button1.Enabled = false;
                checkBox1.Enabled = false;
                textBoxSer.Enabled = false;
                textBoxBase.Enabled = false;

               
               

                countingThread = new Thread(new ThreadStart(RefreshDataGrid));
                countingThread.Start();
            }
           


        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            button2.Enabled = false;
            button1.Enabled = true;
            textBoxSer.Enabled = true;
            textBoxBase.Enabled = true;
            checkBox1.Enabled = true;

            textBox1.Clear();
            Pressed = true;

        }



        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (EmptyAddressInfo == true)
            {
                 countingThread.Abort();
            }
        }

       
    }
}
