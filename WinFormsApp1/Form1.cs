using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            _= this.initialize();
        }

        private bool initialize()
        {
            try
            {
                this.connection = new SqlConnection(new SqlConnectionStringBuilder
                {
                    DataSource = "HAMZAST440\\SQLSERVER",
                    IntegratedSecurity = true,
                    InitialCatalog = "DBMSLabExam"
                }.ConnectionString);
            }
            catch (Exception)
            {
                MessageBox.Show(@"Error connection to the local database", @"ERROR!");
                return false;
            }
            this.fillComboBox();
            this.fillPlasmaDonors();
            return true;
        }

        private void fillPlasmaDonors()
        {
            connection.Open();
            const string plasmaDonors =
                @"SELECT * FROM REGISTERED_PATIENT REG INNER JOIN RECOVERED_PATIENTS REC ON REG.MR = REC.MR AND REC.DONATION_WILLINGNESS = 1";
            SqlDataAdapter sda = new SqlDataAdapter(plasmaDonors, connection);
            DataTable dt = new DataTable();
            sda.Fill(dt);
            this.dataGridView2.DataSource = dt;
            connection.Close();
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) {
            try {
                    connection.Open();
                    string cmdstr = @"SELECT * FROM " + this.comboBox1.Text;
                    SqlDataAdapter sda = new SqlDataAdapter(cmdstr, connection);
                    DataTable dt = new DataTable();
                    sda.Fill(dt);
                    this.dataGridView1.DataSource = dt;
                    connection.Close();
            }
            catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }
        }

        private void fillComboBox()
        {
            connection.Open();
            const string fetchTableNames = @"SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME NOT IN ('SYSDIAGRAMS')";
            DataTable dt = new DataTable();
                SqlDataAdapter sda = new SqlDataAdapter(fetchTableNames, this.connection);
                try {
                    sda.Fill(dt);
                    foreach (DataRow row in dt.Rows)
                    {
                        if (row != null) this.comboBox1.Items.Add(row["TABLE_NAME"]);
                    }
                }
                catch (Exception ex) {
                    MessageBox.Show(ex.Message);
                }

                const string fetchMRNo = "SELECT MR from [REGISTERED_PATIENT]";
                sda = new SqlDataAdapter(fetchMRNo, this.connection);
                try
                {
                    sda.Fill(dt);
                    foreach (DataRow row in dt.Rows)
                    {
                        if (row != null && 
                            (row.ItemArray[0] is DBNull ||
                             string.IsNullOrWhiteSpace(row.ItemArray[0] as string))) 
                            this.comboBox2.Items.Add(row["MR"]);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, @"MR Numbers not found");
                    
                }
                
            connection.Close();
        }

        private void COMMIT_Click(object sender, EventArgs e)
        {
            try {
                connection.Open();
                string cmdstr = @"SELECT * FROM [dbo].[" + this.comboBox1.Text + @"]";
                SqlDataAdapter sda = new SqlDataAdapter(cmdstr, this.connection);
                SqlCommandBuilder cmd = new SqlCommandBuilder(sda);
                sda.InsertCommand = cmd.GetInsertCommand();
                sda.DeleteCommand = cmd.GetDeleteCommand();
                sda.UpdateCommand = cmd.GetUpdateCommand();
                sda.Update((DataTable) this.dataGridView1.DataSource); 
            }
            catch (Exception err) {
                MessageBox.Show(err.ToString(), @"ERROR");
            }
            connection.Close();
        }

        private void generateReport(string data)
        {
            string query =
                @"SELECT * FROM REGISTERED_PATIENT PATIENT LEFT JOIN PATIENTS_INDOORS INDOOR ON PATIENT.MR = INDOOR.MR
										 LEFT JOIN PATIENT_CHECKUP CHECKUP ON PATIENT.MR = CHECKUP.MR
										 LEFT JOIN DEPARTMENT DEPT ON PATIENT.DEPARTMENT = DEPT.NAME
										 LEFT JOIN SYMPTOMS SYMP ON PATIENT.MR = SYMP.MR
										 LEFT JOIN RECOVERED_PATIENTS REC ON PATIENT.MR = REC.MR
                                         WHERE PATIENT.MR=" + data + ";";
            
            try {
                connection.Open();
                SqlDataAdapter sda = new SqlDataAdapter(query, this.connection);
                DataTable dt = new DataTable();
                sda.Fill(dt);
                DataRow row = dt.Rows[0];
                string report = $"Patient MR Number: {row["MR"]}\n" +
                                $"Name: {(row["FIRSTNAME"] as string)?.Trim()} {(row["LASTNAME"] as string)?.Trim()}\n" +
                                $"Maiden Name: {row["FATHER_HUSBAND"]}\n" +
                                $"Date of Birth: {row["DOB"]}\n" +
                                $"Birth Place: {row["BIRTHPLACE"]}\n" +
                                $"Blood Group: {row["BLOODGROUP"]}\n" +
                                $"Address: {row["CURRENT_ADDRESS"]}\n" +
                                $"Permanent Address: {row["PERMANENT_ADDRESS"]}\n" +
                                $"District: {row["DISTRICT"]}\n" +
                                $"Tehsil: {row["TEHSIL"]}\n" +
                                $"State: {row["STATE"]}\n" +
                                $"Caste: {row["CASTE"]}\n" +
                                $"Highest Education: {row["EDUCATION"]}\n" +
                                $"CNIC Number: {row["CNIC"]}\n" +
                                $"Contact: {row["CONTACT"]}\n" +
                                $"Mobile: {row["MOBILE"]}\n" +
                                $"Email: {row["EMAIL"]}\n" +
                                $"City: {row["CITY"]}\n";
                var gender = (string) row["GENDER"];
                if (gender != null && gender.IndexOf("M", StringComparison.Ordinal) != -1)
                {
                    gender = "Male";
                }
                else if (gender != null && gender.IndexOf("F", StringComparison.Ordinal) != -1)
                {
                    gender = "Female";
                }
                else
                {
                    gender = "Non binary";
                }
                report += $"Gender: {gender}\n" +
                          $"Department: {row["DEPARTMENT"]}\n" +
                          $"Doctor's diagnosis: {row["DIAGNOSIS"]}\n" +
                          $"Doctor: {row["DOCTOR"]}\n" +
                          $"Patient ID: {(row["PATIENTNUMBER"] as string)?.Trim()}\n";
                if (!(row["PAYMENT_MODE"] is DBNull))
                {
                    report += "\n\n-------------\nINDOORS DATA\n--------\n\n" +
                              $"Payment Mode: {row["PAYMENT_MODE"]}\n" +
                              $"Room number: {row["Room"]}\n" +
                              $"Admission Date: {row["ADMISSION_DATE"]}\n" +
                              $"Initial Conditions: {row["INITIAL_CONDITIONS"]}\n" +
                              $"INDOOR Doctor's diagnosis: {row["INDOOR_DIAGNOSIS"]}\n" +
                              $"Treatment: {row["TREATMENT"]}\n" +
                              $"Inspected by: {row["NUM_INSPECTING_DOCTORS"]} doctors\n" +
                              $"Attendant: {row["ATTENDANT"]}\n";
                }
                if (!(row["CHECKUPDATE"] is DBNull))
                {
                    report += "\n\n-------------\nCHECKUP DATA\n--------\n\n" +
                              $"DATE: {row["CHECKUPDATE"]}\n";
                }

                if (!(row["DRY_COUGH"] is DBNull))
                {
                    report += "\n\n-------------\nSYMPTOMS\n--------\n\n";
                    if (((bool) row["DRY_COUGH"]))
                    {
                        report += "Dry Cough\n";
                    }
                    if (((bool) row["FEVER"]))
                    {
                        report += "Fever\n";
                    }
                    if (((bool) row["CHEST_PAIN"]))
                    {
                        report += "Chest pain\n";
                    }
                    if (((bool) row["SHORTENED_BREATH"]))
                    {
                        report += "Shortness of breath\n";
                    }
                    if (((bool) row["FATIGUE"]))
                    {
                        report += "Fatigue\n";
                    }
                    if (((bool) row["HEADACHE"]))
                    {
                        report += "Headache\n";
                    }
                    if (((bool) row["VOMITING"]))
                    {
                        report += "Vomiting\n";
                    }
                    if (((bool) row["LOSS_OF_SMELL"]))
                    {
                        report += "Loss of smell\n";
                    }
                    if (((bool) row["LOSS_OF_TASTE"]))
                    {
                        report += "Loss of taste\n";
                    }
                    if (((bool) row["DIARRHEA"]))
                    {
                        report += "Diarrhea\n";
                    }
                }

                if (!(row["DONATION_WILLINGNESS"] is DBNull))
                {
                    report += "\n\n-------------\nDISCHARGE REPORT\n--------\n\n";
                    if (((bool) row["DONATION_WILLINGNESS"]))
                    {
                        report += "Subject is willing to donate plasma\n";
                    }
                    else
                    {
                        report += "Subject is not willing to donate plasma\n";
                    }

                    report += $"Discharge date: {row["DATE_OF_DISCHARGE"]}\n";
                }
                this.richTextBox1.Text = report;

            }
            catch (Exception err) {
                MessageBox.Show(err.ToString(), @"ERROR");
            }
            connection.Close();
        }
        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.generateReport(this.comboBox2.Text);
        }
    }
}