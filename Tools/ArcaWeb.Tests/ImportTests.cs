// Fecha: 05/10/2026 - TAREA: 00064 - Linea: 3 - Casos que impiden conciliar un lote incompleto o alterar importes originales.
using ArcaCliente.Models;
using ArcaCliente.Web.Data;
using ArcaCliente.Web.Models;
using ArcaCliente.Web.Services;
using ClosedXML.Excel;
using System.IO.Compression;
using System.Text;
static class ImportTests
{
    public static void Run(Action<string, Action> test)
    {
        var format = new FileFormat { Date="Fecha", Type="Tipo", Number="Documento", Amount="Importe", NumberMode=NumberMode.Separated, NumberSeparator="-" };
        var config = new ProviderConfiguration { Cuit="30123456781", Format=format, Types=new() { ["FA"]="001", ["NCA"]="003", ["REC"]="EXCLUIR" } };
        UploadedDocument Csv(string rows) => new("proveedor.csv", Encoding.UTF8.GetBytes("Tipo;Fecha;Documento;Importe\n"+rows));
        void Check(bool x) { if(!x) throw new Exception("Fallo de validación"); }
        test("CSV citado y exclusión sin campos fiscales", () => {
            var p=ProviderImporter.Preview(Csv("FA;05/09/2026;00005-00001234;\"1.000,00\"\nREC;;;\nNCA;05/09/2026;00005-00000081;(100,00)"),config);
            Check(p.Ready && p.Included.Count==2 && p.ExcludedCount==1 && p.Included[1].Total==100.00m && p.Rows[2].RawAmount=="(100,00)");
        });
        test("Fila inválida después de la muestra bloquea todo", () => {
            var p=ProviderImporter.Preview(Csv(string.Join('\n',Enumerable.Repeat("FA;05/09/2026;5-1234;100,00",201))+"\nFA;31/02/2026;5-9999;100,00"),config);
            Check(!p.Ready && p.Rows.Last().Errors.Count>0);
        });
        test("Tipo desconocido no se excluye automáticamente", () => {
            var p=ProviderImporter.Preview(Csv("PAGO;;;"),config); Check(!p.Ready && p.UnknownTypes.Contains("PAGO") && p.ExcludedCount==0);
        });
        test("Importe inválido y agrupación inválida no se convierten en cero", () => {
            Check(!ProviderImporter.Preview(Csv("FA;05/09/2026;5-1234;abc"),config).Ready);
            Check(!ProviderImporter.Preview(Csv("FA;05/09/2026;5-1234;1.00,00"),config).Ready);
        });
        test("CUIT normaliza y deduplica; lista inválida no se acepta", () => {
            var r=CuitListImporter.Parse(new("cuits.csv",Encoding.UTF8.GetBytes("CUIT\n30-12345678-1\n30123456781")));
            Check(r.Count==1 && r[0]=="30123456781");
            try { CuitListImporter.Parse(new("cuits.txt",Encoding.UTF8.GetBytes("30123456781\nno"))); throw new Exception("Aceptó lista inválida"); } catch(ImportException) {}
        });
        test("XLSX conserva filas y celdas numéricas", () => {
            using var book=new XLWorkbook(); var ws=book.AddWorksheet("Pendientes");
            ws.Cell(1,1).Value="Tipo";ws.Cell(1,2).Value="Fecha";ws.Cell(1,3).Value="Documento";ws.Cell(1,4).Value="Importe";
            ws.Cell(2,1).Value="FA";ws.Cell(2,2).Value=new DateTime(2026,9,5);ws.Cell(2,3).Value="5-1234";ws.Cell(2,4).Value=1000.5;
            using var stream=new MemoryStream();book.SaveAs(stream);
            var p=ProviderImporter.Preview(new("ejemplo.xlsx",stream.ToArray()),config);Check(p.Ready && p.Included.Single().Total==1000.50m);
        });
        // Fecha: 29/10/2026 - TAREA: 00142 - Linea: 1 - Excel exporta cachés de tablas dinámicas que ClosedXML no puede leer; la hoja de datos debe leerse igual.
        byte[] ExcelConPivote() {
            using var book=new XLWorkbook(); var ws=book.AddWorksheet("ECC");
            ws.Cell(1,1).Value="Tipo";ws.Cell(1,2).Value="Fecha";ws.Cell(1,3).Value="Documento";ws.Cell(1,4).Value="Importe";
            ws.Cell(2,1).Value="FA";ws.Cell(2,2).Value=new DateTime(2026,9,5);ws.Cell(2,3).Value="5-1234";ws.Cell(2,4).Value=1000.5;
            using var plain=new MemoryStream();book.SaveAs(plain);
            using var result=new MemoryStream();
            using(var src=new ZipArchive(new MemoryStream(plain.ToArray()),ZipArchiveMode.Read))
            using(var dst=new ZipArchive(result,ZipArchiveMode.Create,true)) {
                foreach(var e in src.Entries) {
                    using var r=new StreamReader(e.Open()); var text=r.ReadToEnd();
                    if(e.FullName=="xl/workbook.xml") text=text.Replace("</workbook>","<pivotCaches><pivotCache cacheId=\"1\" r:id=\"rIdP1\"/></pivotCaches></workbook>");
                    if(e.FullName=="xl/_rels/workbook.xml.rels") text=text.Replace("</Relationships>","<Relationship Id=\"rIdP1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/pivotCacheDefinition\" Target=\"pivotCache/pivotCacheDefinition1.xml\"/></Relationships>");
                    if(e.FullName=="[Content_Types].xml") text=text.Replace("</Types>","<Override PartName=\"/xl/pivotCache/pivotCacheDefinition1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.pivotCacheDefinition+xml\"/><Override PartName=\"/xl/pivotCache/pivotCacheRecords1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.pivotCacheRecords+xml\"/></Types>");
                    using var w=new StreamWriter(dst.CreateEntry(e.FullName).Open()); w.Write(text);
                }
                const string ns="xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\"";
                // El caché declara dos campos y cada registro trae uno: la misma discrepancia que rompe el archivo real.
                void Add(string name,string xml){using var w=new StreamWriter(dst.CreateEntry(name).Open());w.Write(xml);}
                Add("xl/pivotCache/pivotCacheDefinition1.xml",$"<pivotCacheDefinition {ns} r:id=\"rId1\" recordCount=\"1\"><cacheSource type=\"worksheet\"><worksheetSource ref=\"A1:D2\" sheet=\"ECC\"/></cacheSource><cacheFields count=\"2\"><cacheField name=\"Tipo\" numFmtId=\"0\"><sharedItems/></cacheField><cacheField name=\"Extra\" numFmtId=\"0\"><sharedItems/></cacheField></cacheFields></pivotCacheDefinition>");
                Add("xl/pivotCache/_rels/pivotCacheDefinition1.xml.rels","<Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"><Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/pivotCacheRecords\" Target=\"pivotCacheRecords1.xml\"/></Relationships>");
                Add("xl/pivotCache/pivotCacheRecords1.xml",$"<pivotCacheRecords {ns} count=\"1\"><r><n v=\"1\"/></r></pivotCacheRecords>");
            }
            return result.ToArray();
        }
        test("XLSX con tabla dinámica se lee igual (ClosedXML solo falla)", () => {
            var bytes=ExcelConPivote(); bool falla=false;
            try { using var b=new XLWorkbook(new MemoryStream(bytes)); } catch(Exception) { falla=true; }
            Check(falla);
            var p=ProviderImporter.Preview(new("ECC.xlsx",bytes),new ProviderConfiguration{Cuit="30123456781",Format=new FileFormat{Date="Fecha",Type="Tipo",Number="Documento",Amount="Importe",Sheet="ECC",NumberMode=NumberMode.Separated},Types=new(){["FA"]="001"}});
            Check(p.Ready && p.Included.Single().Total==1000.50m);
        });
        test("Letra opcional se une al tipo para homologar cada combinación", () => {
            var f=new FileFormat{Date="Fecha",Type="Tipo",Letter="Letra",Number="Documento",Amount="Importe",NumberMode=NumberMode.Separated};
            var doc=new UploadedDocument("p.csv",Encoding.UTF8.GetBytes("Tipo;Letra;Fecha;Documento;Importe\nFC;A;05/09/2026;5-1;10,00\nFC;B;05/09/2026;5-2;20,00"));
            var p=ProviderImporter.Preview(doc,new ProviderConfiguration{Cuit="30123456781",Format=f,Types=new(){["FC A"]="001",["FC B"]="006"}});
            Check(p.Ready && p.Included[0].TipoComprobante=="001" && p.Included[1].TipoComprobante=="006");
            f.Letter="Inexistente"; Check(!ProviderImporter.Preview(doc,new ProviderConfiguration{Cuit="30123456781",Format=f}).Ready);
        });
        // Fecha: 30/10/2026 - TAREA: 00150 - Linea: 2 - Comprobante en un solo campo con la letra, en las combinaciones de orden y separador.
        ImportPreview Combined(string value,NumberMode mode,NumberOrder order,string? pointColumn=null) {
            var f=new FileFormat{Date="Fecha",Type="Tipo",Number="Documento",Amount="Importe",NumberMode=mode,NumberOrder=order,NumberSeparator="-",PointLength=4,Point=pointColumn??"PuntoVenta"};
            var doc=new UploadedDocument("c.csv",Encoding.UTF8.GetBytes("Tipo;Fecha;Documento;Importe;PuntoVenta\nFC;05/09/2026;"+value+";10,00;0219"));
            return ProviderImporter.Preview(doc,new ProviderConfiguration{Cuit="30123456781",Format=f,Types=new(){["FC A"]="001"}});
        }
        test("Letra dentro del comprobante: todas las combinaciones", () => {
            foreach(var (value,mode,order) in new[]{("0219A00060055",NumberMode.Fixed,NumberOrder.PointLetterNumber),("0219-A-00060055",NumberMode.Separated,NumberOrder.PointLetterNumber),
                ("A021900060055",NumberMode.Fixed,NumberOrder.LetterPointNumber),("A-0219-00060055",NumberMode.Separated,NumberOrder.LetterPointNumber),
                ("00060055A",NumberMode.Fixed,NumberOrder.NumberLetter),("00060055-A",NumberMode.Separated,NumberOrder.NumberLetter),("0219a00060055",NumberMode.Fixed,NumberOrder.PointLetterNumber)}) {
                var p=Combined(value,mode,order);
                Check(p.Ready && p.Rows[0].RawType=="FC A" && p.Included[0].PuntoVenta=="00219" && p.Included[0].Numero=="00060055" && p.Included[0].TipoComprobante=="001");
            }
        });
        test("Comprobante sin letra sigue igual y letra inválida bloquea", () => {
            var f=new FileFormat{Date="Fecha",Type="Tipo",Number="Documento",Amount="Importe",NumberMode=NumberMode.Fixed,PointLength=4};
            var doc=new UploadedDocument("c.csv",Encoding.UTF8.GetBytes("Tipo;Fecha;Documento;Importe\nFA;05/09/2026;021900060055;10,00"));
            Check(ProviderImporter.Preview(doc,new ProviderConfiguration{Cuit="30123456781",Format=f,Types=new(){["FA"]="001"}}).Ready);
            Check(!Combined("02191000060055",NumberMode.Fixed,NumberOrder.PointLetterNumber).Ready);
            Check(!Combined("0219-A",NumberMode.Separated,NumberOrder.PointLetterNumber).Ready);
        });
        // Fecha: 30/10/2026 - TAREA: 00150 - Linea: 1 - Listado SAP: cabecera repetida, subtotales, pago sin comprobante e importes con el signo al final.
        byte[] Sap() {
            string C(int row,int col,string text,string attrs="")=>$"<td><font><nobr {attrs}id=l{row:0000}{col:000}>{text.Replace(" ","&nbsp;")}</nobr></font></td>";
            string Header(int row)=>"<tr>"+C(row,3,"")+C(row,9,"Ref.fact.","title=\"Referencia a factura\" ")+C(row,20,"Cl")+C(row,23,"Referencia")+C(row,44,"Fecha doc.")+C(row,55,"Importe en ML")+C(row,73,"Fecha pago")+"</tr>";
            string Data(int row,string cl,string reference,string date,string amount)=>"<tr>"+C(row,5,"7000"+row)+C(row,20,cl)+C(row,23,reference)+C(row,44,date)+C(row,55,amount)+C(row,73,"13.10.2026")+"</tr>";
            return Encoding.UTF8.GetBytes("<html><body><table>"+C(2,2,"Deudor")+Header(6)+Data(8,"DG","0219A00060055","18.09.2026","31 255,75-")+Data(9,"DI","0219A00298530","16.09.2026","675 335,76 ")
                +"<tr>"+C(10,5,"*")+C(10,55,"706 591,51 ")+"</tr>"+Header(12)+Data(13,"DZ","C000001900075655","20.10.2026","204 259,12-")+"</table></body></html>");
        }
        test("Listado SAP se lee como tabla y se homologa por clase y letra", () => {
            var t=SapListReader.Read(new("lista.htm",Sap()));
            Check(t.Rows.Count==3 && t.Headers.Contains("Referencia") && t.Headers.Contains("Importe en ML"));
            var f=new FileFormat(); SapListReader.Apply(new("lista.htm",Sap()),f);
            var p=ProviderImporter.Preview(new("lista.htm",Sap()),new ProviderConfiguration{Cuit="30123456781",Format=f,Types=new(){["DI A"]="001",["DG A"]="003",["DZ"]="EXCLUIR"}});
            Check(p.Ready && p.Included.Count==2 && p.ExcludedCount==1);
            Check(p.Included[0].TipoComprobante=="003" && p.Included[0].Total==31255.75m && p.Included[0].PuntoVenta=="00219" && p.Included[0].Numero=="00060055" && p.Included[0].Fecha==new DateTime(2026,9,18));
            Check(FiscalCatalog.Suggest("DI A")=="001" && FiscalCatalog.Suggest("DG A")=="003" && FiscalCatalog.Suggest("DZ")=="EXCLUIR");
        });
        test("HTML que no es un listado SAP se rechaza con un mensaje claro", () => {
            try { SapListReader.Read(new("otro.htm",Encoding.UTF8.GetBytes("<html><body>nada</body></html>")));throw new Exception("Aceptó HTML ajeno"); } catch(ImportException) {}
        });
        // Fecha: 30/10/2026 - TAREA: 00151 - Linea: 1 - Sin columna de tipo, el importe negativo es nota de crédito y el resto factura.
        test("Sin columna de tipo: negativos son notas de crédito", () => {
            var f=new FileFormat{Date="Fecha",Type="",Number="Documento",Amount="Importe",NumberMode=NumberMode.Fixed,NumberOrder=NumberOrder.PointLetterNumber,PointLength=4};
            var doc=new UploadedDocument("s.csv",Encoding.UTF8.GetBytes("Fecha;Documento;Importe\n05/09/2026;0219A00000001;100,00\n05/09/2026;0219A00000002;-50,00\n05/09/2026;0219B00000003;(25,00)\n05/09/2026;0219A00000004;-10,00"));
            var cfg=new ProviderConfiguration{Cuit="30123456781",Format=f,Types=new(){["FC A"]="001",["NC A"]="003",["NC B"]="008"}};
            var p=ProviderImporter.Preview(doc,cfg);
            Check(p.Ready && p.Included.Select(x=>x.TipoComprobante).SequenceEqual(new[]{"001","003","008","003"}) && p.Included[1].Total==50.00m);
            Check(p.Rows.Select(r=>r.RawType).SequenceEqual(new[]{"FC A","NC A","NC B","NC A"}));
            f.Type="Tipo"; Check(!ProviderImporter.Preview(doc,cfg).Ready);
        });
        // Fecha: 16/11/2026 - TAREA: 00192 - Linea: 1 - Máscara del comprobante: p punto de venta, l letra, n número, el resto literal.
        ImportPreview Masked(string mask,string value,string? point=null) {
            var f=new FileFormat{Date="Fecha",Type="Tipo",Number="Documento",Amount="Importe",NumberMode=NumberMode.Mask,NumberMask=mask,Point="PuntoVenta"};
            var doc=new UploadedDocument("m.csv",Encoding.UTF8.GetBytes("Tipo;Fecha;Documento;Importe;PuntoVenta\nFC;05/09/2026;"+value+";10,00;"+(point??"")));
            return ProviderImporter.Preview(doc,new ProviderConfiguration{Cuit="30123456781",Format=f,Types=new(){["FC A"]="001"}});
        }
        test("Máscara: combinaciones válidas", () => {
            foreach(var (mask,value,point) in new[]{("pppplnnnnnnnn","0219A00060055",""),("pppp-l-nnnnnnnn","0219-A-00060055",""),("l-pppp-nnnnnnnn","A-0219-00060055",""),("PPPP L NNNNNNNN","0219 a 00060055",""),("nnnnnnnn/l","00060055/A","0219")}) {
                var p=Masked(mask,value,point);
                Check(p.Ready && p.Rows[0].RawType=="FC A" && p.Included[0].PuntoVenta=="00219" && p.Included[0].Numero=="00060055");
            }
        });
        test("Máscara: valores que no la respetan quedan con error", () => {
            Check(!Masked("pppp-l-nnnnnnnn","0219A00060055").Ready);
            Check(!Masked("pppplnnnnnnnn","0219A0006005").Ready);
            Check(!Masked("pppplnnnnnnnn","0219500060055").Ready);
            Check(!Masked("pppplnnnnnnnn","02X9A00060055").Ready);
            Check(Masked("pppp-l-nnnnnnnn","0219-A-00060055").Ready);
        });
        test("Máscara: se valida al guardar el formato", () => {
            foreach(var bad in new[]{"","pppp","nnnnnnnnn","ppppppnn","pplln"}) {
                try { ConfigurationValidator.Provider(new ProviderConfiguration{Cuit="30123456781",Format=new FileFormat{NumberMode=NumberMode.Mask,NumberMask=bad}});throw new Exception("Aceptó la máscara «"+bad+"»"); } catch(ImportException) {}
            }
            ConfigurationValidator.Provider(new ProviderConfiguration{Cuit="30123456781",Format=new FileFormat{NumberMode=NumberMode.Mask,NumberMask="pppp-l-nnnnnnnn"}});
        });
        // Fecha: 16/11/2026 - TAREA: 00192 - Linea: 3 - Color de directiva: sólo «#RRGGBB», en el perfil, en el Excel y sin cambiar nada cuando no hay color.
        test("Color de directiva: validación y normalización", () => {
            Check(DirectiveColors.Valid("#12ab9f")=="#12AB9F" && DirectiveColors.Valid("red")==null && DirectiveColors.Valid("#12345")==null && DirectiveColors.Valid("#12345g")==null && DirectiveColors.Valid("url(x)")==null && DirectiveColors.Valid(null)==null);
            var perfil=new WebProfile(); perfil.Profile.DirectivasConciliacion.Add(DirectivaConciliacion.CrearUltima());
            perfil.Profile.DirectivasConciliacion[1].Color="#12ab9f"; ConfigurationValidator.Profile(perfil); Check(perfil.Profile.DirectivasConciliacion[1].Color=="#12AB9F");
            perfil.Profile.DirectivasConciliacion[1].Color=""; ConfigurationValidator.Profile(perfil); Check(perfil.Profile.DirectivasConciliacion[1].Color==null);
            perfil.Profile.DirectivasConciliacion[1].Color="expression(alert(1))";
            try { ConfigurationValidator.Profile(perfil);throw new Exception("Aceptó un color inválido"); } catch(ImportException) {}
        });
        test("Excel: las filas de una directiva con color salen de ese color", () => {
            var directivas=new List<DirectivaConciliacion>{DirectivaConciliacion.CrearPrimaria(),DirectivaConciliacion.CrearUltima()}; directivas[1].Color="#FFCC00";
            var items=new List<ItemConciliacion>{new(){Estado=EstadoConciliacion.Conciliado,DirectivaAplicada=2},new(){Estado=EstadoConciliacion.Conciliado,DirectivaAplicada=1},new(){Estado=EstadoConciliacion.SoloSistema,DirectivaAplicada=0}};
            foreach(var fuente in new[]{"Proveedor","ARCA"}) {
                var bytes=ResultExporter.Export(new ReconciliationResult(items,new(),0,0,fuente),directivas);
                using var book=new XLWorkbook(new MemoryStream(bytes)); var hoja=book.Worksheet(1);
                Check(hoja.Cell(3,1).Style.Fill.BackgroundColor.Color.ToArgb()==System.Drawing.Color.FromArgb(255,204,0).ToArgb());
                Check(hoja.Cell(3,14).Style.Fill.BackgroundColor.Color.ToArgb()==System.Drawing.Color.FromArgb(255,204,0).ToArgb());
                Check(hoja.Cell(4,1).Style.Fill.BackgroundColor.Color.ToArgb()!=System.Drawing.Color.FromArgb(255,204,0).ToArgb());
                Check(hoja.Cell(5,1).Style.Fill.BackgroundColor.Color.ToArgb()!=System.Drawing.Color.FromArgb(255,204,0).ToArgb());
            }
            var sinColor=ResultExporter.Export(new ReconciliationResult(items,new(),0,0,"Proveedor"));
            using var libro=new XLWorkbook(new MemoryStream(sinColor));
            Check(libro.Worksheet(1).Cell(3,1).Style.Fill.BackgroundColor.Color.ToArgb()==libro.Worksheet(1).Cell(4,1).Style.Fill.BackgroundColor.Color.ToArgb());
        });
        // Fecha: 16/11/2026 - TAREA: 00192 - Linea: 4 - Filtro por directiva en la pantalla de resultados.
        test("Resultados: filtro por directiva sólo aplica a conciliados y con diferencia", () => {
            var items=new List<ItemConciliacion>{
                new(){Estado=EstadoConciliacion.Conciliado,DirectivaAplicada=1,Numero="1"},new(){Estado=EstadoConciliacion.Conciliado,DirectivaAplicada=2,Numero="2"},new(){Estado=EstadoConciliacion.Conciliado,DirectivaAplicada=2,Numero="3"},
                new(){Estado=EstadoConciliacion.DiferenciaImporte,DirectivaAplicada=2,Numero="4"},new(){Estado=EstadoConciliacion.SoloSistema,DirectivaAplicada=0,Numero="5"}};
            var d2=new HashSet<int>{2}; var ninguna=new HashSet<int>();
            Check(ResultFilter.Apply(items,"Conciliado","",d2).Select(x=>x.Numero).SequenceEqual(new[]{"2","3"}));
            Check(ResultFilter.Apply(items,"Conciliado","",ninguna).Count()==3);
            Check(ResultFilter.Apply(items,"Conciliado","",new HashSet<int>{1,2}).Count()==3);
            Check(ResultFilter.Apply(items,"SoloSistema","",d2).Count()==1);
            Check(ResultFilter.Apply(items,"","3",ninguna).Single().Numero=="3");
            var cuenta=ResultFilter.CountByDirective(items,"Conciliado"); Check(cuenta[1]==1 && cuenta[2]==2 && cuenta.Count==2);
            Check(ResultFilter.HasDirectives("Conciliado") && ResultFilter.HasDirectives("DiferenciaImporte") && !ResultFilter.HasDirectives("SoloSistema") && !ResultFilter.HasDirectives(""));
        });
        test("HTML con celdas sin cierre no cuelga la lectura", () => {
            var bomba=Encoding.UTF8.GetBytes("<html>"+string.Concat(Enumerable.Repeat("<nobr id=l0001001>x",150000)));
            var reloj=System.Diagnostics.Stopwatch.StartNew();
            try { SapListReader.Read(new("bomba.htm",bomba));throw new Exception("Aceptó HTML dañado"); } catch(ImportException) {}
            Check(reloj.Elapsed<TimeSpan.FromSeconds(20));
        });
        test("Listado SAP real del proveedor (si está disponible)", () => {
            var path=@"D:\DESARROLLOS CONTABLE\CONCILIACION FICHA PROVEEDORES\Lista part.indiv.deudores.htm";
            if(!File.Exists(path)) return;
            var doc=new UploadedDocument("Lista part.indiv.deudores.htm",File.ReadAllBytes(path)); var f=new FileFormat(); SapListReader.Apply(doc,f);
            var p=ProviderImporter.Preview(doc,new ProviderConfiguration{Cuit="30123456781",Format=f,Types=new(){["DI A"]="001",["DG A"]="003",["DZ"]="EXCLUIR"}});
            Check(p.Rows.Count==92 && p.Ready && p.Included.Count==91 && p.ExcludedCount==1);
        });
        test("Cabeceras duplicadas y CSV roto se bloquean", () => {
            try { TabularReader.Read(new("m.csv",Encoding.UTF8.GetBytes("Tipo;Tipo\nFA;NC")),format);throw new Exception("Aceptó duplicadas"); } catch(ImportException) {}
            try { TabularReader.Read(Csv("\"FA;sin cierre"),format);throw new Exception("Aceptó CSV roto"); } catch(ImportException) {}
        });
    }
}
