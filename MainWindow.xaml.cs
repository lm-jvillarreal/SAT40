using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using XSDToXML.Utils;

namespace SAT40
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

        }
        static string mainPath = @"C:\Users\jvillarreal\source\repos\SAT40\";
        static string pathXML = mainPath+ @"XMLCFDI.xml";
        private void btnXML_Click(object sender, RoutedEventArgs e)
        {
            AppContext.SetSwitch("Switch.System.Xml.AllowDefaultResolver", true);
            //Obtener numero certificado
            string pathCer = mainPath + @"CSD\EKU9003173C9.cer";
            string pathKey = mainPath + @"CSD\EKU9003173C9.key";
            string clavePrivada = "12345678a";
            //obtenemos el numero
            string numeroCertificado, aa, b, c;
            SelloDigital.leerCER(pathCer, out aa, out b, out c, out numeroCertificado);

            //Datos de CFDI
            Comprobante oComprobante = new Comprobante();
            oComprobante.Version = "4.0";
            oComprobante.Serie = "";
            oComprobante.Folio = "1";
            oComprobante.Fecha = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ss");
            oComprobante.FormaPago = "03";
            oComprobante.NoCertificado = numeroCertificado;
            oComprobante.Certificado = "";
            oComprobante.CondicionesDePago = "";
            oComprobante.SubTotal = 10.00M;
            oComprobante.Descuento = 1M;
            oComprobante.Moneda = "MXN";
            oComprobante.TipoCambio = 0;
            oComprobante.Total = 11.44m;
            oComprobante.TipoDeComprobante = "I";
            oComprobante.MetodoPago = "PUE";
            oComprobante.LugarExpedicion = "67700";

            //Datos del emisor
            ComprobanteEmisor oEmisor = new ComprobanteEmisor();
            oEmisor.Rfc = "VIAJ910829C71";
            oEmisor.Nombre = "Josué Itonio Villarreal Alvarado";
            oEmisor.RegimenFiscal = "605";

            //Datos del receptor
            ComprobanteReceptor oReceptor = new ComprobanteReceptor();
            oReceptor.Rfc = "VIAJ910829C71";
            oReceptor.Nombre = "Josué Itonio Villarreal Alvarado";
            oReceptor.DomicilioFiscalReceptor = "67700";
            oReceptor.RegimenFiscalReceptor = "605";
            oReceptor.UsoCFDI = "G03";

            //asigno emisor y receptor
            oComprobante.Emisor = oEmisor;
            oComprobante.Receptor = oReceptor;

            List<ComprobanteConcepto> lstConceptos = new List<ComprobanteConcepto>();
            ComprobanteConcepto oConcepto = new ComprobanteConcepto();
            oConcepto.Importe = 10m;
            oConcepto.ClaveProdServ = "92111704";
            oConcepto.Cantidad = 1;
            oConcepto.ClaveUnidad = "C81";
            oConcepto.Descripcion = "Un misil para la guerra";
            oConcepto.ValorUnitario = 10m;
            oConcepto.Descuento = 1;

            //Impuesto trasladado
            List<ComprobanteConceptoImpuestosTraslado> lstImpuestosTrasladados = new List<ComprobanteConceptoImpuestosTraslado>();
            ComprobanteConceptoImpuestosTraslado oImpuestosTraslado = new ComprobanteConceptoImpuestosTraslado();
            oImpuestosTraslado.Base = 9.00m;
            oImpuestosTraslado.TasaOCuota = 0.160000m;
            oImpuestosTraslado.TipoFactor = "Tasa";
            oImpuestosTraslado.Impuesto = "002";
            oImpuestosTraslado.Importe = 1.44m;
            lstImpuestosTrasladados.Add(oImpuestosTraslado);

            oConcepto.Impuestos = new ComprobanteConceptoImpuestos();
            oConcepto.Impuestos.Traslados = lstImpuestosTrasladados.ToArray();

            lstConceptos.Add(oConcepto);

            oComprobante.Conceptos = lstConceptos.ToArray();

            //Nodo Impuesto
            List<ComprobanteImpuestosTraslado> lstImpuestosTRASLADOS = new List<ComprobanteImpuestosTraslado>();
            ComprobanteImpuestos oIMPUESTOS = new ComprobanteImpuestos();
            ComprobanteImpuestosTraslado oIT = new ComprobanteImpuestosTraslado();
            oIMPUESTOS.TotalImpuestosTrasladados = 1.44m;
            oIT.Impuesto = "002";
            oIT.TipoFactor = "Tasa";
            oIT.TasaOCuota = 0.160000m;
            oIT.Importe = 1.44m;
            oIT.Base = 9.00m;

            lstImpuestosTRASLADOS.Add(oIT);
            oIMPUESTOS.Traslados = lstImpuestosTRASLADOS.ToArray();
            oComprobante.Impuestos = oIMPUESTOS;

            //Creamos el XML
            createXML(oComprobante);
            string cadenaOriginal = "";
            string pathXSLT = mainPath + @"cadenaoriginal_4_0.xslt";
            System.Xml.Xsl.XslCompiledTransform transformador = new System.Xml.Xsl.XslCompiledTransform(true);
            transformador.Load(pathXSLT);
            using(StringWriter sw = new StringWriter())
            using (XmlWriter XWD = XmlWriter.Create(sw,transformador.OutputSettings))
            {
                transformador.Transform(pathXML, XWD);
                cadenaOriginal = sw.ToString();
            }

            SelloDigital oSelloDigital = new SelloDigital();
            oComprobante.Certificado = oSelloDigital.Certificado(pathCer);
            oComprobante.Sello = oSelloDigital.Sellar(cadenaOriginal, pathKey, clavePrivada);
            createXML(oComprobante);
        }

        private static void createXML(Comprobante oComprobante)
        {
            //SERIALIZAMOS.-------------------------------------------------

            XmlSerializerNamespaces xmlNameSpace = new XmlSerializerNamespaces();

            xmlNameSpace.Add("cfdi", "http://www.sat.gob.mx/cfd/4");
            xmlNameSpace.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");


            XmlSerializer oXmlSerializar = new XmlSerializer(typeof(Comprobante));

            string sXml = "";

            using (var sww = new Utils.StringWriterWithEncoding(Encoding.UTF8))
            {
                using (XmlWriter writter = XmlWriter.Create(sww))
                {

                    oXmlSerializar.Serialize(writter, oComprobante,xmlNameSpace);
                    sXml = sww.ToString();
                }

            }

            //guardamos el string en un archivo
            System.IO.File.WriteAllText(pathXML, sXml);

        }

        private void btnAddenda_Click(object sender, RoutedEventArgs e)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(pathXML);

            XmlElement addenda = doc.CreateElement("cfdi:Addenda","http://www.sat.gob.mx/cfd/4");
            addenda.SetAttribute("xmlns:svam", "https://sistema.yofacturo.mx/esquema");
            addenda.SetAttribute("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
            addenda.SetAttribute("xsi:schemaLocation","https://sistema.yofacturo.mx/esquema https://sistema.yofacturo.mx/esquema/svamCfdiAddenda.xsd");

            XmlElement infoAdicional = doc.CreateElement("Usuario");
            XmlElement texto = doc.CreateElement("Texto");
            infoAdicional.SetAttribute("idusuario", "chuy1");
            texto.SetAttribute("Fechas", "Ventas del 27-03-2022 al 27-03-2022");

            addenda.AppendChild(infoAdicional);
            addenda.AppendChild(texto);
            doc.DocumentElement.AppendChild(addenda);
            doc.Save(pathXML);//Guardar XML
        }
    }
}
