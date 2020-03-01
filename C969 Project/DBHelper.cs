﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace Appointment_Tracker
{
    class DBHelper
    {
        private static int userID;
        private static string userName;
        public static string dataString = "server=3.227.166.251;database=U05DWb;uid=U05DWb;pwd=53688470925; convert zero datetime=True";

        public static string getDataString()
        {
            return dataString;
        }
        public static int getCurrentUserId()
        {
            return userID;
        }

        public static void setCurrentUserId(int currentUserId)
        {
            userID = currentUserId;
        }

        public static string getCurrentUserName()
        {
            return userName;
        }

        public static void setCurrentUserName(string currentUserName)
        {
            userName = currentUserName;
        }

        public static DataTable dashboard(string filter, bool week)
        {

            MySqlConnection conn = new MySqlConnection(dataString);
            conn.Open();
            //Week and month filters
            string query = week ? $"SELECT (select customerName from customer where customerId = appointment.customerId) as 'Customer',  start as 'Start Time', end as 'End Time', location as 'Location', title as 'Title' FROM appointment where start < '{filter}' and end < '{filter}' and createdBy = '{DBHelper.getCurrentUserId()}' and end > now() order by start;"
                : $"SELECT  (select customerName from customer where customerId = appointment.customerId) as 'Customer', start as 'Start Time', end as 'End Time', location as 'Location', title as 'Title' FROM appointment where start < '{filter}' and end < '{filter}' and createdBy = '{DBHelper.getCurrentUserId()}' and end > now() order by start;";
            MySqlCommand cmd = new MySqlCommand(query, conn);
            DataTable dt = new DataTable();
            dt.Load(cmd.ExecuteReader());
            //Converts the time to localtime from utc in DB
            foreach (DataRow row in dt.Rows)
            {
                DateTime utcStart = Convert.ToDateTime(row["Start Time"]);
                DateTime utcEnd = Convert.ToDateTime(row["End Time"]);
                row["Start Time"] = TimeZone.CurrentTimeZone.ToLocalTime(utcStart);
                row["End Time"] = TimeZone.CurrentTimeZone.ToLocalTime(utcEnd);
            }

            conn.Close();
            return dt;

        }

        public static int userCheck(string user, string pass)
        {

            MySqlConnection conn = new MySqlConnection(dataString);
            conn.Open();
            MySqlCommand cmd = new MySqlCommand($"SELECT userId, userName FROM user where userName = '{user}' AND password = '{pass}'", conn);
            MySqlDataReader rdr = cmd.ExecuteReader();
            if (rdr.HasRows)
            {
                rdr.Read();
                setCurrentUserId(Convert.ToInt32(rdr[0]));
                setCurrentUserName(Convert.ToString(rdr[1]));
                rdr.Close();
                conn.Close();
                return 1;
            }
            conn.Close();
            return 0;
        }

        public static DateTime getDateTime()
        {
            return DateTime.Now.ToUniversalTime();

        }

        public static string dateSQLFormat(DateTime dateValue)
        {
            string formatForMySql = dateValue.ToString("yyyy-MM-dd HH:mm");

            return formatForMySql;
        }

        public static void createCustomerRecord(int id, string name, int addressId, int active, DateTime dateTime, string user)
        {
            string sqlDate = dateSQLFormat(dateTime);
            MySqlConnection conn = new MySqlConnection(dataString);
            conn.Open();

            MySqlTransaction transaction = conn.BeginTransaction();
            var query = "INSERT into customer (customerId, customerName, addressId,active, createDate, createdBy, lastUpdateBy) " +
                        $"VALUES ('{id}', '{name}',  '{addressId}', '{active}', '{dateSQLFormat(dateTime)}', '{user}', '{user}')";

            MySqlCommand cmd = new MySqlCommand(query, conn);
            cmd.Transaction = transaction;
            cmd.ExecuteNonQuery();
            transaction.Commit();
            conn.Close();
        }

        public static int getMaxId(string table, string id)
        {

            MySqlConnection conn = new MySqlConnection(dataString);
            conn.Open();
            var query = $"SELECT max({id}) FROM {table}";
            MySqlCommand cmd = new MySqlCommand(query, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();

            if (rdr.HasRows)
            {
                rdr.Read();
                if (rdr[0] == DBNull.Value)
                {
                    return 0;
                }
                return Convert.ToInt32(rdr[0]); ;
            }
            return 0;
        }

        public static int makeCountryRecord(string country)
        {
            int countryID = getMaxId("country", "countryID") + 1;
            string user = getCurrentUserName();
            DateTime utc = getDateTime();
            MySqlConnection conn = new MySqlConnection(dataString);
            conn.Open();

            MySqlTransaction transaction = conn.BeginTransaction();

            var query = "INSERT into country (countryID, country, createDate, createdBy, lastUpdateBy) " +
                        $"VALUES ('{countryID}', '{country}', '{dateSQLFormat(utc)}','{user}', '{user}')";
            
            MySqlCommand cmd = new MySqlCommand(query, conn);
            cmd.Transaction = transaction;
            cmd.ExecuteNonQuery();
            transaction.Commit();
            conn.Close();
            return countryID;
        }

        public static int createCity(int countryID, string city)
        {
            int cityID = getMaxId("city", "cityId") + 1;
            string user = getCurrentUserName();
            DateTime utc = getDateTime();

            MySqlConnection conn = new MySqlConnection(dataString);
            conn.Open();

            MySqlTransaction transaction = conn.BeginTransaction();

            var query = "INSERT into city (cityId, city, countryId, createDate, createdBy, lastUpdateBy) " +
                        $"VALUES ('{cityID}', '{city}', '{countryID}', '{dateSQLFormat(utc)}','{user}', '{user}')";
            MySqlCommand cmd = new MySqlCommand(query, conn);
            cmd.Transaction = transaction;
            cmd.ExecuteNonQuery();
            transaction.Commit();
            conn.Close();

            return cityID;

        }

        public static int createAddress(int cityID, string address, string postalCode, string phone)
        {

            int addressID = getMaxId("address", "addressId") + 1;
            string user = getCurrentUserName();
            DateTime utc = getDateTime();
            MySqlConnection conn = new MySqlConnection(dataString);
            conn.Open();

            MySqlTransaction transaction = conn.BeginTransaction();

            var query = "INSERT into address (addressId, address, cityId, postalCode, phone, createDate, createdBy, lastUpdateBy) " +
                        $"VALUES ('{addressID}', '{address}', '{cityID}', '{postalCode}', '{phone}', '{dateSQLFormat(utc)}','{user}', '{user}')";
            
            MySqlCommand cmd = new MySqlCommand(query, conn);
            cmd.Transaction = transaction;
            cmd.ExecuteNonQuery();
            transaction.Commit();
            conn.Close();

            return addressID;
        }

        public static List<KeyValuePair<string, object>> searchCustomer(int customerID)
        {
            var list = new List<KeyValuePair<string, object>>();
            MySqlConnection conn = new MySqlConnection(dataString);
            conn.Open();
            var query = $"SELECT * FROM customer WHERE customerId = {customerID}";
            MySqlCommand cmd = new MySqlCommand(query, conn);

            MySqlDataReader rdr = cmd.ExecuteReader();
            try
            {
                if (rdr.HasRows)
                {
                    rdr.Read();
                    list.Add(new KeyValuePair<string, object>("customerId", rdr[0]));
                    list.Add(new KeyValuePair<string, object>("customerName", rdr[1]));
                    list.Add(new KeyValuePair<string, object>("addressId", rdr[2]));
                    list.Add(new KeyValuePair<string, object>("active", rdr[3]));
                    rdr.Close();
                }
                else
                {
                    MessageBox.Show("No Customer found with the ID: " + customerID, "Please try again");
                    return null;
                }

                var addressID = list.First(kvp => kvp.Key == "addressId").Value;

                var query2 = $"SELECT * FROM address WHERE addressId = {addressID}";
                cmd.CommandText = query2;
                cmd.Connection = conn;
                MySqlDataReader rdr2 = cmd.ExecuteReader();
                if (rdr2.HasRows)
                {
                    rdr2.Read();
                    list.Add(new KeyValuePair<string, object>("address", rdr2[1]));
                    list.Add(new KeyValuePair<string, object>("cityId", rdr2[3]));
                    list.Add(new KeyValuePair<string, object>("postalCode", rdr2[4]));
                    list.Add(new KeyValuePair<string, object>("phone", rdr2[5]));
                    rdr2.Close();
                }

                var cityID = list.First(kvp => kvp.Key == "cityId").Value;

                var query3 = $"SELECT * FROM city WHERE cityId = {cityID}";
                cmd.CommandText = query3;
                cmd.Connection = conn;
                MySqlDataReader rdr3 = cmd.ExecuteReader();
                if (rdr3.HasRows)
                {
                    rdr3.Read();
                    list.Add(new KeyValuePair<string, object>("city", rdr3[1]));
                    list.Add(new KeyValuePair<string, object>("countryId", rdr3[2]));
                    rdr3.Close();
                }

                var countryID = list.First(kvp => kvp.Key == "countryId").Value;

                var query4 = $"SELECT * FROM country WHERE countryId = {countryID}";
                cmd.CommandText = query4;
                cmd.Connection = conn;
                MySqlDataReader rdr4 = cmd.ExecuteReader();
                if (rdr4.HasRows)
                {
                    rdr4.Read();
                    list.Add(new KeyValuePair<string, object>("country", rdr4[1]));
                    rdr4.Close();
                }

                return list;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        public static List<KeyValuePair<string, object>> lookupAppointment(int appointmentID)
        {
            var list = new List<KeyValuePair<string, object>>();
            MySqlConnection conn = new MySqlConnection(dataString);
            conn.Open();
            var query = $"SELECT * FROM appointment WHERE appointmentId = {appointmentID}";
            MySqlCommand cmd = new MySqlCommand(query, conn);

            MySqlDataReader rdr = cmd.ExecuteReader();
            try
            {
                if (rdr.HasRows)
                {
                    rdr.Read();
                    list.Add(new KeyValuePair<string, object>("appointmentId", rdr[0]));
                    list.Add(new KeyValuePair<string, object>("customerId", rdr[1]));
                    list.Add(new KeyValuePair<string, object>("title", rdr[3]));
                    list.Add(new KeyValuePair<string, object>("description", rdr[4]));
                    list.Add(new KeyValuePair<string, object>("location", rdr[5]));
                    list.Add(new KeyValuePair<string, object>("contact", rdr[6]));
                    list.Add(new KeyValuePair<string, object>("type", rdr[7]));
                    list.Add(new KeyValuePair<string, object>("start", rdr[9]));
                    list.Add(new KeyValuePair<string, object>("end", rdr[10]));
                    rdr.Close();
                }
                else
                {
                    MessageBox.Show("No Appointment found with the ID: " + appointmentID, "Please try again");
                    return null;
                }

                return list;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return null;
            }
        }

        public static void updateCustomer(IDictionary<string, object> dictionary)
        {
            string user = getCurrentUserName();
            DateTime utc = getDateTime();

            MySqlConnection conn = new MySqlConnection(dataString);
            conn.Open();
            MySqlTransaction transaction = conn.BeginTransaction();
            var query = $"UPDATE country" +
                $" SET country = '{dictionary["country"]}', lastUpdateBy = '{user}'" +
                $" WHERE countryId = '{dictionary["countryId"]}'";
            MySqlCommand cmd = new MySqlCommand(query, conn);
            cmd.Transaction = transaction;
            cmd.ExecuteNonQuery();
            transaction.Commit();

            // Start a city transaction.
            transaction = conn.BeginTransaction();
            var query2 = $"UPDATE city" +
                $" SET city = '{dictionary["city"]}', lastUpdateBy = '{user}'" +
                $" WHERE cityId = '{dictionary["cityId"]}'";
            cmd.CommandText = query2;
            cmd.Connection = conn;
            cmd.Transaction = transaction;
            cmd.ExecuteNonQuery();
            transaction.Commit();

            // Start a address transaction.
            transaction = conn.BeginTransaction();
            var query3 = $"UPDATE address" +
                $" SET address = '{dictionary["address"]}', lastUpdateBy = '{user}', postalCode = '{dictionary["postalCode"]}', phone = '{dictionary["phone"]}'" +
                $" WHERE addressId = '{dictionary["addressId"]}'";
            cmd.CommandText = query3;
            cmd.Connection = conn;
            cmd.Transaction = transaction;
            cmd.ExecuteNonQuery();
            transaction.Commit();

            // Start a customer transaction.
            transaction = conn.BeginTransaction();
            var query4 = $"UPDATE customer" +
                $" SET customerName = '{dictionary["customerName"]}', lastUpdateBy = '{user}', active = '{dictionary["active"]}'" +
                $" WHERE customerId = '{dictionary["customerId"]}'";
            cmd.CommandText = query4;
            cmd.Connection = conn;
            cmd.Transaction = transaction;
            cmd.ExecuteNonQuery();
            transaction.Commit();
            conn.Close();
        }

        public static void deleteCustomer(IDictionary<string, object> dictionary)
        {
            MySqlConnection conn = new MySqlConnection(dataString);
            conn.Open();
            var query4 = $"DELETE FROM customer" +
               $" WHERE customerId = '{dictionary["customerId"]}'";
            MySqlCommand cmd = new MySqlCommand(query4, conn);
            MySqlTransaction transaction = conn.BeginTransaction();

            cmd.CommandText = query4;
            cmd.Connection = conn;
            cmd.Transaction = transaction;
            cmd.ExecuteNonQuery();
            transaction.Commit();


            // Start a address transaction.
            transaction = conn.BeginTransaction();
            var query3 = $"DELETE FROM address" +
                $" WHERE addressId = '{dictionary["addressId"]}'";
            cmd.CommandText = query3;
            cmd.Connection = conn;
            cmd.Transaction = transaction;
            cmd.ExecuteNonQuery();
            transaction.Commit();

            // Start a city transaction.
            transaction = conn.BeginTransaction();
            var query2 = $"DELETE FROM city" +
                $" WHERE cityId = '{dictionary["cityId"]}'";
            cmd.CommandText = query2;
            cmd.Connection = conn;
            cmd.Transaction = transaction;
            cmd.ExecuteNonQuery();
            transaction.Commit();

            // Start a country transaction.
            transaction = conn.BeginTransaction();
            var query = $"DELETE FROM country" +
                $" WHERE countryId = '{dictionary["countryId"]}'";
            cmd.CommandText = query;
            cmd.Connection = conn;
            cmd.Transaction = transaction;
            cmd.ExecuteNonQuery();
            transaction.Commit();
            conn.Close();


        }

        public static void createAppointment(int custID, string title, string description, string location, string contact, string type, DateTime start, DateTime endTime)
        {
            int appointID = getMaxId("appointment", "appointmentId") + 1;
            int userID = 1;

            DateTime utc = getDateTime();

            MySqlConnection conn = new MySqlConnection(dataString);
            conn.Open();
            MySqlTransaction transaction = conn.BeginTransaction(); ;
            // Start a local transaction.
            var query = "INSERT into appointment (appointmentId, customerId, userId, title, description, location, contact, type, url, start, end, createDate, createdBy, lastUpdateBy) " +
                        $"VALUES ('{appointID}', '{custID}', '{userID}','{title}', '{description}', '{location}', '{contact}', '{type}', '{custID}', '{dateSQLFormat(start)}','{dateSQLFormat(endTime)}','{dateSQLFormat(utc)}','{userID}','{userID}')";
            MySqlCommand cmd = new MySqlCommand(query, conn);
            cmd.Transaction = transaction;
            cmd.ExecuteNonQuery();
            transaction.Commit();
            conn.Close();

        }
        public static void updateAppointment(IDictionary<string, object> dictionary)
        {
            string user = getCurrentUserName();
            DateTime utc = getDateTime();
            DateTime start = Convert.ToDateTime(dictionary["start"]);
            DateTime end = Convert.ToDateTime(dictionary["end"]);

            MySqlConnection conn = new MySqlConnection(dataString);
            conn.Open();
            MySqlTransaction transaction = conn.BeginTransaction();
            var query = $"UPDATE appointment" +
                $" SET customerId = '{dictionary["customerId"]}', title = '{dictionary["title"]}', description = '{dictionary["description"]}' , location = '{dictionary["location"]}' , contact = '{dictionary["contact"]}' , " +
                $" type = '{dictionary["type"]}' ,  start = '{dateSQLFormat(start.ToUniversalTime())}' , end = '{dateSQLFormat(end.ToUniversalTime())}' , url = '{dictionary["url"]}' , lastUpdate = '{dateSQLFormat(utc)}',  lastUpdateBy = '{user}' " +
                $" WHERE appointmentId = '{dictionary["appointmentId"]}'";
            MySqlCommand cmd = new MySqlCommand(query, conn);
            cmd.Transaction = transaction;
            cmd.ExecuteNonQuery();
            transaction.Commit();
            conn.Close();
        }

        public static void deleteAppointment(IDictionary<string, object> dictionary)
        {
            MySqlConnection conn = new MySqlConnection(dataString);
            conn.Open();
            var query = $"DELETE FROM appointment" +
               $" WHERE appointmentId = '{dictionary["appointmentId"]}'";
            MySqlCommand cmd = new MySqlCommand(query, conn);
            MySqlTransaction transaction = conn.BeginTransaction();
            cmd.CommandText = query;
            cmd.Connection = conn;
            cmd.Transaction = transaction;
            cmd.ExecuteNonQuery();
            transaction.Commit();
            conn.Close();
        }

        public static Dictionary<string, object> getNextAppointment()
        {
            Dictionary<string, object> appointINFO = new Dictionary<string, object>();

            MySqlConnection conn = new MySqlConnection(dataString);
            conn.Open();
            var query = " Select start, (select customerName from customer where customerId = appointment.customerId ) as 'Name' from appointment  where start > now() order by  start limit 1;";
            MySqlCommand cmd = new MySqlCommand(query, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();
            if (rdr.HasRows)
            {
                rdr.Read();
                appointINFO.Add("start", Convert.ToDateTime(rdr[0]).ToLocalTime());
                appointINFO.Add("name", rdr[1]);
            }


            return appointINFO;
        }

        public static int overlap(DateTime start, DateTime end)
        {
            MySqlConnection conn = new MySqlConnection(dataString);
            conn.Open();
            var query = $"SELECT count(*) FROM `appointment` WHERE (('{dateSQLFormat(start.ToUniversalTime())}' > start and '{dateSQLFormat(start.ToUniversalTime())}' < end) or ('{dateSQLFormat(end.ToUniversalTime())}'> start and '{dateSQLFormat(end.ToUniversalTime())}' < end)) and end > now() order by  start limit 1;";
            MySqlCommand cmd = new MySqlCommand(query, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();
            if (rdr.HasRows)
            {
                rdr.Read();
                string count = rdr[0].ToString();
                int result = count == "0" ? 0 : 1;
                return result;
            }
            return 0;
        }

        public static DataTable schedule(string id)
        {

            MySqlConnection conn = new MySqlConnection(dataString);
            conn.Open();

            string query = $"SELECT (select customerName from customer where customerId = appointment.customerId) as 'Customer',  start as 'Start Time', end as 'End Time', location as 'Location', title as 'Title' FROM appointment where createdBy = '{id}' order by start;";
            MySqlCommand cmd = new MySqlCommand(query, conn);
            DataTable dt = new DataTable();
            dt.Load(cmd.ExecuteReader());
            //Converts the time to localtime from utc in DB
            foreach (DataRow row in dt.Rows)
            {
                DateTime utcStart = Convert.ToDateTime(row["Start Time"]);
                DateTime utcEnd = Convert.ToDateTime(row["End Time"]);
                row["Start Time"] = TimeZone.CurrentTimeZone.ToLocalTime(utcStart);
                row["End Time"] = TimeZone.CurrentTimeZone.ToLocalTime(utcEnd);
            }

            conn.Close();
            return dt;
        }
        public static Dictionary<string, object> appointmentReport(string item)
        {
            Dictionary<string, object> reportINFO = new Dictionary<string, object>();

            MySqlConnection conn = new MySqlConnection(dataString);
            conn.Open();
            var query = $"select distinct" +
                $"              (select count(type) from appointment where type = '{item}' and MONTH(appointment.start) = 1) as 'January'," +
                $"                (select count(type) from appointment where type = '{item}' and MONTH(appointment.start) = 2) as 'February'," +
                $"                (select count(type) from appointment where type = '{item}' and MONTH(appointment.start) = 3) as 'March'," +
                $"                (select count(type) from appointment where type = '{item}' and MONTH(appointment.start) = 4) as 'April'," +
                $"                (select count(type) from appointment where type = '{item}' and MONTH(appointment.start) = 5) as 'May'," +
                $"            (select count(type) from appointment where type = '{item}' and MONTH(appointment.start) = 6) as 'June'," +
                $"                (select count(type) from appointment where type = '{item}' and MONTH(appointment.start) = 7) as 'July'," +
                $"                (select count(type) from appointment where type = '{item}' and MONTH(appointment.start) = 8) as 'August'," +
                $"                (select count(type) from appointment where type = '{item}' and MONTH(appointment.start) = 9) as 'September'," +
                $"            (select count(type) from appointment where type = '{item}' and MONTH(appointment.start) = 10) as 'October'," +
                $"                (select count(type) from appointment where type = '{item}' and MONTH(appointment.start) = 11) as 'November'," +
                $"                (select count(type) from appointment where type = '{item}' and MONTH(appointment.start) = 12) as 'December'" +
                $"            from appointment;";

            MySqlCommand cmd = new MySqlCommand(query, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();
            if (rdr.HasRows)
            {
                rdr.Read();
                reportINFO.Add("January", rdr[0]);
                reportINFO.Add("February", rdr[1]);
                reportINFO.Add("March", rdr[2]);
                reportINFO.Add("April", rdr[3]);
                reportINFO.Add("May", rdr[4]);
                reportINFO.Add("June", rdr[5]);
                reportINFO.Add("July", rdr[6]);
                reportINFO.Add("August", rdr[7]);
                reportINFO.Add("September", rdr[8]);
                reportINFO.Add("October", rdr[9]);
                reportINFO.Add("November", rdr[10]);
                reportINFO.Add("December", rdr[11]);
            }

            return reportINFO;
        }
        public static bool checkAppointment(string custID)
        {
            Console.WriteLine(custID);

            MySqlConnection conn = new MySqlConnection(dataString);
            conn.Open();
            var query = $"select* from appointment where customerId = '{custID}'";


            MySqlCommand cmd = new MySqlCommand(query, conn);
            MySqlDataReader rdr = cmd.ExecuteReader();
            if (rdr.HasRows)
            {
                rdr.Read();
                return true;
            }
            return false;
        }
        public static void deleteAppointment(string custID)
        {
            MySqlConnection conn = new MySqlConnection(dataString);
            conn.Open();
            var query = $"DELETE FROM appointment" +
               $" WHERE customerId = '{custID}'";
            MySqlCommand cmd = new MySqlCommand(query, conn);
            MySqlTransaction transaction = conn.BeginTransaction();
            cmd.CommandText = query;
            cmd.Connection = conn;
            cmd.Transaction = transaction;
            cmd.ExecuteNonQuery();
            transaction.Commit();
            conn.Close();
        }
    }
}

