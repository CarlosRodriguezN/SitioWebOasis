using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using SitioWebOasis.CommonControls;

namespace SitioWebOasis.CommonClasses.UI
{
	/// <summary>
	/// Clase base para la mayor�a de p�ginas de este Sitio Web, permite
	/// que la p�gina pertenezca a un subsitio espec�fico
	/// </summary>
	public class OASisPage : System.Web.UI.Page
	{
		protected C_Header ucCabeceraGeneral;
		protected C_Menu ucMenu;
			
		/// <summary>
		/// Retorna el SubSitio de la cabecera
		/// determinando qu� t�tulo y v�nculos (tabs) aparecer�n en la misma
		/// </summary>
		protected string SubSite
		{
			get { return this.ucCabeceraGeneral.SubSite; }
			//set { this.ucCabeceraGeneral.SubSite = value; }
		}
        

		protected override void OnInit(EventArgs e)
		{
			//this.VerificarExistenciaDeUsuario();
			base.OnInit(e);
		}

		protected virtual void CargarLinksEnMenu()
		{
			// ser� sobreecrito en clases derivadas para cargar items en los menus
		}


		protected override void OnPreRender(EventArgs e)
		{
			base.OnPreRender(e);
			/* los links del men� deben ser cargados despu�s de que se hayan procesado
			 los eventos de los controles de la p�gina porque el men� puede cambiar
			 din�micamente (por ejemplo al cambiar de carrera) */
			this.CargarLinksEnMenu();
		} 
	}
}