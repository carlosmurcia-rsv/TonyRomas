using Microsoft.Reporting.WinForms;
using RawPrint;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace TonyRomas
{
    public partial class Form1 : Form
    {
        public string pathExe = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).Remove(0, 6) + "\\";
        DirectoryInfo di = new DirectoryInfo(ConfigurationManager.AppSettings["direccion_data"].ToString());
        DataTable table;

        public String impresor = "";
        String nombre_sucursal ="",tipo_factura="",numero="",chk="";
        String fecha = "", caja = "", cajero = "", mesero = "", mesa = "";
        String Propina = "", subtotal_gravada = "", subtotal_exento="", subtotal_nosujeta="", subtotal_serv="";
        String total = "", pagos = "", credomatic = "", nit_dui="", nombre_cli="";
        String para = "", cant_prod="", promerica="", efectivo="", subtotal_desc="",cxc="";
        String total_enletras;
        //VARIABLES DE SUCURSAL
        String nitEmpresa = ConfigurationManager.AppSettings["nit"].ToString();
        String nrcEmpresa = ConfigurationManager.AppSettings["nrc"].ToString();
        String nombre_largo = ConfigurationManager.AppSettings["nombre_largo"].ToString();
        String direccionEmp = ConfigurationManager.AppSettings["direccion"].ToString();
        String leyendaFiscal = "", direccion_cli="", retencion_1="";
        public String restaurante_xml = ConfigurationManager.AppSettings["nombre"].ToString();

        public string RutaConfigXML = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).Remove(0, 6) + "\\" + @"Configuracion/Config.xml";
        XDocument Config = XDocument.Load(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).Remove(0, 6) + "\\" + @"Configuracion/Config.xml");

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        string ruta_archivo = @"C:\DOT\Originales\" + DateTime.Now.Year + @"\" + DateTime.Now.Month + @"\" + DateTime.Now.Day + @"\";

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        public String guardar_emisores = ConfigurationManager.AppSettings["guardar_emisores"].ToString();
        string ruta_archivo_original = @"C:\DOT\PDFCLIENTE\" + DateTime.Now.Year + @"\" + DateTime.Now.Month + @"\" + DateTime.Now.Day + @"\";
        string ruta_archivo_pdf_emisor = @"C:\DOT\PDFEMISOR\" + DateTime.Now.Year + @"\" + DateTime.Now.Month + @"\" + DateTime.Now.Day + @"\";
        string ruta_archivo_pdf_copia_cliente = @"C:\DOT\PDFCOPICLIENTE\" + DateTime.Now.Year + @"\" + DateTime.Now.Month + @"\" + DateTime.Now.Day + @"\";
        public String impresora = ConfigurationManager.AppSettings["impresora"].ToString();
        string time = DateTime.Now.ToString("ddmmss");
        
        List<string[]> detalle_factura_cliente = new List<string[]>();
        List<datos> datos = new List<datos>();
       

        public Form1()
        {
            InitializeComponent();
        }

       

        private void timer1_Tick(object sender, EventArgs e)
        {
          
           

            timer1.Enabled = false;
            try
            {


                foreach (var i in di.GetFiles())
                {

                    //limpiar datos del objeto datos y listas detalle
                    nombre_sucursal = ""; tipo_factura = ""; numero = ""; chk = "";
                    fecha = ""; caja = ""; cajero = ""; mesero = ""; mesa = "";
                    Propina = ""; subtotal_gravada = ""; subtotal_exento = ""; subtotal_nosujeta = ""; subtotal_serv = "";

                    total = ""; pagos = ""; credomatic = ""; nit_dui = ""; nombre_cli = "";
                    para = ""; cant_prod = ""; promerica = ""; efectivo = ""; subtotal_desc = ""; cxc = "";
                    

                    detalle_factura_cliente.Clear();
                    datos.Clear();

                    string ruta = i.FullName;
                    string nombre_archivo = i.Name;
                    using (StreamReader readFile = new StreamReader(ruta, Encoding.Default))
                    {

                        string line;
                        int iterador = 0;

                        while ((line = readFile.ReadLine()) != null)
                        {
                            //iterador 6 para segunda data
                           // if(iterador==6)
                            if (iterador == 2)
                            {
                
                               string linea = line.Substring(0,11).Trim();
                                //linea del 0 al 15 para segunda data
                              //  string linea = line.Substring(0, 15).Trim();
                                switch (linea)
                                {

                                    case "Factura CF":
                                        factura_cliente(ruta, impresora);
                                        readFile.Close();
                                      

                                        if (!(Directory.Exists(ruta_archivo)))
                                        {
                                            Directory.CreateDirectory(ruta_archivo);
                                        }

                                        System.IO.File.Move(ruta, ruta_archivo + nombre_archivo + time);
                                        File.Delete(ruta);


                                        break;
                                    case "CONSUMIDORFINAL":
                                        factura_cliente_dta2(ruta,impresora);
                                        readFile.Close();
                                        datos.Clear();

                                        if (!(Directory.Exists(ruta_archivo)))
                                        {
                                            Directory.CreateDirectory(ruta_archivo);
                                        }

                                        System.IO.File.Move(ruta, ruta_archivo + nombre_archivo + time);
                                        File.Delete(ruta);


                                        break;
                                  
                                  
                                }


                            }
                         
                            iterador = iterador + 1;

                        }
                        readFile.Close();

                    }

                }


            }
            catch (Exception x)
            {
                Console.Write(x.Message);
            }

         
            timer1.Enabled = true;

     
    }

        private void Form1_Load(object sender, EventArgs e)
        {

            //this.reportViewer1.RefreshReport();
        }

        private void factura_cliente(string ruta, string impresora)
        {
            EscribirEnLog("Procesando facturacion", ruta);
            try
            {
                using (StreamReader readFile = new StreamReader(ruta, Encoding.Default))
                {
                    string line;
                    int iterador = 0;
                    while ((line = readFile.ReadLine()) != null)
                    {
                        if (iterador == 0)
                        {
                            nombre_sucursal = line;
                        }
                        if (iterador == 2)
                        {
                            numero = Regex.Replace(numero, @"[^a-zA-Z]+", "");
                            numero = line.Replace("Factura CF  Nro.: ", " ").Replace("\0", "").Trim();

                        }
                        if (iterador == 3)
                        {
                            chk = line.Substring(4, 9).Trim();
                            fecha = line.Substring(10, 22).Trim();
                        }
                        if (iterador == 4)
                        {
                            string[] commaSeparator = new string[] { ":" };
                            string[] result2;
                            result2 = line.Split(commaSeparator, StringSplitOptions.RemoveEmptyEntries);
                            caja = result2[1].Replace("Cjro", " ").Trim();
                            cajero = result2[2].Trim();

                        }
                        if (iterador == 5)
                        {
                            string[] commaSeparator = new string[] { ":" };
                            string[] result2;
                            result2 = line.Split(commaSeparator, StringSplitOptions.RemoveEmptyEntries);
                            mesero = result2[1].Trim();

                        }

                        if (iterador == 6)
                        {
                            string[] commaSeparator = new string[] { ":" };
                            string[] result2;
                            result2 = line.Split(commaSeparator, StringSplitOptions.RemoveEmptyEntries);
                            mesa = result2[1].Trim();
                        }

                        iterador = iterador + 1;

                    }
                    readFile.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error en la lectura del archivo: " + ex.Message);
            }


            //capturar el detalle de la factura

            using (StreamReader readFile = new StreamReader(ruta, Encoding.Default))
            {

                string t_letra = "PRODUCTOS:";
                String linea;
                int counter = 1;
                while ((linea = readFile.ReadLine()) != null)
                {
                    if (linea.ToLower().Contains(t_letra.ToLower()))
                    {
                        para = Regex.Replace(para, @"[^a-zA-Z]+", "");
                        para = linea.Replace("PRODUCTOS:", "").Replace("\0", "").Replace("$", "").Trim();

                        break;
                    }
                    counter++;
                }

                readFile.Close();
            }



            int counter_inicio = 1;
            int counter_final = 1;

            using (StreamReader readFile = new StreamReader(ruta, Encoding.Default))
            {
                string monto_copa = "PRODUCTOS: ";
                String linea;
                while ((linea = readFile.ReadLine()) != null)
                {
                    if (linea.ToLower().Contains(monto_copa.ToLower()))
                    {
                        break;
                    }
                    counter_inicio++;

                }
                readFile.Close();
            }

            string resultado = "";
            using (StreamReader readFile = new StreamReader(ruta, Encoding.Default))
            {
                string monto_copa = "SERVICIOS:";
                string separador = "---------------------------------";
                string descuentos = "DESCUENTOS:";
                string promo = "PROMO FB";
                String linea;
                while ((linea = readFile.ReadLine()) != null)
                {
                    if (linea.ToLower().Contains(monto_copa.ToLower()) || linea.ToLower().Contains(separador.ToLower()) || linea.ToLower().Contains(descuentos.ToLower()) || linea.ToLower().Contains(promo.ToLower()))
                    {
                        resultado = linea;
                        break;
                    }
                    counter_final++;
                }
                readFile.Close();
            }

            switch (resultado.Trim())
            {
                case "SERVICIOS:":
                    Console.WriteLine("selecciono servicios");
                    using (StreamReader readFile = new StreamReader(ruta, Encoding.Default))
                    {
                        string line;
                        int iterador = 0;

                        while ((line = readFile.ReadLine()) != null)
                        {
                            for (int i = counter_inicio; i <= counter_final - 2; i++)
                            {
                               
                                if (iterador == i && line != "")
                                {
                                    char[] commaSeparator = new char[] { '$' };
                                    string[] result;
                                    result = line.Split(commaSeparator, StringSplitOptions.None);
                                    if (line.Contains("$"))
                                    {
                                        Console.WriteLine(line.Substring(1, 3) + "" + line.Substring(3, 20) + "" + result[2].ToString());
                                        detalle_factura_cliente.Add(new string[] { line.Substring(1, 3), line.Substring(3, 20), result[2].ToString() });
                                    }

                                }
                            }
                            iterador = iterador + 1;

                        }
                        readFile.Close();
                    }
                    break;

                case "---------------------------------":
                    Console.WriteLine("selecciono separdor");

                    using (StreamReader readFile = new StreamReader(ruta, Encoding.Default))
                    {
                        string line;
                        int iterador = 0;

                        while ((line = readFile.ReadLine()) != null)
                        {
                            for (int i = counter_inicio; i <= counter_final - 3; i++)
                            {
                                if (iterador == i && line != "")
                                {
                                    char[] commaSeparator = new char[] { '$' };
                                    string[] result;
                                    result = line.Split(commaSeparator, StringSplitOptions.None);
                                    if (line.Contains("$"))
                                    {
                                        Console.WriteLine(line.Substring(1, 3) + "" + line.Substring(3, 20) + "" + result[2].ToString());
                                        detalle_factura_cliente.Add(new string[] { line.Substring(1, 3), line.Substring(3, 20), result[2].ToString() });
                                    }

                                }
                            }
                            iterador = iterador + 1;

                        }
                        readFile.Close();
                    }

                    break;
                case "DESCUENTOS:":
                    Console.WriteLine("selecciono descuentos");

                    using (StreamReader readFile = new StreamReader(ruta, Encoding.Default))
                    {
                        string line;
                        int iterador = 0;

                        while ((line = readFile.ReadLine()) != null)
                        {
                            for (int i = counter_inicio; i <= counter_final - 2; i++)
                            {
                                if (iterador == i && line != "")
                                {
                                    char[] commaSeparator = new char[] { '$' };
                                    string[] result;
                                    result = line.Split(commaSeparator, StringSplitOptions.None);
                                    if (line.Contains("$"))
                                    {

                                        Console.WriteLine(line.Substring(1, 3) + "" + line.Substring(3, 20) + "" + result[2].ToString());
                                        detalle_factura_cliente.Add(new string[] { line.Substring(1, 3), line.Substring(3, 20), result[2].ToString() });
                                    }


                                }
                            }
                            iterador = iterador + 1;

                        }
                        readFile.Close();
                    }

                    break;

                case "PROMO FB":
                    Console.WriteLine("selecciono promocion");
                    using (StreamReader readFile = new StreamReader(ruta, Encoding.Default))
                    {
                        string line;
                        int iterador = 0;

                        while ((line = readFile.ReadLine()) != null)
                        {
                            for (int i = counter_inicio; i <= counter_final - 2; i++)
                            {
                                if (iterador == i && line != "")
                                {
                                    char[] commaSeparator = new char[] { '$' };
                                    string[] result;
                                    result = line.Split(commaSeparator, StringSplitOptions.None);
                                    if (line.Contains("$"))
                                    {
                                        Console.WriteLine(line.Substring(1, 3) + "" + line.Substring(3, 20) + "" + result[2].ToString());
                                        detalle_factura_cliente.Add(new string[] { line.Substring(1, 3), line.Substring(3, 20), result[2].ToString() });
                                    }

                                }
                            }
                            iterador = iterador + 1;

                        }
                        readFile.Close();
                    }
                    break;

                default:
                    Console.WriteLine("error en la captura de la informacion");
                    break;
            }

            //agregar detalle a datatable detalle_factura
            table = new DataTable("detalle_credito_fiscal");
            table.Columns.Add("cantidad", typeof(string));
            table.Columns.Add("descripcion", typeof(string));
            table.Columns.Add("precio", typeof(string));

            string regex = "(\\[.*\\])|(\".*\")|('.*')|(\\(.*\\))";
            foreach (var i in detalle_factura_cliente)
            {
                table.Rows.Add(i[0].ToString(),Regex.Replace(i[1].ToString(),regex,""), i[2].ToString());

            }




            using (StreamReader readFile = new StreamReader(ruta, Encoding.Default))
            {

                string t_letra = "Propina";
                String linea;
                int counter = 1;
                while ((linea = readFile.ReadLine()) != null)
                {
                    if (linea.ToLower().Contains(t_letra.ToLower()))
                    {
                       Propina= Regex.Replace(Propina, @"[^a-zA-Z]+", "");
                       Propina = linea.Replace("Propina", "").Replace("\0", "").Replace("$","").Trim();
              
                        break;
                    }
                    counter++;
                }

                readFile.Close();
            }

            using (StreamReader readFile = new StreamReader(ruta, Encoding.Default))
            {

                string t_letra = "SUBTOTAL GRAV.:";
                String linea;
                int counter = 1;
                while ((linea = readFile.ReadLine()) != null)
                {
                    if (linea.ToLower().Contains(t_letra.ToLower()))
                    {
                       subtotal_gravada= Regex.Replace(subtotal_gravada, @"[^a-zA-Z]+", "");
                       subtotal_gravada = linea.Replace("SUBTOTAL GRAV.:", "").Replace("\0", "").Replace("$", "").Trim();

                        break;
                    }
                    counter++;
                }

                readFile.Close();
            }

            using (StreamReader readFile = new StreamReader(ruta, Encoding.Default))
            {

                string t_letra = "SUBTOTAL EXENTO:";
                String linea;
                int counter = 1;
                while ((linea = readFile.ReadLine()) != null)
                {
                    if (linea.ToLower().Contains(t_letra.ToLower()))
                    {
                        subtotal_exento = Regex.Replace(subtotal_exento, @"[^a-zA-Z]+", "");
                        subtotal_exento = linea.Replace("SUBTOTAL EXENTO:", "").Replace("\0", "").Replace("$", "").Trim();

                        break;
                    }
                    counter++;
                }

                readFile.Close();
            }

            using (StreamReader readFile = new StreamReader(ruta, Encoding.Default))
            {

                string t_letra = "SUBTOTAL NO SUJETAS";
                String linea;
                int counter = 1;
                while ((linea = readFile.ReadLine()) != null)
                {
                    if (linea.ToLower().Contains(t_letra.ToLower()))
                    {
                        subtotal_nosujeta = Regex.Replace(subtotal_nosujeta, @"[^a-zA-Z]+", "");
                        subtotal_nosujeta = linea.Replace("SUBTOTAL NO SUJETAS", "").Replace("\0", "").Replace("$", "").Trim();

                        break;
                    }
                    counter++;
                }

                readFile.Close();
            }


            using (StreamReader readFile = new StreamReader(ruta, Encoding.Default))
            {

                string t_letra = "SUBTOTAL SERV.:";
                String linea;
                int counter = 1;
                while ((linea = readFile.ReadLine()) != null)
                {
                    if (linea.ToLower().Contains(t_letra.ToLower()))
                    {
                        subtotal_serv = Regex.Replace(subtotal_serv, @"[^a-zA-Z]+", "");
                        subtotal_serv = linea.Replace("SUBTOTAL SERV.:", "").Replace("\0", "").Replace("$", "").Trim();

                        break;
                    }
                    counter++;
                }

                readFile.Close();
            }
            using (StreamReader readFile = new StreamReader(ruta, Encoding.Default))
            {

                string t_letra = "SUBTOTAL DESC.:";
                String linea;
                int counter = 1;
                while ((linea = readFile.ReadLine()) != null)
                {
                    if (linea.ToLower().Contains(t_letra.ToLower()))
                    {
                        subtotal_desc = Regex.Replace(subtotal_desc, @"[^a-zA-Z]+", "");
                        subtotal_desc = linea.Replace("SUBTOTAL DESC.: ", "").Replace("\0", "").Replace("$", "").Trim();

                        break;
                    }
                    counter++;
                }

                readFile.Close();
            }

            using (StreamReader readFile = new StreamReader(ruta, Encoding.Default))
            {

                string t_letra = "TOTAL:";
                String linea;
                int counter = 1;
                while ((linea = readFile.ReadLine()) != null)
                {
                    if (linea.ToLower().Contains(t_letra.ToLower()))
                    {
                        total= Regex.Replace(total, @"[^a-zA-Z]+", "");
                        total = linea.Replace("TOTAL:", "").Replace("\0", "").Replace("$", "").Trim();

                        break;
                    }
                    counter++;
                }

                readFile.Close();
            }

            using (StreamReader readFile = new StreamReader(ruta, Encoding.Default))
            {

                string t_letra = "PAGOS:";
                String linea;
                int counter = 1;
                while ((linea = readFile.ReadLine()) != null)
                {
                    if (linea.ToLower().Contains(t_letra.ToLower()))
                    {
                        pagos = Regex.Replace(pagos, @"[^a-zA-Z]+", "");
                        pagos = linea.Replace("PAGOS: ", "").Replace("\0", "").Replace("$", "").Trim();

                        break;
                    }
                    counter++;
                }

                readFile.Close();
            }

            using (StreamReader readFile = new StreamReader(ruta, Encoding.Default))
            {

                string t_letra = "CREDOMATIC";
                String linea;
                int counter = 1;
                while ((linea = readFile.ReadLine()) != null)
                {
                    if (linea.ToLower().Contains(t_letra.ToLower()))
                    {
                        credomatic = Regex.Replace(credomatic, @"[^a-zA-Z]+", "");
                        credomatic = linea.Replace("CREDOMATIC", "").Replace("\0", "").Replace("$", "").Trim();

                        break;
                    }
                    counter++;
                }

                readFile.Close();
            }



            using (StreamReader readFile = new StreamReader(ruta, Encoding.Default))
            {

                string t_letra = "NIT/DUI:";
                String linea;
                int counter = 1;
                while ((linea = readFile.ReadLine()) != null)
                {
                    if (linea.ToLower().Contains(t_letra.ToLower()))
                    {
                        nit_dui= Regex.Replace(nit_dui, @"[^a-zA-Z]+", "");
                        nit_dui = linea.Replace("NIT/DUI:", "").Replace("\0", "").Replace("$", "").Trim();

                        break;
                    }
                    counter++;
                }

                readFile.Close();
            }
            using (StreamReader readFile = new StreamReader(ruta, Encoding.Default))
            {

                string t_letra = "NOMBRE:";
                String linea;
                int counter = 1;
                while ((linea = readFile.ReadLine()) != null)
                {
                    if (linea.ToLower().Contains(t_letra.ToLower()))
                    {
                        nombre_cli= Regex.Replace(nombre_cli, @"[^a-zA-Z]+", "");
                        nombre_cli = linea.Replace("NOMBRE: ", "").Replace("\0", "").Replace("$", "").Trim();

                        break;
                    }
                    counter++;
                }

                readFile.Close();
            }

            using (StreamReader readFile = new StreamReader(ruta, Encoding.Default))
            {

                string t_letra = "Total Items:";
                String linea;
                int counter = 1;
                while ((linea = readFile.ReadLine()) != null)
                {
                    if (linea.ToLower().Contains(t_letra.ToLower()))
                    {
                        cant_prod = Regex.Replace(cant_prod, @"[^a-zA-Z]+", "");
                        cant_prod = linea.Replace("Total Items: ", "").Replace("\0", "").Replace("$", "").Trim();

                        break;
                    }
                    counter++;
                }

                readFile.Close();
            }
            using (StreamReader readFile = new StreamReader(ruta, Encoding.Default))
            {

                string t_letra = "PROMERICA";
                String linea;
                int counter = 1;
                while ((linea = readFile.ReadLine()) != null)
                {
                    if (linea.ToLower().Contains(t_letra.ToLower()))
                    {
                        promerica = Regex.Replace(promerica, @"[^a-zA-Z]+", "");
                        promerica = linea.Replace("PROMERICA ", "").Replace("\0", "").Replace("$", "").Trim();

                        break;
                    }
                    counter++;
                }

                readFile.Close();
            }

            using (StreamReader readFile = new StreamReader(ruta, Encoding.Default))
            {

                string t_letra = "EFECTIVO";
                String linea;
                int counter = 1;
                while ((linea = readFile.ReadLine()) != null)
                {
                    if (linea.ToLower().Contains(t_letra.ToLower()))
                    {
                        efectivo = "";
                        efectivo = Regex.Replace(efectivo, @"[^a-zA-Z]+", "");
                        efectivo = linea.Replace("EFECTIVO ", "").Replace("\0", "").Replace("$", "").Trim();

                        break;
                    }
                    counter++;
                }

                readFile.Close();
            }

            using (StreamReader readFile = new StreamReader(ruta, Encoding.Default))
            {

                string t_letra = "CxC";
                String linea;
                int counter = 1;
                while ((linea = readFile.ReadLine()) != null)
                {
                    if (linea.ToLower().Contains(t_letra.ToLower()))
                    {
                        cxc = Regex.Replace(cxc, @"[^a-zA-Z]+", "");
                        cxc = linea.Replace("CxC Dom HUGO ", "").Replace("CxC UberEats ", "").Replace("\0", "").Replace("$", "").Trim();

                        break;
                    }
                    counter++;
                }

                readFile.Close();
            }
            decimal totalw =Decimal.Parse(total);
          total_enletras=Conversores.NumeroALetras(totalw);


           

         




            string vTipodoc = "FCF";
            //inicio
            Conexion con = new Conexion();
            string vLectura = "Lec 0001";
            String vLineaDot = vLectura;
            //int vNumero = 0;
            int vDesde = 0;
            int vHasta = 0;
            int vIdResolucion = -1;
            int vActual = 0;
            int vCorrelativoValido = 0;
            string vDesdeString = "";
            string vHastaString = "";
            string vResolucion = "";
            string vSerie = "";
            string vFecha_Resolucion = "";
            String vNumeroString = "";
            string numero_fiscal = "";
            string vEmpresa = "Piramide";
            string vsucursal = "TRMETRO";
            string tirajefactura = "";



            string vFechaimpresion = DateTime.Now.ToString("yyyy-MM-dd");
            //string vLeyendaFiscal = "";
            string vActiva = "True";
            DateTime vFecha;

            try
            {
                con.conectar();
                string cadena = "SELECT TOP (1) Id_Resolucion,desde,hasta,actual,resolucion,serie,fecha_resolucion FROM Resoluciones Where Key_search =" + "'" + vTipodoc + "'" + "and empresa =" + "'" + vEmpresa + "'" + "and Activa =" + "'" + vActiva + "'" + "and SUCURSAL =" + "'" + vsucursal + "'" + "and (Desde<='" + numero + @"' and hasta>='" + numero + @"') ORDER BY Id_Resolucion";

                con.ver(cadena);


                while (con.sqldr.Read())
                {
                    vIdResolucion = Convert.ToInt32(con.sqldr["Id_Resolucion"]);
                    vDesde = Convert.ToInt32(con.sqldr["desde"]);
                    vHasta = Convert.ToInt32(con.sqldr["hasta"]);
                    vActual = Convert.ToInt32(con.sqldr["actual"]);
                    vResolucion = Convert.ToString(con.sqldr["resolucion"]);
                    vSerie = Convert.ToString(con.sqldr["serie"]);
                    vFecha = Convert.ToDateTime(con.sqldr["Fecha_resolucion"]);
                    vFecha_Resolucion = vFecha.ToString("dd-MM-yyyy");
                    vDesdeString = Convert.ToString(vDesde);
                    vHastaString = Convert.ToString(vHasta);



                }
                con.sqldr.Close();
                con.sqlcon.Close();




            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);

            }

            if (vResolucion != "")
            {
                //Revisar que sea una Resolucion valida
                numero_fiscal = vSerie + " " + numero.PadLeft(7, '0');
                tirajefactura = "TIRAJE DEL " + vDesde + " HASTA " + vHasta + " RESOLUCION No.: " + vResolucion + " FECHA: " + vFecha_Resolucion;
                con.conectar();
                string cadena1 = "SELECT * FROM Log_Impresiones Where tipo_documento = '" + vTipodoc + "'and resolucion = '" + vResolucion + "' and numero_Fiscal = '" + numero_fiscal + "'";
                con.ver(cadena1);
                while (con.sqldr.Read())
                {
                    numero = "";
                }
                con.sqldr.Close();
                con.sqlcon.Close();
                //Fin de Recuperacion de Datos de Resolucion

                if (numero != "")
                {
                    //Console.WriteLine("Actualizando registros de la base de datos...");
                    vLineaDot = vLectura + " " + vDesdeString.PadLeft(7, '0') + " " + vHastaString.PadLeft(7, '0') + " " + vResolucion + " " + vSerie + " " + vNumeroString.PadLeft(7, '0') + " " + vFecha_Resolucion;




                    if (vActual > vHasta)
                    {
                        vCorrelativoValido = 0;
                        vLineaDot = "ERROR-01 Resolucion no Valida o Terminada";
                     
                    }
                    else
                    {
                        vCorrelativoValido = 1;

                    }

                    if (vCorrelativoValido > 0)
                    {
                        //aqui van los reportes
                        vLineaDot = vLectura + " " + vDesdeString.PadLeft(7, '0') + " " + vHastaString.PadLeft(7, '0') + " " + vResolucion + " " + vSerie + " " + vNumeroString.PadLeft(7, '0') + " " + vFecha_Resolucion;
                        leyendaFiscal = vLineaDot;


                        datos dt = new datos();
                        dt.cxc = ""; dt.credomatic = ""; dt.efectivo = ""; dt.promerica = "";
                        datos.Clear(); 

                        //encabezado
                        dt.nombre_sucursal = nombre_sucursal;
                        dt.tipo_factura = tipo_factura;
                        dt.numero = numero;
                        dt.chk = chk;
                        dt.fecha = fecha;
                        dt.caja = caja;
                        dt.cajero = cajero;
                        dt.mesero = mesero;
                        dt.mesa = mesa;
                        dt.propina = Propina;
                        dt.subtotal_gravada = subtotal_gravada;
                        dt.subtotal_exento = subtotal_exento;
                        dt.subtotal_nosujeta = subtotal_nosujeta;
                        dt.subtotal_serv = subtotal_serv;
                        dt.total = total;
                        dt.pagos = pagos;
                        dt.credomatic = credomatic;
                        dt.nit_dui = nit_dui;
                        dt.nombre_cli = nombre_cli;
                        dt.para = para;
                        dt.cant_prod = cant_prod;
                        dt.promerica = promerica;
                        dt.efectivo = efectivo;
                        dt.subtotal_desc = subtotal_desc;
                        dt.cxc = cxc;
                        dt.t_letras = total_enletras;


                        datos.Add(dt);
                    


                        //llama el tiempo de ahora, codigo de cliente y tipo de factura para nombrar los pdf's
                        string tiempo = DateTime.Now.ToString("ddmmss");
                        string fac = tiempo + "" + numero;
                        string fac_doc = numero + "_" + tiempo + "" + numero;






                        //generamos el pdf principal donde va el original, duplicado y triplicado que se imprimira
                        using (var viewer = new LocalReport())
                        {
                            viewer.DataSources.Clear();
                    
                            viewer.DataSources.Add(new ReportDataSource("DataSet1", datos));
                            viewer.DataSources.Add(new ReportDataSource("DataSet3", table));
                            viewer.SubreportProcessing += LocalReport_SubreportProcessing;


                            viewer.ReportEmbeddedResource = "TonyRomas.docFCFN.rdlc";

                            //-------------------------------PARAMETROS DEL REPORTE----------------------------------------------------
                            ReportParameter pieFactura = new ReportParameter("pieFactura", "ORIGINAL CLIENTE");
                            viewer.SetParameters(pieFactura);

                            ReportParameter nitE = new ReportParameter("nitEmpresa", nitEmpresa);
                            viewer.SetParameters(nitE);

                            ReportParameter nrcE = new ReportParameter("nrcEmpresa", nrcEmpresa);
                            viewer.SetParameters(nrcE);

                            ReportParameter nomLargo = new ReportParameter("nombreLargo", nombre_largo);
                            viewer.SetParameters(nomLargo);

                            ReportParameter dirEmp = new ReportParameter("direccionEmpresa", direccionEmp);
                            viewer.SetParameters(dirEmp);

                            ReportParameter leyendaPar = new ReportParameter("leyenda", leyendaFiscal);
                            viewer.SetParameters(leyendaPar);

                            ReportParameter numFPar = new ReportParameter("numeroFiscal", numero_fiscal);
                            viewer.SetParameters(numFPar);

                         

                            //-------------------------------------------------------------------------------------------------------------
                            QRCoder.QRCodeGenerator qRCodeGenerator = new QRCoder.QRCodeGenerator();
                            QRCoder.QRCodeData qRCodeData = qRCodeGenerator.CreateQrCode(chk + ";" + total + ";" + nombre_sucursal+ ";" + fecha, QRCoder.QRCodeGenerator.ECCLevel.Q);
                            QRCoder.QRCode qRCode = new QRCoder.QRCode(qRCodeData);
                            Bitmap bmp = qRCode.GetGraphic(9);

                            using (MemoryStream ms = new MemoryStream())
                            {
                                bmp.Save(ms, ImageFormat.Bmp);
                                DSPiramide dSPiramide = new DSPiramide();
                                DSPiramide.QRCodeRow qRCodeRow = dSPiramide.QRCode.NewQRCodeRow();
                                qRCodeRow.imagen = ms.ToArray();
                                dSPiramide.QRCode.AddQRCodeRow(qRCodeRow);

                                ReportDataSource report = new ReportDataSource();
                                report.Name = "DataSet2";
                                report.Value = dSPiramide.QRCode;
                             
                                viewer.DataSources.Add(report);
                                viewer.EnableExternalImages = true;
          
                            }


                            //  viewer.ReportPath = @"C:\Users\elara\Desktop\Farmacias la vida\reporte_Farmacias\reporte_Farmacias\farmaciaslavidaFactura.rdlc";
                            if (!(Directory.Exists(ruta_archivo_original)))
                            {
                                Directory.CreateDirectory(ruta_archivo_original);
                            }
                            File.WriteAllBytes(ruta_archivo_original + numero_fiscal + ".pdf", viewer.Render("PDF"));

                            // Cree una instancia de la impresora
                            IPrinter printer = new Printer();

                            // Imprime el archivo
                         /*     printer.PrintRawFile(impresora,ruta_archivo_original+numero_fiscal+".pdf",numero_fiscal+".pdf");
                            EscribirEnLog("Se guardo el PDF factura cliente en " + impresora,ruta_archivo_original + numero_fiscal + ".pdf");

                             File.Delete(ruta_archivo_original+numero_fiscal+".pdf");
                           EscribirEnLog("eliminando factura cliente de", ruta_archivo_original+ numero_fiscal + ".pdf");*/

                        }

                        //generamos una copia de la factura emisor

                        using (var viewer = new LocalReport())
                        {
                            viewer.DataSources.Clear();

                            viewer.DataSources.Add(new ReportDataSource("DataSet1", datos));
                            viewer.DataSources.Add(new ReportDataSource("DataSet3", table));
                            viewer.SubreportProcessing += LocalReport_SubreportProcessing;


                            viewer.ReportEmbeddedResource = "TonyRomas.docFCFN.rdlc";

                            //-------------------------------PARAMETROS DEL REPORTE----------------------------------------------------
                            ReportParameter pieFactura = new ReportParameter("pieFactura", "COPIA EMISOR");
                            viewer.SetParameters(pieFactura);

                            ReportParameter nitE = new ReportParameter("nitEmpresa", nitEmpresa);
                            viewer.SetParameters(nitE);

                            ReportParameter nrcE = new ReportParameter("nrcEmpresa", nrcEmpresa);
                            viewer.SetParameters(nrcE);

                            ReportParameter nomLargo = new ReportParameter("nombreLargo", nombre_largo);
                            viewer.SetParameters(nomLargo);

                            ReportParameter dirEmp = new ReportParameter("direccionEmpresa", direccionEmp);
                            viewer.SetParameters(dirEmp);

                            ReportParameter leyendaPar = new ReportParameter("leyenda", leyendaFiscal);
                            viewer.SetParameters(leyendaPar);

                            ReportParameter numFPar = new ReportParameter("numeroFiscal", numero_fiscal);
                            viewer.SetParameters(numFPar);

                          

                            //-------------------------------------------------------------------------------------------------------------
                            QRCoder.QRCodeGenerator qRCodeGenerator = new QRCoder.QRCodeGenerator();
                            QRCoder.QRCodeData qRCodeData = qRCodeGenerator.CreateQrCode(chk + ";" + total + ";" + nombre_sucursal + ";" + fecha, QRCoder.QRCodeGenerator.ECCLevel.Q);
                            QRCoder.QRCode qRCode = new QRCoder.QRCode(qRCodeData);
                            Bitmap bmp = qRCode.GetGraphic(9);

                            using (MemoryStream ms = new MemoryStream())
                            {
                                bmp.Save(ms, ImageFormat.Bmp);
                                DSPiramide dSPiramide = new DSPiramide();
                                DSPiramide.QRCodeRow qRCodeRow = dSPiramide.QRCode.NewQRCodeRow();
                                qRCodeRow.imagen = ms.ToArray();
                                dSPiramide.QRCode.AddQRCodeRow(qRCodeRow);

                                ReportDataSource report = new ReportDataSource();
                                report.Name = "DataSet2";
                                report.Value = dSPiramide.QRCode;

                                viewer.DataSources.Add(report);
                                viewer.EnableExternalImages = true;

                            }


                            //  viewer.ReportPath = @"C:\Users\elara\Desktop\Farmacias la vida\reporte_Farmacias\reporte_Farmacias\farmaciaslavidaFactura.rdlc";
                            if (!(Directory.Exists(guardar_emisores)))
                            {
                                Directory.CreateDirectory(guardar_emisores);
                            }
                            File.WriteAllBytes(guardar_emisores + numero_fiscal + ".pdf", viewer.Render("PDF"));

                            // Cree una instancia de la impresora
                            IPrinter printer = new Printer();

                            // Imprime el archivo
                            //   printer.PrintRawFile(impresora,ruta_archivo_pdf+fac+".pdf",fac+".pdf");
                            EscribirEnLog("Se guardo factura cliente PDF factura cliente en " + impresora, guardar_emisores + fac + ".pdf");


                          

                        }

                        //generamos una copia cliente de la factura
                        using (var viewer = new LocalReport())
                        {
                            viewer.DataSources.Clear();

                            viewer.DataSources.Add(new ReportDataSource("DataSet1", datos));
                            viewer.DataSources.Add(new ReportDataSource("DataSet3", table));
                            viewer.SubreportProcessing += LocalReport_SubreportProcessing;


                            viewer.ReportEmbeddedResource = "TonyRomas.docFCFN.rdlc";

                            //-------------------------------PARAMETROS DEL REPORTE----------------------------------------------------
                            ReportParameter pieFactura = new ReportParameter("pieFactura", "COPIA CLIENTE");
                            viewer.SetParameters(pieFactura);

                            ReportParameter nitE = new ReportParameter("nitEmpresa", nitEmpresa);
                            viewer.SetParameters(nitE);

                            ReportParameter nrcE = new ReportParameter("nrcEmpresa", nrcEmpresa);
                            viewer.SetParameters(nrcE);

                            ReportParameter nomLargo = new ReportParameter("nombreLargo", nombre_largo);
                            viewer.SetParameters(nomLargo);

                            ReportParameter dirEmp = new ReportParameter("direccionEmpresa", direccionEmp);
                            viewer.SetParameters(dirEmp);

                            ReportParameter leyendaPar = new ReportParameter("leyenda", leyendaFiscal);
                            viewer.SetParameters(leyendaPar);

                            ReportParameter numFPar = new ReportParameter("numeroFiscal", numero_fiscal);
                            viewer.SetParameters(numFPar);

                          
                            //-------------------------------------------------------------------------------------------------------------
                            QRCoder.QRCodeGenerator qRCodeGenerator = new QRCoder.QRCodeGenerator();
                            QRCoder.QRCodeData qRCodeData = qRCodeGenerator.CreateQrCode(chk + ";" + total + ";" + nombre_sucursal + ";" + fecha, QRCoder.QRCodeGenerator.ECCLevel.Q);
                            QRCoder.QRCode qRCode = new QRCoder.QRCode(qRCodeData);
                            Bitmap bmp = qRCode.GetGraphic(9);

                            using (MemoryStream ms = new MemoryStream())
                            {
                                bmp.Save(ms, ImageFormat.Bmp);
                                DSPiramide dSPiramide = new DSPiramide();
                                DSPiramide.QRCodeRow qRCodeRow = dSPiramide.QRCode.NewQRCodeRow();
                                qRCodeRow.imagen = ms.ToArray();
                                dSPiramide.QRCode.AddQRCodeRow(qRCodeRow);

                                ReportDataSource report = new ReportDataSource();
                                report.Name = "DataSet2";
                                report.Value = dSPiramide.QRCode;

                                viewer.DataSources.Add(report);
                                viewer.EnableExternalImages = true;

                            }


                            //  viewer.ReportPath = @"C:\Users\elara\Desktop\Farmacias la vida\reporte_Farmacias\reporte_Farmacias\farmaciaslavidaFactura.rdlc";
                            if (!(Directory.Exists(ruta_archivo_pdf_copia_cliente)))
                            {
                                Directory.CreateDirectory(ruta_archivo_pdf_copia_cliente);
                            }
                            File.WriteAllBytes(ruta_archivo_pdf_copia_cliente + numero_fiscal + ".pdf", viewer.Render("PDF"));

                            // Cree una instancia de la impresora
                            IPrinter printer = new Printer();

                            // Imprime el archivo
                            //   printer.PrintRawFile(impresora,ruta_archivo_pdf+fac+".pdf",fac+".pdf");
                            EscribirEnLog("Se guardo factura cliente PDF factura cliente en " + impresora, ruta_archivo_pdf_copia_cliente+ fac + ".pdf");


                            // Imprime el archivo
                           /*      printer.PrintRawFile(impresora,ruta_archivo_pdf_copia_cliente+numero_fiscal+".pdf",numero_fiscal+".pdf");
                               EscribirEnLog("Se guardo el PDF factura cliente en " + impresora, ruta_archivo_pdf_copia_cliente + numero_fiscal + ".pdf");

                                File.Delete(ruta_archivo_pdf_copia_cliente + numero_fiscal+".pdf");
                              EscribirEnLog("eliminando factura cliente de", ruta_archivo_pdf_copia_cliente + numero_fiscal + ".pdf");

                            */
                            datos.Clear();
                        }








                        con.conectar();
                        string cadena3 = "UPDATE Resoluciones SET actual =" + "'" + numero + "'" + " " + "where Id_Resolucion =" + "'" + vIdResolucion + "'";

                        con.insertar(cadena3);
                        con.sqlcon.Close();


                        Console.WriteLine("Linea de resolucion actualizada");

                        //Insert
                        con.sqlcon.Open();
                        string cadena4 = "INSERT INTO Log_Impresiones(numero,Numero_Fiscal,Fecha_Impresion,Tipo_documento,Id_resolucion,resolucion,sociedad,codigo_cliente)VALUES('" + numero + "','" + numero_fiscal + "','" + vFechaimpresion + "','" + vTipodoc + "','" + vIdResolucion + "','" + vResolucion + "','" + vEmpresa + "','" + 1 + "')";

                        con.insertar(cadena4);

                        con.sqlcon.Close();
                        con.sqlcon.Open();

                        int logCount = con.CountResolucion(vResolucion, vTipodoc);

                        if (logCount == vHasta)
                        {
                            string cadena5 = "UPDATE Resoluciones SET Activa =" + "'False'" + " " + "where Id_Resolucion =" + "'" + vIdResolucion + "'";
                            con.insertar(cadena5);

                            con.sqlcon.Close();
                        }

                        //terminar




                    }

                }
                else
                {
                    vLineaDot = "REIMPRESION";
                   EscribirEnLog(vLineaDot, "");

                }




            }

            //fin

        }





        /// esto es de la segunda data
        private void factura_cliente_dta2(string ruta, string impresora)
        {
            using (StreamReader readFile = new StreamReader(ruta, Encoding.Default))
            {

                string t_letra = "Restaurante:";
                String linea;
                int counter = 1;
                while ((linea = readFile.ReadLine()) != null)
                {
                    if (linea.ToLower().Contains(t_letra.ToLower()))
                    {
                        restaurante_xml = Regex.Replace(restaurante_xml, @"[^a-zA-Z]+", "");
                        restaurante_xml = linea.Replace("Restaurante: ", "").Replace("\0", "").Replace("$", "").Trim();


                        break;
                    }
                    counter++;
                }

                readFile.Close();
            }
            using (StreamReader readFile = new StreamReader(ruta, Encoding.Default))
            {

                string t_letra = "Caja:";
                String linea;
                int counter = 1;
                while ((linea = readFile.ReadLine()) != null)
                {
                    if (linea.ToLower().Contains(t_letra.ToLower()))
                    {
                        string result = string.Concat(linea.Where(c => Char.IsDigit(c)));
                        caja = result;

                        break;
                    }
                    counter++;
                }

                readFile.Close();
            }
            using (StreamReader readFile = new StreamReader(ruta, Encoding.Default))
            {

                string t_letra = "Mesa:";
                String linea;
                int counter = 1;
                while ((linea = readFile.ReadLine()) != null)
                {
                    if (linea.ToLower().Contains(t_letra.ToLower()))
                    {
                        string result = string.Concat(linea.Where(c => Char.IsDigit(c)));
                        mesa = result;

                        break;
                    }
                    counter++;
                }

                readFile.Close();
            }

            using (StreamReader readFile = new StreamReader(ruta, Encoding.Default))
            {

                string t_letra = "Chk:";
                String linea;
                int counter = 1;
                while ((linea = readFile.ReadLine()) != null)
                {
                    if (linea.ToLower().Contains(t_letra.ToLower()))
                    {
                        chk = linea.Substring(4, 9).Trim();
                        fecha = linea.Substring(10, 18).Trim();
                        break;
                    }
                    counter++;
                }

                readFile.Close();
            }

            using (StreamReader readFile = new StreamReader(ruta, Encoding.Default))
            {

                string t_letra = "Empleado:";
                String linea;
                int counter = 1;
                while ((linea = readFile.ReadLine()) != null)
                {
                    if (linea.ToLower().Contains(t_letra.ToLower()))
                    {
                        mesero = Regex.Replace(mesero, @"[^a-zA-Z]+", "");
                        mesero = linea.Replace("Empleado: ", "").Replace("\0", "").Replace("$", "").Trim();


                        break;
                    }
                    counter++;
                }

                readFile.Close();
            }
            using (StreamReader readFile = new StreamReader(ruta, Encoding.Default))
            {

                string t_letra = "PAGOS:";
                String linea;
                int counter = 1;
                while ((linea = readFile.ReadLine()) != null)
                {
                    if (linea.ToLower().Contains(t_letra.ToLower()))
                    {
                        pagos = Regex.Replace(pagos, @"[^a-zA-Z]+", "");
                        pagos = linea.Replace("PAGOS: ", "").Replace("\0", "").Replace("$", "").Trim();


                        break;
                    }
                    counter++;
                }

                readFile.Close();
            }
            using (StreamReader readFile = new StreamReader(ruta, Encoding.Default))
            {

                string t_letra = "NIT:";
                String linea;
                int counter = 1;
                while ((linea = readFile.ReadLine()) != null)
                {
                    if (linea.ToLower().Contains(t_letra.ToLower()))
                    {
                        nit_dui = Regex.Replace(nit_dui, @"[^a-zA-Z]+", "");
                        nit_dui = linea.Replace("NIT: ", "").Replace("\0", "").Replace("$", "").Trim();


                        break;
                    }
                    counter++;
                }

                readFile.Close();
            }

            using (StreamReader readFile = new StreamReader(ruta, Encoding.Default))
            {

                string t_letra = "CLIENTE:";
                String linea;
                int counter = 1;
                while ((linea = readFile.ReadLine()) != null)
                {
                    if (linea.ToLower().Contains(t_letra.ToLower()))
                    {
                        nombre_cli = Regex.Replace(nombre_cli, @"[^a-zA-Z]+", "");
                        nombre_cli = linea.Replace("CLIENTE: ", "").Replace("\0", "").Replace("$", "").Trim();


                        break;
                    }
                    counter++;
                }

                readFile.Close();
            }
            using (StreamReader readFile = new StreamReader(ruta, Encoding.Default))
            {

                string t_letra = "DIRECCION:";
                String linea;
                int counter = 1;
                while ((linea = readFile.ReadLine()) != null)
                {
                    if (linea.ToLower().Contains(t_letra.ToLower()))
                    {
                        direccion_cli = Regex.Replace(direccion_cli, @"[^a-zA-Z]+", "");
                        direccion_cli = linea.Replace("DIRECCION: ", "").Replace("\0", "").Replace("$", "").Trim();


                        break;
                    }
                    counter++;
                }

                readFile.Close();
            }
            using (StreamReader readFile = new StreamReader(ruta, Encoding.Default))
            {

                string t_letra = "SUBTOTAL GRAV.:";
                String linea;
                int counter = 1;
                while ((linea = readFile.ReadLine()) != null)
                {
                    if (linea.ToLower().Contains(t_letra.ToLower()))
                    {
                        subtotal_gravada = Regex.Replace(subtotal_gravada, @"[^a-zA-Z]+", "");
                        subtotal_gravada = linea.Replace("SUBTOTAL GRAV.: ", "").Replace("\0", "").Replace("$", "").Trim();


                        break;
                    }
                    counter++;
                }

                readFile.Close();
            }
            using (StreamReader readFile = new StreamReader(ruta, Encoding.Default))
            {

                string t_letra = "SUBTOTAL EXENTO:";
                String linea;
                int counter = 1;
                while ((linea = readFile.ReadLine()) != null)
                {
                    if (linea.ToLower().Contains(t_letra.ToLower()))
                    {
                        subtotal_exento = Regex.Replace(subtotal_exento, @"[^a-zA-Z]+", "");
                        subtotal_exento = linea.Replace("SUBTOTAL EXENTO:", "").Replace("\0", "").Replace("$", "").Trim();


                        break;
                    }
                    counter++;
                }

                readFile.Close();
            }
            using (StreamReader readFile = new StreamReader(ruta, Encoding.Default))
            {

                string t_letra = "RETENCION 1%:";
                String linea;
                int counter = 1;
                while ((linea = readFile.ReadLine()) != null)
                {
                    if (linea.ToLower().Contains(t_letra.ToLower()))
                    {
                        retencion_1 = Regex.Replace(retencion_1, @"[^a-zA-Z]+", "");
                        retencion_1 = linea.Replace("RETENCION 1%:", "").Replace("\0", "").Replace("$", "").Trim();


                        break;
                    }
                    counter++;
                }

                readFile.Close();
            }
            using (StreamReader readFile = new StreamReader(ruta, Encoding.Default))
            {

                string t_letra = "PROPINA:";
                String linea;
                int counter = 1;
                while ((linea = readFile.ReadLine()) != null)
                {
                    if (linea.ToLower().Contains(t_letra.ToLower()))
                    {
                        Propina = Regex.Replace(Propina, @"[^a-zA-Z]+", "");
                        Propina = linea.Replace("PROPINA:", "").Replace("\0", "").Replace("$", "").Trim();


                        break;
                    }
                    counter++;
                }

                readFile.Close();
            }
            using (StreamReader readFile = new StreamReader(ruta, Encoding.Default))
            {

                string t_letra = "TOTAL:";
                String linea;
                int counter = 1;
                while ((linea = readFile.ReadLine()) != null)
                {
                    if (linea.ToLower().Contains(t_letra.ToLower()))
                    {
                        total = Regex.Replace(total, @"[^a-zA-Z]+", "");
                        total = linea.Replace("TOTAL:", "").Replace("\0", "").Replace("$", "").Trim();


                        break;
                    }
                    counter++;
                }

                readFile.Close();
            }
            using (StreamReader readFile = new StreamReader(ruta, Encoding.Default))
            {

                string t_letra = "SON:";
                String linea;
                int counter = 1;
                while ((linea = readFile.ReadLine()) != null)
                {
                    if (linea.ToLower().Contains(t_letra.ToLower()))
                    {
                        // total_enletras = Regex.Replace(total_enletras, @"[^a-zA-Z]+", "");
                        total_enletras = linea.Replace("SON:", "").Replace("\0", "").Replace("$", "").Trim();


                        break;
                    }
                    counter++;
                }

                readFile.Close();
            }

            //capturar el detalle de la factura
            //capturar el detalle de la factura

            int counter_inicio = 1;



            using (StreamReader readFile = new StreamReader(ruta, Encoding.Default))
            {
                string monto_copa = "SON: ";
                String linea;
                while ((linea = readFile.ReadLine()) != null)
                {
                    if (linea.ToLower().Contains(monto_copa.ToLower()))
                    {
                        break;
                    }
                    counter_inicio++;

                }
                readFile.Close();
            }


            int counter_final = counter_inicio + 15;

            using (StreamReader readFile = new StreamReader(ruta, Encoding.Default))
            {
                string line;
                int iterador = 0;

                while ((line = readFile.ReadLine()) != null)
                {
                    for (int i = counter_inicio; i <= counter_final; i++)
                    {
                        if (iterador == i && line != "")
                        {
                            string[] commaSeparator = new string[] { "  " };
                            string[] result2;
                            result2 = line.Split(commaSeparator, StringSplitOptions.RemoveEmptyEntries);


                            Console.WriteLine(result2[0] + "--" + result2[1] + "--" + result2[2] + "--" + result2[3]);
                            detalle_factura_cliente.Add(new string[] { result2[0].Trim(), result2[1].Trim(), result2[2].Trim(), result2[3].Trim() });


                        }
                    }
                    iterador = iterador + 1;

                }
                readFile.Close();
            }

            //agregar detalle a datatable detalle_factura
            table = new DataTable("detalle_credito_fiscal");
            table.Columns.Add("cantidad", typeof(string));
            table.Columns.Add("descripcion", typeof(string));
            table.Columns.Add("precio", typeof(string));

 
            foreach (var i in detalle_factura_cliente)
            {
                table.Rows.Add(i[0].ToString(),i[1].ToString(), i[2].ToString());

            }



            string vTipodoc = "FCF";
            //inicio
            Conexion con = new Conexion();
            string vLectura = "Lec 0001";
            String vLineaDot = vLectura;
            //int vNumero = 0;
            int vDesde = 0;
            int vHasta = 0;
            int vIdResolucion = -1;
            int vActual = 0;
            int vCorrelativoValido = 0;
            string vDesdeString = "";
            string vHastaString = "";
            string vResolucion = "";
            string vSerie = "";
            string vFecha_Resolucion = "";
            String vNumeroString = "";
            string numero_fiscal = "";
            string vEmpresa = "Piramide";
            string vsucursal = "TRMETRO";
            string tirajefactura = "";



            string vFechaimpresion = DateTime.Now.ToString("yyyy-MM-dd");
            //string vLeyendaFiscal = "";
            string vActiva = "True";
            DateTime vFecha;


            try
            {
                con.conectar();
                string cadena = "SELECT TOP (1) Id_Resolucion,desde,hasta,actual,resolucion,serie,fecha_resolucion FROM Resoluciones Where Key_search ='" + vTipodoc + "' and empresa ='" + vEmpresa + "' and Activa ='" + vActiva + "' and SUCURSAL ='" + vsucursal + "' ORDER BY Id_Resolucion";

                con.ver(cadena);


                while (con.sqldr.Read())
                {
                    vIdResolucion = Convert.ToInt32(con.sqldr["Id_Resolucion"]);
                    vDesde = Convert.ToInt32(con.sqldr["desde"]);
                    vHasta = Convert.ToInt32(con.sqldr["hasta"]);
                    vActual = Convert.ToInt32(con.sqldr["actual"]);
                    vResolucion = Convert.ToString(con.sqldr["resolucion"]);
                    vSerie = Convert.ToString(con.sqldr["serie"]);
                    vFecha = Convert.ToDateTime(con.sqldr["Fecha_resolucion"]);
                    vFecha_Resolucion = vFecha.ToString("dd-MM-yyyy");
                    vDesdeString = Convert.ToString(vDesde);
                    vHastaString = Convert.ToString(vHasta);



                }
                con.sqldr.Close();
                con.sqlcon.Close();




            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);

            }

            if (vResolucion != "")
            {
                //Revisar que sea una Resolucion valida
                vActual = vActual + 1;
                numero = Convert.ToString(vActual);
                numero_fiscal = vSerie + " " +numero.PadLeft(7, '0');
                tirajefactura = "TIRAJE DEL " + vDesde + " HASTA " + vHasta + " RESOLUCION No.: " + vResolucion + " FECHA: " + vFecha_Resolucion;
                con.conectar();
                string cadena1 = "SELECT * FROM Log_Impresiones Where tipo_documento = '" + vTipodoc + "'and resolucion = '" + vResolucion + "' and numero_Fiscal = '" + numero_fiscal + "'";
                con.ver(cadena1);
                while (con.sqldr.Read())
                {
                    numero = "";
                }
                con.sqldr.Close();
                con.sqlcon.Close();
                //Fin de Recuperacion de Datos de Resolucion

                if (numero != "")
                {
                    //Console.WriteLine("Actualizando registros de la base de datos...");
                    vLineaDot = vLectura + " " + vDesdeString.PadLeft(7, '0') + " " + vHastaString.PadLeft(7, '0') + " " + vResolucion + " " + vSerie + " " + vNumeroString.PadLeft(7, '0') + " " + vFecha_Resolucion;




                    if (vActual > vHasta)
                    {
                        vCorrelativoValido = 0;
                        vLineaDot = "ERROR-01 Resolucion no Valida o Terminada";

                    }
                    else
                    {
                        vCorrelativoValido = 1;

                    }

                    if (vCorrelativoValido > 0)
                    {
                        //aqui van los reportes
                        vLineaDot = vLectura + " " + vDesdeString.PadLeft(7, '0') + " " + vHastaString.PadLeft(7, '0') + " " + vResolucion + " " + vSerie + " " + vNumeroString.PadLeft(7, '0') + " " + vFecha_Resolucion;
                        leyendaFiscal = vLineaDot;


                        datos dt = new datos();
                        List<datos> datos = new List<datos>();

                        //encabezado
                        dt.nombre_sucursal = nombre_sucursal;
                        dt.tipo_factura = tipo_factura;
                        dt.numero = numero;
                        dt.chk = chk;
                        dt.fecha = fecha;
                        dt.caja = caja;
                        dt.cajero = cajero;
                        dt.mesero = mesero;
                        dt.mesa = mesa;
                        dt.propina = Propina;
                        dt.subtotal_gravada = subtotal_gravada;
                        dt.subtotal_exento = subtotal_exento;
                        dt.subtotal_nosujeta = subtotal_nosujeta;
                        dt.subtotal_serv = subtotal_serv;
                        dt.total = total;
                        dt.pagos = pagos;
                        dt.credomatic = credomatic;
                        dt.nit_dui = nit_dui;
                        dt.nombre_cli = nombre_cli;
                        dt.para = para;
                        dt.cant_prod = cant_prod;
                        dt.promerica = promerica;
                        dt.efectivo = efectivo;
                        dt.subtotal_desc = subtotal_desc;
                        dt.cxc = cxc;
                        dt.t_letras = total_enletras;


                        datos.Add(dt);



                        //llama el tiempo de ahora, codigo de cliente y tipo de factura para nombrar los pdf's
                        string tiempo = DateTime.Now.ToString("ddmmss");
                        string fac = tiempo + "" + numero;
                        string fac_doc = numero + "_" + tiempo + "" + numero;






                        //generamos el pdf principal donde va el original, duplicado y triplicado que se imprimira
                        using (var viewer = new LocalReport())
                        {
                            viewer.DataSources.Clear();

                            viewer.DataSources.Add(new ReportDataSource("DataSet1", datos));
                            viewer.DataSources.Add(new ReportDataSource("DataSet3", table));
                            viewer.SubreportProcessing += LocalReport_SubreportProcessing;


                            viewer.ReportEmbeddedResource = "TonyRomas.docFCFN.rdlc";

                            //-------------------------------PARAMETROS DEL REPORTE----------------------------------------------------
                            ReportParameter pieFactura = new ReportParameter("pieFactura", "ORIGINAL CLIENTE");
                            viewer.SetParameters(pieFactura);

                            ReportParameter nitE = new ReportParameter("nitEmpresa", nitEmpresa);
                            viewer.SetParameters(nitE);

                            ReportParameter nrcE = new ReportParameter("nrcEmpresa", nrcEmpresa);
                            viewer.SetParameters(nrcE);

                            ReportParameter nomLargo = new ReportParameter("nombreLargo", nombre_largo);
                            viewer.SetParameters(nomLargo);

                            ReportParameter dirEmp = new ReportParameter("direccionEmpresa", direccionEmp);
                            viewer.SetParameters(dirEmp);

                            ReportParameter leyendaPar = new ReportParameter("leyenda", leyendaFiscal);
                            viewer.SetParameters(leyendaPar);

                            ReportParameter numFPar = new ReportParameter("numeroFiscal", numero_fiscal);
                            viewer.SetParameters(numFPar);



                            //-------------------------------------------------------------------------------------------------------------
                            QRCoder.QRCodeGenerator qRCodeGenerator = new QRCoder.QRCodeGenerator();
                            QRCoder.QRCodeData qRCodeData = qRCodeGenerator.CreateQrCode(chk + ";" + total + ";" + nombre_sucursal + ";" + fecha, QRCoder.QRCodeGenerator.ECCLevel.Q);
                            QRCoder.QRCode qRCode = new QRCoder.QRCode(qRCodeData);
                            Bitmap bmp = qRCode.GetGraphic(9);

                            using (MemoryStream ms = new MemoryStream())
                            {
                                bmp.Save(ms, ImageFormat.Bmp);
                                DSPiramide dSPiramide = new DSPiramide();
                                DSPiramide.QRCodeRow qRCodeRow = dSPiramide.QRCode.NewQRCodeRow();
                                qRCodeRow.imagen = ms.ToArray();
                                dSPiramide.QRCode.AddQRCodeRow(qRCodeRow);

                                ReportDataSource report = new ReportDataSource();
                                report.Name = "DataSet2";
                                report.Value = dSPiramide.QRCode;

                                viewer.DataSources.Add(report);
                                viewer.EnableExternalImages = true;

                            }


                            //  viewer.ReportPath = @"C:\Users\elara\Desktop\Farmacias la vida\reporte_Farmacias\reporte_Farmacias\farmaciaslavidaFactura.rdlc";
                            if (!(Directory.Exists(ruta_archivo_original)))
                            {
                                Directory.CreateDirectory(ruta_archivo_original);
                            }
                            File.WriteAllBytes(ruta_archivo_original + numero_fiscal + ".pdf", viewer.Render("PDF"));

                            // Cree una instancia de la impresora
                            IPrinter printer = new Printer();

                            // Imprime el archivo
                            //   printer.PrintRawFile(impresora,ruta_archivo_pdf+fac+".pdf",fac+".pdf");
                            EscribirEnLog("Se guardo el PDF factura cliente en " + impresora, ruta_archivo_original + fac + ".pdf");


                            /*  PdfDocument doc = new PdfDocument();
                              doc.LoadFromFile(ruta_archivo_pdf + fac + ".pdf");
                              //doc.ad8

                              doc.PrinterName = "RICOHPS";
                              doc.PrintDocument.DocumentName = fac + ".pdf";

                              //doc.PrintDocument.PrinterSettings.Duplex = Duplex.Simplex;
                              doc.PrintDocument.Print();*/

                            // File.Delete(ruta_archivo_pdf+fac+".pdf");
                            // EscribirEnLog("eliminando factura cliente de", ruta_archivo_pdf + fac + ".pdf");

                        }

                        //generamos una copia de la factura emisor

                        using (var viewer = new LocalReport())
                        {
                            viewer.DataSources.Clear();

                            viewer.DataSources.Add(new ReportDataSource("DataSet1", datos));
                            viewer.DataSources.Add(new ReportDataSource("DataSet3", table));
                            viewer.SubreportProcessing += LocalReport_SubreportProcessing;


                            viewer.ReportEmbeddedResource = "TonyRomas.docFCFN.rdlc";

                            //-------------------------------PARAMETROS DEL REPORTE----------------------------------------------------
                            ReportParameter pieFactura = new ReportParameter("pieFactura", "COPIA EMISOR");
                            viewer.SetParameters(pieFactura);

                            ReportParameter nitE = new ReportParameter("nitEmpresa", nitEmpresa);
                            viewer.SetParameters(nitE);

                            ReportParameter nrcE = new ReportParameter("nrcEmpresa", nrcEmpresa);
                            viewer.SetParameters(nrcE);

                            ReportParameter nomLargo = new ReportParameter("nombreLargo", nombre_largo);
                            viewer.SetParameters(nomLargo);

                            ReportParameter dirEmp = new ReportParameter("direccionEmpresa", direccionEmp);
                            viewer.SetParameters(dirEmp);

                            ReportParameter leyendaPar = new ReportParameter("leyenda", leyendaFiscal);
                            viewer.SetParameters(leyendaPar);

                            ReportParameter numFPar = new ReportParameter("numeroFiscal", numero_fiscal);
                            viewer.SetParameters(numFPar);



                            //-------------------------------------------------------------------------------------------------------------
                            QRCoder.QRCodeGenerator qRCodeGenerator = new QRCoder.QRCodeGenerator();
                            QRCoder.QRCodeData qRCodeData = qRCodeGenerator.CreateQrCode(chk + ";" + total + ";" + nombre_sucursal + ";" + fecha, QRCoder.QRCodeGenerator.ECCLevel.Q);
                            QRCoder.QRCode qRCode = new QRCoder.QRCode(qRCodeData);
                            Bitmap bmp = qRCode.GetGraphic(9);

                            using (MemoryStream ms = new MemoryStream())
                            {
                                bmp.Save(ms, ImageFormat.Bmp);
                                DSPiramide dSPiramide = new DSPiramide();
                                DSPiramide.QRCodeRow qRCodeRow = dSPiramide.QRCode.NewQRCodeRow();
                                qRCodeRow.imagen = ms.ToArray();
                                dSPiramide.QRCode.AddQRCodeRow(qRCodeRow);

                                ReportDataSource report = new ReportDataSource();
                                report.Name = "DataSet2";
                                report.Value = dSPiramide.QRCode;

                                viewer.DataSources.Add(report);
                                viewer.EnableExternalImages = true;

                            }


                            //  viewer.ReportPath = @"C:\Users\elara\Desktop\Farmacias la vida\reporte_Farmacias\reporte_Farmacias\farmaciaslavidaFactura.rdlc";
                            if (!(Directory.Exists(ruta_archivo_pdf_emisor)))
                            {
                                Directory.CreateDirectory(ruta_archivo_pdf_emisor);
                            }
                            File.WriteAllBytes(ruta_archivo_pdf_emisor + numero_fiscal + ".pdf", viewer.Render("PDF"));

                            // Cree una instancia de la impresora
                            IPrinter printer = new Printer();

                            // Imprime el archivo
                            //   printer.PrintRawFile(impresora,ruta_archivo_pdf+fac+".pdf",fac+".pdf");
                            EscribirEnLog("Se guardo factura cliente PDF factura cliente en " + impresora, ruta_archivo_pdf_emisor + fac + ".pdf");


                            /*  PdfDocument doc = new PdfDocument();
                              doc.LoadFromFile(ruta_archivo_pdf + fac + ".pdf");
                              //doc.ad8

                              doc.PrinterName = "RICOHPS";
                              doc.PrintDocument.DocumentName = fac + ".pdf";

                              //doc.PrintDocument.PrinterSettings.Duplex = Duplex.Simplex;
                              doc.PrintDocument.Print();*/

                            // File.Delete(ruta_archivo_pdf+fac+".pdf");
                            // EscribirEnLog("eliminando factura cliente de", ruta_archivo_pdf + fac + ".pdf");

                        }

                        //generamos una copia cliente de la factura
                        using (var viewer = new LocalReport())
                        {
                            viewer.DataSources.Clear();

                            viewer.DataSources.Add(new ReportDataSource("DataSet1", datos));
                            viewer.DataSources.Add(new ReportDataSource("DataSet3", table));
                            viewer.SubreportProcessing += LocalReport_SubreportProcessing;


                            viewer.ReportEmbeddedResource = "TonyRomas.docFCFN.rdlc";

                            //-------------------------------PARAMETROS DEL REPORTE----------------------------------------------------
                            ReportParameter pieFactura = new ReportParameter("pieFactura", "COPIA CLIENTE");
                            viewer.SetParameters(pieFactura);

                            ReportParameter nitE = new ReportParameter("nitEmpresa", nitEmpresa);
                            viewer.SetParameters(nitE);

                            ReportParameter nrcE = new ReportParameter("nrcEmpresa", nrcEmpresa);
                            viewer.SetParameters(nrcE);

                            ReportParameter nomLargo = new ReportParameter("nombreLargo", nombre_largo);
                            viewer.SetParameters(nomLargo);

                            ReportParameter dirEmp = new ReportParameter("direccionEmpresa", direccionEmp);
                            viewer.SetParameters(dirEmp);

                            ReportParameter leyendaPar = new ReportParameter("leyenda", leyendaFiscal);
                            viewer.SetParameters(leyendaPar);

                            ReportParameter numFPar = new ReportParameter("numeroFiscal", numero_fiscal);
                            viewer.SetParameters(numFPar);


                            //-------------------------------------------------------------------------------------------------------------
                            QRCoder.QRCodeGenerator qRCodeGenerator = new QRCoder.QRCodeGenerator();
                            QRCoder.QRCodeData qRCodeData = qRCodeGenerator.CreateQrCode(chk + ";" + total + ";" + nombre_sucursal + ";" + fecha, QRCoder.QRCodeGenerator.ECCLevel.Q);
                            QRCoder.QRCode qRCode = new QRCoder.QRCode(qRCodeData);
                            Bitmap bmp = qRCode.GetGraphic(9);

                            using (MemoryStream ms = new MemoryStream())
                            {
                                bmp.Save(ms, ImageFormat.Bmp);
                                DSPiramide dSPiramide = new DSPiramide();
                                DSPiramide.QRCodeRow qRCodeRow = dSPiramide.QRCode.NewQRCodeRow();
                                qRCodeRow.imagen = ms.ToArray();
                                dSPiramide.QRCode.AddQRCodeRow(qRCodeRow);

                                ReportDataSource report = new ReportDataSource();
                                report.Name = "DataSet2";
                                report.Value = dSPiramide.QRCode;

                                viewer.DataSources.Add(report);
                                viewer.EnableExternalImages = true;

                            }


                            //  viewer.ReportPath = @"C:\Users\elara\Desktop\Farmacias la vida\reporte_Farmacias\reporte_Farmacias\farmaciaslavidaFactura.rdlc";
                            if (!(Directory.Exists(ruta_archivo_pdf_copia_cliente)))
                            {
                                Directory.CreateDirectory(ruta_archivo_pdf_copia_cliente);
                            }
                            File.WriteAllBytes(ruta_archivo_pdf_copia_cliente + numero_fiscal + ".pdf", viewer.Render("PDF"));

                            // Cree una instancia de la impresora
                            IPrinter printer = new Printer();

                            // Imprime el archivo
                            //   printer.PrintRawFile(impresora,ruta_archivo_pdf+fac+".pdf",fac+".pdf");
                            EscribirEnLog("Se guardo factura cliente PDF factura cliente en " + impresora, ruta_archivo_pdf_copia_cliente + fac + ".pdf");


                            /*  PdfDocument doc = new PdfDocument();
                              doc.LoadFromFile(ruta_archivo_pdf + fac + ".pdf");
                              //doc.ad8

                              doc.PrinterName = "RICOHPS";
                              doc.PrintDocument.DocumentName = fac + ".pdf";

                              //doc.PrintDocument.PrinterSettings.Duplex = Duplex.Simplex;
                              doc.PrintDocument.Print();*/

                            // File.Delete(ruta_archivo_pdf+fac+".pdf");
                            // EscribirEnLog("eliminando factura cliente de", ruta_archivo_pdf + fac + ".pdf");

                        }








                        con.conectar();
                        string cadena3 = "UPDATE Resoluciones SET actual =" + "'" + numero + "'" + " " + "where Id_Resolucion =" + "'" + vIdResolucion + "'";

                        con.insertar(cadena3);
                        con.sqlcon.Close();


                        Console.WriteLine("Linea de resolucion actualizada");

                        //Insert
                        con.sqlcon.Open();
                        string cadena4 = "INSERT INTO Log_Impresiones(numero,Numero_Fiscal,Fecha_Impresion,Tipo_documento,Id_resolucion,resolucion,sociedad,codigo_cliente)VALUES('" + numero + "','" + numero_fiscal + "','" + vFechaimpresion + "','" + vTipodoc + "','" + vIdResolucion + "','" + vResolucion + "','" + vEmpresa + "','" + 1 + "')";

                        con.insertar(cadena4);

                        con.sqlcon.Close();
                        con.sqlcon.Open();

                        int logCount = con.CountResolucion(vResolucion, vTipodoc);

                        if (logCount == vHasta)
                        {
                            string cadena5 = "UPDATE Resoluciones SET Activa =" + "'False'" + " " + "where Id_Resolucion =" + "'" + vIdResolucion + "'";
                            con.insertar(cadena5);

                            con.sqlcon.Close();
                        }

                        //terminar




                    }











                }
                else
                {
                    vLineaDot = "REIMPRESION";
                    EscribirEnLog(vLineaDot, "");

                }




            }

            //fin









        }






        private void LocalReport_SubreportProcessing(object sender, SubreportProcessingEventArgs e)
        {

            if (table != null)
            {
                var tabla_cliente = from p in table.AsEnumerable()

                                    select new
                                    {
                                        cantidad = p.Field<string>("cantidad"),
                                        descripcion = p.Field<string>("descripcion"),
                                        precio = p.Field<string>("precio")

                                    };

                ReportDataSource rdsdetalle = new ReportDataSource("DataSet1", tabla_cliente);
                e.DataSources.Add(rdsdetalle);


            }
        }


        public void EscribirEnLog(String texto, string documento)
        {
            string path = pathExe + @"Log/" + DateTime.Now.ToString("yyyy") + @"/" + DateTime.Now.ToString("MM") + @"/";
            if (!(Directory.Exists(path)))
            {
                Directory.CreateDirectory(path);
            }
            if (!(File.Exists(path + @"Dia_" + DateTime.Now.ToString("dd") + ".txt")))
            {
                StreamWriter Log = new StreamWriter(path + @"Dia_" + DateTime.Now.ToString("dd") + ".txt");
                Log.WriteLine("-------------LOG FECHA " + DateTime.Now);
                Log.Close();
            }
            File.AppendAllText(path + @"Dia_" + DateTime.Now.ToString("dd") + ".txt", (DateTime.Now) + ": " + texto + " " + documento + " " + Environment.NewLine);
            listBox1.Items.Clear();
            string[] lines = File.ReadAllLines(path + @"Dia_" + DateTime.Now.ToString("dd") + ".txt");
            this.listBox1.Items.AddRange(lines);
            listBox1.SelectedIndex = listBox1.Items.Count - 1;
        }



    }

    
}
