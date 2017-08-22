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

using SitioWebOasis.CommonClasses.GestionUsuarios;

namespace SitioWebOasis.CommonClasses.UI
{
	/// <summary>
	/// Clase base para una p�gina en la cual se desea tener acceso
	/// al objeto Usuario almacenado en sesi�n a trav�s de la porpiedad
	/// UsuarioActual
	/// </summary>
	public class OASisUsuarioPage : OASisPage
	{
    protected override void OnInit(EventArgs e)
		{
			this.VerificarExistenciaDeUsuario();
			base.OnInit(e);
		}

		/// <summary>
		/// Retorna una instancia de la clase usuario que representa
		/// al usuario actual (autenticado o no) que est� accediendo
		/// a la aplicaci�n Web
		/// </summary>
		protected virtual Usuario UsuarioActual
		{
			get	{ return this.Session["UsuarioActual"] as Usuario; }
		}

		private void VerificarExistenciaDeUsuario()
		{
			if (this.UsuarioActual == null)
				throw new Exception("No se tiene acceso al objeto Usuario");
		}

	}
}