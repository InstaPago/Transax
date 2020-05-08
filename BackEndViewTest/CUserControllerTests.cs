using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Umbrella.Controllers;
using Umbrella.Models;

namespace BackEndViewTest
{
    [TestClass]
    public class CUserControllerTests
    {
        /// <summary>
        /// Prueba la modificacion de un usuario del comercio valido
        /// de no existir una declaracion conciliada
        /// </summary>
        [TestMethod]
        public void ModifyCUserTest_ValidCUser_ModifiesCUser()
        {
            // arrange
            Guid cUserID = new Guid("9891aa0a-5fec-4ba6-bdf9-87af66282745");
            bool expectedValue = true;
            bool actualValue = false; 
            CUserController controller = new CUserController();
            ModifyCUserModel modifyCUserModel = new ModifyCUserModel
            {
                id = cUserID,
                Name = "Joiner",
                LastName = "Ochoa",
                RoleId = "CommerceUser",
                TestMode = true
            };

            // act
            controller.ModifyCUser(modifyCUserModel);

            // assert
            Assert.AreEqual(expectedValue, actualValue, "El usuario no pudo ser modificado");
        }

    }
}
