﻿using GestorErrores;
using Microsoft.Reporting.WebForms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OAS_Seguridad.Cliente;
using SitioWebOasis.CommonClasses.GestionUsuarios;
using SitioWebOasis.Library;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net;

namespace SitioWebOasis.Models
{
    public class EvaluacionAcumulativaModel: EvaluacionesDocenteModel
    {
        public string jsonEvAcumulativa { get; set; }
        private string _dtaEvAcumulativa = string.Empty;
        private WSGestorEvaluacion.dtstEvaluacion_Acumulados _dsEvAcumulativa = new WSGestorEvaluacion.dtstEvaluacion_Acumulados();
        

        public EvaluacionAcumulativaModel( string strCodNivel, string strCodAsignatura, string strCodParalelo )
        {
            this._strCodAsignatura = strCodAsignatura;
            this._strCodNivel = strCodNivel;
            this._strCodParalelo = strCodParalelo;
            this._cargarInformacionCarrera();
            this._dsEvAcumulativa = this._CargarNotasEvAcumulativa();

            this.jsonEvAcumulativa = (this._dsEvAcumulativa.Acta.Rows.Count > 0)
                                        ? JsonConvert.SerializeObject(this._dsEvAcumulativa.Acta)
                                        : "";
        }
        

        private WSGestorEvaluacion.dtstEvaluacion_Acumulados _CargarNotasEvAcumulativa()
        {
            WSGestorEvaluacion.dtstEvaluacion_Acumulados rstEvAcumulativa = new WSGestorEvaluacion.dtstEvaluacion_Acumulados();
            WSGestorEvaluacion.dtstEvaluacion_Acumulados dsEvAcumulativa = new WSGestorEvaluacion.dtstEvaluacion_Acumulados();

            try
            {
                ProxySeguro.GestorEvaluacion ge = new ProxySeguro.GestorEvaluacion();
                ge.CookieContainer = new CookieContainer();
                ge.set_fBaseDatos(this._strNombreBD);
                ge.set_fUbicacion(this._strUbicacion);

                rstEvAcumulativa = ge.crearActaArtificialEvaluaciones(  this._dtstPeriodoVigente.Periodos[0]["strCodigo"].ToString(),
                                                                        this._strCodAsignatura,
                                                                        this._strCodNivel,
                                                                        this._strCodParalelo);

                dsEvAcumulativa = (rstEvAcumulativa != null) 
                                    ? rstEvAcumulativa
                                    : new WSGestorEvaluacion.dtstEvaluacion_Acumulados();
            }catch (System.Exception ex){
                Errores err = new Errores();
                err.SetError(ex, "_getAsignaturasDocente");
            }

            return dsEvAcumulativa;
        }
        
        
        public string getHTML_EvaluacionAcumulativa()
        {
            string colorRow = "even";
            string html = string.Empty;
            
            html += " <tr role='row' class='" + colorRow + "'>";
            html += "     <td style='align-content: center; vertical-align: middle; text-align: center;' colspan='9'>" + Language.es_ES.EST_LBL_SIN_REGISTROS + "</td>";
            html += " </tr>";

            if (this._dsEvAcumulativa.Acta.Rows.Count > 0)
            {
                html = string.Empty;
                string numMatricula = string.Empty;
                string promedio = string.Empty;
                string numNivel = string.Empty;
                string estadoCumplimiento = string.Empty;

                foreach (DataRow item in this._dsEvAcumulativa.Acta)
                {
                    colorRow = (colorRow == "odd") ? "even" : "odd";

                    numMatricula = this._getNumOrdinal(item["bytNumMat"].ToString(), "matricula");
                    numNivel = this._getNumOrdinal(item["strCodNivel"].ToString(), "nivel");
                    estadoCumplimiento = (this.strParcialActivo == "3" || this.strParcialActivo == "P") 
                                            ? this._getEstadoCumplimiento(item["Total"].ToString(), item["bytAsistencia"].ToString()) 
                                            : "---" ;

                    html += " <tr id='" + item["strCodigo"] + "' role='row' class='" + colorRow + "'>";
                    html += "     <td style='width: 30px; align-content: center; vertical-align: middle; text-align: center; font-size: 12px;'>" + item["No"] + "</td>";
                    html += "     <td style='width: 300px; align-content: center; vertical-align: middle; text-align: left; font-size: 12px;'>" + item["NombreEstudiante"].ToString().Trim() + "</td>";
                    html += "     <td style='width: 30px;align-content: center; vertical-align: middle; text-align: center; font-size: 12px;'>" + numMatricula + "</td>";
                    html += "     <td style='width: 50px;align-content: center; vertical-align: middle; text-align: center; font-size: 12px;'>" + item["bytNota1"] + "</td>";
                    html += "     <td style='width: 50px;align-content: center; vertical-align: middle; text-align: center; font-size: 12px;'>" + item["bytNota2"] + "</td>";
                    html += "     <td style='width: 50px;align-content: center; vertical-align: middle; text-align: center; font-size: 12px;'>" + item["bytNota3"] + "</td>";
                    html += "     <td style='width: 50px;align-content: center; vertical-align: middle; text-align: center; font-size: 12px;'>" + item["Total"] + "</td>";
                    html += "     <td style='width: 40px;align-content: center; vertical-align: middle; text-align: center; font-size: 12px;'>" + item["bytAsistencia"] + "</td>";
                    html += "     <td style='width: 50px;align-content: center; vertical-align: middle; text-align: center; font-size: 12px;'>" + estadoCumplimiento + "</td>";
                    html += "     <td style='width: 100px;align-content: center; vertical-align: middle; text-align: center;font-size: 12px;'>" + item["strObservaciones"] + "</td>";
                    html += " </tr>";
                }
            }

            return html;
        }


        public bool registrarEvaluacionAcumulativa(List<EvaluacionAcumulativa> dtaEvAcumulativa )
        {
            bool rst = false;

            try{
                //  Recorro DataTable Acta - registro por registro actualizando la informacion
                if( this._updEvAcumulativa(dtaEvAcumulativa)){
                    rst = this._guardarEvAcumulativa();
                    this._dsEvAcumulativa = this._CargarNotasEvAcumulativa();

                    this.jsonEvAcumulativa = (this._dsEvAcumulativa.Acta.Rows.Count > 0)
                                                ? JsonConvert.SerializeObject(this._dsEvAcumulativa.Acta)
                                                : "";
                }
            }
            catch (System.Exception ex)
            {
                Errores err = new Errores();
                err.SetError(ex, "registrarEvaluacionAcumulativa");
            }

            return rst;
        }


        private bool _updEvAcumulativa(List<EvaluacionAcumulativa> dtaEvAcumulativa)
        {
            bool rst = false;
            byte nota = default(byte);

            try
            {
                int numReg = dtaEvAcumulativa.Count;
                if( numReg > 0 && this._dsEvAcumulativa.Acta.Rows.Count > 0){
                    for(int x = 0; x < numReg; x++){
                        if ( this._dsEvAcumulativa.Acta.Rows[0]["sintCodMatricula"].ToString() == dtaEvAcumulativa[0].sintCodMatricula.ToString()){
                            this._dsEvAcumulativa.Acta.Rows[x]["bytAsistencia"] = Convert.ToByte( dtaEvAcumulativa[x].bytAsistencia.ToString() );

                            switch (this.strParcialActivo){
                                case "1": nota = Convert.ToByte(dtaEvAcumulativa[x].bytNota1.ToString()); break;
                                case "2": nota = Convert.ToByte(dtaEvAcumulativa[x].bytNota2.ToString()); break;
                                case "3": nota = Convert.ToByte(dtaEvAcumulativa[x].bytNota3.ToString()); break;
                            }

                            this._dsEvAcumulativa.Acta.Rows[x]["bytNota" + this.strParcialActivo] = nota;
                        }
                    }

                    this._dsEvAcumulativa.AcceptChanges();
                    rst = true;
                }
            }
            catch(Exception ex) {
                Errores err = new Errores();
                err.SetError(ex, "_updEvEstudiantes");
            }

            return rst;
        }


        private bool _guardarEvAcumulativa()
        {
            bool rst = false;
            bool regEvAcumulado = false;
            bool regFchRegistro = false;

            try
            {
                ProxySeguro.GestorEvaluacion ge = new ProxySeguro.GestorEvaluacion();
                ge.CookieContainer = new CookieContainer();
                ge.set_fUbicacion(this._strUbicacion);
                ge.set_fBaseDatos(this._strNombreBD);

                //  Registro de acta
                regEvAcumulado = ge.setActaEvaluaciones(this._dtstPeriodoVigente.Periodos[0]["strCodigo"].ToString(),
                                                        this._strCodAsignatura.ToString(),
                                                        this._strCodNivel.ToString(),
                                                        this._strCodParalelo.ToString(),
                                                        this._dsEvAcumulativa,
                                                        this.UsuarioActual.Nombre.ToString() );

                //  Registro de fecha de ingreso de evaluaciones
                regFchRegistro = ge.ActualizarRegistroFechaIngresoEvaluaciones( this._dtstPeriodoVigente.Periodos[0]["strCodigo"].ToString(),
                                                                                this._strCodAsignatura.ToString(),
                                                                                this._strCodNivel.ToString(),
                                                                                this._strCodParalelo.ToString(),
                                                                                DateTime.Now);

                rst = (regEvAcumulado && regFchRegistro)? true 
                                                        : false;
            }catch (System.Exception ex){
                Errores err = new Errores();
                err.SetError(ex, "_guardarEvAcumulativa");
            }

            return rst;
        }


        private string _getEstadoCumplimiento(string totalAcumulado, string porcientoAsistencia)
        {
            string estCumplimiento = "---";
            int ta = Convert.ToInt16(totalAcumulado);
            int pa = Convert.ToInt16(porcientoAsistencia);

            try
            {
                //  EXONERADO
                if( ta >= 25 && pa >= 70 ){
                    estCumplimiento = "<span class='label label-success'>"+ Language.es_ES.LBL_CUMPLIMIENTO_EXONERADO +"</span>";
                }

                //  EVALUACION FINAL
                if (ta >= 12 && ta < 25 && pa >= 70){
                    estCumplimiento = "<span class='label label-default'>"+ Language.es_ES.LBL_CUMPLIMIENTO_EV_FINAL +"</span>"; ;
                }

                //  REPROBADO
                if (ta < 12 && pa >= 70){
                    estCumplimiento = "<span class='label label-danger'>" + Language.es_ES.LBL_CUMPLIMIENTO_REPROBADO + "</span>"; ;
                }

                //  REPROBADO - FALTAS
                if (pa < 70){
                    estCumplimiento = "<span class='label label-danger'>" + Language.es_ES.LBL_CUMPLIMIENTO_REPROBADO_FALTAS + "</span>"; ;
                }
            }catch(Exception ex){
                estCumplimiento = "---";
                Errores err = new Errores();
                err.SetError(ex, "_guardarEvAcumulativa");
            }

            return estCumplimiento;
        }


        
        public LocalReport getRptEvAcumulativa(string reportPath, string impParcial = "1")
        {
            LocalReport rptEvAcumulativa = new LocalReport();

            try{
                ReportDataSource rds = new ReportDataSource();
                rds.Name = "dtsActasAcumuladas";
                rds.Value = this._dsEvAcumulativa.Acta;

                rptEvAcumulativa.DataSources.Clear();
                rptEvAcumulativa.DataSources.Add(rds);
                rptEvAcumulativa.ReportPath = reportPath;

                rptEvAcumulativa.SetParameters(this._getParametrosGeneralesReporte());
                rptEvAcumulativa.Refresh();
            }
            catch (Exception ex)
            {
                Errores err = new Errores();
                err.SetError(ex, "getReporteHorarios");
            }

            return rptEvAcumulativa;
        }

        private IEnumerable<ReportParameter> _getParametrosGeneralesReporte()
        {
            WSInfoCarreras.ParametrosCarrera pc = this._getParametrosCarrera();
            List<ReportParameter> lstPrmRptEvAcumulativa = new List<ReportParameter>();

            string lblFacultad = "FACULTAD:";
            string lblCarrera = "CARRERA:";
            string lblEscuela = "ESCUELA:";

            string facultad = default(string);
            string carrera = default(string);
            string escuela = default(string);
            string strCurso = default(string);

            try
            {
                ReportParameter prmRptHorarioAcademico = new ReportParameter();

                lstPrmRptHorarioAcademico.Add(  new ReportParameter("strPeriodoAcademico", 
                                                                    this._dtstPeriodoVigente.Periodos[0]["strDescripcion"].ToString().ToUpper()));

                strCurso = (this._strCodNivel != "-2")
                                ? this._getDescripcionCurso(this._strCodNivel, this._strCodParalelo)
                                : "";

                switch (UsuarioActual.CarreraActual.TipoEntidad.ToString())
                {
                    case "CAR":
                        facultad = pc.NombreFacultad;
                        carrera = pc.NombreCarrera;
                        escuela = pc.NombreEscuela;
                        break;

                    case "CAA":
                        lblFacultad = "";
                        lblCarrera = "";
                        lblEscuela = "";

                        facultad = pc.NombreFacultad;
                        carrera = pc.NombreCarrera;
                        escuela = "";
                        break;
                }

                lstPrmRptHorarioAcademico.Add(new ReportParameter("strInstitucion",
                                                                    Language.es_ES.STR_INSTITUCION));

                lstPrmRptHorarioAcademico.Add(new ReportParameter("strLblHorarioAcademico",
                                                                    Language.es_ES.STR_HORARIO_ACADEMICO));

                lstPrmRptHorarioAcademico.Add(new ReportParameter("strLblHorarioExamenes",
                                                                    Language.es_ES.STR_HORARIO_EXAMENES));

                lstPrmRptHorarioAcademico.Add(new ReportParameter("strLblPeriodoAcademico",
                                                                    Language.es_ES.STR_PERIODO_ACADEMICO));

                lstPrmRptHorarioAcademico.Add(new ReportParameter("strLblFacultad",
                                                                    lblFacultad));

                lstPrmRptHorarioAcademico.Add(new ReportParameter("strLblCarrera",
                                                                    lblCarrera));

                lstPrmRptHorarioAcademico.Add(new ReportParameter("strLblEscuela",
                                                                    lblEscuela));

                lstPrmRptHorarioAcademico.Add(new ReportParameter("strFacultad",
                                                                    facultad));

                lstPrmRptHorarioAcademico.Add(new ReportParameter("strEscuela",
                                                                    carrera));

                lstPrmRptHorarioAcademico.Add(new ReportParameter("strCarrera",
                                                                    escuela));

                lstPrmRptHorarioAcademico.Add(new ReportParameter("strCurso",
                                                                    strCurso));

                lstPrmRptHorarioAcademico.Add(new ReportParameter("strFuente",
                                                                    Language.es_ES.STR_FUENTE_REPORTE));
            }
            catch (Exception ex)
            {
                Errores err = new Errores();
                err.SetError(ex, "_getDatosGeneralesReporte");
            }

            return lstPrmRptHorarioAcademico;
        }


        private WSInfoCarreras.ParametrosCarrera _getParametrosCarrera()
        {
            WSInfoCarreras.ParametrosCarrera pc = new WSInfoCarreras.ParametrosCarrera();
            try
            {
                WSInfoCarreras.InfoCarreras ic = new WSInfoCarreras.InfoCarreras();
                pc = ic.GetParametrosCarrera(this.UsuarioActual.CarreraActual.Codigo.ToString());
            }
            catch (Exception ex)
            {
                Errores err = new Errores();
                err.SetError(ex, "_getParametrosCarrera");
            }

            return pc;
        }


    }
}