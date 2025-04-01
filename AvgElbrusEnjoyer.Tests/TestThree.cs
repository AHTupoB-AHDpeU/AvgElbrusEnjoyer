using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Xunit;

namespace AvgElbrusEnjoyer.Tests
{
    public class TestThree
    {
        public class FakeMessageBox
        {
            public static string LastMessage { get; private set; }

            public static void Show(string message)
            {
                LastMessage = message;
            }
        }

        [Fact]
        public void FormRegister_ShouldForm() // Отображение
        {
            var form = new FormRegister();
            form.Show();
            Assert.True(form.WasShown);
        }

        [Fact]
        public void RegisterUser_ShowMessageWhenFieldsAreEmpty()
        {
            // Arrange
            var form = new FormRegister();
            form.textBoxUsername.Text = "";
            form.textBoxEmail.Text = "";
            form.textBoxPhone.Text = "";
            form.textBoxAddress.Text = "";
            form.textBoxPassword.Text = "";

            // Act
            form.regist_Click(null, EventArgs.Empty);

            // Assert
            Assert.True(form.WasMessageBoxShownNo);
        }



        [Fact]
        public void FormOrder_ShouldForm() // Отображение
        {
            var form = new FormOrder("testUser");
            form.Show();
            Assert.True(form.WasShown);
        }

        [Fact]
        public void FormOrder_UserName() // Получает имя пользователя для отображения заказа
        {
            var form = new FormOrder("User2");
            Assert.Equal("User2", form.ReceivedUserName);
        }
    }
}
