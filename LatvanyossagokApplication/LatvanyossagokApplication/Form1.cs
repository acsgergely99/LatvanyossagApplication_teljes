using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LatvanyossagokApplication
{
    public partial class Form1 : Form
    {
        MySqlConnection conn;
        public Form1()
        {
            InitializeComponent();
            conn = new MySqlConnection("Server=localhost;Database=latvanyossagokdb;Uid=root;Pwd=;Port=3306;");
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS varosok (
                                id INT PRIMARY KEY AUTO_INCREMENT,
                                nev VARCHAR(100) UNIQUE NOT NULL,
                                lakossag INT NOT NULL);";
            cmd.ExecuteNonQuery();
            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS latvanyossagok (
                                id INT PRIMARY KEY AUTO_INCREMENT,
                                nev VARCHAR(100) NOT NULL,
                                leiras VARCHAR(200) NOT NULL,
                                ar INT DEFAULT 0 NOT NULL,
                                varos_id INT NOT NULL,
                                FOREIGN KEY (varos_ID)
                                    REFERENCES varosok(id));";
            cmd.ExecuteNonQuery();
            VarosListazas();

        }

        void VarosListazas()
        {
            VarosListbox.Items.Clear();
            var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id, nev, lakossag from varosok";
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    var id = reader.GetInt32("id");
                    var nev = reader.GetString("nev");
                    var lakossag = reader.GetInt32("lakossag");
                    var varos = new Varos(id, nev, lakossag);
                    VarosListbox.Items.Add(varos);
                }
            }
        }
        void LatvanyossagListazas()
        {
            if (VarosListbox.SelectedIndex != -1)
            {
                LatvanyossagListBox.Items.Clear();
                var cmd = conn.CreateCommand();
                var varos = (Varos)VarosListbox.SelectedItem;
                cmd.CommandText = "SELECT id, nev, leiras, ar, varos_id from latvanyossagok WHERE varos_id = @id";
                cmd.Parameters.AddWithValue("@id", varos.Id);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var id = reader.GetInt32("id");
                        var nev = reader.GetString("nev");
                        var leiras = reader.GetString("leiras");
                        var ar = reader.GetInt32("ar");
                        var varos_id = reader.GetInt32("varos_id");
                        LatvanyossagListBox.Items.Add(new Latvanyossag(id, nev, leiras, ar, varos_id));
                    }
                }
            }
        }
        private void VarosHozzaad_Click(object sender, EventArgs e)
        {
            try
            {
                if (!String.IsNullOrWhiteSpace(VarosNev.Text))
                {
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = @"INSERT INTO varosok (nev, lakossag) VALUES (@nev, @lakossag)";
                    cmd.Parameters.AddWithValue("@nev", VarosNev.Text);
                    cmd.Parameters.AddWithValue("@lakossag", VarosLakossag.Value);
                    cmd.ExecuteNonQuery();                   
                }
                else 
                {
                    MessageBox.Show("Minden mezőt ki kell töltened!");

                }
                VarosListazas();
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                {
                    MessageBox.Show("Ez a város már szerepel az adatbázisban!");
                }
                else
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        private void VarosTorles_Click(object sender, EventArgs e)
        {
            if (LatvanyossagListBox.Items.Count > 0)
            {
                MessageBox.Show("Nem törölhető olyan város,aminek látványossága van!");
            }
            else
            {
                if (VarosListbox.SelectedIndex == -1)
                {
                    MessageBox.Show("Nincs kiválasztva város elem!");
                    return;
                }
                var cmd = conn.CreateCommand();
                cmd.CommandText = "DELETE FROM varosok WHERE id = @id";
                var varos = (Varos)VarosListbox.SelectedItem;
                cmd.Parameters.AddWithValue("@id", varos.Id);
                cmd.ExecuteNonQuery();
                LatvanyossagValaszt.Items.Clear();
                VarosListbox.Items.Clear();
                VarosListazas();
            }

        }
        private void VarosListbox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                LatvanyossagListBox.Items.Clear();
                var varos = (Varos)VarosListbox.SelectedItem;
                VarosModosit.Text = varos.Nev;
                NumericVarosModosit.Value = varos.Lakossag;
                LatvanyossagListazas();
            }
            catch (Exception)
            { 
            }
        }
        private void btnModosit_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(VarosModosit.Text) && VarosListbox.SelectedIndex != -1)
            {
                var varos = (Varos)VarosListbox.SelectedItem;
                var cmd = conn.CreateCommand();
                cmd.CommandText = "UPDATE varosok SET nev = @nev, lakossag = @lakossag WHERE id = @id;";
                cmd.Parameters.AddWithValue("@nev", VarosModosit.Text);
                cmd.Parameters.AddWithValue("@lakossag", NumericVarosModosit.Value);
                cmd.Parameters.AddWithValue("@id", varos.Id);
                cmd.ExecuteNonQuery();
                VarosListazas();
            }
        }

        private void LatvanyossagHozzaad_Click(object sender, EventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(LatvanyossagNev.Text) && !String.IsNullOrWhiteSpace(LatvanyossagLeiras.Text) && LatvanyossagValaszt.SelectedIndex != -1)
            {
                var cmd = conn.CreateCommand();
                var varos = (Varos)LatvanyossagValaszt.SelectedItem;
                cmd.CommandText = @"INSERT INTO latvanyossagok (nev, leiras, ar, varos_id) VALUES (@nev, @leiras, @ar, @varos_id)";
                cmd.Parameters.AddWithValue("@nev", LatvanyossagNev.Text);
                cmd.Parameters.AddWithValue("@leiras", LatvanyossagLeiras.Text);
                cmd.Parameters.AddWithValue("@ar", LatvanyossagAr.Value);
                cmd.Parameters.AddWithValue("@varos_id", varos.Id);
                cmd.ExecuteNonQuery();
                LatvanyossagListBox.Items.Clear();
                LatvanyossagListazas();
                VarosListazas();
            }
            else
            {
                MessageBox.Show("Minden mezőt ki kell tölteni!");

            }
        }
    }
}
