// Fecha: 27/10/2026 - TAREA: 00136 - Linea: 1 - Lista de CUITs excluidos de la conciliación PyR, compartida por escritorio y web.
// Vive en ArcaCliente.Core/Web para que la vean los dos hosts; las carpetas Models/Services de este proyecto son vínculos de solo lectura.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcaCliente.Core.Web.Models
{
    /// <summary>
    /// Lista de Cuits para excluir en la importarción de documentos en Percepciones y Retenciones
    /// </summary>
    public class CUITSExcluidos
    {
        public CUITSExcluidos() 
        {
            CUITS = new() { "30500001735","30500003193","30500006613","30500008454","30500010084","30500010912","30500011072","30545788167","30571421352","30574816870","30604731018","30685376349","30692432661","30703088534" };
        }
        public List<string> CUITS { get; set; }
    }
}
