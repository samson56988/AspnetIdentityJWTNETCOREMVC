using AspIdentityShared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

    

        private async void btn_Register(object sender, EventArgs e)
        {
            HttpClient client = new HttpClient();

            var model = new RegisterViewModel
            {
                Email = txt_Email.Text,
                Password = txt_Password.Text,
                ConfirmPassword = txt_Password.Text
            };
            var jsonData = JsonConvert.SerializeObject(model);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("https://localhost:44333/api/Auth/Register", content);
            var responseBody = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<UserManagementResponse>(responseBody);
            if(responseObject.IsSuccess)
            {
                MessageBox.Show("Your Account has been created successfully");
            }
            else 
            {
                MessageBox.Show(responseObject.Errors.FirstOrDefault());
            
            }
        }
    }
}
