using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Configuration;

namespace TonyRomas
{
    public class Conexion
    {

        public string cadconexion;
        public SqlConnection sqlcon;
        public SqlDataReader sqldr;
        public String usuario = ConfigurationManager.AppSettings["usuario"].ToString();
        public String clave = ConfigurationManager.AppSettings["clave"].ToString();
        public String servidor = ConfigurationManager.AppSettings["servidor"].ToString();
        public String BD = ConfigurationManager.AppSettings["bd"].ToString();


        public void conectar()
        {
            cadconexion = "Data Source=" + servidor + "; Initial catalog =" + BD + ";user Id =" + usuario + "; password=" + clave + "";
            sqlcon = new SqlConnection(cadconexion);
        }


        public void ver(string comando)
        {
            try
            {
                conectar();
                sqlcon.Close();
                sqlcon.Open();
                SqlCommand sqlcom = new SqlCommand(comando, sqlcon);
                sqldr = sqlcom.ExecuteReader();

            }
            catch (SqlException e)
            {
                Console.WriteLine(e.Message);

            }

        }

        public void insertar(string comando)
        {
            try
            {
                //Console.WriteLine(comando);
                conectar();
                sqlcon.Close();
                sqlcon.Open();
                SqlCommand sqlcom = new SqlCommand(comando, sqlcon);
                sqlcom.ExecuteNonQuery();

            }
            catch (SqlException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public int CountResolucion(string resolucion, string tipo_doc)
        {
            int count;



            SqlCommand sqlcom = new SqlCommand(resolucion, sqlcon);
            string cadena = @"SELECT COUNT(numero_fiscal)
                            from log_impresiones
                            where resolucion = '" + resolucion + "' and Tipo_documento='" + tipo_doc + "'";
            sqlcom.CommandText = (cadena);

            count = Convert.ToInt32(sqlcom.ExecuteScalar().ToString());


            return count;
            //Fin de Recuperacion de Datos de Resolucion

        }

    }

}
