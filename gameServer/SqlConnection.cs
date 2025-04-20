using System;
using System.Collections.Generic;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Cache;
using System.Text;
using System.Threading.Tasks;

namespace HathatulServer
{
    internal class SqlConnection
    {// this classes object is to communicate with the database. to connect between the database and the project. protection againts SQL injection exists

        /// <summary>
        /// this property 'connectionString' contains the connectionString that connects you to the DataBase
        /// </summary>
        string connectionString;
        /// <summary>
        /// this property 'connection' Actually connects the project to the Sql DataBase
        /// </summary>
        System.Data.SqlClient.SqlConnection connection;

        /// <summary>
        /// this propery 'command' will contain the command for the Sql Database
        /// </summary>
        SqlCommand command;

        /// <summary>
        /// a default constructor. connects the project to the database using the 'connectionString' creating the connection.
        /// </summary>
        public SqlConnection()
        {
            connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=""D:\Visual Studio\gameServer\gameServer\UsersDB.mdf"";Integrated Security=True";
            connection = new System.Data.SqlClient.SqlConnection(connectionString);
            command = new SqlCommand();
        }

        /// <summary>
        /// this function is used when a user is trying to register. its goal is to insert all of the info of the user into the database
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="firstname"></param>
        /// <param name="lastname"></param>
        /// <param name="email"></param>
        /// <param name="city"></param>
        /// <param name="gender"></param>
        /// <returns></returns>
        public int InsertNewUser(string username, string password, string firstname, string lastname, string email, string city, string gender)
        {
            command = new SqlCommand();
            command.CommandText = "INSERT INTO Users VALUES('" + username + "','" + password + "','" + firstname + "','" + lastname + "','" + email + "','" + city + "','" + gender + "','" + "0" + "')";
            connection.Open();
            command.Connection = connection;
            var x = command.ExecuteNonQuery();
            connection.Close();
            return x;
        }

        /// <summary>
        /// this function checks if the name of the user already exists in the databse.
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public bool IsExist(string username)
        {
            command = new SqlCommand();
            command.CommandText = "SELECT COUNT(*) FROM Users WHERE username = @username ";
            command.Parameters.AddWithValue("@username", username);
            connection.Open();
            command.Connection = connection;
            int b = Convert.ToInt32(command.ExecuteScalar());
            connection.Close();
            return b > 0;
        }

        /// <summary>
        /// this function recieves the username and password and checks if it is matching with what the database has.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool LoginCheck(string username, string password)
        {
            command = new SqlCommand();
            command.CommandText = "SELECT COUNT(*) FROM Users WHERE username = @username AND password = @password";
            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@password", password);
            connection.Open();
            command.Connection = connection;
            int b = Convert.ToInt32(command.ExecuteScalar());
            connection.Close();
            return b > 0;
        }

        /// <summary>
        /// this function recieves the username and goes into the database to return the firstname of the certain user
        /// </summary>
        /// <param name="nickname"></param>
        /// <returns></returns>
        public string GetFirstName(string nickname)
        {
            command = new SqlCommand();
            command.CommandText = "SELECT firstName FROM Users Where username = @username ";
            command.Parameters.AddWithValue("@username", nickname);
            connection.Open();
            command.Connection = connection;
            string x = (string)command.ExecuteScalar();
            connection.Close();
            return x;
        }
        
        /// <summary>
        /// this function inserts a Unique token which the client communicates with the server with. and inserts it into the database
        /// </summary>
        /// <param name="username"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public int InsertToken(string username, string token)
        {
            command = new SqlCommand();
            command.CommandText = ("UPDATE Users SET Token = @token WHERE username = @username");
            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@token", token);
            connection.Open();
            command.Connection = connection;
            var x = command.ExecuteNonQuery();
            connection.Close();
            return x;
        }

        /// <summary>
        /// this function recieves the token and checks if it exists in the database
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public bool DoesTokenExist(string token)
        {
            command = new SqlCommand();
            command.CommandText = "SELECT COUNT(*) FROM Users WHERE Token = @token";
            command.Parameters.AddWithValue("@token", token);
            connection.Open();
            command.Connection = connection;
            int b = Convert.ToInt32(command.ExecuteScalar());
            connection.Close();
            connection.Close();
            return b > 0;
        }

        /// <summary>
        /// this function recieves the token and returns the username of the one using the token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public string GetUsernameFromToken(string token)
        {
            command = new SqlCommand();
            command.CommandText = "SELECT username FROM Users Where Token = @token";
            command.Parameters.AddWithValue("@token", token);
            connection.Open();
            command.Connection = connection;
            string b = (string)command.ExecuteScalar();
            connection.Close();
            return b;
        }

        /// <summary>
        /// this function recieves a username and returns the email of the certain user (when someone registers he puts the email)
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public string GetEmailFromUsername(string username)
        {
            command = new SqlCommand();
            command.CommandText = "SELECT email FROM Users Where username = @username";
            command.Parameters.AddWithValue("@username", username);
            connection.Open();
            command.Connection = connection;
            string x = (string)command.ExecuteScalar();
            connection.Close();
            return x;
        }

        /// <summary>
        /// this function changes the password of a certain user(we know which because the function recieves the name of the user)
        /// </summary>
        /// <param name="username"></param>
        /// <param name="NewPassword"></param>
        /// <returns></returns>
        public string InsertNewPassword(string username, string newPassword)
        {
            command = new SqlCommand();
            command.CommandText = ("UPDATE Users SET password = @newPassword WHERE username = @username");
            command.Parameters.AddWithValue("@newPassword", newPassword);
            command.Parameters.AddWithValue("@username", username);
            connection.Open();
            command.Connection = connection;
            string x = (string)command.ExecuteScalar();
            connection.Close();
            return x;
        }
    }
}
