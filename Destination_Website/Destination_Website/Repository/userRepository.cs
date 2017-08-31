using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Dapper;
using System.Linq;
using System.Text.RegularExpressions;

namespace Destination_Website.Repository
{
    public class UserRepository
    {
        private string _connection;

        public UserRepository()
        {
            var isUat = System.Configuration.ConfigurationManager.AppSettings["IsUAT"];
            if (isUat.ToLower() == "y")
            {
                _connection = System.Configuration.ConfigurationManager.AppSettings["Demo"];
            }
            else
            {
                var computerName = Environment.MachineName;

                var connectionCnofig = new Dictionary<string, string>
                {
                    {"Data Source", computerName},
                    {"Initial Catalog", "QRcode_LogIn"},
                    {"Persist Security Info", "True"},
                    {"User ID", "murri_usr"},
                    {"Password", "0000"},
                    {"Pooling", "true"},
                    {"min pool size", "4"},
                    {"max pool size", "32"},
                };
                _connection = string.Join(";", connectionCnofig.Select(x => x.Key + "=" + x.Value));
            }


        }

        public string UserValidatefromQRcode(string userName)
        {
            bool userexitresult = _Userexit(userName);
            if (userexitresult == true)
            {
                return "Login Success";
            }
            else
            {
                return "No User, please register";
            }
        }

        public string UserLogin(string userName, string password)
        {
            bool userexitresult = _Userexit(userName);
            if (userexitresult == true)
            {
                bool uservalidateresult = _UserValidate(userName, password);
                if (uservalidateresult == false) return "Wrong password";
                else return "Login Success";
            }
            else
            {
                return "No User, please register";
            }
        }

        public string UserRegister(string userName, string password)
        {
            bool userexitresult = _Userexit(userName);
            Regex regexIllegalWord = new Regex(@"^[A-Za-z0-9]+$");
            if (!regexIllegalWord.IsMatch(userName) || !regexIllegalWord.IsMatch(password))
                return "Include illegal word";
            else if (userexitresult == false)
            {
                _SetUsers(userName, password);
                var usercount = _GetUsers(userName).Count;
                if (usercount == 1) return "Register Success";
                else return "Something error in DB";
            }
            else
                return "User Exist";
        }

        public bool IsUserDeviceExist(string userName)
        {
            var userdeviceStatus = _GetUsers(userName)[0].DeviceStatus;
            if (userdeviceStatus == 1) return true;
            else return false;
        }

        public void SetDeviceStatus(string userName)
        {
            var sql = @"Update dbo.Users Set [DeviceStatus]=1 where [UserName] = @UserName";
            DoConnection(conn => conn.Query<string>(sql, new {UserName = userName}));
        }

        public void SetGuid(string userName, string userGuid)
        {
            var sql = @"Update dbo.Users Set [GUID]=@GUID where [UserName] = @UserName";
            DoConnection(conn => conn.Query<string>(sql, new {GUID = userGuid, UserName = userName}));
        }

        /*--------------------------------Hub & Url----------------------------------------------------------------------*/
        public void SetHubId(string userName, string userHubId)
        {
            var sql = @"Update dbo.Users Set [HubId]=@HubId where [UserName] = @UserName";
            DoConnection(conn => conn.Query<string>(sql, new { HubId = userHubId, UserName = userName }));
        }

        public string GetHubId(string userName)
        {
            var userhubid = _GetUrl(userName)[0].HubId;
            return userhubid;
        }

        public int GetUrlStatus(string encode)
        {
            var url_status = _GetUrl(encode)[0].enable;
            return url_status;
        }

        public void Update_enable(string encode)
        {
            var sql = @"Update dbo.connection_list Set [enable]=@Enable where [url] = @Encode";
            DoConnection(conn => conn.Query<string>(sql, new { Enable = 1, Encode = encode }));
        }

        /*--------------------------------Hub & Url----------------------------------------------------------------------*/

        public bool ValidateGuid(string userName, string userGuid)
        {
            if (userGuid == _GetUsers(userName)[0].GUID) return true;
            else return false;
        }

        private bool _UserValidate(string userName, string userPassword)
        {
            var userinf = _GetUsers(userName);
            if (userinf[0].UserPassword == userPassword)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool _Userexit(string userName)
        {
            var usercount = _GetUsers(userName).Count;
            if (usercount == 1) return true;
            else return false;
        }

        private void _SetUsers(string account, string password)
        {
            var sqlForGetUser =
                @"INSERT INTO dbo.Users([UserName],[UserPassword],[DeviceStatus]) VALUES (@Account, @Password, @DeviceStatus)";
            DoConnection(conn => conn.Query<string>(sqlForGetUser,
                new {Account = account, Password = password, DeviceStatus = 0}));
        }

        private List<Users> _GetUsers(string custName)
        {
            var sql = @"SELECT * FROM [Users] WHERE UserName=@userName";
            return DoConnection(conn => conn.Query<Users>(sql, new {userName = custName})).ToList();
        }

        private List<connection_list> _GetUrl(string encode)
        {
            var sql = @"SELECT * FROM [connection_list] WHERE url like @Encode";
            var re = DoConnection(conn => conn.Query<connection_list>(sql, new { Encode = encode })).ToList();
            return re;
        }


        /*-------------------------------------Verify Table---------------------------------------------------------------------*/
        //public void SetVerify(string userName, string verifyCode)
        //{
        //    var sql = @"INSERT INTO dbo.Verify([UserName],[VerifyCode]) VALUES (@UserName, @VerifyCode)";
        //    DoConnection(conn => conn.Query<string>(sql, new {UserName = userName, VerifyCode = verifyCode}));
        //}

        //public bool ValidateVerifyCode(string userName, string verifyCode)
        //{
        //    if (verifyCode == _GetVerifyCode(userName)[0].VerifyCode) return true;
        //    else return false;
        //}

        //private List<Verify> _GetVerifyCode(string userName)
        //{
        //    var sql = @"SELECT Top 1 * FROM [Verify] WHERE UserName=@userName order by  createtime desc";
        //    return DoConnection(conn => conn.Query<Verify>(sql, new {UserName = userName})).ToList();
        //}

        /*--------------------------------DB Connection----------------------------------------------------------------------*/
        private T DoConnection<T>(Func<SqlConnection, T> func)
        {
            var conn = new SqlConnection(_connection);
            try
            {
                conn.Open();
                var result = func.Invoke(conn);
                return result;
            }
            finally
            {
                conn.Close();
            }
        }



        public class Users
        {
            public int AccountID { get; set; }
            public string UserName { get; set; }
            public string UserPassword { get; set; }
            public string GUID { get; set; }
            public int DeviceStatus { get; set; }

        }

        public class connection_list
        {
            public string UserName { get; set; }
            public string url { get; set; }
            public DateTime createdtime { get; set; }
            public int enable { get; set; }
            public string HubId { get; set; }
        }
    }
}