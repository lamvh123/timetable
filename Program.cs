using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    class Program
    {
        static void Main(string[] args)
        {
            SqlConnection conn = new SqlConnection();
            conn.ConnectionString =
            "Data Source=localhost;" +
            "Initial Catalog=ECM_DB;" +
            "User id=sa;" +
            "Password=123456;";
            conn.Open();
            string SqlString = "SELECT * FROM TIMETABLE;";
            SqlDataAdapter sda = new SqlDataAdapter(SqlString, conn);
            DataTable dt = new DataTable();
            try
            {
                List<Course> listOfAllClass = new List<Course>();
                //add to list
                Course c = new Course();
                List<int> listDayOfWeek = new List<int>();
                listDayOfWeek.Add(2);
                listDayOfWeek.Add(4);
                listDayOfWeek.Add(6);
                c.dayOfWeek = listDayOfWeek;
                c.startDate = "2019-06-10";
                c.slot = 1;
                listOfAllClass.Add(c);
                //add to list
                c = new Course();
                listDayOfWeek = new List<int>();
                listDayOfWeek.Add(2);
                listDayOfWeek.Add(4);
                listDayOfWeek.Add(6);
                c.dayOfWeek = listDayOfWeek;
                c.startDate = "2019-06-10";
                c.slot = 1;
                listOfAllClass.Add(c);
                for(int i = 0; i < listOfAllClass.Count; i++)
                {
                    int classId = 0;
                    //select class id cua lop de insert, chua co cac class nen tam thoi sinh ra cai nay de insert
                    String sqlGetClassId = "  select next value for CLASS_SEQ  as a;";
                    SqlCommand sqlCommand = new SqlCommand(sqlGetClassId, conn);
                    SqlDataReader sqlGetIDreader = sqlCommand.ExecuteReader();
                    if (sqlGetIDreader.HasRows)
                    {
                        while (sqlGetIDreader.Read())
                        {
                            classId = (int)sqlGetIDreader.GetInt64(0);
                        }
                    }
                    sqlGetIDreader.Close();
                    String[] dateData = listOfAllClass[i].startDate.Split('-');
                    DateTime date = new DateTime(int.Parse(dateData[0]), int.Parse(dateData[1]), int.Parse(dateData[2]));
                    String start = date.ToString("yyyy-MM-dd");
                    // 3 la so buoi/ 1 tuan, 7 la so ngay trong tuan,30 la so buoi hoc, 10 la + them vao cho chac
                    date = date.AddDays(7*30/3 + 10);
                    String end = date.ToString("yyyy-MM-dd");
                    //select ra 30 ngay hoc cu the cua lop
                    String sql = "DECLARE @DateFrom DateTime =@val1, @DateTo DateTime = @val2 ; " +

"WITH CTE(dt) " +
"AS " +
"(" +
      "SELECT @DateFrom " +
      "UNION ALL " + 
      "SELECT DATEADD(d, 1, dt) FROM CTE " +
      "WHERE dt < @DateTo " +
") " +
"SELECT top 30 dt,datepart(dw, dt) FROM CTE  where datepart(dw, dt) = @val3 or datepart(dw, dt) = @val4 or datepart(dw, dt) = @val5; ";
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.Add("@val1", SqlDbType.VarChar).Value = start;
                    cmd.Parameters.Add("@val2", SqlDbType.VarChar).Value = end;
                    cmd.Parameters.AddWithValue("@val3", listOfAllClass[i].dayOfWeek[0]);
                    cmd.Parameters.AddWithValue("@val4", listOfAllClass[i].dayOfWeek[1]);
                    cmd.Parameters.AddWithValue("@val5", listOfAllClass[i].dayOfWeek[2]);
                    cmd.CommandType = CommandType.Text;
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        dt = new DataTable();                       
                        dt.Load(reader);
                        foreach(DataRow dr in dt.Rows)
                        {
                            //String d = dr["dt"].ToString();
                            DateTime d = DateTime.Parse(dr["dt"].ToString());
                            int slot = listOfAllClass[i].slot;
                            //select top 1 room (increase) where no class learns in this day and this slot
                            sql = "select top 1 id from room where id not in ( "+
" select room_id from TIMETABLE where cast(time as date) = @val1 and slot = @val2 group by cast(time as date),slot,room_id) ";
                            
                            cmd = new SqlCommand(sql, conn);
                            cmd.Parameters.AddWithValue("@val1", d);
                            cmd.Parameters.AddWithValue("@val2", slot);
                            reader.Close();
                            SqlDataReader r = cmd.ExecuteReader();
                            //room insert to db, neu ko tim dc room thi se de trong
                            int room = 0;
                            if (r.HasRows)
                            {
                                DataTable insertedData = new DataTable();
                                insertedData.Load(r);
                                foreach(DataRow rw in insertedData.Rows)
                                {
                                    room = int.Parse(rw["id"].ToString());
                                }
                            }
                            String sqlInsert = "insert into Timetable(ID,CLASS_ID,TIME,TEACHER_ID,ROOM_ID,Slot)" +
                                " values(NEXT VALUE FOR TIMETABLE_SEQ,@classid,@time,0,@room,@slot)";
                            SqlCommand insertcmd = new SqlCommand(sqlInsert, conn);
                            
                            insertcmd.Parameters.Add("@time", SqlDbType.DateTime).Value = d; 
                            insertcmd.Parameters.AddWithValue("@classid", classId);
                            insertcmd.Parameters.AddWithValue("@room", room);
                            insertcmd.Parameters.AddWithValue("@slot", slot);
                            r.Close();
                            insertcmd.ExecuteNonQuery();
                        }
                    }

                }
                System.Console.WriteLine("done");
            }
            catch (Exception se)
            {
                System.Console.WriteLine(se.ToString());
                
            }
            finally
            {
                conn.Close();
                System.Console.ReadLine();
            }
        }
    }
}
