using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvgElbrusEnjoyer;
using System.Windows.Forms;
using Xunit;
using static AvgElbrusEnjoyer.Form1;

namespace AvgElbrusEnjoyer.Tests
{
    public class TestOne
    {
        [Fact]
        public void UpdateStatusLabel_UserIsAuthenticated()
        {
            // Arrange
            var form = new Form1();
            FormLogin.IsAuthenticated = true;
            FormLogin.UserName = "TestUser";

            // Act
            form.UpdateStatusLabel();

            // Assert
            Assert.Equal("Добро пожаловать, TestUser!", form.label1.Text);
            Assert.True(form.button3.Enabled);
            Assert.False(form.button1.Enabled);
            Assert.False(form.button2.Enabled);
        }

        [Fact]
        public void UpdateStatusLabel_UserIsNotAuthenticated()
        {
            // Arrange
            var form = new Form1();
            FormLogin.IsAuthenticated = false;

            // Act
            form.UpdateStatusLabel();

            // Assert
            Assert.Equal("Пожалуйста, авторизируйтесь на сайте.", form.label1.Text);
            Assert.False(form.button3.Enabled);
            Assert.True(form.button1.Enabled);
            Assert.True(form.button2.Enabled);
        }

        [Fact]
        public void SetupListView_Should_SetupColumnsCorrectly()
        {
            // Arrange
            var form = new Form1();

            // Act
            form.SetupListView();

            // Assert
            Assert.Equal(1, form.listView1.Columns.Count);
            Assert.Equal("Каталог", form.listView1.Columns[0].Text);
        }

        [Fact]
        public void Button1Click_ShouldOpenLoginForm()
        {
            // Arrange
            var form = new Form1();
            var LoginForm = new FormLogin();

            // Act
            form.button1_Click(LoginForm, EventArgs.Empty);

            // Assert
            Assert.True(LoginForm.WasShown);
        }

        [Fact]
        public void Button2Click_ShouldOpenRegisterForm()
        {
            // Arrange
            var form = new Form1();
            var RegisterForm = new FormRegister();

            // Act
            form.button2_Click(RegisterForm, EventArgs.Empty);

            // Assert
            Assert.True(RegisterForm.WasShown);
        }

        [Fact]
        public void Button3Click_ShouldOpenOrderFormWithUserName()
        {
            // Arrange
            FormLogin.UserName = "TestUser";
            var form = new Form1();
            var OrderForm = new FormOrder(FormLogin.UserName);

            // Act
            form.button3_Click(OrderForm, EventArgs.Empty);

            // Assert
            Assert.True(OrderForm.WasShown);
            Assert.Equal("TestUser", OrderForm.ReceivedUserName);
        }

        /*[Fact]
        public void listView1_SelectedIndexChanged()
        {
            // Arrange
            var form = new Form1();
            form.SetupListView();

            var item = new ListViewItem("Товар1");
            item.SubItems.Add("1");
            item.SubItems.Add("Категория1");
            item.SubItems.Add("100");

            form.listView1.Items.Add(item);
            form.listView1.Items[0].Selected = true;

            // Act
            form.listView1_SelectedIndexChanged(null, EventArgs.Empty);

            // Assert
            Assert.Equal("Вы выбрали: Товар1\nОписание: Категория1\nЦена: 100 руб.", form.LastMessage);
        }*/

    }
}
