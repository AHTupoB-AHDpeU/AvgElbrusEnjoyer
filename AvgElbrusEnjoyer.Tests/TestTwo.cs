using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Xunit;

namespace AvgElbrusEnjoyer.Tests
{
    public class TestTwo
    {
        [Fact]
        public void FormLogin_ShouldInitializeCorrectly() // Корректность формы и данных для неё
        {
            // Act
            var form = new FormLogin();

            // Assert
            Assert.NotNull(form);
            Assert.True(form.WasShown);
            Assert.False(FormLogin.IsAuthenticated);
            Assert.Null(FormLogin.UserName);
            Assert.Equal(0, FormLogin.UserId);
        }

        [Fact]
        public void LoginUser_ShowMessageWhenFieldsAreEmpty() // Пустые поля
        {
            // Arrange
            var form = new FormLogin();
            form.textBoxEmail.Text = "";
            form.textBoxPassword1.Text = "";

            // Act
            var ex = Record.Exception(() => form.LoginUser());

            // Assert
            Assert.Null(ex);
        }

        [Fact]
        public void GetUserId_WhenNoConnection() // Нет соединения
        {
            // Arrange
            var form = new FormLogin();

            // Act
            int userId = form.GetUserId("test@example.com");

            // Assert
            Assert.Equal(0, userId);
        }
    }
}
