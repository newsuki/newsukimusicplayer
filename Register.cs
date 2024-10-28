using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;

namespace MusicPlayer_AK3TRB
{
    public partial class Register : Form
    {
        public Register()
        {
            InitializeComponent();
        }

        private void Register_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (passwordTextBox.Text == confirmTextBox.Text)
            {
                if(emailTextBox.Text.Contains('@') && emailTextBox.Text.Contains('.'))
                {
                    using (SQLiteConnection con = new SQLiteConnection("data source=baza.db"))
                    {
                        con.Open();
                        using (SQLiteCommand cmd = new SQLiteCommand(con))
                        {
                            try
                            {
                                cmd.CommandText = "SELECT COUNT(*) FROM uporabniki WHERE user = @username OR email = @email";
                                cmd.Parameters.AddWithValue("@username", userTextBox.Text);
                                cmd.Parameters.AddWithValue("@email", emailTextBox.Text);
                                int count = Convert.ToInt32(cmd.ExecuteScalar());
                                if (count > 0)
                                {
                                    MessageBox.Show("Username or email already exist!");
                                }
                                else
                                {
                                    cmd.CommandText = "INSERT INTO uporabniki (user, email, password) VALUES (@username, @email, @password)";
                                    cmd.Parameters.AddWithValue("@username", userTextBox.Text);
                                    cmd.Parameters.AddWithValue("@email", emailTextBox.Text);
                                    cmd.Parameters.AddWithValue("@password", passwordTextBox.Text);
                                    cmd.ExecuteNonQuery();
                                    MessageBox.Show("Account created successfully!");
                                }

                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                            }
                        }
                        con.Close();


                    }
                }
                else
                {
                    MessageBox.Show("E-mail is not in correct format!");
                }
            }
            else
            {
                MessageBox.Show("Password doesn't match!");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (SQLiteConnection con = new SQLiteConnection("data source=baza.db"))
            {
                con.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(con))
                {
                    try
                    {
                        cmd.CommandText = "SELECT COUNT(*) FROM uporabniki WHERE user = @username AND email = @email AND password = @password";
                        cmd.Parameters.AddWithValue("@username", userTextBox.Text);
                        cmd.Parameters.AddWithValue("@email", emailTextBox.Text);
                        cmd.Parameters.AddWithValue("@password", passwordTextBox.Text);
                        int count = Convert.ToInt32(cmd.ExecuteScalar());

                        if(count > 0)
                        {
                            Form1 form1 = new Form1();
                            form1.Show();
                            this.Hide();
                            MessageBox.Show("Welcome!");
                        }
                        else
                        {
                            MessageBox.Show("Credentials do not match!");
                        }

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                con.Close();
            }
        }
    }
}
