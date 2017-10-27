﻿using GestorErrores;
using SitioWebOasis.CommonClasses.GestionUsuarios;
using SitioWebOasis.Library;
using System;
using System.Collections.Generic;
using System.Data;

namespace SitioWebOasis.Models
{
    public class DatosAsignaturasDocenteModel: DatosCarrera
    {
        private WSGestorDeReportesMatriculacion.dtstCursosDocente _dtstCursosDocente = new WSGestorDeReportesMatriculacion.dtstCursosDocente();

        public DatosAsignaturasDocenteModel( string strCodCarrera )
        {
            if (!string.IsNullOrEmpty(strCodCarrera)){
                this.UsuarioActual.SetRolCarreraActual( Roles.Docentes,
                                                        strCodCarrera);
            }

            this._dtstPeriodoVigente = this._dataPeriodoAcademicoVigente();
            this._dtstCursosDocente = this._dsAsignaturasDocente();
        }
        
        
        //  Lista de asignaturas por carrera
        private WSGestorDeReportesMatriculacion.dtstCursosDocente _dsAsignaturasDocente()
        {
            WSGestorDeReportesMatriculacion.dtstCursosDocente dsCursosDocente = new WSGestorDeReportesMatriculacion.dtstCursosDocente();
            WSGestorDeReportesMatriculacion.dtstCursosDocente rstCursoDocente = new WSGestorDeReportesMatriculacion.dtstCursosDocente();

            try
            {
                ProxySeguro.GestorDeReportesMatriculacion rm = new ProxySeguro.GestorDeReportesMatriculacion();
                rm.CookieContainer = new System.Net.CookieContainer();
                rm.SetCodCarrera(this.UsuarioActual.CarreraActual.Codigo);

                rstCursoDocente = rm.GetCursosDocente(  this._dtstPeriodoVigente.Periodos[0]["strCodigo"].ToString(),
                                                        this.UsuarioActual.Cedula.ToString());

                if(rstCursoDocente != null){
                    dsCursosDocente = rstCursoDocente;
                }
            }
            catch(Exception ex)
            {
                Errores err = new Errores();
                err.SetError(ex, "_getAsignaturasDocente");
            }

            return dsCursosDocente;
        }


        public string getNombreDocente()
        {
            string nombreDocente = (this.UsuarioActual != null) 
                                        ? this.UsuarioActual.Nombre
                                        : string.Empty;

            return nombreDocente;
        }


        public string getFacultadCarreraDocente()
        {
            string facultadCarreraDocente = string.Empty;
            try
            {
                if (this.UsuarioActual != null)
                {
                    string facultad = (this.UsuarioActual.FacultadActual != null)
                                            ? this.UsuarioActual.FacultadActual.Nombre.ToString() + " / "
                                            : "";

                    string carrera = (this.UsuarioActual.CarreraActual != null)
                                        ? this.UsuarioActual.CarreraActual.Nombre.ToString()
                                        : "";

                    facultadCarreraDocente = facultad + carrera;
                }
            }
            catch (Exception ex) {
                facultadCarreraDocente = string.Empty;
                Errores err = new Errores();
                err.SetError(ex, "getFacultadCarreraDocente");
            }
            

            return facultadCarreraDocente;
        }


        public string getHTMLAsignaturasDocente()
        {
            string rst = string.Empty;
            string nivel = string.Empty;
            string color = "odd";
            string evActiva = string.Empty;
            string parcialActivo = string.Empty;
            string cantidadEstudiantesMatriculados = string.Empty;

            rst += " <tr role='row' class='" + color + "'>";
            rst += "     <td style='align-content: center; vertical-align: middle; text-align: center;' colspan='9'>" + Language.es_ES.EST_LBL_SIN_REGISTROS + "</td>";
            rst += " </tr>";

            if (this._dtstCursosDocente.Cursos.Count > 0){
                int x = 0;
                int posicion = 0;
                EvaluacionActiva objEvActiva = new EvaluacionActiva();
                evActiva = objEvActiva.getDataEvaluacionActiva().Replace("FN", "");

                if( !string.IsNullOrEmpty(evActiva))
                {
                    rst = string.Empty;
                    parcialActivo = (evActiva != "FNP" && evActiva != "ER" && evActiva != "NA")  
                                        ? this._getNumOrdinal(evActiva)
                                        : (evActiva == "FNP")   ? Language.es_ES.DOC_TB_EV_FINAL
                                                                : Language.es_ES.DOC_TB_EV_RECUPERACION;

                    foreach (DataRow item in this._dtstCursosDocente.Cursos){
                        posicion = ++x;
                        color = (color == "odd") ? "even" : "odd";
                        nivel = this._getNumOrdinal(item["strCodNivel"].ToString());
                        cantidadEstudiantesMatriculados = this._cantidadEstudiantesMatriculados(this._dtstPeriodoVigente.Periodos[0]["strCodigo"].ToString(), 
                                                                                                item["strCodMateria"].ToString(), 
                                                                                                item["strCodNivel"].ToString(), 
                                                                                                item["strCodParalelo"].ToString());

                        rst += "<tr id=" + item["strCodMateria"].ToString() + "_" + item["strCodNivel"].ToString() + "_" + item["strCodParalelo"].ToString() + " role='row' class='" + color + "'>";
                        rst += "    <td style='align-content: center; vertical-align: middle; text-align: center;'>" + posicion + "</td>";
                        rst += "    <td style='align-content: center; vertical-align: middle; text-align: left;'><a href='/Docentes/EvaluacionAsignatura/" + item["strCodNivel"].ToString() + "/" + item["strCodMateria"].ToString() + "/" + item["strCodParalelo"].ToString() + "'>" + item["strNombreMateria"].ToString() + "</a></td>";
                        rst += "	<td style='align-content: center; vertical-align: middle; text-align: center;'>" + nivel + "</td>";
                        rst += "	<td style='align-content: center; vertical-align: middle; text-align: center;'>" + item["strCodParalelo"].ToString() + "</td>";
                        rst += "	<td style='align-content: center; vertical-align: middle; text-align: center;'>" + parcialActivo + "</td>";
                        rst += "	<td style='align-content: center; vertical-align: middle; text-align: center;'>" + cantidadEstudiantesMatriculados + "</td>";
                        rst += "	<td style='align-content: center; vertical-align: middle; text-align: center;'> <div class='btn-group btn-group-xs'><button type='button' class='btn btn-danger'>PDF</button><button type='button' class='btn btn-success'>EXCEL</button></div> </td>";
                        rst += "	<td style='align-content: center; vertical-align: middle; text-align: center;'>";
                        rst += "	    <span id='mini-bar-chart"+ posicion + "' class='mini-bar-chart'><canvas width='53' height='25' style='display: inline-block; vertical-align: top; width: 53px; height: 25px;'></canvas></span>";
                        rst += "    </td>";
                        rst += "</tr>";
                    }
                }
            }

            return rst;
        }


        private string _cantidadEstudiantesMatriculados(string periodoVigente, string strCodAsignatura, string strCodNivel, string strCodParalelo)
        {
            string dtaEstMatriculados = string.Empty;
            Int16 tem = default(Int16);
            Int16 temd = default(Int16);
            Int16 temp = default(Int16);

            try {
                ProxySeguro.DatosUsuario du = new ProxySeguro.DatosUsuario();
                DataSet dsEstMatriculados = du.GetNumEstudiantesMatriculadosMateria(this.UsuarioActual.CarreraActual.Codigo.ToString(), 
                                                                                    periodoVigente, 
                                                                                    strCodAsignatura, 
                                                                                    strCodNivel, 
                                                                                    strCodParalelo);

                if( dsEstMatriculados.Tables[0].Rows.Count > 0){
                    tem = Convert.ToInt16(dsEstMatriculados.Tables[0].Compute("SUM(cantidad)", "").ToString());
                    temd = Convert.ToInt16(dsEstMatriculados.Tables[0].Compute("SUM(cantidad)", "strCodEstado = 'DEF'").ToString());
                    string rstEmp = dsEstMatriculados.Tables[0].Compute("SUM(cantidad)", "strCodEstado <> 'DEF'").ToString();
                    temp = (string.IsNullOrEmpty(rstEmp))   ? default(Int16) 
                                                            : Convert.ToInt16(rstEmp.ToString());
                }

                dtaEstMatriculados = (tem == temd)
                                        ? tem.ToString()
                                        : temd.ToString() + " / " + tem;
            } catch(Exception ex) {
                dtaEstMatriculados = string.Empty;
                Errores err = new Errores();
                err.SetError(ex, "_cantidadEstudiantesMatriculados");
            }

            return dtaEstMatriculados;
        }


        
        public List<System.Web.Mvc.SelectListItem> getLstAsignaturasDocente(string strCodAsignatura = "")
        {
            List<System.Web.Mvc.SelectListItem> lstAsignaturasDocente = new List<System.Web.Mvc.SelectListItem>();
            System.Web.Mvc.SelectListItem asignatura = new System.Web.Mvc.SelectListItem();

            if (this._dtstCursosDocente != null && this._dtstCursosDocente.Cursos.Count > 0)
            {
                foreach (DataRow item in this._dtstCursosDocente.Cursos)
                {
                    asignatura = new System.Web.Mvc.SelectListItem();
                    asignatura.Value = item["strCodMateria"].ToString() + "|" + item["strCodNivel"].ToString() + "|" + item["strCodParalelo"].ToString();
                    asignatura.Text = item["strNombreMateria"].ToString();

                    if (strCodAsignatura == item["strCodMateria"].ToString())
                    {
                        asignatura.Selected = true;
                    }

                    lstAsignaturasDocente.Add(asignatura);
                }
            }

            return lstAsignaturasDocente;
        }

    }
}