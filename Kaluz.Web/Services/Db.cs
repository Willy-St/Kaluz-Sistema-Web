using System.Data;
using Microsoft.Data.SqlClient;
namespace Kaluz.Web.Services;
public class Db(IConfiguration cfg) {
 public SqlConnection Open() {var c=new SqlConnection(cfg.GetConnectionString("Kaluz") ?? throw new InvalidOperationException("Falta ConnectionStrings:Kaluz"));c.Open();return c;}
 public static SqlParameter P(string name,object? value)=>new(name,value??DBNull.Value);
 public List<Dictionary<string,object?>> List(string sql,params SqlParameter[] ps) {using var c=Open();using var cmd=new SqlCommand(sql,c);cmd.Parameters.AddRange(ps);using var r=cmd.ExecuteReader();var all=new List<Dictionary<string,object?>>();while(r.Read()){var row=new Dictionary<string,object?>(StringComparer.OrdinalIgnoreCase);for(int i=0;i<r.FieldCount;i++)row[r.GetName(i)]=r.IsDBNull(i)?null:r.GetValue(i);all.Add(row);}return all;}
 public object? Scalar(string sql,params SqlParameter[] ps){using var c=Open();using var cmd=new SqlCommand(sql,c);cmd.Parameters.AddRange(ps);return cmd.ExecuteScalar();}
 public int Exec(string sql,params SqlParameter[] ps){using var c=Open();using var cmd=new SqlCommand(sql,c);cmd.Parameters.AddRange(ps);return cmd.ExecuteNonQuery();}
}
