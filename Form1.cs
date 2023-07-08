using Microsoft.VisualBasic.FileIO;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Sort_and_Locate_and_Count
{
    public partial class Form1 : Form
    {
        // Where the tracking file will be writen.
        private readonly string _FilePath = "C:\\temp"; 
        // Name of the file to use
        private readonly string _TrackingFile = "Location_Group_Count.csv";
        public Form1()
        {
            InitializeComponent();
            CreateTrackingFile();
            FillGridveiw();
        }


        private void CreateTrackingFile()
        {
            if (!Directory.Exists(_FilePath))
            {
                Directory.CreateDirectory(_FilePath);
            }
            if (!File.Exists(_FilePath + "\\" + _TrackingFile))
            {
                using (StreamWriter file = new StreamWriter(_FilePath + "\\" + _TrackingFile))
                {
                    file.WriteLine("Location,Group,Qty");
                }
            }
        }

        private void FillGridveiw()
        {
            dataGridView1.DataSource = GetDataTabletFromCSVFile(_FilePath + "\\" + _TrackingFile);
        }

        private static DataTable GetDataTabletFromCSVFile(string path)
        {
            DataTable csvData = new DataTable();

            try
            {
                using (TextFieldParser csvReader = new TextFieldParser(path))
                {
                    csvReader.SetDelimiters(new string[] { "," });
                    csvReader.HasFieldsEnclosedInQuotes = true;
                    string[] colFields = csvReader.ReadFields();

                    foreach (string column in colFields)
                    {
                        DataColumn serialno = new DataColumn(column);
                        serialno.AllowDBNull = true;
                        csvData.Columns.Add(serialno);
                    }

                    while (!csvReader.EndOfData)
                    {
                        string[] fieldData = csvReader.ReadFields();
                        DataRow dr = csvData.NewRow();
                        //Making empty value as empty
                        for (int i = 0; i < fieldData.Length; i++)
                        {
                            if (fieldData[i] == null)
                                fieldData[i] = string.Empty;

                            dr[i] = fieldData[i];
                        }
                        csvData.Rows.Add(dr);
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            return csvData;
        }


        private void BtnSort_Click(object sender, EventArgs e)
        {
            TextBox1.Text.ToUpper();
            if (TextBox1.Text == "")
            {
                return;
            }
            var _path = _FilePath + "\\" + _TrackingFile;
            string _action = "Insert";
            foreach (var line in File.ReadLines(_path))
            {

                string[] parts = line.Split(',');

                // int _loc = Convert.ToInt32(parts[0]);
                if (parts[1] == TextBox1.Text.ToUpper())
                {
                    _action = "UpdateUnitCount";
                }

            }
            if (_action == "UpdateUnitCount")
            {
                UpdateUnitCount(TextBox1.Text.ToUpper());
            }
            if (_action == "Insert")
            {
                InsertNewGroup(TextBox1.Text.ToUpper());
                //MessageBox.Show("Not Found");
            }
            FillGridveiw();

            foreach (DataGridViewRow dgvr in dataGridView1.Rows)
            {
                //if (dgvr.Cells[7].Value < dgvr.Cells[10].Value)
                //{
                //    dgvr.DefaultCellStyle.ForeColor = Color.Red;
                //}
                string t = dgvr.Cells[1].Value.ToString();
                if (t == TextBox1.Text.ToUpper())
                {
                    //dgvr.DefaultCellStyle.Font.Bold = true;
                    dgvr.DefaultCellStyle.BackColor = Color.Cyan;
                }
            }

            TextBox1.Text = "";
            TextBox1.Select();
            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                dataGridView1.Rows[i].Selected = false;
            }
        }

        private void UpdateUnitCount(string Group)
        {
            string path = _FilePath + "\\" + _TrackingFile;
            var encoding = Encoding.GetEncoding("iso-8859-1");
            var csvLines = File.ReadAllLines(path, encoding);

            for (int i = 0; i < csvLines.Length; i++)
            {
                var values = csvLines[i].Split(',');
                // if (values[1].Contains(Group))
                if (values[1].Equals(Group))
                {
                    string _count = values[2];
                    int NewCount = Convert.ToInt32(values[2]);
                    NewCount = NewCount +1;
                    values[2] = NewCount.ToString();
                    using (FileStream stream = new FileStream(path, FileMode.Create))
                    {
                        using (StreamWriter writer = new StreamWriter(stream, encoding))
                        {
                            for (int currentLine = 0; currentLine < csvLines.Length; ++currentLine)
                            {
                                //if (currentLine == 0)
                                //{ 
                                //}
                                if (currentLine == i)
                                {
                                    writer.WriteLine(string.Join(",", values));
                                }
                                else
                                {
                                    writer.WriteLine(csvLines[currentLine]);
                                }
                            }

                            writer.Close();
                        }

                        stream.Close();
                    }
                }
            }
        }
        private void InsertNewGroup(string Group)
        {
            string UpdateFile = _FilePath + "\\" + _TrackingFile;
            int Location = 0;
            using (StreamReader sr = new StreamReader(UpdateFile))
            {
                sr.ReadLine(); // Skip the header
                String line;

                while ((line = sr.ReadLine()) != null)
                {
                    string[] parts = line.Split(',');

                    int _loc = Convert.ToInt32(parts[0]);
                    if (_loc > Location)
                    {
                        Location = _loc;
                    }
                }
            }
            Location++;
            File.AppendAllText(_FilePath + "\\" + _TrackingFile, Location + "," + Group + "," + "1" + Environment.NewLine);
        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            TextBox1.Text.ToUpper();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (dataGridView1.RowCount != 0)
            {
                int rowindex = dataGridView1.CurrentCell.RowIndex;
                int columnindex = dataGridView1.CurrentCell.ColumnIndex;

                string loc = dataGridView1.Rows[rowindex].Cells[0].Value.ToString();
                string group = dataGridView1.Rows[rowindex].Cells[1].Value.ToString();
                string qty = dataGridView1.Rows[rowindex].Cells[2].Value.ToString();


                DeleteLine(group, _FilePath + "\\" + _TrackingFile);
                FillGridveiw();
                dataGridView1.ClearSelection();
            }
            
        }

        private void DeleteLine(string _group, string file_path)
        {


            DataTable dt = new DataTable();
            using (StreamReader sr = new StreamReader(file_path))
            {
                string[] headers = sr.ReadLine().Split(',');
                foreach (string header in headers)
                {
                    dt.Columns.Add(header);
                }
                while (!sr.EndOfStream)
                {
                    string[] rows = sr.ReadLine().Split(',');
                    if (rows[1] != _group)
                    {
                        DataRow dr = dt.NewRow();
                        for (int i = 0; i < headers.Length; i++)
                        {
                            dr[i] = rows[i];
                        }
                        dt.Rows.Add(dr);
                    }


                }

            }
            ToCSV(dt, file_path);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (File.Exists(_FilePath + "\\" + _TrackingFile))
            {
                File.Delete(_FilePath + "\\" + _TrackingFile);
            }
            CreateTrackingFile();
            FillGridveiw();
        }

        private void ToCSV(DataTable dtDataTable, string strFilePath)
        {
            StreamWriter sw = new StreamWriter(strFilePath, false);
            for (int i = 0; i < dtDataTable.Columns.Count; i++)
            {
                sw.Write(dtDataTable.Columns[i]);
                if (i < dtDataTable.Columns.Count - 1)
                {
                    sw.Write(",");
                }
            }
            sw.Write(sw.NewLine);
            foreach (DataRow dr in dtDataTable.Rows)
            {
                for (int i = 0; i < dtDataTable.Columns.Count; i++)
                {
                    if (!Convert.IsDBNull(dr[i]))
                    {
                        string value = dr[i].ToString();
                        if (value.Contains(','))
                        {
                            value = String.Format("\"{0}\"", value);
                            sw.Write(value);
                        }
                        else
                        {
                            sw.Write(dr[i].ToString());
                        }
                    }
                    if (i < dtDataTable.Columns.Count - 1)
                    {
                        sw.Write(",");
                    }
                }
                sw.Write(sw.NewLine);
            }
            sw.Close();
        }

        private void TextBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                TextBox1.Text.ToUpper();
                if (TextBox1.Text == "")
                {
                    return;
                }
                var _path = _FilePath + "\\" + _TrackingFile;
                string _action = "Insert";
                foreach (var line in File.ReadLines(_path))
                {

                    string[] parts = line.Split(',');

                    // int _loc = Convert.ToInt32(parts[0]);
                    if (parts[1] == TextBox1.Text.ToUpper())
                    {
                        _action = "UpdateUnitCount";
                    }

                }
                if (_action == "UpdateUnitCount")
                {
                    UpdateUnitCount(TextBox1.Text.ToUpper());
                }
                if (_action == "Insert")
                {
                    InsertNewGroup(TextBox1.Text.ToUpper());
                    //MessageBox.Show("Not Found");
                }
                FillGridveiw();

                foreach (DataGridViewRow dgvr in dataGridView1.Rows)
                {
                    //if (dgvr.Cells[7].Value < dgvr.Cells[10].Value)
                    //{
                    //    dgvr.DefaultCellStyle.ForeColor = Color.Red;
                    //}
                    string t = dgvr.Cells[1].Value.ToString();
                    if (t == TextBox1.Text.ToUpper())
                    {
                        //dgvr.DefaultCellStyle.Font.Bold = true;
                        dgvr.DefaultCellStyle.BackColor = Color.Cyan;
                    }
                }

                TextBox1.Text = "";
                TextBox1.Select();
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    dataGridView1.Rows[i].Selected = false;
                }
            }

        }

        //private void TextBox1_TextChanged(object sender, EventArgs e)
        //{

        //}
    }
}
