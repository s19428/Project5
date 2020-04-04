using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebApplication1.DTOs.Requests;

namespace WebApplication1.Services
{
    public class SqlServerStudentDbService : IStudentServiceDb
    {
        private const string connectionString = @"Server=localhost\SQLEXPRESS01;Integrated Security=true;";
        public void EnrollStudent(EnrollStudentRequest request)
        {
            //
            //1. Validation - OK
            //2. Check if studies exists -> 404
            //3. Check if enrollment exists -> INSERT
            //4. Check if index does not exists -> INSERT/400
            //5. return Enrollment model

            using (var con = new SqlConnection(connectionString))
            using (var com = new SqlCommand())
            {

                com.CommandText = "SELECT * FROM Studies WHERE Name=@Name";
                com.Parameters.AddWithValue("Name", request.Studies);
                com.Connection = con;

                con.Open();
                var tran = con.BeginTransaction();
                com.Transaction = tran;

                //2. EXECUTE THE 1 statement
                var dr = com.ExecuteReader();
                if (!dr.Read())
                {
                    dr.Close();
                    tran.Rollback(); ///...
                    //ERROR - 404 - Studies does not exists
                    throw new HttpException(404, "Studies does not exists");
                }
                int idStudies = (int)dr["IdStudy"];
                dr.Close();

                //3.
                com.CommandText = "SELECT * FROM Enrollment WHERE Semester=1 AND IdStudy=@IdStud";
                com.Parameters.AddWithValue("IdStud", idStudies);
                dr = com.ExecuteReader();

                int idEnrollment = 1;

                if (!dr.Read())
                {
                    dr.Close();

                    com.CommandText = "select MAX(IdEnrollment) as IdEnrollment from Enrollment";
                    var dr1 = com.ExecuteReader();
                    if (dr1.Read())
                        idEnrollment = (int)dr1["IdEnrollment"] + 1;
                    dr1.Close();

                    com.CommandText = "insert into Enrollment values (@IdEnrollment, 2, 1, CONVERT(varchar, '2019-10-10', 23));";
                    com.Parameters.AddWithValue("IdEnrollment", idEnrollment);
                    com.ExecuteNonQuery();
                }
                else
                {
                    dr.Close();
                }
                //...

                //4. ....

                //x.. INSERT Student
                /*
                com.CommandText = "INSERT INTO Student(IndexNumber, FirstName, LastName) VALUES (@FirstName, @LastName, .....";
                //...
                com.Parameters.AddWithValue("FistName", request.FirstName);
                //...
                */
                com.CommandText = "insert into Student values (@studentIndexNumber, @studentName, @studentSurname, CONVERT(varchar, @studentBirthDate, 23), @studentStudies);";
                com.Parameters.AddWithValue("studentIndexNumber", request.IndexNumber);
                com.Parameters.AddWithValue("studentName", request.FirstName);
                com.Parameters.AddWithValue("studentSurname", request.LastName);
                com.Parameters.AddWithValue("studentBirthDate", request.Birthdate);
                com.Parameters.AddWithValue("studentStudies", idEnrollment);

                com.ExecuteNonQuery();


                tran.Commit(); //make all the changes in db visible to another users

                ///tran.Rollback();
            }

        }

        public void PromoteStudents(int semester, string studies)
        {
            // 1. Check if Enrollment table contains provided Studies and Semester. Otherwise return 404(Not Found)

            using (var con = new SqlConnection(connectionString))
            using (var com = new SqlCommand())
            {
                // 1.
                com.CommandText = "SELECT * FROM Enrollment WHERE Semester = @semester AND IdStudy = (select s.IdStudy from Studies s where s.name = @studies)";
                com.Parameters.AddWithValue("semester", semester);
                com.Parameters.AddWithValue("studies", studies);

                com.Connection = con;

                con.Open();

                var tran = con.BeginTransaction();
                com.Transaction = tran;

                // 1. execute first statement
                var dr = com.ExecuteReader();
                if (!dr.Read())
                {
                    dr.Close();
                    tran.Rollback();
                    throw new HttpException(404, "Enrollment with thease parameters not found");
                }
                int idStudies = (int)dr["IdStudy"];
                dr.Close();


                tran.Commit(); //make all the changes in db visible to another users

                ///tran.Rollback();
            }
        }
    }
}
